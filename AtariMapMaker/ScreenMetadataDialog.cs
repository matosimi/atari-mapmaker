using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ScreenMetadataDialog : Form
    {
        private AtariMap map;
        private Point screen;

        public ScreenMetadataDialog(AtariMap map, Point screen)
        {
            this.map = map;
            this.screen = screen;
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            this.Text = $"Screen Metadata - ({screen.X},{screen.Y})";
            LoadItems();
        }

        private string Key => $"{screen.X},{screen.Y}";

        private ScreenMetadata GetOrCreateMetadata()
        {
            if (map.ScreenMetadata == null)
                map.ScreenMetadata = new Dictionary<string, ScreenMetadata>();
            if (!map.ScreenMetadata.ContainsKey(Key))
                map.ScreenMetadata[Key] = new ScreenMetadata();
            return map.ScreenMetadata[Key];
        }

        private void LoadItems()
        {
            listViewItems.Items.Clear();
            if (map?.ScreenMetadata == null) return;
            if (!map.ScreenMetadata.ContainsKey(Key)) return;
            var meta = map.ScreenMetadata[Key];
            if (meta?.ParsedItems == null) return;
            foreach (var item in meta.ParsedItems)
            {
                string valueHex = item.Value >= 0 && item.Value <= 255 ? "$" + item.Value.ToString("X2") : "$" + item.Value.ToString("X4");
                var li = new ListViewItem(new[] {
                    item.X.ToString(),
                    item.Y.ToString(),
                    item.Text ?? "",
                    "$" + item.Type.ToString("X2"),
                    valueHex,
                    "$" + item.Color.ToString("X2")
                });
                li.Tag = item;
                listViewItems.Items.Add(li);
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            var meta = GetOrCreateMetadata();
            var item = new MetadataLayerItem { X = 0, Y = 0, Text = "", Type = 0, Value = 0, Color = 0 };
            meta.ParsedItems.Add(item);
            bool isTilemap = map != null && map.IsTilemap;
            using (var edit = new MetadataItemEditDialog(map, item, "Add metadata item", isTilemap))
            {
                if (edit.ShowDialog() == DialogResult.OK)
                    RefreshItem(item);
            }
            LoadItems();
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0) return;
            var item = listViewItems.SelectedItems[0].Tag as MetadataLayerItem;
            if (item == null) return;
            var meta = GetOrCreateMetadata();
            bool isTilemap = map != null && map.IsTilemap;
            using (var edit = new MetadataItemEditDialog(map, item, "Edit metadata item", isTilemap))
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    if (edit.RemoveRequested)
                        meta.ParsedItems.Remove(item);
                    else
                        RefreshItem(item);
                }
            }
            LoadItems();
        }

        private void RefreshItem(MetadataLayerItem item)
        {
            foreach (ListViewItem li in listViewItems.Items)
            {
                if (li.Tag == item)
                {
                    li.SubItems[0].Text = item.X.ToString();
                    li.SubItems[1].Text = item.Y.ToString();
                    li.SubItems[2].Text = item.Text ?? "";
                    li.SubItems[3].Text = "$" + item.Type.ToString("X2");
                    li.SubItems[4].Text = item.Value >= 0 && item.Value <= 255 ? "$" + item.Value.ToString("X2") : "$" + item.Value.ToString("X4");
                    li.SubItems[5].Text = "$" + item.Color.ToString("X2");
                    break;
                }
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0) return;
            var item = listViewItems.SelectedItems[0].Tag as MetadataLayerItem;
            if (item == null) return;
            var meta = GetOrCreateMetadata();
            meta.ParsedItems.Remove(item);
            LoadItems();
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            var meta = GetOrCreateMetadata();
            if (meta?.ParsedItems == null || meta.ParsedItems.Count == 0)
            {
                MessageBox.Show("No metadata items to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var exportForm = new MetadataExportForm(map, screen, meta.ParsedItems))
            {
                exportForm.ShowDialog();
            }
        }
    }
}
