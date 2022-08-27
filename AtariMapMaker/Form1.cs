using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

namespace AtariMapMaker
{
    public partial class MainForm : Form
    {
        private AtariColorPicker colorPickerForm;
        private AtariMap myMap;
        private string mouseStatus = "";
        //private Bitmap dataImage;                                   //picture with map (data only, no zoom)
        private Graphics gr;
        private FontCharPicker myCharPicker;
        private DliForm dliForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "AtariMapMaker v" + version + " by Martin Simecek";
            labelAbout2.Text = "Version " + version + "\n" + Properties.Resources.BuildDate;
            toolTip1.SetToolTip(buttonRefreshFont, "Reload font");

            AtariPalette.Load(Properties.Resources.altirraPAL);
            colorPickerForm = new AtariColorPicker();
            AtariFontRenderer.SetFontData(Properties.Resources.Default, Globals.FontType.Screen);

            myMap = new AtariMap(new Size(4, 4), new Size(32, 20));
            numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.Screens.Width;

            this.FillFontColorList();
            pictureBoxMap.Image = new Bitmap(pictureBoxMap.Width, pictureBoxMap.Height)
            {
                Palette = AtariPalette.GetPalette()
            };
            UpdateEditorWindowSizeInChars();
            //dataImage = new Bitmap(Globals.editorWindowSizeInChars.Width * 8, Globals.editorWindowSizeInChars.Height * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        
            /*
            AtariFontRenderer.RenderMapData(myMap, 0, dataImage);
            gr = Graphics.FromImage(pictureBoxMap.Image);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            */
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);

            myCharPicker = new FontCharPicker(pictureBox2);

            comboOperation.Items.AddRange(new String[4] { "Export", "Import", "Column Export", "Column Import" });
            comboOperation.SelectedIndex = 0;

