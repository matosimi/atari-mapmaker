using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AtariMapMaker
{
    public static class BitmapTilesetRenderer
    {
        /// <summary>
        /// Split a bitmap into tiles of specified size
        /// </summary>
        public static BitmapTilesetData CreateTilesetFromBitmap(Bitmap sourceBitmap, int tileWidth, int tileHeight)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            int tilesPerRow = sourceBitmap.Width / tileWidth;
            int tilesPerColumn = sourceBitmap.Height / tileHeight;
            int totalTiles = tilesPerRow * tilesPerColumn;

            // Convert bitmap to byte array (assuming 8bpp indexed or convert to it)
            byte[] bitmapData = new byte[sourceBitmap.Width * sourceBitmap.Height];

            if (sourceBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                BitmapData bmd = sourceBitmap.LockBits(
                    new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format8bppIndexed);

                unsafe
                {
                    byte* ptr = (byte*)bmd.Scan0;
                    for (int y = 0; y < sourceBitmap.Height; y++)
                    {
                        for (int x = 0; x < sourceBitmap.Width; x++)
                        {
                            bitmapData[y * sourceBitmap.Width + x] = ptr[y * bmd.Stride + x];
                        }
                    }
                }

                sourceBitmap.UnlockBits(bmd);
            }
            else
            {
                // Convert to 8bpp indexed format
                Bitmap converted = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format8bppIndexed);
                converted.Palette = sourceBitmap.Palette;
                using (Graphics g = Graphics.FromImage(converted))
                {
                    g.DrawImage(sourceBitmap, 0, 0);
                }

                BitmapData bmd = converted.LockBits(
                    new Rectangle(0, 0, converted.Width, converted.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format8bppIndexed);

                unsafe
                {
                    byte* ptr = (byte*)bmd.Scan0;
                    for (int y = 0; y < converted.Height; y++)
                    {
                        for (int x = 0; x < converted.Width; x++)
                        {
                            bitmapData[y * converted.Width + x] = ptr[y * bmd.Stride + x];
                        }
                    }
                }

                converted.UnlockBits(bmd);
                converted.Dispose();
            }

            return new BitmapTilesetData
            {
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                BitmapData = bitmapData,
                BitmapWidth = sourceBitmap.Width,
                BitmapHeight = sourceBitmap.Height
            };
        }

        /// <summary>
        /// Get a specific tile from the tileset
        /// </summary>
        public static byte[] GetTile(BitmapTilesetData tileset, int tileIndex)
        {
            if (tileset == null || tileset.BitmapData == null)
                return null;

            int tilesPerRow = tileset.BitmapWidth / tileset.TileWidth;
            int tileX = tileIndex % tilesPerRow;
            int tileY = tileIndex / tilesPerRow;

            byte[] tileData = new byte[tileset.TileWidth * tileset.TileHeight];

            for (int y = 0; y < tileset.TileHeight; y++)
            {
                for (int x = 0; x < tileset.TileWidth; x++)
                {
                    int sourceX = tileX * tileset.TileWidth + x;
                    int sourceY = tileY * tileset.TileHeight + y;
                    int sourceIndex = sourceY * tileset.BitmapWidth + sourceX;
                    int destIndex = y * tileset.TileWidth + x;

                    if (sourceIndex < tileset.BitmapData.Length && destIndex < tileData.Length)
                        tileData[destIndex] = tileset.BitmapData[sourceIndex];
                }
            }

            return tileData;
        }

        /// <summary>
        /// Render a tile from the tileset to a bitmap
        /// </summary>
        public static void RenderTile(BitmapTilesetData tileset, int tileIndex, Bitmap outputBitmap, int destX, int destY)
        {
            if (tileset == null || outputBitmap == null)
                return;

            byte[] tileData = GetTile(tileset, tileIndex);
            if (tileData == null)
                return;

            if (outputBitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                return;

            BitmapData bmd = outputBitmap.LockBits(
                new Rectangle(destX, destY, tileset.TileWidth, tileset.TileHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* ptr = (byte*)bmd.Scan0;
                for (int y = 0; y < tileset.TileHeight; y++)
                {
                    for (int x = 0; x < tileset.TileWidth; x++)
                    {
                        int tileIndex_local = y * tileset.TileWidth + x;
                        if (tileIndex_local < tileData.Length)
                            ptr[y * bmd.Stride + x] = tileData[tileIndex_local];
                    }
                }
            }

            outputBitmap.UnlockBits(bmd);
        }
    }
}
