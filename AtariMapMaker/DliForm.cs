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
        public DliForm(int lines)
        {

            this.window = Globals.WindowType.Dli;
            AtariMap myMap = new AtariMap(new Size(1, 1), new Size(5, lines));
            byte[] dliFormFontData = new byte[1024];

            for (int a = 0; a < lines*5; a++)
                myMap.Data[a] = (a % 5) == 3 ? (byte)0x82 : (byte)(a % 5);

            
            for (int i = 0; i < 8; i++)
            {
                dliFormFontData[i] = 0b01010101;
                dliFormFontData[i + 8] = 0b10101010;
                dliFormFontData[i + 16] = 0b11111111;
            }
            AtariFontRenderer.SetFontData(dliFormFontData, Globals.FontType.Dli);
            AtariPictureTools.AssignWindow(Globals.WindowType.Dli, (Bitmap)pictureBox1.Image, myMap);
            InitializeComponent();
        }

        public void RenderData()
        {
            /*
            Bitmap dliBmp = new Bitmap(5 * 8, myMap.ScreenSize.Height * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            AtariFontRenderer.SelectFont(Globals.FontType.Dli);
            AtariFontRenderer.RenderMapData(myMap, 0, dliBmp);
            pictureBox1.Width = dliBmp.Width * Globals.Zoom;
            pictureBox1.Height = dliBmp.Height * Globals.Zoom;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            AtariPictureTools.AssignWindow((Bitmap)pictureBox1.Image, myMap);
            AtariPictureTools.Redraw(dliBmp, true, false, true);*/
            AtariPictureTools.Redraw(Globals.WindowType.Dli);
        }
        public void ZoomResize()
        {
            this.Width = 5 * Globals.CharSize;
            this.Height = AtariPictureTools.windows[window].map.ScreenSize.Height * Globals.CharSize;
        }
        public void Initialize()
        {
       
            //AtariFontRenderer.FontData = dliFormFontData;
        }

    }
}
