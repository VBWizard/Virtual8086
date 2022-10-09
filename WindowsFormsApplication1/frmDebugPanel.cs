using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualProcessor;

namespace WindowsFormsApplication1
{
    public partial class frmDebugPanel : Form
    {
        bool bAddingOrRemoving = false;
        
        public frmDebugPanel()
        {
            InitializeComponent();
        }

        private void frmDebugPanel_Load(object sender, EventArgs e)
        {
            UpdateBreakpointLV();
        }

        private void UpdateBreakpointLV()
        {
            bAddingOrRemoving = true;
            ListViewItem lvi;
            lvBreakpoints.Items.Clear();

            foreach (cBreakpoint b in Program.mSystem.BreakpointInfo)
            {
                lvi = new ListViewItem("");
                lvi.Checked = b.Enabled;
                lvi.SubItems.Add(b.CS.ToString("X8"));
                lvi.SubItems.Add(b.EIP.ToString("X8"));
                lvi.SubItems.Add(b.InterruptNum.ToString("X2") + "H");
                lvi.SubItems.Add(b.FunctNum.ToString("X2") + "H");
                lvi.SubItems.Add(b.DOSFunct ? "Y" : "N");
                lvi.SubItems.Add(b.TaskName);
                lvi.SubItems.Add(b.DisableOnHit==true?"Y":"N");
                lvi.Tag = b;
                lvBreakpoints.Items.Add(lvi);
            }
            clbBreakOnOptions.Items.Clear();
            clbBreakOnOptions.Items.Add("Mode switch (Real/V8086/Protected)", Program.mSystem.mModeBreakpointSet);
            clbBreakOnOptions.Items.Add("Task switch (TR register changed)", Program.mSystem.mTaskSwitchBreakpointSet);
            clbBreakOnOptions.Items.Add("Switch to CPL 0 (Supervisor)", Program.mSystem.mCPL0SwitchBreakpointSet);
            clbBreakOnOptions.Items.Add("Switch to CPL 3 (User)", Program.mSystem.mCPL3SwitchBreakpointSet);
            clbBreakOnOptions.Items.Add("Memory Paging Exception", Program.mSystem.mPagingExceptionBreakpointSet);

            bAddingOrRemoving = false;
        }

        private void cmdAddBP_Click(object sender, EventArgs e)
        {
            frmBreakpointInput frmBPI = new frmBreakpointInput();
            if (frmBPI.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (frmBPI.txtCS.Enabled)
                    Program.mSystem.AddAddressBreakpoint(UInt32.Parse(frmBPI.txtCS.Text, System.Globalization.NumberStyles.HexNumber), UInt32.Parse(frmBPI.txtEIP.Text, System.Globalization.NumberStyles.HexNumber), frmBPI.ckRemoveOnHit.Checked, false, frmBPI.ckToScreen.Checked, frmBPI.ckToFile.Checked);
                else if (frmBPI.txtIntNum.Enabled)
                    Program.mSystem.AddSoftIntBreakpoint(Byte.Parse(frmBPI.txtIntNum.Text, System.Globalization.NumberStyles.HexNumber), Byte.Parse(frmBPI.txtFunction.Text, System.Globalization.NumberStyles.HexNumber), frmBPI.ckDOSFunct.Checked, frmBPI.ckToScreen.Checked, frmBPI.ckToFile.Checked);
                else if (frmBPI.txtTaskNumber.Enabled)
                    Program.mSystem.AddSwitchToTaskBreakpoint(UInt32.Parse(frmBPI.txtTaskNumber.Text, System.Globalization.NumberStyles.HexNumber), frmBPI.ckRemoveOnHit.Checked, frmBPI.ckToScreen.Checked, frmBPI.ckToFile.Checked);
                else if (frmBPI.txtProcessName.Enabled)
                    Program.mSystem.AddTaskNameBreakpoint(frmBPI.txtProcessName.Text , frmBPI.ckRemoveOnHit.Checked, frmBPI.ckToScreen.Checked, frmBPI.ckToFile.Checked);
                UpdateBreakpointLV();
            }
        }

