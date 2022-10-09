using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using VirtualProcessor.Devices;

namespace VirtualProcessor
{
    public enum eRomTypes
    {
        BIOS,
        Video,
        Misc
    }

    public class cBreakpoint
    {
        public UInt32 CS { get; set; }
        public UInt32 EIP { get; set; }
        public bool DisableOnHit { get; set; }
        public bool RemoveOnHit { get; set; }
        public byte InterruptNum { get; set; }
        public byte FunctNum { get; set; }
        public bool Enabled { get; set; }
        public bool DOSFunct { get; set; }
        public UInt32 TaskNum { get; set; }
        public bool DebugToFile { get; set; }
        public bool DebugToStreen { get; set; }
        public cBreakpoint(UInt32 pCS, UInt32 pEIP, bool pDisableOnHit, bool pRemoveOnHit, bool ToStreen, bool ToFile)
        {
            CS = pCS;
            EIP = pEIP;
            DisableOnHit = pDisableOnHit;
            RemoveOnHit = pRemoveOnHit;
            Enabled = true;
            DebugToFile = ToFile;
            DebugToStreen = ToStreen;
        }
        public cBreakpoint(byte SoftIntNum, byte Function, bool DOSFunction, bool ToStreen, bool ToFile)
        {
            InterruptNum = SoftIntNum;
            FunctNum = Function;
            Enabled = true;
            DOSFunct = DOSFunction;
            DebugToFile = ToFile;
            DebugToStreen = ToStreen;
        }
        public cBreakpoint(UInt32 Task, bool pDisableOnHit, bool ToStreen, bool ToFile)
        {
            TaskNum = Task;
            DisableOnHit = pDisableOnHit;
            DebugToFile = ToFile;
            DebugToStreen = ToStreen;
        }
    }

    public class PCSystem
    {
        public Boolean PoweredUp = false;
        // Address line 20 control:
        //   1 = enabled: extended memory is accessible
        //   0 = disabled: A20 address line is forced low to simulate
        //       an 8088 address map
        Boolean m_enable_a20;
        public Boolean A20Status
        {
            get { return m_enable_a20; }
            set { m_enable_a20 = value; }
        }
        public Processor_80x86 mProc;
        public cDeviceBlock DeviceBlock;
        private FileStream BiosFile, VideoFile;
        internal String FloppyAFile = "", FloppyBFile= "";
        internal eFloppyType FloppyACapacity, FloppyBCapacity;
        public sDebugComponents Debuggies = new sDebugComponents();
        private DateTime mSystemStartDate = DateTime.Now;
        public StreamWriter mMasterDebugFile;
        internal string mCMOSPathFile;
        eProcTypes mProcType;
        private Boolean mDebugToSeparateFiles = false, mResettingSystem = false;
        public UInt32 mTotalMemory;
        internal HDInfo[] Drives = new HDInfo[4];
        internal int mDriveCount = 0;
		public string mFileSuffix;
        private string mDebugFilePath = "", mBiosPathFile = "", mVideoPathFile="", mMiscRomPathFile="";
        internal bool mCR0_WP_Honor_In_Sup_Mode = false;
        public void EnableCR0_WritePotect()
        {
            mCR0_WP_Honor_In_Sup_Mode = true;
        }
        public string DebugFilePath
        {
            get { return mDebugFilePath; }
            set {
                if (Directory.Exists(value))
                    mDebugFilePath = value;
                else
                    throw new Exception("Debugging path '+ value + ' does not exist.  Ensure that the debug path exists or change the DebugFilePath property and try again");
            }
        }
        public int mAddressBreakpointCount = 0;
        internal UInt32[] mInterruptBreakpoint;
        internal int mInterruptBreakpointCount = 0;
        internal uint mTaskNameOffset = 0;
        public uint TaskNameOffset
        {
            get { return mTaskNameOffset; }
            set { mTaskNameOffset = value; }
        }
        public List<cBreakpoint> BreakpointInfo = new List<cBreakpoint>();
        public bool mModeBreakpointSet = false;
        internal bool mModeBreakpoint = false, mTaskSwitchBreakpoint = false, mCPL0SwitchBreakpoint = false,
            mCPL3SwitchBreakpoint = false, mPagingExceptionBreakpoint = false, mSoftIntBreakpoint = false, mSoftIntBreakpointRemoveOnHit = false, mSwitchToTaskBreakpoint = false;
        public bool ModeBreakpoint
        {
            set { if (mModeBreakpointSet) mModeBreakpoint = value; else mModeBreakpoint = false; }
            get { return mModeBreakpoint; }
        }
        public bool mTaskSwitchBreakpointSet = false;
        public bool TaskSwitchBreakpoint
        {
            get { return mTaskSwitchBreakpoint; }
            set { if (mTaskSwitchBreakpointSet) mTaskSwitchBreakpoint = value; else mTaskSwitchBreakpoint = false; }
        }
        public bool mCPL0SwitchBreakpointSet = false;
        public bool CPL0SwitchBreakpoint
        {
            get { return mCPL0SwitchBreakpoint; }
            set { if (mCPL0SwitchBreakpointSet) mCPL0SwitchBreakpoint = value; else mCPL0SwitchBreakpoint = false; }
        }
        public bool mCPL3SwitchBreakpointSet = false;
        public bool CPL3SwitchBreakpoint
        {
            get { return mCPL3SwitchBreakpoint; }
            set { if (mCPL3SwitchBreakpointSet) mCPL3SwitchBreakpoint = value; else mCPL3SwitchBreakpoint = false; }
        }
        public bool mPagingExceptionBreakpointSet = false;
        public bool PagingExceptionBreakpoint
        {
            get { return mPagingExceptionBreakpoint; }
            set { if (mPagingExceptionBreakpointSet) mPagingExceptionBreakpoint = value; else mPagingExceptionBreakpoint = false; }
        }

