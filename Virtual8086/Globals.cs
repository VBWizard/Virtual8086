using System;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using System.IO;

namespace VirtualProcessor
{

    public struct sDebugComponents
    {
        public bool DebugKbd;
        public bool DebugCPU;
        public bool DebugPIC;
        public bool DebugFDC;
        public bool DebugCMOS;
        public bool DebugDMA;
        public bool DebugHDC;
        public bool DebugMemPaging;
        public bool DebugSerial;
        public bool DebugPIT;
        public bool DebugExceptions;
        public bool DebugInterrupts;
        public bool DebugVideo;

        public sDebugComponents(bool Kbd, bool CPU, bool PIC, bool FDC, bool CMOS, bool DMA, bool HDC, bool MemPaging, bool Serial, bool Pit, bool Exceptions, bool Interrupts, bool Video)
        {
            DebugKbd = Kbd;
            DebugCPU = CPU;
            DebugPIC = PIC;
            DebugFDC = FDC;
            DebugCMOS = CMOS;
            DebugDMA = DMA;
            DebugHDC = HDC;
            DebugMemPaging = MemPaging;
            DebugSerial = Serial;
            DebugPIT = Pit;
            DebugExceptions = Exceptions;
            DebugInterrupts = Interrupts;
            DebugVideo = Video;
        }
    }

    public enum eDebuggieNames
    {
        CPU,
        Keyboard,
        PIC,
        Floppy,
        CMOS,
        DMA,
        System,
        HardDrive,
        MemoryPaging,
        Serial,
        PIT,
        Exceptions,
        Interrupts,
        Video
    }
    public enum eFloppyType
    {
        FLOPPY_NONE,
        FLOPPY_160K,
        FLOPPY_180K,
        FLOPPY_320K,
        FLOPPY_360K,
        FLOPPY_720K,
        FLOPPY_1_2M,
        FLOPPY_1_44M,
        FLOPPY_2_88M,
    }
    public enum PagingErrorType : byte
    {
        ERROR_NOT_PRESENT = 0x00,
        ERROR_PROTECTION = 0x01,
        ERROR_RESERVED = 0x08,
        ERROR_CODE_ACCESS = 0x10
    }
    public class Global
    {
        public const string DEBUG_MASTER_FILENAME = "dbg_master";
        public const byte BX_KBD_ELEMENTS = 16;
        public const string DEBUG_CATEGORY_DEVICE = "Devices";
        public const QWord BX_KEY_RELEASED = 0x80000000;
        public const int CPU_NO_EXCEPTION = 0x0;
    }

    public struct sOptions
    {
        public Word keyboard_serial_delay;

        public sOptions(Word KbdSerialDelay)
        {
            keyboard_serial_delay = KbdSerialDelay;
        }



    }

    public struct GlobalRoutines
    {
        static public void UpdateCMOS(string CMOSFile, int Index, UInt32 NewValue)
        {
            UpdateCMOS(CMOSFile, Index + 1, (byte)(NewValue >> 8));
            UpdateCMOS(CMOSFile, Index, (byte)(NewValue & 0xFF));
        }
        static public void UpdateCMOS(string CMOSFile, int Index, byte NewValue)
        {
            byte[] cmos = new byte[128];

            StreamReader sr = new StreamReader(CMOSFile, false);
            BinaryReader br = new BinaryReader(sr.BaseStream);

            br.BaseStream.Read(cmos, 0, 128);
            sr.Close();
            cmos[Index] = NewValue;
            checksum_cmos(cmos);
            StreamWriter sw = new StreamWriter(CMOSFile, false);
            BinaryWriter bw = new BinaryWriter(sw.BaseStream);
            bw.Write(cmos);
//            Console.WriteLine("Wrote CMOS to " + CMOSFile);
            sw.Close();
        }

        static void checksum_cmos(byte[] cmos)
        {
            UInt16 sum = 0;
            for (int i = 0x10; i <= 0x2d; i++)
                sum += cmos[i];
            byte Hi = 0, Lo = 0;
            Hi = (byte)((sum >> 8) & 0xff);
            Lo = (byte)((sum & 0xff));
            cmos[0x2e] = Hi;
            cmos[0x2f] = Lo;
            //Console.WriteLine("Checksum: " + sum.ToString("x").PadLeft(4, '0'));
            //Console.WriteLine("Hi Byte: " + Hi.ToString("x").PadLeft(2, '0'));
            //Console.WriteLine("Lo Byte: " + Lo.ToString("x").PadLeft(2, '0'));
        }

        //DLX = 38
        //Redhat = 32
        public static string GetLinuxCurrentTaskName(PCSystem mSystem)
        {
            bool lNameFound = false;

            if (mSystem.OSType != eOpSysType.Linux)
                return "None";
            string lJobName = "";
            if (mSystem.mProc.regs.TR.SegSel == 0)
                return "None";

            //UInt32 lstartAddr = mSystem.mProc.mTLB.ShallowTranslate(mSystem.mProc, ref sIns, mSystem.mProc.regs.TR.Base - mSystem.TaskNameOffset, false, ePrivLvl.Kernel_Ring_0);
            //if (lstartAddr == 0xf1f1f1f1 || sIns.ExceptionThrown)
            //    return "Paging Error";
            lNameFound = false;
            DWord lStartAddr = mSystem.mProc.mGDTCache[(int)mSystem.mProc.regs.TR.SegSel >> 3].Base;
            try
            {
                for (UInt32 cnt = lStartAddr - 100; cnt < lStartAddr; cnt++)
                {
                    byte lTemp = mSystem.mProc.mem.pMemory(mSystem.mProc, cnt & 0xFFFFFFF);
                    if (lTemp > 31 && lTemp < 127)
                    {
                        lJobName += (char)lTemp;
                        lNameFound = true;
                    }
                    else if (lNameFound)
                        break;
                }
            }
            catch { }
            return lJobName;

        }

        public static string GetLinuxTSSTaskName(PCSystem mSystem, DWord TSSBase)
        {
            bool lNameFound = false;
            if (mSystem.OSType != eOpSysType.Linux)
                return "None";
            string lJobName = "";
            if (TSSBase == 0)
                return "None";

            //UInt32 lstartAddr = mSystem.mProc.mTLB.ShallowTranslate(mSystem.mProc, ref sIns, TSSBase - 100, false, ePrivLvl.Kernel_Ring_0);
            //if (lstartAddr == 0xf1f1f1f1 || sIns.ExceptionThrown)
            //    return "Paging Error";
            lNameFound = false;
            try
            {
                for (UInt32 cnt = TSSBase - 100; cnt < TSSBase; cnt++)
                {
                    byte lTemp = mSystem.mProc.mem.pMemory(mSystem.mProc, cnt & 0xFFFFFFF);
                    if (lTemp > 31 && lTemp < 127)
                    {
                        lJobName += (char)lTemp;
                        lNameFound = true;
                    }
                    else if (lNameFound)
                        break;
                }
            }
            catch { }
            return lJobName;
        }
    }

}