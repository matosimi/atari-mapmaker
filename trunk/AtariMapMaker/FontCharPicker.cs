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
        private AtariPictureTools fontPickerPictureTools;
        private AtariFontRenderer myRenderer;
        private AtariMap myMap;
        private Bitmap dataImage;
        private AtariClipboard clipboard;
        private AtariPalette palette;
        private PictureBox myPictureBox;
        private int zoom;

        public FontCharPicker(AtariFontRenderer mainRenderer, AtariClipboard clipboard, AtariPalette palette, PictureBox outputPB, int zoom)
        {
            this.myRenderer = mainRenderer;
            this.clipboard = clipboard;
            this.palette = palette;
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

   
        public AtariFontRenderer GetRenderer()
        {
            return myRenderer;
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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            fontPickerPictureTools.PreviousMouseLocation = e.Location;
            fontPickerPictureTools.SelectionStart(e.Location);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                fontPickerPictureTools.SelectionChange(dataImage, e.Location);
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            clipboard.isValid = fontPickerPictureTools.SelectionEnd(clipboard, dataImage);
            myPictureBox.Image = clipboard.GetImage();
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
            pictureBox1.Image.Palette = palette.GetPalette();
            myRenderer.RedrawFont();
            myRenderer.RenderData(myMap, 0, dataImage);
        }

        public void RedrawFontWindow()
        {
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_VisibleChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            fontPickerPictureTools = new AtariPictureTools((Bitmap)pictureBox1.Image, myRenderer, myMap, zoom);
            //myRenderer = new AtariFontRenderer("default.fnt");
            
            
            dataImage = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            
            myRenderer.RedrawFont();
            myRenderer.RenderData(myMap, 0, dataImage);
            pictureBox1.Image.Palette = palette.GetPalette();
            fontPickerPictureTools.Redraw(dataImage, true, false, true);
        }

    }
}
