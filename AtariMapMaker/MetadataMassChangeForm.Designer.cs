namespace AtariMapMaker
{
    partial class MetadataMassChangeForm
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
            this.labelFilterBy = new System.Windows.Forms.Label();
            this.comboFilterBy = new System.Windows.Forms.ComboBox();
            this.labelFilterValue = new System.Windows.Forms.Label();
            this.textFilterValue = new System.Windows.Forms.TextBox();
            this.labelChange = new System.Windows.Forms.Label();
            this.comboChangeField = new System.Windows.Forms.ComboBox();
            this.labelNewValue = new System.Windows.Forms.Label();
            this.textNewValue = new System.Windows.Forms.TextBox();
            this.checkGlobal = new System.Windows.Forms.CheckBox();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelFilterBy
            //
            this.labelFilterBy.AutoSize = true;
            this.labelFilterBy.Location = new System.Drawing.Point(12, 12);
            this.labelFilterBy.Name = "labelFilterBy";
            this.labelFilterBy.Text = "Filter by:";
            //
            // comboFilterBy
            //
            this.comboFilterBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFilterBy.Location = new System.Drawing.Point(120, 10);
            this.comboFilterBy.Name = "comboFilterBy";
            this.comboFilterBy.Size = new System.Drawing.Size(100, 21);
            this.comboFilterBy.Items.AddRange(new object[] { "Text", "Value", "Color", "Type (byte)" });
            this.comboFilterBy.SelectedIndex = 0;
            this.comboFilterBy.SelectedIndexChanged += new System.EventHandler(this.UpdateCount);
            //
            // labelFilterValue
            //
            this.labelFilterValue.AutoSize = true;
            this.labelFilterValue.Location = new System.Drawing.Point(12, 40);
            this.labelFilterValue.Name = "labelFilterValue";
            this.labelFilterValue.Text = "Filter value:";
            //
            // textFilterValue
            //
            this.textFilterValue.Location = new System.Drawing.Point(120, 38);
            this.textFilterValue.Name = "textFilterValue";
            this.textFilterValue.Size = new System.Drawing.Size(180, 20);
            this.textFilterValue.TextChanged += new System.EventHandler(this.UpdateCount);
            //
            // labelChange
            //
            this.labelChange.AutoSize = true;
            this.labelChange.Location = new System.Drawing.Point(12, 68);
            this.labelChange.Name = "labelChange";
            this.labelChange.Text = "Change:";
            //
            // comboChangeField
            //
            this.comboChangeField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChangeField.Location = new System.Drawing.Point(120, 66);
            this.comboChangeField.Name = "comboChangeField";
            this.comboChangeField.Size = new System.Drawing.Size(100, 21);
            this.comboChangeField.Items.AddRange(new object[] { "Text", "Value", "Color", "Type (byte)" });
            this.comboChangeField.SelectedIndex = 0;
            //
            // labelNewValue
            //
            this.labelNewValue.AutoSize = true;
            this.labelNewValue.Location = new System.Drawing.Point(12, 96);
            this.labelNewValue.Name = "labelNewValue";
            this.labelNewValue.Text = "New value:";
            //
            // textNewValue
            //
            this.textNewValue.Location = new System.Drawing.Point(120, 94);
            this.textNewValue.Name = "textNewValue";
            this.textNewValue.Size = new System.Drawing.Size(180, 20);
            //
            // checkGlobal
            //
            this.checkGlobal.AutoSize = true;
            this.checkGlobal.Checked = true;
            this.checkGlobal.Location = new System.Drawing.Point(12, 144);
            this.checkGlobal.Name = "checkGlobal";
            this.checkGlobal.Text = "Global (all screens)";
            this.checkGlobal.CheckedChanged += new System.EventHandler(this.UpdateCount);
            //
            // labelCount
            //
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(12, 172);
            this.labelCount.Name = "labelCount";
            this.labelCount.Text = "0 items selected";
            //
            // buttonApply
            //
            this.buttonApply.Location = new System.Drawing.Point(12, 200);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(85, 28);
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.Location = new System.Drawing.Point(105, 200);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(85, 28);
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            //
            // MetadataMassChangeForm
            //
            this.ClientSize = new System.Drawing.Size(312, 268);
            this.Controls.Add(this.labelFilterBy);
            this.Controls.Add(this.comboFilterBy);
            this.Controls.Add(this.labelFilterValue);
            this.Controls.Add(this.textFilterValue);
            this.Controls.Add(this.labelChange);
            this.Controls.Add(this.comboChangeField);
            this.Controls.Add(this.labelNewValue);
            this.Controls.Add(this.textNewValue);
            this.Controls.Add(this.checkGlobal);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonDelete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MetadataMassChangeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mass change metadata";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFilterBy;
        private System.Windows.Forms.ComboBox comboFilterBy;
        private System.Windows.Forms.Label labelFilterValue;
        private System.Windows.Forms.TextBox textFilterValue;
        private System.Windows.Forms.Label labelChange;
        private System.Windows.Forms.ComboBox comboChangeField;
        private System.Windows.Forms.Label labelNewValue;
        private System.Windows.Forms.TextBox textNewValue;
        private System.Windows.Forms.CheckBox checkGlobal;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonDelete;
    }
}
