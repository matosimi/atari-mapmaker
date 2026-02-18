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
        private AtariMap fontPickerMap;
        private readonly PictureBox clipboardPictureBox;
        private AtariMap mainMap;
        public const Globals.WindowType window = Globals.WindowType.CharPicker;

        private static int savedLayoutIndex = 0;

        public FontCharPicker(PictureBox clipboardPictureBox, AtariMap mainMap = null)
        {
            this.clipboardPictureBox = clipboardPictureBox;
            this.mainMap = mainMap;
            InitializeComponent();
            fontPickerMap = CreatePickerMap(16, 16);
            pictureBoxFontPicker.Image = new Bitmap(16 * Globals.CharSize, 16 * Globals.CharSize);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
        }

        public void SetMainMap(AtariMap map)
        {
            mainMap = map;
            RefreshFontCombo();
        }

        private static AtariMap CreatePickerMap(int cols, int rows)
        {
            var map = new AtariMap(new Size(1, 1), new Size(cols, rows));
            for (int a = 0; a < 256; a++)
                map.Data[a] = (byte)a;
            return map;
        }

        private (int cols, int rows) GetLayoutDimensions()
        {
            int idx = comboBoxFontPickerLayout.SelectedIndex;
            if (idx == 1) return (32, 8);
            if (idx == 2) return (8, 32);
            return (16, 16);
        }

        private void ApplyLayout()
        {
            var (cols, rows) = GetLayoutDimensions();
            fontPickerMap = CreatePickerMap(cols, rows);
            int w = cols * Globals.CharSize;
            int h = rows * Globals.CharSize;
            pictureBoxFontPicker.Width = w;
            pictureBoxFontPicker.Height = h;
            pictureBoxFontPicker.Image = new Bitmap(w, h);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker, true, false, true);
        }

        public PictureBox GetPictureBox()
        {
            return this.pictureBoxFontPicker;
        }

        public void SetZoom()
        {
            var (cols, rows) = GetLayoutDimensions();
            pictureBoxFontPicker.Width = cols * Globals.CharSize;
            pictureBoxFontPicker.Height = rows * Globals.CharSize;
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_Load(object sender, EventArgs e)
        {
            if (comboBoxFontPickerLayout.SelectedIndex != savedLayoutIndex)
                comboBoxFontPickerLayout.SelectedIndex = savedLayoutIndex;
            else
                ApplyLayout();
            RefreshFontCombo();
        }

        private void RefreshFontCombo()
        {
            comboBoxFontToPick.Items.Clear();
            if (mainMap != null && mainMap.MultiFontEnabled && mainMap.FontDataArray != null && mainMap.FontDataArray.Length > 0)
            {
                for (int i = 0; i < mainMap.FontDataArray.Length; i++)
                    comboBoxFontToPick.Items.Add("Font " + i);
                comboBoxFontToPick.Enabled = true;
                if (comboBoxFontToPick.SelectedIndex < 0)
                    comboBoxFontToPick.SelectedIndex = 0;
                ComboBoxFontToPick_SelectedIndexChanged(null, null);
            }
            else
            {
                comboBoxFontToPick.Items.Add("Font 0");
                comboBoxFontToPick.SelectedIndex = 0;
                comboBoxFontToPick.Enabled = false;
                AtariFontRenderer.CharPickerFontIndex = null;
                AtariFontRenderer.CharPickerFontSourceMap = null;
            }
        }

        private void ComboBoxFontPickerLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFontPickerLayout.SelectedIndex < 0) return;
            savedLayoutIndex = comboBoxFontPickerLayout.SelectedIndex;
            ApplyLayout();
        }

        private void ComboBoxFontToPick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFontToPick.SelectedIndex < 0 || mainMap == null) return;
            int idx = comboBoxFontToPick.SelectedIndex;
            AtariFontRenderer.CharPickerFontIndex = idx;
            AtariFontRenderer.CharPickerFontSourceMap = mainMap;
            RedrawFontWindow();
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
                AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
                AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.CharPicker);
                pictureBoxFontPicker.Refresh();
            }

            int xx = fontPickerMap.OffsetX + e.X / Globals.CharSize;
            int yy = fontPickerMap.OffsetY + e.Y / Globals.CharSize;
            if (xx < fontPickerMap.Stride && yy < fontPickerMap.MapSize.Height * fontPickerMap.ScreenSize.Height && xx >= 0 && yy >= 0)
            {
                byte charVal = fontPickerMap.Data[xx + yy * fontPickerMap.Stride];
                this.Text = "FontCharPicker - Char: $" + String.Format("{0:X2}", charVal) + " (" + charVal + ")";
            }
        }

        private void PictureBoxFontPicker_MouseUp(object sender, MouseEventArgs e)
        {
            AtariClipboard.IsValid = AtariPictureTools.SelectionEnd(Globals.WindowType.CharPicker);
            clipboardPictureBox.Image = AtariClipboard.ClipboardImage;
        }

        private void FontCharPicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide instead of close so the form is never disposed (only when app exits)
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
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
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);

            //AtariPictureTools.AssignWindow((Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
            //myRenderer = new AtariFontRenderer("default.fnt");


            //dataImage = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //AtariFontRenderer.RedrawFont();
            //AtariFontRenderer.RenderMapData(fontPickerMap, 0, dataImage);
            //pictureBoxFontPicker.Image.Palette = AtariPalette.GetPalette();
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker, true, false, true);
        }

    }
}
