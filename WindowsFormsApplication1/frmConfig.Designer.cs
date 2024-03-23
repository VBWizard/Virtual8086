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
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            cbFD2Capacity = new System.Windows.Forms.ComboBox();
            cbFD1Capacity = new System.Windows.Forms.ComboBox();
            ckFD2Enabled = new System.Windows.Forms.CheckBox();
            ckFD1Enabled = new System.Windows.Forms.CheckBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            cmdGetFile2 = new System.Windows.Forms.Button();
            ckHD2Installed = new System.Windows.Forms.CheckBox();
            txtSPT2 = new System.Windows.Forms.TextBox();
            txtHeads2 = new System.Windows.Forms.TextBox();
            txtCyl2 = new System.Windows.Forms.TextBox();
            txtHD2Filename = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox4 = new System.Windows.Forms.GroupBox();
            cmdGetFile1 = new System.Windows.Forms.Button();
            ckHD1Installed = new System.Windows.Forms.CheckBox();
            txtSPT1 = new System.Windows.Forms.TextBox();
            txtHeads1 = new System.Windows.Forms.TextBox();
            txtHD1Filename = new System.Windows.Forms.TextBox();
            txtCyl1 = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            cmdGetVidBIOSFile = new System.Windows.Forms.Button();
            cmdGetFD2 = new System.Windows.Forms.Button();
            cmdGetBIOSFile = new System.Windows.Forms.Button();
            cmdGetFD1 = new System.Windows.Forms.Button();
            cmdGetCMOSFile = new System.Windows.Forms.Button();
            txtVidBIOSFilename = new System.Windows.Forms.TextBox();
            label15 = new System.Windows.Forms.Label();
            txtFD2Filename = new System.Windows.Forms.TextBox();
            txtBIOSFilename = new System.Windows.Forms.TextBox();
            label17 = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            txtFD1Filename = new System.Windows.Forms.TextBox();
            label16 = new System.Windows.Forms.Label();
            txtCMOSFilename = new System.Windows.Forms.TextBox();
            label9 = new System.Windows.Forms.Label();
            tbMemory = new System.Windows.Forms.TrackBar();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label24 = new System.Windows.Forms.Label();
            txtMemoryAmt = new System.Windows.Forms.TextBox();
            label23 = new System.Windows.Forms.Label();
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ofdHD1 = new System.Windows.Forms.OpenFileDialog();
            cmdOK = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            clbOptions = new System.Windows.Forms.CheckedListBox();
            txtDebugAtCS = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            grpDebugAt = new System.Windows.Forms.GroupBox();
            label11 = new System.Windows.Forms.Label();
            txtDebugAtEIP = new System.Windows.Forms.TextBox();
            grpDieAt = new System.Windows.Forms.GroupBox();
            label12 = new System.Windows.Forms.Label();
            txtDieAtEIP = new System.Windows.Forms.TextBox();
            label13 = new System.Windows.Forms.Label();
            txtDieAtCS = new System.Windows.Forms.TextBox();
            groupBox5 = new System.Windows.Forms.GroupBox();
            cboBootDevice = new System.Windows.Forms.ComboBox();
            button1 = new System.Windows.Forms.Button();
            label20 = new System.Windows.Forms.Label();
            label19 = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            txtMaxDbgFileSize = new System.Windows.Forms.TextBox();
            txtDebugPath = new System.Windows.Forms.TextBox();
            fb1 = new System.Windows.Forms.FolderBrowserDialog();
            label21 = new System.Windows.Forms.Label();
            txtTimerTick = new System.Windows.Forms.TextBox();
            label22 = new System.Windows.Forms.Label();
            cmdSwapHDs = new System.Windows.Forms.Button();
            label25 = new System.Windows.Forms.Label();
            txtTaskNameOffset = new System.Windows.Forms.TextBox();
            groupBox6 = new System.Windows.Forms.GroupBox();
            label26 = new System.Windows.Forms.Label();
            txtDumpAtEIP = new System.Windows.Forms.TextBox();
            label27 = new System.Windows.Forms.Label();
            txtDumpAtCS = new System.Windows.Forms.TextBox();
            cmdSwapFDs = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbMemory).BeginInit();
            groupBox2.SuspendLayout();
            grpDebugAt.SuspendLayout();
            grpDieAt.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox6.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(32, 26);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 20);
            label1.TabIndex = 0;
            label1.Text = "Filename";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            groupBox1.Controls.Add(cbFD2Capacity);
            groupBox1.Controls.Add(cbFD1Capacity);
            groupBox1.Controls.Add(ckFD2Enabled);
            groupBox1.Controls.Add(ckFD1Enabled);
            groupBox1.Controls.Add(groupBox3);
            groupBox1.Controls.Add(groupBox4);
            groupBox1.Controls.Add(cmdGetVidBIOSFile);
            groupBox1.Controls.Add(cmdGetFD2);
            groupBox1.Controls.Add(cmdGetBIOSFile);
            groupBox1.Controls.Add(cmdGetFD1);
            groupBox1.Controls.Add(cmdGetCMOSFile);
            groupBox1.Controls.Add(txtVidBIOSFilename);
            groupBox1.Controls.Add(label15);
            groupBox1.Controls.Add(txtFD2Filename);
            groupBox1.Controls.Add(txtBIOSFilename);
            groupBox1.Controls.Add(label17);
            groupBox1.Controls.Add(label14);
            groupBox1.Controls.Add(txtFD1Filename);
            groupBox1.Controls.Add(label16);
            groupBox1.Controls.Add(txtCMOSFilename);
            groupBox1.Controls.Add(label9);
            groupBox1.Location = new System.Drawing.Point(36, 18);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox1.Size = new System.Drawing.Size(834, 453);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Files";
            // 
            // cbFD2Capacity
            // 
            cbFD2Capacity.FormattingEnabled = true;
            cbFD2Capacity.Items.AddRange(new object[] { "720K", "1_2M", "1_44M", "2_88M" });
            cbFD2Capacity.Location = new System.Drawing.Point(651, 282);
            cbFD2Capacity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbFD2Capacity.Name = "cbFD2Capacity";
            cbFD2Capacity.Size = new System.Drawing.Size(88, 28);
            cbFD2Capacity.TabIndex = 9;
            // 
            // cbFD1Capacity
            // 
            cbFD1Capacity.FormattingEnabled = true;
            cbFD1Capacity.Items.AddRange(new object[] { "720K", "1_2M", "1_44M", "2_88M" });
            cbFD1Capacity.Location = new System.Drawing.Point(651, 245);
            cbFD1Capacity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbFD1Capacity.Name = "cbFD1Capacity";
            cbFD1Capacity.Size = new System.Drawing.Size(89, 28);
            cbFD1Capacity.TabIndex = 5;
            // 
            // ckFD2Enabled
            // 
            ckFD2Enabled.AutoSize = true;
            ckFD2Enabled.Location = new System.Drawing.Point(748, 282);
            ckFD2Enabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            ckFD2Enabled.Name = "ckFD2Enabled";
            ckFD2Enabled.Size = new System.Drawing.Size(84, 24);
            ckFD2Enabled.TabIndex = 10;
            ckFD2Enabled.Text = "Inserted";
            ckFD2Enabled.UseVisualStyleBackColor = true;
            // 
            // ckFD1Enabled
            // 
            ckFD1Enabled.AutoSize = true;
            ckFD1Enabled.Location = new System.Drawing.Point(748, 249);
            ckFD1Enabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            ckFD1Enabled.Name = "ckFD1Enabled";
            ckFD1Enabled.Size = new System.Drawing.Size(84, 24);
            ckFD1Enabled.TabIndex = 6;
            ckFD1Enabled.Text = "Inserted";
            ckFD1Enabled.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cmdGetFile2);
            groupBox3.Controls.Add(ckHD2Installed);
            groupBox3.Controls.Add(txtSPT2);
            groupBox3.Controls.Add(txtHeads2);
            groupBox3.Controls.Add(txtCyl2);
            groupBox3.Controls.Add(txtHD2Filename);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(label2);
            groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox3.Location = new System.Drawing.Point(13, 128);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox3.Size = new System.Drawing.Size(812, 106);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Hard Drive 2";
            // 
            // cmdGetFile2
            // 
            cmdGetFile2.Location = new System.Drawing.Point(760, 20);
            cmdGetFile2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetFile2.Name = "cmdGetFile2";
            cmdGetFile2.Size = new System.Drawing.Size(39, 34);
            cmdGetFile2.TabIndex = 2;
            cmdGetFile2.Text = "...";
            cmdGetFile2.UseVisualStyleBackColor = true;
            cmdGetFile2.Click += cmdGetFile2_Click;
            // 
            // ckHD2Installed
            // 
            ckHD2Installed.AutoSize = true;
            ckHD2Installed.Location = new System.Drawing.Point(520, 57);
            ckHD2Installed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            ckHD2Installed.Name = "ckHD2Installed";
            ckHD2Installed.Size = new System.Drawing.Size(82, 21);
            ckHD2Installed.TabIndex = 6;
            ckHD2Installed.Text = "Installed";
            ckHD2Installed.UseVisualStyleBackColor = true;
            // 
            // txtSPT2
            // 
            txtSPT2.Location = new System.Drawing.Point(411, 57);
            txtSPT2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtSPT2.Name = "txtSPT2";
            txtSPT2.Size = new System.Drawing.Size(73, 23);
            txtSPT2.TabIndex = 5;
            txtSPT2.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // txtHeads2
            // 
            txtHeads2.Location = new System.Drawing.Point(253, 57);
            txtHeads2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtHeads2.Name = "txtHeads2";
            txtHeads2.Size = new System.Drawing.Size(73, 23);
            txtHeads2.TabIndex = 4;
            txtHeads2.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // txtCyl2
            // 
            txtCyl2.Location = new System.Drawing.Point(100, 57);
            txtCyl2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtCyl2.Name = "txtCyl2";
            txtCyl2.Size = new System.Drawing.Size(73, 23);
            txtCyl2.TabIndex = 3;
            txtCyl2.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // txtHD2Filename
            // 
            txtHD2Filename.Location = new System.Drawing.Point(100, 22);
            txtHD2Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtHD2Filename.Name = "txtHD2Filename";
            txtHD2Filename.Size = new System.Drawing.Size(697, 23);
            txtHD2Filename.TabIndex = 1;
            txtHD2Filename.Validating += txtHD2Filename_Validating;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(352, 63);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(35, 17);
            label5.TabIndex = 3;
            label5.Text = "SPT";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(195, 63);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(49, 17);
            label4.TabIndex = 3;
            label4.Text = "Heads";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(32, 63);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(66, 17);
            label3.TabIndex = 3;
            label3.Text = "Cylinders";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(32, 31);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(65, 17);
            label2.TabIndex = 3;
            label2.Text = "Filename";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(cmdGetFile1);
            groupBox4.Controls.Add(ckHD1Installed);
            groupBox4.Controls.Add(txtSPT1);
            groupBox4.Controls.Add(label1);
            groupBox4.Controls.Add(txtHeads1);
            groupBox4.Controls.Add(txtHD1Filename);
            groupBox4.Controls.Add(txtCyl1);
            groupBox4.Controls.Add(label6);
            groupBox4.Controls.Add(label8);
            groupBox4.Controls.Add(label7);
            groupBox4.Location = new System.Drawing.Point(13, 25);
            groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox4.Size = new System.Drawing.Size(812, 102);
            groupBox4.TabIndex = 1;
            groupBox4.TabStop = false;
            groupBox4.Text = "Hard Drive 1";
            // 
            // cmdGetFile1
            // 
            cmdGetFile1.Location = new System.Drawing.Point(760, 20);
            cmdGetFile1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetFile1.Name = "cmdGetFile1";
            cmdGetFile1.Size = new System.Drawing.Size(39, 34);
            cmdGetFile1.TabIndex = 2;
            cmdGetFile1.Text = "...";
            cmdGetFile1.UseVisualStyleBackColor = true;
            cmdGetFile1.Click += button1_Click;
            // 
            // ckHD1Installed
            // 
            ckHD1Installed.AutoSize = true;
            ckHD1Installed.Location = new System.Drawing.Point(520, 60);
            ckHD1Installed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            ckHD1Installed.Name = "ckHD1Installed";
            ckHD1Installed.Size = new System.Drawing.Size(87, 24);
            ckHD1Installed.TabIndex = 6;
            ckHD1Installed.Text = "Installed";
            ckHD1Installed.UseVisualStyleBackColor = true;
            // 
            // txtSPT1
            // 
            txtSPT1.Location = new System.Drawing.Point(411, 57);
            txtSPT1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtSPT1.Name = "txtSPT1";
            txtSPT1.Size = new System.Drawing.Size(73, 27);
            txtSPT1.TabIndex = 5;
            txtSPT1.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // txtHeads1
            // 
            txtHeads1.Location = new System.Drawing.Point(253, 57);
            txtHeads1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtHeads1.Name = "txtHeads1";
            txtHeads1.Size = new System.Drawing.Size(73, 27);
            txtHeads1.TabIndex = 4;
            txtHeads1.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // txtHD1Filename
            // 
            txtHD1Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtHD1Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtHD1Filename.Location = new System.Drawing.Point(100, 22);
            txtHD1Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtHD1Filename.Name = "txtHD1Filename";
            txtHD1Filename.Size = new System.Drawing.Size(697, 27);
            txtHD1Filename.TabIndex = 1;
            txtHD1Filename.Validating += txtHD1Filename_Validating;
            // 
            // txtCyl1
            // 
            txtCyl1.Location = new System.Drawing.Point(100, 57);
            txtCyl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtCyl1.Name = "txtCyl1";
            txtCyl1.Size = new System.Drawing.Size(73, 27);
            txtCyl1.TabIndex = 3;
            txtCyl1.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(32, 63);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(69, 20);
            label6.TabIndex = 3;
            label6.Text = "Cylinders";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(352, 63);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(33, 20);
            label8.TabIndex = 3;
            label8.Text = "SPT";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(195, 63);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(51, 20);
            label7.TabIndex = 3;
            label7.Text = "Heads";
            // 
            // cmdGetVidBIOSFile
            // 
            cmdGetVidBIOSFile.Location = new System.Drawing.Point(760, 405);
            cmdGetVidBIOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetVidBIOSFile.Name = "cmdGetVidBIOSFile";
            cmdGetVidBIOSFile.Size = new System.Drawing.Size(39, 34);
            cmdGetVidBIOSFile.TabIndex = 16;
            cmdGetVidBIOSFile.Text = "...";
            cmdGetVidBIOSFile.UseVisualStyleBackColor = true;
            cmdGetVidBIOSFile.Click += cmdGetVidBIOSFile_Click;
            // 
            // cmdGetFD2
            // 
            cmdGetFD2.AutoSize = true;
            cmdGetFD2.Location = new System.Drawing.Point(596, 282);
            cmdGetFD2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetFD2.Name = "cmdGetFD2";
            cmdGetFD2.Size = new System.Drawing.Size(41, 46);
            cmdGetFD2.TabIndex = 8;
            cmdGetFD2.Text = "...";
            cmdGetFD2.UseVisualStyleBackColor = true;
            cmdGetFD2.Click += cmdGetFD2_Click;
            // 
            // cmdGetBIOSFile
            // 
            cmdGetBIOSFile.Location = new System.Drawing.Point(760, 365);
            cmdGetBIOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetBIOSFile.Name = "cmdGetBIOSFile";
            cmdGetBIOSFile.Size = new System.Drawing.Size(39, 34);
            cmdGetBIOSFile.TabIndex = 14;
            cmdGetBIOSFile.Text = "...";
            cmdGetBIOSFile.UseVisualStyleBackColor = true;
            cmdGetBIOSFile.Click += cmdGetBIOSFile_Click;
            // 
            // cmdGetFD1
            // 
            cmdGetFD1.AutoSize = true;
            cmdGetFD1.Location = new System.Drawing.Point(596, 243);
            cmdGetFD1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetFD1.Name = "cmdGetFD1";
            cmdGetFD1.Size = new System.Drawing.Size(41, 46);
            cmdGetFD1.TabIndex = 4;
            cmdGetFD1.Text = "...";
            cmdGetFD1.UseVisualStyleBackColor = true;
            cmdGetFD1.Click += cmdGetFD1_Click;
            // 
            // cmdGetCMOSFile
            // 
            cmdGetCMOSFile.Location = new System.Drawing.Point(760, 325);
            cmdGetCMOSFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdGetCMOSFile.Name = "cmdGetCMOSFile";
            cmdGetCMOSFile.Size = new System.Drawing.Size(39, 34);
            cmdGetCMOSFile.TabIndex = 12;
            cmdGetCMOSFile.Text = "...";
            cmdGetCMOSFile.UseVisualStyleBackColor = true;
            cmdGetCMOSFile.Click += cmdGetCMOSFile_Click;
            // 
            // txtVidBIOSFilename
            // 
            txtVidBIOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtVidBIOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtVidBIOSFilename.Location = new System.Drawing.Point(107, 406);
            txtVidBIOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtVidBIOSFilename.Name = "txtVidBIOSFilename";
            txtVidBIOSFilename.Size = new System.Drawing.Size(690, 27);
            txtVidBIOSFilename.TabIndex = 15;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(20, 411);
            label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(84, 20);
            label15.TabIndex = 7;
            label15.Text = "Video BIOS";
            // 
            // txtFD2Filename
            // 
            txtFD2Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtFD2Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtFD2Filename.Location = new System.Drawing.Point(107, 285);
            txtFD2Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtFD2Filename.Name = "txtFD2Filename";
            txtFD2Filename.Size = new System.Drawing.Size(525, 27);
            txtFD2Filename.TabIndex = 7;
            // 
            // txtBIOSFilename
            // 
            txtBIOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtBIOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtBIOSFilename.Location = new System.Drawing.Point(107, 366);
            txtBIOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtBIOSFilename.Name = "txtBIOSFilename";
            txtBIOSFilename.Size = new System.Drawing.Size(690, 27);
            txtBIOSFilename.TabIndex = 13;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(40, 289);
            label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(66, 20);
            label17.TabIndex = 7;
            label17.Text = "Floppy 2";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(60, 371);
            label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(41, 20);
            label14.TabIndex = 7;
            label14.Text = "BIOS";
            // 
            // txtFD1Filename
            // 
            txtFD1Filename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtFD1Filename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtFD1Filename.Location = new System.Drawing.Point(107, 245);
            txtFD1Filename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtFD1Filename.Name = "txtFD1Filename";
            txtFD1Filename.Size = new System.Drawing.Size(525, 27);
            txtFD1Filename.TabIndex = 3;
            txtFD1Filename.TextChanged += txtFD1Filename_TextChanged;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new System.Drawing.Point(40, 249);
            label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(66, 20);
            label16.TabIndex = 7;
            label16.Text = "Floppy 1";
            // 
            // txtCMOSFilename
            // 
            txtCMOSFilename.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtCMOSFilename.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtCMOSFilename.Location = new System.Drawing.Point(107, 326);
            txtCMOSFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtCMOSFilename.Name = "txtCMOSFilename";
            txtCMOSFilename.Size = new System.Drawing.Size(690, 27);
            txtCMOSFilename.TabIndex = 11;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(52, 331);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(50, 20);
            label9.TabIndex = 7;
            label9.Text = "CMOS";
            // 
            // tbMemory
            // 
            tbMemory.LargeChange = 10;
            tbMemory.Location = new System.Drawing.Point(8, 26);
            tbMemory.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tbMemory.Maximum = 2048;
            tbMemory.Minimum = 1;
            tbMemory.Name = "tbMemory";
            tbMemory.Size = new System.Drawing.Size(194, 56);
            tbMemory.TabIndex = 1;
            tbMemory.TickFrequency = 100;
            tbMemory.TickStyle = System.Windows.Forms.TickStyle.Both;
            tbMemory.Value = 2048;
            tbMemory.Scroll += tbMemory_Scroll;
            // 
            // groupBox2
            // 
            groupBox2.BackColor = System.Drawing.Color.FromArgb(192, 192, 255);
            groupBox2.Controls.Add(label24);
            groupBox2.Controls.Add(txtMemoryAmt);
            groupBox2.Controls.Add(tbMemory);
            groupBox2.Controls.Add(label23);
            groupBox2.Location = new System.Drawing.Point(962, 18);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox2.Size = new System.Drawing.Size(315, 109);
            groupBox2.TabIndex = 21;
            groupBox2.TabStop = false;
            groupBox2.Text = "Memory";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new System.Drawing.Point(19, 78);
            label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(256, 20);
            label24.TabIndex = 12;
            label24.Text = "(memory amount Includes base 1MB)";
            // 
            // txtMemoryAmt
            // 
            txtMemoryAmt.Location = new System.Drawing.Point(204, 43);
            txtMemoryAmt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMemoryAmt.Name = "txtMemoryAmt";
            txtMemoryAmt.Size = new System.Drawing.Size(48, 27);
            txtMemoryAmt.TabIndex = 2;
            txtMemoryAmt.Text = "0000";
            txtMemoryAmt.KeyPress += NumericOnly_KeyPress_Handler;
            txtMemoryAmt.Leave += txtMemoryAmt_Leave;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new System.Drawing.Point(261, 48);
            label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(31, 20);
            label23.TabIndex = 3;
            label23.Text = "MB";
            // 
            // ofdHD1
            // 
            ofdHD1.CheckPathExists = false;
            ofdHD1.DefaultExt = "img";
            ofdHD1.FileName = "*.img";
            // 
            // cmdOK
            // 
            cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            cmdOK.Location = new System.Drawing.Point(1059, 752);
            cmdOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(100, 35);
            cmdOK.TabIndex = 31;
            cmdOK.Text = "&Ok";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.CausesValidation = false;
            cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cmdCancel.Location = new System.Drawing.Point(1166, 752);
            cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(100, 35);
            cmdCancel.TabIndex = 32;
            cmdCancel.Text = "&Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // clbOptions
            // 
            clbOptions.BackColor = System.Drawing.Color.AliceBlue;
            clbOptions.CheckOnClick = true;
            clbOptions.FormattingEnabled = true;
            clbOptions.Location = new System.Drawing.Point(962, 137);
            clbOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            clbOptions.Name = "clbOptions";
            clbOptions.Size = new System.Drawing.Size(242, 268);
            clbOptions.TabIndex = 22;
            clbOptions.ThreeDCheckBoxes = true;
            clbOptions.ItemCheck += checkedListBox1_ItemCheck;
            // 
            // txtDebugAtCS
            // 
            txtDebugAtCS.Location = new System.Drawing.Point(55, 25);
            txtDebugAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDebugAtCS.Name = "txtDebugAtCS";
            txtDebugAtCS.Size = new System.Drawing.Size(76, 27);
            txtDebugAtCS.TabIndex = 25;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(19, 31);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(26, 20);
            label10.TabIndex = 10;
            label10.Text = "CS";
            // 
            // grpDebugAt
            // 
            grpDebugAt.Controls.Add(label11);
            grpDebugAt.Controls.Add(txtDebugAtEIP);
            grpDebugAt.Controls.Add(label10);
            grpDebugAt.Controls.Add(txtDebugAtCS);
            grpDebugAt.Location = new System.Drawing.Point(960, 523);
            grpDebugAt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            grpDebugAt.Name = "grpDebugAt";
            grpDebugAt.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            grpDebugAt.Size = new System.Drawing.Size(315, 72);
            grpDebugAt.TabIndex = 12;
            grpDebugAt.TabStop = false;
            grpDebugAt.Text = "Debug-At";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(167, 31);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(29, 20);
            label11.TabIndex = 10;
            label11.Text = "EIP";
            // 
            // txtDebugAtEIP
            // 
            txtDebugAtEIP.Location = new System.Drawing.Point(203, 25);
            txtDebugAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDebugAtEIP.Name = "txtDebugAtEIP";
            txtDebugAtEIP.Size = new System.Drawing.Size(76, 27);
            txtDebugAtEIP.TabIndex = 26;
            // 
            // grpDieAt
            // 
            grpDieAt.Controls.Add(label12);
            grpDieAt.Controls.Add(txtDieAtEIP);
            grpDieAt.Controls.Add(label13);
            grpDieAt.Controls.Add(txtDieAtCS);
            grpDieAt.Location = new System.Drawing.Point(962, 589);
            grpDieAt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            grpDieAt.Name = "grpDieAt";
            grpDieAt.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            grpDieAt.Size = new System.Drawing.Size(315, 72);
            grpDieAt.TabIndex = 12;
            grpDieAt.TabStop = false;
            grpDieAt.Text = "Die-At";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(167, 31);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(29, 20);
            label12.TabIndex = 10;
            label12.Text = "EIP";
            // 
            // txtDieAtEIP
            // 
            txtDieAtEIP.Location = new System.Drawing.Point(203, 25);
            txtDieAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDieAtEIP.Name = "txtDieAtEIP";
            txtDieAtEIP.Size = new System.Drawing.Size(76, 27);
            txtDieAtEIP.TabIndex = 28;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(19, 31);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(26, 20);
            label13.TabIndex = 10;
            label13.Text = "CS";
            // 
            // txtDieAtCS
            // 
            txtDieAtCS.Location = new System.Drawing.Point(55, 25);
            txtDieAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDieAtCS.Name = "txtDieAtCS";
            txtDieAtCS.Size = new System.Drawing.Size(76, 27);
            txtDieAtCS.TabIndex = 27;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(cboBootDevice);
            groupBox5.Controls.Add(button1);
            groupBox5.Controls.Add(label20);
            groupBox5.Controls.Add(label19);
            groupBox5.Controls.Add(label18);
            groupBox5.Controls.Add(txtMaxDbgFileSize);
            groupBox5.Controls.Add(txtDebugPath);
            groupBox5.Location = new System.Drawing.Point(36, 508);
            groupBox5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox5.Size = new System.Drawing.Size(668, 212);
            groupBox5.TabIndex = 13;
            groupBox5.TabStop = false;
            groupBox5.Text = "Debug";
            // 
            // cboBootDevice
            // 
            cboBootDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboBootDevice.Items.AddRange(new object[] { "Floppy", "Hard Drive" });
            cboBootDevice.Location = new System.Drawing.Point(105, 106);
            cboBootDevice.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cboBootDevice.Name = "cboBootDevice";
            cboBootDevice.Size = new System.Drawing.Size(160, 28);
            cboBootDevice.TabIndex = 20;
            cboBootDevice.SelectionChangeCommitted += cboBootDevice_SelectionChangeCommitted;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button1.Location = new System.Drawing.Point(595, 18);
            button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(39, 34);
            button1.TabIndex = 18;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(4, 111);
            label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(79, 20);
            label20.TabIndex = 0;
            label20.Text = "Boot From";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(4, 68);
            label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(95, 20);
            label19.TabIndex = 0;
            label19.Text = "Max File Size";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(59, 25);
            label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(37, 20);
            label18.TabIndex = 0;
            label18.Text = "Path";
            // 
            // txtMaxDbgFileSize
            // 
            txtMaxDbgFileSize.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtMaxDbgFileSize.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtMaxDbgFileSize.Location = new System.Drawing.Point(105, 63);
            txtMaxDbgFileSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMaxDbgFileSize.Name = "txtMaxDbgFileSize";
            txtMaxDbgFileSize.Size = new System.Drawing.Size(123, 27);
            txtMaxDbgFileSize.TabIndex = 19;
            txtMaxDbgFileSize.KeyPress += NumericOnly_KeyPress_Handler;
            txtMaxDbgFileSize.Validating += txtMaxDbgFileSize_Validating;
            // 
            // txtDebugPath
            // 
            txtDebugPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            txtDebugPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            txtDebugPath.Location = new System.Drawing.Point(105, 20);
            txtDebugPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDebugPath.Name = "txtDebugPath";
            txtDebugPath.Size = new System.Drawing.Size(527, 27);
            txtDebugPath.TabIndex = 17;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new System.Drawing.Point(956, 435);
            label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(150, 20);
            label21.TabIndex = 3;
            label21.Text = "Timer Tick Slowdown";
            // 
            // txtTimerTick
            // 
            txtTimerTick.Location = new System.Drawing.Point(1102, 429);
            txtTimerTick.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtTimerTick.Name = "txtTimerTick";
            txtTimerTick.Size = new System.Drawing.Size(73, 27);
            txtTimerTick.TabIndex = 23;
            txtTimerTick.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(1180, 435);
            label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(38, 20);
            label22.TabIndex = 3;
            label22.Text = "(ms)";
            // 
            // cmdSwapHDs
            // 
            cmdSwapHDs.Location = new System.Drawing.Point(869, 103);
            cmdSwapHDs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdSwapHDs.Name = "cmdSwapHDs";
            cmdSwapHDs.Size = new System.Drawing.Size(78, 74);
            cmdSwapHDs.TabIndex = 5;
            cmdSwapHDs.Text = "Swap HDs";
            cmdSwapHDs.UseVisualStyleBackColor = true;
            cmdSwapHDs.Click += cmdSwapHDs_Click;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(967, 474);
            label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(124, 20);
            label25.TabIndex = 3;
            label25.Text = "Task Name Offset";
            // 
            // txtTaskNameOffset
            // 
            txtTaskNameOffset.Location = new System.Drawing.Point(1102, 468);
            txtTaskNameOffset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtTaskNameOffset.Name = "txtTaskNameOffset";
            txtTaskNameOffset.Size = new System.Drawing.Size(73, 27);
            txtTaskNameOffset.TabIndex = 24;
            txtTaskNameOffset.KeyPress += NumericOnly_KeyPress_Handler;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(label26);
            groupBox6.Controls.Add(txtDumpAtEIP);
            groupBox6.Controls.Add(label27);
            groupBox6.Controls.Add(txtDumpAtCS);
            groupBox6.Location = new System.Drawing.Point(962, 657);
            groupBox6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox6.Size = new System.Drawing.Size(315, 72);
            groupBox6.TabIndex = 12;
            groupBox6.TabStop = false;
            groupBox6.Text = "Dump Mem At";
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new System.Drawing.Point(167, 31);
            label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(29, 20);
            label26.TabIndex = 10;
            label26.Text = "EIP";
            // 
            // txtDumpAtEIP
            // 
            txtDumpAtEIP.Location = new System.Drawing.Point(203, 25);
            txtDumpAtEIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDumpAtEIP.Name = "txtDumpAtEIP";
            txtDumpAtEIP.Size = new System.Drawing.Size(76, 27);
            txtDumpAtEIP.TabIndex = 30;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new System.Drawing.Point(19, 31);
            label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label27.Name = "label27";
            label27.Size = new System.Drawing.Size(26, 20);
            label27.TabIndex = 10;
            label27.Text = "CS";
            // 
            // txtDumpAtCS
            // 
            txtDumpAtCS.Location = new System.Drawing.Point(55, 25);
            txtDumpAtCS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtDumpAtCS.Name = "txtDumpAtCS";
            txtDumpAtCS.Size = new System.Drawing.Size(76, 27);
            txtDumpAtCS.TabIndex = 29;
            // 
            // cmdSwapFDs
            // 
            cmdSwapFDs.Location = new System.Drawing.Point(869, 254);
            cmdSwapFDs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmdSwapFDs.Name = "cmdSwapFDs";
            cmdSwapFDs.Size = new System.Drawing.Size(78, 74);
            cmdSwapFDs.TabIndex = 5;
            cmdSwapFDs.Text = "Swap FDs";
            cmdSwapFDs.UseVisualStyleBackColor = true;
            cmdSwapFDs.Click += cmdSwapFDs_Click;
            // 
            // frmConfig
            // 
            AcceptButton = cmdOK;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cmdCancel;
            ClientSize = new System.Drawing.Size(1288, 806);
            Controls.Add(cmdSwapFDs);
            Controls.Add(cmdSwapHDs);
            Controls.Add(groupBox5);
            Controls.Add(groupBox6);
            Controls.Add(grpDieAt);
            Controls.Add(grpDebugAt);
            Controls.Add(clbOptions);
            Controls.Add(cmdCancel);
            Controls.Add(txtTaskNameOffset);
            Controls.Add(txtTimerTick);
            Controls.Add(label25);
            Controls.Add(label22);
            Controls.Add(label21);
            Controls.Add(cmdOK);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "frmConfig";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Emulator Settings";
            Load += frmConfig_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbMemory).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            grpDebugAt.ResumeLayout(false);
            grpDebugAt.PerformLayout();
            grpDieAt.ResumeLayout(false);
            grpDieAt.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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