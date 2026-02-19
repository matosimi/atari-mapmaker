namespace AtariMapMaker
{
    partial class MetadataItemEditDialog
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
            this.labelX = new System.Windows.Forms.Label();
            this.numericX = new System.Windows.Forms.NumericUpDown();
            this.labelY = new System.Windows.Forms.Label();
            this.numericY = new System.Windows.Forms.NumericUpDown();
            this.labelType = new System.Windows.Forms.Label();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.labelValue = new System.Windows.Forms.Label();
            this.textBoxValueHex = new System.Windows.Forms.TextBox();
            this.labelColor = new System.Windows.Forms.Label();
            this.labelColorHex = new System.Windows.Forms.Label();
            this.buttonPickColor = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
            this.SuspendLayout();
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(12, 14);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(24, 20);
            this.labelX.TabIndex = 0;
            this.labelX.Text = "X:";
            // 
            // numericX
            // 
            this.numericX.Location = new System.Drawing.Point(60, 12);
            this.numericX.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericX.Name = "numericX";
            this.numericX.Size = new System.Drawing.Size(60, 26);
            this.numericX.TabIndex = 1;
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Location = new System.Drawing.Point(211, 14);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(24, 20);
            this.labelY.TabIndex = 2;
            this.labelY.Text = "Y:";
            // 
            // numericY
            // 
            this.numericY.Location = new System.Drawing.Point(241, 12);
            this.numericY.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericY.Name = "numericY";
            this.numericY.Size = new System.Drawing.Size(60, 26);
            this.numericY.TabIndex = 3;
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.Location = new System.Drawing.Point(12, 50);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(47, 20);
            this.labelType.TabIndex = 4;
            this.labelType.Text = "Type:";
            // 
            // comboBoxType
            // 
            this.comboBoxType.Location = new System.Drawing.Point(88, 47);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(303, 28);
            this.comboBoxType.TabIndex = 5;
            this.comboBoxType.SelectionChangeCommitted += new System.EventHandler(this.ComboBoxType_SelectionChangeCommitted);
            // 
            // labelValue
            // 
            this.labelValue.AutoSize = true;
            this.labelValue.Location = new System.Drawing.Point(12, 87);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(93, 20);
            this.labelValue.TabIndex = 6;
            this.labelValue.Text = "Value (hex):";
            // 
            // textBoxValueHex
            // 
            this.textBoxValueHex.Location = new System.Drawing.Point(151, 84);
            this.textBoxValueHex.MaxLength = 4;
            this.textBoxValueHex.Name = "textBoxValueHex";
            this.textBoxValueHex.Size = new System.Drawing.Size(69, 26);
            this.textBoxValueHex.TabIndex = 7;
            // 
            // labelColor
            // 
            this.labelColor.AutoSize = true;
            this.labelColor.Location = new System.Drawing.Point(12, 126);
            this.labelColor.Name = "labelColor";
            this.labelColor.Size = new System.Drawing.Size(50, 20);
            this.labelColor.TabIndex = 8;
            this.labelColor.Text = "Color:";
            // 
            // labelColorHex
            // 
            this.labelColorHex.AutoSize = true;
            this.labelColorHex.Location = new System.Drawing.Point(84, 126);
            this.labelColorHex.Name = "labelColorHex";
            this.labelColorHex.Size = new System.Drawing.Size(36, 20);
            this.labelColorHex.TabIndex = 9;
            this.labelColorHex.Text = "$00";
            // 
            // buttonPickColor
            // 
            this.buttonPickColor.Enabled = false;
            this.buttonPickColor.Location = new System.Drawing.Point(151, 116);
            this.buttonPickColor.Name = "buttonPickColor";
            this.buttonPickColor.Size = new System.Drawing.Size(89, 35);
            this.buttonPickColor.TabIndex = 10;
            this.buttonPickColor.Text = "Pick...";
            this.buttonPickColor.UseVisualStyleBackColor = true;
            this.buttonPickColor.Click += new System.EventHandler(this.ButtonPickColor_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(12, 172);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(93, 35);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(131, 172);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 35);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(241, 172);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(89, 35);
            this.buttonRemove.TabIndex = 13;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
            // 
            // MetadataItemEditDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(432, 219);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.numericX);
            this.Controls.Add(this.labelY);
            this.Controls.Add(this.numericY);
            this.Controls.Add(this.labelType);
            this.Controls.Add(this.comboBoxType);
            this.Controls.Add(this.labelValue);
            this.Controls.Add(this.textBoxValueHex);
            this.Controls.Add(this.labelColor);
            this.Controls.Add(this.labelColorHex);
            this.Controls.Add(this.buttonPickColor);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonRemove);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MetadataItemEditDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Metadata item";
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.NumericUpDown numericX;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.NumericUpDown numericY;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.TextBox textBoxValueHex;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.Label labelColorHex;
        private System.Windows.Forms.Button buttonPickColor;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRemove;
    }
}
