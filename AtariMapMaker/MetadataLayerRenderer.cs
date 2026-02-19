using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AtariMapMaker
{
    public static class MetadataLayerRenderer
    {
        /// <summary>
        /// Render metadata layer overlay on top of the map. Uses map offset so overlay matches visible area.
        /// </summary>
        public static void RenderMetadataLayer(AtariMap map, Graphics graphics, Rectangle viewport, int zoom)
        {
            if (map == null || map.ScreenMetadata == null || graphics == null)
                return;

            int screenCharWidth = map.ScreenSize.Width;
            int screenCharHeight = map.ScreenSize.Height;
            int cellW = 1, cellH = 1;
            if (map.IsTilemap && map.TilemapInfo != null)
            {
                screenCharWidth = map.ScreenSize.Width * map.TilemapInfo.TileWidth;
                screenCharHeight = map.ScreenSize.Height * map.TilemapInfo.TileHeight;
                cellW = map.TilemapInfo.TileWidth;
                cellH = map.TilemapInfo.TileHeight;
            }
            int pxPerChar = 8 * zoom;
            int offsetPxX = map.OffsetX * pxPerChar;
            int offsetPxY = map.OffsetY * pxPerChar;

            int startScreenX = (map.OffsetX + viewport.X / pxPerChar) / screenCharWidth;
            int startScreenY = (map.OffsetY + viewport.Y / pxPerChar) / screenCharHeight;
            int endScreenX = (map.OffsetX + (viewport.X + viewport.Width) / pxPerChar) / screenCharWidth + 1;
            int endScreenY = (map.OffsetY + (viewport.Y + viewport.Height) / pxPerChar) / screenCharHeight + 1;

            for (int screenY = startScreenY; screenY <= endScreenY && screenY < map.MapSize.Height; screenY++)
            {
                for (int screenX = startScreenX; screenX <= endScreenX && screenX < map.MapSize.Width; screenX++)
                {
                    List<MetadataLayerItem> items = MetadataParser.GetScreenMetadataItems(map, screenX, screenY);
                    if (items == null || items.Count == 0)
                        continue;

                    int screenPixelX = (screenX * screenCharWidth) * pxPerChar - offsetPxX;
                    int screenPixelY = (screenY * screenCharHeight) * pxPerChar - offsetPxY;

                    foreach (var item in items)
                    {
                        int cellX = item.X;
                        int cellY = item.Y;

                        if (map.IsTilemap && map.TilemapInfo != null)
                        {
                            if (cellX < 0 || cellX >= map.ScreenSize.Width || cellY < 0 || cellY >= map.ScreenSize.Height)
                                continue;
                        }
                        else
                        {
                            if (cellX < 0 || cellX >= screenCharWidth || cellY < 0 || cellY >= screenCharHeight)
                                continue;
                        }

                        int pixelX = screenPixelX + cellX * cellW * pxPerChar;
                        int pixelY = screenPixelY + cellY * cellH * pxPerChar;
                        int cellPxW = cellW * pxPerChar;
                        int cellPxH = cellH * pxPerChar;
                        if (pixelX + cellPxW < 0 || pixelY + cellPxH < 0 || pixelX >= viewport.Width || pixelY >= viewport.Height)
                            continue;

                        int tileHeightChars = (map.IsTilemap && map.TilemapInfo != null) ? map.TilemapInfo.TileHeight : 1;
                        DrawMetadataItem(graphics, item, pixelX, pixelY, zoom, showText: true, cellWidthPx: cellPxW, cellHeightPx: cellPxH, tileHeightChars: tileHeightChars);
                        // Laser direction indicator: diagonal line (1 char) adjacent to laser, only for LASER metadata
                        if (string.Equals((item.Text ?? "").Trim(), "LASER", StringComparison.OrdinalIgnoreCase))
                            DrawLaserDirectionLine(graphics, item, pixelX, pixelY, zoom, cellPxW, cellPxH);
                    }
                }
            }
        }

        /// <summary>Creates a small bitmap preview of a metadata item for the clipboard picture box.</summary>
        /// <param name="cellOnly">If true, only the cell (color + value hex) is drawn; text is never shown (used for clipboard).</param>
        /// <param name="cellWidthPixels">Optional. When set with cellHeightPixels, draws tile-sized cell (e.g. for tilemap overlay).</param>
        /// <param name="cellHeightPixels">Optional. When set with cellWidthPixels, draws tile-sized cell.</param>
        public static System.Drawing.Bitmap CreateMetadataItemPreviewBitmap(MetadataLayerItem item, int cellSizePixels = 64, bool cellOnly = false, int cellWidthPixels = 0, int cellHeightPixels = 0)
        {
            if (item == null) return null;
            int zoom = Math.Max(1, cellSizePixels / 8);
            if (cellWidthPixels <= 0 || cellHeightPixels <= 0)
                zoom = Math.Max(1, cellSizePixels / 8);
            int cw = (cellWidthPixels > 0 && cellHeightPixels > 0) ? cellWidthPixels : (8 * zoom);
            int ch = (cellWidthPixels > 0 && cellHeightPixels > 0) ? cellHeightPixels : (8 * zoom);
            int lineHeightPx = 8 * zoom;
            int tileHeightChars = (ch > lineHeightPx) ? (ch / lineHeightPx) : 1;
            int w = cw;
            int h;
            int drawY;
            if (cellOnly)
            {
                h = ch;
                drawY = 0;
            }
            else if (tileHeightChars == 1 && !string.IsNullOrEmpty(item?.Text))
            {
                h = ch + lineHeightPx;
                drawY = lineHeightPx;
            }
            else
            {
                // tileHeightChars >= 2 or no text: text and value are both inside the tile, no extra height
                h = ch;
                drawY = 0;
            }
            var bmp = new System.Drawing.Bitmap(w, Math.Max(h, ch));
            using (var gr = System.Drawing.Graphics.FromImage(bmp))
            {
                gr.Clear(System.Drawing.Color.FromArgb(AtariPalette.GetPalette().Entries[0].ToArgb()));
                DrawMetadataItem(gr, item, 0, drawY, zoom, showText: !cellOnly, cellWidthPx: cw, cellHeightPx: ch, tileHeightChars: tileHeightChars);
            }
            return bmp;
        }

        private static void DrawMetadataItem(Graphics graphics, MetadataLayerItem item, int x, int y, int zoom, bool showText = true, int cellWidthPx = 0, int cellHeightPx = 0, int tileHeightChars = 1)
        {
            int cellW = cellWidthPx > 0 ? cellWidthPx : (8 * zoom);
            int cellH = cellHeightPx > 0 ? cellHeightPx : (8 * zoom);
            Color metaColor = AtariPalette.GetColor(item.Color);
            using (Brush brush = new SolidBrush(Color.FromArgb(180, metaColor)))
            {
                graphics.FillRectangle(brush, x, y, cellW, cellH);
            }

            string valueHex = item.Value >= 0 && item.Value <= 255 ? item.Value.ToString("X2") : item.Value.ToString("X4");
            double luminance = 0.299 * metaColor.R + 0.587 * metaColor.G + 0.114 * metaColor.B;
            Color valueTextColor = luminance > 128 ? Color.Black : Color.White;
            int fontSz = Math.Max(4, Math.Min(cellH / 2, 4 * zoom));
            int lineHeightPx = 8 * zoom;

            using (Font font = new Font("Segoe UI", fontSz))
            using (Brush valueBrush = new SolidBrush(valueTextColor))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                if (tileHeightChars >= 2)
                {
                    // Tile at least 2 chars high: text on line 0, value on line 1 (both inside tile)
                    if (showText && Globals.MetadataLayerShowText && !string.IsNullOrEmpty(item.Text))
                        graphics.DrawString(item.Text, font, textBrush, x, y);
                    graphics.DrawString(valueHex, font, valueBrush, x, y + lineHeightPx);
                }
                else
                {
                    // Tile 1 char high: value on line 0 (inside tile), text on line -1 (one char above the tile)
                    graphics.DrawString(valueHex, font, valueBrush, x, y);
                    if (showText && Globals.MetadataLayerShowText && !string.IsNullOrEmpty(item.Text))
                        graphics.DrawString(item.Text, font, textBrush, x, y - lineHeightPx);
                }
            }
        }

        /// <summary>
        /// Draws a 1-char diagonal line adjacent to a LASER metadata item to show emission direction (value 0-7).
        /// Clockwise from top-left: 0=\ from top, 1=/ from top, 2=/ from right, 3=\ from right, 4=\ from bottom, 5=/ from bottom, 6=/ from left, 7=\ from left.
        /// Line is drawn outward from the edge (adjacent to the laser element), 1 char size, in the laser's color.
        /// </summary>
        private static void DrawLaserDirectionLine(Graphics graphics, MetadataLayerItem item, int x, int y, int zoom, int cellPxW, int cellPxH)
        {
            int dir = item.Value & 7;
            int L = 8 * zoom; // 1 char length
            float cx = x + cellPxW / 2f;
            float cy = y + cellPxH / 2f;
            float x1, y1, x2, y2;
            switch (dir)
            {
                case 1: // \ from center of top edge (outward = up, backslash = up-right)
                    x1 = cx; y1 = y; x2 = cx + L; y2 = y - L; break;
                case 0: // / from center of top edge (outward = up, slash = up-left)
                    x1 = cx; y1 = y; x2 = cx - L; y2 = y - L; break;
                case 2: // / from center of right edge (outward = right, slash = up-right)
                    x1 = x + cellPxW; y1 = cy; x2 = x + cellPxW + L; y2 = cy - L; break;
                case 3: // \ from center of right edge (outward = right, backslash = down-right)
                    x1 = x + cellPxW; y1 = cy; x2 = x + cellPxW + L; y2 = cy + L; break;
                case 4: // \ from center of bottom edge (outward = down, backslash = down-right)
                    x1 = cx; y1 = y + cellPxH; x2 = cx + L; y2 = y + cellPxH + L; break;
                case 5: // / from center of bottom edge (outward = down, slash = down-left)
                    x1 = cx; y1 = y + cellPxH; x2 = cx - L; y2 = y + cellPxH + L; break;
                case 7: // / from center of left edge (outward = left, slash = up-left)
                    x1 = x; y1 = cy; x2 = x - L; y2 = cy - L; break;
                case 6: // \ from center of left edge (outward = left, backslash = down-left)
                    x1 = x; y1 = cy; x2 = x - L; y2 = cy + L; break;
                default:
                    return;
            }
            Color lineColor = AtariPalette.GetColor(item.Color);
            using (var pen = new Pen(lineColor, Math.Max(1, zoom)))
            {
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }
    }
}
