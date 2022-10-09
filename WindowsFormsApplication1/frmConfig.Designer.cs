namespace WindowsFormsApplication1
{
    partial class frmConfig
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
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbFD2Capacity = new System.Windows.Forms.ComboBox();
            this.cbFD1Capacity = new System.Windows.Forms.ComboBox();
            this.ckFD2Enabled = new System.Windows.Forms.CheckBox();
            this.ckFD1Enabled = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmdGetFile2 = new System.Windows.Forms.Button();
            this.ckHD2Installed = new System.Windows.Forms.CheckBox();
            this.txtSPT2 = new System.Windows.Forms.TextBox();
            this.txtHeads2 = new System.Windows.Forms.TextBox();
            this.txtCyl2 = new System.Windows.Forms.TextBox();
            this.txtHD2Filename = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cmdGetFile1 = new System.Windows.Forms.Button();
            this.ckHD1Installed = new System.Windows.Forms.CheckBox();
            this.txtSPT1 = new System.Windows.Forms.TextBox();
            this.txtHeads1 = new System.Windows.Forms.TextBox();
            this.txtHD1Filename = new System.Windows.Forms.TextBox();
            this.txtCyl1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmdGetVidBIOSFile = new System.Windows.Forms.Button();
            this.cmdGetFD2 = new System.Windows.Forms.Button();
            this.cmdGetBIOSFile = new System.Windows.Forms.Button();
            this.cmdGetFD1 = new System.Windows.Forms.Button();
            this.cmdGetCMOSFile = new System.Windows.Forms.Button();
            this.txtVidBIOSFilename = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtFD2Filename = new System.Windows.Forms.TextBox();
            this.txtBIOSFilename = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.txtFD1Filename = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtCMOSFilename = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbMemory = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label24 = new System.Windows.Forms.Label();
            this.txtMemoryAmt = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.ofdHD1 = new System.Windows.Forms.OpenFileDialog();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.clbOptions = new System.Windows.Forms.CheckedListBox();
            this.txtDebugAtCS = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.grpDebugAt = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtDebugAtEIP = new System.Windows.Forms.TextBox();
            this.grpDieAt = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtDieAtEIP = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtDieAtCS = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cboBootDevice = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtMaxDbgFileSize = new System.Windows.Forms.TextBox();
            this.txtDebugPath = new System.Windows.Forms.TextBox();
            this.fb1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label21 = new System.Windows.Forms.Label();
            this.txtTimerTick = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.cmdSwapHDs = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.txtTaskNameOffset = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label26 = new System.Windows.Forms.Label();
            this.txtDumpAtEIP = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.txtDumpAtCS = new System.Windows.Forms.TextBox();
            this.cmdSwapFDs = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMemory)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.grpDebugAt.SuspendLayout();
            this.grpDieAt.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filename";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.groupBox1.Controls.Add(this.cbFD2Capacity);
            this.groupBox1.Controls.Add(this.cbFD1Capacity);
            this.groupBox1.Controls.Add(this.ckFD2Enabled);
            this.groupBox1.Controls.Add(this.ckFD1Enabled);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.cmdGetVidBIOSFile);
            this.groupBox1.Controls.Add(this.cmdGetFD2);
            this.groupBox1.Controls.Add(this.cmdGetBIOSFile);
            this.groupBox1.Controls.Add(this.cmdGetFD1);
            this.groupBox1.Controls.Add(this.cmdGetCMOSFile);
            this.groupBox1.Controls.Add(this.txtVidBIOSFilename);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.txtFD2Filename);
            this.groupBox1.Controls.Add(this.txtBIOSFilename);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.txtFD1Filename);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.txtCMOSFilename);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Location = new System.Drawing.Point(40, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(938, 462);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Files";
            // 
            // cbFD2Capacity
            // 
            this.cbFD2Capacity.FormattingEnabled = true;
            this.cbFD2Capacity.Items.AddRange(new object[] {
            "720K",
            "1_2M",
            "1_44M",
            "2_88M"});
            this.cbFD2Capacity.Location = new System.Drawing.Point(732, 282);
            this.cbFD2Capacity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbFD2Capacity.Name = "cbFD2Capacity";
            this.cbFD2Capacity.Size = new System.Drawing.Size(98, 28);
            this.cbFD2Capacity.TabIndex = 9;
            // 
            // cbFD1Capacity
            // 
            this.cbFD1Capacity.FormattingEnabled = true;
            this.cbFD1Capacity.Items.AddRange(new object[] {
            "720K",
            "1_2M",
            "1_44M",
            "2_88M"});
            this.cbFD1Capacity.Location = new System.Drawing.Point(732, 245);
            this.cbFD1Capacity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbFD1Capacity.Name = "cbFD1Capacity";
            this.cbFD1Capacity.Size = new System.Drawing.Size(100, 28);
            this.cbFD1Capacity.TabIndex = 5;
            // 
            // ckFD2Enabled
            // 
            this.ckFD2Enabled.AutoSize = true;
            this.ckFD2Enabled.Location = new System.Drawing.Point(842, 282);
            this.ckFD2Enabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ckFD2Enabled.Name = "ckFD2Enabled";
            this.ckFD2Enabled.Size = new System.Drawing.Size(94, 24);
            this.ckFD2Enabled.TabIndex = 10;
            this.ckFD2Enabled.Text = "Inserted";
            this.ckFD2Enabled.UseVisualStyleBackColor = true;
            // 
            // ckFD1Enabled
            // 
            this.ckFD1Enabled.AutoSize = true;
            this.ckFD1Enabled.Location = new System.Drawing.Point(842, 249);
            this.ckFD1Enabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ckFD1Enabled.Name = "ckFD1Enabled";
            this.ckFD1Enabled.Size = new System.Drawing.Size(94, 24);
            this.ckFD1Enabled.TabIndex = 6;
            this.ckFD1Enabled.Text = "Inserted";
            this.ckFD1Enabled.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmdGetFile2);
            this.groupBox3.Controls.Add(this.ckHD2Installed);
            this.groupBox3.Controls.Add(this.txtSPT2);
            this.groupBox3.Controls.Add(this.txtHeads2);
            this.groupBox3.Controls.Add(this.txtCyl2);
            this.groupBox3.Controls.Add(this.txtHD2Filename);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(15, 128);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Size = new System.Drawing.Size(914, 106);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Hard Drive 2";
            // 
            // cmdGetFile2
            // 
            this.cmdGetFile2.Location = new System.Drawing.Point(855, 20);
            this.cmdGetFile2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetFile2.Name = "cmdGetFile2";
            this.cmdGetFile2.Size = new System.Drawing.Size(44, 34);
            this.cmdGetFile2.TabIndex = 2;
            this.cmdGetFile2.Text = "...";
            this.cmdGetFile2.UseVisualStyleBackColor = true;
            this.cmdGetFile2.Click += new System.EventHandler(this.cmdGetFile2_Click);
            // 
            // ckHD2Installed
            // 
            this.ckHD2Installed.AutoSize = true;
            this.ckHD2Installed.Location = new System.Drawing.Point(585, 57);
            this.ckHD2Installed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ckHD2Installed.Name = "ckHD2Installed";
            this.ckHD2Installed.Size = new System.Drawing.Size(97, 24);
            this.ckHD2Installed.TabIndex = 6;
            this.ckHD2Installed.Text = "Installed";
            this.ckHD2Installed.UseVisualStyleBackColor = true;
            // 
            // txtSPT2
            // 
            this.txtSPT2.Location = new System.Drawing.Point(462, 57);
            this.txtSPT2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSPT2.Name = "txtSPT2";
            this.txtSPT2.Size = new System.Drawing.Size(82, 26);
            this.txtSPT2.TabIndex = 5;
            this.txtSPT2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // txtHeads2
            // 
            this.txtHeads2.Location = new System.Drawing.Point(285, 57);
            this.txtHeads2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHeads2.Name = "txtHeads2";
            this.txtHeads2.Size = new System.Drawing.Size(82, 26);
            this.txtHeads2.TabIndex = 4;
            this.txtHeads2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // txtCyl2
            // 
            this.txtCyl2.Location = new System.Drawing.Point(112, 57);
            this.txtCyl2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCyl2.Name = "txtCyl2";
            this.txtCyl2.Size = new System.Drawing.Size(82, 26);
            this.txtCyl2.TabIndex = 3;
            this.txtCyl2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // txtHD2Filename
            // 
            this.txtHD2Filename.Location = new System.Drawing.Point(112, 22);
            this.txtHD2Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHD2Filename.Name = "txtHD2Filename";
            this.txtHD2Filename.Size = new System.Drawing.Size(784, 26);
            this.txtHD2Filename.TabIndex = 1;
            this.txtHD2Filename.Validating += new System.ComponentModel.CancelEventHandler(this.txtHD2Filename_Validating);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(396, 63);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 20);
            this.label5.TabIndex = 3;
            this.label5.Text = "SPT";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(219, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "Heads";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 63);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "Cylinders";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Filename";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cmdGetFile1);
            this.groupBox4.Controls.Add(this.ckHD1Installed);
            this.groupBox4.Controls.Add(this.txtSPT1);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.txtHeads1);
            this.groupBox4.Controls.Add(this.txtHD1Filename);
            this.groupBox4.Controls.Add(this.txtCyl1);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(15, 25);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(914, 102);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Hard Drive 1";
            // 
            // cmdGetFile1
            // 
            this.cmdGetFile1.Location = new System.Drawing.Point(855, 20);
            this.cmdGetFile1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetFile1.Name = "cmdGetFile1";
            this.cmdGetFile1.Size = new System.Drawing.Size(44, 34);
            this.cmdGetFile1.TabIndex = 2;
            this.cmdGetFile1.Text = "...";
            this.cmdGetFile1.UseVisualStyleBackColor = true;
            this.cmdGetFile1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ckHD1Installed
            // 
            this.ckHD1Installed.AutoSize = true;
            this.ckHD1Installed.Location = new System.Drawing.Point(585, 60);
            this.ckHD1Installed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ckHD1Installed.Name = "ckHD1Installed";
            this.ckHD1Installed.Size = new System.Drawing.Size(95, 24);
            this.ckHD1Installed.TabIndex = 6;
            this.ckHD1Installed.Text = "Installed";
            this.ckHD1Installed.UseVisualStyleBackColor = true;
            // 
            // txtSPT1
            // 
            this.txtSPT1.Location = new System.Drawing.Point(462, 57);
            this.txtSPT1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSPT1.Name = "txtSPT1";
            this.txtSPT1.Size = new System.Drawing.Size(82, 26);
            this.txtSPT1.TabIndex = 5;
            this.txtSPT1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // txtHeads1
            // 
            this.txtHeads1.Location = new System.Drawing.Point(285, 57);
            this.txtHeads1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHeads1.Name = "txtHeads1";
            this.txtHeads1.Size = new System.Drawing.Size(82, 26);
            this.txtHeads1.TabIndex = 4;
            this.txtHeads1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // txtHD1Filename
            // 
            this.txtHD1Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtHD1Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtHD1Filename.Location = new System.Drawing.Point(112, 22);
            this.txtHD1Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHD1Filename.Name = "txtHD1Filename";
            this.txtHD1Filename.Size = new System.Drawing.Size(784, 26);
            this.txtHD1Filename.TabIndex = 1;
            this.txtHD1Filename.Validating += new System.ComponentModel.CancelEventHandler(this.txtHD1Filename_Validating);
            // 
            // txtCyl1
            // 
            this.txtCyl1.Location = new System.Drawing.Point(112, 57);
            this.txtCyl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCyl1.Name = "txtCyl1";
            this.txtCyl1.Size = new System.Drawing.Size(82, 26);
            this.txtCyl1.TabIndex = 3;
            this.txtCyl1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 63);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 20);
            this.label6.TabIndex = 3;
            this.label6.Text = "Cylinders";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(396, 63);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 20);
            this.label8.TabIndex = 3;
            this.label8.Text = "SPT";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(219, 63);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 20);
            this.label7.TabIndex = 3;
            this.label7.Text = "Heads";
            // 
            // cmdGetVidBIOSFile
            // 
            this.cmdGetVidBIOSFile.Location = new System.Drawing.Point(855, 405);
            this.cmdGetVidBIOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetVidBIOSFile.Name = "cmdGetVidBIOSFile";
            this.cmdGetVidBIOSFile.Size = new System.Drawing.Size(44, 34);
            this.cmdGetVidBIOSFile.TabIndex = 16;
            this.cmdGetVidBIOSFile.Text = "...";
            this.cmdGetVidBIOSFile.UseVisualStyleBackColor = true;
            this.cmdGetVidBIOSFile.Click += new System.EventHandler(this.cmdGetVidBIOSFile_Click);
            // 
            // cmdGetFD2
            // 
            this.cmdGetFD2.AutoSize = true;
            this.cmdGetFD2.Location = new System.Drawing.Point(670, 282);
            this.cmdGetFD2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetFD2.Name = "cmdGetFD2";
            this.cmdGetFD2.Size = new System.Drawing.Size(46, 46);
            this.cmdGetFD2.TabIndex = 8;
            this.cmdGetFD2.Text = "...";
            this.cmdGetFD2.UseVisualStyleBackColor = true;
            this.cmdGetFD2.Click += new System.EventHandler(this.cmdGetFD2_Click);
            // 
            // cmdGetBIOSFile
            // 
            this.cmdGetBIOSFile.Location = new System.Drawing.Point(855, 365);
            this.cmdGetBIOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetBIOSFile.Name = "cmdGetBIOSFile";
            this.cmdGetBIOSFile.Size = new System.Drawing.Size(44, 34);
            this.cmdGetBIOSFile.TabIndex = 14;
            this.cmdGetBIOSFile.Text = "...";
            this.cmdGetBIOSFile.UseVisualStyleBackColor = true;
            this.cmdGetBIOSFile.Click += new System.EventHandler(this.cmdGetBIOSFile_Click);
            // 
            // cmdGetFD1
            // 
            this.cmdGetFD1.AutoSize = true;
            this.cmdGetFD1.Location = new System.Drawing.Point(670, 243);
            this.cmdGetFD1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetFD1.Name = "cmdGetFD1";
            this.cmdGetFD1.Size = new System.Drawing.Size(46, 46);
            this.cmdGetFD1.TabIndex = 4;
            this.cmdGetFD1.Text = "...";
            this.cmdGetFD1.UseVisualStyleBackColor = true;
            this.cmdGetFD1.Click += new System.EventHandler(this.cmdGetFD1_Click);
            // 
            // cmdGetCMOSFile
            // 
            this.cmdGetCMOSFile.Location = new System.Drawing.Point(855, 325);
            this.cmdGetCMOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdGetCMOSFile.Name = "cmdGetCMOSFile";
            this.cmdGetCMOSFile.Size = new System.Drawing.Size(44, 34);
            this.cmdGetCMOSFile.TabIndex = 12;
            this.cmdGetCMOSFile.Text = "...";
            this.cmdGetCMOSFile.UseVisualStyleBackColor = true;
            this.cmdGetCMOSFile.Click += new System.EventHandler(this.cmdGetCMOSFile_Click);
            // 
            // txtVidBIOSFilename
            // 
            this.txtVidBIOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtVidBIOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtVidBIOSFilename.Location = new System.Drawing.Point(120, 406);
            this.txtVidBIOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtVidBIOSFilename.Name = "txtVidBIOSFilename";
            this.txtVidBIOSFilename.Size = new System.Drawing.Size(776, 26);
            this.txtVidBIOSFilename.TabIndex = 15;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(22, 411);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(93, 20);
            this.label15.TabIndex = 7;
            this.label15.Text = "Video BIOS";
            // 
            // txtFD2Filename
            // 
            this.txtFD2Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtFD2Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtFD2Filename.Location = new System.Drawing.Point(120, 285);
            this.txtFD2Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFD2Filename.Name = "txtFD2Filename";
            this.txtFD2Filename.Size = new System.Drawing.Size(590, 26);
            this.txtFD2Filename.TabIndex = 7;
            // 
            // txtBIOSFilename
            // 
            this.txtBIOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtBIOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtBIOSFilename.Location = new System.Drawing.Point(120, 366);
            this.txtBIOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtBIOSFilename.Name = "txtBIOSFilename";
            this.txtBIOSFilename.Size = new System.Drawing.Size(776, 26);
            this.txtBIOSFilename.TabIndex = 13;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(45, 289);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(69, 20);
            this.label17.TabIndex = 7;
            this.label17.Text = "Floppy 2";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(68, 371);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(48, 20);
            this.label14.TabIndex = 7;
            this.label14.Text = "BIOS";
            // 
            // txtFD1Filename
            // 
            this.txtFD1Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtFD1Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtFD1Filename.Location = new System.Drawing.Point(120, 245);
            this.txtFD1Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFD1Filename.Name = "txtFD1Filename";
            this.txtFD1Filename.Size = new System.Drawing.Size(590, 26);
            this.txtFD1Filename.TabIndex = 3;
            this.txtFD1Filename.TextChanged += new System.EventHandler(this.txtFD1Filename_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(45, 249);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(69, 20);
            this.label16.TabIndex = 7;
            this.label16.Text = "Floppy 1";
            // 
            // txtCMOSFilename
            // 
            this.txtCMOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtCMOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtCMOSFilename.Location = new System.Drawing.Point(120, 326);
            this.txtCMOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCMOSFilename.Name = "txtCMOSFilename";
            this.txtCMOSFilename.Size = new System.Drawing.Size(776, 26);
            this.txtCMOSFilename.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(58, 331);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 20);
            this.label9.TabIndex = 7;
            this.label9.Text = "CMOS";
            // 
            // tbMemory
            // 
            this.tbMemory.LargeChange = 10;
            this.tbMemory.Location = new System.Drawing.Point(9, 26);
            this.tbMemory.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbMemory.Maximum = 2048;
            this.tbMemory.Minimum = 1;
            this.tbMemory.Name = "tbMemory";
            this.tbMemory.Size = new System.Drawing.Size(218, 69);
            this.tbMemory.TabIndex = 1;
            this.tbMemory.TickFrequency = 100;
            this.tbMemory.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tbMemory.Value = 2048;
            this.tbMemory.Scroll += new System.EventHandler(this.tbMemory_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.groupBox2.Controls.Add(this.label24);
            this.groupBox2.Controls.Add(this.txtMemoryAmt);
            this.groupBox2.Controls.Add(this.tbMemory);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Location = new System.Drawing.Point(1082, 18);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Size = new System.Drawing.Size(354, 109);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Memory";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(21, 78);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(273, 20);
            this.label24.TabIndex = 12;
            this.label24.Text = "(memory amount Includes base 1MB)";
            // 
            // txtMemoryAmt
            // 
            this.txtMemoryAmt.Location = new System.Drawing.Point(230, 43);
            this.txtMemoryAmt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMemoryAmt.Name = "txtMemoryAmt";
            this.txtMemoryAmt.Size = new System.Drawing.Size(54, 26);
            this.txtMemoryAmt.TabIndex = 2;
            this.txtMemoryAmt.Text = "0000";
            this.txtMemoryAmt.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            this.txtMemoryAmt.Leave += new System.EventHandler(this.txtMemoryAmt_Leave);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(294, 48);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(33, 20);
            this.label23.TabIndex = 3;
            this.label23.Text = "MB";
            // 
            // ofdHD1
            // 
            this.ofdHD1.CheckPathExists = false;
            this.ofdHD1.DefaultExt = "img";
            this.ofdHD1.FileName = "*.img";
            // 
            // cmdOK
            // 
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(1191, 752);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(112, 35);
            this.cmdOK.TabIndex = 31;
            this.cmdOK.Text = "&Ok";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.CausesValidation = false;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(1312, 752);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(112, 35);
            this.cmdCancel.TabIndex = 32;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // clbOptions
            // 
            this.clbOptions.BackColor = System.Drawing.Color.AliceBlue;
            this.clbOptions.CheckOnClick = true;
            this.clbOptions.FormattingEnabled = true;
            this.clbOptions.Location = new System.Drawing.Point(1082, 137);
            this.clbOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.clbOptions.Name = "clbOptions";
            this.clbOptions.Size = new System.Drawing.Size(272, 280);
            this.clbOptions.TabIndex = 22;
            this.clbOptions.ThreeDCheckBoxes = true;
            this.clbOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
            // 
            // txtDebugAtCS
            // 
            this.txtDebugAtCS.Location = new System.Drawing.Point(62, 25);
            this.txtDebugAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDebugAtCS.Name = "txtDebugAtCS";
            this.txtDebugAtCS.Size = new System.Drawing.Size(85, 26);
            this.txtDebugAtCS.TabIndex = 25;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(21, 31);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(31, 20);
            this.label10.TabIndex = 10;
            this.label10.Text = "CS";
            // 
            // grpDebugAt
            // 
            this.grpDebugAt.Controls.Add(this.label11);
            this.grpDebugAt.Controls.Add(this.txtDebugAtEIP);
            this.grpDebugAt.Controls.Add(this.label10);
            this.grpDebugAt.Controls.Add(this.txtDebugAtCS);
            this.grpDebugAt.Location = new System.Drawing.Point(1080, 523);
            this.grpDebugAt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDebugAt.Name = "grpDebugAt";
            this.grpDebugAt.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDebugAt.Size = new System.Drawing.Size(354, 72);
            this.grpDebugAt.TabIndex = 12;
            this.grpDebugAt.TabStop = false;
            this.grpDebugAt.Text = "Debug-At";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(188, 31);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 20);
            this.label11.TabIndex = 10;
            this.label11.Text = "EIP";
            // 
            // txtDebugAtEIP
            // 
            this.txtDebugAtEIP.Location = new System.Drawing.Point(228, 25);
            this.txtDebugAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDebugAtEIP.Name = "txtDebugAtEIP";
            this.txtDebugAtEIP.Size = new System.Drawing.Size(85, 26);
            this.txtDebugAtEIP.TabIndex = 26;
            // 
            // grpDieAt
            // 
            this.grpDieAt.Controls.Add(this.label12);
            this.grpDieAt.Controls.Add(this.txtDieAtEIP);
            this.grpDieAt.Controls.Add(this.label13);
            this.grpDieAt.Controls.Add(this.txtDieAtCS);
            this.grpDieAt.Location = new System.Drawing.Point(1082, 589);
            this.grpDieAt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDieAt.Name = "grpDieAt";
            this.grpDieAt.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDieAt.Size = new System.Drawing.Size(354, 72);
            this.grpDieAt.TabIndex = 12;
            this.grpDieAt.TabStop = false;
            this.grpDieAt.Text = "Die-At";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(188, 31);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 20);
            this.label12.TabIndex = 10;
            this.label12.Text = "EIP";
            // 
            // txtDieAtEIP
            // 
            this.txtDieAtEIP.Location = new System.Drawing.Point(228, 25);
            this.txtDieAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDieAtEIP.Name = "txtDieAtEIP";
            this.txtDieAtEIP.Size = new System.Drawing.Size(85, 26);
            this.txtDieAtEIP.TabIndex = 28;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(21, 31);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(31, 20);
            this.label13.TabIndex = 10;
            this.label13.Text = "CS";
            // 
            // txtDieAtCS
            // 
            this.txtDieAtCS.Location = new System.Drawing.Point(62, 25);
            this.txtDieAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDieAtCS.Name = "txtDieAtCS";
            this.txtDieAtCS.Size = new System.Drawing.Size(85, 26);
            this.txtDieAtCS.TabIndex = 27;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cboBootDevice);
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.label20);
            this.groupBox5.Controls.Add(this.label19);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.txtMaxDbgFileSize);
            this.groupBox5.Controls.Add(this.txtDebugPath);
            this.groupBox5.Location = new System.Drawing.Point(40, 508);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox5.Size = new System.Drawing.Size(752, 212);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Debug";
            // 
            // cboBootDevice
            // 
            this.cboBootDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBootDevice.Items.AddRange(new object[] {
            "Floppy",
            "Hard Drive"});
            this.cboBootDevice.Location = new System.Drawing.Point(118, 106);
            this.cboBootDevice.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboBootDevice.Name = "cboBootDevice";
            this.cboBootDevice.Size = new System.Drawing.Size(180, 28);
            this.cboBootDevice.TabIndex = 20;
            this.cboBootDevice.SelectionChangeCommitted += new System.EventHandler(this.cboBootDevice_SelectionChangeCommitted);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(669, 18);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(44, 34);
            this.button1.TabIndex = 18;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(4, 111);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(84, 20);
            this.label20.TabIndex = 0;
            this.label20.Text = "Boot From";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(4, 68);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(102, 20);
            this.label19.TabIndex = 0;
            this.label19.Text = "Max File Size";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(66, 25);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(42, 20);
            this.label18.TabIndex = 0;
            this.label18.Text = "Path";
            // 
            // txtMaxDbgFileSize
            // 
            this.txtMaxDbgFileSize.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtMaxDbgFileSize.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtMaxDbgFileSize.Location = new System.Drawing.Point(118, 63);
            this.txtMaxDbgFileSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxDbgFileSize.Name = "txtMaxDbgFileSize";
            this.txtMaxDbgFileSize.Size = new System.Drawing.Size(138, 26);
            this.txtMaxDbgFileSize.TabIndex = 19;
            this.txtMaxDbgFileSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            this.txtMaxDbgFileSize.Validating += new System.ComponentModel.CancelEventHandler(this.txtMaxDbgFileSize_Validating);
            // 
            // txtDebugPath
            // 
            this.txtDebugPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtDebugPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtDebugPath.Location = new System.Drawing.Point(118, 20);
            this.txtDebugPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDebugPath.Name = "txtDebugPath";
            this.txtDebugPath.Size = new System.Drawing.Size(592, 26);
            this.txtDebugPath.TabIndex = 17;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(1076, 435);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(156, 20);
            this.label21.TabIndex = 3;
            this.label21.Text = "Timer Tick Slowdown";
            // 
            // txtTimerTick
            // 
            this.txtTimerTick.Location = new System.Drawing.Point(1240, 429);
            this.txtTimerTick.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTimerTick.Name = "txtTimerTick";
            this.txtTimerTick.Size = new System.Drawing.Size(82, 26);
            this.txtTimerTick.TabIndex = 23;
            this.txtTimerTick.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(1328, 435);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(40, 20);
            this.label22.TabIndex = 3;
            this.label22.Text = "(ms)";
            // 
            // cmdSwapHDs
            // 
            this.cmdSwapHDs.Location = new System.Drawing.Point(978, 103);
            this.cmdSwapHDs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdSwapHDs.Name = "cmdSwapHDs";
            this.cmdSwapHDs.Size = new System.Drawing.Size(88, 74);
            this.cmdSwapHDs.TabIndex = 5;
            this.cmdSwapHDs.Text = "Swap HDs";
            this.cmdSwapHDs.UseVisualStyleBackColor = true;
            this.cmdSwapHDs.Click += new System.EventHandler(this.cmdSwapHDs_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(1088, 474);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(137, 20);
            this.label25.TabIndex = 3;
            this.label25.Text = "Task Name Offset";
            // 
            // txtTaskNameOffset
            // 
            this.txtTaskNameOffset.Location = new System.Drawing.Point(1240, 468);
            this.txtTaskNameOffset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTaskNameOffset.Name = "txtTaskNameOffset";
            this.txtTaskNameOffset.Size = new System.Drawing.Size(82, 26);
            this.txtTaskNameOffset.TabIndex = 24;
            this.txtTaskNameOffset.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress_Handler);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label26);
            this.groupBox6.Controls.Add(this.txtDumpAtEIP);
            this.groupBox6.Controls.Add(this.label27);
            this.groupBox6.Controls.Add(this.txtDumpAtCS);
            this.groupBox6.Location = new System.Drawing.Point(1082, 657);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox6.Size = new System.Drawing.Size(354, 72);
            this.groupBox6.TabIndex = 12;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Dump Mem At";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(188, 31);
            this.label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(35, 20);
            this.label26.TabIndex = 10;
            this.label26.Text = "EIP";
            // 
            // txtDumpAtEIP
            // 
            this.txtDumpAtEIP.Location = new System.Drawing.Point(228, 25);
            this.txtDumpAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDumpAtEIP.Name = "txtDumpAtEIP";
            this.txtDumpAtEIP.Size = new System.Drawing.Size(85, 26);
            this.txtDumpAtEIP.TabIndex = 30;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(21, 31);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(31, 20);
            this.label27.TabIndex = 10;
            this.label27.Text = "CS";
            // 
            // txtDumpAtCS
            // 
            this.txtDumpAtCS.Location = new System.Drawing.Point(62, 25);
            this.txtDumpAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDumpAtCS.Name = "txtDumpAtCS";
            this.txtDumpAtCS.Size = new System.Drawing.Size(85, 26);
            this.txtDumpAtCS.TabIndex = 29;
            // 
            // cmdSwapFDs
            // 
            this.cmdSwapFDs.Location = new System.Drawing.Point(978, 254);
            this.cmdSwapFDs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdSwapFDs.Name = "cmdSwapFDs";
            this.cmdSwapFDs.Size = new System.Drawing.Size(88, 74);
            this.cmdSwapFDs.TabIndex = 5;
            this.cmdSwapFDs.Text = "Swap FDs";
            this.cmdSwapFDs.UseVisualStyleBackColor = true;
            this.cmdSwapFDs.Click += new System.EventHandler(this.cmdSwapFDs_Click);
            // 
            // frmConfig
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(1449, 806);
            this.Controls.Add(this.cmdSwapFDs);
            this.Controls.Add(this.cmdSwapHDs);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.grpDieAt);
            this.Controls.Add(this.grpDebugAt);
            this.Controls.Add(this.clbOptions);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtTaskNameOffset);
            this.Controls.Add(this.txtTimerTick);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Emulator Settings";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMemory)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.grpDebugAt.ResumeLayout(false);
            this.grpDebugAt.PerformLayout();
            this.grpDieAt.ResumeLayout(false);
            this.grpDieAt.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtHD1Filename;
        private System.Windows.Forms.TrackBar tbMemory;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtMemoryAmt;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog ofdHD1;
        private System.Windows.Forms.TextBox txtHD2Filename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtSPT2;
        private System.Windows.Forms.TextBox txtHeads2;
        private System.Windows.Forms.TextBox txtCyl2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSPT1;
        private System.Windows.Forms.TextBox txtHeads1;
        private System.Windows.Forms.TextBox txtCyl1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckedListBox clbOptions;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCMOSFilename;
        private System.Windows.Forms.TextBox txtDebugAtCS;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button cmdGetCMOSFile;
        private System.Windows.Forms.GroupBox grpDebugAt;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtDebugAtEIP;
        private System.Windows.Forms.GroupBox grpDieAt;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtDieAtEIP;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtDieAtCS;
        private System.Windows.Forms.Button cmdGetVidBIOSFile;
        private System.Windows.Forms.Button cmdGetBIOSFile;
        private System.Windows.Forms.TextBox txtVidBIOSFilename;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtBIOSFilename;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button cmdGetFD2;
        private System.Windows.Forms.Button cmdGetFD1;
        private System.Windows.Forms.TextBox txtFD2Filename;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtFD1Filename;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox ckFD2Enabled;
        private System.Windows.Forms.CheckBox ckFD1Enabled;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtDebugPath;
        private System.Windows.Forms.FolderBrowserDialog fb1;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtMaxDbgFileSize;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtTimerTick;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button cmdSwapHDs;
        private System.Windows.Forms.CheckBox ckHD2Installed;
        private System.Windows.Forms.CheckBox ckHD1Installed;
        private System.Windows.Forms.ComboBox cboBootDevice;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.ComboBox cbFD1Capacity;
        private System.Windows.Forms.ComboBox cbFD2Capacity;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox txtTaskNameOffset;
        private System.Windows.Forms.Button cmdGetFile2;
        private System.Windows.Forms.Button cmdGetFile1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox txtDumpAtEIP;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox txtDumpAtCS;
        private System.Windows.Forms.Button cmdSwapFDs;
    }
}