using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AtariMapMaker
{
    public static class ScreenLinkRenderer
    {
        /// <summary>
        /// Render linked screen overlay with transparency
        /// </summary>
        public static void RenderScreenLinkOverlay(AtariMap map, Graphics graphics, Rectangle viewport, int zoom, int screenX, int screenY)
        {
            if (map == null || map.ScreenLinks == null || graphics == null)
                return;

            // Find links for this screen
            var links = map.ScreenLinks.Where(link => 
                link.SourceScreen.X == screenX && link.SourceScreen.Y == screenY).ToList();

            if (links.Count == 0)
                return;

            foreach (var link in links)
            {
                RenderLinkedScreen(map, graphics, link, viewport, zoom);
            }
        }

        private static void RenderLinkedScreen(AtariMap map, Graphics graphics, ScreenLink link, Rectangle viewport, int zoom)
        {
            int sourceScreenPixelX = link.SourceScreen.X * map.ScreenSize.Width * 8 * zoom;
            int sourceScreenPixelY = link.SourceScreen.Y * map.ScreenSize.Height * 8 * zoom;
            int linkedScreenPixelX = link.LinkedScreen.X * map.ScreenSize.Width * 8 * zoom;
            int linkedScreenPixelY = link.LinkedScreen.Y * map.ScreenSize.Height * 8 * zoom;

            // Calculate offset
            int offsetX = linkedScreenPixelX - sourceScreenPixelX;
            int offsetY = linkedScreenPixelY - sourceScreenPixelY;

            // Create a bitmap for the linked screen
            Bitmap linkedScreenBitmap = new Bitmap(
                map.ScreenSize.Width * 8 * zoom,
                map.ScreenSize.Height * 8 * zoom,
                PixelFormat.Format32bppArgb);

            // Render the linked screen to the bitmap
            RenderScreenToBitmap(map, link.LinkedScreen, linkedScreenBitmap, zoom);

            // Apply transparency
            float alpha = link.Transparency;
            ColorMatrix matrix = new ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, alpha, 0},
                new float[] {0, 0, 0, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Draw the linked screen with transparency
            graphics.DrawImage(
                linkedScreenBitmap,
                new Rectangle(sourceScreenPixelX + offsetX, sourceScreenPixelY + offsetY, 
                    linkedScreenBitmap.Width, linkedScreenBitmap.Height),
                0, 0, linkedScreenBitmap.Width, linkedScreenBitmap.Height,
                GraphicsUnit.Pixel,
                attributes);

            linkedScreenBitmap.Dispose();
            attributes.Dispose();
        }

        private static void RenderScreenToBitmap(AtariMap map, Point screen, Bitmap bitmap, int zoom)
        {
            // This is a simplified version - in practice, you'd use AtariFontRenderer
            // to render the screen data to the bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                // Full implementation would render the screen's character data here
            }
        }
    }
}
