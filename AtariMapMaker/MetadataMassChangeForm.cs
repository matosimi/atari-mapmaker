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
            this.Font = new Font("Segoe UI", 8F);
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

        private bool TryParseTypeByte(string s, out byte value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().Replace("$", "");
            return byte.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)
                || byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
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
                case 3: return TryParseTypeByte(filterVal, out byte t) && item.Type == t;
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
            else if (changeField == 2)
            {
                if (!TryParseColor(newVal, out _))
                {
                    MessageBox.Show("New value must be a color (hex $XX or 0-255).", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (changeField == 3)
            {
                if (!TryParseTypeByte(newVal, out _))
                {
                    MessageBox.Show("New type must be a byte (hex $00-$FF or decimal 0-255).", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Invalid change field.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var matched = GetItemsInScope().Where(i => Matches(i, filterBy, filterVal)).ToList();

            if (changeField == 0)
            {
                var distinctTypes = matched.Select(i => i.Type).Distinct().ToList();
                if (distinctTypes.Count > 1)
                {
                    MessageBox.Show("Mass text change applies when all matched items share the same type byte. Filter to one type or change items separately.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (distinctTypes.Count == 1 && map != null)
                {
                    MetadataTypeRegistry.EnsureLabelsDictionary(map);
                    try
                    {
                        MetadataTypeRegistry.SetLabelForTypeAndSyncItems(map, distinctTypes[0], newVal);
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.Message, "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                onApply?.Invoke();
                UpdateCount(null, EventArgs.Empty);
                MessageBox.Show(matched.Count + " item(s) updated.", "Mass change", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int count = 0;
            foreach (var item in matched)
            {
                if (changeField == 1) { TryParseValue(newVal, out int v); item.Value = v; }
                else if (changeField == 2) { TryParseColor(newVal, out byte c); item.Color = (byte)(c & 0xFE); }
                else if (changeField == 3)
                {
                    TryParseTypeByte(newVal, out byte newType);
                    MetadataTypeRegistry.EnsureLabelsDictionary(map);
                    if (!map.MetadataTypeLabels.TryGetValue(newType, out string lab))
                    {
                        string hint = matched.Count > 0 ? (matched[0].Text ?? "").Trim() : "";
                        if (!string.IsNullOrEmpty(hint))
                        {
                            byte? otherType = MetadataTypeRegistry.FindTypeByLabel(map, hint);
                            if (otherType.HasValue && otherType.Value != newType)
                                hint = "";
                        }
                        if (string.IsNullOrEmpty(hint))
                            hint = "T" + newType.ToString("X2");
                        map.MetadataTypeLabels[newType] = hint;
                        lab = hint;
                    }
                    item.Type = newType;
                    item.Text = lab;
                }
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
