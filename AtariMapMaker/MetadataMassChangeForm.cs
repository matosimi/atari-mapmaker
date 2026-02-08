using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    /// <summary>Popup for mass changing metadata: filter by Text/Value/Color, then set Text/Value/Color. Global or current screen.</summary>
    public class MetadataMassChangeForm : Form
    {
        private AtariMap map;
        private Func<Point> getScreenForLocal;
        private ComboBox comboFilterBy;
        private TextBox textFilterValue;
        private ComboBox comboChangeField;
        private TextBox textNewValue;
        private CheckBox checkGlobal;
        private Label labelCount;
        private Button buttonApply;
        private Button buttonDelete;
        private Action onApply;

        public MetadataMassChangeForm(AtariMap map, Func<Point> getScreenForLocal, Action onApplyCallback)
        {
            this.map = map;
            this.getScreenForLocal = getScreenForLocal ?? (() => new Point(0, 0));
            this.onApply = onApplyCallback;
            InitializeComponent();
            UpdateCount();
        }

        private void InitializeComponent()
        {
            int y = 12;
            var lblFilter = new Label { Text = "Filter by:", Location = new Point(12, y), AutoSize = true };
            comboFilterBy = new ComboBox { Location = new Point(120, y - 2), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            comboFilterBy.Items.AddRange(new object[] { "Text", "Value", "Color" });
            comboFilterBy.SelectedIndex = 0;
            comboFilterBy.SelectedIndexChanged += (s, e) => UpdateCount();
            y += 28;
            var lblFilterVal = new Label { Text = "Filter value:", Location = new Point(12, y), AutoSize = true };
            textFilterValue = new TextBox { Location = new Point(120, y - 2), Width = 180 };
            textFilterValue.TextChanged += (s, e) => UpdateCount();
            y += 28;
            var lblChange = new Label { Text = "Change:", Location = new Point(12, y), AutoSize = true };
            comboChangeField = new ComboBox { Location = new Point(120, y - 2), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            comboChangeField.Items.AddRange(new object[] { "Text", "Value", "Color" });
            comboChangeField.SelectedIndex = 0;
            y += 28;
            var lblNewVal = new Label { Text = "New value:", Location = new Point(12, y), AutoSize = true };
            textNewValue = new TextBox { Location = new Point(120, y - 2), Width = 180 };
            y += 28;
            checkGlobal = new CheckBox { Text = "Global (all screens)", Location = new Point(12, y), AutoSize = true, Checked = true };
            checkGlobal.CheckedChanged += (s, e) => UpdateCount();
            y += 26;
            labelCount = new Label { Text = "0 items selected", Location = new Point(12, y), AutoSize = true };
            y += 28;
            buttonApply = new Button { Text = "Apply", Location = new Point(12, y), Size = new Size(85, 28) };
            buttonApply.Click += ButtonApply_Click;
            buttonDelete = new Button { Text = "Delete", Location = new Point(105, y), Size = new Size(85, 28) };
            buttonDelete.Click += ButtonDelete_Click;
            this.ClientSize = new Size(312, y + 40);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Mass change metadata";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.Add(lblFilter); this.Controls.Add(comboFilterBy);
            this.Controls.Add(lblFilterVal); this.Controls.Add(textFilterValue);
            this.Controls.Add(lblChange); this.Controls.Add(comboChangeField);
            this.Controls.Add(lblNewVal); this.Controls.Add(textNewValue);
            this.Controls.Add(checkGlobal);
            this.Controls.Add(labelCount);
            this.Controls.Add(buttonApply); this.Controls.Add(buttonDelete);
        }

        private IEnumerable<MetadataLayerItem> GetItemsInScope()
        {
            if (map?.ScreenMetadata == null) yield break;
            if (checkGlobal.Checked)
            {
                foreach (var kv in map.ScreenMetadata)
                {
                    if (kv.Value?.ParsedItems == null) continue;
                    foreach (var item in kv.Value.ParsedItems)
                        yield return item;
                }
            }
            else
            {
                Point screenLocal = getScreenForLocal();
                string key = $"{screenLocal.X},{screenLocal.Y}";
                if (!map.ScreenMetadata.TryGetValue(key, out var meta) || meta?.ParsedItems == null) yield break;
                foreach (var item in meta.ParsedItems)
                    yield return item;
            }
        }

        private IEnumerable<KeyValuePair<string, MetadataLayerItem>> GetItemsInScopeWithKeys()
        {
            if (map?.ScreenMetadata == null) yield break;
            if (checkGlobal.Checked)
            {
                foreach (var kv in map.ScreenMetadata)
                {
                    if (kv.Value?.ParsedItems == null) continue;
                    foreach (var item in kv.Value.ParsedItems)
                        yield return new KeyValuePair<string, MetadataLayerItem>(kv.Key, item);
                }
            }
            else
            {
                Point screenLocal = getScreenForLocal();
                string key = $"{screenLocal.X},{screenLocal.Y}";
                if (!map.ScreenMetadata.TryGetValue(key, out var meta) || meta?.ParsedItems == null) yield break;
                foreach (var item in meta.ParsedItems)
                    yield return new KeyValuePair<string, MetadataLayerItem>(key, item);
            }
        }

        private bool TryParseColor(string s, out byte value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().Replace("$", "");
            return byte.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)
                || byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private bool TryParseValue(string s, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().Replace("$", "");
            if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) return true;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private bool Matches(MetadataLayerItem item, int filterBy, string filterVal)
        {
            if (string.IsNullOrWhiteSpace(filterVal)) return false;
            filterVal = filterVal.Trim();
            switch (filterBy)
            {
                case 0: return (item.Text ?? "").Trim().Equals(filterVal, StringComparison.OrdinalIgnoreCase);
                case 1: return TryParseValue(filterVal, out int v) && item.Value == v;
                case 2: return TryParseColor(filterVal, out byte c) && item.Color == c;
                default: return false;
            }
        }

        private void UpdateCount()
        {
            int filterBy = comboFilterBy.SelectedIndex;
            string filterVal = textFilterValue?.Text ?? "";
            int n = GetItemsInScope().Count(item => Matches(item, filterBy, filterVal));
            if (labelCount != null)
                labelCount.Text = n + " item(s) selected";
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            int filterBy = comboFilterBy.SelectedIndex;
            int changeField = comboChangeField.SelectedIndex;
            string filterVal = textFilterValue.Text ?? "";
            string newVal = textNewValue.Text ?? "";

            if (string.IsNullOrWhiteSpace(filterVal))
            {
                MessageBox.Show("Please enter a filter value.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (changeField == 0)
            {
                // Change Text: new value is string, no parse
            }
            else if (changeField == 1)
            {
                if (!TryParseValue(newVal, out _))
                {
                    MessageBox.Show("New value must be a number (hex $XX/$XXXX or decimal).", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (!TryParseColor(newVal, out _))
                {
                    MessageBox.Show("New value must be a color (hex $XX or 0-255).", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int count = 0;
            foreach (var item in GetItemsInScope().Where(i => Matches(i, filterBy, filterVal)))
            {
                if (changeField == 0) item.Text = newVal;
                else if (changeField == 1) { TryParseValue(newVal, out int v); item.Value = v; }
                else { TryParseColor(newVal, out byte c); item.Color = (byte)(c & 0xFE); }
                count++;
            }

            onApply?.Invoke();
            UpdateCount();
            MessageBox.Show(count + " item(s) updated.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            int filterBy = comboFilterBy.SelectedIndex;
            string filterVal = textFilterValue.Text ?? "";
            if (string.IsNullOrWhiteSpace(filterVal))
            {
                MessageBox.Show("Please enter a filter value.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var toRemove = GetItemsInScopeWithKeys()
                .Where(p => Matches(p.Value, filterBy, filterVal))
                .ToList();
            if (toRemove.Count == 0)
            {
                MessageBox.Show("No items match the filter.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var result = MessageBox.Show("Delete " + toRemove.Count + " item(s)?", "Mass change - Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            foreach (var p in toRemove)
            {
                if (map.ScreenMetadata != null && map.ScreenMetadata.TryGetValue(p.Key, out var meta) && meta?.ParsedItems != null)
                    meta.ParsedItems.Remove(p.Value);
            }
            onApply?.Invoke();
            UpdateCount();
            MessageBox.Show(toRemove.Count + " item(s) deleted.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
