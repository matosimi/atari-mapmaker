using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    /// <summary>Popup for mass changing metadata: filter by Text/Value/Color, then set Text/Value/Color. Global or current screen.</summary>
    public partial class MetadataMassChangeForm : Form
    {
        private AtariMap map;
        private Func<Point> getScreenForLocal;
        private Action onApply;

        public MetadataMassChangeForm(AtariMap map, Func<Point> getScreenForLocal, Action onApplyCallback)
        {
            this.map = map;
            this.getScreenForLocal = getScreenForLocal ?? (() => new Point(0, 0));
            this.onApply = onApplyCallback;
            InitializeComponent();
            UpdateCount(null, EventArgs.Empty);
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

        private void UpdateCount(object sender, EventArgs e)
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
            UpdateCount(null, EventArgs.Empty);
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
            UpdateCount(null, EventArgs.Empty);
            MessageBox.Show(toRemove.Count + " item(s) deleted.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
