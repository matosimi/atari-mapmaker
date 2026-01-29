using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AtariMapMaker
{
    public static class MetadataLayerRenderer
    {
        /// <summary>
        /// Render metadata layer overlay on top of the map
        /// </summary>
        public static void RenderMetadataLayer(AtariMap map, Graphics graphics, Rectangle viewport, int zoom)
        {
            if (map == null || map.ScreenMetadata == null || graphics == null)
                return;

            // Iterate through all screens in viewport
            int startScreenX = viewport.X / (map.ScreenSize.Width * 8 * zoom);
            int startScreenY = viewport.Y / (map.ScreenSize.Height * 8 * zoom);
            int endScreenX = (viewport.X + viewport.Width) / (map.ScreenSize.Width * 8 * zoom) + 1;
            int endScreenY = (viewport.Y + viewport.Height) / (map.ScreenSize.Height * 8 * zoom) + 1;

            for (int screenY = startScreenY; screenY <= endScreenY && screenY < map.MapSize.Height; screenY++)
            {
                for (int screenX = startScreenX; screenX <= endScreenX && screenX < map.MapSize.Width; screenX++)
                {
                    List<MetadataLayerItem> items = MetadataParser.GetScreenMetadataItems(map, screenX, screenY);
                    if (items == null || items.Count == 0)
                        continue;

                    int screenPixelX = screenX * map.ScreenSize.Width * 8 * zoom;
                    int screenPixelY = screenY * map.ScreenSize.Height * 8 * zoom;

                    foreach (var item in items)
                    {
                        int charX = item.X;
                        int charY = item.Y;

                        if (charX < 0 || charX >= map.ScreenSize.Width || 
                            charY < 0 || charY >= map.ScreenSize.Height)
                            continue;

                        int pixelX = screenPixelX + charX * 8 * zoom;
                        int pixelY = screenPixelY + charY * 8 * zoom;

                        // Draw metadata indicator
                        DrawMetadataItem(graphics, item, pixelX, pixelY, zoom);
                    }
                }
            }
        }

        private static void DrawMetadataItem(Graphics graphics, MetadataLayerItem item, int x, int y, int zoom)
        {
            // Draw a semi-transparent rectangle or text indicator
            using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.Yellow)))
            {
                graphics.FillRectangle(brush, x, y, 8 * zoom, 8 * zoom);
            }

            // Draw text if available
            if (!string.IsNullOrEmpty(item.Text))
            {
                using (Font font = new Font("Arial", 6 * zoom))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    graphics.DrawString(item.Text, font, textBrush, x, y);
                }
            }

            // Draw value if available
            if (item.Value != 0)
            {
                using (Font font = new Font("Arial", 6 * zoom))
                using (Brush textBrush = new SolidBrush(Color.Cyan))
                {
                    graphics.DrawString(item.Value.ToString(), font, textBrush, x, y + 8 * zoom);
                }
            }

            // Draw color indicator if available
            if (item.Color != 0)
            {
                Color atariColor = AtariPalette.GetColor(item.Color);
                using (Brush brush = new SolidBrush(atariColor))
                {
                    graphics.FillEllipse(brush, x + 6 * zoom, y + 6 * zoom, 2 * zoom, 2 * zoom);
                }
            }
        }
    }
}
