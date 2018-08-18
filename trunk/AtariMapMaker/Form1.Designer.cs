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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelChar = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.labelScreen = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.buttonLoadFont = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDrawBorders = new System.Windows.Forms.CheckBox();
            this.cbDrawGrid = new System.Windows.Forms.CheckBox();
            this.tbZoom = new System.Windows.Forms.TrackBar();
            this.btnShowFont = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnImport = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
            this.lblDataWidth = new System.Windows.Forms.Label();
            this.btnPerform = new System.Windows.Forms.Button();
            this.comboOperation = new System.Windows.Forms.ComboBox();
            this.lblScrToXY = new System.Windows.Forms.Label();
            this.lblScrFromXY = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.btnHOBOimport = new System.Windows.Forms.Button();
            this.buttonShiftChars = new System.Windows.Forms.Button();
            this.btn_hoboexport = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
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
            this.btnNewMap = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.buttonRefreshFont = new System.Windows.Forms.Button();
            this.labelAbout2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.labelChar);
            this.splitContainer1.Panel2.Controls.Add(this.labelPosition);
            this.splitContainer1.Panel2.Controls.Add(this.labelScreen);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(914, 542);
            this.splitContainer1.SplitterDistance = 674;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(674, 542);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.ClientSizeChanged += new System.EventHandler(this.pictureBox1_ClientSizeChanged);
            this.pictureBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDoubleClick);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            // 
            // labelChar
            // 
            this.labelChar.AutoSize = true;
            this.labelChar.Location = new System.Drawing.Point(119, 492);
            this.labelChar.Name = "labelChar";
            this.labelChar.Size = new System.Drawing.Size(65, 13);
            this.labelChar.TabIndex = 3;
            this.labelChar.Text = "Char $00 (0)";
            this.labelChar.Click += new System.EventHandler(this.label1_Click);
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(9, 508);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(62, 13);
            this.labelPosition.TabIndex = 2;
            this.labelPosition.Text = "Position 0:0";
            // 
            // labelScreen
            // 
            this.labelScreen.AutoSize = true;
            this.labelScreen.Location = new System.Drawing.Point(9, 492);
            this.labelScreen.Name = "labelScreen";
            this.labelScreen.Size = new System.Drawing.Size(59, 13);
            this.labelScreen.TabIndex = 1;
            this.labelScreen.Text = "Screen 0:0";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(2, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(234, 489);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonRefreshFont);
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.buttonLoadFont);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.btnShowFont);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(226, 463);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Colors";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pictureBox2);
            this.groupBox4.Location = new System.Drawing.Point(7, 279);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(216, 181);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Clipboard";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(11, 19);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(195, 156);
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            // 
            // buttonLoadFont
            // 
            this.buttonLoadFont.Location = new System.Drawing.Point(138, 249);
            this.buttonLoadFont.Name = "buttonLoadFont";
            this.buttonLoadFont.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadFont.TabIndex = 9;
            this.buttonLoadFont.Text = "Load Font";
            this.buttonLoadFont.UseVisualStyleBackColor = true;
            this.buttonLoadFont.Click += new System.EventHandler(this.buttonLoadFont_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listView1);
            this.groupBox2.Location = new System.Drawing.Point(6, 66);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(216, 177);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Colors";
            // 
            // listView1
            // 
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(12, 19);
            this.listView1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(195, 146);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            this.listView1.MouseLeave += new System.EventHandler(this.listView1_MouseLeave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbDrawBorders);
            this.groupBox1.Controls.Add(this.cbDrawGrid);
            this.groupBox1.Controls.Add(this.tbZoom);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(216, 54);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Zoom";
            // 
            // cbDrawBorders
            // 
            this.cbDrawBorders.AutoSize = true;
            this.cbDrawBorders.Checked = true;
            this.cbDrawBorders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawBorders.Location = new System.Drawing.Point(109, 31);
            this.cbDrawBorders.Name = "cbDrawBorders";
            this.cbDrawBorders.Size = new System.Drawing.Size(98, 17);
            this.cbDrawBorders.TabIndex = 2;
            this.cbDrawBorders.Text = "Screen borders";
            this.cbDrawBorders.UseVisualStyleBackColor = true;
            this.cbDrawBorders.CheckedChanged += new System.EventHandler(this.cbDrawBorders_CheckedChanged);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(109, 13);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(45, 17);
            this.cbDrawGrid.TabIndex = 1;
            this.cbDrawGrid.Text = "Grid";
            this.cbDrawGrid.UseVisualStyleBackColor = true;
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // tbZoom
            // 
            this.tbZoom.AutoSize = false;
            this.tbZoom.LargeChange = 1;
            this.tbZoom.Location = new System.Drawing.Point(12, 13);
            this.tbZoom.Maximum = 3;
            this.tbZoom.Name = "tbZoom";
            this.tbZoom.Size = new System.Drawing.Size(91, 35);
            this.tbZoom.TabIndex = 0;
            this.tbZoom.Value = 1;
            this.tbZoom.Scroll += new System.EventHandler(this.tbZoom_Scroll);
            // 
            // btnShowFont
            // 
            this.btnShowFont.Location = new System.Drawing.Point(6, 249);
            this.btnShowFont.Name = "btnShowFont";
            this.btnShowFont.Size = new System.Drawing.Size(75, 23);
            this.btnShowFont.TabIndex = 3;
            this.btnShowFont.Text = "Show font";
            this.btnShowFont.UseVisualStyleBackColor = true;
            this.btnShowFont.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnImport);
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.buttonExport);
            this.tabPage2.Controls.Add(this.btnHOBOimport);
            this.tabPage2.Controls.Add(this.buttonShiftChars);
            this.tabPage2.Controls.Add(this.btn_hoboexport);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(226, 463);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Load/Save/Export";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(116, 348);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 19;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Visible = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.numericUpDown6);
            this.groupBox5.Controls.Add(this.lblDataWidth);
            this.groupBox5.Controls.Add(this.btnPerform);
            this.groupBox5.Controls.Add(this.comboOperation);
            this.groupBox5.Controls.Add(this.lblScrToXY);
            this.groupBox5.Controls.Add(this.lblScrFromXY);
            this.groupBox5.Controls.Add(this.numericUpDown1);
            this.groupBox5.Controls.Add(this.numericUpDown3);
            this.groupBox5.Controls.Add(this.numericUpDown5);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.numericUpDown2);
            this.groupBox5.Controls.Add(this.numericUpDown4);
            this.groupBox5.Location = new System.Drawing.Point(6, 75);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(212, 249);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Data operations";
            // 
            // numericUpDown6
            // 
            this.numericUpDown6.Location = new System.Drawing.Point(131, 184);
            this.numericUpDown6.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown6.Name = "numericUpDown6";
            this.numericUpDown6.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown6.TabIndex = 20;
            this.numericUpDown6.Value = new decimal(new int[] {
            106,
            0,
            0,
            0});
            // 
            // lblDataWidth
            // 
            this.lblDataWidth.Location = new System.Drawing.Point(3, 186);
            this.lblDataWidth.Name = "lblDataWidth";
            this.lblDataWidth.Size = new System.Drawing.Size(130, 18);
            this.lblDataWidth.TabIndex = 19;
            this.lblDataWidth.Text = "Dataline width";
            // 
            // btnPerform
            // 
            this.btnPerform.Location = new System.Drawing.Point(6, 216);
            this.btnPerform.Name = "btnPerform";
            this.btnPerform.Size = new System.Drawing.Size(200, 23);
            this.btnPerform.TabIndex = 18;
            this.btnPerform.Text = "Perform";
            this.btnPerform.UseVisualStyleBackColor = true;
            this.btnPerform.Click += new System.EventHandler(this.btnPerform_Click);
            // 
            // comboOperation
            // 
            this.comboOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOperation.FormattingEnabled = true;
            this.comboOperation.Location = new System.Drawing.Point(6, 20);
            this.comboOperation.Name = "comboOperation";
            this.comboOperation.Size = new System.Drawing.Size(200, 21);
            this.comboOperation.TabIndex = 15;
            this.comboOperation.SelectedIndexChanged += new System.EventHandler(this.comboOperation_SelectedIndexChanged);
            // 
            // lblScrToXY
            // 
            this.lblScrToXY.Location = new System.Drawing.Point(3, 111);
            this.lblScrToXY.Name = "lblScrToXY";
            this.lblScrToXY.Size = new System.Drawing.Size(100, 15);
            this.lblScrToXY.TabIndex = 17;
            this.lblScrToXY.Text = "Screen to X,Y";
            // 
            // lblScrFromXY
            // 
            this.lblScrFromXY.Location = new System.Drawing.Point(3, 61);
            this.lblScrFromXY.Name = "lblScrFromXY";
            this.lblScrFromXY.Size = new System.Drawing.Size(100, 14);
            this.lblScrFromXY.TabIndex = 9;
            this.lblScrFromXY.Text = "Screen from X,Y";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(6, 78);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown1.TabIndex = 3;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(131, 78);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown3.TabIndex = 5;
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(131, 163);
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown5.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 165);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 18);
            this.label3.TabIndex = 11;
            this.label3.Text = "Extra chars in line (suffix)\r\n";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(6, 129);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown2.TabIndex = 4;
            this.numericUpDown2.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(131, 129);
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown4.TabIndex = 6;
            this.numericUpDown4.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonLoad);
            this.groupBox3.Controls.Add(this.buttonSave);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(212, 63);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Map";
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(6, 19);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 29);
            this.buttonLoad.TabIndex = 0;
            this.buttonLoad.Text = "Load Map";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(131, 19);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 29);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save Map";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(116, 377);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Visible = false;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // btnHOBOimport
            // 
            this.btnHOBOimport.Location = new System.Drawing.Point(116, 434);
            this.btnHOBOimport.Margin = new System.Windows.Forms.Padding(2);
            this.btnHOBOimport.Name = "btnHOBOimport";
            this.btnHOBOimport.Size = new System.Drawing.Size(105, 23);
            this.btnHOBOimport.TabIndex = 14;
            this.btnHOBOimport.Text = "Column Import";
            this.btnHOBOimport.UseVisualStyleBackColor = true;
            this.btnHOBOimport.Visible = false;
            this.btnHOBOimport.Click += new System.EventHandler(this.btnHOBOimport_Click);
            // 
            // buttonShiftChars
            // 
            this.buttonShiftChars.Location = new System.Drawing.Point(8, 405);
            this.buttonShiftChars.Name = "buttonShiftChars";
            this.buttonShiftChars.Size = new System.Drawing.Size(105, 52);
            this.buttonShiftChars.TabIndex = 12;
            this.buttonShiftChars.Text = "Shift chars\r\n64-79 -> 80-95\r\n32-47 -> 64-79";
            this.buttonShiftChars.UseVisualStyleBackColor = true;
            this.buttonShiftChars.Visible = false;
            this.buttonShiftChars.Click += new System.EventHandler(this.buttonShiftChars_Click);
            // 
            // btn_hoboexport
            // 
            this.btn_hoboexport.Location = new System.Drawing.Point(116, 405);
            this.btn_hoboexport.Margin = new System.Windows.Forms.Padding(2);
            this.btn_hoboexport.Name = "btn_hoboexport";
            this.btn_hoboexport.Size = new System.Drawing.Size(105, 23);
            this.btn_hoboexport.TabIndex = 13;
            this.btn_hoboexport.Text = "Column Export";
            this.btn_hoboexport.UseVisualStyleBackColor = true;
            this.btn_hoboexport.Visible = false;
            this.btn_hoboexport.Click += new System.EventHandler(this.btn_hoboexport_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox6);
            this.tabPage3.Controls.Add(this.nudMapH);
            this.tabPage3.Controls.Add(this.nudMapW);
            this.tabPage3.Controls.Add(this.lblMapSize);
            this.tabPage3.Controls.Add(this.nudScreenH);
            this.tabPage3.Controls.Add(this.lblScreenSize);
            this.tabPage3.Controls.Add(this.nudScreenW);
            this.tabPage3.Controls.Add(this.btnNewMap);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(226, 463);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "New Map";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.labelAbout2);
            this.groupBox6.Controls.Add(this.linkLabel2);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.linkLabel1);
            this.groupBox6.Controls.Add(this.labelAbout);
            this.groupBox6.Location = new System.Drawing.Point(7, 267);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(211, 184);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "About";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(60, 159);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(83, 13);
            this.linkLabel2.TabIndex = 3;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Sourceforge.net";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Sources:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(6, 75);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(119, 13);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://matosimi.atari.org";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // labelAbout
            // 
            this.labelAbout.Location = new System.Drawing.Point(6, 20);
            this.labelAbout.Name = "labelAbout";
            this.labelAbout.Size = new System.Drawing.Size(198, 19);
            this.labelAbout.TabIndex = 0;
            this.labelAbout.Text = "Created by Martin Šimeèek";
            // 
            // nudMapH
            // 
            this.nudMapH.Location = new System.Drawing.Point(103, 111);
            this.nudMapH.Name = "nudMapH";
            this.nudMapH.Size = new System.Drawing.Size(79, 20);
            this.nudMapH.TabIndex = 6;
            this.nudMapH.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // nudMapW
            // 
            this.nudMapW.Location = new System.Drawing.Point(9, 111);
            this.nudMapW.Name = "nudMapW";
            this.nudMapW.Size = new System.Drawing.Size(79, 20);
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
            this.lblMapSize.Location = new System.Drawing.Point(6, 80);
            this.lblMapSize.Name = "lblMapSize";
            this.lblMapSize.Size = new System.Drawing.Size(174, 13);
            this.lblMapSize.TabIndex = 4;
            this.lblMapSize.Text = "Map size (Width, Height) in screens";
            // 
            // nudScreenH
            // 
            this.nudScreenH.Location = new System.Drawing.Point(103, 41);
            this.nudScreenH.Name = "nudScreenH";
            this.nudScreenH.Size = new System.Drawing.Size(79, 20);
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
            this.lblScreenSize.Location = new System.Drawing.Point(6, 15);
            this.lblScreenSize.Name = "lblScreenSize";
            this.lblScreenSize.Size = new System.Drawing.Size(176, 13);
            this.lblScreenSize.TabIndex = 2;
            this.lblScreenSize.Text = "Screen size (Width, Height) in chars";
            // 
            // nudScreenW
            // 
            this.nudScreenW.Location = new System.Drawing.Point(9, 41);
            this.nudScreenW.Name = "nudScreenW";
            this.nudScreenW.Size = new System.Drawing.Size(79, 20);
            this.nudScreenW.TabIndex = 1;
            this.nudScreenW.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // btnNewMap
            // 
            this.btnNewMap.Location = new System.Drawing.Point(6, 149);
            this.btnNewMap.Name = "btnNewMap";
            this.btnNewMap.Size = new System.Drawing.Size(101, 30);
            this.btnNewMap.TabIndex = 0;
            this.btnNewMap.Text = "Create new map";
            this.btnNewMap.UseVisualStyleBackColor = true;
            this.btnNewMap.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonRefreshFont
            // 
            this.buttonRefreshFont.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonRefreshFont.Location = new System.Drawing.Point(112, 249);
            this.buttonRefreshFont.Name = "buttonRefreshFont";
            this.buttonRefreshFont.Size = new System.Drawing.Size(26, 23);
            this.buttonRefreshFont.TabIndex = 11;
            this.buttonRefreshFont.Text = "P";
            this.buttonRefreshFont.UseVisualStyleBackColor = true;
            this.buttonRefreshFont.Click += new System.EventHandler(this.buttonRefreshFont_Click);
            // 
            // labelAbout2
            // 
            this.labelAbout2.Location = new System.Drawing.Point(6, 39);
            this.labelAbout2.Name = "labelAbout2";
            this.labelAbout2.Size = new System.Drawing.Size(198, 36);
            this.labelAbout2.TabIndex = 4;
            this.labelAbout2.Text = "\nversion 1.1 - 18.08.2018";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 542);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "AtariMapMaker v1.0 by Martin Simecek";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnShowFont;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelScreen;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Label lblScrFromXY;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Button buttonShiftChars;
        private System.Windows.Forms.Button btn_hoboexport;
        private System.Windows.Forms.Button btnHOBOimport;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.NumericUpDown nudMapH;
        private System.Windows.Forms.NumericUpDown nudMapW;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.NumericUpDown nudScreenH;
        private System.Windows.Forms.Label lblScreenSize;
        private System.Windows.Forms.NumericUpDown nudScreenW;
        private System.Windows.Forms.Button btnNewMap;
        private System.Windows.Forms.TrackBar tbZoom;
        private System.Windows.Forms.CheckBox cbDrawBorders;
        private System.Windows.Forms.CheckBox cbDrawGrid;
        private System.Windows.Forms.Label labelChar;
        private System.Windows.Forms.Button buttonLoadFont;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboOperation;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblScrToXY;
        private System.Windows.Forms.Button btnPerform;
        private System.Windows.Forms.NumericUpDown numericUpDown6;
        private System.Windows.Forms.Label lblDataWidth;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label labelAbout;
        private System.Windows.Forms.Button buttonRefreshFont;
        private System.Windows.Forms.Label labelAbout2;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

