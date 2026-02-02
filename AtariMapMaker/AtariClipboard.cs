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
        public static Bitmap ClipboardImage { get; private set; }
        public static Bitmap UnderClipBoardImage { get; set; }
        public static Graphics UnderImageGraphics { get; set; }
        public static bool IsValid { get; set; }
        public static bool IsTileIndexes { get; set; }  // True if clipboard contains tile indexes (for tilemaps), false if characters
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
            ClipboardImage = new Bitmap(mouseSelection.Width, mouseSelection.Height);
            gr = Graphics.FromImage(ClipboardImage);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            gr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
            gr.Dispose();

            //data part - default to character mode
            IsTileIndexes = false;
            ClipboardWidth = mouseSelection.Width / Globals.CharSize;
            ClipboardHeight = mouseSelection.Height / Globals.CharSize;
            data = new byte[ClipboardWidth, ClipboardHeight];
            int xo = mouseSelection.X / Globals.CharSize;
            int yo = mouseSelection.Y / Globals.CharSize;
            
            // For tilemaps, use CharData if available; otherwise use Data
            byte[] sourceData = dataSource.Data;
            int sourceStride = dataSource.Stride;
            if (dataSource.IsTilemap && dataSource.CharData != null && dataSource.CharData.Length > 0)
            {
                sourceData = dataSource.CharData;
                sourceStride = dataSource.CharStride;
            }
            
            for (int y = 0; y < ClipboardHeight; y++)
                for (int x = 0; x < ClipboardWidth; x++)
                {
                    int dataIndex = offset + x + xo + (y + yo) * sourceStride;
                    if (dataIndex >= 0 && dataIndex < sourceData.Length)
                        data[x, y] = sourceData[dataIndex];
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
                                dataSource.Data[destIndex] = data[x, y];
                            }
                        }
                    }
                }
            }
        }
    }
}