        public PCSystem(UInt32 TotalMemory, eProcTypes ProcessorType, string CMOSPathFile)
        {
            mCMOSPathFile = CMOSPathFile;
            mTotalMemory = TotalMemory;
            mProcType = ProcessorType;
            mProc = new Processor_80x86(this, TotalMemory, ProcessorType);
        }
        public void ColdBoot()
        {
            do
            {
                mFileSuffix = DateTime.Now.ToString("yyyyMMddhhmmss");
                if (mResettingSystem)
                {
                    mProc = null;
                    mProc = new Processor_80x86(this, mTotalMemory, mProcType);
                    mResettingSystem = false;
                }
                if (!Directory.Exists(mDebugFilePath))
                    throw new Exception("Debugging path '+ value + ' does not exist.  Ensure that the debug path exists or change the DebugFilePath property and try again");
                A20Status = false;
                if (BiosFile == null || VideoFile == null)
                    throw new Exception("Call the LoadRom method to load BIOS and Video ROMs (at a minimum) before booting");
                LoadRom(this.mBiosPathFile, eRomTypes.BIOS, 0);
                LoadRom(this.mVideoPathFile, eRomTypes.Video, 0);
                if (mMiscRomPathFile != "")
                    LoadRom(this.mMiscRomPathFile, eRomTypes.Misc, 0xD0000);

                //if (System.Threading.Thread.CurrentThread.Name =="")
                //    System.Threading.Thread.CurrentThread.Name = "Main";
                string lNow = DateTime.Now.ToString("yyyymmdd_hhMMss");
                //mMasterDebugFile = new StreamWriter(Global.DEBUGGING_PATH + Global.DEBUG_MASTER_FILENAME + lNow + ".txt", false, Encoding.Default, 128);
                if (mMasterDebugFile == null)
                {
                    mMasterDebugFile = new StreamWriter(mDebugFilePath + Global.DEBUG_MASTER_FILENAME + mFileSuffix + ".txt", true, Encoding.Default, 1024 * 1024 * 10);
                    mMasterDebugFile.WriteLine("\r\n\r\n********************** SYSTEM STARTED AT " + DateTime.Now.ToString() + "**********************");
                    mMasterDebugFile.Flush();
                }
                if (DeviceBlock == null)
                {
                    DeviceBlock = new cDeviceBlock(this);
                    do { System.Threading.Thread.Sleep(0); } while (!DeviceBlock.mPoweredUp);
                }
                PoweredUp = true;
#if DOTRIES
            try
            {
                mProc.StartExecution();
            }
            catch (Exception e)
            {
                //throw e;
                mMasterDebugFile.WriteLine("PCSystem: Exception caught, message is: " + e.Message);
                mMasterDebugFile.WriteLine("Aborting emulation.");

            }   
            finally
            {
                if (mMasterDebugFile != null)
                {
                    mMasterDebugFile.Close();
                    mMasterDebugFile = null;
                }
                //ShutDown();
                PoweredUp = false;
            }
#else
                mProc.StartExecution();
#endif
                DeviceBlock.Shutdown();
                do { System.Threading.Thread.Sleep(0); } while (DeviceBlock.mPoweredUp);
                DeviceBlock = null;
                this.PoweredUp = false;
            }
            while (mProc.mProcessorStatus != eProcessorStatus.ShuttingDown);
            PoweredUp = false;
            mProc.mProcessorStatus = eProcessorStatus.PoweredOff;
        }
        public void ResetSystem()
        {
            mProc.mProcessorStatus = eProcessorStatus.Resetting;
            mResettingSystem = true;
            mProc.PowerOff = true;
        }

