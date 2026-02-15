namespace AtariMapMaker
{
    partial class ScreenLinkDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.labelLinkedScreenX = new System.Windows.Forms.Label();
            this.numericUpDownLinkedX = new System.Windows.Forms.NumericUpDown();
            this.labelLinkedScreenY = new System.Windows.Forms.Label();
            this.numericUpDownLinkedY = new System.Windows.Forms.NumericUpDown();
            this.labelTransparency = new System.Windows.Forms.Label();
            this.trackBarTransparency = new System.Windows.Forms.TrackBar();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonUnlink = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinkedX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinkedY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTransparency)).BeginInit();
            this.SuspendLayout();
            //
            // labelLinkedScreenX
            //
            this.labelLinkedScreenX.AutoSize = true;
            this.labelLinkedScreenX.Location = new System.Drawing.Point(12, 15);
            this.labelLinkedScreenX.Name = "labelLinkedScreenX";
            this.labelLinkedScreenX.Text = "Linked Screen X:";
            //
            // numericUpDownLinkedX
            //
            this.numericUpDownLinkedX.Location = new System.Drawing.Point(120, 12);
            this.numericUpDownLinkedX.Maximum = 99;
            this.numericUpDownLinkedX.Minimum = 0;
            this.numericUpDownLinkedX.Name = "numericUpDownLinkedX";
            this.numericUpDownLinkedX.Size = new System.Drawing.Size(100, 20);
            //
            // labelLinkedScreenY
            //
            this.labelLinkedScreenY.AutoSize = true;
            this.labelLinkedScreenY.Location = new System.Drawing.Point(12, 45);
            this.labelLinkedScreenY.Name = "labelLinkedScreenY";
            this.labelLinkedScreenY.Text = "Linked Screen Y:";
            //
            // numericUpDownLinkedY
            //
            this.numericUpDownLinkedY.Location = new System.Drawing.Point(120, 42);
            this.numericUpDownLinkedY.Maximum = 99;
            this.numericUpDownLinkedY.Minimum = 0;
            this.numericUpDownLinkedY.Name = "numericUpDownLinkedY";
            this.numericUpDownLinkedY.Size = new System.Drawing.Size(100, 20);
            //
            // labelTransparency
            //
            this.labelTransparency.AutoSize = true;
            this.labelTransparency.Location = new System.Drawing.Point(12, 75);
            this.labelTransparency.Name = "labelTransparency";
            this.labelTransparency.Text = "Transparency: 50%";
            //
            // trackBarTransparency
            //
            this.trackBarTransparency.Location = new System.Drawing.Point(120, 72);
            this.trackBarTransparency.Maximum = 100;
            this.trackBarTransparency.Minimum = 0;
            this.trackBarTransparency.Name = "trackBarTransparency";
            this.trackBarTransparency.Size = new System.Drawing.Size(200, 45);
            this.trackBarTransparency.TickFrequency = 10;
            this.trackBarTransparency.Value = 50;
            this.trackBarTransparency.Scroll += new System.EventHandler(this.TrackBarTransparency_Scroll);
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(12, 120);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            //
            // buttonUnlink
            //
            this.buttonUnlink.Location = new System.Drawing.Point(93, 120);
            this.buttonUnlink.Name = "buttonUnlink";
            this.buttonUnlink.Size = new System.Drawing.Size(75, 23);
            this.buttonUnlink.Text = "Unlink";
            this.buttonUnlink.UseVisualStyleBackColor = true;
            this.buttonUnlink.Click += new System.EventHandler(this.ButtonUnlink_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(174, 120);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // ScreenLinkDialog
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(324, 155);
            this.Controls.Add(this.labelLinkedScreenX);
            this.Controls.Add(this.numericUpDownLinkedX);
            this.Controls.Add(this.labelLinkedScreenY);
            this.Controls.Add(this.numericUpDownLinkedY);
            this.Controls.Add(this.labelTransparency);
            this.Controls.Add(this.trackBarTransparency);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonUnlink);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenLinkDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Link Screen";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinkedX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinkedY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTransparency)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelLinkedScreenX;
        private System.Windows.Forms.NumericUpDown numericUpDownLinkedX;
        private System.Windows.Forms.Label labelLinkedScreenY;
        private System.Windows.Forms.NumericUpDown numericUpDownLinkedY;
        private System.Windows.Forms.Label labelTransparency;
        private System.Windows.Forms.TrackBar trackBarTransparency;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonUnlink;
        private System.Windows.Forms.Button buttonCancel;
    }
}
