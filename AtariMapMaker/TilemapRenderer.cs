using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace AtariMapMaker
{
    public static class TilemapRenderer
    {
        /// <summary>
        /// Render a tilemap using submap screens as tiles
        /// </summary>
        public static void RenderTilemap(AtariMap tilemap, AtariMap submap, Bitmap outBmp)
        {
            if (tilemap == null || submap == null || !tilemap.IsTilemap || tilemap.TilemapInfo == null)
                return;

            if (outBmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Output bitmap of RenderTilemap MUST be 8bppIndexed palette!");

            int tileWidth = tilemap.TilemapInfo.TileWidth;
            int tileHeight = tilemap.TilemapInfo.TileHeight;

            // Validate tile dimensions match submap screen size
            if (tileWidth != submap.ScreenSize.Width || tileHeight != submap.ScreenSize.Height)
            {
                // Try to adjust or use default
                tileWidth = submap.ScreenSize.Width;
                tileHeight = submap.ScreenSize.Height;
            }

            int width = outBmp.Width / 8;
            int height = outBmp.Height / 8;
            int widthFull = width;
            int heightFull = height;

            if (tilemap.OffsetX + width > tilemap.Stride)
                width = tilemap.Stride - tilemap.OffsetX;
            if (tilemap.OffsetY + height > tilemap.MapSize.Height * tilemap.ScreenSize.Height)
            {
                height = tilemap.ScreenSize.Height * tilemap.MapSize.Height - tilemap.OffsetY;
                height = Math.Max(height, 0);
            }

            BitmapData bmd = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* row = (byte*)bmd.Scan0;

                for (int y = 0; y < height; y++)
                {
                    int absoluteY = tilemap.OffsetY + y;
                    int tileY = absoluteY / tileHeight;
                    int pixelYInTile = absoluteY % tileHeight;

                    for (int x = 0; x < width; x++)
                    {
                        int absoluteX = tilemap.OffsetX + x;
                        int tileX = absoluteX / tileWidth;
                        int pixelXInTile = absoluteX % tileWidth;

                        // Get tile index from tilemap data
                        // Data array is indexed by tile coordinates, not character coordinates
                        int tileIndex = 0;
                        int tileDataIndex = tileY * tilemap.Stride + tileX;
                        if (tileDataIndex >= 0 && tileDataIndex < tilemap.Data.Length)
                            tileIndex = tilemap.Data[tileDataIndex];

                        // Get corresponding screen from submap
                        int submapScreenIndex = GetSubmapScreenIndex(tilemap, tileX, tileY, tileIndex);
                        int submapScreenX = submapScreenIndex % submap.MapSize.Width;
                        int submapScreenY = submapScreenIndex / submap.MapSize.Width;

                        // Calculate position in submap data
                        int submapCharX = submapScreenX * submap.ScreenSize.Width + pixelXInTile;
                        int submapCharY = submapScreenY * submap.ScreenSize.Height + pixelYInTile;

                        if (submapCharX < submap.Stride && submapCharY < submap.MapSize.Height * submap.ScreenSize.Height)
                        {
                            int submapCharIndex = submapCharX + submapCharY * submap.Stride;
                            byte charValue = submap.Data[submapCharIndex];

                            // Render character from submap's font
                            RenderCharFromSubmap(submap, charValue, pixelXInTile, pixelYInTile, row, x, y, bmd.Stride);
                        }
                    }
                }

                // Fill off-map area
                byte offMapColor = (AtariFontRenderer.Color5[4] & 0x0f) < 0x04 ? 
                    (byte)(AtariFontRenderer.Color5[4] + 0x04) : 
                    (byte)(AtariFontRenderer.Color5[4] - 0x04);

                for (int x = width; x < widthFull; x++)
                    for (int c = 0; c < 8; c++)
                        row[x * 8 + c] = offMapColor;

                int offUnderPixelAmount = (heightFull - height) * 8 * 8 * widthFull;
                for (int i = 0; i < offUnderPixelAmount; i++)
                    row[i] = offMapColor;
            }

            outBmp.UnlockBits(bmd);
        }

        private static int GetSubmapScreenIndex(AtariMap tilemap, int tileX, int tileY, int tileIndex)
        {
            // Use tileIndex from tilemap data to determine which submap screen to use
            // For now, use tileIndex directly as screen index
            // This can be enhanced with tile byte value mapping later
            return tileIndex;
        }

        private static unsafe void RenderCharFromSubmap(AtariMap submap, byte charValue, int charX, int charY, 
            byte* outputRow, int outputX, int outputY, int outputStride)
        {
            // Get font for this line in submap
            int lineInScreen = charY % submap.ScreenSize.Height;
            byte fontIndex = submap.GetFontForLine(lineInScreen);
            byte[] fontData = submap.GetFontData(fontIndex);

            if (fontData == null)
                return;

            // Get font bitmap (simplified - would need to access cached font)
            // For now, use the default rendering approach
            // This is a simplified version - full implementation would use AtariFontRenderer
            // to get the actual pixel data from the font bitmap
        }
    }
}
