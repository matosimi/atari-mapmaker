using System;
using System.IO;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class TilemapConfigDialog : Form
    {
        private AtariMap map;
        private TextBox textBoxSubmapPath;
        private Button buttonBrowseSubmap;
        private Label labelTileInfo;
        private ComboBox comboBoxNumberingPattern;
        private CheckBox checkBoxShowByteOverlay;
        private TrackBar trackBarOverlayTransparency;
        private Label labelTransparency;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonRenumberTiles;

        public TilemapConfigDialog(AtariMap map)
        {
            this.map = map;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.textBoxSubmapPath = new TextBox();
            this.buttonBrowseSubmap = new Button();
            this.labelTileInfo = new Label();
            this.comboBoxNumberingPattern = new ComboBox();
            this.checkBoxShowByteOverlay = new CheckBox();
            this.trackBarOverlayTransparency = new TrackBar();
            this.labelTransparency = new Label();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.buttonRenumberTiles = new Button();
            this.SuspendLayout();

            // Label for Submap Path
            Label labelSubmap = new Label();
            labelSubmap.AutoSize = true;
            labelSubmap.Location = new System.Drawing.Point(12, 12);
            labelSubmap.Text = "Submap File:";
            labelSubmap.Size = new System.Drawing.Size(70, 13);

            // textBoxSubmapPath
            this.textBoxSubmapPath.Location = new System.Drawing.Point(88, 9);
            this.textBoxSubmapPath.Name = "textBoxSubmapPath";
            this.textBoxSubmapPath.Size = new System.Drawing.Size(300, 20);
            this.textBoxSubmapPath.TabIndex = 1;
            this.textBoxSubmapPath.ReadOnly = true;

            // buttonBrowseSubmap
            this.buttonBrowseSubmap.Location = new System.Drawing.Point(394, 7);
            this.buttonBrowseSubmap.Name = "buttonBrowseSubmap";
            this.buttonBrowseSubmap.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseSubmap.Text = "Browse...";
            this.buttonBrowseSubmap.TabIndex = 2;
            this.buttonBrowseSubmap.UseVisualStyleBackColor = true;
            this.buttonBrowseSubmap.Click += ButtonBrowseSubmap_Click;

            // Label for Tile Info (read-only, from submap)
            Label labelTileInfoLabel = new Label();
            labelTileInfoLabel.AutoSize = true;
            labelTileInfoLabel.Location = new System.Drawing.Point(12, 42);
            labelTileInfoLabel.Text = "Tile Info:";
            labelTileInfoLabel.Size = new System.Drawing.Size(60, 13);

            // labelTileInfo (displays tile info from submap)
            this.labelTileInfo.AutoSize = true;
            this.labelTileInfo.Location = new System.Drawing.Point(78, 42);
            this.labelTileInfo.Name = "labelTileInfo";
            this.labelTileInfo.Size = new System.Drawing.Size(200, 13);
            this.labelTileInfo.Text = "(No submap loaded)";

            // Label for Numbering Pattern
            Label labelPattern = new Label();
            labelPattern.AutoSize = true;
            labelPattern.Location = new System.Drawing.Point(12, 72);
            labelPattern.Text = "Numbering Pattern:";
            labelPattern.Size = new System.Drawing.Size(100, 13);

            // comboBoxNumberingPattern
            this.comboBoxNumberingPattern.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxNumberingPattern.Location = new System.Drawing.Point(118, 69);
            this.comboBoxNumberingPattern.Name = "comboBoxNumberingPattern";
            this.comboBoxNumberingPattern.Size = new System.Drawing.Size(150, 21);
            this.comboBoxNumberingPattern.TabIndex = 5;
            this.comboBoxNumberingPattern.Items.AddRange(new string[] { "row-major", "column-major" });

            // buttonRenumberTiles
            this.buttonRenumberTiles.Location = new System.Drawing.Point(274, 67);
            this.buttonRenumberTiles.Name = "buttonRenumberTiles";
            this.buttonRenumberTiles.Size = new System.Drawing.Size(100, 23);
            this.buttonRenumberTiles.Text = "Renumber Tiles";
            this.buttonRenumberTiles.TabIndex = 6;
            this.buttonRenumberTiles.UseVisualStyleBackColor = true;
            this.buttonRenumberTiles.Click += ButtonRenumberTiles_Click;

            // checkBoxShowByteOverlay
            this.checkBoxShowByteOverlay.AutoSize = true;
            this.checkBoxShowByteOverlay.Location = new System.Drawing.Point(12, 102);
            this.checkBoxShowByteOverlay.Name = "checkBoxShowByteOverlay";
            this.checkBoxShowByteOverlay.Size = new System.Drawing.Size(120, 17);
            this.checkBoxShowByteOverlay.Text = "Show Byte Overlay";
            this.checkBoxShowByteOverlay.TabIndex = 7;

            // Label for Transparency
            this.labelTransparency.AutoSize = true;
            this.labelTransparency.Location = new System.Drawing.Point(12, 127);
            this.labelTransparency.Text = "Overlay Transparency: 50%";
            this.labelTransparency.Size = new System.Drawing.Size(150, 13);

            // trackBarOverlayTransparency
            this.trackBarOverlayTransparency.Location = new System.Drawing.Point(168, 122);
            this.trackBarOverlayTransparency.Name = "trackBarOverlayTransparency";
            this.trackBarOverlayTransparency.Size = new System.Drawing.Size(200, 45);
            this.trackBarOverlayTransparency.Minimum = 0;
            this.trackBarOverlayTransparency.Maximum = 100;
            this.trackBarOverlayTransparency.Value = 50;
            this.trackBarOverlayTransparency.TickFrequency = 10;
            this.trackBarOverlayTransparency.TabIndex = 8;
            this.trackBarOverlayTransparency.ValueChanged += TrackBarOverlayTransparency_ValueChanged;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(313, 172);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(394, 172);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // TilemapConfigDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(481, 207);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.trackBarOverlayTransparency);
            this.Controls.Add(this.labelTransparency);
            this.Controls.Add(this.checkBoxShowByteOverlay);
            this.Controls.Add(this.buttonRenumberTiles);
            this.Controls.Add(this.comboBoxNumberingPattern);
            this.Controls.Add(this.labelTileInfo);
            this.Controls.Add(this.buttonBrowseSubmap);
            this.Controls.Add(this.textBoxSubmapPath);
            this.Controls.Add(labelSubmap);
            this.Controls.Add(labelTileInfoLabel);
            this.Controls.Add(labelPattern);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TilemapConfigDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Tilemap Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            if (map == null || !map.IsTilemap) return;

            if (!string.IsNullOrEmpty(map.SubmapPath))
            {
                textBoxSubmapPath.Text = map.SubmapPath;
                UpdateTileInfo();
            }
            else
            {
                labelTileInfo.Text = "(No submap loaded)";
            }

            if (map.TilemapInfo != null)
            {
                if (!string.IsNullOrEmpty(map.TilemapInfo.NumberingPattern))
                {
                    int index = comboBoxNumberingPattern.Items.IndexOf(map.TilemapInfo.NumberingPattern);
                    if (index >= 0)
                        comboBoxNumberingPattern.SelectedIndex = index;
                    else
                        comboBoxNumberingPattern.SelectedIndex = 0;
                }
                else
                {
                    comboBoxNumberingPattern.SelectedIndex = 0;
                }

                checkBoxShowByteOverlay.Checked = map.TilemapInfo.ShowByteOverlay;
                trackBarOverlayTransparency.Value = (int)(map.TilemapInfo.ByteOverlayTransparency * 100);
                UpdateTransparencyLabel();
            }
            else
            {
                // Default values
                comboBoxNumberingPattern.SelectedIndex = 0;
                checkBoxShowByteOverlay.Checked = false;
                trackBarOverlayTransparency.Value = 50;
                UpdateTransparencyLabel();
            }

            UpdateUI();
        }

        private void UpdateTileInfo()
        {
            if (string.IsNullOrEmpty(textBoxSubmapPath.Text) || !File.Exists(textBoxSubmapPath.Text))
            {
                labelTileInfo.Text = "(No submap loaded)";
                return;
            }

            try
            {
                AtariMap submap = SubmapManager.LoadSubmap(textBoxSubmapPath.Text);
                int tileWidth = submap.ScreenSize.Width;
                int tileHeight = submap.ScreenSize.Height;
                int totalTiles = submap.MapSize.Width * submap.MapSize.Height;
                labelTileInfo.Text = $"{totalTiles} tiles of size {tileWidth}x{tileHeight} linked";
            }
            catch
            {
                labelTileInfo.Text = "(Error loading submap)";
            }
        }

        private void UpdateUI()
        {
            // All controls are always enabled for tilemap (since this dialog only opens for tilemaps)
            bool overlayEnabled = checkBoxShowByteOverlay.Checked;
            trackBarOverlayTransparency.Enabled = overlayEnabled;
        }

        private void ButtonBrowseSubmap_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Atari Map Files (*.atrmap)|*.atrmap|All Files (*.*)|*.*";
            openDialog.Title = "Select Submap File";
            
            if (!string.IsNullOrEmpty(textBoxSubmapPath.Text))
            {
                string directory = Path.GetDirectoryName(textBoxSubmapPath.Text);
                if (Directory.Exists(directory))
                    openDialog.InitialDirectory = directory;
            }

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSubmapPath.Text = openDialog.FileName;
                UpdateTileInfo();
            }
        }

        private void ButtonRenumberTiles_Click(object sender, EventArgs e)
        {
            if (map == null || !map.IsTilemap) return;

            if (map.TilemapInfo == null)
            {
                map.TilemapInfo = new TilemapData();
            }

            string pattern = comboBoxNumberingPattern.SelectedItem?.ToString() ?? "row-major";
            TilemapEditor.ApplyNumberingPattern(map, pattern);
            
            MessageBox.Show("Tiles have been renumbered using the selected pattern.", "Renumber Tiles", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TrackBarOverlayTransparency_ValueChanged(object sender, EventArgs e)
        {
            UpdateTransparencyLabel();
        }

        private void UpdateTransparencyLabel()
        {
            labelTransparency.Text = $"Overlay Transparency: {trackBarOverlayTransparency.Value}%";
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (map == null || !map.IsTilemap) return;

            // Set submap path
            if (!string.IsNullOrEmpty(textBoxSubmapPath.Text))
            {
                if (File.Exists(textBoxSubmapPath.Text))
                {
                    map.SubmapPath = textBoxSubmapPath.Text;
                    map.ClearSubmapCache(); // Clear cache when submap path changes
                }
                else
                {
                    MessageBox.Show("Submap file does not exist. Please select a valid file.", 
                        "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }
            else
            {
                MessageBox.Show("Submap file is required for tilemap. Please select a submap file.", 
                    "Submap Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Load submap to get tile dimensions and inherit fonts
            if (!string.IsNullOrEmpty(textBoxSubmapPath.Text) && File.Exists(textBoxSubmapPath.Text))
            {
                try
                {
                    AtariMap submap = SubmapManager.LoadSubmap(textBoxSubmapPath.Text);
                    
                    // Initialize or update TilemapInfo
                    if (map.TilemapInfo == null)
                    {
                        map.TilemapInfo = new TilemapData();
                    }

                    // Tile size comes from submap screen size
                    map.TilemapInfo.TileWidth = submap.ScreenSize.Width;
                    map.TilemapInfo.TileHeight = submap.ScreenSize.Height;
                    map.TilemapInfo.NumberingPattern = comboBoxNumberingPattern.SelectedItem?.ToString() ?? "row-major";
                    map.TilemapInfo.ShowByteOverlay = checkBoxShowByteOverlay.Checked;
                    map.TilemapInfo.ByteOverlayTransparency = trackBarOverlayTransparency.Value / 100.0f;
                    
                    // Inherit all fonts from submap
                    if (submap.FontDataArray != null)
                    {
                        for (int i = 0; i < submap.FontDataArray.Length && i < 8; i++)
                        {
                            if (submap.FontDataArray[i] != null)
                            {
                                string fontFileName = (submap.FontFileNames != null && i < submap.FontFileNames.Length) 
                                    ? submap.FontFileNames[i] : null;
                                map.SetFontData(submap.FontDataArray[i], i, fontFileName);
                            }
                        }
                    }
                    
                    // Set first font as active screen font if available
                    if (submap.FontDataArray != null && submap.FontDataArray[0] != null)
                    {
                        AtariFontRenderer.SetFontData(submap.FontDataArray[0], Globals.FontType.Screen);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading submap: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }
        }
    }
}
