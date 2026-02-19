using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class MetadataItemEditDialog : Form
    {
        private MetadataLayerItem item;
        private byte selectedColorIndex;
        private bool isTilemap;

        /// <summary>True if user clicked Remove (caller should remove item from list).</summary>
        public bool RemoveRequested { get; private set; }

        private static readonly string[] LasermaniaTypes = new[]
        {
            "end", "laser", "start", "key", "gate", "beam_in", "beam_out", "color accent"/*, "memory capsule", "sensor"*/
        };

        private static byte GetPredefinedColor(string type)
        {
            if (string.IsNullOrEmpty(type)) return 0x70;
            switch (type.Trim().ToLowerInvariant())
            {
                case "end": return 0xda;
                case "laser": return 0x3a;
                case "start": return 0xb8;
                case "key": return 0x4a;
                case "gate": return 0x44;
                case "beam_in": return 0x76;
                case "beam_out": return 0x72;
                case "color accent": return 0x70;
                /*case "memory capsule": return 0x21;
                case "sensor": return 0x35; */
                default: return 0x70;
            }
        }

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
            textBoxValueHex.Text = this.item.Value >= 0 && this.item.Value <= 255
                ? this.item.Value.ToString("X2")
                : this.item.Value.ToString("X4");
            selectedColorIndex = (byte)Math.Max((byte)0, Math.Min((byte)255, this.item.Color));

            if (isTilemap)
            {
                // Lasermania: only valid types from combobox, predefined color per type, no color picking
                comboBoxType.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBoxType.Items.Clear();
                comboBoxType.Items.AddRange(LasermaniaTypes.Cast<object>().ToArray());
                string existing = (this.item.Text ?? "").Trim();
                int idx = Array.FindIndex(LasermaniaTypes, t => string.Equals(t, existing, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                    comboBoxType.SelectedIndex = idx;
                else if (LasermaniaTypes.Length > 0)
                    comboBoxType.SelectedIndex = 0;
                selectedColorIndex = GetPredefinedColor(comboBoxType.Text);
                buttonPickColor.Enabled = false;
            }
            else
            {
                // Free-form: editable combobox (user can type any text), color picker enabled
                comboBoxType.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxType.Text = this.item.Text ?? "";
            }

            UpdateColorLabel();
        }

        private void ComboBoxType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (!isTilemap) return;
            string t = comboBoxType.Text;
            if (!string.IsNullOrEmpty(t))
            {
                selectedColorIndex = GetPredefinedColor(t);
                UpdateColorLabel();
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            // OK just closes with DialogResult.OK
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            RemoveRequested = true;
            DialogResult = DialogResult.OK;
            Close();
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
                item.Text = comboBoxType.Text?.Trim() ?? "";
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
