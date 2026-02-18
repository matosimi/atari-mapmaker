namespace AtariMapMaker
{
    partial class TilePicker
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
            this.pictureBoxTilePicker = new System.Windows.Forms.PictureBox();
            this.comboBoxTilePickerLayout = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBoxSkipEmptyRows = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTilePicker)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxTilePicker
            // 
            this.pictureBoxTilePicker.Location = new System.Drawing.Point(3, 37);
            this.pictureBoxTilePicker.Name = "pictureBoxTilePicker";
            this.pictureBoxTilePicker.Size = new System.Drawing.Size(400, 320);
            this.pictureBoxTilePicker.TabIndex = 0;
            this.pictureBoxTilePicker.TabStop = false;
            this.pictureBoxTilePicker.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxTilePicker_Paint);
            this.pictureBoxTilePicker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxTilePicker_MouseDown);
            this.pictureBoxTilePicker.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxTilePicker_MouseMove);
            this.pictureBoxTilePicker.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxTilePicker_MouseUp);
            // 
            // comboBoxTilePickerLayout
            // 
            this.comboBoxTilePickerLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTilePickerLayout.FormattingEnabled = true;
            this.comboBoxTilePickerLayout.Items.AddRange(new object[] {
            "4 per row",
            "8 per row",
            "12 per row",
            "16 per row",
            "20 per row",
            "24 per row",
            "32 per row"});
            this.comboBoxTilePickerLayout.Location = new System.Drawing.Point(3, 3);
            this.comboBoxTilePickerLayout.Name = "comboBoxTilePickerLayout";
            this.comboBoxTilePickerLayout.Size = new System.Drawing.Size(171, 28);
            this.comboBoxTilePickerLayout.TabIndex = 4;
            this.comboBoxTilePickerLayout.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTilePickerLayout_SelectedIndexChanged);
            this.checkBoxSkipEmptyRows.CheckedChanged += new System.EventHandler(this.CheckBoxSkipEmptyRows_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.comboBoxTilePickerLayout);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxSkipEmptyRows);
            this.flowLayoutPanel1.Controls.Add(this.pictureBoxTilePicker);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(400, 400);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // checkBoxSkipEmptyRows
            // 
            this.checkBoxSkipEmptyRows.AutoSize = true;
            this.checkBoxSkipEmptyRows.Location = new System.Drawing.Point(180, 3);
            this.checkBoxSkipEmptyRows.Name = "checkBoxSkipEmptyRows";
            this.checkBoxSkipEmptyRows.Size = new System.Drawing.Size(150, 24);
            this.checkBoxSkipEmptyRows.TabIndex = 5;
            this.checkBoxSkipEmptyRows.Text = "Skip empty rows";
            this.checkBoxSkipEmptyRows.UseVisualStyleBackColor = true;
            // 
            // TilePicker
            // 
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "TilePicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tile Picker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TilePicker_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTilePicker)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxTilePicker;
        private System.Windows.Forms.ComboBox comboBoxTilePickerLayout;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxSkipEmptyRows;
    }
}
