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
            RegenerateOutput(null, EventArgs.Empty);
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
                sb.AppendLine(coordOrder == 2 ? "; dta a(index)[,value] ;text" : "; dta x,y[,value] ;text");

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
                    if (coordOrder == 2)
                    {
                        int index = item.Y * screenCharWidth + item.X;
                        string indexHex = (index & 0xFFFF).ToString("X4");
                        string valueHex = four ? $"a(${(item.Value & 0xFFFF).ToString("X4")})" : item.Value.ToString("X2");
                        if (groupBy == 3)
                            sb.AppendLine($"\tdta a(${indexHex}),{valueHex}");
                        else
                            sb.AppendLine($"\tdta a(${indexHex}),{valueHex}\t;{Escape(item.Text)}");
                    }
                    else
                    {
                        string xHex = (item.X & 0xFF).ToString("X2");
                        string yHex = (item.Y & 0xFF).ToString("X2");
                        bool omitValue = (groupBy == 2);
                        bool omitText = (groupBy == 3);
                        if (omitValue)
                            sb.AppendLine(omitText ? $"\tdta ${xHex},${yHex}" : $"\tdta ${xHex},${yHex}\t;{Escape(item.Text)}");
                        else
                        {
                            string valueHex = four ? $"a(${(item.Value & 0xFFFF).ToString("X4")})" : item.Value.ToString("X2");
                            sb.AppendLine(omitText ? $"\tdta ${xHex},${yHex},${valueHex}" : $"\tdta ${xHex},${yHex},${valueHex}\t;{Escape(item.Text)}");
                        }
                    }
                }

                sb.AppendLine();
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

            sb.AppendLine("\t;6. values");
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

            sb.AppendLine("\t;7. colors");
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
            if (groupBy == 3) return item.Text ?? "";
            return null;
        }

        private static string GroupComment(int groupBy, object key)
        {
            if (groupBy == 1) return "; ##### color $" + ((int)(byte)key).ToString("X2") + " ##########";
            if (groupBy == 2) return "; ##### value $" + (key is int v ? (v <= 255 ? v.ToString("X2") : (v & 0xFFFF).ToString("X4")) : key.ToString()) + " ##########";
            if (groupBy == 3) return "; ##### text: \"" + (key.ToString().Replace("\"", "\\\"")) + "\" ##########";
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
            List<MetadataLayerItem> byPosition()
            {
                if (coordOrder == 0) return items.OrderBy(i => i.Y).ThenBy(i => i.X).ToList();
                if (coordOrder == 1) return items.OrderBy(i => i.X).ThenBy(i => i.Y).ToList();
                return items.OrderBy(indexOf).ToList();
            }
            if (groupBy == 0) return byPosition();
            if (groupBy == 1) return coordOrder == 0 ? items.OrderBy(i => i.Color).ThenBy(i => i.Y).ThenBy(i => i.X).ToList()
                : coordOrder == 1 ? items.OrderBy(i => i.Color).ThenBy(i => i.X).ThenBy(i => i.Y).ToList()
                : items.OrderBy(i => i.Color).ThenBy(indexOf).ToList();
            if (groupBy == 2) return coordOrder == 0 ? items.OrderBy(i => i.Value).ThenBy(i => i.Y).ThenBy(i => i.X).ToList()
                : coordOrder == 1 ? items.OrderBy(i => i.Value).ThenBy(i => i.X).ThenBy(i => i.Y).ToList()
                : items.OrderBy(i => i.Value).ThenBy(indexOf).ToList();
            if (groupBy == 3) return coordOrder == 0 ? items.OrderBy(i => i.Text ?? "").ThenBy(i => i.Y).ThenBy(i => i.X).ToList()
                : coordOrder == 1 ? items.OrderBy(i => i.Text ?? "").ThenBy(i => i.X).ThenBy(i => i.Y).ToList()
                : items.OrderBy(i => i.Text ?? "").ThenBy(indexOf).ToList();
            return byPosition();
        }
    }
}
