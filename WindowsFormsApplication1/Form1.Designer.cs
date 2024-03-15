namespace WindowsFormsApplication1
{
    partial class Form1
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
            this.ggMemory = new Graphicgrid.graphicgrid();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ggMemory
            // 
            this.ggMemory.Cells = new System.Drawing.Size(10, 10);
            this.ggMemory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ggMemory.GridColor = System.Drawing.Color.White;
            this.ggMemory.Location = new System.Drawing.Point(0, 0);
            this.ggMemory.Margin = new System.Windows.Forms.Padding(4);
            this.ggMemory.Name = "ggMemory";
            this.ggMemory.ShowGrid = true;
            this.ggMemory.Size = new System.Drawing.Size(1148, 1036);
            this.ggMemory.TabIndex = 2;
            this.ggMemory.gridClick += new Graphicgrid.graphicgrid.gridClickEventHandler(this.ggMemory_gridClick);
            this.ggMemory.Click += new System.EventHandler(this.ggMemory_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // cmdClose
            // 
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Location = new System.Drawing.Point(1011, 311);
            this.cmdClose.Margin = new System.Windows.Forms.Padding(4);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(100, 28);
            this.cmdClose.TabIndex = 3;
            this.cmdClose.Text = "&Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdClose;
            this.ClientSize = new System.Drawing.Size(1148, 1036);
            this.Controls.Add(this.ggMemory);
            this.Controls.Add(this.cmdClose);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Active Memory Usage Map";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private Graphicgrid.graphicgrid ggMemory;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdClose;
    }
}