using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace VirtualProcessor
{
    [StructLayout(LayoutKind.Explicit)]
    public struct sOpVal
    {
        [FieldOffset(0)]
        public byte OpByte;
        [FieldOffset(0)]
        public UInt16 OpWord;
        [FieldOffset(0)]
        public UInt32 OpDWord;
        [FieldOffset(0)]
        public UInt64 OpQWord;

        public sOpVal(UInt64 Value)
        {
            OpByte = 0;
            OpWord = 0;
            OpDWord = 0;
            OpQWord = Value;
        }
    }

    public class Instruct
    {
        #region Variables & Definitions
        internal string mName;
        public string Name { get { return mName; } }
        protected string mDescription;
        internal string Description { get { return mDescription; } }
        internal cOpCodeList mOpCodes = new cOpCodeList();

        internal Processor_80x86 mProc;
        public cValue Op1Addr, Op2Addr, Op3Addr;
        public cValue Op1Val, Op2Val, Op3Val;
        public string Operand1SValue, Operand2SValue, Operand3SValue;
        public UInt64 UsageCount = 0;
        public Double TotalTimeInInstruct = 0;
        internal DateTime mLastStart;
        //From the decoder, set by the mProcessor just before calling us to execute
        public sInstruction DecodedInstruction;
        //Ref booleans are set during decoding
        public bool Operand1IsRef, Operand2IsRef, Operand3IsRef, REPAble=false;
        public sOpCode ChosenOpCode;
        #region Valid Processors for instruction
        protected bool mProc8086 = false, mProc8088 = false, mProc80186 = false, mProc80286 = false,
           mProc80386 = false, mProc80486 = false, mProcPentium = false, mProcPentiumPro = false;
        internal bool Proc8086 { get { return mProc8086; } }
        internal bool Proc8088 { get { return mProc8088; } }
        internal bool Proc80186 { get { return mProc80186; } }
        internal bool Proc80286 { get { return mProc80286; } }
        internal bool Proc80386 { get { return mProc80386; } }
        internal bool Proc80486 { get { return mProc80486; } }
        internal bool ProcPentium { get { return mProcPentium; } }
        internal bool ProcPentiumPro { get { return mProcPentiumPro; } }
        #endregion
        protected eFLAGS mModFlags;
        internal eFLAGS FlagsMod { get { return mModFlags; } }
        public TypeCode Op1TypeCode, Op2TypeCode, Op3TypeCode;
        public sOpVal Op1Value, Op2Value, Op3Value;
        public DWord Op1Add, Op2Add, Op3Add;
        internal bool mInternalInstructionCall = false, mOverride1 = false, mOverride2 = false;
        public bool FPUInstruction = false, PageFaultException = false;
        internal bool lOpSize16, lAddrSize16, lSetupOnce = false;

        #region ResolveOp Variables
        sOpCodeAddressingMethod OcAM = sOpCodeAddressingMethod.None;
        sOpCodeOperandType OcOT = sOpCodeOperandType.None;
        sOperand Operand = new sOperand(); //Don't want to initialize it but the compiler won't let me not init it
        eGeneralRegister ChosenOpCode_Register = eGeneralRegister.NONE;
        bool HasImmediateData = false, OpHasEffective = false;
        #endregion

        #endregion

        public Instruct() { }

        internal void SetupExecution(Processor_80x86 Proc)
        {
            mProc = Proc;

            mInternalInstructionCall = false;
            if (!lSetupOnce)
                lOpSize16 = mProc.OpSize16;

            if (mProc.mCurrInstructOpMode != ProcessorMode.Protected)
            {   //Real or Virtual-8086
                bool lTempOpSize32 = false;
                bool lTempAddrSize32 = false;

                if (DecodedInstruction.OpSizePrefixFound)
                    lTempOpSize32 = !lTempOpSize32;
                lOpSize16 = !lTempOpSize32;

                if (DecodedInstruction.AddrSizePrefixFound)
                    lTempAddrSize32 = !lTempAddrSize32;
                lAddrSize16 = !lTempAddrSize32;
                //Already Set Once above
                //lOpSize16 = mProc.OpSize16;
                lAddrSize16 = lOpSize16;
            }
            else
            {
                switch (mProc.mCurrentInstruction.DecodedInstruction.OverrideSegment)
                {
                    case eGeneralRegister.CS:
                        if (mProc.regs.CS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.CS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.CS.Selector.granularity.OpSize32;

                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    case eGeneralRegister.DS:
                        if (mProc.regs.DS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.DS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.DS.Selector.granularity.OpSize32;

//                            if (DecodedInstruction.OpSizePrefixFound)
//                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

//                            if (DecodedInstruction.AddrSizePrefixFound)
//                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;
                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    case eGeneralRegister.ES:
                        if (mProc.regs.ES.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.ES.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.ES.Selector.granularity.OpSize32;

                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    case eGeneralRegister.FS:
                        if (mProc.regs.FS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.FS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.FS.Selector.granularity.OpSize32;

                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    case eGeneralRegister.GS:
                        if (mProc.regs.GS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.GS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.GS.Selector.granularity.OpSize32;
                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    case eGeneralRegister.SS:
                        if (mProc.regs.SS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.SS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.SS.Selector.granularity.OpSize32;

                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                    default:
                        if (mProc.regs.CS.mDescriptorNum != 0)
                        {
                            bool lTempOpSize32 = mProc.regs.CS.Selector.granularity.OpSize32;
                            bool lTempAddrSize32 = mProc.regs.CS.Selector.granularity.OpSize32;

                            //                            if (DecodedInstruction.OpSizePrefixFound)
                            //                                lTempOpSize32 = !lTempOpSize32;
                            lOpSize16 = !lTempOpSize32;

                            //                            if (DecodedInstruction.AddrSizePrefixFound)
                            //                                lTempAddrSize32 = !lTempAddrSize32;
                            lAddrSize16 = !lTempAddrSize32;

                        }
                        else
                        {
                            //Already Set Once above
                            //lOpSize16 = mProc.OpSize16;
                            lAddrSize16 = lOpSize16;
                        }
                        break;
                }
            }

            if (ChosenOpCode.Op1AM != sOpCodeAddressingMethod.None)
            {
                ResolveOp2(1, ref Op1Add, ref Op1Value.OpByte, ref Op1Value.OpWord, ref Op1Value.OpDWord, ref Op1Value.OpQWord, ref Op1TypeCode, ref Operand1SValue);
                if (!PageFaultException && (Op1Add < Processor_80x86.REGADDRBASE) && (mProc.regs.CR0 & 0x80000000) == 0x80000000 && (DecodedInstruction.OpCode != 0x8d) && (!ChosenOpCode.ImmedOp1) && (DecodedInstruction.Op1.Register == eGeneralRegister.NONE))
                    if (mProc.mem.PageAccessWillCausePF(mProc, Op1Add, false))
                    {
                        PageFaultException = true;
                        return;
                    }
            }
            else
            {
                Op1Add = 0;
                Op1Value.OpQWord = 0;
            }
            if (ChosenOpCode.Op2AM != sOpCodeAddressingMethod.None && !PageFaultException)
            {
                ResolveOp2(2, ref Op2Add, ref Op2Value.OpByte, ref Op2Value.OpWord, ref Op2Value.OpDWord, ref Op2Value.OpQWord, ref Op2TypeCode, ref Operand2SValue);
                if (!PageFaultException && (Op2Add < Processor_80x86.REGADDRBASE) &&  (mProc.regs.CR0 & 0x80000000) == 0x80000000 && (DecodedInstruction.OpCode != 0x8d) && (!ChosenOpCode.ImmedOp2) && (DecodedInstruction.Op2.Register == eGeneralRegister.NONE))
                    if (mProc.mem.PageAccessWillCausePF(mProc, Op2Add, false))
                    {
                        PageFaultException = true;
                        return;
                    }
            }
            else
            {
                Op2Add = 0;
                Op2Value.OpQWord = 0;
            }
            if (ChosenOpCode.Op3AM != sOpCodeAddressingMethod.None && !PageFaultException)
            {
                ResolveOp2(3, ref Op3Add, ref Op3Value.OpByte, ref Op3Value.OpWord, ref Op3Value.OpDWord, ref Op3Value.OpQWord, ref Op3TypeCode, ref Operand3SValue);
                if (!PageFaultException && (Op3Add < Processor_80x86.REGADDRBASE) && (mProc.regs.CR0 & 0x80000000) == 0x80000000 && (DecodedInstruction.OpCode != 0x8d) && (!ChosenOpCode.ImmedOp3) && (DecodedInstruction.Op3.Register == eGeneralRegister.NONE))
                    if (mProc.mem.PageAccessWillCausePF(mProc, Op3Add, false))
                    {
                        PageFaultException = true;
                        return;
                    }
            }
            else
            {
                Op3Add = 0;
                Op3Value.OpQWord = 0;
            }
        }

        internal bool valid = true;


        protected void ResolveOp2(int OpNum, ref DWord OpAdd, ref byte OpByte, ref Word OpWord, ref DWord OpDWord, ref QWord OpQWord, ref TypeCode OpTypeCode, ref String OpVals)
        {
            HasImmediateData = false;

            OpAdd = 0;
            OpQWord = 0;
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals = "";
#endif
            #region Populate variables based on OpNum
            switch (OpNum)
            {
                case 1:
                    OcAM = ChosenOpCode.Op1AM;
                    OcOT = ChosenOpCode.Op1OT;
                    OpHasEffective = DecodedInstruction.Op1.HasEffective;
                    Operand = DecodedInstruction.Op1;
                    ChosenOpCode_Register = ChosenOpCode.Register1;
                    HasImmediateData = ChosenOpCode.ImmedOp1;
                    Operand1IsRef = !HasImmediateData;
                    if (OcAM == sOpCodeAddressingMethod.OpOffset)
                        Operand1IsRef = true;
                    break;
                case 2:
                    OcAM = ChosenOpCode.Op2AM;
                    OcOT = ChosenOpCode.Op2OT;
                    OpHasEffective = DecodedInstruction.Op2.HasEffective;
                    Operand = DecodedInstruction.Op2;
                    ChosenOpCode_Register = ChosenOpCode.Register2;
                    HasImmediateData = ChosenOpCode.ImmedOp2;
                    Operand2IsRef = !HasImmediateData;
                    if (OcAM == sOpCodeAddressingMethod.OpOffset)
                        Operand2IsRef = true;
                    break;
                case 3:
                    OcAM = ChosenOpCode.Op3AM;
                    OcOT = ChosenOpCode.Op3OT;
                    OpHasEffective = DecodedInstruction.Op3.HasEffective;
                    Operand = DecodedInstruction.Op3;
                    ChosenOpCode_Register = ChosenOpCode.Register3;
                    HasImmediateData = ChosenOpCode.ImmedOp3;
                    Operand3IsRef = !HasImmediateData;
                    if (OcAM == sOpCodeAddressingMethod.OpOffset)
                        Operand3IsRef = true;
                    break;
            }
            #endregion
            #region Fix up OpType
            if (OcOT == sOpCodeOperandType.ByteOrWord)
                if (mProc.OpSize16)
                    OcOT = sOpCodeOperandType.Byte;
                else
                    OcOT = sOpCodeOperandType.Word;
            else if (OcOT == sOpCodeOperandType.WordOrDWord)
                if (mProc.OpSize16)
                    OcOT = sOpCodeOperandType.Word;
                else
                    OcOT = sOpCodeOperandType.DWord;
            else if (OcOT == sOpCodeOperandType.Pointer)
                if (mProc.OpSize16)
                    OcOT = sOpCodeOperandType.DWord;
                else
                    OcOT = sOpCodeOperandType.QWord;

            #endregion

            #region OppOffset
            if (OcAM == sOpCodeAddressingMethod.OpOffset)
            {
                if (Operand.HasImm8)
                {
                    OpAdd = Operand.Imm8;
                }
                else if (Operand.HasImm16)
                {
                    OpAdd = Operand.Imm16;
                }
                else if (Operand.HasImm32)
                {
                    OpAdd = Operand.Imm32;
                }
                else
                    throw new Exception("Where's the data?");
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                {
                    if (Operand.HasImm16)
                        OpVals = "[" + OpAdd.ToString("X4") + "]";
                    else
                        OpVals = "[" + OpAdd.ToString("X8") + "]";
                }
#endif
                if (DecodedInstruction.OverrideSegment != eGeneralRegister.NONE)
                {
                    if (mProc.ProtectedModeActive)
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(DecodedInstruction.OverrideSegment);
                    }
                    else
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(DecodedInstruction.OverrideSegment) << 4;
                    }
                }
                else
                {
                    if (mProc.ProtectedModeActive)
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(eGeneralRegister.DS);
                    }
                    else
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(eGeneralRegister.DS) << 4;
                    }
                }
                #region Get Value for Address by OpType
                if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 && mProc.mem.PageAccessWillCausePF(mProc, OpAdd, false))
                    PageFaultException = true;
                else
                    switch (OcOT)
                    {
                        case sOpCodeOperandType.Byte: OpByte = mProc.mem.GetByte(mProc, OpAdd); OpTypeCode = TypeCode.Byte; break;
                        case sOpCodeOperandType.Word: OpWord = mProc.mem.GetWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt16; break;
                        case sOpCodeOperandType.DWord: OpDWord = mProc.mem.GetDWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt32; break;
                        case sOpCodeOperandType.QWord: OpQWord = mProc.mem.GetQWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt64; break;
                        case sOpCodeOperandType.Pointer:
                        case sOpCodeOperandType.PseudoDesc:
                            OpQWord = mProc.mem.GetQWord(mProc, OpAdd) & 0xFFFFFFFFFFFF; OpTypeCode = TypeCode.UInt64; break;
                    }
                #endregion
                return;
            }
            #endregion
            #region HasImmediateData
            else if (HasImmediateData)
            {
                if (Operand.HasImm8)
                {
                    OpByte = Operand.Imm8; OpTypeCode = TypeCode.Byte;
                }
                else if (Operand.HasImm16)
                {
                    OpWord = Operand.Imm16; OpTypeCode = TypeCode.UInt16;
                }
                else if (Operand.HasImm32)
                {
                    OpDWord = Operand.Imm32; OpTypeCode = TypeCode.UInt32;
                }
                else if (Operand.HasImm64)
                {
                    OpQWord = Operand.Imm64; OpTypeCode = TypeCode.UInt64;
                }
                else
                    throw new Exception("ResolveOp: Has immediate data but it isn't filled in!");
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                {
                    if (OcAM == sOpCodeAddressingMethod.JmpRelOffset)
                    {
                        UInt16 lTemp = mProc.regs.IP;

                        if (Operand.HasImm8)
                        {
                            byte lTempB = OpByte;
                            if (Misc.IsNegative(lTempB))
                            {
                                lTempB = Misc.Negate(lTempB);
                                lTemp -= lTempB;
                            }
                            else
                                lTemp += lTempB;
                        }
                        else
                            lTemp += OpWord;

                        lTemp += DecodedInstruction.BytesUsed;
                        OpVals = lTemp.ToString("X4");
                    }
                    else
                    {
                        if (Operand.HasImm8)
                            OpVals = OpByte.ToString("X2");
                        else if (Operand.HasImm16)
                            OpVals = OpWord.ToString("X4");
                        else if (Operand.HasImm32)
                            OpVals = OpDWord.ToString("X8");
                        else if (Operand.HasImm64)
                            OpVals = OpQWord.ToString("X16");
                    }
                }
#endif
                return;
            }
            #endregion
            #region Register in operand filled
            else if (Operand.Register != eGeneralRegister.NONE)
            {
                OpAdd = mProc.GetRegAddrForRegEnum(Operand.Register);
                if ((int)Operand.Register <= 0x08)
                {
                    OpByte = mProc.GetByteRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.Byte;
                }
                else if ((int)Operand.Register <= 0x18)
                {
                    OpWord = mProc.GetWordRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.UInt16;
                }
                else if ((int)Operand.Register <= 0xF000)
                {
                    OpDWord = mProc.GetDWordRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.UInt32;
                }
                else
                    throw new Exception("Huh?");
                //OpAdd = mProc.GetRegAddrForRegEnum(Operand.Register);

#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals = Operand.Register.ToString();
#endif
                return;
            }
            #endregion
            #region OpHasEffective
            else if (OpHasEffective)
            {
                UInt16 lTempAddr16 = 0;
                UInt32 lTempAddr32 = 0;
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals += "[";
#endif

                if (Operand.EffReg1 != eGeneralRegister.NONE)
                {
                    if (lAddrSize16)
                    {
                        if ((int)Operand.EffReg1 <= 0x08)
                        {
                            lTempAddr16 += mProc.GetByteRegValueForRegEnum(Operand.EffReg1);
                        }
                        else if ((int)Operand.EffReg1 <= 0x18)
                        {
                            lTempAddr16 += mProc.GetWordRegValueForRegEnum(Operand.EffReg1);
                        }
                        else if ((int)Operand.EffReg1 <= 0xF000)
                        {
                            //Only getting the bottom word of the dword register
                            lTempAddr16 += (Word)mProc.GetDWordRegValueForRegEnum(Operand.EffReg1);
                            //throw new ExceptionNumber("32 big register to 16 bit address?");
                        }
                    }
                    else
                    {
                        if ((int)Operand.EffReg1 <= 0x08)
                        {
                            lTempAddr32 += mProc.GetByteRegValueForRegEnum(Operand.EffReg1);
                        }
                        else if ((int)Operand.EffReg1 <= 0x18)
                        {
                            lTempAddr32 += mProc.GetWordRegValueForRegEnum(Operand.EffReg1);
                        }
                        else if ((int)Operand.EffReg1 <= 0xF000)
                        {
                            lTempAddr32 += mProc.GetDWordRegValueForRegEnum(Operand.EffReg1);
                        }

                    }
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals += Operand.EffReg1.ToString();
#endif
                }
                if (Operand.SIBMultiplier > 0)
                {
                    if (lAddrSize16)
                        lTempAddr16 *= (Operand.SIBMultiplier);
                    else
                        lTempAddr32 *= Operand.SIBMultiplier;
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals += "*" + Operand.SIBMultiplier.ToString("X1");
#endif
                }
                if (Operand.EffReg2 != eGeneralRegister.NONE)
                {
                    if (lAddrSize16)
                        lTempAddr16 += (UInt16)(mProc.GetRegValueForRegEnum(Operand.EffReg2) & 0xFFFF);
                    else
                        lTempAddr32 += mProc.GetRegValueForRegEnum(Operand.EffReg2);
#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                    OpVals += "+" + Operand.EffReg2.ToString();
#endif
                }
                if (Operand.HasDisp8)
                {
                    if (Operand.DispIsNegative)
                        if (lAddrSize16)
                            lTempAddr16 -= Operand.Disp8;
                        else
                            lTempAddr32 -= Operand.Disp8;
                    else
                        if (lAddrSize16)
                            lTempAddr16 += Operand.Disp8;
                        else
                            lTempAddr32 += Operand.Disp8;
                }
                else if (Operand.HasDisp16)
                    if (lAddrSize16)
                        lTempAddr16 += Operand.Disp16;
                    else
                        lTempAddr32 += Operand.Disp16;
                else if (Operand.HasDisp32)
                    if (lAddrSize16)
                        if (Operand.DispIsNegative)
                            lTempAddr16 -= (Word)Operand.Disp32;
                        else
                            lTempAddr16 += (Word)Operand.Disp32;
                    else
                        if (Operand.DispIsNegative)
                            lTempAddr32 += Operand.Disp32;
                        else
                            lTempAddr32 += Operand.Disp32;

#if DECODE_MAKE_STRINGS
                if (mProc.mGenerateDecodeStrings)
                {
                    if (Operand.HasDisp8 || Operand.HasDisp16 || Operand.HasDisp32)
                    {
                        string lTemp = "";
                        if (OpVals != "[")
                            lTemp = "+";
                        if (Operand.HasDisp8)
                        {
                            if (Operand.DispIsNegative)
                            {
                                if (lTemp == "")
                                    OpVals += Operand.Disp8.ToString("X2");
                                else
                                    OpVals += "-" + Operand.Disp8.ToString("X2");
                            }
                            else
                                OpVals += lTemp + Operand.Disp8.ToString("X2");
                        }
                        else if (Operand.HasDisp16)
                            OpVals += lTemp + Operand.Disp16.ToString("X4");
                        else if (Operand.HasDisp32)
                            OpVals += lTemp + Operand.Disp32.ToString("X8");

                    }
                    OpVals += "]";
                }
#endif
                if (DecodedInstruction.OverrideSegment != eGeneralRegister.NONE && DecodedInstruction.RealOpCode != 0x8d)
                    OpAdd = mProc.GetDWordRegValueForRegEnum(DecodedInstruction.OverrideSegment);
                //if (mProc.OperatingMode != ProcessorMode.Protected)
                if (mProc.ProtectedModeActive & mProc.regs.CS.DescriptorNum > 0)
                { }
                else
                    OpAdd = OpAdd << 4;
                if (lAddrSize16)
                    OpAdd += lTempAddr16;
                else
                    OpAdd += lTempAddr32;

                //The instructions LEA, LDS, LES, LSS don't use the value at the memory location, but rather the memory location itself, as their value
                //We only care about this in debug mode as we'll see the value for the operand, so skip this logic if not debugging
                switch (DecodedInstruction.RealOpCode)
                {
                    case 0x8d: OpDWord = OpAdd; break;
                    default:
                        if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 && mProc.mem.PageAccessWillCausePF(mProc, OpAdd, false))
                            PageFaultException = true;
                        else
                            switch (OcOT)
                            {
                                case sOpCodeOperandType.Byte: OpByte = mProc.mem.GetByte(mProc, OpAdd); OpTypeCode = TypeCode.Byte; break;
                                case sOpCodeOperandType.Word: OpWord = mProc.mem.GetWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt16; break;
                                case sOpCodeOperandType.DWord: OpDWord = mProc.mem.GetDWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt32; break;
                                case sOpCodeOperandType.QWord: OpQWord = mProc.mem.GetQWord(mProc, OpAdd); OpTypeCode = TypeCode.UInt64; break;
                                case sOpCodeOperandType.Pointer:
                                case sOpCodeOperandType.PseudoDesc:
                                    OpQWord = mProc.mem.GetQWord(mProc, OpAdd) & 0xFFFFFFFFFFFF; OpTypeCode = TypeCode.UInt64; break;
                            }
                        break;
                }
                return;
            #endregion
            }
            throw new Exception("ResolveOp: If you got here, you missed something!");
        }
        protected void UpdIPForShortJump(sOpVal Offset, TypeCode OpType)
        {

            switch (OpType)
            {
                case TypeCode.Byte:
                    if ((Offset.OpByte & 0x80) == 0x80)
                    {
                        Offset.OpByte = (byte)((~Offset.OpByte) + 1);
                        mProc.regs.EIP -= Offset.OpByte;
                    }
                    else
                        mProc.regs.EIP += Offset.OpByte;
                    return;
                case TypeCode.UInt16:
                    if ((Offset.OpWord & 0x8000) == 0x8000)
                    {
                        Offset.OpWord = (Word)((~Offset.OpWord) + 1);
                        mProc.regs.EIP -= Offset.OpWord;
                    }
                    else
                        mProc.regs.EIP += Offset.OpWord;
                    return;
                case TypeCode.UInt32:
                    if ((Offset.OpDWord & 0x80000000) == 0x80000000)
                    {
                        Offset.OpDWord = (DWord)((~Offset.OpDWord) + 1);
                        mProc.regs.EIP -= Offset.OpDWord;
                    }
                    else
                        mProc.regs.EIP += Offset.OpWord;
                    return;
                default:
                    throw new Exception("Shouldn't get here");
                    break;
            }
        }
        internal static bool UpdateForNegativeAll(ref Processor_80x86 mProc, ref sOpVal Value, TypeCode OpType)
        {
            if (OpType == TypeCode.UInt64)
                return false;

            {
                switch (OpType)
                {
                    case TypeCode.Byte:
                        if ((Value.OpByte & 0x80) == 0x80)
                        {
                            Value.OpByte = (byte)(~Value.OpByte + 1);
                            return true;
                        }
                        break;
                    case TypeCode.UInt16:
                        if ((Value.OpWord & 0x8000) == 0x8000)
                        {
                            Value.OpWord = (Word)(~Value.OpWord + 1);
                            return true;
                        }
                        break;
                    case TypeCode.UInt32:
                        if ((Value.OpDWord & 0x80000000) == 0x80000000)
                        {
                            Value.OpDWord = (DWord)(~Value.OpDWord + 1);
                            return true;
                        }
                        break;
                    default:
                        throw new Exception("D'oh!");
                        break;
                }
            }
            return false;
        }
        /*        public virtual void Impl(ref Processor_80x86 mProc) { throw new ExceptionNumber("Need instruction '" + mProc.mCurrentOperation.OpCode.Name + "'override for this signature"); }
*/
        public virtual void Impl()
        {
        }
        //Turn a reference into a value.
        internal UInt32 GetSegOverriddenAddress(Processor_80x86 mProc, UInt32 Addr)
        { 
            if ( (Addr >= Processor_80x86.REGADDRBASE) || (mProc.mSegmentOverride == 0))
                return Addr;
            if (mProc.OperatingMode == ProcessorMode.Protected)
            {
                if (mProc.mSegmentOverride == Processor_80x86.RCS)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.CS)  + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RES)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.ES) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RSS)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.SS) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RFS)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.FS) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RGS)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.GS) + Addr;

                return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.DS) + Addr;
            }
            else
            {
                if (mProc.mSegmentOverride == Processor_80x86.RDS)
                    return (mProc.regs.DS.Value << 4) + Addr;
                if (mProc.mSegmentOverride == Processor_80x86.RCS)
                    return (mProc.regs.CS.Value << 4) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RES)
                    return (mProc.regs.ES.Value << 4) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RSS)
                    return (mProc.regs.SS.Value << 4) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RFS)
                    return (mProc.regs.FS.Value << 4) + Addr;
                else if (mProc.mSegmentOverride == Processor_80x86.RGS)
                    return (mProc.regs.GS.Value << 4) + Addr;

                return (mProc.regs.DS.Value << 4) + Addr;
            }
            throw new Exception("D'oh");
        }
        internal void SetFlagsForSubtraction(Processor_80x86 mProc, sOpVal PreVal1, sOpVal PreVal2, sOpVal Op1Val, TypeCode Op1Type, bool SetOFFlag)
        {
            mProc.regs.setFlagZF(Op1Val.OpQWord);
            mProc.regs.setFlagAF(PreVal1.OpQWord, Op1Val.OpQWord);
            mProc.regs.setFlagPF(Op1Val.OpQWord);

            switch (Op1Type)
            {
                case TypeCode.Byte:
                    mProc.regs.setFlagSF(Op1Val.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.regs.setFlagSF(Op1Val.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.regs.setFlagSF(Op1Val.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.regs.setFlagSF(Op1Val.OpQWord);
                    break;
            }
        }
        internal void SetFlagsForAddition(Processor_80x86 mProc, sOpVal PreVal1, sOpVal PreVal2, sOpVal Op1ValSigned, sOpVal Op1ValUnsigned, TypeCode Op1Type)
        {
            mProc.regs.setFlagZF(Op1ValSigned.OpQWord);
            mProc.regs.setFlagAF(Op1ValSigned.OpQWord, PreVal1.OpQWord);
            mProc.regs.setFlagPF(Op1ValSigned.OpQWord);

            switch (Op1Type)
            {
                case TypeCode.Byte:
                    mProc.regs.setFlagOF_Add(PreVal1.OpByte, PreVal2.OpByte, Op1ValSigned.OpByte);
                    mProc.regs.setFlagSF(Op1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.regs.setFlagOF_Add(PreVal1.OpWord, PreVal2.OpWord, Op1ValSigned.OpWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.regs.setFlagOF_Add(PreVal1.OpDWord, PreVal2.OpDWord, Op1ValSigned.OpDWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.regs.setFlagOF_Add(PreVal1.OpQWord, PreVal2.OpQWord, Op1ValSigned.OpQWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpQWord);
                    break;
            }


        }

        public Instruct CopyOf()
        {
            //Instruct wise;
            //wise = (Instruct)MemberwiseClone();
            //Instruct il;
            //il = (Instruct)Misc.CloneObjectWithIL(this);
            //return (Instruct)MemberwiseClone();
            return (Instruct)Misc.CloneObjectWithIL(this);
        }

    }
}
