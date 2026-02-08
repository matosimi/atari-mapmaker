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
        private ListView listViewItems;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonDelete;
        private Button buttonExport;
        private Button buttonOK;

        public ScreenMetadataDialog(AtariMap map, Point screen)
        {
            this.map = map;
            this.screen = screen;
            InitializeComponent();
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

        private void InitializeComponent()
        {
            this.listViewItems = new ListView();
            this.listViewItems.View = View.Details;
            this.listViewItems.FullRowSelect = true;
            this.listViewItems.GridLines = true;
            this.listViewItems.Columns.Add("X", 40);
            this.listViewItems.Columns.Add("Y", 40);
            this.listViewItems.Columns.Add("Text", 120);
            this.listViewItems.Columns.Add("Value", 50);
            this.listViewItems.Columns.Add("Color", 50);
            this.listViewItems.Location = new Point(12, 12);
            this.listViewItems.Size = new Size(400, 180);
            this.listViewItems.DoubleClick += (s, e) => ButtonEdit_Click(s, e);

            this.buttonAdd = new Button();
            this.buttonAdd.Text = "Add";
            this.buttonAdd.Location = new Point(12, 200);
            this.buttonAdd.Size = new Size(60, 25);
            this.buttonAdd.Click += ButtonAdd_Click;

            this.buttonEdit = new Button();
            this.buttonEdit.Text = "Edit";
            this.buttonEdit.Location = new Point(78, 200);
            this.buttonEdit.Size = new Size(60, 25);
            this.buttonEdit.Click += ButtonEdit_Click;

            this.buttonDelete = new Button();
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.Location = new Point(144, 200);
            this.buttonDelete.Size = new Size(60, 25);
            this.buttonDelete.Click += ButtonDelete_Click;

            this.buttonExport = new Button();
            this.buttonExport.Text = "Export...";
            this.buttonExport.Location = new Point(210, 200);
            this.buttonExport.Size = new Size(75, 25);
            this.buttonExport.Click += ButtonExport_Click;

            this.buttonOK = new Button();
            this.buttonOK.Text = "OK";
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(337, 200);
            this.buttonOK.Size = new Size(75, 25);

            this.AcceptButton = this.buttonOK;
            this.CancelButton = new Button { DialogResult = DialogResult.Cancel };
            this.ClientSize = new Size(424, 235);
            this.Controls.Add(this.listViewItems);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = $"Screen Metadata - ({screen.X},{screen.Y})";
            this.StartPosition = FormStartPosition.CenterParent;
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
            var item = new MetadataLayerItem { X = 0, Y = 0, Text = "", Value = 0, Color = 0 };
            meta.ParsedItems.Add(item);
            bool isTilemap = map != null && map.IsTilemap;
            using (var edit = new MetadataItemEditDialog(item, "Add metadata item", isTilemap))
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
            using (var edit = new MetadataItemEditDialog(item, "Edit metadata item", isTilemap))
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
                    li.SubItems[3].Text = item.Value.ToString();
                    li.SubItems[4].Text = item.Color.ToString();
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
