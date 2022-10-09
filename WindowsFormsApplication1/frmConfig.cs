using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication1
{

    public partial class frmConfig : Form
    {
        ErrorProvider err = new ErrorProvider();

        public frmConfig()
        {
            InitializeComponent();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            tbMemory.Value = default_set.Default.PhysicalMemoryAmount / 1024 / 1024;
            txtMemoryAmt.Text = tbMemory.Value.ToString("#,##0");
            txtHD1Filename.Text = default_set.Default.HD1PathAndFile;
            txtCyl1.Text = default_set.Default.HD1Cyls.ToString();
            txtHeads1.Text = default_set.Default.HD1Heads.ToString();
            txtSPT1.Text = default_set.Default.HD1SPT.ToString();

            txtHD2Filename.Text = default_set.Default.HD2PathAndFile;
            txtCyl2.Text = default_set.Default.HD2Cyls.ToString();
            txtHeads2.Text = default_set.Default.HD2Heads.ToString();
            txtSPT2.Text = default_set.Default.HD2SPT.ToString();

            txtCMOSFilename.Text = default_set.Default.CMOSPathAndFile;
            txtBIOSFilename.Text = default_set.Default.BIOSPathAndFile;
            txtVidBIOSFilename.Text = default_set.Default.VideoBiosPathAndFile;

            txtFD1Filename.Text = default_set.Default.FD1PathAndFile;
            txtFD2Filename.Text = default_set.Default.FD2PathAndFile;

            txtDebugPath.Text = default_set.Default.Debug_Path;
            txtMaxDbgFileSize.Text = default_set.Default.DebugCodeMaxFileSize.ToString("#,##0");
            txtTimerTick.Text = default_set.Default.TimerTickInterval.ToString();
            txtHD1Filename.Focus();

            txtDebugAtCS.Text = default_set.Default.DebugAtSegment.ToString("x8");
            txtDebugAtEIP.Text = default_set.Default.DebugAtAddress.ToString("x8");
            txtDieAtCS.Text = default_set.Default.DieAtSegment.ToString("x8");
            txtDieAtEIP.Text = default_set.Default.DieAtAddress.ToString("x8");
            txtDumpAtCS.Text = default_set.Default.DumpAtSegment.ToString("x8");
            txtDumpAtEIP.Text = default_set.Default.DumpAtAddress.ToString("x8");

            tbMemory.Value = default_set.Default.PhysicalMemoryAmount / 1024 / 1024;
            txtMemoryAmt.Text = tbMemory.Value.ToString("0");
            clbOptions.Items.Add("Calculate Timings", default_set.Default.CalculateTimings);
            clbOptions.Items.Add("Debug-At Enabled", default_set.Default.DebugAtEnabled);
            clbOptions.Items.Add("Die-At Enabled", default_set.Default.DieAtEnabled);
            clbOptions.Items.Add("Dump-At Enabled", default_set.Default.DieAtEnabled);
            clbOptions.Items.Add("Display BIOS code in debug", default_set.Default.DisplayBIOSWhenDebugging);
            clbOptions.Items.Add("Display Vid code in debug", default_set.Default.DisplayVGABIOSWhenDebugging);
            clbOptions.Items.Add("Dump memory on shutdown", default_set.Default.DumpMemOnShutdown);
            clbOptions.Items.Add("Dump memory above 1 MB", default_set.Default.DumpMemAbove1MB);
            //clbOptions.Items.Add("Dump GDT on shutdown",default_set.Default. );
            //clbOptions.Items.Add("Dump IDT on shutdown",default_set.Default. );
            //clbOptions.Items.Add("Dump LDT on shutdown",default_set.Default. );
            //clbOptions.Items.Add("Dump TLB contents on shutdown",default_set.Default. );
            clbOptions.Items.Add("Start emulator on load", default_set.Default.StartEmuOnLoad);
            clbOptions.Items.Add("Manual Debugging Enabled", default_set.Default.ManualDebuggingEnabled);
            clbOptions.Items.Add("Honor WP in supervisor mode", default_set.Default.WP_BIT);
            clbOptions.Items.Add("Debug ports", default_set.Default.DebugPorts);
            ckFD1Enabled.Checked = default_set.Default.FD1Inserted;
            ckFD2Enabled.Checked = default_set.Default.FD2Inserted;
            if (!default_set.Default.DebugAtEnabled)
                grpDebugAt.Enabled = false;
            else
                grpDebugAt.Enabled = true;
            if (!default_set.Default.DieAtEnabled)
                grpDieAt.Enabled = false;
            else
                grpDieAt.Enabled = true;

            ckHD1Installed.Checked = default_set.Default.HD1Installed;
            ckHD2Installed.Checked = default_set.Default.HD2Installed;
            if (default_set.Default.BootDevice == "Floppy")
                cboBootDevice.SelectedIndex = 0;
            else
                cboBootDevice.SelectedIndex = 1;
            cbFD1Capacity.Text = default_set.Default.FD1Type;
            cbFD2Capacity.Text = default_set.Default.FD2Type;
            txtTaskNameOffset.Text = default_set.Default.TaskNameOffset.ToString();
        }

        private void LoadConfig()
        {

        }

        private void tbMemory_Scroll(object sender, EventArgs e)
        {
            txtMemoryAmt.Text = tbMemory.Value.ToString("#,##0");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtHD1Filename.Text != "" && new FileInfo(txtHD1Filename.Text).Exists)
            {
                ofdHD1.FileName = txtHD1Filename.Text;
                ofdHD1.InitialDirectory = txtHD1Filename.Text.Substring(0,txtHD1Filename.Text.LastIndexOf("\\"));
            }
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtHD1Filename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetFile2_Click(object sender, EventArgs e)
        {
            if (txtHD1Filename.Text != "" && new FileInfo(txtHD2Filename.Text).Exists)
            {
                ofdHD1.FileName = txtHD1Filename.Text;
                ofdHD1.InitialDirectory = txtHD1Filename.Text.Substring(0, txtHD1Filename.Text.LastIndexOf("\\"));
            }
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtHD2Filename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetCMOSFile_Click(object sender, EventArgs e)
        {
            if (txtCMOSFilename.Text != "" && new FileInfo(txtCMOSFilename.Text).Exists)
                ofdHD1.FileName = txtCMOSFilename.Text;
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtCMOSFilename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetFD1_Click(object sender, EventArgs e)
        {
            if (txtFD1Filename.Text != "" && new FileInfo(txtFD1Filename.Text).Exists)
                ofdHD1.FileName = txtFD1Filename.Text;
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtFD1Filename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetFD2_Click(object sender, EventArgs e)
        {
            if (txtFD2Filename.Text != "" && new FileInfo(txtFD2Filename.Text).Exists)
                ofdHD1.FileName = txtFD2Filename.Text;
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtFD2Filename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetBIOSFile_Click(object sender, EventArgs e)
        {
            if (txtBIOSFilename.Text != "" && new FileInfo(txtBIOSFilename.Text).Exists)
                ofdHD1.FileName = txtBIOSFilename.Text;
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtBIOSFilename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void cmdGetVidBIOSFile_Click(object sender, EventArgs e)
        {
            if (txtVidBIOSFilename.Text != "" && new FileInfo(txtVidBIOSFilename.Text).Exists)
                ofdHD1.FileName = txtVidBIOSFilename.Text;
            else
                ofdHD1.FileName = "";
            ofdHD1.ShowDialog();
            if (ofdHD1.FileName != "")
                txtVidBIOSFilename.Text = ofdHD1.FileName;
            else
                ofdHD1.FileName = "";
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            switch (e.Index)
            {
                case 1: //DebugAt
                    if (e.NewValue == CheckState.Unchecked)
                        grpDebugAt.Enabled = false;
                    else
                        grpDebugAt.Enabled = true;
                    break;
                case 2: //DieAt
                    if (e.NewValue == CheckState.Unchecked)
                        grpDieAt.Enabled = false;
                    else
                        grpDieAt.Enabled = true;
                    break;
            }
        
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (txtDebugPath.Text != "" && new FileInfo(txtDebugPath.Text).Exists)
                fb1.SelectedPath = txtDebugPath.Text;
            else
                fb1.SelectedPath = "";
            ofdHD1.ShowDialog();
            if (fb1.SelectedPath != "")
                txtDebugPath.Text = fb1.SelectedPath;
        }

        private void txtMaxDbgFileSize_Validating(object sender, CancelEventArgs e)
        {
            int Num;
            if (!int.TryParse(txtMaxDbgFileSize.Text.Replace(",", ""), out Num))
            {
                err.SetError(txtMaxDbgFileSize, "Debug Max File Size is not valid.");
                e.Cancel = true;
            }
            else
                err.Clear();
        }

        private void txtHD1Filename_Validating(object sender, CancelEventArgs e)
        {
            if (ckHD1Installed.Checked && !new FileInfo(txtHD1Filename.Text).Exists)
            {
                err.SetError(txtHD1Filename, "HD 1 Filename is not an existing file");
                e.Cancel = true;
            }
        }

        private void txtHD2Filename_Validating(object sender, CancelEventArgs e)
        {
            if (ckHD2Installed.Checked && !new FileInfo(txtHD2Filename.Text).Exists)
            {
                err.SetError(txtHD2Filename, "HD 2 Filename is not an existing file");
                e.Cancel = true;
            }
        }

        private void cmdSwapHDs_Click(object sender, EventArgs e)
        {
            string lTempFilename, lTempCyls, lTempHeads, lTempSPT;

            lTempFilename = txtHD1Filename.Text;
            lTempCyls = txtCyl1.Text;
            lTempHeads = txtHeads1.Text;
            lTempSPT = txtSPT1.Text;

            txtHD1Filename.Text = txtHD2Filename.Text;
            txtCyl1.Text = txtCyl2.Text;
            txtHeads1.Text = txtHeads2.Text;
            txtSPT1.Text = txtSPT2.Text;

            txtHD2Filename.Text = lTempFilename;
            txtCyl2.Text = lTempCyls;
            txtHeads2.Text = lTempHeads;
            txtSPT2.Text = lTempSPT;
        }

        private void cboBootDevice_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if ((string)cboBootDevice.SelectedItem == "Floppy" && !ckFD1Enabled.Checked && !ckFD2Enabled.Checked)
                err.SetError(cboBootDevice, "Floppy selected as boot device, but no floppies inserted");
            else if ((string)cboBootDevice.SelectedValue == "Hard Drive" && !ckHD1Installed.Checked && !ckHD2Installed.Checked)
                err.SetError(cboBootDevice, "Hard drive selected as boot device, but no hard drives installed");
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
            {
                MessageBox.Show("Invalid values detected.  Fix them and then click Ok again");
                return;
            }
            default_set.Default.HD1PathAndFile = txtHD1Filename.Text;
            default_set.Default.HD2PathAndFile = txtHD2Filename.Text;
            default_set.Default.HD1Cyls = System.Convert.ToUInt32(txtCyl1.Text);
            default_set.Default.HD2Cyls = System.Convert.ToUInt32(txtCyl2.Text);
            default_set.Default.HD1Heads = System.Convert.ToUInt32(txtHeads1.Text);
            default_set.Default.HD2Heads = System.Convert.ToUInt32(txtHeads2.Text);
            default_set.Default.HD1SPT = System.Convert.ToUInt32(txtSPT1.Text);
            default_set.Default.HD2SPT = System.Convert.ToUInt32(txtSPT2.Text);
            default_set.Default.FD1PathAndFile = txtFD1Filename.Text;
            default_set.Default.FD2PathAndFile = txtFD2Filename.Text;
            default_set.Default.FD1Inserted = ckFD1Enabled.Checked;
            default_set.Default.FD2Inserted = ckFD2Enabled.Checked;
            default_set.Default.CMOSPathAndFile = txtCMOSFilename.Text;
            default_set.Default.BIOSPathAndFile = txtBIOSFilename.Text;
            default_set.Default.VideoBiosPathAndFile = txtVidBIOSFilename.Text;
            default_set.Default.Debug_Path = txtDebugPath.Text;
            default_set.Default.DebugCodeMaxFileSize = System.Convert.ToInt32(txtMaxDbgFileSize.Text.Replace(",", ""));
            default_set.Default.PhysicalMemoryAmount = (System.Convert.ToInt32(txtMemoryAmt.Text.Replace(",", ""))) * 1024 * 1024;
            default_set.Default.TimerTickInterval = System.Convert.ToInt32(txtTimerTick.Text);
            default_set.Default.CalculateTimings = clbOptions.CheckedIndices.Contains(0);
            default_set.Default.DebugAtEnabled = clbOptions.CheckedIndices.Contains(1);
            default_set.Default.DieAtEnabled = clbOptions.CheckedIndices.Contains(2);
            default_set.Default.DumpAtEnabled = clbOptions.CheckedIndices.Contains(3);
            if (Program.mSystem != null)
            {
                if (default_set.Default.DumpAtEnabled)
                    Program.mSystem.mProc.WatchExecutionAddress += Program.HandleTakeADump;
                else
                    Program.mSystem.mProc.WatchExecutionAddress -= Program.HandleTakeADump;
            }
            default_set.Default.DisplayBIOSWhenDebugging = clbOptions.CheckedIndices.Contains(4);
            default_set.Default.DisplayVGABIOSWhenDebugging = clbOptions.CheckedIndices.Contains(5);
            default_set.Default.DumpMemOnShutdown = clbOptions.CheckedIndices.Contains(6);
            default_set.Default.DumpMemAbove1MB = clbOptions.CheckedIndices.Contains(7);
            default_set.Default.StartEmuOnLoad = clbOptions.CheckedIndices.Contains(8);
            default_set.Default.ManualDebuggingEnabled = clbOptions.CheckedIndices.Contains(9);
            default_set.Default.WP_BIT = clbOptions.CheckedIndices.Contains(10);
            default_set.Default.DebugPorts = clbOptions.CheckedIndices.Contains(11);
            default_set.Default.HD1Installed = ckHD1Installed.Checked;
            default_set.Default.HD2Installed = ckHD2Installed.Checked;
            default_set.Default.BootDevice = (string)cboBootDevice.SelectedItem;
            default_set.Default.DebugAtSegment = UInt32.Parse(txtDebugAtCS.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.DebugAtAddress= UInt32.Parse(txtDebugAtEIP.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.DieAtSegment = UInt32.Parse(txtDieAtCS.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.DieAtAddress = UInt32.Parse(txtDieAtEIP.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.DumpAtSegment = UInt32.Parse(txtDumpAtCS.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.DumpAtAddress = UInt32.Parse(txtDumpAtEIP.Text, System.Globalization.NumberStyles.HexNumber);
            default_set.Default.FD1Type = cbFD1Capacity.Text;
            default_set.Default.FD2Type = cbFD2Capacity.Text;
            default_set.Default.TaskNameOffset = UInt32.Parse(txtTaskNameOffset.Text);

            if (Program.mSystem != null && Program.mSystem.DeviceBlock != null && Program.mSystem.DeviceBlock.mCMOS != null)
            {
                Program.mSystem.DeviceBlock.mCMOS.mPauseCMOSForUpdate = true;
                Thread.Sleep(1000);
            }
            if (default_set.Default.BootDevice == "Floppy")
                VirtualProcessor.GlobalRoutines.UpdateCMOS(default_set.Default.CMOSPathAndFile , 0x2d, 0x23);
            else
                VirtualProcessor.GlobalRoutines.UpdateCMOS(default_set.Default.CMOSPathAndFile, 0x2d, 0x03);
            UInt16 lMemory = (UInt16)((default_set.Default.PhysicalMemoryAmount-(1024*1024)) / 1024);
            VirtualProcessor.GlobalRoutines.UpdateCMOS(default_set.Default.CMOSPathAndFile, 0x17, (UInt16)(lMemory));
            VirtualProcessor.GlobalRoutines.UpdateCMOS(default_set.Default.CMOSPathAndFile, 0x30, (UInt16)(lMemory));
            if (Program.mSystem != null && Program.mSystem.DeviceBlock != null && Program.mSystem.DeviceBlock.mCMOS != null)
            {
                Program.mSystem.DeviceBlock.mCMOS.mPauseCMOSForUpdate = false;
                if (txtFD1Filename.Text != Program.mSystem.DeviceBlock.mFloppy.s.media[0].Filename && ckFD1Enabled.Checked)
                    Program.mSystem.LoadDrive(txtFD1Filename.Text, default_set.Default.FD1Type, 1);
                if (txtFD2Filename.Text != Program.mSystem.DeviceBlock.mFloppy.s.media[1].Filename && ckFD2Enabled.Checked)
                    Program.mSystem.LoadDrive(txtFD2Filename.Text, default_set.Default.FD2Type, 2);

                Program.mSystem.mProc.mTimerTickMSec = System.Convert.ToInt32(txtTimerTick.Text);
            }
            default_set.Default.Save();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            txtHD1Filename.CausesValidation = false;
        }

        private void txtMemoryAmt_Leave(object sender, EventArgs e)
        {
            Int32 lTest = 0;
            if (Int32.TryParse(txtMemoryAmt.Text, out lTest))
                if (lTest <= 2048)
                    tbMemory.Value = System.Convert.ToInt32(txtMemoryAmt.Text);
        }

        private void NumericOnly_KeyPress_Handler(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+") && (e.KeyChar != 8))
                e.Handled = true;
        }

        private void cmdSwapFDs_Click(object sender, EventArgs e)
        {
            String lTempFlp1 = txtFD1Filename.Text;
            
            txtFD1Filename.Text = txtFD2Filename.Text;
            txtFD2Filename.Text = lTempFlp1;
        }

        private void txtFD1Filename_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
