using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class MapDescriptionDialog : Form
    {
        private AtariMap map;
        private TextBox textBoxDescription;
        private Button buttonOK;
        private Button buttonCancel;

        public MapDescriptionDialog(AtariMap map)
        {
            this.map = map;
            InitializeComponent();
            if (map != null && !string.IsNullOrEmpty(map.MapDescription))
                textBoxDescription.Text = map.MapDescription;
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
            label.Text = "Map Description:";

            // textBoxDescription
            this.textBoxDescription.Location = new Point(12, 35);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new Size(400, 200);
            this.textBoxDescription.ScrollBars = ScrollBars.Vertical;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(256, 245);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(337, 245);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // MapDescriptionDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(424, 280);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(label);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapDescriptionDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Map Description";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (map != null)
                map.MapDescription = textBoxDescription.Text;
        }
    }
}
