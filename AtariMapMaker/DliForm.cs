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
    public partial class DliForm : Form
    {
        private readonly AtariMap dliMap;
        public const Globals.WindowType window = Globals.WindowType.Dli;
        private AtariColorPicker colorPicker;

        public DliForm(int lines)
        {

            dliMap = new AtariMap(new Size(1, 1), new Size(5, lines));
            byte[] dliFormFontData = new byte[1024];

            for (int a = 0; a < lines*5; a++)
                dliMap.Data[a] = (a % 5) == 3 ? (byte)0x82 : (byte)(a % 5);

            
            for (int i = 0; i < 8; i++)
            {
                dliFormFontData[i] = 0b01010101;
                dliFormFontData[i + 8] = 0b10101010;
                dliFormFontData[i + 16] = 0b11111111;
            }
            AtariFontRenderer.SetFontData(dliFormFontData, Globals.FontType.Dli);
            InitializeComponent();
            pictureBoxDli.Image = new Bitmap(5 * Globals.CharSize, lines * Globals.CharSize);
            this.Refresh();
            AtariPictureTools.AssignWindow(Globals.WindowType.Dli, (Bitmap)pictureBoxDli.Image, dliMap);
            colorPicker = new AtariColorPicker();
        }

        public void RenderData()
        {
            AtariPictureTools.Redraw(Globals.WindowType.Dli);
            pictureBoxDli.Refresh();
        }
        public void ZoomResize()
        {
            pictureBoxDli.Width = 5 * Globals.CharSize;
            pictureBoxDli.Height = AtariPictureTools.windows[window].map.ScreenSize.Height * Globals.CharSize;
            pictureBoxDli.Image = new Bitmap(pictureBoxDli.Width, pictureBoxDli.Height);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxDli.Image, dliMap);
            AtariPictureTools.Redraw(Globals.WindowType.Dli, true, false, true);
        }
        public void Initialize()
        {
       
            //AtariFontRenderer.FontData = dliFormFontData;
        }

        private void PictureBoxDli_MouseDown(object sender, MouseEventArgs e)
        {
            int xchar = e.X / Globals.CharSize;
            int ychar = e.Y / Globals.CharSize;
            colorPicker.Pick(dliMap.GetColorData(xchar + ychar*dliMap.Stride)[xchar]);
            dliMap.SetColor(0, 0, ychar, xchar, colorPicker.PickedColorIndex());
            AtariPictureTools.Redraw(window);
            pictureBoxDli.Refresh();
        }
    }
}
