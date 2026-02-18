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
        private Label labelSubmap;
        private Label labelTileInfoLabel;
        private Label labelPattern;
        private Button buttonRenumberTiles;

        public TilemapConfigDialog(AtariMap map)
        {
            this.map = map;
            InitializeComponent();
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.textBoxSubmapPath = new System.Windows.Forms.TextBox();
            this.buttonBrowseSubmap = new System.Windows.Forms.Button();
            this.labelTileInfo = new System.Windows.Forms.Label();
            this.comboBoxNumberingPattern = new System.Windows.Forms.ComboBox();
            this.checkBoxShowByteOverlay = new System.Windows.Forms.CheckBox();
            this.trackBarOverlayTransparency = new System.Windows.Forms.TrackBar();
            this.labelTransparency = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRenumberTiles = new System.Windows.Forms.Button();
            this.labelSubmap = new System.Windows.Forms.Label();
            this.labelTileInfoLabel = new System.Windows.Forms.Label();
            this.labelPattern = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOverlayTransparency)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxSubmapPath
            // 
            this.textBoxSubmapPath.Location = new System.Drawing.Point(88, 9);
            this.textBoxSubmapPath.Name = "textBoxSubmapPath";
            this.textBoxSubmapPath.ReadOnly = true;
            this.textBoxSubmapPath.Size = new System.Drawing.Size(300, 26);
            this.textBoxSubmapPath.TabIndex = 1;
            // 
            // buttonBrowseSubmap
            // 
            this.buttonBrowseSubmap.Location = new System.Drawing.Point(394, 7);
            this.buttonBrowseSubmap.Name = "buttonBrowseSubmap";
            this.buttonBrowseSubmap.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseSubmap.TabIndex = 2;
            this.buttonBrowseSubmap.Text = "Browse...";
            this.buttonBrowseSubmap.UseVisualStyleBackColor = true;
            // 
            // labelTileInfo
            // 
            this.labelTileInfo.AutoSize = true;
            this.labelTileInfo.Location = new System.Drawing.Point(78, 42);
            this.labelTileInfo.Name = "labelTileInfo";
            this.labelTileInfo.Size = new System.Drawing.Size(152, 20);
            this.labelTileInfo.TabIndex = 12;
            this.labelTileInfo.Text = "(No submap loaded)";
            // 
            // comboBoxNumberingPattern
            // 
            this.comboBoxNumberingPattern.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNumberingPattern.Items.AddRange(new object[] {
            "row-major",
            "column-major"});
            this.comboBoxNumberingPattern.Location = new System.Drawing.Point(118, 69);
            this.comboBoxNumberingPattern.Name = "comboBoxNumberingPattern";
            this.comboBoxNumberingPattern.Size = new System.Drawing.Size(150, 28);
            this.comboBoxNumberingPattern.TabIndex = 5;
            this.comboBoxNumberingPattern.Visible = false;
            // 
            // checkBoxShowByteOverlay
            // 
            this.checkBoxShowByteOverlay.AutoSize = true;
            this.checkBoxShowByteOverlay.Location = new System.Drawing.Point(12, 102);
            this.checkBoxShowByteOverlay.Name = "checkBoxShowByteOverlay";
            this.checkBoxShowByteOverlay.Size = new System.Drawing.Size(167, 24);
            this.checkBoxShowByteOverlay.TabIndex = 7;
            this.checkBoxShowByteOverlay.Text = "Show Byte Overlay";
            this.checkBoxShowByteOverlay.CheckedChanged += new System.EventHandler(this.CheckBoxShowByteOverlay_CheckedChanged);
            // 
            // trackBarOverlayTransparency
            // 
            this.trackBarOverlayTransparency.Location = new System.Drawing.Point(168, 122);
            this.trackBarOverlayTransparency.Maximum = 100;
            this.trackBarOverlayTransparency.Name = "trackBarOverlayTransparency";
            this.trackBarOverlayTransparency.Size = new System.Drawing.Size(200, 69);
            this.trackBarOverlayTransparency.TabIndex = 8;
            this.trackBarOverlayTransparency.TickFrequency = 10;
            this.trackBarOverlayTransparency.Value = 50;
            // 
            // labelTransparency
            // 
            this.labelTransparency.AutoSize = true;
            this.labelTransparency.Location = new System.Drawing.Point(12, 127);
            this.labelTransparency.Name = "labelTransparency";
            this.labelTransparency.Size = new System.Drawing.Size(201, 20);
            this.labelTransparency.TabIndex = 11;
            this.labelTransparency.Text = "Overlay Transparency: 50%";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(313, 172);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(394, 172);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonRenumberTiles
            // 
            this.buttonRenumberTiles.Location = new System.Drawing.Point(274, 67);
            this.buttonRenumberTiles.Name = "buttonRenumberTiles";
            this.buttonRenumberTiles.Size = new System.Drawing.Size(100, 23);
            this.buttonRenumberTiles.TabIndex = 6;
            this.buttonRenumberTiles.Text = "Renumber Tiles";
            this.buttonRenumberTiles.UseVisualStyleBackColor = true;
            this.buttonRenumberTiles.Visible = false;
            // 
            // labelSubmap
            // 
            this.labelSubmap.AutoSize = true;
            this.labelSubmap.Location = new System.Drawing.Point(12, 12);
            this.labelSubmap.Name = "labelSubmap";
            this.labelSubmap.Size = new System.Drawing.Size(102, 20);
            this.labelSubmap.TabIndex = 13;
            this.labelSubmap.Text = "Submap File:";
            // 
            // labelTileInfoLabel
            // 
            this.labelTileInfoLabel.AutoSize = true;
            this.labelTileInfoLabel.Location = new System.Drawing.Point(12, 42);
            this.labelTileInfoLabel.Name = "labelTileInfoLabel";
            this.labelTileInfoLabel.Size = new System.Drawing.Size(69, 20);
            this.labelTileInfoLabel.TabIndex = 14;
            this.labelTileInfoLabel.Text = "Tile Info:";
            // 
            // labelPattern
            // 
            this.labelPattern.AutoSize = true;
            this.labelPattern.Location = new System.Drawing.Point(12, 72);
            this.labelPattern.Name = "labelPattern";
            this.labelPattern.Size = new System.Drawing.Size(146, 20);
            this.labelPattern.TabIndex = 15;
            this.labelPattern.Text = "Numbering Pattern:";
            this.labelPattern.Visible = false;
            // 
            // TilemapConfigDialog
            // 
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
            this.Controls.Add(this.labelSubmap);
            this.Controls.Add(this.labelTileInfoLabel);
            this.Controls.Add(this.labelPattern);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TilemapConfigDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tilemap Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOverlayTransparency)).EndInit();
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

        private void CheckBoxShowByteOverlay_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
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

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
