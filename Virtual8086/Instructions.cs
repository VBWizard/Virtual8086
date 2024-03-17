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
using System.Globalization;

namespace VirtualProcessor
{
    public enum sOpCodeAddressingMethod
    {
        None,
        DirectAddress,  //A
        RegControlReg,  //C
        RegDebugReg,    //D
        EType,          //E
        EFlags,         //F
        GenReg,         //G
        ImmedData,      //I
        JmpRelOffset,   //J
        MemoryOnly,     //M
        OpOffset,       //O
        MMXPkdQWord,    //P
        QType,          //Q
        ModGenReg,      //R
        RegSegReg,      //S
        RegTestReg,     //T
        XMM128Reg,      //V
        WType,          //W
        DSSIMem,        //X
        ESDIMem,        //Y
        NamedRegister,
        TheNumberOne,
        Int3
    }

    public enum sOpCodeOperandType
    {
        None,
        BoundType,      //a
        Byte,           //b
        ByteOrWord,     //c
        DWord,          //d
        DQWord,         //dq
        Pointer,        //p
        QWMMXReg,       //pi
        Packed128FP,    //ps
        QWord,          //q
        PseudoDesc,     //s
        Scalar,         //ss
        DWordIntReg,    //si
        WordOrDWord,    //v
        Word            //w
    }

    public struct sOpCode
    {
        public UInt16 OpCode;
        public string Instruction;
        public sOpCodeAddressingMethod Op1AM, Op2AM, Op3AM;
        public sOpCodeOperandType Op1OT, Op2OT, Op3OT;
        public eGeneralRegister Register1, Register2, Register3;
        public bool ImmedOp1, ImmedOp2, ImmedOp3;
        public bool Op1UsesModRMRegSIB, Op2UsesModRMRegSIB, Op3UsesModRMRegSIB;
        public UInt64 UsageCount;

        //Old, to be deleted
        public String Operand1;
        public string Operand2;
        public string Operand3;
        public bool Op1IsReg, Op2IsReg, Op3IsReg;
        public byte Mod;
        public byte Reg;
        public byte RM;
        public byte SIB;
    }

