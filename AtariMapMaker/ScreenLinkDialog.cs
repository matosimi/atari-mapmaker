using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ScreenLinkDialog : Form
    {
        private AtariMap map;
        private Point sourceScreen;
        private NumericUpDown numericUpDownLinkedX;
        private NumericUpDown numericUpDownLinkedY;
        private TrackBar trackBarTransparency;
        private Label labelTransparency;
        private Button buttonOK;
        private Button buttonUnlink;
        private Button buttonCancel;

        public ScreenLinkDialog(AtariMap map, Point sourceScreen)
        {
            this.map = map;
            this.sourceScreen = sourceScreen;
            InitializeComponent();
            LoadExistingLink();
        }

        private void LoadExistingLink()
        {
            if (map?.ScreenLinks == null) return;
            var existing = map.ScreenLinks.Find(l => l.SourceScreen.X == sourceScreen.X && l.SourceScreen.Y == sourceScreen.Y);
            if (existing == null) return;
            numericUpDownLinkedX.Value = Math.Max(numericUpDownLinkedX.Minimum, Math.Min(numericUpDownLinkedX.Maximum, existing.LinkedScreen.X));
            numericUpDownLinkedY.Value = Math.Max(numericUpDownLinkedY.Minimum, Math.Min(numericUpDownLinkedY.Maximum, existing.LinkedScreen.Y));
            trackBarTransparency.Value = (int)(existing.Transparency * 100);
            labelTransparency.Text = $"Transparency: {trackBarTransparency.Value}%";
        }

        private void InitializeComponent()
        {
            this.numericUpDownLinkedX = new NumericUpDown();
            this.numericUpDownLinkedY = new NumericUpDown();
            this.trackBarTransparency = new TrackBar();
            this.labelTransparency = new Label();
            this.buttonOK = new Button();
            this.buttonUnlink = new Button();
            this.buttonCancel = new Button();
            Label label1 = new Label();
            Label label2 = new Label();
            this.SuspendLayout();

            // label1
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Text = "Linked Screen X:";

            // numericUpDownLinkedX
            this.numericUpDownLinkedX.Location = new Point(120, 12);
            this.numericUpDownLinkedX.Maximum = map != null ? map.MapSize.Width - 1 : 100;
            this.numericUpDownLinkedX.Minimum = 0;
            this.numericUpDownLinkedX.Size = new Size(100, 20);

            // label2
            label2.AutoSize = true;
            label2.Location = new Point(12, 45);
            label2.Text = "Linked Screen Y:";

            // numericUpDownLinkedY
            this.numericUpDownLinkedY.Location = new Point(120, 42);
            this.numericUpDownLinkedY.Maximum = map != null ? map.MapSize.Height - 1 : 100;
            this.numericUpDownLinkedY.Minimum = 0;
            this.numericUpDownLinkedY.Size = new Size(100, 20);

            // labelTransparency
            this.labelTransparency.AutoSize = true;
            this.labelTransparency.Location = new Point(12, 75);
            this.labelTransparency.Text = "Transparency: 50%";

            // trackBarTransparency
            this.trackBarTransparency.Location = new Point(120, 72);
            this.trackBarTransparency.Maximum = 100;
            this.trackBarTransparency.Minimum = 0;
            this.trackBarTransparency.Value = 50;
            this.trackBarTransparency.Size = new Size(200, 45);
            this.trackBarTransparency.TickFrequency = 10;
            this.trackBarTransparency.Scroll += TrackBarTransparency_Scroll;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(12, 120);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonUnlink
            this.buttonUnlink.Location = new Point(93, 120);
            this.buttonUnlink.Size = new Size(75, 23);
            this.buttonUnlink.Text = "Unlink";
            this.buttonUnlink.UseVisualStyleBackColor = true;
            this.buttonUnlink.Click += ButtonUnlink_Click;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(174, 120);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // ScreenLinkDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(324, 155);
            this.Controls.Add(this.buttonUnlink);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.trackBarTransparency);
            this.Controls.Add(this.labelTransparency);
            this.Controls.Add(this.numericUpDownLinkedY);
            this.Controls.Add(label2);
            this.Controls.Add(this.numericUpDownLinkedX);
            this.Controls.Add(label1);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenLinkDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = $"Link Screen {sourceScreen.X},{sourceScreen.Y}";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void TrackBarTransparency_Scroll(object sender, EventArgs e)
        {
            labelTransparency.Text = $"Transparency: {trackBarTransparency.Value}%";
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (map != null && map.ScreenLinks != null)
            {
                map.ScreenLinks.RemoveAll(l => l.SourceScreen.X == sourceScreen.X && l.SourceScreen.Y == sourceScreen.Y);
                map.ScreenLinks.Add(new ScreenLink
                {
                    SourceScreen = sourceScreen,
                    LinkedScreen = new Point((int)numericUpDownLinkedX.Value, (int)numericUpDownLinkedY.Value),
                    Transparency = trackBarTransparency.Value / 100.0f
                });
            }
        }

        private void ButtonUnlink_Click(object sender, EventArgs e)
        {
            if (map?.ScreenLinks != null)
            {
                map.ScreenLinks.RemoveAll(l => l.SourceScreen.X == sourceScreen.X && l.SourceScreen.Y == sourceScreen.Y);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
