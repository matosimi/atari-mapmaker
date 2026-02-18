using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public class MetadataItemEditDialog : Form
    {
        private MetadataLayerItem item;
        private TextBox textBoxText;
        private TextBox textBoxValueHex;
        private NumericUpDown numericX;
        private NumericUpDown numericY;
        private Button buttonPickColor;
        private Label labelColorHex;
        private byte selectedColorIndex;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonRemove;

        /// <summary>True if user clicked Remove (caller should remove item from list).</summary>
        public bool RemoveRequested { get; private set; }

        private bool isTilemap;

        public MetadataItemEditDialog(MetadataLayerItem item, string title, bool isTilemap = false)
        {
            this.item = item ?? new MetadataLayerItem();
            this.isTilemap = isTilemap;
            this.Text = title;
            RemoveRequested = false;
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            textBoxValueHex.MaxLength = isTilemap ? 4 : 2;
            numericX.Value = Math.Max(0, Math.Min(this.item.X, 255));
            numericY.Value = Math.Max(0, Math.Min(this.item.Y, 255));
            textBoxText.Text = this.item.Text ?? "";
            textBoxValueHex.Text = this.item.Value >= 0 && this.item.Value <= 255
                ? this.item.Value.ToString("X2")
                : this.item.Value.ToString("X4");
            selectedColorIndex = (byte)Math.Max((byte)0, Math.Min((byte)255, this.item.Color));
            UpdateColorLabel();
        }

        private void InitializeComponent()
        {
            int y = 12;
            var lblX = new Label { Text = "X:", Location = new Point(12, y), AutoSize = true };
            numericX = new NumericUpDown { Location = new Point(60, y - 2), Width = 60, Minimum = 0, Maximum = 255 };
            y += 28;
            var lblY = new Label { Text = "Y:", Location = new Point(12, y), AutoSize = true };
            numericY = new NumericUpDown { Location = new Point(60, y - 2), Width = 60, Minimum = 0, Maximum = 255 };
            y += 28;
            var lblText = new Label { Text = "Text:", Location = new Point(12, y), AutoSize = true };
            textBoxText = new TextBox { Location = new Point(60, y - 2), Width = 220 };
            y += 28;
            var lblValue = new Label { Text = "Value (hex):", Location = new Point(12, y), AutoSize = true };
            textBoxValueHex = new TextBox { Location = new Point(90, y - 2), Width = 60, MaxLength = 4 };
            y += 28;
            var lblColor = new Label { Text = "Color:", Location = new Point(12, y), AutoSize = true };
            labelColorHex = new Label { Text = "$00", Location = new Point(60, y), AutoSize = true };
            buttonPickColor = new Button { Text = "Pick...", Location = new Point(120, y - 2), Width = 60, Height = 22 };
            buttonPickColor.Click += ButtonPickColor_Click;
            y += 32;
            buttonOK = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(12, y), Size = new Size(75, 25) };
            buttonCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(95, y), Size = new Size(75, 25) };
            buttonRemove = new Button { Text = "Remove", Location = new Point(178, y), Size = new Size(75, 25) };
            buttonRemove.Click += (s, e) =>
            {
                RemoveRequested = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
            this.ClientSize = new Size(265, y + 35);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.Add(lblX); this.Controls.Add(numericX);
            this.Controls.Add(lblY); this.Controls.Add(numericY);
            this.Controls.Add(lblText); this.Controls.Add(textBoxText);
            this.Controls.Add(lblValue); this.Controls.Add(textBoxValueHex);
            this.Controls.Add(lblColor); this.Controls.Add(labelColorHex); this.Controls.Add(buttonPickColor);
            this.Controls.Add(buttonOK); this.Controls.Add(buttonCancel); this.Controls.Add(buttonRemove);
        }

        private void UpdateColorLabel()
        {
            labelColorHex.Text = "$" + selectedColorIndex.ToString("X2");
        }

        private void ButtonPickColor_Click(object sender, EventArgs e)
        {
            using (var picker = new AtariColorPicker())
            {
                picker.Pick(selectedColorIndex);
                selectedColorIndex = picker.PickedColorIndex;
                UpdateColorLabel();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !RemoveRequested)
            {
                item.X = (int)numericX.Value;
                item.Y = (int)numericY.Value;
                item.Text = textBoxText.Text;
                string hex = (textBoxValueHex.Text ?? "").Trim().Replace("$", "").Replace("0x", "").Replace("0X", "");
                if (string.IsNullOrEmpty(hex)) hex = "0";
                int val;
                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
                {
                    if (!isTilemap && val > 255) val = 255;
                    if (val < 0) val = 0;
                    item.Value = val;
                }
                item.Color = selectedColorIndex;
            }
            base.OnFormClosing(e);
        }
    }
}
