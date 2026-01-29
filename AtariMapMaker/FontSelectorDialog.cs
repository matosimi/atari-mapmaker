using System;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class FontSelectorDialog : Form
    {
        private AtariMap map;
        private int line;
        private ComboBox comboBoxFont;
        private ComboBox comboBoxTemplate;
        private CheckBox checkBoxLockTemplate;
        private Button buttonOK;
        private Button buttonCancel;

        public FontSelectorDialog(AtariMap map, int line)
        {
            this.map = map;
            this.line = line;
            InitializeComponent();
            PopulateFonts();
            PopulateTemplates();
            UpdateUI();
        }

        private void InitializeComponent()
        {
            this.comboBoxFont = new ComboBox();
            this.comboBoxTemplate = new ComboBox();
            this.checkBoxLockTemplate = new CheckBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.SuspendLayout();

            // comboBoxFont
            this.comboBoxFont.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxFont.Location = new System.Drawing.Point(12, 12);
            this.comboBoxFont.Name = "comboBoxFont";
            this.comboBoxFont.Size = new System.Drawing.Size(200, 21);
            this.comboBoxFont.TabIndex = 0;
            this.comboBoxFont.SelectedIndexChanged += ComboBoxFont_SelectedIndexChanged;

            // comboBoxTemplate
            this.comboBoxTemplate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxTemplate.Location = new System.Drawing.Point(12, 45);
            this.comboBoxTemplate.Name = "comboBoxTemplate";
            this.comboBoxTemplate.Size = new System.Drawing.Size(200, 21);
            this.comboBoxTemplate.TabIndex = 1;
            this.comboBoxTemplate.SelectedIndexChanged += ComboBoxTemplate_SelectedIndexChanged;

            // checkBoxLockTemplate
            this.checkBoxLockTemplate.AutoSize = true;
            this.checkBoxLockTemplate.Location = new System.Drawing.Point(12, 75);
            this.checkBoxLockTemplate.Name = "checkBoxLockTemplate";
            this.checkBoxLockTemplate.Size = new System.Drawing.Size(100, 17);
            this.checkBoxLockTemplate.TabIndex = 2;
            this.checkBoxLockTemplate.Text = "Lock Template";
            this.checkBoxLockTemplate.CheckedChanged += CheckBoxLockTemplate_CheckedChanged;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(56, 105);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(137, 105);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // FontSelectorDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(224, 140);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxLockTemplate);
            this.Controls.Add(this.comboBoxTemplate);
            this.Controls.Add(this.comboBoxFont);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FontSelectorDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = $"Font Selector - Line {line}";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PopulateFonts()
        {
            comboBoxFont.Items.Clear();
            comboBoxFont.Items.Add("Font 0");
            if (map.FontDataArray != null)
            {
                for (int i = 1; i < map.FontDataArray.Length; i++)
                {
                    if (map.FontDataArray[i] != null)
                    {
                        comboBoxFont.Items.Add($"Font {i}");
                    }
                }
            }
            comboBoxFont.SelectedIndex = 0;
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

        private void UpdateUI()
        {
            if (map.FontLineMapping != null && line >= 0 && line < map.FontLineMapping.Length)
            {
                byte fontIndex = map.FontLineMapping[line];
                if (fontIndex < comboBoxFont.Items.Count)
                    comboBoxFont.SelectedIndex = fontIndex;
            }
            checkBoxLockTemplate.Checked = map.FontTemplateLocked;
        }

        private void ComboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled || map.FontTemplateLocked)
                return;  // Don't allow changes if multifont disabled or template is locked

            if (comboBoxFont.SelectedIndex >= 0 && map.FontLineMapping != null && 
                line >= 0 && line < map.FontLineMapping.Length)
            {
                map.SetFontForLine(line, (byte)comboBoxFont.SelectedIndex);
                AtariFontRenderer.ClearFontCache();
            }
        }

        private void ComboBoxTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled || map.FontTemplateLocked)
                return;  // Don't allow changes if multifont disabled or template is locked

            if (comboBoxTemplate.SelectedIndex >= 0)
            {
                string templateName = comboBoxTemplate.SelectedItem.ToString();
                FontTemplateManager.ApplyTemplate(map, templateName);
                AtariFontRenderer.ClearFontCache();
                UpdateUI();
            }
        }

        private void CheckBoxLockTemplate_CheckedChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled)
                return;
            map.FontTemplateLocked = checkBoxLockTemplate.Checked;
            comboBoxFont.Enabled = !map.FontTemplateLocked && map.MultiFontEnabled;
            comboBoxTemplate.Enabled = !map.FontTemplateLocked && map.MultiFontEnabled;
        }
    }
}
