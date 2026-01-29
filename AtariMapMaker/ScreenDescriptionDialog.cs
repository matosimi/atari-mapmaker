using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ScreenDescriptionDialog : Form
    {
        private AtariMap map;
        private Point screen;
        private TextBox textBoxDescription;
        private Button buttonOK;
        private Button buttonCancel;

        public ScreenDescriptionDialog(AtariMap map, Point screen)
        {
            this.map = map;
            this.screen = screen;
            InitializeComponent();
            LoadDescription();
        }

        private void InitializeComponent()
        {
            this.textBoxDescription = new TextBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            Label label = new Label();
            this.SuspendLayout();

            // label
            label.AutoSize = true;
            label.Location = new Point(12, 15);
            label.Text = "Screen Description:";

            // textBoxDescription
            this.textBoxDescription.Location = new Point(12, 35);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Size = new Size(400, 150);
            this.textBoxDescription.ScrollBars = ScrollBars.Vertical;

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

            // ScreenDescriptionDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(424, 230);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(label);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenDescriptionDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = $"Screen Description - {screen.X},{screen.Y}";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadDescription()
        {
            if (map != null && map.ScreenDescriptions != null)
            {
                string key = $"{screen.X},{screen.Y}";
                if (map.ScreenDescriptions.ContainsKey(key))
                    textBoxDescription.Text = map.ScreenDescriptions[key];
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (map == null) return;
            if (map.ScreenDescriptions == null)
                map.ScreenDescriptions = new Dictionary<string, string>();

            string key = $"{screen.X},{screen.Y}";
            map.ScreenDescriptions[key] = textBoxDescription.Text;
        }
    }
}