        private void frmDebugPanel_Activated(object sender, EventArgs e)
        {
            UpdateBreakpointLV();
        }

        private void clbBreakOnOptions_SelectedValueChanged(object sender, EventArgs e)
        {
            string lSelected = clbBreakOnOptions.SelectedItem.ToString().ToUpper();
            if (lSelected.Contains("MODE SWITCH"))
            {
                Program.mSystem.mModeBreakpointSet = clbBreakOnOptions.CheckedIndices.Contains(0);
                if (Program.mSystem.mModeBreakpointSet)
                {
                    Program.mStartDebuggingInstanceCount += 1;
                    if (Program.mStartDebuggingInstanceCount == 1)
                        Program.mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
                }

            }
            else if (lSelected.Contains("TASK SWITCH"))
            {
                Program.mSystem.mTaskSwitchBreakpointSet = clbBreakOnOptions.CheckedIndices.Contains(1);
                if (Program.mSystem.mTaskSwitchBreakpointSet)
                {
                    Program.mStartDebuggingInstanceCount += 1;
                    if (Program.mStartDebuggingInstanceCount == 1)
                        Program.mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
                }
                else
                {
                    Program.mStartDebuggingInstanceCount -= 1;
                    if (Program.mStartDebuggingInstanceCount == 0)
                        Program.mSystem.mProc.StartDebugging -= Program.HandleStartDebugging;
                }
            }
            else if (lSelected.Contains("CPL 0"))
            {
                Program.mSystem.mCPL0SwitchBreakpointSet = clbBreakOnOptions.CheckedIndices.Contains(2);
                if (Program.mSystem.mCPL0SwitchBreakpointSet)
                {
                    Program.mStartDebuggingInstanceCount += 1;
                    if (Program.mStartDebuggingInstanceCount == 1)
                        Program.mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
                }
                else
                {
                    Program.mStartDebuggingInstanceCount -= 1;
                    if (Program.mStartDebuggingInstanceCount == 0)
                        Program.mSystem.mProc.StartDebugging -= Program.HandleStartDebugging;
                }
            }
            else if (lSelected.Contains("CPL 3"))
            {
                Program.mSystem.mCPL3SwitchBreakpointSet = clbBreakOnOptions.CheckedIndices.Contains(3);
                if (Program.mSystem.mCPL3SwitchBreakpointSet)
                {
                    Program.mStartDebuggingInstanceCount += 1;
                    if (Program.mStartDebuggingInstanceCount == 1)
                        Program.mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
                }
                else
                {
                    Program.mStartDebuggingInstanceCount -= 1;
                    if (Program.mStartDebuggingInstanceCount == 0)
                        Program.mSystem.mProc.StartDebugging -= Program.HandleStartDebugging;
                }
            }
            else if (lSelected.Contains("PAGING EXCEPTION"))
            {
                Program.mSystem.mPagingExceptionBreakpointSet = clbBreakOnOptions.CheckedIndices.Contains(4);
                if (Program.mSystem.mPagingExceptionBreakpointSet)
                {
                    Program.mStartDebuggingInstanceCount += 1;
                    if (Program.mStartDebuggingInstanceCount == 1)
                        Program.mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
                }
                else
                {
                    Program.mStartDebuggingInstanceCount -= 1;
                    if (Program.mStartDebuggingInstanceCount == 0)
                        Program.mSystem.mProc.StartDebugging -= Program.HandleStartDebugging;
                }
            }

        }

        private void lvBreakpoints_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!bAddingOrRemoving)
                Program.mSystem.BreakpointInfo[e.Item.Index].Enabled = e.Item.Checked;
        }

        private void cmdRemoveBP_Click(object sender, EventArgs e)
        {
            if (lvBreakpoints.SelectedIndices.Count > 0)
            {
                Program.mSystem.RemoveAddressBreakpoint((cBreakpoint)lvBreakpoints.SelectedItems[0].Tag);
                UpdateBreakpointLV();
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
