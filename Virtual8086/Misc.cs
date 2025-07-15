using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace VirtualProcessor
{

    public enum ProcessorMode
    {
        Real,
        Virtual8086,
        Protected
    }

    public enum eProcessorStatus : int
    {
        Decoding,
        Setup,
        Execution,
        PostExecution,
        ShuttingDown,
        Resetting,
        PoweredOff,
        NotDecoding
    }

    [FlagsAttribute]
    #region Flags enum
    public enum eFLAGS : int
    {
        NONE = 0,
        CF = 1,
        PF = 2,
        AF = 4,
        ZF = 6,
        SF = 7,
        TF = 8,
        IF = 9,
        DF = 10,
        OF = 11,
        IOPL = 12,
        NT = 14,
        RF = 16,
        VM = 17,
        AC = 18, 
        VIF = 19,
        VIP = 20,
        ID = 21
    }
    #endregion

    #region Operand enum
    public enum Operand
    {
        Reg8,
        Reg16,
        Mem,
        Mem16,
        Mem32,
        Imm8,
        Imm16,
        Accum
    }
    #endregion

    public struct sFlags
    {
        Processor_80x86 mProc;
        internal bool mPFDirty, mCFDirty, mIFDirty, mAFDirty, mZFDirty, mSFDirty, mTFDirty, mDFDirty, mOFDirty, mNTDirty, mRFDirty, mVMDirty, mACDirty, mVIFDirty, mVIPDirty, mVIDDirty;

        public sFlags(Processor_80x86 Proc)
        {
            mProc = Proc;
            aCF = aIF = aPF = aAF = aZF = aSF = aTF = aDF = aOF = aNT = aRF = aVM = aAC = aVIF = aVIP = aVID = false;
            mPFDirty = mCFDirty = mIFDirty = mAFDirty = mZFDirty = mSFDirty = mTFDirty = mDFDirty = mOFDirty = mNTDirty = mRFDirty = mVMDirty = mACDirty = mVIFDirty = mVIPDirty = mVIDDirty = false;
            aIOPL = 0;
        }

        public bool FlagsDirty
        {
            set { mPFDirty = mCFDirty = mIFDirty = mAFDirty = mZFDirty = mSFDirty = mTFDirty = mDFDirty = mOFDirty = mNTDirty = mRFDirty = mVMDirty = mACDirty = mVIFDirty = mVIPDirty = mVIDDirty = value; }
        }

        public bool CF { get { if (mCFDirty) { aCF = ((mProc.regs.EFLAGS & 0x01) == 0x01); mCFDirty = false; }  return aCF; } }
        public bool PF { get { if (mPFDirty) { aPF = ((mProc.regs.EFLAGS & 0x04) == 0x04); mPFDirty = false; } return aPF; } }
        public bool AF { get { if (mAFDirty) {aAF = ((mProc.regs.EFLAGS & 0x10) == 0x10); mAFDirty = false;} return aAF; } }
        public bool ZF { get { if (mZFDirty) {aZF = ((mProc.regs.EFLAGS & 0x40) == 0x40); mZFDirty = false;} return aZF; } }
        public bool SF { get { if (mSFDirty) {aSF = ((mProc.regs.EFLAGS & 0x80) == 0x80); mSFDirty = false;} return aSF; } }
        public bool TF { get { if (mTFDirty) {aTF = ((mProc.regs.EFLAGS & 0x100) == 0x100); mTFDirty = false;} return aTF; } }
        public bool IF { get { if (mIFDirty) {aIF = ((mProc.regs.EFLAGS & 0x200) == 0x200); mIFDirty = false;} return aIF; } }
        public bool DF { get { if (mDFDirty) {aDF = ((mProc.regs.EFLAGS & 0x400) == 0x400); mDFDirty = false;} return aDF; } }
        public bool OF { get { if (mOFDirty) { aOF = ((mProc.regs.EFLAGS & 0x800) == 0x800); mOFDirty = false; } return aOF; } }
        public byte IOPL
        {
            get
            {
                aIOPL = (byte)((mProc.regs.EFLAGS & 0x3000) >> 13);
                return aIOPL;
            }
        }
        public bool NT { get { if (mNTDirty) {aNT = ((mProc.regs.EFLAGS & 0x4000) == 0x4000); mNTDirty = false;} return aNT; } }

        public bool RF { get { if (mRFDirty) {aRF = ((mProc.regs.EFLAGS & 0x10000) == 0x10000); mNTDirty = false; } return aRF; } }
        public bool VM { get { if (mVMDirty) {aVM = System.Convert.ToBoolean(Misc.getBit(mProc.regs.EFLAGS, (int)eFLAGS.VM)); mVMDirty = false;} return aVM; } }

        public bool AC { get { if (mACDirty) {aAC = System.Convert.ToBoolean(Misc.getBit(mProc.regs.EFLAGS, (int)eFLAGS.AC)); mACDirty = false;} return aAC; } }
        public bool VIF { get { if (mVIFDirty) {aVIF = System.Convert.ToBoolean(Misc.getBit(mProc.regs.EFLAGS, (int)eFLAGS.VIF)); mVIFDirty = false;} return aVIF; } }
        public bool VIP { get { if (mVIPDirty) {aVIP = System.Convert.ToBoolean(Misc.getBit(mProc.regs.EFLAGS, (int)eFLAGS.VIP)); mVIPDirty = false;} return aVIP; } }
        public bool VID { get { if (mVIDDirty) {aVID = System.Convert.ToBoolean(Misc.getBit(mProc.regs.EFLAGS, (int)eFLAGS.ID)); mVIDDirty = false;} return aVID; } }

        private bool aCF, aPF, aAF, aZF, aSF, aTF, aIF, aDF, aOF, aNT, 
            //EFlags Registers
            aRF, aVM, aAC, aVIF, aVIP, aVID;
        private byte aIOPL;

        public void SetValues(UInt16 InVal)
        {
            aCF = ((InVal & 0x01) == 0x01);
            aPF = ((InVal & 0x04) == 0x04);
            aAF = ((InVal & 0x10) == 0x10);
            aZF = ((InVal & 0x40) == 0x40);
            aSF = ((InVal & 0x80) == 0x80);
            aTF = ((InVal & 0x100) == 0x100);
            aIF = ((InVal & 0x200) == 0x200);
            aDF = ((InVal & 0x400) == 0x400);
            aOF = ((InVal & 0x800) == 0x800);
            aIOPL = (byte)( (Misc.getBit(InVal, (int)eFLAGS.IOPL) << 1 ) + Misc.getBit(InVal, (int)eFLAGS.IOPL));
            aNT = ((InVal & 0x4000) == 0x4000);
        }
        public void SetValues(UInt32 InVal)
        {
            SetValues((UInt16)(InVal & 0xFFFF));
            aRF = ((InVal & 0x10000) == 0x10000);
            aVM = ((InVal & 0x20000) == 0x20000);
            aAC = ((InVal & 0x40000) == 0x40000);
            aAC = System.Convert.ToBoolean(Misc.getBit(InVal, (int)eFLAGS.AC));
            aVID = System.Convert.ToBoolean(Misc.getBit(InVal, (int)eFLAGS.ID));
            aVIF = System.Convert.ToBoolean(Misc.getBit(InVal, (int)eFLAGS.VIF));
            aVIP = System.Convert.ToBoolean(Misc.getBit(InVal, (int)eFLAGS.VIP));
            FlagsDirty = false;
        }
    }
    /// <summary>
    /// Descriptor Register type - 
    /// GDTR, IDTR = Table
    /// LDTR, TR = Segment
    /// </summary>

    public enum eGeneralRegister : uint
    {
        //Warning: Changing these values will have a negative effect since they are used in the code
        NONE = 0,
        AL = 1,    
        CL = 2,    
        DL = 3,    
        BL = 4,    
        AH = 5,    
        CH = 6,    
        DH = 7,    
        BH = 8,    
        AX = 0X09, 
        CX = 0X0a, 
        DX = 0X0b, 
        BX = 0X0c, 
        SP = 0X0d, 
        BP = 0X0e, 
        SI = 0X0f, 
        DI = 0X10,
        ST0 = 0x0011,
        ST1 = 0x0012,
        ST2 = 0x0013,
        ST3 = 0x0014,
        ST4 = 0x0015,
        ST5 = 0x0016,
        ST6 = 0x0017,
        ST7 = 0x0018,

        EAX = 0x0019,
        ECX = 0x001a,
        EDX = 0x001b,
        EBX = 0x001c,
        ESP = 0x001d,
        EBP = 0x001e,
        ESI = 0x001f,
        EDI = 0x0020,
        CS = 0x002e,
        DS = 0x003e,
        ES = 0x0026,
        FS = 0x0064,
        GS = 0x0065,
        SS = 0x0036,
        CR0 = 0x2000,
        CR2 = 0x2002,
        CR3 = 0x3003,
        CR4 = 0x3004,
        DR0 = 0x3100,
        DR1 = 0x3101,
        DR2 = 0x3102,
        DR3 = 0x3103,
        DR6 = 0x3106,
        DR7 = 0x3107,
        FLAGS = 0xF000,
        DISP32 = 0xFFFF,
    }

    public enum OperationType
    {
        Addition,
        Subtraction
    }

    public enum SegSelTableIndicator : uint
    {
        GDT = 0,
        LDT = 1
    }

    public enum eGDTSegType : int
    {
        Data_RO = 0,
        Data_RO_Accessed = 1,
        Data_RW = 2,
        Data_RW_Accessed = 3,
        Data_RO_Expand_Down = 4,
        Data_RO_Expand_Down_Accessed = 5,
        Data_RW_Expand_DOwn = 6,
        Data_RW_Expand_Down_Accessed = 7,
        Code_Exec_Only = 8,
        Code_Exec_Only_Accessed = 9,
        Code_Read = 10,
        Code_Read_Accessed = 11,
        Code_Exec_Only_Conforming = 12,
        Code_Exec_Only_Conforming_Accessed = 13,
        Code_Exec_RO_Conforming = 14,
        Code_Exec_RO_Conforming_Accessed = 15
    }

    public enum eGDTDescType : int
    {
        System = 0,
        Code_or_Data = 1
    }

    public enum eSystemOrGateDescType : int
    {
        Reserved = 0,
        TSS_16_Av = 1,
        LDT = 2,
        TSS_16_Bu = 3,
        Call_Gate_16 = 4,
        Task_Gate = 5,
        Interrupt_Gate_16 = 6,
        Trap_Gate_16 = 7,
        Reserved2 = 8,
        TSS_32_Av = 9,
        Reserved3 = 10,
        TSS_32_Bu = 11,
        Call_Gate_32 = 12,
        Reserved4 = 13,
        Interrupt_Gate_32 = 14,
        Trap_Gate_32 = 15
    }

    public enum ePrivLvl : int
    {
        Kernel_Ring_0 = 0,
        OS_Ring_1 = 1,
        OS_Ring_2 = 2,
        App_Level_3 = 3
    }

    public enum eSwitchSource : int
    {
        SWITCH_FROM_CALL,
        SWITCH_FROM_INT,
        SWITCH_FROM_JMP,
        SWITCH_FROM_IRET
    }

    public struct sGDTAccess
    {
        /// <summary>
        /// Bit 7
        /// </summary>
        public bool Present;
        /// <summary>
        /// Bits 6 & 5
        /// </summary>
        public ePrivLvl PrivLvl { get { return m_PrivLvl;} set { m_PrivLvl = value; } }
        internal ePrivLvl m_PrivLvl;
        /// <summary>
        /// Bit 4
        /// </summary>
        public eGDTDescType DescType;
        /// <summary>
        /// Bits 3-0
        /// </summary>
        public eGDTSegType SegType { get { return m_SegType; } }
        internal eGDTSegType m_SegType;
        internal eSystemOrGateDescType m_SystemDescType;
        public eSystemOrGateDescType SystemDescType { get { return m_SystemDescType; } set { m_SystemDescType = value; } }
    }

    public struct sGDTGranularity
    {
        public byte Limit_Granularity;
        public bool OpSize32;
    }

    public struct sGDTEntry
    {
        internal UInt16 limit_low;           // The lower 16 bits of the limit.
        internal byte limit_hi;
        internal UInt16 base_low;            // The lower 16 bits of the base.
        internal byte base_middle;         // The next 8 bits of the base.
        public sGDTAccess access;              // Access flags, determine what ring this segment can be used in.
        public sGDTGranularity granularity;
        internal byte base_high;           // The last 8 bits of the base.
        internal QWord mValue;
        internal UInt32 m_Base, m_TableEntryNumber;
        internal UInt32 m_limit;

        public QWord Value { get { return mValue; } }
        public UInt32 Base { get { return m_Base; } }
        public bool Present { get { return access.Present; } }
        public UInt32 Number { get { return m_TableEntryNumber; } }
        public UInt32 Limit { get { return m_limit; } set { m_limit = value; }  }
        public string DescType { get { return access.DescType.ToString(); } }
        public string SegType { get { return access.m_SegType.ToString(); } }
        public string PrivLvl { get { return access.m_PrivLvl.ToString(); } }
        public bool Opsize32 { get { return granularity.OpSize32; } }
        public string SystemDescType { get { return access.SystemDescType.ToString(); } }
        
        public void Parse(UInt64 Entry)
        {
            if (Entry > 0)
            {
                mValue = Entry;
                limit_low = (Word)(Entry);
                limit_hi = (byte)(Misc.GetHi(Misc.GetHi(Entry)) & 0x0f);
                base_low = Misc.GetHi((UInt32)Entry);
                base_middle = Misc.GetLo(Misc.GetLo((DWord)Misc.GetHi((QWord)Entry)));

                byte lAccess = (byte)((Misc.GetHi(Misc.GetLo((DWord)Misc.GetHi((QWord)Entry)))));
                access.Present = (lAccess & 0x80) == 0x80;
                access.m_PrivLvl = (ePrivLvl)((lAccess >> 5) & 3);
                access.DescType = (eGDTDescType)Misc.getBit(lAccess, 4);
                access.m_SegType = (eGDTSegType)(lAccess & 0x0f);
                if (access.DescType == eGDTDescType.System)
                    access.m_SystemDescType = (eSystemOrGateDescType)(Misc.GetHi(Misc.GetLo((DWord)Misc.GetHi((QWord)Entry))) & 0x0f);
                else
                    access.m_SegType = (eGDTSegType)(Misc.GetHi(Misc.GetLo((DWord)Misc.GetHi((QWord)Entry))) & 0x0f);


                byte lGranularity = (Byte)Misc.GetLo(Misc.GetHi((DWord)Misc.GetHi((QWord)Entry)));
                granularity.Limit_Granularity = Misc.getBit((Byte)Misc.GetLo(Misc.GetHi((DWord)Misc.GetHi((QWord)Entry))), 7);
                granularity.OpSize32 = Misc.getBit((Byte)Misc.GetLo(Misc.GetHi((DWord)Misc.GetHi((QWord)Entry))), 6) == 1;

                //base_high = Misc.GetHi((Word)Misc.GetHi((Word)Entry));
                base_high = Misc.GetHi((Word)Misc.GetHi((DWord)Misc.GetHi((QWord)Entry)));
                m_Base = (UInt32)((base_high << 24) | (base_middle << 16) | base_low);
                Limit = (UInt32)(limit_hi << 16 | limit_low);
            }
        }

        public sGDTEntry(UInt64 EntryData, UInt16 TableEntryNumber)
        {
            mValue = 0; m_limit = 0; limit_low = 0; base_low = 0; base_middle = 0; m_Base = 0; granularity = new sGDTGranularity(); access = new sGDTAccess(); base_high = 0; limit_hi = 0;
            m_TableEntryNumber = TableEntryNumber;
            Parse(EntryData);
        }
    }

    public struct sIDTEntry
    {
        private QWord mValue;
        public UInt32 PEP_Offset;
        public ePrivLvl Descriptor_PL;
        bool Present;
        public UInt16 SegSelector;
        public Word GDTCachePointer;
        public sGDTEntry GDTEntry;
        public eSystemOrGateDescType GateType;
        public QWord Value { get { return mValue; } }
        public bool GateSize32;

        public void Parse(cGDTCache GDTCache)
        {
            UInt16 Hi, MidHi, MidLo, Lo;

            if (mValue > 0)
            {
                Hi = Misc.GetHi(Misc.GetHi(mValue));
                Lo = Misc.GetLo(Misc.GetLo(mValue));
                MidHi = Misc.GetLo(Misc.GetHi(mValue));
                MidLo = Misc.GetHi(Misc.GetLo(mValue));

                PEP_Offset = (UInt32)((Hi << 16) | Lo);
                Present = Misc.GetBits3(MidHi, 15, 1) == 1;
                Descriptor_PL = (ePrivLvl)Misc.GetBits3(MidHi, 13, 2);
                SegSelector = MidLo;
                GDTCachePointer = (Word)(SegSelector >> 3);
                //CLR 04/01/2014 - got rid of the = in >= because TinyCore wanted to load an IDT that didn't have a matching GDT entry yet
                if (GDTCache != null && GDTCache.Count > GDTCachePointer)
                    GDTEntry = GDTCache[GDTCachePointer];
                GateType = (eSystemOrGateDescType)Misc.GetBits3(MidHi, 8, 5);
                GateSize32 = Misc.GetBits3(MidHi, 11, 1) == 1;
            }
        }

        public sIDTEntry(UInt64 EntryData, cGDTCache GDTCache)
        {
            mValue = EntryData;
            PEP_Offset = 0; Descriptor_PL = ePrivLvl.App_Level_3; Present = false; GateType = eSystemOrGateDescType.Reserved; SegSelector = 0; GDTEntry = new sGDTEntry();
            GateSize32 = false;
            GDTEntry = new sGDTEntry();
            GDTCachePointer = 0;
            Parse(GDTCache);
        }

    }

    public struct sTSS
    {
        public bool TSS_is_32bit, TrapFlg;
        public UInt16 PrevTaskLink, SS0, SS1, SS2, ES, CS, SS, DS, FS, GS, LDT_SegSel, IO_Map_Base_Add;
        public UInt32 ESP0, ESP1, ESP2, CR3, EIP, EFLAGS, EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI;

        public byte[] GetValue(Processor_80x86 mProc, ref sInstruction sIns)
        {
            return mProc.mem.Chunk(mProc, ref sIns,0,mProc.regs.TR.mBase,0x67);
        }

        public void Commit(Processor_80x86 mProc, ref sInstruction sIns)
        {
            mProc.mem.RefreshSuspended = true;
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 0, PrevTaskLink);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 4, ESP0);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 8, SS0);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 12, ESP1);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 16, SS1);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 20, ESP2);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 24, SS2);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 28, CR3);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 32, EIP);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 36, EFLAGS);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 40, EAX);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 44, ECX);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 48, EDX);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 52, EBX);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 56, ESP);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 60, EBP);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 64, ESI);
            mProc.mem.SetDWord(mProc, ref sIns, mProc.regs.TR.mBase + 68, EDI);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 72, ES);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 76, CS);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 80, SS);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 84, DS);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 88, FS);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 92, GS);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 96, LDT_SegSel);
            mProc.mem.SetWord(mProc, ref sIns, mProc.regs.TR.mBase + 102, IO_Map_Base_Add);
            mProc.mem.RefreshSuspended = false;
        }

        public sTSS(byte[] Value, bool TSS32)
        {
            int ptr = 0;

            TSS_is_32bit = TSS32;
            PrevTaskLink = (Word)((Value[1]) << 8 | Value[0]);
            ESP0 = (DWord)((Value[7]) << 24 | (Value[6]) << 16 | (Value[5]) << 8 | (Value[4]));
            SS0 = (Word)((Value[9]) << 8 | Value[8]);
            ESP1 = (DWord)((Value[15]) << 24 | (Value[14]) << 16 | (Value[13]) << 8 | (Value[12]));
            SS1 = (Word)((Value[17]) << 8 | Value[16]);
            ESP2 = (DWord)((Value[23]) << 24 | (Value[22]) << 16 | (Value[21]) << 8 | (Value[20]));
            SS2 = (Word)((Value[25]) << 8 | Value[24]);

            ptr = 28;
            CR3 = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr += 4; //32
            EIP = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //36
            EFLAGS = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //40
            EAX = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //44
            ECX = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //48
            EDX = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //52
            EBX = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //56
            ESP = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr +=4;  //60
            EBP = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr += 4;  //64
            ESI = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr += 4;  //68
            EDI = (DWord)(Value[ptr + 3] << 24 | Value[ptr + 2] << 16 | Value[ptr + 1] << 8 | Value[ptr]);
            ptr += 4;  //72
            ES = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //76
            CS = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //80
            SS = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //84
            DS = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //88
            FS = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //92
            GS = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //96
            LDT_SegSel = (Word)((Value[ptr + 1]) << 8 | Value[ptr]);
            ptr += 4;  //10
            TrapFlg = (Value[ptr] & 0x01) == 1;
            IO_Map_Base_Add = (Word)((Value[ptr + 3]) << 8 | Value[ptr + 2]);
        }

    }

    public static class Misc
    {
        public const byte GDT_ACCESS_SEG_TYPE_CODE = 1, GDT_ACCESS_SEG_TYPE_DATA = 2;
		public const int  PAGING_SUPERVISOR_MASK = 0x0, PAGING_USER_MASK = 0x4;
		public const int PAGING_PAGE_READ_ONLY_MASK = 0, PAGING_PAGE_READ_WRITE_MASK = 0x2;
		public const int PAGING_PAGE_PRESENT = 1, PAGING_PAGE_NOT_PRESENT = 0;
		public const int CR0_WP_BIT_MASK = 0x10000;

		public const byte EXCEPTION_SUPERVISOR_MODE = 0,  EXCEPTION_USER_MODE = 0x4, EXCEPTION_PAGE_NOT_PRESENT = 0, EXCEPTION_PAGE_PROTECTION_ERROR = 1,
			EXCEPTION_ERROR_READ = 0, EXCEPTION_ERROR_WRITE=0x2;

        public static byte GetBits3(UInt64 b, int offset, int count)
        {
            return (Byte)((UInt64)(b >> offset) & (UInt64)((1 << count) - 1));
        }
        public static byte getBit(UInt64 InVal, int BitNumber)
        {
            return GetBits3(InVal, BitNumber, 1);
            //return (byte)((InVal & (UInt64)(1 << (BitNumber))) >> BitNumber);
//getBit(new cValue(InVal), BitNumber);
            //byte lTemp;
            //if (((UInt64)Math.Pow(2, BitNumber) & InVal) == Math.Pow(2, BitNumber))
            //    lTemp = 1;
            //else
            //    lTemp = 0;
            //return lTemp;
        }
        //public static byte getBit(UInt32 InVal, int BitNumber)
        //{
        //    byte lTemp;
        //    if (((DWord)Math.Pow(2, BitNumber) & InVal) == Math.Pow(2, BitNumber))
        //        lTemp = 1;
        //    else
        //        lTemp = 0;
        //    return lTemp;
        //    //String lVal = Convert.ToString(InVal, 2).PadLeft(32, '0');
        //    //return Convert.ToByte(lVal.Substring(31 - BitNumber/*-1*/, 1));
        //}
        //public static byte getBit(UInt16 InVal, int BitNumber)
        //{
        //    byte lTemp;
        //    if (((Word)Math.Pow(2, BitNumber) & InVal) == Math.Pow(2, BitNumber))
        //        lTemp = 1;
        //    else
        //        lTemp = 0;
        //    return lTemp;
        //    //String lVal = Convert.ToString(InVal, 2).PadLeft(16, '0');
        //    //return Convert.ToByte(lVal.Substring(15 - BitNumber/*-1*/, 1));
        //}
        //public static byte getBit(byte InVal, int BitNumber)
        //{
        //    byte lTemp;
        //    if ( ((byte)Math.Pow(2, BitNumber) & InVal) == Math.Pow(2, BitNumber))
        //        lTemp = 1;
        //    else
        //        lTemp = 0;
        //    return lTemp;
        //    //String lVal = Convert.ToString(InVal, 2).PadLeft(8, '0');
        //    //return Convert.ToByte(lVal.Substring(7 - BitNumber/*-1*/, 1));
        //}
        public static byte setBit(byte InVal, int BitNumber, bool Value)
        {
            if (Value == true)
                return (byte)(InVal | (1 << BitNumber));
            else
                return (byte)(InVal & (0xFF - (1 << BitNumber)));
        }
        public static UInt16 setBit(UInt16 InVal, int BitNumber, bool Value)
        {
            if (Value == true)
                return (UInt16)(InVal | (1 << BitNumber));
            else
                return (UInt16)(InVal & (0xFFFF - (1 << BitNumber)));
        }
        public static UInt32 setBit(UInt32 InVal, int BitNumber, bool Value)
        {
            if (Value == true)
                return (UInt32)(InVal | (UInt32)(1 << BitNumber));
            else
                return (UInt32)(InVal & (0xFFFFFFFF - (1 << BitNumber)));
        }
        public static UInt64 setBit(UInt64 InVal, int BitNumber, bool Value)
        {
            if (Value == true)
                return (UInt64)(InVal | (UInt64)((UInt64)1 << BitNumber));
            else
                return (UInt64)(InVal & (UInt64)(0xFFFFFFFFFFFFFFFF - ((UInt64)1 << BitNumber)));
        }

        public static byte Negate(byte InVal)
        {
            return (byte)(-(InVal));
        }
        public static UInt16 Negate(UInt16 InVal)
        {
            //InVal ^= 0xFFFF;
            //++InVal;
            //return InVal;
            return (Word)(-(InVal));
        }
        public static UInt32 Negate(UInt32 InVal)
        {
            //InVal ^= 0xFFFFFFFF;
            //++InVal;
            //return InVal;
            return (DWord)(-(InVal));
        }
        public static UInt64 Negate(UInt64 InVal)
        {
//            return InVal ^ 0xFFFFFFFFFFFFFFFF;
            //InVal ^= 0xFFFFFFFFFFFFFFFF;
            //++InVal;
            //return ++InVal;
            throw new Exception("Cannot negate a 64 bit value!");
            //return (QWord)(-(InVal));
        }

        public static bool IsNegative(byte Value)
        {
            if ((Value & 0x80) == 0x80)
                return true;
            else
                return false;
        }
        public static bool IsNegative(UInt16 Value)
        {
            if ((Value & 0x8000) == 0x8000)
                return true;
            else
                return false;
        }
        public static bool IsNegative(UInt32 Value)
        {
            if ((Value & 0x80000000) == 0x80000000)
                return true;
            else
                return false;
        }

        public static byte GetHi(UInt16 Value)
        {
            return (byte)(Value >> 8);
        }
        public static byte GetLo(UInt16 Value)
        {
            return (byte)Value;
        }

        public static UInt16 GetHi(UInt32 Value)
        {
            return (UInt16)(Value >> 16);
        }
        public static UInt16 GetLo(UInt32 Value)
        {
            return (UInt16)Value;
        }

        public static UInt32 GetHi(UInt64 Value)
        {
            return (UInt32)(Value  >> 32);
        }
        public static UInt32 GetLo(UInt64 Value)
        {
            return (UInt32)Value;
        }
        public static UInt32 GetHi(Int64 Value)
        {
            return (UInt32)(Value >> 32);
        }
        public static UInt32 GetLo(Int64 Value)
        {
            return (UInt32)Value;
        }

        public static UInt16 SetHi(UInt16 Dest, byte Value)
        {
            Dest &= 0x00FF;
            return (UInt16)((Value << 8) | Dest);
        }
        public static UInt16 SetLo(UInt16 Dest, byte Value)
        {
            Dest &= 0xFF00;
            return (UInt16)(Value | Dest);
        }
        public static UInt32 SetHi(UInt32 Dest, UInt16 Value)
        {
            Dest &= 0x0000FFFF;
            return (UInt32)((Value << 16) + Dest);
        }
        public static UInt32 SetLo(UInt32 Dest, UInt16 Value)
        {
            Dest &= 0xFFFF0000;
            return (UInt32)(Value | Dest);
        }
        public static UInt64 SetHi(UInt64 Dest, UInt32 Value)
        {
            Dest &= 0x00000000FFFFFFFF;
            UInt64 lValue = ((UInt64)(Value) << 32);
            return (UInt64)(lValue | Dest);
        }
        public static UInt64 SetLo(UInt64 Dest, UInt32 Value)
        {
            Dest &= 0xFFFFFFFF00000000;
            UInt64 lValue = (UInt64)Value;
            return (UInt64)((lValue | Dest));
        }

        public static byte GetHiNibble(byte Value)
        {
            return ((byte)(Value>> 4));
        }
        public static byte GetLoNibble(byte Value)
        {
            return ((byte)(Value & 0x0F));
        }

        //public static UInt16 RotateLeft(this UInt16 value, int count)
        //{ return (UInt16)((value << count) | (value >> (16 - count))); }
        //public static UInt16 RotateRight(this UInt16 value, int count)
        //{ return (UInt16)((value >> count) | (value << (16 - count))); }
        public static void SignExtend(ref sOpVal Value, ref TypeCode FromType, TypeCode ToType)
        {
            switch (ToType)
            {
                case TypeCode.Byte: //ToType = Byte, nothing to do
                    return;
                case TypeCode.UInt16:
                    if (FromType == TypeCode.Byte)
                        if ((Value.OpByte & 0x80) == 0x80)
                        {
                            //return new cValue((UInt16)(0xFF00 + Value));
                            Value.OpWord = (Word)(0xFF00 | Value.OpByte);
                            FromType = ToType;
                            return;
                        }
                        else
                        {
                            FromType = ToType;
                            return;
                        }
                    else
                        return;
                case TypeCode.UInt32:
                    if (FromType == TypeCode.Byte)
                         if ((Value.OpByte & 0x80) == 0x80)
                        {
                            //return new cValue((UInt16)(0xFF00 + Value));
                            Value.OpDWord = (DWord)(0xFFFFFF00 | Value.OpByte);
                            FromType = ToType;
                            return;
                        }
                        else
                        {
                            FromType = ToType;
                            return;
                        }
                   else if (FromType == TypeCode.UInt16)
                        if ((Value.OpWord & 0x8000) == 0x8000)
                        {
                            Value.OpDWord = (DWord)(0xFFFF0000 | Value.OpWord);
                            FromType = ToType;
                            return;
                        }
                        else
                        //return new cValue((UInt32)Value.mByte);
                        {
                            FromType = ToType;
                            return;
                        }
                    else
                        return;
                case TypeCode.UInt64:
                    if (FromType == TypeCode.Byte)
                        if ((Value.OpByte & 0x80) == 0x80 /*Misc.getBit(Value, 7) == 1*/)
                        {
                            Value.OpQWord = 0xFFFFFFFFFFFFFF00 | Value.OpByte;
                            FromType = ToType;
                            return;
                        }
                        else
                        {
                            FromType = ToType;
                            return;
                        }
                    else if (FromType == TypeCode.UInt16)
                        if ((Value.OpWord & 0x8000) == 0x8000)
                        {
                            Value.OpQWord = 0xFFFFFFFFFFFF0000 | Value.OpWord;
                            FromType = ToType;
                            return;
                        }
                        else
                        {
                            FromType = ToType;
                            return;
                        }
                    else if (FromType == TypeCode.UInt32)
                        if ((Value.OpWord & 0x80000000) == 0x80000000)
                        {
                             Value.OpQWord = 0xFFFFFFFF00000000 | Value.OpDWord;
                            FromType = ToType;
                            return;
                        }
                        else
                        {
                            FromType = ToType;
                            return;
                        }
                    else
                        return;
            }
            throw new Exception("Can't sign extend the value " + Value + "as it is not a Byte, Word, DWord or QWord");
        }

        public static sOpVal SignExtend2(sOpVal Value, ref TypeCode FromType, TypeCode ToType)
        {
            switch (ToType)
            {
                case TypeCode.Byte: //ToType = Byte, nothing to do
                    return Value;
                case TypeCode.UInt16:
                    if (FromType == TypeCode.Byte)
                        if ((Value.OpByte & 0x80) == 0x80)
                        {
                            //return new cValue((UInt16)(0xFF00 + Value));
                            Value.OpWord = (Word)(0xFF00 | Value.OpByte);
                            FromType = ToType;
                            return Value;
                        }
                        else
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else
                        return Value;
                case TypeCode.UInt32:
                    if (FromType == TypeCode.Byte)
                        if ((Value.OpByte & 0x80) == 0x80)
                        {
                            //return new cValue((UInt16)(0xFF00 + Value));
                            Value.OpDWord = (DWord)(0xFFFFFF00 | Value.OpByte);
                            FromType = ToType;
                            return Value;
                        }
                        else
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else if (FromType == TypeCode.UInt16)
                        if ((Value.OpWord & 0x8000) == 0x8000)
                        {
                            Value.OpDWord = (DWord)(0xFFFF0000 | Value.OpWord);
                            FromType = ToType;
                            return Value;
                        }
                        else
                        //return new cValue((UInt32)Value.mByte);
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else
                        return Value;
                case TypeCode.UInt64:
                    if (FromType == TypeCode.Byte)
                        if ((Value.OpByte & 0x80) == 0x80 /*Misc.getBit(Value, 7) == 1*/)
                        {
                            Value.OpQWord = 0xFFFFFFFFFFFFFF00 | Value.OpByte;
                            FromType = ToType;
                            return Value;
                        }
                        else
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else if (FromType == TypeCode.UInt16)
                        if ((Value.OpWord & 0x8000) == 0x8000)
                        {
                            Value.OpQWord = 0xFFFFFFFFFFFF0000 | Value.OpWord;
                            FromType = ToType;
                            return Value;
                        }
                        else
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else if (FromType == TypeCode.UInt32)
                        if ((Value.OpWord & 0x80000000) == 0x80000000)
                        {
                            Value.OpQWord = 0xFFFFFFFF00000000 | Value.OpDWord;
                            FromType = ToType;
                            return Value;
                        }
                        else
                        {
                            FromType = ToType;
                            return Value;
                        }
                    else
                        return Value;
            }
            throw new Exception("Can't sign extend the value " + Value + "as it is not a Byte, Word, DWord or QWord");
        }

        public static UInt16 SignExtend(byte Value)
        {
            if ((Value & 0x80) == 0x80)
                return (Word)(0xFF00 | Value);
            else
                return (Word)Value;
        }
        public static UInt32 SignExtend(UInt16 Value)
        {
            if ((Value & 0x8000) == 0x8000)
                return (DWord)(0xFFFF0000 | Value);
            else
                return (DWord)Value;
        }
        public static UInt64 SignExtend(UInt32 Value)
        {
            if ((Value & 0x80000000) == 0x80000000)
                return (DWord)(0xFFFFFFFF00000000 | Value);
            else
                return (QWord)Value;
        }

        public static sOpVal RotateLeft(sOpVal Value, int count, TypeCode OpType)
        {
            int lRotBack = 0;

            switch (OpType)
            {
                case TypeCode.Byte:
                    lRotBack = 8;
                    Value.OpByte = (byte)((Value.OpByte << count) | (Value.OpByte >> (lRotBack - count)));
                    return Value;
                case TypeCode.UInt16:
                    lRotBack = 16;
                    Value.OpWord = (Word)((Value.OpWord << count) | (Value.OpWord >> (lRotBack - count)));
                    return Value;
                case TypeCode.UInt32:
                    lRotBack = 32;
                    Value.OpDWord = (DWord)((Value.OpDWord << count) | (Value.OpDWord >> (lRotBack - count)));
                    return Value;
                case TypeCode.UInt64:
                    lRotBack = 64;
                    Value.OpQWord = (QWord)((Value.OpQWord << count) | (Value.OpQWord >> (lRotBack - count)));
                    return Value;
                default:
                    throw new Exception("D'Oh!");
            }
        }
        public static sOpVal RotateRight(sOpVal Value, int count, TypeCode OpType)
        {
            int lRotBack = 0;

            switch (OpType)
            {
                case TypeCode.Byte:
                    lRotBack = 8;
                    Value.OpByte = (byte)((Value.OpByte >> count) | (Value.OpByte << (lRotBack - count)));
                    return Value;
                case TypeCode.UInt16:
                    lRotBack = 16;
                    Value.OpWord = (Word)((Value.OpWord >> count) | (Value.OpWord << (lRotBack - count)));
                    return Value;
                case TypeCode.UInt32:
                    lRotBack = 32;
                    Value.OpDWord = (DWord)((Value.OpDWord >> count) | (Value.OpDWord << (lRotBack - count)));
                    return Value;
                case TypeCode.UInt64:
                    lRotBack = 64;
                    Value.OpQWord = (QWord)((Value.OpQWord >> count) | (Value.OpQWord << (lRotBack - count)));
                    return Value;
                default:
                    throw new Exception("D'Oh!");
            }
        }
 
        public static byte[,] ModRMMap1Byte = new byte[16, 16] 
        {
           //0 1 2 3 4 5 6 7  8 9 A B C D E F
            {1,1,1,1,0,0,0,0, 1,1,1,1,0,0,0,0}, // 00
	        {1,1,1,1,0,0,0,0, 1,1,1,1,0,0,0,0}, // 10
            {1,1,1,1,0,0,0,0, 1,1,1,1,0,0,0,0}, // 20
	        {1,1,1,1,0,0,0,0, 1,1,1,1,0,0,0,0}, // 30
            {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // 40
	        {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // 50
            {0,0,1,1,0,0,0,0, 0,1,0,1,0,0,0,0}, // 60
	        {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // 70
            {1,1,1,1,1,1,1,1, 1,1,1,1,1,1,1,1}, // 80
	        {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // 90
            {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // A0
	        {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // B0
            {1,1,0,0,1,1,1,1, 0,0,0,0,0,0,0,0}, // C0
	        {1,1,1,1,0,0,0,0, 0,1,0,0,0,0,0,0}, // D0
            {0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0}, // E0
	        {0,0,0,0,0,0,1,1, 0,0,0,0,0,0,1,1}  // F0
        };

        public static byte[] parity = new byte[0x100] {
	1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
	0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
	0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
	1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
	0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
	1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
	1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
	0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1
};

        //Thanks BOCHS!!!
        public static byte[,] ModRMMap2Byte = new byte[16, 16] 
        {
          /* 0 1 2 3 4 5 6 7 8 9 a b c d e f            */
           	{1,1,1,1,2,0,0,0,0,0,2,0,2,1,0,1}, /* 0F 00 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F 10 */
	        {1,1,1,1,1,2,1,2,1,1,1,1,1,1,1,1}, /* 0F 20 */
	        {0,0,0,0,0,0,2,2,1,2,1,2,2,2,2,2}, /* 0F 30 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F 40 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F 50 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F 60 */
	        {1,1,1,1,1,1,1,0,1,1,2,2,1,1,1,1}, /* 0F 70 */
	        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, /* 0F 80 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F 90 */
	        {0,0,0,1,1,1,0,0,0,0,0,1,1,1,1,1}, /* 0F A0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F B0 */
	        {1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0}, /* 0F C0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F D0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* 0F E0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2}  /* 0F F0 */
        };

        public static byte[,] FPUModRMMap2Byte = new byte[16, 16] 
        {
          /* 0 1 2 3 4 5 6 7 8 9 a b c d e f            */
           	{1,1,1,1,2,0,0,0,0,0,2,0,2,1,0,1}, /* D0 00 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* D1 10 */
	        {1,1,1,1,1,2,1,2,1,1,1,1,1,1,1,1}, /* D2 20 */
	        {0,0,0,0,0,0,2,2,1,2,1,2,2,2,2,2}, /* D3 30 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* D4 40 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* D5 50 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* D6 60 */
	        {1,1,1,1,1,1,1,0,1,1,2,2,1,1,1,1}, /* D7 70 */
	        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, /* D8 80 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* D9 90 */
	        {0,0,0,1,1,1,0,0,0,0,0,1,1,1,1,1}, /* DA A0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* DB B0 */
	        {1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0}, /* DC C0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* DD D0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, /* DE E0 */
	        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2}  /* DF F0 */
        };

        public static int ModRMRequired(UInt32 Opcode, bool TwoByteOpcode)
        {
            //Example: 0x31 XOR - (3,1) = 1
            //Example: 0x5C POP - (5,C) = 0
            if (TwoByteOpcode)
                return ModRMMap2Byte[((byte)(Opcode) & 0xF0) >> 4, (byte)(Opcode) & 0x0F];
            else
                return ModRMMap1Byte[ (Opcode & 0xF0) >> 4, Opcode & 0x0F];
        }

        public static eGeneralRegister GetRegIDForRegName(String RegName)
        {
            //eGeneralRegister

            switch (RegName)
            {
                case "AL": return eGeneralRegister.AL;
                case "CL": return eGeneralRegister.CL;
                case "DL": return eGeneralRegister.DL;
                case "BL": return eGeneralRegister.BL;
                case "AH": return eGeneralRegister.AH;
                case "CH": return eGeneralRegister.CH;
                case "DH": return eGeneralRegister.DH;
                case "BH": return eGeneralRegister.BH;
                case "AX": return eGeneralRegister.AX;
                case "CX": return eGeneralRegister.CX;
                case "DX": return eGeneralRegister.DX;
                case "BX": return eGeneralRegister.BX;
                case "SP": return eGeneralRegister.SP;
                case "BP": return eGeneralRegister.BP;
                case "SI": return eGeneralRegister.SI;
                case "DI": return eGeneralRegister.DI;
                case "EAX": return eGeneralRegister.EAX;
                case "ECX": return eGeneralRegister.ECX;
                case "EDX": return eGeneralRegister.EDX;
                case "EBX": return eGeneralRegister.EBX;
                case "ESP": return eGeneralRegister.ESP;
                case "EBP": return eGeneralRegister.EBP;
                case "ESI": return eGeneralRegister.ESI;
                case "EDI": return eGeneralRegister.EDI;
                case "CS": return eGeneralRegister.CS;
                case "DS": return eGeneralRegister.DS;
                case "ES": return eGeneralRegister.ES;
                case "FS": return eGeneralRegister.FS;
                case "GS": return eGeneralRegister.GS;
                case "SS": return eGeneralRegister.SS;
                case "NONE": return eGeneralRegister.NONE;
                default:
                    return eGeneralRegister.NONE;
            }

        }

        #region Descriptor Stuff
        /// <summary>
        /// Base = Upper 4 bytes, Limit = Lower 2 bytes
        /// </summary>
        /// <param name="Base"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        public static UInt64 GetPModeDescriptor(UInt32 Base, UInt16 Limit)
        {
            return (UInt64)((0 << 16) + (Base << 32) + Limit);
        }
        public static void ParsePModeDescriptor(Processor_80x86 mProc, UInt64 Descriptor, ref UInt32 Base, ref UInt16 Limit, ref UInt16 SegSel)
        {
            Limit = GetPModeLimit(Descriptor);
            SegSel = GetPModeSegSel(Descriptor);
            if (mProc.OpSize16)
                Base = GetPModeBase(Descriptor) & 0x00FFFFFF;
            else
                Base = GetPModeBase(Descriptor);

        }
        public static UInt32 GetPModeBase(UInt64 Descriptor)
        {
            return (UInt32)(Descriptor >> 16);
        }
        public static UInt16 GetPModeLimit(UInt64 Descriptor)
        {
            return (Word)Descriptor;
        }
        public static UInt16 GetPModeSegSel(UInt64 Descriptor)
        {
            return (UInt16)((Descriptor & 0xFFFF000000000000) >> 48);
        }
        #endregion

        public static UInt32 GetSelectorForSegment(Processor_80x86 mProc, UInt32 Location, UInt32 NullValue)
        {
            if (Location == Processor_80x86.RCS)
                if (mProc.regs.CS.mDescriptorNum > 0)
                    return mProc.regs.CS.mWholeSelectorValue;
                else return NullValue;
            else if (Location == Processor_80x86.RDS)
                if (mProc.regs.DS.mDescriptorNum > 0)
                    return mProc.regs.DS.mWholeSelectorValue;
                else return NullValue;
            else if (Location == Processor_80x86.RES)
                if (mProc.regs.ES.mDescriptorNum > 0)
                    return mProc.regs.ES.mWholeSelectorValue;
                else return NullValue;
            else if (Location == Processor_80x86.RFS)
                if (mProc.regs.FS.mDescriptorNum > 0)
                    return mProc.regs.FS.mWholeSelectorValue;
                else return NullValue;
            else if (Location == Processor_80x86.RGS)
                if (mProc.regs.GS.mDescriptorNum > 0)
                    return mProc.regs.GS.mWholeSelectorValue;
                else return NullValue;
            else if (Location == Processor_80x86.RSS)
                if (mProc.regs.SS.mDescriptorNum > 0)
                    return mProc.regs.SS.mWholeSelectorValue;
                else return NullValue;
            else
                throw new Exception("GetSelectorForSegment: Shouldn't get here!");

        }

        #region Lists of Names
        //public static ArrayList alRegisterNames = new ArrayList { 
        //"EAX", "EBX", "ECX", "EDX", "ESP", "EBP", "ESI", "EDI", "DS", "SS", "ES", "CS", "IP", "FLAGS",
        //"AX", "BX", "CX", "DX", "AH", "AL", "BH", "BL", "CH", "CL", "DH", "DL", "DI", "BP", "SI", "SP"};

        public static string[] alRegisterNames = new string[32] {"AX", "BX", "CX", "DX", "AH", "AL", "BH", "BL", "CH", "CL", "DH", "DL", 
            "DI", "BP", "SI", "SP", "EAX", "EBX", "ECX", "EDX", "ESP", "EBP", "ESI", "EDI","DS", "SS", "ES", "CS", "FS", "GS", "IP", "FLAGS"};
        
        
        #endregion

        static Dictionary<Type, Delegate> _cachedIL = new Dictionary<Type, Delegate>();

    }

    public class cGDTCache : System.Collections.CollectionBase
    {
        public void SetBusyFlag(Processor_80x86 mProc, Word EntryNum, bool Value)
        {
            byte lOldValue, lNewValue;

            lOldValue = mProc.mem.pMemory(mProc, (UInt32)(mProc.regs.GDTR.Base + (8 * EntryNum) + 5));
            if (Value)
                lNewValue = (byte)(lOldValue | 0x2);
            else
                lNewValue = (byte)(lOldValue & ~0x2);
            PhysicalMem.pMemory(mProc, (UInt32)(mProc.regs.GDTR.Base + (8 * EntryNum) + 5), lNewValue);
        }
        
        public sGDTEntry this[int index]
        {
            get {

                if (this.List.Count < index)
                    //throw new Exception("Table entry of " + index.ToString("X2") + "H is greater than the size of the GDT/LDT table, which has " + this.List.Count + "items.");
                    return (sGDTEntry)this.List[0];
                else
                   return (sGDTEntry)this.List[index]; }
            set { this.List[index] = value; }
        }

        public int IndexOf(sGDTEntry item)
        {
            return base.List.IndexOf(item);
        }

        public int Add (sGDTEntry item)
        {
            return this.List.Add(item);
        }

        public void Remove(sGDTEntry item)
        {
            this.InnerList.Remove(item);
        }

        public bool Contains(sGDTEntry item)
        {
            return this.List.Contains(item);
        }

        public void Insert(int Index, sGDTEntry item)
        {
            this.List.Insert(Index, item);
        }

        public void Populate(UInt64[] TableData, UInt16 LimitHigh)
        {
            //This may be a hack, I'm not sure
            //GRUB LGDT's with all zeroes, meaning nothing is loaded into the cache
            //So I decided to load an entry with all zeroes if the table passed is empty
            if (TableData.Count() == 0)
                TableData = new UInt64[1];
            UInt16 lCount = 0;
            foreach (UInt64 Entry in TableData)
            {
                Add(new sGDTEntry(Entry, lCount++));
            }
        }

        public cGDTCache(UInt64[] TableData, UInt16 LimitHigh)
        {
            Populate(TableData, LimitHigh);
        }

        public sGDTEntry[] ListActive()
        {
            sGDTEntry[] g = new sGDTEntry[this.List.Count];
            int lCount = 0;

            foreach (sGDTEntry s in this.List)
            {
                if (s.Present == true && (s.SystemDescType.Contains("TSS") || s.SystemDescType.Contains("Res")))
                    g[lCount++] = s;
            }
            Array.Resize(ref g, lCount);
            return g;
        }

        public sGDTEntry[] ListFirstBusyTSS()
        {
            sGDTEntry[] g = new sGDTEntry[this.List.Count];
            int lCount = 0;

            foreach (sGDTEntry s in this.List)
            {
                if (s.Present == true && s.SystemDescType.Contains("TSS") && s.SystemDescType.Contains("_Bu"))
                {
                    g[lCount++] = s;
                    break;
                }
            }
            Array.Resize(ref g, lCount);
            return g;
        }
    }

    public class cIDTCache : System.Collections.CollectionBase
    {
        public sIDTEntry this[int index]
        {
            get { return (sIDTEntry)this.List[index]; }
            set { this.List[index] = value; }
        }

        public int IndexOf(sIDTEntry item)
        {
            return base.List.IndexOf(item);
        }

        public int Add (sIDTEntry item)
        {
            return this.List.Add(item);
        }

        public void Remove(sIDTEntry item)
        {
            this.InnerList.Remove(item);
        }

        public bool Contains(sIDTEntry item)
        {
            return this.List.Contains(item);
        }

        public void Insert(int Index, sIDTEntry item)
        {
            this.List.Insert(Index, item);
        }

        public void Populate(UInt64[] TableData, cGDTCache GDTCache)
        {
            if (TableData.Count() == 0)
                TableData = new UInt64[1];

            foreach (UInt64 Entry in TableData)
            {
                Add(new sIDTEntry(Entry, GDTCache));
            }
        }

        public cIDTCache(UInt64[] TableData, cGDTCache GDTCache )
        {
            Populate(TableData, GDTCache);
        }
    }
}
