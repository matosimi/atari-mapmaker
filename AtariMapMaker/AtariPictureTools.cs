using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;

namespace AtariMapMaker
{
    public struct AtariWindow
    {
        public AtariMap map;
        public Bitmap fontRendererFontImage;
        public Bitmap fontRendererMapImage;
        public Graphics pictureBoxGraphics;
        public Globals.FontType fontType;
    }
    public static class AtariPictureTools
    {
        //private static AtariMap myMap;
        //private static Graphics gr;
        private static Rectangle mouseSelection = new Rectangle();
        //private static Bitmap destImage;
        private static Point prevMouseLoc;
        private static int previousOffset;
        private static readonly Pen screenSeparatorPen = new Pen(Color.Red);
        private static readonly Pen selectionPen = new Pen(Color.Lime);
        private static readonly Color gridColor = Color.White;
        private static bool drawScreenBorders = true;
        private static bool drawGrid = true;
        private static Point dliFormOrigin;
        public static Dictionary<Globals.WindowType, AtariWindow> windows = new Dictionary<Globals.WindowType, AtariWindow>();

        public static void AssignWindow(Globals.WindowType windowType, Bitmap destinationPictureBoxImage, AtariMap windowMap)
        {
            Globals.FontType myFontType = windowType == Globals.WindowType.Dli ? Globals.FontType.Dli : Globals.FontType.Screen;
            //int myMapOffset = 0;

            if (windows.ContainsKey(windowType))
            {
                //myMapOffset = windows[windowType].mapOffset;
                windows.Remove(windowType);
            }

            int emptyWidth = destinationPictureBoxImage.Width % (8 * Globals.Zoom);
            int emptyHeight = destinationPictureBoxImage.Height % (8 * Globals.Zoom);
            AtariWindow window = new AtariWindow()
            {
                
                fontRendererMapImage = new Bitmap((destinationPictureBoxImage.Width - emptyWidth) / Globals.Zoom, (destinationPictureBoxImage.Height - emptyHeight) / Globals.Zoom, System.Drawing.Imaging.PixelFormat.Format8bppIndexed),
                pictureBoxGraphics = Graphics.FromImage(destinationPictureBoxImage),
                map = windowMap,
                fontType = myFontType,
                fontRendererFontImage = AtariFontRenderer.fonts[myFontType].bitmap
            };
            window.fontRendererMapImage.Palette = AtariPalette.GetPalette();
            window.pictureBoxGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            windows.Add(windowType, window);
        }
       
        public static void SetGridVisibility(bool _drawScreenBorders, bool _drawGrid)
        {
            drawGrid = _drawGrid;
            drawScreenBorders = _drawScreenBorders;
        }
 
        public static void SelectionStart(Point firstCorner, Globals.WindowType window)
        {
            mouseSelection.X = firstCorner.X - (firstCorner.X % Globals.CharSize);
            mouseSelection.Y = firstCorner.Y - (firstCorner.Y % Globals.CharSize);
            mouseSelection.Width = Globals.CharSize;
            mouseSelection.Height = Globals.CharSize;
            DrawSelection(window);
        }

        public static void SelectionChange(Point newCorner, Globals.WindowType window)
        {
            int mx = newCorner.X - (newCorner.X % Globals.CharSize);
            int my = newCorner.Y - (newCorner.Y % Globals.CharSize);
            if (Math.Abs(mx - mouseSelection.X) != mouseSelection.Width ||
                Math.Abs(my - mouseSelection.Y) != mouseSelection.Height)
            {

                mouseSelection.Width = Globals.CharSize + mx - mouseSelection.X;
                mouseSelection.Height = Globals.CharSize + my - mouseSelection.Y;
                if (mx - mouseSelection.X < 0)
                {
                    mouseSelection.Width -= Globals.CharSize;
                }
                if (my - mouseSelection.Y < 0)
                {
                    mouseSelection.Height -= Globals.CharSize;
                }

                //selection out of picturebox bounds - do not copy, do not draw selection
                if (mouseSelection.Width + mouseSelection.X > windows[window].fontRendererMapImage.Width * Globals.Zoom ||
                    mouseSelection.Height + mouseSelection.Y > windows[window].fontRendererMapImage.Height * Globals.Zoom ||
                    newCorner.X < 0 ||
                    newCorner.Y < 0)
                {
                    mouseSelection.Width = 0;
                    mouseSelection.Height = 0;
                }

                //selection out of data bounds - do not copy, do not draw selection
                if (mouseSelection.X / Globals.CharSize + windows[window].map.OffsetX > windows[window].map.Stride ||
                    mouseSelection.Y / Globals.CharSize + windows[window].map.OffsetY > windows[window].map.MapSize.Height * windows[window].map.ScreenSize.Height ||
                    (mouseSelection.X + mouseSelection.Width) / Globals.CharSize + windows[window].map.OffsetX > windows[window].map.Stride ||
                    (mouseSelection.Y + mouseSelection.Height)/ Globals.CharSize + windows[window].map.OffsetY > windows[window].map.MapSize.Height * windows[window].map.ScreenSize.Height)
                {
                    mouseSelection.Width = 0;
                    mouseSelection.Height = 0;
                }

                // gr.DrawImage(dataImage, 0, 0, dataImage.Width * zoom, dataImage.Height * zoom);
                Redraw(window);

                DrawSelection(window);
            }
        }

