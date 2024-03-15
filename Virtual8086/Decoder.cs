using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor
{
    public struct sOperand
    {
        public bool HasImm8, HasImm16, HasImm32, HasImm64, HasDisp8, HasDisp16, HasDisp32, HasEffective, DispIsNegative;
        public byte Disp8, Imm8, SIBMultiplier;
        public UInt16 Disp16, Imm16;
        public UInt32 Disp32, Imm32;
        public UInt64 Imm64;
        public eGeneralRegister EffReg1, EffReg2, Register;
    }

    public struct sInstruction
    {
        public UInt32 testVal;
        public char BytesUsed;
        public char BranchHint;
        public UInt32 RealOpCode;
        public UInt32 OpCode;
        public eGeneralRegister OverrideSegment;
        public bool InstructionOpSize16, InstructionAddSize16, Lock, RepNZ, RepZ, IsTwoByteOpCode, HasSIB;
        public Byte ModRegRM, ModRegRM_Mod, ModRegRM_Reg, ModRegRM_RM;
        public Byte SIB, Scale, Index, Base;
        public bool OpSizeBit, DirectionBit, SignBit, FWait, OpSizePrefixFound, AddrSizePrefixFound;
        public Byte ConditionField, OpcodeRegisterField;
        public UInt32 InstructionAddress;
        public bool ExceptionThrown;
        public UInt32 ExceptionNumber;
        public UInt32 ExceptionErrorCode, ExceptionAddress;
        internal eProcessorStatus mInstructionStatus;
        public sOperand Op1, Op2, Op3;
        public UInt32 InstructionEIP;
        public Instruct mChosenInstruction;
        public sOpCode ChosenOpCode;
        public byte[] bytes;
        public string Operand1SValue, Operand2SValue, Operand3SValue, CombinedSValue;
        public bool Operand1IsRef, Operand2IsRef, Operand3IsRef;
        public TypeCode Op1TypeCode, Op2TypeCode, Op3TypeCode;
        public sOpVal Op1Value, Op2Value, Op3Value;
        public DWord Op1Add, Op2Add, Op3Add;
        internal bool lOpSize16, lAddrSize16, mOverrideAddrSizeFor32BitGate;
        #region ResolveOp Variables
        public sOpCodeAddressingMethod OcAM;
        public sOpCodeOperandType OcOT;
        public sOperand Operand; //Don't want to initialize it but the compiler won't let me not init it
        public eGeneralRegister ChosenOpCode_Register;
        public bool HasImmediateData, OpHasEffective;
        public bool Op1Populated, Op2Populated, Op3Populated;
        public sOperand Op1Operand, Op2Operand, Op3Operand;

        public void Init()
        {
            RealOpCode = 0;
            OpCode = 0;
            OverrideSegment = 0;
            InstructionOpSize16 = false; InstructionAddSize16 = false; Lock = false; RepNZ = false; RepZ = false; IsTwoByteOpCode = false; HasSIB = false; OpSizeBit = false; DirectionBit = false; SignBit = false; FWait = false; OpSizePrefixFound = false; AddrSizePrefixFound = false;
            ExceptionThrown = false; Operand1IsRef = false; Operand2IsRef = false; Operand3IsRef = false; lOpSize16 = false; lAddrSize16 = false; mOverrideAddrSizeFor32BitGate = false; HasImmediateData = false; OpHasEffective = false; Op1Populated = false; Op2Populated = false; Op3Populated = false;
            InstructionEIP = 0; ExceptionAddress = 0; ExceptionErrorCode = 0; ExceptionNumber = 0; InstructionAddress = 0; ModRegRM = 0; ModRegRM_Mod = 0; ModRegRM_Reg = 0; ModRegRM_RM = 0; SIB = 0; Scale = 0; Index = 0; Base = 0; ConditionField = 0; OpcodeRegisterField = 0;
            mInstructionStatus = 0;
            bytes = null;
            ChosenOpCode_Register = 0;
            Op1 = new sOperand();
            Op2 = new sOperand();
            Op3 = new sOperand();
            /*Op1.Imm64 = 0; Op1.Imm32 = 0; Op1.Disp32 = 0; Op1.Imm16 = 0; Op1.Disp16 = 0; Op1.Imm8 = 0; Op1.Disp8 = 0;
            Op1.SIBMultiplier = 0;
            Op1.DispIsNegative = false; Op1.HasDisp16 = false; Op1.HasDisp32 = false; Op1.HasDisp8 = false; Op1.HasEffective = false; Op1.HasImm16 = false; Op1.HasImm32 = false; Op1.HasImm64 = false; Op1.HasImm8 = false;
            Op1.Register = 0;
            Op1.EffReg1 = 0;
            Op1.EffReg2 = 0;
            Op2.Imm64 = 0; Op2.Imm32 = 0; Op2.Disp32 = 0; Op2.Imm16 = 0; Op2.Disp16 = 0; Op2.Imm8 = 0; Op2.Disp8 = 0;
            Op2.SIBMultiplier = 0;
            Op2.DispIsNegative = false; Op2.HasDisp16 = false; Op2.HasDisp32 = false; Op2.HasDisp8 = Op2.HasEffective = Op2.HasImm16 = Op2.HasImm32 = Op2.HasImm64 = Op2.HasImm8 = false;
            Op2.Register = 0;
            Op2.EffReg1 = 0;
            Op2.EffReg2 = 0;
            Op3.Imm64 = 0; Op3.Imm32 = 0; Op3.Disp32 = 0; Op3.Imm16 = 0; Op3.Disp16 = 0; Op3.Imm8 = 0; Op3.Disp8 = 0;
            Op3.SIBMultiplier = 0;
            Op3.DispIsNegative = false; Op3.HasDisp16 = false; Op3.HasDisp32 = false; Op3.HasDisp8 = false; Op3.HasEffective = false; Op3.HasImm16 = false; Op3.HasImm32 = false; Op3.HasImm64 = false; Op3.HasImm8 = false;
            Op3.Register = 0;
            Op3.EffReg1 = 0;
            Op3.EffReg2 = 0;*/
            //Op3 = Op2;
             Op1Add = 0; Op2Add = 0; Op3Add = 0;
            Op1Value.OpQWord = 0;
            Op2Value.OpQWord = 0;
            Op3Value.OpQWord = 0;
            //Op1 = new sOperand();
            //Op2 = new sOperand();
            //Op3 = new sOperand();
        }
        #endregion
    }


    public class Decoder
    {
        #region Variables
        public UInt32 mDecodeOffset;
        private eGeneralRegister mTempEffReg1, mTempEffReg2, mTemp_MODRM_MODFieldRegister, mTemp_MODRM_REGFieldRegister, mOverrideSegment;
        public UInt32 mDisplacement32, mImmediate, mSIBDisplacement;
        public UInt16 mDisplacement16;
        public byte mDisplacement8;
        public byte mTempSIBMultiplier;
        private readonly InstructionList<Instruct> mIList;
        //private Instruct mChosenInstruction;
        private bool mOpFound, mProcessor_OpSize16, mProcessor_AddrSize16, mOrigProcessor_OpSize16, mOrigProcessor_AddrSize16,
            mHasDisp8, mHasDisp16, mHasDisp32, mDispIsNegative, mHasMODRMEffective, mFPUInstructRequiresModRM, mProcessor_ProtectedModeActive;
        private readonly Processor_80x86 mProc;
        private UInt32 mInitialDecodeOffset;
        private byte mTempPrefixByte;
        private bool mTempPrefixByteUsed;
        /// <summary>
        /// Indicates whether the CS:EIP of the processor should be used.  If false then the mDecodeOffset must be set manually before Decode() is called.
        /// </summary>
        private bool mUseProcessorCS_IP = false;
        /// <summary>
        /// Indicates whether exceptions should be triggered by this Decoder instance.  If false then mValidDecode will be set to False whenever an exception would have been triggered
        /// </summary>
        private bool mTriggerExceptions = true;
        /// <summary>
        /// Indicates whether the instruction returned by Decode() is valid or not.  Only used if TriggerExceptions=false
        /// </summary>
        public bool mValidDecode;
        UInt64 cnt;
        public UInt32 mInstructionEIP;
        byte mNextByte;
        #endregion

        public Decoder(Processor_80x86 Proc, InstructionList<Instruct> IList, sOpCodePointer[] OpcodeIndexer, bool UseProcessorCS_IP, bool TriggerExceptions)
        {
            mProc = Proc;
            mIList = IList;
            mUseProcessorCS_IP = UseProcessorCS_IP;
            mTriggerExceptions = TriggerExceptions;
            Process p = Process.GetCurrentProcess();
            p.PriorityBoostEnabled = true;
            p.PriorityClass = ProcessPriorityClass.High;
            //p.ProcessorAffinity = (IntPtr)0xE0;
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
        }

        public void Decode(ref sInstruction mInstruction)
        {

            #region Initialize
            mTempEffReg1 = 0;
            mTempEffReg2 = 0;
            mTemp_MODRM_MODFieldRegister = 0;
            mTemp_MODRM_REGFieldRegister = 0;
            mDisplacement32 = 0;
            mImmediate = 0;
            mSIBDisplacement = 0;
            mTempSIBMultiplier = 0;
            mDisplacement16 = 0;
            mDisplacement8 = 0;
            mHasMODRMEffective = false;
            mHasDisp8 = false;
            mHasDisp16 = false;
            mHasDisp32 = false;
            mDispIsNegative = false;
            mFPUInstructRequiresModRM = false;

            mValidDecode = true;
            mOrigProcessor_OpSize16 = mProc.OpSize16;
            mProcessor_OpSize16 = mOrigProcessor_OpSize16;
            mOrigProcessor_AddrSize16 = mProc.AddrSize16;
            mProcessor_AddrSize16 = mOrigProcessor_AddrSize16;
            mProcessor_ProtectedModeActive = Processor_80x86.mProtectedModeActive;
            //mInstruction = new sInstruction();
            mInstruction.Init();
            mInstruction.mInstructionStatus = eProcessorStatus.Decoding;

            mOverrideSegment = eGeneralRegister.DS;


            //If bool set we use processor's CS_IP, otherwise we assume that the current mDecodeOffset is correct
            if (mUseProcessorCS_IP)
            {
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected && mProc.regs.CS.mDescriptorNum > 0)
                    mDecodeOffset = mProc.regs.CS.Value + mProc.regs.EIP;
                else
                    mDecodeOffset = (mProc.regs.CS.Value << 4) + mProc.regs.EIP;
                mInstruction.InstructionEIP = mProc.regs.EIP;
            }
            else
                mInstruction.InstructionEIP = mInstructionEIP;

            if (mDecodeOffset == 0x15A561)
            {
                int a = 0;
                a += 1 - 1 + a - 25;
            }
            mInitialDecodeOffset = mDecodeOffset;
            mInstruction.InstructionAddress = mInitialDecodeOffset;
            #endregion
            bool lPrefixFound = true;
            //Get any prefixes

            while (lPrefixFound)
            {
                lPrefixFound = false;
                mTempPrefixByte = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset);
                mTempPrefixByteUsed = false;
                switch (mTempPrefixByte)
                {
                    case 0x26:  //26 = ES seg override
                        mInstruction.OverrideSegment = eGeneralRegister.ES;
                        lPrefixFound = true;
                        break;
                    case 0x64:  //64 = FS seg override
                        mInstruction.OverrideSegment = eGeneralRegister.FS;
                        lPrefixFound = true;
                        break;
                    case 0x65:  //65 = GS seg override
                        mInstruction.OverrideSegment = eGeneralRegister.GS;
                        lPrefixFound = true;
                        break;
                    case 0x36:  //36 = SS seg override
                        mInstruction.OverrideSegment = eGeneralRegister.SS;
                        lPrefixFound = true;
                        break;
                    case 0x2E:  //2E = CS seg override or Hint
                        mInstruction.OverrideSegment = eGeneralRegister.CS;
                        lPrefixFound = true;
                        break;
                    case 0x3E:  //3E = DS seg override or Hint
                        mInstruction.OverrideSegment = eGeneralRegister.DS;
                        lPrefixFound = true;
                        break;
                    case 0x66:  //66 = Operand size override prefix
                        mProcessor_OpSize16 = !mOrigProcessor_OpSize16;
                        mInstruction.OpSizePrefixFound = true;
                        lPrefixFound = true;
                        break;
                    case 0x67:  //67 = Address-size override prefix
                        mProcessor_AddrSize16 = !mOrigProcessor_AddrSize16;
                        mInstruction.AddrSizePrefixFound = true;
                        lPrefixFound = true;
                        break;
                    case 0xF0:
                        mInstruction.Lock = true;
                        lPrefixFound = true;
                        //NOTE: This prefix didn't have the 3 lines of code that the others had
                        break;
                    case 0xF2:
                        mInstruction.RepNZ = true;
                        lPrefixFound = true;
                        break;
                    case 0xF3:
                        mInstruction.RepZ = true;
                        lPrefixFound = true;
                        break;
                }
                if (lPrefixFound)
                {
                    mDecodeOffset++;
                    mInstruction.BytesUsed++;
                    mTempPrefixByteUsed = true;
                }
            }

            if (mInstruction.ExceptionThrown) { if (!mTriggerExceptions) mInstruction.ExceptionThrown = false; mValidDecode = false; return; }
            #region Opcode Processing
            //Get the OpCode
            if (mTempPrefixByteUsed)
                mInstruction.OpCode = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++);
            else
            {
                mInstruction.OpCode = mTempPrefixByte;
                mDecodeOffset++;
            }

            if (mInstruction.ExceptionThrown) { if (!mTriggerExceptions) mInstruction.ExceptionThrown = false; mValidDecode = false; return; }
            mInstruction.BytesUsed++;

            if ((mInstruction.OpCode == 0x0F) || (mInstruction.OpCode >= 0xD8 && mInstruction.OpCode <= 0xDF))
            {
                mNextByte = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset);
                if (mInstruction.ExceptionThrown) { if (!mTriggerExceptions) mInstruction.ExceptionThrown = false; mValidDecode = false; return; }
                //FPU instructions that have a MODRM > 0xBF use the MODRM as the 2nd byte of the instruction, which consumes the byte
                //All other FPU instructions use bits 5,4,3 as an extension to the instruction, and do NOT consume the byte (which will later be used as the Mod/RM)
                if (mInstruction.OpCode == 0x0F || (mNextByte > 0xBF))
                {
                    mInstruction.OpCode = (UInt16)(mInstruction.OpCode << 8 | mNextByte);
                    mInstruction.IsTwoByteOpCode = true;
                    mInstruction.BytesUsed++;
                    mDecodeOffset++;
                }
                else
                {
                    mInstruction.OpCode = (mInstruction.OpCode << 8) | (byte)((mNextByte >> 3) & 0x07);
                    mFPUInstructRequiresModRM = true;
                }
            }

            mInstruction.RealOpCode = mInstruction.OpCode;
            switch (mInstruction.OpCode)
            {
                //For Group OpCodes, append the REG from the ModRMReg to the OpCode (i.e. 0x80 becomes 0x8001-0x8007)
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0xc0:
                case 0xc1:
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xf6:
                case 0xf7:
                case 0xfe:
                case 0xff:
                case 0x0F00:
                case 0x0F01:
                case 0x0fBA:
                case 0x0FAE:
                    //Move Group instructions 0F00 and 0F01 to AE and AF so that with the REG appened, they will be AE0? and AF0? (unused 2 byte instructions)
                    //The alternative would be to  extend the OpCode array we save opcodes in to 3 bytes or 0x0F0107 (983303) entries!
                    if (mInstruction.OpCode == 0x0FD3)
                        mInstruction.OpCode = 0xAC;
                    if (mInstruction.OpCode == 0x0FBA)
                        mInstruction.OpCode = 0xAD;
                    else if (mInstruction.OpCode == 0x0f00)
                        mInstruction.OpCode = 0xAE;
                    else if (mInstruction.OpCode == 0x0f01)
                        mInstruction.OpCode = 0xAF;
                    mInstruction.OpCode = (mInstruction.OpCode << 8) | (byte)((mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset) >> 3) & 0x07);
                    if (mInstruction.ExceptionThrown) { if (!mTriggerExceptions) mInstruction.ExceptionThrown = false; mValidDecode = false; return; }
                    break;
            }

            //Create an instance of the matching instruction
            try
            {
                mInstruction.mChosenInstruction = mIList[mInstruction.OpCode];
                //mChosenInstruction = Misc.CloneObjectWithIL(mIList[mInstruction.OpCode]);
            }
            catch (Exception e)
            {
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.CPU, "INVALID OPCODE EXCEPTION: Unable to find instruction for OpCode " + mInstruction.OpCode.ToString("X4") + " at CS:EIP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8"));
                throw new Exception("INVALID OPCODE EXCEPTION: Unable to find instruction for OpCode " + mInstruction.OpCode.ToString("X4") + " at CS:EIP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8"));
                System.Diagnostics.Debug.WriteLine("INVALID OPCODE EXCEPTION: Unable to find instruction for OpCode " + mInstruction.OpCode.ToString("X4") + " at CS:EIP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8"));
                mValidDecode = false;
                if (mTriggerExceptions)
                {
                    mInstruction.ExceptionNumber = 0x6;
                    mInstruction.ExceptionThrown = true;
                    mInstruction.ExceptionAddress = mInstruction.InstructionAddress;
                    mInstruction.ExceptionErrorCode = 0;
                    mInstruction.ExceptionNumber = 0x06;
                }
                return;
                //throw new Exception("Unable to find instruction   OpCode " + mInstruction.OpCode.ToString("x") + " at CS:EIP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8"));
            }
            mInstruction.ChosenOpCode = mProc.OpCodeIndexer[mInstruction.OpCode].OpCode;
            mOpFound = true;
            if (mInstruction.ChosenOpCode.OpCode == 0xFFFF)
                mOpFound = false;
            //for (int c = 0; c < mChosenInstruction.mOpCodes.Count; c++)
            //{
            //    mChosenOpCode = mChosenInstruction.mOpCodes[c];
            //    if (mChosenOpCode.OpCode == mInstruction.OpCode)
            //    {
            //        mChosenInstruction.ChosenOpCode = mChosenOpCode;
            //        mOpFound = true;
            //        break;
            //    }
            //}

            if (!mOpFound)
                throw new Exception("Could not find correct version of instruction for OpCode: " + mInstruction.OpCode.ToString("x"));
            #endregion

            mInstruction.OpSizeBit = ((mInstruction.RealOpCode & 0x01) == 0x01);
            mInstruction.DirectionBit = ((mInstruction.RealOpCode & 0x02) == 0x02);
            //s (Sign) bit / d (Direction) bit - same as above
            mInstruction.SignBit = ((mInstruction.RealOpCode & 0x02) == 0x02);
            //Condition Field - same as above
            mInstruction.ConditionField = (byte)(mInstruction.RealOpCode & 0x0F);
            //Opcode Register Field - same as above
            mInstruction.OpcodeRegisterField = (byte)(mInstruction.RealOpCode & 0x07);
            //Process ModRM & SIB - Effective addresses not calculated


            //Get ModRM and SIB if instruction has them
            GetModRMAndSIB(ref mInstruction);

            //LES and LSS instructions don't have the OpSize bit set for some reason
            if (mInstruction.RealOpCode == 0xc4 || mInstruction.RealOpCode == 0x0fb2)
                mInstruction.OpSizeBit = true;

            #region Operand 1 Processing
            if (mInstruction.ChosenOpCode.Op1AM != sOpCodeAddressingMethod.None)
                switch (mInstruction.ChosenOpCode.Op1AM)
                {
                    case sOpCodeAddressingMethod.TheNumberOne:
                        mInstruction.Op1.Imm8 = 1; mInstruction.Op1.HasImm8 = true; break;
                    case sOpCodeAddressingMethod.OpOffset:          //O - The instruction has no ModR/M byte; the offset of the operand is coded as a word or double word (depending on address size attribute) in the instruction. No base register, index register, or scaling factor can be applied (for example, MOV (A0–A3)).
                        if (mProcessor_AddrSize16)
                        { mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; }
                        else
                        { mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; }
                        break;
                    case sOpCodeAddressingMethod.JmpRelOffset:      //J - J	The instruction contains a relative offset to be added to the instruction pointer register
                        #region Get byte(s) immediately following the instruction
                        switch (mInstruction.ChosenOpCode.Op1OT)
                        {
                            case sOpCodeOperandType.Byte: mInstruction.Op1.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op1.HasImm8 = true; break;
                            case sOpCodeOperandType.Word: mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; break;
                            case sOpCodeOperandType.ByteOrWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op1.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op1.HasImm8 = true; }
                                else
                                { mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; }
                                break;
                            case sOpCodeOperandType.DWord: mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; break;
                            case sOpCodeOperandType.WordOrDWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; break; }
                                else
                                { mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; break; }
                            case sOpCodeOperandType.QWord:
                                { mInstruction.Op1.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 8; mInstruction.Op1.HasImm64 = true; break; }
                            default:
                                throw new Exception("Decoder: JmpRelOffset - Op1 - Shouldn't get here");
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.NamedRegister:     //Register named in Operand definition
                        mInstruction.Op1.Register = GetRegEnumForOpcodeNamedByOpSize(mInstruction.ChosenOpCode.Register1); break;
                    case sOpCodeAddressingMethod.DirectAddress:     //A - Direct address. The instruction has no ModR/M byte; the address of the operand is encoded in the instruction; and no base register, index register, or scaling factor can be applied (for example, far JMP (EA)).
                        #region Direct Address
                        switch (mInstruction.ChosenOpCode.Op1OT)
                        {
                            case sOpCodeOperandType.Pointer:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; break; }
                                else //48-bit pointer - 6 bytes
                                { mInstruction.Op1.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset) & 0x0000FFFFFFFFFFFF; mInstruction.Op1.HasImm64 = true; mDecodeOffset += 6; }
                                break;
                            default:
                                throw new Exception("Decode: DirectAddress Op1 - Shouldn't get here!");
                        }
                        break;
                        #endregion
                    case sOpCodeAddressingMethod.MemoryOnly:        //M - The ModR/M byte may refer only to memory (for example, BOUND, LES, LDS, LSS, LFS, LGS, CMPXCHG8B).
                        #region MemoryOnly
                        #region SetC up register via ModMR/SIB Ignoring REG
                        //mInstruction.Op1.Register = GetRegEnumForOpType(mTemp_MODRM_MODFieldRegister, mChosenOpCode.Op1OT);
                        if (mInstruction.OverrideSegment == 0 && mOverrideSegment != eGeneralRegister.NONE && mInstruction.ChosenOpCode.Op1OT != sOpCodeOperandType.None)
                        {
                            mInstruction.OverrideSegment = mOverrideSegment;
                        }
                        mInstruction.Op1.HasEffective = mHasMODRMEffective;
                        mInstruction.Op1.EffReg1 = mTempEffReg1;
                        mInstruction.Op1.EffReg2 = mTempEffReg2;
                        if (mHasDisp8)
                        {
                            mInstruction.Op1.Disp8 = mDisplacement8;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp8 = true;
                        }
                        else if (mHasDisp16)
                        {
                            mInstruction.Op1.Disp16 = mDisplacement16;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp16 = true;
                        }
                        else if (mHasDisp32)
                        {
                            mInstruction.Op1.Disp32 = mDisplacement32;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp32 = true;
                        }

                        if (mInstruction.HasSIB)
                        {
                            mInstruction.Op1.Disp32 += mSIBDisplacement;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp32 = mHasDisp32;
                            mInstruction.Op1.SIBMultiplier = mTempSIBMultiplier;
                        }
                        #endregion
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.ImmedData:         //I - Immediate data. The operand value is encoded in subsequent bytes of the instruction.
                        #region Get byte(s) immediately following the instruction
                        switch (mInstruction.ChosenOpCode.Op1OT)
                        {
                            case sOpCodeOperandType.Byte: mInstruction.Op1.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op1.HasImm8 = true; break;
                            case sOpCodeOperandType.Word: mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; break;
                            case sOpCodeOperandType.ByteOrWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op1.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op1.HasImm8 = true; }
                                else
                                { mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; }
                                break;
                            case sOpCodeOperandType.DWord: mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; break;
                            case sOpCodeOperandType.WordOrDWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op1.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op1.HasImm16 = true; break; }
                                else
                                { mInstruction.Op1.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op1.HasImm32 = true; break; }
                            case sOpCodeOperandType.QWord:
                                { mInstruction.Op1.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 8; mInstruction.Op1.HasImm64 = true; break; }

                            default:
                                throw new Exception("Decoder: ImmedData - Op1 - Shouldn't get here");
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.EFlags:            //F
                        mInstruction.Op1.Register = eGeneralRegister.FLAGS; break;
                    case sOpCodeAddressingMethod.Int3:
                        mInstruction.Op1.Imm8 = 3; mInstruction.Op1.HasImm8 = true; break;
                    #region ModRM/SIB Opcodes
                    case sOpCodeAddressingMethod.RegControlReg:     //C - The reg field of the ModR/M byte selects a control register 
                        mInstruction.Op1.Register = IdentifyControlRegister(mInstruction.ModRegRM_Reg);
                        mHasMODRMEffective = false; break;
                    case sOpCodeAddressingMethod.RegDebugReg:       //D - The reg field of the ModR/M byte selects a debug register 
                        mInstruction.Op1.Register = IdentifyDebugRegister(mInstruction.ModRegRM_Reg);
                        mHasMODRMEffective = false; break;
                    case sOpCodeAddressingMethod.EType:             //E - A ModR/M byte follows the opcode and specifies the operand. The operand is either a general-purpose register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, a displacement.
                        #region SetC up register via ModMR/SIB using Reg
                        //For Op1, Direction=0 is ModRM/SIB, Direction=1 = Reg
                        if (mTemp_MODRM_MODFieldRegister != eGeneralRegister.NONE)
                            mInstruction.Op1.Register = GetRegEnumForOpType(mTemp_MODRM_MODFieldRegister, mInstruction.ChosenOpCode.Op1OT);
                        if (mInstruction.OverrideSegment == 0 && mOverrideSegment != eGeneralRegister.NONE && mInstruction.ChosenOpCode.Op1OT != sOpCodeOperandType.None)
                        {
                            mInstruction.OverrideSegment = mOverrideSegment;
                        }
                        mInstruction.Op1.HasEffective = mHasMODRMEffective;
                        mInstruction.Op1.EffReg1 = mTempEffReg1;
                        mInstruction.Op1.EffReg2 = mTempEffReg2;
                        if (mHasDisp8)
                        {
                            mInstruction.Op1.Disp8 = mDisplacement8;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp8 = true;
                        }
                        else if (mHasDisp16)
                        {
                            mInstruction.Op1.Disp16 = mDisplacement16;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp16 = true;
                        }
                        else if (mHasDisp32)
                        {
                            mInstruction.Op1.Disp32 = mDisplacement32;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp32 = true;
                        }

                        if (mInstruction.HasSIB)
                        {
                            mInstruction.Op1.Disp32 += mSIBDisplacement;
                            mInstruction.Op1.DispIsNegative = mDispIsNegative;
                            mInstruction.Op1.HasDisp32 = mHasDisp32;
                            mInstruction.Op1.SIBMultiplier = mTempSIBMultiplier;
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.GenReg:            //G - The reg field of the ModR/M byte selects a general register 
                        if (mTemp_MODRM_REGFieldRegister != eGeneralRegister.NONE) 
                            mInstruction.Op1.Register = GetRegEnumForOpType(mTemp_MODRM_REGFieldRegister, mInstruction.ChosenOpCode.Op1OT); break;
                    case sOpCodeAddressingMethod.MMXPkdQWord:       //P - The reg field of the ModR/M byte selects a packed quadword MMX technology register.
                        GetRegEnumForMMXReg(); break;
                    case sOpCodeAddressingMethod.QType:             //Q - A ModR/M byte follows the opcode and specifies the operand. The operand is either an MMX technology register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, and a displacement.
                        #region SetC up register via ModMR/SIB using Reg as MMX Register
                        throw new Exception("MMX not coded yet");
                    //For Op1, Direction=0 is Reg, Direction=1 = ModRM
                    //if (!mInstruction.DirectionBit)
                    //    mInstruction.Op1.Register = GetRegEnumForMMXReg(mTemp_MODRM_REGFieldRegister);
                    //else
                    //{
                    //    mInstruction.Op1.EffReg1 = mTempEffReg1;
                    //    mInstruction.Op1.EffReg2 = mTempEffReg2;
                    //    mInstruction.Op1.Displacement = mDisplacement;
                    //}
                    //if (mInstruction.HasSIB)
                    //{
                    //    mInstruction.Op1.Displacement = mSIBDisplacement;
                    //    mInstruction.Op1.SIBMultiplier = mTempSIBMultiplier;
                    //}
                        #endregion
                    case sOpCodeAddressingMethod.ModGenReg:         //R - The mod field of the ModR/M byte may refer only to a general register
                        if (mTemp_MODRM_MODFieldRegister != eGeneralRegister.NONE)
                            mInstruction.Op1.Register = GetRegEnumForOpType(mTemp_MODRM_MODFieldRegister, mInstruction.ChosenOpCode.Op1OT); break;
                    case sOpCodeAddressingMethod.RegSegReg:         //S - The reg field of the ModR/M byte selects a segment register
                        mInstruction.Op1.Register = GetModRMSegReg(ref mInstruction); break;
                    case sOpCodeAddressingMethod.RegTestReg:        //T
                        mInstruction.Op1.Register = GetModRMTestReg(); break;
                    case sOpCodeAddressingMethod.XMM128Reg:         //V
                        throw new Exception("XMM not coded yet");
                    case sOpCodeAddressingMethod.WType:             //W - A ModR/M byte follows the opcode and specifies the operand. The operand is either a 128-bit XMM register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, and a displacement
                        throw new Exception("XMM not coded yet");
                        break;
                    #endregion
                    case sOpCodeAddressingMethod.None: break;
                    default:
                        throw new Exception("Uncoded Addressing Method: " + mInstruction.ChosenOpCode.Op1AM);
                }
            #endregion
            #region Operand 2 Processing
            if (mInstruction.ChosenOpCode.Op2AM != sOpCodeAddressingMethod.None)
                switch (mInstruction.ChosenOpCode.Op2AM)
                {
                    case sOpCodeAddressingMethod.TheNumberOne:
                        mInstruction.Op2.Imm8 = 1; mInstruction.Op2.HasImm8 = true; break;
                    case sOpCodeAddressingMethod.OpOffset:          //The instruction has no ModR/M byte; the offset of the operand is coded as a word or double word (depending on address size attribute) in the instruction. No base register, index register, or scaling factor can be applied (for example, MOV (A0–A3)).
                        if (mProcessor_AddrSize16)
                        { mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; }
                        else
                        { mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; }
                        break;
                    case sOpCodeAddressingMethod.JmpRelOffset:      //J - J	The instruction contains a relative offset to be added to the instruction pointer register
                        #region Get byte(s) immediately following the instruction
                        switch (mInstruction.ChosenOpCode.Op2OT)
                        {
                            case sOpCodeOperandType.Byte: mInstruction.Op2.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op2.HasImm8 = true; break;
                            case sOpCodeOperandType.Word: mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; break;
                            case sOpCodeOperandType.ByteOrWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op2.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op2.HasImm8 = true; }
                                else
                                { mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; }
                                break;
                            case sOpCodeOperandType.DWord: mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; break;
                            case sOpCodeOperandType.WordOrDWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; break; }
                                else
                                { mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; break; }
                            case sOpCodeOperandType.QWord:
                                { mInstruction.Op2.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 8; mInstruction.Op2.HasImm64 = true; break; }
                            default:
                                throw new Exception("Decoder: JmpRelOffset - Op2 - Shouldn't get here");
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.NamedRegister:     //Register named in Operand definition
                        mInstruction.Op2.Register = GetRegEnumForOpcodeNamedByOpSize(mInstruction.ChosenOpCode.Register2); break;
                    case sOpCodeAddressingMethod.DirectAddress:     //A - Direct address. The instruction has no ModR/M byte; the address of the operand is encoded in the instruction; and no base register, index register, or scaling factor can be applied (for example, far JMP (EA)).
                        #region Direct Address
                        switch (mInstruction.ChosenOpCode.Op2OT)
                        {
                            case sOpCodeOperandType.Pointer:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; break; }
                                else //48-bit pointer - 6 bytes
                                { mInstruction.Op2.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset) & 0x0000FFFFFFFFFFFF; mInstruction.Op2.HasImm64 = true; mDecodeOffset += 6; }
                                break;
                            default:
                                throw new Exception("Decode: DirectAddress Op2 - Shouldn't get here!");
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.MemoryOnly:        //M - The ModR/M byte may refer only to memory (for example, BOUND, LES, LDS, LSS, LFS, LGS, CMPXCHG8B).
                        #region MemoryOnly
                        #region SetC up register via ModMR/SIB Ignoring REG
                        //Added 3rd condition for LEA instruction which doesn't want a segment override
                        if (mInstruction.OverrideSegment == 0 && mOverrideSegment != eGeneralRegister.NONE && mInstruction.ChosenOpCode.Op2OT != sOpCodeOperandType.None)
                        {
                            mInstruction.OverrideSegment = mOverrideSegment;
                        }
                        mInstruction.Op2.HasEffective = mHasMODRMEffective;
                        mInstruction.Op2.EffReg1 = mTempEffReg1;
                        mInstruction.Op2.EffReg2 = mTempEffReg2;
                        if (mHasDisp8)
                        {
                            mInstruction.Op2.Disp8 = mDisplacement8;
                            mInstruction.Op2.DispIsNegative = mDispIsNegative;
                            mInstruction.Op2.HasDisp8 = true;
                        }
                        else if (mHasDisp16)
                        {
                            mInstruction.Op2.Disp16 = mDisplacement16;
                            mInstruction.Op2.HasDisp16 = true;
                        }
                        else if (mHasDisp32)
                        {
                            mInstruction.Op2.Disp32 = mDisplacement32;
                            mInstruction.Op2.HasDisp32 = true;
                        }

                        if (mInstruction.HasSIB)
                        {
                            mInstruction.Op2.Disp32 += mSIBDisplacement;
                            mInstruction.Op2.HasDisp32 = mHasDisp32;
                            mInstruction.Op2.SIBMultiplier = mTempSIBMultiplier;
                        }
                        #endregion
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.ImmedData:         //I - Immediate data. The operand value is encoded in subsequent bytes of the instruction.
                        #region Get byte(s) immediately following the instruction
                        switch (mInstruction.ChosenOpCode.Op2OT)
                        {
                            case sOpCodeOperandType.Byte: mInstruction.Op2.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op2.HasImm8 = true; break;
                            case sOpCodeOperandType.Word: mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; break;
                            case sOpCodeOperandType.ByteOrWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op2.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op2.HasImm8 = true; }
                                else
                                { mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; }
                                break;
                            case sOpCodeOperandType.DWord: mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; break;
                            case sOpCodeOperandType.WordOrDWord:
                                if (mProcessor_OpSize16)
                                { mInstruction.Op2.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op2.HasImm16 = true; break; }
                                else
                                { mInstruction.Op2.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op2.HasImm32 = true; break; }
                            case sOpCodeOperandType.QWord:
                                { mInstruction.Op2.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 8; mInstruction.Op2.HasImm64 = true; break; }
                            default:
                                throw new Exception("Decoder: ImmedData - Op2 - Shouldn't get here");
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.EFlags:            //F
                        mInstruction.Op2.Register = eGeneralRegister.FLAGS; break;
                    case sOpCodeAddressingMethod.Int3:
                        break;
                    #region ModRM/SIB Opcodes
                    case sOpCodeAddressingMethod.RegControlReg:     //C - The reg field of the ModR/M byte selects a control register 
                        mInstruction.Op2.Register = IdentifyControlRegister(mInstruction.ModRegRM_Reg);
                        mHasMODRMEffective = false; break;
                    case sOpCodeAddressingMethod.RegDebugReg:       //D - The reg field of the ModR/M byte selects a debug register 
                        mInstruction.Op2.Register = IdentifyDebugRegister(mInstruction.ModRegRM_Reg);
                        mHasMODRMEffective = false; break;
                    case sOpCodeAddressingMethod.EType:             //E - A ModR/M byte follows the opcode and specifies the operand. The operand is either a general-purpose register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, a displacement.
                        #region SetC up register via ModMR/SIB using Reg
                        //For Op2, Direction=0 is Reg, Direction=1 = ModRM
                        if (mTemp_MODRM_MODFieldRegister != eGeneralRegister.NONE)
                            mInstruction.Op2.Register = GetRegEnumForOpType(mTemp_MODRM_MODFieldRegister, mInstruction.ChosenOpCode.Op2OT);
                        if (mInstruction.OverrideSegment == 0 && mOverrideSegment != eGeneralRegister.NONE && mInstruction.ChosenOpCode.Op1OT != sOpCodeOperandType.None)
                        {
                            mInstruction.OverrideSegment = mOverrideSegment;
                        }
                        mInstruction.Op2.HasEffective = mHasMODRMEffective;
                        mInstruction.Op2.EffReg1 = mTempEffReg1;
                        mInstruction.Op2.EffReg2 = mTempEffReg2;
                        if (mHasDisp8)
                        {
                            mInstruction.Op2.Disp8 = mDisplacement8;
                            mInstruction.Op2.DispIsNegative = mDispIsNegative;
                            mInstruction.Op2.HasDisp8 = true;
                        }
                        else if (mHasDisp16)
                        {
                            mInstruction.Op2.Disp16 = mDisplacement16;
                            mInstruction.Op2.DispIsNegative = mDispIsNegative;
                            mInstruction.Op2.HasDisp16 = true;
                        }
                        else if (mHasDisp32)
                        {
                            mInstruction.Op2.Disp32 = mDisplacement32;
                            mInstruction.Op2.DispIsNegative = mDispIsNegative;
                            mInstruction.Op2.HasDisp32 = true;
                        }

                        if (mInstruction.HasSIB)
                        {
                            mInstruction.Op2.Disp32 += mSIBDisplacement;
                            mInstruction.Op2.DispIsNegative = mDispIsNegative;
                            mInstruction.Op2.HasDisp32 = mHasDisp32;
                            mInstruction.Op2.SIBMultiplier = mTempSIBMultiplier;
                        }
                        #endregion
                        break;
                    case sOpCodeAddressingMethod.GenReg:            //G - The reg field of the ModR/M byte selects a general register 
                        if (mTemp_MODRM_REGFieldRegister != eGeneralRegister.NONE)
                            mInstruction.Op2.Register = GetRegEnumForOpType(mTemp_MODRM_REGFieldRegister, mInstruction.ChosenOpCode.Op2OT); break;
                    case sOpCodeAddressingMethod.MMXPkdQWord:       //P - The reg field of the ModR/M byte selects a packed quadword MMX technology register.
                        GetRegEnumForMMXReg(); break;
                    case sOpCodeAddressingMethod.QType:             //Q - A ModR/M byte follows the opcode and specifies the operand. The operand is either an MMX technology register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, and a displacement.
                        #region SetC up register via ModMR/SIB using Reg as MMX Register
                        throw new Exception("MMX not coded yet");
                    //For Op2, Direction=0 is Reg, Direction=1 = ModRM
                    //if (!mInstruction.DirectionBit)
                    //    mInstruction.Op2.Register = GetRegEnumForMMXReg(mTemp_MODRM_REGFieldRegister);
                    //else
                    //{
                    //    mInstruction.Op2.EffReg1 = mTempEffReg1;
                    //    mInstruction.Op2.EffReg2 = mTempEffReg2;
                    //    mInstruction.Op2.Displacement = mDisplacement;
                    //}
                    //if (mInstruction.HasSIB)
                    //{
                    //    mInstruction.Op2.Displacement = mSIBDisplacement;
                    //    mInstruction.Op2.SIBMultiplier = mTempSIBMultiplier;
                    //}
                        #endregion
                    case sOpCodeAddressingMethod.ModGenReg:         //R - The mod field of the ModR/M byte may refer only to a general register
                        if (mTemp_MODRM_MODFieldRegister != eGeneralRegister.NONE)
                            mInstruction.Op2.Register = GetRegEnumForOpType(mTemp_MODRM_MODFieldRegister, mInstruction.ChosenOpCode.Op2OT); break;
                    case sOpCodeAddressingMethod.RegSegReg:         //S - The reg field of the ModR/M byte selects a segment register
                        mInstruction.Op2.Register = GetModRMSegReg(ref mInstruction); break;
                    case sOpCodeAddressingMethod.RegTestReg:        //T
                        mInstruction.Op2.Register = GetModRMTestReg(); break;
                    case sOpCodeAddressingMethod.XMM128Reg:         //V
                        throw new Exception("XMM not coded yet");
                    case sOpCodeAddressingMethod.WType:             //W - A ModR/M byte follows the opcode and specifies the operand. The operand is either a 128-bit XMM register or a memory address. If it is a memory address, the address is computed from a segment register and any of the following values: a base register, an index register, a scaling factor, and a displacement
                        throw new Exception("XMM not coded yet");
                    #endregion
                    case sOpCodeAddressingMethod.None: break;
                    default:
                        throw new Exception("Uncoded Addressing Method: " + mInstruction.ChosenOpCode.Op2AM);
                }
            #endregion
            #region Operand 3 Processing
            switch (mInstruction.ChosenOpCode.Op3AM)
            {
                ////Operand 3 can only be an immediate?
                case sOpCodeAddressingMethod.TheNumberOne:
                    mInstruction.Op3.Imm8 = 1; mInstruction.Op3.HasImm8 = true; break;
                case sOpCodeAddressingMethod.NamedRegister:     //Register named in Operand definition
                    mInstruction.Op3.Register = GetRegEnumForOpcodeNamedByOpSize(mInstruction.ChosenOpCode.Register3); break;
                case sOpCodeAddressingMethod.ImmedData:         //I
                    #region Get byte(s) immediately following the instruction
                    switch (mInstruction.ChosenOpCode.Op3OT)
                    {
                        case sOpCodeOperandType.Byte: mInstruction.Op3.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op3.HasImm8 = true; break;
                        case sOpCodeOperandType.Word: mInstruction.Op3.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op3.HasImm16 = true; break;
                        case sOpCodeOperandType.ByteOrWord:
                            if (mProcessor_OpSize16)
                            { mInstruction.Op3.Imm8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset++; mInstruction.Op3.HasImm8 = true; }
                            else
                            { mInstruction.Op3.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op3.HasImm16 = true; }
                            break;
                        case sOpCodeOperandType.DWord: mInstruction.Op3.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op3.HasImm32 = true; break;
                        case sOpCodeOperandType.WordOrDWord:
                            if (mProcessor_OpSize16)
                            { mInstruction.Op3.Imm16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 2; mInstruction.Op3.HasImm16 = true; break; }
                            else
                            { mInstruction.Op3.Imm32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 4; mInstruction.Op3.HasImm32 = true; break; }
                        case sOpCodeOperandType.QWord:
                            { mInstruction.Op3.Imm64 = mProc.mem.GetQWord(mProc, ref mInstruction, mDecodeOffset); mDecodeOffset += 8; mInstruction.Op3.HasImm64 = true; break; }
                        default:
                            throw new Exception("Decoder: ImmedData - Op3 - Shouldn't get here");
                    }
                    #endregion
                    break;
                case sOpCodeAddressingMethod.Int3:
                    break;
            }
            #endregion

            //All done, return the result!
            mInstruction.BytesUsed = (char)(mDecodeOffset - mInitialDecodeOffset);