    public class AAA : Instruct
    {
        public AAA()
        {
            mName = "AAA";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "ASCII Adjust After Addition ";
            mModFlags = eFLAGS.AF | eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.AL & 0xF) > 9 || mProc.regs.FLAGSB.AF)
            {
                mProc.regs.AL += 6;
                mProc.regs.AH += 1;
                mProc.regs.setFlagAF(true);
                mProc.regs.setFlagCF(true);
            }
            else
            {
                mProc.regs.setFlagAF(false);
                mProc.regs.setFlagCF(false);
            }
            mProc.regs.AL &= 0xF;
            #region Instructions
            /*Operation
                IF ((AL AND 0FH) > 9) OR (AF = 1)
                THEN
                AL ? AL + 6;
                AH ? AH + 1;
                AF ? 1;
                CF ? 1;
                ELSE
                AF ? 0;
                CF ? 0;
                FI;
                AL ? AL AND 0FH;
                Flags Affected
                The AF and CF flags are set to 1 if the adjustment results in a decimal carry; otherwise they are
                set to 0. The OF, SF, ZF, and PF flags are undefined.
                Exceptions (All Operating Modes)
                None.  
            */
            #endregion
        }
    }
    public class AAD : Instruct
    {
        public AAD()
        {
            mName = "AAD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Ascii Adjust for Division";
            mModFlags = eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.AL = (byte)((mProc.regs.AL + (CurrentDecode.Op1Value.OpByte * mProc.regs.AH)) & 0xff);
            mProc.regs.AH = 0;
            mProc.regs.setFlagSF(mProc.regs.AL);
            mProc.regs.setFlagZF(mProc.regs.AL);
            mProc.regs.setFlagPF(mProc.regs.AL);
            #region Instructions
            /*  Operation
                tempAL ? AL;
                tempAH ? AH;
                AL ? (tempAL + (tempAH ? imm8)) AND FFH; (* imm8 is set to 0AH for the AAD mnemonic *)
                AH ? 0
                The immediate value (imm8) is taken from the second byte of the instruction.
                Flags Affected
                The SF, ZF, and PF flags are set according to the resulting binary value in the AL register; the
                OF, AF, and CF flags are undefined.
                Exceptions (All Operating Modes)
                None.
             */
            #endregion
        }
    }
    public class AAM : Instruct
    {
        public AAM()
        {
            mName = "AAM";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "ASCII Adjust AX After Multiply";
            mModFlags = eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (sInstruction.bytes.Length > 1)
                mProc.regs.AH = (byte)(mProc.regs.AL / CurrentDecode.Op1Value.OpByte);
            else
                mProc.regs.AH = (byte)(mProc.regs.AL / 10);
            mProc.regs.AL %= 0x0a;
            mProc.regs.setFlagSF(mProc.regs.AL);
            mProc.regs.setFlagZF(mProc.regs.AL);
            mProc.regs.setFlagPF(mProc.regs.AL);
            #region Instructions
            /*
                Operation
                tempAL ? AL;
                AH ? tempAL / imm8; (* imm8 is set to 0AH for the AAM mnemonic *)
                AL ? tempAL MOD imm8;
                The immediate value (imm8) is taken from the second byte of the instruction.
                Flags Affected
                The SF, ZF, and PF flags are set according to the resulting binary value in the AL register. The
                OF, AF, and CF flags are undefined.
                Exceptions (All Operating Modes)
                None with the default immediate value of 0AH. If, however, an immediate value of 0 is used, it
                will cause a #DE (divide error) exception.
            */
            #endregion
        }
    }
    public class AAS : Instruct
    {
        public AAS()
        {
            mName = "AAS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Ascii Adjust for Subtraction";
            mModFlags = eFLAGS.SF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.AL & 0xF) > 9 || mProc.regs.FLAGSB.AF)
            {
                mProc.regs.AL -= 6;
                mProc.regs.AH -= 1;
                mProc.regs.setFlagAF(true);
                mProc.regs.setFlagCF(true);
            }
            else
            {
                mProc.regs.setFlagAF(false);
                mProc.regs.setFlagCF(false);
            }
            mProc.regs.AL &= 0xF;
            #region Instructions
            /*
                Operation
                IF ((AL AND 0FH) > 9) OR (AF = 1)
                THEN
                AL ? AL – 6;
                AH ? AH – 1;
                AF ? 1;
                CF ? 1;
                ELSE
                CF ? 0;
                AF ? 0;
                FI;
                AL ? AL AND 0FH;
                Flags Affected
                The AF and CF flags are set to 1 if there is a decimal borrow; otherwise, they are set to 0. The
                OF, SF, ZF, and PF flags are undefined.
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class ADC : Instruct
    {
        public ADC()
        {
            mName = "ADC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Add With Carry";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.CF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //CLR 09/02/2013 - Changed all calls to setFlagCF_ADC to use lOpValSigned instead of UnSigned

            /*Always 2 parameters*/
            sOpVal lPreVal1 = CurrentDecode.Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValUnsigned = CurrentDecode.Op1Value,
                   lOp1ValSigned = CurrentDecode.Op1Value;
            byte lCF = (byte)(mProc.regs.FLAGSB.CF ? 1 : 0);


            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);


            //CLR 02/24/2014 - Replaced UInt16, UInt23 & UInt64 with their signed siblings
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValUnsigned.OpByte += (byte)(lOp2Value.OpByte + lCF);
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpByte = (byte)((sbyte)(lOp1ValSigned.OpByte) + (sbyte)(lOp2Value.OpByte) + (sbyte)lCF);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpByte);
                    mProc.regs.setFlagCF_ADC(CurrentDecode.Op1Value.OpByte, lOp1ValSigned.OpByte, lOp2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValUnsigned.OpWord += (Word)(lOp2Value.OpWord + lCF);
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpWord = (Word)((Int16)(lOp1ValSigned.OpWord) + (Int16)(lOp2Value.OpWord) + (Int16)lCF);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpWord);
                    mProc.regs.setFlagCF_ADC(CurrentDecode.Op1Value.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValUnsigned.OpDWord += lOp2Value.OpDWord + lCF;
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpDWord = (DWord)((Int32)(lOp1ValSigned.OpDWord) + (Int32)(lOp2Value.OpDWord) + (Int32)lCF);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagCF_ADC(CurrentDecode.Op1Value.OpDWord, lOp1ValSigned.OpDWord, lOp2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValUnsigned.OpQWord += lOp2Value.OpQWord + lCF;
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpQWord = (QWord)((Int64)(lOp1ValSigned.OpQWord) + (Int64)(lOp2Value.OpQWord) + (Int64)lCF);
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagCF_ADC(CurrentDecode.Op1Value.OpQWord, lOp1ValSigned.OpQWord, lOp2Value.OpQWord);
                    break;
            }

            //Set the flags
            SetFlagsForAddition(mProc, lPreVal1, lOp2Value, lOp1ValSigned, lOp1ValUnsigned, CurrentDecode.Op1TypeCode);

            #region Instructions
            /*
                        Operation
                        DEST ? DEST + SRC;
                        Flags Affected
                        The OF, SF, ZF, AF, CF, and PF flags are set according to the result.
                        Protected Mode Exceptions
                        #GP(0) If the destination is located in a non-writable segment.
                        If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        If the DS, ES, FS, or GS register is used to access memory and it contains
                        a null segment selector.
                        #SS(0) If a memory operand effective address is outside the SS segment limit.
                        #PF(fault-code) If a page fault occurs.
                        #AC(0) If alignment checking is enabled and an unaligned memory reference is
                        made while the current privilege level is 3.
                        Real-Address Mode Exceptions
                        #GP If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        #SS If a memory operand effective address is outside the SS segment limit.
                        Virtual-8086 Mode Exceptions
                        #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        #SS(0) If a memory operand effective address is outside the SS segment limit.
                        #PF(fault-code) If a page fault occurs.
                        #AC(0) If alignment checking is enabled and an unaligned memory reference is
                        made.
             */
            #endregion
        }
    }
    public class ADD : Instruct
    {
        public ADD()
        {
            mName = "ADD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Add";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.CF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //CLR 09/02/2013 - Changed all calls to setFlagCF_ADC to use lOpValSigned instead of UnSigned

            /*Always 2 parameters*/
            sOpVal lPreVal1 = CurrentDecode.Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValUnsigned = CurrentDecode.Op1Value,
                   lOp1ValSigned = CurrentDecode.Op1Value;

            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    //lOp1ValUnsigned.OpByte += CurrentDecode.Op2Value.OpByte;
                    lOp1ValSigned.OpByte = (byte)((sbyte)lOp1ValSigned.OpByte + (sbyte)lOp2Value.OpByte);
                    mProc.regs.setFlagCF_Add(CurrentDecode.Op1Value.OpByte, lOp1ValSigned.OpByte, lOp2Value.OpByte);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    //lOp1ValUnsigned.OpWord += CurrentDecode.Op2Value.OpWord;
                    lOp1ValSigned.OpWord = (UInt16)((Int16)lOp1ValSigned.OpWord + (Int16)lOp2Value.OpWord);
                    mProc.regs.setFlagCF_Add(CurrentDecode.Op1Value.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    //lOp1ValUnsigned.OpDWord += CurrentDecode.Op2Value.OpDWord;
                    lOp1ValSigned.OpDWord = (UInt32)((Int32)lOp1ValSigned.OpDWord + (Int32)lOp2Value.OpDWord);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpDWord);
                   mProc.regs.setFlagCF_Add(CurrentDecode.Op1Value.OpDWord, lOp1ValSigned.OpDWord, lOp2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    //lOp1ValUnsigned.OpQWord += CurrentDecode.Op2Value.OpQWord;
                    lOp1ValSigned.OpQWord = (UInt64)((Int64)lOp1ValSigned.OpQWord + (Int64)lOp2Value.OpQWord);
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagCF_Add(CurrentDecode.Op1Value.OpQWord, lOp1ValSigned.OpQWord, lOp2Value.OpQWord);
                    break;
            }

            //    //Set the flags
            SetFlagsForAddition(mProc, lPreVal1, lOp2Value, lOp1ValSigned, lOp1ValUnsigned, CurrentDecode.Op1TypeCode);

            #region Instructions
            /*
                        Operation
                        DEST ? DEST + SRC;
                        Flags Affected
                        The OF, SF, ZF, AF, CF, and PF flags are set according to the result.
                        Protected Mode Exceptions
                        #GP(0) If the destination is located in a non-writable segment.
                        If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        If the DS, ES, FS, or GS register is used to access memory and it contains
                        a null segment selector.
                        #SS(0) If a memory operand effective address is outside the SS segment limit.
                        #PF(fault-code) If a page fault occurs.
                        #AC(0) If alignment checking is enabled and an unaligned memory reference is
                        made while the current privilege level is 3.
                        Real-Address Mode Exceptions
                        #GP If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        #SS If a memory operand effective address is outside the SS segment limit.
                        Virtual-8086 Mode Exceptions
                        #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
                        GS segment limit.
                        #SS(0) If a memory operand effective address is outside the SS segment limit.
                        #PF(fault-code) If a page fault occurs.
                        #AC(0) If alignment checking is enabled and an unaligned memory reference is
                        made.
             */
            #endregion
        }  
    }
    public class AND : Instruct
    {
        public AND()
        {
            mName = "AND";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Logical AND";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - Added setFlagSF to each case statement

            /*Always 2 parameters*/
            //capture value pre-operation
            sOpVal lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal /*lOp1ValUnsigned = Op1Value,*/
                   lOp1ValSigned = CurrentDecode.Op1Value;

            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte &= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpByte);
                    mProc.regs.setFlagSF(lOp1ValSigned.OpByte);
                    mProc.regs.setFlagZF(lOp1ValSigned.OpByte);
                    mProc.regs.setFlagPF(lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord &= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpWord);
                    mProc.regs.setFlagSF(lOp1ValSigned.OpWord);
                    mProc.regs.setFlagZF(lOp1ValSigned.OpWord);
                    mProc.regs.setFlagPF(lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord &= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagSF(lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagZF(lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagPF(lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord &= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagSF(lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagZF(lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagPF(lOp1ValSigned.OpQWord);
                    break;
            }
            //Set the flags
            mProc.regs.setFlagOF(false);
            mProc.regs.setFlagCF(false);
            #region Set SF
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.regs.setFlagSF(lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.regs.setFlagSF(lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.regs.setFlagSF(lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.regs.setFlagSF(lOp1ValSigned.OpQWord);
                    break;
            }
            #endregion

            //AF flag undefined
            #region Instructions
            /*
                Operation
                DEST ? DEST AND SRC;
                Flags Affected
                The OF and CF flags are cleared; the SF, ZF, and PF flags are set according to the result. The
                state of the AF flag is undefined.
                Protected Mode Exceptions
                #GP(0) If the destination operand points to a non-writable segment.
                If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                If the DS, ES, FS, or GS register contains a null segment selector.
                #SS(0) If a memory operand effective address is outside the SS segment limit.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made while the current privilege level is 3.
                Real-Address Mode Exceptions
                #GP If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                #SS If a memory operand effective address is outside the SS segment limit.
                Virtual-8086 Mode Exceptions
                #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                #SS(0) If a memory operand effective address is outside the SS segment limit.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made.
                */
            #endregion
        }
 
    }
    public class ARPL : Instruct
    {
        public ARPL()
        {
            mName = "ARPL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Adjust RPL Field of Segment Selector";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //CLR 09/12/2013 - Code block was empty, added logic
            if ((CurrentDecode.Op1Value.OpWord & 0x3) < (CurrentDecode.Op2Value.OpWord & 0x3))
            {
                mProc.regs.setFlagZF(true);
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)((CurrentDecode.Op1Value.OpWord & 0xFC) + (CurrentDecode.Op2Value.OpWord & 0x3)));
            }
            else
                mProc.regs.setFlagZF(false);
            #region Instructions
            #endregion

        }
    }
    public class BSF : Instruct
    {
        public BSF()
        {
            mName = "BSF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Scan Forward";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTopBitNum = 0;
            if (CurrentDecode.Op2TypeCode == TypeCode.UInt16)
                lTopBitNum = 15;
            else
                lTopBitNum = 31;

            if ((lTopBitNum == 15 && CurrentDecode.Op2Value.OpWord == 0) || (lTopBitNum == 31 && CurrentDecode.Op2Value.OpDWord == 0))
            {
                mProc.regs.setFlagZF(true);
                CurrentDecode.Op1Value.OpQWord = 0;
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);
            }
            else
            {
                mProc.regs.setFlagZF(false);
                for (int c = 0; c <= lTopBitNum; c++)
                    if (Misc.GetBits3(CurrentDecode.Op2Value.OpQWord, c, 1) == 1)
                    {
                        if (lTopBitNum == 15)
                        {
                            CurrentDecode.Op1Value.OpWord = (Word)c;
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
                        }
                        else
                        {
                            CurrentDecode.Op1Value.OpDWord = (DWord)c;
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);
                        }
            
                        return;
                    }
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
            }

            ///AF flag undefined
            #region Instructions
            #endregion
        }
    }
    public class BSR : Instruct
    {
        public BSR()
        {
            mName = "BSR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Scan Reverse";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTopBitNum = 0;
            if (CurrentDecode.Op2TypeCode == TypeCode.UInt16)
                lTopBitNum = 15;
            else
                lTopBitNum = 31;

            if ((lTopBitNum == 15 && CurrentDecode.Op2Value.OpWord == 0) || (lTopBitNum == 31 && CurrentDecode.Op2Value.OpDWord == 0))
            {
                mProc.regs.setFlagZF(true);
                CurrentDecode.Op1Value.OpQWord = 0;
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);
            }
            else
            {
                mProc.regs.setFlagZF(false);
                for (int c = lTopBitNum; c >= 0; c--)
                    if (Misc.GetBits3(CurrentDecode.Op2Value.OpQWord, c, 1) == 1)
                    {
                        if (lTopBitNum == 15)
                        {
                            CurrentDecode.Op1Value.OpWord = (Word)c;
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
                        }
                        else
                        {
                            CurrentDecode.Op1Value.OpDWord = (DWord)c;
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);
                        }
            
                        return;
                    }
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
            }
            ///AF flag undefined

            #region Instructions
            #endregion
        }
    }
    public class BSWAP : Instruct
    {
        public BSWAP()
        {
            mName = "BSWAP";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Byte Swap";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Updates to shifting logic

            UInt32 lDest = CurrentDecode.Op1Value.OpByte;

            lDest <<= 24;
            lDest += (UInt32)((CurrentDecode.Op1Value.OpDWord & 0x0000FF00) << 8);
            lDest += (UInt32)((CurrentDecode.Op1Value.OpDWord & 0x00FF0000) >> 8);
            lDest += (UInt32)((CurrentDecode.Op1Value.OpDWord & 0xFF000000) >> 24);

            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lDest);

            #region Instructions
            #endregion
        }
    }
    public class BT : Instruct
    {
        public BT()
        {
            mName = "BT";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Test";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {   //CLR 08/19/2015 - Updated to look like BTS without the S
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = CurrentDecode.Op1Value;

            if (CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
            {
                lTheBit = Misc.GetBits3(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
            }
            else
            {
                lTheByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8));
                mProc.regs.setFlagCF(BT.TestBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8)));
            }

        }

        public static bool TestBit(DWord Value, int BitToTest)
        {
            return Misc.GetBits3(Value, BitToTest, 1) == 1;
        }

    }
    public class BTC : Instruct
    {
        public BTC()
        {
            mName = "BTC";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Test and Compliment";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = CurrentDecode.Op1Value;

            if (CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
            {
                lTheBit = Misc.GetBits3(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                if (lTheBit == 0)
                    lOp1Val.OpDWord = Misc.setBit(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, true);
                else
                    lOp1Val.OpDWord = Misc.setBit(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, false);
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8));
                if (BT.TestBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8)))
                {
                    mProc.regs.setFlagCF(true);
                    lTheByte = Misc.setBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8), false);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8), lTheByte);
                }
                else
                {
                    mProc.regs.setFlagCF(false);
                    lTheByte = Misc.setBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8), true);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8), lTheByte);
                }
            }

            ///AF flag undefined
            #region Instructions
            #endregion
        }

    }
    public class BTR : Instruct
    {
        public BTR()
        {
            mName = "BTR";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Test And Reset";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = CurrentDecode.Op1Value;

            if (CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
            {
                lTheBit = Misc.GetBits3(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                lOp1Val.OpDWord = Misc.setBit(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, false);
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8));
                mProc.regs.setFlagCF(BT.TestBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8)));
                lTheByte = Misc.setBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8), false);
                mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8), lTheByte);
            }
            ///AF flag undefined
            #region Instructions
            #endregion

        }
    }
    public class BTS : Instruct
    {
        public BTS()
        {
            mName = "BTS";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Bit Test And SetC";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = CurrentDecode.Op1Value;

            if (CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
            {
                lTheBit = Misc.GetBits3(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                lOp1Val.OpDWord = Misc.setBit(CurrentDecode.Op1Value.OpDWord, (int)CurrentDecode.Op2Value.OpByte, true);
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8));
                mProc.regs.setFlagCF(BT.TestBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8)));
                lTheByte = Misc.setBit(lTheByte, (int)(CurrentDecode.Op2Value.OpDWord % 8), true);
                mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add + (CurrentDecode.Op2Value.OpDWord / 8), lTheByte);
            }
            ///AF flag undefined

            #region Instructions
            #endregion
        }
    }
    public class BOUND : Instruct
    {
        public BOUND()
        {
            mName = "BOUND";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Check Array Index Against Bounds";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //Operand1 = Array Index (signed integer)
            //Operand2 = Mem location with a par of signed word integers (double-word for 32 bit)
            //First dw integer is lower bound, 2nd is upper bound
            int lLowerBound = 0;
            int lUpperBound = 0;
            if (CurrentDecode.lOpSize16)
            {
                lLowerBound = (int)mProc.mem.GetWord(mProc, ref CurrentDecode, GetSegOverriddenAddress(mProc, CurrentDecode.Op2Add));
                lUpperBound = (int)mProc.mem.GetWord(mProc, ref CurrentDecode, GetSegOverriddenAddress(mProc, CurrentDecode.Op2Add + 2));
            }
            else
            {
                lLowerBound = (int)mProc.mem.GetDWord(mProc, ref CurrentDecode, GetSegOverriddenAddress(mProc, CurrentDecode.Op2Add));
                lUpperBound = (int)mProc.mem.GetDWord(mProc, ref CurrentDecode, GetSegOverriddenAddress(mProc, CurrentDecode.Op2Add + 4));
            }
            int lCheckVal = (int)CurrentDecode.Op1Value.OpDWord;
            if (lCheckVal < lLowerBound || lCheckVal > lUpperBound)
            {
                CurrentDecode.ExceptionThrown = true;
                CurrentDecode.ExceptionNumber = 0x5;
            }

            //NOTES:
            //CPU-generated (80186+) - BOUND RANGE EXCEEDED
            //Desc: Generated by BOUND instruction when the value to be tested is less than the indicated lower bound or greater than the indicated upper bound. 
            //Note: Returning from this interrupt re-executes the failing BOUND instruction 
        }
    }
    public class CALL : Instruct
    {
        public CALL()
        {
            mName = "CALL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Call Procedure";
            mModFlags = 0;
            mFlowReturnsLater = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lTempIP = mProc.regs.IP;
            UInt32 lTempEIP = mProc.regs.EIP;
            sInstruction ins = CurrentDecode;
            sInstruction insPush = CurrentDecode;

            switch (CurrentDecode.OpCode)
            {
                case 0xe8:
                case 0xff02:
                    insPush.Op1Add = Processor_80x86.REIP;
                    insPush.Op1Value    .OpDWord = mProc.regs.EIP;
                    insPush.Operand1IsRef = false;
                    mProc.PUSH.Impl(ref insPush);
                    if (insPush.ExceptionThrown)
                {
                    return;
                }
                ins.Op1Value = CurrentDecode.Op1Value;
                ins.Op1TypeCode = CurrentDecode.Op1TypeCode;
                ins.ChosenOpCode = CurrentDecode.ChosenOpCode;
                ins.Operand1IsRef = CurrentDecode.Operand1IsRef;
                ins.lOpSize16 = CurrentDecode.lOpSize16;
                ins.ChosenOpCode.Op1AM = CurrentDecode.ChosenOpCode.Op1AM;
//                if (CurrentDecode.OpCode == 0xe8)
//                    ins.ChosenOpCode.Op1AM = sOpCodeAddressingMethod.JmpRelOffset;
//                else
//                    ins.ChosenOpCode.Op1AM = sOpCodeAddressingMethod.DirectAddress;
                mProc.JMP.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
break;
                case 0xff03:
                    break;
                default:
                    insPush.Op1Add = Processor_80x86.RCS;
                    if (mProc.OperatingMode == ProcessorMode.Protected)
                        insPush.Op1Value.OpDWord = mProc.regs.CS.mWholeSelectorValue;
                    else
                        insPush.Op1Value.OpDWord = mProc.regs.CS.Value;
                    //CLR 08/15/2015 - Added Operand1IsRef=false
                    insPush.Operand1IsRef = false;
                    mProc.PUSH.Impl(ref insPush);
                    if (insPush.ExceptionThrown)
                {
                    
                    return;
                }
                    insPush.Op1Add = Processor_80x86.REIP;
                    insPush.Op1Value.OpDWord = mProc.regs.EIP;
                    insPush.Operand1IsRef = false;
                    mProc.PUSH.Impl(ref insPush);
                    insPush.Op1Add = 0x0;
                    if (insPush.ExceptionThrown)
                {
                    
                    return;
                }
                    if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected && mProc.regs.CS.mDescriptorNum > 0 && mProc.regs.CS.mDescriptorNum <= mProc.mGDTCache.Count)
                    {
//20240309
                        //get the descriptor for CS.  If it is a call gate 
                        if (mProc.mGDTCache[(int)(CurrentDecode.Op1Value.OpQWord >> 32) >> 3].access.m_SystemDescType == eSystemOrGateDescType.Call_Gate_32)
                        {
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, mProc.mGDTCache[(int)(CurrentDecode.Op1Value.OpQWord >> 32) >> 3].Base);
                            mProc.regs.EIP = (UInt32)mProc.mGDTCache[(int)(CurrentDecode.Op1Value.OpQWord >> 32) >> 3].Value & 0xFFFF;
                        }
                        else
                        {
                            //QWord didn't work
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord));
                            mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
                        }
                    }
                    else
                    {
//                        if (CurrentDecode.Op1TypeCode == TypeCode.UInt64)
                            //QWord didn't work
                            mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
//                        else
//                            mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
                        mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
            
                    }
                    break;
            }
            return;


            #region Instructions
            #endregion
        }
    }
    public class CALLF : Instruct
    {
        public CALLF()
        {
            mName = "CALLF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Call Procedure (far)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lTempIP = mProc.regs.IP;
            UInt32 lTempEIP = mProc.regs.EIP;
            sInstruction ins = CurrentDecode;

            ins.Op1Add = Processor_80x86.RCS;
            ins.Op1Value.OpDWord = mProc.regs.CS.Value;
            ins.Operand1IsRef = false;
            mProc.PUSH.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                
                return;
            }
            ins.Op1Add = Processor_80x86.RIP;
            ins.Op1Value.OpDWord = mProc.regs.EIP;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Add = 0x0;

            if (mProc.OperatingMode == ProcessorMode.Protected)
            {
                if (CurrentDecode.lOpSize16)
                {
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord));
                    mProc.regs.EIP = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add);
                }
                else
                {
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, (DWord)(CurrentDecode.Op1Value.OpQWord >> 32) & 0xFFFF);
                   mProc.regs.EIP = mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add);
                }
            }
            else
            {
                //20240309
                if (CurrentDecode.Op1TypeCode == TypeCode.UInt64)
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, (DWord)(CurrentDecode.Op1Value.OpQWord >> 0x20));
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, CurrentDecode.Op1Value.OpDWord >> 0x10);
                mProc.regs.EIP = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add);
            }

            #region Instructions
            #endregion
        }
    }
    public class CBW : Instruct
    {
        public CBW()
        {
            mName = "CBW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Convert Byte to Word";
            mModFlags = eFLAGS.SF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Change to more efficient logic
            if (CurrentDecode.lOpSize16)
            {
                //08/28/10 - Changed to a simpler calculation which is done by CWD
                //09/04/10 - Per Intel, if Operand Size = 32 then CWDE is executed instead
                mProc.regs.AX = Misc.SignExtend(mProc.regs.AL);
            }
            else  //CWDE
                mProc.regs.EAX = Misc.SignExtend(mProc.regs.AX);
            #region Instructions
            /*
                Operation
                IF OperandSize = 16 (* instruction = CBW *)
                THEN AX ? SignExtend(AL);
                ELSE (* OperandSize = 32, instruction = CWDE *)
                EAX ? SignExtend(AX);
                FI;
                Flags Affected
                None.
                Exceptions (All Operating Modes)
                None.                
                 */
            #endregion
        }
    }
    public class CLC : Instruct
    {
        public CLC()
        {
            mName = "CLC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Clear Carry Flag";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.setFlagCF(false);
            #region Instructions
            /*
            Operation
            CF ? 0;
            Flags Affected
            The CF flag is set to 0. The OF, ZF, SF, AF, and PF flags are unaffected.
            Exceptions (All Operating Modes)
            None.
    */
            #endregion
        }
    }
    public class CLD : Instruct
    {
        public CLD()
        {
            mName = "CLD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Clear Direction Flag";
            mModFlags = eFLAGS.DF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.setFlagDF(false);
            #region Instructions
            /*
            Operation
            DF ? 0;
            Flags Affected
            The DF flag is set to 0. The CF, OF, ZF, SF, AF, and PF flags are unaffected.
            Exceptions (All Operating Modes)
            None.
            */
            #endregion
        }
    }
    public class CLI : Instruct
    {
        public CLI()
        {
            mName = "CLI";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Clear Interrupt Flag";
            mModFlags = eFLAGS.IF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.mSTIAfterNextInstr = mProc.mSTICalled = false;
            //20240309 - this caused msdos to break
/*            if (mProc.regs.CPL > ePrivLvl.Kernel_Ring_0 && mProc.OperatingMode == ProcessorMode.Protected)
            {
                mProc.sCurrentDecode.ExceptionNumber = 13;
                mProc.sCurrentDecode.ExceptionErrorCode = 0;
                mProc.sCurrentDecode.ExceptionThrown = true;
            }
            else
*/                mProc.regs.setFlagIF(false);
            #region Instructions
            /*
            Operation
            IF PE = 0 (* Executing in real-address mode *)
            THEN
            IF ? 0; (* SetC Interrupt Flag *)
            ELSE
            IF VM = 0 (* Executing in protected mode *)
            THEN
            IF CPL ? IOPL
            THEN
            IF ? 0; (* SetC Interrupt Flag *)
            ELSE
            #GP(0);
            FI;
            FI;
            FI;
            Flags Affected
            The IF is set to 0 if the CPL is equal to or less than the IOPL; otherwise, it is not affected. The
            other flags in the EFLAGS register are unaffected.
            Protected Mode Exceptions
            #GP(0) If the CPL is greater (has less privilege) than the IOPL of the current
            program or procedure.
            Real-Address Mode Exceptions
            None.
            Virtual-8086 Mode Exceptions
            #GP(0) If the CPL is greater (has less privilege) than the IOPL of the current
            program or procedure.
            */
            #endregion
        }
    }
    public class CLTS : Instruct
    {
        public CLTS()
        {
            mName = "CLTS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Clear Task Switching Flag";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.CR0 &= 0xFFFFFFF7;
        }
    }
    public class CMC : Instruct
    {
        public CMC()
        {
            mName = "CMC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Compliment Carry Flag";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Changed to a more efficient calculation
            mProc.regs.setFlagCF(!mProc.regs.FLAGSB.CF);
            #region Instructions
            /*
             Operation
            CF ? NOT CF;
            Flags Affected
            The CF flag contains the complement of its original value. The OF, ZF, SF, AF, and PF flags are
            unaffected.
            Exceptions (All Operating Modes)
            None.
            */
            #endregion
        }
    }
    public class CMOVcc : Instruct
    {
        public CMOVcc()
        {
            mName = "CMOVcc";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Conditional Move";
            //mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            bool lExecuteMove = false;

            switch (CurrentDecode.RealOpCode)
            {
                case 0x0F40:   //CMOVO - Overflow
                    mName = "CMOVO";
                    if (mProc.regs.FLAGSB.OF)
                        lExecuteMove = true;
                    break;
                case 0x0F41:    //CMOVNO - Not Overflow
                    mName = "CMOVNO";
                    if (mProc.regs.FLAGSB.OF == false)
                        lExecuteMove = true;
                    break;
                case 0x0F42:    //CMOVB - Below
                    mName = "CMOVB";
                    if (mProc.regs.FLAGSB.CF == true)
                        lExecuteMove = true;
                    break;
                case 0x0F43:    //CMOVAE - Above or Equal
                    mName = "CMOVAE";
                    if (mProc.regs.FLAGSB.CF == false)
                        lExecuteMove = true;
                    break;
                case 0x0F44:    //CMOVE - Equal
                    mName = "CMOVE";
                    if (mProc.regs.FLAGSB.ZF)
                        lExecuteMove = true;
                    break;
                case 0x0F45:    //CMOVNE - Not Equal
                    mName = "CMOVNE";
                    if (mProc.regs.FLAGSB.ZF == false)
                        lExecuteMove = true;
                    break;
                case 0x0F46:    //CMOVBE - Below or Equal
                    mName = "CMOVBE";
                    if (mProc.regs.FLAGSB.CF == true || mProc.regs.FLAGSB.ZF == true)
                        lExecuteMove = true;
                    break;
                case 0x0F47:    //CMOVA - Above
                    mName = "CMOVA";
                    if (mProc.regs.FLAGSB.CF == false && mProc.regs.FLAGSB.ZF == false)
                        lExecuteMove = true;
                    break;
                case 0x0F48:   //CMOVS - Sign
                    mName = "CMOVS";
                    if (mProc.regs.FLAGSB.SF)
                        lExecuteMove = true;
                    break;
                case 0x0F49:   //CMOVNS - Not Sign
                    mName = "CMOVNS";
                    if (!mProc.regs.FLAGSB.SF)
                        lExecuteMove = true;
                    break;
                case 0x0F4A:   //CMOVP - Parity
                    mName = "CMOVP";
                    if (mProc.regs.FLAGSB.PF)
                        lExecuteMove = true;
                    break;
                case 0x0F4B:   //CMOVNP/CMOVPO - Not Parity
                    mName = "CMOVNP";
                    if (!mProc.regs.FLAGSB.PF)
                        lExecuteMove = true;
                    break;
                case 0x0F4c:    //CMOVL - Less than
                    mName = "CMOVL";
                    if (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF)
                        lExecuteMove = true;
                    break;
                case 0x0F4d:    //CMOVGE - Greater or Equal
                    mName = "CMOVGE";
                    if (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)
                        lExecuteMove = true;
                    break;
                case 0x0F4e:    //CMOVLE Less than or Equal
                    mName = "CMOVLE";
                    if (mProc.regs.FLAGSB.ZF == true || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                        lExecuteMove = true;
                    break;
                case 0x0F4f:    //CMOVG - Greater
                    mName = "CMOVG";
                    if (mProc.regs.FLAGSB.ZF == false && (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF))
                        lExecuteMove = true;
                    break;
                default:
                    throw new Exception("You missed one!");
            }
            if (lExecuteMove)
            {
                Processor_80x86.MOV.Impl(ref CurrentDecode);
    
            }
        }
    }
    public class CMP : Instruct
    {
        sOpVal lOp2Value;
        sOpVal lOp1ValSigned; 
        public CMP()
        {
            mName = "CMP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Compare Two Operands";
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            /*Always 2 parameters*/
            //capture value pre-operation
            lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            lOp1ValSigned = CurrentDecode.Op1Value;

            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte -= lOp2Value.OpByte;
                    mProc.regs.setFlagCF_SUB_CMP(CurrentDecode.Op1Value.OpByte, lOp1ValSigned.OpByte, lOp2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord -= lOp2Value.OpWord;
                    mProc.regs.setFlagCF_SUB_CMP(CurrentDecode.Op1Value.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord -= lOp2Value.OpDWord;
                    mProc.regs.setFlagCF_SUB_CMP(CurrentDecode.Op1Value.OpDWord, lOp1ValSigned.OpDWord, lOp2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord -= lOp2Value.OpQWord;
                    mProc.regs.setFlagCF_SUB_CMP(CurrentDecode.Op1Value.OpQWord, lOp1ValSigned.OpQWord, lOp2Value.OpQWord);
                    break;
            }
            SetFlagsForSubtraction(mProc, CurrentDecode.Op1Value, lOp2Value, lOp1ValSigned, CurrentDecode.Op1TypeCode);
        }
    }
    public class CMPSB : Instruct
    {
        sOpVal Value1, Value2;
        public CMPSB()
        {
            mName = "CMPSB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare String Bytes";
            mModFlags = eFLAGS.SF;
            Value1 = new sOpVal();
            Value2 = new sOpVal();
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Re-enabled NeedToInterruptLoop

            DWord lSource = 0;
            DWord lDest = 0;

            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;
            bool lRepeating = mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT;
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            //CMPSB has no parameters, but rather uses DS:SI, ES:DI, so we need to grab the values from memory
            //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason

            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.mCurrInstructAddrSize16);

            do
            {
                Value1.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown) { return; }
                Value2.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, lDest);
                if (CurrentDecode.ExceptionThrown) { return; }
                sOpVal lPreVal1 = Value1;

                //notice that we don't save the result, we just use it to save the flags
                Value1.OpByte -= Value2.OpByte;
                lLoopCounter -= 1;
                SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.Byte);
                mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpByte, Value1.OpByte, Value2.OpByte);
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 1, true, true, true, false);
                if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_NOT_ZERO && mProc.regs.FLAGSB.ZF == true)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_ZERO && mProc.regs.FLAGSB.ZF == false)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);
        }
    }
    public class CMPSW : Instruct
    {
        sOpVal Value1, Value2;
        public CMPSW()
        {
            mName = "CMPSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare String Word";
            Value1 = new sOpVal();
            Value2 = new sOpVal();
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Re-enabled NeedToInterruptLoop
            DWord lSource = 0;
            DWord lDest = 0;
            bool lRepeating = mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT;

            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            if (!mProc.mCurrInstructOpSize16)
            {
                mProc.CMPSD.UsageCount++;
                mProc.CMPSD.Impl(ref CurrentDecode);
                //lCMPSD.Impl(ref CurrentDecode);
    
                return;
            }

            //CMPSW has no parameters, but rather uses DS:SI, ES:DI, so we need to grab the values from memory
            //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason

            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.mCurrInstructAddrSize16);

            do
            {
                Value1.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown) { return; }
                Value2.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, lDest);
                if (CurrentDecode.ExceptionThrown) { return; }
                sOpVal lPreVal1 = Value1;

                //notice that we don't save the result, we just use it to save the flags
                Value1.OpWord -= Value2.OpWord;
                lLoopCounter -= 1;
                SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.UInt16);
                mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpWord, Value1.OpWord, Value2.OpWord);
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 2,true, true, true, false);
                if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_NOT_ZERO && mProc.regs.FLAGSB.ZF)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_ZERO && !mProc.regs.FLAGSB.ZF)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);
        }
    }
    public class CMPSD : Instruct
    {
        sOpVal Value1, Value2;
        public CMPSD()
        {
            mName = "CMPSD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare String Double Word";
            Value1 = new sOpVal();
            Value2 = new sOpVal();
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/02/2013 - CLR: Re-enabled NeedToInterruptLoop
            DWord lSource = 0;
            DWord lDest = 0;

            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;
            bool lRepeating = mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            //CMPSW has no parameters, but rather uses DS:SI, ES:DI, so we need to grab the values from memory
            //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason

            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.mCurrInstructAddrSize16);

            do
            {
                Value1.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown) { return; }
                Value2.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, lDest);
                if (CurrentDecode.ExceptionThrown) { return; }
                sOpVal lPreVal1 = Value1;

                //notice that we don't save the result, we just use it to save the flags
                Value1.OpDWord -= Value2.OpDWord;
                lLoopCounter -= 1;
                SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.UInt32);
                mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpDWord, Value1.OpDWord, Value2.OpDWord);
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 4, true, true, true, false);
                if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_NOT_ZERO && mProc.regs.FLAGSB.ZF == true)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if (mProc.mRepeatCondition == Processor_80x86.REPEAT_TILL_ZERO && mProc.regs.FLAGSB.ZF == false)
                {
                    mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                    break;
                }
                else if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);
        }
    }
    public class CMPXCHG : Instruct
    {
        public CMPXCHG()
        {
            mName = "CMPXCHG";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = false;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Compare and Exchange";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.ChosenOpCode.OpCode)
            {
                case 0x0FB0:    //8-bit
                    if (mProc.regs.AL == CurrentDecode.Op1Value.OpByte)
                    {
                        mProc.regs.setFlagZF(true);
                        mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpByte);
                    }
                    else
                    {
                        mProc.regs.setFlagZF(false);
                        mProc.regs.AL = CurrentDecode.Op1Value.OpByte;
                    }
                    break;
                case 0x0FB1:
                    if (CurrentDecode.lOpSize16)    //16-bit
                        if (mProc.regs.AX == CurrentDecode.Op1Value.OpWord)
                        {
                            mProc.regs.setFlagZF(true);
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpWord);
                        }
                        else
                        {
                            mProc.regs.setFlagZF(false);
                            mProc.regs.AX = CurrentDecode.Op1Value.OpWord;
                        }
                    else     //32-bit
                        if (mProc.regs.EAX == CurrentDecode.Op1Value.OpDWord)
                        {
                            mProc.regs.setFlagZF(true);
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpDWord);
                        }
                        else
                        {
                            mProc.regs.setFlagZF(false);
                            mProc.regs.EAX = CurrentDecode.Op1Value.OpDWord;
                        }
                    break;
                case 0x0FC7:
                    {
                        UInt64 lValue = (mProc.regs.EDX << 32) | mProc.regs.EAX;
                        if (lValue == CurrentDecode.Op2Value.OpQWord)
                        {
                            mProc.regs.setFlagZF(true);
                            UInt64 lValue2 = (mProc.regs.ECX << 32) | mProc.regs.EBX;
                            mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lValue);
                        }
                        else
                        {
                            mProc.regs.setFlagZF(false);
                            mProc.regs.EDX = (DWord)(CurrentDecode.Op1Value.OpQWord >> 32);
                            mProc.regs.EAX = CurrentDecode.Op1Value.OpDWord;
                        }
                        break;
                    }
                default:
                    throw new Exception("Invalid opcode for CMPXCHG");

            }

        }

    }
    public class CPUID : Instruct
    {
        public CPUID()
        {
            mName = "CPUID";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "CPU Identification";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (mProc.regs.EAX)
            {
                case 0x00:
                    mProc.regs.EAX = 0x01;
                    mProc.regs.EBX = 0x68747541;
                    mProc.regs.ECX = 0x444D4163;
                    mProc.regs.EDX = 0x69746E65;
                    break;
                case 0x01:
                    mProc.regs.EAX = 0x40F8B2;
                    mProc.regs.EBX = 0x10000; //0x01010800;
                    mProc.regs.ECX = 0x0;
                    //mProc.regs.EDX = 0x178BFBFF;
                    //CLR 08/19/2015 - Changed from 2010 to A010 to add CMOV
                    //CLR 08/19/2015 - Changed from A010 to A110 to addd CX8
                    mProc.regs.EDX = 0xA110; //(FPU, PGE, TSC=2011)
                    if (mProc.mSystem.bFPUEnabled)
                        mProc.regs.EDX |= 1;
                    mProc.regs.EDX |= 1 << 8;
                    break;
                case 0x80000000:
                    mProc.regs.EAX = 0x1;
                    mProc.regs.EBX = 0x68747541;
                    mProc.regs.ECX = 0x444D4163;
                    mProc.regs.EDX = 0x2000;
                    break;
                case 0x80000001:
                    mProc.regs.EAX = 0x40F8B2;
                    mProc.regs.EBX = 0x0;
                    mProc.regs.ECX = 0x1f;
                    mProc.regs.EDX = 0x2011; //0xEBD3FBFF;
                    break;
                case 0x80000002:
                    mProc.regs.EAX = 0x20444D41;
                    mProc.regs.EBX = 0x6C687441;
                    mProc.regs.ECX = 0x74286E6F;
                    mProc.regs.EDX = 0x3620296D;
                    break;
                case 0x80000003:
                    mProc.regs.EAX = 0x32582034;
                    mProc.regs.EBX = 0x61754420;
                    mProc.regs.ECX = 0x6F43206C;
                    mProc.regs.EDX = 0x50206572;
                    break;
                case 0x80000004:
                    mProc.regs.EAX = 0x65636F72;
                    mProc.regs.EBX = 0x726F7373;
                    mProc.regs.ECX = 0x30383320;
                    mProc.regs.EDX = 0x00002B30;
                    break;
                default:
                    mProc.regs.EAX = 0x01;
                    mProc.regs.EBX = 0x0;
                    mProc.regs.ECX = 0x0;
                    mProc.regs.EDX = 0x0;
                    break;
            }
        }
    }
    public class CWD : Instruct
    {
        public CWD()
        {
            mName = "CWD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Convert Word to Doubleword";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //08/28/10 - Changed last param from 15 to 7 - its AH after all, duuh!
            //09/04/10 - Per Intel, if Operand Size = 32 then CDQ is executed instead
            if (CurrentDecode.lOpSize16)
            {
                if (Misc.getBit(mProc.regs.AX, 15) == 1)
                    mProc.regs.DX = 0xFFFF;
                else
                    mProc.regs.DX = 0;
            }
            else  //
            {
                if (Misc.getBit(mProc.regs.EAX, 31) == 1)
                    mProc.regs.EDX = 0xFFFFFFFF;
                else
                    mProc.regs.EDX = 0;

            }
            #region Instructions
            /*
                Operation
                IF OperandSize = 16 (* CWD instruction *)
                THEN DX ? SignExtend(AX);
                ELSE (* OperandSize = 32, CDQ instruction *)
                EDX ? SignExtend(EAX);
                FI;
                Flags Affected
                None.
                Exceptions (All Operating Modes)
                None.
    */
            #endregion
        }
    }
    public class DAA : Instruct
    {
        //TODO: Verify handling of the CF for this instruction
        public DAA()
        {
            mName = "DAA";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Decimal Adjust AL after Addition";
            mModFlags = eFLAGS.CF | eFLAGS.AF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            bool lTempCF = false;
            byte lAL = mProc.regs.AL;
            if (((lAL & 0x0F) > 0x9) || mProc.regs.FLAGSB.AF)
            {
                lTempCF = ((mProc.regs.AL > 0xf9) || mProc.regs.FLAGSB.CF);
                mProc.regs.AL += 0x06;
                //lTempCF = mProc.regs.FLAGSB.CF;
                ////TODO: Fix this CF if the one below it isn't correct!!!
                //mProc.regs.setFlagCF(lALBefore, new cValue((byte)mProc.regs.AL));

                //                mProc.regs.setFlagCF(lTempCF | mProc.regs.FLAGSB.CF);
                mProc.regs.setFlagAF(true);
            }
            else
            {
                mProc.regs.setFlagAF(false);
            }

            if ((lAL > 0x99) || mProc.regs.FLAGSB.CF)
            {
                mProc.regs.AL += 0x60;

                lTempCF = true;
            }
            else
                lTempCF = false;

            mProc.regs.setFlagCF(lTempCF);
            mProc.regs.setFlagSF(mProc.regs.AL);
            //            mProc.regs.setFlagSF(mProc.regs.AL >> 7 == 1);
            //mProc.regs.setFlagSF((UInt16)(mProc.regs.AL));
            mProc.regs.setFlagZF(mProc.regs.AL);
            mProc.regs.setFlagPF(mProc.regs.AL);
            mProc.regs.setFlagOF(false);
            #region Instructions
            /*
            Operation
            IF (((AL AND 0FH) > 9) or AF = 1)
            THEN
            AL ? AL + 6;
            CF ? CF OR CarryFromLastAddition; (* CF OR carry from AL ? AL + 6 *)
            AF ? 1;
            ELSE
            AF ? 0;
            FI;
            IF ((AL AND F0H) > 90H) or CF = 1)
            THEN
            AL ? AL + 60H;
            CF ? 1;
            ELSE
            CF ? 0;
            FI;
            Flags Affected
            The CF and AF flags are set if the adjustment of the value results in a decimal carry in either
            digit of the result (see the “Operation” section above). The SF, ZF, and PF flags are set according
            to the result. The OF flag is undefined.
            Exceptions (All Operating Modes)
            None.
            */
            #endregion
        }
    }
    public class DAS : Instruct
    {
        public DAS()
        {
            mName = "DAS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Decimal Adjust AL after Subtraction";
            mModFlags = eFLAGS.CF | eFLAGS.AF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            bool lCFBefore = false;
            byte lALBefore = mProc.regs.AL;
            if ((mProc.regs.AL & 0xF) > 0x9 || (Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.AF) == 1))
            {
                mProc.regs.AL -= 0x6;

                lCFBefore = mProc.regs.FLAGSB.CF;
                mProc.regs.setFlagCF_SUB_CMP(lALBefore, mProc.regs.AL, 0x6);

                mProc.regs.setFlagCF(lCFBefore | mProc.regs.FLAGSB.CF);
                mProc.regs.setFlagAF(true);
            }
            else
                mProc.regs.setFlagAF(false);

            if (mProc.regs.AL > 0x9F || mProc.regs.FLAGSB.CF)
            {
                mProc.regs.AL -= 0x60;

                mProc.regs.setFlagCF(true);
                mProc.regs.setFlagAF(true);
            }
            else
            {
                mProc.regs.setFlagCF(false);
            }
            mProc.regs.setFlagSF((UInt16)(mProc.regs.AL));
            mProc.regs.setFlagZF((UInt16)(mProc.regs.AL));
            mProc.regs.setFlagPF((UInt16)(mProc.regs.AL));


            #region Instructions
            /*
                Operation
                IF (AL AND 0FH) > 9 OR AF = 1
                THEN
                AL ? AL ? 6;
                CF ? CF OR BorrowFromLastSubtraction; (* CF OR borrow from AL ? AL ? 6 *)
                AF ? 1;
                ELSE AF ? 0;
                FI;
                IF ((AL > 9FH) or CF = 1)
                THEN
                AL ? AL ? 60H;
                CF ? 1;
                ELSE CF ? 0;
                FI;
                Example
                SUB AL, BL Before: AL=35H BL=47H EFLAGS(OSZAPC)=
                After: AL=EEH BL=47H EFLAGS(0SZAPC)=010111
                DAA Before: AL=EEH BL=47H EFLAGS(OSZAPC)=010111
                After: AL=88H BL=47H EFLAGS(0SZAPC)=X10111
                Flags Affected
                The CF and AF flags are set if the adjustment of the value results in a decimal borrow in either
                digit of the result (see the “Operation” section above). The SF, ZF, and PF flags are set according
                to the result. The OF flag is undefined.
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class DEC : Instruct
    {
        public DEC()
        {
            mName = "DEC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Decrement by 1";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;

            //Do the math!
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte--;
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    CurrentDecode.Op2Value.OpByte = 0xff;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    mProc.regs.setFlagOF_Sub(lOp1Value.OpByte, CurrentDecode.Op2Value.OpByte, CurrentDecode.Op1Value.OpByte);
                    mProc.regs.setFlagZF(lOp1Value.OpByte);
                    mProc.regs.setFlagAF(lPreVal1.OpByte, lOp1Value.OpByte);
                    mProc.regs.setFlagPF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord--;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    CurrentDecode.Op2Value.OpWord = 0xffff;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    mProc.regs.setFlagOF_Sub(lOp1Value.OpWord, CurrentDecode.Op2Value.OpWord, CurrentDecode.Op1Value.OpWord);
                    mProc.regs.setFlagZF(lOp1Value.OpWord);
                    mProc.regs.setFlagAF(lPreVal1.OpWord, lOp1Value.OpWord);
                    mProc.regs.setFlagPF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord--;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    CurrentDecode.Op2Value.OpDWord = 0xffffffff;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    mProc.regs.setFlagOF_Sub(lOp1Value.OpDWord, CurrentDecode.Op2Value.OpDWord, CurrentDecode.Op1Value.OpDWord);
                    mProc.regs.setFlagZF(lOp1Value.OpDWord);
                    mProc.regs.setFlagAF(lPreVal1.OpDWord, lOp1Value.OpDWord);
                    mProc.regs.setFlagPF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord--;
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    CurrentDecode.Op2Value.OpQWord = 0xffffffffffffffff;
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    mProc.regs.setFlagOF_Sub(lOp1Value.OpQWord, CurrentDecode.Op2Value.OpQWord, CurrentDecode.Op1Value.OpQWord);
                    mProc.regs.setFlagZF(lOp1Value.OpQWord);
                    mProc.regs.setFlagAF(lPreVal1.OpQWord, lOp1Value.OpQWord);
                    mProc.regs.setFlagPF(lOp1Value.OpQWord);
                    break;
            }
            #region Instructions
            /*
                Description
                Subtracts 1 from the destination operand, while preserving the state of the CF flag. The destination
                operand can be a register or a memory location. This instruction allows a loop counter to be
                updated without disturbing the CF flag. (To perform a decrement operation that updates the CF
                flag, use a SUB instruction with an immediate operand of 1.)
                This instruction can be used with a LOCK prefix to allow the instruction to be executed atomically.
                Operation
                DEST ? DEST – 1;
                Flags Affected
                The CF flag is not affected. The OF, SF, ZF, AF, and PF flags are set according to the result.
                Protected Mode Exceptions
                #GP(0) If the destination operand is located in a non-writable segment.
                If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                If the DS, ES, FS, or GS register contains a null segment selector.
                #SS(0) If a memory operand effective address is outside the SS segment limit.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made while the current privilege level is 3.
                Real-Address Mode Exceptions
                #GP If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                #SS If a memory operand effective address is outside the SS segment limit.
                Virtual-8086 Mode Exceptions
                #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
                GS segment limit.
                #SS(0) If a memory operand effective address is outside the SS segment limit.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made.
                 */
            #endregion

        }
    }
    public class DIV : Instruct
    {
        public DIV()
        {
            mName = "DIV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Unsigned Divide";
            mModFlags = eFLAGS.SF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1Value.OpQWord == 0)
            {
                //Attempted divide by zero!
                CurrentDecode.ExceptionNumber = 0x0;
                CurrentDecode.ExceptionThrown = true;
    
                return;
            }

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    Word lTempAX = mProc.regs.AX;
                    mProc.regs.AL = (byte)(lTempAX / (UInt16)CurrentDecode.Op1Value.OpByte);
                    mProc.regs.AH = (byte)(lTempAX % (UInt16)CurrentDecode.Op1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    UInt32 lDXAX = (UInt32)((mProc.regs.DX << 16) + mProc.regs.AX);
                    mProc.regs.AX = (UInt16)(lDXAX / (UInt32)CurrentDecode.Op1Value.OpWord);
                    mProc.regs.DX = (UInt16)(lDXAX % (UInt32)CurrentDecode.Op1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    UInt64 lEDXEAX = (UInt64)((mProc.regs.EDX << 32) + mProc.regs.EAX); //09/03/2013 - CLR: Fixed shift to be 32 instead of 16
                    mProc.regs.EAX = (UInt32)(lEDXEAX / (UInt64)CurrentDecode.Op1Value.OpDWord);
                    mProc.regs.EDX = (UInt32)(lEDXEAX % (UInt64)CurrentDecode.Op1Value.OpDWord);
                    break;
                default:
                    throw new Exception("d'oh!");
            }
        }
    }
    //public class DB : Instruct
    //{
    //    public DB()
    //    {
    //        mName = "DB";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        mOp.OpCode = 0xDB;
    //        mOp.Instruction = "DB";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }

    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    //public class DC : Instruct
    //{
    //    public DC()
    //    {
    //        mName = "DC";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        mOp.OpCode = 0xDC;
    //        mOp.Instruction = "DC";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }
    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    //public class DD : Instruct
    //{
    //    public DD()
    //    {
    //        mName = "DD";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        mOp.OpCode = 0xDD;
    //        mOp.Instruction = "DD";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }

    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    //public class DE : Instruct
    //{
    //    public DE()
    //    {
    //        mName = "DE";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        mOp.OpCode = 0xDE;
    //        mOp.Instruction = "DE";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }
    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    //public class DF : Instruct
    //{
    //    public DF()
    //    {
    //        mName = "DF";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        mOp.OpCode = 0xDF;
    //        mOp.Instruction = "DF";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }
    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    //public class DW : Instruct
    //{
    //    public DW()
    //    {
    //        mName = "DW";
    //        mProc8086 = true;
    //        mProc8088 = true;
    //        mProc80186 = true;
    //        mProc80286 = true;
    //        mProc80386 = true;
    //        mProc80486 = true;
    //        mProcPentium = true;
    //        mProcPentiumPro = true;
    //        mDescription = "Escape Opcode";
    //        mModFlags = 0;
    //        sOpCode mOp = new sOpCode();
    //        //            mOp.OpCode = 0xDW;
    //        mOp.Instruction = "DW";
    //        mOp.Operand1 = "ESC";
    //        OpCodes.Add(mOp);
    //    }

    //    public override void Impl(ref sInstruction CurrentDecode)
    //    {
    //        throw new ExceptionNumber("Invalid OpCode " + Name);
    //        #region Instructions
    //        #endregion
    //    }
    //}
    public class ENTER : Instruct
    {
        public ENTER()
        {
            mName = "ENTER";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Make stack frame";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sOpVal lOp2Value = new sOpVal();
            lOp2Value.OpByte = (byte)(CurrentDecode.Op2Value.OpByte % 0x20);
            sInstruction ins = CurrentDecode;
            sOpVal FrameTemp;

            //Debug.WriteLine("Before ENTER, SP=" + OpDecoder.ValueToHex(mProc.regs.SP, 4) + ", BP=" + OpDecoder.ValueToHex(mProc.regs.BP, 4));

            FrameTemp = new sOpVal();

            if (mProc.mCurrInstructAddrSize16)
            {
                ins.Op1Add = Processor_80x86.RBP;
                ins.Op1Value.OpWord = mProc.regs.BP;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                FrameTemp.OpWord = mProc.regs.SP;
            }
            else
            {
                ins.Op1Add = Processor_80x86.REBP;
                ins.Op1Value.OpDWord = mProc.regs.EBP;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                FrameTemp.OpDWord = mProc.regs.ESP;
            }
            if (lOp2Value.OpByte > 0)
            {
                for (int c = 1; c < lOp2Value.OpByte - 1; c++)
                {
                    if (!CurrentDecode.lOpSize16) //32 bit operand size
                    {
                        if (!mProc.mCurrInstructAddrSize16) //32 bit address size
                        {
                            mProc.regs.EBP -= 4;
                            ins.Op1Add = Processor_80x86.REBP;
                            ins.Op1Value.OpDWord = mProc.regs.EBP;
                            ins.Operand1IsRef = false;
                            mProc.PUSH.Impl(ref ins);
                            if (ins.ExceptionThrown)
                            {
                                
                                return;
                            }
                        }
                        else                    //16 bit address size
                        {
                            mProc.regs.BP -= 2;
                            ins.Op1Add = Processor_80x86.RBP;
                            ins.Op1Value.OpWord = mProc.regs.BP;
                            ins.Operand1IsRef = false;
                            mProc.PUSH.Impl(ref ins);
                            if (ins.ExceptionThrown)
                            {
                                
                                return;
                            }
                        }
                    }
                    else                 //16 bit operand size
                    {
                        if (!mProc.mCurrInstructAddrSize16) //32 bit address size
                        {
                            mProc.regs.EBP -= 4;
                            ins.Op1Add = Processor_80x86.REBP;
                            ins.Op1Value.OpDWord = mProc.regs.EBP;
                            ins.Operand1IsRef = false;
                            mProc.PUSH.Impl(ref ins);
                            if (ins.ExceptionThrown)
                            {
                                
                                return;
                            }
                        }
                        else                     //16 bit address size
                        {
                            mProc.regs.BP -= 2;
                            ins.Op1Add = Processor_80x86.RBP;
                            ins.Op1Value.OpWord = mProc.regs.BP;
                            ins.Operand1IsRef = false;
                            mProc.PUSH.Impl(ref CurrentDecode);
                            if (ins.ExceptionThrown)
                            {
                                
                                return;
                            }
                        }

                    }
                }
                ins.Op1Value.OpDWord = FrameTemp.OpDWord;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
            }
            //CONTINUE
            if (!mProc.mCurrInstructAddrSize16)             //32 bit address size
            {
                mProc.regs.EBP = FrameTemp.OpDWord;
                mProc.regs.ESP = mProc.regs.EBP - CurrentDecode.Op1Value.OpDWord;
            }
            else
            {
                mProc.regs.BP = FrameTemp.OpWord;
                mProc.regs.SP = (Word)(mProc.regs.BP - CurrentDecode.Op1Value.OpWord);
            }
            #region Instructions
            /*
                 NestingLevel ? NestingLevel MOD 32
                IF StackSize = 32
                THEN
                Push(EBP) ;
                FrameTemp ? ESP;
                ELSE (* StackSize = 16*)
                Push(BP);
                FrameTemp ? SP;
                FI;
                IF NestingLevel = 0
                THEN GOTO CONTINUE;
                FI;
                IF (NestingLevel > 0)
                FOR i ? 1 TO (NestingLevel ? 1)
                DO
                IF OperandSize = 32
                THEN
                IF StackSize = 32
                EBP ? EBP ? 4;
                Push([EBP]); (* doubleword push *)
                ELSE (* StackSize = 16*)
                BP ? BP ? 4;
                Push([BP]); (* doubleword push *)
                FI;
                ELSE (* OperandSize = 16 *)
                IF StackSize = 32
                THEN
                EBP ? EBP ? 2;
                Push([EBP]); (* word push *)
                ELSE (* StackSize = 16*)
                BP ? BP ? 2;
                Push([BP]); (* word push *)
                FI;
                FI;
                OD;
                IF OperandSize = 32
                THEN
                Push(FrameTemp); (* doubleword push *)
                ELSE (* OperandSize = 16 *)
                Push(FrameTemp); (* word push *)
                FI;
                GOTO CONTINUE;
                FI;
                CONTINUE:
                IF StackSize = 32
                THEN
                EBP ? FrameTemp
                ESP ? EBP ? Size;
                ELSE (* StackSize = 16*)
                BP ? FrameTemp
                SP ? BP ? Size;
                FI;
                END;
                */
            #endregion
        }
    }

    public class FABS : Instruct
    {
        public FABS()
        {
            mName = "FABS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Absolute Value";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //D9 E1 FABS Valid Valid Replace ST with its absolute value.
            mProc.mFPU.mDataReg[mProc.mFPU.ST(0)] = Math.Abs(mProc.mFPU.mDataReg[mProc.mFPU.ST(0)]);
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FADD : Instruct
    {
        public FADD()
        {
            mName = "FADD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Add";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.OpCode)
            {
                //D8 /0 FADD m32fp Add m32fp to ST(0) and store result in ST(0)
                case 0xD800:
                    mProc.mFPU.Add(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), 1, false, false);
                    break;
                //DA /0 FIADD m32int Add m32int to ST(0) and store result in ST(0)
                case 0xDA00:
                    mProc.mFPU.Add(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, true, false);
                    break;
                case 0xDEC0:
                    //FADDP Add ST(0) to ST(1), store result in ST(1), and pop the register stack
                    mProc.mFPU.Add(1, 0, 2, false, true);
                    break;
                case 0xDEC1:
                    //FADDP Add ST(0) to ST(1), store result in ST(1), and pop the register stack
                    mProc.mFPU.Add(1, 0, 2, false, true);
                    break;
                case 0xDE00: case 0xDE80: case 0xDE81: case 0xDE82: case 0xDE83: case 0xDE84: case 0xDE85: case 0xDE86: case 0xDE87:
                    mProc.mFPU.Add(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpWord), 1, true, false);
                    break;
                default:
                    throw new Exception($"OpCode {CurrentDecode.OpCode.ToString("X")} Not coded yet");
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FCHS : Instruct
    {
        public FCHS()
        {
            mName = "FCHS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Change Sign of ST(0)";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            Int64 lTemp = System.Convert.ToInt64(mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0)));
            lTemp = ~(lTemp);
            mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = System.Convert.ToDouble(lTemp);
        }
    }
    public class FCMOV : Instruct
    {
        public FCMOV()
        {
            mName = "FCMOV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Floating Point Conditional Move";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.OpCode)
            {
                //FCMOVB - Move if Below (CF set)
                case 0xDAC0:
                case 0xDAC1:
                case 0xDAC2:
                case 0xDAC3:
                case 0xDAC4:
                case 0xDAC5:
                case 0xDAC6:
                case 0xDAC7:
                    if (mProc.regs.FLAGSB.CF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDAC0))];
                    return;
                //FCMOVE Move if equal (ZF=1)*/
                case 0xDAC8:
                case 0xDAC9:
                case 0xDACA:
                case 0xDACB:
                case 0xDACC:
                case 0xDACD:
                case 0xDACE:
                case 0xDACF:
                    if (mProc.regs.FLAGSB.ZF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDAC8))];
                    return;
                //FCMOVBE Move if below or equal (CF=1 or ZF=1)*/
                case 0xDAD0:
                case 0xDAD1:
                case 0xDAD2:
                case 0xDAD3:
                case 0xDAD4:
                case 0xDAD5:
                case 0xDAD6:
                case 0xDAD7:
                    if (mProc.regs.FLAGSB.ZF || mProc.regs.FLAGSB.CF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDAD0))];
                    return;
                //FCMOVE Move if unordered (PF=1)*/
                case 0xDAD8:
                case 0xDAD9:
                case 0xDADA:
                case 0xDADB:
                case 0xDADC:
                case 0xDADD:
                case 0xDADE:
                case 0xDADF:
                    if (mProc.regs.FLAGSB.PF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDAD8))];
                    return;
                //FCMOVNB Move if not below (CF=0)*/
                case 0xDBC0:
                case 0xDBC1:
                case 0xDBC2:
                case 0xDBC3:
                case 0xDBC4:
                case 0xDBC5:
                case 0xDBC6:
                case 0xDBC7:
                    if (!mProc.regs.FLAGSB.CF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDAC0))];
                    return;
                //FCMOVNE Move if not equal (ZF=0)*/
                case 0xDBC8:
                case 0xDBC9:
                case 0xDBCA:
                case 0xDBCB:
                case 0xDBCC:
                case 0xDBCD:
                case 0xDBCE:
                case 0xDBCF:
                    if (!mProc.regs.FLAGSB.ZF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDBC8))];
                    return;
                //FCMOVNBE Move if not below or equal (CF=0 AND ZF=0)*/
                case 0xDBD0:
                case 0xDBD1:
                case 0xDBD2:
                case 0xDBD3:
                case 0xDBD4:
                case 0xDBD5:
                case 0xDBD6:
                case 0xDBD7:
                    if (!mProc.regs.FLAGSB.CF && !mProc.regs.FLAGSB.ZF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDBD0))];
                    return;
                //FCMOVNU Move if not unordered (PF=0)*/
                case 0xDBD8:
                case 0xDBD9:
                case 0xDBDA:
                case 0xDBDB:
                case 0xDBDC:
                case 0xDBDD:
                case 0xDBDE:
                case 0xDBDF:
                    if (!mProc.regs.FLAGSB.PF)
                        mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDBD8))];
                    return;
                default:
                    throw new Exception("ruh roh yo");
            }
        }
    }
    public class FCOM : Instruct
    {
        public FCOM()
        {
            mName = "FCOM";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Compare Real";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            Double lReg0;
            Double lReg1;
            lReg0 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0));
            byte[] bytes;

            switch (CurrentDecode.OpCode)
            {
                case 0xDC03:
                    //DC /3 FCOMP m64fp Compare ST(0) with m64fp and pop register stack.
                    lReg0 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0));
                    bytes = new byte[8];
                    FLD.GetBytes(ref CurrentDecode, mProc, 0, CurrentDecode.Op1Add, ref bytes);
                    lReg1 = BitConverter.ToDouble(bytes,0);
                    Compare(lReg0, lReg1);
                    mProc.mFPU.Pop();
                    break;
                case 0xDED9:    //FUCOMPP
                    //DE D9 FCOMPP Valid Valid Compare ST(0) with ST(1) and pop register stack twice
                    lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(1));
                    Compare(lReg0, lReg1);
                    mProc.mFPU.Pop();
                    mProc.mFPU.Pop();
                    break;
                case 0xD8D0:
                case 0xD8D1:
                case 0xD8D2:
                case 0xD8D3:
                case 0xD8D4:
                case 0xD8D5:
                case 0xD8D6:
                case 0xD8D7:
                    //D8 D0+i FCOM ST(i) Compare ST(0) with ST(i). - Same as below without the pop
                    lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xD8D0)));
                    Compare(lReg0, lReg1);
                    break;
                case 0xD8D8:
                case 0xD8D9:
                case 0xD8DA:
                case 0xD8DB:
                case 0xD8DC:
                case 0xD8DD:
                case 0xD8DE:
                case 0xD8DF:
                    //D8 D8+i FCOMP ST(i) Compare ST(0) with ST(i) and pop register stack.
                    //D8 D9 FCOMP Compare ST(0) with ST(1) and pop register stack.
                    lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xD8D8)));
                    Compare(lReg0, lReg1);
                    mProc.mFPU.Pop();
                    break;
                case 0xDC02:
                    Compare(lReg0, CurrentDecode.Op2Value.OpQWord);
                    break;
                case 0xDA03:
                    //FICOMP m32int - Compare ST(0) with m32int and pop stack register
                    lReg0 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0));
                    Compare(lReg0, CurrentDecode.Op1Value.OpDWord);
                    mProc.mFPU.Pop();
                    break;
                default:
                case 0xDDE1:
                case 0xDDE8:
                case 0xDDE9:
                case 0xDDEA:
                case 0xDDEB:
                case 0xDDEC:
                case 0xDDED:
                case 0xDDEE:
                case 0xDDEF:
                    if (CurrentDecode.OpCode == 0xDDE1)
                        lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(1));
                    else
                        lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDDE8)));
                    Compare(lReg0, lReg1);

                    if (CurrentDecode.OpCode == 0xDDE1)
                        return;
                    mProc.mFPU.Pop();
                    break;
                case 0xDB06: //FCOMI
                    lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST((int)(CurrentDecode.OpCode - 0xDB00)));
                    CompareI(lReg0, lReg1);
                    break;
                    throw new Exception("Not coded yet");
            }

        }
        public void Compare(Double Val1, Double Val2)
        {
            if (Double.IsNaN(Val1) || Double.IsNaN(Val2))
            {
                mProc.mFPU.mStatusReg.InvalidOp = true;
                if (mProc.mFPU.mControlReg.InvalidOpMask == true)
                    mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC1 = true;
                return;
            }
            if (Val1 > Val2)
                mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC1 = false;
            else if (Val1 < Val2)
            {
                mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = false;
                mProc.mFPU.mStatusReg.CC0 = true;
            }
            else
            {
                mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC0 = false;
                mProc.mFPU.mStatusReg.CC3 = true;
            }
        }
        public void CompareI(Double Val1, Double Val2)
        {
            if (Double.IsNaN(Val1) || Double.IsNaN(Val2))
            {
                mProc.regs.setFlagZF(true);
                mProc.regs.setFlagPF(true);
                mProc.regs.setFlagCF(true);
            }
            else if (Val1 > Val2)
            {
                mProc.regs.setFlagZF(false);
                mProc.regs.setFlagPF(false);
                mProc.regs.setFlagCF(false);
            }
            else if (Val2 > Val1)
            {
                mProc.regs.setFlagZF(false);
                mProc.regs.setFlagPF(false);
                mProc.regs.setFlagCF(true);
            }
            else //(Val2 == Val1)
            {
                mProc.regs.setFlagZF(true);
                mProc.regs.setFlagPF(false);
                mProc.regs.setFlagCF(false);
            }

        }
    }
    public class FCLEX : Instruct
    {
        public FCLEX()
        {
            mName = "FCLEX";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Clear Exceptions";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.mFPU.mStatusReg.Parse((Word)(mProc.mFPU.mStatusReg.Value & 0x7F00));
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FDIV : Instruct
    {
        public FDIV()
        {
            mName = "FDIV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Divide";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte[] bytes;
            switch (CurrentDecode.OpCode)
            {
                case 0xDC06:
                    bytes = mProc.mem.Chunk(mProc, ref CurrentDecode, 0, CurrentDecode.Op1Add, 8);
                    mProc.mFPU.Divide(0, System.Convert.ToDouble(BitConverter.ToDouble(bytes, 0)), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, false, false);
                    break;
                case 0xDEF8:
                case 0xDEF9:
                case 0xDEFA:
                case 0xDEFB:
                case 0xDEFC:
                case 0xDEFD:
                case 0xDEFE:
                case 0xDEFF:
                    UInt32 idx = CurrentDecode.OpCode - 0xDEF8;
                    //Divide ST(i) by ST(0), store result in ST(i), and pop the register stack.
                    //DE F9 FDIVP Divide ST(1) by ST(0), store result in ST(1), and pop the register stack
                    mProc.mFPU.Divide(idx, 0, 2, false, true);
                    //mProc.mFPU.Divide(1, 0, 2, false, true);
                    break;
                case 0xDA06:
                    //Divide ST(0) by m32int and store result in ST(0)
                    mProc.mFPU.Divide(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, true, false);
                    break;
                case 0xD8F0:
                case 0xD8F1:
                case 0xD8F2:
                case 0xD8F3:
                case 0xD8F4:
                case 0xD8F5:
                case 0xD8F6:
                case 0xD8F7:
                    //D8 F0+i FDIV ST(0), ST(i) Divide ST(0) by ST(i) and store result in ST(0)
                    mProc.mFPU.Divide(0, CurrentDecode.OpCode - 0xD8F0, 2, false, false);
                    break;
                case 0xD806:
                    //Divide ST(0) by m32fp and store result in ST(0)
                    bytes = mProc.mem.Chunk(mProc, ref CurrentDecode, 0, CurrentDecode.Op1Add, 4);
                    Double lTemp = System.Convert.ToDouble(BitConverter.ToSingle(bytes, 0));
                    mProc.mFPU.Divide(lTemp, 0, 1, false, false);
                    break;
                default:
                    throw new Exception($"OpCode {CurrentDecode.OpCode.ToString("X")} Not coded yet");
            }

            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FDIVR : Instruct
    {
        public FDIVR()
        {
            mName = "FDIVR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Reverse Divide";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.OpCode)
            {
                case 0xDA07:
                    //DA /7 FIDIVR m32int Valid Valid Divide m32int by ST(0) and store result in ST(0).
                    mProc.mFPU.Divide(CurrentDecode.Op1Value.OpDWord, 0, 1, true, false);
                    break;
                //D8 /7 FDIVR m32fp Valid Valid Divide m32fp by ST(0) and store result in ST(0).
                case 0xD807:
                    mProc.mFPU.Divide((BitConverter.Int64BitsToDouble((long)CurrentDecode.Op1Value.OpDWord)), 0, 1, false, false);
                    break;
                case (0xDCF0):
                case (0xDCF1):
                case (0xDCF2):
                case (0xDCF3):
                case (0xDCF4):
                case (0xDCF5):
                case (0xDCF6):
                case (0xDCF7):
                    //DC F0+i FDIVR ST(i), ST(0) Valid Valid Divide ST(0) by ST(i) and store result in ST(i).
                    mProc.mFPU.Divide(CurrentDecode.OpCode - 0xDCF0, 0, 2, false, false);
                    break;
                case 0xDE07:
                    //FIDIVR DE /7 FIDIVR m16int Divide m16int by ST(0) and store result in ST(0)
                    mProc.mFPU.Divide(CurrentDecode.Op1Value.OpWord, 0, 1, true, false);
                    break;
                case 0xDEF0:
                case 0xDEF1:
                case 0xDEF2:
                case 0xDEF3:
                case 0xDEF4:
                case 0xDEF5:
                case 0xDEF6:
                case 0xDEF7:
                    //DE F0+i FDIVRP ST(i), ST(0) Divide ST(0) by ST(i), store result in ST(i), and pop the register stack
                    mProc.mFPU.Divide(CurrentDecode.OpCode - 0xDEF0, 0, 2, false, true);
                    break;
                default:
                    throw new Exception("Not coded yet");
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FINIT : Instruct
    {
        public FINIT()
        {
            mName = "FINIT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Initialize";
            mModFlags = 0;
            FPUInstruction = false; //CPU instruction that affects the FPU
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.FWAIT.Impl(ref CurrentDecode);

            mProc.mFPU.Init();
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FILD : Instruct
    {
        public FILD()
        {
            mName = "FILD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Load Integer";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            try
            {
                switch (CurrentDecode.OpCode)
                {
                    case 0xDB00:
                        mProc.mFPU.PushInt(System.Convert.ToInt32(CurrentDecode.Op1Value.OpDWord));
                        break;
                    case 0xDF00:
                        mProc.mFPU.PushInt(System.Convert.ToInt16(CurrentDecode.Op1Value.OpWord));
                        break;
                    case 0xDF05:
                        mProc.mFPU.PushInt((Int64)CurrentDecode.Op1Value.OpQWord);
                        break;
                }
            }
            catch
            {
                //Do something here!!!
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FIST : Instruct
    {
        public FIST()
        {
            mName = "FIST";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Integer";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sOpVal OpVal = new sOpVal(0);

            switch (CurrentDecode.OpCode)
            {
                case 0xDF02:
                    //DF /2 FIST m16int Store ST(0) in m16int
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)System.Convert.ToInt16(mProc.mFPU.GetDataRegSingle(mProc.mFPU.ST(0))));
                    break;
                case 0xDB02:
                    //DB /2 FIST m32int Store ST(0) in m32int
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (DWord)System.Convert.ToInt32(mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0))));

                        //mProc.mFPU.mStatusReg.InvalidOp = true;
                        //DecodedInstruction.ExceptionNumber = 0x10;
                        //DecodedInstruction.ExceptionThrown = true;
                        //DecodedInstruction.ExceptionAddress = DecodedInstruction.InstructionAddress;
                        //DecodedInstruction.ExceptionErrorCode = 0;

                        break;
                case 0xDF03:
                    try
                    {
                        //DF /3 FISTP m16int Store ST(0) in m16int and pop register stack
                        mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)System.Convert.ToInt16(mProc.mFPU.Pop()));
                    }
                    catch
                    {
                        mProc.mFPU.mStatusReg.InvalidOp = true;
                        CurrentDecode.ExceptionNumber = 0x10;
                        CurrentDecode.ExceptionThrown = true;
                        CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                        CurrentDecode.ExceptionErrorCode = 0;
                    }
                    break;
                case 0xDB03:
                    try
                    {
                        mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (DWord)System.Convert.ToInt32(mProc.mFPU.Pop()));
                    }
                    catch
                    {
                        mProc.mFPU.mStatusReg.InvalidOp = true;
                        CurrentDecode.ExceptionNumber = 0x10;
                        CurrentDecode.ExceptionThrown = true;
                        CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                        CurrentDecode.ExceptionErrorCode = 0;
                    }
                    break;
                case 0xDF07:
                    try
                    {
                        mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (QWord)System.Convert.ToInt64(mProc.mFPU.Pop()));
                    }
                    catch
                    {
                        mProc.mFPU.mStatusReg.InvalidOp = true;
                        CurrentDecode.ExceptionNumber = 0x10;
                        CurrentDecode.ExceptionThrown = true;
                        CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                        CurrentDecode.ExceptionErrorCode = 0;
                    }
                    break;

            }

            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FLDCW : Instruct
    {
        public FLDCW()
        {
            mName = "FLDCW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Load Control Word";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.mFPU.mControlReg.Parse(CurrentDecode.Op1Value.OpWord);
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FLD : Instruct
    {
        public FLD()
        {
            mName = "FLD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Load Floating Point Value";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte[] bytes;

            switch (CurrentDecode.OpCode)
            {
                case 0xD900:
                    //D9 /0 FLD m32fp Valid Valid Push m32fp onto the FPU register stack.
                    bytes = new byte[4];
                    FLD.GetBytes(ref CurrentDecode, mProc, 0, CurrentDecode.Op1Add, ref bytes);
                    mProc.mFPU.Push(System.Convert.ToDouble(BitConverter.ToSingle(bytes, 0)));
                    //mProc.mFPU.Push(BitConverter.ToDouble(new byte[] { (byte)(Op1Value.OpDWord & 0xFF), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF00) >> 8),
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF0000) >> 16),
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF000000) >> 24) }, 0));
                    break;

                case 0xDD00:
                    //DD /0 FLD m64fp Valid Valid Push m64fp onto the FPU register stack.
                    bytes = new byte[8];
                    FLD.GetBytes(ref CurrentDecode, mProc, 0, CurrentDecode.Op1Add, ref bytes);
                    mProc.mFPU.Push(BitConverter.ToDouble(bytes, 0));
                    //mProc.mFPU.Push(BitConverter.ToDouble(new byte[] { (byte)(Op1Value.OpDWord & 0xFF), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF00) >> 8),
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF0000) >> 16),
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF000000) >> 24), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF00000000) >> 32), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF0000000000) >> 40), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF000000000000) >> 48), 
                    //                                                   (byte)((Op1Value.OpDWord & 0xFF00000000000000) >> 56)}, 0));
                    break;
                case 0xDB05:
                    //DB /5 FLD FLD m80fp Push m80fp onto the FPU register stack.
                    bytes = new byte[10];
                    FLD.GetBytes(ref CurrentDecode, mProc, 0, CurrentDecode.Op1Add, ref bytes);
                    mProc.mFPU.Push(BitConverter.ToDouble(bytes, 0));
                    break;
                case 0xD9C0:
                case 0xD9C1:
                case 0xD9C2:
                case 0xD9C3:
                case 0xD9C4:
                case 0xD9C5:
                case 0xD9C6:
                case 0xD9C7:
                    //D9 C0+i FLD ST(i) Valid Valid Push ST(i) onto the FPU register stack.
                    mProc.mFPU.Push(mProc.mFPU.mDataReg[mProc.mFPU.ST((int)CurrentDecode.OpCode & 0x7)]);
                    break;
                default:
                    throw new Exception("Not coded yet");
            }

            #region Instructions
            /*
    */
            #endregion
        }
        static internal void GetBytes(ref sInstruction CurrentDecode, Processor_80x86 pProc, DWord Seg, DWord Ofs, ref byte[] bytes)
        {
            bytes = pProc.mem.Chunk(pProc, ref CurrentDecode, 0, Ofs, (uint)bytes.Length);
        }
    }
    public class FLD1 : Instruct
    {
        public FLD1()
        {
            mName = "FLD1";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Push 1 onto stack";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.mFPU.Push(System.Convert.ToDouble(1));
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FLDZ : Instruct
    {
        public FLDZ()
        {
            mName = "FLDZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Push 0.0 onto stack";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //clr 08/17/2015 - Changed (1) to (0)
            mProc.mFPU.Push(System.Convert.ToDouble(0));
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FMUL : Instruct
    {
        public FMUL()
        {
            mName = "FMUL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Multiply";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {

            switch (CurrentDecode.OpCode)
            {
                case 0xD801:
                    //D8 /1 FMUL m32fp Multiply ST(0) by m32fp and store result in ST(0)
                    mProc.mFPU.Multiply(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, false, false);
                    break;
                case 0xD8C8:
                case 0xD8C9:
                case 0xD8CA:
                case 0xD8CB:
                case 0xD8CC:
                case 0xD8CD:
                case 0xD8CE:
                case 0xD8CF:
                    //D8 C8+i FMUL ST(0), ST(i) Multiply ST(0) by ST(i) and store result in ST(0)
                    mProc.mFPU.Multiply(0,CurrentDecode.OpCode - 0xD8C8, 2, false, false);
                    break;
                case 0xDC01:
                    //DC /1 FMUL m64fp Multiply ST(0) by m64fp and store result in ST(0)
                    mProc.mFPU.Multiply((BitConverter.Int64BitsToDouble((long)CurrentDecode.Op1Value.OpQWord)), 0, 1, false, false);
                    break;
                    //DE C8+i FMULP ST(i), ST(0) Multiply ST(i) by ST(0), store result in ST(i), and pop the register stack
                case 0xDEC8:
                case 0xDEC9:
                case 0xDECA:
                case 0xDECB:
                case 0xDECC:
                case 0xDECD:
                case 0xDECE:
                case 0xDECF:
                    mProc.mFPU.Multiply(CurrentDecode.OpCode - 0xDEC8, 0, 2, false, true);
                    break;
                    //DC C8+i FMUL ST(i), ST(0) Multiply ST(i) by ST(0) and store result in ST(i)
                case 0xDCC8:
                case 0xDCC9:
                case 0xDCCa:
                case 0xDCCb:
                case 0xDCCc:
                case 0xDCCd:
                case 0xDCCe:
                case 0xDCCf:
                    mProc.mFPU.Multiply(CurrentDecode.OpCode - 0xDCC8, 0, 2, false, false);
                    break;
                default:
                    throw new Exception("FMUL opcode not coded yet");
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FIMUL : Instruct
    {
        public FIMUL()
        {
            mName = "FIMUL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Integer Multiply";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
             switch (CurrentDecode.OpCode)
            {
                case 0xDA01:
                    //DA /1 FIMUL m32int Multiply ST(0) by m32int and store result in ST(0)
                    mProc.mFPU.Multiply(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, true, false);
                    break;
                default:
                    throw new Exception("FIMUL opcode not coded yet");
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FRNDINT : Instruct
    {
        public FRNDINT()
        {
            mName = "FRNDINT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Round ST(0) to Integer";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.mFPU.mDataReg[mProc.mFPU.ST(0)] = System.Math.Round(mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0)));
        }
    }
    public class FSAVE : Instruct
    {
        public FSAVE()
        {
            mName = "FSAVE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Floating Point Value";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lOp1Add = CurrentDecode.Op1Add + 24;
            mProc.FSTENV.Impl(ref CurrentDecode);
            if (CurrentDecode.ExceptionThrown)
            {
    
                return;
            }

            //HACK: need to code based on OpSize16
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[0]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[1]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[2]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[3]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[4]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[5]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[6]));
            lOp1Add += 8;
            mProc.mem.SetQWord(mProc, ref CurrentDecode, lOp1Add, System.Convert.ToUInt64(mProc.mFPU.DataReg[7]));

            mProc.mFPU.mStatusReg = new FPUStatus(); ;
            mProc.mFPU.mTagReg = new FPUTagReg(0);
            mProc.mFPU.mLastData = 0;
            mProc.mFPU.mLastIP = 0;
            mProc.mFPU.mLastOpCode = 0;

        }
    }
    public class FST : Instruct
    {
        public FST()
        {
            mName = "FST";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Floating Point Value";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte[] bytes;

            switch (CurrentDecode.OpCode)
            {
                //FST m32fp Copy ST(0) to m32fp
                case 0xD902:
                //D9 /3 FSTP m32fp Copy ST(0) to m32fp and pop register stack.
                case 0xD903:
                    bytes = BitConverter.GetBytes(mProc.mFPU.GetDataRegSingle(mProc.mFPU.ST(0)));
                    for (int cnt = 0; cnt < bytes.Count(); cnt++)
                        mProc.mem.SetByte(mProc, ref CurrentDecode, (DWord)(CurrentDecode.Op1Add + cnt), bytes[cnt]);
                    if (CurrentDecode.OpCode == 0xD903)
                        mProc.mFPU.Pop();
                    break;
                case 0xDD02:
                //DD /2 FST m64fp Valid Valid Copy ST(0) to m64fp.
                case 0xDD03:
                    //DD/3 FSTP m64fp Valid Valid Copy ST(0) to m64fp and pop register stack.
                    bytes = BitConverter.GetBytes(mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0)));
                    for (int cnt = 0; cnt < bytes.Count(); cnt++)
                        mProc.mem.SetByte(mProc, ref CurrentDecode, (DWord)(CurrentDecode.Op1Add + cnt), bytes[cnt]);
                    if (CurrentDecode.OpCode == 0xDD03)
                        mProc.mFPU.Pop();
                    break;
                case 0xDDD8:
                case 0xDDD9:
                case 0xDDDA:
                case 0xDDDB:
                case 0xDDDC:
                case 0xDDDD:
                case 0xDDDE:
                case 0xDDDF:
                    //DD D8+i FSTP Copy ST(0) to ST(i) and pop register stack.
                    mProc.mFPU.mDataReg[mProc.mFPU.ST((int)CurrentDecode.OpCode - 0xDDD8)] = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0));
                    mProc.mFPU.Pop();
                    break;
                default:
                    throw new Exception("Not coded yet");
            }

            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FSTSW : Instruct
    {
        public FSTSW()
        {
            mName = "FSTSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Status Word";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.OpCode)
            {
                case 0xDD07:
                    if (mProc.mSystem.bFPUEnabled)
                        mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mFPU.mStatusReg.Value);
                    else
                        mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
                    //mProc.mem.SetWord(mProc, ref DecodedInstruction, Op1Add, mProc.mFPU.mStatusReg.Value);
                    break;
                case 0xDFE0:
                    if (mProc.mSystem.bFPUEnabled)
                        mProc.regs.AX = mProc.mFPU.mStatusReg.Value;
                    else
                        mProc.regs.AX = 0;
                    break;
            }
            #region Instructions
            /*
    */
            #endregion

        }
    }
    public class FSTCW : Instruct
    {
        public FSTCW()
        {
            mName = "FSTCW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Control Word";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.mSystem.bFPUEnabled)
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mFPU.mControlReg.Value);
            else
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, 0);
            #region Instructions
            /*
    */
            #endregion

        }
    }
    public class FSTENV : Instruct
    {
        public FSTENV()
        {
            mName = "FSTENV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Store Environment";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lOp1Add = CurrentDecode.Op1Add;

            if (mProc.OperatingMode == ProcessorMode.Protected)
                //Protected mode
                if (CurrentDecode.lOpSize16)
                {   //16 bit Protected
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastIP));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandSegSel));
                }
                else
                {   //32 bit Protected
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastIP));
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOpCode & 0x7FF));
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr));
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mTagReg.Value);
                }
            else
                //Real or VMode
                if (CurrentDecode.lOpSize16)
                {   //16 bit Real/VMode
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastIP & 0xFFFF));
                    lOp1Add += 2;
                    Word lTemp = (Word)(mProc.mFPU.mLastIP & 0xF0000 >> 4);
                    lTemp |= (Word)(mProc.mFPU.mLastOpCode & 0x7FF);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, lTemp);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xF0000 >> 4));
                }
                else
                {   //32 bit Real/VMode
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastIP & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    DWord lTemp = (DWord)(mProc.mFPU.mLastIP & 0xFFFF0000 >> 4);
                    lTemp |= (Word)(mProc.mFPU.mLastOpCode & 0x7FF);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, lOp1Add, lTemp);
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, lOp1Add, (DWord)(mProc.mFPU.mLastIP & 0xFFFF0000 >> 4));
                }

            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FSUB : Instruct
    {
        public FSUB()
        {
            mName = "FSUB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Subtract";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.OpCode)
            {
                /*FISUB m32int Subtract m32int from ST(0) and store result in ST(0)*/
                case 0xDA04:
                    mProc.mFPU.Sub(0, System.Convert.ToDouble(CurrentDecode.Op1Value.OpDWord), FPU.FPU_DEST_INTERNAL_SOURCE_EXTERNAL, true, false, false);
                    break;
                /*FSUB ST(0), ST(i) Subtract ST(i) from ST(0) and store result in ST(0)*/
                case 0xD8E0:
                case 0xD8E1:
                case 0xD8E2:
                case 0xD8E3:
                case 0xD8E4:
                case 0xD8E5:
                case 0xD8E6:
                case 0xD8E7:
                    mProc.mFPU.Sub(0, CurrentDecode.OpCode - 0xD8E0, 2, false, false, false);
                    break;
                case 0xDEE1:
                    /*FSUBRP */
                    mProc.mFPU.Sub(1, 0, 2, false, true, true);
                    break;
                case 0xDEE9:
                    /*FSUBP*/
                    mProc.mFPU.Sub(1, 0, 2, false, true, false);
                    break;
                default:
                    throw new Exception("Not coded yet");
            }
            #region Instructions
            /*
    */
            #endregion
        }
    }
    public class FUCOM : Instruct
    {
        public FUCOM()
        {
            mName = "FUCOM";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Compare";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            Double lReg0 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(0));
            Double lReg1 = mProc.mFPU.GetDataRegDouble(mProc.mFPU.ST(1));
            switch (CurrentDecode.OpCode)
            {
                case 0xDAE9:    //FUCOMPP
                    if (Double.IsNaN(lReg0) || Double.IsNaN(lReg1))
                        mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC1 = true;
                    else if (lReg0 > lReg1)
                        mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC1 = false;
                    else if (lReg0 < lReg1)
                    {
                        mProc.mFPU.mStatusReg.CC3 = mProc.mFPU.mStatusReg.CC2 = false;
                        mProc.mFPU.mStatusReg.CC0 = true;
                    }
                    else
                    {
                        mProc.mFPU.mStatusReg.CC2 = mProc.mFPU.mStatusReg.CC0 = false;
                        mProc.mFPU.mStatusReg.CC3 = true;
                    }
                    mProc.mFPU.Pop();
                    mProc.mFPU.Pop();
                    break;
                default:
                    throw new Exception("Not coded yet");
            }

        }
    }
    public class FWAIT : Instruct
    {
        public FWAIT()
        {
            mName = "FWAIT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Wait on FPU to clear #BUSY then process pending exceptions";
            mModFlags = 0;
            FPUInstruction = false;  //NOTE: This is a CPU instruction which waits FOR the FPU to be non-busy
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            while (mProc.Signals.BUSY)
                System.Threading.Thread.Yield();
            if (mProc.mFPU.mStatusReg.ZeroDivide)
            {
                CurrentDecode.ExceptionNumber = 0x10;
                CurrentDecode.ExceptionThrown = true;
                CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                CurrentDecode.ExceptionErrorCode = 0;
    
                //mProc.ExceptionErrorCode = 0x4;
            }
            #region Instructions
            #endregion
        }
    }
    public class FXCH : Instruct
    {
        public FXCH()
        {
            mName = "FXCH";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "FPU: Exchange Register Contents";
            mModFlags = 0;
            FPUInstruction = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            Double lTemp;
            switch (CurrentDecode.OpCode)
            {
                case 0xD9C8:
                case 0xD9C9:
                case 0xD9Ca:
                case 0xD9Cb:
                case 0xD9Cc:
                case 0xD9Cd:
                case 0xD9Ce:
                case 0xD9Cf:
                    lTemp = mProc.mFPU.DataReg[mProc.mFPU.ST(0)];
                    mProc.mFPU.DataReg[mProc.mFPU.ST(0)] = mProc.mFPU.DataReg[mProc.mFPU.ST((int)CurrentDecode.OpCode - 0xD9C8)];
                    mProc.mFPU.DataReg[mProc.mFPU.ST((int)CurrentDecode.OpCode - 0xD9C8)] = lTemp;
                    break;

            }
        }
    }

    public class HLT : Instruct
    {
        public HLT()
        {
            mName = "HLT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "HALT";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //mProc.regs.setFlagIF(true);
            if (!VerifyCPL0(mProc, ref CurrentDecode))
            {
    
                return;
            }
            //System.Diagnostics.Debug.WriteLine("HALT called from: " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.IP.ToString("X8"));
            mProc.HaltProcessor = true;
            #region Instructions
            /*
                Operation
                Enter Halt state;
                Flags Affected
                None.
                Protected Mode Exceptions
                #GP(0) If the current privilege level is not 0.
                Real-Address Mode Exceptions
                None.
                Virtual-8086 Mode Exceptions
                #GP(0) If the current privilege level is not 0.
*/
            #endregion
        }
    }
    public class ICEBP : Instruct
    {
        public ICEBP()
        {
            mName = "ICEBP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Single Step";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lTemp = 0x01;
            CurrentDecode.Op1Value.OpByte = lTemp;
            mProc.INT.Impl(ref CurrentDecode);

                #region Instructions
            #endregion
        }
    }
    public class IDIV : Instruct
    {
        public IDIV()
        {
            mName = "IDIV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Signed Divide";
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1Value.OpQWord == 0)
            {
                CurrentDecode.ExceptionNumber = 0x00;
                CurrentDecode.ExceptionThrown = true;
                CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                CurrentDecode.ExceptionErrorCode = 0;
    
                return;
            }

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    Word lTempAX = Misc.SignExtend(mProc.regs.AL);
                    mProc.regs.AL = (byte)(lTempAX / (Int16)CurrentDecode.Op1Value.OpByte);
                    mProc.regs.AH = (byte)(lTempAX % (UInt16)CurrentDecode.Op1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    if (Misc.IsNegative(mProc.regs.AX))
                        mProc.regs.DX = 0xFFFF;
                    UInt32 lDXAX = (UInt32)((mProc.regs.DX << 16) + mProc.regs.AX);
                    mProc.regs.AX = (UInt16)((Int32)lDXAX / (Int32)CurrentDecode.Op1Value.OpWord);
                    mProc.regs.DX = (UInt16)((Int32)lDXAX % (Int32)CurrentDecode.Op1Value.OpWord);
                    //SetC the flags
                    break;
                case TypeCode.UInt32:
                    if (Misc.IsNegative(mProc.regs.EAX))
                        mProc.regs.EDX = 0xFFFFFFFF;
                    Int64 lEDXEAX = mProc.regs.EDX;
                    lEDXEAX <<= 32;
                    lEDXEAX |= mProc.regs.EAX;
                    mProc.regs.EAX = (UInt32)(lEDXEAX / (Int64)CurrentDecode.Op1Value.OpDWord);
                    mProc.regs.EDX = (UInt32)(lEDXEAX % (Int64)CurrentDecode.Op1Value.OpDWord);
                    break;
                #region Instructions
                /*
            Operation
                IF SRC = 0
                THEN #DE; (* divide error *)
                FI;
                IF OpernadSize = 8 (* word/byte operation *)
                THEN
                temp ? AX / SRC; (* signed division *)
                IF (temp > 7FH) OR (temp < 80H)
                (* if a positive result is greater than 7FH or a negative result is less than 80H *)
                THEN #DE; (* divide error *) ;
                ELSE
                AL ? temp;
                AH ? AX SignedModulus SRC;
                FI;
                ELSE
                IF OpernadSize = 16 (* doubleword/word operation *)
                THEN
                temp ? DX:AX / SRC; (* signed division *)
                IF (temp > 7FFFH) OR (temp < 8000H)
                (* if a positive result is greater than 7FFFH *)
                (* or a negative result is less than 8000H *)
                THEN #DE; (* divide error *) ;
                ELSE
                AX ? temp;
                DX ? DX:AX SignedModulus SRC;
                FI;
                ELSE (* quadword/doubleword operation *)
                temp ? EDX:EAX / SRC; (* signed division *)
                IF (temp > 7FFFFFFFH) OR (temp < 80000000H)
                (* if a positive result is greater than 7FFFFFFFH *)
                (* or a negative result is less than 80000000H *)
                THEN #DE; (* divide error *) ;
                ELSE
                EAX ? temp;
                EDX ? EDXE:AX SignedModulus SRC;
                FI;
                FI;
                FI;
            */
                #endregion
            }
        }
    }
    public class IMUL : Instruct
    {
        //TODO: Need to verify CF handling for this instruction
        public IMUL()
        {
            mName = "IMUL";
            mProc8086 = true;//Not immediate version
            mProc8088 = true;//Not immediate version
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Signed Multiply";
            mModFlags = eFLAGS.OF | eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = CurrentDecode.Op2Value; // SignExtendOp(Op2Val, CurrentDecode.Operand2IsRef, Op1Val.GetTypeCode()); ;

            sbyte mOp1_8 = 0, mOp2_8 = 0;
            Int16 mOp1_16 = 0, mOp2_16 = 0;
            Int32 mOp1_32 = 0, mOp2_32 = 0, mTemp_32 = 0;
            Int64 mTemp_64 = 0;
            switch (CurrentDecode.RealOpCode)
            {
                #region 0xF6
                case 0xF6:      //AX? AL ? r/m byte
                    mOp1_8 = (sbyte)mProc.regs.AL;
                    mOp2_8 = (sbyte)CurrentDecode.Op1Value.OpByte;
                    mProc.regs.AX = (UInt16)(Int16)(mOp1_8 * mOp2_8);
                    IMUL_SetFlags(mProc.regs.AX, mProc.regs.AL);
                    break;
                #endregion
                #region 0xF7
                case 0xF7:
                    if (CurrentDecode.lOpSize16)    //DX:AX ? AX ? r/m word 
                    {
                        mOp1_16 = (Int16)mProc.regs.AX;
                        mOp2_16 = (Int16)CurrentDecode.Op1Value.OpWord;
                        mProc.regs.DX = (UInt16)(Int16)Misc.GetHi((mOp1_16 * mOp2_16));
                        mProc.regs.AX = (UInt16)(Int16)Misc.GetLo((mOp1_16 * mOp2_16));
                        IMUL_SetFlags(Misc.SignExtend(mProc.regs.AX), (Int64)((mProc.regs.DX << 16) + mProc.regs.AX));
                    }
                    else                    //EDX:EAX ? EAX ? r/m doubleword
                    {
                        mOp1_32 = (Int32)mProc.regs.EAX;
                        mOp2_32 = (Int32)CurrentDecode.Op1Value.OpDWord;
                        mProc.regs.EDX = Misc.GetHi((Int64)(mOp1_32 * mOp2_32));
                        mProc.regs.EAX = Misc.GetLo((Int64)(mOp1_32 * mOp2_32));
                        IMUL_SetFlags((Int64)mProc.regs.EAX, (Int64)((mProc.regs.EDX << 32) + mProc.regs.EAX));


                    }
                    break;
                #endregion
                #region 0x0FAF
                case 0x0FAF:
                    if (CurrentDecode.lOpSize16)    //IMUL r16,r/m16 word register ? word register ? r/m word
                    {
                        mOp1_16 = (Int16)CurrentDecode.Op1Value.OpWord;
                        mOp2_16 = (Int16)lOp2Value.OpWord;
                        mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                        mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                        //TODO: Make sure this works!
                        mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mOp1_16);
                        IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                    }
                    else                    //IMUL r32,r/m32 doubleword register ? doubleword register ? r/m doubleword
                    {
                        mOp1_32 = (Int32)CurrentDecode.Op1Value.OpDWord;
                        mOp2_32 = (Int32)lOp2Value.OpDWord;
                        mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                        mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                        mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt32)mOp1_32);
                        IMUL_SetFlags((Int64)mTemp_64, (Int64)mOp1_32);
                    }
                    break;
                #endregion
                #region 0x6B
                case 0x6B:
                    if (CurrentDecode.Op3Value.OpQWord != 0)
                    {
                        if (CurrentDecode.lOpSize16)    //IMUL r16,r/m16,imm8 word register ? r/m16 ? sign-extended immediate byte
                        {
                            mOp1_16 = (Int16)lOp2Value.OpWord;
                            mOp2_16 = (Int16)Misc.SignExtend(CurrentDecode.Op3Value.OpByte);
                            mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)(mTemp_32), (Int64)(mOp1_16));
                        }
                        else                    //IMUL r32,r/m32,imm8 doubleword register ? r/m32 ? sign-extended immediate byte
                        {
                            mOp1_32 = (Int32)lOp2Value.OpDWord;
                            mOp2_32 = (Int32)Misc.SignExtend(CurrentDecode.Op3Value.OpWord);
                            mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)(mTemp_64), (Int64)(mOp1_32));
                        }
                    } //end 3 operands
                    else
                    {
                        if (CurrentDecode.lOpSize16)    //IMUL r16,imm8 word register ? word register ? sign-extended immediate byte
                        {
                            mOp1_16 = (Int16)CurrentDecode.Op1Value.OpWord;
                            mOp2_16 = (Int16)Misc.SignExtend(lOp2Value.OpByte);
                            mTemp_32 = mOp1_16 * mOp2_16;
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)(mTemp_32), (Int64)(mOp1_16));
                        }
                        else                    //IMUL r32,imm8 doubleword register ? doubleword register ? signextended immediate byte
                        {
                            mOp1_32 = (Int32)CurrentDecode.Op1Value.OpDWord;
                            mOp2_32 = (Int32)Misc.SignExtend(lOp2Value.OpWord);
                            mTemp_64 = mOp1_32 * mOp2_32;
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)(mTemp_64), (Int64)(mOp1_32));
                        }
                    }
                    break;
                #endregion
                #region 0x69
                case 0x69:
                    if (CurrentDecode.Op3Value.OpDWord != 0xFFFFFFFF)
                    {
                        if (CurrentDecode.lOpSize16)    //IMUL r16,r/m16,imm16 word register ? r/m16 ? immediate word
                        {
                            mOp1_16 = (Int16)lOp2Value.OpWord;
                            mOp2_16 = (Int16)CurrentDecode.Op3Value.OpWord;
                            mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)(mOp1_16 * (Int16)mOp2_16);
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                        }
                        else                    //IMUL r32,r/m32,imm32 doubleword register ? r/m32 ? immediate doubleword
                        {
                            mOp1_32 = (Int32)lOp2Value.OpDWord;
                            mOp2_32 = (Int32)CurrentDecode.Op3Value.OpDWord;
                            mTemp_64 = (mOp1_32 * mOp2_32);
                            mOp1_32 = (mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((mTemp_64), (Int64)(mOp1_32));
                        }
                    }
                    else
                    {
                        if (CurrentDecode.lOpSize16)    //word register ? r/m16 ? immediate word
                        {
                            mOp1_16 = (Int16)CurrentDecode.Op1Value.OpWord;
                            mOp2_16 = (Int16)lOp2Value.OpWord;
                            mTemp_32 = (mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                        }
                        else                    //doubleword register ? r/m32 ? immediate doubleword
                        {
                            mOp1_32 = (Int32)CurrentDecode.Op1Value.OpDWord;
                            mOp2_32 = (Int32)lOp2Value.OpDWord;
                            mTemp_64 = (mOp1_32 * mOp2_32);
                            mOp1_32 = (mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags(mTemp_64, (Int64)mOp1_32);
                        }
                    }
                    break;
                #endregion
                default:
                    throw new Exception("D'oh!");
            }

            #region Instructions
            #endregion
        }
        private void IMUL_SetFlags(Int64 Value1, Int64 Value2)
        {
            if (Value1 == Value2)
            {
                mProc.regs.setFlagCF(false);
                mProc.regs.setFlagOF(false);
            }
            else
            {
                mProc.regs.setFlagCF(true);
                mProc.regs.setFlagOF(true);
            }
        }
    }
    public class IN : Instruct
    {
        public IN()
        {
            mName = "IN";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "***";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (byte)mProc.ports.In(CurrentDecode.Op2Value.OpWord, CurrentDecode.Op1TypeCode));
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (UInt16)mProc.ports.In(CurrentDecode.Op2Value.OpWord, CurrentDecode.Op1TypeCode));
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.ports.In(CurrentDecode.Op2Value.OpWord, CurrentDecode.Op1TypeCode));
                    break;
            }

        }
    }
    public class INVLPG : Instruct
    {
        public INVLPG()
        {
            mName = "INVLPG";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = false;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Invalidate TLB Entry for page that contains m";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lMemAddr = CurrentDecode.Op1Add;
            if (mProc.regs.CPL != 0)
            {
                //If not CPL = 0 then 
                CurrentDecode.ExceptionNumber = 0x0D;
                CurrentDecode.ExceptionThrown = true;
                CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                CurrentDecode.ExceptionErrorCode = 0;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.System, "Attempt to INVLPG page containing address " + lMemAddr.ToString("X8") + " however CPL (" + mProc.regs.CPL.ToString("X1") + ") is not zero.  GPF (11) exception triggered");

            }
            else
            {
                cTLB.InvalidatePage(mProc, lMemAddr);
            }

        }
    }
    public class INC : Instruct
    {
        public INC()
        {
            mName = "INC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Increment by 1";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
          public override void Impl(ref sInstruction CurrentDecode)
        {
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;

            //Do the math!
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte++;
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    CurrentDecode.Op2Value.OpByte = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    mProc.regs.setFlagOF_Add(lOp1Value.OpByte, CurrentDecode.Op2Value.OpByte, CurrentDecode.Op1Value.OpByte);
                    mProc.regs.setFlagZF(lOp1Value.OpByte);
                    mProc.regs.setFlagAF(lOp1Value.OpByte, lPreVal1.OpByte);
                    mProc.regs.setFlagPF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord++;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    CurrentDecode.Op2Value.OpWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    mProc.regs.setFlagOF_Add(lOp1Value.OpWord, CurrentDecode.Op2Value.OpWord, CurrentDecode.Op1Value.OpWord);
                    mProc.regs.setFlagZF(lOp1Value.OpWord);
                    mProc.regs.setFlagAF(lOp1Value.OpWord, lPreVal1.OpWord);
                    mProc.regs.setFlagPF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord++;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    CurrentDecode.Op2Value.OpDWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    mProc.regs.setFlagOF_Add(lOp1Value.OpDWord, CurrentDecode.Op2Value.OpDWord, CurrentDecode.Op1Value.OpDWord);
                    mProc.regs.setFlagZF(lOp1Value.OpDWord);
                    mProc.regs.setFlagAF(lOp1Value.OpDWord, lPreVal1.OpDWord);
                    mProc.regs.setFlagPF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord++;
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    CurrentDecode.Op2Value.OpQWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    mProc.regs.setFlagOF_Add(lOp1Value.OpQWord, CurrentDecode.Op2Value.OpQWord, CurrentDecode.Op1Value.OpQWord);
                    mProc.regs.setFlagZF(lOp1Value.OpQWord);
                    mProc.regs.setFlagAF(lOp1Value.OpQWord, lPreVal1.OpQWord);
                    mProc.regs.setFlagPF(lOp1Value.OpQWord);
                    break;
            }

        }
    }
    public class INS : Instruct
    {
        public INS()
        {
            mName = "INS";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "***";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lDest, lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            do
            {
                if (CurrentDecode.RealOpCode == 0x6c)
                {
                    if (mProc.mCurrInstructAddrSize16)
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
                    else
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, lDest, (byte)mProc.ports.In(mProc.regs.DX, TypeCode.Byte));
                    if (CurrentDecode.ExceptionThrown) return;
                    MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 1, false, true, false, false);

                }
                else
                {
                    if (CurrentDecode.lOpSize16)
                    {
                        if (mProc.mCurrInstructAddrSize16)
                            lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
                        else
                            lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
                        mProc.mem.SetWord(mProc, ref CurrentDecode, lDest, (Word)mProc.ports.In(mProc.regs.DX, TypeCode.UInt16));
                        if (CurrentDecode.ExceptionThrown) return;
                        MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 2, false, true, false, false);
                    }
                    else
                    {
                        if (mProc.mCurrInstructAddrSize16)
                            lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
                        else
                            lDest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
                        mProc.mem.SetDWord(mProc, ref CurrentDecode, lDest, mProc.ports.In(mProc.regs.DX, TypeCode.UInt32));
                        if (CurrentDecode.ExceptionThrown) return;
                        MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 4, false, true, false, false);
                    }
                }
                //if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)
                //    if (mProc.OpSize16)
                //        mProc.regs.CX--;
                //    else
                //        mProc.regs.ECX--;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);
            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

        }
    }
    public class INT : Instruct
    {
        public INT()
        {
            mName = "INT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Call to Interrupt Procedure";
            mFlowReturnsLater = true;
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lTempSS, lTempESP;
            UInt32 lTempEFLAGS, lNewSS, lNewESP, lTempCS;
            sIDTEntry lIDT;
            sGDTEntry lGDT;
            sInstruction ins = CurrentDecode;

            if (mProc.mSystem.Debuggies.DebugInterrupts)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.Interrupts, " INT " + CurrentDecode.Op1Value.OpByte.ToString("X2") + " called from " + mProc.regs.CS.Value.ToString("X4") + ":" + mProc.regs.EIP.ToString("X8") + "(" + 
                    CurrentDecode.InstructionAddress.ToString("X8") + ") : " + mProc.regs.CS.Value.ToString("X8") + ", " + mProc.regs.DS.Value.ToString("X8") + ", " + mProc.regs.ES.Value.ToString("X8") + ", " + mProc.regs.FS.Value.ToString("X8") + ", " + mProc.regs.GS.Value.ToString("X8") + ", " + mProc.regs.SS.Value.ToString("X8") + ", " + mProc.regs.ESP.ToString("X8") + " - FLAGS:" + mProc.regs.EFLAGS.ToString("X4") + " - " + mProc.OperatingMode);

            ins.ExceptionThrown = false;
            ins.Operand1IsRef = false;
            if (mProc.OperatingMode != ProcessorMode.Protected)
                lTempSS = mProc.regs.SS.Value;
            else
                lTempSS = mProc.regs.SS.mWholeSelectorValue;
            lTempESP = mProc.regs.ESP;

            switch (mProc.OperatingMode)
            {
                case ProcessorMode.Real:
                    Word lTemp = mProc.regs.FLAGS;

                    ins.Op1Add = 0x0;
                    ins.Op1Value.OpWord = lTemp;
                    ins.Operand1IsRef = false;
                    mProc.PUSH.Impl(ref ins);
                    mProc.regs.setFlagIF(false);
                    mProc.regs.setFlagTF(false);
                    mProc.regs.setFlagAC(false);

                    ins.Op1Add = Processor_80x86.RCS;
                    ins.Op1Value.OpDWord = mProc.regs.CS.Value;
                    mProc.PUSH.Impl(ref ins);
                    ins.Op1Add = Processor_80x86.RIP;
                    ins.Op1Value.OpDWord = mProc.regs.IP;
                    mProc.PUSH.Impl(ref ins);
                    if (ins.ExceptionThrown)
                    {
                        
                        return;
                    }
                    ins.Op1Add = 0x0;
                    mProc.regs.CS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, (CurrentDecode.Op1Value.OpDWord * 4 + 2));
                    mProc.regs.IP = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Value.OpDWord * 4);
                    if (mProc.mSystem.Debuggies.DebugInterrupts)
                        mProc.mSystem.PrintDebugMsg(eDebuggieNames.Interrupts, $"New CS = {mProc.regs.CS.Value.ToString("X4")}, New IP = {mProc.regs.IP.ToString("X4")}");
                    break;

                default:    //protected or virtual mode
                    //Called interrupt not in IDT, GPF
                    if (CurrentDecode.Op1Value.OpWord > mProc.mIDTCache.Count - 1
                         || (mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType != eSystemOrGateDescType.Interrupt_Gate_32
                         && mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType != eSystemOrGateDescType.Interrupt_Gate_16
                         && mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType != eSystemOrGateDescType.Trap_Gate_16
                         && mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType != eSystemOrGateDescType.Trap_Gate_32
                         && mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType != eSystemOrGateDescType.Task_Gate))
                    {   //#GP((DEST ? 8) + 2 + EXT)");
                        //Decimal 13 (GP)
                        mProc.mExceptionWhileExcepting = true;
                        CurrentDecode.ExceptionThrown = true;
                        CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                        CurrentDecode.ExceptionErrorCode = CurrentDecode.Op1Value.OpDWord; // *8 + 2; 08/18/2013 - CLR - don't seem to need to encode the error code
                        CurrentDecode.ExceptionNumber = 0x0d;
            
                        return;
                    }

                    lIDT = mProc.mIDTCache[CurrentDecode.Op1Value.OpWord];
                    if ((lIDT.Descriptor_PL < mProc.regs.CPL) && (!CurrentDecode.ExceptionThrown) && (!mProc.ServicingIRQ))
                    //(* PE=1, DPL<CPL, software interrupt *)
                    {   //#GP((vector_number ? 8) + 2 + EXT)");
                        //Decimal 13 (GP)
                        mProc.mExceptionWhileExcepting = true;
                        CurrentDecode.ExceptionNumber = 0x0d;
                        CurrentDecode.ExceptionThrown = true;
                        CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                        CurrentDecode.ExceptionErrorCode = CurrentDecode.Op1Value.OpDWord * 8 + 2;
            
                        return;
                    }
                    //if (!lGDT.access.Present)
                    //    throw new Exception("Code #NP((vector_number ? 8) + 2 + EXT)");
                    if (mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Task_Gate)
                    {
                        throw new Exception("Code TASK-GATE");
                    }
                    else
                    {   //TRAP-OR-INTERRUPT-GATE (* PE=1, trap/interrupt gate *)
                        if (lIDT.GDTCachePointer == 0)
                            throw new Exception("#GP(0H + EXT)");
                        if (lIDT.GDTCachePointer > mProc.mGDTCache.Count - 1)
                            throw new Exception("Code #GP(selector + EXT)");
                        lGDT = mProc.mGDTCache[lIDT.GDTCachePointer];
                        if (lGDT.access.PrivLvl > mProc.regs.CPL)
                            throw new Exception("Code #GP(selector + EXT)");

                        if (!lGDT.access.Present)
                            throw new Exception("#NP(selector + EXT)");
                        if ((lGDT.access.SegType != eGDTSegType.Code_Exec_Only_Conforming
                            && lGDT.access.SegType != eGDTSegType.Code_Exec_Only_Conforming_Accessed
                            && lGDT.access.SegType != eGDTSegType.Code_Exec_RO_Conforming
                            && lGDT.access.SegType != eGDTSegType.Code_Exec_RO_Conforming_Accessed)
                            && lGDT.access.PrivLvl < mProc.regs.CPL)
                            if ((mProc.regs.EFLAGS & 0x20000) != 0X20000)
                            {
                                //INTER-PRIVILEGE-LEVEL-INTERRUPT; (* PE=1, interrupt or trap gate, nonconforming *)
                                if (mProc.mSystem.Debuggies.DebugCPU && lIDT.Descriptor_PL == ePrivLvl.Kernel_Ring_0 && mProc.regs.CPL == ePrivLvl.App_Level_3)
                                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.CPU, "INT-Pushed\tSS=" + mProc.regs.SS.Value.ToString("X8") + ", ESP=" + mProc.regs.ESP.ToString("X8") + ",FLAGS= " + mProc.regs.EFLAGS.ToString("X8") + ", CS=" + mProc.regs.CS.Value.ToString("X4") + ", EIP=" + mProc.regs.EIP.ToString("X8"));
                                ins.Op1Add = 0;
                                //CLR 08/01/2013 - 
                                mProc.regs.mOverrideCPL = true;
                                mProc.regs.mOverrideCPLValue = 0;
                                if (mProc.mCurrTSS.TSS_is_32bit)
                                {
                                    mProc.regs.SS.Value = mProc.mCurrTSS.SS0;
                                    mProc.regs.ESP = mProc.mCurrTSS.ESP0;
                                }
                                else
                                {
                                    mProc.regs.SS.Value = mProc.mCurrTSS.SS0;
                                    mProc.regs.SP = (UInt16)mProc.mCurrTSS.ESP0;
                                }
                                ins.Op1Value.OpDWord = lTempSS; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins);
                                if (ins.ExceptionThrown)
                                {

                                    return;
                                }
                                ins.Op1Value.OpDWord = lTempESP; mProc.PUSH.Impl(ref ins);
                                ins.Op1Value.OpDWord = mProc.regs.EFLAGS; mProc.PUSH.Impl(ref ins);
                                if (CurrentDecode.ExceptionThrown)
                                {
                                    ins.Op1Add = Processor_80x86.RCS; mProc.PUSH.Impl(ref ins); ins.Op1Add = 0x0;
                                    //No need to subtract bytes used, already did that in the page fault handler
                                    ins.Op1Value.OpDWord = mProc.regs.EIP/* - mProc.mCurrentInstruction.DecodedInstruction.BytesUsed*/; mProc.PUSH.Impl(ref ins);
                                    ins.Op1Value.OpDWord = CurrentDecode.ExceptionErrorCode;
                                    mProc.PUSH.Impl(ref ins);
                                    if (mProc.OpSize16)
                                    {
                                        ins.Op1Value.OpWord = 0;
                                        mProc.PUSH.Impl(ref ins);
                                    }
                                }
                                else
                                {
                                    ins.Op1Add = Processor_80x86.RCS; ins.Operand1IsRef = true; mProc.PUSH.Impl(ref ins); ins.Op1Add = 0x0;
                                    ins.Op1Value.OpDWord = mProc.regs.EIP; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins);
                                }
                                mProc.regs.mOverrideCPL = false;
                                mProc.regs.CS.Value = lIDT.SegSelector;
                                mProc.regs.EIP = lIDT.PEP_Offset;
                                mProc.mOverrideStackSize = false;
                                CurrentDecode.ExceptionNumber = Global.CPU_NO_EXCEPTION;
                                if (mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_16 ||
                                  mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_32)
                                    mProc.regs.setFlagIF(false);
                                mProc.regs.setFlagTF(false);
                                mProc.regs.setFlagNT(false);
                                mProc.regs.setFlagVM(false);
                                mProc.regs.setFlagRF(false);
                                mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                                //Update the processor's operating mode
                                Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;
                                //mProc.regs.CPL = mProc.regs.CS.Selector.access.PrivLvl;
                                //mProc.regs.CS.Selector.access.PrivLvl

                                return;
                            }
                            //(* code segment, DPL<CPL, VM=0 *)
                            else  //VM=1
                                if (lGDT.access.PrivLvl != 0)
                                throw new Exception("Code #GP(new code segment selector)");
                            else    //INTERRUPT-FROM-VIRTUAL-8086-MODE;
                            {
                                //CLR 07/11/2013 - Added CPL because of paging issues when a paging error occurs while coming out of V86 Mode for a clock tick
                                //CLR 07/19/2013 - Removed when I moved CPL to CS functionality instead of a separate variable
                                //mProc.regs.CPL = 0;
                                if (lGDT.access.PrivLvl != ePrivLvl.Kernel_Ring_0)
                                    throw new Exception("INT 0x" + CurrentDecode.Op1Value.OpWord.ToString("X3") + ": GPE - Interrupt DPL != 0 on VMode Interrupt call");
                                //INTERRUPT-FROM-VIRTUAL-8086-MODE
                                if (mProc.mCurrTSS.TSS_is_32bit)
                                {
                                    lNewSS = mProc.mCurrTSS.SS0;
                                    lNewESP = mProc.mCurrTSS.ESP0;
                                }
                                else
                                    throw new Exception("16 bit TSS not coded yet");
                                lTempEFLAGS = mProc.regs.EFLAGS;
                                mProc.regs.setFlagVM(false);
                                mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                                //Update the processor's operating mode
                                Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;
                                lTempCS = mProc.regs.CS.mRealModeValue;
                                //mProc.mOpSize16 = false;
                                //mProc.mAddrSize16 = false;
                                mProc.regs.SS.Value = lNewSS;
                                mProc.regs.ESP = lNewESP;

                                if (mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_32)
                                {
                                    CurrentDecode.mOverrideAddrSizeFor32BitGate = true;
                                    ins.mOverrideAddrSizeFor32BitGate = true;
                                }
                                //Don't pass the register address because we are in Protected mode now
                                //Passing the address would cause the selectors to be pushed instead of the register values
                                ins.Op1Add = 0x00;
                                ins.Op1Value.OpDWord = mProc.regs.GS.mRealModeValue; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins);
                                if (ins.ExceptionThrown)
                                {

                                    return;
                                }
                                ins.Op1Value.OpDWord = mProc.regs.FS.mRealModeValue; mProc.PUSH.Impl(ref ins);
                                ins.Op1Value.OpDWord = mProc.regs.DS.mRealModeValue; mProc.PUSH.Impl(ref ins);
                                ins.Op1Value.OpDWord = mProc.regs.ES.mRealModeValue; mProc.PUSH.Impl(ref ins);
                                /*mProc.PUSH.Op1Add = Processor_80x86.RSS;*/
                                ins.Op1Value.OpDWord = lTempSS; mProc.PUSH.Impl(ref ins);
                                /*mProc.PUSH.Op1Add = Processor_80x86.RESP;  CLR 07/10/2013 - remarked out this and RSS above*/
                                ins.Op1Value.OpDWord = lTempESP; mProc.PUSH.Impl(ref ins);
                                ins.Op1Add = 0;
                                ins.Op1Value.OpDWord = lTempEFLAGS; mProc.PUSH.Impl(ref ins);

                                //Per IASD Vol 3, pg 162 ...
                                //Error codes are not pushed on the stack for exceptions that are generated externally (with the
                                //INTR or LINT[1:0] pins) or the INT n instruction, even if an error code is normally produced
                                //for those exceptions
                                if (CurrentDecode.ExceptionThrown || CurrentDecode.Op1Value.OpWord == 0x14)
                                {
                                    ins.Op1Value.OpDWord = lTempCS; mProc.PUSH.Impl(ref ins);
                                    ins.Op1Value.OpDWord = mProc.regs.EIP; mProc.PUSH.Impl(ref ins);
                                    //CLR 07/11/2013 - changed from the *8)+2 to just ExceptionErrorCode
                                    //For Exception D, code was already calculated above.

                                    //if (Op1Value.OpWord == 0xD)
                                    //    mProc.PUSH.Op1Value.OpDWord = mProc.ExceptionErrorCode;
                                    //else
                                    ins.Op1Value.OpDWord = (CurrentDecode.ExceptionErrorCode * 8) + 2;
                                    mProc.PUSH.Impl(ref ins);
                                    if (mProc.OpSize16 && !CurrentDecode.mOverrideAddrSizeFor32BitGate)
                                    {
                                        ins.Op1Value.OpDWord = 0;
                                        mProc.PUSH.Impl(ref ins);
                                    }
                                }
                                else
                                {
                                    ins.Op1Value.OpDWord = lTempCS; mProc.PUSH.Impl(ref ins);
                                    ins.Op1Value.OpDWord = mProc.regs.EIP; mProc.PUSH.Impl(ref ins);
                                }

                                CurrentDecode.mOverrideAddrSizeFor32BitGate = false;
                                ins.mOverrideAddrSizeFor32BitGate = true;

                                mProc.regs.GS.Value = 0;
                                mProc.regs.setFlagTF(false);
                                mProc.regs.setFlagRF(false);
                                mProc.regs.setFlagIF(false);
                                //This call triggers the change from Virtual-8086 to Protected mode
                                mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                                //Update the processor's operating mode
                                Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;

                                mProc.regs.FS.Value = 0;
                                mProc.regs.DS.Value = 0;
                                mProc.regs.ES.Value = 0;

                                mProc.regs.CS.Value = lIDT.SegSelector;
                                mProc.regs.EIP = lIDT.PEP_Offset;
                                mProc.mOverrideStackSize = false;
                                CurrentDecode.ExceptionNumber = Global.CPU_NO_EXCEPTION;

                                return;
                            }
                        else    //(* PE=1, interrupt or trap gate, DPL ? CPL *)
                            if (mProc.regs.FLAGSB.VM)
                        //throw new Exception("Code #GP(new code segment selector)");
                        {
                            //Decimal 13 (GP)
                            mProc.mExceptionWhileExcepting = true;
                            CurrentDecode.ExceptionNumber = 0x0d;
                            CurrentDecode.ExceptionThrown = true;
                            CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                            CurrentDecode.ExceptionErrorCode = lIDT.SegSelector;

                            return;
                        }

                        else if ((lGDT.access.SegType == eGDTSegType.Code_Exec_Only_Conforming
                        || lGDT.access.SegType == eGDTSegType.Code_Exec_Only_Conforming_Accessed
                        || lGDT.access.SegType == eGDTSegType.Code_Exec_RO_Conforming
                        || lGDT.access.SegType == eGDTSegType.Code_Exec_RO_Conforming_Accessed)
                        || lGDT.access.PrivLvl == mProc.regs.CPL)
                        {
                            //INTRA-PRIVILEGE-LEVEL-INTERRUPT: (* PE=1, DPL = CPL or conforming segment *)
                            ins.Op1Add = 0x0;
                            ins.Op1TypeCode = TypeCode.UInt32;
                            ins.Op1Value.OpDWord = mProc.regs.EFLAGS; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins);
                            if (ins.ExceptionThrown)
                            {

                                return;
                            }
                            if (CurrentDecode.ExceptionThrown)
                            {
                                ins.Op1Add = Processor_80x86.RCS; ins.Operand1IsRef = true; mProc.PUSH.Impl(ref ins); ins.Op1Add = 0x0;
                                ins.Op1Value.OpDWord = mProc.regs.EIP; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins); ins.Op1Add = 0x0;
                                ins.Op1Value.OpDWord = CurrentDecode.ExceptionErrorCode;
                                mProc.PUSH.Impl(ref ins);
                                if (mProc.OpSize16)
                                {
                                    ins.Op1Value.OpWord = 0;
                                    mProc.PUSH.Impl(ref ins);
                                }
                            }
                            else
                            {
                                ins.Op1Add = Processor_80x86.RCS; ins.Operand1IsRef = true; mProc.PUSH.Impl(ref ins); ins.Op1Add = 0x0;
                                ins.Op1Value.OpDWord = mProc.regs.EIP; ins.Operand1IsRef = false; mProc.PUSH.Impl(ref ins);
                            }
