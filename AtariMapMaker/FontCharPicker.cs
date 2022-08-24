using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class FontCharPicker : Form
    {
        private readonly AtariMap myMap;
        private Bitmap dataImage;
        private readonly PictureBox myPictureBox;
        private int zoom;

        public FontCharPicker(PictureBox outputPB, int zoom)
        {
            this.myPictureBox = outputPB;
            this.zoom = zoom;
            this.myMap = new AtariMap(new Size(1, 1), new Size(16, 16));
            for (int a = 0; a < 256; a++)
                myMap.Data[a] = (byte)a;

            
            InitializeComponent();
        }

        public PictureBox GetPictureBox()
        {
            return this.pictureBox1;
        }

        public void SetZoom(int zoom)
        {
            this.zoom = zoom;
            pictureBox1.Width = zoom * 128;
            pictureBox1.Height = zoom * 128;
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_Load(object sender, EventArgs e)
        {
 
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            AtariPictureTools.PreviousMouseLocation = e.Location;
            AtariPictureTools.SelectionStart(e.Location);
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AtariPictureTools.SelectionChange(dataImage, e.Location);
                pictureBox1.Invalidate();
            }

            int xx = (AtariFontRenderer.OffsetX + e.X / (zoom * 8));
            int yy = (AtariFontRenderer.OffsetY + e.Y / (zoom * 8));
            if (xx < myMap.Stride && yy < myMap.Screens.Height * myMap.ScreenSize.Height)
            {
                byte charVal = myMap.Data[xx + yy * myMap.Stride];
                this.Text = "FontCharPicker - Char: $" + String.Format("{0:X2}", charVal) + " (" + charVal + ")";
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            AtariClipboard.IsValid = AtariPictureTools.SelectionEnd(dataImage);
            myPictureBox.Image = AtariClipboard.GetImage();
        }

        private void FontCharPicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void FontCharPicker_Shown(object sender, EventArgs e)
        {
            pictureBox1.Image.Palette = AtariPalette.GetPalette();
            AtariFontRenderer.RedrawFont();
            AtariFontRenderer.RenderData(myMap, 0, dataImage);
        }

        public void RedrawFontWindow()
        {
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_VisibleChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            AtariPictureTools.Initialize((Bitmap)pictureBox1.Image, myMap, zoom);
            //myRenderer = new AtariFontRenderer("default.fnt");
            
            
            dataImage = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            AtariFontRenderer.RedrawFont();
            AtariFontRenderer.RenderData(myMap, 0, dataImage);
            pictureBox1.Image.Palette = AtariPalette.GetPalette();
            AtariPictureTools.Redraw(dataImage, true, false, true);
        }

    }
}
