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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFontPicker)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBoxFontPicker.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxFontPicker.Name = "pictureBox1";
            this.pictureBoxFontPicker.Size = new System.Drawing.Size(256, 256);
            this.pictureBoxFontPicker.TabIndex = 0;
            this.pictureBoxFontPicker.TabStop = false;
            this.pictureBoxFontPicker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseDown);
            this.pictureBoxFontPicker.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseMove);
            this.pictureBoxFontPicker.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFontPicker_MouseUp);
            // 
            // FontCharPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(265, 261);
            this.Controls.Add(this.pictureBoxFontPicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxFontPicker;
    }
}