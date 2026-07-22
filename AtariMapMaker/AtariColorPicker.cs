//Form na zobrazenie dialogu na vybratie farby z objektu palety AtariPalette

using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class AtariColorPicker : Form
    {
        private byte selectedColorIndex;
        private byte oldColorIndex;
        private Color selectedColor;

        private bool dualMode;
        private bool editingPrimary = true;
        private byte primaryColor;
        private byte alternateColor;
        private byte oldPrimaryColor;
        private byte oldAlternateColor;
        private bool accepted;

        private Panel panelDual;
        private Label labelPrimaryTitle;
        private Label labelAlternateTitle;
        private Label labelPrimaryValue;
        private Label labelAlternateValue;
        private PictureBox pictureBoxPrimary;
        private PictureBox pictureBoxAlternate;
        private Button buttonOk;

        public AtariColorPicker()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            BuildDualControls();
            SetSingleModeLayout();
        }

        public byte PickedColorIndex { get { return selectedColorIndex; } }

        /// <summary>Alternate color chosen in dual mode; same as initial if cancelled or single mode.</summary>
        public byte PickedAlternateIndex { get { return alternateColor; } }

        public bool PickedNewColor
        {
            get
            {
                if (!accepted) return false;
                if (dualMode)
                    return primaryColor != oldPrimaryColor || alternateColor != oldAlternateColor;
                return selectedColorIndex != oldColorIndex;
            }
        }

        public bool PickedPrimaryChanged
        {
            get { return accepted && primaryColor != oldPrimaryColor; }
        }

        public bool PickedAlternateChanged
        {
            get { return accepted && dualMode && alternateColor != oldAlternateColor; }
        }

        /// <summary>Single-color pick (existing behavior).</summary>
        public Color Pick(byte oldColorIndex)
        {
            dualMode = false;
            accepted = false;
            this.oldColorIndex = oldColorIndex;
            this.selectedColorIndex = oldColorIndex;
            this.primaryColor = oldColorIndex;
            this.oldPrimaryColor = oldColorIndex;
            SetSingleModeLayout();
            this.RenderPalette();
            this.DrawSelection(selectedColorIndex, selectedColorIndex);
            this.ShowDialog();
            return selectedColor;
        }

        /// <summary>ALPA dual pick: primary (per-line / COLPF) and matching global alternate.</summary>
        public Color Pick(byte primary, byte alternate)
        {
            dualMode = true;
            accepted = false;
            editingPrimary = true;
            oldPrimaryColor = primary;
            oldAlternateColor = alternate;
            primaryColor = primary;
            alternateColor = alternate;
            oldColorIndex = primary;
            selectedColorIndex = primary;
            SetDualModeLayout();
            UpdateDualSwatches();
            HighlightActiveTarget();
            this.RenderPalette();
            this.ShowDialog();
            selectedColor = AtariPalette.GetColor(primaryColor);
            selectedColorIndex = primaryColor;
            return selectedColor;
        }

        private void BuildDualControls()
        {
            panelDual = new Panel
            {
                Location = new Point(160, 8),
                Size = new Size(100, 270),
                Visible = false
            };

            labelPrimaryTitle = new Label
            {
                Text = "Primary",
                Location = new Point(0, 0),
                Size = new Size(100, 16),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pictureBoxPrimary = new PictureBox
            {
                Location = new Point(11, 18),
                Size = new Size(78, 56),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            pictureBoxPrimary.Click += (s, e) => { editingPrimary = true; HighlightActiveTarget(); RenderPalette(); };
            labelPrimaryValue = new Label
            {
                Location = new Point(0, 76),
                Size = new Size(100, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelAlternateTitle = new Label
            {
                Text = "Alternate",
                Location = new Point(0, 108),
                Size = new Size(100, 16),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pictureBoxAlternate = new PictureBox
            {
                Location = new Point(11, 126),
                Size = new Size(78, 56),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            pictureBoxAlternate.Click += (s, e) => { editingPrimary = false; HighlightActiveTarget(); RenderPalette(); };
            labelAlternateValue = new Label
            {
                Location = new Point(0, 184),
                Size = new Size(100, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            buttonOk = new Button
            {
                Text = "OK",
                Location = new Point(11, 220),
                Size = new Size(78, 28)
            };
            buttonOk.Click += (s, e) =>
            {
                accepted = true;
                selectedColorIndex = primaryColor;
                selectedColor = AtariPalette.GetColor(primaryColor);
                Close();
            };

            panelDual.Controls.Add(labelPrimaryTitle);
            panelDual.Controls.Add(pictureBoxPrimary);
            panelDual.Controls.Add(labelPrimaryValue);
            panelDual.Controls.Add(labelAlternateTitle);
            panelDual.Controls.Add(pictureBoxAlternate);
            panelDual.Controls.Add(labelAlternateValue);
            panelDual.Controls.Add(buttonOk);
            Controls.Add(panelDual);
        }

        private void SetSingleModeLayout()
        {
            panelDual.Visible = false;
            pictureBox2.Visible = true;
            labelOldCol.Visible = true;
            labelNewCol.Visible = true;
            Text = "AtariColorPicker";
        }

        private void SetDualModeLayout()
        {
            pictureBox2.Visible = false;
            labelOldCol.Visible = false;
            labelNewCol.Visible = false;
            panelDual.Visible = true;
            Text = "Primary + Alternate";
        }

        private void UpdateDualSwatches()
        {
            labelPrimaryValue.Text = "was $" + oldPrimaryColor.ToString("X2") + "\nnow $" + primaryColor.ToString("X2");
            labelAlternateValue.Text = "was $" + oldAlternateColor.ToString("X2") + "\nnow $" + alternateColor.ToString("X2");
            DrawDualSwatch(pictureBoxPrimary, oldPrimaryColor, primaryColor);
            DrawDualSwatch(pictureBoxAlternate, oldAlternateColor, alternateColor);
        }

        private static void DrawSwatch(PictureBox box, byte colorIndex)
        {
            Bitmap bmp = new Bitmap(box.Width, box.Height);
            using (Graphics gr = Graphics.FromImage(bmp))
                gr.Clear(AtariPalette.GetColor(colorIndex));
            if (box.Image != null)
                box.Image.Dispose();
            box.Image = bmp;
        }

        /// <summary>Top half = previous color, bottom half = current selection (same as single-mode preview).</summary>
        private static void DrawDualSwatch(PictureBox box, byte oldColor, byte newColor)
        {
            Bitmap bmp = new Bitmap(box.Width, box.Height);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.FillRectangle(new SolidBrush(AtariPalette.GetColor(oldColor)), 0, 0, box.Width, box.Height / 2);
                gr.FillRectangle(new SolidBrush(AtariPalette.GetColor(newColor)), 0, box.Height / 2, box.Width, box.Height - box.Height / 2);
            }
            if (box.Image != null)
                box.Image.Dispose();
            box.Image = bmp;
        }

        private void HighlightActiveTarget()
        {
            pictureBoxPrimary.BorderStyle = editingPrimary ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
            pictureBoxAlternate.BorderStyle = editingPrimary ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
        }

        private void RenderPalette()
        {
            byte highlight = dualMode
                ? (editingPrimary ? primaryColor : alternateColor)
                : selectedColorIndex;
            Bitmap matrix = new Bitmap(128 + 16, 256 + 16);
            Graphics gr = Graphics.FromImage(matrix);
            gr.Clear(this.BackColor);
            for (int y = 0; y < 16; y++)
            {
                gr.DrawString(String.Format("{0:X}", y), this.Font, new SolidBrush(this.ForeColor), 16 * 8 + 2, y * 16);
                for (int x = 0; x < 8; x++)
                    gr.FillRectangle(new SolidBrush(AtariPalette.GetColor(y * 16 + x * 2)), x * 16, y * 16, 16, 16);
            }
            gr.DrawRectangle(new Pen(new SolidBrush(Color.White)), (highlight % 16) * 8, (highlight / 16) * 16, 15, 15);

            for (int x = 0; x < 8; x++)
                gr.DrawString(String.Format("{0:X}", x * 2), this.Font, new SolidBrush(this.ForeColor), 16 * x, 16 * 16 + 2);
            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            pictureBox1.Image = matrix;

            gr.Dispose();
        }

        private void DrawSelection(int oldColor, int newColor)
        {
            int w = pictureBox2.Width;
            int h = pictureBox2.Height;
            labelOldCol.Text = "$" + String.Format("{0:X2}", oldColor) + " - " + oldColor.ToString();
            labelNewCol.Text = "$" + String.Format("{0:X2}", newColor) + " - " + newColor.ToString();
            Bitmap clr = new Bitmap(w, h);
            Graphics gr = Graphics.FromImage(clr);
            gr.FillRectangle(new SolidBrush(AtariPalette.GetColor(oldColor)), 0, 0, w, h / 2);
            gr.FillRectangle(new SolidBrush(AtariPalette.GetColor(newColor)), 0, h / 2, w, h / 2);
            gr.Dispose();
            if (pictureBox2.Image != null)
                pictureBox2.Image.Dispose();
            pictureBox2.Image = clr;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.X >= 128 || e.Y >= 256)
                return;

            byte index = (byte)((e.X / 16) * 2 + (e.Y / 16) * 16);
            if (dualMode)
            {
                if (editingPrimary)
                    primaryColor = index;
                else
                    alternateColor = index;
                UpdateDualSwatches();
                RenderPalette();
            }
            else
            {
                selectedColorIndex = index;
                selectedColor = AtariPalette.GetColor(selectedColorIndex);
                accepted = true;
                this.Close();
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X < 128 && e.Y < 256)
            {
                int index = (e.X / 16) * 2 + (e.Y / 16) * 16;
                if (dualMode)
                {
                    if (editingPrimary)
                        labelPrimaryValue.Text = "$" + index.ToString("X2") + " - " + index + " ?";
                    else
                        labelAlternateValue.Text = "$" + index.ToString("X2") + " - " + index + " ?";
                }
                else
                    DrawSelection(selectedColorIndex, index);
            }
        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (dualMode)
                UpdateDualSwatches();
        }

        private void AtariColorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                accepted = false;
                if (dualMode)
                {
                    primaryColor = oldPrimaryColor;
                    alternateColor = oldAlternateColor;
                    selectedColorIndex = oldPrimaryColor;
                }
                else
                    this.selectedColorIndex = this.oldColorIndex;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter && dualMode)
            {
                accepted = true;
                selectedColorIndex = primaryColor;
                selectedColor = AtariPalette.GetColor(primaryColor);
                this.Close();
            }
        }
    }
}
