using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class FontTemplateDialog : Form
    {
        private AtariMap map;
        private ComboBox comboBoxTemplate;
        private CheckBox checkBoxLockTemplate;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonLoadFont;
        private Button buttonRemoveFont;
        private Button buttonExportFont;
        private ComboBox comboBoxFontSlot;
        private Label labelFontFileName;

        public FontTemplateDialog(AtariMap map)
        {
            this.map = map;
            InitializeComponent();
            PopulateTemplates();
            PopulateFontSlots();
            UpdateUI();
        }

        private void InitializeComponent()
        {
            this.comboBoxTemplate = new ComboBox();
            this.checkBoxLockTemplate = new CheckBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.buttonLoadFont = new Button();
            this.buttonRemoveFont = new Button();
            this.buttonExportFont = new Button();
            this.comboBoxFontSlot = new ComboBox();
            this.labelFontFileName = new Label();
            Label labelTemplate = new Label();
            Label labelFontSlot = new Label();
            this.SuspendLayout();

            // labelTemplate
            labelTemplate.AutoSize = true;
            labelTemplate.Location = new System.Drawing.Point(12, 15);
            labelTemplate.Name = "labelTemplate";
            labelTemplate.Size = new System.Drawing.Size(51, 13);
            labelTemplate.Text = "Template:";

            // comboBoxTemplate
            this.comboBoxTemplate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxTemplate.Location = new System.Drawing.Point(80, 12);
            this.comboBoxTemplate.Name = "comboBoxTemplate";
            this.comboBoxTemplate.Size = new System.Drawing.Size(200, 21);
            this.comboBoxTemplate.TabIndex = 0;
            this.comboBoxTemplate.SelectedIndexChanged += ComboBoxTemplate_SelectedIndexChanged;

            // checkBoxLockTemplate
            this.checkBoxLockTemplate.AutoSize = true;
            this.checkBoxLockTemplate.Location = new System.Drawing.Point(80, 45);
            this.checkBoxLockTemplate.Name = "checkBoxLockTemplate";
            this.checkBoxLockTemplate.Size = new System.Drawing.Size(100, 17);
            this.checkBoxLockTemplate.TabIndex = 1;
            this.checkBoxLockTemplate.Text = "Lock Template";
            this.checkBoxLockTemplate.CheckedChanged += CheckBoxLockTemplate_CheckedChanged;

            // labelFontSlot
            labelFontSlot.AutoSize = true;
            labelFontSlot.Location = new System.Drawing.Point(12, 75);
            labelFontSlot.Name = "labelFontSlot";
            labelFontSlot.Size = new System.Drawing.Size(55, 13);
            labelFontSlot.Text = "Font Slot:";

            // comboBoxFontSlot
            this.comboBoxFontSlot.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxFontSlot.Location = new System.Drawing.Point(80, 72);
            this.comboBoxFontSlot.Name = "comboBoxFontSlot";
            this.comboBoxFontSlot.Size = new System.Drawing.Size(120, 21);
            this.comboBoxFontSlot.TabIndex = 2;

            // buttonLoadFont
            this.buttonLoadFont.Location = new System.Drawing.Point(206, 70);
            this.buttonLoadFont.Name = "buttonLoadFont";
            this.buttonLoadFont.Size = new System.Drawing.Size(74, 23);
            this.buttonLoadFont.TabIndex = 3;
            this.buttonLoadFont.Text = "Load Font";
            this.buttonLoadFont.UseVisualStyleBackColor = true;
            this.buttonLoadFont.Click += ButtonLoadFont_Click;

            // buttonRemoveFont
            this.buttonRemoveFont.Location = new System.Drawing.Point(286, 70);
            this.buttonRemoveFont.Name = "buttonRemoveFont";
            this.buttonRemoveFont.Size = new System.Drawing.Size(74, 23);
            this.buttonRemoveFont.TabIndex = 4;
            this.buttonRemoveFont.Text = "Remove";
            this.buttonRemoveFont.UseVisualStyleBackColor = true;
            this.buttonRemoveFont.Click += ButtonRemoveFont_Click;

            // buttonExportFont
            this.buttonExportFont.Location = new System.Drawing.Point(206, 100);
            this.buttonExportFont.Name = "buttonExportFont";
            this.buttonExportFont.Size = new System.Drawing.Size(74, 23);
            this.buttonExportFont.TabIndex = 5;
            this.buttonExportFont.Text = "Export Font";
            this.buttonExportFont.UseVisualStyleBackColor = true;
            this.buttonExportFont.Click += ButtonExportFont_Click;

            // labelFontFileName
            this.labelFontFileName.AutoSize = true;
            this.labelFontFileName.Location = new System.Drawing.Point(80, 100);
            this.labelFontFileName.Name = "labelFontFileName";
            this.labelFontFileName.Size = new System.Drawing.Size(0, 13);
            this.labelFontFileName.MaximumSize = new System.Drawing.Size(280, 0);

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(124, 125);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(205, 125);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // FontTemplateDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(372, 160);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonExportFont);
            this.Controls.Add(this.labelFontFileName);
            this.Controls.Add(this.buttonRemoveFont);
            this.Controls.Add(this.buttonLoadFont);
            this.Controls.Add(this.comboBoxFontSlot);
            this.Controls.Add(labelFontSlot);
            this.Controls.Add(this.checkBoxLockTemplate);
            this.Controls.Add(this.comboBoxTemplate);
            this.Controls.Add(labelTemplate);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FontTemplateDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Font Template Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PopulateTemplates()
        {
            comboBoxTemplate.Items.Clear();
            foreach (var template in FontTemplateManager.GetTemplates())
            {
                comboBoxTemplate.Items.Add(template.Name);
            }
            if (!string.IsNullOrEmpty(map.FontTemplatePattern))
            {
                int index = comboBoxTemplate.Items.IndexOf(map.FontTemplatePattern);
                if (index >= 0)
                    comboBoxTemplate.SelectedIndex = index;
                else
                    comboBoxTemplate.SelectedIndex = 0;
            }
            else
            {
                comboBoxTemplate.SelectedIndex = 0;
            }
        }

        private void PopulateFontSlots()
        {
            comboBoxFontSlot.Items.Clear();
            for (int i = 0; i < 8; i++)
            {
                string status = (map.FontDataArray != null && map.FontDataArray[i] != null) ? " (loaded)" : " (empty)";
                comboBoxFontSlot.Items.Add($"Font {i}{status}");
            }
            comboBoxFontSlot.SelectedIndex = 0;
            comboBoxFontSlot.SelectedIndexChanged += ComboBoxFontSlot_SelectedIndexChanged;
            UpdateFontFileNameDisplay();
        }

        private void ComboBoxFontSlot_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFontFileNameDisplay();
        }

        private void UpdateFontFileNameDisplay()
        {
            if (comboBoxFontSlot.SelectedIndex >= 0)
            {
                int slotIndex = comboBoxFontSlot.SelectedIndex;
                if (map.FontFileNames != null && slotIndex < map.FontFileNames.Length && 
                    !string.IsNullOrEmpty(map.FontFileNames[slotIndex]))
                {
                    string fileName = System.IO.Path.GetFileName(map.FontFileNames[slotIndex]);
                    labelFontFileName.Text = $"File: {fileName}";
                    buttonRemoveFont.Enabled = map.MultiFontEnabled;
                }
                else
                {
                    // Check if slot 0 has a font (single font mode)
                    if (slotIndex == 0 && map.FontDataArray != null && map.FontDataArray[0] != null)
                    {
                        labelFontFileName.Text = "File: (inherited from single font)";
                    }
                    else
                    {
                        labelFontFileName.Text = "File: (none)";
                    }
                    buttonRemoveFont.Enabled = false;
                }
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool multifontEnabled = map.MultiFontEnabled;
            checkBoxLockTemplate.Checked = map.FontTemplateLocked;
            checkBoxLockTemplate.Enabled = multifontEnabled;
            comboBoxTemplate.Enabled = !map.FontTemplateLocked && multifontEnabled;
            comboBoxFontSlot.Enabled = multifontEnabled;
            buttonLoadFont.Enabled = multifontEnabled;
            bool hasFont = comboBoxFontSlot.SelectedIndex >= 0 && 
                map.FontDataArray != null && comboBoxFontSlot.SelectedIndex < map.FontDataArray.Length &&
                map.FontDataArray[comboBoxFontSlot.SelectedIndex] != null;
            buttonRemoveFont.Enabled = multifontEnabled && hasFont;
            buttonExportFont.Enabled = hasFont || comboBoxFontSlot.SelectedIndex == 0; // Can always export font 0 (single font)
        }

        private void ComboBoxTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled || map.FontTemplateLocked)
                return;

            if (comboBoxTemplate.SelectedIndex >= 0)
            {
                string templateName = comboBoxTemplate.SelectedItem.ToString();
                FontTemplateManager.ApplyTemplate(map, templateName);
                AtariFontRenderer.ClearFontCache();
            }
        }

        private void CheckBoxLockTemplate_CheckedChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled)
                return;
            map.FontTemplateLocked = checkBoxLockTemplate.Checked;
            comboBoxTemplate.Enabled = !map.FontTemplateLocked && map.MultiFontEnabled;
        }

        private void ButtonLoadFont_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Atari Font (*.fnt)|*.fnt";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                int fontSlot = comboBoxFontSlot.SelectedIndex;
                byte[] fontData = new byte[1024 * 2];
                FileStream fs = new FileStream(openDialog.FileName, FileMode.Open);
                fs.Read(fontData, 0, 1024);
                fs.Close();
                for (int a = 0; a < 1024; a++)
                {
                    fontData[a + 1024] = (byte)(fontData[a] ^ 0x80);
                }

                map.SetFontData(fontData, fontSlot, openDialog.FileName);
                AtariFontRenderer.ClearFontCache();
                
                if (fontSlot == 0)
                {
                    AtariFontRenderer.SetFontData(fontData, Globals.FontType.Screen);
                }

                PopulateFontSlots();
                comboBoxFontSlot.SelectedIndex = fontSlot;
                UpdateFontFileNameDisplay();
            }
        }

        private void ButtonRemoveFont_Click(object sender, EventArgs e)
        {
            if (comboBoxFontSlot.SelectedIndex >= 0)
            {
                int fontSlot = comboBoxFontSlot.SelectedIndex;
                if (MessageBox.Show($"Remove font from slot {fontSlot}?", "Confirm Remove", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    map.ClearFontSlot(fontSlot);
                    AtariFontRenderer.ClearFontCache();
                    
                    if (fontSlot == 0)
                    {
                        // If removing font 0, we need to set a default or keep using the last loaded font
                        // For now, just clear the cache
                    }

                    PopulateFontSlots();
                    comboBoxFontSlot.SelectedIndex = fontSlot;
                    UpdateFontFileNameDisplay();
                }
            }
        }

        private void ButtonExportFont_Click(object sender, EventArgs e)
        {
            if (comboBoxFontSlot.SelectedIndex < 0)
                return;

            int fontSlot = comboBoxFontSlot.SelectedIndex;
            byte[] fontData = null;
            string defaultFileName = "font.fnt";

            // Get font data - either from the slot or from font 0 (single font) if slot is empty
            if (map.FontDataArray != null && fontSlot < map.FontDataArray.Length && 
                map.FontDataArray[fontSlot] != null)
            {
                fontData = map.FontDataArray[fontSlot];
                // Get default filename from stored filename or suggest based on slot
                if (map.FontFileNames != null && fontSlot < map.FontFileNames.Length && 
                    !string.IsNullOrEmpty(map.FontFileNames[fontSlot]))
                {
                    defaultFileName = System.IO.Path.GetFileName(map.FontFileNames[fontSlot]);
                }
                else
                {
                    defaultFileName = $"font{fontSlot}.fnt";
                }
            }
            else if (fontSlot == 0)
            {
                // Export the single font (font 0) - get from AtariFontRenderer
                if (AtariFontRenderer.fonts.ContainsKey(Globals.FontType.Screen))
                {
                    fontData = AtariFontRenderer.fonts[Globals.FontType.Screen].data;
                    if (!string.IsNullOrEmpty(AtariFontRenderer.LastFontFile))
                    {
                        defaultFileName = System.IO.Path.GetFileName(AtariFontRenderer.LastFontFile);
                    }
                }
            }

            if (fontData == null)
            {
                MessageBox.Show("No font data available to export.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Atari Font (*.fnt)|*.fnt";
            saveDialog.FileName = defaultFileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Export only first 1024 bytes (standard Atari font format)
                byte[] exportData = new byte[1024];
                Array.Copy(fontData, exportData, Math.Min(1024, fontData.Length));
                File.WriteAllBytes(saveDialog.FileName, exportData);
                MessageBox.Show("Font exported successfully.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
