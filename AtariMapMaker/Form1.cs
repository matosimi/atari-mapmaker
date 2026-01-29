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
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace AtariMapMaker
{
    public partial class MainForm : Form
    {
        private AtariColorPicker colorPickerForm;
        private AtariMap myMap;
        private string mouseStatus = "";
        //private Bitmap dataImage;                                   //picture with map (data only, no zoom)
        //private Graphics gr;
        private FontCharPicker myCharPicker;
        private DliForm dliForm;
        private Point currentScreen = new Point(0, 0);
        private bool ScreenSelectionShown = false;
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
            undoManager = new UndoManager(myMap);
            numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;

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

            myCharPicker = new FontCharPicker(pictureBoxClipboard);

            comboOperation.Items.AddRange(new String[8] { "Export", "Import", "Column Export", "Column Import", "Export DLI", "Import DLI", "Export screen by screen", "Import screen by screen" });
            comboOperation.SelectedIndex = 0;

            dliForm = new DliForm(myMap, pictureBoxMap);
            dliForm.RenderData();

            // Add new UI elements for v2.0 features
            AddV2UIElements();

        }

        private CheckBox checkBoxMultiFont;

        private void AddV2UIElements()
        {
            // Add MultiFont checkbox in the Colors group
            checkBoxMultiFont = new CheckBox();
            checkBoxMultiFont.Text = "Enable Multi-Font";
            checkBoxMultiFont.Location = new Point(checkBoxShowDli.Location.X + 10, checkBoxShowDli.Location.Y + checkBoxShowDli.Height + 10);
            checkBoxMultiFont.Size = new Size(150, 20);
            checkBoxMultiFont.Checked = myMap != null ? myMap.MultiFontEnabled : false;
            checkBoxMultiFont.CheckedChanged += CheckBoxMultiFont_CheckedChanged;
            groupBoxDli.Controls.Add(checkBoxMultiFont);
            
            // Update font template button and element library button visibility based on multifont
            UpdateMultiFontUI();

            // Add Font Template button next to Load Font button
            Button buttonFontTemplate = new Button();
            buttonFontTemplate.Text = "Font Templates";
            buttonFontTemplate.Location = new Point(buttonLoadFont.Location.X, buttonLoadFont.Location.Y - 45);
            buttonFontTemplate.Size = new Size(112, 35);
            buttonFontTemplate.Click += ButtonFontTemplate_Click;
            flowLayoutPanel1.Controls.Add(buttonFontTemplate);

            // Add Element Library button
            Button buttonElementLibrary = new Button();
            buttonElementLibrary.Text = "Element Library";
            buttonElementLibrary.Location = new Point(buttonLoadFont.Location.X, buttonLoadFont.Location.Y + 45);
            buttonElementLibrary.Size = new Size(112, 35);
            buttonElementLibrary.Click += ButtonElementLibrary_Click;
            flowLayoutPanel1.Controls.Add(buttonElementLibrary);

            // Add Map Description button
            Button buttonMapDescription = new Button();
            buttonMapDescription.Text = "Map Description";
            buttonMapDescription.Location = new Point(buttonLoadFont.Location.X, buttonLoadFont.Location.Y + 90);
            buttonMapDescription.Size = new Size(112, 35);
            buttonMapDescription.Click += ButtonMapDescription_Click;
            flowLayoutPanel1.Controls.Add(buttonMapDescription);

            // Add Export Font button (for single font)
            Button buttonExportFont = new Button();
            buttonExportFont.Text = "Export Font";
            buttonExportFont.Size = new Size(112, 35);
            buttonExportFont.Top = buttonLoadFont.Bottom + 10;
            buttonExportFont.Click += ButtonExportFont_Click;
            groupBoxFont.Controls.Add(buttonExportFont);

            // Add context menu items for screen operations
            ToolStripMenuItem menuItemLinkScreen = new ToolStripMenuItem("Link to Screen...");
            menuItemLinkScreen.Click += MenuItemLinkScreen_Click;
            contextMenuStripScreen.Items.Add(new ToolStripSeparator());
            contextMenuStripScreen.Items.Add(menuItemLinkScreen);

            ToolStripMenuItem menuItemScreenMetadata = new ToolStripMenuItem("Screen Metadata...");
            menuItemScreenMetadata.Click += MenuItemScreenMetadata_Click;
            contextMenuStripScreen.Items.Add(menuItemScreenMetadata);

            ToolStripMenuItem menuItemScreenDescription = new ToolStripMenuItem("Screen Description...");
            menuItemScreenDescription.Click += MenuItemScreenDescription_Click;
            contextMenuStripScreen.Items.Add(menuItemScreenDescription);

            // Add Undo/Redo to a menu (if there's a menu bar) or create keyboard shortcuts
            // For now, add them to the context menu as well
            ToolStripMenuItem menuItemUndo = new ToolStripMenuItem("Undo");
            menuItemUndo.ShortcutKeys = Keys.Control | Keys.Z;
            menuItemUndo.Click += MenuItemUndo_Click;
            contextMenuStripScreen.Items.Add(new ToolStripSeparator());
            contextMenuStripScreen.Items.Add(menuItemUndo);

            ToolStripMenuItem menuItemRedo = new ToolStripMenuItem("Redo");
            menuItemRedo.ShortcutKeys = Keys.Control | Keys.Y;
            menuItemRedo.Click += MenuItemRedo_Click;
            contextMenuStripScreen.Items.Add(menuItemRedo);
        }

        private UndoManager undoManager;

        private void MenuItemUndo_Click(object sender, EventArgs e)
        {
            if (undoManager != null && undoManager.CanUndo())
            {
                undoManager.Undo();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void MenuItemRedo_Click(object sender, EventArgs e)
        {
            if (undoManager != null && undoManager.CanRedo())
            {
                undoManager.Redo();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void MenuItemLinkScreen_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ScreenLinkDialog dialog = new ScreenLinkDialog(myMap, currentScreen))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
            }
        }

        private void MenuItemScreenMetadata_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ScreenMetadataDialog dialog = new ScreenMetadataDialog(myMap, currentScreen))
            {
                dialog.ShowDialog();
            }
        }

        private void MenuItemScreenDescription_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ScreenDescriptionDialog dialog = new ScreenDescriptionDialog(myMap, currentScreen))
            {
                dialog.ShowDialog();
            }
        }

        private void UpdateEditorWindowSizeInChars()
        {
            Globals.editorWindowSizeInChars = new Size(pictureBoxMap.Width / Globals.CharSize, pictureBoxMap.Height / Globals.CharSize);
        }

        private void ButtonShowFont_Click(object sender, EventArgs e)
        {
            myCharPicker.SetZoom();
            myCharPicker.Refresh();
            myCharPicker.Show();
            myCharPicker.BringToFront();
        }

        private void FillFontColorList()
        {
            listViewColors.Clear();
            listViewColors.LargeImageList = GetFontColorImageList(AtariFontRenderer.Color5);
            listViewColors.Columns.Add("Color");
            listViewColors.Columns.Add("Value");
            listViewColors.Columns.Add("Address");
            listViewColors.SmallImageList = listViewColors.LargeImageList;
            for (int i = 0; i < 5; i++)
            {
                ListViewItem lvi = new ListViewItem
                {
                    ImageIndex = i,
                    Text = i == 4 ? "COLBAK" : $"COLPF{i}"
                };
                lvi.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[i]));
                lvi.SubItems.Add("$" + String.Format("{0:X4}", 0xd016 + i));
                listViewColors.Items.Add(lvi);
            }
            //add scanline alter colors only when possible
            if (AtariFontRenderer.Color5.Length > 5)
            {
                ListViewItem lviAlter = new ListViewItem
                {
                    ImageIndex = 5,
                    Text = "PF3 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[5]));
                listViewColors.Items.Add(lviAlter);

                lviAlter = new ListViewItem
                {
                    ImageIndex = 6,
                    Text = "PF0 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[6]));
                listViewColors.Items.Add(lviAlter);

                lviAlter = new ListViewItem
                {
                    ImageIndex = 7,
                    Text = "PF2 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[7]));
                listViewColors.Items.Add(lviAlter);

                listViewColors.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewColors.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private ImageList GetFontColorImageList(byte[] color5)
        {
            ImageList il = new ImageList
            {
                ColorDepth = ColorDepth.Depth24Bit,
                ImageSize = new Size(30, 20)
            };
            for (int i = 0; i < color5.Length; i++)
            {
                Bitmap bmp = new Bitmap(il.ImageSize.Width, il.ImageSize.Height);
                Graphics gr = Graphics.FromImage(bmp);
                gr.Clear(AtariPalette.GetColor(color5[i]));
                il.Images.Add(bmp);

            }
            return il;
        }

        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            for (int a = 0; a < listViewColors.Items.Count; a++)
            {
                listViewColors.Items[a].Selected = false;
            }
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewColors.SelectedItems.Count == 1)
            {
                int colorIndex = listViewColors.SelectedItems[0].Index;
                byte index = AtariFontRenderer.Color5[colorIndex];
                colorPickerForm.Owner = this;
                colorPickerForm.Pick(index);

                AtariFontRenderer.Color5[colorIndex] = colorPickerForm.PickedColorIndex;
                FillFontColorList();
                AtariFontRenderer.RedrawFontImage(Globals.FontType.Screen);
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
                myCharPicker.Refresh();
                myCharPicker.GetPictureBox().Refresh();
                //pictureBoxClipboard.Image = AtariPictureTools.windows[Globals.WindowType.Editor].destinationImage;

                //AtariFontRenderer.RedrawFont();
                //pictureBoxMap.Invalidate();
                //myCharPicker.GetRenderer().RedrawFont();
                //myCharPicker.RedrawFontWindow();
                //RedrawEditorWindow();
            }
        }

        private void PictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            int xx = myMap.OffsetX + e.X / Globals.CharSize;
            int yy = myMap.OffsetY + e.Y / Globals.CharSize;
            int scrx = xx / myMap.ScreenSize.Width;
            int scry = yy / myMap.ScreenSize.Height;
            int posx = xx % myMap.ScreenSize.Width;
            int posy = yy % myMap.ScreenSize.Height;
            currentScreen.X = scrx;
            currentScreen.Y = scry;

            if (e.Button == MouseButtons.Right)     //SCROLL
            {
                if (AtariPictureTools.Scroll(e.Location, Globals.WindowType.Editor))
                {
                    AtariPictureTools.PreviousMouseLocation = e.Location;
                    AtariPictureTools.PreviousOffset = myMap.Offset;
                    if (tabControl1.SelectedTab == tabPage2 && checkBoxShowScreenSelection.Checked)
                    {
                        ShowScreenSelection(false);
                    }
                    else 
                        pictureBoxMap.Refresh();
                }

                if (checkBoxEditDli.Checked)
                    UpdateAndShowDliForm(scrx, scry, true);
            }


            if (e.Button == MouseButtons.Left)      //SELECT 
            {
                if (mouseStatus == "SELECTION")
                {
                    AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
            }

            if (e.Button == MouseButtons.None)  //nothing is pressed
            {
                if (AtariClipboard.IsValid)  //copy mode (shows alpha blended clipBoard)
                {
                    //AtariPictureTools.AssignWindow((Bitmap)pictureBoxMap.Image, myMap);
                    
                    AtariPictureTools.DrawClipBoard(e.Location, (Bitmap)pictureBoxMap.Image); //TODO: to where?
                    pictureBoxMap.Refresh();
                    AtariPictureTools.DrawUnderClipBoard(e.Location);
                }
            }
            

            if (xx < myMap.Stride && yy < myMap.MapSize.Height * myMap.ScreenSize.Height)
            {
                labelScreen.Text = $"Screen: {currentScreen.X}:{currentScreen.Y}";
                labelPosition.Text = "Position: " + posx.ToString() + ":" + posy.ToString() + " (" + xx.ToString() + ":" + yy.ToString() + ")";
                byte charVal = myMap.Data[xx + yy * myMap.Stride];
                labelChar.Text = "Char: $" + String.Format("{0:X2}", charVal) + " (" + charVal + ")";
                //calculate the occurence
                (int idx, int amnt) = myMap.CharOccurence(new Point(currentScreen.X,currentScreen.Y), posx, posy, charVal);
                labelCharOccurence.Text = $"{idx} of {amnt}";
            }
        }

        private void UpdateAndShowDliForm(int scrx,int scry, bool justUpdatePosition = false)
        {
            //check for out of bounds screens
            if (scrx >= myMap.MapSize.Width || scry >= myMap.MapSize.Height)
            {
                dliForm.Hide();
                return;
            }
            
            Point dliPoint = DliFormOrigin(scrx, scry);
            if (dliPoint.X != -1)
            {
                Rectangle r = this.RectangleToScreen(this.ClientRectangle);
                dliForm.Left = r.Left + dliPoint.X * Globals.CharSize;
                dliForm.Top = r.Top + dliPoint.Y * Globals.CharSize;
                dliForm.Owner = this;
                if (!justUpdatePosition)
                {
                    myMap.CopyDliColorsFullScreen(scrx + scry * myMap.MapSize.Width, dliForm.DliMap, 0);  //copy screen colors to DLI color editor
                    dliForm.RenderData();
                }
                else
                    dliForm.RenderData(true);
                dliForm.Show(scrx + scry * myMap.MapSize.Width);

            }
            else
            {
                dliForm.Hide();
            }
        }

        private Point DliFormOrigin(int scrx, int scry)
        {
            int xmin = (scrx + 1) * myMap.ScreenSize.Width - 1;
            int xmax = (scrx + 1) * myMap.ScreenSize.Width + 4;
            int ymin = scry * myMap.ScreenSize.Height;
            int ymax = (scry + 1) * myMap.ScreenSize.Height;

            Rectangle visibleArea = new Rectangle(myMap.OffsetX, myMap.OffsetY, Globals.editorWindowSizeInChars.Width, Globals.editorWindowSizeInChars.Height);
            if (visibleArea.Contains(new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin)))
                return new Point(xmin + 1 - myMap.OffsetX, ymin - myMap.OffsetY);
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
                    if (myMap.OffsetX + (e.X / Globals.CharSize) + AtariClipboard.ClipboardWidth > myMap.Stride)
                    { }
                    else
                    {
                        AtariClipboard.SetDataSource(myMap);     //to copy always to map (not to char selector)
                        int addoffset = (e.X / Globals.CharSize) + myMap.Stride * (e.Y / Globals.CharSize);
                        AtariClipboard.Paste(myMap.Offset + addoffset);
                        //RedrawEditorWindow();
                        AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    }
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
                AtariPictureTools.PreviousOffset = myMap.Offset;

                if (checkBoxEditDli.Checked)
                {
                    int xx = myMap.OffsetX + e.X / Globals.CharSize;
                    int yy = myMap.OffsetY + e.Y / Globals.CharSize;
                    int scrx = xx / myMap.ScreenSize.Width;
                    int scry = yy / myMap.ScreenSize.Height;
                    UpdateAndShowDliForm(scrx, scry);
                }
                else
                    dliForm.Hide();
            }
        }



        private void PictureBoxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mouseStatus == "SELECTION")
                {
                    if (AtariPictureTools.SelectionEnd(Globals.WindowType.Editor))
                    {
                        AtariClipboard.IsValid = true;
                        pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                        pictureBoxMap.Refresh();
                    }
                    mouseStatus = "";   //reset mouse status no matter if selection end is valid or not
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                contextMenuStripScreen.Show(pictureBoxMap, e.Location);
            }

        }

        private void PictureBoxMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                AtariClipboard.IsValid = false;
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
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
            int ignoredWidth = pictureBoxMap.Width % (8 * Globals.Zoom);
            int ignoredHeight = pictureBoxMap.Height % (8 * Globals.Zoom);
            try
            {
                pictureBoxMap.Image = new Bitmap(pictureBoxMap.Width - ignoredWidth, pictureBoxMap.Height - ignoredHeight);
            }
            catch (ArgumentException)   //fix crash when window shrinked to 0 width or height
            { }
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    SaveMap(saveFileDialog1.FileName); //test.dat
                    break;
            }
            //saveFileDialog1.Filter = "AtariMap (*.amp)|*.amp";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    Save(saveFileDialog1.FileName);

        }

        private void SaveMap(string filename)
        {
            AtariJson.Atrmap atrmap = new AtariJson.Atrmap
            {
                MapData = myMap.Data.Select(i => (int)i).ToArray(),
                MapSize = myMap.MapSize,
                MapScreenSize = myMap.ScreenSize,
                FontData = AtariFontRenderer.fonts[Globals.FontType.Screen].data.Select(i => (int)i).ToArray(),
                Color5 = AtariFontRenderer.Color5.Select(i => (int)i).ToArray(),
                DliData = myMap.ColorData.Select(i => (int)i).ToArray(),
                // v2.0 fields
                FontDataArray = myMap.FontDataArray?.Select(font => font?.Select(i => (int)i).ToArray()).ToArray(),
                FontFileNames = myMap.FontFileNames,
                FontLineMapping = myMap.FontLineMapping?.Select(i => (int)i).ToArray(),
                FontTemplateLocked = myMap.FontTemplateLocked,
                FontTemplatePattern = myMap.FontTemplatePattern,
                MultiFontEnabled = myMap.MultiFontEnabled,
                MapDescription = myMap.MapDescription,
                ScreenDescriptions = myMap.ScreenDescriptions,
                ScreenMetadataDict = myMap.ScreenMetadata,
                SubmapPath = myMap.SubmapPath,
                IsTilemap = myMap.IsTilemap,
                TilemapInfo = myMap.TilemapInfo,
                BitmapTileset = myMap.BitmapTileset,
                ElementLibrary = myMap.ElementLibrary,
                ScreenLinks = myMap.ScreenLinks
            };
            AtariJson.SaveAtrMap(atrmap, filename);
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    LoadMap(openFileDialog1.FileName);
                    
                    AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                    AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
                  
                    checkBoxShowDli.Checked = true;
                    this.FillFontColorList();
                    // Update multifont checkbox
                    if (checkBoxMultiFont != null)
                        checkBoxMultiFont.Checked = myMap.MultiFontEnabled;
                    RedrawEditorWindow();
                    //myCharPicker.GetRenderer().FontData = AtariFontRenderer.FontData;
                    //myCharPicker.GetRenderer().Color5 = AtariFontRenderer.Color5;
                    myCharPicker.RedrawFontWindow();
                    dliForm.Dispose();
                    dliForm = new DliForm(myMap, pictureBoxMap);
                    dliForm.RenderData();
                    dliForm.ZoomResize();
                    numericUpDownScreenFromX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenFromY.Maximum = myMap.MapSize.Height;
                    numericUpDownScreenToX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenToY.Maximum = myMap.MapSize.Height;
                    numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                    break;
            }
        }

        private void LoadMap(string fileName)
        {
            AtariJson.ParseAtrmap(fileName);
            myMap = new AtariMap(AtariJson.ParsedData.MapSize, AtariJson.ParsedData.MapScreenSize)
            {
                Data = AtariJson.ParsedData.MapData.Select(i => (byte)i).ToArray()
            };
            
            AtariFontRenderer.Color5 = AtariJson.ParsedData.Color5.Select(i => (byte)i).ToArray();
            
            // Load fonts - handle v1.2 and v2.0 formats
            if (AtariJson.ParsedData.FontDataArray != null && AtariJson.ParsedData.FontDataArray.Length > 0)
            {
                // v2.0 format with multiple fonts
                for (int i = 0; i < AtariJson.ParsedData.FontDataArray.Length && i < 8; i++)
                {
                    if (AtariJson.ParsedData.FontDataArray[i] != null)
                    {
                        string fileNameFont = (AtariJson.ParsedData.FontFileNames != null && i < AtariJson.ParsedData.FontFileNames.Length) 
                            ? AtariJson.ParsedData.FontFileNames[i] : null;
                        myMap.SetFontData(AtariJson.ParsedData.FontDataArray[i].Select(j => (byte)j).ToArray(), i, fileNameFont);
                    }
                }
                // Set the first font as the active screen font
                if (AtariJson.ParsedData.FontDataArray[0] != null)
                {
                    AtariFontRenderer.SetFontData(AtariJson.ParsedData.FontDataArray[0].Select(i => (byte)i).ToArray(), Globals.FontType.Screen);
                }
            }
            else if (AtariJson.ParsedData.FontData != null)
            {
                // v1.2 format - migrate to v2.0
                myMap.SetFontData(AtariJson.ParsedData.FontData.Select(i => (byte)i).ToArray(), 0);
                AtariFontRenderer.SetFontData(AtariJson.ParsedData.FontData.Select(i => (byte)i).ToArray(), Globals.FontType.Screen);
            }
            
            // Load font line mapping
            if (AtariJson.ParsedData.FontLineMapping != null)
            {
                myMap.FontLineMapping = AtariJson.ParsedData.FontLineMapping.Select(i => (byte)i).ToArray();
            }
            else
            {
                // Initialize to all font 0
                myMap.SetFontForAllLines(0);
            }
            
            // Load font template settings
            if (AtariJson.ParsedData.FontTemplateLocked.HasValue)
                myMap.FontTemplateLocked = AtariJson.ParsedData.FontTemplateLocked.Value;
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.FontTemplatePattern))
                myMap.FontTemplatePattern = AtariJson.ParsedData.FontTemplatePattern;
            if (AtariJson.ParsedData.MultiFontEnabled.HasValue)
                myMap.MultiFontEnabled = AtariJson.ParsedData.MultiFontEnabled.Value;
            
            // Load DLI data
            if (AtariJson.ParsedData.DliData == null)
                myMap.InitDliColorFullMap();
            else
                myMap.ColorData = AtariJson.ParsedData.DliData.Select(i => (byte)i).ToArray();

            // Load v2.0 fields
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.MapDescription))
                myMap.MapDescription = AtariJson.ParsedData.MapDescription;
            if (AtariJson.ParsedData.ScreenDescriptions != null)
                myMap.ScreenDescriptions = AtariJson.ParsedData.ScreenDescriptions;
            if (AtariJson.ParsedData.ScreenMetadataDict != null)
                myMap.ScreenMetadata = AtariJson.ParsedData.ScreenMetadataDict;
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.SubmapPath))
                myMap.SubmapPath = AtariJson.ParsedData.SubmapPath;
            if (AtariJson.ParsedData.IsTilemap.HasValue)
                myMap.IsTilemap = AtariJson.ParsedData.IsTilemap.Value;
            if (AtariJson.ParsedData.TilemapInfo != null)
                myMap.TilemapInfo = AtariJson.ParsedData.TilemapInfo;
            if (AtariJson.ParsedData.BitmapTileset != null)
                myMap.BitmapTileset = AtariJson.ParsedData.BitmapTileset;
            if (AtariJson.ParsedData.ElementLibrary != null)
                myMap.ElementLibrary = AtariJson.ParsedData.ElementLibrary;
            if (AtariJson.ParsedData.ScreenLinks != null)
                myMap.ScreenLinks = AtariJson.ParsedData.ScreenLinks;
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.Export((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, (int)numericUpDown5.Value, saveFileDialog1.FileName);
                    int width = (int)((numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1) * myMap.ScreenSize.Width + numericUpDown5.Value);
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

        private void ExportScreens(int x1, int y1, int x2, int y2, string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);

            byte myData;
            for (int ymap = y1; ymap <= y2; ymap++)
                for (int xmap = x1; xmap <= x2; xmap++)
                {
                    int xs = xmap * myMap.ScreenSize.Width;
                    int ys = ymap * myMap.ScreenSize.Height;
                    int xf = (xmap + 1) * myMap.ScreenSize.Width;
                    int yf = (ymap + 1) * myMap.ScreenSize.Height;

                    for (int y = ys; y < yf; y++)
                        for (int x = xs; x < xf; x++)
                        {

                            if (x < xf)
                                myData = myMap.Data[x + y * myMap.Stride];
                            else
                                myData = 0;

                            fs.WriteByte(myData);
                        }
                }
            fs.Close();
            fs.Dispose();
        }

        private void ExportScreenByScreen()
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ExportScreens((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    int width = myMap.ScreenSize.Width;
                    MessageBox.Show("Export dataline width: " + width);
                    break;
            }
        }

        private void ImportScreenByScreen()
        {
            openFileDialog1.Filter = "Map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ImportScreens((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
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

        // this imports screens 1 by 1 from the ScreenFrom screen rows and columns
        private void ImportScreens(int xmap, int ymap, string filename)
        {
            /*
             * for (int ymap = y1; ymap <= y2; ymap++)
                for (int xmap = x1; xmap <= x2; xmap++)
                {
                    int xs = xmap * myMap.ScreenSize.Width;
                    int ys = ymap * myMap.ScreenSize.Height;
                    int xf = (xmap + 1) * myMap.ScreenSize.Width;
                    int yf = (ymap + 1) * myMap.ScreenSize.Height;
            */

            int screenSize = myMap.ScreenSize.Width * myMap.ScreenSize.Height;
            int importedScreens = 0;
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);

            while (fs.Position <= (fs.Length - screenSize))
            {

                byte myData;

                int xs = xmap * myMap.ScreenSize.Width;
                int ys = ymap * myMap.ScreenSize.Height;
                int xf = (xmap + 1) * myMap.ScreenSize.Width;
                int yf = (ymap + 1) * myMap.ScreenSize.Height;

                //Import single screen
                for (int y = ys; y < yf; y++)
                    for (int x = xs; x < xf; x++)
                    {
                        myData = (byte)fs.ReadByte();
                        myMap.Data[x + y * myMap.Stride] = myData;
                    }
                importedScreens++;

                //go to next screen
                xmap++;
                if (xmap == myMap.MapSize.Width)
                {
                    xmap = 0;
                    ymap++;
                }
                if (ymap == myMap.MapSize.Height)
                {
                    MessageBox.Show($"Reading aborted! Reached bottom edge of map.\nImported screens: {importedScreens}\nSkipped screens (beyond map bounds): {(fs.Length - importedScreens * screenSize) / screenSize}");
                    break;
                }
            }
            MessageBox.Show($"Imported screens: {importedScreens}\n");
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
                if (y == myMap.MapSize.Height * myMap.ScreenSize.Height)
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
                    LoadFontToSlot(openFileDialog1.FileName, 0);
                    // Also update the multifont checkbox state if needed
                    if (checkBoxMultiFont != null && !checkBoxMultiFont.Checked)
                    {
                        // When loading font in single font mode, ensure font 0 is set
                        // This is already done in LoadFontToSlot, but we make sure it's the active font
                    }
                    break;
            }
        }

        private void LoadFontToSlot(string fileName, int fontSlot)
        {
            byte[] fontData = new byte[1024 * 2];
            FileStream fs = new FileStream(fileName, FileMode.Open);
            fs.Read(fontData, 0, 1024);
            fs.Close();
            for (int a = 0; a < 1024; a++)
            {
                fontData[a + 1024] = (byte)(fontData[a] ^ 0x80);
            }

            myMap.SetFontData(fontData, fontSlot, fileName);
            AtariFontRenderer.ClearFontCache();

            // If loading to slot 0, also set it as the active screen font
            if (fontSlot == 0)
            {
                AtariFontRenderer.SetFontData(fontData, Globals.FontType.Screen);
            }

            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
            pictureBoxMap.Refresh();
            myCharPicker.Refresh();
        }

        private void CheckBoxMultiFont_CheckedChanged(object sender, EventArgs e)
        {
            if (myMap != null)
            {
                myMap.MultiFontEnabled = checkBoxMultiFont.Checked;
                AtariFontRenderer.ClearFontCache();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
                UpdateMultiFontUI();
                
                // Update DLI form to show/hide font column
                if (dliForm != null && dliForm.Visible)
                {
                    dliForm.ZoomResize();
                    dliForm.Show(dliForm.screenNumber); // Refresh the form with current screen
                }
            }
        }

        private void UpdateMultiFontUI()
        {
            bool enabled = checkBoxMultiFont != null && checkBoxMultiFont.Checked;
            // Enable/disable font template button based on multifont checkbox
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Button btn && btn.Text == "Font Templates")
                {
                    btn.Enabled = enabled;
                    break;
                }
            }
        }

        private void ButtonFontTemplate_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (FontTemplateDialog dialog = new FontTemplateDialog(myMap))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AtariFontRenderer.ClearFontCache();
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
            }
        }

        private void ButtonElementLibrary_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ElementLibraryDialog dialog = new ElementLibraryDialog(myMap))
            {
                dialog.ShowDialog();
            }
        }

        private void ButtonMapDescription_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (MapDescriptionDialog dialog = new MapDescriptionDialog(myMap))
            {
                dialog.ShowDialog();
            }
        }

        private void ButtonExportFont_Click(object sender, EventArgs e)
        {
            // Export the single font (font 0 or the active screen font)
            byte[] fontData = null;
            string defaultFileName = "font.fnt";

            // Try to get from font 0 first
            if (myMap != null && myMap.FontDataArray != null && myMap.FontDataArray[0] != null)
            {
                fontData = myMap.FontDataArray[0];
                if (myMap.FontFileNames != null && myMap.FontFileNames[0] != null)
                {
                    defaultFileName = System.IO.Path.GetFileName(myMap.FontFileNames[0]);
                }
            }
            // Otherwise get from AtariFontRenderer (the active screen font)
            else if (AtariFontRenderer.fonts.ContainsKey(Globals.FontType.Screen))
            {
                fontData = AtariFontRenderer.fonts[Globals.FontType.Screen].data;
                if (!string.IsNullOrEmpty(AtariFontRenderer.LastFontFile))
                {
                    defaultFileName = System.IO.Path.GetFileName(AtariFontRenderer.LastFontFile);
                }
            }

            if (fontData == null)
            {
                MessageBox.Show("No font data available to export.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Atari Font (*.fnt)|*.fnt";
            saveDialog.FileName = defaultFileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Export only first 1024 bytes (standard Atari font format)
                byte[] exportData = new byte[1024];
                Array.Copy(fontData, exportData, Math.Min(1024, fontData.Length));
                File.WriteAllBytes(saveDialog.FileName, exportData);
                MessageBox.Show("Font exported successfully.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    this.ExportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    break;
            }
        }

        private void DliExport()
        {
            saveFileDialog1.Filter = "Dli column export (*.dat)|*.dat";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create);

                byte myData;

                int charOffset = (int)numericUpDownScreenFromX.Value * myMap.ScreenSize.Width + (int)numericUpDownScreenFromY.Value * myMap.Stride * myMap.ScreenSize.Height;
                for (int x = 0; x < 5; x++)
                {
                    if (maskedTextBoxDli.Text[x] == '0') continue;    //skip 0 masks
                    for (int y = 0; y < myMap.ScreenSize.Height; y++)
                    {
                        byte[] color5 = myMap.GetDliColor5(charOffset + y * myMap.Stride);
                        myData = color5[x];
                        fs.WriteByte(myData);
                    }
                }
                fs.Close();
                fs.Dispose();
            }
        }

        private void DliImport()
        {
            openFileDialog1.Filter = "Dli column export (*.dat)|*.dat";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open);
                
                for (int x = 0; x < 5; x++)
                {
                    if (maskedTextBoxDli.Text[x] == '0') continue;    //skip 0 masks
                    for (int y = 0; y < myMap.ScreenSize.Height; y++)
                    {
                        myMap.SetDliColor((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, y, x, (byte)fs.ReadByte());
                    }
                }
                fs.Close();
                fs.Dispose();
            }
        }

        private void ButtonHoboImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Column based map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ImportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, openFileDialog1.FileName);
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
                    this.Import((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDown6.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void BtnNewMap_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Create new map? (current mapdata will be deleted!)", "New map", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                AtariFontRenderer.SetAlpa(checkBoxAlpa.Checked);
                myMap = new AtariMap(new Size((int)nudMapW.Value, (int)nudMapH.Value), new Size((int)nudScreenW.Value, (int)nudScreenH.Value));
                AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                numericUpDownScreenFromX.Maximum = nudMapW.Value - 1;
                numericUpDownScreenToX.Maximum = nudMapW.Value - 1;
                numericUpDownScreenFromY.Maximum = nudMapH.Value - 1;
                numericUpDownScreenToY.Maximum = nudMapH.Value - 1;

                RedrawEditorWindow();
                dliForm.Dispose();
                dliForm = new DliForm(myMap, pictureBoxMap);
                dliForm.RenderData();
            }

        }

        private void TrackBarZoom_Scroll(object sender, EventArgs e)
        {
            Globals.Zoom = trackBarZoom.Value;
            PictureBoxMap_ClientSizeChanged(null, null);
            myCharPicker.SetZoom();
            dliForm.ZoomResize();
            dliForm.Hide();
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
            if (!ScreenSelectionShown)  //refresh only when selection is not supposed to be drawn (prevents flickering)
                pictureBoxMap.Refresh();
            ScreenSelectionShown = false;
        }

        private void ComboBoxOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import","Export DLI"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = true;
                    numericUpDown5.Enabled = true;
                    numericUpDown6.Enabled = false;
                    break;
                case 1:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = true;
                    break;
                case 2:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = true;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 3:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 4:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 5:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
            }
            maskedTextBoxDli.Visible = labelDliMask.Visible = comboOperation.SelectedIndex == 4;

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
                case 4:
                    DliExport();
                    break;
                case 5:
                    DliImport();
                    break;
                case 6:
                    ExportScreenByScreen();
                    break;
                case 7:
                    ImportScreenByScreen();
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

        private void CheckBoxEditDli_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEditDli.Checked)
            {
                checkBoxShowDli.Checked = true;
            }
            else
            {
                dliForm.Hide();
            }
            AtariFontRenderer.UseDli = checkBoxShowDli.Checked;
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxMap.Refresh();

        }

        private void CheckBoxShowDli_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxShowDli.Checked) checkBoxEditDli.Checked = false;
            AtariFontRenderer.UseDli = checkBoxShowDli.Checked;
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxMap.Refresh();
        }

        private void ButtonReplaceCurrentScreen_Click(object sender, EventArgs e)
        {
            if (numericUpDownReplace1.Value != numericUpDownReplace2.Value)
            {
                myMap.SwapChar((byte)numericUpDownReplace1.Value, (byte)numericUpDownReplace2.Value, radioButtonWholeMap.Checked, new Point(currentScreen.X,currentScreen.Y));
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void ButtonAddScreenRowToMap_Click(object sender, EventArgs e)
        {
            AtariMap newMap = new AtariMap(new Size(myMap.MapSize.Width, myMap.MapSize.Height + 1), myMap.ScreenSize);
            Array.Copy(myMap.Data, newMap.Data, myMap.Data.Length);
            newMap.InitDliColorFullMap();
            Array.Copy(myMap.ColorData, newMap.ColorData, myMap.ColorData.Length);
            myMap = newMap;
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            dliForm.Dispose();
            dliForm = new DliForm(myMap, pictureBoxMap);
            dliForm.RenderData();
            RedrawEditorWindow();
            //extend export selection option with additional screen row
            numericUpDownScreenFromY.Maximum++;
            numericUpDownScreenToY.Maximum++;
        }

        private void ToolStripMenuItemClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Clear Screen {currentScreen.X}:{currentScreen.Y}?", "Confirmation popup", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                myMap.ClearScreen(currentScreen);
                RedrawEditorWindow();
            }
        }

        private void ToolStripMenuItemHFlip_Click(object sender, EventArgs e)
        {
            myMap.FlipScreen(currentScreen, true);
            RedrawEditorWindow();
        }

        private void ToolStripMenuItemVFlip_Click(object sender, EventArgs e)
        {
            myMap.FlipScreen(currentScreen, false);
            RedrawEditorWindow();
        }

        private void ShowScreenSelection(bool redraw = true)
        {
            if (redraw)
                RedrawEditorWindow();
            int left = (int)numericUpDownScreenFromX.Value * myMap.ScreenSize.Width;
            int top = (int)numericUpDownScreenFromY.Value * myMap.ScreenSize.Height;
            int width = (int)(numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1) * myMap.ScreenSize.Width * Globals.CharSize;
            int height = (int)(numericUpDownScreenToY.Value - numericUpDownScreenFromY.Value + 1) * myMap.ScreenSize.Height * Globals.CharSize;
            Graphics g = Graphics.FromImage(pictureBoxMap.Image);
            Brush b = new HatchBrush(HatchStyle.Percent80, Color.FromArgb(96, Color.GreenYellow));
            g.FillRectangle(b, (left - myMap.OffsetX) * Globals.CharSize, (top - myMap.OffsetY) * Globals.CharSize, width, height);
            pictureBoxMap.Refresh();
            this.ScreenSelectionShown = true;
        }

        private void CheckBoxShowScreenSelection_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowScreenSelection.Checked)
            {
                ShowScreenSelection();
            }
            else
            {
                ScreenSelectionShown = false;
                RedrawEditorWindow();
            }
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2 && checkBoxShowScreenSelection.Checked && !ScreenSelectionShown)
            {
                ShowScreenSelection();
            } 
            if (tabControl1.SelectedTab != tabPage2 && ScreenSelectionShown)
            {
                ScreenSelectionShown = false;
                RedrawEditorWindow();
            }
        }

        private void NumericUpDownScreenSelection_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxShowScreenSelection.Checked)
            {
                ShowScreenSelection();
            }
        }

        private void labelPosition_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            flowLayoutPanel1.Refresh();
            flowLayoutPanel1.Invalidate();
        }
    }
}