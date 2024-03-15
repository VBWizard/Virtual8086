using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace VirtualProcessor
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MMReg
    {
        [FieldOffset(0)]
        /// <summary>
        /// Bottom 16 bits of a 48 bit descriptor
        /// </summary>
        internal UInt32 mLimit;
        [FieldOffset(4)]
        /// <summary>
        /// Middle 32 bits of a 48 bit descriptor
        /// </summary>
        internal UInt32 mBase;
        [FieldOffset(8)]
        /// <summary>
        /// Top 16 bits of a 48 bit descriptor
        /// </summary>
        internal UInt16 mSegSel;
        [FieldOffset(0)]
        private UInt64 mValue;
        [FieldOffset(12)]
        internal UInt64 mOriginalValue;
        [FieldOffset(24)]
        private String mName;
        [FieldOffset(200)]
        internal byte[] lCache;
        public String Name { get { return mName; } }
        public UInt16 SegSel { get { return mSegSel; } }
        public UInt32 Base { get { return mBase; } }
        public UInt32 Limit { get { return mLimit; } }
        public UInt64 Value { get { return mValue; } set { mValue = value; } }
        public void Parse(Processor_80x86 mProc, UInt64 Descriptor)
        {
            mLimit = (UInt16)Descriptor; // Misc.GetPModeLimit(Descriptor);
            mSegSel = (UInt16)((Descriptor & 0xFFFF000000000000) >> 48); //Misc.GetPModeSegSel(Descriptor);
            if (mProc.mCurrInstructOpSize16)
                mBase = Misc.GetPModeBase(Descriptor) & 0x00FFFFFF;
            else
                mBase = Misc.GetPModeBase(Descriptor);
            mOriginalValue = Descriptor;
        }

        public MMReg(String Name, int dummy)
        {
            mSegSel = 0; mBase = 0; mLimit = 0; mValue = 0;
            mName = Name;
            mOriginalValue = 0;
            lCache = new byte[0];
        }
    }

    public struct sSegmentSelector
    {
        private readonly Processor_80x86 mProc;
        internal UInt32 mDescriptorNum, mPrevDescriptorNum;
        public UInt32 DescriptorNum { get { return mDescriptorNum; } }
        private SegSelTableIndicator TableIndicator;
        internal sGDTEntry mSelector, mPrevSelector;
        public sGDTEntry Selector { get { return mSelector; } }
        internal UInt32 mRealModeValue;
        private UInt32 mRegMemoryAddress;
        public UInt32 MemAddr { get { return mRegMemoryAddress; } }
        private eGeneralRegister mWhichReg;
        internal UInt32 mWholeSelectorValue;
        public UInt32 Value
        {
            get
            {
                //if (mWhichReg == eGeneralRegister.CS && mProc.OperatingMode == ProcessorMode.Protected && mDescriptorNum > 0)
                //{
                //    if (mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Av || mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                //    {
                //        lTemp = (UInt32)((mProc.mem.pMemory(mProc, mSelector.Base + 0x4e) << 8)) | (mProc.mem.pMemory(mProc, mSelector.Base + 0x4f));
                //        return lTemp;
                //    }
                //    else
                //    {
                //        return mSelector.Base;
                //        //lTemp = (UInt32)(mProc.mem.pMemory(mProc, mProc.regs.GDTR.Base + (mDescriptorNum * 8) + 2) << 8);
                //        //lTemp |= (UInt32)(mProc.mem.pMemory(mProc, mProc.regs.GDTR.Base + (mDescriptorNum * 8) + 3));
                //        //lTemp |= (UInt32)(mProc.mem.pMemory(mProc, mProc.regs.GDTR.Base + (mDescriptorNum * 8) + 4) << 16);
                //        //lTemp |= (UInt32)(mProc.mem.pMemory(mProc, mProc.regs.GDTR.Base + (mDescriptorNum * 8) + 7) << 24);
                //        //return lTemp;
                //    }
                //}
                //else
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected && mDescriptorNum > 0)
                {
                    //if (mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Av || mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                    //{
                    //    UInt32 lCSAddr = (UInt32)((mProc.mem.pMemory(mProc, mSelector.Base + 0x4e) << 8)) | (mProc.mem.pMemory(mProc, mSelector.Base + 0x4f));
                    //}
                    return mSelector.m_Base;
                }
                else
                    return (UInt16)mRealModeValue;
            }
            set
            {
#if DEBUGGER_FEATURE
                ePrivLvl lCurrCPL = mProc.regs.CS.Selector.access.PrivLvl;
#endif
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                {   //Expecting to receive a Descriptor number, NOT a segment address
                    mPrevDescriptorNum = mDescriptorNum;
                    mDescriptorNum = (value & 0xFFFF) >> 3;
                    TableIndicator = (SegSelTableIndicator)((value & 0x04) >> 2);
                    //if (mSelector & 0x04 != 0x04)
                    //mProc.RefreshGDTCache();
                    mPrevSelector = mSelector;
                    if (TableIndicator == SegSelTableIndicator.GDT)
                        mSelector = mProc.mGDTCache[(int)mDescriptorNum];
                    else
                        mSelector = mProc.mLDTCache[(int)mDescriptorNum];

                    mWholeSelectorValue = value;
                    if (mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Av || mSelector.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                    {
                        UInt32 lCSAddr = (UInt32)((mProc.mem.pMemory(mProc, mSelector.Base + 0x4e) << 8)) | (mProc.mem.pMemory(mProc, mSelector.Base + 0x4f));
                        if (lCSAddr == 0)
                        {
                            mSelector = mPrevSelector;
                            mDescriptorNum = mPrevDescriptorNum;
                        }
                    }
#if DEBUGGER_FEATURE
                    if (mProc.regs.CS.Selector.access.PrivLvl == ePrivLvl.App_Level_3 && lCurrCPL != ePrivLvl.App_Level_3)
                        mProc.mSystem.CPL3SwitchBreakpoint = true;
                    else if (mProc.regs.CS.Selector.access.PrivLvl != ePrivLvl.App_Level_3 && lCurrCPL == ePrivLvl.App_Level_3)
                        mProc.mSystem.CPL0SwitchBreakpoint = true;
#endif
                }
                else
                //Expecting a segment address
                {
                    mRealModeValue = value;
                }
            }
        }
        public String Name
        {
            get { return mWhichReg.ToString(); }
        }
        public sSegmentSelector(Processor_80x86 proc, UInt32 RegisterMemoryAddress, eGeneralRegister WhichReg)
        {
            mProc = proc;
            mRegMemoryAddress = RegisterMemoryAddress;
            mWhichReg = WhichReg;
            mRealModeValue = 0;
            mSelector = mPrevSelector = new sGDTEntry();
            mDescriptorNum = mPrevDescriptorNum = 0;
            TableIndicator = 0;
            mWholeSelectorValue = 0;
        }
        public void ResetOnEnterPM()
        {
            mDescriptorNum = 0;
            mSelector = new sGDTEntry(0,0);
            mWholeSelectorValue = 0;
        }

    }

   [StructLayout(LayoutKind.Explicit)]
   public struct RegStruct
    {
        public RegStruct(Processor_80x86 proc)
        {
            mOverrideCPL = false;
            mOverrideCPLValue = ePrivLvl.OS_Ring_2;
            mProc = proc;
            FLAGSB = new sFlags(proc);
            GDTR = new MMReg("GDTR",0); ;
            IDTR = new MMReg("IDTR",0);
            LDTR = new MMReg("LDTR",0);
            TR = new MMReg("TR",0);
            TR.mBase = 0;
            TR.mLimit = 0xFFFF;
            CS = new sSegmentSelector(mProc, Processor_80x86.RCS, eGeneralRegister.CS);
            DS = new sSegmentSelector(mProc, Processor_80x86.RDS, eGeneralRegister.DS);
            ES = new sSegmentSelector(mProc, Processor_80x86.RES, eGeneralRegister.ES);
            FS = new sSegmentSelector(mProc, Processor_80x86.RFS, eGeneralRegister.FS);
            GS = new sSegmentSelector(mProc, Processor_80x86.RGS, eGeneralRegister.GS);
            SS = new sSegmentSelector(mProc, Processor_80x86.RSS, eGeneralRegister.SS);
            IP = AX = BX = CX = DX = SI = DI = BP = SP = FLAGS = 0;
            AL = AH = BL = BH = CL = CH = DL = DH = 0;
            EIP = EAX = EBX = ECX = EDX = ESI = EDI = EBP = ESP = EFLAGS = CR0 = CR2 = mCR3 = mCR4 = DR0 = DR1 = DR2 = DR3 = DR6 = DR7 = 0;
        }

        [FieldOffset(0)]
        public byte AL;
        [FieldOffset(1)]
        public byte AH;
        [FieldOffset(0)]
        public UInt16 AX;
        [FieldOffset(0)]
        public UInt32 EAX;

        [FieldOffset(4)]
        public byte BL;
        [FieldOffset(5)]
        public byte BH;
        [FieldOffset(4)]
        public UInt16 BX;
        [FieldOffset(4)]
        public UInt32 EBX;

        [FieldOffset(8)]
        public byte CL;
        [FieldOffset(9)]
        public byte CH;
        [FieldOffset(8)]
        public UInt16 CX;
        [FieldOffset(8)]
        public UInt32 ECX;

        [FieldOffset(12)]
        public byte DL;
        [FieldOffset(13)]
        public byte DH;
        [FieldOffset(12)]
        public UInt16 DX;
        [FieldOffset(12)]
        public UInt32 EDX;

        [FieldOffset(16)]
        public UInt16 SI;
        [FieldOffset(16)]
        public UInt32 ESI;

        [FieldOffset(20)]
        public UInt16 DI;
        [FieldOffset(20)]
        public UInt32 EDI;

        [FieldOffset(24)]
        public UInt16 BP;
        [FieldOffset(24)]
        public UInt32 EBP;

        [FieldOffset(28)]
        public UInt16 SP;
        [FieldOffset(28)]
        public UInt32 ESP;

        [FieldOffset(32)]
        public UInt16 IP;
        [FieldOffset(32)]
        public UInt32 EIP;

        [FieldOffset(36)]
        public UInt32 EFLAGS;
        [FieldOffset(36)]
        public UInt16 FLAGS;
        [FieldOffset(40)]
        public UInt32 CR0;
        //{
        //    get {return mCR0;}
        //    set 
        //    {
        //        mCR0 = value;
        //    }
        //}
        //[FieldOffset(40)]
        //internal UInt32 mCR0;
        [FieldOffset(44)]
        public UInt32 CR2;
        public UInt32 CR3
        {
            get { return mCR3; }
            set
            {
                mCR3 = value;
                if ((CR0 & 0x80000001) == 0x80000001)
                    mProc.mTLB.Flush(mProc);
            }
        }
        [FieldOffset(52)]
        internal UInt32 mCR3;
        public UInt32 CR4
		{
			get {return mCR4;}
			set 
			{
				mCR4 = value;
			}
		}
		[FieldOffset(60)]
        internal UInt32 mCR4;
        [FieldOffset(64)]
        internal ePrivLvl mOverrideCPLValue;
        public ePrivLvl CPL
        {
            get {
                if (mOverrideCPL == true)
                    return mOverrideCPLValue;
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Virtual8086)
                    return ePrivLvl.App_Level_3;
                else if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Real)
                    return ePrivLvl.Kernel_Ring_0;
                else
                    return mProc.regs.CS.Selector.access.PrivLvl; 
            }
        }
        [FieldOffset(68)]
        internal bool mOverrideCPL;
       [FieldOffset(240)]
        public MMReg TR;

       [FieldOffset(1950)]
       public UInt32 DR0;
       [FieldOffset(1954)]
       public UInt32 DR1;
       [FieldOffset(1958)]
       public UInt32 DR2;
       [FieldOffset(1962)]
       public UInt32 DR3;
       [FieldOffset(1966)]
       public UInt32 DR6;
       [FieldOffset(1970)]
       public UInt32 DR7;
       
       [FieldOffset(2000)]
        public MMReg IDTR;
        [FieldOffset(5000)]
        public MMReg GDTR;
        [FieldOffset(8000)]
        public MMReg LDTR;

        [FieldOffset(9000)]
        public sSegmentSelector CS;
        [FieldOffset(10000)]
        public sSegmentSelector DS;
        [FieldOffset(11000)]
        public sSegmentSelector ES;
        [FieldOffset(12000)]
        public sSegmentSelector FS;
        [FieldOffset(13000)]
        public sSegmentSelector GS;
        [FieldOffset(14000)]
        public sSegmentSelector SS;
        [FieldOffset(15000)]
        public sFlags FLAGSB;
        [FieldOffset(16000)]
        private readonly Processor_80x86 mProc;

        public void ResetFlags()
        {
            EFLAGS = 2;
            setFlagIF(true);
        }

        #region Set Flag Methods
        /// <summary>
        /// SetC Sign Flag (SF) - set to 1 when result is negative. When result is positive it is set to 0. (This flag takes the value of the most significant bit.) 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns></returns>
        public void setFlagSF(UInt64 InVal)
        {
            if ((InVal & 0x8000000000000000) == 0x8000000000000000)
                FLAGS |= 0x80;
            else
                FLAGS &= 0xff7f;
            mProc.regs.FLAGSB.mSFDirty = true;
        }
        public void setFlagSF(UInt32 InVal)
        {
            if ((InVal & 0x80000000) == 0x80000000)
                FLAGS |= 0x80;
            else
                FLAGS &= 0xff7f;
            mProc.regs.FLAGSB.mSFDirty = true;
        }
        public void setFlagSF(UInt16 InVal)
        {
            if ((InVal & 0x8000) == 0x8000)
                FLAGS |= 0x80;
            else
                FLAGS &= 0xff7f;
            mProc.regs.FLAGSB.mSFDirty = true;
        }
        public void setFlagSF(byte InVal)
        {
            if ((InVal & 0x80) == 0x80)
                FLAGS |= 0x80;
            else
                FLAGS &= 0xff7f;
            mProc.regs.FLAGSB.mSFDirty = true;
        }
        public void setFlagSF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.SF, Value);
            if (Value)
                FLAGS |= 0x80;
            else
                FLAGS &= 0xff7f;
            mProc.regs.FLAGSB.mSFDirty = true;
            //FLAGSB.SetValues(FLAGS);
        }
        /// <summary>
        /// SetC Zero Flag (ZF) - set to 1 when result is zero. For non-zero result this flag is set to 0.
        /// </summary>
            /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns></returns>
        public void setFlagZF(UInt64 InVal)
        {
            //08/31/10 - updated for performance
            if (InVal == 0)
                FLAGS |= 0x40;
            else
                FLAGS &= 0xFFBF;
            mProc.regs.FLAGSB.mZFDirty = true;
        }
        public void setFlagZF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.ZF, Value);
            if (Value)
                FLAGS |= 0x40;
            else
                FLAGS &= 0xFFBF;
            mProc.regs.FLAGSB.mZFDirty = true;
        }
        /// <summary>
        /// SetC Parity Flag (PF) - this flag is set to 1 when there is even number of one bits in result, and to 0 when there is odd number of one bits. 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns>Updated Flags word</returns>
        public void setFlagPF(UInt64 InVal)
        {
            //Per intel 386 manual, modified this to only consider the bottom 8 bits
            if ((Misc.parity[InVal & 0xFF]) > 0 ) 
            //| Misc.parity[(InVal >> 8) & 0xFF] | Misc.parity[(InVal >> 16) & 0xFF] | Misc.parity[(InVal >> 24) & 0xFF] | Misc.parity[(InVal >> 32) & 0xFF] | Misc.parity[(InVal >> 40) & 0xFF] | Misc.parity[(InVal >> 48) & 0xFF] | Misc.parity[(InVal >> 56) & 0xFF]) > 0)
                FLAGS |= 0x4;
            else
                FLAGS &= 0xFFFB;
            mProc.regs.FLAGSB.mPFDirty = true;
       
        }
        public void setFlagPF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.PF, Value);
            if (Value)
                FLAGS |= 0x4;
            else
                FLAGS &= 0xFFFB;
            mProc.regs.FLAGSB.mPFDirty = true;
        }
        /// <summary>
        /// SetC when MSBs of preval & postval don't match
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="inPreVal"></param>
        /// <param name="inPostVal"></param>
        /// <returns></returns>
        public void setFlagOF_Add(byte PreVal1, byte PreVal2, byte PostVal)
        {
            if ((byte)(((PreVal1 ^ PreVal2 ^ 0x80) & (PostVal ^ PreVal1)) & 0x80) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            return;
        }
        public void setFlagOF_Sub(byte PreVal1, byte PreVal2, byte PostVal)
        {
            if ((byte)((PreVal1 ^ PreVal2) & (PreVal1 ^ PostVal) & 0x80) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
        }
        
       public void setFlagOF_Sub(UInt16 PreVal1, UInt16 PreVal2, UInt16 PostVal)
        {
            if ((byte)((PreVal1 ^ PreVal2) & (PreVal1 ^ PostVal) & 0x8000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
        }
        public void setFlagOF_Sub(UInt32 PreVal1, UInt32 PreVal2, UInt32 PostVal)
        {
            if ((byte)((PreVal1 ^ PreVal2) & (PreVal1 ^ PostVal) & 0x80000000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
        }
        public void setFlagOF_Sub(UInt64 PreVal1, UInt64 PreVal2, UInt64 PostVal)
        {
            if ( (byte)((PreVal1 ^ PreVal2) & (PreVal1 ^ PostVal) & 0x8000000000000000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
        }
        public void setFlagOF_Add(UInt16 PreVal1, UInt16 PreVal2, UInt16 PostVal)
        {
            if ((UInt16)(((PreVal1 ^ PreVal2 ^ 0x8000) & (PostVal ^ PreVal1)) & 0x8000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            return;
        }
        public void setFlagOF_Add(UInt32 PreVal1, UInt32 PreVal2, UInt32 PostVal)
        {
            if ((UInt32)(((PreVal1 ^ PreVal2 ^ 0x80000000) & (PostVal ^ PreVal1)) & 0x80000000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
            return;
        }
        public void setFlagOF_Add(UInt64 PreVal1, UInt64 PreVal2, UInt64 PostVal)
        {
            if ((UInt64)(((PreVal1 ^ PreVal2 ^ 0x8000000000000000) & (PostVal ^ PreVal1)) & 0x8000000000000000) > 0)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
            return;
        }
        public void setFlagOF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.OF, Value);
            if (Value)
                FLAGS |= 0x800;
            else
                FLAGS &= 0xF7FF;
            mProc.regs.FLAGSB.mOFDirty = true;
        }
        /// <summary>
        /// Auxiliary Flag (AF) - set to 1 when there is an unsigned overflow for low nibble (4 bits). 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="inPreVal">Value prior to operation</param>
        /// <param name="inPostVal">Value after operation</param>
        /// <returns>Updated Flags word</returns>
        public void setFlagAF(UInt64 inPreVal, UInt64 inPostVal)
        {
            //if ((inPreVal & 0x0f) < (inPostVal & 0x0f))
            //    setFlagAF(true);
            //else
            //    setFlagAF(false);
            setFlagAF(((inPreVal & 0x0f) < (inPostVal & 0x0f)));

        }
        public void setFlagAF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.AF, Value);
            if (Value)
                FLAGS |= 0x10;
            else
                FLAGS &= 0xFFEF;
            mProc.regs.FLAGSB.mAFDirty = true;
        }
        /// <summary>
        /// SetC when an arithmetic carry or borrow has been generated out of the most significant bit position.
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of the last instruction</param>
        /// <returns></returns>

        public void setFlagCF_Add(byte OldVal, byte NewVal, byte Source)
        {
            if (NewVal < OldVal)
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_Add(UInt16 OldVal, UInt16 NewVal, UInt16 Source)
        {
            if (NewVal < OldVal)
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_Add(UInt32 OldVal, UInt32 NewVal, UInt32 Source)
        {
            if (NewVal < OldVal)
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_Add(UInt64 OldVal, UInt64 NewVal, UInt64 Source)
        {
            if (NewVal < OldVal)
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_ADC(byte OldVal, byte NewVal, byte Source)
        {
            if ((NewVal < OldVal) || (FLAGSB.CF && (NewVal == OldVal)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_ADC(UInt16 OldVal, UInt16 NewVal, UInt16 Source)
        {
            if ((NewVal < OldVal) || (FLAGSB.CF && (NewVal == OldVal)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_ADC(UInt32 OldVal, UInt32 NewVal, UInt32 Source)
        {
            if ((NewVal < OldVal) || (FLAGSB.CF && (NewVal == OldVal)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_ADC(UInt64 OldVal, UInt64 NewVal, UInt64 Source)
        {
            if ((NewVal < OldVal) || (FLAGSB.CF && (NewVal == OldVal)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        //(lf_var1b < lf_resb) || (lflags.oldcf && (lf_var2b==0xff))
        public void setFlagCF_SBB(byte OldVal, byte NewVal, byte Source)
        {
            if ((OldVal < NewVal) || (FLAGSB.CF && (Source == 0xff)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SBB(UInt16 OldVal, UInt16 NewVal, UInt16 Source)
        {
            if ((OldVal < NewVal) || (FLAGSB.CF && (Source == 0xffff)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SBB(UInt32 OldVal, UInt32 NewVal, UInt32 Source)
        {
            if ((OldVal < NewVal) || (FLAGSB.CF && (Source == 0xffffffff)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SBB(UInt64 OldVal, UInt64 NewVal, UInt64 Source)
        {
            if ((OldVal < NewVal) || (FLAGSB.CF && (Source == 0xffffffffffffffff)))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }

        public void setFlagCF_SUB_CMP(byte OldVal, byte NewVal, byte Source)
        {
            if ((OldVal < NewVal) || (OldVal == NewVal && Source > 0))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SUB_CMP(UInt16 OldVal, UInt16 NewVal, UInt16 Source)
        {
            if ((OldVal < NewVal) || (OldVal == NewVal && Source > 0))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SUB_CMP(UInt32 OldVal, UInt32 NewVal, UInt32 Source)
        {
            if ((OldVal < NewVal) || (OldVal == NewVal && Source > 0))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF_SUB_CMP(UInt64 OldVal, UInt64 NewVal, UInt64 Source)
        {
            if ((OldVal < NewVal) || (OldVal == NewVal && Source > 0))
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagCF(bool Value)
        {
           //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.CF, Value);
            if (Value)
                FLAGS |= 0x1;
            else
                FLAGS &= 0xFFFE;
            mProc.regs.FLAGSB.mCFDirty = true;
        }
        public void setFlagIF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.IF, Value);
            if (Value)
                FLAGS |= 0x200;
            else
                FLAGS &= 0xFDFF;
            mProc.regs.FLAGSB.mIFDirty = true;
        }
        public void setFlagTF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.TF, Value);
            if (Value)
                FLAGS |= 0x100;
            else
                FLAGS &= 0xFEFF;
            mProc.regs.FLAGSB.mTFDirty = true;
        }
        public void setFlagDF(bool Value)
        {
            //FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.DF, Value);
            if (Value)
                FLAGS |= 0x400;
            else
                FLAGS &= 0xFBFF;
            mProc.regs.FLAGSB.mDFDirty = true;

        }
        public void setFlagRF(bool Value)
        {
            if (Value)
                EFLAGS |= 0x10000;
            else
                EFLAGS &= 0xFFFEFFFF;
            mProc.regs.FLAGSB.mRFDirty = true;

        }
        public void setFlagAC(bool Value)
        {
            if (Value)
                EFLAGS |= 0x40000;
            else
                EFLAGS &= 0xFFFBFFFF;
            mProc.regs.FLAGSB.mACDirty = true;
        }
        public void setFlagID(bool Value)
        {
            if (Value)
                EFLAGS |= 0x200000;
            else
                EFLAGS &= 0xFFDFFFFF;
        }
        public void setFlagIOPL(UInt16 Value)
        {
            FLAGS |= (UInt16)((Value & 3000));
        }
        public void setFlagVM(bool Value)
        {
            if (Value)
                EFLAGS |= 0x20000;
            else
                EFLAGS &= 0xFFFDFFFF;
            mProc.regs.FLAGSB.mVMDirty = true;
        }
        public void setFlagVIF(bool Value)
        {
            if (Value)
                EFLAGS |= 0x80000;
            else
                EFLAGS &= 0xFFF7FFFF;
            mProc.regs.FLAGSB.mVIFDirty= true;
        }
        public void setFlagVIP(bool Value)
        {
            if (Value)
                EFLAGS |= 0x100000;
            else
                EFLAGS &= 0xFFEFFFFF;
        }
        public void setFlagNT(bool Value)
        {
            if (Value)
                EFLAGS |= 0x4000;
            else
                EFLAGS &= 0xFFFFBFFF;
            mProc.regs.FLAGSB.mNTDirty = true;
        }

       #endregion

    }

}