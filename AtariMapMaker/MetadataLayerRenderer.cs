using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AtariMapMaker
{
    public static class MetadataLayerRenderer
    {
        private const int ValueLinkPaletteIndex = 0x0E;

        private struct MetaEndpoint
        {
            public float X, Y;
            public int Order;
        }

        /// <summary>
        /// Draw link lines between metadata items (same color and/or same value). Call after map characters, before <see cref="RenderMetadataLayer"/>.
        /// </summary>
        public static void RenderMetadataLinkLines(AtariMap map, Graphics graphics, Rectangle viewport, int zoom)
        {
            _ = viewport;
            if (map == null || map.ScreenMetadata == null || graphics == null)
                return;
            if (!Globals.MetadataLayerShowColorLinks && !Globals.MetadataLayerShowValueLinks)
                return;

            var byColor = new Dictionary<byte, List<MetaEndpoint>>();
            var byValue = new Dictionary<int, List<MetaEndpoint>>();

            for (int screenY = 0; screenY < map.MapSize.Height; screenY++)
            {
                for (int screenX = 0; screenX < map.MapSize.Width; screenX++)
                {
                    List<MetadataLayerItem> items = MetadataParser.GetScreenMetadataItems(map, screenX, screenY);
                    if (items == null || items.Count == 0)
                        continue;

                    foreach (var item in items)
                    {
                        if (!TryGetMetadataItemCenter(map, screenX, screenY, item, zoom, out float cx, out float cy))
                            continue;

                        int order = MetadataTravelOrder(screenX, screenY, item);

                        if (Globals.MetadataLayerShowColorLinks)
                        {
                            byte c = item.Color;
                            if (!byColor.TryGetValue(c, out var colorList))
                            {
                                colorList = new List<MetaEndpoint>();
                                byColor[c] = colorList;
                            }
                            colorList.Add(new MetaEndpoint { X = cx, Y = cy, Order = order });
                        }

                        if (Globals.MetadataLayerShowValueLinks)
                        {
                            int v = item.Value;
                            if (!byValue.TryGetValue(v, out var valueList))
                            {
                                valueList = new List<MetaEndpoint>();
                                byValue[v] = valueList;
                            }
                            valueList.Add(new MetaEndpoint { X = cx, Y = cy, Order = order });
                        }
                    }
                }
            }

            float penWidthColor = Math.Max(1f, zoom);
            float penWidthValue = Math.Max(0.7f, zoom * 0.45f);
            SmoothingMode prevSmooth = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            try
            {
                if (Globals.MetadataLayerShowColorLinks)
                {
                    foreach (var kv in byColor)
                    {
                        List<MetaEndpoint> pts = kv.Value;
                        if (pts.Count < 2)
                            continue;
                        pts.Sort((a, b) => a.Order.CompareTo(b.Order));
                        Color lineCol = AtariPalette.GetColor(kv.Key);
                        using (var pen = new Pen(Color.FromArgb(230, lineCol.R, lineCol.G, lineCol.B), penWidthColor))
                        {
                            for (int i = 0; i < pts.Count - 1; i++)
                                graphics.DrawLine(pen, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
                        }
                    }
                }

                if (Globals.MetadataLayerShowValueLinks)
                {
                    Color vcol = AtariPalette.GetColor(ValueLinkPaletteIndex);
                    using (var pen = new Pen(Color.FromArgb(230, vcol.R, vcol.G, vcol.B), penWidthValue))
                    {
                        foreach (var kv in byValue)
                        {
                            List<MetaEndpoint> pts = kv.Value;
                            if (pts.Count < 2)
                                continue;
                            pts.Sort((a, b) => a.Order.CompareTo(b.Order));
                            for (int i = 0; i < pts.Count - 1; i++)
                                graphics.DrawLine(pen, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
                        }
                    }
                }
            }
            finally
            {
                graphics.SmoothingMode = prevSmooth;
            }
        }

        private static int MetadataTravelOrder(int screenX, int screenY, MetadataLayerItem item)
        {
            const int B = 1024;
            return (((screenY * B) + screenX) * B + item.Y) * B + item.X;
        }

        private static bool TryGetMetadataItemCenter(AtariMap map, int screenX, int screenY, MetadataLayerItem item, int zoom, out float cx, out float cy)
        {
            cx = cy = 0f;
            if (map == null || item == null)
                return false;

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

            int cellX = item.X;
            int cellY = item.Y;

            if (map.IsTilemap && map.TilemapInfo != null)
            {
                if (cellX < 0 || cellX >= map.ScreenSize.Width || cellY < 0 || cellY >= map.ScreenSize.Height)
                    return false;
            }
            else
            {
                if (cellX < 0 || cellX >= screenCharWidth || cellY < 0 || cellY >= screenCharHeight)
                    return false;
            }

            int pxPerChar = 8 * zoom;
            int offsetPxX = map.OffsetX * pxPerChar;
            int offsetPxY = map.OffsetY * pxPerChar;

            int screenPixelX = (screenX * screenCharWidth) * pxPerChar - offsetPxX;
            int screenPixelY = (screenY * screenCharHeight) * pxPerChar - offsetPxY;
            int pixelX = screenPixelX + cellX * cellW * pxPerChar;
            int pixelY = screenPixelY + cellY * cellH * pxPerChar;
            int cellPxW = cellW * pxPerChar;
            int cellPxH = cellH * pxPerChar;

            cx = pixelX + cellPxW / 2f;
            cy = pixelY + cellPxH / 2f;
            return true;
        }

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
    }
}
