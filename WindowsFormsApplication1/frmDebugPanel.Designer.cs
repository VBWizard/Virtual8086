namespace WindowsFormsApplication1
{
    partial class frmDebugPanel
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
            this.lvBreakpoints = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdAddBP = new System.Windows.Forms.Button();
            this.cmdRemoveBP = new System.Windows.Forms.Button();
            this.clbBreakOnOptions = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdClose = new System.Windows.Forms.Button();
            this.chTaskName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvBreakpoints
            // 
            this.lvBreakpoints.CheckBoxes = true;
            this.lvBreakpoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.chTaskName,
            this.columnHeader4});
            this.lvBreakpoints.FullRowSelect = true;
            this.lvBreakpoints.Location = new System.Drawing.Point(23, 24);
            this.lvBreakpoints.MultiSelect = false;
            this.lvBreakpoints.Name = "lvBreakpoints";
            this.lvBreakpoints.Size = new System.Drawing.Size(728, 251);
            this.lvBreakpoints.TabIndex = 0;
            this.lvBreakpoints.UseCompatibleStateImageBehavior = false;
            this.lvBreakpoints.View = System.Windows.Forms.View.Details;
            this.lvBreakpoints.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvBreakpoints_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Enabled";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "CS";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "(E)IP";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Interrupt #";
            this.columnHeader5.Width = 80;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Function";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "DOS Int";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "RemoveOnHit";
            this.columnHeader4.Width = 100;
            // 
            // cmdAddBP
            // 
            this.cmdAddBP.Location = new System.Drawing.Point(476, 281);
            this.cmdAddBP.Name = "cmdAddBP";
            this.cmdAddBP.Size = new System.Drawing.Size(131, 23);
            this.cmdAddBP.TabIndex = 1;
            this.cmdAddBP.Text = "&Add Breakpoint";
            this.cmdAddBP.UseVisualStyleBackColor = true;
            this.cmdAddBP.Click += new System.EventHandler(this.cmdAddBP_Click);
            // 
            // cmdRemoveBP
            // 
            this.cmdRemoveBP.Location = new System.Drawing.Point(620, 281);
            this.cmdRemoveBP.Name = "cmdRemoveBP";
            this.cmdRemoveBP.Size = new System.Drawing.Size(131, 23);
            this.cmdRemoveBP.TabIndex = 1;
            this.cmdRemoveBP.Text = "&Remove Breakpoint";
            this.cmdRemoveBP.UseVisualStyleBackColor = true;
            this.cmdRemoveBP.Click += new System.EventHandler(this.cmdRemoveBP_Click);
            // 
            // clbBreakOnOptions
            // 
            this.clbBreakOnOptions.CheckOnClick = true;
            this.clbBreakOnOptions.FormattingEnabled = true;
            this.clbBreakOnOptions.Location = new System.Drawing.Point(772, 54);
            this.clbBreakOnOptions.Name = "clbBreakOnOptions";
            this.clbBreakOnOptions.Size = new System.Drawing.Size(220, 94);
            this.clbBreakOnOptions.TabIndex = 2;
            this.clbBreakOnOptions.SelectedValueChanged += new System.EventHandler(this.clbBreakOnOptions_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(772, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(220, 27);
            this.label1.TabIndex = 3;
            this.label1.Text = "Break on ...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cmdClose
            // 
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Location = new System.Drawing.Point(772, 281);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 4;
            this.cmdClose.Text = "&Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // chTaskName
            // 
            this.chTaskName.Text = "Task Name";
            this.chTaskName.Width = 143;
            // 
            // frmDebugPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdClose;
            this.ClientSize = new System.Drawing.Size(1010, 312);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clbBreakOnOptions);
            this.Controls.Add(this.cmdRemoveBP);
            this.Controls.Add(this.cmdAddBP);
            this.Controls.Add(this.lvBreakpoints);
            this.Name = "frmDebugPanel";
            this.Text = "frmDebugPanel";
            this.Activated += new System.EventHandler(this.frmDebugPanel_Activated);
            this.Load += new System.EventHandler(this.frmDebugPanel_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvBreakpoints;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button cmdAddBP;
        private System.Windows.Forms.Button cmdRemoveBP;
        private System.Windows.Forms.CheckedListBox clbBreakOnOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader chTaskName;
    }
}