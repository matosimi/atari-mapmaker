namespace AtariMapMaker
{
    partial class MetadataExportForm
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
            this.labelGroupBy = new System.Windows.Forms.Label();
            this.comboGroupBy = new System.Windows.Forms.ComboBox();
            this.labelCoordOrder = new System.Windows.Forms.Label();
            this.comboCoordOrder = new System.Windows.Forms.ComboBox();
            this.checkHeader = new System.Windows.Forms.CheckBox();
            this.labelExportOutput = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelGroupBy
            //
            this.labelGroupBy.AutoSize = true;
            this.labelGroupBy.Location = new System.Drawing.Point(12, 12);
            this.labelGroupBy.Name = "labelGroupBy";
            this.labelGroupBy.Text = "Group by:";
            //
            // comboGroupBy
            //
            this.comboGroupBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboGroupBy.Location = new System.Drawing.Point(100, 10);
            this.comboGroupBy.Name = "comboGroupBy";
            this.comboGroupBy.Size = new System.Drawing.Size(180, 21);
            this.comboGroupBy.Items.AddRange(new object[] { "None", "Color", "Value", "Text" });
            this.comboGroupBy.SelectedIndex = 0;
            this.comboGroupBy.SelectedIndexChanged += new System.EventHandler(this.RegenerateOutput);
            //
            // labelCoordOrder
            //
            this.labelCoordOrder.AutoSize = true;
            this.labelCoordOrder.Location = new System.Drawing.Point(12, 40);
            this.labelCoordOrder.Name = "labelCoordOrder";
            this.labelCoordOrder.Text = "Coordinate / order:";
            //
            // comboCoordOrder
            //
            this.comboCoordOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCoordOrder.Location = new System.Drawing.Point(140, 38);
            this.comboCoordOrder.Name = "comboCoordOrder";
            this.comboCoordOrder.Size = new System.Drawing.Size(200, 21);
            this.comboCoordOrder.Items.AddRange(new object[] { "x,y (order Y then X)", "x,y (order X then Y)", "index dta a($xxxx)" });
            this.comboCoordOrder.SelectedIndex = 0;
            this.comboCoordOrder.SelectedIndexChanged += new System.EventHandler(this.RegenerateOutput);
            //
            // checkHeader
            //
            this.checkHeader.Location = new System.Drawing.Point(12, 68);
            this.checkHeader.Name = "checkHeader";
            this.checkHeader.Text = "Include header line";
            this.checkHeader.CheckedChanged += new System.EventHandler(this.RegenerateOutput);
            //
            // labelExportOutput
            //
            this.labelExportOutput.AutoSize = true;
            this.labelExportOutput.Location = new System.Drawing.Point(12, 96);
            this.labelExportOutput.Name = "labelExportOutput";
            this.labelExportOutput.Text = "Export output:";
            //
            // textBoxOutput
            //
            this.textBoxOutput.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutput.Location = new System.Drawing.Point(12, 118);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOutput.Size = new System.Drawing.Size(520, 320);
            //
            // buttonCopy
            //
            this.buttonCopy.Location = new System.Drawing.Point(12, 444);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(120, 28);
            this.buttonCopy.Text = "Copy to clipboard";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.ButtonCopy_Click);
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(272, 444);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(120, 28);
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            //
            // MetadataExportForm
            //
            this.AcceptButton = this.buttonClose;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(544, 484);
            this.Controls.Add(this.labelGroupBy);
            this.Controls.Add(this.comboGroupBy);
            this.Controls.Add(this.labelCoordOrder);
            this.Controls.Add(this.comboCoordOrder);
            this.Controls.Add(this.checkHeader);
            this.Controls.Add(this.labelExportOutput);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.buttonClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MetadataExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export metadata";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelGroupBy;
        private System.Windows.Forms.ComboBox comboGroupBy;
        private System.Windows.Forms.Label labelCoordOrder;
        private System.Windows.Forms.ComboBox comboCoordOrder;
        private System.Windows.Forms.CheckBox checkHeader;
        private System.Windows.Forms.Label labelExportOutput;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonClose;
    }
}
