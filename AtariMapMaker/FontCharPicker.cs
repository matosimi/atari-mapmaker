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
        private readonly AtariMap fontPickerMap;
        private Bitmap dataImage;
        private readonly PictureBox myPictureBox;
    
        public FontCharPicker(PictureBox outputPB)
        {
            this.myPictureBox = outputPB;
            this.fontPickerMap = new AtariMap(new Size(1, 1), new Size(16, 16));
            for (int a = 0; a < 256; a++)
                fontPickerMap.Data[a] = (byte)a;

            
            InitializeComponent();
        }

        public PictureBox GetPictureBox()
        {
            return this.pictureBoxFontPicker;
        }

        public void SetZoom()
        {
            pictureBoxFontPicker.Width = Globals.Zoom * 128;
            pictureBoxFontPicker.Height = Globals.Zoom * 128;
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_Load(object sender, EventArgs e)
        {
 
        }

        private void PictureBoxFontPicker_MouseDown(object sender, MouseEventArgs e)
        {
            //AtariPictureTools.AssignWindow((Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
            AtariPictureTools.PreviousMouseLocation = e.Location;
            AtariPictureTools.SelectionStart(e.Location, Globals.WindowType.CharPicker);
        }

        private void PictureBoxFontPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.CharPicker);
                pictureBoxFontPicker.Invalidate();
            }

            int xx = AtariFontRenderer.OffsetX + e.X / Globals.CharSize;
            int yy = AtariFontRenderer.OffsetY + e.Y / Globals.CharSize;
            if (xx < fontPickerMap.Stride && yy < fontPickerMap.Screens.Height * fontPickerMap.ScreenSize.Height)
            {
                byte charVal = fontPickerMap.Data[xx + yy * fontPickerMap.Stride];
                this.Text = "FontCharPicker - Char: $" + String.Format("{0:X2}", charVal) + " (" + charVal + ")";
            }
        }

        private void PictureBoxFontPicker_MouseUp(object sender, MouseEventArgs e)
        {
            AtariClipboard.IsValid = AtariPictureTools.SelectionEnd(Globals.WindowType.CharPicker);
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
            /*
            pictureBoxFontPicker.Image.Palette = AtariPalette.GetPalette();
            AtariFontRenderer.SelectFont(Globals.FontType.Screen);
            AtariFontRenderer.RedrawFont();
            AtariFontRenderer.RenderMapData(fontPickerMap, 0, dataImage);*/
        }

        public void RedrawFontWindow()
        {
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_VisibleChanged(object sender, EventArgs e)
        {
            pictureBoxFontPicker.Image = new Bitmap(pictureBoxFontPicker.Width, pictureBoxFontPicker.Height);

            //AtariPictureTools.AssignWindow((Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
            //myRenderer = new AtariFontRenderer("default.fnt");
            
            
            dataImage = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //AtariFontRenderer.RedrawFont();
            //AtariFontRenderer.RenderMapData(fontPickerMap, 0, dataImage);
            //pictureBoxFontPicker.Image.Palette = AtariPalette.GetPalette();
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker, true, false, true);
        }

    }
}
