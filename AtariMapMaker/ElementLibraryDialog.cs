using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class ElementLibraryDialog : Form
    {
        private AtariMap map;
        private ListView listViewElements;
        private Button buttonSaveSelection;
        private Button buttonPaste;
        private Button buttonRename;
        private Button buttonDelete;
        private Button buttonClose;
        private CheckBox checkBoxSkipZeroBytes;

        public ElementLibraryDialog(AtariMap map)
        {
            this.map = map;
            InitializeComponent();
            RefreshElementList();
        }

        private void InitializeComponent()
        {
            this.listViewElements = new ListView();
            this.buttonSaveSelection = new Button();
            this.buttonPaste = new Button();
            this.buttonRename = new Button();
            this.buttonDelete = new Button();
            this.buttonClose = new Button();
            this.checkBoxSkipZeroBytes = new CheckBox();
            this.SuspendLayout();

            // listViewElements
            this.listViewElements.FullRowSelect = true;
            this.listViewElements.GridLines = true;
            this.listViewElements.Location = new Point(12, 12);
            this.listViewElements.Name = "listViewElements";
            this.listViewElements.Size = new Size(400, 300);
            this.listViewElements.View = View.Details;
            this.listViewElements.Columns.Add("Name", 200);
            this.listViewElements.Columns.Add("Size", 100);
            this.listViewElements.DoubleClick += ListViewElements_DoubleClick;

            // buttonSaveSelection
            this.buttonSaveSelection.Text = "Save Selection to Library";
            this.buttonSaveSelection.Location = new Point(12, 320);
            this.buttonSaveSelection.Size = new Size(180, 30);
            this.buttonSaveSelection.Click += ButtonSaveSelection_Click;

            // buttonPaste
            this.buttonPaste.Text = "Paste Selected";
            this.buttonPaste.Location = new Point(200, 320);
            this.buttonPaste.Size = new Size(100, 30);
            this.buttonPaste.Click += ButtonPaste_Click;

            // checkBoxSkipZeroBytes
            this.checkBoxSkipZeroBytes.Text = "Skip Zero Bytes (Transparency)";
            this.checkBoxSkipZeroBytes.Location = new Point(310, 325);
            this.checkBoxSkipZeroBytes.AutoSize = true;

            // buttonRename
            this.buttonRename.Text = "Rename";
            this.buttonRename.Location = new Point(12, 360);
            this.buttonRename.Size = new Size(100, 30);
            this.buttonRename.Click += ButtonRename_Click;

            // buttonDelete
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.Location = new Point(120, 360);
            this.buttonDelete.Size = new Size(100, 30);
            this.buttonDelete.Click += ButtonDelete_Click;

            // buttonClose
            this.buttonClose.Text = "Close";
            this.buttonClose.DialogResult = DialogResult.OK;
            this.buttonClose.Location = new Point(330, 360);
            this.buttonClose.Size = new Size(82, 30);

            // ElementLibraryDialog
            this.ClientSize = new Size(424, 402);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonRename);
            this.Controls.Add(this.checkBoxSkipZeroBytes);
            this.Controls.Add(this.buttonPaste);
            this.Controls.Add(this.buttonSaveSelection);
            this.Controls.Add(this.listViewElements);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementLibraryDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Element Library";
            this.ResumeLayout(false);
            this.PerformLayout();
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
                    // Get selection rectangle from clipboard
                    Rectangle selection = new Rectangle(0, 0, AtariClipboard.ClipboardWidth, AtariClipboard.ClipboardHeight);
                    ElementLibraryManager.SaveToLibrary(map, selection, dialog.InputText);
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
            // Paste at current cursor position (simplified - would need to get from map)
            Point destination = new Point(0, 0); // TODO: Get from current map position
            ElementLibraryManager.PasteFromLibrary(map, elementName, destination, checkBoxSkipZeroBytes.Checked);
            this.DialogResult = DialogResult.OK;
        }

        private void ListViewElements_DoubleClick(object sender, EventArgs e)
        {
            ButtonPaste_Click(sender, e);
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
