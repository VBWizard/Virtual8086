namespace WindowsFormsApplication1
{
    partial class frmMemDisplay
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
            this.hbMemory = new Be.Windows.Forms.HexBox();
            this.txtPhysAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdGo = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdClose = new System.Windows.Forms.Button();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hbMemory
            // 
            this.hbMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hbMemory.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hbMemory.GroupSeparatorVisible = true;
            this.hbMemory.GroupSize = 8;
            this.hbMemory.InfoForeColor = System.Drawing.Color.Empty;
            this.hbMemory.Location = new System.Drawing.Point(12, 12);
            this.hbMemory.Name = "hbMemory";
            this.hbMemory.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hbMemory.Size = new System.Drawing.Size(928, 715);
            this.hbMemory.TabIndex = 0;
            this.hbMemory.UseFixedBytesPerLine = true;
            this.hbMemory.VScrollBarVisible = true;
            this.hbMemory.Copied += new System.EventHandler(this.hbmemory_CopiedHex);
            // 
            // txtPhysAddr
            // 
            this.txtPhysAddr.Location = new System.Drawing.Point(3, 3);
            this.txtPhysAddr.Name = "txtPhysAddr";
            this.txtPhysAddr.Size = new System.Drawing.Size(156, 20);
            this.txtPhysAddr.TabIndex = 0;
            this.txtPhysAddr.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhysAddr_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 555);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Address";
            // 
            // cmdGo
            // 
            this.cmdGo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdGo.Location = new System.Drawing.Point(246, 3);
            this.cmdGo.Name = "cmdGo";
            this.cmdGo.Size = new System.Drawing.Size(75, 23);
            this.cmdGo.TabIndex = 1;
            this.cmdGo.Text = "&Go";
            this.cmdGo.UseVisualStyleBackColor = true;
            this.cmdGo.Click += new System.EventHandler(this.cmdGo_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.txtPhysAddr);
            this.flowLayoutPanel1.Controls.Add(this.cmdClose);
            this.flowLayoutPanel1.Controls.Add(this.cmdGo);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 781);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(959, 35);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Location = new System.Drawing.Point(165, 3);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 3;
            this.cmdClose.Text = "&Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(480, 743);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(156, 20);
            this.txtAddress.TabIndex = 0;
            this.txtAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhysAddr_KeyPress);
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(653, 743);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(156, 20);
            this.txtValue.TabIndex = 0;
            this.txtValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhysAddr_KeyPress);
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.Location = new System.Drawing.Point(844, 739);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(75, 23);
            this.cmdUpdate.TabIndex = 5;
            this.cmdUpdate.Text = "&Update";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
            // 
            // frmMemDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdClose;
            this.ClientSize = new System.Drawing.Size(959, 816);
            this.Controls.Add(this.cmdUpdate);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hbMemory);
            this.Name = "frmMemDisplay";
            this.Text = "frmMemDisplay";
            this.Resize += new System.EventHandler(this.frmMemDisplay_Resize);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Be.Windows.Forms.HexBox hbMemory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        public System.Windows.Forms.TextBox txtPhysAddr;
        public System.Windows.Forms.Button cmdGo;
        private System.Windows.Forms.Button cmdClose;
        public System.Windows.Forms.TextBox txtAddress;
        public System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Button cmdUpdate;
    }
}