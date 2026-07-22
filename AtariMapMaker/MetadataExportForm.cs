using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class MetadataExportForm : Form
    {
        private AtariMap map;
        private Point screen;
        private List<MetadataLayerItem> items;
        private int screenCharWidth;
        private int screenCharHeight;

        public MetadataExportForm(AtariMap map, Point screen, List<MetadataLayerItem> items)
        {
            this.map = map;
            this.screen = screen;
            this.items = new List<MetadataLayerItem>(items ?? new List<MetadataLayerItem>());
            screenCharWidth = map?.ScreenSize.Width ?? 40;
            screenCharHeight = map?.ScreenSize.Height ?? 25;
            if (map?.IsTilemap == true && map.TilemapInfo != null)
            {
                screenCharWidth = map.ScreenSize.Width;
                screenCharHeight = map.ScreenSize.Height;
            }
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            LoadExportSettings();
            FormClosing += MetadataExportForm_FormClosing;
            RegenerateOutput(null, EventArgs.Empty);
        }

        // Session-only: remembered while the app is running
        private static int savedGroupBy;
        private static int savedCoordOrder;
        private static bool savedIncludeHeader;
        private static bool savedIncludeTypes;
        private static bool savedIncludeColors;
        private static bool savedColumnMode = true;
        private static bool hasSavedSettings;

        private void LoadExportSettings()
        {
            if (!hasSavedSettings) return;
            if (comboGroupBy.Items.Count > 0)
                comboGroupBy.SelectedIndex = Math.Max(0, Math.Min(savedGroupBy, comboGroupBy.Items.Count - 1));
            if (comboCoordOrder.Items.Count > 0)
                comboCoordOrder.SelectedIndex = Math.Max(0, Math.Min(savedCoordOrder, comboCoordOrder.Items.Count - 1));
            checkHeader.Checked = savedIncludeHeader;
            checkBoxIncludeTypes.Checked = savedIncludeTypes;
            checkBoxIncludeColors.Checked = savedIncludeColors;
            checkBoxColumnModeDisplay.Checked = savedColumnMode;
        }

        private void SaveExportSettings()
        {
            savedGroupBy = comboGroupBy.SelectedIndex;
            savedCoordOrder = comboCoordOrder.SelectedIndex;
            savedIncludeHeader = checkHeader.Checked;
            savedIncludeTypes = checkBoxIncludeTypes.Checked;
            savedIncludeColors = checkBoxIncludeColors.Checked;
            savedColumnMode = checkBoxColumnModeDisplay.Checked;
            hasSavedSettings = true;
        }

        private void MetadataExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveExportSettings();
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(textBoxOutput.Text);
                MessageBox.Show("Copied to clipboard.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Copy failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private const int AddressListPerLine = 8;

        private void RegenerateOutput(object sender, EventArgs e)
        {
            var sorted = SortItems();
            int groupBy = comboGroupBy.SelectedIndex;
            int coordOrder = comboCoordOrder.SelectedIndex;
            bool header = checkHeader.Checked;
            bool includeColors = checkBoxIncludeColors.Checked;
            bool includeTypes = checkBoxIncludeTypes.Checked;
            bool columnModeLists = checkBoxColumnModeDisplay.Checked;

            var groups = new List<List<MetadataLayerItem>>();
            if (groupBy == 0)
                groups.Add(sorted);
            else
            {
                object lastKey = null;
                var current = new List<MetadataLayerItem>();
                foreach (var item in sorted)
                {
                    object groupKey = GetGroupKey(item, groupBy);
                    if (groupKey != null && !groupKey.Equals(lastKey))
                    {
                        if (current.Count > 0) { groups.Add(current); current = new List<MetadataLayerItem>(); }
                        lastKey = groupKey;
                    }
                    current.Add(item);
                }
                if (current.Count > 0) groups.Add(current);
            }

            var sb = new StringBuilder();
            if (header)
            {
                if (coordOrder == 2)
                {
                    string opt = "[,value]";
                    if (includeTypes) opt += "[,type]";
                    if (includeColors) opt += "[,color]";
                    sb.AppendLine("; dta a(index)" + opt + " ;text");
                }
                else
                {
                    string opt = "[,value]";
                    if (includeTypes) opt += "[,type]";
                    if (includeColors) opt += "[,color]";
                    sb.AppendLine("; dta x,y" + opt + " ;text");
                }
            }

            for (int g = 0; g < groups.Count; g++)
            {
                var groupItems = groups[g];
                if (groupItems.Count == 0) continue;

                if (groupBy != 0)
                {
                    if (g > 0) sb.AppendLine();
                    sb.AppendLine(GroupComment(groupBy, GetGroupKey(groupItems[0], groupBy)));
                }
                bool four = false; //check if 16bit value exists -> all 16bit
                foreach (var item in groupItems)
                {
                    if (item.Value > 255)
                    {
                        four = true;
                        break;
                    }
                }
                foreach (var item in groupItems)
                {
                    string colorHex = (item.Color & 0xFF).ToString("X2");
                    string colorTail = includeColors ? $",${colorHex}" : "";
                    string typeHex = (item.Type & 0xFF).ToString("X2");
                    string typeTail = includeTypes ? $",${typeHex}" : "";
                    if (coordOrder == 2)
                    {
                        int index = item.Y * screenCharWidth + item.X;
                        string indexHex = (index & 0xFFFF).ToString("X4");
                        string valueHex = four ? $"a(${(item.Value & 0xFFFF).ToString("X4")})" : item.Value.ToString("X2");
                        if (groupBy == 3)
                            sb.AppendLine($"\tdta a(${indexHex}),{valueHex}{typeTail}{colorTail}");
                        else
                            sb.AppendLine($"\tdta a(${indexHex}),{valueHex}{typeTail}{colorTail}\t;{Escape(item.Text)}");
                    }
                    else
                    {
                        string xHex = (item.X & 0xFF).ToString("X2");
                        string yHex = (item.Y & 0xFF).ToString("X2");
                        bool omitValue = (groupBy == 2);
                        bool omitText = (groupBy == 3);
                        if (omitValue)
                            sb.AppendLine(omitText ? $"\tdta ${xHex},${yHex}{typeTail}{colorTail}" : $"\tdta ${xHex},${yHex}{typeTail}{colorTail}\t;{Escape(item.Text)}");
                        else
                        {
                            string valueHex = four ? $"a(${(item.Value & 0xFFFF).ToString("X4")})" : item.Value.ToString("X2");
                            sb.AppendLine(omitText ? $"\tdta ${xHex},${yHex},${valueHex}{typeTail}{colorTail}" : $"\tdta ${xHex},${yHex},${valueHex}{typeTail}{colorTail}\t;{Escape(item.Text)}");
                        }
                    }
                }

                sb.AppendLine();
                if (columnModeLists)
                    AppendAddressLists(sb, groupItems);
            }

            textBoxOutput.Text = sb.ToString();
        }

        private void AppendAddressLists(StringBuilder sb, List<MetadataLayerItem> list)
        {
            if (list.Count == 0) return;

            sb.AppendLine("\t;1. indexes");
            for (int i = 0; i < list.Count; i++)
            {
                int index = list[i].Y * screenCharWidth + list[i].X;
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta a(");
                sb.Append("$" + (index & 0xFFFF).ToString("X4"));
                if ((i + 1) % AddressListPerLine == 0 || i == list.Count - 1) sb.Append(")");
            }
            sb.AppendLine();
            sb.AppendLine("\t;2. lo indexes");
            for (int i = 0; i < list.Count; i++)
            {
                int index = list[i].Y * screenCharWidth + list[i].X;
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + (index & 0xFF).ToString("X2"));
            }
            
            sb.AppendLine();

            sb.AppendLine("\t;3. hi indexes");
            for (int i = 0; i < list.Count; i++)
            {
                int index = list[i].Y * screenCharWidth + list[i].X;
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + ((index >> 8) & 0xFF).ToString("X2"));
            }
            
            sb.AppendLine();

            sb.AppendLine("\t;4. x coordinates");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + (list[i].X & 0xFF).ToString("X2"));
            }
            sb.AppendLine();

            sb.AppendLine("\t;5. y coordinates");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + (list[i].Y & 0xFF).ToString("X2"));
            }
            sb.AppendLine();

            sb.AppendLine("\t;6. types");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + ((int)list[i].Type).ToString("X2"));
            }
            sb.AppendLine();

            sb.AppendLine("\t;7. values");
            bool four = false; //check if 16bit value exists -> all 16bit
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Value > 255)
                {
                    four = true;
                    break;
                }    
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append(four == false ? "\tdta " : "\tdta a(");
                int v = list[i].Value;
                if (four)
                {
                    sb.Append($"${(v & 0xFFFF).ToString("X4")}");
                    if ((i + 1) % AddressListPerLine == 0 || i == list.Count - 1) sb.Append(")");
                }
                else
                    sb.Append($"${v.ToString("X2")}");
            }
            sb.AppendLine();

            sb.AppendLine("\t;8. colors");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0 && i % AddressListPerLine == 0) sb.AppendLine();
                if (i > 0 && i % AddressListPerLine != 0) sb.Append(",");
                if (i % AddressListPerLine == 0) sb.Append("\tdta ");
                sb.Append("$" + ((int)list[i].Color).ToString("X2"));
            }
            sb.AppendLine();
        }

        private static object GetGroupKey(MetadataLayerItem item, int groupBy)
        {
            if (groupBy == 1) return item.Color;
            if (groupBy == 2) return item.Value;
            if (groupBy == 3) return item.Type;
            return null;
        }

        private string GroupComment(int groupBy, object key)
        {
            if (groupBy == 1) return "; ##### color $" + ((int)(byte)key).ToString("X2") + " ##########";
            if (groupBy == 2) return "; ##### value $" + (key is int v ? (v <= 255 ? v.ToString("X2") : (v & 0xFFFF).ToString("X4")) : key.ToString()) + " ##########";
            if (groupBy == 3)
            {
                byte t = key is byte bt ? bt : (byte)0;
                string tx = "";
                if (map?.MetadataTypeLabels != null && map.MetadataTypeLabels.TryGetValue(t, out string lab))
                    tx = lab ?? "";
                return "; ##### type $" + t.ToString("X2") + " text: \"" + tx.Replace("\"", "\\\"") + "\" ##########";
            }
            return "";
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }

        private List<MetadataLayerItem> SortItems()
        {
            int groupBy = comboGroupBy.SelectedIndex;
            int coordOrder = comboCoordOrder.SelectedIndex;
            Func<MetadataLayerItem, int> indexOf = i => i.Y * screenCharWidth + i.X;

            if (groupBy == 0)
            {
                if (coordOrder == 3)
                    return items.OrderBy(i => i.Type).ThenBy(i => i.Value).ThenBy(i => i.Y).ThenBy(i => i.X).ToList();
                if (coordOrder == 4)
                    return items.OrderBy(i => i.Value).ThenBy(i => i.Type).ThenBy(i => i.Y).ThenBy(i => i.X).ToList();
                if (coordOrder == 0) return items.OrderBy(i => i.Y).ThenBy(i => i.X).ToList();
                if (coordOrder == 1) return items.OrderBy(i => i.X).ThenBy(i => i.Y).ToList();
                return items.OrderBy(indexOf).ToList();
            }

            IOrderedEnumerable<MetadataLayerItem> ordered =
                groupBy == 1 ? items.OrderBy(i => i.Color)
                : groupBy == 2 ? items.OrderBy(i => i.Value)
                : groupBy == 3 ? items.OrderBy(i => i.Type)
                : items.OrderBy(i => i.Y);

            return ApplySecondarySortKey(ordered, coordOrder, indexOf).ToList();
        }

        /// <summary>Tie-break after primary group/sort key (color, value, or type).</summary>
        private static IOrderedEnumerable<MetadataLayerItem> ApplySecondarySortKey(
            IOrderedEnumerable<MetadataLayerItem> primary,
            int coordOrder,
            Func<MetadataLayerItem, int> indexOf)
        {
            if (coordOrder == 0) return primary.ThenBy(i => i.Y).ThenBy(i => i.X);
            if (coordOrder == 1) return primary.ThenBy(i => i.X).ThenBy(i => i.Y);
            if (coordOrder == 2) return primary.ThenBy(indexOf);
            if (coordOrder == 3) return primary.ThenBy(i => i.Type).ThenBy(i => i.Value).ThenBy(i => i.Y).ThenBy(i => i.X);
            if (coordOrder == 4) return primary.ThenBy(i => i.Value).ThenBy(i => i.Type).ThenBy(i => i.Y).ThenBy(i => i.X);
            return primary.ThenBy(indexOf);
        }
    }
}
