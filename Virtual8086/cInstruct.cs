﻿using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

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

    public class Instruct : Object
    {
        #region Variables & Definitions
        public string mName;
        public string Name { get { return mName; } }
        public string mDescription;
        internal string Description { get { return mDescription; } }
        internal cOpCodeList mOpCodes = new cOpCodeList();
        public bool mFlowReturnsLater;

        public Processor_80x86 mProc; //TODO: Figure out how to make this interal again!
        public UInt64 UsageCount = 0;
        public Double TotalTimeInInstruct = 0;
        internal DateTime mLastStart;
        //From the decoder, set by the mProcessor just before calling us to execute
        //public sInstruction DecodedInstruction;
        //Ref booleans are set during decoding
        public bool REPAble = false;
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
        internal bool mNoSetMemoryToValue = false, mOverride1 = false, mOverride2 = false;
        public bool FPUInstruction = false;
        /// <summary>
        /// True if the current call to the INT instruction was generated by software, false if hardware or exception generated
        /// </summary>
        internal bool mIntIsSoftware;
        internal bool valid = true;
        sOperand Operand;

        #endregion

        public Instruct()
        {
        }

        public Instruct(Processor_80x86 pProc)
        {
            mProc = pProc;
        }

        public void SetupExecution(Processor_80x86 Proc, ref sInstruction CurrentDecode)
        {
            if (mProc == null)
                mProc = Proc;
            mIntIsSoftware = true;
            mNoSetMemoryToValue = false;

            //if (!lSetupOnce)
            CurrentDecode.lOpSize16 = mProc.mCurrInstructOpSize16;
            CurrentDecode.lAddrSize16 = mProc.mCurrInstructAddrSize16;

            if (CurrentDecode.ChosenOpCode.Op1AM != sOpCodeAddressingMethod.None)
            {
                ResolveOp2(1, ref CurrentDecode.Op1Add, ref CurrentDecode.Op1Value, ref CurrentDecode.Op1TypeCode, /*ref CurrentDecode.Operand1SValue,*/ ref CurrentDecode);
                if (CurrentDecode.ExceptionThrown)
                    return;
#if DEBUG
                CurrentDecode.Op1Operand = Operand;
                CurrentDecode.Op1Populated = true;
#endif
            }
            else
            {
                CurrentDecode.Op1Add = 0;
                CurrentDecode.Op1Value.OpQWord = 0;
                CurrentDecode.Op1Populated = false;
            }
            if (CurrentDecode.ChosenOpCode.Op2AM != sOpCodeAddressingMethod.None && !CurrentDecode.ExceptionThrown)
            {
                ResolveOp2(2, ref CurrentDecode.Op2Add, ref CurrentDecode.Op2Value, ref CurrentDecode.Op2TypeCode, /*ref CurrentDecode.Operand2SValue, */ref CurrentDecode);
                if (CurrentDecode.ExceptionThrown)
                    return;
                //CLR 03/28/2014 - doesn't make sense to ahve these but they may break CombinedSValue
#if DEBUG
                CurrentDecode.Op2Operand = Operand;
                CurrentDecode.Op2Populated = true;
#endif
            }
            else
            {
                CurrentDecode.Op2Add = 0;
                CurrentDecode.Op2Value.OpQWord = 0;
                CurrentDecode.Op2Populated = false;
            }
            if (CurrentDecode.ChosenOpCode.Op3AM != sOpCodeAddressingMethod.None && !CurrentDecode.ExceptionThrown)
            {
                ResolveOp2(3, ref CurrentDecode.Op3Add, ref CurrentDecode.Op3Value, ref CurrentDecode.Op3TypeCode, /*ref CurrentDecode.Operand3SValue, */ref CurrentDecode);
                if (CurrentDecode.ExceptionThrown)
                    return;
                //CLR 03/28/2014 - doesn't make sense to ahve these but they may break CombinedSValue
                //Op3Operand = Operand;

#if DEBUG
                CurrentDecode.Op3Populated = true;
                CurrentDecode.Op3Operand = Operand;
#endif
            }
            else
            {
                CurrentDecode.Op3Add = 0;
                CurrentDecode.Op3Value.OpQWord = 0;
                CurrentDecode.Op3Populated = false;
            }
#if DECODE_MAKE_STRINGS
            if (mProc.mGenerateDecodeStrings)
            {
                if (CurrentDecode.Op1Populated)
                    CurrentDecode.CombinedSValue = BuildOpString(1, CurrentDecode.Op1Operand, CurrentDecode.Op1Add, CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode, ref CurrentDecode);
                else
                    CurrentDecode.CombinedSValue = "";
                if (CurrentDecode.Op2Populated)
                    CurrentDecode.CombinedSValue += "," + BuildOpString(2, CurrentDecode.Op2Operand, CurrentDecode.Op2Add, CurrentDecode.Op2Value, CurrentDecode.Op2TypeCode, ref CurrentDecode);
                if (CurrentDecode.Op3Populated)
                    CurrentDecode.CombinedSValue += "," + BuildOpString(3, CurrentDecode.Op3Operand, CurrentDecode.Op3Add, CurrentDecode.Op3Value, CurrentDecode.Op3TypeCode, ref CurrentDecode);

            }
#endif

        }

        public string ToString(ref sInstruction CurrentDecode)
        {
            //return base.ToString();
            SetupExecution(mProc, ref CurrentDecode);
            CurrentDecode.CombinedSValue = BuildOpString(1, CurrentDecode.Op1Operand, CurrentDecode.Op1Add, CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode, ref CurrentDecode);
            if (CurrentDecode.Op2Populated)
                CurrentDecode.CombinedSValue += "," + BuildOpString(2, CurrentDecode.Op2Operand, CurrentDecode.Op2Add, CurrentDecode.Op2Value, CurrentDecode.Op2TypeCode, ref CurrentDecode);
            if (CurrentDecode.Op3Populated)
                CurrentDecode.CombinedSValue += "," + BuildOpString(3, CurrentDecode.Op3Operand, CurrentDecode.Op3Add, CurrentDecode.Op3Value, CurrentDecode.Op3TypeCode, ref CurrentDecode);
            CurrentDecode.CombinedSValue = this.Name + "\t" + CurrentDecode.CombinedSValue;
            return CurrentDecode.CombinedSValue.ToString();
        }

        protected string BuildOpString(int OpNum, sOperand Operand, DWord OpAdd, sOpVal Val, TypeCode OpTypeCode, ref sInstruction CurrentDecode)
        {
            StringBuilder lRetVal = new StringBuilder();
            bool OpHasEffective = false;
            sOpCodeAddressingMethod OcAM = sOpCodeAddressingMethod.None;
            bool HasImmediateData = false;

            switch (OpNum)
            {
                case 1:
                    OcAM = CurrentDecode.ChosenOpCode.Op1AM;
                    //OcOT = ChosenOpCode.Op1OT;
                    OpHasEffective = CurrentDecode.Op1.HasEffective;
                    //Operand = CurrentDecode.Op1;
                    //ChosenOpCode_Register = ChosenOpCode.Register1;
                    HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp1;
                    //Operand1IsRef = !HasImmediateData;
                    //if (OcAM == sOpCodeAddressingMethod.OpOffset)
                    //    Operand1IsRef = true;
                    break;
                case 2:
                    OcAM = CurrentDecode.ChosenOpCode.Op2AM;
                    //OcOT = ChosenOpCode.Op2OT;
                    OpHasEffective = CurrentDecode.Op2.HasEffective;
                    //Operand = CurrentDecode.Op2;
                    //ChosenOpCode_Register = ChosenOpCode.Register2;
                    HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp2;
                    //Operand2IsRef = !HasImmediateData;
                    //if (OcAM == sOpCodeAddressingMethod.OpOffset)
                    //    Operand2IsRef = true;
                    break;
                case 3:
                    OcAM = CurrentDecode.ChosenOpCode.Op3AM;
                    //OcOT = ChosenOpCode.Op3OT;
                    OpHasEffective = CurrentDecode.Op3.HasEffective;
                    //Operand = CurrentDecode.Op3;
                    //ChosenOpCode_Register = ChosenOpCode.Register3;
                    HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp3;
                    //Operand3IsRef = !HasImmediateData;
                    //if (OcAM == sOpCodeAddressingMethod.OpOffset)
                    //    Operand3IsRef = true;
                    break;
            }

            //OpOffset
            if (OcAM == sOpCodeAddressingMethod.OpOffset)
            {
                if (Operand.HasImm16)
                    lRetVal.AppendFormat("[{0}]", OpAdd.ToString("X4"));
                else
                    lRetVal.AppendFormat("[{0}]", OpAdd.ToString("X8"));
            }
            else if (HasImmediateData)
            {
                if (OcAM == sOpCodeAddressingMethod.JmpRelOffset)
                {
                    UInt16 lTemp = (UInt16)CurrentDecode.InstructionEIP;

                    if (Operand.HasImm8)
                    {
                        byte lTempB = Val.OpByte;
                        if (Misc.IsNegative(lTempB))
                        {
                            lTempB = Misc.Negate(lTempB);
                            lTemp -= lTempB;
                        }
                        else
                            lTemp += lTempB;
                    }
                    else
                        lTemp += Val.OpWord;

                    lTemp += CurrentDecode.BytesUsed;
                    lRetVal.Append(lTemp.ToString("X4"));
                }
                else
                {
                    if (Operand.HasImm8)
                        lRetVal.Append(Val.OpByte.ToString("X2"));
                    else if (Operand.HasImm16)
                        lRetVal.Append(Val.OpWord.ToString("X4"));
                    else if (Operand.HasImm32)
                        lRetVal.Append(Val.OpDWord.ToString("X8"));
                    else if (Operand.HasImm64)
                        lRetVal.Append(Val.OpQWord.ToString("X16"));
                }
            }
            //Register in operand filled
            else if (Operand.Register != eGeneralRegister.NONE)
            {
                lRetVal.Append(Operand.Register.ToString());
            }

            //OpHasEffective
            else if (OpHasEffective)
            {
                lRetVal.Append("[");
                if (Operand.EffReg1 != eGeneralRegister.NONE)
                    lRetVal.Append(Operand.EffReg1.ToString());
                if (Operand.SIBMultiplier > 0)
                    lRetVal.AppendFormat("*{0}", Operand.SIBMultiplier.ToString("X1"));
                if (Operand.EffReg2 != eGeneralRegister.NONE)
                    lRetVal.AppendFormat("+{0}", Operand.EffReg2.ToString());
                if (Operand.HasDisp8 || Operand.HasDisp16 || Operand.HasDisp32)
                {
                    string lTemp = "";
                    if (lRetVal.Length != 1)
                        lTemp = "+";
                    if (Operand.HasDisp8)
                    {
                        if (Operand.DispIsNegative)
                        {
                            if (lTemp.Length == 0)
                                lRetVal.Append(Operand.Disp8.ToString("X2"));
                            else
                                lRetVal.AppendFormat("-{0}", Operand.Disp8.ToString("X2"));
                        }
                        else
                            lRetVal.Append(lTemp + Operand.Disp8.ToString("X2"));
                    }
                    else if (Operand.HasDisp16)
                        lRetVal.Append(lTemp + Operand.Disp16.ToString("X4"));
                    else if (Operand.HasDisp32)
                        lRetVal.Append(lTemp + Operand.Disp32.ToString("X8"));
                }
                lRetVal.Append("]");
            }

            return lRetVal.ToString();
        }

        protected void ResolveOp2(int OpNum, ref DWord OpAdd, ref sOpVal Val, ref TypeCode OpTypeCode, /*ref String OpVals, */ref sInstruction CurrentDecode)
        {
            bool OpHasEffective;
            bool HasImmediateData;
            OpAdd = 0;
            Val.OpQWord = 0;

            sOpCodeAddressingMethod OcAM = sOpCodeAddressingMethod.None;
            sOpCodeOperandType OcOT = sOpCodeOperandType.None;
            #region Populate variables based on OpNum
            if (OpNum == 1)
            {
                OcAM = CurrentDecode.ChosenOpCode.Op1AM;
                OcOT = CurrentDecode.ChosenOpCode.Op1OT;
                OpHasEffective = CurrentDecode.Op1.HasEffective;
                Operand = CurrentDecode.Op1;
                HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp1;
                CurrentDecode.Operand1IsRef = !HasImmediateData;
                if (CurrentDecode.ChosenOpCode.Op1AM == sOpCodeAddressingMethod.OpOffset)
                    CurrentDecode.Operand1IsRef = true;
            }
            else if (OpNum == 2)
            {
                OcAM = CurrentDecode.ChosenOpCode.Op2AM;
                OcOT = CurrentDecode.ChosenOpCode.Op2OT;
                OpHasEffective = CurrentDecode.Op2.HasEffective;
                Operand = CurrentDecode.Op2;
                HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp2;
                CurrentDecode.Operand2IsRef = !HasImmediateData;
                if (OcAM == sOpCodeAddressingMethod.OpOffset)
                    CurrentDecode.Operand2IsRef = true;
            }
            else
            {
                OcAM = CurrentDecode.ChosenOpCode.Op3AM;
                OcOT = CurrentDecode.ChosenOpCode.Op3OT;
                OpHasEffective = CurrentDecode.Op3.HasEffective;
                Operand = CurrentDecode.Op3;
                HasImmediateData = CurrentDecode.ChosenOpCode.ImmedOp3;
                CurrentDecode.Operand3IsRef = !HasImmediateData;
                if (OcAM == sOpCodeAddressingMethod.OpOffset)
                    CurrentDecode.Operand3IsRef = true;
            }
            #endregion
            #region Fix up OpType
            if (OcOT == sOpCodeOperandType.ByteOrWord)
                if (mProc.mCurrInstructOpSize16)
                    OcOT = sOpCodeOperandType.Byte;
                else
                    OcOT = sOpCodeOperandType.Word;
            else if (OcOT == sOpCodeOperandType.WordOrDWord)
                if (mProc.mCurrInstructOpSize16)
                    OcOT = sOpCodeOperandType.Word;
                else
                    OcOT = sOpCodeOperandType.DWord;
            else if (OcOT == sOpCodeOperandType.Pointer)
                if (mProc.mCurrInstructOpSize16)
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

                if (CurrentDecode.OverrideSegment != eGeneralRegister.NONE)
                {
                    if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(CurrentDecode.OverrideSegment);
                    }
                    else
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(CurrentDecode.OverrideSegment) << 4;
                    }
                }
                else
                {
                    if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(eGeneralRegister.DS);
                    }
                    else
                    {
                        OpAdd += mProc.GetDWordRegValueForRegEnum(eGeneralRegister.DS) << 4;
                    }
                }
                #region Get Value for Address by OpType
                if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 && PhysicalMem.PageAccessWillCausePF(mProc, ref CurrentDecode, OpAdd, false))
                {
                    return;
                }
                else
                    switch (OcOT)
                    {
                        case sOpCodeOperandType.Byte: Val.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.Byte; break;
                        case sOpCodeOperandType.Word: Val.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt16; break;
                        case sOpCodeOperandType.DWord: Val.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt32; break;
                        case sOpCodeOperandType.QWord: Val.OpQWord = mProc.mem.GetQWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt64; break;
                        case sOpCodeOperandType.Pointer:
                        case sOpCodeOperandType.PseudoDesc:
                            Val.OpQWord = mProc.mem.GetQWord(mProc, ref CurrentDecode, OpAdd) & 0xFFFFFFFFFFFF; OpTypeCode = TypeCode.UInt64; break;
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
                    Val.OpByte = Operand.Imm8; OpTypeCode = TypeCode.Byte;
                }
                else if (Operand.HasImm16)
                {
                    Val.OpWord = Operand.Imm16; OpTypeCode = TypeCode.UInt16;
                }
                else if (Operand.HasImm32)
                {
                    Val.OpDWord = Operand.Imm32; OpTypeCode = TypeCode.UInt32;
                }
                else if (Operand.HasImm64)
                {
                    Val.OpQWord = Operand.Imm64; OpTypeCode = TypeCode.UInt64;
                }
                else
                    throw new Exception("ResolveOp: Has immediate data but it isn't filled in!");
                return;
            }
            #endregion
            #region Register in operand filled
            else if (Operand.Register != eGeneralRegister.NONE)
            {
                OpAdd = Processor_80x86.GetRegAddrForRegEnum(Operand.Register);
                if ((int)Operand.Register <= 0x08)
                {
                    Val.OpByte = mProc.GetByteRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.Byte;
                }
                else if ((int)Operand.Register <= 0x18)
                {
                    Val.OpWord = mProc.GetWordRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.UInt16;
                }
                else if ((int)Operand.Register <= 0xF000)
                {
                    Val.OpDWord = mProc.GetDWordRegValueForRegEnum(Operand.Register);
                    OpTypeCode = TypeCode.UInt32;
                }
                else
                    throw new Exception("Huh?");
                //OpAdd = mProc.GetRegAddrForRegEnum(Operand.Register);
                return;
            }
            #endregion
            #region OpHasEffective
            else if (OpHasEffective)
            {
                UInt16 lTempAddr16 = 0;
                UInt32 lTempAddr32 = 0;

                if (Operand.EffReg1 != eGeneralRegister.NONE)
                {
                    if (CurrentDecode.lAddrSize16)
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
                }
                if (Operand.SIBMultiplier > 0)
                {
                    if (CurrentDecode.lAddrSize16)
                        lTempAddr16 *= (Operand.SIBMultiplier);
                    else
                        lTempAddr32 *= Operand.SIBMultiplier;
                }
                if (Operand.EffReg2 != eGeneralRegister.NONE)
                {
                    if (CurrentDecode.lAddrSize16)
                        lTempAddr16 += (UInt16)(mProc.GetRegValueForRegEnum(Operand.EffReg2) & 0xFFFF);
                    else
                        lTempAddr32 += mProc.GetRegValueForRegEnum(Operand.EffReg2);
                }
                if (Operand.HasDisp8)
                {
                    if (Operand.DispIsNegative)
                        if (CurrentDecode.lAddrSize16)
                            lTempAddr16 -= Operand.Disp8;
                        else
                            lTempAddr32 -= Operand.Disp8;
                    else
                        if (CurrentDecode.lAddrSize16)
                        lTempAddr16 += Operand.Disp8;
                    else
                        lTempAddr32 += Operand.Disp8;
                }
                else if (Operand.HasDisp16)
                    if (CurrentDecode.lAddrSize16)
                        lTempAddr16 += Operand.Disp16;
                    else
                        lTempAddr32 += Operand.Disp16;
                else if (Operand.HasDisp32)
                    if (CurrentDecode.lAddrSize16)
                        if (Operand.DispIsNegative)
                            lTempAddr16 -= (Word)Operand.Disp32;
                        else
                            lTempAddr16 += (Word)Operand.Disp32;
                    else
                        if (Operand.DispIsNegative)
                        lTempAddr32 += Operand.Disp32;
                    else
                        lTempAddr32 += Operand.Disp32;


                if (CurrentDecode.OverrideSegment != eGeneralRegister.NONE && CurrentDecode.RealOpCode != 0x8d)
                    OpAdd = mProc.GetDWordRegValueForRegEnum(CurrentDecode.OverrideSegment);
                //if (mProc.OperatingMode != ProcessorMode.Protected)
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected && mProc.regs.CS.mDescriptorNum > 0)
                {
                    /*if (CurrentDecode.InstructionAddSize16) 
                        OpAdd = OpAdd << 4;*/
                }
                else
                    OpAdd = OpAdd << 4;
                if (CurrentDecode.lAddrSize16)
                    OpAdd += lTempAddr16;
                else
                    OpAdd += lTempAddr32;

                //The instructions LEA, LDS, LES, LSS don't use the value at the memory location, but rather the memory location itself, as their value
                //We only care about this in debug mode as we'll see the value for the operand, so skip this logic if not debugging
                switch (CurrentDecode.RealOpCode)
                {
                    case 0x8d: Val.OpDWord = OpAdd; break;
                    default:
                        if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 && PhysicalMem.PageAccessWillCausePF(mProc, ref CurrentDecode, OpAdd, false))
                        {
                            return;
                        }
                        else
                            switch (OcOT)
                            {
                                case sOpCodeOperandType.Byte: Val.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.Byte; break;
                                case sOpCodeOperandType.Word: Val.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt16; break;
                                case sOpCodeOperandType.DWord: Val.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt32; break;
                                case sOpCodeOperandType.QWord: Val.OpQWord = mProc.mem.GetQWord(mProc, ref CurrentDecode, OpAdd); OpTypeCode = TypeCode.UInt64; break;
                                case sOpCodeOperandType.Pointer:
                                case sOpCodeOperandType.PseudoDesc:
                                    Val.OpQWord = mProc.mem.GetQWord(mProc, ref CurrentDecode, OpAdd) & 0xFFFFFFFFFFFF; OpTypeCode = TypeCode.UInt64; break;
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

            if (OpType == TypeCode.UInt32)
            {
                if ((Offset.OpDWord & 0x80000000) == 0x80000000)
                {
                    Offset.OpDWord = ((~Offset.OpDWord) + 1);
                    mProc.regs.EIP -= Offset.OpDWord;
                }
                else
                    mProc.regs.EIP += Offset.OpDWord;
                return;
            }
            else if (OpType == TypeCode.UInt16)
            {
                if ((Offset.OpWord & 0x8000) == 0x8000)
                {
                    Offset.OpWord = (Word)((~Offset.OpWord) + 1);
                    mProc.regs.EIP -= Offset.OpWord;
                }
                else
                    mProc.regs.EIP += Offset.OpWord;
                return;
            }
            else
            {
                if ((Offset.OpByte & 0x80) == 0x80)
                {
                    Offset.OpByte = (byte)((~Offset.OpByte) + 1);
                    mProc.regs.EIP -= Offset.OpByte;
                }
                else
                    mProc.regs.EIP += Offset.OpByte;
                return;
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
                            Value.OpDWord = (~Value.OpDWord + 1);
                            return true;
                        }
                        break;
                    default:
                        throw new Exception("D'oh!");
                }
            }
            return false;
        }
        /*        public virtual void Impl(ref Processor_80x86 mProc) { throw new ExceptionNumber("Need instruction '" + mProc.mCurrentOperation.OpCode.Name + "'override for this signature"); }
*/
        public virtual void Impl()
        {
        }
        public virtual void Impl(ref sInstruction CurrentDecode)
        {
        }
        //Turn a reference into a value.
        internal UInt32 GetSegOverriddenAddress(Processor_80x86 mProc, UInt32 Addr)
        {
            if ((Addr >= Processor_80x86.REGADDRBASE) && (Addr <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS) || (mProc.mSegmentOverride == 0))
                return Addr;
            if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
            {
                if (mProc.mSegmentOverride == Processor_80x86.RCS)
                    return mProc.GetDWordRegValueForRegEnum(eGeneralRegister.CS) + Addr;
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
        }
        internal void SetFlagsForSubtraction(Processor_80x86 mProc, sOpVal PreVal1, sOpVal PreVal2, sOpVal Op1Val, TypeCode Op1Type)
        {
            switch (Op1Type)
            {
                case TypeCode.Byte:
                    mProc.regs.setFlagZF(Op1Val.OpByte);
                    mProc.regs.setFlagSF(Op1Val.OpByte);
                    mProc.regs.setFlagAF(PreVal1.OpByte, Op1Val.OpByte);
                    mProc.regs.setFlagPF(Op1Val.OpByte);
                    mProc.regs.setFlagOF_Sub(PreVal1.OpByte, PreVal2.OpByte, Op1Val.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.regs.setFlagZF(Op1Val.OpWord);
                    mProc.regs.setFlagSF(Op1Val.OpWord);
                    mProc.regs.setFlagAF(PreVal1.OpWord, Op1Val.OpWord);
                    mProc.regs.setFlagPF(Op1Val.OpWord);
                    mProc.regs.setFlagOF_Sub(PreVal1.OpWord, PreVal2.OpWord, Op1Val.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.regs.setFlagZF(Op1Val.OpDWord);
                    mProc.regs.setFlagSF(Op1Val.OpDWord);
                    mProc.regs.setFlagAF(PreVal1.OpDWord, Op1Val.OpDWord);
                    mProc.regs.setFlagPF(Op1Val.OpDWord);
                    mProc.regs.setFlagOF_Sub(PreVal1.OpDWord, PreVal2.OpDWord, Op1Val.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.regs.setFlagZF(Op1Val.OpQWord);
                    mProc.regs.setFlagSF(Op1Val.OpQWord);
                    mProc.regs.setFlagAF(PreVal1.OpQWord, Op1Val.OpQWord);
                    mProc.regs.setFlagPF(Op1Val.OpQWord);
                    mProc.regs.setFlagOF_Sub(PreVal1.OpQWord, PreVal2.OpQWord, Op1Val.OpQWord);
                    break;
            }
        }
        internal void SetFlagsForAddition(Processor_80x86 mProc, sOpVal PreVal1, sOpVal PreVal2, sOpVal Op1ValSigned, sOpVal Op1ValUnsigned, TypeCode Op1Type)
        {
            mProc.regs.setFlagAF(Op1ValSigned.OpQWord, PreVal1.OpQWord);
            //mProc.regs.setFlagCF(Op1ValUnsigned.OpQWord, PreVal1.OpQWord, PreVal2.OpQWord);
            if (Misc.parity[Op1ValSigned.OpByte] == 0x1)
                mProc.regs.FLAGS |= 0x4;
            else
                mProc.regs.FLAGS &= 0xFFFB;


            switch (Op1Type)
            {
                case TypeCode.Byte:
                    mProc.regs.setFlagPF(Op1ValSigned.OpByte);
                    mProc.regs.setFlagZF(Op1ValSigned.OpByte);
                    mProc.regs.setFlagOF_Add(PreVal1.OpByte, PreVal2.OpByte, Op1ValSigned.OpByte);
                    mProc.regs.setFlagSF(Op1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.regs.setFlagZF(Op1ValSigned.OpWord);
                    mProc.regs.setFlagOF_Add(PreVal1.OpWord, PreVal2.OpWord, Op1ValSigned.OpWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.regs.setFlagZF(Op1ValSigned.OpDWord);
                    mProc.regs.setFlagOF_Add(PreVal1.OpDWord, PreVal2.OpDWord, Op1ValSigned.OpDWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.regs.setFlagZF(Op1ValSigned.OpQWord);
                    mProc.regs.setFlagOF_Add(PreVal1.OpQWord, PreVal2.OpQWord, Op1ValSigned.OpQWord);
                    mProc.regs.setFlagSF(Op1ValSigned.OpQWord);
                    break;
            }



        }
        internal static bool VerifyCPL0(Processor_80x86 mProc, ref sInstruction mIns)
        {
            if (mProc.regs.CPL != 0 && mProc.OperatingMode != ProcessorMode.Virtual8086)
            {
                mIns.ExceptionNumber = 0xD;
                mIns.ExceptionThrown = true;
                mIns.ExceptionErrorCode = 0;
                mIns.ExceptionAddress = mIns.InstructionAddress;

                if (mProc.mSystem.Debuggies.DebugCPU || mProc.mSystem.Debuggies.DebugExceptions)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, mProc.sCurrentDecode.mChosenInstruction.Name + " executed @ CPL of " + mProc.regs.CPL + ".  Exception 13 triggered");
                return false;
            }
            return true;
        }
        internal static void SetLoopComplete(Processor_80x86 mProc)
        {
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
}