            dliForm = new DliForm(myMap.ScreenSize.Height);
            dliForm.RenderData();

        }

        private void UpdateEditorWindowSizeInChars()
        {
            Globals.editorWindowSizeInChars = new Size(pictureBoxMap.Width / Globals.CharSize, pictureBoxMap.Height / Globals.CharSize);
        }

        private void ButtonShowFont_Click(object sender, EventArgs e)
        {
            myCharPicker.SetZoom();
            myCharPicker.Invalidate();
            myCharPicker.Show();
            myCharPicker.BringToFront();
        }

        private void FillFontColorList()
        {
            listView1.Clear();
            listView1.LargeImageList = GetFontColorImageList(AtariFontRenderer.Color5);
            listView1.Columns.Add("Color");
            listView1.Columns.Add("Value");
            listView1.Columns.Add("Address");
            listView1.SmallImageList = listView1.LargeImageList;
            for (int i = 0; i < 5; i++)
            {
                ListViewItem lvi = new ListViewItem
                {
                    ImageIndex = i,
                    Text = i == 4 ? "COLBAK" : $"COLPF{i}"
                };
                lvi.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[i])); 
                lvi.SubItems.Add("$" + String.Format("{0:X4}", 0xd016 + i));
                listView1.Items.Add(lvi);
            }
            listView1.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private ImageList GetFontColorImageList(byte[] color5)
        {
            ImageList il = new ImageList();
            Size size = new Size(30, 20);
            il.ImageSize = size;
            for (int i = 0; i < 5; i++)
            {
                Bitmap bmp = new Bitmap(size.Width, size.Height);
                Graphics gr = Graphics.FromImage(bmp);
                gr.Clear(AtariPalette.GetColor(color5[i]));
                il.Images.Add(bmp);

            }
            return il;
        }

        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            for (int a = 0; a < listView1.Items.Count; a++)
            {
                listView1.Items[a].Selected = false;
            }
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int colorIndex = listView1.SelectedItems[0].Index;
                byte index = AtariFontRenderer.Color5[colorIndex];
                colorPickerForm.TopMost = true;
                colorPickerForm.Pick(index);

                AtariFontRenderer.Color5[colorIndex] = (byte)colorPickerForm.PickedColorIndex();
                //myCharPicker.GetRenderer().Color5[colorIndex] = (byte)colorPicker.PickedColorIndex();
                FillFontColorList();
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                //AtariFontRenderer.RedrawFont();
                //pictureBoxMap.Invalidate();
                //myCharPicker.GetRenderer().RedrawFont();
                //myCharPicker.RedrawFontWindow();
                //RedrawEditorWindow();
            }
        }

        private void PictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Right)     //SCROLL
            {
                AtariPictureTools.Scroll(e.Location, Globals.WindowType.Editor);
                pictureBoxMap.Invalidate();
            }


            if (e.Button == MouseButtons.Left)      //SELECT 
            {
                if (mouseStatus == "SELECTION")
                {
                    AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.Editor);
                    pictureBoxMap.Invalidate();
                }
            }

            if (e.Button == MouseButtons.None)  //nothing is pressed
            {
                if (AtariClipboard.IsValid)  //copy mode (shows alpha blended clipBoard)
                {
                    //AtariPictureTools.AssignWindow((Bitmap)pictureBoxMap.Image, myMap);
                    AtariPictureTools.DrawClipBoard(e.Location); //TODO: to where?
                    pictureBoxMap.Refresh();
                    AtariPictureTools.DrawUnderClipBoard(e.Location);
                }
            }
            int xx = AtariFontRenderer.OffsetX + e.X / Globals.CharSize;
            int yy = AtariFontRenderer.OffsetY + e.Y / Globals.CharSize;
            int scrx = xx / myMap.ScreenSize.Width;
            int scry = yy / myMap.ScreenSize.Height;
            int posx = xx % myMap.ScreenSize.Width;
            int posy = yy % myMap.ScreenSize.Height;

            if (xx < myMap.Stride && yy < myMap.Screens.Height * myMap.ScreenSize.Height)
            {
                labelScreen.Text = "Screen: " + scrx.ToString() + ":" + scry.ToString();
                labelPosition.Text = "Position: " + posx.ToString() + ":" + posy.ToString() + " (" + xx.ToString() + ":" + yy.ToString() + ")";
                byte charVal = myMap.Data[xx + yy * myMap.Stride];
                labelChar.Text = "Char: $" + String.Format("{0:X2}", charVal) + " (" + charVal + ")";

                Point dliPoint = DliFormOrigin(scrx, scry);
                if (dliPoint.X != -1)
                {
                    //dliForm.Left = this.Left + this.ClientRectangle.Left + pictureBoxMap.Left + dliPoint.X * Globals.CharSize;
                    //dliForm.Top = this.Top + this.ClientRectangle.Top + pictureBoxMap.Top + dliPoint.Y * Globals.CharSize;
                    Rectangle r = this.RectangleToScreen(this.ClientRectangle);
                    dliForm.Left = r.Left + dliPoint.X * Globals.CharSize;
                    dliForm.Top = r.Top + dliPoint.Y * Globals.CharSize;
                    //dliForm.ZoomResize();
                    dliForm.TopMost = true;
                    dliForm.Show();
                }
                else
                {
                    dliForm.Hide();
                }
            }
        }

        private Point DliFormOrigin(int scrx, int scry)
        {
            int xmin = (scrx + 1) * myMap.ScreenSize.Width - 1;
            int xmax = (scrx + 1) * myMap.ScreenSize.Width + 4;
            int ymin = scry * myMap.ScreenSize.Height;
            int ymax = (scry + 1) * myMap.ScreenSize.Height;

            Rectangle visibleArea = new Rectangle(AtariFontRenderer.OffsetX, AtariFontRenderer.OffsetY, Globals.editorWindowSizeInChars.Width, Globals.editorWindowSizeInChars.Height);
            if (visibleArea.Contains(new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin)))
                return new Point(xmin + 1 - AtariFontRenderer.OffsetX, ymin - AtariFontRenderer.OffsetY);
            else
                return new Point(-1, -1);
        }

        private void PictureBoxMap_MouseDown(object sender, MouseEventArgs e)
        {
            AtariPictureTools.PreviousMouseLocation = e.Location;
            if (e.Button == MouseButtons.Left)
            {

                if (AtariClipboard.IsValid)
                {
                    AtariClipboard.SetDataSource(myMap);     //to copy always to map (not to char selector)
                    int addoffset = (e.X / Globals.CharSize) + myMap.Stride * (e.Y / Globals.CharSize);
                    AtariClipboard.Paste(AtariFontRenderer.offset + addoffset);
                    RedrawEditorWindow();
                }
                else
                {
                    if (mouseStatus == "")
                    {
                        AtariPictureTools.SelectionStart(e.Location, Globals.WindowType.Editor);
                        mouseStatus = "SELECTION";

                    }
                }

            }

            if (e.Button == MouseButtons.Right)
            {
                if (mouseStatus == "SELECTION")
                    mouseStatus = "";
            }
        }



        private void PictureBoxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                if (mouseStatus == "SELECTION")
                {
                    AtariPictureTools.SelectionEnd(Globals.WindowType.Editor);
                    AtariClipboard.IsValid = true;
                    pictureBox2.Image = AtariClipboard.GetImage();
                    pictureBoxMap.Invalidate();
                    mouseStatus = "";
                }
        }

        private void PictureBoxMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                AtariClipboard.IsValid = false;
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Invalidate();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            //this.Text = "debug: " + mouseStatus + " " + drawNo.ToString();

        }

        private void PictureBoxMap_Resize(object sender, EventArgs e)
        {
            /*pictureBox1.Width = splitContainer1.Panel1.Width;

            pictureBox1.Height = splitContainer1.Panel1.Height - toolStrip1.Height;
            */
            /*if (dataImage != null)
                dataImage.Dispose();
            dataImage = new Bitmap(pictureBox1.Width / zoomMultiplier[zoom], pictureBox1.Height / zoomMultiplier[zoom], System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            gr.Dispose();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            gr = Graphics.FromImage(pictureBox1.Image);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            myRenderer.RenderData(myMap, myRenderer.offset, dataImage);
            pictureBox1.Invalidate();
            */
        }

        private void PictureBoxMap_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateEditorWindowSizeInChars();

            if (myMap == null)
                return;

            if (dataImage != null)
                dataImage.Dispose();
            dataImage = new Bitmap(pictureBoxMap.Width / Globals.Zoom, pictureBoxMap.Height / Globals.Zoom, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            if (pictureBoxMap.Image != null)
            {
                pictureBoxMap.Image.Dispose();
                gr.Dispose();
            }
            pictureBoxMap.Image = new Bitmap(pictureBoxMap.Width, pictureBoxMap.Height);

            gr = Graphics.FromImage(pictureBoxMap.Image);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            AtariFontRenderer.SelectFont(Globals.FontType.Screen);
            AtariFontRenderer.RenderMapData(myMap, AtariFontRenderer.offset, dataImage);
            AtariPictureTools.SetDestImage((Bitmap)pictureBoxMap.Image, gr);   //update of new image in picturebox
            AtariPictureTools.AssignWindow((Bitmap)pictureBoxMap.Image, myMap);
            AtariPictureTools.Redraw(dataImage);
            pictureBoxMap.Invalidate();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    Save(saveFileDialog1.FileName); //test.dat
                    break;
            }
            //saveFileDialog1.Filter = "AtariMap (*.amp)|*.amp";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    Save(saveFileDialog1.FileName);

        }

        private void Save(string filename)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, myMap);
            //bf.Serialize(ms, AtariFontRenderer);
            byte[] bb = ms.ToArray();
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            //fs.Write(bb, 0, bb.Length);

            fs.Write(bb, 0, bb.Length);
            fs.Close();
            ms.Close();
            ms.Dispose();

            //save
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    BinaryFormatter bf = new BinaryFormatter();
                    System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open); //test.dat
                    myMap = (AtariMap)bf.Deserialize(fs);
                    //buttonLoad.Text = fs.Position.ToString();
                    //myRenderer = (AtariFontRenderer)bf.Deserialize(fs);
                    //buttonLoad.Text = myMap.ScreenSize.Height.ToString();
                    //AtariPictureTools.AssignWindow((Bitmap)pictureBoxMap.Image, myMap);
                    AtariPictureTools.SetMap(Globals.WindowType.Editor, myMap);
                    AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
                  
                    fs.Close();
                    this.FillFontColorList();
                    RedrawEditorWindow();
                    //myCharPicker.GetRenderer().FontData = AtariFontRenderer.FontData;
                    //myCharPicker.GetRenderer().Color5 = AtariFontRenderer.Color5;
                    myCharPicker.RedrawFontWindow();
                    break;
            }
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.Export((int)numericUpDown1.Value, (int)numericUpDown3.Value, (int)numericUpDown2.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value, saveFileDialog1.FileName);
                    int width = (int)((numericUpDown2.Value - numericUpDown1.Value + 1) * myMap.ScreenSize.Width + numericUpDown5.Value);
                    MessageBox.Show("Export dataline width: " + width);
                    break;
            }
        }

        private void Export(int x1, int y1, int x2, int y2, int extraCharsOnLine, string filename)
        {
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = (x2 + 1) * myMap.ScreenSize.Width;
            int yf = (y2 + 1) * myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;
            for (int y = ys; y < yf; y++)
                for (int x = xs; x < xf + extraCharsOnLine; x++)
                {

                    if (x < xf)
                        myData = myMap.Data[x + y * myMap.Stride];
                    else
                        myData = 0;

                    fs.WriteByte(myData);
                }
            fs.Close();
            fs.Dispose();
        }

        private void ExportColumns(int x1, int y1, int x2, int y2, string filename)
        {
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = (x2 + 1) * myMap.ScreenSize.Width;
            int yf = (y2 + 1) * myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;

            for (int x = xs; x < xf; x++)
                for (int y = ys; y < yf; y++)
                {


                    myData = myMap.Data[x + y * myMap.Stride];

                    fs.WriteByte(myData);
                }
            fs.Close();
            fs.Dispose();
        }

        private void ImportColumns(int x1, int y1, string filename)
        {
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int yf = ys + myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int x = xs;
            while (fs.Position <= (fs.Length - myMap.ScreenSize.Height))
            {

                for (int y = ys; y < yf; y++)
                {
                    myData = (byte)fs.ReadByte();

                    myMap.Data[x + y * myMap.Stride] = myData;

                }
                x++;
                if (x == myMap.Stride)
                {
                    MessageBox.Show("Reading aborted! Reached right edge of map.");
                    break;
                }
            }
            fs.Close();
            fs.Dispose();

        }

        private void Import(int x1, int y1, int width, string filename)
        {
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = xs + width;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int y = ys;
            while (fs.Position <= (fs.Length - width))
            {
                for (int x = xs; x < xf; x++)
                {
                    myData = (byte)fs.ReadByte();
                    myMap.Data[x + y * myMap.Stride] = myData;

                }
                y++;
                if (y == myMap.Screens.Height * myMap.ScreenSize.Height)
                {
                    MessageBox.Show("Reading aborted! Reached bottom edge of map.");
                    break;
                }
            }
    
            fs.Close();
            fs.Dispose();

        }

        private void ButtonLoadFont_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Atari Font (*.fnt)|*.fnt";
            switch (openFileDialog1.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.OK:

                    AtariFontRenderer.LoadFont(openFileDialog1.FileName, Globals.FontType.Screen);
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
                    //myCharPicker.GetRenderer().LoadFont(openFileDialog1.FileName);
                    //myCharPicker.GetRenderer().RedrawFont();
                    //myCharPicker.RedrawFontWindow();
                    //RedrawEditorWindow();
                    break;
            }
        }
   

        private void ButtonShiftChars_Click(object sender, EventArgs e)
        {
            //64-79 -> 80-95
            //32-47 -> 64-79
            if (MessageBox.Show("R U sure?", "Shift characters in map", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i < myMap.Data.Length; i++)
                {
                    if (myMap.Data[i] >= 64 && myMap.Data[i] <= 79)
                    {
                        myMap.Data[i] += 16;
                    }
                    else
                    {
                        if (myMap.Data[i] >= 32 && myMap.Data[i] <= 47)
                        {
                            myMap.Data[i] += 32;
                        }
                    }
                }
                MessageBox.Show("Done");
            }
        }

        private void ButtonHoboExport_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ExportColumns((int)numericUpDown1.Value, (int)numericUpDown3.Value, (int)numericUpDown2.Value, (int)numericUpDown4.Value, saveFileDialog1.FileName);
                    break;
            }
        }

        private void ButtonHoboImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Column based map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ImportColumns((int)numericUpDown1.Value, (int)numericUpDown3.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void ButtonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.Import((int)numericUpDown1.Value, (int)numericUpDown3.Value, (int)numericUpDown6.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void BtnNewMap_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Create new map? (current mapdata will be deleted!)", "New map", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
               
                myMap = new AtariMap(new Size((int)nudMapW.Value, (int)nudMapH.Value), new Size((int)nudScreenW.Value, (int)nudScreenH.Value));
                AtariPictureTools.SetMap(Globals.WindowType.Editor, myMap);
                numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.Screens.Width;
                RedrawEditorWindow();
            }

        }

        private void TrackBarZoom_Scroll(object sender, EventArgs e)
        {
            Globals.Zoom = trackBarZoom.Value;
            PictureBoxMap_ClientSizeChanged(null, null);
        }

        private void ComboBoxDrawGrid_CheckedChanged(object sender, EventArgs e)
        {
            AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
            RedrawEditorWindow();
        }

        private void ComboBoxDrawBorders_CheckedChanged(object sender, EventArgs e)
        {
            AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
            RedrawEditorWindow();
        }

        private void RedrawEditorWindow()
        {
            //AtariFontRenderer.RenderMapData(myMap, AtariFontRenderer.offset, dataImage); //redraw data
            AtariPictureTools.Redraw(Globals.WindowType.Editor); //dataImage);                         //redraw grids
            pictureBoxMap.Invalidate();
        }

        private void ComboBoxOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    numericUpDown1.Enabled = true;
                    numericUpDown2.Enabled = true;
                    numericUpDown3.Enabled = true;
                    numericUpDown4.Enabled = true;
                    numericUpDown5.Enabled = true;
                    numericUpDown6.Enabled = false;
                    break;
                case 1:
                    numericUpDown1.Enabled = true;
                    numericUpDown2.Enabled = false;
                    numericUpDown3.Enabled = true;
                    numericUpDown4.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = true;
                    break;
                case 2:
                    numericUpDown1.Enabled = true;
                    numericUpDown2.Enabled = true;
                    numericUpDown3.Enabled = true;
                    numericUpDown4.Enabled = true;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 3:
                    numericUpDown1.Enabled = true;
                    numericUpDown2.Enabled = false;
                    numericUpDown3.Enabled = true;
                    numericUpDown4.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
            
            }
        }

        private void ButtonPerform_Click(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    ButtonExport_Click(null, null);
                    break;
                case 1:
                    ButtonImport_Click(null, null);
                    break;
                case 2:
                    ButtonHoboExport_Click(null, null);
                    break;
                case 3:
                    ButtonHoboImport_Click(null, null);
                    break;
            }
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://sourceforge.net/projects/atari-mapmaker/");
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://matosimi.atari.org");
        }

        private void ButtonRefreshFont_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(AtariFontRenderer.LastFontFile))
            {
                AtariFontRenderer.LoadFont(AtariFontRenderer.LastFontFile, Globals.FontType.Screen);
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);

                //AtariFontRenderer.RedrawFont();
                //myCharPicker.GetRenderer().LoadFont(myRenderer.LastFontFile);
                //myCharPicker.GetRenderer().RedrawFont();
                //myCharPicker.RedrawFontWindow();
                //RedrawEditorWindow();
            }
        }
    }
}