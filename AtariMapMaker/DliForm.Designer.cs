namespace AtariMapMaker
{
    partial class DliForm
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
            this.pictureBoxColors = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColors)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxColors
            // 
            this.pictureBoxColors.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxColors.Name = "pictureBoxColors";
            this.pictureBoxColors.Size = new System.Drawing.Size(125, 342);
            this.pictureBoxColors.TabIndex = 0;
            this.pictureBoxColors.TabStop = false;
            // 
            // DliForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(594, 450);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBoxColors);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DliForm";
            this.ShowInTaskbar = false;
            this.Text = "DliForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxColors;
    }
}