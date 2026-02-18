namespace AtariMapMaker
{
    partial class ElementLibraryDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.listViewElements = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonSaveSelection = new System.Windows.Forms.Button();
            this.buttonPaste = new System.Windows.Forms.Button();
            this.buttonRename = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listViewElements
            //
            this.listViewElements.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderSize});
            this.listViewElements.FullRowSelect = true;
            this.listViewElements.GridLines = true;
            this.listViewElements.Location = new System.Drawing.Point(12, 12);
            this.listViewElements.Name = "listViewElements";
            this.listViewElements.Size = new System.Drawing.Size(400, 300);
            this.listViewElements.TabIndex = 0;
            this.listViewElements.UseCompatibleStateImageBehavior = false;
            this.listViewElements.View = System.Windows.Forms.View.Details;
            this.listViewElements.SelectedIndexChanged += new System.EventHandler(this.ListViewElements_SelectedIndexChanged);
            this.listViewElements.DoubleClick += new System.EventHandler(this.ListViewElements_DoubleClick);
            //
            // columnHeaderName
            //
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 200;
            //
            // columnHeaderSize
            //
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 100;
            //
            // buttonSaveSelection
            //
            this.buttonSaveSelection.Location = new System.Drawing.Point(12, 320);
            this.buttonSaveSelection.Name = "buttonSaveSelection";
            this.buttonSaveSelection.Size = new System.Drawing.Size(180, 30);
            this.buttonSaveSelection.TabIndex = 1;
            this.buttonSaveSelection.Text = "Save Selection to Library";
            this.buttonSaveSelection.UseVisualStyleBackColor = true;
            this.buttonSaveSelection.Click += new System.EventHandler(this.ButtonSaveSelection_Click);
            //
            // buttonPaste
            //
            this.buttonPaste.Location = new System.Drawing.Point(200, 320);
            this.buttonPaste.Name = "buttonPaste";
            this.buttonPaste.Size = new System.Drawing.Size(100, 30);
            this.buttonPaste.TabIndex = 2;
            this.buttonPaste.Text = "Paste Selected";
            this.buttonPaste.UseVisualStyleBackColor = true;
            this.buttonPaste.Click += new System.EventHandler(this.ButtonPaste_Click);
            //
            // buttonRename
            //
            this.buttonRename.Location = new System.Drawing.Point(12, 360);
            this.buttonRename.Name = "buttonRename";
            this.buttonRename.Size = new System.Drawing.Size(100, 30);
            this.buttonRename.TabIndex = 4;
            this.buttonRename.Text = "Rename";
            this.buttonRename.UseVisualStyleBackColor = true;
            this.buttonRename.Click += new System.EventHandler(this.ButtonRename_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.Location = new System.Drawing.Point(120, 360);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(100, 30);
            this.buttonDelete.TabIndex = 5;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            //
            // buttonClose
            //
            this.buttonClose.Location = new System.Drawing.Point(330, 360);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(82, 30);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            //
            // ElementLibraryDialog
            //
            this.ClientSize = new System.Drawing.Size(424, 402);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonRename);
            this.Controls.Add(this.buttonPaste);
            this.Controls.Add(this.buttonSaveSelection);
            this.Controls.Add(this.listViewElements);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementLibraryDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Element Library";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView listViewElements;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.Button buttonSaveSelection;
        private System.Windows.Forms.Button buttonPaste;
        private System.Windows.Forms.Button buttonRename;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonClose;
    }
}
