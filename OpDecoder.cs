using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using Word = System.UInt16;
using DWord = System.UInt32;

namespace VirtualProcessor
{

    enum eOpSizes : byte
    {
        Bit8,
        Bit16,
        Bit32,
        Bit64
    }
    public struct sOperation
    {
        public Processor_80x86 proc;
        public bool InvalidInstruction;
        public Instruct OpCode;
        public cValue Op1Val;
        public bool Op1IsRef;
        public cValue Op2Val;
        public bool Op2IsRef;
        public cValue Op3Val;
        public bool Op3IsRef;
        public byte OpSize;
        public byte Length;
        public byte[] Bytes;
        public string Op1Vals, Op2Vals, Op3Vals;

    }
    struct sOpAndOperands
    {
        public string Operand1, Operand2, Operand3;
    }

    public class OpDecoder
    {
        private static sOpCode mMatchingOpCodeRec;
        private static InstructionList mInstList;
        public static byte[] instrBytes = new byte[4];
        private static sOperation mOperation;
        private static sOpAndOperands mOpAndOperands;
        private static UInt32 mDecodeOffset = 0;
        private static bool mOpCodeFound = false;
        private static byte mCurrOperandNum = 1;
        private static Processor_80x86 mProc;
        private static bool mModRMSet, mInstructionIsEscape = false;
        private static bool mInvalidInstruction = false;
        private static byte mSIB = 0;

