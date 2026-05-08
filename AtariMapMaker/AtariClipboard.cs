using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace AtariMapMaker
{
    public static class AtariClipboard
    {
        private static byte[,] data;
        private static AtariMap dataSource;
        private static Graphics gr;
        public static int ClipboardWidth { get; private set; }
        public static int ClipboardHeight { get; private set; }
        public static Bitmap ClipboardImage { get; set; }
        public static Bitmap UnderClipBoardImage { get; set; }
        public static Graphics UnderImageGraphics { get; set; }
        public static bool IsValid { get; set; }
        public static bool IsTileIndexes { get; set; }  // True if clipboard contains tile indexes (for tilemaps), false if characters
        public static bool SkipZero { get; set; }  // If true, skip pasting 0 chars/tiles (transparency)
        
        /// <summary>
        /// Gets the clipboard data array (for inverse operation)
        /// </summary>
        public static byte[,] GetData()
        {
            return data;
        }
        public static void SetDataSource(AtariMap myMap)
        {
            dataSource = myMap;
        }

        /// <summary>
        /// Sets clipboard to tile data (width, height, tile indexes). Does not create an image;
        /// caller must call RegenerateClipboardImage() to build the clipboard image (same as map selection).
        /// </summary>
        public static void SetTileData(int width, int height, byte[,] tileData)
        {
            if (tileData == null || width <= 0 || height <= 0)
                return;
            IsTileIndexes = true;
            ClipboardWidth = width;
            ClipboardHeight = height;
            data = new byte[width, height];
            for (int y = 0; y < height && y < tileData.GetLength(1); y++)
                for (int x = 0; x < width && x < tileData.GetLength(0); x++)
                    data[x, y] = tileData[x, y];
            if (ClipboardImage != null)
            {
                ClipboardImage.Dispose();
                ClipboardImage = null;
            }
            if (UnderClipBoardImage != null)
            {
                UnderClipBoardImage.Dispose();
                UnderClipBoardImage = null;
            }
            if (UnderImageGraphics != null)
            {
                UnderImageGraphics.Dispose();
                UnderImageGraphics = null;
            }
        }

        public static void Copy(Bitmap srcBmp, Rectangle mouseSelection, int offset)
        {
            if (dataSource == null)
                return;

            if (ClipboardImage != null)
            {
                ClipboardImage.Dispose();
                ClipboardImage = null;
            }

            int tileWidthChars = 1;
            int tileHeightChars = 1;

            // Data first (SkipZero transparency mask uses the same grid as Paste)
            if (dataSource.IsTilemap && dataSource.TilemapInfo != null)
            {
                tileWidthChars = dataSource.TilemapInfo.TileWidth;
                tileHeightChars = dataSource.TilemapInfo.TileHeight;
                int charX = mouseSelection.X / Globals.CharSize;
                int charY = mouseSelection.Y / Globals.CharSize;
                int charWidth = mouseSelection.Width / Globals.CharSize;
                int charHeight = mouseSelection.Height / Globals.CharSize;
                int tileWidthInTiles = charWidth / tileWidthChars;
                int tileHeightInTiles = charHeight / tileHeightChars;
                int absoluteCharX = dataSource.OffsetX + charX;
                int absoluteCharY = dataSource.OffsetY + charY;
                int absoluteTileX = absoluteCharX / tileWidthChars;
                int absoluteTileY = absoluteCharY / tileHeightChars;
                IsTileIndexes = true;
                ClipboardWidth = tileWidthInTiles;
                ClipboardHeight = tileHeightInTiles;
                data = new byte[ClipboardWidth, ClipboardHeight];
                int tilesPerRow = dataSource.Stride;
                for (int y = 0; y < ClipboardHeight; y++)
                {
                    for (int x = 0; x < ClipboardWidth; x++)
                    {
                        int tileIndex = (absoluteTileY + y) * tilesPerRow + (absoluteTileX + x);
                        if (tileIndex >= 0 && tileIndex < dataSource.Data.Length)
                            data[x, y] = dataSource.Data[tileIndex];
                    }
                }
            }
            else
            {
                IsTileIndexes = false;
                ClipboardWidth = mouseSelection.Width / Globals.CharSize;
                ClipboardHeight = mouseSelection.Height / Globals.CharSize;
                data = new byte[ClipboardWidth, ClipboardHeight];
                int xo = mouseSelection.X / Globals.CharSize;
                int yo = mouseSelection.Y / Globals.CharSize;
                byte[] sourceData = dataSource.Data;
                int sourceStride = dataSource.Stride;
                for (int y = 0; y < ClipboardHeight; y++)
                    for (int x = 0; x < ClipboardWidth; x++)
                    {
                        int dataIndex = offset + x + xo + (y + yo) * sourceStride;
                        if (dataIndex >= 0 && dataIndex < sourceData.Length)
                            data[x, y] = sourceData[dataIndex];
                    }
            }

            // Clipboard image: fast 8bpp crop from indexed renderer buffer, or draw + quantize fallback.
            // Crop is in unzoomed buffer space; editor overlay expects zoomed pixel size (same as mouseSelection).
            if (!SkipZero)
            {
                Bitmap bmp8;
                if (!TryBuild8bppClipboardCrop(srcBmp, mouseSelection, out bmp8))
                    bmp8 = Build8bppClipboardViaDrawAndQuantize(srcBmp, mouseSelection);
                else
                    bmp8 = EnsureClipboard8bppDisplaySize(bmp8, mouseSelection.Width, mouseSelection.Height);
                ClipboardImage = bmp8;
            }
            else
            {
                Bitmap temp8;
                if (!TryBuild8bppClipboardCrop(srcBmp, mouseSelection, out temp8))
                    temp8 = Build8bppClipboardViaDrawAndQuantize(srcBmp, mouseSelection);
                else
                    temp8 = EnsureClipboard8bppDisplaySize(temp8, mouseSelection.Width, mouseSelection.Height);
                try
                {
                    ClipboardImage = ConvertOpaque8bppToArgb32(temp8);
                    int cellPxW = IsTileIndexes ? tileWidthChars * Globals.CharSize : Globals.CharSize;
                    int cellPxH = IsTileIndexes ? tileHeightChars * Globals.CharSize : Globals.CharSize;
                    ApplySkipZeroTransparencyMask(ClipboardImage, data, ClipboardWidth, ClipboardHeight, cellPxW, cellPxH);
                }
                finally
                {
                    temp8.Dispose();
                }
            }
        }

        /// <summary>LockBits copy from Format8bppIndexed map buffer; no RGB requantization.</summary>
        private static bool TryBuild8bppClipboardCrop(Bitmap srcBmp, Rectangle mouseSelectionZoomed, out Bitmap bmp8)
        {
            bmp8 = null;
            if (srcBmp == null || srcBmp.PixelFormat != PixelFormat.Format8bppIndexed)
                return false;
            Rectangle srcRect = Globals.UnzoomRectangle(mouseSelectionZoomed);
            if (srcRect.Width <= 0 || srcRect.Height <= 0)
                return false;
            if (srcRect.X < 0 || srcRect.Y < 0 || srcRect.Right > srcBmp.Width || srcRect.Bottom > srcBmp.Height)
                return false;

            bmp8 = new Bitmap(srcRect.Width, srcRect.Height, PixelFormat.Format8bppIndexed);
            bmp8.Palette = AtariPalette.GetPalette();

            BitmapData srcData = srcBmp.LockBits(srcRect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData dstData = bmp8.LockBits(
                new Rectangle(0, 0, bmp8.Width, bmp8.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                unsafe
                {
                    byte* srcBase = (byte*)srcData.Scan0;
                    byte* dstBase = (byte*)dstData.Scan0;
                    int w = srcRect.Width;
                    int h = srcRect.Height;
                    for (int y = 0; y < h; y++)
                    {
                        byte* s = srcBase + y * srcData.Stride;
                        byte* d = dstBase + y * dstData.Stride;
                        for (int x = 0; x < w; x++)
                            d[x] = s[x];
                    }
                }
            }
            finally
            {
                srcBmp.UnlockBits(srcData);
                bmp8.UnlockBits(dstData);
            }

            return true;
        }

        /// <summary>
        /// Fast crop is 1 map pixel : 1 unzoomed pixel; clipboard overlay uses picture-box pixels (zoomed), same as <paramref name="mouseSelection"/> size.
        /// </summary>
        private static Bitmap EnsureClipboard8bppDisplaySize(Bitmap bmp8, int displayWidth, int displayHeight)
        {
            if (bmp8 == null)
                return null;
            if (bmp8.Width == displayWidth && bmp8.Height == displayHeight)
                return bmp8;
            return ScaleNearestNeighbor8bppIndexed(bmp8, displayWidth, displayHeight);
        }

        private static Bitmap ScaleNearestNeighbor8bppIndexed(Bitmap src, int dstW, int dstH)
        {
            var dst = new Bitmap(dstW, dstH, PixelFormat.Format8bppIndexed);
            dst.Palette = AtariPalette.GetPalette();
            int srcW = src.Width;
            int srcH = src.Height;
            if (srcW <= 0 || srcH <= 0)
            {
                src.Dispose();
                return dst;
            }
            BitmapData sbd = src.LockBits(
                new Rectangle(0, 0, srcW, srcH),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData dbd = dst.LockBits(
                new Rectangle(0, 0, dstW, dstH),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                unsafe
                {
                    byte* sp = (byte*)sbd.Scan0;
                    byte* dp = (byte*)dbd.Scan0;
                    for (int dy = 0; dy < dstH; dy++)
                    {
                        int sy = (dy * srcH) / dstH;
                        if (sy >= srcH)
                            sy = srcH - 1;
                        byte* srow = sp + sy * sbd.Stride;
                        byte* drow = dp + dy * dbd.Stride;
                        for (int dx = 0; dx < dstW; dx++)
                        {
                            int sx = (dx * srcW) / dstW;
                            if (sx >= srcW)
                                sx = srcW - 1;
                            drow[dx] = srow[sx];
                        }
                    }
                }
            }
            finally
            {
                src.UnlockBits(sbd);
                dst.UnlockBits(dbd);
                src.Dispose();
            }
            return dst;
        }

        /// <summary>Draw crop to 32bpp then match palette (fallback when source is not indexed or out of bounds).</summary>
        private static Bitmap Build8bppClipboardViaDrawAndQuantize(Bitmap srcBmp, Rectangle mouseSelection)
        {
            Bitmap temp32Bit = new Bitmap(mouseSelection.Width, mouseSelection.Height, PixelFormat.Format32bppArgb);
            using (Graphics tempGr = Graphics.FromImage(temp32Bit))
            {
                tempGr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                tempGr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                tempGr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
            }

            Bitmap bmp8 = new Bitmap(mouseSelection.Width, mouseSelection.Height, PixelFormat.Format8bppIndexed);
            bmp8.Palette = AtariPalette.GetPalette();
            Color[] palette = AtariPalette.GetPalette().Entries;

            BitmapData srcData = temp32Bit.LockBits(
                new Rectangle(0, 0, temp32Bit.Width, temp32Bit.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = bmp8.LockBits(
                new Rectangle(0, 0, bmp8.Width, bmp8.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                int* srcPtr = (int*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcWordsPerRow = srcData.Stride / 4;

                for (int y = 0; y < temp32Bit.Height; y++)
                {
                    for (int x = 0; x < temp32Bit.Width; x++)
                    {
                        int argb = srcPtr[y * srcWordsPerRow + x];
                        Color color = Color.FromArgb(argb);

                        int bestIndex = 0;
                        int minDistSq = int.MaxValue;
                        for (int i = 0; i < palette.Length; i++)
                        {
                            int dr = color.R - palette[i].R;
                            int dg = color.G - palette[i].G;
                            int db = color.B - palette[i].B;
                            int d = dr * dr + dg * dg + db * db;
                            if (d < minDistSq)
                            {
                                minDistSq = d;
                                bestIndex = i;
                            }
                        }

                        dstPtr[y * dstData.Stride + x] = (byte)bestIndex;
                    }
                }
            }

            temp32Bit.UnlockBits(srcData);
            bmp8.UnlockBits(dstData);
            temp32Bit.Dispose();
            return bmp8;
        }

        private static Bitmap ConvertOpaque8bppToArgb32(Bitmap src8)
        {
            Color[] pal = src8.Palette.Entries;
            var dst32 = new Bitmap(src8.Width, src8.Height, PixelFormat.Format32bppArgb);
            BitmapData srcData = src8.LockBits(
                new Rectangle(0, 0, src8.Width, src8.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData dstData = dst32.LockBits(
                new Rectangle(0, 0, dst32.Width, dst32.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    byte* sp = (byte*)srcData.Scan0;
                    int* dp = (int*)dstData.Scan0;
                    int w = src8.Width;
                    int h = src8.Height;
                    int dStride = dstData.Stride / 4;
                    for (int y = 0; y < h; y++)
                    {
                        byte* srow = sp + y * srcData.Stride;
                        int* drow = dp + y * dStride;
                        for (int x = 0; x < w; x++)
                        {
                            int idx = srow[x];
                            int rgb = pal[idx].ToArgb() & 0x00FFFFFF;
                            drow[x] = rgb | unchecked((int)0xFF000000);
                        }
                    }
                }
            }
            finally
            {
                src8.UnlockBits(srcData);
                dst32.UnlockBits(dstData);
            }
            return dst32;
        }

        /// <summary>Full alpha=0 for each cell where pasted data is 0x00 (same rule as <see cref="Paste"/>).</summary>
        private static void ApplySkipZeroTransparencyMask(Bitmap argb32, byte[,] clipData, int cellsW, int cellsH, int cellPixelW, int cellPixelH)
        {
            if (argb32 == null || clipData == null)
                return;
            BitmapData bd = argb32.LockBits(
                new Rectangle(0, 0, argb32.Width, argb32.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    int* p = (int*)bd.Scan0;
                    int strideInts = bd.Stride / 4;
                    for (int cy = 0; cy < cellsH; cy++)
                    {
                        for (int cx = 0; cx < cellsW; cx++)
                        {
                            if (clipData[cx, cy] != 0)
                                continue;
                            int x0 = cx * cellPixelW;
                            int y0 = cy * cellPixelH;
                            for (int yy = 0; yy < cellPixelH; yy++)
                            {
                                int* row = p + (y0 + yy) * strideInts + x0;
                                for (int xx = 0; xx < cellPixelW; xx++)
                                    row[xx] &= 0x00FFFFFF;
                            }
                        }
                    }
                }
            }
            finally
            {
                argb32.UnlockBits(bd);
            }
        }
        
        /// <summary>
        /// Copy tile indexes to clipboard (for tilemaps)
        /// </summary>
        public static void CopyTileIndexes(byte[] tileIndexes, int width, int height, Bitmap previewImage)
        {
            IsTileIndexes = true;
            ClipboardWidth = width;
            ClipboardHeight = height;
            
            //graphical part
            if (ClipboardImage != null)
            {
                ClipboardImage.Dispose();
            }
            ClipboardImage = previewImage;
            
            // Initialize under-image for DrawClipBoard
            if (UnderClipBoardImage != null)
            {
                UnderClipBoardImage.Dispose();
            }
            UnderClipBoardImage = new Bitmap(ClipboardImage);
            if (UnderImageGraphics != null)
            {
                UnderImageGraphics.Dispose();
            }
            UnderImageGraphics = Graphics.FromImage(UnderClipBoardImage);
            
            //data part - store tile indexes
            data = new byte[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (index < tileIndexes.Length)
                        data[x, y] = tileIndexes[index];
                }
        }

        /// <summary>
        /// Copy a library element to the clipboard. For character data: preview from font. For tile data: mapForTilemap required for correct preview size.
        /// </summary>
        public static void CopyFromLibraryElement(LibraryElement element, AtariMap mapForTilemap = null)
        {
            if (element == null || element.Data == null || element.Size.Width <= 0 || element.Size.Height <= 0)
                return;

            int w = element.Size.Width;
            int h = element.Size.Height;

            IsTileIndexes = element.IsTileData;
            ClipboardWidth = w;
            ClipboardHeight = h;

            // Build 2D data from element.Data (row-major)
            byte[,] tileData = new byte[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    if (index < element.Data.Length)
                        tileData[x, y] = element.Data[index];
                }

            if (element.IsTileData)
            {
                // Same path as map selection: set tile data only; caller calls RegenerateClipboardImage() to render
                SetTileData(w, h, tileData);
            }
            else
            {
                // Character mode: set data and render preview from font
                IsTileIndexes = false;
                ClipboardWidth = w;
                ClipboardHeight = h;
                data = tileData;
                Bitmap preview = new Bitmap(w * 8, h * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                preview.Palette = AtariPalette.GetIndexedColor5Palette();
                AtariFontRenderer.RenderClipboardData(data, w, h, preview);
                if (ClipboardImage != null)
                    ClipboardImage.Dispose();
                ClipboardImage = preview;
                if (UnderClipBoardImage != null)
                    UnderClipBoardImage.Dispose();
                UnderClipBoardImage = new Bitmap(ClipboardImage);
                if (UnderImageGraphics != null)
                    UnderImageGraphics.Dispose();
                UnderImageGraphics = Graphics.FromImage(UnderClipBoardImage);
            }

            IsValid = true;
        }

        public static void Paste(int offset)
        {
            if (dataSource == null || !IsValid)
                return;
            
            if (IsTileIndexes && dataSource.IsTilemap)
            {
                // Paste tile indexes (for tilemaps)
                if (dataSource.TilemapInfo != null)
                {
                    int tileWidth = dataSource.TilemapInfo.TileWidth;
                    int tileHeight = dataSource.TilemapInfo.TileHeight;
                    
                    // offset is in character coordinates (myMap.Offset + charOffset)
                    // Convert to absolute character coordinates
                    int charOffset = offset - dataSource.Offset;
                    // For tilemaps, use CharStride (character stride)
                    int charStride = dataSource.CharStride;
                    int absoluteCharX = charOffset % charStride;
                    int absoluteCharY = charOffset / charStride;
                    
                    // Convert to tile coordinates
                    int tileX = absoluteCharX / tileWidth;
                    int tileY = absoluteCharY / tileHeight;
                    
                    // For tilemaps, Data array stores tile indexes
                    // Stride is in tile units: MapSize.Width * ScreenSize.Width (tiles per row)
                    int tilesPerRow = dataSource.Stride;
                    
                    // Paste tile indexes and expand to CharData
                    for (int y = 0; y < ClipboardHeight; y++)
                    {
                        for (int x = 0; x < ClipboardWidth; x++)
                        {
                            int destTileX = tileX + x;
                            int destTileY = tileY + y;
                            int tileIndex = destTileY * tilesPerRow + destTileX;
                            
                            if (tileIndex >= 0 && tileIndex < dataSource.Data.Length)
                            {
                                byte tileIdx = data[x, y];
                                
                                // Skip zero tiles if SkipZero is enabled
                                if (SkipZero && tileIdx == 0)
                                    continue;
                                
                                dataSource.Data[tileIndex] = tileIdx;
                                
                                // Expand tile to CharData array for fast rendering
                                dataSource.ExpandTileToCharData(destTileX, destTileY, tileIdx);
                            }
                        }
                    }
                }
            }
            else
            {
                // Paste characters (normal mode)
                // offset is already absolute (myMap.Offset + charOffset); use CharStride so tilemaps work too
                int charStride = dataSource.CharStride;
                int charOffset = offset - dataSource.Offset;
                int charX = charOffset % charStride;
                int charY = charOffset / charStride;
                int maxCharHeight = dataSource.MapSize.Height * dataSource.ScreenSize.Height;
                if (dataSource.IsTilemap && dataSource.TilemapInfo != null)
                    maxCharHeight *= dataSource.TilemapInfo.TileHeight;

                // Check bounds
                if (charX + ClipboardWidth <= charStride &&
                    charY + ClipboardHeight <= maxCharHeight)
                {
                    for (int y = 0; y < ClipboardHeight; y++)
                    {
                        for (int x = 0; x < ClipboardWidth; x++)
                        {
                            int destCharX = charX + x;
                            int destCharY = charY + y;
                            int destIndex = destCharX + destCharY * charStride;
                            
                            if (destIndex >= 0 && destIndex < dataSource.Data.Length)
                            {
                                byte charVal = data[x, y];
                                
                                // Skip zero chars if SkipZero is enabled
                                if (SkipZero && charVal == 0)
                                    continue;
                                
                                dataSource.Data[destIndex] = charVal;
                            }
                        }
                    }
                }
            }
        }
    }
}
