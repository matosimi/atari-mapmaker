using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ScreenLinkDialog : Form
    {
        private AtariMap map;
        private Point sourceScreen;

        public ScreenLinkDialog(AtariMap map, Point sourceScreen)
        {
            this.map = map;
            this.sourceScreen = sourceScreen;
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            if (map != null)
            {
                numericUpDownLinkedX.Maximum = map.MapSize.Width - 1;
                numericUpDownLinkedY.Maximum = map.MapSize.Height - 1;
            }
            this.Text = $"Link Screen {sourceScreen.X},{sourceScreen.Y}";
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