        public static sOperation Decode(Processor_80x86 proc)
        {
            mInvalidInstruction = false;
            mInstructionIsEscape = false;
            mModRMSet = false;
            mOperation = new sOperation();
            mOpAndOperands = new sOpAndOperands();
            mOpCodeFound = false;
            mCurrOperandNum = 1;
            mProc = proc;
            mInstList = mProc.Instructions;
            if (mProc.ProtectedModeActive)
                mDecodeOffset = proc.regs.EIP;
            else
                mDecodeOffset = PhysicalMem.GetLocForSegOfs(proc, proc.regs.CS.Value, proc.regs.EIP);

            //if (mProc.regs.EIP == 0xcc8a && mProc.regs.CS == 0x0fa9)
            //    Debugger.Break();

            //int a2 = 0;
            //if (mDecodeOffset == 0x0000800C)
            //    a2 += 1;

            while (1 == 1)
            {
                bool lPrefixFound = false;
                //Loop to find all prefixes
                lPrefixFound = DecodePrefix(mProc.mem.GetByte(mDecodeOffset));
                if (!lPrefixFound)
                    break;
            }
            mCurrOperandNum = 0;
            ParseOpCodeAndOperands();
            if (!mInvalidInstruction)
            {
                mOperation.OpCode.Op1Val = mOperation.Op1Val;
                mOperation.OpCode.Op2Val = mOperation.Op2Val;
                mOperation.OpCode.Op3Val = mOperation.Op3Val;
                mOperation.OpCode.Operand1SValue = mOperation.Op1Vals;
                mOperation.OpCode.Operand1IsRef = mOperation.Op1IsRef;
                mOperation.OpCode.Operand2SValue = mOperation.Op2Vals;
                mOperation.OpCode.Operand2IsRef = mOperation.Op2IsRef;
                mOperation.OpCode.Operand3SValue = mOperation.Op3Vals;
                mOperation.OpCode.Operand3IsRef = mOperation.Op3IsRef;
                mMatchingOpCodeRec.Operand1 = mOpAndOperands.Operand1;
                mMatchingOpCodeRec.Operand2 = mOpAndOperands.Operand2;
                mMatchingOpCodeRec.Operand3 = mOpAndOperands.Operand3;
                mOperation.OpCode.ChosenOpCode = mMatchingOpCodeRec;
            }
            else
            {
                mOperation.InvalidInstruction = true;
            }
            if (mProc.ProtectedModeActive)
                mOperation.Length = (byte)(mDecodeOffset - mProc.regs.EIP);
            else
                mOperation.Length = (byte)(mDecodeOffset - PhysicalMem.GetLocForSegOfs(proc, proc.regs.CS.Value, proc.regs.IP));
            mOperation.Bytes = new byte[mOperation.Length];
            for (UInt32 cnt = (UInt32)mDecodeOffset - mOperation.Length; cnt<mDecodeOffset; cnt++)
                mOperation.Bytes[cnt - (mDecodeOffset - mOperation.Length)] = proc.mem.GetByte(cnt);
            mOperation.proc = proc;
            return mOperation;
        }
        private static bool DecodePrefix(byte ToDecode)
        {
            switch (ToDecode)
            {
                //0F = 2 byte instructions

                //26 = ES seg override
                case 0x26:
                    mProc.mSegmentOverride = mProc.RES;
                    mDecodeOffset++;
                    return true;
                //2E = CS seg override
                case 0x2E:
                    if ((mProc.mem.GetByte(mDecodeOffset + 1) >= 0x70 && mProc.mem.GetByte(mDecodeOffset + 1) <= 0x7E) || mProc.mem.GetByte(mDecodeOffset + 1) == 0xE3)
                        mProc.mBranchHint = 0x2E;
                    else
                    {
                        mProc.mSegmentOverride = mProc.RCS;
                    }
                    mDecodeOffset++;
                    return true;
                //36 = SS seg override
                case 0x36:
                    mProc.mSegmentOverride = mProc.RSS;
                    mDecodeOffset++;
                    return true;
                //3E = DS seg override
                case 0x3E:
                    if ((mProc.mem.GetByte(mDecodeOffset + 1) >= 0x70 && mProc.mem.GetByte(mDecodeOffset + 1) <= 0x7E) || mProc.mem.GetByte(mDecodeOffset + 1) == 0xE3)
                        mProc.mBranchHint = 0x3E;
                    else
                    {
                        mProc.mSegmentOverride = mProc.RDS;
                    }
                    mDecodeOffset++;
                    return true;
                //64 = FS seg override
                case 0x64:
                    mProc.mSegmentOverride = mProc.RFS;
                    mDecodeOffset++;
                    return true;
                //65 = GS seg override
                case 0x65:
                    mProc.mSegmentOverride = mProc.RGS;
                    mDecodeOffset++;
                    return true;
                //66 = Operand size override prefix
                //09/19/2010 - SetC to always = false instead of ^=true
                case 0x66:
                    mProc.mOpSize16 = false;
                    mDecodeOffset++;
                    return true;
                //67 = Address-size override prefix
                //09/19/2010 - SetC to always = false instead of ^=true
                case 0x67:
                    mProc.mAddrSize16 = false;
                    mDecodeOffset++;
                    return true;
                //F0 = Lock
                case 0xF0:
                    mProc.Signals.LOCK = true ;
                    mDecodeOffset++;
                    return true;
                //F2 = Repne/RepnZ
                case 0xF2:
                    //remarked after new decoder                    mProc.RepeatCondition = "nz";
                    mDecodeOffset++;
                    return true;
                //F3 = RepE/RepZ
                case 0xF3:
//remarked after new decoder                    mProc.RepeatCondition = "z";
                    mDecodeOffset++;
                    return true;
            }
            return false;

        }
        private static void ParseOpCodeAndOperands()
        {
            GetInstruction();
            if (mInstructionIsEscape || mInvalidInstruction)
                return;
            //Our map (XML file) has blank operands, so shift the ones after over the blanks)

            //Both operands are in 1 
            if (mMatchingOpCodeRec.Operand1.Length > 3)
                FixupOperand(ref mMatchingOpCodeRec,1);
            if (mMatchingOpCodeRec.Operand2.Length > 3)
                FixupOperand(ref mMatchingOpCodeRec,2);
            if (mMatchingOpCodeRec.Operand3.Length > 3)
                FixupOperand(ref mMatchingOpCodeRec, 3);

            ShiftOperands();

            byte lModRM = 0;

            #region Process up to 3 operands
            //Parse each operand/operand type
            for (int lOpNum = 1; lOpNum <= 3; lOpNum++)
            {
                string lCurrentOperand;

                mCurrOperandNum++;
                //Sometime one OpCode fills both Operands, so we don't need to process Op2 if Op1 did
                if (lOpNum == 2 & (object)(mOperation.Op2Val) != null)
                    break;

                if (lOpNum == 1)
                    lCurrentOperand = mOpAndOperands.Operand1;
                else if (lOpNum == 2)
                    lCurrentOperand = mOpAndOperands.Operand2;
                else
                    lCurrentOperand = mOpAndOperands.Operand3;

                if (lCurrentOperand != null && lCurrentOperand.Length > 0 )
                {
                    string lRegName="";
                    //string lRegName = CheckForRegisterOperand(lCurrentOperand);


                    //if (lOpNum == 1 && (lRegName == null && mMatchingOpCodeRec.Op1IsReg || lRegName != null && (!mMatchingOpCodeRec.Op1IsReg)))
                    //    System.Diagnostics.Debugger.Break();
                    //if (lOpNum == 2 && (lRegName == null && mMatchingOpCodeRec.Op2IsReg || lRegName != null && (!mMatchingOpCodeRec.Op2IsReg)))
                    //    System.Diagnostics.Debugger.Break();
                    //if (lOpNum == 3 && (lRegName == null && mMatchingOpCodeRec.Op3IsReg || lRegName != null && (!mMatchingOpCodeRec.Op3IsReg)))
                    //    System.Diagnostics.Debugger.Break();
                    //if (lRegName != null)
                    if ((lOpNum == 1 && mMatchingOpCodeRec.Op1IsReg) || 
                        (lOpNum == 2 && mMatchingOpCodeRec.Op2IsReg) || 
                        (lOpNum == 3 && mMatchingOpCodeRec.Op3IsReg))
                    {
                        switch (lOpNum)
                        {
                            case 1:
                                lRegName = mMatchingOpCodeRec.Operand1;
                                break;
                            case 2:
                                lRegName = mMatchingOpCodeRec.Operand2;
                                break;
                            case 3:
                                lRegName = mMatchingOpCodeRec.Operand3;
                                break;
                            default:
                                break;
                        }
                        if (mProc.OpSize16 && lRegName.Length == 3)
                            lRegName = lRegName.Substring(1, 2);
                        //Only processing 16 bits (for now)
                        if (lOpNum == 1)
                        {
                            mOperation.Op1Val = GetRegisterForRegName(lRegName, ref mOperation.OpSize);
                            mOperation.Op1Vals = lRegName;
                            mOperation.Op1IsRef = true;
                        }
                        else
                        {
                            mOperation.Op2Val = GetRegisterForRegName(lRegName, ref mOperation.OpSize);
                            mOperation.Op2Vals = lRegName;
                            mOperation.Op2IsRef = true;
                        }
                    }
                    //Operand does not specify a register
                    else
                    {
                        if (lOpNum == 1)
                        {
                            DecodeNonRegisterOperand(ref mOperation.Op1Val, ref mOperation.Op1Vals, ref mOperation.Op2Val, ref mOperation.Op2Vals, lCurrentOperand, ref mOperation.Op1IsRef, ref lModRM);
                        }
                        else if (lOpNum == 2)
                        {
                            //Junk objects to pass to the call to process the 2nd operand
                            //The first call can fill both operands, so we need to pass both operands to the first call
                            cValue lJunk1 = new cValue(0);
                            string lJunk2 = "";
                            DecodeNonRegisterOperand(ref mOperation.Op2Val, ref mOperation.Op2Vals, ref lJunk1, ref lJunk2, lCurrentOperand, ref mOperation.Op2IsRef, ref lModRM);
                        }
                        else
                        {
                            //Junk objects to pass to the call to process the 2nd operand
                            //The first call can fill both operands, so we need to pass both operands to the first call
                            cValue lJunk1 = new cValue(0);
                            string lJunk2 = "";
                            DecodeNonRegisterOperand(ref mOperation.Op3Val, ref mOperation.Op3Vals, ref lJunk1, ref lJunk2, lCurrentOperand, ref mOperation.Op3IsRef, ref lModRM);
                        }
                    }
                }
            }
            #endregion

            if (lModRM != 0)
            {
                mMatchingOpCodeRec.Mod = (byte)((lModRM & 0xC0) >> 6);
                mMatchingOpCodeRec.Reg = (byte)((lModRM & 0x38) >> 3);
                mMatchingOpCodeRec.RM = (byte)(lModRM & 0x7);
            }
            mMatchingOpCodeRec.SIB = mSIB;
        }
        public static string CheckForRegisterOperand(string Op)
        {
            if (mProc.OpSize16 && Op.Length == 3)
                Op = Op.Substring(1, 2);

            for (int cnt = 0; cnt < 32; cnt++)
                if (Op == Misc.alRegisterNames[cnt])
                { return Op; }
            return null;
        }
        public static cValue GetRegisterForRegName(string RegName, ref byte OpSize)
        {
            cValue lRet;
            //if (mProc.OpSize16 & RegName.Length == 3)
            //    RegName = RegName.Substring(1, 2);
            switch (RegName)
            {
                case "EAX":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REAX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "AX":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RAX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "AH":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RAH);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "AL":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RAL);
                    lRet.mIsRegister=true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "EBX":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REBX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "BX":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RBX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "BH":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RBH);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "BL":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RBL);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "ECX":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.RECX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "CX":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RCX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "CH":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RCH);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "CL":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RCL);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "EDX":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REDX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "DX":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RDX);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "DH":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RDH);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "DL":
                    OpSize = (byte)eOpSizes.Bit8;
                    lRet = new cValue(mProc.RDL);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsByte = true;
                    return lRet;
                case "ESP":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.RESP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "SP":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RSP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "EBP":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REBP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "BP":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RBP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "ESI":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.RESI);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "SI":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RSI);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "EDI":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REDI);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "DI":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RDI);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "DS":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RDS);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "SS":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RSS);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "ES":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RES);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "CS":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RCS);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "FS":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RFS);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "GS":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RGS);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                case "EIP":
                    OpSize = (byte)eOpSizes.Bit32;
                    lRet = new cValue(mProc.REIP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsDWord = true;
                    return lRet;
                case "IP":
                    OpSize = (byte)eOpSizes.Bit16;
                    lRet = new cValue(mProc.RIP);
                    lRet.mIsRegister = true;
                    lRet.mRegisterIsWord = true;
                    return lRet;
                default:
                    throw new Exception("OpDecoder: Unable to identify register '" + RegName + "' by name");
            }
        }
        public static void DecodeNonRegisterOperand(ref cValue OpVal, ref String OpVals, ref cValue Op2Val, ref String Op2Vals, String Op, ref bool IsRef, ref byte ModRM)
        {
            //OpCode specifies a number
            if (Op[0] >= 48 & Op[0] <= 57 & Op.Length==1)
                { OpVal = new cValue(System.Convert.ToByte(Op)); 
#if DECODE_MAKE_STRINGS 
                OpVals = Op; 
#endif
                return;}

            
            switch (Op.Substring(0, 1))
            {
                //direct address
                case "A":
                    if (mProc.OpSize16)
                    {
                        //32 bit pointer
                        OpVal = new cValue(mProc.mem.GetDWord(mDecodeOffset));
//                        OpVal = new cValue((DWord)(mProc.mem.mMemBytes[mDecodeOffset] << 8) + (mProc.mem.mMemBytes[mDecodeOffset + 1] << 8) + (mProc.mem.mMemBytes[mDecodeOffset + 2] << 8) + mProc.mem.mMemBytes[mDecodeOffset + 3]);
#if DECODE_MAKE_STRINGS 
                        OpVals = ValueToHex((OpVal), "d");
                        if (!mProc.ProtectedModeActive)
                            OpVals = OpVals.Substring(0, 4) + ":" + OpVals.Substring(4, 4);
#endif
                        mDecodeOffset += 4;
                    }
                    else
                    {
                        //48 bit pointer ... faking it for now ... and now for real!
                        OpVal = new cValue((UInt64)(mProc.mem.GetQWord(mDecodeOffset) & 0x0000FFFFFFFFFFFF));
#if DECODE_MAKE_STRINGS 
                        OpVals = ValueToHex((OpVal.mQWord>>16),8);
#endif
                        mDecodeOffset += 6;
                    } 
                    break;

                case "C":
                    if (!mModRMSet)
                    {
                        ModRM = mProc.mem.GetByte(mDecodeOffset++);
                        mModRMSet = true;
                    }
                    ModRMAddressingSpecialPurpose(ModRM, ref OpVal, ref OpVals, Op.Substring(1, 1), ref IsRef);
                    break;
                case "D":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method D not implemented");
                
                //A ModR/M byte follows the opcode and specifies the operand. The operand is either a general-purpose register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, a displacement.
                case "E":
                    if (!mModRMSet)
                    {
                        ModRM = mProc.mem.GetByte(mDecodeOffset++);
                        mModRMSet = true;
                    }
                    ModRMAddressingRegAndMem(ModRM, ref OpVal, ref OpVals, ref IsRef, Op.Substring(1, 1));
                    //ModRMAddressingGeneralRegisters(ModRM, ref Op2Val, ref Op2Vals, Op.Substring(1, 1));
                    break;
                
                //FLAGS unused
                case "F":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method F not implemented");
                
                //G	The reg field of the ModR/M byte selects a general register (for example, AX (000)).
                case "G":
                    if (!mModRMSet)
                    {
                        ModRM = mProc.mem.GetByte(mDecodeOffset++);
                        mModRMSet = true;
                    }
                    ModRMAddressingGeneralRegisters(ModRM, ref OpVal, ref OpVals, Op.Substring(1, 1), ref IsRef);
                    break;
                
                case "I":
                    if (Op.Contains("b"))
                    {
                        OpVal = new cValue(mProc.mem.GetByte(mDecodeOffset++));
#if DECODE_MAKE_STRINGS 
                        OpVals = ValueToHex((OpVal), Op.Substring(1, 1));
#endif
                    }
                    else if (mProc.OpSize16)
                    {
                        OpVal = new cValue(mProc.mem.GetWord(mDecodeOffset));
                        mDecodeOffset += 2;
#if DECODE_MAKE_STRINGS 
                        OpVals = ValueToHex((OpVal), Op.Substring(1, 1));
#endif
                    }
                    else
                    {
                        OpVal = new cValue(mProc.mem.GetDWord(mDecodeOffset));
                        mDecodeOffset += 4;
#if DECODE_MAKE_STRINGS 
                        OpVals = ValueToHex((OpVal), Op.Substring(1, 1));
#endif
                    }
                    break;
                
                //Jump to offset, so add offset to current instruction offset
                //Don't add to IP because it will be incremented by insruction offset later
                case "J":
                    cValue lNewAddress;
                    if (Op.Substring(1, 1).Contains("b"))
                    {
                        mDecodeOffset += 1;
                        OpVal = new cValue((byte)(mProc.mem.GetByte(mDecodeOffset - 1)));
#if DECODE_MAKE_STRINGS
                        //if (mProc.regs.CS == 0x0FA9 && mProc.regs.IP == 0x3CF1)
                        //    Debugger.Break();
                        lNewAddress = new cValue((Word)(mProc.regs.IP + (sbyte)(OpVal.mByte) + 2));
                        //Add instruction length
                        OpVals = ValueToHex(lNewAddress, 4);
#endif
                    }
                    else if ((Op.Contains("v") && mProc.OpSize16) || (Op.Contains("w")))
                    {
                        mDecodeOffset += 2;
                        OpVal = new cValue((Word)(mProc.mem.GetWord(mDecodeOffset - 2)));
#if DECODE_MAKE_STRINGS
                        lNewAddress = new cValue((Word)(mProc.regs.IP + (Int16)(OpVal.mWord) + 2));
                        //Add instruction length
                        if ((mMatchingOpCodeRec.OpCode >> 8) != 0)
                            lNewAddress += 2;
                        else
                            lNewAddress += 1;
                        OpVals = ValueToHex(lNewAddress, 4);
#endif
                    }
                    else //only option left is v/32 bit
                    {
                        mDecodeOffset += 4;
                        OpVal = new cValue((DWord)(mProc.mem.GetDWord(mDecodeOffset - 4)));
#if DECODE_MAKE_STRINGS
                        lNewAddress = new cValue((Word)(mProc.regs.IP + (Int32)(OpVal.mDWord) + 2));
                        //Add instruction length
                        if ((mMatchingOpCodeRec.OpCode >> 8) != 0)
                            lNewAddress += 2;
                        else
                            lNewAddress += 1;
                        OpVals = ValueToHex(lNewAddress, 4);
#endif
                    }
                    break;

                //Operand is in memory location referenced by ModR/M
                case "M":
                    IsRef = true;
                    if (Op == "Mp")
                    {
                        if (!mModRMSet)
                        {
                            ModRM = mProc.mem.GetByte(mDecodeOffset++);
                            mModRMSet = true;
                        }
                        ModRMAddressingRegAndMem(ModRM, ref OpVal, ref OpVals, ref IsRef, "Mb");
                        break;
                    }
                    else if (Op == "M" || Op.Contains("-1"))
                    {
                        if (!mModRMSet)
                        {
                            ModRM = mProc.mem.GetByte(mDecodeOffset++);
                            mModRMSet = true;
                        }
                        ModRMAddressingRegAndMem(ModRM, ref OpVal, ref OpVals, ref IsRef, "Mb");
                    }
                    //This should cover Ma where the Operand is a memory address
                    else
                    {
                        if (!mModRMSet)
                        {
                            ModRM = mProc.mem.GetByte(mDecodeOffset++);
                            mModRMSet = true;
                        }
                        ModRMAddressingRegAndMem(ModRM, ref OpVal, ref OpVals, ref IsRef, Op.Substring(1, 1));
                    }
                    break;
                //The instruction has no ModR/M byte; the offset of the operand is coded as a word or double word (depending on address size attribute) in the instruction. No base register, index register, or scaling factor can be applied (for example, MOV (A0–A3)).
                //Changed from GetByte/GetWord to GetWord/Double GetWord per debugging
                case "O":
                    if (mProc.mSegmentOverride == 0)
                        mProc.mSegmentOverride = mProc.RDS;
                    IsRef = true;
                    if (Op.Substring(1, 1).Contains("w") || (Op.Substring(1, 1) == "v" & mProc.AddrSize16 == true))
                    {
                        OpVal = new cValue((UInt16)mProc.mem.GetWord(mDecodeOffset));
#if DECODE_MAKE_STRINGS 
                        OpVals = "[" + ValueToHex((OpVal), "4") + "]";
#endif
                        mDecodeOffset += 2;
                    }
                    else if (Op.Substring(1, 1).Contains("v") & mProc.AddrSize16 == false)
                    {
                        OpVal = new cValue((UInt32)mProc.mem.GetDWord(mDecodeOffset));
#if DECODE_MAKE_STRINGS 
                        OpVals = "[" + ValueToHex((OpVal), "4") + "]";
#endif
                        mDecodeOffset += 4;
                    }
                    else if (Op.Substring(1, 1).Contains("b"))
                    {
                        OpVal = new cValue((UInt16)mProc.mem.GetWord(mDecodeOffset));
#if DECODE_MAKE_STRINGS 
                        OpVals = "[" + ValueToHex((OpVal), "2") + "]";
#endif
                        mDecodeOffset += 2;
                    }
                    else
                    {
                        OpVal = new cValue((UInt32)mProc.mem.GetDWord(mDecodeOffset));
#if DECODE_MAKE_STRINGS 
                        OpVals = "[" + ValueToHex((OpVal), Op.Substring(1, 1)) + "]";
#endif
                        mDecodeOffset += 4;
                    }
                    break;
                case "P":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method P not implemented (MMX)");
                case "Q":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method P not implemented (MMX/32 bit)");

                //R	The mod field of the ModR/M byte may refer only to a general register (for example, MOV (0F20-0F24, 0F26)).
                case "R":
                    if (!mModRMSet)
                    {
                        ModRM = mProc.mem.GetByte(mDecodeOffset++);
                        mModRMSet = true;
                    }
                    ModRMAddressingRegAndMem(ModRM, ref OpVal, ref OpVals, ref IsRef, Op.Substring(1, 1));
                    break;

                //The reg field of the ModR/M byte selects a segment register (for example, MOV (8C,8E)).
                case "S":
                    if (!mModRMSet)
                    {
                        ModRM = mProc.mem.GetByte(mDecodeOffset++);
                        mModRMSet = true;
                    }
                    byte lReg = (byte)((ModRM & 0x38) >> 3);
                    IsRef = true;
                    #region S - Choose Register
                    switch (lReg)
                    {
                        case 0:
                            {
                                OpVal = new cValue(mProc.RES);
                                OpVal.mIsRegister = true;
                                OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                OpVals = "ES";
#endif
                            }
                            break;
                        case 1:
                            {
                                OpVal = new cValue(mProc.RCS);
                                OpVal.mIsRegister = true;
                                OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                OpVals = "CS";
#endif
                            }
                            break;
                        case 2:
                            {
                                OpVal = new cValue(mProc.RSS);
                                OpVal.mIsRegister = true;
                                OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                OpVals = "SS";
#endif
                            }
                            break;
                        case 3:
                            {
                                OpVal = new cValue(mProc.RDS);
                                OpVal.mIsRegister = true;
                                OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                OpVals = "DS";
#endif
                            }
                            break;
                        case 4:
                            {
                                if (mProc.ProtectedModeActive)
                                {
                                    OpVal = new cValue(mProc.RFS);
                                    OpVal.mIsRegister = true;
                                    OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS
                                    OpVals = "FS";
#endif
                                }
                                else
                                OpVal = new cValue(mProc.RES);
                                OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                OpVals = "ES";
#endif
                            }
                            break;
                        case 5:
                            {
                                if (mProc.ProtectedModeActive)
                                {
                                    OpVal = new cValue(mProc.RGS);
                                    OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                    OpVals = "GS";
#endif
                                }
                                else
                                {
                                    OpVal = new cValue(mProc.RES);
                                    OpVal.mRegisterIsWord = true;
#if DECODE_MAKE_STRINGS 
                                    OpVals = "ES";
#endif
                                }
                            }
                            break;
                        //TODO: Establish 4 & 5 code when 32 bit is implemented (6 & 7 are reserved)
                    }
                    #endregion
                    break;
                case "T":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method T not implemented (TEST registers)");
                case "V":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method V not implemented (128 bit XMM register)");
                case "W":
                    throw new Exception("OpDecoder:DecodeNonRegisterOperand: Addressing method W not implemented (XMM)");
               
                //Memory addressed by the DS:SI register pair (for example, MOVS, CMPS, OUTS, or LODS).
                case "X":
                    OpVal = new cValue(PhysicalMem.GetLocForSegOfs(mProc,mProc.regs.DS.Value, mProc.regs.SI));
#if DECODE_MAKE_STRINGS 
                    OpVals = ValueToHex((OpVal), Op.Substring(1, 1));
#endif
                    break;

                //Memory addressed by the ES:DI register pair (for example, MOVS, CMPS, INS, STOS, or SCAS).
                case "Y":
                    OpVal = new cValue(PhysicalMem.GetLocForSegOfs(mProc,mProc.regs.ES.Value, mProc.regs.DI));
#if DECODE_MAKE_STRINGS 
                    OpVals = "[" + ValueToHex((OpVal), Op.Substring(1, 1)) + "]";
#endif
                    break;


                default:
                    throw new Exception("DecodeNonRegisterOperand: Can't decode addressing method '" + Op + "'");
            }
        }
        public static void ModRMAddressingRegAndMem(byte ModRM, ref cValue OpVal, ref string OpVals, ref bool IsRef, string Op2)
        {
            if (!mProc.AddrSize16)
            {
                ModRMAddressingRegAndMem32(ModRM, ref OpVal, ref OpVals, ref IsRef, Op2);
                return;
            }
            byte lMod = (byte)((ModRM & 0xC0) >> 6);
            byte lReg = (byte)((ModRM & 0x38) >> 3);
            byte lRM = (byte)(ModRM & 0x7);
            cValue lOpVal = new cValue(0);

            #region MOD 0,1,2 Processing
            if (lMod < 3)
            {
                IsRef = true;
                #region ModRegRM_RM Processing
                switch (lRM)
                {
                    case 0: lOpVal = new cValue((UInt16)(mProc.regs.BX + mProc.regs.SI)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[BX+SI"; else OpVals = "[BX+SI]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 1: lOpVal = new cValue((UInt16)(mProc.regs.BX + mProc.regs.DI)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[BX+DI"; else OpVals = "[BX+DI]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 2: lOpVal = new cValue((UInt16)(mProc.regs.BP + mProc.regs.SI)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[BP+SI"; else OpVals = "[BP+SI]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RSS; break;
                    case 3: lOpVal = new cValue((UInt16)(mProc.regs.BP + mProc.regs.DI)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[BP+DI"; else OpVals = "[BP+DI]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RSS; break;
                    case 4:
                        if (mProc.OpSize16)
                        {
                            lOpVal = new cValue((UInt16)(mProc.regs.SI));
#if DECODE_MAKE_STRINGS 
                            if (lMod == 1 || lMod == 2)
                                OpVals = "[SI";
                            else
                                OpVals = "[SI]";
#endif
                        }
                        else
                        {
                            lOpVal = new cValue((UInt16)(mProc.regs.ESI));
#if DECODE_MAKE_STRINGS 
                            if (lMod == 1 || lMod == 2)
                                OpVals = "[ESI";
                            else
                                OpVals = "[ESI]";
#endif
                        }
                            
                        if (mProc.mSegmentOverride == 0) 
                            mProc.mSegmentOverride = mProc.RDS; 
                        break;
                    case 5: lOpVal = new cValue((UInt16)(mProc.regs.DI)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[DI"; else OpVals = "[DI]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; 
                        break;
                    ///Must change to byte regardless of op size (per b type)
                    case 6:lOpVal = new cValue((UInt16)(mProc.regs.BP)); 
                        if (lMod == 1 || lMod == 2)
                        {
#if DECODE_MAKE_STRINGS
                            if (lMod == 1 || lMod == 2) OpVals = "[BP"; else OpVals = "[BP]";
#endif
                            //Segment is SS for BP, DS for others
                            if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RSS;
                        }
                        else if (lMod == 0)
                        {
                            lOpVal = new cValue((UInt16)mProc.mem.GetWord(mDecodeOffset)); mDecodeOffset += 2;
                            if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "[" + ValueToHex(lOpVal, 2) + "]"; 
#endif
                        }
                            break;
                    case 7: lOpVal = new cValue((UInt16)(mProc.regs.BX)); 
#if DECODE_MAKE_STRINGS 
                        if (lMod == 1 || lMod == 2) OpVals = "[BX"; else OpVals = "[BX]"; 
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                }
                #endregion
                #region MOD Processing
                switch (lMod)
                {
                    case 1:
                        cValue lAdd = new cValue(mProc.mem.GetByte(mDecodeOffset++));
                        if (Misc.getBit(lAdd, 7) == 1)
                        {
                            lOpVal -= Misc.Negate(lAdd.mByte);
#if DECODE_MAKE_STRINGS 
                            OpVals += "-" + ValueToHex(Misc.Negate(lAdd.mByte),2) + "]";
#endif
                        }
                        else
                        {
                            lOpVal += lAdd;
#if DECODE_MAKE_STRINGS 
                            OpVals += "+" + ValueToHex(lAdd) + "]";
#endif
                        }
                    

                        break;
                    case 2:

                        switch (lRM)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7: lOpVal += (Int16)mProc.mem.GetWord(mDecodeOffset); 
#if DECODE_MAKE_STRINGS 
                            OpVals += "+" + ValueToHex(mProc.mem.GetWord(mDecodeOffset), 4) + "]"; 
#endif
                            mDecodeOffset += 2; break;
                        }
                        break;
                    default:
                        break;
                }
                #endregion
            }
            #endregion
            #region MOD=3 Processing
            else
            {
                if (Op2.Contains("w") || Op2.Contains("v") || Op2.Contains("p"))
                {
                    IsRef = true;
                    if (mProc.OpSize16)
                        switch (lRM)
                        {
                            case 0: lOpVal = new cValue(mProc.RAX); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "AX"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 1: lOpVal = new cValue(mProc.RCX); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "CX"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 2: lOpVal = new cValue(mProc.RDX); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "DX"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 3: lOpVal = new cValue(mProc.RBX); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "BX"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 4: lOpVal = new cValue(mProc.RSP); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "SP"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 5: lOpVal = new cValue(mProc.RBP); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "BP"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 6: lOpVal = new cValue(mProc.RSI); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "SI"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 7: lOpVal = new cValue(mProc.RDI); 
#if DECODE_MAKE_STRINGS 
                                OpVals = "DI"; 
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                        }
                    else
                        switch (lRM)
                        {
                            case 0: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "EAX"; 
#endif
                                lOpVal = new cValue(mProc.REAX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 1: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "ECX"; 
#endif
                                lOpVal = new cValue(mProc.RECX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 2: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "EDX";
#endif
                                lOpVal = new cValue(mProc.REDX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 3: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "EBX"; 
#endif
                                lOpVal = new cValue(mProc.REBX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 4: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "ESP"; 
#endif
                                lOpVal = new cValue(mProc.RESP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 5: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "EBP"; 
#endif
                                lOpVal = new cValue(mProc.REBP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 6: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "ESI"; 
#endif
                                lOpVal = new cValue(mProc.RESI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 7: 
#if DECODE_MAKE_STRINGS 
                                OpVals = "EDI"; 
#endif
                                lOpVal = new cValue(mProc.REDI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        }
                }
                else if (Op2.Contains("d"))
                {
                    IsRef = true;
                    switch (lRM)
                    {
                        case 0:
                            lOpVal = new cValue(mProc.REAX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 1: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "ECX"; 
#endif
                            lOpVal = new cValue(mProc.RECX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 2: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "EDX"; 
#endif
                            lOpVal = new cValue(mProc.REDX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 3: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "EBX"; 
#endif
                            lOpVal = new cValue(mProc.REBX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 4: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "ESP"; 
#endif
                            lOpVal = new cValue(mProc.RESP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 5: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "EBP"; 
#endif
                            lOpVal = new cValue(mProc.REBP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 6: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "ESI"; 
#endif
                            lOpVal = new cValue(mProc.RESI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 7: 
#if DECODE_MAKE_STRINGS 
                            OpVals = "EDI"; 
#endif
                            lOpVal = new cValue(mProc.REDI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                    }
                }
                else
                {
                    IsRef = true;
                    switch (lRM)
                    {
                        case 0: lOpVal = new cValue(mProc.RAL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "AL"; 
#endif
                            break;
                        case 1: lOpVal = new cValue(mProc.RCL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "CL"; 
#endif
                            break;
                        case 2: lOpVal = new cValue(mProc.RDL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true;
#if DECODE_MAKE_STRINGS 
                            OpVals = "DL"; 
#endif
                            break;
                        case 3: lOpVal = new cValue(mProc.RBL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "BL"; 
#endif
                            break;
                        case 4: lOpVal = new cValue(mProc.RAH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true;
#if DECODE_MAKE_STRINGS 
                            OpVals = "AH"; 
#endif
                            break;
                        case 5: lOpVal = new cValue(mProc.RCH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "CH"; 
#endif
                            break;
                        case 6: lOpVal = new cValue(mProc.RDH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "DH"; 
#endif
                            break;
                        case 7: lOpVal = new cValue(mProc.RBH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS 
                            OpVals = "BH"; 
#endif
                            break;
                    }
                }
            }
            #endregion

            OpVal = lOpVal;
            return;
        }
        public static void ModRMAddressingRegAndMem32(byte ModRM, ref cValue OpVal, ref string OpVals, ref bool IsRef, string Op2)
        {
            byte lMod = (byte)((ModRM & 0xC0) >> 6);
            byte lReg = (byte)((ModRM & 0x38) >> 3);
            byte lRM = (byte)(ModRM & 0x7);
            cValue lOpVal = new cValue(0);
            #region MOD 0,1,2 Processing
            if (lMod < 3)
            {
                IsRef = true;
                #region ModRegRM_RM Processing
                switch (lRM)
                {
                    case 0: lOpVal = new cValue((UInt32)(mProc.regs.EAX));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[EAX+"; else OpVals = "[EAX]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break; 
                    case 1:
                        lOpVal = new cValue((UInt32)(mProc.regs.ECX));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[ECX+"; else OpVals = "[ECX]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 2:
                        lOpVal = new cValue((UInt32)(mProc.regs.EDX));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[EDX+"; else OpVals = "[EDX]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 3:
                        lOpVal = new cValue((UInt32)(mProc.regs.EBX));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[EBX+"; else OpVals = "[EBX]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 4://SIB Byte processing
                        SIBProcessing(lMod, ref lOpVal, ref OpVals);
                        break;
                    case 5: //32 bit displacement added to index
                        if (lMod == 0)
                        {
                            lOpVal = new cValue((UInt32)mProc.mem.GetDWord(mDecodeOffset)); mDecodeOffset += 4;
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[" + ValueToHex(lOpVal, 8) + "+"; else OpVals = "[" + ValueToHex(lOpVal, 8) + "]";
#endif
                            if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                        }
                        else
                        {
                            lOpVal = new cValue((UInt32)(mProc.regs.EBP));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[EBP+"; else OpVals = "[EBP]";
#endif
                            if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                        }
                    case 6:
                        lOpVal = new cValue((UInt32)(mProc.regs.ESI));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[ESI+"; else OpVals = "[ESI]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                    case 7:
                        lOpVal = new cValue((UInt32)(mProc.regs.EDI));
#if DECODE_MAKE_STRINGS
                        if (lMod == 1 || lMod == 2) OpVals = "[EDI+"; else OpVals = "[EDI]";
#endif
                        if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS; break;
                }
                #endregion
                #region MOD Processing
                switch (lMod)
                {
                    case 1:
                        lOpVal += Misc.SignExtend(new cValue(mProc.mem.GetByte(mDecodeOffset)), TypeCode.UInt16); 
#if DECODE_MAKE_STRINGS
                        OpVals += ValueToHex((mProc.mem.GetByte(mDecodeOffset)), 2) + "]";
#endif
                        mDecodeOffset++;
                        break;
                    case 2:
                        lOpVal += new cValue(mProc.mem.GetDWord(mDecodeOffset));
#if DECODE_MAKE_STRINGS
                        OpVals += ValueToHex((mProc.mem.GetDWord(mDecodeOffset)), 8) + "]";
#endif
                        mDecodeOffset += 4;
                        break;
                }
                #endregion
            }
            #endregion
            #region MOD=3 Processing
            else
            {
                if (Op2.Contains("w") || Op2.Contains("v") || Op2.Contains("p"))
                {
                    IsRef = true;
                    if (mProc.OpSize16)
                        switch (lRM)
                        {
                            case 0: lOpVal = new cValue(mProc.RAX);
#if DECODE_MAKE_STRINGS
                                OpVals = "AX";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 1: lOpVal = new cValue(mProc.RCX);
#if DECODE_MAKE_STRINGS
                                OpVals = "CX";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 2: lOpVal = new cValue(mProc.RDX);
#if DECODE_MAKE_STRINGS
                                OpVals = "DX";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 3: lOpVal = new cValue(mProc.RBX);
#if DECODE_MAKE_STRINGS
                                OpVals = "BX";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 4: lOpVal = new cValue(mProc.RSP);
#if DECODE_MAKE_STRINGS
                                OpVals = "SP";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 5: lOpVal = new cValue(mProc.RBP);
#if DECODE_MAKE_STRINGS
                                OpVals = "BP";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 6: lOpVal = new cValue(mProc.RSI);
#if DECODE_MAKE_STRINGS
                                OpVals = "SI";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                            case 7: lOpVal = new cValue(mProc.RDI);
#if DECODE_MAKE_STRINGS
                                OpVals = "DI";
#endif
                                lOpVal.mIsRegister = true; lOpVal.mRegisterIsWord = true; break;
                        }
                    else
                        switch (lRM)
                        {
                            case 0:
#if DECODE_MAKE_STRINGS
                                OpVals = "EAX";
#endif
                                lOpVal = new cValue(mProc.REAX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 1:
#if DECODE_MAKE_STRINGS
                                OpVals = "ECX";
#endif
                                lOpVal = new cValue(mProc.RECX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 2:
#if DECODE_MAKE_STRINGS
                                OpVals = "EDX";
#endif
                                lOpVal = new cValue(mProc.REDX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 3:
#if DECODE_MAKE_STRINGS
                                OpVals = "EBX";
#endif
                                lOpVal = new cValue(mProc.REBX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 4:
#if DECODE_MAKE_STRINGS
                                OpVals = "ESP";
#endif
                                lOpVal = new cValue(mProc.RESP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 5:
#if DECODE_MAKE_STRINGS
                                OpVals = "EBP";
#endif
                                lOpVal = new cValue(mProc.REBP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 6:
#if DECODE_MAKE_STRINGS
                                OpVals = "ESI";
#endif
                                lOpVal = new cValue(mProc.RESI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                            case 7:
#if DECODE_MAKE_STRINGS
                                OpVals = "EDI";
#endif
                                lOpVal = new cValue(mProc.REDI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        }
                }
                else if (Op2.Contains("d"))
                {
                    IsRef = true;
                    switch (lRM)
                    {
                        case 0:
#if DECODE_MAKE_STRINGS
                            OpVals = "EAX";
#endif
                            lOpVal = new cValue(mProc.REAX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 1:
#if DECODE_MAKE_STRINGS
                            OpVals = "ECX";
#endif
                            lOpVal = new cValue(mProc.RECX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 2:
#if DECODE_MAKE_STRINGS
                            OpVals = "EDX";
#endif
                            lOpVal = new cValue(mProc.REDX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 3:
#if DECODE_MAKE_STRINGS
                            OpVals = "EBX";
#endif
                            lOpVal = new cValue(mProc.REBX); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 4:
#if DECODE_MAKE_STRINGS
                            OpVals = "ESP";
#endif
                            lOpVal = new cValue(mProc.RESP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 5:
#if DECODE_MAKE_STRINGS
                            OpVals = "EBP";
#endif
                            lOpVal = new cValue(mProc.REBP); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 6:
#if DECODE_MAKE_STRINGS
                            OpVals = "ESI";
#endif
                            lOpVal = new cValue(mProc.RESI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                        case 7:
#if DECODE_MAKE_STRINGS
                            OpVals = "EDI";
#endif
                            lOpVal = new cValue(mProc.REDI); lOpVal.mIsRegister = true; lOpVal.mRegisterIsDWord = true; break;
                    }
                }
                else
                {
                    IsRef = true;
                    switch (lRM)
                    {
                        case 0: lOpVal = new cValue(mProc.RAL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "AL"; 
#endif
                            break;
                        case 1: lOpVal = new cValue(mProc.RCL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "CL"; 
#endif
                            break;
                        case 2: lOpVal = new cValue(mProc.RDL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "DL"; 
#endif
                            break;
                        case 3: lOpVal = new cValue(mProc.RBL); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "BL"; 
#endif
                            break;
                        case 4: lOpVal = new cValue(mProc.RAH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "AH"; 
#endif
                            break;
                        case 5: lOpVal = new cValue(mProc.RCH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "CH"; 
#endif
                            break;
                        case 6: lOpVal = new cValue(mProc.RDH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "DH"; 
#endif
                            break;
                        case 7: lOpVal = new cValue(mProc.RBH); lOpVal.mIsRegister = true; lOpVal.mRegisterIsByte = true; 
#if DECODE_MAKE_STRINGS
                            OpVals = "BH"; 
#endif
                            break;
                    }
                }
            }
            #endregion
            OpVal = lOpVal;

        }
        public static void SIBProcessing(byte MOD, ref cValue OpVal, ref String OpVals)
        {
            mSIB = mProc.mem.GetByte(mDecodeOffset++);
            byte lScale = (byte)((mSIB & 0xC0) >> 6);
            byte lIndex = (byte)((mSIB & 0x38) >> 3);
            byte lBase = (byte)(mSIB & 0x7);
            byte lMultiplier=0; //calculated from the scale

            if (mProc.mSegmentOverride == 0) mProc.mSegmentOverride = mProc.RDS;

            switch (lScale)
            {
                case 0: lMultiplier = 1; break;
                case 1: lMultiplier = 2; break;
                case 2: lMultiplier = 4; break;
                case 3: lMultiplier = 8; break;
            }

#if DECODE_MAKE_STRINGS
            OpVals = "[";
#endif
            if (OpVal == null)
                OpVal = new cValue((UInt32)0);
            OpVal += SIBIdentify(lIndex, MOD, "INDEX", ref OpVals)*lMultiplier;
#if DECODE_MAKE_STRINGS
            if (lMultiplier != 1)
                OpVals += "*" + lMultiplier;
#endif
            OpVal += SIBIdentify(lBase, MOD, "BASE", ref OpVals);
            if (MOD == 0 || MOD == 3) OpVals += "]";

        }
        public static cValue SIBIdentify(byte ToID, byte Mod, String IDWhich, ref String OpVals)
        {
            switch (ToID)
            {
                case 0:
#if DECODE_MAKE_STRINGS
                    OpVals += "EAX";
#endif
                    return new cValue(mProc.regs.EAX);
                case 1:
#if DECODE_MAKE_STRINGS
                    OpVals += "ECX";
#endif
                    return new cValue(mProc.regs.ECX);
                case 2:
#if DECODE_MAKE_STRINGS
                    OpVals += "EDX";
#endif
                    return new cValue(mProc.regs.EDX);
                case 3:
#if DECODE_MAKE_STRINGS
                    OpVals += "EBX";
#endif
                    return new cValue(mProc.regs.EBX);
                case 4:
#if DECODE_MAKE_STRINGS
                    //The [*] nomenclature means a disp32 with no base if MOD is 00, [EBP] otherwise. This provides the following addressing modes:
                    if (IDWhich == "INDEX")
                        OpVals += "";
                    else
                        OpVals += "ESP";
#endif
                    if (IDWhich == "INDEX")
                        return new cValue((UInt32)0);
                    else
                        return new cValue(mProc.regs.ESP);
                case 5:
#if DECODE_MAKE_STRINGS
                    if (IDWhich == "INDEX" && Mod == 0)
                        OpVals += "EBP";
                    else
                        OpVals += ValueToHex(new cValue((UInt32)mProc.mem.GetDWord(mDecodeOffset)));
#endif
                    if (IDWhich == "INDEX" && Mod == 0)
                        return new cValue(mProc.regs.EBP);
                    else
                    {
                        mDecodeOffset += 4;
                        return new cValue((UInt32)mProc.mem.GetDWord(mDecodeOffset - 4));
                    }
                case 6:
#if DECODE_MAKE_STRINGS
                    OpVals += "ESI";
#endif
                    return new cValue(mProc.regs.ESI);
                case 7:
#if DECODE_MAKE_STRINGS
                    OpVals += "EDI";
#endif
                    return new cValue(mProc.regs.EDI);
                default:
                    throw new Exception("D'oh!");
            }
        }
        public static void ModRMAddressingGeneralRegisters(byte ModRM, ref cValue OpVal, ref string OpVals, string Op2, ref bool IsRef)
        {

            byte lReg = (byte)((ModRM & 0x38) >> 0x3);

            IsRef = true;
            if (Op2.Contains("w") || Op2.Contains("v"))
                if (mProc.OpSize16)
                {
                    switch (lReg)
                    {
                        case 0: OpVal = new cValue(mProc.RAX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "AX"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 1: OpVal = new cValue(mProc.RCX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "CX"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 2: OpVal = new cValue(mProc.RDX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "DX"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 3: OpVal = new cValue(mProc.RBX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "BX"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 4: OpVal = new cValue(mProc.RSP); 
#if DECODE_MAKE_STRINGS
                            OpVals = "SP"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 5: OpVal = new cValue(mProc.RBP); 
#if DECODE_MAKE_STRINGS
                            OpVals = "BP"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 6: OpVal = new cValue(mProc.RSI); 
#if DECODE_MAKE_STRINGS
                            OpVals = "SI"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                        case 7: OpVal = new cValue(mProc.RDI); 
#if DECODE_MAKE_STRINGS
                            OpVals = "DI"; 
#endif
                            OpVal.mRegisterIsWord = true; OpVal.mIsRegister = true; break;
                    }
                }
                else
                {
                    switch (lReg)
                    {
                        case 0: OpVal = new cValue(mProc.RAX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "EAX"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 1: OpVal = new cValue(mProc.RCX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "ECX"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 2: OpVal = new cValue(mProc.RDX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "EDX"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 3: OpVal = new cValue(mProc.RBX); 
#if DECODE_MAKE_STRINGS
                            OpVals = "EBX"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 4: OpVal = new cValue(mProc.RSP); 
#if DECODE_MAKE_STRINGS
                            OpVals = "ESP"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 5: OpVal = new cValue(mProc.RBP); 
#if DECODE_MAKE_STRINGS
                            OpVals = "EBP"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 6: OpVal = new cValue(mProc.RSI); 
#if DECODE_MAKE_STRINGS
                            OpVals = "ESI"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                        case 7: OpVal = new cValue(mProc.RDI); 
#if DECODE_MAKE_STRINGS
                            OpVals = "EDI"; 
#endif
                            OpVal.mRegisterIsDWord = true; OpVal.mIsRegister = true; break;
                    }
                }
            else
                switch (lReg)
                {
                    case 0: OpVal = new cValue(mProc.RAL); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "AL"; 
#endif
                        break;
                    case 1: OpVal = new cValue(mProc.RCL); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "CL"; 
#endif
                        break;
                    case 2: OpVal = new cValue(mProc.RDL); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "DL"; 
#endif
                        break;
                    case 3: OpVal = new cValue(mProc.RBL); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "BL"; 
#endif
                        break;
                    case 4: OpVal = new cValue(mProc.RAH); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "AH"; 
#endif
                        break;
                    case 5: OpVal = new cValue(mProc.RCH); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "CH"; 
#endif
                        break;
                    case 6: OpVal = new cValue(mProc.RDH); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "DH"; 
#endif
                        break;
                    case 7: OpVal = new cValue(mProc.RBH); OpVal.mRegisterIsByte = true; OpVal.mIsRegister = true; 
#if DECODE_MAKE_STRINGS
                        OpVals = "BH"; 
#endif
                        break;
                }
            return;
        }
        public static void ModRMAddressingSpecialPurpose(byte ModRM, ref cValue OpVal, ref string OpVals, string Op2, ref bool IsRef)
        {
            byte lReg = (byte)((ModRM & 0x38) >> 0x3);

            IsRef = true;
            switch (lReg)
            {
                case 0: OpVal = new cValue(mProc.RCR0); 
#if DECODE_MAKE_STRINGS
                    OpVals = "CR0"; 
#endif
                    break;
                case 1: break;
                case 2: OpVal = new cValue(mProc.RCR2); 
#if DECODE_MAKE_STRINGS
                    OpVals = "CR2"; 
#endif
                    break;
                case 3: OpVal = new cValue(mProc.RCR3); 
#if DECODE_MAKE_STRINGS
                    OpVals = "CR3"; 
#endif
                    break;
                case 4: OpVal = new cValue(mProc.RCR4); 
#if DECODE_MAKE_STRINGS
                    OpVals = "CR4"; 
#endif
                    break;
            }
            return;
        }

        public static void GetInstruction()
        {
            //            if (mProc.regs.CS == 0xf000 && mProc.regs.IP == 0x910A)
            //                Debugger.Break();
            UInt16 lMemOpCode = mProc.mem.GetByte(mDecodeOffset);
            byte lMemOpCodeLen = 1;
            byte lMemOpCodeFmtLen = 2;
            UInt32 lInstrStartAddr = mDecodeOffset;

            //09/06/10: Added 0xDB - FPU - to be moved later, just experimenting
            if (lMemOpCode == 0x0F /*|| lMemOpCode == 0xDB*/)
            {
                lMemOpCode = (UInt16)((lMemOpCode << 8) + mProc.mem.GetByte(mDecodeOffset + 1));
                lMemOpCodeLen = 2;
                lMemOpCodeFmtLen = 4;
            }
            mOpCodeFound = false;
//X            Instruct lInstruct = Instruct.NewInstruct(mProc.OpCodeIndexer[lMemOpCode].InstructionName);
            //if (lInstruct != null)
            //{
            //    mOperation.OpCode = lInstruct;
            //    mMatchingOpCodeRec = mOperation.OpCode.OpCodes[lMemOpCode];
            //    mOperation.OpCode.ChosenOpCode = mMatchingOpCodeRec;
            //    mDecodeOffset += lMemOpCodeLen;
            //    mOpCodeFound = true;
            //    if (mMatchingOpCodeRec.Instruction.Contains("GRP"))
            //        FixupGroupInstruction();
            //    if (mMatchingOpCodeRec.Operand1 == "ESC")
            //        mInstructionIsEscape = true;
            //    return;
            //}
            #region Old Opcode finder
            /*
            foreach (Instruct lInstr in mInstList)
                foreach (sOpCode lOpCode in lInstr.OpCodes)
                {
                    if (lOpCode.OpCode == lMemOpCode)
                    {
                        mMatchingOpCodeRec = lOpCode;
                        lInstr.ChosenOpCode = lOpCode;
                        mOperation.OpCode = lInstr;
                        mDecodeOffset += lMemOpCodeLen;
                        mOpCodeFound = true;
                        if (mMatchingOpCodeRec.Instruction.StartsWith("GRP"))
                            FixupGroupInstruction();
                        if (mMatchingOpCodeRec.Operand1 == "ESC")
                            mInstructionIsEscape = true;
                        return;
                    }
                }
            */
            #endregion
            if (!mOpCodeFound)
            {
                Debug.WriteLine("Cannot decode opcode " + lMemOpCode.ToString("x").PadLeft(lMemOpCodeFmtLen, '0').ToUpper() + " at locaion " + lInstrStartAddr.ToString("x").PadLeft(8));
                //throw new Exception("Cannot decode opcode " + lMemOpCode.ToString("x").PadLeft(lMemOpCodeFmtLen, '0').ToUpper() + " at locaion " + lInstrStartAddr.ToString("x").PadLeft(8));
                mOperation.InvalidInstruction = true;
                mInvalidInstruction = true;
            }
        }
        public static void FixupGroupInstruction()
        {
            switch (mMatchingOpCodeRec.Instruction)
            {
                case "GRP1":
                    {
                        //000=ADD, 001=OR, 010=ADC, 011=SBB, 100=AND, 101=SUB, 110=XOR, 111=CMP
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["ADD"]; break;
                            case 1: mOperation.OpCode = mInstList["OR"]; break;
                            case 2: mOperation.OpCode = mInstList["ADC"]; break;
                            case 3: mOperation.OpCode = mInstList["SBB"]; break;
                            case 4: mOperation.OpCode = mInstList["AND"]; break;
                            case 5: mOperation.OpCode = mInstList["SUB"]; break;
                            case 6: mOperation.OpCode = mInstList["XOR"]; break;
                            case 7: mOperation.OpCode = mInstList["CMP"]; break;
                            default: return;  throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP2":
                    {
                        //000=ROL, 001=ROR, 010=RCL, 011=RCR, 100=SHL, 101=SHR, 110=---, 111=SAR
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["ROL"]; break;
                            case 1: mOperation.OpCode = mInstList["ROR"]; break;
                            case 2: mOperation.OpCode = mInstList["RCL"]; break;
                            case 3: mOperation.OpCode = mInstList["RCR"]; break;
                            case 4: mOperation.OpCode = mInstList["SHL"]; break;
                            case 5: mOperation.OpCode = mInstList["SHR"]; break;
                            case 6: mInvalidInstruction = true; mDecodeOffset++; break; //May be SAL alias
                            case 7: mOperation.OpCode = mInstList["SAR"]; break;
                            default: return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP3a":
                    {
                        //000=TEST, 001=---, 010=NOT, 011=NEG, 100=MUL, 101=IMUL, 110=DIV, 111=IDIV
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: //1 is an alias for 0 so drop through to 1 if 0
                            case 1: mOperation.OpCode = mInstList["TEST"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "Ib"; break;
                            case 2: mOperation.OpCode = mInstList["NOT"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 3: mOperation.OpCode = mInstList["NEG"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 4: mOperation.OpCode = mInstList["MUL"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 5: mOperation.OpCode = mInstList["IMUL"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 6: mOperation.OpCode = mInstList["DIV"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 7: mOperation.OpCode = mInstList["IDIV"]; mMatchingOpCodeRec.Operand1 = "Eb"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            default: return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP3b":
                    {
                        //000=TEST, 001=---, 010=NOT, 011=NEG, 100=MUL, 101=IMUL, 110=DIV, 111=IDIV
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: //1 is an alias for 0 so drop through to 1 if 0
                            case 1: mOperation.OpCode = mInstList["TEST"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "Iv"; break;
                            case 2: mOperation.OpCode = mInstList["NOT"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 3: mOperation.OpCode = mInstList["NEG"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 4: mOperation.OpCode = mInstList["MUL"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 5: mOperation.OpCode = mInstList["IMUL"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 6: mOperation.OpCode = mInstList["DIV"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            case 7: mOperation.OpCode = mInstList["IDIV"]; mMatchingOpCodeRec.Operand1 = "Ev"; mMatchingOpCodeRec.Operand2 = "-1"; break;
                            default: return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP4":
                    {
                        //000=INC, 001=DEC, 010=---, 011=---, 100=---, 101=---, 110=---, 111=---
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["INC"]; break;
                            case 1: mOperation.OpCode = mInstList["DEC"]; break;
                            case 2: mInvalidInstruction = true; mDecodeOffset++; break;
                            case 3: mInvalidInstruction = true; mDecodeOffset++; break;
                            case 4: mInvalidInstruction = true; mDecodeOffset++; break;
                            case 5: mInvalidInstruction = true; mDecodeOffset++; break;
                            case 6: mInvalidInstruction = true; mDecodeOffset++; break;
                            case 7: mInvalidInstruction = true; mDecodeOffset++; break;
                            default: return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP5":
                    {
                        //000=INC, 001=DEC, 010=CALL, 011=CALL, 100=JMP, 101=JMP, 110=PUSH, 111=---
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["INC"]; break;
                            case 1: mOperation.OpCode = mInstList["DEC"]; break;
                            case 2: mOperation.OpCode = mInstList["CALL"]; mMatchingOpCodeRec.Operand1 = "Ev"; break;
                            //Changed this from CALLF to CALL instead of creating a new instruction
                            //because my CALL instruction can handle far calls!!!
                            case 3: mOperation.OpCode = mInstList["CALLF"]; mMatchingOpCodeRec.Operand1 = "Mp"; break;
                            case 4: mOperation.OpCode = mInstList["JMP"]; mMatchingOpCodeRec.Operand1 = "Ev"; break;
                            case 5: mOperation.OpCode = mInstList["JMPF"]; mMatchingOpCodeRec.Operand1 = "Ep"; break;
                            case 6: mOperation.OpCode = mInstList["PUSH"]; mMatchingOpCodeRec.Operand1 = "Ev"; break;
                            case 7: mInvalidInstruction = true;  mDecodeOffset++; break;
                            default: return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP60":
                    if (mProc.AddrSize16)
                    { mOperation.OpCode = mInstList["PUSHA"]; }
                    else
                    { mOperation.OpCode = mInstList["PUSHAD"]; }
                    break;
                case "GRP61":
                    if (mProc.AddrSize16)
                    { mOperation.OpCode = mInstList["POPA"]; }
                    else
                    { mOperation.OpCode = mInstList["POPAD"]; }
                    break;
                case "GRP7":
                    {
                        //000
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: /*mOperation.OpCode = mInstList["SLDT"];*/ break;
                            case 1: break;
                            case 4: mOperation.OpCode = mInstList["VERR"]; mMatchingOpCodeRec.Operand1 = "Ew"; break;
                            case 5: mOperation.OpCode = mInstList["VERW"]; mMatchingOpCodeRec.Operand1 = "Ew"; break;
                            default:
                                Debug.WriteLine("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                                return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRP8":
                    {
                        //000
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: /*mOperation.OpCode = mInstList["SLDT"];*/ break;
                            case 1: break;
                            case 2: mOperation.OpCode = mInstList["LGDT"]; mMatchingOpCodeRec.Operand1 = "Ms"; break;
                            case 3: mOperation.OpCode = mInstList["LIDT"]; mMatchingOpCodeRec.Operand1 = "Ms"; break;
                            case 4: mOperation.OpCode = mInstList["SMSW"]; mMatchingOpCodeRec.Operand1 = "EW"; break;
                            default:
                                Debug.WriteLine("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3)) + "'");
                                return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3)) + "'");
                        }
                    }
                    break;
                case "GRPC0":
                    {
                        //000
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["ROL"]; break;
                            case 1: mOperation.OpCode = mInstList["ROR"]; break;
                            case 2: mOperation.OpCode = mInstList["RCL"]; break;
                            case 3: mOperation.OpCode = mInstList["RCR"]; break;
                            case 4: mOperation.OpCode = mInstList["SHL"]; break;
                            case 5: mOperation.OpCode = mInstList["SHR"]; break;
                            case 6: mOperation.OpCode = mInstList["SHL"]; break;
                            case 7: mOperation.OpCode = mInstList["SAR"]; break;
                            default:
                                Debug.WriteLine("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                                return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRPC1":
                    {
                        //000
                        //Peek at byte following instruction
                        switch ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x38) >> 3))
                        {
                            case 0: mOperation.OpCode = mInstList["ROL"]; break;
                            case 1: mOperation.OpCode = mInstList["ROR"]; break;
                            case 2: mOperation.OpCode = mInstList["RCL"]; break;
                            case 3: mOperation.OpCode = mInstList["RCR"]; break;
                            case 4: mOperation.OpCode = mInstList["SHL"]; break;
                            case 5: mOperation.OpCode = mInstList["SHR"]; break;
                            case 6: mOperation.OpCode = mInstList["SHL"]; break;
                            case 7: mOperation.OpCode = mInstList["SAR"]; break;
                            default:
                                Debug.WriteLine("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                                return; throw new Exception("FixupGroupInstruction: Could not find Instruction for '" + ((byte)((mProc.mem.GetByte(mDecodeOffset) & 0x1C) >> 8)) + "'");
                        }
                    }
                    break;
                case "GRPPUSH":
                    {
                        if (mProc.AddrSize16)
                        {
                            mOperation.OpCode = mInstList["PUSHA"]; break;
                        }
                        else
                        {
                            mOperation.OpCode = mInstList["PUSHAD"]; break;

                        }
                    }
            }
        }
        public static void FixupOperand(ref sOpCode lRec, byte OpToFix)
        {
            string[] lOps;
            if (OpToFix==1)
                lOps = lRec.Operand1.Split(' ');
            else
                lOps = lRec.Operand1.Split(' ');
            lRec.Operand1 = lOps[0];
            lRec.Operand2 = lOps[1];

        }
        public static void ShiftOperands()
        {
            if (mMatchingOpCodeRec.Operand1 != "-1")
                mOpAndOperands.Operand1 = mMatchingOpCodeRec.Operand1;
            if (mMatchingOpCodeRec.Operand2 != "-1")
                if (mOpAndOperands.Operand1 == null)
                    mOpAndOperands.Operand1 = mMatchingOpCodeRec.Operand2;
                else
                    mOpAndOperands.Operand2 = mMatchingOpCodeRec.Operand2;

            if (mMatchingOpCodeRec.Operand3 != "-1")
                if (mOpAndOperands.Operand1 == null)
                    mOpAndOperands.Operand1 = mMatchingOpCodeRec.Operand3;
                else if (mOpAndOperands.Operand2 == null)
                    mOpAndOperands.Operand2 = mMatchingOpCodeRec.Operand3;
                else
                    mOpAndOperands.Operand3 = mMatchingOpCodeRec.Operand3;
        }

        public static string ValueToHex(UInt32 Value, int Size)
        {
            //return ValueToHex(new cValue((UInt32)Value));
            return string.Format("{0:X}", Value).PadLeft(Size, '0');
        }
        public static string ValueToHex(UInt32 Value, string OpType)
        {
            int lSize = 0;
            switch (OpType)
            {
                case "b":
                case "s":
                    lSize = 2;
                    break;
                case "v":
                    if (mProc.OpSize16)
                        lSize = 4;
                    else
                        lSize = 8;
                    break;
                case "w":
                    lSize = 4;
                    break;
                case "d":
                case "si":
                case "p":
                    lSize = 8;
                    break;
                default:
                    lSize = 4;
                    break;
            }
            return ValueToHex(Value, lSize);
        }
        public static string ValueToHex(Object Value, string OpType)
        {
            UInt32 lValue = System.Convert.ToUInt32(Value);
            return ValueToHex(lValue, OpType);
        }
        public static string ValueToHex(Object Value, int Size)
        {
            UInt32 lValue = System.Convert.ToUInt32(Value);
            return ValueToHex(lValue, Size);
        }
        public static string ValueToHex(cValue Value)
        {
            switch (Value.GetTypeCode())
            {
                case TypeCode.Byte:
                    return string.Format("{0:X}", Value.GetByte()).PadLeft(2, '0');
                case TypeCode.UInt16:
                    return string.Format("{0:X}", Value.GetWord()).PadLeft(4, '0');
                case TypeCode.UInt32:
                    return string.Format("{0:X}", Value.GetDWord()).PadLeft(16, '0');
                case TypeCode.UInt64:
                    return string.Format("{0:X}", Value.GetQWord()).PadLeft(32, '0');
                default:
                    return "";
            }
        }
        public int ConvertToSigned(Byte Value)
        {
            int lTemp = 0;
            if ((Value & 0x80) == 0x80)
            {
                lTemp = (0xFF - Value) + 1;
                lTemp = lTemp - (2 * lTemp);
                return lTemp;
            }
            else
                return Value;
        }
        public int ConvertToSigned(Word Value)
        {
            int lTemp = 0;
            if ((Value & 0x8000) == 0x8000)
            {
                lTemp = (0xFFFF - Value) + 1;
                lTemp = lTemp - (2 * lTemp);
                return lTemp;
            }
            else
                return Value;
        }

        //public int ConvertToSigned(DWord Value)
        //{
        //    int lTemp = 0;
        //    if ((Value & 0x80000000) == 0x80000000)
        //    {
        //        lTemp = (int)(0xFFFFFFFF - Value) + 1;
        //        lTemp = lTemp - (2 * lTemp);
        //        return lTemp;
        //    }
        //    else
        //        return Value;
        //}
    }
}
