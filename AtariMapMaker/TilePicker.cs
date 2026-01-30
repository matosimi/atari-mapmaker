using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace AtariMapMaker
{
    public partial class TilePicker : Form
    {
        private readonly AtariMap tilemap;
        private AtariMap pickerMap; // Map containing all tiles laid out
        private readonly PictureBox clipboardPictureBox;
        private PictureBox pictureBoxTilePicker;
        private int tileWidth;
        private int tileHeight;
        private int tilesPerRow;
        private int tilesPerColumn;
        private AtariMap submap; // Cached submap
        public const Globals.WindowType window = Globals.WindowType.CharPicker;

        public TilePicker(AtariMap tilemap, PictureBox clipboardPictureBox)
        {
            this.tilemap = tilemap;
            this.clipboardPictureBox = clipboardPictureBox;
            
            if (tilemap == null || !tilemap.IsTilemap || string.IsNullOrEmpty(tilemap.SubmapPath) || !File.Exists(tilemap.SubmapPath))
                throw new Exception("Invalid tilemap configuration");

            // Load submap
            submap = SubmapManager.LoadSubmap(tilemap.SubmapPath);
            tileWidth = submap.ScreenSize.Width;
            tileHeight = submap.ScreenSize.Height;
            
            // Calculate how many tiles fit in the picker window
            int windowWidth = 400; // pixels
            int windowHeight = 400; // pixels
            tilesPerRow = Math.Max(1, windowWidth / (tileWidth * Globals.CharSize));
            tilesPerColumn = Math.Max(1, windowHeight / (tileHeight * Globals.CharSize));
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.pictureBoxTilePicker = new PictureBox();
            this.SuspendLayout();

            // pictureBoxTilePicker
            this.pictureBoxTilePicker.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxTilePicker.Name = "pictureBoxTilePicker";
            this.pictureBoxTilePicker.Size = new System.Drawing.Size(400, 400);
            this.pictureBoxTilePicker.TabIndex = 0;
            this.pictureBoxTilePicker.TabStop = false;
            this.pictureBoxTilePicker.MouseDown += PictureBoxTilePicker_MouseDown;
            this.pictureBoxTilePicker.MouseMove += PictureBoxTilePicker_MouseMove;
            this.pictureBoxTilePicker.MouseUp += PictureBoxTilePicker_MouseUp;

            // TilePicker
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this.pictureBoxTilePicker);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Name = "TilePicker";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Tile Picker";
            this.FormClosing += TilePicker_FormClosing;
            this.ResumeLayout(false);
            
            // Create picker map and render tiles
            CreatePickerMap();
            RenderTiles();
        }

        private void CreatePickerMap()
        {
            // Create a map that contains all tiles laid out
            int totalTiles = submap.MapSize.Width * submap.MapSize.Height;
            int mapWidth = Math.Min(tilesPerRow, totalTiles);
            int mapHeight = (int)Math.Ceiling((double)totalTiles / tilesPerRow);
            
            pickerMap = new AtariMap(new Size(mapWidth, mapHeight), new Size(tileWidth, tileHeight));
            
            // Copy tiles from submap to picker map
            for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++)
            {
                int pickerX = tileIndex % tilesPerRow;
                int pickerY = tileIndex / tilesPerRow;
                
                if (pickerX >= mapWidth || pickerY >= mapHeight)
                    continue;
                
                int screenX = tileIndex % submap.MapSize.Width;
                int screenY = tileIndex / submap.MapSize.Width;
                int screenStartX = screenX * submap.ScreenSize.Width;
                int screenStartY = screenY * submap.ScreenSize.Height;
                
                // Copy tile data
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
            if (submap.FontLineMappingPerScreen != null)
            {
                for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++)
                {
                    int pickerX = tileIndex % tilesPerRow;
                    int pickerY = tileIndex / tilesPerRow;
                    
                    if (pickerX >= mapWidth || pickerY >= mapHeight)
                        continue;
                    
                    int screenX = tileIndex % submap.MapSize.Width;
                    int screenY = tileIndex / submap.MapSize.Width;
                    int submapScreenOffset = (screenY * submap.MapSize.Width + screenX) * submap.ScreenSize.Height;
                    int pickerScreenOffset = (pickerY * mapWidth + pickerX) * pickerMap.ScreenSize.Height;
                    
                    for (int line = 0; line < tileHeight && line < pickerMap.ScreenSize.Height; line++)
                    {
                        int submapIndex = submapScreenOffset + line;
                        int pickerIndex = pickerScreenOffset + line;
                        if (submapIndex < submap.FontLineMappingPerScreen.Length && 
                            pickerIndex < pickerMap.FontLineMappingPerScreen.Length)
                        {
                            pickerMap.FontLineMappingPerScreen[pickerIndex] = submap.FontLineMappingPerScreen[submapIndex];
                        }
                    }
                }
            }
            
            pickerMap.MultiFontEnabled = submap.MultiFontEnabled;
        }

        private void RenderTiles()
        {
            // Adjust picture box size based on current zoom
            int totalTiles = submap.MapSize.Width * submap.MapSize.Height;
            int mapWidth = Math.Min(tilesPerRow, totalTiles);
            int mapHeight = (int)Math.Ceiling((double)totalTiles / tilesPerRow);
            
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
            AtariPictureTools.Redraw(window, true, false, true);
        }

        private void PictureBoxTilePicker_MouseDown(object sender, MouseEventArgs e)
        {
            // Ensure window is assigned with picker map before starting selection
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxTilePicker.Image, pickerMap);
            AtariPictureTools.PreviousMouseLocation = e.Location;
            AtariPictureTools.SelectionStart(e.Location, window);
        }

        private void PictureBoxTilePicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxTilePicker.Image, pickerMap);
                AtariPictureTools.SelectionChange(e.Location, window);
                pictureBoxTilePicker.Refresh();
            }
            
            int tileX = e.X / (tileWidth * Globals.CharSize);
            int tileY = e.Y / (tileHeight * Globals.CharSize);
            int tileIndex = tileY * tilesPerRow + tileX;
            
            if (tileIndex < submap.MapSize.Width * submap.MapSize.Height)
            {
                this.Text = $"Tile Picker - Tile {tileIndex} ({tileX}, {tileY})";
            }
        }

        private void PictureBoxTilePicker_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AtariClipboard.IsValid = AtariPictureTools.SelectionEnd(window);
                clipboardPictureBox.Image = AtariClipboard.ClipboardImage;
            }
        }

        private void TilePicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
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
