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
            this.pictureBoxDli = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDli)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxDli
            // 
            this.pictureBoxDli.Location = new System.Drawing.Point(1, 0);
            this.pictureBoxDli.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxDli.Name = "pictureBoxDli";
            this.pictureBoxDli.Size = new System.Drawing.Size(80, 320);
            this.pictureBoxDli.TabIndex = 0;
            this.pictureBoxDli.TabStop = false;
            this.pictureBoxDli.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDli_MouseDown);
            // 
            // DliForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(162, 336);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBoxDli);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DliForm";
            this.ShowInTaskbar = false;
            this.Text = "DliForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDli)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxDli;
    }
}