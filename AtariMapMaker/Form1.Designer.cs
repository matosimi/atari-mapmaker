namespace AtariMapMaker
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxMap = new System.Windows.Forms.PictureBox();
            this.labelCharOccurence = new System.Windows.Forms.Label();
            this.labelChar = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.labelScreen = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageColors = new System.Windows.Forms.TabPage();
            this.checkBoxShowDli = new System.Windows.Forms.CheckBox();
            this.checkBoxEditDli = new System.Windows.Forms.CheckBox();
            this.buttonRefreshFont = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pictureBoxClipboard = new System.Windows.Forms.PictureBox();
            this.buttonLoadFont = new System.Windows.Forms.Button();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.listViewColors = new System.Windows.Forms.ListView();
            this.groupBoxZoom = new System.Windows.Forms.GroupBox();
            this.comboBoxDrawBorders = new System.Windows.Forms.CheckBox();
            this.comboBoxDrawGrid = new System.Windows.Forms.CheckBox();
            this.trackBarZoom = new System.Windows.Forms.TrackBar();
            this.buttonShowFont = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonImport = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowScreenSelection = new System.Windows.Forms.CheckBox();
            this.labelDliMask = new System.Windows.Forms.Label();
            this.maskedTextBoxDli = new System.Windows.Forms.MaskedTextBox();
            this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
            this.lblDataWidth = new System.Windows.Forms.Label();
            this.buttonPerform = new System.Windows.Forms.Button();
            this.comboOperation = new System.Windows.Forms.ComboBox();
            this.lblScrToXY = new System.Windows.Forms.Label();
            this.lblScrFromXY = new System.Windows.Forms.Label();
            this.numericUpDownScreenFromX = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownScreenFromY = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownScreenToX = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownScreenToY = new System.Windows.Forms.NumericUpDown();
            this.groupBoxMap = new System.Windows.Forms.GroupBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonHoboImport = new System.Windows.Forms.Button();
            this.buttonShiftChars = new System.Windows.Forms.Button();
            this.buttonHoboExport = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.checkBoxAlpa = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.labelAbout2 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.labelAbout = new System.Windows.Forms.Label();
            this.nudMapH = new System.Windows.Forms.NumericUpDown();
            this.nudMapW = new System.Windows.Forms.NumericUpDown();
            this.lblMapSize = new System.Windows.Forms.Label();
            this.nudScreenH = new System.Windows.Forms.NumericUpDown();
            this.lblScreenSize = new System.Windows.Forms.Label();
            this.nudScreenW = new System.Windows.Forms.NumericUpDown();
            this.buttonNewMap = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.buttonAddScreenRowToMap = new System.Windows.Forms.Button();
            this.radioButtonWholeMap = new System.Windows.Forms.RadioButton();
            this.radioButtonCurrentScreen = new System.Windows.Forms.RadioButton();
            this.buttonReplaceCurrentScreen = new System.Windows.Forms.Button();
            this.numericUpDownReplace2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownReplace1 = new System.Windows.Forms.NumericUpDown();
            this.labelReplace = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStripScreen = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemClear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemHFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemVFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.groupBoxDli = new System.Windows.Forms.GroupBox();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMap)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageColors.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClipboard)).BeginInit();
            this.groupBoxColors.SuspendLayout();
            this.groupBoxZoom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenFromX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenFromY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenToX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenToY)).BeginInit();
            this.groupBoxMap.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReplace2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReplace1)).BeginInit();
            this.contextMenuStripScreen.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            this.groupBoxDli.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBoxMap);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(857, 894);
            this.splitContainer1.SplitterDistance = 429;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer1_SplitterMoving);
            // 
            // pictureBoxMap
            // 
            this.pictureBoxMap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBoxMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMap.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxMap.Name = "pictureBoxMap";
            this.pictureBoxMap.Size = new System.Drawing.Size(429, 894);
            this.pictureBoxMap.TabIndex = 0;
            this.pictureBoxMap.TabStop = false;
            this.pictureBoxMap.ClientSizeChanged += new System.EventHandler(this.PictureBoxMap_ClientSizeChanged);
            this.pictureBoxMap.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMap_MouseDoubleClick);
            this.pictureBoxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMap_MouseDown);
            this.pictureBoxMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMap_MouseMove);
            this.pictureBoxMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMap_MouseUp);
            this.pictureBoxMap.Resize += new System.EventHandler(this.PictureBoxMap_Resize);
            // 
            // labelCharOccurence
            // 
            this.labelCharOccurence.AutoSize = true;
            this.labelCharOccurence.Location = new System.Drawing.Point(205, 34);
            this.labelCharOccurence.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCharOccurence.Name = "labelCharOccurence";
            this.labelCharOccurence.Size = new System.Drawing.Size(49, 20);
            this.labelCharOccurence.TabIndex = 4;
            this.labelCharOccurence.Text = "1 of 1";
            // 
            // labelChar
            // 
            this.labelChar.AutoSize = true;
            this.labelChar.Location = new System.Drawing.Point(202, 14);
            this.labelChar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelChar.Name = "labelChar";
            this.labelChar.Size = new System.Drawing.Size(97, 20);
            this.labelChar.TabIndex = 3;
            this.labelChar.Text = "Char $00 (0)";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(5, 43);
            this.labelPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(91, 20);
            this.labelPosition.TabIndex = 2;
            this.labelPosition.Text = "Position 0:0";
            this.labelPosition.Click += new System.EventHandler(this.labelPosition_Click);
            // 
            // labelScreen
            // 
            this.labelScreen.AutoSize = true;
            this.labelScreen.Location = new System.Drawing.Point(5, 14);
            this.labelScreen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelScreen.Name = "labelScreen";
            this.labelScreen.Size = new System.Drawing.Size(86, 20);
            this.labelScreen.TabIndex = 1;
            this.labelScreen.Text = "Screen 0:0";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageColors);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(422, 894);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1_SelectedIndexChanged);
            // 
            // tabPageColors
            // 
            this.tabPageColors.Controls.Add(this.flowLayoutPanel1);
            this.tabPageColors.Location = new System.Drawing.Point(4, 54);
            this.tabPageColors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageColors.Name = "tabPageColors";
            this.tabPageColors.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageColors.Size = new System.Drawing.Size(414, 836);
            this.tabPageColors.TabIndex = 0;
            this.tabPageColors.Text = "Colors";
            this.tabPageColors.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowDli
            // 
            this.checkBoxShowDli.AutoSize = true;
            this.checkBoxShowDli.Location = new System.Drawing.Point(40, 19);
            this.checkBoxShowDli.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxShowDli.Name = "checkBoxShowDli";
            this.checkBoxShowDli.Size = new System.Drawing.Size(105, 24);
            this.checkBoxShowDli.TabIndex = 12;
            this.checkBoxShowDli.Text = "Show DLI";
            this.checkBoxShowDli.UseVisualStyleBackColor = true;
            this.checkBoxShowDli.CheckedChanged += new System.EventHandler(this.CheckBoxShowDli_CheckedChanged);
            // 
            // checkBoxEditDli
            // 
            this.checkBoxEditDli.AutoSize = true;
            this.checkBoxEditDli.Location = new System.Drawing.Point(185, 19);
            this.checkBoxEditDli.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxEditDli.Name = "checkBoxEditDli";
            this.checkBoxEditDli.Size = new System.Drawing.Size(93, 24);
            this.checkBoxEditDli.TabIndex = 3;
            this.checkBoxEditDli.Text = "Edit DLI";
            this.checkBoxEditDli.UseVisualStyleBackColor = true;
            this.checkBoxEditDli.CheckedChanged += new System.EventHandler(this.CheckBoxEditDli_CheckedChanged);
            // 
            // buttonRefreshFont
            // 
            this.buttonRefreshFont.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonRefreshFont.Location = new System.Drawing.Point(173, 22);
            this.buttonRefreshFont.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonRefreshFont.Name = "buttonRefreshFont";
            this.buttonRefreshFont.Size = new System.Drawing.Size(39, 35);
            this.buttonRefreshFont.TabIndex = 11;
            this.buttonRefreshFont.Text = "P";
            this.buttonRefreshFont.UseVisualStyleBackColor = true;
            this.buttonRefreshFont.Click += new System.EventHandler(this.ButtonRefreshFont_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pictureBoxClipboard);
            this.groupBox4.Location = new System.Drawing.Point(4, 380);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(209, 211);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Clipboard";
            // 
            // pictureBoxClipboard
            // 
            this.pictureBoxClipboard.Location = new System.Drawing.Point(16, 29);
            this.pictureBoxClipboard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxClipboard.Name = "pictureBoxClipboard";
            this.pictureBoxClipboard.Size = new System.Drawing.Size(178, 172);
            this.pictureBoxClipboard.TabIndex = 6;
            this.pictureBoxClipboard.TabStop = false;
            // 
            // buttonLoadFont
            // 
            this.buttonLoadFont.Location = new System.Drawing.Point(220, 23);
            this.buttonLoadFont.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonLoadFont.Name = "buttonLoadFont";
            this.buttonLoadFont.Size = new System.Drawing.Size(112, 35);
            this.buttonLoadFont.TabIndex = 9;
            this.buttonLoadFont.Text = "Load Font";
            this.buttonLoadFont.UseVisualStyleBackColor = true;
            this.buttonLoadFont.Click += new System.EventHandler(this.ButtonLoadFont_Click);
            // 
            // groupBoxColors
            // 
            this.groupBoxColors.Controls.Add(this.listViewColors);
            this.groupBoxColors.Location = new System.Drawing.Point(4, 98);
            this.groupBoxColors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColors.Name = "groupBoxColors";
            this.groupBoxColors.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColors.Size = new System.Drawing.Size(397, 272);
            this.groupBoxColors.TabIndex = 5;
            this.groupBoxColors.TabStop = false;
            this.groupBoxColors.Text = "Colors";
            // 
            // listViewColors
            // 
            this.listViewColors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewColors.FullRowSelect = true;
            this.listViewColors.GridLines = true;
            this.listViewColors.HideSelection = false;
            this.listViewColors.Location = new System.Drawing.Point(4, 24);
            this.listViewColors.Margin = new System.Windows.Forms.Padding(0, 5, 4, 5);
            this.listViewColors.MinimumSize = new System.Drawing.Size(100, 100);
            this.listViewColors.MultiSelect = false;
            this.listViewColors.Name = "listViewColors";
            this.listViewColors.Size = new System.Drawing.Size(389, 243);
            this.listViewColors.TabIndex = 1;
            this.listViewColors.UseCompatibleStateImageBehavior = false;
            this.listViewColors.View = System.Windows.Forms.View.Details;
            this.listViewColors.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseDoubleClick);
            this.listViewColors.MouseLeave += new System.EventHandler(this.ListView1_MouseLeave);
            // 
            // groupBoxZoom
            // 
            this.groupBoxZoom.Controls.Add(this.comboBoxDrawBorders);
            this.groupBoxZoom.Controls.Add(this.comboBoxDrawGrid);
            this.groupBoxZoom.Controls.Add(this.trackBarZoom);
            this.groupBoxZoom.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxZoom.Location = new System.Drawing.Point(4, 5);
            this.groupBoxZoom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxZoom.Name = "groupBoxZoom";
            this.groupBoxZoom.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxZoom.Size = new System.Drawing.Size(324, 83);
            this.groupBoxZoom.TabIndex = 4;
            this.groupBoxZoom.TabStop = false;
            this.groupBoxZoom.Text = "Zoom";
            // 
            // comboBoxDrawBorders
            // 
            this.comboBoxDrawBorders.AutoSize = true;
            this.comboBoxDrawBorders.Checked = true;
            this.comboBoxDrawBorders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.comboBoxDrawBorders.Location = new System.Drawing.Point(164, 48);
            this.comboBoxDrawBorders.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxDrawBorders.Name = "comboBoxDrawBorders";
            this.comboBoxDrawBorders.Size = new System.Drawing.Size(144, 24);
            this.comboBoxDrawBorders.TabIndex = 2;
            this.comboBoxDrawBorders.Text = "Screen borders";
            this.comboBoxDrawBorders.UseVisualStyleBackColor = true;
            this.comboBoxDrawBorders.CheckedChanged += new System.EventHandler(this.ComboBoxDrawBorders_CheckedChanged);
            // 
            // comboBoxDrawGrid
            // 
            this.comboBoxDrawGrid.AutoSize = true;
            this.comboBoxDrawGrid.Checked = true;
            this.comboBoxDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.comboBoxDrawGrid.Location = new System.Drawing.Point(164, 20);
            this.comboBoxDrawGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxDrawGrid.Name = "comboBoxDrawGrid";
            this.comboBoxDrawGrid.Size = new System.Drawing.Size(65, 24);
            this.comboBoxDrawGrid.TabIndex = 1;
            this.comboBoxDrawGrid.Text = "Grid";
            this.comboBoxDrawGrid.UseVisualStyleBackColor = true;
            this.comboBoxDrawGrid.CheckedChanged += new System.EventHandler(this.ComboBoxDrawGrid_CheckedChanged);
            // 
            // trackBarZoom
            // 
            this.trackBarZoom.AutoSize = false;
            this.trackBarZoom.LargeChange = 1;
            this.trackBarZoom.Location = new System.Drawing.Point(18, 20);
            this.trackBarZoom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarZoom.Maximum = 3;
            this.trackBarZoom.Name = "trackBarZoom";
            this.trackBarZoom.Size = new System.Drawing.Size(136, 54);
            this.trackBarZoom.TabIndex = 0;
            this.trackBarZoom.Value = 1;
            this.trackBarZoom.Scroll += new System.EventHandler(this.TrackBarZoom_Scroll);
            // 
            // buttonShowFont
            // 
            this.buttonShowFont.Location = new System.Drawing.Point(7, 22);
            this.buttonShowFont.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonShowFont.Name = "buttonShowFont";
            this.buttonShowFont.Size = new System.Drawing.Size(112, 35);
            this.buttonShowFont.TabIndex = 3;
            this.buttonShowFont.Text = "Show font";
            this.buttonShowFont.UseVisualStyleBackColor = true;
            this.buttonShowFont.Click += new System.EventHandler(this.ButtonShowFont_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonImport);
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Controls.Add(this.groupBoxMap);
            this.tabPage2.Controls.Add(this.buttonExport);
            this.tabPage2.Controls.Add(this.buttonHoboImport);
            this.tabPage2.Controls.Add(this.buttonShiftChars);
            this.tabPage2.Controls.Add(this.buttonHoboExport);
            this.tabPage2.Location = new System.Drawing.Point(4, 54);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Size = new System.Drawing.Size(414, 836);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Load/Save/Export";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(174, 535);
            this.buttonImport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(112, 35);
            this.buttonImport.TabIndex = 19;
            this.buttonImport.Text = "Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Visible = false;
            this.buttonImport.Click += new System.EventHandler(this.ButtonImport_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBoxShowScreenSelection);
            this.groupBox5.Controls.Add(this.labelDliMask);
            this.groupBox5.Controls.Add(this.maskedTextBoxDli);
            this.groupBox5.Controls.Add(this.numericUpDown6);
            this.groupBox5.Controls.Add(this.lblDataWidth);
            this.groupBox5.Controls.Add(this.buttonPerform);
            this.groupBox5.Controls.Add(this.comboOperation);
            this.groupBox5.Controls.Add(this.lblScrToXY);
            this.groupBox5.Controls.Add(this.lblScrFromXY);
            this.groupBox5.Controls.Add(this.numericUpDownScreenFromX);
            this.groupBox5.Controls.Add(this.numericUpDownScreenFromY);
            this.groupBox5.Controls.Add(this.numericUpDown5);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.numericUpDownScreenToX);
            this.groupBox5.Controls.Add(this.numericUpDownScreenToY);
            this.groupBox5.Location = new System.Drawing.Point(9, 115);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Size = new System.Drawing.Size(318, 411);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Data operations";
            // 
            // checkBoxShowScreenSelection
            // 
            this.checkBoxShowScreenSelection.AutoSize = true;
            this.checkBoxShowScreenSelection.Checked = true;
            this.checkBoxShowScreenSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowScreenSelection.Location = new System.Drawing.Point(9, 67);
            this.checkBoxShowScreenSelection.Name = "checkBoxShowScreenSelection";
            this.checkBoxShowScreenSelection.Size = new System.Drawing.Size(200, 24);
            this.checkBoxShowScreenSelection.TabIndex = 23;
            this.checkBoxShowScreenSelection.Text = "Show Screen Selection";
            this.checkBoxShowScreenSelection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxShowScreenSelection.UseVisualStyleBackColor = true;
            this.checkBoxShowScreenSelection.CheckedChanged += new System.EventHandler(this.CheckBoxShowScreenSelection_CheckedChanged);
            // 
            // labelDliMask
            // 
            this.labelDliMask.Location = new System.Drawing.Point(4, 335);
            this.labelDliMask.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDliMask.Name = "labelDliMask";
            this.labelDliMask.Size = new System.Drawing.Size(80, 26);
            this.labelDliMask.TabIndex = 22;
            this.labelDliMask.Text = "DLI mask";
            // 
            // maskedTextBoxDli
            // 
            this.maskedTextBoxDli.Location = new System.Drawing.Point(196, 331);
            this.maskedTextBoxDli.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.maskedTextBoxDli.Mask = "00000";
            this.maskedTextBoxDli.Name = "maskedTextBoxDli";
            this.maskedTextBoxDli.Size = new System.Drawing.Size(102, 26);
            this.maskedTextBoxDli.TabIndex = 21;
            this.maskedTextBoxDli.Text = "01110";
            this.maskedTextBoxDli.ValidatingType = typeof(int);
            // 
            // numericUpDown6
            // 
            this.numericUpDown6.Location = new System.Drawing.Point(196, 283);
            this.numericUpDown6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDown6.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.numericUpDown6.Name = "numericUpDown6";
            this.numericUpDown6.Size = new System.Drawing.Size(104, 26);
            this.numericUpDown6.TabIndex = 20;
            this.numericUpDown6.Value = new decimal(new int[] {
            106,
            0,
            0,
            0});
            // 
            // lblDataWidth
            // 
            this.lblDataWidth.Location = new System.Drawing.Point(4, 286);
            this.lblDataWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDataWidth.Name = "lblDataWidth";
            this.lblDataWidth.Size = new System.Drawing.Size(195, 28);
            this.lblDataWidth.TabIndex = 19;
            this.lblDataWidth.Text = "Dataline width";
            // 
            // buttonPerform
            // 
            this.buttonPerform.Location = new System.Drawing.Point(9, 366);
            this.buttonPerform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonPerform.Name = "buttonPerform";
            this.buttonPerform.Size = new System.Drawing.Size(300, 35);
            this.buttonPerform.TabIndex = 18;
            this.buttonPerform.Text = "Perform";
            this.buttonPerform.UseVisualStyleBackColor = true;
            this.buttonPerform.Click += new System.EventHandler(this.ButtonPerform_Click);
            // 
            // comboOperation
            // 
            this.comboOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOperation.FormattingEnabled = true;
            this.comboOperation.Location = new System.Drawing.Point(9, 31);
            this.comboOperation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboOperation.Name = "comboOperation";
            this.comboOperation.Size = new System.Drawing.Size(298, 28);
            this.comboOperation.TabIndex = 15;
            this.comboOperation.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOperation_SelectedIndexChanged);
            // 
            // lblScrToXY
            // 
            this.lblScrToXY.Location = new System.Drawing.Point(4, 171);
            this.lblScrToXY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScrToXY.Name = "lblScrToXY";
            this.lblScrToXY.Size = new System.Drawing.Size(150, 23);
            this.lblScrToXY.TabIndex = 17;
            this.lblScrToXY.Text = "Screen to X,Y";
            // 
            // lblScrFromXY
            // 
            this.lblScrFromXY.Location = new System.Drawing.Point(4, 94);
            this.lblScrFromXY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScrFromXY.Name = "lblScrFromXY";
            this.lblScrFromXY.Size = new System.Drawing.Size(150, 22);
            this.lblScrFromXY.TabIndex = 9;
            this.lblScrFromXY.Text = "Screen from X,Y";
            // 
            // numericUpDownScreenFromX
            // 
            this.numericUpDownScreenFromX.Location = new System.Drawing.Point(9, 120);
            this.numericUpDownScreenFromX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownScreenFromX.Name = "numericUpDownScreenFromX";
            this.numericUpDownScreenFromX.Size = new System.Drawing.Size(104, 26);
            this.numericUpDownScreenFromX.TabIndex = 3;
            this.numericUpDownScreenFromX.ValueChanged += new System.EventHandler(this.NumericUpDownScreenSelection_ValueChanged);
            // 
            // numericUpDownScreenFromY
            // 
            this.numericUpDownScreenFromY.Location = new System.Drawing.Point(196, 120);
            this.numericUpDownScreenFromY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownScreenFromY.Name = "numericUpDownScreenFromY";
            this.numericUpDownScreenFromY.Size = new System.Drawing.Size(104, 26);
            this.numericUpDownScreenFromY.TabIndex = 5;
            this.numericUpDownScreenFromY.ValueChanged += new System.EventHandler(this.NumericUpDownScreenSelection_ValueChanged);
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(196, 251);
            this.numericUpDown5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(104, 26);
            this.numericUpDown5.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(4, 254);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(195, 28);
            this.label3.TabIndex = 11;
            this.label3.Text = "Extra chars in line (suffix)\r\n";
            // 
            // numericUpDownScreenToX
            // 
            this.numericUpDownScreenToX.Location = new System.Drawing.Point(9, 198);
            this.numericUpDownScreenToX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownScreenToX.Name = "numericUpDownScreenToX";
            this.numericUpDownScreenToX.Size = new System.Drawing.Size(104, 26);
            this.numericUpDownScreenToX.TabIndex = 4;
            this.numericUpDownScreenToX.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownScreenToX.ValueChanged += new System.EventHandler(this.NumericUpDownScreenSelection_ValueChanged);
            // 
            // numericUpDownScreenToY
            // 
            this.numericUpDownScreenToY.Location = new System.Drawing.Point(196, 198);
            this.numericUpDownScreenToY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownScreenToY.Name = "numericUpDownScreenToY";
            this.numericUpDownScreenToY.Size = new System.Drawing.Size(104, 26);
            this.numericUpDownScreenToY.TabIndex = 6;
            this.numericUpDownScreenToY.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownScreenToY.ValueChanged += new System.EventHandler(this.NumericUpDownScreenSelection_ValueChanged);
            // 
            // groupBoxMap
            // 
            this.groupBoxMap.Controls.Add(this.buttonLoad);
            this.groupBoxMap.Controls.Add(this.buttonSave);
            this.groupBoxMap.Location = new System.Drawing.Point(9, 9);
            this.groupBoxMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxMap.Name = "groupBoxMap";
            this.groupBoxMap.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxMap.Size = new System.Drawing.Size(318, 97);
            this.groupBoxMap.TabIndex = 16;
            this.groupBoxMap.TabStop = false;
            this.groupBoxMap.Text = "Map";
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(9, 29);
            this.buttonLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(112, 45);
            this.buttonLoad.TabIndex = 0;
            this.buttonLoad.Text = "Load Map";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(196, 29);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(112, 45);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save Map";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(174, 580);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(112, 35);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Visible = false;
            this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);
            // 
            // buttonHoboImport
            // 
            this.buttonHoboImport.Location = new System.Drawing.Point(174, 668);
            this.buttonHoboImport.Name = "buttonHoboImport";
            this.buttonHoboImport.Size = new System.Drawing.Size(158, 35);
            this.buttonHoboImport.TabIndex = 14;
            this.buttonHoboImport.Text = "Column Import";
            this.buttonHoboImport.UseVisualStyleBackColor = true;
            this.buttonHoboImport.Visible = false;
            this.buttonHoboImport.Click += new System.EventHandler(this.ButtonHoboImport_Click);
            // 
            // buttonShiftChars
            // 
            this.buttonShiftChars.Location = new System.Drawing.Point(12, 623);
            this.buttonShiftChars.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonShiftChars.Name = "buttonShiftChars";
            this.buttonShiftChars.Size = new System.Drawing.Size(158, 80);
            this.buttonShiftChars.TabIndex = 12;
            this.buttonShiftChars.Text = "Shift chars\r\n64-79 -> 80-95\r\n32-47 -> 64-79";
            this.buttonShiftChars.UseVisualStyleBackColor = true;
            this.buttonShiftChars.Visible = false;
            this.buttonShiftChars.Click += new System.EventHandler(this.ButtonShiftChars_Click);
            // 
            // buttonHoboExport
            // 
            this.buttonHoboExport.Location = new System.Drawing.Point(174, 623);
            this.buttonHoboExport.Name = "buttonHoboExport";
            this.buttonHoboExport.Size = new System.Drawing.Size(158, 35);
            this.buttonHoboExport.TabIndex = 13;
            this.buttonHoboExport.Text = "Column Export";
            this.buttonHoboExport.UseVisualStyleBackColor = true;
            this.buttonHoboExport.Visible = false;
            this.buttonHoboExport.Click += new System.EventHandler(this.ButtonHoboExport_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.checkBoxAlpa);
            this.tabPage3.Controls.Add(this.groupBox6);
            this.tabPage3.Controls.Add(this.nudMapH);
            this.tabPage3.Controls.Add(this.nudMapW);
            this.tabPage3.Controls.Add(this.lblMapSize);
            this.tabPage3.Controls.Add(this.nudScreenH);
            this.tabPage3.Controls.Add(this.lblScreenSize);
            this.tabPage3.Controls.Add(this.nudScreenW);
            this.tabPage3.Controls.Add(this.buttonNewMap);
            this.tabPage3.Location = new System.Drawing.Point(4, 54);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage3.Size = new System.Drawing.Size(414, 836);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "New Map";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // checkBoxAlpa
            // 
            this.checkBoxAlpa.AutoSize = true;
            this.checkBoxAlpa.Location = new System.Drawing.Point(10, 357);
            this.checkBoxAlpa.Name = "checkBoxAlpa";
            this.checkBoxAlpa.Size = new System.Drawing.Size(76, 24);
            this.checkBoxAlpa.TabIndex = 8;
            this.checkBoxAlpa.Text = "ALPA";
            this.checkBoxAlpa.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.labelAbout2);
            this.groupBox6.Controls.Add(this.linkLabel2);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.linkLabel1);
            this.groupBox6.Controls.Add(this.labelAbout);
            this.groupBox6.Location = new System.Drawing.Point(10, 411);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox6.Size = new System.Drawing.Size(316, 283);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "About";
            // 
            // labelAbout2
            // 
            this.labelAbout2.Location = new System.Drawing.Point(9, 60);
            this.labelAbout2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAbout2.Name = "labelAbout2";
            this.labelAbout2.Size = new System.Drawing.Size(297, 55);
            this.labelAbout2.TabIndex = 4;
            this.labelAbout2.Text = "\nversion 1.1 - 18.08.2018";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(90, 245);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(124, 20);
            this.linkLabel2.TabIndex = 3;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Sourceforge.net";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 245);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Sources:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(9, 115);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(174, 20);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://matosimi.atari.org";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // labelAbout
            // 
            this.labelAbout.Location = new System.Drawing.Point(9, 31);
            this.labelAbout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAbout.Name = "labelAbout";
            this.labelAbout.Size = new System.Drawing.Size(297, 29);
            this.labelAbout.TabIndex = 0;
            this.labelAbout.Text = "Created by Martin Šimeèek";
            // 
            // nudMapH
            // 
            this.nudMapH.Location = new System.Drawing.Point(154, 171);
            this.nudMapH.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudMapH.Name = "nudMapH";
            this.nudMapH.Size = new System.Drawing.Size(118, 26);
            this.nudMapH.TabIndex = 6;
            this.nudMapH.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // nudMapW
            // 
            this.nudMapW.Location = new System.Drawing.Point(14, 171);
            this.nudMapW.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudMapW.Name = "nudMapW";
            this.nudMapW.Size = new System.Drawing.Size(118, 26);
            this.nudMapW.TabIndex = 5;
            this.nudMapW.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblMapSize
            // 
            this.lblMapSize.AutoSize = true;
            this.lblMapSize.Location = new System.Drawing.Point(9, 123);
            this.lblMapSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMapSize.Name = "lblMapSize";
            this.lblMapSize.Size = new System.Drawing.Size(258, 20);
            this.lblMapSize.TabIndex = 4;
            this.lblMapSize.Text = "Map size (Width, Height) in screens";
            // 
            // nudScreenH
            // 
            this.nudScreenH.Location = new System.Drawing.Point(154, 63);
            this.nudScreenH.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudScreenH.Name = "nudScreenH";
            this.nudScreenH.Size = new System.Drawing.Size(118, 26);
            this.nudScreenH.TabIndex = 3;
            this.nudScreenH.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // lblScreenSize
            // 
            this.lblScreenSize.AutoSize = true;
            this.lblScreenSize.Location = new System.Drawing.Point(9, 23);
            this.lblScreenSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScreenSize.Name = "lblScreenSize";
            this.lblScreenSize.Size = new System.Drawing.Size(261, 20);
            this.lblScreenSize.TabIndex = 2;
            this.lblScreenSize.Text = "Screen size (Width, Height) in chars";
            // 
            // nudScreenW
            // 
            this.nudScreenW.Location = new System.Drawing.Point(14, 63);
            this.nudScreenW.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudScreenW.Name = "nudScreenW";
            this.nudScreenW.Size = new System.Drawing.Size(118, 26);
            this.nudScreenW.TabIndex = 1;
            this.nudScreenW.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // buttonNewMap
            // 
            this.buttonNewMap.Location = new System.Drawing.Point(9, 229);
            this.buttonNewMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNewMap.Name = "buttonNewMap";
            this.buttonNewMap.Size = new System.Drawing.Size(152, 46);
            this.buttonNewMap.TabIndex = 0;
            this.buttonNewMap.Text = "Create new map";
            this.buttonNewMap.UseVisualStyleBackColor = true;
            this.buttonNewMap.Click += new System.EventHandler(this.BtnNewMap_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.buttonAddScreenRowToMap);
            this.tabPage4.Controls.Add(this.radioButtonWholeMap);
            this.tabPage4.Controls.Add(this.radioButtonCurrentScreen);
            this.tabPage4.Controls.Add(this.buttonReplaceCurrentScreen);
            this.tabPage4.Controls.Add(this.numericUpDownReplace2);
            this.tabPage4.Controls.Add(this.numericUpDownReplace1);
            this.tabPage4.Controls.Add(this.labelReplace);
            this.tabPage4.Location = new System.Drawing.Point(4, 54);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage4.Size = new System.Drawing.Size(414, 836);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Data manipulation";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // buttonAddScreenRowToMap
            // 
            this.buttonAddScreenRowToMap.Location = new System.Drawing.Point(9, 229);
            this.buttonAddScreenRowToMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonAddScreenRowToMap.Name = "buttonAddScreenRowToMap";
            this.buttonAddScreenRowToMap.Size = new System.Drawing.Size(318, 35);
            this.buttonAddScreenRowToMap.TabIndex = 8;
            this.buttonAddScreenRowToMap.Text = "Extend map with additional row";
            this.buttonAddScreenRowToMap.UseVisualStyleBackColor = true;
            this.buttonAddScreenRowToMap.Click += new System.EventHandler(this.ButtonAddScreenRowToMap_Click);
            // 
            // radioButtonWholeMap
            // 
            this.radioButtonWholeMap.AutoSize = true;
            this.radioButtonWholeMap.Location = new System.Drawing.Point(18, 72);
            this.radioButtonWholeMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonWholeMap.Name = "radioButtonWholeMap";
            this.radioButtonWholeMap.Size = new System.Drawing.Size(114, 24);
            this.radioButtonWholeMap.TabIndex = 7;
            this.radioButtonWholeMap.Text = "Whole map";
            this.radioButtonWholeMap.UseVisualStyleBackColor = true;
            // 
            // radioButtonCurrentScreen
            // 
            this.radioButtonCurrentScreen.AutoSize = true;
            this.radioButtonCurrentScreen.Checked = true;
            this.radioButtonCurrentScreen.Location = new System.Drawing.Point(186, 72);
            this.radioButtonCurrentScreen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonCurrentScreen.Name = "radioButtonCurrentScreen";
            this.radioButtonCurrentScreen.Size = new System.Drawing.Size(139, 24);
            this.radioButtonCurrentScreen.TabIndex = 6;
            this.radioButtonCurrentScreen.TabStop = true;
            this.radioButtonCurrentScreen.Text = "Current screen";
            this.radioButtonCurrentScreen.UseVisualStyleBackColor = true;
            // 
            // buttonReplaceCurrentScreen
            // 
            this.buttonReplaceCurrentScreen.Location = new System.Drawing.Point(9, 108);
            this.buttonReplaceCurrentScreen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonReplaceCurrentScreen.Name = "buttonReplaceCurrentScreen";
            this.buttonReplaceCurrentScreen.Size = new System.Drawing.Size(318, 35);
            this.buttonReplaceCurrentScreen.TabIndex = 5;
            this.buttonReplaceCurrentScreen.Text = "Perform replacement";
            this.buttonReplaceCurrentScreen.UseVisualStyleBackColor = true;
            this.buttonReplaceCurrentScreen.Click += new System.EventHandler(this.ButtonReplaceCurrentScreen_Click);
            // 
            // numericUpDownReplace2
            // 
            this.numericUpDownReplace2.Location = new System.Drawing.Point(258, 18);
            this.numericUpDownReplace2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownReplace2.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownReplace2.Name = "numericUpDownReplace2";
            this.numericUpDownReplace2.Size = new System.Drawing.Size(69, 26);
            this.numericUpDownReplace2.TabIndex = 4;
            // 
            // numericUpDownReplace1
            // 
            this.numericUpDownReplace1.Location = new System.Drawing.Point(174, 18);
            this.numericUpDownReplace1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownReplace1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownReplace1.Name = "numericUpDownReplace1";
            this.numericUpDownReplace1.Size = new System.Drawing.Size(69, 26);
            this.numericUpDownReplace1.TabIndex = 3;
            // 
            // labelReplace
            // 
            this.labelReplace.AutoSize = true;
            this.labelReplace.Location = new System.Drawing.Point(14, 18);
            this.labelReplace.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelReplace.Name = "labelReplace";
            this.labelReplace.Size = new System.Drawing.Size(111, 20);
            this.labelReplace.TabIndex = 0;
            this.labelReplace.Text = "Replace chars";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // contextMenuStripScreen
            // 
            this.contextMenuStripScreen.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripScreen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemClear,
            this.toolStripMenuItemHFlip,
            this.toolStripMenuItemVFlip});
            this.contextMenuStripScreen.Name = "contextMenuStripScreen";
            this.contextMenuStripScreen.Size = new System.Drawing.Size(257, 100);
            // 
            // toolStripMenuItemClear
            // 
            this.toolStripMenuItemClear.Name = "toolStripMenuItemClear";
            this.toolStripMenuItemClear.Size = new System.Drawing.Size(256, 32);
            this.toolStripMenuItemClear.Text = "Clear screen";
            this.toolStripMenuItemClear.Click += new System.EventHandler(this.ToolStripMenuItemClear_Click);
            // 
            // toolStripMenuItemHFlip
            // 
            this.toolStripMenuItemHFlip.Name = "toolStripMenuItemHFlip";
            this.toolStripMenuItemHFlip.Size = new System.Drawing.Size(256, 32);
            this.toolStripMenuItemHFlip.Text = "Flip Screen Horizontal";
            this.toolStripMenuItemHFlip.Click += new System.EventHandler(this.ToolStripMenuItemHFlip_Click);
            // 
            // toolStripMenuItemVFlip
            // 
            this.toolStripMenuItemVFlip.Name = "toolStripMenuItemVFlip";
            this.toolStripMenuItemVFlip.Size = new System.Drawing.Size(256, 32);
            this.toolStripMenuItemVFlip.Text = "Flip Screen Vertical";
            this.toolStripMenuItemVFlip.Click += new System.EventHandler(this.ToolStripMenuItemVFlip_Click);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.AutoSize = true;
            this.groupBoxFont.Controls.Add(this.buttonRefreshFont);
            this.groupBoxFont.Controls.Add(this.buttonLoadFont);
            this.groupBoxFont.Controls.Add(this.buttonShowFont);
            this.groupBoxFont.Location = new System.Drawing.Point(3, 675);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(339, 85);
            this.groupBoxFont.TabIndex = 13;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // groupBoxDli
            // 
            this.groupBoxDli.AutoSize = true;
            this.groupBoxDli.Controls.Add(this.checkBoxShowDli);
            this.groupBoxDli.Controls.Add(this.checkBoxEditDli);
            this.groupBoxDli.Location = new System.Drawing.Point(3, 599);
            this.groupBoxDli.Name = "groupBoxDli";
            this.groupBoxDli.Size = new System.Drawing.Size(285, 70);
            this.groupBoxDli.TabIndex = 14;
            this.groupBoxDli.TabStop = false;
            this.groupBoxDli.Text = "DLI";
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.labelCharOccurence);
            this.panelStatus.Controls.Add(this.labelScreen);
            this.panelStatus.Controls.Add(this.labelChar);
            this.panelStatus.Controls.Add(this.labelPosition);
            this.panelStatus.Location = new System.Drawing.Point(3, 766);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(331, 78);
            this.panelStatus.TabIndex = 15;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBoxZoom);
            this.flowLayoutPanel1.Controls.Add(this.groupBoxColors);
            this.flowLayoutPanel1.Controls.Add(this.groupBox4);
            this.flowLayoutPanel1.Controls.Add(this.groupBoxDli);
            this.flowLayoutPanel1.Controls.Add(this.groupBoxFont);
            this.flowLayoutPanel1.Controls.Add(this.panelStatus);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(406, 826);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 894);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "AtariMapMaker by Martin Simecek";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMap)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageColors.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClipboard)).EndInit();
            this.groupBoxColors.ResumeLayout(false);
            this.groupBoxZoom.ResumeLayout(false);
            this.groupBoxZoom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenFromX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenFromY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenToX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenToY)).EndInit();
            this.groupBoxMap.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReplace2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReplace1)).EndInit();
            this.contextMenuStripScreen.ResumeLayout(false);
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxDli.ResumeLayout(false);
            this.groupBoxDli.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBoxMap;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageColors;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button buttonShowFont;
        private System.Windows.Forms.ListView listViewColors;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.GroupBox groupBoxZoom;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBoxClipboard;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelScreen;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Label lblScrFromXY;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenToY;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenFromY;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenToX;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenFromX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Button buttonShiftChars;
        private System.Windows.Forms.Button buttonHoboExport;
        private System.Windows.Forms.Button buttonHoboImport;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.NumericUpDown nudMapH;
        private System.Windows.Forms.NumericUpDown nudMapW;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.NumericUpDown nudScreenH;
        private System.Windows.Forms.Label lblScreenSize;
        private System.Windows.Forms.NumericUpDown nudScreenW;
        private System.Windows.Forms.Button buttonNewMap;
        private System.Windows.Forms.TrackBar trackBarZoom;
        private System.Windows.Forms.CheckBox comboBoxDrawBorders;
        private System.Windows.Forms.CheckBox comboBoxDrawGrid;
        private System.Windows.Forms.Label labelChar;
        private System.Windows.Forms.Button buttonLoadFont;
        private System.Windows.Forms.GroupBox groupBoxMap;
        private System.Windows.Forms.ComboBox comboOperation;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblScrToXY;
        private System.Windows.Forms.Button buttonPerform;
        private System.Windows.Forms.NumericUpDown numericUpDown6;
        private System.Windows.Forms.Label lblDataWidth;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label labelAbout;
        private System.Windows.Forms.Button buttonRefreshFont;
        private System.Windows.Forms.Label labelAbout2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxEditDli;
        private System.Windows.Forms.CheckBox checkBoxShowDli;
        private System.Windows.Forms.Label labelDliMask;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxDli;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RadioButton radioButtonWholeMap;
        private System.Windows.Forms.RadioButton radioButtonCurrentScreen;
        private System.Windows.Forms.Button buttonReplaceCurrentScreen;
        private System.Windows.Forms.NumericUpDown numericUpDownReplace2;
        private System.Windows.Forms.NumericUpDown numericUpDownReplace1;
        private System.Windows.Forms.Label labelReplace;
        private System.Windows.Forms.Button buttonAddScreenRowToMap;
        private System.Windows.Forms.CheckBox checkBoxAlpa;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripScreen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClear;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHFlip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVFlip;
        private System.Windows.Forms.CheckBox checkBoxShowScreenSelection;
        private System.Windows.Forms.Label labelCharOccurence;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.GroupBox groupBoxDli;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}

