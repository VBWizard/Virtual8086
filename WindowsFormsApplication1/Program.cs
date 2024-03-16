using System;
using System.Linq;
using System.Windows.Forms;
using VirtualProcessor;
using System.Threading;
using DWord = System.UInt32;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    static class Program
    {
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();


#if WIN
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

#endif
        private const int GWL_STYLE = -16, GWL_EXSTYLE = -20;              //hex constant for style changing
        private const UInt32 WS_SYSMENU = 0x00080000, WS_CHILD = 0x40000000, WS_DLGFRAME = 0x00400000, WS_BORDER = 0x00800000, WS_OVERLAPPED = 0, WM_CHANGEUISTATE = 0x0127, WS_EX_TOOLWINDOW = 0x00000080, WS_CLIPSIBLINGS = 0x04000000,
            WS_THICKFRAME = 0x00040000, WS_CAPTION = 0x00C00000, WS_MINIMIZEBOX = 0x00020000, WS_MINIMIZE = 0x20000000, WS_MAXIMIZEBOX = 0x00010000;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        public static PCSystem mSystem = null;
        public static bool bShowTheCursor = false, mShowIRQServiceMessage, mIRQIsReallyException,
            mShowAssemblerCode = default_set.Default.ShowAssemblerCode, mNowDebugging, mSingleStep;
        static Thread DisplayThread, DebugDisplayThread, EmuThread, KBDThread, PortThread;
        static ThreadStart DisplayThreadStart, DebugDisplayThreadStart, EmuThreadStart, KBDThreadStart, PortThreadStart;
        public static bool mResetEmulation, mExitMainDisplayThread, mResetMainConsole, mbShowIPS = true;
        static byte mPortInByte = 0, IRQOrExceptionNum = 0;
        static public frmMain_New mMainForm;
        static internal IntPtr hConsole, hConsoleOfChild;
        static StreamWriter DebugCodeFile = null, DebugPortFile = null;
        static ArrayList mCurrINTNumArray = new ArrayList(), mCurrINTRetAddrArray = new ArrayList();
        static int DebugCodeFileNumber;
        static bool StartEmulatorThreadActive = false;
        static string[] MapFileEntries = new string[10000];
        static UInt32[] MapFileAddresses = new UInt32[10000];
        static UInt32[] MapFileAddressesSorted = new UInt32[10000];
        static int MapFileEntryCount = 0;
        static bool mShiftDown = false, mCtrlDown = false, mAltDown = false;
        static UInt32 mExceptionErrorCode;
        static public int mStartDebuggingInstanceCount = 0;
        //static IVT100Decoder vt100 = new VT100Decoder();
        //public static VTScreen screen = new VTScreen(80, 80);
        

        public static void RunEmu(Object stateInfo)
        {
            //vt100 = new VT100Decoder();
            //screen = new VTScreen(80, 80);
            //vt100.Encoding = Encoding.ASCII;
            //vt100.Subscribe(screen);

            StartEmulatorThreadActive = true;
            Thread.CurrentThread.Name = "StartEmulatorThread";
            EmuThreadStart = new ThreadStart(StartEmu);
            EmuThread = new Thread(EmuThreadStart);
            EmuThread.Name = "EmuThread";
            EmuThread.Start();
            EmuThread.Priority = ThreadPriority.Highest;
            while (mSystem == null || mSystem.mProc == null)
                Thread.Sleep(0);
            Thread.Sleep(100);
            mMainForm.mProc = mSystem.mProc;
            while (1 == 1)
            {
                while (mSystem == null || mSystem.mProc == null)
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(10);
                if (mSystem.mProc.ProcessorStatus == eProcessorStatus.ShuttingDown || mSystem.mProc.ProcessorStatus == eProcessorStatus.Resetting)
                {
                    //Console.Clear();
                    Console.SetCursorPosition(0, 32);
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Press <POWER> button to start the emulation.!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (default_set.Default.DebugPorts)
                        if (DebugPortFile != null)
                            DebugPortFile.Close();
                    EmuThread = null;
                    break;
                }
            }

            StartEmulatorThreadActive = false;
            //DebugPortFile.Close();
        }

        private static void StartEmu()
        {
#if DOTRIES
            try
            {
#endif
            mSingleStep = false;
            mSystem = new PCSystem((DWord)default_set.Default.PhysicalMemoryAmount, eProcTypes.i80486, default_set.Default.CMOSPathAndFile);
            mSystem.mProc.mTimerTickMSec = default_set.Default.TimerTickInterval;
            if (default_set.Default.CalculateTimings)
                mSystem.mProc.mCalculateInstructionTimings = true;
            else
                mSystem.mProc.mCalculateInstructionTimings = false;
            //if (default_set.Default.FD1PathAndFile != "")
            if (default_set.Default.FD1Inserted)
                mSystem.LoadDrive(default_set.Default.FD1PathAndFile, default_set.Default.FD1Type, 1);
            if (default_set.Default.FD2Inserted)
                mSystem.LoadDrive(default_set.Default.FD2PathAndFile, default_set.Default.FD1Type, 2);
            mSystem.DebugFilePath = default_set.Default.Debug_Path;
            if (default_set.Default.HD1Installed)
                mSystem.LoadDrive(default_set.Default.HD1PathAndFile, default_set.Default.HD1Cyls, default_set.Default.HD1Heads, default_set.Default.HD1SPT, default_set.Default.HD1DeviceType);
            if (default_set.Default.HD2Installed)
                mSystem.LoadDrive(default_set.Default.HD2PathAndFile, default_set.Default.HD2Cyls, default_set.Default.HD2Heads, default_set.Default.HD2SPT, default_set.Default.HD2DeviceType);

            mSystem.LoadRom(default_set.Default.BIOSPathAndFile, eRomTypes.BIOS, 0);
            mSystem.LoadRom(default_set.Default.VideoBiosPathAndFile, eRomTypes.Video, 0);

            //if (default_set.Default.MiscROMPathAndFile != "")
            //    mSystem.LoadRom(default_set.Default.MiscROMPathAndFile, eRomTypes.Misc, default_set.Default.MiscROMLoadAddress);

            if (default_set.Default.WP_BIT)
                mSystem.EnableCR0_WritePotect();

            if (default_set.Default.DebugAtEnabled)
                mSystem.AddAddressBreakpoint(default_set.Default.DebugAtSegment, (UInt32)default_set.Default.DebugAtAddress, false, false, false, true);

            if (default_set.Default.DieAtEnabled)
                mSystem.AddAddressBreakpoint(default_set.Default.DieAtSegment, (UInt32)default_set.Default.DieAtAddress, false, false, false, true);

            if (default_set.Default.DebugPorts)
            {
                mSystem.mProc.ports.HandleDataInDone += HandleDataInEvent;
                mSystem.mProc.ports.HandleDataOutDone += HandleDataOutEvent;
                DebugPortFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Ports_Filename);
            }
            System.Diagnostics.Debug.WriteLine("Now debugging to file: " + default_set.Default.Debug_Path + default_set.Default.Debug_Ports_Filename);
            mExitMainDisplayThread = false;
            Console.Clear();
            DisplayThreadStart = new ThreadStart(ShowDisplay);
            DisplayThread = new Thread(DisplayThreadStart);
            DisplayThread.Name = "DisplayThread";
            DisplayThread.Start();

#if DEBUG
            //if (default_set.Default.DebugAtEnabled)
            //{
//                mStartDebuggingInstanceCount += 1;
//                if (mStartDebuggingInstanceCount == 1)
//                    mSystem.mProc.StartDebugging += HandleStartDebugging;
//                mStartDebuggingInstanceCount += 1;
            if (default_set.Default.DumpAtEnabled)     
                mSystem.mProc.WatchExecutionAddress += HandleTakeADump;
                
            //}

#endif
            KBDThreadStart = new ThreadStart(HandleDisplayKeyboard);
            KBDThread = new Thread(KBDThreadStart);
            KBDThread.Name = "KBDThread";
            KBDThread.Start();


            //if (mMainForm != null && mMainForm.mSystem == null)

#if TERMINAL_ATTACAHED
            PortThreadStart = new ThreadStart(HandleSerialIO);
                PortThread = new Thread(PortThreadStart);
                PortThread.Start();
#endif
#if DOTRIES
                try
                {
#endif
            mSystem.TaskNameOffset = default_set.Default.TaskNameOffset;
            mSystem.Debuggies = new sDebugComponents(false, false, false, false, false, false, false, false, true, false, false, false, false);
            mSystem.OSType = eOpSysType.Linux;
            mSystem.FPUEnabled(true);
            mSystem.ColdBoot();
#if DOTRIES
                }
                catch (Exception exc2)
                {
                    System.Diagnostics.Debug.WriteLine("StartEmulator error --> " + exc2.Message);
                    MessageBox.Show("StartEmulator error --> " + exc2.Message);
                    mExitMainDisplayThread = true;
                }
            }
#endif
#if DOTRIES
                catch (Exception exc)
                {
                    mExitMainDisplayThread = true;
                }
#endif
        }
        #region Main Display Related
        public static void ShowDisplay()
        {
            byte[] mScreenBytes = new byte[80 * 200 * 2];
            Processor_80x86 sender = (Processor_80x86)mSystem.mProc;
            int lPosX, lPosY;
            int lCurrPosY = 0;
            int lBS = 0xB8000;
            int lBSOffset = 0;
            int lBPage;
            int lBPageSize;
            byte lBCursorX = 0, lBCursorY = 0;
            String ProcMode = "R", Floppy = " ", HardDrive = " ";
            TimeSpan TotalTime;
            bool bSuccess;
            long lResult;
            Encoding enc = Encoding.GetEncoding("us-ascii",
                                                      new EncoderExceptionFallback(),
                                                      new DecoderExceptionFallback());
            byte lByte;
            char lChar;
            byte lAttrib;
            int lCursorPos = 0;


            while (1 == 1)
            {
                Thread.Sleep(10);
                lBPage = 0;  //mSystem.PhysicalMem.GetByte(sender, 0x400 + 0x62);
                while (mSystem.DeviceBlock == null)
                { }
                try { lBSOffset = ((mSystem.DeviceBlock.mVGA.s.CRTC.reg[0xC] << 8) | mSystem.DeviceBlock.mVGA.s.CRTC.reg[0xD]) * 2; }// = mSystem.mProc.mem.GetWord(sender, 0x400 + 0x4e);
                catch { }
                lBPageSize = 0; // mSystem.mProc.mem.GetWord(sender, 0x400 + 0x4c);
                if (lBPageSize == 0)
                    lBPageSize = 40000; //4000;
                lBS = 0xb8000 + lBSOffset;

                lPosX = -1; lPosY = 0; lCurrPosY = 0;
                for (int cnt = 0; cnt < (lBPageSize) - 2; cnt += 2)
                {
                    lPosY = (cnt / 160);
                    if (lPosY != lCurrPosY)
                    {
                        lCurrPosY++;
                        lPosX = 0;
                    }
                    else
                        lPosX++;
                    try
                    {
                        //clChar = (char)PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)];
                        lByte = PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)];
                        lAttrib = PhysicalMem.mMemBytes[(UInt32)(lBS + cnt + 1)];
                        if (mScreenBytes[cnt] != lByte || // PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)] || 
                            mScreenBytes[cnt + 1] != (byte)lAttrib)
                        {
                            mScreenBytes[cnt] = lByte;
                            mScreenBytes[cnt + 1] = (byte)lAttrib;
                            Console.SetCursorPosition(lPosX, lPosY); //removed try with empty catch
#if !DEBUG
                            if (lByte > 127)
                            {
                                lChar = Encoding.GetEncoding(437).GetChars(new byte[] { lByte })[0];
                                Console.Write("{0}", lChar);
                            }
                            else
#endif
#if WIN
                                SetConsoleTextAttribute(hConsole, (byte)lAttrib);
#endif
                            Console.Write((char)lByte);
                        }
                    }
                    catch (Exception exc2) { }
                }

                if (mResetMainConsole)
                {
                    Console.Clear();
                    mScreenBytes = new byte[80 * 200 * 2];
                    mSystem.mProc.mem.InitializeVideoMem();
                    //for (int cnt2 = 0; cnt2 < 80 * 50 * 2; cnt2 += 2)
                    //{
                    //    mScreenBytes[cnt2] = 0x0;
                    //    mScreenBytes[cnt2 + 1] = 0x0;
                    //}
                    mResetMainConsole = false;
                }
                if (mExitMainDisplayThread)
                    return;
                /*try
                {
                    if (bShowTheCursor)
                    {
                        try { lCursorPos = ((mSystem.DeviceBlock.mVGA.s.CRTC.reg[0xE] << 8) | mSystem.DeviceBlock.mVGA.s.CRTC.reg[0xF]); }
                        catch { }
                        if (lBSOffset > 0)
                        {
                            lBSOffset /= 2;
                            lBCursorX = (byte)((lCursorPos - lBSOffset) % 80);
                            lBCursorY = (byte)((lCursorPos - lBSOffset) / 80);
                        }
                        else
                        {
                            lBCursorX = (byte)((lCursorPos) % 80);
                            lBCursorY = (byte)((lCursorPos) / 80);
                        }
                        
                        
                        lBCursorX = PhysicalMem.mMemBytes[(UInt32)(0x450)]; // mSystem.PhysicalMem.GetByte(sender, 0x400 + 0x50);
                        lBCursorY = PhysicalMem.mMemBytes[(UInt32)(0x451)]; // mSystem.PhysicalMem.GetByte(sender, 0x400 + 0x51);
                        //try { Console.SetCursorPosition(lBCursorX, lBCursorY); }
                        //catch { }
                    }
                }
                catch { }*/
                //Application.DoEvents();
            }
        }
        public static void HandleDisplayKeyboard()
        {
            Processor_80x86 sender = mSystem.mProc;

            while (1 == 1)
            {
                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo lKey = Console.ReadKey(true);
                    HandleDisplayKeystroke(sender, lKey);
                }
                Thread.Sleep(75);
                if (mExitMainDisplayThread)
                {
                    return;
                }
            }
        }
        public static void HandleDisplayKeystroke(Processor_80x86 mProc, ConsoleKeyInfo lKey)
        {
            String lKeyScan;
            int scancode = 0;

            lKeyScan = GetKeyString(lKey);
            //            if (lKey.Key == ConsoleKey.F12 && lKey.Modifiers == ConsoleModifiers.Alt)
            //            {
            //                if (DebugDisplayThread == null)
            //                {
            //                    DebugDisplayThreadStart = new ThreadStart(LoadDebugConsole);
            //                    DebugDisplayThread = new Thread(DebugDisplayThreadStart);
            //                    DebugDisplayThread.Start();
            //                    return;
            //                }
            //            }

            if (lKey.Key == ConsoleKey.Tab && lKey.Modifiers == ConsoleModifiers.Control)
            {

                if (!mNowDebugging)
                    AttemptToEnableDebugging();
                else
                    StopDebugging();
                return;
            }
            //            else if (lKey.Key == ConsoleKey.F10) mProc.PowerOff = true;
            //else if (lKey.Key == ConsoleKey.Home) mSystem.ResetSystem();
            else if (lKey.Key == ConsoleKey.Delete && ((lKey.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control) )
            {
                mSingleStep = !mSingleStep;
                DoSingleStep(mSingleStep);
                return;
            }
            //else if (lKey.Key == ConsoleKey.Tab) { mScreenReset = true;  mShowDebugOnConsole ^= true; Console.Clear(); }
            else if (lKey.Key == ConsoleKey.I && lKey.Modifiers == ConsoleModifiers.Control)
                mbShowIPS = !(mbShowIPS);
            else if (lKey.Key == ConsoleKey.Pause)
            {
                Console.WriteLine("Paused, press any key to continue");
                while (Console.KeyAvailable == false)
                    Thread.Sleep(250);
                Console.ReadKey(true);
            }
            if (lKey.KeyChar == 0x03)
                lKeyScan = "C";
            //else if (lKeyScan == "OEM3")
            //{
            //    mSystem.mProc.mem.SetByte(0x417, (byte)(mSystem.PhysicalMem.GetByte(0x417) | 0x04));
            //    lKeyScan = "C";
            //}
            else if (lKeyScan == "F9")
            {
                mResetMainConsole = true;
                return;
            }
            if (lKeyScan != "")
            {
                if ((lKey.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control && !mCtrlDown)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x1D);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x04);
                    mCtrlDown = true;
                }
                else if (mCtrlDown && ((lKey.Modifiers & ConsoleModifiers.Control) != ConsoleModifiers.Control))
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x1D | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xFB);
                    mCtrlDown = false;
                }
                if ((lKey.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift && !mShiftDown)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x2a);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x02);
                    mShiftDown = true;
                }
                else if (mShiftDown && ((lKey.Modifiers & ConsoleModifiers.Shift) != ConsoleModifiers.Shift))
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x2a | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xFD);
                    mShiftDown = false;
                }
                if ((lKey.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt && !mAltDown)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x38);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x08);
                    mAltDown = true;
                }
                else if (mAltDown && ((lKey.Modifiers & ConsoleModifiers.Alt) != ConsoleModifiers.Alt))
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x38 | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xF7);
                    mAltDown = false;
                }
            }
            try
            {
                scancode = mSystem.DeviceBlock.mKeyboard.ScanCodeDict[lKeyScan];
                if (mMainForm.ckEchotoTty.Checked)
                {

                    if (lKey.KeyChar == '\r')
                    {
                        mSystem.DeviceBlock.mSerial.rx_fifo_enq(0, 0x0d);

                        //mSystem.DeviceBlock.mSerial.serialPorts[0].tsrbuffer = 0x0d;
                        //mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.tsr_empty = false;
                        return;
                    }
                    else if (lKey.KeyChar == '\n')
                        return;
                    mSystem.DeviceBlock.mSerial.rx_fifo_enq(0, (byte)lKey.KeyChar);
                    //mSystem.DeviceBlock.mSerial.serialPorts[0].tsrbuffer = (byte)lKey.KeyChar;
                    //mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.tsr_empty = false;
                }
                else if (scancode > 0 /*&& scancode != 0x1b*/) //Commented out ] exclusion
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode((ulong)scancode);
                    scancode = scancode | 0x80;
                    Thread.Sleep(30);
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode((ulong)scancode);
                }
            }
            catch { Console.Beep(1000, 150); return; }
        }
        public static String GetKeyString(ConsoleKeyInfo lKey)
        {
            if (lKey.KeyChar == '\0' || lKey.KeyChar.ToString().Trim() == "" || lKey.KeyChar == '\b')
                return lKey.Key.ToString().ToUpper();
            else
                return lKey.KeyChar.ToString().ToUpper();

        }
        static void HandleDataInEvent(object sender, Ports.CustomEventArgs e)
        {
            //if (/*e.PortInfo.Portnum == 0x64 || */e.PortInfo.Portnum == 0x80)
            //    return;
            if (e.PortInfo.Portnum != 0x3d4 && e.PortInfo.Portnum != 0x3d5)
                return;
            string lWrite = DateTime.Now.ToString();
            lWrite += "\tData In on port " + e.PortInfo.Portnum.ToString("x").PadLeft(4, '0').ToUpper() +
                "\t Value\t" + e.PortInfo.Value.ToString("x").PadLeft(8, '0').ToUpper();
            //lWrite += "\t\tIn Call Count = " + e.PortInfo.InCount.GetDWord();
            //Debug_Ports_Filename
            DebugPortFile.WriteLine(lWrite);
            DebugPortFile.Flush();
            //Debug.WriteLine(lWrite);
            //mPortFile.WriteLine("\tInstruct @: " + mCurrentOp.proc.regs.CS.ToString("x").PadLeft(4, '0').ToUpper() +
            //     ":" + mCurrentOp.proc.regs.IP.ToString("x").PadLeft(4, '0').ToUpper() + "\n");
        }
        static void HandleDataOutEvent(object sender, Ports.CustomEventArgs e)
        {
            //if (e.PortInfo.Portnum == 0x64 || e.PortInfo.Portnum == 0x80)
            //if ((e.PortInfo.Portnum == 0x20 && e.PortInfo.Value == 0x20))
//                return;
//            if (e.PortInfo.Portnum != 0x3d4 && e.PortInfo.Portnum != 0x3d5)
//                return;
            string lWrite = DateTime.Now.ToString();
            //lWrite += sender.regs.CS.Value.ToString("X8") + ":" + sender.regs.IP.ToString("X8");
            if (e.PortInfo.Portnum == 0x21 || e.PortInfo.Portnum == 0x20) return;
                lWrite += "\tData Out on port " + e.PortInfo.Portnum.ToString("x").PadLeft(4, '0').ToUpper() +
                "\t Value\t" + e.PortInfo.Value.ToString("x").PadLeft(8, '0').ToUpper() + "\t";
            if (e.PortInfo.Size == TypeCode.Byte & e.PortInfo.Value > 0x13)
            {
                lWrite += "'" + System.Convert.ToChar(e.PortInfo.Value);
                //                System.Diagnostics.Debug.Write(System.Convert.ToChar(e.PortInfo.Value));
                //                if (e.PortInfo.Value == 10)
                //                    System.Console.Beep(4000, 100);
            }
            //lWrite += "\t\tOut Call Count = " + e.PortInfo.OutCount.GetDWord();
            DebugPortFile.WriteLine(lWrite);

            if (e.PortInfo.Portnum == 0xFFF0)
                System.Diagnostics.Debug.Write(System.Convert.ToChar(e.PortInfo.Value));

        }
        public static void DoSingleStep(bool Enabled)
        {
            mSingleStep = Enabled;
            mSystem.mProc.mExportDebug = Enabled;
            if (Enabled)
            {
                mSystem.mProc.mSingleStep = true;
                mSystem.mProc.SingleStepEvent += mMainForm.HandleSingleStepEvent;
            }
            else
            {
                mSystem.mProc.mSingleStep = false;
                mSystem.mProc.SingleStepEvent -= mMainForm.HandleSingleStepEvent;
            }
        }
