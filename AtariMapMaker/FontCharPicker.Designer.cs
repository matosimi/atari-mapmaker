namespace AtariMapMaker
{
    partial class FontCharPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxFontPicker = new System.Windows.Forms.PictureBox();
            this.comboBoxFontToPick = new System.Windows.Forms.ComboBox();
            this.comboBoxFontPickerLayout = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanelSelection = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFontPicker)).BeginInit();
            this.flowLayoutPanelSelection.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxFontPicker
            // 
            this.pictureBoxFontPicker.Location = new System.Drawing.Point(4, 39);
            this.pictureBoxFontPicker.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxFontPicker.Name = "pictureBoxFontPicker";
            this.pictureBoxFontPicker.Size = new System.Drawing.Size(384, 394);
            this.pictureBoxFontPicker.TabIndex = 0;
            this.pictureBoxFontPicker.TabStop = false;
            this.pictureBoxFontPicker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseDown);
            this.pictureBoxFontPicker.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseMove);
            this.pictureBoxFontPicker.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseUp);
            // 
            // comboBoxFontToPick
            // 
            this.comboBoxFontToPick.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontToPick.FormattingEnabled = true;
            this.comboBoxFontToPick.Location = new System.Drawing.Point(3, 3);
            this.comboBoxFontToPick.Name = "comboBoxFontToPick";
            this.comboBoxFontToPick.Size = new System.Drawing.Size(189, 28);
            this.comboBoxFontToPick.TabIndex = 2;
            this.comboBoxFontToPick.SelectedIndexChanged += new System.EventHandler(this.ComboBoxFontToPick_SelectedIndexChanged);
            // 
            // comboBoxFontPickerLayout
            // 
            this.comboBoxFontPickerLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontPickerLayout.FormattingEnabled = true;
            this.comboBoxFontPickerLayout.Items.AddRange(new object[] { "16x16", "32x8", "8x32" });
            this.comboBoxFontPickerLayout.Location = new System.Drawing.Point(198, 3);
            this.comboBoxFontPickerLayout.Name = "comboBoxFontPickerLayout";
            this.comboBoxFontPickerLayout.Size = new System.Drawing.Size(171, 28);
            this.comboBoxFontPickerLayout.TabIndex = 3;
            this.comboBoxFontPickerLayout.SelectedIndexChanged += new System.EventHandler(this.ComboBoxFontPickerLayout_SelectedIndexChanged);
            // 
            // flowLayoutPanelSelection
            // 
            this.flowLayoutPanelSelection.Controls.Add(this.comboBoxFontToPick);
            this.flowLayoutPanelSelection.Controls.Add(this.comboBoxFontPickerLayout);
            this.flowLayoutPanelSelection.Controls.Add(this.pictureBoxFontPicker);
            this.flowLayoutPanelSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelSelection.Name = "flowLayoutPanelSelection";
            this.flowLayoutPanelSelection.Size = new System.Drawing.Size(466, 468);
            this.flowLayoutPanelSelection.TabIndex = 2;
            // 
            // FontCharPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = false;
            this.ClientSize = new System.Drawing.Size(466, 468);
            this.Controls.Add(this.flowLayoutPanelSelection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(200, 150);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FontCharPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FontCharPicker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontCharPicker_FormClosing);
            this.Load += new System.EventHandler(this.FontCharPicker_Load);
            this.Shown += new System.EventHandler(this.FontCharPicker_Shown);
            this.VisibleChanged += new System.EventHandler(this.FontCharPicker_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFontPicker)).EndInit();
            this.flowLayoutPanelSelection.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxFontPicker;
        private System.Windows.Forms.ComboBox comboBoxFontPickerLayout;
        private System.Windows.Forms.ComboBox comboBoxFontToPick;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSelection;
    }
}