using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace AtariMapMaker
{
    public static class ScreenLinkRenderer
    {
        /// <summary>
        /// Draw the linked screen (from mapImage) into the given destination rect with transparency.
        /// Layer order: 1. transparent linked screen (this), 2. current screen (drawn by caller).
        /// </summary>
        public static void DrawLinkedScreenIntoRect(AtariMap map, Bitmap mapImage, Graphics graphics, ScreenLink link, Rectangle destRect)
        {
            if (map == null || mapImage == null || graphics == null || link == null)
                return;
            int screenCharWidth = map.ScreenSize.Width;
            int screenCharHeight = map.ScreenSize.Height;
            if (map.IsTilemap && map.TilemapInfo != null)
            {
                screenCharWidth = map.ScreenSize.Width * map.TilemapInfo.TileWidth;
                screenCharHeight = map.ScreenSize.Height * map.TilemapInfo.TileHeight;
            }
            int zoom = destRect.Width / (screenCharWidth * 8);
            if (zoom < 1) zoom = 1;
            int linkedSrcX = (link.LinkedScreen.X * screenCharWidth - map.OffsetX) * 8;
            int linkedSrcY = (link.LinkedScreen.Y * screenCharHeight - map.OffsetY) * 8;
            int linkedSrcW = screenCharWidth * 8;
            int linkedSrcH = screenCharHeight * 8;
            if (linkedSrcX < 0 || linkedSrcY < 0 || linkedSrcX + linkedSrcW > mapImage.Width || linkedSrcY + linkedSrcH > mapImage.Height)
                return;
            float alpha = link.Transparency;
            ColorMatrix matrix = new ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, alpha, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            using (ImageAttributes attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(mapImage, destRect, linkedSrcX, linkedSrcY, linkedSrcW, linkedSrcH, GraphicsUnit.Pixel, attributes);
            }
        }
    }
}
