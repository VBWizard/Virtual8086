using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualProcessor;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Console = System.Console;
using Word = System.UInt16;
using DWord = System.UInt32;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;

//DebugCodeMaxFileSize
namespace Virtual8086_Console
{
    class Virtual8086_Console
    {
#if WIN
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
#endif
        const int DEBUG_CONSOLE_MAIN_WINDOW = 0, DEBUG_CONSOLE_CODE_VIEWPORT = 1, DEBUG_CONSOLE_COMMAND_VIEWPORT = 2, DEBUG_CONSOLE_ERROR_VIEWPORT = 3;
        static uint STD_OUTPUT_HANDLE = 0xfffffff5;

        static PCSystem mSystem = null;
        static String mDebugInput;
        static bool mShowAssemblerCode = default_set.Default.ShowAssemblerCode, mStartEmulation, mResetEmulation,
            mResetMainConsole, mExitMainDisplayThread, mSingleStep, mNowDebugging, mDebugWindowClosed, mShowIRQServiceMessage, mIRQIsReallyException;
        static bool mbShowIPS = true;
        static IntPtr hConsole;
        static Thread DisplayThread, DebugDisplayThread, EmuThread, KBDThread, PortThread;
        static ThreadStart DisplayThreadStart, DebugDisplayThreadStart, EmuThreadStart, KBDThreadStart, PortThreadStart;
        static int EmulationRunCount = 0, mCommandHistoryPtr = 0, DebugCodeFileNumber = 0;
        static StreamWriter DebugCodeFile = null, DebugPortFile = null;
        static ArrayList mCurrINTNumArray = new ArrayList(), mCurrINTRetAddrArray = new ArrayList();
        static ArrayList mCommandHistory = new ArrayList();
        static byte IRQOrExceptionNum = 0;
        static byte lPortInByte = 0;

        static void Main(string[] args)
        {
            //Main console configuration
#if WIN
            hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
#endif
            Console.OutputEncoding = Encoding.GetEncoding(28591);
            Console.TreatControlCAsInput = true;
            MainLoop();

        }

        private static void MainLoop()
        {
            if (EmulationRunCount == 0 && default_set.Default.StartEmuOnLoad)
            {
                EmuThreadStart = new ThreadStart(StartEmulator);
                EmuThread = new Thread(EmuThreadStart);
                EmuThread.Start();
                EmuThread.Priority = ThreadPriority.Highest;
                Thread.Sleep(1000);
            }
            while (1==1)
            {
                Thread.Sleep(1000);
                //Application.DoEvents();
                if (mStartEmulation)
                {
                    EmuThreadStart = new ThreadStart(StartEmulator);
                    EmuThread = new Thread(EmuThreadStart);
                    EmuThread.Start();
                    EmuThread.Priority = ThreadPriority.Highest;
                    mStartEmulation = false;
                }
                if (mResetEmulation)
                {
                    mSystem.ShutDown();
                    mStartEmulation = true;
                    mResetEmulation = false;
                }
				Thread.Sleep(500);
				if ((mSystem != null) && mSystem.mProc.PowerOff)
				{
					return;
				}
            }
        }

