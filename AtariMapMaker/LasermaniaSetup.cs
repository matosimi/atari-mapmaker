using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class LasermaniaSetup : Form
    {
        public LasermaniaSetup()
        {
            InitializeComponent();
            button1.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            buttonAltirraBrowse.Click += ButtonAltirraBrowse_Click;
        }

        public string EmulatorPath { get => textBox1?.Text ?? ""; set => textBox1.Text = value ?? ""; }
        public string AdditionalOptions { get => textBox2?.Text ?? ""; set => textBox2.Text = value ?? ""; }

        private void ButtonAltirraBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select emulator (Altirra)";
                dlg.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*";
                if (!string.IsNullOrEmpty(textBox1.Text))
                    dlg.InitialDirectory = Path.GetDirectoryName(textBox1.Text);
                if (dlg.ShowDialog() == DialogResult.OK)
                    textBox1.Text = dlg.FileName;
            }
        }
    }
}
