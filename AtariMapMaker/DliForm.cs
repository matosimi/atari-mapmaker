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
        public DliForm(PictureBox outputPB, int lines)
        {
            this.myPictureBox = outputPB;
            this.myMap = new AtariMap(new Size(1, 1), new Size(5, lines));
            byte[] dliFormFontData = new byte[1024];

            for (int a = 0; a < lines*5; a++)
                myMap.Data[a] = (byte)(a % 5);

            for (int i = 0; i < 8; i++)
            {
                dliFormFontData[i] = 0b01010101;
                dliFormFontData[i + 8] = 0b10101010;
                dliFormFontData[i + 16] = 0b11111111;
            }
            InitializeComponent();
        }

        public void Initialize()
        {
       
            //AtariFontRenderer.FontData = dliFormFontData;
        }

    }
}