//20240309
                                if ((int)mProc.regs.CPL != (lIDT.SegSelector & 3))
                                if (mProc.regs.CPL == ePrivLvl.App_Level_3)
                                    mProc.regs.CS.Value = (DWord)(lIDT.SegSelector | (int)mProc.regs.CPL);
                                else
                                    mProc.regs.CS.Value = (DWord)(lIDT.SegSelector & 0xFFF8);
                            else
                                mProc.regs.CS.Value = lIDT.SegSelector;
                            mProc.regs.EIP = lIDT.PEP_Offset;
                            mProc.mOverrideStackSize = false;
                            CurrentDecode.ExceptionNumber = Global.CPU_NO_EXCEPTION;
                            if (mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_16 ||
                                mProc.mIDTCache[CurrentDecode.Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_32)
                                mProc.regs.setFlagIF(false);
                            mProc.regs.setFlagTF(false);
                            mProc.regs.setFlagNT(false);
                            mProc.regs.setFlagVM(false);
                            mProc.regs.setFlagRF(false);
                            mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                            //Update the processor's operating mode
                            Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;
                            return;
                        }
                        else    //(* PE=1, interrupt or trap gate, nonconforming *)
                        {   //#GP(CodeSegmentSelector + EXT)
                            //(* code segment, DPL>CPL *)
                            mProc.mExceptionWhileExcepting = true;
                            CurrentDecode.ExceptionNumber = 0x0d;
                            CurrentDecode.ExceptionThrown = true;
                            CurrentDecode.ExceptionAddress = CurrentDecode.InstructionAddress;
                            CurrentDecode.ExceptionErrorCode = lIDT.SegSelector;

                            return;
                        }

                        throw new Exception("INT: Shouldn't get here!");
                    }

            }

        }
    }
    public class INTO : Instruct
    {
        public INTO()
        {
            mName = "INTO";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Interrupt 4—if overflow flag is 1";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.OF)
            {
                CurrentDecode.Op1Value.OpQWord = 0x04;
                mProc.INT.Impl(ref CurrentDecode);
    
            }
            #region Instructions
            #endregion
        }
    }
    public class IRET : Instruct
    {
        public IRET()
        {
            mName = "IRET";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Interrupt return (16 bit operand size)";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF | eFLAGS.CF | eFLAGS.DF | eFLAGS.IF | eFLAGS.TF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.POP.mNoSetMemoryToValue = false;
            sInstruction ins = CurrentDecode;
            UInt32 lPreIRetFlags, lTempCS, lTempEIP, lTempEFlags, lTempSS, lTempESP, lTempES, lTempDS, lTempFS, lTempGS;
            UInt16 lTempFlags = 0;
            ePrivLvl lPriorCPL;

            //if (mProc.regs.CS.Value == 0x12f000 && mProc.regs.IP == 0xf94)
            //{

            //    DWord a = mProc.regs.EIP;
            //    a = a + 1 - 1;
            //}

            if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Real)
            {
                #region Real Mode IRET
                ins.Operand1IsRef = true;
                ins.Op1Add = Processor_80x86.RIP;
                mProc.POP.Impl(ref ins);

                ins.Op1Add = Processor_80x86.RCS;
                mProc.POP.Impl(ref ins);

                lPreIRetFlags = mProc.regs.EFLAGS;

                ins.Op1Add = 0x0;
                mProc.POPF.Impl(ref ins);

                //Backwards from Intel docs because I pop directly to the flags register, not to tempEFlags
                mProc.regs.EFLAGS = (UInt32)(mProc.regs.EFLAGS & 0x257FD5) | (UInt32)(lPreIRetFlags & 0x1A0000);
                #endregion
            }
            else
            {   //RETURN-FROM-VIRTUAL-8086-MODE
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Virtual8086)
                {
                    if (CurrentDecode.lOpSize16)
                    {
                        mProc.POP.mNoSetMemoryToValue = true;
                        mProc.POP.Impl(ref ins);
                        mProc.regs.EIP = ins.Op1Value.OpDWord;
                        mProc.POP.Impl(ref ins);
                        if (ins.ExceptionThrown)
                        {
                            
                            return;
                        }
                        mProc.regs.CS.Value = ins.Op1Value.OpDWord;
                        mProc.POP.Impl(ref ins);
                        lTempFlags = (Word)(ins.Op1Value.OpWord); // ^ 0x3000);
                        mProc.regs.FLAGS = (Word)(lTempFlags | 0x200);  //clr 07/10/2013 (added | 0x200)
                    }
                    else
                    {
                        throw new Exception("Time to code 32 bit verson of RETURN-FROM-VIRTUAL-8086-MODE!");
                    }
                }
                else
                {
                    if (mProc.regs.FLAGSB.NT)
                    {
                        //if (mProc.mCurrTSS.PrevTaskLink)
                        //throw new Exception("Time to code GOTO TASK-RETURN!");
                    }

                    mProc.POP.mNoSetMemoryToValue = true;
                    ins.Op1Add = 0;
                    mProc.POP.Impl(ref ins);
                    if (ins.ExceptionThrown)
                    {
                        
                        return;
                    }
                    lTempEIP = ins.Op1Value.OpDWord;
                    //int a = 0;
                    //if (lTempEIP == 0x600137E2)
                    //    a = a + 1 - 1;
                    //                    if (lTempEIP == 0x0)
                    //if (mProc.mem.GetDWord(mProc, ref DecodedInstruction, mProc.regs.ESP) == 0x600137E2)
                    //    a += 1;
                    mProc.POP.Impl(ref ins);
                    lTempCS = ins.Op1Value.OpDWord;
                    mProc.POP.Impl(ref ins);
                    lTempEFlags = ins.Op1Value.OpDWord;

                    //RETURN-TO-VIRTUAL-8086-MODE
                    //Flags(VM) = 1
                    if ((UInt32)(lTempEFlags & 0x20000) == 0x20000 && mProc.regs.CS.Selector.access.PrivLvl == 0)
                    {
                        //Can't zero descriptors or we blow away SS where the stack is
                        //mProc.ZeroDescriptors();
                        ins.Op1Add = 0;
                        mProc.POP.Impl(ref ins);
                        lTempESP = ins.Op1Value.OpDWord;
                        ins.Op1Add = 0;
                        mProc.POP.Impl(ref ins);
                        lTempSS = ins.Op1Value.OpWord;
                        mProc.POP.Impl(ref ins);
                        lTempES = ins.Op1Value.OpWord;
                        mProc.POP.Impl(ref ins);
                        lTempDS = ins.Op1Value.OpWord;
                        mProc.POP.Impl(ref ins);
                        lTempFS = ins.Op1Value.OpWord;
                        mProc.POP.Impl(ref ins);
                        lTempGS = ins.Op1Value.OpWord;
                        //07/23/2013 - Moving this back down, was right at the beginning of the IF.  Need to retrieve values from stack BEFORE switching to VMode
                        mProc.regs.EFLAGS = lTempEFlags;
                        //Setting EFLAGS puts the processor in Virtual-8086 mode
                        mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                        //Update the processor's operating mode
                        Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;

                        mProc.regs.EIP = lTempEIP & 0xFFFF;

                        mProc.regs.CS.Value = lTempCS;
                        mProc.regs.SS.Value = lTempSS;
                        mProc.regs.ESP = lTempESP;
                        mProc.regs.DS.Value = lTempDS;
                        mProc.regs.ES.Value = lTempES;
                        mProc.regs.FS.Value = lTempFS;
                        mProc.regs.GS.Value = lTempGS;
                        //mProc.regs.setFlagIF(true); //clr 07/10/2013
                    }
                    else    //PROTECTED-MODE-RETURN
                    {
                        if (mProc.mGDTCache[(int)lTempCS >> 3].access.PrivLvl > mProc.regs.CPL)
                        {
                            //mProc.regs.CPL = mProc.mGDTCache[(int)lTempCS >> 3].access.PrivLvl;
                            //mProc.POP.Op1Add = Processor_80x86.RESP;
                            ins.Op1Add = 0;
                            mProc.POP.Impl(ref ins);
                            if (ins.ExceptionThrown)
                            {
                                
                                return;
                            }
                            lTempESP = ins.Op1Value.OpDWord;
                            mProc.POP.Impl(ref ins);
                            ins.Op1Add = 0;
                            lTempSS = ins.Op1Value.OpWord;
                            mProc.regs.SS.Value = lTempSS;
                            mProc.regs.ESP = lTempESP;
                        }
                        //RETURN-TO-SAME-PRIVILEGE-LEVEL
                        mProc.regs.EIP = lTempEIP;
                        //not sure why this references a real mode register, changing it
                        //mProc.regs.CS.mRealModeValue = lTempCS << 4;

                        //Hack with a TRY
                        lPriorCPL = mProc.regs.CPL;
                        mProc.regs.CS.Value = lTempCS;
                        //Set the 16 bit flags
                        if ((mProc.regs.FLAGS & 0x100) == 0x100)
                            System.Diagnostics.Debugger.Break();
                        mProc.regs.FLAGS = (Word)lTempEFlags;
                        mProc.regs.FLAGSB.SetValues((UInt16)lTempEFlags);
                        Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;
                        if (!CurrentDecode.lOpSize16)
                        {
                            mProc.regs.setFlagRF((lTempEFlags & 0x10000) == 0x10000);
                            mProc.regs.setFlagAC((lTempEFlags & 0x40000) == 0x40000);
                            mProc.regs.setFlagID((lTempEFlags & 0x200000) == 0x200000);
                        }
                        //If CPL <= IOPL
                        //mProc.regs.setFlagIF((lTempEFlags & 0x200) == 0x200); //CLR 06/24/2013 - Remarked out then back in
                        //If CPL = 0
                        mProc.regs.setFlagIOPL((UInt16)lTempEFlags);
                        if (!CurrentDecode.lOpSize16)
                        {
                            mProc.regs.setFlagVM((lTempEFlags & 0x20000) == 0x20000);
                            mProc.regs.setFlagVIF((lTempEFlags & 0x80000) == 0x80000);
                            mProc.regs.setFlagVIP((lTempEFlags & 0x100000) == 0x100000);
                            mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                            //Update the processor's operating mode
                            Processor_80x86.mCurrInstructOpMode = mProc.OperatingMode;
                        }
                        //if (mProc.mSystem.Debuggies.DebugCPU && mProc.regs.CPL == ePrivLvl.App_Level_3)
                        //    mProc.mSystem.PrintDebugMsg(eDebuggieNames.CPU, "INT-Popped \tSS=" + mProc.regs.SS.Value.ToString("X8") + ", ESP=" + mProc.regs.ESP.ToString("X8") + ",FLAGS= " + mProc.regs.EFLAGS.ToString("X8") + ", CS=" + mProc.regs.CS.Value.ToString("X4") + ", EIP=" + mProc.regs.EIP.ToString("X8"));
                    }
                }
            }
            
            #region Instructions
            #endregion
            if (mProc.mSystem.Debuggies.DebugInterrupts)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.Interrupts, " IRET cmpltd to     " + mProc.regs.CS.Value.ToString("X4") + ":" + mProc.regs.EIP.ToString("X8") + "(" + CurrentDecode.InstructionAddress.ToString("X8") + ") : " + mProc.regs.CS.Value.ToString("X8") + ", " + mProc.regs.DS.Value.ToString("X8") + ", " + mProc.regs.ES.Value.ToString("X8") + ", " + mProc.regs.FS.Value.ToString("X8") + ", " + mProc.regs.GS.Value.ToString("X8") + ", " + mProc.regs.SS.Value.ToString("X8") + ", " + mProc.regs.ESP.ToString("X8") + " - FLAGS:" + mProc.regs.EFLAGS.ToString("X4") + " - " + mProc.OperatingMode);
        }
    }
    public class JA : Instruct
    {
        public JA()
        {
            mName = "JA";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if above (CF=0 and ZF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.CF && !mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JAE : Instruct
    {
        public JAE()
        {
            mName = "JAE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if above or equal (CF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JB : Instruct
    {
        public JB()
        {
            mName = "JB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if below (CF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JBE : Instruct
    {
        public JBE()
        {
            mName = "JBE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if below or equal (CF=1 or ZF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.CF || mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JC : Instruct
    {
        public JC()
        {
            mName = "JC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if carry (CF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JCXZ : Instruct
    {
        public JCXZ()
        {
            mName = "JCXZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if CX register is 0";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.mCurrInstructAddrSize16)
            {
                if (mProc.regs.CX == 0)
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
            }
            else
                if (mProc.regs.ECX == 0)
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);

        }
    }
    public class JE : Instruct
    {
        public JE()
        {
            mName = "JE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if equal (ZF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JG : Instruct
    {
        public JG()
        {
            mName = "JG";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if greater (ZF=0 and SF=OF))";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((!mProc.regs.FLAGSB.ZF) & (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JGE : Instruct
    {
        public JGE()
        {
            mName = "JGE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if greater or equal (SF=OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JL : Instruct
    {
        public JL()
        {
            mName = "JL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if less (SF<>OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JLE : Instruct
    {
        public JLE()
        {
            mName = "JLE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if less or equal (ZF=1 or SF<>OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/03/2010: Semantic change to match BOCHS
            if (mProc.regs.FLAGSB.ZF || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JMP : Instruct
    {
        public JMP()
        {
            mName = "JMP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Unconditional Jump";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            bool lIsNegative = false;

            //if (ChosenOpCode.Instruction!=null && !ChosenOpCode.Operand1.StartsWith("E") && !ChosenOpCode.Operand1.StartsWith("A"))
            if (CurrentDecode.ChosenOpCode.Op1AM != sOpCodeAddressingMethod.EType && CurrentDecode.ChosenOpCode.Op1AM != sOpCodeAddressingMethod.DirectAddress)
                //Added to check for negative jump value
                lIsNegative = UpdateForNegativeAll(ref mProc, ref CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);

            if (CurrentDecode.Operand1IsRef)
            {
                mProc.regs.EIP = CurrentDecode.Op1Value.OpDWord;
            }
            else
                switch (CurrentDecode.Op1TypeCode)
                {
                    case TypeCode.Byte:
                        if (!lIsNegative)
                            mProc.regs.EIP += CurrentDecode.Op1Value.OpByte;
                        else
                            mProc.regs.EIP -= CurrentDecode.Op1Value.OpByte;
                        break;
                    case TypeCode.UInt16:
                        if (!lIsNegative)
                            mProc.regs.EIP += CurrentDecode.Op1Value.OpWord;
                        else
                            mProc.regs.EIP -= CurrentDecode.Op1Value.OpWord;
                        break;
                    case TypeCode.UInt32:
                        if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                        {
                            if (CurrentDecode.ChosenOpCode.Op1AM == sOpCodeAddressingMethod.JmpRelOffset)
                            {
                                if (!lIsNegative)
                                    mProc.regs.EIP += (UInt32)CurrentDecode.Op1Value.OpDWord;
                                else
                                    mProc.regs.EIP -= (UInt32)CurrentDecode.Op1Value.OpDWord;
                            }
                            else
                            {
                                mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord & 0x00FFFFFF);
                                mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
                            }
                        }
                        else
                        {
                            if (CurrentDecode.lOpSize16 && mProc.mCurrInstructAddrSize16 && CurrentDecode.OpCode != 0xE8)
                            {
                                mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
                                mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, CurrentDecode.Op1Value.OpDWord);
                            }
                            else
                            {
                                if (!lIsNegative)
                                    mProc.regs.EIP += (UInt32)CurrentDecode.Op1Value.OpDWord;
                                else
                                    mProc.regs.EIP -= (UInt32)CurrentDecode.Op1Value.OpDWord;
                                break;
                            }
                        }
                        break;
                    case TypeCode.UInt64:
                        if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                        {
                            mProc.regs.CS.Value = Misc.GetHi(CurrentDecode.Op1Value.OpQWord);
                            mProc.regs.EIP = CurrentDecode.Op1Value.OpDWord;
                            break;
                        }
                        else
                        {
                            mProc.regs.CS.Value = (UInt16)(CurrentDecode.Op1Value.OpQWord >> 32);
                            mProc.regs.EIP = CurrentDecode.Op1Value.OpDWord;
                        }
                        break;

                }
            if (mProc.OperatingMode != ProcessorMode.Protected)
                mProc.regs.EIP &= 0xFFFF;

        }
    }
    public class JMPF : Instruct
    {
        public JMPF()
        {
            mName = "JMPF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Far Unconditional Jump";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //TODO: Make this work correctly.  Currently the OpCode EA for JMPF (direct bytes in operand version?) points to JMP in the spreadsheet
            //so JMP is (successfully) doing this work

            //mProc.RefreshGDTCache();

            if ((CurrentDecode.OpCode == 0xFF05 || CurrentDecode.OpCode == 0xEA) && Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
            {
                #region NOTES:
                /*   33 // Notes:
                     34 // ======
                     35 
                     36   // ======================
                     37   // 286 Task State Segment
                     38   // ======================
                     39   // dynamic item                      | hex  dec  offset
                     40   // 0       task LDT selector         | 2a   42
                     41   // 1       DS selector               | 28   40
                     42   // 1       SS selector               | 26   38
                     43   // 1       CS selector               | 24   36
                     44   // 1       ES selector               | 22   34
                     45   // 1       DI                        | 20   32
                     46   // 1       SI                        | 1e   30
                     47   // 1       BP                        | 1c   28
                     48   // 1       SP                        | 1a   26
                     49   // 1       BX                        | 18   24
                     50   // 1       DX                        | 16   22
                     51   // 1       CX                        | 14   20
                     52   // 1       AX                        | 12   18
                     53   // 1       flag word                 | 10   16
                     54   // 1       IP (entry point)          | 0e   14
                     55   // 0       SS for CPL 2              | 0c   12
                     56   // 0       SP for CPL 2              | 0a   10
                     57   // 0       SS for CPL 1              | 08   08
                     58   // 0       SP for CPL 1              | 06   06
                     59   // 0       SS for CPL 0              | 04   04
                     60   // 0       SP for CPL 0              | 02   02
                     61   //         back link selector to TSS | 00   00
                     62 
                     63 
                     64   // ======================
                     65   // 386 Task State Segment
                     66   // ======================
                     67   // |31            16|15                    0| hex dec
                     68   // |I/O Map Base    |000000000000000000000|T| 64  100 static
                     69   // |0000000000000000| LDT                   | 60  96  static
                     70   // |0000000000000000| GS selector           | 5c  92  dynamic
                     71   // |0000000000000000| FS selector           | 58  88  dynamic
                     72   // |0000000000000000| DS selector           | 54  84  dynamic
                     73   // |0000000000000000| SS selector           | 50  80  dynamic
                     74   // |0000000000000000| CS selector           | 4c  76  dynamic
                     75   // |0000000000000000| ES selector           | 48  72  dynamic
                     76   // |                EDI                     | 44  68  dynamic
                     77   // |                ESI                     | 40  64  dynamic
                     78   // |                EBP                     | 3c  60  dynamic
                     79   // |                ESP                     | 38  56  dynamic
                     80   // |                EBX                     | 34  52  dynamic
                     81   // |                EDX                     | 30  48  dynamic
                     82   // |                ECX                     | 2c  44  dynamic
                     83   // |                EAX                     | 28  40  dynamic
                     84   // |                EFLAGS                  | 24  36  dynamic
                     85   // |                EIP (entry point)       | 20  32  dynamic
                     86   // |           CR3 (PDPR)                   | 1c  28  static
                     87   // |000000000000000 | SS for CPL 2          | 18  24  static
                     88   // |           ESP for CPL 2                | 14  20  static
                     89   // |000000000000000 | SS for CPL 1          | 10  16  static
                     90   // |           ESP for CPL 1                | 0c  12  static
                     91   // |000000000000000 | SS for CPL 0          | 08  08  static
                     92   // |           ESP for CPL 0                | 04  04  static
                     93   // |000000000000000 | back link to prev TSS | 00  00  dynamic (updated only when return expected)
                     94 
                     95 
                     96   // ==================================================
                     97   // Effect of task switch on Busy, NT, and Link Fields
                     98   // ==================================================
                     99 
                    100   // Field         jump        call/interrupt     iret
                    101   // ------------------------------------------------------
                    102   // new busy bit  Set         Set                No change
                    103   // old busy bit  Cleared     No change          Cleared
                    104   // new NT flag   No change   Set                No change
                    105   // old NT flag   No change   No change          Cleared
                    106   // new link      No change   old TSS selector   No change
                    107   // old link      No change   No change          No change
                    108   // CR0.TS        Set         Set                Set
                    109 
                    110   // Note: I checked 386, 486, and Pentium, and they all exhibited
                    111   //       exactly the same behaviour as above.  There seems to
                    112   //       be some misprints in the Intel docs.
                    */
                #endregion

                Word lTop = (Word)(CurrentDecode.Op1Value.OpQWord >> 32);
                Byte lTSSDescriptor = (byte)(lTop >> 3);
                sGDTEntry theDescriptor = mProc.mGDTCache[lTSSDescriptor];
                int new_TSS_max = 0;

                    mProc.mSwitchSource = eSwitchSource.SWITCH_FROM_JMP;

                    /*Step 1: The following checks are made before calling task_switch(),
                        for JMP & CALL only. These checks are NOT made for exceptions,
                        interrupts & IRET.

                        1) TSS DPL must be >= CPL
                        2) TSS DPL must be >= TSS selector RPL
                        3) TSS descriptor is not busy.*/
                    if (theDescriptor.Value == 0)
                        throw new Exception($"Task switch: Bad TSS Selector {lTSSDescriptor.ToString("X")} of JMPF address {CurrentDecode.Op1Value.OpQWord.ToString("X")}");
                    if (!theDescriptor.access.Present)
                        throw new Exception("Task switch: TSS descriptor is not present");

                    /*  STEP 2: The processor performs limit-checking on the target TSS
                            to verify that the TSS limit is greater than or equal
                            to 67h (2Bh for 16-bit TSS).*/
                    if (theDescriptor.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Av || theDescriptor.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                        new_TSS_max = 0x67;
                    else
                        new_TSS_max = 0x2b;

                    if (theDescriptor.Limit < new_TSS_max)
                        throw new Exception("Task switch: new TSS limit < correct size for type");

                    if (theDescriptor.Base == mProc.regs.CS.Selector.Base)
                        if (mProc.mSystem.Debuggies.DebugCPU)
                            mProc.mSystem.PrintDebugMsg(eDebuggieNames.CPU, "Task switching to same task (Base = Base)");

                    // Check that old TSS, new TSS, and all segment descriptors
                    // used in the task switch are paged in.
                    //N/A FOR NOW

                    // Privilege and busy checks done in CALL, JUMP, INT, IRET

                    // STEP 3: Save the current task state in the TSS. Up to this point,
                    //         any exception that occurs aborts the task switch without
                    //         changing the processor state.
                    /* save current machine state in old task's TSS */
                    UInt32 lOldEFlags = mProc.regs.EFLAGS;
                    //sTSS lNewTSS = new sTSS(mProc.mCurrTSS.GetValue(mProc), new_TSS_max==0x67?true:false);

                    if (theDescriptor.access.m_SystemDescType == eSystemOrGateDescType.TSS_16_Bu || theDescriptor.access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                        lOldEFlags &= 0xFFFFBFFF;

                    if (new_TSS_max == 0x67)
                    {
                        mProc.mCurrTSS.EIP = mProc.regs.EIP;
                        mProc.mCurrTSS.EFLAGS = mProc.regs.EFLAGS;
                        mProc.mCurrTSS.EAX = mProc.regs.EAX;
                        mProc.mCurrTSS.ECX = mProc.regs.ECX;
                        mProc.mCurrTSS.EDX = mProc.regs.EDX;
                        mProc.mCurrTSS.EBX = mProc.regs.EBX;
                        if (mProc.regs.CPL == ePrivLvl.Kernel_Ring_0 && theDescriptor.access.PrivLvl == ePrivLvl.App_Level_3)
                            mProc.mCurrTSS.ESP0 = mProc.regs.ESP;
                        else
                            mProc.mCurrTSS.ESP = mProc.regs.ESP;
                        mProc.mCurrTSS.EBP = mProc.regs.EBP;
                        mProc.mCurrTSS.ESI = mProc.regs.ESI;
                        mProc.mCurrTSS.EDI = mProc.regs.EDI;
                        mProc.mCurrTSS.ES = (UInt16)mProc.regs.ES.mWholeSelectorValue;
                        mProc.mCurrTSS.CS = (UInt16)mProc.regs.CS.mWholeSelectorValue;
                        if (mProc.regs.CPL == ePrivLvl.Kernel_Ring_0 && theDescriptor.access.PrivLvl == ePrivLvl.App_Level_3)
                            mProc.mCurrTSS.SS0 = (UInt16)mProc.regs.SS.mWholeSelectorValue;
                        else
                            mProc.mCurrTSS.SS = (UInt16)mProc.regs.SS.mWholeSelectorValue;
                        mProc.mCurrTSS.DS = (UInt16)mProc.regs.DS.mWholeSelectorValue;
                        mProc.mCurrTSS.GS = (UInt16)mProc.regs.GS.mWholeSelectorValue;
                        mProc.mCurrTSS.FS = (UInt16)mProc.regs.FS.mWholeSelectorValue;
                        mProc.mCurrTSS.CR3 = mProc.regs.CR3;
                    }
                    else
                    {
                        mProc.mCurrTSS.EIP = mProc.regs.IP;
                        mProc.mCurrTSS.EFLAGS = mProc.regs.FLAGS;
                        mProc.mCurrTSS.EAX = mProc.regs.AX;
                        mProc.mCurrTSS.ECX = mProc.regs.CX;
                        mProc.mCurrTSS.EDX = mProc.regs.DX;
                        mProc.mCurrTSS.EBX = mProc.regs.BX;
                        mProc.mCurrTSS.ESP = mProc.regs.SP;
                        mProc.mCurrTSS.EBP = mProc.regs.BP;
                        mProc.mCurrTSS.ESI = mProc.regs.SI;
                        mProc.mCurrTSS.EDI = mProc.regs.DI;
                        mProc.mCurrTSS.ES = (UInt16)mProc.regs.ES.mWholeSelectorValue;
                        mProc.mCurrTSS.CS = (UInt16)mProc.regs.CS.mWholeSelectorValue;
                        mProc.mCurrTSS.SS = (UInt16)mProc.regs.SS.mWholeSelectorValue;
                        mProc.mCurrTSS.DS = (UInt16)mProc.regs.DS.mWholeSelectorValue;
                        mProc.mCurrTSS.CR3 = mProc.regs.CR3;
                    }

                    // effect on link field of new task
                    if (mProc.mSwitchSource == eSwitchSource.SWITCH_FROM_CALL || mProc.mSwitchSource == eSwitchSource.SWITCH_FROM_INT)
                        // set to selector of old task's TSS
                        mProc.mCurrTSS.PrevTaskLink = mProc.regs.TR.mSegSel;

                    //mProc.mCurrTSS = new sTSS(mProc.mem.Chunk(mProc, 0, mProc.regs.TR.mBase, 104),
                    //    mProc.mGDTCache[lTSSDescriptor].access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[lTSSDescriptor >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu);

                    //Set busy flag of current task!!!
                    if (mProc.mSwitchSource != eSwitchSource.SWITCH_FROM_IRET)
                        if (new_TSS_max == 0x67)
                            mProc.mGDTCache.SetBusyFlag(mProc, lTSSDescriptor, true);
                        else
                            mProc.mGDTCache.SetBusyFlag(mProc, lTSSDescriptor, false);

                    // Step 6: If JMP or IRET, clear busy bit in old task TSS descriptor,
                    //         otherwise leave set.
                    if (mProc.mSwitchSource == eSwitchSource.SWITCH_FROM_JMP || mProc.mSwitchSource == eSwitchSource.SWITCH_FROM_IRET)
                        mProc.mGDTCache.SetBusyFlag(mProc, (Word)(mProc.regs.TR.SegSel >> 3), false);

                    // Commit point.  At this point, we commit to the new
                    // processing, we complete the task switch without performing
                    // additional access and segment availablility checks and
                    // generate the appropriate exception prior to beginning
                    // execution of the new task.
                    mProc.mCurrTSS.Commit(mProc, ref CurrentDecode);

                    // Step 7: Load the task register with the segment selector and
                    //        descriptor for the new task TSS.
                    mProc.regs.TR.mSegSel = lTop;
                    mProc.mSystem.TaskSwitchBreakpoint = true;
                    if (mProc.mSystem.BreakOnSwitchToTaskNum((UInt32)((mProc.regs.TR.SegSel & 0xFFFF) >> 3)))
                        mProc.mSystem.mSwitchToTaskBreakpoint = true;
                    DWord lTempCR3 = mProc.mem.GetDWord(mProc, ref CurrentDecode, theDescriptor.Base + 28);
                    //CLR 07/17/2013: Added the conditional move of CR3 back in to match BOCHS code.
                    if (lTempCR3 != mProc.regs.CR3)
                        mProc.regs.CR3 = lTempCR3;
                    LTR.Load(mProc, ref CurrentDecode);

                    // Step 8: Set TS flag in the CR0 image stored in the new task TSS.
                    mProc.regs.CR0 |= 0x8;
                    // Task switch clears LE/L3/L2/L1/L0 in DR7
                    mProc.regs.DR7 &= ~(UInt32)(0x00000155);

                    // Step 9: If call or interrupt, set the NT flag in the eflags
                    //         image stored in new task's TSS.  If IRET or JMP,
                    //         NT is restored from new TSS eflags image. (no change)


                    // Step 10: Load the new task (dynamic) state from new TSS.
                    //          Any errors associated with loading and qualification of
                    //          segment descriptors in this step occur in the new task's
                    //          context.  State loaded here includes LDTR, CR3,
                    //          EFLAGS, EIP, general purpose registers, and segment
                    //          descriptor parts of the segment registers.
                    mProc.regs.EIP = mProc.mCurrTSS.EIP;
                    mProc.regs.EAX = mProc.mCurrTSS.EAX;
                    mProc.regs.ECX = mProc.mCurrTSS.ECX;
                    mProc.regs.EDX = mProc.mCurrTSS.EDX;
                    mProc.regs.EBX = mProc.mCurrTSS.EBX;
                    if (mProc.regs.CPL == ePrivLvl.App_Level_3 && theDescriptor.access.PrivLvl == ePrivLvl.Kernel_Ring_0)
                        mProc.regs.ESP = mProc.mCurrTSS.ESP0;
                    else
                        mProc.regs.ESP = mProc.mCurrTSS.ESP;
                    mProc.regs.EBP = mProc.mCurrTSS.EBP;
                    mProc.regs.ESI = mProc.mCurrTSS.ESI;
                    mProc.regs.EDI = mProc.mCurrTSS.EDI;

                    mProc.regs.EFLAGS = mProc.mCurrTSS.EFLAGS;
                    mProc.regs.CS.Value = mProc.mCurrTSS.CS;
                    if (mProc.regs.CPL == ePrivLvl.App_Level_3 && theDescriptor.access.PrivLvl == ePrivLvl.Kernel_Ring_0)
                        mProc.regs.SS.Value = mProc.mCurrTSS.SS0;
                    else
                        mProc.regs.SS.Value = mProc.mCurrTSS.SS;
                    mProc.regs.DS.Value = mProc.mCurrTSS.DS;
                    mProc.regs.ES.Value = mProc.mCurrTSS.ES;
                    mProc.regs.FS.Value = mProc.mCurrTSS.FS;
                    mProc.regs.GS.Value = mProc.mCurrTSS.GS;
                    mProc.regs.LDTR.mSegSel = mProc.mCurrTSS.LDT_SegSel;
                    LLDT.Load(mProc, ref CurrentDecode);

                    /* set CPL to 3 to force a privilege level change and stack switch if SS
                    is not properly loaded */
                    //mProc.regs.CPL = 3;
                    return;
                }


            if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                mProc.mem.SetDWord(mProc, ref CurrentDecode, Processor_80x86.RCS, PhysicalMem.GetSegForLoc(mProc, (UInt32)CurrentDecode.Op2Value.OpWord));
            else
                mProc.regs.CS.Value = CurrentDecode.Op1Value.OpDWord >> 0x10;
            mProc.regs.EIP = CurrentDecode.Op1Value.OpWord;
        }
    }
    public class JNA : Instruct
    {
        public JNA()
        {
            mName = "JNA";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not above (CF=1 or ZF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.FLAGSB.ZF) || (mProc.regs.FLAGSB.CF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNAE : Instruct
    {
        public JNAE()
        {
            mName = "JNAE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not above or equal (CF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNB : Instruct
    {
        public JNB()
        {
            mName = "JNB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not below (CF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNBE : Instruct
    {
        public JNBE()
        {
            mName = "JNBE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not below or equal (CF=0 and ZF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //09/03/10: Changed per BOCHS ... semantic only no logical change
            if ((!mProc.regs.FLAGSB.CF && !mProc.regs.FLAGSB.ZF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNC : Instruct
    {
        public JNC()
        {
            mName = "JNC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not carry (CF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((!mProc.regs.FLAGSB.CF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNE : Instruct
    {
        public JNE()
        {
            mName = "JNE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not equal (ZF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((!mProc.regs.FLAGSB.ZF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNG : Instruct
    {
        public JNG()
        {
            mName = "JNG";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not greater (ZF=1 or SF<>OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.FLAGSB.ZF) || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNGE : Instruct
    {
        public JNGE()
        {
            mName = "JNGE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not greater or equal (SF<>OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNLE : Instruct
    {
        public JNLE()
        {
            mName = "JNLE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not less or equal (ZF=0 and SF=OF)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //9/1/10: changed to match BOCHS instruction
            if ((!mProc.regs.FLAGSB.ZF && (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)))
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);

        }
    }
    public class JNO : Instruct
    {
        public JNO()
        {
            mName = "JNO";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not overflow (OF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNP : Instruct
    {
        public JNP()
        {
            mName = "JNP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not parity (PF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNS : Instruct
    {
        public JNS()
        {
            mName = "JNS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not sign (SF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.SF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JNZ : Instruct
    {
        public JNZ()
        {
            mName = "JNZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if not zero (ZF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JO : Instruct
    {
        public JO()
        {
            mName = "JO";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if overflow (OF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JP : Instruct
    {
        public JP()
        {
            mName = "JP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if parity (PF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JPE : Instruct
    {
        public JPE()
        {
            mName = "JPE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if parity even (PF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JPO : Instruct
    {
        public JPO()
        {
            mName = "JPO";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if parity odd (PF=0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (!mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);

        }
    }
    public class JS : Instruct
    {
        public JS()
        {
            mName = "JS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if sign (SF=1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.SF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class JZ : Instruct
    {
        public JZ()
        {
            mName = "JZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Jump short if zero (ZF = 1)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
        }
    }
    public class LAR : Instruct
    {
        public LAR()
        {
            mName = "LAR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Access Rights Byte";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
            {
                CurrentDecode.Op1Value.OpWord = (Word)(CurrentDecode.Op2Value.OpWord & 0xFF00);
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
            }
            else
            {
                CurrentDecode.Op1Value.OpDWord = CurrentDecode.Op2Value.OpDWord & 0x00f0ff00;
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);
            }

        }
    }
    public class LAHF : Instruct
    {
        public LAHF()
        {
            mName = "LAHF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Status Flags into AH Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //mProc.regs.AH = (byte)(mProc.regs.FLAGS & 0x00FF);
            ////09/03/2010: I already had the statement above but not the return below, so after setting
            ////AH=FLAGS, I would get the bits below, doesn't make sense
            //return;
            //mProc.regs.AH = (byte)(Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.SF));
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH += (byte)(Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.ZF));
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH += (byte)(Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.AF));
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH += (byte)(Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.PF));
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH += 1;
            //mProc.regs.AH <<= 1;
            //mProc.regs.AH += (byte)(Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.CF));

            mProc.regs.AH = (byte)mProc.regs.FLAGS;
            #region Instructions
            /*
                Operation
                AH ? EFLAGS(SF:ZF:0:AF:0:PF:1:CF);
                Flags Affected
                None (that is, the state of the flags in the EFLAGS register is not affected).
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class LEA : Instruct
    {
        public LEA()
        {
            mName = "LEA";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Effective Address";
            mModFlags = 0;
        }
        //TODO: Per Intel instructions, different size LEAs per opsize & addsize
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)CurrentDecode.Op2Add);
            else
                if (mProc.mCurrInstructAddrSize16)
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)CurrentDecode.Op2Add);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Add);

        }
    }
    public class LEAVE : Instruct
    {
        public LEAVE()
        {
            mName = "LEAVE";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "High Level Procedure Exit";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //07/19/2013 - changed stack size info back to what it used to be
            bool lAddrSize16 = mProc.AddrSize16Stack;
            sInstruction ins = CurrentDecode;

            UInt32 lTempEBP = mProc.regs.EBP;
            UInt16 lTempBP = mProc.regs.BP;
            UInt32 lPreESP = mProc.regs.ESP;

            mProc.POP.mNoSetMemoryToValue = false;
            ins.Op1Add = 0x0;
            if (CurrentDecode.lOpSize16)
            {
                mProc.regs.SP = mProc.regs.BP;
                ins.Op1Add = Processor_80x86.RBP;
                ins.Operand1IsRef = true;
                mProc.POP.Impl(ref ins);
            }
            else
            {
                mProc.regs.ESP = mProc.regs.EBP;
                ins.Op1Add = Processor_80x86.REBP;
                ins.Operand1IsRef = true;
                mProc.POP.Impl(ref ins);

                if (mProc.regs.ESP != lTempEBP + 4)
                    throw new Exception("Huh?");
            }
            if (ins.ExceptionThrown)
            {
                
                return;
            }
            //Debug.WriteLine("After LEAVE, SP=" + OpDecoder.ValueToHex(mProc.regs.SP, 4) + ", BP=" + OpDecoder.ValueToHex(mProc.regs.BP, 4));
            
            #region Instructions
            /*
             Operation
                IF StackAddressSize = 32
                THEN
                ESP ? EBP;
                ELSE (* StackAddressSize = 16*)
                SP ? BP;
                FI;
                IF OperandSize = 32
                THEN
                EBP ? Pop();
                ELSE (* OperandSize = 16*)
                BP ? op();
                FI;
                Flags Affected
                None.
                Protected Mode Exceptions
                #SS(0) If the EBP register points to a location that is not within the limits of the
                current stack segment.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made while the current privilege level is 3.
                Real-Address Mode Exceptions
                #GP If the EBP register points to a location outside of the effective address
                space from 0 to FFFFH.
                Virtual-8086 Mode Exceptions
                #GP(0) If the EBP register points to a location outside of the effective address
                space from 0 to FFFFH.
                #PF(fault-code) If a page fault occurs.
                #AC(0) If alignment checking is enabled and an unaligned memory reference is
                made.*/
            #endregion
        }
    }
    public class LDS : Instruct
    {
        public LDS()
        {
            mName = "LDS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Far Pointer - DS:Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
            {
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.DS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.DS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 4);
            }

        }
    }
    public class LES : Instruct
    {
        public LES()
        {
            mName = "LES";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Far Pointer - ES/Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //TODO: Update for protected mode 32 bit
            if (CurrentDecode.lOpSize16)
            {
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.ES.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.ES.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 4);
            }

            #region Instructions
            #endregion
        }
    }
    public class LFS : Instruct
    {
        public LFS()
        {
            mName = "LFS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Far Pointer - FS:Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
            {
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.FS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.FS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 4);
            }

        }
    }
    public class LGS : Instruct
    {
        public LGS()
        {
            mName = "LGS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Far Pointer - GS:Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
            {
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.GS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.GS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 4);
            }

        }
    }
    public class LGDT : Instruct
    {
        public LGDT()
        {
            mName = "LGDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Global Interrupt Descriptor Table Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1Value.OpQWord != mProc.regs.GDTR.mOriginalValue)
            {
                mProc.regs.GDTR.Parse(mProc, CurrentDecode.Op1Value.OpQWord);
                if (Processor_80x86.mCurrInstructOpMode != ProcessorMode.Real)
                    mProc.regs.GDTR.mBase = PhysicalMem.PagedMemoryAddress(mProc, ref CurrentDecode, mProc.regs.GDTR.Base, false);
                if (CurrentDecode.ExceptionThrown)
                    return;
            }
            mProc.RefreshGDTCache();
            #region Instructions
            #endregion

        }
    }
    public class LIDT : Instruct
    {
        public LIDT()
        {
            mName = "LIDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Interrupt Descriptor Table Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1Value.OpQWord != mProc.regs.IDTR.mOriginalValue)
            {
                mProc.regs.IDTR.Parse(mProc, CurrentDecode.Op1Value.OpQWord);
                if (Processor_80x86.mCurrInstructOpMode != ProcessorMode.Real)
                    mProc.regs.IDTR.mBase = PhysicalMem.PagedMemoryAddress(mProc, ref CurrentDecode, mProc.regs.IDTR.Base, false);
                if (CurrentDecode.ExceptionThrown)
                    return;
            }
            mProc.RefreshIDTCache();

            #region Instructions
            #endregion
        }
    }
    public class LLDT : Instruct
    {
        public LLDT()
        {
            mName = "LLDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Local Descriptor Table";
            mModFlags = 0;
        }
        public static void Load(Processor_80x86 mProc, ref sInstruction sIns)
        {
            mProc.regs.LDTR.mBase = mProc.mGDTCache[mProc.regs.LDTR.mSegSel >> 3].Base;
            if (Processor_80x86.mCurrInstructOpMode != ProcessorMode.Real && mProc.regs.LDTR.mSegSel != 0x0)
                mProc.regs.LDTR.mBase = PhysicalMem.PagedMemoryAddress(mProc, ref sIns, mProc.regs.LDTR.Base, false);
            if (sIns.ExceptionThrown)
                return;
            mProc.regs.LDTR.mLimit = mProc.mGDTCache[mProc.regs.LDTR.mSegSel >> 3].Limit;
            mProc.RefreshLDTCache();
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.LDTR.mSegSel = CurrentDecode.Op1Value.OpWord;
            Load(mProc, ref CurrentDecode);
            #region Instructions
            #endregion

        }
    }
    public class LMSW : Instruct
    {
        public LMSW()
        {
            mName = "LMSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Machine Status Word (CR0)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.CR0 |= (byte)(CurrentDecode.Op1Value.OpWord & 0x0f);
            #region Instructions
            #endregion
        }
    }
    public class LODSB : Instruct
    {
        public LODSB()
        {
            mName = "LODSB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Load String Bytes";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, mProc.mCurrInstructAddrSize16);
            do
            {
                mProc.regs.AL = PhysicalMem.GetByte(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 1, true, false, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                { return; }
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

        }
    }
    public class LODSW : Instruct
    {
        public LODSW()
        {
            mName = "LODSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Load String Word";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (!mProc.mCurrInstructOpSize16)
            {
                mProc.LODSD.UsageCount++;
                mProc.LODSD.Impl(ref CurrentDecode);
    
                return;
            }

            //if (mProc.mCurrInstructAddrSize16)
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.SI);
            //else
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.ESI);

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, mProc.mCurrInstructAddrSize16);
            do
            {
                mProc.regs.AX = mProc.mem.GetWord(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 2, true, false, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                { return; }
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

        }
    }
    public class LODSD : Instruct
    {
        public LODSD()
        {
            mName = "LODSD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Load String DoubleWord";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, mProc.mCurrInstructAddrSize16);
            do
            {
                mProc.regs.EAX = mProc.mem.GetDWord(mProc, ref CurrentDecode, lSource);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 4, true, false, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                { return; }
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

        }
    }
    public class LOOP : Instruct
    {
        public LOOP()
        {
            mName = "LOOP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mFlowReturnsLater = true;
            REPAble = true;
            mDescription = "Decrement CX and Loop if CX Not Zero ";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.mCurrInstructAddrSize16)
            {
                if (--mProc.regs.CX != 0)
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
            }
            else
                if (--mProc.regs.ECX != 0)
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);


            #region Instructions
            #endregion
        }
    }
    public class LOOPE : Instruct
    {
        public LOOPE()
        {
            mName = "LOOPE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mFlowReturnsLater = true;
            REPAble = true;
            mDescription = "Loop While Equal";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (mProc.mAddrSize16)
            {
                if ((--mProc.regs.CX != 0) & (mProc.regs.FLAGSB.ZF))
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
            }
            else
                if ((--mProc.regs.ECX != 0) & (mProc.regs.FLAGSB.ZF))
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);

            //UpdIPForShortJump(Op1Val);
            #region Instructions
            #endregion
        }
    }
    public class LOOPZ : Instruct
    {
        public LOOPZ()
        {
            mName = "LOOPZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mFlowReturnsLater = true;
            mDescription = "Loop While Zero";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.LOOPE.Impl(ref CurrentDecode);

        }
    }
    public class LOOPNE : Instruct
    {
        public LOOPNE()
        {
            mName = "LOOPNE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mFlowReturnsLater = true;
            mDescription = "Loop While Not Equal";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            LoopNE_Logic(mProc, ref CurrentDecode);

            #region Instructions
            #endregion
        }
        public void LoopNE_Logic(Processor_80x86 mProc, ref sInstruction CurrentDecode)
        {
            if (mProc.mCurrInstructAddrSize16)
            {
                if ((--mProc.regs.CX != 0) && (!mProc.regs.FLAGSB.ZF))
                {
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
                }
            }
            else
                if ((--mProc.regs.ECX != 0) && (!mProc.regs.FLAGSB.ZF))
                {
                    UpdIPForShortJump(CurrentDecode.Op1Value, CurrentDecode.Op1TypeCode);
                }
        }
    }
    public class LOOPNZ : Instruct
    {
        public LOOPNZ()
        {
            mName = "LOOPNZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mFlowReturnsLater = true;
            REPAble = true;
            mDescription = "Loop While Not Equal";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            ((LOOPNE)(mProc.LOOPNE)).LoopNE_Logic(mProc, ref CurrentDecode);


            #region Instructions
            #endregion
        }
    }
    public class LSS : Instruct
    {
        public LSS()
        {
            mName = "LSS";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Far Pointer - SS:Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.lOpSize16)
            {
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.SS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, mProc.mem.GetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add));
                mProc.regs.SS.Value = mProc.mem.GetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add + 4);
            }

        }
    }
    public class LTR : Instruct
    {
        public LTR()
        {
            mName = "LTR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Load Task Register";
            mModFlags = 0;
        }
        public static void Load(Processor_80x86 mProc, ref sInstruction sIns)
        {
            mProc.regs.TR.mBase = mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].Base;
            mProc.regs.TR.mLimit = mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].Limit;
            mProc.regs.TR.lCache = mProc.mem.Chunk(mProc, ref sIns, 0, mProc.regs.TR.mBase, 104);
            mProc.mCurrTSS = new sTSS(mProc.mem.Chunk(mProc, ref sIns, 0, mProc.regs.TR.mBase, 104),
                mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.m_SystemDescType == eSystemOrGateDescType.TSS_32_Bu);
            mProc.mSystem.TaskSwitchBreakpoint = true;
            if (mProc.mSystem.BreakOnSwitchToTaskNum((UInt32)((mProc.regs.TR.SegSel & 0xFFFF) >> 3)))
                mProc.mSystem.mSwitchToTaskBreakpoint = true;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.TR.mSegSel = CurrentDecode.Op1Value.OpWord;
            Load(mProc, ref CurrentDecode);

            #region Instructions
            #endregion
        }
    }
    public class MOV : Instruct
    {
        public MOV()
        {
            mName = "MOV";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Move";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //if (mProc.regs.EIP == 0x0C09 || mProc.regs.EIP == 0x0C0B)
                //System.Diagnostics.Debugger.Break();
            if (CurrentDecode.Op1Add == Processor_80x86.REGADDRBASE + Processor_80x86.RCSOFS)
            {
                //#UD - Can't mov to CS
                CurrentDecode.ExceptionNumber = 0x6;
                CurrentDecode.ExceptionThrown = true;
                return;

            }
            if ((CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE + Processor_80x86.RCSOFS) && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS) && Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
            {
            }
                if ((CurrentDecode.Op2Add >= Processor_80x86.REGADDRBASE + Processor_80x86.RCSOFS) && (CurrentDecode.Op2Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS) && Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
            {
                if (CurrentDecode.lOpSize16)
                    CurrentDecode.Op2Value.OpWord = (UInt16)Misc.GetSelectorForSegment(mProc, CurrentDecode.Op2Add, CurrentDecode.Op2Value.OpWord);
                else
                    CurrentDecode.Op2Value.OpDWord = Misc.GetSelectorForSegment(mProc, CurrentDecode.Op2Add, CurrentDecode.Op2Value.OpDWord);
            }

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpQWord);
                    break;
                default:
                    throw new Exception("MOV: Unknown Operand 1 type code");
            }

        }

    }
    public class MOVSB : Instruct
    {
        public MOVSB()
        {
            mName = "MOVSB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Move byte at address DS:(E)SI to address ES:(E)DI";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lDest = 0;

            //mProc.mCurrentInstruction.mName = "MOVSB\n";
            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            MOVSW.SetupMOVSSrc(mProc, ref lSource, mProc.mCurrInstructAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, false);

            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetByte(mProc, ref CurrentDecode, lDest, PhysicalMem.GetByte(mProc, ref CurrentDecode, lSource));
                //mProc.mCurrentInstruction.mName += PhysicalMem.GetByte(mProc, ref DecodedInstruction, lSource).ToString("X2") + ", ";
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 1, true, true, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class MOVSW : Instruct
    {
        public MOVSW()
        {
            mName = "MOVSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Move byte at address DS:(E)SI to address ES:(E)DI";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lDest = 0;



            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            ////Remove next 4 lines
            //MOVSW.SetupMOVSSrc(mProc, ref lSource, mProc.mCurrInstructAddrSize16, true);
            //MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, false);
            //if (lLoopCounter > 0)
            //    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "MOVSW: Entered - Source = " + lSource.ToString("X8") + " Dest = " + lDest.ToString("X8") + " - ECX = " + mProc.regs.ECX.ToString("X8"));


            if (!mProc.mCurrInstructOpSize16)
            {
                mProc.MOVSD.UsageCount++;
                this.UsageCount--;
                mProc.MOVSD.Impl(ref CurrentDecode);
    
                return;
            }

            MOVSW.SetupMOVSSrc(mProc, ref lSource, mProc.mCurrInstructAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, false);
            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetWord(mProc, ref CurrentDecode, lDest, mProc.mem.GetWord(mProc, ref CurrentDecode, lSource));
                //mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "MOVSW: " + lSource.ToString("X8") + " --> " + lDest.ToString("X8") + " = " + mProc.mem.GetWord(mProc, ref DecodedInstruction, lSource).ToString("X4"));
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 2, true, true, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
        }
        public static void IncDec(Processor_80x86 mProc, ref DWord Dest, ref DWord Src, bool AddrSize16, byte Size, bool IncSource, bool IncDest, bool SourceOverridable, bool DestOverridable)
        {
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)
                if (AddrSize16)
                    mProc.regs.CX--;
                else
                    mProc.regs.ECX--;

            if (AddrSize16)
                if (mProc.regs.FLAGSB.DF)
                {
                    if (IncSource)
                    {
                        mProc.regs.SI -= Size;
                        Src -= Size;
                    }
                    if (IncDest)
                    {
                        mProc.regs.DI -= Size;
                        Dest -= Size;
                    }
                }
                else
                {
                    if (IncSource)
                    {
                        mProc.regs.SI += Size;
                        Src += Size;
                    }
                    if (IncDest)
                    {
                        mProc.regs.DI += Size;
                        Dest += Size;
                    }
                }
            else
                if (mProc.regs.FLAGSB.DF)
                {
                    if (IncSource)
                    {
                        mProc.regs.ESI -= Size;
                        Src -= Size;
                    }
                    if (IncDest)
                    {
                        mProc.regs.EDI -= Size;
                        Dest -= Size;
                    }
                }
                else
                {
                    if (IncSource)
                    {
                        mProc.regs.ESI += Size;
                        Src += Size;
                    }
                    if (IncDest)
                    {
                        mProc.regs.EDI += Size;
                        Dest += Size;
                    }
                }

            if (IncSource && ((AddrSize16 && (mProc.regs.SI >= 0xFFFD || mProc.regs.SI <= 0x004)) || (!AddrSize16 && (mProc.regs.ESI >= 0xFFFFFFFD || mProc.regs.ESI <= 0x004))))
                SetupMOVSSrc(mProc, ref Src, AddrSize16, SourceOverridable);

            if (IncDest && ((AddrSize16 && (mProc.regs.DI >= 0xFFFD || mProc.regs.DI <= 0x004)) || (!AddrSize16 && (mProc.regs.EDI >= 0xFFFFFFFD || mProc.regs.EDI <= 0x004))))
                SetupMOVSDest(mProc, ref Dest, AddrSize16, DestOverridable, false);

        }
        public static void SetupMOVSSrcDest(Processor_80x86 mProc, ref DWord Source, ref DWord Dest, bool AddrSize16)
        {
            switch (mProc.mSegmentOverride)
            {
                case Processor_80x86.RSS:
                    if (AddrSize16)
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SI);
                    else
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESI);
                    break;
                case Processor_80x86.RES:
                    if (AddrSize16)
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.SI);
                    else
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.ESI);
                    break;
                case Processor_80x86.RCS:
                    if (AddrSize16)
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.SI);
                    else
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.ESI);
                    break;
                default:
                    if (AddrSize16)
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.SI);
                    else
                        Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.ESI);
                    break;
            }
            if (AddrSize16)
                Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
            else
                Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
        }
        public static void SetupMOVSSrc(Processor_80x86 mProc, ref DWord Source, bool AddrSize16, bool Overrideable)
        {
            if (mProc.mSegmentOverride == Processor_80x86.RSS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == Processor_80x86.RES && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == Processor_80x86.RFS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.FS, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.FS, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == Processor_80x86.RGS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.GS, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.GS, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == Processor_80x86.RCS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.ESI);
            else
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.ESI);
        }
        public static void SetupMOVSDest(Processor_80x86 mProc, ref DWord Dest, bool AddrSize16, bool Overrideable, bool DWordInstruction)
        {
            if (mProc.mSegmentOverride == Processor_80x86.RSS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == Processor_80x86.RES && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == Processor_80x86.RFS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.FS, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.FS, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == Processor_80x86.RGS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.GS, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.GS, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == Processor_80x86.RCS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.CS, mProc.regs.EDI);
            else
                if (AddrSize16)
                    //if ( (mProc.mSystem.OSType == eOpSysType.Linux &&  AddrSize16 )) //|| (mProc.mSystem.OSType == eOpSysType.Other && (AddrSize16 || (DWordInstruction==true && !AddrSize16))))
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI);
        }
    }
    public class MOVSD : Instruct
    {
        public MOVSD()
        {
            mName = "MOVSD";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Move byte at address DS:(E)SI to address ES:(E)DI";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lSource = 0;
            DWord lDest = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            MOVSW.SetupMOVSSrc(mProc, ref lSource, mProc.mCurrInstructAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, true);

            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetDWord(mProc, ref CurrentDecode, lDest, mProc.mem.GetDWord(mProc, ref CurrentDecode, lSource));
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lSource, mProc.mCurrInstructAddrSize16, 4, true, true, true, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class MOVSX : Instruct
    {
        public MOVSX()
        {
            mName = "MOVSX";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Move - Sign Extend";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.OpCode == 0x0FBE)
                if (CurrentDecode.lOpSize16)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Misc.SignExtend(CurrentDecode.Op2Value.OpByte));
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Misc.SignExtend(Misc.SignExtend(CurrentDecode.Op2Value.OpByte)));
            else
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Misc.SignExtend(CurrentDecode.Op2Value.OpWord));

        }
    }
    public class MOVZX : Instruct
    {
        public MOVZX()
        {
            mName = "MOVZX";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Move - Zero Extend";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.OpCode == 0x0FB6)
                if (CurrentDecode.lOpSize16)
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpByte);
                else
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpByte);
            else
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpWord);

        }
    }
    public class MUL : Instruct
    {
        public MUL()
        {
            mName = "MUL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Unsigned Multiply";
            mModFlags = eFLAGS.OF | eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lRes32 = 0;
            UInt64 lRes64 = 0;
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.regs.AX = (UInt16)((UInt16)mProc.regs.AL * (UInt16)CurrentDecode.Op1Value.OpByte);
                    if (mProc.regs.AH == 0)
                    {
                        mProc.regs.setFlagOF(false);
                        mProc.regs.setFlagCF(false);
                    }
                    else
                    {
                        mProc.regs.setFlagOF(true);
                        mProc.regs.setFlagCF(true);
                    }

                    break;
                case TypeCode.UInt16:
                    lRes32 = ((UInt32)mProc.regs.AX * (UInt32)CurrentDecode.Op1Value.OpWord);
                    mProc.regs.DX = Misc.GetHi(lRes32);
                    mProc.regs.AX = Misc.GetLo(lRes32);
                    if (mProc.regs.DX == 0)
                    {
                        mProc.regs.setFlagOF(false);
                        mProc.regs.setFlagCF(false);
                    }
                    else
                    {
                        mProc.regs.setFlagOF(true);
                        mProc.regs.setFlagCF(true);
                    }
                    break;
                default:
                    lRes64 = (UInt64)((UInt64)mProc.regs.EAX * (UInt64)CurrentDecode.Op1Value.OpQWord);
                    mProc.regs.EDX = Misc.GetHi(lRes64);
                    mProc.regs.EAX = Misc.GetLo(lRes64);
                    if (mProc.regs.EDX == 0)
                    {
                        mProc.regs.setFlagOF(false);
                        mProc.regs.setFlagCF(false);
                    }
                    else
                    {
                        mProc.regs.setFlagOF(true);
                        mProc.regs.setFlagCF(true);
                    }
                    break;
            }
        }
    }
    public class NEG : Instruct
    {
        public NEG()
        {
            mName = "NEG";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Two's Complement Negation";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sOpVal lOp1Val = CurrentDecode.Op1Value, lPreVal1 = CurrentDecode.Op1Value;

            mProc.regs.setFlagCF(CurrentDecode.Op1Value.OpQWord > 0);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Val.OpByte = (byte)-(lOp1Val.OpByte);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpByte);
                    mProc.regs.setFlagOF_Sub(lPreVal1.OpByte, (byte)-(lOp1Val.OpByte), lOp1Val.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Val.OpWord = (Word)(-(lOp1Val.OpWord));
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpWord);
                    mProc.regs.setFlagOF_Sub(lPreVal1.OpWord, (Word)(-(lOp1Val.OpWord)), lOp1Val.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Val.OpDWord = (DWord)(-(lOp1Val.OpDWord));
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Val.OpDWord);
                    mProc.regs.setFlagOF_Sub(lPreVal1.OpDWord, (Word)(-(lOp1Val.OpDWord)), lOp1Val.OpDWord);
                    break;
                default:
                    throw new Exception("Cannot negate a quad word");
            }

            SetFlagsForSubtraction(mProc, lPreVal1, CurrentDecode.Op1Value, lOp1Val, CurrentDecode.Op1TypeCode);

            #region Instructions
            #endregion
        }
    }
    public class NOP : Instruct
    {
        public NOP()
        {
            mName = "NOP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "No Operation";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //todo: Make this call XCHG AX,AX once XCHG is coded
            //Naah, we'll just simulate the length the instructon would take to execute
            //System.Threading.Thread.Sleep(1);
            #region Instructions
            #endregion
        }
    }
    public class NOT : Instruct
    {
        public NOT()
        {
            mName = "NOT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "One's Complement Negation";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lOp1Value = CurrentDecode.Op1Value;

            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte = (byte)(~lOp1Value.OpByte);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    return;
                case TypeCode.UInt16:
                    lOp1Value.OpWord = (Word)(~lOp1Value.OpWord);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    return;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord = (DWord)(~lOp1Value.OpDWord);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    return;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord = (QWord)(~lOp1Value.OpQWord);
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    return;
            }

            //No flags set
            #region Instructions
            #endregion
        }
    }
    public class OR : Instruct
    {
        public OR()
        {
            mName = "OR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Logical Inclusive OR";
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = CurrentDecode.Op2Value;
            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);
            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte |= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord |= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord |= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord |= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            //Set the flags
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);

            //AF flag undefined
            #region Instructions
            #endregion
        }
    }
    public class OUT : Instruct
    {
        public OUT()
        {
            mName = "OUT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Output Data to Port";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {

            switch (CurrentDecode.RealOpCode)
            {
                case 0xE6:
                    mProc.ports.Out(CurrentDecode.Op1Value.OpByte, mProc.regs.AL, TypeCode.Byte);
                    return;
                case 0xE7:
                    if (CurrentDecode.lOpSize16)
                        mProc.ports.Out(CurrentDecode.Op1Value.OpByte, mProc.regs.AX, TypeCode.UInt16);
                    else
                        mProc.ports.Out(CurrentDecode.Op1Value.OpByte, mProc.regs.EAX, TypeCode.UInt32);
                    return;
                case 0xEE:
                    mProc.ports.Out(mProc.regs.DX, mProc.regs.AL, TypeCode.Byte);
                    return;
                case 0xEF:
                    if (CurrentDecode.lOpSize16)
                        mProc.ports.Out(mProc.regs.DX, mProc.regs.AX, TypeCode.UInt16);
                    else
                        mProc.ports.Out(mProc.regs.DX, mProc.regs.EAX, TypeCode.UInt32);
                    return;
            }
            #region Instructions
            #endregion
        }
    }
    public class OUTS : Instruct
    {
        public OUTS()
        {
            mName = "OUTS";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Output String to Port";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {


            DWord lSource, lJunk = 0;
            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            do
            {
                if (mProc.mCurrInstructAddrSize16)
                    lSource = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.SI);
                else
                    lSource = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.ESI);

                if (CurrentDecode.RealOpCode == 0x6e)
                {
                    mProc.ports.Out(CurrentDecode.Op1Value.OpByte, PhysicalMem.GetByte(mProc, ref CurrentDecode, lSource), TypeCode.Byte);
                    if (CurrentDecode.ExceptionThrown)
                    { return; }
                    MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 1, true, false, false, false);
                }
                else
                {
                    if (CurrentDecode.lOpSize16)
                    {
                        mProc.ports.Out(mProc.regs.DX, (Word)mProc.mem.GetWord(mProc, ref CurrentDecode, lSource), TypeCode.UInt16);
                        MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 2, true, false, false, false);
                    }
                    else
                    {
                        mProc.ports.Out(mProc.regs.DX, (DWord)mProc.mem.GetDWord(mProc, ref CurrentDecode, lSource), TypeCode.UInt32);
                        MOVSW.IncDec(mProc, ref lJunk, ref lSource, mProc.mCurrInstructAddrSize16, 4, true, false, false, false);
                    }
                }
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class POP : Instruct
    {
        public POP()
        {
            mName = "POP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "POP value Off the Stack";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //07/19/2013
            //Operand size. The D flag in the current code-segment descriptor determines the default operand size; it may be overridden by instruction prefixes (66H or REX.W).
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16Stack;

            CurrentDecode.Op1Value.OpQWord = 0;
            if (lAddrSize16)
                if (lOpSize16)
                {
                    CurrentDecode.Op1Value.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SP));
                    if (CurrentDecode.Op1Add != Processor_80x86.RSP) mProc.regs.SP += 2;
                }
                else
                {
                    CurrentDecode.Op1Value.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SP));
                    if (CurrentDecode.Op1Add != Processor_80x86.RSP) mProc.regs.SP += 4;
                }
            else
                if (lOpSize16)
                {
                    CurrentDecode.Op1Value.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESP));
                    if (CurrentDecode.Op1Add != Processor_80x86.RSP) mProc.regs.ESP += 2;
                }
                else
                {
                    CurrentDecode.Op1Value.OpDWord = mProc.mem.GetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESP));
                    if (CurrentDecode.Op1Add != Processor_80x86.RSP) mProc.regs.ESP += 4;
                }

            if (CurrentDecode.Op1.EffReg1 == eGeneralRegister.SP || CurrentDecode.Op1.EffReg1 == eGeneralRegister.ESP
            || CurrentDecode.Op1.EffReg2 == eGeneralRegister.SP || CurrentDecode.Op1.EffReg2 == eGeneralRegister.ESP)
            {
                if (lAddrSize16)
                    if (lOpSize16)
                        CurrentDecode.Op1Add += 2;
                    else
                        CurrentDecode.Op1Add += 4;
                else
                    if (lOpSize16)
                        CurrentDecode.Op1Add += 2;
                    else
                        CurrentDecode.Op1Add += 4;
            }

            //Called by IRET for example ... we just want the value back in Op1Val
            if (this.mNoSetMemoryToValue)
                return;

            if (lOpSize16)
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpWord);
            else
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op1Value.OpDWord);

            if (CurrentDecode.Op1Add == Processor_80x86.RSP && lOpSize16)
            {
                mProc.regs.ESP &= 0x0000FFFF;
                //mProc.regs.SP += 2;
            }

            #region Instructions
            #endregion
        }
    }
    public class POPA : Instruct
    {
        public POPA()
        {
            mName = "POPA";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Pop All Registers off Stack (80188)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sInstruction ins = CurrentDecode;

            if (CurrentDecode.lOpSize16)
            {
                //Instruct mProc.POP = mProc.Instructions["POP"];
                mProc.POP.mNoSetMemoryToValue = false;
                ins.Operand1IsRef = true;
                ins.Op1Add = Processor_80x86.RDI; mProc.POP.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                ins.Op1Add = Processor_80x86.RSI; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.RBP; mProc.POP.Impl(ref ins);
                mProc.regs.SP += 2;
                ins.Op1Add = Processor_80x86.RBX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.RDX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.RCX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.RAX; mProc.POP.Impl(ref ins);
            }
            else
            {
                mProc.POP.mNoSetMemoryToValue = false;
                ins.Op1Add = Processor_80x86.REDI; mProc.POP.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                ins.Op1Add = Processor_80x86.RESI; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.REBP; mProc.POP.Impl(ref ins);
                mProc.regs.SP += 4;
                ins.Op1Add = Processor_80x86.REBX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.REDX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.RECX; mProc.POP.Impl(ref ins);
                ins.Op1Add = Processor_80x86.REAX; mProc.POP.Impl(ref ins);
            }
        }
    }
    public class POPF : Instruct
    {
        public POPF()
        {
            mName = "POPF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Pop Flags off Stack ";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sInstruction ins = CurrentDecode;
            DWord lTempFlags, lPrePopFlags;

            mProc.POP.mNoSetMemoryToValue = true;
            ins.Operand1IsRef = false;
            mProc.POP.Impl(ref ins);
            if (ins.ExceptionThrown)
            {

                return;
            }
            lTempFlags = ins.Op1Value.OpDWord;
            {
                if (mProc.regs.CS.Selector.access.PrivLvl == 0)
                {
                    if (CurrentDecode.lOpSize16)
                    {
                        mProc.regs.FLAGS = (Word)(lTempFlags | 0x2);
                    }
                    else
                    {
                        lPrePopFlags = (DWord)(mProc.regs.EFLAGS & 0x1A0000);
                        mProc.regs.EFLAGS = lTempFlags & 0x25FFFF;
                        mProc.regs.EFLAGS |= lPrePopFlags | 0x2;
                    }
                }
                else //IOPL > 0
                    if (CurrentDecode.lOpSize16)
                {
                    //Keep only the following flag ... IOPL
                    lPrePopFlags = (Word)(mProc.regs.FLAGS & 0x3000);
                    mProc.regs.FLAGS = (Word)(lTempFlags & 0x3D7FFF);
                    mProc.regs.FLAGS |= (Word)(lPrePopFlags | 0x2);
                    mProc.regs.setFlagVIP(false);
                    mProc.regs.setFlagVIF(false);
                }
                else
                {
                    lPrePopFlags = (Word)(mProc.regs.FLAGS & 0x2B000);
                    mProc.regs.EFLAGS = lTempFlags & 0x3D4FFF;
                    mProc.regs.EFLAGS |= (DWord)lPrePopFlags | 0x2;
                    mProc.regs.setFlagVIP(false);
                    mProc.regs.setFlagVIF(false);
                }
            }
        }
    }
    public class PUSH : Instruct
    {
        public PUSH()
        {
            mName = "PUSH";
            mProc8086 = true;   //Not immediate version
            mProc8088 = true;   //Not immediate version
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Push Value Onto the Stack";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //C07/23/2013 - Changed stack logic again, both here and on POP
            bool lMyOpSize16 = mProc.OpSize16;
            bool lMyAddrSize16Stack = mProc.AddrSize16Stack;
            UInt32 lESP = mProc.regs.ESP;
            UInt16 lSP = mProc.regs.SP;

            //If other instructions call this one (like CALL) then we only want to use the 16 bit registers
            if (mNoSetMemoryToValue)
                lMyAddrSize16Stack = true;
            if (CurrentDecode.mOverrideAddrSizeFor32BitGate)
            {
                lMyOpSize16 = false;
            }

            UInt32 lTemp32 = CurrentDecode.Op1Value.OpDWord;

            if (CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
                CurrentDecode.Operand1IsRef = true;

            if (CurrentDecode.Operand1IsRef && CurrentDecode.Op1Add == Processor_80x86.RSP)  //SP & ESP have the same location
                lTemp32 = mProc.regs.ESP;
            if ((CurrentDecode.Op1Add >= Processor_80x86.REGADDRBASE + Processor_80x86.RCSOFS && (CurrentDecode.Op1Add <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS)) && Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
            {
                DWord lTempT = Misc.GetSelectorForSegment(mProc, CurrentDecode.Op1Add, 0);
                if (lTempT != 0)
                {
                    lTemp32 = lTempT;
                }
            }

            if (lMyAddrSize16Stack)
                if (lMyOpSize16)
                    mProc.regs.SP -= 2;
                else
                    mProc.regs.SP -= 4;
            else
                if (lMyOpSize16)
                    mProc.regs.ESP -= 2;
                else
                    mProc.regs.ESP -= 4;


            int a = 0;
            if (!CurrentDecode.Operand1IsRef && CurrentDecode.OpCode == 0x6A && CurrentDecode.Op1TypeCode == TypeCode.Byte && lTemp32 >= 0x100)
                a = (int)mProc.regs.ESP + 5 - 5;
            if (!CurrentDecode.Operand1IsRef && CurrentDecode.OpCode == 0x6A && CurrentDecode.Op1TypeCode == TypeCode.Byte && lTemp32 < 0x100)
            {
                //Otherwise we are pushing 32, so make the word a dword
                if (lMyOpSize16)
                    //Operand size is 16 then we are pushing 16 bits so make the byte into a sign extended word
                    lTemp32 = (Word)Misc.SignExtend((byte)lTemp32);
                else
                    //Operand size is 32 then we are pushing 32 bits so make the byte into a sign extended dword
                    lTemp32 = Misc.SignExtend((Word)Misc.SignExtend((byte)lTemp32));
            }

            if (lMyAddrSize16Stack)
            {
                //If the source operand is an immediate and its size is less than the operand size, a sign-extended value is pushed on the stack
                if (lMyOpSize16)
                {
                    mProc.mem.SetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SP), (Word)lTemp32);
                }
                else
                {
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.SP), lTemp32);
                }
            }
            else
            {
                if (lMyOpSize16)
                {
                    mProc.mem.SetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESP), (Word)lTemp32);
                }
                else
                {
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.SS, mProc.regs.ESP), lTemp32);
                }
            }

#if !DISABLE_STACK_OVERFLOW
            if (mProc.regs.SP <= 1)
                throw new  StackOverflowException();
#endif
        }
        //only to be used publicly
        #region Instructions
        #endregion
        //only to be used publicly
    }
    public class PUSHA : Instruct
    {
        public PUSHA()
        {
            mName = "PUSHA";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PUsh All Registers onto Stack (80188)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //C07/23/2013 - Changed stack logic again, both here and on POP

            UInt16 lOrigSP = mProc.regs.SP;
            UInt32 lOrigESP = mProc.regs.ESP;
            sInstruction ins = CurrentDecode;

            if (CurrentDecode.lOpSize16)
            {
                ins.Op1Value.OpWord = mProc.regs.AX;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                ins.Op1Value.OpWord = mProc.regs.CX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = mProc.regs.DX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = mProc.regs.BX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = lOrigSP;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = mProc.regs.BP;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = mProc.regs.SI;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpWord = mProc.regs.DI;
                mProc.PUSH.Impl(ref ins);
            }
            else
            {
                ins.Op1Value.OpDWord = mProc.regs.EAX;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
                ins.Op1Value.OpDWord = mProc.regs.ECX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = mProc.regs.EDX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = mProc.regs.EBX;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = lOrigESP;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = mProc.regs.EBP;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = mProc.regs.ESI;
                mProc.PUSH.Impl(ref ins);
                ins.Op1Value.OpDWord = mProc.regs.EDI;
                mProc.PUSH.Impl(ref ins);
                
            
            }
        }
    }
    public class PUSHAD : Instruct
    {
        public PUSHAD()
        {
            mName = "PUSHAD";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Push All Extended Registers onto Stack (80188)";
            mModFlags = 0;
        }

        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lOrigESP = mProc.regs.ESP;
            sInstruction ins = CurrentDecode;

            ins.Op1Value.OpDWord = mProc.regs.EAX;
            ins.Operand1IsRef = false;
            mProc.PUSH.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                
                return;
            }
            ins.Op1Value.OpDWord = mProc.regs.ECX;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = mProc.regs.EDX;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = mProc.regs.EBX;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = lOrigESP;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = mProc.regs.EBP;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = mProc.regs.ESI;
            mProc.PUSH.Impl(ref ins);
            ins.Op1Value.OpDWord = mProc.regs.EDI;
            mProc.PUSH.Impl(ref ins);

        }
    }
    public class PUSHF : Instruct
    {
        public PUSHF()
        {
            mName = "PUSHF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Push FLAGS Register onto the Stack";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sInstruction ins = CurrentDecode;

            //Push will decide whether to push the whole 32 bit register or not
            if (CurrentDecode.lOpSize16)
            {
                ins.Op1Add = 0;
                ins.Op1Value.OpQWord = mProc.regs.FLAGS;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
            }
            else
            {
                ins.Op1Value.OpQWord = mProc.regs.EFLAGS & 0xFCFFFF;
                ins.Operand1IsRef = false;
                mProc.PUSH.Impl(ref ins);
                if (ins.ExceptionThrown)
                {
                    
                    return;
                }
            }
            
            #region Instructions
            #endregion
        }
    }
    public class RCL : Instruct
    {
        public RCL()
        {
            mName = "RCL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Rotate Left with Carry";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            byte lTopBitNum = 0;
            QWord lTempCF = 0;

            //09/03/2010: Setting max count values by Op2Val type per Intel (got values from BOCHS)
            //if (Op2TypeCode == TypeCode.Byte)
            //    Op2Value.OpByte &= 0x7;
            //else if (Op2TypeCode == TypeCode.UInt16)
            //    Op2Value.OpWord &= 0x0f;
            //else if (Op2TypeCode == TypeCode.UInt32)
            //    Op2Value.OpDWord &= 0x1f;
            //else
            //    Op2Value.OpQWord &= 0x3f;

            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (byte)(lOp1Value.OpByte & 0x80);
                        lOp1Value.OpByte = (byte)((lOp1Value.OpByte << 1) | (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x80);
                    }
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (Word)(lOp1Value.OpWord & 0x8000);
                        lOp1Value.OpWord = (Word)((lOp1Value.OpWord << 1) | (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x8000);
                    }
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (DWord)(lOp1Value.OpDWord & 0x80000000);
                        lOp1Value.OpDWord = (DWord)((lOp1Value.OpDWord << 1) + (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x80000000);
                    }
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (QWord)(lOp1Value.OpQWord & 0x8000000000000000);
                        lOp1Value.OpQWord = (QWord)((lOp1Value.OpQWord << 1) + (byte)(mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x8000000000000000);
                    }
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }


            if (CurrentDecode.Op2Value.OpByte == 1)
            {
                int lMSBDest = Misc.getBit(lOp1Value.OpQWord, lTopBitNum);
                int lXORd = lMSBDest ^ (mProc.regs.FLAGS & 0x01);
                mProc.regs.setFlagOF(lXORd == 1);
            }
            #region Instructions
            #endregion
        }
    }
    public class RCR : Instruct
    {
        public RCR()
        {
            mName = "RCR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Rotate Right with Carry";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            byte lTopBitNum = 0;
            int lTempCF = 0, lTempFLAGS0;

            //09/03/2010: Setting max count values by Op2Val type per Intel (got values from BOCHS)
            //if (Op2TypeCode == TypeCode.Byte)
            //    Op2Value.OpByte &= 0x7;
            //else if (Op2TypeCode == TypeCode.UInt16)
            //    Op2Value.OpWord &= 0x0f;
            //else if (Op2TypeCode == TypeCode.UInt32)
            //    Op2Value.OpDWord &= 0x1f;
            //else
            //    Op2Value.OpQWord &= 0x3f;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte: lTopBitNum = 7; break;
                case TypeCode.UInt16: lTopBitNum = 15; break;
                case TypeCode.UInt32: lTopBitNum = 31; break;
                case TypeCode.UInt64: lTopBitNum = 63; break;

            }
            if (CurrentDecode.Op2Value.OpByte == 1)
            {
                int lMSBDest = Misc.getBit(lOp1Value.OpQWord, lTopBitNum);
                int lXORd = lMSBDest ^ (mProc.regs.FLAGS & 0x01);
                mProc.regs.setFlagOF(lXORd == 1);
            }

            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)lOp1Value.OpByte & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 7;
                        lOp1Value.OpByte = (byte)(lTempFLAGS0 | (lOp1Value.OpByte >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)lOp1Value.OpWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 15;
                        lOp1Value.OpWord = (Word)(lTempFLAGS0 | (lOp1Value.OpWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)lOp1Value.OpDWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 31;
                        lOp1Value.OpDWord = (DWord)((byte)lTempFLAGS0 | (lOp1Value.OpDWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    for (byte cnt = 0; cnt < CurrentDecode.Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)lOp1Value.OpQWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 63;
                        lOp1Value.OpQWord = (QWord)((byte)lTempFLAGS0 | (lOp1Value.OpQWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    break;
            }

            #region Instructions
            #endregion
        }
    }
    public class REP : Instruct
    {
        public REP()
        {
            mName = "REP";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PREFIX: Repeat till CX = 0 or condition met";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.SetupInstructionLoop("");
            #region Instructions
            #endregion
        }
    }
    public class REPE : Instruct
    {
        public REPE()
        {
            mName = "REPE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PREFIX: Repeat till CX = 0 or condition met (Same as REPZ)";
            mModFlags = 0;
        }

        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.REPZ.Impl(ref CurrentDecode);

            #region Instructions
            #endregion
        }
    }
    public class REPNE : Instruct
    {
        public REPNE()
        {
            mName = "REPNE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PREFIX: Repeat till CX = 0 or condition met";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.SetupInstructionLoop("nz");
            #region Instructions
            #endregion
        }
    }
    public class REPNZ : Instruct
    {
        public REPNZ()
        {
            mName = "REPNZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PREFIX: Repeat till CX = 0 or condition met (same as REPNE)";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.REPNE.Impl(ref CurrentDecode);

            #region Instructions
            #endregion
        }
    }
    public class REPZ : Instruct
    {
        public REPZ()
        {
            mName = "REPZ";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "PREFIX: Repeat till CX = 0 or condition met";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.SetupInstructionLoop("z");
            #region Instructions
            #endregion
        }
    }
    public class RET : Instruct
    {
        public RET()
        {
            mName = "RET";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Near Return to calling procedure";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sInstruction ins = CurrentDecode;

            mProc.POP.mNoSetMemoryToValue = false;
            if (CurrentDecode.lOpSize16)
            {
                ins.Op1Add = Processor_80x86.RIP;
                mProc.POP.Impl(ref ins);
            }
            else
            {
                ins.Op1Add = Processor_80x86.REIP;
                mProc.POP.Impl(ref ins);
            }
            if (ins.ExceptionThrown)
            {
                
                return;
            }
            ins.Op1Add = 0;

            //TODO: Need to update this for StackSize=32
            mProc.regs.SP += (UInt16)(CurrentDecode.Op1Value.OpWord & 0xFFFF);

            #region Instructions
            #endregion
        }
    }
    public class RETF : Instruct
    {
        public RETF()
        {
            mName = "RETF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Far return to calling procedure";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lTempEIP = mProc.regs.EIP;
            sInstruction ins = CurrentDecode;

            mProc.POP.mNoSetMemoryToValue = false;
            ins.Op1Add = Processor_80x86.REIP;
            mProc.POP.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                
                return;
            }
            ins.Op1Add = Processor_80x86.RCS;
            mProc.POP.Impl(ref ins);
            ins.Op1Add = 0x0;
            //TODO: Need to update for StackSize=32
            mProc.regs.SP += (UInt16)(CurrentDecode.Op1Value.OpWord & 0xFFFF);
            #region Instructions
            #endregion
        }
    }
    public class RDTSC : Instruct
    {
        public RDTSC()
        {
            mName = "RDTSC";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = false;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Read TimeStamp Counter";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.EAX = (UInt32)mProc.mInsructionsExecuted * 25;
            mProc.regs.EDX = (UInt32)(((mProc.mInsructionsExecuted * 25) & 0xFFFFFFFF00000000) >> 32);
            #region Instructions
            #endregion
        }
    }
    public class ROL : Instruct
    {
        public ROL()
        {
            mName = "ROL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Rotate Left";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            byte lTopBitNum = 0;

            //09/03/2010: Setting max count values by Op2Val type per Intel (got values from BOCHS)
            //if (Op2TypeCode == TypeCode.Byte)
            //    Op2Value.OpByte = (byte)(Op2Value.OpByte % 8);
            //else if (Op2TypeCode == TypeCode.UInt16)
            //    Op2Value.OpWord %= 16;
            //else if (Op2TypeCode == TypeCode.UInt32)
            //    Op2Value.OpDWord %= 32;
            //else
            //    Op2Value.OpQWord %= 32;
            //Do the operation
            //According to Intel's instructions OpVal2 can only be a byte
            lOp1Value = Misc.RotateLeft(CurrentDecode.Op1Value, CurrentDecode.Op2Value.OpByte, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }
            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 0x01);

            if (CurrentDecode.Op2Value.OpByte == 1)
            {
                int lMSBDest = Misc.getBit(lOp1Value.OpQWord, lTopBitNum);
                int lXORd = lMSBDest ^ (mProc.regs.FLAGS & 0x01);
                mProc.regs.setFlagOF(lXORd == 1);
            }

            #region Instructions
            #endregion
        }
    }
    public class ROR : Instruct
    {
        public ROR()
        {
            mName = "ROR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Rotate Right";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            byte lTopBitNum = 0;

            //09/03/2010: Setting max count values by Op2Val type per Intel (got values from BOCHS)
            //if (Op2TypeCode == TypeCode.Byte)
            //    Op2Value.OpByte = (byte)((Op2Value.OpByte & 0x1f) % 9);
            //else if (Op2TypeCode == TypeCode.UInt16)
            //    Op2Value.OpByte = (byte)((Op2Value.OpByte & 0x1f) % 17);
            //else if (Op2TypeCode == TypeCode.UInt32)
            //    Op2Value.OpByte = (byte)((Op2Value.OpByte & 0x1f));
            //else
            //    Op2Value.OpQWord &= 0x3f;

            //Do the operation
            //According to Intel's instructions OpVal2 can only be a byte
            lOp1Value = Misc.RotateRight(CurrentDecode.Op1Value, CurrentDecode.Op2Value.OpByte, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }
            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpQWord, lTopBitNum) == 1);
            if (CurrentDecode.Op2Value.OpByte == 1)
            {
                int lMSBDest = Misc.getBit(lOp1Value.OpQWord, lTopBitNum);
                int lXORd = lMSBDest ^ (mProc.regs.FLAGS & 0x01);
                mProc.regs.setFlagOF(lXORd == 1);
            }

            #region Instructions
            #endregion
        }
    }
    public class SAHF : Instruct
    {
        public SAHF()
        {
            mName = "SAHF";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store AH into FLAGS";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //byte sTempFlags = Misc.getBit(mProc.regs.AH, 7);
            //sTempFlags <<= 1;
            //sTempFlags += Misc.getBit(mProc.regs.AH, 6);
            //sTempFlags <<= 1;
            //sTempFlags <<= 1;
            //sTempFlags += Misc.getBit(mProc.regs.AH, 4);
            //sTempFlags <<= 1;
            //sTempFlags <<= 1;
            //sTempFlags += Misc.getBit(mProc.regs.AH, 2);
            //sTempFlags <<= 1;
            //sTempFlags += 1;
            //sTempFlags <<= 1;
            //sTempFlags += Misc.getBit(mProc.regs.AH, 0);

            //08/27/10 - changed from ^= to=  **** OMG THIS FIXED IT, A> prompt!!! ****
            mProc.regs.FLAGS = (Word)((mProc.regs.FLAGS & 0xFF00) + mProc.regs.AH);
            #region Instructions
            /*
              Operation
            EFLAGS(SF:ZF:0:AF:0:PF:1:CF) ? AH;
            Flags Affected
            The SF, ZF, AF, PF, and CF flags are loaded with values from the AH register. Bits 1, 3, and 5
            of the EFLAGS register are unaffected, with the values remaining 1, 0, and 0, respectively.
            Exceptions (All Operating Modes)
            None.
            */
            #endregion
        }
    }
    //TODO: Instructions of A type (Ascii) need to be checked for signage ... I think the operands are supposed to be treated as signed
    public class SAL : Instruct
    {
        public SAL()
        {
            mName = "SAL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Arithmetic Left";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.SHL.Impl(ref CurrentDecode);
            #region Instructions
            #endregion
        }
    }
    //TODO: Instructions of A type (Ascii) need to be checked for signage ... I think the operands are supposed to be treated as signed
    public class SALC : Instruct
    {
        public SALC()
        {
            mName = "SALC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Set AL to FF if Carry flag is set";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if ((mProc.regs.FLAGS & 0x01) == 0x01)
                mProc.regs.AL = 0xff;
            else
                mProc.regs.AL = 0x00;
            #region Instructions
            #endregion
        }
    }
    public class SAR : Instruct
    {
        public SAR()
        {
            mName = "SAR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Arithmetic Right";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            int lTempCount = CurrentDecode.Op2Value.OpByte;
            sOpVal lOp1Value = CurrentDecode.Op1Value;
            byte lTempB;
            Word lTempW;
            DWord lTempD;
            QWord lTempQ;

            lTempB = (byte)(lOp1Value.OpByte & 0x80);
            lTempW = (Word)(lOp1Value.OpWord & 0x8000);
            lTempD = (DWord)(lOp1Value.OpDWord & 0x80000000);
            lTempQ = (QWord)(lOp1Value.OpQWord & 0x8000000000000000);

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            if (CurrentDecode.Op1Value.OpDWord >= 0x80000000)
                lTempCount = lTempCount + 1 - 1;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpByte /= 2;
                        lOp1Value.OpByte |= lTempB;
                    }
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpWord /= 2;
                        lOp1Value.OpWord |= lTempW;
                    }
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpDWord /= 2;
                        lOp1Value.OpDWord |= lTempD;
                    }
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpQWord /= 2;
                        lOp1Value.OpQWord |= lTempQ;
                    }
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            if (lTempCount == 1)
                mProc.regs.setFlagOF(false);
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);

            #region Instructions
            #endregion
        }

    }
    public class SBB : Instruct
    {
        public SBB()
        {
            mName = "SBB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Subtract with borrow";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.CF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = CurrentDecode.Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValSigned = CurrentDecode.Op1Value;

            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte = (byte)(lOp1ValSigned.OpByte - (lOp2Value.OpByte + (mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpByte);
                    mProc.regs.setFlagCF_SBB(lPreVal1.OpByte, lOp1ValSigned.OpByte, lOp2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord = (Word)((Int16)lOp1ValSigned.OpWord - ((Int16)lOp2Value.OpWord + (Int16)(mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpWord);
                    mProc.regs.setFlagCF_SBB(lPreVal1.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord = (DWord)((Int32)lOp1ValSigned.OpDWord - ((Int32)lOp2Value.OpDWord + (Int32)(mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagCF_SBB(lPreVal1.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord = (QWord)((Int64)lOp1ValSigned.OpQWord - ((Int64)lOp2Value.OpQWord + (Int64)(mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagCF_SBB(lPreVal1.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
            }

            //Set the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lOp2Value, lOp1ValSigned, CurrentDecode.Op1TypeCode);

        }
    }
    public class SCASB : Instruct
    {
        sOpVal lPreVal1;
        sOpVal lAL;
        sOpVal lSource;
        public SCASB()
        {
            mName = "SCASB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare AL with byte at ES:(E)DI and set status flags";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            lSource.OpQWord = 0;

            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            lPreVal1.OpByte = mProc.regs.AL;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            if (mProc.mCurrInstructAddrSize16)
                lSource.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI));
            else
                lSource.OpByte = PhysicalMem.GetByte(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI));
            if (CurrentDecode.ExceptionThrown)
            { return; }
            lAL.OpQWord = lPreVal1.OpQWord;
            lAL.OpByte -= lSource.OpByte;


            //notice that we don't save the result, we just use it to save the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lSource, lAL, TypeCode.Byte);
            mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpByte, lAL.OpByte, lSource.OpByte);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI--;
                else
                    mProc.regs.EDI--;
            }
            else
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI++;
                else
                    mProc.regs.EDI++;
            }
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.CX--;
                else
                    mProc.regs.ECX--;

        }
    }
    public class SCASD : Instruct
    {
        DWord lSource = 0;
        sOpVal lPreVal1;
        DWord lLoopCounter = 0;
        public SCASD()
        {
            mName = "SCASD";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = false;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare EAX with doubleword at ES:(E)DI and set status flags";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
 
            lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            lPreVal1.OpQWord = 0;
            lPreVal1.OpDWord = mProc.regs.EAX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            if (mProc.mCurrInstructAddrSize16)
                lSource = mProc.mem.GetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI));
            else
                lSource = mProc.mem.GetDWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI));
            if (CurrentDecode.ExceptionThrown)
            { return; }
            sOpVal lAL = lPreVal1;
            lAL.OpDWord -= lSource;


            //notice that we don't save the result, we just use it to save the flags
            mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpDWord, lAL.OpDWord, lSource);
            mProc.regs.setFlagOF_Sub(lPreVal1.OpDWord, lSource, lAL.OpDWord);
            mProc.regs.setFlagSF(lAL.OpDWord);
            mProc.regs.setFlagZF(lAL.OpDWord);
            mProc.regs.setFlagAF(lPreVal1.OpDWord, lAL.OpDWord);
            mProc.regs.setFlagPF(lAL.OpDWord);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI -= 4;
                else
                    mProc.regs.EDI -= 4;
            }
            else
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI += 4;
                else
                    mProc.regs.EDI += 4;
            }
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.CX--;
                else
                    mProc.regs.ECX--;

            #region Instructions
            #endregion
        }
    }
    public class SCASW : Instruct
    {
        DWord lLoopCounter;
        sOpVal lPreVal1;
        sOpVal lSource;
        public SCASW()
        {
            mName = "SCASW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Compare AL with byte at ES:DI and set status flags";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {

            lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;
            lSource.OpQWord = 0;
            lPreVal1.OpQWord = 0;
            lPreVal1.OpWord = mProc.regs.AX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            if (!mProc.mCurrInstructOpSize16)
            {
                mProc.SCASD.UsageCount++;
                this.UsageCount--;
                mProc.SCASD.Impl(ref CurrentDecode);
    
                return;
            }

            if (mProc.mCurrInstructAddrSize16)
                lSource.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.DI));
            else
                lSource.OpWord = mProc.mem.GetWord(mProc, ref CurrentDecode, PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, mProc.regs.EDI));
            if (CurrentDecode.ExceptionThrown)
            { return; }
            sOpVal lAL = lPreVal1;
            lAL.OpWord -= lSource.OpWord;


            //notice that we don't save the result, we just use it to save the flags
            //            SetFlagsForSubtraction(mProc, lPreVal1, lTemp, lAL, TypeCode.UInt16, true, true);
            mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpWord, lAL.OpWord, lSource.OpWord);
            mProc.regs.setFlagOF_Sub(lPreVal1.OpWord, lSource.OpWord, lAL.OpWord);
            mProc.regs.setFlagSF(lAL.OpWord);
            mProc.regs.setFlagZF(lAL.OpWord);
            mProc.regs.setFlagAF(lPreVal1.OpWord, lAL.OpWord);
            mProc.regs.setFlagPF(lAL.OpWord);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI -= 2;
                else
                    mProc.regs.EDI -= 2;
            }
            else
            {
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.DI += 2;
                else
                    mProc.regs.EDI += 2;
            }
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)
                if (mProc.mCurrInstructAddrSize16)
                    mProc.regs.CX--;
                else
                    mProc.regs.ECX--;

            #region Instructions
            #endregion
        }
    }
    public class SETcc : Instruct
    {
        public SETcc()
        {
            mName = "SETcc";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Set Operand to 0/1 based on various conditions";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            byte lOp1Value = 0;

            switch (CurrentDecode.RealOpCode)
            {
                case 0x0F90:   //SETO - Overflow
                    mName = "SETO";
                    if (mProc.regs.FLAGSB.OF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F91:    //SETNO - Not Overflow
                    mName = "SETNO";
                    if (mProc.regs.FLAGSB.OF == false)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F92:    //SETB - Below
                    mName = "SETB";
                    if (mProc.regs.FLAGSB.CF == true)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F93:    //SETAE - Above or Equal
                    mName = "SETAE";
                    if (mProc.regs.FLAGSB.CF == false)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F94:    //SETE - Equal
                    mName = "SETE";
                    if (mProc.regs.FLAGSB.ZF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F95:    //SETNE - Not Equal
                    mName = "SETNE";
                    if (mProc.regs.FLAGSB.ZF == false)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F96:    //SETBE - Below or Equal
                    mName = "SETBE";
                    if (mProc.regs.FLAGSB.CF == true || mProc.regs.FLAGSB.ZF == true)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F97:    //SETA - Above
                    mName = "SETA";
                    if (mProc.regs.FLAGSB.CF == false && mProc.regs.FLAGSB.ZF == false)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F98:   //SETS - Sign
                    mName = "SETS";
                    if (mProc.regs.FLAGSB.SF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F99:   //SETNS - Not Sign
                    mName = "SETNS";
                    if (!mProc.regs.FLAGSB.SF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9A:   //SETP - Parity
                    mName = "SETP";
                    if (mProc.regs.FLAGSB.PF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9B:   //SETNP/SETPO - Not Parity
                    mName = "SETNP";
                    if (!mProc.regs.FLAGSB.PF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9c:    //SETL - Less than
                    mName = "SETL";
                    if (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9d:    //SETGE - Greater or Equal
                    mName = "SETGE";
                    if (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9e:    //SETLE Less than or Equal
                    mName = "SETLE";
                    if (mProc.regs.FLAGSB.ZF == true || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                case 0x0F9f:    //SETG - Greater
                    mName = "SETG";
                    if (mProc.regs.FLAGSB.ZF == false && (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF))
                        lOp1Value = 1;
                    else
                        lOp1Value = 0;
                    break;
                default:
                    throw new Exception("You missed one!");
            }

            mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value);
            if (CurrentDecode.ExceptionThrown)
            { return; }

        }
    }
    public class SHL : Instruct
    {
        public SHL()
        {
            mName = "SHL";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Left";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            int lTempCount = CurrentDecode.Op2Value.OpByte, lTopBitNum = 0;
            sOpVal lOp1Value = CurrentDecode.Op1Value;

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lTopBitNum = 7;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpByte, lTopBitNum) == 1);
                        lOp1Value.OpByte *= 2;
                    }
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lTopBitNum = 15;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpWord, lTopBitNum) == 1);
                        lOp1Value.OpWord *= 2;
                    }
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lTopBitNum = 31;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpDWord, lTopBitNum) == 1);
                        lOp1Value.OpDWord *= 2;
                    }
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lTopBitNum = 63;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpQWord, lTopBitNum) == 1);
                        lOp1Value.OpQWord *= 2;
                    }
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            if (lTempCount == 1)
            {
                bool lTempOF = ((Misc.getBit(lOp1Value.OpQWord, lTopBitNum)) == 1) ^ mProc.regs.FLAGSB.CF;
                mProc.regs.setFlagOF(lTempOF);
            }
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);

            #region Instructions
            #endregion
        }
    }
    public class SHR : Instruct
    {
        public SHR()
        {
            mName = "SHR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Right";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            int lTempCount = CurrentDecode.Op2Value.OpByte, lTopBitNum = 0;
            sOpVal lOp1Value = CurrentDecode.Op1Value;

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lTopBitNum = 7;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpByte /= 2;
                    }
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lTopBitNum = 15;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpWord /= 2;
                    }
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lTopBitNum = 31;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpDWord /= 2;
                    }
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lTopBitNum = 63;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpQWord /= 2;
                    }
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            if (lTempCount == 1)
            {
                bool lTempOF = ((Misc.getBit(lOp1Value.OpQWord, lTopBitNum)) == 1);
                mProc.regs.setFlagOF(lTempOF);
            }
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);

            #region Instructions
            #endregion
        }
    }
    public class SHRD : Instruct
    {
        public SHRD()
        {
            mName = "SHRD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Right Double Precision";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            int Count = CurrentDecode.Op3Value.OpByte % 0x20;
            int Size = 0; // Op1Val.TopBitNum + 1;
            sOpVal Dest = CurrentDecode.Op1Value;
            sOpVal Src = CurrentDecode.Op2Value;

            if (Count == 0)
                return;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte: Size = 8; break;
                case TypeCode.UInt16: Size = 16; break;
                case TypeCode.UInt32: Size = 32; break;
                case TypeCode.UInt64: Size = 64; break;
            }

            if (Count > Size)
                return;

            mProc.regs.setFlagCF(Misc.getBit(Dest.OpQWord, Count - 1) == 1);

            for (int i = 0; i <= Size - 1 - Count; i++)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Dest.OpQWord, i + Count) == 1);

            for (int i = Size - Count; i <= Size - 1; i++)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Src.OpQWord, i + Count - Size) == 1);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpByte);
                    mProc.regs.setFlagSF(Dest.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpWord);
                    mProc.regs.setFlagSF(Dest.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpDWord);
                    mProc.regs.setFlagSF(Dest.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpQWord);
                    mProc.regs.setFlagSF(Dest.OpQWord);
                    break;
            }
            mProc.regs.setFlagPF(Dest.OpQWord);
            mProc.regs.setFlagZF(Dest.OpQWord);

            #region Instructions
            #endregion
        }
    }
    public class SHLD : Instruct
    {
        public SHLD()
        {
            mName = "SHLD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Shift Left Double Precision";
            mModFlags = eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF | eFLAGS.OF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            int Count = CurrentDecode.Op3Value.OpByte % 0x20;
            int Size = 0; // Op1Val.TopBitNum + 1;
            sOpVal Dest = CurrentDecode.Op1Value;
            sOpVal Src = CurrentDecode.Op2Value;

            if (Count == 0)
                return;

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte: Size = 8; break;
                case TypeCode.UInt16: Size = 16; break;
                case TypeCode.UInt32: Size = 32; break;
                case TypeCode.UInt64: Size = 64; break;
            }

            if (Count > Size)
                return;

            mProc.regs.setFlagCF(Misc.getBit(Dest.OpQWord, Size - Count) == 1);

            for (int i = Size - 1; i >= Count; i--)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Dest.OpQWord, i - Count) == 1);

            for (int i = Count; i >= 0; i--)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Src.OpQWord, i - Count + Size) == 1);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpByte);
                    mProc.regs.setFlagSF(Dest.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpWord);
                    mProc.regs.setFlagSF(Dest.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpDWord);
                    mProc.regs.setFlagSF(Dest.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, Dest.OpQWord);
                    mProc.regs.setFlagSF(Dest.OpQWord);
                    break;
            }
            mProc.regs.setFlagPF(Dest.OpQWord);
            mProc.regs.setFlagZF(Dest.OpQWord);

            #region Instructions
            #endregion
        }
    }
    public class SIDT : Instruct
    {
        public SIDT()
        {
            mName = "SIDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store Interrupt Descriptor Table Register value";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lLimit;
            UInt32 lBase;

            lLimit = (UInt16)mProc.regs.IDTR.Limit;
            lBase = mProc.regs.IDTR.Base;

            if (CurrentDecode.lOpSize16)
                lBase &= 0x00FFFFFF;

            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lLimit);
            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add + 2, lBase);

            #region Instructions
            #endregion
        }
    }
    public class SGDT : Instruct
    {
        public SGDT()
        {
            mName = "SGDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store Global Descriptor Table Register value";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lLimit;
            UInt32 lBase;

            lLimit = (UInt16)mProc.regs.GDTR.Limit;
            lBase = mProc.regs.GDTR.Base;

            if (CurrentDecode.lOpSize16)
                lBase &= 0x00FFFFFF;

            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lLimit);
            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add + 2, lBase);

            #region Instructions
            #endregion
        }
    }
    public class SLDT : Instruct
    {
        public SLDT()
        {
            mName = "SLDT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store Local Descriptor Table Register value";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lLimit;
            UInt32 lBase;

            lLimit = (UInt16)mProc.regs.LDTR.Limit;
            lBase = mProc.regs.LDTR.Base;

            if (CurrentDecode.lOpSize16)
                lBase &= 0x00FFFFFF;

            mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lLimit);
            mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add + 4, lBase);

            #region Instructions
            #endregion
        }
    }
    public class SMSW : Instruct
    {
        public SMSW()
        {
            mName = "SMSW";
            mProc8086 = false;
            mProc8088 = false;
            mProc80186 = false;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store Machine Status Word";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1.Register != eGeneralRegister.NONE)
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (mProc.regs.CR0 & 0xFFFF));
            else
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)mProc.regs.CR0);
            #region Instructions
            #endregion
        }
    }
    public class STC : Instruct
    {
        public STC()
        {
            mName = "STC";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "SetC Carry Flag";
            mModFlags = eFLAGS.CF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.setFlagCF(true);
            #region Instructions
            /*
              Operation
                CF ? 1;
                Flags Affected
                The CF flag is set. The OF, ZF, SF, AF, and PF flags are unaffected.
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class STD : Instruct
    {
        public STD()
        {
            mName = "STD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "SetC Direction Flag";
            mModFlags = eFLAGS.DF;
        }

        public override void Impl(ref sInstruction CurrentDecode)
        {
            mProc.regs.setFlagDF(true);

            #region Instructions
            /*
              Operation
                CF ? 1;
                Flags Affected
                The CF flag is set. The OF, ZF, SF, AF, and PF flags are unaffected.
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class STI : Instruct
    {
        public STI()
        {
            mName = "STI";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "SetC Interrupt Flag";
            mModFlags = eFLAGS.IF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //STI won't be processed until the end of the NEXT instruction
            mProc.mSTICalled = true;
            //mProc.regs.setFlagIF(true);

            #region Instructions
            /*
              Operation
                CF ? 1;
                Flags Affected
                The CF flag is set. The OF, ZF, SF, AF, and PF flags are unaffected.
                Exceptions (All Operating Modes)
                None.
                */
            #endregion
        }
    }
    public class STR : Instruct
    {
        public STR()
        {
            mName = "STR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Store Task Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            if (CurrentDecode.Op1.Register != eGeneralRegister.NONE)
                mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (DWord)(mProc.regs.TR.SegSel & 0xFFFF));
            else
                mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, (Word)mProc.regs.TR.SegSel);

            #region Instructions
            #endregion
        }
    }
    public class STOSB : Instruct
    {
        public STOSB()
        {
            mName = "STOSB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Store String - Bytes";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            DWord lDest = 0;
            DWord lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, false);
            do
            {

                mProc.mem.SetByte(mProc, ref CurrentDecode, lDest, mProc.regs.AL);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 1, false, true, false, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class STOSW : Instruct
    {
        public STOSW()
        {
            mName = "STOSW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Store String - Word";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lDest = 0;
            UInt32 lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }

            if (!mProc.mCurrInstructOpSize16)
            {
                mProc.STOSD.UsageCount++;
                this.UsageCount--;
                mProc.STOSD.Impl(ref CurrentDecode);
    
                return;
            }

            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, false);

            do
            {

                mProc.mem.SetWord(mProc, ref CurrentDecode, lDest, mProc.regs.AX);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 2, false, true, false, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class STOSD : Instruct
    {
        public STOSD()
        {
            mName = "STOSD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            REPAble = true;
            mDescription = "Store String - DoubleWord";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 lDest = 0, lJunk = 0;


            DWord lLoopCounter = mProc.mCurrInstructAddrSize16 ? mProc.regs.CX : mProc.regs.ECX;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && ((lLoopCounter == 0)))
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            //MOVSW.SetupMOVSSrc(mProc, ref lSource, mProc.mCurrInstructAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, mProc.mCurrInstructAddrSize16, false, true);

            //MOVSW.SetupMOVSSrcDest(mProc, ref lJunk, ref lDest, mProc.mCurrInstructAddrSize16);
            do
            {

                mProc.mem.SetDWord(mProc, ref CurrentDecode, lDest, mProc.regs.EAX);
                if (CurrentDecode.ExceptionThrown)
                { return; }
                MOVSW.IncDec(mProc, ref lDest, ref lJunk, mProc.mCurrInstructAddrSize16, 4, false, true, false, false);
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                    return;
            }
            while (--lLoopCounter > 0 && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT);

            if (mProc.mCurrInstructAddrSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
        }
    }
    public class SUB : Instruct
    {
        public SUB()
        {
            mName = "SUB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Subtract";
            mModFlags = eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = CurrentDecode.Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = CurrentDecode.Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValSigned = CurrentDecode.Op1Value;

            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);

            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte = (byte)((sbyte)lOp1ValSigned.OpByte - (sbyte)lOp2Value.OpByte);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpByte);
                    mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpByte, lOp1ValSigned.OpByte, lOp2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord = (Word)((Int16)lOp1ValSigned.OpWord - (Int16)lOp2Value.OpWord);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpWord);
                    mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpWord, lOp1ValSigned.OpWord, lOp2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord = (DWord)((Int32)lOp1ValSigned.OpDWord - (Int32)lOp2Value.OpDWord);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpDWord);
                    mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpDWord, lOp1ValSigned.OpDWord, lOp2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord = (QWord)((Int64)lOp1ValSigned.OpQWord - (Int64)lOp2Value.OpQWord);
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1ValSigned.OpQWord);
                    mProc.regs.setFlagCF_SUB_CMP(lPreVal1.OpQWord, lOp1ValSigned.OpQWord, lOp2Value.OpQWord);
                    break;
            }

            //SetC the flags
            //    //SetC the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lOp2Value, lOp1ValSigned, CurrentDecode.Op1TypeCode);
            //            SetFlagsForAddition(mProc, lPreVal1, Op2Val, lOp1ValSigned, lOp1ValUnsigned);

        }
    }
    public class TEST : Instruct
    {
        public TEST()
        {
            mName = "TEST";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Test For Bit Pattern";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte &= CurrentDecode.Op2Value.OpByte;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    mProc.regs.setFlagPF(lOp1Value.OpByte);
                    mProc.regs.setFlagZF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord &= CurrentDecode.Op2Value.OpWord;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    mProc.regs.setFlagPF(lOp1Value.OpWord);
                    mProc.regs.setFlagZF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord &= CurrentDecode.Op2Value.OpDWord;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    mProc.regs.setFlagPF(lOp1Value.OpDWord);
                    mProc.regs.setFlagZF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord &= CurrentDecode.Op2Value.OpQWord;
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    mProc.regs.setFlagPF(lOp1Value.OpQWord);
                    mProc.regs.setFlagZF(lOp1Value.OpQWord);
                    break;
            }

            //Set the flags
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            //AF flag undefined
            #region Instructions
            #endregion
        }
    }
    public class VERR : Instruct
    {
        public VERR()
        {
            mName = "VERR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Verify a segment for reading";
            mModFlags = eFLAGS.ZF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //TODO: Fix VERR hack
            mProc.regs.setFlagZF(true);
            #region Instructions
            /*
            */
            #endregion
        }
    }
    public class VERW : Instruct
    {
        public VERW()
        {
            mName = "VERW";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Verify a segment for writing";
            mModFlags = eFLAGS.ZF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //TODO: Fix VERW hack
            mProc.regs.setFlagZF(true);
            #region Instructions
            /*
            */
            #endregion
        }
    }
    public class XADD : Instruct
    {
        public XADD()
        {
            mName = "XADD";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Exchange and add";
            mModFlags = eFLAGS.ZF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            sInstruction ins = CurrentDecode;

            ins.Op1TypeCode = CurrentDecode.Op2TypeCode;
            ins.Op2TypeCode = CurrentDecode.Op1TypeCode;
            ins.Operand1IsRef = CurrentDecode.Operand2IsRef;
            ins.Operand2IsRef = CurrentDecode.Operand1IsRef;
            ins.Op1Value = CurrentDecode.Op2Value;
            ins.Op1Add = CurrentDecode.Op2Add;
            ins.Op2Value = CurrentDecode.Op1Value;
            ins.Op2Add = CurrentDecode.Op1Add;
            mProc.ADD.Impl(ref ins);
            CurrentDecode = ins;
        }
    }
    public class XCHG : Instruct
    {
        public XCHG()
        {
            mName = "XCHG";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Exchange Register/Memory with Register";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op2Add, CurrentDecode.Op1Value.OpByte);
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add, CurrentDecode.Op1Value.OpWord);
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add, CurrentDecode.Op1Value.OpDWord);
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op2Add, CurrentDecode.Op1Value.OpQWord);
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, CurrentDecode.Op2Value.OpQWord);
                    break;
            }


            #region Instructions
            #endregion
        }
    }
    public class XLAT : Instruct
    {
        public XLAT()
        {
            mName = "XLAT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Table Look-up Translation using Parameter as Index";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt16 lTemp = 0;
            if (mProc.mSegmentOverride == 0)
                mProc.mSegmentOverride = Processor_80x86.RDS;
            if (mProc.mCurrInstructAddrSize16)
                lTemp = (UInt16)(mProc.regs.BX + mProc.regs.AL);
            else
                lTemp = (UInt16)(mProc.regs.EBX + mProc.regs.AL);

            UInt32 loc = GetSegOverriddenAddress(mProc, lTemp);
            mProc.regs.AL = PhysicalMem.GetByte(mProc, ref CurrentDecode, loc);

            #region Instructions
            #endregion
        }
    }
    public class XLATB : Instruct
    {
        public XLATB()
        {
            mName = "XLATB";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Table Look-up Translation using AL as Index";
            mModFlags = 0;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            UInt32 TableBase = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.DS, mProc.regs.BX);
            mProc.regs.AL = PhysicalMem.GetByte(mProc, ref CurrentDecode, TableBase + mProc.regs.AL);

            throw new Exception("Partially implemented!");
            #region Instructions
            #endregion
        }
    }
    public class XOR : Instruct
    {
        public XOR()
        {
            mName = "XOR";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Test For Bit Pattern";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl(ref sInstruction CurrentDecode)
        {
            //capture value pre-operation
            sOpVal lPreVal1 = CurrentDecode.Op1Value, lOp1Value = CurrentDecode.Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = CurrentDecode.Op2Value;
            if (!CurrentDecode.Operand2IsRef && (CurrentDecode.Op2TypeCode != CurrentDecode.Op1TypeCode))
                Misc.SignExtend(ref lOp2Value, ref CurrentDecode.Op2TypeCode, CurrentDecode.Op1TypeCode);
            //Do the operation
            switch (CurrentDecode.Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte ^= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord ^= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord ^= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord ^= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc, ref CurrentDecode, CurrentDecode.Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            //Set the flags
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);
            //AF flag undefined

            #region Instructions
            #endregion
        }
    }
    public class GRP1 : Instruct
    {
        public GRP1()
        {
            mName = "GRP1";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group1 - OpCode = 83";
            mModFlags = 0;
        }
    }
    public class GRP2 : Instruct
    {
        public GRP2()
        {
            mName = "GRP2";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group2";
            mModFlags = 0;
        }
    }
    public class GRP3a : Instruct
    {
        public GRP3a()
        {
            mName = "GRP3a";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group3a";
            mModFlags = 0;
        }
    }
    public class GRP3b : Instruct
    {
        public GRP3b()
        {
            mName = "GRP3b";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group3b";
            mModFlags = 0;
        }
    }
    public class GRP4 : Instruct
    {
        public GRP4()
        {
            mName = "GRP4";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group4";
            mModFlags = 0;
        }
    }
    public class GRP5 : Instruct
    {
        public GRP5()
        {
            mName = "GRP5";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group5";
            mModFlags = 0;
        }
    }
    public class GRP60 : Instruct
    {
        public GRP60()
        {
            mName = "GRP60";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "OpCode 60 group";
            mModFlags = 0;
        }
    }
    public class GRP61 : Instruct
    {
        public GRP61()
        {
            mName = "GRP61";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "OpCode 61 group";
            mModFlags = 0;
        }
    }
    public class GRP7 : Instruct
    {
        public GRP7()
        {
            mName = "GRP7";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group7";
            mModFlags = 0;
        }
    }
    public class GRP8 : Instruct
    {
        public GRP8()
        {
            mName = "GRP8";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group8";
            mModFlags = 0;
        }
    }
    public class GRPC0 : Instruct
    {
        public GRPC0()
        {
            mName = "GRPC0";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group for OpCode C0";
            mModFlags = 0;
        }
    }
    public class GRPC1 : Instruct
    {
        public GRPC1()
        {
            mName = "GRPC1";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group for OpCode C1";
            mModFlags = 0;
        }
    }
    public class GRPPUSH : Instruct
    {
        public GRPPUSH()
        {
            mName = "GRPPUSH";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Group for OpCode 60";
            mModFlags = 0;
        }
    }
}
