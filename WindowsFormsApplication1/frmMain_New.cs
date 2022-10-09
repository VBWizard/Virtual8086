using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualProcessor;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using VirtualProcessor;
using System.Collections;

namespace WindowsFormsApplication1
{
    public partial class frmMain_New : Form
    {
        private readonly int ADDRESS_COUNT_TO_DECODE = 200;
        internal PCSystem mSystem = null;
        public IntPtr hConsole;
        public const int FD_MS_BUSY = 0x10;
        private ListViewItem mCurrTooltipItem;
        private int prevWidth = 0;
        private string mCurrJobName;
        frmConfig frmCfg;
        bool lGDTTableInitialized;
        public Form1 frmMemoryForm;
        internal VirtualProcessor.Decoder mDecoder;
        internal char mlastKeyPressed = '~';
        String lEAX, lEBX, lECX, lEDX, lESI, lEDI, lESP, lEBP, lCS, lDS, lES, lFS, lGS, lSS, atESP;
        public bool mSingleStepUpdateTime = false;
        public UInt32[] mDecodedAddresses;
        public int mDecodedAddressCount;
        public UInt32 mDecodeNextSequentialEIP = 0;
        private InstructionList<Instruct> mMyInstructions;
        Instruct mCurrInstruction = new VirtualProcessor.Instruct();
        OpcodeLoader ocl = new VirtualProcessor.OpcodeLoader();
        frmMemDisplay frMemDisplay;
        frmDebugPanel frmDbg;
        bool mSuperFastBeenThrough = false;
        int mHDTimeout = 0;
        const int HD_MAX_TIMEOUT = 50;
        sInstruction sIns;
        public Processor_80x86 mProc;

        public frmMain_New()
        {
            InitializeComponent();
            //this.KeyPreview = true;
            mDecodedAddresses = new UInt32[ADDRESS_COUNT_TO_DECODE];
            lbInstructions.Columns[0].Width = 175;
            lbInstructions.Columns[1].Width = 125;
            lbInstructions.Columns[2].Width = 250;
        }

        private void pbScreen_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            Font myFont = new Font("Lucida Console", 10, FontStyle.Regular);
            e.Graphics.DrawString("Hello .NET Guide!\n\rLine two!", myFont, Brushes.LightGreen, new PointF(2, 2));
            myFont = new Font("Lucida Console", 10, FontStyle.Regular);
            e.Graphics.DrawString("Hello .NET Guide!", myFont, Brushes.Green, new PointF(2, 20));
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            UpdateForm();
            timer1.Start();
        }

