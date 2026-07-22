using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            listViewItems.MultiSelect = true;
            LoadItems();
            UpdatePasteButton();
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

        private void UpdatePasteButton()
        {
            int n = ScreenMetadataListClipboard.Count;
            buttonPaste.Text = n > 0 ? $"Paste ({n})" : "Paste";
            buttonPaste.Enabled = n > 0;
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
            var meta = GetOrCreateMetadata();
            var toRemove = new List<MetadataLayerItem>();
            foreach (ListViewItem li in listViewItems.SelectedItems)
            {
                if (li.Tag is MetadataLayerItem item)
                    toRemove.Add(item);
            }
            foreach (var item in toRemove)
                meta.ParsedItems.Remove(item);
            LoadItems();
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select one or more metadata items to copy.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var selected = new List<MetadataLayerItem>();
            foreach (ListViewItem li in listViewItems.SelectedItems)
            {
                if (li.Tag is MetadataLayerItem item)
                    selected.Add(item);
            }
            ScreenMetadataListClipboard.Copy(selected);
            UpdatePasteButton();
        }

        private void ButtonPaste_Click(object sender, EventArgs e)
        {
            if (ScreenMetadataListClipboard.Count == 0) return;
            var meta = GetOrCreateMetadata();
            if (meta.ParsedItems == null)
                meta.ParsedItems = new List<MetadataLayerItem>();

            var overlapping = ScreenMetadataListClipboard.Items
                .Where(c => meta.ParsedItems.Any(e2 => e2.X == c.X && e2.Y == c.Y))
                .ToList();
            if (overlapping.Count > 0)
            {
                string msg = overlapping.Count == 1
                    ? $"Clipboard item at ({overlapping[0].X},{overlapping[0].Y}) overlaps existing metadata on this screen. Paste anyway?"
                    : $"{overlapping.Count} clipboard items overlap existing metadata on this screen. Paste anyway?";
                if (MessageBox.Show(msg, "Paste overlap", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                    return;
            }

            foreach (var c in ScreenMetadataListClipboard.Items)
            {
                meta.ParsedItems.RemoveAll(e2 => e2.X == c.X && e2.Y == c.Y);
                meta.ParsedItems.Add(new MetadataLayerItem
                {
                    X = c.X,
                    Y = c.Y,
                    Text = c.Text ?? "",
                    Type = c.Type,
                    Value = c.Value,
                    Color = c.Color
                });
            }
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