        private static void StartEmulator()
        {
#if DOTRIES
            try
            {
#endif
                mSystem = new PCSystem((DWord)default_set.Default.PhysicalMemoryAmount, default_set.Default.ProcessorType, default_set.Default.CMOSPathAndFile);
                if (default_set.Default.TimerTickInterval == 0)
                    mSystem.mProc.mTimerTickMSec = 10000000;
                else
                    mSystem.mProc.mTimerTickMSec = default_set.Default.TimerTickInterval;
                if (default_set.Default.CalculateTimings)
                    mSystem.mProc.mCalculateInstructionTimings = true;
                else
                    mSystem.mProc.mCalculateInstructionTimings = false;
                //if (default_set.Default.FD1PathAndFile != "")
                //if (default_set.Default.FD1PathAndFile != "")
                //    mSystem.LoadDrive(default_set.Default.FD1PathAndFile, default_set.Default.FD1Type, 1);
                //if (default_set.Default.FD2PathAndFile != "")
                //    mSystem.LoadDrive(default_set.Default.FD2PathAndFile, default_set.Default.FD1Type, 2);
                //mSystem.LoadDrive(@"E:\ISOs\Floppies\damn-bootfloppy.img", 1); //Graphical
                //mSystem.LoadDrive(@"e:\isos\Floppies\GRUB_BOOT_allinone.img",1);
                //mSystem.LoadDrive(@"e:\isos\Floppies\trinux-0.890.flp", 1);
                //mSystem.LoadDrive(@"E:\Downloads\Dev\picobsd.img", 1);
                //mSystem.LoadDrive(@"E:\ISOs\Floppies\Unix\a-linux\a-linux.img", 1);
                //mSystem.LoadDrive(@"e:\isos\Floppies\banshee-b.img", 1);
                //mSystem.LoadDrive(@"e:\isos\Floppies\freebsd_v4_kern.flp", 1);
                //mSystem.LoadDrive(@"e:\isos\Floppies\disk1.img", 1);

                //mSystem.LoadDrive(@"E:\ISOs\Floppies\Unix\floppix\disk1.img", 1);
                //mSystem.LoadDrive(@"E:\ISOs\Floppies\Unix\floppix\disk2.img", 2);
                mSystem.DebugFilePath = default_set.Default.Debug_Path;
                if (default_set.Default.HD1PathAndFile != "")
                    mSystem.LoadDrive(default_set.Default.HD1PathAndFile, default_set.Default.HD1Cyls, default_set.Default.HD1Heads, default_set.Default.HD1SPT, default_set.Default.HD1DeviceType);
                if (default_set.Default.HD2PathAndFile != "")
                    mSystem.LoadDrive(default_set.Default.HD2PathAndFile, default_set.Default.HD2Cyls, default_set.Default.HD2Heads, default_set.Default.HD2SPT, default_set.Default.HD2DeviceType);

                mSystem.LoadRom(default_set.Default.BIOSPathAndFile, eRomTypes.BIOS, 0);
                mSystem.LoadRom(default_set.Default.VideoBiosPathAndFile, eRomTypes.Video, 0);

                if (default_set.Default.MiscROMPathAndFile != "")
                    mSystem.LoadRom(default_set.Default.MiscROMPathAndFile, eRomTypes.Misc, default_set.Default.MiscROMLoadAddress);

                mExitMainDisplayThread = false;
                DisplayThreadStart = new ThreadStart(ShowDisplay);
                DisplayThread = new Thread(DisplayThreadStart);
                DisplayThread.Start();

                KBDThreadStart = new ThreadStart(HandleDisplayKeyboard);
                KBDThread = new Thread(KBDThreadStart);
                KBDThread.Start();

                PortThreadStart = new ThreadStart(HandlePortIO);
                PortThread = new Thread(PortThreadStart);
                PortThread.Start();
                
                mSystem.mProc.ServiceInterruptStart += ServiceInterruptStart;
                mSystem.mProc.ParseCompleteEvent += HandleParseDoneEvent;
                mSystem.mProc.InstructCompleteEvent += HandleInstructDoneEvent;

#if DOTRIES
                try
                {
#endif
#if DEBUG
            mSystem.Debuggies.DebugFDC =  mSystem.Debuggies.DebugPIC = mSystem.Debuggies.DebugCPU = mSystem.Debuggies.DebugHDC = mSystem.Debuggies.DebugDMA  = mSystem.Debuggies.DebugMemPaging = true;
#else
                    /*mSystem.Debuggies.DebugCPU = mSystem.Debuggies.DebugMemPaging = true;*/
            //mSystem.Debuggies.DebugMemPaging = true;//mSystem.Debuggies.DebugCPU = true; //mSystem.Debuggies.DebugHDC
            mSystem.Debuggies.DebugSerial = true;
            mSystem.Debuggies.DebugCPU = true;
            mSystem.Debuggies.DebugMemPaging = true;

#endif
            mSystem.ColdBoot();
#if DOTRIES
                }
                catch (Exception exc)
                {
                    mExitMainDisplayThread = true;
                }
#endif
                mExitMainDisplayThread = true;
                mSystem.mProc.ParseCompleteEvent -= HandleParseDoneEvent;
                mSystem.mProc.InstructCompleteEvent -= HandleInstructDoneEvent;
                if (DebugCodeFile != null)
                {
                    //Thread.Sleep(4000);
                    DebugCodeFile.Flush();
                    DebugCodeFile.Close();
                }
                if (DebugPortFile != null)
                    DebugPortFile.Close();
                if (default_set.Default.DumpMemOnShutdown == true)
                    DumpMemToFile();
#if DOTRIES
            }
            catch (Exception exc2)
            {
                System.Diagnostics.Debug.WriteLine("StartEmulator error --> " + exc2.Message);
                MessageBox.Show("StartEmulator error --> " + exc2.Message);
            }
#endif
        }