        public static bool SelectionEnd(Globals.WindowType window)
        {
            if (mouseSelection.Width == 0 || mouseSelection.Height == 0)
                return false;
            if (AtariClipboard.UnderClipBoardImage != null)
                AtariClipboard.UnderClipBoardImage.Dispose();
            if (AtariClipboard.UnderImageGraphics != null)
                AtariClipboard.UnderImageGraphics.Dispose();


            if (mouseSelection.Width < 0)
            {
                mouseSelection.X += mouseSelection.Width;
                mouseSelection.Width = -mouseSelection.Width;
            }
            if (mouseSelection.Height < 0)
            {
                mouseSelection.Y += mouseSelection.Height;
                mouseSelection.Height = -mouseSelection.Height;
            }

            AtariClipboard.SetDataSource(windows[window].map);
            //Redraw(window, true, false, false);
            AtariClipboard.Copy(windows[window].fontRendererMapImage, mouseSelection, windows[window].map.Offset);
            
            //Redraw(window, false, true, true);
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.ClipboardImage);
            AtariClipboard.UnderImageGraphics = Graphics.FromImage(AtariClipboard.UnderClipBoardImage);
            return true;
        }

        public static Point PreviousMouseLocation
        {
            get
            {
                return prevMouseLoc;
            }
            set
            {
                prevMouseLoc = value;
            }
        }

        public static int PreviousOffset
        {
            set { previousOffset = value; }
        }

        private static void DrawSelection(Globals.WindowType window)
        {
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X + mouseSelection.Width, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y + mouseSelection.Height, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X, mouseSelection.Y + mouseSelection.Height);
        }

        public static void Redraw(Globals.WindowType windowType)
        {
            Redraw(windowType, true, drawScreenBorders, drawGrid);
        }

        public static Point GetDliFormOrigin()
        {
            return dliFormOrigin;
        }

        public static void Redraw(Globals.WindowType window, bool drawData, bool drawScreenBorders, bool drawGrid)
        {
            Bitmap mapImage = windows[window].fontRendererMapImage;
            AtariMap myMap = windows[window].map;
            Graphics gr = windows[window].pictureBoxGraphics;

            if (drawData)
            {
                AtariFontRenderer.RenderMapData(myMap, windows[window].fontType, mapImage);
                gr.DrawImage(mapImage, 0, 0, mapImage.Width * Globals.Zoom, mapImage.Height * Globals.Zoom);
            }
            //separatory screenov
            if (drawScreenBorders)
            {
                for (int x = 0; x <= (mapImage.Size.Width / 8) / myMap.ScreenSize.Width; x++)
                    gr.DrawLine(screenSeparatorPen, (-myMap.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                                                 0, (-myMap.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                                                 mapImage.Size.Height * Globals.Zoom);
                dliFormOrigin = new Point((-myMap.OffsetX % myMap.ScreenSize.Width + (0 + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                (-myMap.OffsetY % myMap.ScreenSize.Height + (0 + 1) * myMap.ScreenSize.Height) * Globals.CharSize);



                for (int y = 0; y <= (mapImage.Size.Height / 8) / myMap.ScreenSize.Height; y++)
                    gr.DrawLine(screenSeparatorPen, 0, (-myMap.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * Globals.CharSize,
                                      mapImage.Width * Globals.Zoom, (-myMap.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * Globals.CharSize);
            }
            //grid
            if (drawGrid)
            {
                Brush gridBrush = new SolidBrush(gridColor);
                for (int x = 0; x < (mapImage.Size.Width / 8); x++)
                    for (int y = 0; y < (mapImage.Size.Height / 8); y++)
                        if (myMap.OffsetX + x < myMap.Stride && myMap.OffsetY + y < myMap.MapSize.Height * myMap.ScreenSize.Height)
                            gr.FillRectangle(gridBrush, x * Globals.CharSize, y * Globals.CharSize, 1, 1);
                            //destImage.SetPixel(x * Globals.CharSize, y * Globals.CharSize, gridColor);
            }
        }
        /// <summary>
        /// Performs scroll of a window.
        /// </summary>
        /// <param name="newMouseLoc"></param>
        /// <param name="window"></param>
        /// <returns>True if scrolled.</returns>
        public static bool Scroll(Point newMouseLoc, Globals.WindowType window)
        {
            windows[window].map.Offset = previousOffset;
            bool newOffset = AtariFontRenderer.CalculateOffset((newMouseLoc.X - prevMouseLoc.X) / Globals.CharSize, (newMouseLoc.Y - prevMouseLoc.Y) / Globals.CharSize, windows[window].map);

            if (newOffset)
                Redraw(window);

            return newOffset;
        }

        public static void DrawClipBoard(Point location, Bitmap pictureBoxImage)
        {
            //store contents editor window contents under the current clipboard position -> underimage
            AtariClipboard.UnderImageGraphics.DrawImage(pictureBoxImage, 0, 0, new Rectangle(location.X - location.X % Globals.CharSize, location.Y - location.Y % Globals.CharSize, AtariClipboard.ClipboardImage.Width, AtariClipboard.ClipboardImage.Height), GraphicsUnit.Pixel);
            //draw clipboard -> location inside editor window 
            windows[Globals.WindowType.Editor].pictureBoxGraphics.DrawImage(AtariClipboard.ClipboardImage, location.X - location.X % Globals.CharSize, location.Y - location.Y % Globals.CharSize);
        }

        public static void DrawUnderClipBoard(Point location)
        {
            windows[Globals.WindowType.Editor].pictureBoxGraphics.DrawImage(AtariClipboard.UnderClipBoardImage, location.X - location .X % Globals.CharSize, location .Y - location .Y % Globals.CharSize, AtariClipboard.ClipboardImage.Width, AtariClipboard.ClipboardImage.Height);
        }
        /*
        public static void SetDestImage(Bitmap _destImage, Graphics _gr)
        {
            destImage = _destImage;
            gr = _gr;
        }*/
    }
}