#if TERMINAL_ATTACAHED
        public static void HandleSerialIO()
        {
            const int MAX_BUFF_SIZE = 2000;
            byte[] buffer = new byte[MAX_BUFF_SIZE];
            byte[] sendBuff;
            int bufferPtr = 0;
            bool byteRcvd = false;

            Processor_80x86 sender = (Processor_80x86)mSystem.mProc;
            while (mSystem.mProc.mSystem.DeviceBlock == null || mSystem.DeviceBlock.mSerial == null)
                Thread.Sleep(10);
            while (1 == 1)
            {

                mPortInByte = 0;
                if (mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].fifo_cntl.enable && mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.tsr_empty == false)
                {
                    mPortInByte = mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].tsrbuffer;
                    mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.tsr_empty = true;
                    byteRcvd = true;
                }
                else if (!mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].fifo_cntl.enable && mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.thr_empty == false)
                {
                    mPortInByte = mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].thrbuffer;
                    mSystem.mProc.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.thr_empty = true;
                    byteRcvd = true;
                }

                if (byteRcvd)
                {
                    buffer[bufferPtr++] = mPortInByte;
                    System.Diagnostics.Debug.Write(":" + mPortInByte.ToString("X2"));
                }
                else if (bufferPtr > 0)
                {
                    sendBuff = new byte[bufferPtr];
                    for (int cnt = 0; cnt < bufferPtr; cnt++)
                        sendBuff[cnt] = buffer[cnt];
                    //vt100.Input(sendBuff);
                    bufferPtr = 0;
                    buffer = new byte[MAX_BUFF_SIZE];
                    byteRcvd = false;
                }
                Thread.Sleep(10);
                //if (mPortInByte > 0 && mPortInByte <= 127)
                //                        Debug.WriteLine("Byte Received = " + mPortInByte.ToString("x2"));
                if (mExitMainDisplayThread)
                    return;
            }
        }
