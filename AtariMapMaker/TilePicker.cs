using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace AtariMapMaker
{
    public partial class TilePicker : Form
    {
        private readonly AtariMap tilemap;
        private AtariMap pickerMap; // Map containing all tiles laid out
        private readonly PictureBox clipboardPictureBox;
        private int tileWidth;
        private int tileHeight;
        private int tilesPerRow;
        private int tilesPerColumn;
        private AtariMap submap; // Cached submap
        public const Globals.WindowType window = Globals.WindowType.CharPicker;
        private static int savedTilePickerLayoutIndex = 3; // default 16 per row

        public TilePicker(AtariMap tilemap, PictureBox clipboardPictureBox)
        {
            this.tilemap = tilemap;
            this.clipboardPictureBox = clipboardPictureBox;
            
            if (tilemap == null || !tilemap.IsTilemap || string.IsNullOrEmpty(tilemap.SubmapPath) || !File.Exists(tilemap.SubmapPath))
                throw new Exception("Invalid tilemap configuration");

            submap = SubmapManager.LoadSubmap(tilemap.SubmapPath);
            tileWidth = submap.ScreenSize.Width;
            tileHeight = submap.ScreenSize.Height;
            
            InitializeComponent();
            comboBoxTilePickerLayout.SelectedIndex = savedTilePickerLayoutIndex;
            tilesPerRow = GetTilesPerRowFromLayout();
            CreatePickerMap();
            RenderTiles();
        }

        private int GetTilesPerRowFromLayout()
        {
            int idx = comboBoxTilePickerLayout.SelectedIndex;
            if (idx < 0) return 16;
            // Combo items: 4, 8, 12, 16, 20, 24, 32 (index 6 is 32, not 28)
            return idx == 6 ? 32 : (4 + idx * 4);
        }

        private void CheckBoxSkipEmptyRows_CheckedChanged(object sender, EventArgs e)
        {
            CreatePickerMap();
            RenderTiles();
        }

        /// <summary>When Skip empty rows is checked, returns the number of rows to show (from bottom up: skip rows that are entirely empty).</summary>
        private int GetVisibleRowCount(int totalTiles, int cols)
        {
            if (checkBoxSkipEmptyRows == null || !checkBoxSkipEmptyRows.Checked || totalTiles <= 0 || cols <= 0)
                return (int)Math.Ceiling((double)totalTiles / cols);
            int fullRows = (int)Math.Ceiling((double)totalTiles / cols);
            if (fullRows <= 0) return 1;
            // From bottom row to top: find first row that has at least one non-empty tile
            for (int row = fullRows - 1; row >= 0; row--)
            {
                int tileStart = row * cols;
                int tileEnd = Math.Min(tileStart + cols, totalTiles);
                for (int t = tileStart; t < tileEnd; t++)
                {
                    if (!IsTileEmpty(t)) return row + 1;
                }
            }
            return 1;
        }

        private bool IsTileEmpty(int tileIndex)
        {
            int screenX = tileIndex % submap.MapSize.Width;
            int screenY = tileIndex / submap.MapSize.Width;
            int screenStartX = screenX * submap.ScreenSize.Width;
            int screenStartY = screenY * submap.ScreenSize.Height;
            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    int idx = (screenStartX + x) + (screenStartY + y) * submap.Stride;
                    if (idx < submap.Data.Length && submap.Data[idx] != 0) return false;
                }
            }
            return true;
        }

        private void ComboBoxTilePickerLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTilePickerLayout.SelectedIndex < 0) return;
            savedTilePickerLayoutIndex = comboBoxTilePickerLayout.SelectedIndex;
            tilesPerRow = GetTilesPerRowFromLayout();
            CreatePickerMap();
            RenderTiles();
        }

        private void CreatePickerMap()
        {
            int totalTiles = submap.MapSize.Width * submap.MapSize.Height;
            int mapWidth = tilesPerRow;
            int mapHeight = GetVisibleRowCount(totalTiles, tilesPerRow);
            if (mapHeight < 1) mapHeight = 1;
            
            pickerMap = new AtariMap(new Size(mapWidth, mapHeight), new Size(tileWidth, tileHeight));
            
            // Copy tiles from submap to picker map; fill partial row with empty (tile 0)
            for (int cellIndex = 0; cellIndex < mapWidth * mapHeight; cellIndex++)
            {
                int pickerX = cellIndex % tilesPerRow;
                int pickerY = cellIndex / tilesPerRow;
                int tileIndex = cellIndex < totalTiles ? cellIndex : 0;
                
                int screenX = tileIndex % submap.MapSize.Width;
                int screenY = tileIndex / submap.MapSize.Width;
                int screenStartX = screenX * submap.ScreenSize.Width;
                int screenStartY = screenY * submap.ScreenSize.Height;
                
                for (int y = 0; y < tileHeight; y++)
                {
                    for (int x = 0; x < tileWidth; x++)
                    {
                        int srcX = screenStartX + x;
                        int srcY = screenStartY + y;
                        int srcIndex = srcX + srcY * submap.Stride;
                        int dstX = pickerX * tileWidth + x;
                        int dstY = pickerY * tileHeight + y;
                        int dstIndex = dstX + dstY * pickerMap.Stride;
                        
                        if (srcIndex < submap.Data.Length && dstIndex < pickerMap.Data.Length)
                            pickerMap.Data[dstIndex] = submap.Data[srcIndex];
                    }
                }
            }
            
            // Copy fonts from submap
            if (submap.FontDataArray != null)
            {
                for (int i = 0; i < submap.FontDataArray.Length; i++)
                {
                    if (submap.FontDataArray[i] != null)
                        pickerMap.SetFontData(submap.FontDataArray[i], i);
                }
            }
            
            // Copy font line mappings from submap
            // Each tile in pickerMap is treated as a "screen", so we need to copy font mappings correctly
            // If a tile has a reference assigned, use lines from referenced tile. Otherwise use the tile's own lines.
            if (submap.FontLineMappingPerScreen != null)
            {
                // Initialize font line mappings if not already done
                if (pickerMap.FontLineMappingPerScreen == null)
                {
                    pickerMap.FontLineMappingPerScreen = new byte[pickerMap.MapSize.Width * pickerMap.MapSize.Height * pickerMap.ScreenSize.Height];
                }
                
                for (int cellIndex = 0; cellIndex < mapWidth * mapHeight; cellIndex++)
                {
                    int pickerX = cellIndex % tilesPerRow;
                    int pickerY = cellIndex / tilesPerRow;
                    int tileIndex = cellIndex < totalTiles ? cellIndex : 0;
                    
                    // Get the submap screen (tile) coordinates
                    int submapScreenX = tileIndex % submap.MapSize.Width;
                    int submapScreenY = tileIndex / submap.MapSize.Width;
                    
                    // Check if this tile references another tile
                    // Use GetReferencedScreen logic inline since it's private
                    int actualSubmapScreenX = submapScreenX;
                    int actualSubmapScreenY = submapScreenY;
                    if (submap.FontLineMappingReferences != null)
                    {
                        string key = $"{submapScreenX},{submapScreenY}";
                        if (submap.FontLineMappingReferences.ContainsKey(key))
                        {
                            ScreenReference refScreen = submap.FontLineMappingReferences[key];
                            // Only use reference if it's different from the tile itself
                            if (refScreen.X != submapScreenX || refScreen.Y != submapScreenY)
                            {
                                actualSubmapScreenX = refScreen.X;
                                actualSubmapScreenY = refScreen.Y;
                            }
                        }
                    }
                    
                    // Calculate offsets for font line mapping using the actual (possibly referenced) screen
                    // In submap: each screen has ScreenSize.Height lines
                    int submapScreenOffset = (actualSubmapScreenY * submap.MapSize.Width + actualSubmapScreenX) * submap.ScreenSize.Height;
                    
                    // In pickerMap: each "screen" (tile) has ScreenSize.Height lines (which equals tileHeight)
                    int pickerScreenOffset = (pickerY * pickerMap.MapSize.Width + pickerX) * pickerMap.ScreenSize.Height;
                    
                    // Copy font mappings for each line of the tile from the actual (possibly referenced) screen
                    for (int line = 0; line < tileHeight && line < pickerMap.ScreenSize.Height; line++)
                    {
                        int submapIndex = submapScreenOffset + line;
                        int pickerIndex = pickerScreenOffset + line;
                        if (submapIndex >= 0 && submapIndex < submap.FontLineMappingPerScreen.Length && 
                            pickerIndex >= 0 && pickerIndex < pickerMap.FontLineMappingPerScreen.Length)
                        {
                            pickerMap.FontLineMappingPerScreen[pickerIndex] = submap.FontLineMappingPerScreen[submapIndex];
                        }
                    }
                }
            }
            else
            {
                // If submap has no font line mappings, initialize to all font 0
                if (pickerMap.FontLineMappingPerScreen == null)
                {
                    pickerMap.FontLineMappingPerScreen = new byte[pickerMap.MapSize.Width * pickerMap.MapSize.Height * pickerMap.ScreenSize.Height];
                }
            }
            
            // Ensure multi-font is enabled for picker map (fonts come from tiles)
            pickerMap.MultiFontEnabled = true;
            
            // Initialize font line mapping references - each tile should reference itself (no cross-referencing)
            if (pickerMap.FontLineMappingReferences == null)
            {
                pickerMap.FontLineMappingReferences = new Dictionary<string, ScreenReference>();
            }
            
            // Set each tile to reference itself
            for (int ty = 0; ty < mapHeight; ty++)
            {
                for (int tx = 0; tx < mapWidth; tx++)
                {
                    string key = $"{tx},{ty}";
                    pickerMap.FontLineMappingReferences[key] = new ScreenReference(tx, ty);
                }
            }
        }

        private void RenderTiles()
        {
            int totalTiles = submap.MapSize.Width * submap.MapSize.Height;
            int mapWidth = tilesPerRow;
            int mapHeight = GetVisibleRowCount(totalTiles, tilesPerRow);
            if (mapHeight < 1) mapHeight = 1;
            
            int newWidth = mapWidth * tileWidth * Globals.CharSize;
            int newHeight = mapHeight * tileHeight * Globals.CharSize;
            
            if (pictureBoxTilePicker.Width != newWidth || pictureBoxTilePicker.Height != newHeight)
            {
                pictureBoxTilePicker.Width = newWidth;
                pictureBoxTilePicker.Height = newHeight;
                this.ClientSize = new Size(newWidth + 10, newHeight + 30);
            }
            
            pictureBoxTilePicker.Image = new Bitmap(pictureBoxTilePicker.Width, pictureBoxTilePicker.Height);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxTilePicker.Image, pickerMap);
            // For tilepicker: draw grid points only (no screen lines), one point per tile
            AtariPictureTools.Redraw(window, true, false, false); // drawScreenBorders=false, drawGrid=false (we'll draw our own grid)
            DrawTileGrid();
        }
        
        private void DrawTileGrid()
        {
            if (pictureBoxTilePicker.Image == null || pickerMap == null) return;
            
            using (Graphics gr = Graphics.FromImage(pictureBoxTilePicker.Image))
            {
                int mapWidth = pickerMap.MapSize.Width;
                int mapHeight = pickerMap.MapSize.Height;
                
                int tilePixelWidth = tileWidth * Globals.CharSize;
                int tilePixelHeight = tileHeight * Globals.CharSize;
                
                // Draw grid points at tile boundaries (one point per tile)
                using (Brush gridBrush = new SolidBrush(Color.White))
                {
                    for (int ty = 0; ty <= mapHeight; ty++)
                    {
                        for (int tx = 0; tx <= mapWidth; tx++)
                        {
                            int pixelX = tx * tilePixelWidth;
                            int pixelY = ty * tilePixelHeight;
                            gr.FillRectangle(gridBrush, pixelX, pixelY, 1, 1);
                        }
                    }
                }
            }
            pictureBoxTilePicker.Refresh();
        }

        // Tile-based selection state
        private Rectangle tileSelection = Rectangle.Empty;
        private Point tileSelectionStart = Point.Empty;
        private bool isSelecting = false;
        
        private void PictureBoxTilePicker_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Snap to tile boundaries
                int tilePixelWidth = tileWidth * Globals.CharSize;
                int tilePixelHeight = tileHeight * Globals.CharSize;
                int tileX = e.X / tilePixelWidth;
                int tileY = e.Y / tilePixelHeight;
                
                tileSelectionStart = new Point(tileX, tileY);
                tileSelection = new Rectangle(tileX * tilePixelWidth, tileY * tilePixelHeight, tilePixelWidth, tilePixelHeight);
                isSelecting = true;
                pictureBoxTilePicker.Refresh();
            }
        }

        private void PictureBoxTilePicker_MouseMove(object sender, MouseEventArgs e)
        {
            int tilePixelWidth = tileWidth * Globals.CharSize;
            int tilePixelHeight = tileHeight * Globals.CharSize;
            int tileX = e.X / tilePixelWidth;
            int tileY = e.Y / tilePixelHeight;
            int tileIndex = tileY * tilesPerRow + tileX;
            
            if (tileIndex < submap.MapSize.Width * submap.MapSize.Height)
            {
                this.Text = $"Tile Picker - Tile {tileIndex} ({tileX}, {tileY})";
            }
            
            if (isSelecting && e.Button == MouseButtons.Left)
            {
                // Update selection rectangle, snapping to tile boundaries
                int startTileX = Math.Min(tileSelectionStart.X, tileX);
                int startTileY = Math.Min(tileSelectionStart.Y, tileY);
                int endTileX = Math.Max(tileSelectionStart.X, tileX);
                int endTileY = Math.Max(tileSelectionStart.Y, tileY);
                
                tileSelection = new Rectangle(
                    startTileX * tilePixelWidth,
                    startTileY * tilePixelHeight,
                    (endTileX - startTileX + 1) * tilePixelWidth,
                    (endTileY - startTileY + 1) * tilePixelHeight);
                
                pictureBoxTilePicker.Refresh();
            }
        }

        private void PictureBoxTilePicker_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;
                
                if (tileSelection.Width > 0 && tileSelection.Height > 0)
                {
                    // Calculate which tiles are selected
                    int startTileX = tileSelection.X / (tileWidth * Globals.CharSize);
                    int startTileY = tileSelection.Y / (tileHeight * Globals.CharSize);
                    int endTileX = (tileSelection.X + tileSelection.Width - 1) / (tileWidth * Globals.CharSize);
                    int endTileY = (tileSelection.Y + tileSelection.Height - 1) / (tileHeight * Globals.CharSize);
                    
                    int selectedTileWidth = endTileX - startTileX + 1;
                    int selectedTileHeight = endTileY - startTileY + 1;
                    
                    // Collect tile indexes and build preview by rendering selected tiles (same rendering as picker – correct colors/fonts)
                    List<byte> tileIndexes = new List<byte>();
                    for (int ty = startTileY; ty <= endTileY; ty++)
                    {
                        for (int tx = startTileX; tx <= endTileX; tx++)
                        {
                            int pickerTileIndex = ty * tilesPerRow + tx;
                            if (pickerTileIndex < submap.MapSize.Width * submap.MapSize.Height)
                                tileIndexes.Add((byte)pickerTileIndex);
                        }
                    }
                    int previewCharWidth = selectedTileWidth * tileWidth;
                    int previewCharHeight = selectedTileHeight * tileHeight;
                    int previewPixelWidthNoZoom = previewCharWidth * 8;
                    int previewPixelHeightNoZoom = previewCharHeight * 8;
                    Bitmap previewImage = new Bitmap(previewPixelWidthNoZoom, previewPixelHeightNoZoom, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    previewImage.Palette = AtariPalette.GetPalette();
                    AtariMap tempMap = new AtariMap(new Size(selectedTileWidth, selectedTileHeight), new Size(tileWidth, tileHeight));
                    tempMap.MultiFontEnabled = true;
                    tempMap.FontLineMappingPerScreen = new byte[tempMap.MapSize.Width * tempMap.MapSize.Height * tempMap.ScreenSize.Height];
                    for (int ty = 0; ty < selectedTileHeight; ty++)
                    {
                        for (int tx = 0; tx < selectedTileWidth; tx++)
                        {
                            int pickerTileIndex = (startTileY + ty) * tilesPerRow + (startTileX + tx);
                            if (pickerTileIndex < submap.MapSize.Width * submap.MapSize.Height)
                            {
                                int submapScreenX = pickerTileIndex % submap.MapSize.Width;
                                int submapScreenY = pickerTileIndex / submap.MapSize.Width;
                                int submapScreenStartX = submapScreenX * submap.ScreenSize.Width;
                                int submapScreenStartY = submapScreenY * submap.ScreenSize.Height;
                                for (int y = 0; y < tileHeight; y++)
                                {
                                    for (int x = 0; x < tileWidth; x++)
                                    {
                                        int srcIndex = (submapScreenStartX + x) + (submapScreenStartY + y) * submap.Stride;
                                        int dstIndex = (tx * tileWidth + x) + (ty * tileHeight + y) * tempMap.Stride;
                                        if (srcIndex < submap.Data.Length && dstIndex < tempMap.Data.Length)
                                            tempMap.Data[dstIndex] = submap.Data[srcIndex];
                                    }
                                }
                                if (submap.FontLineMappingPerScreen != null)
                                {
                                    int actualSx = submapScreenX;
                                    int actualSy = submapScreenY;
                                    if (submap.FontLineMappingReferences != null)
                                    {
                                        string key = $"{submapScreenX},{submapScreenY}";
                                        if (submap.FontLineMappingReferences.ContainsKey(key))
                                        {
                                            var refScreen = submap.FontLineMappingReferences[key];
                                            if (refScreen.X != submapScreenX || refScreen.Y != submapScreenY)
                                            {
                                                actualSx = refScreen.X;
                                                actualSy = refScreen.Y;
                                            }
                                        }
                                    }
                                    int submapScreenOffset = (actualSy * submap.MapSize.Width + actualSx) * submap.ScreenSize.Height;
                                    int tempScreenOffset = (ty * tempMap.MapSize.Width + tx) * tempMap.ScreenSize.Height;
                                    for (int line = 0; line < tileHeight; line++)
                                    {
                                        int submapIndex = submapScreenOffset + line;
                                        int tempIndex = tempScreenOffset + line;
                                        if (submapIndex < submap.FontLineMappingPerScreen.Length && tempIndex < tempMap.FontLineMappingPerScreen.Length)
                                            tempMap.FontLineMappingPerScreen[tempIndex] = submap.FontLineMappingPerScreen[submapIndex];
                                    }
                                }
                                else
                                {
                                    int tempScreenOffset = (ty * tempMap.MapSize.Width + tx) * tempMap.ScreenSize.Height;
                                    for (int line = 0; line < tileHeight; line++)
                                    {
                                        int tempIndex = tempScreenOffset + line;
                                        if (tempIndex < tempMap.FontLineMappingPerScreen.Length)
                                            tempMap.FontLineMappingPerScreen[tempIndex] = 0;
                                    }
                                }
                            }
                        }
                    }
                    if (submap.FontDataArray != null)
                    {
                        for (int i = 0; i < submap.FontDataArray.Length; i++)
                            if (submap.FontDataArray[i] != null)
                                tempMap.SetFontData(submap.FontDataArray[i], i);
                    }
                    if (tempMap.FontLineMappingPerScreen == null)
                    {
                        tempMap.FontLineMappingPerScreen = new byte[tempMap.MapSize.Width * tempMap.MapSize.Height * tempMap.ScreenSize.Height];
                        for (int i = 0; i < tempMap.FontLineMappingPerScreen.Length; i++)
                            tempMap.FontLineMappingPerScreen[i] = 0;
                    }
                    AtariFontRenderer.RenderMapData(tempMap, Globals.FontType.Screen, previewImage);
                    int previewPixelWidthWithZoom = previewPixelWidthNoZoom * Globals.Zoom;
                    int previewPixelHeightWithZoom = previewPixelHeightNoZoom * Globals.Zoom;
                    Bitmap previewImageWithZoom = new Bitmap(previewPixelWidthWithZoom, previewPixelHeightWithZoom, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    previewImageWithZoom.Palette = AtariPalette.GetPalette();
                    var srcBd = previewImage.LockBits(new Rectangle(0, 0, previewPixelWidthNoZoom, previewPixelHeightNoZoom), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    var dstBd = previewImageWithZoom.LockBits(new Rectangle(0, 0, previewPixelWidthWithZoom, previewPixelHeightWithZoom), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    unsafe
                    {
                        byte* srcPtr = (byte*)srcBd.Scan0;
                        byte* dstPtr = (byte*)dstBd.Scan0;
                        for (int y = 0; y < previewPixelHeightWithZoom; y++)
                        {
                            int sy = y / Globals.Zoom;
                            if (sy >= previewPixelHeightNoZoom) sy = previewPixelHeightNoZoom - 1;
                            for (int x = 0; x < previewPixelWidthWithZoom; x++)
                            {
                                int sx = x / Globals.Zoom;
                                if (sx >= previewPixelWidthNoZoom) sx = previewPixelWidthNoZoom - 1;
                                dstPtr[y * dstBd.Stride + x] = srcPtr[sy * srcBd.Stride + sx];
                            }
                        }
                    }
                    previewImage.UnlockBits(srcBd);
                    previewImageWithZoom.UnlockBits(dstBd);
                    previewImage.Dispose();
                    AtariClipboard.SetDataSource(tilemap);
                    AtariClipboard.CopyTileIndexes(tileIndexes.ToArray(), selectedTileWidth, selectedTileHeight, previewImageWithZoom);
                    AtariClipboard.IsValid = true;
                    clipboardPictureBox.Image = AtariClipboard.ClipboardImage;
                }
            }
        }
        
        private void PictureBoxTilePicker_Paint(object sender, PaintEventArgs e)
        {
            // Draw selection rectangle if selecting
            if (isSelecting && tileSelection.Width > 0 && tileSelection.Height > 0)
            {
                using (Pen selectionPen = new Pen(Color.Lime, 2))
                {
                    e.Graphics.DrawRectangle(selectionPen, tileSelection);
                }
            }
        }

        private void TilePicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide instead of close so the form is never disposed (only when app exits)
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        public void Refresh()
        {
            if (pickerMap != null)
            {
                // Ensure multi-font is enabled for picker map (fonts come from tiles)
                pickerMap.MultiFontEnabled = true;
                RenderTiles();
                pictureBoxTilePicker.Refresh();
            }
        }
    }
}
