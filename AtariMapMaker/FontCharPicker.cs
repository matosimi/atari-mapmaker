using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class FontCharPicker : Form
    {
        private AtariMap fontPickerMap;
        private readonly PictureBox clipboardPictureBox;
        private AtariMap mainMap;
        private Point colorSourceScreen = new Point(0, 0);
        public const Globals.WindowType window = Globals.WindowType.CharPicker;

        private static int savedLayoutIndex = 0;
        private CheckBox checkBoxUseScreenColors;
        private Label labelDliLine;
        private ComboBox comboBoxDliLine;

        public FontCharPicker(PictureBox clipboardPictureBox, AtariMap mainMap = null)
        {
            this.clipboardPictureBox = clipboardPictureBox;
            this.mainMap = mainMap;
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            BuildScreenColorControls();
            fontPickerMap = CreatePickerMap(16, 16);
            pictureBoxFontPicker.Image = new Bitmap(16 * Globals.CharSize, 16 * Globals.CharSize);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
        }

        private void BuildScreenColorControls()
        {
            checkBoxUseScreenColors = new CheckBox
            {
                Text = "Screen colors",
                AutoSize = true,
                Margin = new Padding(8, 6, 3, 3)
            };
            checkBoxUseScreenColors.CheckedChanged += (s, e) =>
            {
                UpdateDliLineComboEnabled();
                ApplyColorOverrideAndRedraw();
            };

            labelDliLine = new Label
            {
                Text = "DLI line:",
                AutoSize = true,
                Margin = new Padding(8, 8, 0, 3),
                TextAlign = ContentAlignment.MiddleLeft
            };

            comboBoxDliLine = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 70,
                Margin = new Padding(3, 3, 3, 3)
            };
            comboBoxDliLine.SelectedIndexChanged += (s, e) => ApplyColorOverrideAndRedraw();

            flowLayoutPanelSelection.Controls.Add(checkBoxUseScreenColors);
            flowLayoutPanelSelection.Controls.Add(labelDliLine);
            flowLayoutPanelSelection.Controls.Add(comboBoxDliLine);
            // Keep picture box last
            flowLayoutPanelSelection.Controls.SetChildIndex(pictureBoxFontPicker, flowLayoutPanelSelection.Controls.Count - 1);
            UpdateDliLineComboEnabled();
        }

        public void SetMainMap(AtariMap map)
        {
            mainMap = map;
            RefreshFontCombo();
            RefreshDliLineCombo();
            ApplyColorOverrideAndRedraw();
        }

        public void NotifyScreenChanged(Point screen)
        {
            colorSourceScreen = screen;
            if (checkBoxUseScreenColors != null && checkBoxUseScreenColors.Checked)
                ApplyColorOverrideAndRedraw();
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
            ApplyColorOverrideAndRedraw();
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
            RefreshDliLineCombo();
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

        private void RefreshDliLineCombo()
        {
            if (comboBoxDliLine == null) return;
            int prev = comboBoxDliLine.SelectedIndex;
            comboBoxDliLine.Items.Clear();
            int lines = 25;
            if (mainMap != null)
            {
                lines = mainMap.ScreenSize.Height;
                if (mainMap.IsTilemap && mainMap.TilemapInfo != null && mainMap.TilemapInfo.TileHeight > 0)
                    lines = mainMap.ScreenSize.Height * mainMap.TilemapInfo.TileHeight;
            }
            for (int i = 0; i < lines; i++)
                comboBoxDliLine.Items.Add(i.ToString());
            if (comboBoxDliLine.Items.Count > 0)
                comboBoxDliLine.SelectedIndex = Math.Max(0, Math.Min(prev, comboBoxDliLine.Items.Count - 1));
            UpdateDliLineComboEnabled();
        }

        private void UpdateDliLineComboEnabled()
        {
            bool on = checkBoxUseScreenColors != null && checkBoxUseScreenColors.Checked;
            if (comboBoxDliLine != null) comboBoxDliLine.Enabled = on;
            if (labelDliLine != null) labelDliLine.Enabled = on;
        }

        private void ApplyColorOverrideAndRedraw()
        {
            if (checkBoxUseScreenColors != null && checkBoxUseScreenColors.Checked && mainMap != null)
            {
                int line = comboBoxDliLine != null && comboBoxDliLine.SelectedIndex >= 0 ? comboBoxDliLine.SelectedIndex : 0;
                byte[] lineColors = GetScreenLineColors(colorSourceScreen.X, colorSourceScreen.Y, line);
                AtariFontRenderer.CharPickerColorOverride = lineColors;
            }
            else
            {
                AtariFontRenderer.CharPickerColorOverride = null;
            }
            RedrawFontWindow();
        }

        private byte[] GetScreenLineColors(int screenX, int screenY, int line)
        {
            int screenCharHeight = mainMap.ScreenSize.Height;
            if (mainMap.IsTilemap && mainMap.TilemapInfo != null && mainMap.TilemapInfo.TileHeight > 0)
                screenCharHeight = mainMap.ScreenSize.Height * mainMap.TilemapInfo.TileHeight;
            if (line < 0) line = 0;
            if (line >= screenCharHeight) line = screenCharHeight - 1;

            int screenCharWidth = mainMap.ScreenSize.Width;
            if (mainMap.IsTilemap && mainMap.TilemapInfo != null && mainMap.TilemapInfo.TileWidth > 0)
                screenCharWidth = mainMap.ScreenSize.Width * mainMap.TilemapInfo.TileWidth;

            int stride = mainMap.IsTilemap ? mainMap.CharStride : mainMap.Stride;
            int charOffset = screenY * screenCharHeight * stride + screenX * screenCharWidth + line * stride;
            byte[] colors = mainMap.GetDliColor5(charOffset);
            if (colors[0] == Globals.DEFAULT_COLOR)
            {
                byte[] g = new byte[AtariMap.DliColorsPerLine];
                int n = Math.Min(AtariFontRenderer.Color5.Length, g.Length);
                Array.Copy(AtariFontRenderer.Color5, g, n);
                if (AtariFontRenderer.Color5.Length > 5 && AtariFontRenderer.Color5.Length < 9 && g.Length > 8)
                    g[8] = AtariFontRenderer.Color5.Length > 1 ? AtariFontRenderer.Color5[1] : g[1];
                return g;
            }
            return colors;
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
            ApplyColorOverrideAndRedraw();
        }

        private void PictureBoxFontPicker_MouseDown(object sender, MouseEventArgs e)
        {
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
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                AtariFontRenderer.CharPickerColorOverride = null;
                this.Hide();
                e.Cancel = true;
            }
        }

        private void FontCharPicker_Shown(object sender, EventArgs e)
        {
        }

        public void RedrawFontWindow()
        {
            FontCharPicker_VisibleChanged(null, null);
        }

        private void FontCharPicker_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                AtariFontRenderer.CharPickerColorOverride = null;
                return;
            }
            pictureBoxFontPicker.Image = new Bitmap(pictureBoxFontPicker.Width, pictureBoxFontPicker.Height);
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxFontPicker.Image, fontPickerMap);
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker, true, false, true);
        }
    }
}