#endif
        #endregion
        public static void DumpMemToFile()
        {
            DumpMemToFile(false);
        }
        public static void DumpMemToFile(bool WhileRunning)
        {
            StringBuilder m = new StringBuilder();
            StringBuilder p = new StringBuilder();
            StreamWriter lMemFile;

            UInt32 lSeg;
            UInt32 lOfs;
            TimeSpan TotalTime = new TimeSpan();
            if (mSystem == null || mSystem.mProc == null)
                return;

            if (!WhileRunning)
                lMemFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Mem_Filename.Replace(".", "_" + mSystem.mProc.mSystem.mFileSuffix + "."));
            else
                lMemFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Mem_Filename.Replace(".", "_" + mSystem.mProc.mSystem.mFileSuffix + "-" + new Random().Next(1000000) + "."));

            m.Append("\nStarted @ " + mSystem.mProc.StartedAt + "\nStopped @ " + mSystem.mProc.StoppedAt + "\n");
            m.Append("\n\rInstructions decoded: " + mSystem.mProc.InstructionsDecoded + " (" + Math.Round(mSystem.mProc.InstructionsDecoded / TotalTime.TotalSeconds, 0) + ")\n\r");
            m.Append("\n\rInstructions executed: " + mSystem.mProc.InstructionsExecuted + " (" + Math.Round(mSystem.mProc.InstructionsExecuted / TotalTime.TotalSeconds, 0) + ")\n\r");
            //m.Append(cValue.createCount);
            m.Append("\n\rInstruction cache hits: " + mSystem.mProc.mCacheHits);
            m.Append("\n\rInstruction cache misses: " + mSystem.mProc.mCacheMisses);
            m.Append("\n\rTLB Hits: " + cTLB.mHits);
            m.Append("\n\rTLB Misses: " + cTLB.mMisses);
            m.Append("\n\rTLB Flushes: " + cTLB.mFlushes);
            m.Append("\n\rGDT Cache Resets: " + mSystem.mProc.GDTCacheResetsExecuted + " (" + mSystem.mProc.TimeInGDTRefresh.ToString() + ")\n\r");
            //m.Append("\n\rTime in timed section: " + mSystem.mProc.mMSinTimedSection + "\n\r");
            int mLength = PhysicalMem.mMemBytes.Length;

            lMemFile.Write(m);
            lMemFile.Flush();
            m = new StringBuilder();

            if (!default_set.Default.DumpMemAbove1MB)
                mLength = 1024 * 1024;
            //if (default_set.Default.DumpMemMB > 0 && default_set.Default.DumpMemMB * 1024 * 1024 < PhysicalMem.mMemBytes.Length)
            //    mLength = 1024 * 1024 * default_set.Default.DumpMemMB;
            for (UInt32 cnt = 0; cnt < mLength; cnt++)
            {
                if (cnt % (1024 * 1024) == 0)
                {
                    lMemFile.Write(m);
                    lMemFile.Flush();
                    m = new StringBuilder();
                }
                if (cnt % 32 == 0)
                {
                    if (cnt > 0)
                        m.Append(" ");
                    if (cnt > 0)
                        for (UInt32 cnt2 = cnt - 32; cnt2 < cnt; cnt2++)
                        {
                            if (PhysicalMem.mMemBytes[cnt2] >= 0x20 && PhysicalMem.mMemBytes[cnt2] <= 0x7e)
                                m.Append(System.Convert.ToChar(PhysicalMem.mMemBytes[cnt2]));
                            else
                                m.Append(".");
                            //if (cnt2 == cnt - 17)
                            //    m.Append("\t");
                        }
                    if (cnt > 0)
                        m.Append("\n\r");
                    lSeg = (UInt32)(PhysicalMem.GetSegForLoc(mSystem.mProc, (DWord)cnt));
                    lOfs = PhysicalMem.GetOfsForLoc(mSystem.mProc, cnt);
                    m.AppendFormat("{0}:{1}\t", lSeg.ToString("x").ToUpper().PadLeft(8, '0'), lOfs.ToString("x").ToUpper().PadLeft(4, '0'));
                }
                else if (cnt > 0 && cnt % 16 == 0)
                {
                    lSeg = (UInt32)(PhysicalMem.GetSegForLoc(mSystem.mProc, (DWord)cnt));
                    lOfs = PhysicalMem.GetOfsForLoc(mSystem.mProc, cnt)+16;
                    m.AppendFormat("\t{0}:{1}\t", lSeg.ToString("x").ToUpper().PadLeft(8, '0'), (lOfs - 16).ToString("x").ToUpper().PadLeft(4, '0'));
                }

                m.AppendFormat("{0} ", PhysicalMem.mMemBytes[cnt].ToString("X").PadLeft(2, '0'));
            }

            p.AppendFormat("\n\r\n\r");

            m.Append("\n\r\n\rInstructions: \n\r");

            Instruct i;
            for (uint cnt=0;cnt< Processor_80x86.Instructions.Count;cnt++)
            {
                i = Processor_80x86.Instructions[cnt];
                if (i != null)
                {
                    m.AppendFormat("\t{0}\t{1}\t{2}\t", i.Name, i.UsageCount, System.Math.Round(i.TotalTimeInInstruct, 3));
                    if (i.UsageCount > 0)
                        m.AppendFormat("{0}\n", ((i.TotalTimeInInstruct) / i.UsageCount));
                    else
                        m.AppendFormat("0\n");
                }
            }

            Processor_80x86 mProc = mSystem.mProc;
            m.AppendFormat("\n\r\n\r");
            //if (mSystem.mProc.regs.SS.Selector.granularity.OpSize32)
            //lStackVal = new cValue(mSystem.mProc.mem.GetDWord(mSystem.mProc, PhysicalMem.GetLocForSegOfs(mSystem.mProc, mSystem.mProc.regs.SS.Value, mSystem.mProc.regs.SP)));
            //else
            //lStackVal = new cValue(mSystem.mProc.mem.GetDWord(mSystem.mProc, mSystem.mProc.regs.SS.Value + mSystem.mProc.regs.ESP));
            UInt32 lStackVal = 0;
            m.AppendFormat("EAX={0}  EBX={1}  ECX={2}  EDX={3}  M={4}  INT={5}\n\r", mSystem.mProc.regs.EAX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EBX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.ECX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EDX.ToString("x").ToUpper().PadLeft(8, '0'), Processor_80x86.mCurrInstructOpMode.ToString().Substring(0, 1), 0);
            m.AppendFormat("EBP={1}  ESI={2}  EDI={3}  ESP={0}  [ESP]={4}  EIP={5}\n\r", mSystem.mProc.regs.ESP.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EBP.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.ESI.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EDI.ToString("x").ToUpper().PadLeft(8, '0'), lStackVal.ToString("x").ToUpper().PadLeft(8, '0'), mProc.regs.EIP.ToString("X8"));
            m.AppendFormat("LDT={1}  CR0={2}  CR2={9}   CR3={8}  CR4={3}    O16={4} A16={5} SO16={6} SA16={7}\n\r", mSystem.mProc.regs.GDTR.Value.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.LDTR.Value.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR0.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR4.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.OpSize16.ToString().Substring(0, 1), mSystem.mProc.AddrSize16.ToString().Substring(0, 1), mSystem.mProc.OpSize16.ToString().Substring(0, 1), mSystem.mProc.AddrSize16Stack.ToString().Substring(0, 1), mSystem.mProc.regs.CR3.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR2.ToString("x").ToUpper().PadLeft(8, '0'));
            m.AppendFormat("SS={2} ({7})   DS={0} ({8})   ES={1} ({9})   CS={3} ({10})   FS={4} ({11})   GS={5} ({12})  EFLAGS={6}\n\r", mSystem.mProc.regs.DS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.ES.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.SS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.CS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.FS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.GS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.EFLAGS.ToString("x").ToUpper().PadLeft(8, '0'),
                mSystem.mProc.regs.SS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.DS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.ES.DescriptorNum.ToString("X3"), mSystem.mProc.regs.CS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.FS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.GS.DescriptorNum.ToString("X3"));
            m.AppendFormat("GDT={0}   LDT={1}   IDT={2}\n\r", mProc.regs.GDTR.Base.ToString("X8"), mProc.regs.LDTR.Base.ToString("X8"), mProc.regs.IDTR.Base.ToString("X8"));
            m.AppendFormat("CR0={0}   CR2={1}   CR3={2}   CR4={3}\n\r", mProc.regs.CR0.ToString("X8"), mProc.regs.CR2.ToString("X8"), mProc.regs.CR3.ToString("X8"), mProc.regs.CR4.ToString("X8"));
            m.AppendFormat("TR={0}    EIP={1}\n\r", mProc.regs.TR.SegSel.ToString("X8"), mProc.regs.EIP.ToString("X"));
            m.Append("Next 80 bytes at CS:EIP\n\t");
            sInstruction sIns = new sInstruction();
            try
            {
                for (UInt32 cnt1 = 0; cnt1 < 40; cnt1++)
                    m.AppendFormat("{0} ", PhysicalMem.GetByte(mSystem.mProc, ref sIns, (UInt32)(mProc.sCurrentDecode.InstructionAddress + cnt1)).ToString("X2"));
                m.AppendFormat("\n\r\n\r");
            }
            catch { m.AppendFormat("\n\r\n\r"); }
            m.AppendFormat("GDT Table\n\r");
            m.Append("Entry\tBase\tPresent\tOpSize32\tSegType\tSDescType\tDescType\tPrivLvl\tValue\n\r");
            if (mProc.mGDTCache != null)
                for (int cnt = 0; cnt < mProc.mGDTCache.Count; cnt++)
                    //if (mProc.mGDTCache[cnt].access.Present)
                    m.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n\r",
                        cnt.ToString("X4"),
                        mProc.mGDTCache[cnt].Base.ToString("X8"),
                        mProc.mGDTCache[cnt].access.Present,
                        mProc.mGDTCache[cnt].granularity.OpSize32,
                        mProc.mGDTCache[cnt].access.SegType,
                        mProc.mGDTCache[cnt].access.SystemDescType,
                        mProc.mGDTCache[cnt].access.DescType, mProc.mGDTCache[cnt].access.PrivLvl, mProc.mGDTCache[cnt].Value.ToString("X16"));

            m.AppendFormat("\n\r\n\r");

            m.AppendFormat("LDT Table\n\r");
            m.Append("Entry\tBase\tPresent\tOpSize32\tSegType\tSDescType\tDescType\tPrivLvl\tValue\n\r");
            if (mProc.mLDTCache != null)
                for (int cnt = 0; cnt < mProc.mLDTCache.Count; cnt++)
                    //if (mProc.mGDTCache[cnt].access.Present)
                    m.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n\r",
                        cnt.ToString("X4"),
                        mProc.mLDTCache[cnt].Base.ToString("X8"),
                        mProc.mLDTCache[cnt].access.Present,
                        mProc.mLDTCache[cnt].granularity.OpSize32,
                        mProc.mLDTCache[cnt].access.SegType,
                        mProc.mLDTCache[cnt].access.SystemDescType,
                        mProc.mLDTCache[cnt].access.DescType, mProc.mLDTCache[cnt].access.PrivLvl, mProc.mLDTCache[cnt].Value.ToString("X16"));

            m.AppendFormat("\n\r\n\r");
            m.AppendFormat("IDT Table\n\r");
            m.Append("EntryNum\tGDT\tPEPOffset\tGateType\tGateSize32\tPrivLvl\n\r");
            if (mProc.mIDTCache != null)
                for (int cnt = 0; cnt < mProc.mIDTCache.Count; cnt++)
                    if (cnt > 255)
                        break;
                    else
                        //if (mProc.mIDTCache[cnt].SegSelector > 0)
                        m.AppendFormat("{0}\t\t{1}\t{2}\t{3}\t{4}\t{5}\n\r",
                            cnt.ToString("X4"),
                            mProc.mIDTCache[cnt].GDTCachePointer.ToString("X4"),
                            mProc.mIDTCache[cnt].PEP_Offset.ToString("X8"),
                            mProc.mIDTCache[cnt].GateType,
                            mProc.mIDTCache[cnt].GateSize32,
                            mProc.mIDTCache[cnt].Descriptor_PL);
            m.Append("\n\r\n\r");
            m.Append("TLB Cache");
            m.Append("Logical\tPhysical\tType\n\r");
            //for (int cnt = 0; cnt < mProc.mTLB.mCurrEntries; cnt++)
            //    m.AppendFormat("{0}\t{1}\t{2}\n\r", mProc.mTLB.mLogicalAddr[cnt].ToString("X8"), mProc.mTLB.mPhysAddr[cnt].ToString("X8"), mProc.mTLB.mType[cnt].ToString("X4"));

            //for (int cnt = 0; cnt < 0xFFFF; cnt++)
            //    if (mProc.OpCodeCounts[cnt] != 0)
            //        m.AppendFormat("{0}: {1}\n\r",cnt.ToString("X4"), mProc.OpCodeCounts[cnt].ToString());

            lMemFile.Write(m);
            lMemFile.Close();

            //CLR 03/11/2014 - Added dump of page tables per process

            m = new StringBuilder();
            if (!WhileRunning)
                lMemFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Mem_Filename.Replace(".", "_" + mSystem.mProc.mSystem.mFileSuffix + "_ptables."));
            else
                lMemFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Mem_Filename.Replace(".", "_" + mSystem.mProc.mSystem.mFileSuffix + "-" + new Random().Next(1000000) + "."));

            m.Append("Per-process page tables\n\n");

            DWord lPageTableBase, lTemp1, lCurrentPageTableEntry;
            int lColumns = 0;
            DWord lCurrentEntryNum = 0;
            DWord lColsPerRow = 16;
            UInt64 lValue = 0;
            UInt64 lTempCount;
            if (mProc.mGDTCache != null)
                foreach (sGDTEntry s in mProc.mGDTCache)
                {
                    lColumns = 0;
                    if (s.SystemDescType.StartsWith("TSS"))
                    {
                        //mSystem.mProc.mTLB.ShallowTranslate(mProc, ref sIns, s.Base - mSystem.TaskNameOffset, false, ePrivLvl.Kernel_Ring_0);
                        lPageTableBase = mProc.mem.pMemoryD(mProc, (s.Base & 0x0FFFFFFF) + 28);
                        m.Append("GDT Entry #=" + s.Number.ToString("X8") + "\tProgram Name=" + GlobalRoutines.GetLinuxTSSTaskName(mSystem, s.Base & 0x0FFFFFFF) + "\tSegment Type=" + s.SystemDescType + "\tPrivLvl=" + s.PrivLvl + "\tTSS Loc=" + s.Base.ToString("X8") + "\tPage Table Base=" + lPageTableBase.ToString("X8") + "\n");
                        lCurrentEntryNum = 0;
                        if (lPageTableBase < PhysicalMem.mMemBytes.Length && lPageTableBase > 0)
                            for (DWord DirTablePointer = 0; DirTablePointer < 4096; DirTablePointer += 4)
                            {
                                lCurrentEntryNum = (DirTablePointer << 18) * 4;
                                lCurrentPageTableEntry = mProc.mem.pMemoryD(mProc, (lPageTableBase + DirTablePointer) );
                                if (lCurrentPageTableEntry > 0)
                                {
                                    m.Append("\n" + lCurrentEntryNum.ToString("X8") + "\t");
                                    lColumns = -1;
                                    lTempCount = 0;
                                    for (DWord PageTablePointer = lCurrentPageTableEntry & 0xFFFFF000; PageTablePointer < (lCurrentPageTableEntry & 0xFFFFF000) + 4096; PageTablePointer += 4)
                                    {
                                        lValue = mProc.mem.pMemoryD(mProc, PageTablePointer);
                                        if (lValue == 0xF0F0F0F0)
                                            lValue = 0;
                                        lTempCount++;
                                        if (++lColumns == lColsPerRow)
                                        {
                                            m.Append("\n" + (lCurrentEntryNum + (lTempCount * 4096)).ToString("X8") + "\t");
                                            lColumns = 0;
                                        }
                                        m.Append(lValue.ToString("X8") + " ");
                                        //lCurrentEntryNum++;
                                        lMemFile.Write(m);
                                        m = new StringBuilder();
                                    }
                                    m.Append("\n");
                                }
                            }

                        m.Append("\n\n");

                    }
                }
            lMemFile.Write(m);
            lMemFile.Close();
        }
        #region Event Handlers

        static void ServiceInterruptStart(object sender, Processor_80x86.CustomEventArgs e)
        {
            if (e.ProcInfo.sCurrentDecode.ExceptionThrown)
            {
                mIRQIsReallyException = true;
                IRQOrExceptionNum = (byte)e.ProcInfo.sCurrentDecode.ExceptionNumber;
            }
            else
            {
                IRQOrExceptionNum = e.ProcInfo.mLastIRQVector;
                mIRQIsReallyException = false;
            }
            mShowIRQServiceMessage = true;
        }

        internal static void HandleTakeADump(object sender, Processor_80x86.CEAStartDebugging e)
        {
            if (default_set.Default.DumpAtEnabled && mSystem.mProc.regs.CS.Value == default_set.Default.DumpAtSegment && (mSystem.mProc.regs.EIP == default_set.Default.DumpAtAddress || default_set.Default.DebugAtAddress == 0xFFFFFFFF))
                if (!mNowDebugging)
                    DumpMemToFile();
        }
        
        
        internal static void HandleStartDebugging(object sender, Processor_80x86.CEAStartDebugging e)
        {
            mSingleStep = mSystem.mProc.mSingleStep;
            DoSingleStep(mSingleStep);
            if (!mNowDebugging)
                if ((e.ProcessorFoundDebuggingStart) || default_set.Default.DebugAtEnabled && mSystem.mProc.regs.CS.Value == default_set.Default.DebugAtSegment && (mSystem.mProc.regs.EIP == default_set.Default.DebugAtAddress || default_set.Default.DebugAtAddress == 0xFFFFFFFF))
                    AttemptToEnableDebugging();
                else if (default_set.Default.DieAtEnabled && mSystem.mProc.regs.CS.Value == default_set.Default.DieAtSegment && (mSystem.mProc.regs.EIP == default_set.Default.DieAtAddress || default_set.Default.DieAtAddress == 0xFFFFFFFF))
                    mSystem.mProc.PowerOff = true;
            //mMainForm.HandleSingleStepEvent(new object(), new Processor_80x86.CustomEventArgs(mSystem.mProc));
        }
        static void HandleParseDoneEvent(object sender, Processor_80x86.CustomEventArgs e)
        {
            Processor_80x86 lSender = (Processor_80x86)sender;
            Processor_80x86 proc = lSender;

            if (mNowDebugging)
            {
                DebugPrint();
                if (DebugCodeFile.BaseStream.Length > default_set.Default.DebugCodeMaxFileSize)
                    OpenNewDebugCodeFile();
            }

            //if (mShowDebugOnConsole)
            //{
            //    if (lSender.mCurrentInstruction.Name == "INT")
            //        mCurrINTNumArray.Insert(0, lsender.sCurrentDecode.Op1Value.OpByte.ToString("x").ToUpper().PadLeft(2, '0') + " (" + lSender.regs.AX.ToString("x").ToUpper().PadLeft(4, '0') + ")");
            //    else if ((lSender.mCurrentInstruction.Name == "IRET" || lSender.mCurrentInstruction.Name == "RETF") && mCurrINTNumArray.Count > 0)
            //        mCurrINTNumArray.RemoveAt(0);
            //    DebugPrint(lSender);
            //}
        }
        static void HandleInstructDoneEvent(object sender, Processor_80x86.CustomEventArgs e)
        {
            string lTemp;
            Processor_80x86 proc = (Processor_80x86)sender;
            if (!mNowDebugging)
            { return; }

            if (!default_set.Default.DisplayBIOSWhenDebugging)
                if (proc.regs.CS.Value == 0xf000)
                    return;
            if (!default_set.Default.DisplayVGABIOSWhenDebugging)
                if (proc.regs.CS.Value == 0xc000)
                    return;

            int lCount = 0;

            StringBuilder lOutput = new StringBuilder();
            
            bool lPrintedSomething = false;
            if (proc.sCurrentDecode.Operand1IsRef)
            {
                //if (proc.sCurrentDecode.Name.Contains("MOVS"))
                //    lOutput.AppendFormat("BYTES = {0}\n", proc.sCurrentDecode.Name);
                lTemp = GetMapFileValue(proc.sCurrentDecode.Op1Add);
                if (lTemp != "")
                    lOutput.AppendFormat("\ndest = ~{0} ({1})", lTemp, proc.sCurrentDecode.Op1Add.ToString("x").PadLeft(8, '0').ToUpper());
                else
                    lOutput.AppendFormat("\ndest = {0}", proc.sCurrentDecode.Op1Add.ToString("x").PadLeft(8, '0').ToUpper());

                try { lOutput.AppendFormat(" >{0}<", PhysicalMem.PagedMemoryAddress(proc, ref proc.sCurrentDecode, proc.sCurrentDecode.Op1Add, false).ToString("x").PadLeft(8, '0').ToUpper()); }
                catch
                {
                    //Dunno why this code is setting Exception number/error code ... removing it
                    //proc.ExceptionNumber = 0x00;
                    //proc.ExceptionErrorCode = 0x0;
                }
                lOutput.AppendFormat(" ({0})", proc.sCurrentDecode.Op1Value.OpQWord.ToString("x").PadLeft(8, '0').ToUpper());
                lCount++;
                lPrintedSomething = true;
            }
            if (proc.sCurrentDecode.Operand2IsRef && proc.sCurrentDecode.OpCode != 0x8d)
            {
                lTemp = GetMapFileValue(proc.sCurrentDecode.Op2Add);
                if (lTemp != "")
                    lOutput.AppendFormat("\t src = ~{0} ({1})", lTemp, proc.sCurrentDecode.Op2Add.ToString("x").PadLeft(8, '0').ToUpper());
                else
                    lOutput.AppendFormat("\t src = {0}", proc.sCurrentDecode.Op2Add.ToString("x").PadLeft(8, '0').ToUpper());
                try { lOutput.AppendFormat(" >{0}<", proc.mTLB.ShallowTranslate(proc, ref proc.sCurrentDecode, proc.sCurrentDecode.Op2Add, false, ePrivLvl.Kernel_Ring_0).ToString("x").PadLeft(8, '0').ToUpper()); }
                catch
                {
                    //Dunno why this code is setting Exception number/error code ... removing it
                    //proc.ExceptionNumber = 0x00;
                    //proc.ExceptionErrorCode = 0x0;
                }
                lOutput.AppendFormat(" ({0})", proc.sCurrentDecode.Op2Value.OpQWord.ToString("x").PadLeft(8, '0').ToUpper());
                lCount++;
                lPrintedSomething = true;
            }

            if (proc.sCurrentDecode.mChosenInstruction.FPUInstruction)
            {
                lOutput.AppendFormat("\n\rFPU Regs: 0={0}   1={1}   2={2}   3={3}   4={4}   5={5}   6={6}   7={7}   A={8}", proc.mFPU.DataReg[0], proc.mFPU.DataReg[1], proc.mFPU.DataReg[2], proc.mFPU.DataReg[3], proc.mFPU.DataReg[4], proc.mFPU.DataReg[5], proc.mFPU.DataReg[6], proc.mFPU.DataReg[7], proc.mFPU.StatusReg.Top);
            }

            if (proc.sCurrentDecode.OpCode == 0xCD && (proc.regs.CR0 & 0x80000000) == 0x80000000)
            {
                lOutput.AppendFormat("\n\rEAX= " + proc.mTLB.ShallowTranslate(proc, ref proc.sCurrentDecode, proc.regs.EAX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("EBX= " + proc.mTLB.ShallowTranslate(proc, ref proc.sCurrentDecode, proc.regs.EBX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("ECX= " + proc.mTLB.ShallowTranslate(proc, ref proc.sCurrentDecode, proc.regs.ECX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("EDX= " + proc.mTLB.ShallowTranslate(proc, ref proc.sCurrentDecode, proc.regs.EDX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lCount++;
                lPrintedSomething = true;
            }
            if (lPrintedSomething)
                lOutput.AppendFormat("\n\r");

            if (DebugCodeFile != null)
            {
                DebugCodeFile.Write(lOutput);
                if (lCount == 0)
                    DebugCodeFile.WriteLine();
                DebugCodeFile.Flush();
            }
        }
        static void OpenNewDebugCodeFile()
        {
            if (DebugCodeFile != null)
            {
                DebugCodeFile.Close();
                DebugCodeFile = null;
            }
            string lFileName = DebugCodeFileName();
            DebugCodeFile = new StreamWriter(lFileName);
        }
        static string DebugCodeFileName()
        {
            int ExtensionIdx;
            string filename = default_set.Default.Debug_Path + default_set.Default.Debug_Code_Filename;
            ExtensionIdx = filename.LastIndexOf(".");
            DebugCodeFileNumber++;
            filename = filename.Substring(0, ExtensionIdx) + "_" + DebugCodeFileNumber.ToString().Trim() + filename.Substring(ExtensionIdx, filename.Length - ExtensionIdx);
            return filename;
        }

        public static void DebugPrint()
        {
            Processor_80x86 sender = mSystem.mProc;
            StringBuilder lOutput, lInstrOutput;
            StringBuilder bytes = new StringBuilder();
            String lSegment = "";
            int lNum = 0;
            UInt32 lIP = 0;
            string lProt = "";
            ConsoleKeyInfo lKey;
            String lCurrIntNum = "";
            string lTemp;

            if (sender.sCurrentDecode.mChosenInstruction.Name == "INT")
            {
                mCurrINTNumArray.Insert(0, sender.sCurrentDecode.Op1Value.OpByte.ToString("x").ToUpper().PadLeft(2, '0') + " (" + sender.regs.AX.ToString("x").ToUpper().PadLeft(4, '0') + ")");
                mCurrINTRetAddrArray.Insert(0, sender.sCurrentDecode.InstructionAddress + sender.sCurrentDecode.BytesUsed);
            }
            else if (mCurrINTNumArray.Count > 0 && sender.sCurrentDecode.InstructionAddress == (UInt32)(mCurrINTRetAddrArray[0]))
            {
                mCurrINTNumArray.RemoveAt(0);
                mCurrINTRetAddrArray.RemoveAt(0);
            }

            bool lKeyRead = false;
            //if (mSingleStep)
            //    while (!lKeyRead)
            //        if (Console.KeyAvailable)
            //        {
            //            lKey = Console.ReadKey(true);
            //            if (lKey.Key == ConsoleKey.Delete)
            //                mSingleStep = false;
            //            lKeyRead = true;
            //        }

            if (!default_set.Default.DisplayBIOSWhenDebugging)
                if (sender.regs.CS.Value == 0xf000)
                    return;
            if (!default_set.Default.DisplayVGABIOSWhenDebugging)
                if (sender.regs.CS.Value == 0xc000)
                    return;
            if (sender.ProtectedModeActive && !sender.regs.FLAGSB.VM || !sender.OpSize16 || !sender.AddrSize16)
            {
                lNum = 8;
                lIP = sender.regs.EIP;
            }
            else
            {
                lNum = 4;
                lIP = sender.regs.IP;
            }
            if (sender.ProtectedModeActive && sender.regs.FLAGSB.VM)
                lProt = "Virt";
            else if (sender.ProtectedModeActive)
                lProt = "Prot";
            else
                lProt = "Real";

            lOutput = new StringBuilder();
            lInstrOutput = new StringBuilder();
            //if (mServiceInterruptStart != "")
            //{
            //    lOutput.AppendFormat("{0}\n", mServiceInterruptStart);
            //    mServiceInterruptStart = "";
            //}
            //if (mInvalidOpCodeStart != "")
            //{
            //    lOutput.AppendFormat("{0}\n", mInvalidOpCodeStart);
            //    mInvalidOpCodeStart = "";
            //}
            //if (mCurrINTNumArray.Count > 0)
            //    mCurrINTNum = (String)mCurrINTNumArray[0];
            //else
            //    mCurrINTNum = "";
            if (mCurrINTNumArray.Count > 0)
                lCurrIntNum = (String)mCurrINTNumArray[0];
            else
                lCurrIntNum = "";
            if (mShowIRQServiceMessage)
            {
                mShowIRQServiceMessage = false;
                if (mIRQIsReallyException)
                    lOutput.AppendFormat("\n********** Exception Handler {0} starts **********\n", IRQOrExceptionNum.ToString("X2"));
                else
                    lOutput.AppendFormat("\n********** Servicing IRQ (INT # {0}) starts **********\n", IRQOrExceptionNum.ToString("X2"));
            }
            if (lNum == 8)
            {
                UInt32 lStackVal = 0;
                if (sender.AddrSize16)
                    lStackVal = sender.mem.GetDWord(mSystem.mProc, ref sender.sCurrentDecode, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.SP));
                else
                    try
                    {
                        lStackVal = sender.mem.GetDWord(mSystem.mProc, ref sender.sCurrentDecode, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.ESP));
                    }
                    catch { lStackVal = 0; }
                lOutput.AppendFormat("EAX={0}  EBX={1}  ECX={2}  EDX={3}  M={4}  INT={5}   TASK={6}\n", sender.regs.EAX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EBX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.ECX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EDX.ToString("x").ToUpper().PadLeft(8, '0'), lProt.Substring(0, 1), lCurrIntNum, GlobalRoutines.GetLinuxCurrentTaskName(sender.mSystem));
                lOutput.AppendFormat("EBP={1}  ESI={2}  EDI={3}  ESP={0}  [ESP]={4}  CPL={5} ({6})   ", sender.regs.ESP.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EBP.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.ESI.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EDI.ToString("x").ToUpper().PadLeft(8, '0'), lStackVal.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CPL.ToString("X"), sender.regs.CS.Selector.access.PrivLvl.ToString("X"));
                if ((sender.regs.CR0 & 0x80000000) == 0x80000000)
                {
                    //lOutput.AppendFormat("(stackaddr={0})  TLB-C={1}\n", sender.mTLB.ShallowTranslate(mSystem.mProc, ref sender.sCurrentDecode, (DWord)(sender.regs.SS.Value + sender.regs.ESP), false, ePrivLvl.Kernel_Ring_0).ToString("X8"), sender.mTLB.mCurrEntries);

                }
                else
                    lOutput.Append("\n");
                lOutput.AppendFormat("TR={7}  LDT={0}  GDT={10}  CR0={1}  CR2={9}  CR3={8}  CR4={2}    O16={3} A16={4} SO16={5} SA16={6}\n", sender.regs.LDTR.SegSel.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR0.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR4.ToString("x").ToUpper().PadLeft(8, '0'), sender.OpSize16.ToString().Substring(0, 1), sender.AddrSize16.ToString().Substring(0, 1), sender.OpSize16.ToString().Substring(0, 1), sender.AddrSize16Stack.ToString().Substring(0, 1), sender.regs.TR.SegSel.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR3.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR2.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.GDTR.Value.ToString("x").ToUpper().PadLeft(8,'0'));
                lOutput.AppendFormat("SS={2} ({7})   DS={0} ({8})   ES={1} ({9})   CS={3} ({10})   FS={4} ({11})   GS={5} ({12})  EFLAGS={6}\n", sender.regs.DS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.ES.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.SS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.CS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.FS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.GS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.EFLAGS.ToString("x").ToUpper().PadLeft(8, '0'),
                    sender.regs.SS.DescriptorNum.ToString("X3"), sender.regs.DS.DescriptorNum.ToString("X3"), sender.regs.ES.DescriptorNum.ToString("X3"), sender.regs.CS.DescriptorNum.ToString("X3"), sender.regs.FS.DescriptorNum.ToString("X3"), sender.regs.GS.DescriptorNum.ToString("X3"));
            }
            else
            {
                lOutput.AppendFormat("AX={0}  BX={1}  CX={2}  DX={3}  M={4}  INT={5}\n", sender.regs.AX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.BX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.CX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.DX.ToString("x").ToUpper().PadLeft(lNum, '0'), lProt.Substring(0, 1), lCurrIntNum);
                lOutput.AppendFormat("BP={0}  SI={1}  DI={2}  SP={3}  [SP]={4}\n", sender.regs.BP.ToString("X4"), sender.regs.SI.ToString("X4"), sender.regs.DI.ToString("X4"), sender.regs.SP.ToString("X4"), sender.mem.GetWord(sender, ref sender.sCurrentDecode, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.SP)).ToString("X4"), sender.regs.GDTR.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.IDTR.Value.ToString("x").ToUpper().PadLeft(4, '0'));
                lOutput.AppendFormat("SS={0}  DS={1}  ES={2}  CS={3}  FLAGS={4}\n", sender.regs.SS.Value.ToString("X4"), sender.regs.DS.Value.ToString("X4"), sender.regs.ES.Value.ToString("X4"), sender.regs.CS.Value.ToString("X4"), sender.regs.FLAGS.ToString("X4"));
            }
            if (lNum == 4)
            {
                lOutput.AppendFormat(sender.regs.CS.Value.ToString("X4") + ":");
                lInstrOutput.AppendFormat(String.Format("{0:x}", sender.regs.CS.Value).ToUpper().PadLeft(4, '0') + ":");
                lOutput.AppendFormat(String.Format("{0:x}", lIP).ToUpper().PadLeft(4, '0') + " ");
                lInstrOutput.AppendFormat(String.Format("{0:x}", lIP).ToUpper().PadLeft(4, '0') + " ");
            }
            else
            {
                lOutput.AppendFormat(":" + lIP.ToString("X8") + " (" + sender.mTLB.ShallowTranslate(sender, ref sender.sCurrentDecode, (DWord)(sender.regs.CS.Value + sender.regs.EIP), false, ePrivLvl.Kernel_Ring_0).ToString("X8") + ") ");
            }
            lOutput.Append("\t");
            /*if (sender.sCurrentDecode.bytes != null)
                for (int cnt = 0; cnt < sender.sCurrentDecode.bytes.Count(); cnt++)
                    bytes.Append(String.Format("{0:x}", sender.sCurrentDecode.bytes[cnt]).ToUpper().PadLeft(2, '0'));*/
            lOutput.AppendFormat(bytes.ToString().PadRight(15, ' '));
            lInstrOutput.AppendFormat(bytes.ToString().PadRight(15, ' '));
            if (sender.sCurrentDecode.OverrideSegment != eGeneralRegister.DS &&
                sender.sCurrentDecode.OverrideSegment != eGeneralRegister.NONE)
            {
                lSegment = "* " + sender.sCurrentDecode.OverrideSegment.ToString() + " *";
            }
            else
                lSegment = "";
            /*if (sender.sCurrentDecode.bytes != null && sender.sCurrentDecode.bytes[0] == 0xf2)
                if (sender.sCurrentDecode.mChosenInstruction.Name.Contains("CMPS") || sender.sCurrentDecode.mChosenInstruction.Name.Contains("SCAS"))
                    lOutput.Append("REPNE");
                else
                    lOutput.Append("REP");*/

            /*if (sender.sCurrentDecode.bytes != null && sender.sCurrentDecode.bytes[0] == 0xf3)
                if (sender.sCurrentDecode.mChosenInstruction.Name.Contains("CMPS") || sender.sCurrentDecode.mChosenInstruction.Name.Contains("SCAS"))
                    lOutput.Append("REPE");
                else
                    lOutput.Append("REP");*/

            lOutput.AppendFormat("\t" + ((sender.sCurrentDecode.mChosenInstruction.FPUInstruction) ? "(F) " : "") + sender.sCurrentDecode.mChosenInstruction.Name + "\t" + sender.sCurrentDecode.Operand1SValue);

            //lTemp = GetMapFileValue(sender.sCurrentDecode.Op1Add);
            //if (lTemp != "")
            //    lInstrOutput.AppendFormat("\t" + sender.mCurrentInstruction.Name + "\t" + lTemp);
            //else
            //    lInstrOutput.AppendFormat("\t" + sender.mCurrentInstruction.Name + "\t" + sender.sCurrentDecode.Operand1SValue);
            //if (sender.sCurrentDecode.Operand2SValue != null)
            //{
            //    lTemp = GetMapFileValue(sender.sCurrentDecode.Op2Add);
            //    if (lTemp != "")
            //    {
            //        lOutput.AppendFormat("," + lTemp);
            //        lInstrOutput.AppendFormat("," + lTemp + "(" + sender.sCurrentDecode.Operand2SValue + ")");
            //    }
            //    else
            //    {
            //        lOutput.AppendFormat("," + sender.sCurrentDecode.Operand2SValue);
            //        lInstrOutput.AppendFormat("," + sender.sCurrentDecode.Operand2SValue);
            //    }
            //}
            //if (sender.sCurrentDecode.Operand3SValue != null)
            //{
            //    lOutput.AppendFormat("," + sender.sCurrentDecode.Operand3SValue);
            //    lInstrOutput.AppendFormat("," + sender.sCurrentDecode.Operand3SValue);
            //}
            lOutput.Append(sender.sCurrentDecode.CombinedSValue);
            lInstrOutput.Append(sender.sCurrentDecode.CombinedSValue);
            lOutput.Append("\t" + lSegment);
            lInstrOutput.Append("\t" + lSegment);

            lTemp = GetMapFileValue(sender.regs.CS.Value + lIP);
            if (lTemp != "")
                lOutput.AppendFormat("\t\t\t\t\t\t\t\t\t\t @ {0}", lTemp);
            else


                if (mShowAssemblerCode)
                {
                }
            if (DebugCodeFile != null)
            {
                DebugCodeFile.Write(lOutput);
                //temporarily added flush to test something
                //DebugCodeFile.Flush();
            }
            sender.sCurrentDecode.ExceptionThrown = false;
        }
        static void AttemptToEnableDebugging()
        {
            if (!default_set.Default.DebugToFile && !default_set.Default.DebugToScreen)
                System.Diagnostics.Debug.WriteLine("DebugAt: No destination (file or screen) enabled, can't debug at assigned address!");
            else
            {
                if (default_set.Default.DebugToFile && DebugCodeFile == null)
                {
                    OpenNewDebugCodeFile();
                }
                if (default_set.Default.SaveDebugPorts && DebugPortFile == null)
                {
                    DebugPortFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Ports_Filename);
                    System.Diagnostics.Debug.WriteLine("Now debugging to file: " + default_set.Default.Debug_Path + default_set.Default.Debug_Ports_Filename);
                    mSystem.mProc.ports.HandleDataInDone += HandleDataInEvent;
                    mSystem.mProc.ports.HandleDataOutDone += HandleDataOutEvent;
                }
                if (default_set.Default.DebugToScreen)
                {

                    mShowAssemblerCode = true;
                }
                mSystem.mProc.mGenerateDecodeStrings = true;
                Thread.Sleep(25);
                mNowDebugging = true;
                mSystem.mProc.mExportDebug = true;
                mSystem.mProc.ServiceInterruptStart += ServiceInterruptStart;
                mSystem.mProc.ParseCompleteEvent += HandleParseDoneEvent;
                mSystem.mProc.InstructCompleteEvent += HandleInstructDoneEvent;
            }
        }
        public static void StopDebugging()
        {
            mSystem.mProc.StartDebugging -= HandleStartDebugging;
            {
                Program.mStartDebuggingInstanceCount -= 1;
                if (Program.mStartDebuggingInstanceCount == 0)
                    Program.mSystem.mProc.StartDebugging -= Program.HandleStartDebugging;
            }
            mSystem.mProc.ports.HandleDataInDone -= HandleDataInEvent;
            mSystem.mProc.ports.HandleDataOutDone -= HandleDataOutEvent;
            mSystem.mProc.ServiceInterruptStart -= ServiceInterruptStart;
            mSystem.mProc.ParseCompleteEvent -= HandleParseDoneEvent;
            mSystem.mProc.InstructCompleteEvent -= HandleInstructDoneEvent;
            mNowDebugging = false;
            mSystem.mProc.mGenerateDecodeStrings = false;
            Thread.Sleep(150);
            if (DebugCodeFile != null)
            {
                DebugCodeFile.Close();
                DebugCodeFile = null;
                System.Diagnostics.Debug.WriteLine("Stopped debugging to file");
            }
            if (DebugPortFile != null)
            {
                DebugPortFile.Close();
                DebugPortFile = null;
            }
            mSystem.mProc.mExportDebug = false;
        }
        #endregion

        [STAThread]
        static void Main()
        {
            //Initialize();
            //s.BytesUsed = (char)34;
            //Stopwatch st = new Stopwatch();
            //st.Start();
            //for (int cnt = 0; cnt < 10000000; cnt++)
            //    DoWork2(ref s, 400);
            //st.Stop();
            //MessageBox.Show("MS taken: " + st.ElapsedMilliseconds);

            //Console related
            //Process p = Process.GetCurrentProcess();
            //p.PriorityBoostEnabled = true;
            //p.PriorityClass = ProcessPriorityClass.High;
            //p.ProcessorAffinity = (IntPtr)0x1C;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            AllocConsole();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            Console.OutputEncoding = encoding;
            Console.TreatControlCAsInput = true;
            Console.Title = "VirtualProcessor - Teminal";
            Console.WriteLine("Press <POWER> button to start the emulation.!");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Main form related
            mMainForm = new frmMain_New();
            //hConsole = Utility.FindWindow(IntPtr.Zero,"VirtualProcessor - Teminal");       

            hConsoleOfChild = Utility.FindWindowByCaption(IntPtr.Zero, "VirtualProcessor - Teminal");
            hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            //if (!default_set.Default.StartVideoDetached)
            //{
            //    SetParent(hConsoleOfChild, mMainForm.picConsoleDock.Handle);
            //    Utility.SendMessage(mMainForm.picConsoleDock.Handle, WM_CHANGEUISTATE, 3, 0);
            //    Utility.ToggleTitleBar(hConsoleOfChild, false);
            //    Utility.SetWindowLong(hConsoleOfChild, GWL_EXSTYLE, WS_EX_TOOLWINDOW);
            //}
            bShowTheCursor = true;
            try { Console.CursorVisible = true; }
            catch { }
            mMainForm.Show();
            mMainForm.hConsole = hConsoleOfChild;
            //Utility.ToggleTitleBar(hConsoleOfChild, false);
            //mMainForm.WindowState = FormWindowState.Maximized;
            mMainForm.mSystem = mSystem;
            mMainForm.DoResize();
            Thread.CurrentThread.Name = "UIThread";
            Utility.SetFocus(hConsoleOfChild);
            if (default_set.Default.MapFile_Use)
                ReadMapFile();
            Application.Run(mMainForm);
            if (default_set.Default.DumpMemOnShutdown)
                Program.DumpMemToFile();
            do { Thread.Sleep(100); } while (StartEmulatorThreadActive);
        }
        static void ReadMapFile()
        {
            StreamReader r = new StreamReader(default_set.Default.MapFile);
            string lTemp;
            int cnt = 0;
            for (int cnt2 = 0; cnt2 < 10000; cnt2++)
                MapFileAddressesSorted[cnt2] = 0xFFFFFFFF;
            while (!r.EndOfStream)
            {
                lTemp = r.ReadLine();
                MapFileAddresses[cnt] = (UInt32)int.Parse(lTemp.Substring(0, lTemp.IndexOf(" ")), System.Globalization.NumberStyles.HexNumber);
                if (MapFileAddresses[cnt] < 0xc0000000)
                    MapFileAddresses[cnt] += 0xc0000000;
                MapFileAddressesSorted[cnt] = MapFileAddresses[cnt];
                MapFileEntries[cnt++] = lTemp.Substring(lTemp.LastIndexOf(" ") + 1, lTemp.Length - lTemp.LastIndexOf(" ") - 1);
            }
            MapFileEntryCount = cnt;
            Array.Sort(MapFileAddressesSorted);
        }
        public static string GetMapFileValue(UInt32 Value)
        {
            //temporarily returning, screwing up paging again!
            //return "";
            if (!default_set.Default.MapFile_Use && ( Value < 0xc0000000 || Value > 0xCFFFFFFF) )
                return "";
            for (int cnt = 0; cnt < MapFileEntryCount; cnt++)
                if (MapFileAddressesSorted[cnt] == Value)
                    return GetMapFileEntryUnsorted(MapFileAddressesSorted[cnt]);
                else if (MapFileAddressesSorted[cnt] > Value)
                {
                    if (cnt == 0)
                        return "";
                    if (MapFileEntries[cnt - 1] == "gcc2_compiled.")
                        return GetMapFileEntryUnsorted(MapFileAddressesSorted[cnt - 2]);
                    return GetMapFileEntryUnsorted(MapFileAddressesSorted[cnt - 1]);
                }
            return "";
        }
        public static string GetMapFileEntryUnsorted(UInt32 Key)
        {
            for (int cnt = 0; cnt < MapFileEntryCount; cnt++)
                if (MapFileAddresses[cnt] == Key)
                    return MapFileEntries[cnt];
            return "";
        }
    }
}
