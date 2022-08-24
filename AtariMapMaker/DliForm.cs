using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class DliForm : AtariForm
    {
        public DliForm(PictureBox outputPB, int zoom, int lines)
        {
            this.myPictureBox = outputPB;
            this.zoom = zoom;
            this.myMap = new AtariMap(new Size(1, 1), new Size(5, lines));
            for (int a = 0; a < lines*5; a++)
                myMap.Data[a] = (byte)(a % 5);
            InitializeComponent();
        }

    }
}
