using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ElementLibraryDialog : Form
    {
        private AtariMap map;
        private Action onClipboardSet;
        private Action onInvertRequested;

        public ElementLibraryDialog(AtariMap map, Action onClipboardSet = null, Action onInvertRequested = null)
        {
            this.map = map;
            this.onClipboardSet = onClipboardSet;
            this.onInvertRequested = onInvertRequested;
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            this.KeyPreview = true;
            this.KeyDown += ElementLibraryDialog_KeyDown;
            this.FormClosing += ElementLibraryDialog_FormClosing;
            RefreshElementList();
        }

        private void ElementLibraryDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide instead of close so the form is never disposed (only when app exits)
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void ElementLibraryDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.I && AtariClipboard.IsValid && !AtariClipboard.IsTileIndexes)
            {
                onInvertRequested?.Invoke();
                e.Handled = true;
            }
        }

        public void SetMap(AtariMap newMap)
        {
            map = newMap;
            RefreshElementList();
        }

        private void RefreshElementList()
        {
            listViewElements.Items.Clear();
            if (map != null && map.ElementLibrary != null)
            {
                foreach (var element in map.ElementLibrary.Values)
                {
                    ListViewItem item = new ListViewItem(element.Name);
                    item.SubItems.Add($"{element.Size.Width}×{element.Size.Height}");
                    item.Tag = element.Name;
                    listViewElements.Items.Add(item);
                }
            }
        }

        private void ButtonSaveSelection_Click(object sender, EventArgs e)
        {
            if (!AtariClipboard.IsValid)
            {
                MessageBox.Show("Please select an area first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (InputDialog dialog = new InputDialog("Enter element name:", "Save to Library", "Element1"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    ElementLibraryManager.SaveClipboardToLibrary(map, dialog.InputText);
                    RefreshElementList();
                }
            }
        }

        private void ButtonPaste_Click(object sender, EventArgs e)
        {
            if (listViewElements.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an element to paste.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string elementName = listViewElements.SelectedItems[0].Tag.ToString();
            Point destination = new Point(0, 0);
            ElementLibraryManager.PasteFromLibrary(map, elementName, destination, AtariClipboard.SkipZero);
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void CopySelectedElementToClipboard()
        {
            if (listViewElements.SelectedItems.Count == 0)
                return;
            string elementName = listViewElements.SelectedItems[0].Tag?.ToString();
            if (string.IsNullOrEmpty(elementName) || map?.ElementLibrary == null || !map.ElementLibrary.ContainsKey(elementName))
                return;
            LibraryElement element = map.ElementLibrary[elementName];
            AtariClipboard.CopyFromLibraryElement(element, map);
            onClipboardSet?.Invoke();
        }

        private void ListViewElements_SelectedIndexChanged(object sender, EventArgs e)
        {
            CopySelectedElementToClipboard();
        }

        private void ListViewElements_DoubleClick(object sender, EventArgs e)
        {
            CopySelectedElementToClipboard();
        }

        private void ButtonRename_Click(object sender, EventArgs e)
        {
            if (listViewElements.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an element to rename.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string oldName = listViewElements.SelectedItems[0].Tag.ToString();
            using (InputDialog dialog = new InputDialog("Enter new name:", "Rename Element", oldName))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    string newName = dialog.InputText;
                    if (newName != oldName)
                    {
                        if (ElementLibraryManager.RenameElement(map, oldName, newName))
                        {
                            RefreshElementList();
                        }
                        else
                        {
                            MessageBox.Show("Failed to rename element. Name may already exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (listViewElements.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an element to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string elementName = listViewElements.SelectedItems[0].Tag.ToString();
            if (MessageBox.Show($"Delete element '{elementName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ElementLibraryManager.DeleteElement(map, elementName);
                RefreshElementList();
            }
        }
    }
}