        private void UpdateForm()
        {

            cmdStepInto.Enabled = Program.mSingleStep;
            cmdStepOver.Enabled = Program.mSingleStep;
            cmdContinue.Enabled = Program.mSingleStep;
            try
            {
                //CLR 08/29/2022: TODO: Fixme - remarked out as this was taking a T O N o f CPU time.
                /*if (mSystem != null && mSystem.mProc != null)
                    txtCurrJob.Text = GlobalRoutines.GetLinuxCurrentTaskName(mSystem);*/

                if (Program.mSystem != null)
                    mSystem = Program.mSystem;
                if (mSystem == null || mSystem.DeviceBlock == null)
                    return;
                if (mSystem != null && mSystem.mProc != null)
                {
                    if (ckShowAllGDT.Checked)
                    {
                        UInt32 lSelected = (UInt32)(mSystem.mProc.regs.TR.SegSel >> 3);
                        lblTaskNo.Text = lSelected.ToString();
                        for (int cnt = 0; cnt < dgGDT.Rows.Count; cnt++)
                            if ((UInt32)dgGDT.Rows[cnt].Cells[4].Value == (lSelected))
                            {
                                dgGDT.Rows[cnt].Selected = true;
                                dgGDT.Rows[cnt].Cells[0].Value = txtCurrJob.Text;
                                dgGDT.FirstDisplayedScrollingRowIndex = dgGDT.SelectedRows[0].Index;
                                break;
                            }
                    }
                    //lblFloppyDriveLED
                    if (mSystem.DeviceBlock != null && mSystem.DeviceBlock.mFloppy.s.floppy_buffer_index != 0 && lblFloppyDriveLED.BackColor != Color.LightGreen)
                    {
                        lblFloppyDriveLED.BackColor = Color.LightGreen;
                        lblFloppyDriveLED.ForeColor = Color.Black;
                    }
                    else if (lblFloppyDriveLED.ForeColor != Color.White)
                    {
                        lblFloppyDriveLED.ForeColor = Color.White;
                        lblFloppyDriveLED.BackColor = Color.Black;
                    }
                    if (!mSystem.mProc.HaltProcessor)
                    {
                        lblInactiveLED.BackColor = Color.LightGreen;
                        lblInactiveLED.ForeColor = Color.Black;
                    }
                    else if (lblInactiveLED.BackColor != Color.Black)
                    {
                        lblInactiveLED.ForeColor = Color.Orange;
                        lblInactiveLED.BackColor = Color.Black;
                    }
                    txtCR0.Text = mSystem.mProc.regs.CR0.ToString("X8");
                    txtCR2.Text = mSystem.mProc.regs.CR2.ToString("X8");
                    txtCR3.Text = mSystem.mProc.regs.CR3.ToString("X8");
                    txtCSIP.Text = mSystem.mProc.regs.CS.Value.ToString("X8") + ":" + mSystem.mProc.regs.EIP.ToString("X8");
                    //if (mSystem.DeviceBlock.mSerial.serialPorts[0]. != null)
                    
                    txtGDT.Text = mSystem.mProc.regs.GDTR.Value.ToString("X16");
                    //if (mSystem.mProc.ports.ActivePort > 0)
                    //    txtPort.Text = mSystem.mProc.ports.ActivePort.ToString("X4");
                    txtIPS.Text = (mSystem.mProc.InstructionsExecuted / (DateTime.Now - mSystem.mProc.StartedAt).TotalSeconds).ToString("#,###");  //lIPS /= 1000;
                    txtTIE.Text = mSystem.mProc.InstructionsExecuted.ToString("#,###");
                    //txtTLBEntryCount.Text = mSystem.mProc.mTLB.mCurrEntries.ToString("#,###");
                    txtTLBHits.Text = mSystem.mProc.mTLB.mHits.ToString("#,###");
                    txtTLBMisses.Text = mSystem.mProc.mTLB.mMisses.ToString("#,###");
                    txtTLBFlushes.Text = mSystem.mProc.mTLB.mFlushes.ToString("#,###");
                    //txtConsoleText.Text = Program.screen.ToString();
                    txtUpTime.Text = (System.DateTime.Now - mSystem.mProc.StartedAt).Days.ToString("00") + "   " + (System.DateTime.Now - mSystem.mProc.StartedAt).Hours.ToString("00") + ":" + (System.DateTime.Now - mSystem.mProc.StartedAt).Minutes.ToString("00") + ":" + (System.DateTime.Now - mSystem.mProc.StartedAt).Seconds.ToString("00");
                    lblCPUMode.Text = Processor_80x86.mCurrInstructOpMode.ToString().Substring(0, 1);
                    string lTR = ((mSystem.mProc.regs.TR.SegSel & 0xFFFF) >> 3).ToString("X4");
                    txtTaskNumber.Text = lTR;
                    UInt16 lActivePort = mSystem.mProc.ports.ActivePort;
                    if (lActivePort > 0 && lActivePort != 0x20 && lActivePort != 0xA0)
                    {
                        lblPortActivity.ForeColor = Color.Black;
                        lblPortActivity.BackColor = Color.LightBlue;
                        lblPortActivity.Text = lActivePort.ToString("X2");
                    }
                    else
                    {
                        lblPortActivity.ForeColor = Color.Gray;
                        lblPortActivity.BackColor = Color.Black;

                    }
                    if (mSystem.mProc.regs.CPL > ePrivLvl.OS_Ring_2)
                    {
                        txtCurrJob.BackColor = Color.LightGreen;
                        txtCurrJob.ForeColor = Color.Black;
                    }
                    else if (txtCurrJob.BackColor != Color.Black)
                    {
                        txtCurrJob.ForeColor = Color.White;
                        txtCurrJob.BackColor = Color.Black;
                    }
                    if ((mSystem.mProc.regs.EFLAGS & (1 << 9)) == (1 << 9))
                    {
                        lblIntsEnabled.BackColor = Color.LightGreen;
                        lblIntsEnabled.ForeColor = Color.Black;
                    }
                    else
                    {
                        lblIntsEnabled.ForeColor = Color.White;
                        lblIntsEnabled.BackColor = Color.Black;
                    }

                    if (Program.mSingleStep)
                    {
                        if ((mSystem.mProc.regs.EFLAGS & 0x1) == 0x1)
                        {
                            lblCFInd.BackColor = Color.LightGreen;
                            lblCFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblCFInd.ForeColor = Color.White;
                            lblCFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 2)) == (1 << 2))
                        {
                            lblPFInd.BackColor = Color.LightGreen;
                            lblPFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblPFInd.ForeColor = Color.White;
                            lblPFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 4)) == (1 << 4))
                        {
                            lblAFInd.BackColor = Color.LightGreen;
                            lblAFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblAFInd.ForeColor = Color.White;
                            lblAFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 6)) == (1 << 6))
                        {
                            lblZFInd.BackColor = Color.LightGreen;
                            lblZFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblZFInd.ForeColor = Color.White;
                            lblZFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 7)) == (1 << 7))
                        {
                            lblSFInd.BackColor = Color.LightGreen;
                            lblSFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblSFInd.ForeColor = Color.White;
                            lblSFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 8)) == (1 << 8))
                        {
                            lblTFInd.BackColor = Color.LightGreen;
                            lblTFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblTFInd.ForeColor = Color.White;
                            lblTFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 9)) == (1 << 9))
                        {
                            lblIFInd.BackColor = Color.LightGreen;
                            lblIFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblIFInd.ForeColor = Color.White;
                            lblIFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 10)) == (1 << 10))
                        {
                            lblDFInd.BackColor = Color.LightGreen;
                            lblDFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblDFInd.ForeColor = Color.White;
                            lblDFInd.BackColor = Color.Black;
                        }
                        if ((mSystem.mProc.regs.EFLAGS & (1 << 11)) == (1 << 11))
                        {
                            lblOFInd.BackColor = Color.LightGreen;
                            lblOFInd.ForeColor = Color.Black;
                        }
                        else
                        {
                            lblOFInd.ForeColor = Color.White;
                            lblOFInd.BackColor = Color.Black;
                        }
                        txtFlagsValue.Text = Program.mSystem.mProc.regs.EFLAGS.ToString("X8");
                    }
                }
                if (mSingleStepUpdateTime)
                {
                    mSingleStepUpdateTime = false;
                    ShowRegisters();
                }
            }
            catch { }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

            //Utility.MoveWindow(hConsole, picConsoleDock.Left, picConsoleDock.Top, picConsoleDock.Width, picConsoleDock.Height, true);

            //int fixedWidth = SystemInformation.VerticalScrollBarWidth +
            //dgGDT.RowHeadersWidth + 2;
            //int mul = 100 * (dgGDT.Width - fixedWidth) / (prevWidth - fixedWidth);

            //prevWidth = dgGDT.Width;
            //dgGDT.Width = fixedWidth;
        }

        public void DoResize()
        {
            OnResize(EventArgs.Empty);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (frmMemoryForm != null)
            {
                frmMemoryForm.Close();
                frmMemoryForm = null;
            }
            if (mSystem != null && mSystem.PoweredUp)
                cmdPowerOff_Click(this, new EventArgs());
            Program.mExitMainDisplayThread = true;
        }

        private void cmdShutdown_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            Utility.SetForegroundWindow(hConsole);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Program.bShowTheCursor = cbCursor.Checked;
        }

        private void cbCursor_CheckedChanged(object sender, EventArgs e)
        {
            Program.bShowTheCursor = cbCursor.Checked;
            Console.CursorVisible = cbCursor.Checked;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            mSystem.ResetSystem();
            Program.mResetMainConsole = true;
            Console.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ckMemDump.Checked = default_set.Default.DumpMemOnShutdown;
        }

        private void cmdUpdateTLB_Click(object sender, EventArgs e)
        {
            ListViewItem lvi;

            lvTLBCacheEntries.BeginUpdate();
            lvTLBCacheEntries.Items.Clear();
            if (mSystem != null && mSystem.mProc != null)
            {
//                for (int cnt = 0; cnt < mSystem.mProc.mTLB.mCurrEntries; cnt++)
//                {
            //        lvi = new System.Windows.Forms.ListViewItem(new string[] {
            //mSystem.mProc.mTLB.mLogicalAddr[cnt].ToString("X8") }, -1);
            //        //                    lvi.ToolTipText = mSystem.mProc.mTLB.mPhysAddr[cnt].ToString("X8");
            //        lvi.Tag = mSystem.mProc.mTLB.mPhysAddr[cnt].ToString("X8");
            //        lvTLBCacheEntries.Items.Add(lvi);
            //        lvTLBCacheEntries.Items[lvTLBCacheEntries.Items.Count - 1].ToolTipText = mSystem.mProc.mTLB.mPhysAddr[cnt].ToString("X8");
 //               }
            }
            lvTLBCacheEntries.EndUpdate();

        }

        private void UpdateGDT()
        {

            //07/13/2013 - Temporarily disabling this function because it is triggering paging exceptions
            string lJobName;
            sGDTEntry[] lEntries;
            if (mSystem == null || mSystem.mProc == null)
                return;

            if (Processor_80x86.mCurrInstructOpMode == VirtualProcessor.ProcessorMode.Real)
                return;

//            if (ckShowAllGDT.Checked)
//                lEntries = mSystem.mProc.mGDTCache.ListActive();
//            else
                //lEntries = mSystem.mProc.mGDTCache.ListFirstBusyTSS();
                //lEntries = mSystem.mProc.mGDTCache.ListActiveTask();

//            if (dgGDT.DataSource != lEntries)
//                dgGDT.DataSource = lEntries;
//            else
//                dgGDT.Refresh();
//dgGDT.Rows[cnt].Cells[0].Value = GlobalRoutines.GetLinuxTSSTaskName(mSystem, mSystem.mProc.mGDTCache[System.Convert.ToInt32(dgGDT.Rows[0].Cells[0].Value)].Base);
//            for (int cnt=0;cnt<lEntries.Count();cnt++)
//            {
//                dgGDT.Rows[cnt].Cells[0].Value = GlobalRoutines.GetLinuxTSSTaskName(mSystem, lEntries[cnt].Base);
//            }
            //dgGDT.Columns.Clear();
            //dgGDT.Columns.Add(

            //if (!ckShowAllGDT.Checked)
            //    return;
            //foreach (DataGridViewRow row in dgGDT.Rows)
            //{
            //    lJobName = "";
            //    UInt32 lBase = (DWord)row.Cells[2].Value;
            //    lBase = mSystem.mProc.mTLB.ShallowTranslate(mSystem.mProc, ref sIns, lBase, false, VirtualProcessor.ePrivLvl.Kernel_Ring_0);
            //    if (!sIns.ExceptionThrown)
            //    {
            //        if (row.Cells[10].Value.ToString().Contains("TSS") && (bool)row.Cells[3].Value == true && lBase < PhysicalMem.mMemBytes.Length)
            //        {
            //            lJobName = GlobalRoutines.GetLinuxTSSTaskName(mSystem, lBase);
            //            if (lJobName != "")
            //                row.Cells[0].Value = lJobName;
            //            else
            //                break;
            //        }
            //    }
            //}
        }
        private void ckMemDump_CheckedChanged(object sender, EventArgs e)
        {
            default_set.Default.DumpMemOnShutdown = ckMemDump.Checked;
        }

        private void cmdPowerOff_Click(object sender, EventArgs e)
        {
            mCurrJobName = "";
            if (mSystem != null && mSystem.PoweredUp)
            {
                cmdReset.Enabled = false;
                cmdPowerOff.Enabled = false;
                cmdPowerOff.BackColor = Color.LightGray;
                cmdPowerOff.ForeColor = Color.Gainsboro;
                Program.mExitMainDisplayThread = true;
                Program.mSystem.ShutDown();
                while (Program.mSystem.PoweredUp)
                    Application.DoEvents();
                Thread.Sleep(3000);
                cmdPowerOff.BackColor = Color.Red;
                cmdPowerOff.ForeColor = Color.Black;
                cmdPowerOff.Enabled = true;
                timer1.Stop();
                mSuperFastBeenThrough = false;
                this.tmrSuperFast.Enabled = false;
            }
            else
            {
                //Start up emulator
                System.Threading.WaitCallback wcb = new WaitCallback(Program.RunEmu);
                System.Threading.ThreadPool.QueueUserWorkItem(wcb);
                Thread.Sleep(50);
                mSystem = Program.mSystem;
                cmdPowerOff.BackColor = Color.Chartreuse;
                cmdReset.Enabled = true;
                Utility.SetForegroundWindow(hConsole);
                timer1.Start();
                mSuperFastBeenThrough = false;
                this.tmrSuperFast.Enabled = true;
            }
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            mCurrJobName = "";
            mSystem.ResetSystem();
        }

        private void tmrGDT_Tick(object sender, EventArgs e)
        {
//#if DEBUG
            tmrGDT.Enabled = false;
            UpdateGDT();
            tmrGDT.Enabled = true;
//#endif
        }

        private void ckShowAllGDT_CheckedChanged(object sender, EventArgs e)
        {
            if (ckShowAllGDT.Checked)
                tmrGDT.Interval = 2000;
            else
                tmrGDT.Interval = 1000;

        }

        private void cmdConfigure_Click(object sender, EventArgs e)
        {
            frmCfg = new frmConfig();
            frmCfg.ShowDialog(this);
            if (mSystem != null && mSystem.mProc != null)
                mSystem.mProc.mCalculateInstructionTimings = default_set.Default.CalculateTimings;
        }

        private void cmdPickFD1_Click(object sender, EventArgs e)
        {
            ChangeFloppy(1);
        }

        private void cmdPickFD2_Click(object sender, EventArgs e)
        {
            ChangeFloppy(2);
        }

        private void ChangeFloppy(int FloppyNum)
        {
            string glOrignalFilename = mSystem.DeviceBlock.mFloppy.s.media[FloppyNum].Filename;
            ofd1.FileName = mSystem.DeviceBlock.mFloppy.s.media[FloppyNum].Filename;
            if (ofd1.ShowDialog() == System.Windows.Forms.DialogResult.OK && glOrignalFilename != ofd1.FileName)
            {
                mSystem.DeviceBlock.mFloppy.s.media[FloppyNum].Filename = ofd1.FileName;
                mSystem.DeviceBlock.mFloppy.LoadDrive(FloppyNum);
            }
        }

        private void cmdShowMemUsage_Click(object sender, EventArgs e)
        {
            frmMemoryForm = new Form1();
            frmMemoryForm.Show(this);
        }

        public void HandleSingleStepEvent(object sender, Processor_80x86.CustomEventArgs e)
        {
            DWord lDecodeStartAddress;
            if (mProc == null)
                mProc = mProc = e.ProcInfo;
            UInt32 lCS = mProc.regs.CS.Value;
            UInt32 lEIP = mProc.regs.EIP;
            String s, theIns;
            Instruct lCurrInstruction = new VirtualProcessor.Instruct();
            bool lFoundCurrIns = false;
            MethodInvoker m;
            ArrayList lListViewItems = new ArrayList();

            mProc = e.ProcInfo;
            m = new MethodInvoker(() => lFoundCurrIns = this.UpdateList());
            if (!default_set.Default.DisplayBIOSWhenDebugging && mProc.regs.CS.Value == 0xF000)
                return;
            if (!default_set.Default.DisplayVGABIOSWhenDebugging && mProc.regs.CS.Value == 0xC000)
                return;

            if (mDecoder == null)
            {
                mMyInstructions = new VirtualProcessor.InstructionList<Instruct>();
                Program.mSystem.mProc.InitInstructions(mMyInstructions);
                ocl.AddOpCodesToInstructionList(ref mMyInstructions, ref Program.mSystem.mProc.OpCodeIndexer);
                mMyInstructions.FixupIndex(Program.mSystem.mProc);
                mDecoder = new VirtualProcessor.Decoder(Program.mSystem.mProc, mMyInstructions, Program.mSystem.mProc.OpCodeIndexer, false, false);
            }
            lDecodeStartAddress = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.EIP);

            if (mDecodedAddressCount > 0)
            {
                this.Invoke(m);
                if (lFoundCurrIns)
                    goto SleepyTime;
            }
            mDecodedAddressCount = 0;
            mDecoder.mDecodeOffset = lDecodeStartAddress;
            for (int cnt = 0; cnt < ADDRESS_COUNT_TO_DECODE - 1; cnt++)
            {
                lDecodeStartAddress = PhysicalMem.GetLocForSegOfs(mProc, lCS, lEIP);
                mDecoder.mInstructionEIP = lEIP;
                mDecoder.mDecodeOffset = lDecodeStartAddress;
                //Console.WriteLine("Decode @ " + mDecoder.mDecodeOffset.ToString("X8"));
                
                try {
                    mDecoder.Decode(ref sIns);
                    mCurrInstruction = sIns.mChosenInstruction;
                    lCurrInstruction = sIns.mChosenInstruction;
                }
                catch (Exception exc)
                { break; }
                if (!mDecoder.mValidDecode)
                    break;
                s = lCurrInstruction.ToString(ref sIns);
                if (cnt == 0)
                {
                    SaveRegisters(mProc);
                    mCurrInstruction = lCurrInstruction;
                }
                else if (cnt == 1)
                    mDecodeNextSequentialEIP = lEIP;
                mDecodedAddresses[mDecodedAddressCount++] = lEIP;
                theIns = "";
                ListViewItem lvi = new ListViewItem(lCS.ToString("X8") + ":" + lEIP.ToString("X8") + " ");
                for (int cnt2 = 0; cnt2 < sIns.BytesUsed; cnt2++)
                    theIns += sIns.bytes[cnt2].ToString("X2");
                //Subitem 1
                lvi.SubItems.Add(theIns);
                //Subitem 2
                lvi.SubItems.Add(s.Replace("\t", "  "));
                if (cnt == 0)
                    lvi.BackColor = Color.LightBlue;
                //Subitem 3
                lvi.SubItems.Add(lCS.ToString());
                //Subitem 4
                lvi.SubItems.Add(lEIP.ToString());
                lvi.SubItems.Add(Program.GetMapFileValue(sIns.InstructionAddress));
                lvi.ToolTipText = lCurrInstruction.Name + ": " + lCurrInstruction.mDescription;
                lListViewItems.Add(lvi);
                lEIP += sIns.BytesUsed;
            }
            //lbInstructions.Invoke(new MethodInvoker(() => lbInstructions.SelectedIndex = 0));
            lbInstructions.Invoke(new MethodInvoker(() => this.PopulateListView(lListViewItems)));
            this.Invoke(m);

        SleepyTime:
            while (1 == 1)
            {
                if (mlastKeyPressed != '~')
                {

                    mlastKeyPressed = '~';
                    break;
                }
                if (!Program.mSingleStep || Program.mSystem.mProc.PowerOff)
                    break;
                Thread.Sleep(75);
            }
        }

        void SaveRegisters(Processor_80x86 mProc)
        {
            String lFmt = "X4";
            sInstruction sIns = new VirtualProcessor.sInstruction();

            //if ((!mCurrIns.DecodedInstruction.InstructionOpSize16) || (!mCurrIns.DecodedInstruction.InstructionAddSize16))
            lFmt = "X8";
            lEAX = mProc.regs.EAX.ToString(lFmt);
            lEBX = mProc.regs.EBX.ToString(lFmt);
            lECX = mProc.regs.ECX.ToString(lFmt);
            lEDX = mProc.regs.EDX.ToString(lFmt);
            lESI = mProc.regs.ESI.ToString(lFmt);
            lEDI = mProc.regs.EDI.ToString(lFmt);
            lESP = mProc.regs.ESP.ToString(lFmt);
            lEBP = mProc.regs.EBP.ToString(lFmt);
            if (Processor_80x86.mCurrInstructOpMode == VirtualProcessor.ProcessorMode.Protected)
                lFmt = "X8";
            else
                lFmt = "X4";
            lCS = mProc.regs.CS.Value.ToString(lFmt);
            lDS = mProc.regs.DS.Value.ToString(lFmt);
            lES = mProc.regs.ES.Value.ToString(lFmt);
            lFS = mProc.regs.FS.Value.ToString(lFmt);
            lGS = mProc.regs.GS.Value.ToString(lFmt);
            lSS = mProc.regs.SS.Value.ToString(lFmt);

            UInt32 lTemp = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESP);
            UInt32 lTempAddy = 0;
            atESP = "";
            for (UInt32 cnt = lTemp; cnt < lTemp + (mProc.AddrSize16Stack ? 2 : 4) * 10; cnt += (UInt32)(mProc.AddrSize16Stack ? 2 : 4))
            {
                string sNew;
                if ((mProc.regs.CR0 & 0x80000000) == 0x80000000)
                    lTempAddy = mProc.mTLB.ShallowTranslate(mProc, ref sIns, cnt, false, VirtualProcessor.ePrivLvl.Kernel_Ring_0);
                else
                    lTempAddy = cnt;
                if (!sIns.ExceptionThrown && lTempAddy < 0xF1f1f1f1)
                {
                    if (mProc.AddrSize16Stack)
                        sNew = (mProc.mem.pMemory(mProc, lTempAddy + 1) << 8 | mProc.mem.pMemory(mProc, lTempAddy)).ToString("X4");
                    else
                        sNew = (mProc.mem.pMemory(mProc, lTempAddy + 3) << 24 | mProc.mem.pMemory(mProc, lTempAddy + 2) << 16 | mProc.mem.pMemory(mProc, lTempAddy + 1) << 8 | mProc.mem.pMemory(mProc, lTempAddy)).ToString("X8");
                    atESP = atESP + sNew + "\r\n";
                }
                else
                    break;
            }
            mSingleStepUpdateTime = true;
        }

        void ShowRegisters()
        {
            txtEAX.Text = lEAX;
            txtEBX.Text = lEBX;
            txtECX.Text = lECX;
            txtEDX.Text = lEDX;
            txtESI.Text = lESI;
            txtEDI.Text = lEDI;
            txtESP.Text = lESP;
            txtEBP.Text = lEBP;
            txtCS.Text = lCS;
            txtDS.Text = lDS;
            txtES.Text = lES;
            txtFS.Text = lFS;
            txtGS.Text = lGS;
            txtSS.Text = lSS;
            txtAtESP.Text = atESP;
        }

        //private void txtNewBPAddress_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    bool lRemoveBPOnHit = false;
        //    if (sender == txtNewBPAddress && e.KeyChar == '\r')
        //    {
        //        if (txtNewBPAddress.Text.Contains("Y") || txtNewBPAddress.Text.Contains("y") || txtNewBPAddress.Text.Contains("FFFFFFFF") || txtNewBPAddress.Text.Contains("ffffffff"))
        //        {
        //            txtNewBPAddress.Text = txtNewBPAddress.Text.Replace("Y", "").Replace("y", "");
        //            lRemoveBPOnHit = true;
        //        }
        //        UInt32 lCS = 0, lEIP = 0;
        //        ParseCSEIPFromString(txtNewBPAddress.Text, ref lCS, ref lEIP, 999999);
        //        if (MessageBox.Show("Do you wish to add a breakpoint at: " + lCS.ToString("X8") + ":" + lEIP.ToString("X8") + "?", "Add Breakpoint", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
        //        {
        //            Program.mSystem.AddAddressBreakpoint(lCS, lEIP, lRemoveBPOnHit, false, true, false);
        //            //if (!default_set.Default.DebugAtEnabled)
        //            //{
        //            Program.mStartDebuggingInstanceCount += 1;
        //            if (Program.mStartDebuggingInstanceCount == 1)
        //                mSystem.mProc.StartDebugging += Program.HandleStartDebugging;
        //            //}
        //        }
        //    }
        //}

        private void cmdStep_Click(object sender, EventArgs e)
        {
            if (Program.mSingleStep)
                mlastKeyPressed = 'i';
        }

        private void cmdStepOver_Click(object sender, EventArgs e)
        {
            cmdBreak.Enabled = true;
            mDecoder.mInstructionEIP = mProc.regs.EIP;
            mDecoder.mDecodeOffset = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.EIP); ;
            try
            {
                mDecoder.Decode(ref sIns);
                mCurrInstruction = sIns.mChosenInstruction;
            }
            catch { return;  }
            if (!mCurrInstruction.mFlowReturnsLater)
            {
                cmdStep_Click(sender, e);
                return;
            }
            Program.mSystem.AddAddressBreakpoint(Program.mSystem.mProc.regs.CS.Value, mDecodeNextSequentialEIP, false, true, true, false);
            cmdContinue_Click(sender, e);
        }

        private void memoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frMemDisplay = new frmMemDisplay();
            frMemDisplay.Show();
        }

        private void lbInstructions_DoubleClick(object sender, EventArgs e)
        {
            UInt32 lCS = 0, lEIP = 0;

            ParseCSEIPFromString(lbInstructions.SelectedItems[0].ToString(), ref lCS, ref lEIP, 8);
            Program.mSystem.AddAddressBreakpoint(lCS, lEIP, false, false, true, false);
        }

        void ParseCSEIPFromString(string Value, ref UInt32 CS, ref UInt32 EIP, int EIPLen)
        {
            CS = UInt32.Parse(Value.Substring(0, Value.IndexOf(":")), System.Globalization.NumberStyles.HexNumber);
            if (EIPLen < 10)
                EIP = UInt32.Parse(Value.Substring(Value.IndexOf(":") + 1, EIPLen), System.Globalization.NumberStyles.HexNumber); //(int)(Value.Length - Value.IndexOf(":") - 1)
            else
                EIP = UInt32.Parse(Value.Substring(Value.IndexOf(":") + 1, (int)(Value.Length - Value.IndexOf(":") - 1)), System.Globalization.NumberStyles.HexNumber);
        }

        private void cmdContinue_Click(object sender, EventArgs e)
        {
            cmdBreak.Enabled = true;
            Program.DoSingleStep(false);
        }

        public bool UpdateList()
        {
            bool lIsBPRow = false;
            bool lFoundCurrInstr = false;
            UInt32 lDecodeStartAddress = 0;
            sInstruction sIns = new sInstruction();
            int cnt = 0;
            try
            {
                for (cnt = 0; cnt < ADDRESS_COUNT_TO_DECODE - 1; cnt++)
                {
                    lIsBPRow = false;
                    foreach (cBreakpoint b in Program.mSystem.BreakpointInfo)
                        if (b.CS == System.Convert.ToUInt32(lbInstructions.Items[cnt].SubItems[3].Text) && b.EIP == System.Convert.ToUInt32(lbInstructions.Items[cnt].SubItems[4].Text) && !b.RemoveOnHit && b.Enabled)
                        {
                            lIsBPRow = true;
                            break;
                        }

                    if (mDecodedAddresses[cnt] == Program.mSystem.mProc.regs.EIP)
                    {
                        lFoundCurrInstr = true;
                        SaveRegisters(Program.mSystem.mProc);
                        if (cnt < ADDRESS_COUNT_TO_DECODE - 2)
                            mDecodeNextSequentialEIP = mDecodedAddresses[cnt + 1];
                        lbInstructions.Items[cnt].BackColor = Color.LightBlue;
                        lbInstructions.Items[cnt].EnsureVisible();
                        lDecodeStartAddress = PhysicalMem.GetLocForSegOfs(Program.mSystem.mProc, Program.mSystem.mProc.regs.CS.Value, Program.mSystem.mProc.regs.EIP);
                        mDecoder.mInstructionEIP = Program.mSystem.mProc.regs.EIP;
                        mDecoder.mDecodeOffset = lDecodeStartAddress;
                        //Console.WriteLine("Decode @ " + mDecoder.mDecodeOffset.ToString("X8"));
                        try {
                            mDecoder.Decode(ref sIns);
                        }
                        catch (Exception exc)
                        { break; }
                        string s = mCurrInstruction.ToString();
                        if (!mDecoder.mValidDecode)
                            break;

                        if (sIns.Operand1IsRef)
                            txtOp1Value.Text = (sIns.Op1TypeCode == TypeCode.Byte
                                ? sIns.Op1Value.OpByte.ToString("X2") :
                                    sIns.Op1TypeCode == TypeCode.UInt16 ? sIns.Op1Value.OpWord.ToString("X4") : sIns.Op1Value.OpDWord.ToString("X8"));
                        else
                            txtOp1Value.Text = "";
                        if (sIns.Operand2IsRef)
                            txtOp2Value.Text = (sIns.Op2TypeCode == TypeCode.Byte
                                ? sIns.Op2Value.OpByte.ToString("X2") :
                                    sIns.Op2TypeCode == TypeCode.UInt16 ? sIns.Op2Value.OpWord.ToString("X4") : sIns.Op2Value.OpDWord.ToString("X8"));
                        else
                            txtOp2Value.Text = "";

                    }
                    else if (lIsBPRow)
                    {
                        lbInstructions.Items[cnt].BackColor = Color.White;
                        lbInstructions.Items[cnt].BackColor = Color.Red;
                    }
                    else
                    {
                        lbInstructions.Items[cnt].BackColor = Color.White;
                    }
                    if (lIsBPRow && mDecodedAddresses[cnt] == Program.mSystem.mProc.regs.EIP)
                        lbInstructions.Items[cnt].BackColor = Color.LightGreen;

                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
            return lFoundCurrInstr;

        }

        private void PopulateListView(ArrayList items)
        {
            lbInstructions.Items.Clear();
            foreach (ListViewItem a in items)
                lbInstructions.Items.Add(a);
            if (this.lbInstructions.SelectedIndices.Count > 0)
                for (int i = 0; i < this.lbInstructions.SelectedIndices.Count; i++)
                {
                    this.lbInstructions.Items[this.lbInstructions.SelectedIndices[i]].Selected = false;
                }
        }

        private void lbInstructions_DoubleClick_1(object sender, EventArgs e)
        {

        }

        private void lbInstructions_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void memWindowForOperand1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMemDisplay frm = new frmMemDisplay();
            frm.txtPhysAddr.Text = sIns.Op1Add.ToString("X8");
            frm.cmdGo_Click(this, new EventArgs());
            frm.ShowDialog();
        }

        private void dIsplayMemoryWindowForOperand2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMemDisplay frm = new frmMemDisplay();
            frm.txtPhysAddr.Text = sIns.Op2Add.ToString("X8");
            frm.cmdGo_Click(this, new EventArgs());
            frm.ShowDialog();
        }

        private void breakpointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmDbg == null)
            {
                frmDbg = new frmDebugPanel();
                frmDbg.Show();
                frmDbg.FormClosed += new System.Windows.Forms.FormClosedEventHandler(NullForm);
            }
            else
                frmDbg.Visible = true;
                frmDbg.Focus();
        }

        private void NullForm(object sender, FormClosedEventArgs e)
        {
            if (((Form)sender).Name == "frmDebugPanel")
                frmDbg = null;

        }

        private void cmdDumpMemNow_Click(object sender, EventArgs e)
        {
            this.mSystem.mProc.mExternalPauseActive = true;
            Cursor.Current = Cursors.WaitCursor;
            Program.DumpMemToFile(true);
            Cursor.Current = Cursors.Default;
            this.mSystem.mProc.mExternalPauseActive = false;
        }

        private void tmrSuperFast_Tick(object sender, EventArgs e)
        {
            //if (mSystem != null && mSystem.DeviceBlock != null && mSystem.DeviceBlock.mSerial != null && mSystem.DeviceBlock.mSerial.serialPorts.Count() == 0)
            //{
            //    tmrSuperFast.Enabled = false;
            //    return;
            //}
            if (mSystem == null || mSystem.DeviceBlock == null)
                return;
            if (mSystem.DeviceBlock.mHDC.ControllerBusy)
                mHDTimeout = HD_MAX_TIMEOUT;
            else
                if (mHDTimeout > 0)
                    mHDTimeout--;
            if (mHDTimeout > 0)
            {
                if (mSystem.DeviceBlock.mHDC.mDirectionIsOut)
                    lblHardDriveLED.BackColor = Color.Red;
                else
                    lblHardDriveLED.BackColor = Color.LightGreen;
                lblHardDriveLED.ForeColor = Color.Black;
            }
            else if (lblHardDriveLED.BackColor != Color.Black)
            {
                lblHardDriveLED.ForeColor = Color.White;
                lblHardDriveLED.BackColor = Color.Black;
            }
            return;
            try
            {
                if (mSuperFastBeenThrough || (mSystem != null && mSystem.DeviceBlock != null && mSystem.DeviceBlock.mSerial != null))
                {
                    lblS0SND.BackColor = (mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.tsr_empty ? Color.Black : Color.LightGreen);
                    lblS0RCV.BackColor = (mSystem.DeviceBlock.mSerial.serialPorts[0].line_cntl.dlab ? Color.LightGreen : Color.Black);
                    lblS0CTS.BackColor = (mSystem.DeviceBlock.mSerial.serialPorts[0].modem_status.cts == true ? Color.LightGreen : Color.Black);
                    lblS0RTS.BackColor = (mSystem.DeviceBlock.mSerial.serialPorts[0].modem_cntl.rts == true ? Color.LightGreen : Color.Black);
                    mSuperFastBeenThrough = true;
                }
            }
            catch
            {
                mSuperFastBeenThrough = false;
            }
        }

        private void cmdBreak_Click(object sender, EventArgs e)
        {
            cmdBreak.Enabled = false;
            mSystem.DeviceBlock.mPIT.mPICTimers[0].Stop();
            mSystem.mProc.mSingleStep = true;
            Processor_80x86.CEAStartDebugging d = new Processor_80x86.CEAStartDebugging(mSystem.mProc);
             mSystem.mProc.OnStartDebugging(d);
             //mSystem.mProc.SingleStepEvent += HandleSingleStepEvent;
             mSystem.mProc.mSingleStep = true;
        }
    }
}