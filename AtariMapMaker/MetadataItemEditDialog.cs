using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public class MetadataItemEditDialog : Form
    {
        private readonly AtariMap map;
        private MetadataLayerItem item;
        private ComboBox comboTypeText;
        private TextBox textBoxValueHex;
        private NumericUpDown numericX;
        private NumericUpDown numericY;
        private Button buttonPickColor;
        private Button buttonGuessColor;
        private Label labelColorHex;
        private byte selectedColorIndex;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonRemove;
        private Button buttonPruneUnusedTypes;
        private bool suppressComboEvent;
        private bool isTilemap;

        /// <summary>True if user clicked Remove (caller should remove item from list).</summary>
        public bool RemoveRequested { get; private set; }

        public MetadataItemEditDialog(AtariMap map, MetadataLayerItem item, string title, bool isTilemap = false)
        {
            this.map = map;
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
            selectedColorIndex = (byte)Math.Max(0, Math.Min(255, (int)this.item.Color));
            if (this.map != null && this.map.MetadataTypeLabels != null && this.map.MetadataTypeLabels.TryGetValue(this.item.Type, out string lab))
                this.item.Text = lab;
            PopulateTypeCombo();
            UpdateColorLabel();
        }

        private void PopulateTypeCombo()
        {
            suppressComboEvent = true;
            comboTypeText.Items.Clear();
            if (map != null && map.MetadataTypeLabels != null)
            {
                foreach (var kv in map.MetadataTypeLabels.OrderBy(k => k.Key))
                    comboTypeText.Items.Add(new MetadataTypeListEntry(kv.Key, kv.Value));
            }
            int select = -1;
            for (int i = 0; i < comboTypeText.Items.Count; i++)
            {
                if (comboTypeText.Items[i] is MetadataTypeListEntry en && en.Type == item.Type)
                {
                    select = i;
                    break;
                }
            }
            if (select >= 0)
                comboTypeText.SelectedIndex = select;
            else
                comboTypeText.Text = "$" + item.Type.ToString("X2") + "  " + (item.Text ?? "");
            suppressComboEvent = false;
        }

        private void InitializeComponent()
        {
            int y = 12;
            Label lblX = new Label();
            lblX.Text = "X:";
            lblX.Location = new Point(12, y);
            lblX.AutoSize = true;
            numericX = new NumericUpDown();
            numericX.Location = new Point(60, y - 2);
            numericX.Width = 60;
            numericX.Minimum = 0;
            numericX.Maximum = 255;
            y += 28;
            Label lblY = new Label();
            lblY.Text = "Y:";
            lblY.Location = new Point(12, y);
            lblY.AutoSize = true;
            numericY = new NumericUpDown();
            numericY.Location = new Point(60, y - 2);
            numericY.Width = 60;
            numericY.Minimum = 0;
            numericY.Maximum = 255;
            y += 28;
            Label lblType = new Label();
            lblType.Text = "Type / text:";
            lblType.Location = new Point(12, y);
            lblType.AutoSize = true;
            comboTypeText = new ComboBox();
            comboTypeText.DropDownStyle = ComboBoxStyle.DropDown;
            comboTypeText.Location = new Point(90, y - 2);
            comboTypeText.Width = 224;
            comboTypeText.Sorted = false;
            comboTypeText.SelectedIndexChanged += ComboTypeText_SelectedIndexChanged;
            buttonPruneUnusedTypes = new Button();
            buttonPruneUnusedTypes.Text = "Prune...";
            buttonPruneUnusedTypes.Location = new Point(318, y - 2);
            buttonPruneUnusedTypes.Size = new Size(78, 23);
            buttonPruneUnusedTypes.UseVisualStyleBackColor = true;
            buttonPruneUnusedTypes.Click += ButtonPruneUnusedTypes_Click;
            var toolTipPrune = new ToolTip();
            toolTipPrune.SetToolTip(buttonPruneUnusedTypes, "Remove type labels not used by any metadata item on the map (updates this list).");
            y += 28;
            Label lblValue = new Label();
            lblValue.Text = "Value (hex):";
            lblValue.Location = new Point(12, y);
            lblValue.AutoSize = true;
            textBoxValueHex = new TextBox();
            textBoxValueHex.Location = new Point(90, y - 2);
            textBoxValueHex.Width = 60;
            textBoxValueHex.MaxLength = 4;
            y += 28;
            Label lblColor = new Label();
            lblColor.Text = "Color:";
            lblColor.Location = new Point(12, y);
            lblColor.AutoSize = true;
            labelColorHex = new Label();
            labelColorHex.Text = "$00";
            labelColorHex.Location = new Point(60, y);
            labelColorHex.AutoSize = true;
            buttonPickColor = new Button();
            buttonPickColor.Text = "Pick...";
            buttonPickColor.Location = new Point(120, y - 2);
            buttonPickColor.Width = 60;
            buttonPickColor.Height = 22;
            buttonPickColor.Click += ButtonPickColor_Click;
            buttonGuessColor = new Button();
            buttonGuessColor.Text = "Suggest";
            buttonGuessColor.Location = new Point(186, y - 2);
            buttonGuessColor.Width = 58;
            buttonGuessColor.Height = 22;
            buttonGuessColor.Click += ButtonGuessColor_Click;
            y += 32;
            buttonOK = new Button();
            buttonOK.Text = "OK";
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(12, y);
            buttonOK.Size = new Size(75, 25);
            buttonCancel = new Button();
            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(95, y);
            buttonCancel.Size = new Size(75, 25);
            buttonRemove = new Button();
            buttonRemove.Text = "Remove";
            buttonRemove.Location = new Point(178, y);
            buttonRemove.Size = new Size(75, 25);
            buttonRemove.Click += ButtonRemove_Click;

            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
            this.ClientSize = new Size(412, y + 35);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.Add(lblX);
            this.Controls.Add(numericX);
            this.Controls.Add(lblY);
            this.Controls.Add(numericY);
            this.Controls.Add(lblType);
            this.Controls.Add(comboTypeText);
            this.Controls.Add(buttonPruneUnusedTypes);
            this.Controls.Add(lblValue);
            this.Controls.Add(textBoxValueHex);
            this.Controls.Add(lblColor);
            this.Controls.Add(labelColorHex);
            this.Controls.Add(buttonPickColor);
            this.Controls.Add(buttonGuessColor);
            this.Controls.Add(buttonOK);
            this.Controls.Add(buttonCancel);
            this.Controls.Add(buttonRemove);
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            RemoveRequested = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonPruneUnusedTypes_Click(object sender, EventArgs e)
        {
            if (map == null) return;
            int n = MetadataTypeRegistry.PruneUnusedTypeLabels(map);
            if (n == 0)
                MessageBox.Show("No unused type labels to remove.", "Metadata types", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show(n + " unused type label(s) removed from the map.", "Metadata types", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PopulateTypeCombo();
            }
        }

        private void ComboTypeText_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressComboEvent) return;
            if (comboTypeText.SelectedItem is MetadataTypeListEntry ent && map != null)
            {
                byte? c = MetadataTypeRegistry.GetRepresentativeColorForType(map, ent.Type);
                if (c.HasValue)
                    selectedColorIndex = c.Value;
                UpdateColorLabel();
            }
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

        /// <summary>Type byte implied by the type combo (selection, $XX prefix, label lookup, or original item).</summary>
        private byte GetTypeByteForGuess()
        {
            if (comboTypeText.SelectedItem is MetadataTypeListEntry ent)
                return ent.Type;
            string input = MetadataTypeRegistry.NormalizeLabel(comboTypeText.Text);
            if (input.StartsWith("$", StringComparison.Ordinal))
            {
                int i = 1;
                while (i < input.Length && IsHexDigit(input[i])) i++;
                if (i > 1 && byte.TryParse(input.Substring(1, i - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte tb))
                    return tb;
            }
            if (map != null)
            {
                byte? byLabel = MetadataTypeRegistry.FindTypeByLabel(map, input);
                if (byLabel.HasValue)
                    return byLabel.Value;
            }
            return item.Type;
        }

        private static bool IsHexDigit(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        private void ButtonGuessColor_Click(object sender, EventArgs e)
        {
            if (map?.ScreenMetadata == null) return;
            byte typeByte = GetTypeByteForGuess();
            var sameType = new List<MetadataLayerItem>();
            foreach (var other in MetadataTypeRegistry.EnumerateAllItems(map))
            {
                if (ReferenceEquals(other, item)) continue;
                if (other.Type == typeByte)
                    sameType.Add(other);
            }
            if (sameType.Count == 0) return;
            int pick = new Random().Next(sameType.Count);
            selectedColorIndex = (byte)Math.Max(0, Math.Min(255, (int)sameType[pick].Color));
            UpdateColorLabel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !RemoveRequested)
            {
                if (map == null)
                {
                    MessageBox.Show("Map reference missing.", "Metadata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    base.OnFormClosing(e);
                    return;
                }
                MetadataTypeRegistry.EnsureLabelsDictionary(map);
                string input = comboTypeText.Text ?? "";
                if (!MetadataTypeRegistry.TryResolveInput(map, input, out byte resolvedType, out string resolvedLabel, out string err))
                {
                    MessageBox.Show(err ?? "Invalid type / label.", "Metadata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    base.OnFormClosing(e);
                    return;
                }
                item.X = (int)numericX.Value;
                item.Y = (int)numericY.Value;
                item.Type = resolvedType;
                item.Text = resolvedLabel;
                string hex = (textBoxValueHex.Text ?? "").Trim().Replace("$", "").Replace("0x", "").Replace("0X", "");
                if (string.IsNullOrEmpty(hex)) hex = "0";
                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int val))
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
