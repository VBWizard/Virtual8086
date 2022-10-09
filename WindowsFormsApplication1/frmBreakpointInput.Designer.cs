namespace WindowsFormsApplication1
{
    partial class frmBreakpointInput
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
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtCS = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtEIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ckRemoveOnHit = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtIntNum = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.txtFunction = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ckDOSFunct = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.txtTaskNumber = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.ckToScreen = new System.Windows.Forms.CheckBox();
            this.ckToFile = new System.Windows.Forms.CheckBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.panel9 = new System.Windows.Forms.Panel();
            this.txtProcessName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel9.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOk
            // 
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Enabled = false;
            this.cmdOk.Location = new System.Drawing.Point(204, 423);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(75, 23);
            this.cmdOk.TabIndex = 15;
            this.cmdOk.Text = "&Ok";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(285, 423);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 16;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtCS);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(15, 14);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(338, 26);
            this.panel1.TabIndex = 1;
            // 
            // txtCS
            // 
            this.txtCS.Location = new System.Drawing.Point(87, 2);
            this.txtCS.Name = "txtCS";
            this.txtCS.Size = new System.Drawing.Size(234, 20);
            this.txtCS.TabIndex = 2;
            this.txtCS.TextChanged += new System.EventHandler(this.Textboxes_TextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(4, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "CS";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtEIP);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(15, 46);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(338, 26);
            this.panel2.TabIndex = 2;
            // 
            // txtEIP
            // 
            this.txtEIP.Location = new System.Drawing.Point(87, 2);
            this.txtEIP.Name = "txtEIP";
            this.txtEIP.Size = new System.Drawing.Size(234, 20);
            this.txtEIP.TabIndex = 3;
            this.txtEIP.TextChanged += new System.EventHandler(this.txtEIP_TextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(4, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "EIP";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ckRemoveOnHit
            // 
            this.ckRemoveOnHit.AutoSize = true;
            this.ckRemoveOnHit.Location = new System.Drawing.Point(102, 131);
            this.ckRemoveOnHit.Name = "ckRemoveOnHit";
            this.ckRemoveOnHit.Size = new System.Drawing.Size(92, 17);
            this.ckRemoveOnHit.TabIndex = 6;
            this.ckRemoveOnHit.Text = "Disable on Hit";
            this.ckRemoveOnHit.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(101, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(234, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "OR";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtIntNum);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Location = new System.Drawing.Point(15, 25);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(338, 26);
            this.panel3.TabIndex = 7;
            // 
            // txtIntNum
            // 
            this.txtIntNum.Location = new System.Drawing.Point(87, 2);
            this.txtIntNum.Name = "txtIntNum";
            this.txtIntNum.Size = new System.Drawing.Size(234, 20);
            this.txtIntNum.TabIndex = 8;
            this.txtIntNum.TextChanged += new System.EventHandler(this.Textboxes_TextChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(4, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 17);
            this.label4.TabIndex = 0;
            this.label4.Text = "Int #";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(32, 414);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 32);
            this.label5.TabIndex = 6;
            this.label5.Text = "NOTE: Enter all numbers in Hex with no 0x prefix or H suffix";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.txtFunction);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Location = new System.Drawing.Point(15, 57);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(338, 26);
            this.panel4.TabIndex = 4;
            // 
            // txtFunction
            // 
            this.txtFunction.Location = new System.Drawing.Point(87, 2);
            this.txtFunction.Name = "txtFunction";
            this.txtFunction.Size = new System.Drawing.Size(234, 20);
            this.txtFunction.TabIndex = 9;
            this.txtFunction.Text = "0";
            this.txtFunction.TextChanged += new System.EventHandler(this.txtFunction_TextChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(4, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Function #";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ckDOSFunct
            // 
            this.ckDOSFunct.Location = new System.Drawing.Point(99, 89);
            this.ckDOSFunct.Name = "ckDOSFunct";
            this.ckDOSFunct.Size = new System.Drawing.Size(264, 36);
            this.ckDOSFunct.TabIndex = 10;
            this.ckDOSFunct.Text = "DOS Interrupt (Function # in AH instead of EAX like Linux)";
            this.ckDOSFunct.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(96, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(234, 17);
            this.label7.TabIndex = 0;
            this.label7.Text = "OR";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.txtTaskNumber);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Location = new System.Drawing.Point(15, 95);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(338, 26);
            this.panel5.TabIndex = 4;
            // 
            // txtTaskNumber
            // 
            this.txtTaskNumber.Location = new System.Drawing.Point(87, 2);
            this.txtTaskNumber.Name = "txtTaskNumber";
            this.txtTaskNumber.Size = new System.Drawing.Size(234, 20);
            this.txtTaskNumber.TabIndex = 5;
            this.txtTaskNumber.TextChanged += new System.EventHandler(this.Textboxes_TextChanged);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(4, 5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = "Task Number";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.panel1);
            this.panel6.Controls.Add(this.panel5);
            this.panel6.Controls.Add(this.ckRemoveOnHit);
            this.panel6.Controls.Add(this.panel2);
            this.panel6.Controls.Add(this.label7);
            this.panel6.Location = new System.Drawing.Point(12, 28);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(366, 156);
            this.panel6.TabIndex = 1;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.panel3);
            this.panel7.Controls.Add(this.panel4);
            this.panel7.Controls.Add(this.ckDOSFunct);
            this.panel7.Controls.Add(this.label3);
            this.panel7.Location = new System.Drawing.Point(12, 182);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(366, 128);
            this.panel7.TabIndex = 2;
            // 
            // ckToScreen
            // 
            this.ckToScreen.AutoSize = true;
            this.ckToScreen.Checked = true;
            this.ckToScreen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckToScreen.Location = new System.Drawing.Point(111, 394);
            this.ckToScreen.Name = "ckToScreen";
            this.ckToScreen.Size = new System.Drawing.Size(76, 17);
            this.ckToScreen.TabIndex = 13;
            this.ckToScreen.Text = "To Screen";
            this.ckToScreen.UseVisualStyleBackColor = true;
            // 
            // ckToFile
            // 
            this.ckToFile.AutoSize = true;
            this.ckToFile.Location = new System.Drawing.Point(246, 394);
            this.ckToFile.Name = "ckToFile";
            this.ckToFile.Size = new System.Drawing.Size(58, 17);
            this.ckToFile.TabIndex = 14;
            this.ckToFile.Text = "To File";
            this.ckToFile.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.panel9);
            this.panel8.Controls.Add(this.label9);
            this.panel8.Location = new System.Drawing.Point(12, 313);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(366, 63);
            this.panel8.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(99, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(234, 17);
            this.label9.TabIndex = 1;
            this.label9.Text = "OR";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.txtProcessName);
            this.panel9.Controls.Add(this.label10);
            this.panel9.Location = new System.Drawing.Point(15, 30);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(338, 26);
            this.panel9.TabIndex = 11;
            // 
            // txtProcessName
            // 
            this.txtProcessName.Location = new System.Drawing.Point(87, 2);
            this.txtProcessName.Name = "txtProcessName";
            this.txtProcessName.Size = new System.Drawing.Size(234, 20);
            this.txtProcessName.TabIndex = 12;
            this.txtProcessName.TextChanged += new System.EventHandler(this.Textboxes_TextChanged);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(4, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 17);
            this.label10.TabIndex = 0;
            this.label10.Text = "Process Name";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmBreakpointInput
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(423, 460);
            this.Controls.Add(this.panel8);
            this.Controls.Add(this.ckToFile);
            this.Controls.Add(this.ckToScreen);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.KeyPreview = true;
            this.Name = "frmBreakpointInput";
            this.Text = "Breakpoints";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtCS;
        public System.Windows.Forms.TextBox txtEIP;
        public System.Windows.Forms.CheckBox ckRemoveOnHit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.TextBox txtIntNum;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.TextBox txtFunction;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.CheckBox ckDOSFunct;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel5;
        public System.Windows.Forms.TextBox txtTaskNumber;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel7;
        public System.Windows.Forms.CheckBox ckToScreen;
        public System.Windows.Forms.CheckBox ckToFile;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel9;
        public System.Windows.Forms.TextBox txtProcessName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
    }
}