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
        private readonly AtariMap screenMap;
        public const Globals.WindowType window = Globals.WindowType.Dli;
        private readonly AtariColorPicker colorPicker;
        private int screenNumber;
        private readonly PictureBox pictureBoxScreen;
        private readonly byte[,] clipBoard;
        private Globals.ClipBoardEnum clipBoardDataType;
        private Point clickedChar;

        public DliForm(AtariMap screenMap, PictureBox pictureBoxScreen)
        {
            this.screenMap = screenMap;
            this.pictureBoxScreen = pictureBoxScreen;
            int lines = screenMap.ScreenSize.Height;
            clipBoard = new byte[lines, 5];
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

        public AtariMap DliMap { get { return dliMap; } }
        public void Show(int screenNumber)
        {
            this.screenNumber = screenNumber;
            this.Show();
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
            clickedChar = new Point(xchar, ychar);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    byte[] color5 = dliMap.GetColorData(xchar + ychar * dliMap.Stride);
                    colorPicker.Pick(color5[0] == Globals.DEFAULT_COLOR ? AtariFontRenderer.Color5[xchar] : color5[xchar]);
                    if (colorPicker.PickedNewColor)
                    {
                        if (color5[0] == Globals.DEFAULT_COLOR)
                            InitializeDliDataForSelectedScreen();   //initialize DLI data

                        dliMap.SetColor(0, 0, ychar, xchar, colorPicker.PickedColorIndex);
                        screenMap.SetColor(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, ychar, xchar, colorPicker.PickedColorIndex);
                        RedrawDliAndMap();
                    }
                    break;
                case MouseButtons.Right:
                    FillContextMenu();
                    contextMenuStripDli.Show(pictureBoxDli, e.X, e.Y);
                    break;
            }

        }
        /// <summary>
        /// Fills up DLI data of selected screen with current common colors (AtariFontRenderer.Color5)
        /// </summary>
        private void InitializeDliDataForSelectedScreen()
        {
            dliMap.SetColorData(0, 0, 0, -1, AtariFontRenderer.Color5);
            screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, 0, -1, AtariFontRenderer.Color5);
        }

        private void FillContextMenu()
        {
            fillDown5ToolStripMenuItem.DropDownItems.Clear();
            fillDown1ToolStripMenuItem.DropDownItems.Clear();
            for (int i = 1; i < dliMap.ScreenSize.Height - clickedChar.Y; i++)
            {
                ToolStripItem tsi = fillDown5ToolStripMenuItem.DropDownItems.Add(i.ToString());
                tsi.Click += Number_Click;
                tsi = fillDown1ToolStripMenuItem.DropDownItems.Add(i.ToString());
                tsi.Click += Number_Click;
            }
            paste1FromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.color;
            paste5FromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.color5;
            pasteAllFromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.colorAll;
        }

        private void Number_Click(object sender, EventArgs e)
        {
            var tsi = (ToolStripItem)sender;
            bool wholeLine = tsi.OwnerItem.OwnerItem.Name == colorsToolStripMenuItem.Name;
            int lines = int.Parse(tsi.Text) + 1;
            FillDown(wholeLine, lines);
            RedrawDliAndMap();
        }

        private void PickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBoxDli_MouseDown(sender, new MouseEventArgs(MouseButtons.Left, 1, clickedChar.X*Globals.CharSize, clickedChar.Y*Globals.CharSize, 0));
        }

        private void FillDown(bool wholeLine, int lines)
        {
            byte[] color5 = dliMap.GetColorData(clickedChar.X + 5*clickedChar.Y);
            int startingLine = clickedChar.Y;

            if (wholeLine)
            {
                dliMap.SetColorData(0, 0, startingLine, lines, color5);
                screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, startingLine, lines, color5);
            }
            else
            {
                dliMap.SetColorData(0, 0, startingLine, lines, clickedChar.X, color5[clickedChar.X]);
                screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, startingLine, lines, clickedChar.X, color5[clickedChar.X]);
            }
            RedrawDliAndMap();
        }

        private void RedrawDliAndMap()
        {
            AtariPictureTools.Redraw(window);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxDli.Refresh();
            pictureBoxScreen.Refresh();
        }

        private void CopyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetColorData(clickedChar);
            clipBoard[0, 0] = color5[clickedChar.X];
            clipBoardDataType = Globals.ClipBoardEnum.color;
        }

        private void Copy5ToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetColorData(clickedChar);
            for (int i = 0; i < color5.Length; i++)
                clipBoard[0, i] = color5[i];
            clipBoardDataType = Globals.ClipBoardEnum.color5;
        }

        private void CopyAllToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dliMap.ScreenSize.Height; i++)
            {
                byte[] color5 = dliMap.GetColorData(i * 5);
                for (int j = 0; j < 5; j++)
                    clipBoard[i, j] = color5[j];
            }
            clipBoardDataType = Globals.ClipBoardEnum.colorAll;
        }

        private void Paste1FromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetColorData(clickedChar);
            if (color5[0] == Globals.DEFAULT_COLOR)
                InitializeDliDataForSelectedScreen();   //initialize DLI data

            dliMap.SetColor(0, 0, clickedChar.Y, clickedChar.X, clipBoard[0, 0]);
            screenMap.SetColor(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, clickedChar.Y, clickedChar.X, clipBoard[0, 0]);
            RedrawDliAndMap();
        }

        private void Paste5FromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetColorData(clickedChar);
            if (color5[0] == Globals.DEFAULT_COLOR)
                InitializeDliDataForSelectedScreen();   //initialize DLI data

            for (int i = 0; i < 5; i++)
                color5[i] = clipBoard[0, i];
            dliMap.SetColorData(0, 0, clickedChar.Y, 1, color5);
            screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, clickedChar.Y, 1, color5);
            RedrawDliAndMap();
        }

        private void PasteAllFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = new byte[5];
            for (int j = 0; j < dliMap.ScreenSize.Height; j++)
            {
                for (int i = 0; i < 5; i++)
                    color5[i] = clipBoard[j, i];
                dliMap.SetColorData(0, 0, j, 1, color5);
                screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width, j, 1, color5);
            }
            RedrawDliAndMap();
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = new byte[5];
            color5[0] = Globals.DEFAULT_COLOR;
            dliMap.SetColorData(0,0,0,-1,color5);
            screenMap.SetColorData(screenNumber % screenMap.Screens.Width, screenNumber / screenMap.Screens.Width,0 , -1, color5);
            RedrawDliAndMap();
        }
    }
}
