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

        public static void Copy(Bitmap srcBmp, Rectangle mouseSelection, int offset)
        {
            if (dataSource == null)
                return;
            
            //graphical part
            if (ClipboardImage != null)
            {
                ClipboardImage.Dispose();
            }
            
            // If SkipZero is enabled, create a 32-bit ARGB bitmap to support transparency
            // Otherwise use 8-bit indexed for better performance
            if (SkipZero)
            {
                // Create 32-bit ARGB bitmap for transparency support.
                // tempBitmap is 32bpp (default); we must lock it as Format32bppArgb when reading.
                Bitmap tempBitmap = new Bitmap(mouseSelection.Width, mouseSelection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics tempGr = Graphics.FromImage(tempBitmap))
                {
                    tempGr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    tempGr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    tempGr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
                }
                
                ClipboardImage = new Bitmap(tempBitmap.Width, tempBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData srcData = tempBitmap.LockBits(
                    new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData dstData = ClipboardImage.LockBits(
                    new Rectangle(0, 0, ClipboardImage.Width, ClipboardImage.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    int* srcPtr = (int*)srcData.Scan0;
                    int* dstPtr = (int*)dstData.Scan0;
                    int w = tempBitmap.Width;
                    int h = tempBitmap.Height;
                    int srcStride = srcData.Stride / 4;
                    int dstStride = dstData.Stride / 4;
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            dstPtr[y * dstStride + x] = srcPtr[y * srcStride + x];
                }
                tempBitmap.UnlockBits(srcData);
                ClipboardImage.UnlockBits(dstData);
                tempBitmap.Dispose();
            }
            else
            {
                // Use 8-bit indexed bitmap for normal operation
                // First draw to a 32-bit bitmap (Graphics can't be created from 8-bit indexed)
                Bitmap temp32Bit = new Bitmap(mouseSelection.Width, mouseSelection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics tempGr = Graphics.FromImage(temp32Bit))
                {
                    tempGr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    tempGr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    tempGr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
                }
                
                // Convert 32-bit ARGB to 8-bit indexed using palette
                ClipboardImage = new Bitmap(mouseSelection.Width, mouseSelection.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                ClipboardImage.Palette = AtariPalette.GetPalette();
                Color[] palette = AtariPalette.GetPalette().Entries;
                
                BitmapData srcData = temp32Bit.LockBits(
                    new Rectangle(0, 0, temp32Bit.Width, temp32Bit.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData dstData = ClipboardImage.LockBits(
                    new Rectangle(0, 0, ClipboardImage.Width, ClipboardImage.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                
                unsafe
                {
                    int* srcPtr = (int*)srcData.Scan0;
                    byte* dstPtr = (byte*)dstData.Scan0;
                    
                    for (int y = 0; y < temp32Bit.Height; y++)
                    {
                        for (int x = 0; x < temp32Bit.Width; x++)
                        {
                            int argb = srcPtr[y * srcData.Stride / 4 + x];
                            Color color = Color.FromArgb(argb);
                            
                            // Find closest palette color
                            int bestIndex = 0;
                            double minDistance = double.MaxValue;
                            for (int i = 0; i < palette.Length; i++)
                            {
                                double distance = Math.Sqrt(
                                    Math.Pow(color.R - palette[i].R, 2) +
                                    Math.Pow(color.G - palette[i].G, 2) +
                                    Math.Pow(color.B - palette[i].B, 2));
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    bestIndex = i;
                                }
                            }
                            
                            dstPtr[y * dstData.Stride + x] = (byte)bestIndex;
                        }
                    }
                }
                
                temp32Bit.UnlockBits(srcData);
                ClipboardImage.UnlockBits(dstData);
                temp32Bit.Dispose();
            }

            // For tilemaps, copy tile indexes instead of characters
            if (dataSource.IsTilemap && dataSource.TilemapInfo != null)
            {
                int tileWidth = dataSource.TilemapInfo.TileWidth;
                int tileHeight = dataSource.TilemapInfo.TileHeight;
                
                // Selection is in pixels, convert to character coordinates
                int charX = mouseSelection.X / Globals.CharSize;
                int charY = mouseSelection.Y / Globals.CharSize;
                int charWidth = mouseSelection.Width / Globals.CharSize;
                int charHeight = mouseSelection.Height / Globals.CharSize;
                
                // Convert to tile coordinates (relative to visible area)
                int tileXStart = charX / tileWidth;
                int tileYStart = charY / tileHeight;
                int tileWidthInTiles = charWidth / tileWidth;
                int tileHeightInTiles = charHeight / tileHeight;
                
                // Convert to absolute tile coordinates
                int absoluteCharX = dataSource.OffsetX + charX;
                int absoluteCharY = dataSource.OffsetY + charY;
                int absoluteTileX = absoluteCharX / tileWidth;
                int absoluteTileY = absoluteCharY / tileHeight;
                
                // Copy tile indexes
                IsTileIndexes = true;
                ClipboardWidth = tileWidthInTiles;
                ClipboardHeight = tileHeightInTiles;
                data = new byte[ClipboardWidth, ClipboardHeight];
                
                int tilesPerRow = dataSource.Stride; // Stride is in tile units for tilemaps
                
                for (int y = 0; y < ClipboardHeight; y++)
                {
                    for (int x = 0; x < ClipboardWidth; x++)
                    {
                        int destTileX = absoluteTileX + x;
                        int destTileY = absoluteTileY + y;
                        int tileIndex = destTileY * tilesPerRow + destTileX;
                        
                        if (tileIndex >= 0 && tileIndex < dataSource.Data.Length)
                        {
                            data[x, y] = dataSource.Data[tileIndex];
                        }
                    }
                }
            }
            else
            {
                // Normal character mode
                IsTileIndexes = false;
                ClipboardWidth = mouseSelection.Width / Globals.CharSize;
                ClipboardHeight = mouseSelection.Height / Globals.CharSize;
                data = new byte[ClipboardWidth, ClipboardHeight];
                int xo = mouseSelection.X / Globals.CharSize;
                int yo = mouseSelection.Y / Globals.CharSize;
                
                // For normal maps, use Data array
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
                // offset is already absolute (myMap.Offset + charOffset), so use it directly
                int charOffset = offset - dataSource.Offset;
                int charX = charOffset % dataSource.Stride;
                int charY = charOffset / dataSource.Stride;
                
                // Check bounds
                if (charX + ClipboardWidth <= dataSource.Stride && 
                    charY + ClipboardHeight <= dataSource.MapSize.Height * dataSource.ScreenSize.Height)
                {
                    for (int y = 0; y < ClipboardHeight; y++)
                    {
                        for (int x = 0; x < ClipboardWidth; x++)
                        {
                            int destCharX = charX + x;
                            int destCharY = charY + y;
                            int destIndex = destCharX + destCharY * dataSource.Stride;
                            
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