        #region Main Display Related
        public static void ShowDisplay()
        {
            byte[] mScreenBytes = new byte[80 * 100 * 2];
            Processor_80x86 sender = (Processor_80x86)mSystem.mProc;
            int lPosX, lPosY;
            int lCurrPosY = 0;
            int lBS = 0xB8000;
            int lBSOffset = 0;
            int lBPage;
            int lBPageSize;
            double lIPS = 0;
            byte lBCursorX = 0, lBCursorY = 0;
            String ProcMode = "R", Floppy = " ", HardDrive = " ";
            TimeSpan TotalTime;

            while (1 == 1)
            {
                lBPage = 0;  //sender.mem.GetByte(sender, 0x400 + 0x62);
                lBSOffset = 0; // = sender.mem.GetWord(sender, 0x400 + 0x4e);
                lBPageSize = 0; // sender.mem.GetWord(sender, 0x400 + 0x4c);
                if (lBPageSize == 0)
                    lBPageSize = 12000;
                lBS = 0xb8000 + lBSOffset;

                lPosX = 0; lPosY = 0; lCurrPosY = 0;
                //Console.Clear();
                //Console.SetCursorPosition(0, 0);
                for (int cnt = 0; cnt < (lBPageSize) -2; cnt += 2)
                {
                    lPosY = (cnt / 160);
                    if (lPosY != lCurrPosY)
                    {
                        lCurrPosY++;
                        lPosX = 0;
                    }
                    else
                        lPosX++;
                    if (mScreenBytes[cnt] != PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)] || // sender.mem.mMemBytes[(UInt32)(lBS + cnt)] || 
                        mScreenBytes[cnt + 1] != PhysicalMem.mMemBytes[(UInt32)(lBS + cnt + 1)])
                    {
                        mScreenBytes[cnt] = PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)];
                        mScreenBytes[cnt + 1] = PhysicalMem.mMemBytes[(UInt32)(lBS + cnt + 1)];
                        try { Console.SetCursorPosition(lPosX, lPosY); } catch {}
                        Console.Write(System.Convert.ToChar(PhysicalMem.mMemBytes[(UInt32)(lBS + cnt)]));
#if WIN
                        SetConsoleTextAttribute(hConsole, PhysicalMem.mMemBytes[(UInt32)(lBS + cnt + 1)]);
#endif
                    }
                    if (mResetMainConsole)
                    {
                        mScreenBytes = new byte[80 * 50 * 2];
                        for (int c = 0; c < (80 * 50 * 2) / 2; c += 2)
                            if (c % 2 == 0)
                                mScreenBytes[c] = 20;
                            else
                                mScreenBytes[c] = 7;
                        Console.Clear();
                        mResetMainConsole = false;
                        continue;
                    }
                    if (mExitMainDisplayThread)
                        return;
                }
                //X = lBCursorPos / 50 / 2
                //Y = 
                
                lBCursorX = PhysicalMem.mMemBytes[(UInt32)(0x450)]; // sender.mem.GetByte(sender, 0x400 + 0x50);
                lBCursorY = PhysicalMem.mMemBytes[(UInt32)(0x451)]; // sender.mem.GetByte(sender, 0x400 + 0x51);
                if (((DateTime.Now - sender.StartedAt).Seconds) > 0)
                {
                    TotalTime = DateTime.Now - mSystem.mProc.StartedAt;

                    if (mbShowIPS)
                    {
                        lIPS = mSystem.mProc.InstructionsExecuted / TotalTime.TotalSeconds;
                        lIPS /= 1000;
                        //lIPS = (int)Math.Truncate((decimal)(System.Convert.ToInt64(sender.InstructionsExecuted) / (DateTime.Now - sender.StartedAt).TotalSeconds));
                        try { Console.SetCursorPosition(70, 0); }
                        catch { }
                        Console.Write(lIPS.ToString("#,###") + "   ");
                    }
                }
                
                if (mSystem.mProc.InstructionsExecuted > 0)
				{
	                try {Console.SetCursorPosition(67, 0);} catch{}
					Console.Write(sender.mSystem.DeviceBlock.mHDC.ControllerBusy?"H":" " );
				}
                //Console.Write(" - " + sender.mem.GetDWord(sender, 0x00103444).ToString("X8"));
                try { Console.SetCursorPosition(lBCursorX, lBCursorY);} catch {}
                //Application.DoEvents();
                Thread.Sleep(100);
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
                //Application.DoEvents();
                Thread.Sleep(50);
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
                mSystem.mProc.mExportDebug = true;
                //if (mShowDebugOnConsole)
                //    StopTimer();
                //else
                //{
                //    Console.Clear();
                //    //mScreenReset = true;
                //    StartTimer();
                //    DebugConsoleWriteLine("Showing main display");
                //}
                return;
            }
            if (lKey.KeyChar == 0x03)
                lKeyScan = "C";
            else if (lKey.Key == ConsoleKey.F10) mProc.PowerOff = true;
            //else if (lKey.Key == ConsoleKey.Home) mSystem.ResetSystem();
            else if (lKey.Key == ConsoleKey.Delete) mSingleStep ^= true;
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
            //else if (lKeyScan == "OEM3")
            //{
            //    mSystem.mProc.mem.SetByte(0x417, (byte)(mSystem.mProc.mem.GetByte(0x417) | 0x04));
            //    lKeyScan = "C";
            //}
            else if (lKeyScan == "F12")
            {
                mResetMainConsole = true;
                return;
            }
            if (lKeyScan != "")
            {
                if ((lKey.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x1D);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x04);
                }
                else
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x1D | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xFB);
                }
                if ((lKey.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x2a);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x02);
                }
                else
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x2a | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xFD);
                }
                if ((lKey.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x38);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] | 0x08);
                }
                else
                {
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode(0x38 | 0x80);
                    PhysicalMem.mMemBytes[0x417] = (byte)(PhysicalMem.mMemBytes[0x417] & 0xF7);
                }
            }
            try
            {
                scancode = mSystem.DeviceBlock.mKeyboard.ScanCodeDict[lKeyScan];
                if (scancode > 0)
                    mSystem.DeviceBlock.mKeyboard.GenerateScanCode((ulong)scancode);
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
        public static void HandlePortIO()
        {
            Processor_80x86 sender = (Processor_80x86)mSystem.mProc;
            while (sender.mSystem.DeviceBlock == null)
                Thread.Sleep(10);
            //while (1 == 1)
            //{
            //    if (sender.mSystem.DeviceBlock.mSerial != null && !sender.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.thr_empty)
            //    {
            //        if (sender.mSystem.DeviceBlock.mSerial.serialPorts[0].fifo_cntl.enable)
            //            lPortInByte = sender.mSystem.DeviceBlock.mSerial.serialPorts[0].tx_fifo[0];
            //        else
            //            lPortInByte = sender.mSystem.DeviceBlock.mSerial.serialPorts[0].thrbuffer;
            //        sender.mSystem.DeviceBlock.mSerial.serialPorts[0].line_status.thr_empty = true;
            //    }
            //    Thread.Sleep(10);
            //    if (mExitMainDisplayThread)
            //        return;
            //}
        }
        #endregion

        #region "Dump" methods
        public static void DumpMemToFile()
        {
            StringBuilder m = new StringBuilder();
            StringBuilder p = new StringBuilder();

            UInt32 lSeg;
            UInt32 lOfs;
            TimeSpan TotalTime = mSystem.mProc.StoppedAt - mSystem.mProc.StartedAt;
            sInstruction sIns = new sInstruction();

            m.Append("\n\rStarted @ " + mSystem.mProc.StartedAt + "\n\rStopped @ " + mSystem.mProc.StoppedAt + "\n\r");
            m.Append("\n\rInstructions decoded: " + mSystem.mProc.InstructionsDecoded + " (" + Math.Round(mSystem.mProc.InstructionsDecoded / TotalTime.TotalSeconds, 0) + ")\n\r");
            m.Append("\n\rInstructions executed: " + mSystem.mProc.InstructionsExecuted + " (" + Math.Round(mSystem.mProc.InstructionsExecuted / TotalTime.TotalSeconds, 0) + ")\n\r");
            //m.Append(cValue.createCount);
            m.Append("\n\rInstruction cache hits: " + mSystem.mProc.mCacheHits);
            m.Append("\n\rInstruction cache misses: " + mSystem.mProc.mCacheMisses);
            m.Append("\n\rTLB Hits: " + mSystem.mProc.mTLB.mHits);
            m.Append("\n\rTLB Misses: " + mSystem.mProc.mTLB.mMisses);
            m.Append("\n\rTLB Flushes: " + mSystem.mProc.mTLB.mFlushes + "\n\r");
            //m.Append("\n\rTime in timed section: " + mSystem.mProc.mMSinTimedSection + "\n\r");
            int mLength = PhysicalMem.mMemBytes.Length;
            if (!default_set.Default.DumpMemAbove1MB)
                mLength = 1024 * 1024;
            for (UInt32 cnt = 0; cnt < mLength; cnt++)
            {
                if (cnt % 16 == 0)
                {
                    if (cnt > 0)
                        m.Append(" ");
                    if (cnt > 0)
                        for (UInt32 cnt2 = cnt - 16; cnt2 < cnt; cnt2++)
                            if (PhysicalMem.mMemBytes[cnt2] >= 0x20 && PhysicalMem.mMemBytes[cnt2] <= 0x7e)
                                m.Append(System.Convert.ToChar(PhysicalMem.mMemBytes[cnt2]));
                            else
                                m.Append(".");
                    if (cnt > 0)
                        m.Append("\n\r");
                    lSeg = (UInt32)(PhysicalMem.GetSegForLoc(mSystem.mProc, cnt ));
                    lOfs = PhysicalMem.GetOfsForLoc(mSystem.mProc, cnt);
                    m.AppendFormat("{0}:{1}\t", lSeg.ToString("x").ToUpper().PadLeft(8, '0'), lOfs.ToString("x").ToUpper().PadLeft(4, '0'));
                }
                m.AppendFormat("{0} ", PhysicalMem.mMemBytes[cnt].ToString("x").ToUpper().PadLeft(2, '0'));
            }

            p.AppendFormat("\n\r\n\r");

            m.Append("\n\r\n\rInstructions: \n\r");

            foreach (Instruct i in mSystem.mProc.Instructions)
            {
                m.AppendFormat("\t{0}\t{1}\t{2}\t", i.Name, i.UsageCount, System.Math.Round(i.TotalTimeInInstruct / 1000, 3));
                if (i.UsageCount > 0)
                    m.AppendFormat("{0}\n", ((i.TotalTimeInInstruct / 1000) / i.UsageCount));
                else
                    m.AppendFormat("0\n");
            }

            Processor_80x86 mProc = mSystem.mProc;
            DWord lStackVal = 0;
            m.AppendFormat("\n\r\n\r");
            //if (mSystem.mProc.regs.SS.Selector.granularity.OpSize32)
                //lStackVal = new cValue(mSystem.mProc.mem.GetDWord(mSystem.mProc, PhysicalMem.GetLocForSegOfs(mSystem.mProc, mSystem.mProc.regs.SS.Value, mSystem.mProc.regs.SP)));
            //else
                //lStackVal = new cValue(mSystem.mProc.mem.GetDWord(mSystem.mProc, mSystem.mProc.regs.SS.Value + mSystem.mProc.regs.ESP));
            m.AppendFormat("EAX={0}  EBX={1}  ECX={2}  EDX={3}  M={4}  INT={5}\n\r", mSystem.mProc.regs.EAX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EBX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.ECX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EDX.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.OperatingMode.ToString().Substring(0, 1), 0);
            m.AppendFormat("EBP={1}  ESI={2}  EDI={3}  ESP={0}  [ESP]={4}  EIP={5}\n\r", mSystem.mProc.regs.ESP.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EBP.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.ESI.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.EDI.ToString("x").ToUpper().PadLeft(8, '0'), lStackVal.ToString("x").ToUpper().PadLeft(8, '0'), mProc.regs.EIP.ToString("X8"));
            m.AppendFormat("LDT={1}  CR0={2}  CR2={9}   CR3={8}  CR4={3}    O16={4} A16={5} SO16={6} SA16={7}\n\r", mSystem.mProc.regs.GDTR.Value.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.LDTR.Value.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR0.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR4.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.OpSize16.ToString().Substring(0, 1), mSystem.mProc.AddrSize16.ToString().Substring(0, 1), mSystem.mProc.OpSize16.ToString().Substring(0, 1), mSystem.mProc.AddrSize16Stack.ToString().Substring(0, 1), mSystem.mProc.regs.CR3.ToString("x").ToUpper().PadLeft(8, '0'), mSystem.mProc.regs.CR2.ToString("x").ToUpper().PadLeft(8, '0'));
            m.AppendFormat("SS={2} ({7})   DS={0} ({8})   ES={1} ({9})   CS={3} ({10})   FS={4} ({11})   GS={5} ({12})  EFLAGS={6}\n\r", mSystem.mProc.regs.DS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.ES.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.SS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.CS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.FS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.GS.Value.ToString("x").ToUpper().PadLeft(4, '0'), mSystem.mProc.regs.EFLAGS.ToString("x").ToUpper().PadLeft(8, '0'),
                mSystem.mProc.regs.SS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.DS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.ES.DescriptorNum.ToString("X3"), mSystem.mProc.regs.CS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.FS.DescriptorNum.ToString("X3"), mSystem.mProc.regs.GS.DescriptorNum.ToString("X3"));
            m.AppendFormat("GDT={0}   LDT={1}   IDT={2}\n\r", mProc.regs.GDTR.Base.ToString("X8"), mProc.regs.LDTR.Base.ToString("X8"), mProc.regs.IDTR.Base.ToString("X8"));
            m.AppendFormat("CR0={0}   CR2={1}   CR3={2}   CR4={3}\n\r", mProc.regs.CR0.ToString("X8"), mProc.regs.CR2.ToString("X8"), mProc.regs.CR3.ToString("X8"), mProc.regs.CR4.ToString("X8"));
            m.AppendFormat("TR={0}    EIP={1}\n\r", mProc.regs.TR.SegSel.ToString("X8"), mProc.regs.EIP.ToString("X"));
            m.Append("Next 80 bytes at CS:EIP\n\t");
            try
            {
                for (UInt32 cnt1 = 0; cnt1 < 40; cnt1++)
                    m.AppendFormat("{0} ", mProc.mem.GetByte(mProc, ref sIns, (UInt32)(mProc.mCurrentInstruction.DecodedInstruction.InstructionAddress + cnt1)).ToString("X2"));
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
            m.Append("Logical\tPhysical\tType");
            for (int cnt = 0; cnt < mProc.mTLB.mCurrEntryPtr;cnt++ )
                m.AppendFormat( "{0}\t{1}\t{2}\n\r",mProc.mTLB.mLogicalAddr[cnt].ToString("X8"), mProc.mTLB.mPhysAddr[cnt].ToString("X8"), mProc.mTLB.mType[cnt].ToString("X4"));

            //for (int cnt = 0; cnt < 0xFFFF; cnt++)
            //    if (mProc.OpCodeCounts[cnt] != 0)
            //        m.AppendFormat("{0}: {1}\n\r",cnt.ToString("X4"), mProc.OpCodeCounts[cnt].ToString());

            StreamWriter lMemFile = new StreamWriter(default_set.Default.Debug_Path + default_set.Default.Debug_Mem_Filename.Replace(".","_" + mProc.mSystem.mFileSuffix + "."));
            lMemFile.Write(m);
            lMemFile.Close();
        }
        #endregion

        #region Event Handlers

        static void ServiceInterruptStart(object sender, Processor_80x86.CustomEventArgs e)
        {
            if (e.ProcInfo.mCurrentInstruction.DecodedInstruction.ExceptionThrown)
            {
                mIRQIsReallyException = true;
                IRQOrExceptionNum = (byte)e.ProcInfo.mCurrentInstruction.DecodedInstruction.ExceptionNumber;
            }
            else
            {
                IRQOrExceptionNum = e.ProcInfo.mLastIRQVector;
                mIRQIsReallyException = false;
            }
            mShowIRQServiceMessage = true;
        }

        static void HandleParseDoneEvent(object sender, Processor_80x86.CustomEventArgs e)
        {
            Processor_80x86 lSender = (Processor_80x86)sender;
            Processor_80x86 proc = lSender;

            if (default_set.Default.DieAtEnabled)
                if (lSender.regs.CS.Value == default_set.Default.DieAtSegment && lSender.regs.EIP == default_set.Default.DieAtAddress)
                    lSender.PowerOff = true;

            if (default_set.Default.DebugAtEnabled && !mNowDebugging /* && lSender.mCurrentInstruction.Name=="JMP"*/)
                if (mSystem.mProc.OperatingMode == ProcessorMode.Protected)
                {
                    if (mSystem.mProc.regs.CS.Value == default_set.Default.DebugAtSegment && (mSystem.mProc.regs.EIP == default_set.Default.DebugAtAddress || default_set.Default.DebugAtAddress == 0xFFFF))
                        AttemptToEnableDebugging();
                }
                else
                    if (mSystem.mProc.regs.CS.Value == default_set.Default.DebugAtSegment && (mSystem.mProc.regs.EIP == default_set.Default.DebugAtAddress || default_set.Default.DebugAtAddress == 0xFFFF))
                        AttemptToEnableDebugging();
            if (mNowDebugging)
            {
                DebugPrint();
                if (DebugCodeFile.BaseStream.Length > default_set.Default.DebugCodeMaxFileSize)
                    OpenNewDebugCodeFile();
            }
            //if (mShowDebugOnConsole)
            //{
            //    if (lSender.mCurrentInstruction.Name == "INT")
            //        mCurrINTNumArray.Insert(0, lSender.mCurrentInstruction.Op1Value.OpByte.ToString("x").ToUpper().PadLeft(2, '0') + " (" + lSender.regs.AX.ToString("x").ToUpper().PadLeft(4, '0') + ")");
            //    else if ((lSender.mCurrentInstruction.Name == "IRET" || lSender.mCurrentInstruction.Name == "RETF") && mCurrINTNumArray.Count > 0)
            //        mCurrINTNumArray.RemoveAt(0);
            //    DebugPrint(lSender);
            //}
        }
        static void HandleInstructDoneEvent(object sender, Processor_80x86.CustomEventArgs e)
        {
            Processor_80x86 proc = (Processor_80x86)sender;
            sInstruction sIns = new sInstruction();
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
            bool PFException = false;
            if (proc.mCurrentInstruction.Operand1IsRef)
            {
                lOutput.AppendFormat("\ndest = {0}", proc.mCurrentInstruction.Op1Add.ToString("x").PadLeft(8, '0').ToUpper());
                try { lOutput.AppendFormat(" >{0}<", proc.mem.PagedMemoryAddress(proc, ref sIns, proc.mCurrentInstruction.Op1Add, false).ToString("x").PadLeft(8, '0').ToUpper()); }
                catch
                {
                    //Dunno why this code is setting Exception number/error code ... removing it
                    //proc.ExceptionNumber = 0x00;
                    //proc.ExceptionErrorCode = 0x0;
                }
                lOutput.AppendFormat(" ({0})", proc.mCurrentInstruction.Op1Value.OpQWord.ToString("x").PadLeft(8, '0').ToUpper());
                lCount++;
                lPrintedSomething = true;
            }
            if (proc.mCurrentInstruction.Operand2IsRef && proc.mCurrentInstruction.DecodedInstruction.OpCode != 0x8d)
            {
                lOutput.AppendFormat("\t src = {0}", proc.mCurrentInstruction.Op2Add.ToString("x").PadLeft(8, '0').ToUpper());
                try { lOutput.AppendFormat(" >{0}<", proc.mem.PagedMemoryAddress(proc, ref sIns, proc.mCurrentInstruction.Op2Add, false).ToString("x").PadLeft(8, '0').ToUpper()); }
                catch
                {
                }
                lOutput.AppendFormat(" ({0})", proc.mCurrentInstruction.Op2Value.OpQWord.ToString("x").PadLeft(8, '0').ToUpper());
                lCount++;
                lPrintedSomething = true;
            }
            if (proc.mCurrentInstruction.DecodedInstruction.OpCode == 0xCD && (proc.regs.CR0 & 0x80000000) == 0x80000000)
            {
                lOutput.AppendFormat("\n\rEAX= " + proc.mTLB.ShallowTranslate(proc, ref sIns, proc.regs.EAX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("EBX= " + proc.mTLB.ShallowTranslate(proc, ref sIns, proc.regs.EBX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("ECX= " + proc.mTLB.ShallowTranslate(proc, ref sIns, proc.regs.ECX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lOutput.AppendFormat("EDX= " + proc.mTLB.ShallowTranslate(proc, ref sIns, proc.regs.EDX, false, ePrivLvl.Kernel_Ring_0).ToString("X8") + "\t");
                lCount++;
                lPrintedSomething = true;
            } 
            if (lPrintedSomething)
                lOutput.AppendFormat("\n\r");
            lOutput.Append("\n\r");
            
            if (DebugCodeFile != null)
            {
                DebugCodeFile.Write(lOutput);
                if (lCount == 0)
                    DebugCodeFile.WriteLine();
                //DebugCodeFile.Flush();
            }
        }
        static void HandleDataInEvent(object sender, Ports.CustomEventArgs e)
        {
            //if (/*e.PortInfo.Portnum == 0x64 || */e.PortInfo.Portnum == 0x80)
            //    return;
            string lWrite = new string('0', 1);
            lWrite = "Data In on port " + e.PortInfo.Portnum.ToString("x").PadLeft(4, '0').ToUpper() +
                "\t Value\t" + e.PortInfo.Value.ToString("x").PadLeft(8, '0').ToUpper();
            //lWrite += "\t\tIn Call Count = " + e.PortInfo.InCount.GetDWord();
            //Debug_Ports_Filename
            DebugPortFile.WriteLine(lWrite);
            //Debug.WriteLine(lWrite);
            //mPortFile.WriteLine("\tInstruct @: " + mCurrentOp.proc.regs.CS.ToString("x").PadLeft(4, '0').ToUpper() +
            //     ":" + mCurrentOp.proc.regs.IP.ToString("x").PadLeft(4, '0').ToUpper() + "\n\r");
        }
        static void HandleDataOutEvent(object sender, Ports.CustomEventArgs e)
        {
            //if (e.PortInfo.Portnum == 0x64 || e.PortInfo.Portnum == 0x80)
            if ( (e.PortInfo.Portnum == 0x20 && e.PortInfo.Value == 0x20) )
                return;
            string lWrite = new string('0', 1);
            //lWrite += mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.IP.ToString("X8");
            lWrite += "Data Out on port " + e.PortInfo.Portnum.ToString("x").PadLeft(4, '0').ToUpper() +
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
            sInstruction sIns = new sInstruction();

            if (sender.mCurrentInstruction.Name == "INT")
            {
                mCurrINTNumArray.Insert(0, sender.mCurrentInstruction.Op1Value.OpByte.ToString("x").ToUpper().PadLeft(2, '0') + " (" + sender.regs.AX.ToString("x").ToUpper().PadLeft(4, '0') + ")");
                mCurrINTRetAddrArray.Insert(0, sender.mCurrentInstruction.DecodedInstruction.InstructionAddress + sender.mCurrentInstruction.DecodedInstruction.BytesUsed);
            }
            else if (mCurrINTNumArray.Count > 0 && sender.mCurrentInstruction.DecodedInstruction.InstructionAddress == (UInt64)(mCurrINTRetAddrArray[0]))
            {
                mCurrINTNumArray.RemoveAt(0);
                mCurrINTRetAddrArray.RemoveAt(0);
            }

            bool lKeyRead = false;
            if (mSingleStep)
                while (!lKeyRead)
                    if (Console.KeyAvailable)
                    {
                        lKey = Console.ReadKey(true);
                        if (lKey.Key == ConsoleKey.Delete)
                            mSingleStep = false;
                        lKeyRead = true;
                    }

            if (!default_set.Default.DisplayBIOSWhenDebugging)
                if (sender.regs.CS.Value == 0xf000)
                    return;
            if (!default_set.Default.DisplayVGABIOSWhenDebugging)
                if (sender.regs.CS.Value == 0xc000)
                    return;
            if (sender.ProtectedModeActive || !sender.OpSize16 || !sender.AddrSize16)
            {
                lNum = 8;
                lIP = sender.regs.EIP;
            }
            else
            {
                lNum = 4;
                lIP = sender.regs.IP;
            }
            if (sender.ProtectedModeActive)
                lProt = "Prot";
            else if (!sender.ProtectedModeActive && sender.regs.FLAGSB.VM)
                lProt = "Virt";
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
                    lOutput.AppendFormat("********** Exception Handler {0} starts **********\n\r", IRQOrExceptionNum.ToString("X2"));
                else
                    lOutput.AppendFormat("********** Servicing IRQ (INT # {0}) starts **********\n\r", IRQOrExceptionNum.ToString("X2"));
            }
            if (lNum == 8)
            {
                DWord lStackVal = 0;
                if (sender.AddrSize16)
                    lStackVal = sender.mem.GetDWord(sender, ref sIns, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.SP));
                else
                try
                {
                    lStackVal = sender.mem.GetDWord(sender, ref sIns, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.ESP));
                }
                catch { lStackVal = 0; }
                lOutput.AppendFormat("EAX={0}  EBX={1}  ECX={2}  EDX={3}  M={4}  INT={5}\n\r", sender.regs.EAX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EBX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.ECX.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EDX.ToString("x").ToUpper().PadLeft(8, '0'), lProt.Substring(0, 1), lCurrIntNum);
                lOutput.AppendFormat("EBP={1}  ESI={2}  EDI={3}  ESP={0}  [ESP]={4}  CPL={5}    ", sender.regs.ESP.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EBP.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.ESI.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.EDI.ToString("x").ToUpper().PadLeft(8, '0'), lStackVal.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CPL.ToString("X").Substring(0,1));
                if ((sender.regs.CR0 & 0x80000000) == 0x80000000 )
                {
                    bool lBTemp = false;
                    lOutput.AppendFormat("(stackaddr={0})  TLB-C={1}\n\r", sender.mTLB.ShallowTranslate(sender, ref sIns, (DWord)(sender.regs.SS.Value + sender.regs.ESP), false, ePrivLvl.Kernel_Ring_0).ToString("X8"), sender.mTLB.mCurrEntryPtr);

                }
                else
                    lOutput.Append("\n\r");
                lOutput.AppendFormat("TR={7}  LDT={0}  CR0={1}  CR2={9}  CR3={8}  CR4={2}    O16={3} A16={4} SO16={5} SA16={6}\n\r", sender.regs.LDTR.SegSel.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR0.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR4.ToString("x").ToUpper().PadLeft(8, '0'), sender.OpSize16.ToString().Substring(0, 1), sender.AddrSize16.ToString().Substring(0, 1), sender.OpSize16.ToString().Substring(0, 1), sender.AddrSize16Stack.ToString().Substring(0, 1), sender.regs.TR.SegSel.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR3.ToString("x").ToUpper().PadLeft(8, '0'), sender.regs.CR2.ToString("x").ToUpper().PadLeft(8, '0'));
                lOutput.AppendFormat("SS={2} ({7})   DS={0} ({8})   ES={1} ({9})   CS={3} ({10})   FS={4} ({11})   GS={5} ({12})  EFLAGS={6}\n\r", sender.regs.DS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.ES.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.SS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.CS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.FS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.GS.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.EFLAGS.ToString("x").ToUpper().PadLeft(8, '0'),
                    sender.regs.SS.DescriptorNum.ToString("X3"), sender.regs.DS.DescriptorNum.ToString("X3"), sender.regs.ES.DescriptorNum.ToString("X3"), sender.regs.CS.DescriptorNum.ToString("X3"), sender.regs.FS.DescriptorNum.ToString("X3"), sender.regs.GS.DescriptorNum.ToString("X3"));
            }
            else
            {
                lOutput.AppendFormat("AX={0}  BX={1}  CX={2}  DX={3}  M={4}  INT={5}\n\r", sender.regs.AX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.BX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.CX.ToString("x").ToUpper().PadLeft(lNum, '0'), sender.regs.DX.ToString("x").ToUpper().PadLeft(lNum, '0'), lProt.Substring(0, 1), lCurrIntNum);
                lOutput.AppendFormat("BP={0}  SI={1}  DI={2}  SP={3}  [SP]={4}\n\r", sender.regs.BP.ToString("X4"), sender.regs.SI.ToString("X4"), sender.regs.DI.ToString("X4"), sender.regs.SP.ToString("X4"), sender.mem.GetWord(sender, ref sIns, PhysicalMem.GetLocForSegOfs(sender, sender.regs.SS.Value, sender.regs.SP)).ToString("X4"), sender.regs.GDTR.Value.ToString("x").ToUpper().PadLeft(4, '0'), sender.regs.IDTR.Value.ToString("x").ToUpper().PadLeft(4, '0'));
                lOutput.AppendFormat("SS={0}  DS={1}  ES={2}  CS={3}  FLAGS={4}\n\r", sender.regs.SS.Value.ToString("X4"), sender.regs.DS.Value.ToString("X4"), sender.regs.ES.Value.ToString("X4"), sender.regs.CS.Value.ToString("X4"), sender.regs.FLAGS.ToString("X4"));
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
                lOutput.AppendFormat(":" + lIP.ToString("X8") + " ");
            }
            for (int cnt = 0; cnt < sender.mCurrentInstruction.DecodedInstruction.bytes.Count(); cnt++)
                bytes.Append(String.Format("{0:x}", sender.mCurrentInstruction.DecodedInstruction.bytes[cnt]).ToUpper().PadLeft(2, '0'));
            lOutput.AppendFormat(bytes.ToString().PadRight(15, ' '));
            lInstrOutput.AppendFormat(bytes.ToString().PadRight(15, ' '));
            if (sender.mCurrentInstruction.DecodedInstruction.OverrideSegment != eGeneralRegister.DS &&
                sender.mCurrentInstruction.DecodedInstruction.OverrideSegment != eGeneralRegister.NONE)
            {
                lSegment = "* " + sender.mCurrentInstruction.DecodedInstruction.OverrideSegment.ToString() + " *";
            }
            else
                lSegment = "";
            if (sender.mCurrentInstruction.DecodedInstruction.bytes[0] == 0xf2)
                if (sender.mCurrentInstruction.Name.Contains("CMPS") || sender.mCurrentInstruction.Name.Contains("SCAS"))
                    lOutput.Append("REPNE");
                else
                    lOutput.Append("REP");

            if (sender.mCurrentInstruction.DecodedInstruction.bytes[0] == 0xf3)
                if (sender.mCurrentInstruction.Name.Contains("CMPS") || sender.mCurrentInstruction.Name.Contains("SCAS"))
                    lOutput.Append("REPE");
                else
                    lOutput.Append("REP");

            lOutput.AppendFormat("\t" + ((sender.mCurrentInstruction.FPUInstruction) ? "(F) " : "") + sender.mCurrentInstruction.Name + "\t" + sender.mCurrentInstruction.Operand1SValue);


            lInstrOutput.AppendFormat("\t" + sender.mCurrentInstruction.Name + "\t" + sender.mCurrentInstruction.Operand1SValue);
            if (sender.mCurrentInstruction.Operand2SValue != null)
            {
                lOutput.AppendFormat("," + sender.mCurrentInstruction.Operand2SValue);
                lInstrOutput.AppendFormat("," + sender.mCurrentInstruction.Operand2SValue);
            }
            if (sender.mCurrentInstruction.Operand3SValue != null)
            {
                lOutput.AppendFormat("," + sender.mCurrentInstruction.Operand3SValue);
                lInstrOutput.AppendFormat("," + sender.mCurrentInstruction.Operand3SValue);
            }
            lOutput.Append("\t" + lSegment);
            lInstrOutput.Append("\t" + lSegment);

            if (mShowAssemblerCode)
            {
            }
            if (DebugCodeFile != null)
            {
                DebugCodeFile.Write(lOutput);
                //temporarily added flush to test something
                //DebugCodeFile.Flush();
            }

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
            }
        }
        public static void StopDebugging()
        {
            mSystem.mProc.ports.HandleDataInDone -= HandleDataInEvent;
            mSystem.mProc.ports.HandleDataOutDone -= HandleDataOutEvent;
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

    }
}
