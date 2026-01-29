using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ScreenMetadataDialog : Form
    {
        private AtariMap map;
        private Point screen;
        private TextBox textBoxRawText;
        private TextBox textBoxRegex;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonParse;

        public ScreenMetadataDialog(AtariMap map, Point screen)
        {
            this.map = map;
            this.screen = screen;
            InitializeComponent();
            LoadMetadata();
        }

        private void InitializeComponent()
        {
            this.textBoxRawText = new TextBox();
            this.textBoxRegex = new TextBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.buttonParse = new Button();
            Label label1 = new Label();
            Label label2 = new Label();
            this.SuspendLayout();

            // label1
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Text = "Metadata Text:";

            // textBoxRawText
            this.textBoxRawText.Location = new Point(12, 35);
            this.textBoxRawText.Multiline = true;
            this.textBoxRawText.Size = new Size(400, 100);
            this.textBoxRawText.ScrollBars = ScrollBars.Vertical;

            // label2
            label2.AutoSize = true;
            label2.Location = new Point(12, 145);
            label2.Text = "Regex Pattern:";

            // textBoxRegex
            this.textBoxRegex.Location = new Point(12, 165);
            this.textBoxRegex.Size = new Size(400, 20);

            // buttonParse
            this.buttonParse.Text = "Parse";
            this.buttonParse.Location = new Point(12, 195);
            this.buttonParse.Size = new Size(75, 23);
            this.buttonParse.Click += ButtonParse_Click;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(256, 195);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(337, 195);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // ScreenMetadataDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(424, 230);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonParse);
            this.Controls.Add(this.textBoxRegex);
            this.Controls.Add(label2);
            this.Controls.Add(this.textBoxRawText);
            this.Controls.Add(label1);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenMetadataDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = $"Screen Metadata - {screen.X},{screen.Y}";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadMetadata()
        {
            if (map != null && map.ScreenMetadata != null)
            {
                string key = $"{screen.X},{screen.Y}";
                if (map.ScreenMetadata.ContainsKey(key))
                {
                    ScreenMetadata metadata = map.ScreenMetadata[key];
                    if (metadata != null)
                    {
                        textBoxRawText.Text = metadata.RawText ?? "";
                        textBoxRegex.Text = metadata.RegexPattern ?? "";
                    }
                }
            }
        }

        private void ButtonParse_Click(object sender, EventArgs e)
        {
            if (map == null) return;
            string key = $"{screen.X},{screen.Y}";
            if (map.ScreenMetadata == null)
                map.ScreenMetadata = new Dictionary<string, ScreenMetadata>();

            if (!map.ScreenMetadata.ContainsKey(key))
                map.ScreenMetadata[key] = new ScreenMetadata();

            ScreenMetadata metadata = map.ScreenMetadata[key];
            metadata.RawText = textBoxRawText.Text;
            metadata.RegexPattern = textBoxRegex.Text;
            MetadataParser.UpdateScreenMetadata(map, screen.X, screen.Y);
            MessageBox.Show($"Parsed {metadata.ParsedItems.Count} items.", "Parse Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (map == null) return;
            string key = $"{screen.X},{screen.Y}";
            if (map.ScreenMetadata == null)
                map.ScreenMetadata = new Dictionary<string, ScreenMetadata>();

            if (!map.ScreenMetadata.ContainsKey(key))
                map.ScreenMetadata[key] = new ScreenMetadata();

            ScreenMetadata metadata = map.ScreenMetadata[key];
            metadata.RawText = textBoxRawText.Text;
            metadata.RegexPattern = textBoxRegex.Text;
            MetadataParser.UpdateScreenMetadata(map, screen.X, screen.Y);
        }
    }
}