        public void ShutDown()
        {
           mProc.mProcessorStatus = eProcessorStatus.ShuttingDown;
            mProc.PowerOff = true;
        }


        /// <summary>
        /// Define a floppy drive to the system
        /// </summary>
        /// <param name="PathAndFilename"></param>
        public void LoadDrive(String PathAndFilename, string Capacity, int FloppDriveNum)
        {
            eFloppyType lTemp;
            if (!new FileInfo(PathAndFilename).Exists)
                throw new Exception("Cannot find floppy file: " + PathAndFilename);
            
            switch (Capacity)
            {
                case "720K":
                    lTemp = eFloppyType.FLOPPY_720K;
                    break;
                case "1_44M":
                    lTemp = eFloppyType.FLOPPY_1_44M;
                    break;
                case "2_88M":
                    lTemp = eFloppyType.FLOPPY_2_88M;
                    break;
                case "1_2M":
                    lTemp = eFloppyType.FLOPPY_1_2M;
                    break;
                default:
                    lTemp = eFloppyType.FLOPPY_360K;
                    break;
            }

            if (FloppDriveNum == 1)
            {
                FloppyAFile = PathAndFilename;
                FloppyACapacity = lTemp;
                if (DeviceBlock != null && DeviceBlock.mFloppy != null)
                    DeviceBlock.mFloppy.LoadDrive(1);
            }
            else if (FloppDriveNum == 2)
            {
                FloppyBFile = PathAndFilename;
                FloppyBCapacity = lTemp;
                if (DeviceBlock != null && DeviceBlock.mFloppy != null)
                    DeviceBlock.mFloppy.LoadDrive(1);
            }
            else
                throw new Exception("System only supports 2 floppies!");

        }

        /// <summary>
        /// Define a hard drive or CDROM to the system
        /// </summary>
        /// <param name="PathAndFilename"></param>
        /// <param name="Cylinders"></param>
        /// <param name="Heads"></param>
        /// <param name="SectorsPerTrack"></param>
        /// <param name="DeviceType"></param>
        public void LoadDrive(String PathAndFilename, ulong Cylinders, ulong Heads, ulong SectorsPerTrack, device_type_t DeviceType)
        {
            if (!new FileInfo(PathAndFilename).Exists)
                throw new Exception("Cannot find drive file: " + PathAndFilename);
            Drives[mDriveCount].PathAndFile = PathAndFilename;
            Drives[mDriveCount].Cylinders = Cylinders;
            Drives[mDriveCount].Heads = Heads;
            Drives[mDriveCount].SectorsPerTrack = SectorsPerTrack;
            Drives[mDriveCount].DeviceType = DeviceType;
            mDriveCount++;
        }

        public void LoadRom(string Path, eRomTypes RomType, int MiscRomLoadAddr)
        {
            if (!PoweredUp)
                switch (RomType)
                {
                    case eRomTypes.BIOS:
                        mBiosPathFile = Path;
                        BiosFile = new FileStream(Path, FileMode.Open, FileAccess.Read);
                        if (BiosFile.Length <= 0x10000)
                            BiosFile.Read(mProc.mem.mMemBytes, 0xf0000, (int)BiosFile.Length);
                        else
                            BiosFile.Read(mProc.mem.mMemBytes, 0xE0000, (int)BiosFile.Length);
                        BiosFile.Close();
                        break;
                    case eRomTypes.Video:
                        mVideoPathFile = Path;
                        VideoFile = new FileStream(Path, FileMode.Open, FileAccess.Read);
                        VideoFile.Read(mProc.mem.mMemBytes, 0xC0000, (int)VideoFile.Length);
                        VideoFile.Close();
                        break;
                    case eRomTypes.Misc:
                        mMiscRomPathFile = Path;
                        FileStream lTempFile = new FileStream(Path, FileMode.Open, FileAccess.Read);
                        lTempFile.Read(mProc.mem.mMemBytes, MiscRomLoadAddr, (int)lTempFile.Length);
                        lTempFile.Close();
                        break;
                }
            else
                throw new Exception("Load Roms BEFORE powering up!");
        }

        public void HandleException(Object Sender, Exception e)
        {
            cDevice lTemp = null;
            if (Sender.GetType().BaseType == typeof(cDevice))
                lTemp = (cDevice)Sender;
            //for now just throw the exception, but later we will do something else like print to the screen
            if (lTemp != null)
                PrintDebugMsg(eDebuggieNames.System, lTemp.DeviceName + " - EXCEPTION (" + System.Threading.Thread.CurrentThread.Name + "): " + e.Message);
            else
                PrintDebugMsg(eDebuggieNames.System, "EXCEPTION: (" + System.Threading.Thread.CurrentThread.Name + "): " + e.Message);
            ShutDown();
            throw e;
        }