#if DEBUG
            mInstruction.bytes = new byte[/*16*/mInstruction.BytesUsed];
            for (cnt = mInitialDecodeOffset; cnt < mDecodeOffset/*16*/; cnt++)
                mInstruction.bytes[cnt - mInitialDecodeOffset] = mProc.mem.GetByte(mProc, ref mInstruction, (uint)cnt);
#endif
            //System.Diagnostics.Debug.WriteLine("");
            mInstruction.InstructionOpSize16 = !mInstruction.OpSizePrefixFound;
            mInstruction.InstructionAddSize16 = !mInstruction.AddrSizePrefixFound;
            mInstruction.mInstructionStatus = eProcessorStatus.NotDecoding;
            return;
        }

        private void GetModRMAndSIB(ref sInstruction mInstruction)
        {
            UInt32 lRealOpCode = mInstruction.RealOpCode;

            UInt32 lTemp = mInstruction.OpCode & 0xFF00;
            if (lTemp >= 0xD800 && lTemp <= 0xDF00)
                if (!mFPUInstructRequiresModRM)
                    return;
            if ((Misc.ModRMRequired(lRealOpCode, mInstruction.IsTwoByteOpCode) > 0) || mFPUInstructRequiresModRM)
            {
                mInstruction.ModRegRM = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++);
                mInstruction.ModRegRM_Mod = (byte)(mInstruction.ModRegRM >> 6);
                mInstruction.ModRegRM_Reg = (byte)((mInstruction.ModRegRM >> 3) & 0x07);
                mInstruction.ModRegRM_RM = (byte)(mInstruction.ModRegRM & 0x07);
                mInstruction.BytesUsed++;
                //Get SIB
                ////Logic from: http://www.devmaster.net/forums/showthread.php?t=2311
                //if (((mInstruction.ModRegRM & 0x07) == 0x04) && (mInstruction.ModRegRM & 0xC0) != 0xC0)
                //if (!mProcessor_AddrSize16 && ( ( (mInstruction.ModRegRM & 0x07) == 0x04) && (mInstruction.ModRegRM & 0xC0) != 0xC0))
                //{
                //}
                ProcessModRMAndSIB(ref mInstruction);
            }
        }

        private void ProcessModRMAndSIB(ref sInstruction mInstruction)
        {
            if (mProcessor_AddrSize16)
                PreProcessModRM16(ref mInstruction);
            else
                PreProcessModRM32(ref mInstruction);
        }

        private void PreProcessModRM16(ref sInstruction mInstruction)
        {
            #region MOD/RM Processing
            //Process the displacement first
            if (mInstruction.ModRegRM_Mod <= 2)
                switch (mInstruction.ModRegRM_Mod)
                {
                    case 0x00:
                        if (mInstruction.ModRegRM_RM == 0x06)
                        {
                            mHasMODRMEffective = true;
                            mOverrideSegment = eGeneralRegister.DS;
                            mDisplacement16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset);
                            mHasDisp16 = true;
                            //My current decoder doesn't sign extend 16 bit ModRM displacements so I won't here
                            mDecodeOffset += 2;
                        }
                        break;
                    case 0x01:
                        mHasMODRMEffective = true;
                        mOverrideSegment = eGeneralRegister.DS;
                        mDisplacement8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++); mInstruction.BytesUsed++;
                        if (Misc.IsNegative(mDisplacement8))
                        {
                            mDisplacement8 = Misc.Negate(mDisplacement8);
                            mDispIsNegative = true;
                        }
                        else
                            mDispIsNegative = false;
                        mHasDisp8 = true;
                        break;
                    case 0x02:
                        mHasMODRMEffective = true;
                        mOverrideSegment = eGeneralRegister.DS;
                        mDisplacement16 = mProc.mem.GetWord(mProc, ref mInstruction, mDecodeOffset);
                        mHasDisp16 = true;
                        //My current decoder doesn't sign extend 16 bit ModRM displacements so I won't here
                        mDecodeOffset += 2;
                        break;
                }
            if (mInstruction.ModRegRM_Mod <= 3)
                //Process the rest of the effective address ModRegRM_Mod value
                switch (mInstruction.ModRegRM_Mod)
                {
                    case 0:
                    case 1:
                    case 2:
                        mHasMODRMEffective = true;
                        switch (mInstruction.ModRegRM_RM)
                        {
                            case 0: mTempEffReg1 = eGeneralRegister.BX; mTempEffReg2 = eGeneralRegister.SI; mOverrideSegment = eGeneralRegister.DS; break;
                            case 1: mTempEffReg1 = eGeneralRegister.BX; mTempEffReg2 = eGeneralRegister.DI; mOverrideSegment = eGeneralRegister.DS; break;
                            case 2: mTempEffReg1 = eGeneralRegister.BP; mTempEffReg2 = eGeneralRegister.SI; mOverrideSegment = eGeneralRegister.SS; break;
                            case 3: mTempEffReg1 = eGeneralRegister.BP; mTempEffReg2 = eGeneralRegister.DI; mOverrideSegment = eGeneralRegister.SS; break;
                            case 4: mTempEffReg1 = eGeneralRegister.SI; mOverrideSegment = eGeneralRegister.DS; break;
                            case 5: mTempEffReg1 = eGeneralRegister.DI; mOverrideSegment = eGeneralRegister.DS; break;
                            case 6: if (mInstruction.ModRegRM_Mod > 0) { mTempEffReg1 = eGeneralRegister.BP; mOverrideSegment = eGeneralRegister.SS; } break;
                            case 7: mTempEffReg1 = eGeneralRegister.BX; mOverrideSegment = eGeneralRegister.DS; break;
                        }
                        break;
                    case 3:
                        switch (mInstruction.ModRegRM_RM)
                        {
                            //Note, these are to be modified by the (s)ize bit ... by the instruction
                            case 0: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                            case 1: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                            case 2: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                            case 3: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                            case 4: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                            case 5: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                            case 6: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                            case 7: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
                        }
                        break;
                }
            #endregion
            #region ModRegRM_Reg Processing
            switch (mInstruction.ModRegRM_Reg)
            {//mTempREGFieldReg
                case 0: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                case 1: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                case 2: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                case 3: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                case 4: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                case 5: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                case 6: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                case 7: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
            }
            #endregion

        }

        private void PreProcessModRM32(ref sInstruction mInstruction)
        {
            #region MOD/ModRegRM_RM Processing
            //Process the non-displacement portion first
            switch (mInstruction.ModRegRM_Mod)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    mHasMODRMEffective = true;
                    //mOverrideSegment = eGeneralRegister.DS; 
                    switch (mInstruction.ModRegRM_RM)
                    {
                        case 0: mTempEffReg1 = eGeneralRegister.EAX; break;
                        case 1: mTempEffReg1 = eGeneralRegister.ECX; break;
                        case 2: mTempEffReg1 = eGeneralRegister.EDX; break;
                        case 3: mTempEffReg1 = eGeneralRegister.EBX; break;
                        case 4: PreProcessSIB(ref mInstruction); break;
                        case 5:
                            if (mInstruction.ModRegRM_Mod == 1 || mInstruction.ModRegRM_Mod == 2)
                            {
                                mTempEffReg1 = eGeneralRegister.EBP;
                                mOverrideSegment = eGeneralRegister.SS;
                            }
                            break;
                        //Process the displacement 
                        case 6: mTempEffReg1 = eGeneralRegister.ESI; break;
                        case 7: mTempEffReg1 = eGeneralRegister.EDI; break;
                    }
                    break;
                case 3:
                    switch (mInstruction.ModRegRM_RM)
                    {
                        //Note, these are to be modified by the (s)ize bit ... by the instruction
                        case 0: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                        case 1: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                        case 2: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                        case 3: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                        //case 4: break; //No ModRegRM_Reg processing needed, already done in SIB processing
                        case 4: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                        case 5: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                        case 6: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                        case 7: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
                    }
                    break;
            }
            //Process the displacement 
            if (!mHasDisp32)
                switch (mInstruction.ModRegRM_Mod)
                {
                    /*case 1: mDisplacement16 = (UInt16)(Misc.SignExtend(mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++)));
                        mInstruction.BytesUsed++;
                        mHasDisp16 = true;
                        //mOverrideSegment = eGeneralRegister.SS;
                        break;*/
                    case 1: mDisplacement8 = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++); mInstruction.BytesUsed++;
                        if ((mDisplacement8 & 0x80) == 0x80)
                        {
                            mDisplacement8 = Misc.Negate(mDisplacement8);
                            mDispIsNegative = true;
                        }
                        else
                            mDispIsNegative = false;
                        mHasDisp8 = true;
                        break;
                    case 0:
                    case 2:
                        if (mInstruction.ModRegRM_Mod == 2 || (mInstruction.ModRegRM_Mod == 0 && mInstruction.ModRegRM_RM == 5))
                        {
                            mDisplacement32 = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset);
                            mInstruction.BytesUsed += (char)4;
                            mDecodeOffset += 4;
                            mHasDisp32 = true;
                        }
                        break;
                }
            #endregion
            #region ModRegRM_Reg Processing
            switch (mInstruction.ModRegRM_Reg)
            {//These will be fixed up later
                case 0: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                case 1: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                case 2: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                case 3: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                case 4: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                case 5: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                case 6: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                case 7: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
            }
            #endregion
        }

        private void PreProcessSIB(ref sInstruction mInstruction)
        {
            mInstruction.SIB = mProc.mem.GetByte(mProc, ref mInstruction, mDecodeOffset++);
            mInstruction.Scale = (byte)(mInstruction.SIB >> 6);
            mInstruction.Index = (byte)(((mInstruction.SIB >> 3) & 0x07));
            mInstruction.Base = (byte)(mInstruction.SIB & 0x07);
            mInstruction.BytesUsed++;
            mInstruction.HasSIB = true;
            //SIBMultiplier
            mHasMODRMEffective = true;
            switch (mInstruction.Scale)
            {
                //case 0: mTempSIBMultiplier = 1; break;
                case 1: mTempSIBMultiplier = 2; break;
                case 2: mTempSIBMultiplier = 4; break;
                case 3: mTempSIBMultiplier = 8; break;
            }
            #region MOD/ModRegRM_RM Processing
            switch (mInstruction.ModRegRM_Mod)
            {
                case 0:
                case 1:
                case 2:
                    mTempEffReg1 = IdentifySIBRegister(ref mInstruction, mInstruction.Index, 0);
                    mTempEffReg2 = IdentifySIBRegister(ref mInstruction, mInstruction.Base, 1);
                    break;
                case 3:
                    switch (mInstruction.ModRegRM_RM)
                    {//These will be fixed up later
                        case 0: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                        case 1: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                        case 2: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                        case 3: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                        case 4: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                        case 5: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                        case 6: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                        case 7: mTemp_MODRM_MODFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
                    }
                    break;
            }
            #endregion
            #region ModRegRM_Reg Processing
            switch (mInstruction.ModRegRM_Reg)
            {//These will be fixed up later
                case 0: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AL); break;
                case 1: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CL); break;
                case 2: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DL); break;
                case 3: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BL); break;
                case 4: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.AH); break;
                case 5: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.CH); break;
                case 6: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.DH); break;
                case 7: mTemp_MODRM_REGFieldRegister = GetRegEnumForSetWBit(mInstruction.OpSizeBit, eGeneralRegister.BH); break;
            }
            #endregion
        }

        private eGeneralRegister IdentifySIBRegister(ref sInstruction mInstruction, byte ToId, int Type)
        {
            switch (ToId)
            {
                case 0: return eGeneralRegister.EAX;
                case 1: return eGeneralRegister.ECX;
                case 2: return eGeneralRegister.EDX;
                case 3: return eGeneralRegister.EBX;
                case 4:
                    if (Type == 0)  //Index
                        return eGeneralRegister.NONE;
                    else            //Base
                    {
                        if (mInstruction.OverrideSegment == eGeneralRegister.NONE)
                            mInstruction.OverrideSegment = eGeneralRegister.SS;
                        return eGeneralRegister.ESP;
                    }
                case 5:
                    if (Type == 0)  //Index
                    {
                        if (mInstruction.OverrideSegment == eGeneralRegister.NONE)
                            mInstruction.OverrideSegment = eGeneralRegister.SS;
                        return eGeneralRegister.EBP;
                    }
                    else            //Base
                    {
                        if (mInstruction.ModRegRM_Mod == 0)
                        {
                            mSIBDisplacement = mProc.mem.GetDWord(mProc, ref mInstruction, mDecodeOffset);
                            //if (Misc.IsNegative(mSIBDisplacement))
                            //{
                            //    mSIBDisplacement = Misc.Negate(mSIBDisplacement);
                            //    mDispIsNegative = true;
                            //}
                            //else
                            mDispIsNegative = false;
                            mHasDisp32 = true;
                            mDecodeOffset += 4;
                            return eGeneralRegister.NONE;
                        }
                        else
                            return eGeneralRegister.EBP;
                    }
                case 6:
                    if (mInstruction.OverrideSegment == eGeneralRegister.NONE)
                        mInstruction.OverrideSegment = eGeneralRegister.DS;
                    return eGeneralRegister.ESI;
                case 7:
                    if (mInstruction.OverrideSegment == eGeneralRegister.NONE)
                        mInstruction.OverrideSegment = eGeneralRegister.DS;
                    return eGeneralRegister.EDI;
                default:
                    return eGeneralRegister.NONE;
            }
        }

        private static eGeneralRegister IdentifyDebugRegister(byte eeeField)
        {
            switch (eeeField)
            {
                case 0:
                    return eGeneralRegister.DR0;
                case 1:
                    return eGeneralRegister.DR1;
                case 2:
                    return eGeneralRegister.DR2;
                case 3:
                    return eGeneralRegister.DR3;
                case 6:
                    return eGeneralRegister.DR6;
                case 7:
                    return eGeneralRegister.DR7;
            }
            throw new Exception("Cannot identify Debug Register: " + eeeField.ToString("x"));
        }

        private static eGeneralRegister IdentifyControlRegister(byte eeeField)
        {
            switch (eeeField)
            {
                case 0:
                    return eGeneralRegister.CR0;
                case 2:
                    return eGeneralRegister.CR2;
                case 3:
                    return eGeneralRegister.CR3;
                case 4:
                    return eGeneralRegister.CR4;
            }
            throw new Exception("Cannot identify Control Register: " + eeeField.ToString("x"));
        }

        public eGeneralRegister GetModRMSegReg(ref sInstruction mInstruction)
        {
            switch (mInstruction.ModRegRM_Reg)
            {
                case 0: return eGeneralRegister.ES;
                case 1: return eGeneralRegister.CS;
                case 2: return eGeneralRegister.SS;
                case 3: return eGeneralRegister.DS;
                case 4: return eGeneralRegister.FS;
                case 5: return eGeneralRegister.GS;
            }
            if ((!mProcessor_OpSize16) || (mProcessor_ProtectedModeActive && mProcessor_OpSize16))
                switch (mInstruction.ModRegRM_Reg)
                {
                    case 4: return eGeneralRegister.FS;
                    case 5: return eGeneralRegister.GS;
                    default:
                        throw new Exception("Segment Registers 6 & 7 are reserved!");
                }

            throw new Exception("Seg reg " + mInstruction.ModRegRM_Reg + " selected, can only have 0-3 unless Operand Size is 32 bit");
        }

        public eGeneralRegister GetModRMTestReg()
        {
            throw new Exception("No documentation for test registers, can't get one!");
        }

        public eGeneralRegister GetModRMControlReg(ref sInstruction mInstruction)
        {
            switch (mInstruction.ModRegRM_Reg)
            {
                case 0: return eGeneralRegister.CR0;
                case 2: return eGeneralRegister.CR2;
                case 3: return eGeneralRegister.CR3;
                case 4: return eGeneralRegister.CR4;
            }
            throw new Exception("Control reg " + mInstruction.ModRegRM_Reg + " selected, it is reserved, can't use it");
        }

        public eGeneralRegister GetModRMDebugReg(ref sInstruction mInstruction)
        {
            switch (mInstruction.ModRegRM_Reg)
            {
                case 0: return eGeneralRegister.DR0;
                case 1: return eGeneralRegister.DR1;
                case 2: return eGeneralRegister.DR2;
                case 3: return eGeneralRegister.DR3;
                case 6: return eGeneralRegister.DR6;
                case 7: return eGeneralRegister.DR7;
            }
            throw new Exception("Debug reg " + mInstruction.ModRegRM_Reg + " selected, it is reserved, can't use it");
        }

        public eGeneralRegister GetRegEnumForMMXReg()
        {
            //switch (mInstruction.ModRegRM_Reg)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //    case 3:
            //    case 4:
            //    case 5:
            //    case 6:
            //    case 7:
            //}
            throw new Exception("MMX registers not yet defined!");
        }

        public static eGeneralRegister GetRegEnumForSetWBit(bool OpSizeBit, eGeneralRegister Reg)
        {
            if (!OpSizeBit)
                return Reg;
            switch (Reg)
            {
                case eGeneralRegister.AL: return eGeneralRegister.AX;
                case eGeneralRegister.CL: return eGeneralRegister.CX;
                case eGeneralRegister.DL: return eGeneralRegister.DX;
                case eGeneralRegister.BL: return eGeneralRegister.BX;
                case eGeneralRegister.AH: return eGeneralRegister.SP;
                case eGeneralRegister.CH: return eGeneralRegister.BP;
                case eGeneralRegister.DH: return eGeneralRegister.SI;
                case eGeneralRegister.BH: return eGeneralRegister.DI;
            }
            return Reg;
            throw new Exception("GetRegEnumForSetWBit: Shouldn't get here!");
        }

        public eGeneralRegister GetRegEnumForOpcodeNamedByOpSize(eGeneralRegister Reg)
        {
            if (!mProcessor_OpSize16)
                return Reg;
            switch (Reg)
            {
                case eGeneralRegister.EAX: return eGeneralRegister.AX;
                case eGeneralRegister.ECX: return eGeneralRegister.CX;
                case eGeneralRegister.EDX: return eGeneralRegister.DX;
                case eGeneralRegister.EBX: return eGeneralRegister.BX;
                case eGeneralRegister.ESP: return eGeneralRegister.SP;
                case eGeneralRegister.EBP: return eGeneralRegister.BP;
                case eGeneralRegister.ESI: return eGeneralRegister.SI;
                case eGeneralRegister.EDI: return eGeneralRegister.DI;
                case eGeneralRegister.AX: return (eGeneralRegister.EAX);
                case eGeneralRegister.CX: return (eGeneralRegister.ECX);
                case eGeneralRegister.DX: return (eGeneralRegister.EDX);
                case eGeneralRegister.BX: return (eGeneralRegister.EBX);
                case eGeneralRegister.SP: return (eGeneralRegister.ESP);
                case eGeneralRegister.BP: return (eGeneralRegister.EBP);
                case eGeneralRegister.SI: return (eGeneralRegister.ESI);
                case eGeneralRegister.DI: return (eGeneralRegister.EDI);
            }
            return Reg;
            throw new Exception("GetRegEnumForOpSize: Shouldn't get here!");
        }

        /// <summary>
        /// Decide which register to use for MOD/RM/REG registers, based on the OpType - Registers have already been through the W Bit code
        /// </summary>
        /// <param name="Reg"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public eGeneralRegister GetRegEnumForOpType(eGeneralRegister Reg, sOpCodeOperandType Type)
        {
            switch (Type)
            {
                case sOpCodeOperandType.Byte:
                    return Reg;
                case sOpCodeOperandType.Word:
                    switch (Reg)
                    {
                        case eGeneralRegister.AL: return eGeneralRegister.AX;
                        case eGeneralRegister.DL: return eGeneralRegister.DX;
                        case eGeneralRegister.CL: return eGeneralRegister.CX;
                        case eGeneralRegister.BL: return eGeneralRegister.BX;
                        case eGeneralRegister.AH: return eGeneralRegister.SP;
                        case eGeneralRegister.CH: return eGeneralRegister.BP;
                        case eGeneralRegister.DH: return eGeneralRegister.SI;
                        case eGeneralRegister.BH: return eGeneralRegister.DI;
                        default: return Reg;
                    }
                case sOpCodeOperandType.ByteOrWord:
                    switch (Reg)
                    {
                        case eGeneralRegister.AL: return mProcessor_OpSize16 ? eGeneralRegister.AX : Reg;
                        case eGeneralRegister.DL: return mProcessor_OpSize16 ? eGeneralRegister.DX : Reg;
                        case eGeneralRegister.CL: return mProcessor_OpSize16 ? eGeneralRegister.CX : Reg;
                        case eGeneralRegister.BL: return mProcessor_OpSize16 ? eGeneralRegister.BX : Reg;
                        case eGeneralRegister.AH: return mProcessor_OpSize16 ? eGeneralRegister.SP : Reg;
                        case eGeneralRegister.CH: return mProcessor_OpSize16 ? eGeneralRegister.BP : Reg;
                        case eGeneralRegister.DH: return mProcessor_OpSize16 ? eGeneralRegister.SI : Reg;
                        case eGeneralRegister.BH: return mProcessor_OpSize16 ? eGeneralRegister.DI : Reg;
                        default: return Reg;
                    }
                case sOpCodeOperandType.DWord:
                    switch (Reg)
                    {
                        case eGeneralRegister.AL: return eGeneralRegister.EAX;
                        case eGeneralRegister.DL: return eGeneralRegister.EDX;
                        case eGeneralRegister.CL: return eGeneralRegister.ECX;
                        case eGeneralRegister.BL: return eGeneralRegister.EBX;
                        case eGeneralRegister.AH: return eGeneralRegister.ESP;
                        case eGeneralRegister.CH: return eGeneralRegister.EBP;
                        case eGeneralRegister.DH: return eGeneralRegister.ESI;
                        case eGeneralRegister.BH: return eGeneralRegister.EDI;
                        case eGeneralRegister.AX: return eGeneralRegister.EAX;
                        case eGeneralRegister.DX: return eGeneralRegister.EDX;
                        case eGeneralRegister.CX: return eGeneralRegister.ECX;
                        case eGeneralRegister.BX: return eGeneralRegister.EBX;
                        case eGeneralRegister.SP: return eGeneralRegister.ESP;
                        case eGeneralRegister.BP: return eGeneralRegister.EBP;
                        case eGeneralRegister.SI: return eGeneralRegister.ESI;
                        case eGeneralRegister.DI: return eGeneralRegister.EDI;
                        default: throw new Exception("should not get here");  return Reg;
                    }
                case sOpCodeOperandType.WordOrDWord:
                    switch (Reg)
                    {
                        case eGeneralRegister.AL: return (mProcessor_OpSize16 ? eGeneralRegister.AX : eGeneralRegister.EAX);
                        case eGeneralRegister.DL: return (mProcessor_OpSize16 ? eGeneralRegister.DX : eGeneralRegister.EDX);
                        case eGeneralRegister.CL: return (mProcessor_OpSize16 ? eGeneralRegister.CX : eGeneralRegister.ECX);
                        case eGeneralRegister.BL: return (mProcessor_OpSize16 ? eGeneralRegister.BX : eGeneralRegister.EBX);
                        case eGeneralRegister.AH: return (mProcessor_OpSize16 ? eGeneralRegister.SP : eGeneralRegister.ESP);
                        case eGeneralRegister.CH: return (mProcessor_OpSize16 ? eGeneralRegister.BP : eGeneralRegister.EBP);
                        case eGeneralRegister.DH: return (mProcessor_OpSize16 ? eGeneralRegister.SI : eGeneralRegister.ESI);
                        case eGeneralRegister.BH: return (mProcessor_OpSize16 ? eGeneralRegister.DI : eGeneralRegister.EDI);


                        case eGeneralRegister.AX: return (mProcessor_OpSize16 ? eGeneralRegister.AX : eGeneralRegister.EAX);
                        case eGeneralRegister.DX: return (mProcessor_OpSize16 ? eGeneralRegister.DX : eGeneralRegister.EDX);
                        case eGeneralRegister.CX: return (mProcessor_OpSize16 ? eGeneralRegister.CX : eGeneralRegister.ECX);
                        case eGeneralRegister.BX: return (mProcessor_OpSize16 ? eGeneralRegister.BX : eGeneralRegister.EBX);
                        case eGeneralRegister.SP: return (mProcessor_OpSize16 ? eGeneralRegister.SP : eGeneralRegister.ESP);
                        case eGeneralRegister.BP: return (mProcessor_OpSize16 ? eGeneralRegister.BP : eGeneralRegister.EBP);
                        case eGeneralRegister.SI: return (mProcessor_OpSize16 ? eGeneralRegister.SI : eGeneralRegister.ESI);
                        case eGeneralRegister.DI: return (mProcessor_OpSize16 ? eGeneralRegister.DI : eGeneralRegister.EDI);

                        default: throw new Exception("should not get here 2"); return Reg;
                    }
            }
            throw new Exception("CRAP!");
        }

    }


}