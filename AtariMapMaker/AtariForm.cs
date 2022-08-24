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
    public abstract partial class AtariForm : Form
    {
        protected AtariMap myMap;
        protected Bitmap dataImage;
        protected PictureBox myPictureBox;
        protected int zoom;
            
        /*
        protected AtariForm(AtariFontRenderer mainRenderer, AtariClipboard clipboard, AtariPalette palette, PictureBox outputPB, int zoom)
        {
            this.myRenderer = mainRenderer;
            this.clipboard = clipboard;
            this.palette = palette;
            this.myPictureBox = outputPB;
            this.zoom = zoom;
            InitializeComponent();
        }*/

        public PictureBox GetPictureBox()
        {
            return this.pictureBox1;
        }

        public void SetZoom(int zoom)
        {
            this.zoom = zoom;
            pictureBox1.Width = zoom * 128;
            pictureBox1.Height = zoom * 128;
            AtariForm_VisibleChanged(null, null);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            AtariPictureTools.PreviousMouseLocation = e.Location;
            AtariPictureTools.SelectionStart(e.Location);
        }

        private void AtariForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void AtariForm_Shown(object sender, EventArgs e)
        {
            pictureBox1.Image.Palette = AtariPalette.GetPalette();
            AtariFontRenderer.RedrawFont();
            AtariFontRenderer.RenderData(myMap, 0, dataImage);
        }

        public void RedrawFontWindow()
        {
            AtariForm_VisibleChanged(null, null);
        }

        private void AtariForm_VisibleChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            AtariPictureTools.Initialize((Bitmap)pictureBox1.Image, myMap, zoom);
            
            dataImage = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            AtariFontRenderer.RedrawFont();
            AtariFontRenderer.RenderData(myMap, 0, dataImage);
            pictureBox1.Image.Palette = AtariPalette.GetPalette();
            AtariPictureTools.Redraw(dataImage, true, false, true);
        }

    }
}