        public void PrintDebugMsg(eDebuggieNames Source, String Message)
        {
            string CurrentTime = System.DateTime.Now.ToString();
            string lSource = new string('c', 1);
            StreamWriter lDebugFile;
            
            string lMessage = DateTime.Now.ToString("hh:mm:ss.fff tt");
            string lMessage2;
            string lTask = GlobalRoutines.GetLinuxCurrentTaskName(this);

            lMessage += " (" + System.Threading.Thread.CurrentThread.Name;
            if (lTask.Length != 0)
                lMessage += " - " + lTask + "\t";
            else
                lMessage += "-Unknown\t";
            lMessage += "): ";    
            lMessage += Source.ToString() + ": " + Message;
            if (mProc.mCurrInstructOpMode == ProcessorMode.Protected)
                lMessage2 = "\t\t\t\t CPU currently at: " + mProc.regs.CS.DescriptorNum.ToString("X4") + ":" + mProc.regs.EIP.ToString("X8") + "(" + mProc.mCurrentInstruction.DecodedInstruction.InstructionAddress.ToString("X8") + ")";
            else
                lMessage2 = "\t\t\t\t CPU currently at: " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8");
            if (mDebugToSeparateFiles)
            {
                lDebugFile = new StreamWriter(mDebugFilePath + "dbg_" + lSource + ".txt", true);
                lock (lDebugFile)
                {
                    lDebugFile.Write(lMessage);
                    lDebugFile.WriteLine(lMessage2);
                    lDebugFile.Close();
                }
            }
            else
            {
                if (mMasterDebugFile != null)
                {
                    mMasterDebugFile.Write(lMessage);
                    mMasterDebugFile.WriteLine(lMessage2);
                    mMasterDebugFile.Flush();
                }
            }
        }

        public void AddAddressBreakpoint(UInt32 CSExpanded, UInt32 EIP, bool DisableOnHit, bool RemoveOnHit, bool ToScreen, bool ToFile)
        {
            BreakpointInfo.Add(new cBreakpoint(CSExpanded, EIP, DisableOnHit, RemoveOnHit, ToScreen, ToFile));
        }
        /// <summary>
        /// Add a software interrupt (instruction INT) breakpoint
        /// </summary>
        /// <param name="InterruptNum"></param>
        /// <param name="Function"></param>
        /// <param name="DOSInterrupt - True for DOS interrupt (function in AH) or False for Linux interrupt (function in EAX)"></param>
        public void AddSoftIntBreakpoint(byte InterruptNum, byte Function, bool DOSInterrupt, bool ToScreen, bool ToFile)
        {
            BreakpointInfo.Add(new cBreakpoint(InterruptNum, Function, DOSInterrupt, ToScreen, ToFile));
        }

        public void AddSwitchToTaskBreakpoint(UInt32 Task, bool DisableOnHit, bool ToScreen, bool ToFile)
        {
            BreakpointInfo.Add(new cBreakpoint(Task, DisableOnHit, ToScreen, ToFile));
        }
        public void RemoveAddressBreakpoint(cBreakpoint b)
        {
            cBreakpoint p = BreakpointInfo.Where(u => u.CS == b.CS && u.EIP == b.EIP).FirstOrDefault();
            BreakpointInfo.Remove(p);
        }

        public bool AddressBreakpointExists(cBreakpoint b)
        {
            cBreakpoint p = BreakpointInfo.Where(u => u.CS == b.CS && u.EIP == b.EIP).FirstOrDefault();
            if (p == null)
                return false;
            else
                return true;
        }

        public long AddressBreakpointCount()
        {
            return  BreakpointInfo.Where(u => u.InterruptNum == 0 && u.TaskNum == 0 && u.Enabled).LongCount();
        }

        public long InterruptBreakpointCount()
        {
            return BreakpointInfo.Where(u => u.InterruptNum != 0 && u.Enabled).LongCount();
        }

        public long BreakpointOnTaskCount()
        {
            return BreakpointInfo.Where(u => u.TaskNum != 0 && u.Enabled).LongCount();
        }

        public bool BreakOnSwitchToTaskNum(UInt32 Task)
        {
            return BreakpointInfo.Where(u => u.TaskNum == Task && u.Enabled).LongCount() > 0;
        }

        public struct HDInfo
        {
            public String PathAndFile;
            public ulong Cylinders, Heads, SectorsPerTrack;
            public device_type_t DeviceType;
        }
    }
}
