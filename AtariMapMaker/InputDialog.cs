using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class InputDialog : Form
    {
        private TextBox textBoxInput;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelPrompt;

        public string InputText { get; private set; }

        public InputDialog(string prompt, string title, string defaultValue = "")
        {
            InitializeComponent(prompt, title, defaultValue);
        }

        private void InitializeComponent(string prompt, string title, string defaultValue)
        {
            this.textBoxInput = new TextBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelPrompt = new Label();
            this.SuspendLayout();

            // labelPrompt
            this.labelPrompt.AutoSize = true;
            this.labelPrompt.Location = new Point(12, 15);
            this.labelPrompt.Text = prompt;

            // textBoxInput
            this.textBoxInput.Location = new Point(12, 35);
            this.textBoxInput.Size = new Size(300, 20);
            this.textBoxInput.Text = defaultValue;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(156, 65);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(237, 65);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // InputDialog
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(324, 100);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.labelPrompt);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = title;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            InputText = textBoxInput.Text;
        }
    }
}
