using System;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class FontSelectorDialog : Form
    {
        private AtariMap map;
        private int line;
        private int screenX;
        private int screenY;
        private ComboBox comboBoxFont;
        private ComboBox comboBoxTemplate;
        private CheckBox checkBoxLockTemplate;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonApply;
        private bool templateMode;

        public FontSelectorDialog(AtariMap map, int line, int screenX = 0, int screenY = 0, bool templateMode = false)
        {
            this.map = map;
            this.line = line;
            this.screenX = screenX;
            this.screenY = screenY;
            this.templateMode = templateMode;
            InitializeComponent();
            if (templateMode)
            {
                PopulateTemplates();
                this.Text = "Apply Font Template";
            }
            else
            {
                PopulateFonts();
                PopulateTemplates();
                UpdateUI();
            }
        }

        private void InitializeComponent()
        {
            this.comboBoxFont = new ComboBox();
            this.comboBoxTemplate = new ComboBox();
            this.checkBoxLockTemplate = new CheckBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.buttonApply = new Button();
            this.SuspendLayout();

            if (templateMode)
            {
                // Template mode: only show template combobox and Apply/Cancel buttons
                // comboBoxTemplate
                this.comboBoxTemplate.DropDownStyle = ComboBoxStyle.DropDownList;
                this.comboBoxTemplate.Location = new System.Drawing.Point(12, 12);
                this.comboBoxTemplate.Name = "comboBoxTemplate";
                this.comboBoxTemplate.Size = new System.Drawing.Size(200, 21);
                this.comboBoxTemplate.TabIndex = 0;

                // buttonApply
                this.buttonApply.Location = new System.Drawing.Point(56, 45);
                this.buttonApply.Name = "buttonApply";
                this.buttonApply.Size = new System.Drawing.Size(75, 23);
                this.buttonApply.TabIndex = 1;
                this.buttonApply.Text = "Apply";
                this.buttonApply.UseVisualStyleBackColor = true;
                this.buttonApply.Click += ButtonApply_Click;

                // buttonCancel
                this.buttonCancel.DialogResult = DialogResult.Cancel;
                this.buttonCancel.Location = new System.Drawing.Point(137, 45);
                this.buttonCancel.Name = "buttonCancel";
                this.buttonCancel.Size = new System.Drawing.Size(75, 23);
                this.buttonCancel.TabIndex = 2;
                this.buttonCancel.Text = "Cancel";
                this.buttonCancel.UseVisualStyleBackColor = true;

                // FontSelectorDialog
                this.CancelButton = this.buttonCancel;
                this.ClientSize = new System.Drawing.Size(224, 80);
                this.Controls.Add(this.buttonCancel);
                this.Controls.Add(this.buttonApply);
                this.Controls.Add(this.comboBoxTemplate);
            }
            else
            {
                // Original mode: show font selector, template, and lock checkbox
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
            }
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
            if (line >= 0 && line < map.ScreenSize.Height)
            {
                byte fontIndex = map.GetFontForLine(screenX, screenY, line);
                if (fontIndex < comboBoxFont.Items.Count)
                    comboBoxFont.SelectedIndex = fontIndex;
            }
            checkBoxLockTemplate.Checked = map.FontTemplateLocked;
        }

        private void ComboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!map.MultiFontEnabled || map.FontTemplateLocked)
                return;  // Don't allow changes if multifont disabled or template is locked

            if (comboBoxFont.SelectedIndex >= 0 && line >= 0 && line < map.ScreenSize.Height)
            {
                map.SetFontForLine(screenX, screenY, line, (byte)comboBoxFont.SelectedIndex);
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

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (comboBoxTemplate.SelectedIndex >= 0)
            {
                string templateName = comboBoxTemplate.SelectedItem.ToString();
                // Apply template only to the specified screen
                FontTemplateManager.ApplyTemplate(map, templateName, screenX, screenY);
                AtariFontRenderer.ClearFontCache();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
