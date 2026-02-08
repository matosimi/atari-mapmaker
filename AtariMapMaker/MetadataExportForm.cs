using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public class MetadataExportForm : Form
    {
        private AtariMap map;
        private Point screen;
        private List<MetadataLayerItem> items;
        private ComboBox comboGroupBy;
        private ComboBox comboCoordOrder;
        private CheckBox checkHeader;
        private TextBox textBoxOutput;
        private Button buttonCopy;
        private Button buttonClose;
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
                // For tilemap, metadata X,Y are tile positions; index = tile index (tiles per row = ScreenSize.Width)
                screenCharWidth = map.ScreenSize.Width;
                screenCharHeight = map.ScreenSize.Height;
            }
            InitializeComponent();
            RegenerateOutput();
        }

        private void InitializeComponent()
        {
            int y = 12;
            var lblGroup = new Label { Text = "Group by:", Location = new Point(12, y), AutoSize = true };
            comboGroupBy = new ComboBox { Location = new Point(100, y - 2), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            comboGroupBy.Items.AddRange(new object[] { "None", "Color", "Value", "Text" });
            comboGroupBy.SelectedIndex = 0;
            comboGroupBy.SelectedIndexChanged += (s, e) => RegenerateOutput();
            y += 28;
            var lblCoord = new Label { Text = "Coordinate / order:", Location = new Point(12, y), AutoSize = true };
            comboCoordOrder = new ComboBox { Location = new Point(140, y - 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            comboCoordOrder.Items.AddRange(new object[] { "x,y (order Y then X)", "x,y (order X then Y)", "index dta a($xxxx)" });
            comboCoordOrder.SelectedIndex = 0;
            comboCoordOrder.SelectedIndexChanged += (s, e) => RegenerateOutput();
            y += 28;
            checkHeader = new CheckBox { Text = "Include header line", Location = new Point(12, y), Checked = false };
            checkHeader.CheckedChanged += (s, e) => RegenerateOutput();
            y += 28;
            var lblOut = new Label { Text = "Export output:", Location = new Point(12, y), AutoSize = true };
            y += 22;
            textBoxOutput = new TextBox { Location = new Point(12, y), Size = new Size(520, 320), Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true, Font = new Font("Consolas", 9) };
            y += 328;
            buttonCopy = new Button { Text = "Copy to clipboard", Location = new Point(12, y), Size = new Size(120, 28) };
            buttonCopy.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(textBoxOutput.Text);
                    MessageBox.Show("Copied to clipboard.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Copy failed", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            buttonClose = new Button { Text = "Close", Location = new Point(272, y), Size = new Size(120, 28), DialogResult = DialogResult.OK };
            this.AcceptButton = buttonClose;
            this.CancelButton = buttonClose;
            this.ClientSize = new Size(544, y + 40);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Export metadata";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.Add(lblGroup); this.Controls.Add(comboGroupBy);
            this.Controls.Add(lblCoord); this.Controls.Add(comboCoordOrder);
            this.Controls.Add(checkHeader);
            this.Controls.Add(lblOut); this.Controls.Add(textBoxOutput);
            this.Controls.Add(buttonCopy); this.Controls.Add(buttonClose);
        }

        private const int AddressListPerLine = 8;

        private void RegenerateOutput()
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
                sb.AppendLine(coordOrder == 2 ? "\t; dta a(index),a(value) ;text" : "\t; dta x,y,value ;text");

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
            if (groupBy == 1) return "; ##### color $" + ((int)(byte)key).ToString("X2") + " #####";
            if (groupBy == 2) return "; ##### value $" + (key is int v ? (v <= 255 ? v.ToString("X2") : (v & 0xFFFF).ToString("X4")) : key.ToString()) + " #####";
            if (groupBy == 3) return "; ##### text: \"" + (key.ToString().Replace("\"", "\\\"")) + "\" #####";
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
