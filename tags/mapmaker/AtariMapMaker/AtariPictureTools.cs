using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AtariMapMaker
{
    class AtariPictureTools
    {
        AtariFontRenderer myRenderer;
        AtariMap myMap;
        Graphics gr;
        Rectangle mouseSelection = new Rectangle();
        Bitmap destImage;
        Point prevMouseLoc;
        Pen screenSeparatorPen = new Pen(Color.Red);
        Pen selectionPen = new Pen(Color.Lime);
        Color gridColor = Color.White;
        int zoom;
        int charsize;
        bool drawScreenBorders = true;
        bool drawGrid = true;

        public AtariPictureTools(Bitmap destImage, AtariFontRenderer myRenderer, AtariMap myMap, int zoom)
        {
            this.myMap = myMap;
            this.myRenderer = myRenderer;
            this.zoom = zoom;
            this.charsize = zoom * 8;
            this.destImage = destImage;
            this.gr = Graphics.FromImage(destImage);
            this.gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }

        public void SetGridVisibility(bool drawScreenBorders, bool drawGrid)
        {
            this.drawGrid = drawGrid;
            this.drawScreenBorders = drawScreenBorders;
        }

        public void SetMap(AtariMap myMap)
        {
            this.myMap = myMap;
        }

        public void SetZoom(int zoom)
        {
            this.zoom = zoom;
            this.charsize = zoom * 8;
        }

        public void SelectionStart(Point firstCorner)
        {
            mouseSelection.X = firstCorner.X - (firstCorner.X % charsize);
            mouseSelection.Y = firstCorner.Y - (firstCorner.Y % charsize);
            mouseSelection.Width = charsize;
            mouseSelection.Height = charsize;
            DrawSelection();
        }

        public void SelectionChange(Bitmap dataImage, Point newCorner)
        {
            int mx = newCorner.X - (newCorner.X % charsize);
            int my = newCorner.Y - (newCorner.Y % charsize);
            if (Math.Abs(mx - mouseSelection.X) != mouseSelection.Width ||
                Math.Abs(my - mouseSelection.Y) != mouseSelection.Height)
            {

                mouseSelection.Width = charsize + mx - mouseSelection.X;
                mouseSelection.Height = charsize + my - mouseSelection.Y;

                if (mouseSelection.Width + mouseSelection.X > dataImage.Width*zoom ||
                    mouseSelection.Height + mouseSelection.Y > dataImage.Height*zoom)
                {
                    //selection out of bounds - do not copy, do not draw selection
                    mouseSelection.Width = 0;
                    mouseSelection.Height = 0;
                }

                // gr.DrawImage(dataImage, 0, 0, dataImage.Width * zoom, dataImage.Height * zoom);
                this.Redraw(dataImage);

                DrawSelection();
            }
        }

        public bool SelectionEnd(AtariClipboard clipBoard, Bitmap dataImage)
        {
            if (mouseSelection.Width == 0 || mouseSelection.Height == 0)
                return false;
            if (clipBoard.UnderClipBoardImage != null)
                clipBoard.UnderClipBoardImage.Dispose();
            if (clipBoard.UnderImageGraphics != null)
                clipBoard.UnderImageGraphics.Dispose();


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

            clipBoard.SetDataSource(myMap);
            this.Redraw(dataImage, true, false, false);
            clipBoard.Copy(this.destImage, mouseSelection, myRenderer.offset, zoom);
            
            this.Redraw(dataImage, false, true, true);
            clipBoard.UnderClipBoardImage = new Bitmap(clipBoard.GetImage());
            clipBoard.UnderImageGraphics = Graphics.FromImage(clipBoard.UnderClipBoardImage);
            return true;
        }

        public Point PreviousMouseLocation
        {
            get
            {
                return this.prevMouseLoc;
            }
            set
            {
                this.prevMouseLoc = value;
            }
        }

        private void DrawSelection()
        {
            gr.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y);
            gr.DrawLine(selectionPen, mouseSelection.X + mouseSelection.Width, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            gr.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y + mouseSelection.Height, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            gr.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X, mouseSelection.Y + mouseSelection.Height);
        }

        public void Redraw(Bitmap dataImage)
        {
            Redraw(dataImage, true, drawScreenBorders, drawGrid);
        }

        public void Redraw(Bitmap dataImage, bool drawData, bool drawScreenBorders, bool drawGrid)
        {
            if (drawData)
                gr.DrawImage(dataImage, 0, 0, dataImage.Width * zoom, dataImage.Height * zoom);

            //separatory screenov
            if (drawScreenBorders)
            {
                for (int x = 0; x <= (destImage.Size.Width / charsize) / myMap.ScreenSize.Width; x++)
                    gr.DrawLine(screenSeparatorPen, (-myRenderer.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * charsize,
                                                 0, (-myRenderer.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * charsize,
                                                 destImage.Size.Height);



                for (int y = 0; y <= (destImage.Size.Height / charsize) / myMap.ScreenSize.Height; y++)
                    gr.DrawLine(screenSeparatorPen, 0, (-myRenderer.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * charsize,
                                      destImage.Width, (-myRenderer.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * charsize);
            }
            //grid
            if (drawGrid)
                for (int x = 0; x < (destImage.Size.Width / charsize); x++)
                    for (int y = 0; y < (destImage.Size.Height / charsize); y++)
                        if (myRenderer.OffsetX + x < myMap.Stride && myRenderer.OffsetY + y < myMap.Screens.Height * myMap.ScreenSize.Height)
                            destImage.SetPixel(x * charsize, y * charsize, gridColor);
        }

        public void Scroll(Bitmap dataImage, Point newMouseLoc)
        {
           
            bool newoffset = myRenderer.CalculateOffset((newMouseLoc.X - prevMouseLoc.X) / charsize, (newMouseLoc.Y - prevMouseLoc.Y) / charsize, myMap);

            if (newoffset)
            {

                prevMouseLoc.X = newMouseLoc.X;
                prevMouseLoc.Y = newMouseLoc.Y;

                myRenderer.RenderData(myMap, myRenderer.offset, dataImage);
                this.Redraw(dataImage);
            }
        }

        public void DrawClipBoard(AtariClipboard clipBoard, Point location)
        {
            clipBoard.UnderImageGraphics.DrawImage(destImage, 0, 0, new Rectangle(location.X - location.X % charsize, location.Y - location.Y % charsize, clipBoard.GetImage().Width, clipBoard.GetImage().Height), GraphicsUnit.Pixel);
            gr.DrawImage(clipBoard.GetImage(), location.X - location.X % charsize, location.Y - location.Y % charsize);
        }

        public void DrawUnderClipBoard(AtariClipboard clipBoard, Point location)
        {
            gr.DrawImage(clipBoard.UnderClipBoardImage, location.X - location .X % charsize, location .Y - location .Y % charsize);
        }

        public void SetDestImage(Bitmap destImage, Graphics gr)
        {
            this.destImage = destImage;
            this.gr = gr;
        }
    }
}
