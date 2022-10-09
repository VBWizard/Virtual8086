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
        public int UseNewDecoder;
        
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
        public override void Impl()
        {
            if ((mProc.regs.AL & 0xF) > 9 || mProc.regs.FLAGSB.AF)
            {
                mProc.regs.AL += 6;
                mProc.regs.AH += 1;
                mProc.regs.setFlagAF(true);
                mProc.regs.setFlagCF(true);
                //mProc.regs.FLAGS = Misc.setBit(mProc.regs.FLAGS, (int)eFLAGS.AF, true);
                //mProc.regs.FLAGS = Misc.setBit(mProc.regs.FLAGS, (int)eFLAGS.CF, true);
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
                AL ← AL + 6;
                AH ← AH + 1;
                AF ← 1;
                CF ← 1;
                ELSE
                AF ← 0;
                CF ← 0;
                FI;
                AL ← AL AND 0FH;
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
        public override void Impl()
        {
            mProc.regs.AL = (byte)((Op1Value.OpByte * mProc.regs.AH) & 0xff);
            mProc.regs.AH = 0;
            mProc.regs.setFlagSF(mProc.regs.AL);
            mProc.regs.setFlagZF(mProc.regs.AL);
            mProc.regs.setFlagPF(mProc.regs.AL);
            #region Instructions
            /*  Operation
                tempAL ← AL;
                tempAH ← AH;
                AL ← (tempAL + (tempAH ∗ imm8)) AND FFH; (* imm8 is set to 0AH for the AAD mnemonic *)
                AH ← 0
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
        public override void Impl()
        {
            mProc.regs.AH = (byte)(mProc.regs.AL / Op1Value.OpByte);
            mProc.regs.AL %= 0x0a;
            mProc.regs.setFlagSF(mProc.regs.AL);
            mProc.regs.setFlagZF(mProc.regs.AL);
            mProc.regs.setFlagPF(mProc.regs.AL);
            #region Instructions
            /*
                Operation
                tempAL ← AL;
                AH ← tempAL / imm8; (* imm8 is set to 0AH for the AAM mnemonic *)
                AL ← tempAL MOD imm8;
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
        public override void Impl()
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
                AL ← AL – 6;
                AH ← AH – 1;
                AF ← 1;
                CF ← 1;
                ELSE
                CF ← 0;
                AF ← 0;
                FI;
                AL ← AL AND 0FH;
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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValUnsigned = Op1Value,
                   lOp1ValSigned = Op1Value;
            byte lCF = (byte)(mProc.regs.FLAGSB.CF ? 1 : 0);


            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValUnsigned.OpByte += (byte)(lOp2Value.OpByte + lCF);
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpByte = (byte)((sbyte)(lOp1ValSigned.OpByte) + (sbyte)(lOp2Value.OpByte) + (byte)lCF);
                    mProc.mem.SetByte(mProc, Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValUnsigned.OpWord += (Word)(Op2Value.OpWord + lCF);
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpWord = (Word)((Int16)(lOp1ValSigned.OpWord) + (Int16)(lOp2Value.OpWord) + (Int16)lCF);
                    mProc.mem.SetWord(mProc, Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValUnsigned.OpDWord += Op2Value.OpDWord + lCF;
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpDWord = (DWord)((UInt32)(lOp1ValSigned.OpDWord) + (UInt32)(lOp2Value.OpDWord) + (UInt32)lCF);
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValUnsigned.OpQWord += Op2Value.OpQWord + lCF;
                    //CLR - 07/12/2011: Changed signed value to be truly signed
                    lOp1ValSigned.OpQWord = (QWord)((UInt64)(lOp1ValSigned.OpQWord) + (UInt64)(lOp2Value.OpQWord) + (UInt64)lCF);
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1ValSigned.OpQWord);
                    break;
            }

            //Set the flags
            SetFlagsForAddition(mProc, lPreVal1, lOp2Value, lOp1ValSigned, lOp1ValUnsigned, Op1TypeCode);

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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValUnsigned = Op1Value,
                   lOp1ValSigned = Op1Value;
            
            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValUnsigned.OpByte += Op2Value.OpByte;
                    lOp1ValSigned.OpByte = (byte)((sbyte)lOp1ValSigned.OpByte + (sbyte)lOp2Value.OpByte);
                    mProc.mem.SetByte(mProc,Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValUnsigned.OpWord += Op2Value.OpWord;
                    lOp1ValSigned.OpWord = (UInt16)((Int16)lOp1ValSigned.OpWord + (Int16)lOp2Value.OpWord);
                    mProc.mem.SetWord(mProc, Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValUnsigned.OpDWord += Op2Value.OpDWord;
                    lOp1ValSigned.OpDWord = (UInt32)((Int32)lOp1ValSigned.OpDWord + (Int32)lOp2Value.OpDWord);
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValUnsigned.OpQWord += Op2Value.OpQWord;
                    lOp1ValSigned.OpQWord = (UInt64)((Int64)lOp1ValSigned.OpQWord + (Int64)lOp2Value.OpQWord);
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1ValSigned.OpQWord);
                    break;
            }

            //    //Set the flags
            SetFlagsForAddition(mProc, lPreVal1, lOp2Value, lOp1ValSigned, lOp1ValUnsigned, Op1TypeCode);
            //            SetFlagsForAddition(mProc, lPreVal1, Op2Val, lOp1ValSigned, lOp1ValUnsigned);

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
        }        //public override void Impl()
        //{
        //    /*Always 2 parameters*/
        //    //capture value pre-operation
        //    cValue lPreVal1 = Op1Val;

        //    //08/27/2010 - changed to only sign extend if source is an immediate
        //    //08/28/2010 - Changed to use SignExtendOp
        //    cValue lOp2Val = SignExtendOp(Op2Val, Operand2IsRef, Op1Val.GetTypeCode()); ;
        //    cValue lOp1Val = Op1Val;

        //    lOp1Val += lOp2Val;

        //    mProc.mem.SetC(mProc,Op1Addr, lOp1Val);

        //    //SetC the flags
        //    mProc.regs.setFlagCF(lOp1Val, lPreVal1);
        //    mProc.regs.setFlagSF(lOp1Val);
        //    mProc.regs.setFlagOF(lPreVal1, lOp1Val);
        //    mProc.regs.setFlagZF(lOp1Val);
        //    mProc.regs.setFlagAF(lPreVal1, lOp1Val);
        //    mProc.regs.setFlagPF(lOp1Val);

        //    #region Instructions
        //    /*
        //                Operation
        //                DEST ← DEST + SRC;
        //                Flags Affected
        //                The OF, SF, ZF, AF, CF, and PF flags are set according to the result.
        //                Protected Mode Exceptions
        //                #GP(0) If the destination is located in a non-writable segment.
        //                If a memory operand effective address is outside the CS, DS, ES, FS, or
        //                GS segment limit.
        //                If the DS, ES, FS, or GS register is used to access memory and it contains
        //                a null segment selector.
        //                #SS(0) If a memory operand effective address is outside the SS segment limit.
        //                #PF(fault-code) If a page fault occurs.
        //                #AC(0) If alignment checking is enabled and an unaligned memory reference is
        //                made while the current privilege level is 3.
        //                Real-Address Mode Exceptions
        //                #GP If a memory operand effective address is outside the CS, DS, ES, FS, or
        //                GS segment limit.
        //                #SS If a memory operand effective address is outside the SS segment limit.
        //                Virtual-8086 Mode Exceptions
        //                #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
        //                GS segment limit.
        //                #SS(0) If a memory operand effective address is outside the SS segment limit.
        //                #PF(fault-code) If a page fault occurs.
        //                #AC(0) If alignment checking is enabled and an unaligned memory reference is
        //                made.
        //     */
        //    #endregion
        //}
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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal /*lOp1ValUnsigned = Op1Value,*/
                   lOp1ValSigned = Op1Value;

            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte &= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc,Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord &= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord &= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord &= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc,Op1Add, lOp1ValSigned.OpQWord);
                    break;
            }            
            //Set the flags
            mProc.regs.setFlagOF(false);
            mProc.regs.setFlagCF(false);
            #region Set SF
            switch (Op1TypeCode)
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
            mProc.regs.setFlagZF(lOp1ValSigned.OpQWord);
            mProc.regs.setFlagPF(lOp1ValSigned.OpQWord);
            //AF flag undefined
            #region Instructions
            /*
                Operation
                DEST ← DEST AND SRC;
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
        public override void Impl()
        {
            int a = 0;

                a += 1;
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
        public override void Impl()
        {
            byte lTopBitNum = 0;
            if (Op2TypeCode == TypeCode.UInt16)
                lTopBitNum = 15;
            else
                lTopBitNum = 31;

            if ((lTopBitNum == 15 && Op2Value.OpWord == 0) || (lTopBitNum == 31 && Op2Value.OpDWord == 0))
            {
                mProc.regs.setFlagZF(true);
                Op1Value.OpQWord = 0;
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, Op1Add, Op1Value.OpWord);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, Op1Value.OpDWord);
            }
            else
            {
                mProc.regs.setFlagZF(false);
                for (int c = 0; c <= lTopBitNum; c++)
                    if (Misc.GetBits3(Op2Value.OpQWord, c, 1) == 1)
                    {
                        Op1Value.OpWord = (Word)c;
                        if (lTopBitNum == 15)
                            mProc.mem.SetWord(mProc, Op1Add, Op1Value.OpWord);
                        else
                            mProc.mem.SetDWord(mProc, Op1Add, Op1Value.OpDWord);
                        return;
                    }
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, Op1Add, 0);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, 0);
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
        public override void Impl()
        {
            byte lTopBitNum = 0;
            if (Op2TypeCode == TypeCode.UInt16)
                lTopBitNum = 15;
            else
                lTopBitNum = 31;

            if ( (lTopBitNum == 15 && Op2Value.OpWord == 0) || (lTopBitNum == 31 && Op2Value.OpDWord == 0) )
            {
                mProc.regs.setFlagZF(true);
                Op1Value.OpQWord = 0;
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, Op1Add, Op1Value.OpWord);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, Op1Value.OpDWord);
            }
            else
            {
                mProc.regs.setFlagZF(false);
                for (int c = lTopBitNum; c == 0; c--)
                    if (Misc.GetBits3(Op2Value.OpQWord, c, 1) == 1)
                    {
                        Op1Value.OpWord = (Word)c;
                        if (lTopBitNum == 15)
                            mProc.mem.SetWord(mProc, Op1Add, Op1Value.OpWord);
                        else
                            mProc.mem.SetDWord(mProc, Op1Add, Op1Value.OpDWord);
                        return;
                    }
                if (lTopBitNum == 15)
                    mProc.mem.SetWord(mProc, Op1Add, 0);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, 0);
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
        public override void Impl()
        {
            UInt32 lDest = Op1Value.OpByte;

            lDest <<= 8;
            lDest += (UInt32)((Op1Value.OpDWord & 0x0000FF00) >> 8);
            lDest <<= 8;
            lDest += (UInt32)((Op1Value.OpDWord & 0x00FF0000) >> 16);
            lDest <<= 8;
            lDest += (UInt32)((Op1Value.OpDWord & 0xFF000000) >> 24);

            mProc.mem.SetDWord(mProc, Op1Add, lDest);

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
        public override void Impl()
        {
            if (Op1Add >= mProc.REGADDRBASE)
                mProc.regs.setFlagCF(Misc.GetBits3(Op1Value.OpQWord, (int)Op2Value.OpByte, 1) == 1);
            else
            {
                mProc.regs.setFlagCF(TestBit(mProc.mem.GetByte(mProc, Op1Add + (Op2Value.OpDWord / 8)), (int)(Op2Value.OpDWord % 8)));
            }
            ///AF flag undefined
            #region Instructions
            #endregion
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
        public override void Impl()
        {
             byte lTheByte, lTheBit;
            sOpVal lOp1Val = Op1Value;

            if (Op1Add >= mProc.REGADDRBASE)
            {
                lTheBit = Misc.GetBits3(Op1Value.OpDWord, (int)Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                if (lTheBit==0)
                    lOp1Val.OpDWord = Misc.setBit(Op1Value.OpDWord, (int)Op2Value.OpByte, true);
                else
                    lOp1Val.OpDWord = Misc.setBit(Op1Value.OpDWord, (int)Op2Value.OpByte, false);
                mProc.mem.SetDWord(mProc, Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = mProc.mem.GetByte(mProc, Op1Add + (Op2Value.OpDWord / 8));
                if (BT.TestBit(lTheByte, (int)(Op2Value.OpDWord % 8)))
                {
                    mProc.regs.setFlagCF(true);
                    lTheByte = Misc.setBit(lTheByte, (int)(Op2Value.OpDWord % 8), false);
                    mProc.mem.SetByte(mProc, Op1Add + (Op2Value.OpDWord / 8), lTheByte);
                }
                else
                {
                    mProc.regs.setFlagCF(false);
                    lTheByte = Misc.setBit(lTheByte, (int)(Op2Value.OpDWord % 8), true);
                    mProc.mem.SetByte(mProc, Op1Add + (Op2Value.OpDWord / 8), lTheByte);
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
        public override void Impl()
        {
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = Op1Value;

            if (Op1Add >= mProc.REGADDRBASE)
            {
                lTheBit = Misc.GetBits3(Op1Value.OpDWord, (int)Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                lOp1Val.OpDWord = Misc.setBit(Op1Value.OpDWord, (int)Op2Value.OpByte, false);
                mProc.mem.SetDWord(mProc, Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = mProc.mem.GetByte(mProc,Op1Add + (Op2Value.OpDWord / 8));
                mProc.regs.setFlagCF(BT.TestBit(lTheByte, (int)(Op2Value.OpDWord % 8)));
                lTheByte = Misc.setBit(lTheByte, (int)(Op2Value.OpDWord % 8), false);
                mProc.mem.SetByte(mProc,Op1Add + (Op2Value.OpDWord / 8), lTheByte);
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
        public override void Impl()
        {
            byte lTheByte, lTheBit;
            sOpVal lOp1Val = Op1Value;

            if (Op1Add >= mProc.REGADDRBASE)
            {
                lTheBit = Misc.GetBits3(Op1Value.OpDWord, (int)Op2Value.OpByte, 1);
                mProc.regs.setFlagCF(lTheBit == 1);
                lOp1Val.OpDWord = Misc.setBit(Op1Value.OpDWord, (int)Op2Value.OpByte, true);
                mProc.mem.SetDWord(mProc, Op1Add, lOp1Val.OpDWord);
            }
            else
            {
                lTheByte = mProc.mem.GetByte(mProc,Op1Add + (Op2Value.OpDWord / 8));
                mProc.regs.setFlagCF(BT.TestBit(lTheByte, (int)(Op2Value.OpDWord % 8)));
                lTheByte = Misc.setBit(lTheByte, (int)(Op2Value.OpDWord % 8), true);
                mProc.mem.SetByte(mProc, Op1Add + (Op2Value.OpDWord / 8), lTheByte);
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
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Check Array Index Against Bounds";
            mModFlags = eFLAGS.OF | eFLAGS.CF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.PF;
        }
        public override void Impl()
        {
            //Operand1 = Array Index (signed integer)
            //Operand2 = Mem location with a par of signed word integers (double-word for 32 bit)
            //First dw integer is lower bound, 2nd is upper bound
            int lLowerBound = (int)Op2Value.OpDWord;
            int lUpperBound = (int)mProc.mem.GetWord(mProc,GetSegOverriddenAddress(mProc, Op2Add + 2));
            int lCheckVal = (int)Op1Value.OpDWord;
            if (lCheckVal < lLowerBound || lCheckVal > lUpperBound)
            {
                mProc.Instructions["INT"].Op1Value.OpQWord = 5;
                mProc.Instructions["INT"].Impl();
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
            mDescription = "***";
            mModFlags = 0;
        }
        public override void Impl()
        {
            bool lIsNegative = false;
            UInt16 lTempIP = mProc.regs.IP;
            UInt32 lTempEIP = mProc.regs.EIP;
            Instruct lPush = mProc.Instructions["PUSH"];
            Instruct lJMP = mProc.Instructions["JMP"];

            //lPush.mInternalInstructionCall = true;
            lPush.mProc = mProc;

            switch (ChosenOpCode.OpCode)
            {
                case 0xe8:
                case 0xff02:
                        lPush.Op1Add = mProc.REIP;
                        lPush.Op1Value.OpDWord = mProc.regs.EIP;
                        lPush.Operand1IsRef = false;
                        lPush.Impl();
                        lJMP.Op1Value = Op1Value;
                        lJMP.Op1TypeCode = Op1TypeCode;
                        lJMP.ChosenOpCode = ChosenOpCode;
                        lJMP.Operand1IsRef = Operand1IsRef;
                        lJMP.Impl();
                        break;
                case 0xff03:
                        int a = 0;
                        a += 1;
                        break;
                default:
                    lPush.Op1Add = mProc.RCS;
                    lPush.Op1Value.OpDWord = mProc.regs.CS.Value;
                    lPush.Impl();
                    lPush.Op1Add = mProc.REIP;
                    lPush.Op1Value.OpDWord = mProc.regs.EIP;
                    lPush.Operand1IsRef = false;
                    lPush.Impl();
                    lPush.Op1Add = 0x0;
                    if (mProc.OperatingMode == ProcessorMode.Protected)
                    {
                        mProc.mem.SetDWord(mProc, mProc.RCS, PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord));
                        //mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, (UInt32)Op1Val);
                        mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                    }
                    else
                    {
                        mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord);
                        mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                    }
                    break;
            }
            return;
            if (Op1TypeCode == TypeCode.UInt32 && mProc.OpSize16)
            {
                lPush.Op1Add = mProc.RCS;
                lPush.Op1Value.OpDWord = mProc.regs.CS.Value;
                lPush.Impl();
            }
            lPush.Op1Add = mProc.REIP;
            lPush.Op1Value.OpDWord = mProc.regs.EIP;
            lPush.Operand1IsRef = false;
            lPush.Impl();
            lPush.Op1Add = 0x0;

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                    lJMP.Op1Value = Op1Value;
                    lJMP.Op1TypeCode = Op1TypeCode;
                    lJMP.ChosenOpCode = ChosenOpCode;
                    lJMP.Operand1IsRef = Operand1IsRef;
                    lJMP.Impl();
                    break;
                case TypeCode.UInt32:
                    if (mProc.OperatingMode == ProcessorMode.Protected)
                    {
                        mProc.mem.SetDWord(mProc, mProc.RCS, PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord));
                        //mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, (UInt32)Op1Val);
                        mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                    }
                    else
                    {
                        mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord);
                        mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                    }
                    break;
            }
            mProc.regs.EIP &= 0xFFFF;

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
            mDescription = "***";
            mModFlags = 0;
        }
        public override void Impl()
        {
            UInt16 lTempIP = mProc.regs.IP;
            UInt32 lTempEIP = mProc.regs.EIP;
            Instruct lPush = mProc.Instructions["PUSH"];

            lPush.Op1Add = mProc.RCS;
            lPush.Op1Value.OpDWord = mProc.regs.CS.Value;
            lPush.Operand1IsRef = false;
            lPush.Impl();
            lPush.Op1Add = mProc.RIP;
            lPush.Op1Value.OpDWord = mProc.regs.EIP;
            lPush.Impl();
            lPush.Op1Add = 0x0;

            if (mProc.ProtectedModeActive)
                mProc.mem.SetDWord(mProc, mProc.RCS, PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord));
            else
                mProc.mem.SetDWord(mProc, mProc.RCS, Op1Value.OpDWord >> 0x10);
            mProc.regs.EIP = mProc.mem.GetWord(mProc,Op1Add);


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
            mDescription = "CBW—Convert GetByte to GetWord";
            mModFlags = eFLAGS.SF;
        }
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                //08/28/10 - Changed to a simpler calculation which is done by CWD
                //09/04/10 - Per Intel, if Operand Size = 32 then CWDE is executed instead
                if (Misc.getBit(mProc.regs.AL, 7) == 1)
                    mProc.regs.AH = 0xFF;
                else
                    mProc.regs.AH = 0x00;
            }
            else
                if (Misc.getBit(mProc.regs.AX, 15) == 1)
                    mProc.regs.EAX = (DWord)((0xFFFF << 16) + mProc.regs.AX);
                else
                    mProc.regs.EAX &= 0x0000FFFF;
                
           #region Instructions
            /*
                Operation
                IF OperandSize = 16 (* instruction = CBW *)
                THEN AX ← SignExtend(AL);
                ELSE (* OperandSize = 32, instruction = CWDE *)
                EAX ← SignExtend(AX);
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
        public override void Impl()
        {
            mProc.regs.setFlagCF(false);
            #region Instructions
            /*
            Operation
            CF ← 0;
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
        public override void Impl()
        {
            mProc.regs.setFlagDF(false);
            #region Instructions
            /*
            Operation
            DF ← 0;
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
        public override void Impl()
        {
            mProc.mSTIAfterNextInstr = mProc.mSTICalled = false;
            mProc.regs.setFlagIF(false);
            #region Instructions
            /*
            Operation
            IF PE = 0 (* Executing in real-address mode *)
            THEN
            IF ← 0; (* SetC Interrupt Flag *)
            ELSE
            IF VM = 0 (* Executing in protected mode *)
            THEN
            IF CPL ≤ IOPL
            THEN
            IF ← 0; (* SetC Interrupt Flag *)
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
        public override void Impl()
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.CF)
                mProc.regs.setFlagCF(false);
            else
                mProc.regs.setFlagCF(true);
            #region Instructions
            /*
             Operation
            CF ← NOT CF;
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
        public override void Impl()
        {
            bool lExecuteMove = false;

            switch (DecodedInstruction.RealOpCode)
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
                Instruct lMov = mProc.Instructions["MOV"];
                lMov.Op1TypeCode = Op1TypeCode;
                lMov.Op2TypeCode = Op2TypeCode;
                lMov.Op1Value = Op1Value;
                lMov.Op2Value = Op2Value;
                lMov.Op1Add = Op1Add;
                lMov.Op2Add = Op2Add;
                lMov.Operand1IsRef = Operand1IsRef;
                lMov.Operand2IsRef = Operand2IsRef;
                lMov.Impl();
            }
        }
    }
    public class CMP : Instruct
    {
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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValSigned = Op1Value;

            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte -= lOp2Value.OpByte;
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord -= lOp2Value.OpWord;
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord -= lOp2Value.OpDWord;
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord -= lOp2Value.OpQWord;
                    break;
            }


            //mProc.mem.SetC(mProc, Op1Addr, lOp1ValSigned);
            //SetC the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lOp2Value, lOp1ValSigned, Op1TypeCode, true, true);
        }
    }
    public class CMPSB : Instruct
    {
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
            mDescription = "Compare String GetByte";
            mModFlags = eFLAGS.SF;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            sOpVal Value1 = new sOpVal(), Value2 = new sOpVal();

            //08/28/10 - Changed this back to public comparison rather than using the CMP instruction as CMP potentially
            //sign extends, but we don't want to
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            //CMPSB has no parameters, but rather uses DS:SI, ES:DI, so we need to grab the values from memory
            //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason

            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.AddrSize16);
            Value1.OpByte = mProc.mem.GetByte(mProc,lSource);
            Value2.OpByte = mProc.mem.GetByte(mProc,lDest);
            sOpVal lPreVal1 = Value1;

            Value1.OpByte -= Value2.OpByte;

            if (mProc.regs.FLAGSB.DF == false)
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI++;
                    mProc.regs.DI++;
                }
                else
                {
                    mProc.regs.ESI++;
                    mProc.regs.EDI++;
                }
            }
            else
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI--;
                    mProc.regs.DI--;
                }
                else
                {
                    mProc.regs.ESI--;
                    mProc.regs.EDI--;
                }
            }
            //notice that we don't save the result, we just use it to save the flags
            SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.Byte, true, true);
            #region Instructions
            /*
            Operation
            temp ←SRC1 − SRC2;
            SetStatusFlags(temp);
            IF (byte comparison)
            THEN IF DF = 0
            THEN
            (E)SI ← (E)SI + 1;
            (E)DI ← (E)DI + 1;
            ELSE
            (E)SI ← (E)SI – 1;
            (E)DI ← (E)DI – 1;
            FI;
            ELSE IF (word comparison)
            THEN IF DF = 0
            (E)SI ← (E)SI + 2;
            (E)DI ← (E)DI + 2;
            ELSE
            (E)SI ← (E)SI – 2;
            (E)DI ← (E)DI – 2;
            FI;
            ELSE (* doubleword comparison*)
            THEN IF DF = 0
            (E)SI ← (E)SI + 4;
            (E)DI ← (E)DI + 4;
            ELSE
            (E)SI ← (E)SI – 4;
            (E)DI ← (E)DI – 4;
            FI;
            FI;
            Flags Affected
            The CF, OF, SF, ZF, AF, and PF flags are set according to the temporary result of the comparison.
            Protected Mode Exceptions
            #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
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
    public class CMPSW : Instruct
    {
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
            mDescription = "Compare String GetWord";
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            sOpVal Value1 = new sOpVal(), Value2 = new sOpVal();
            //08/28/10 - Changed this back to public comparison rather than using the CMP instruction as CMP potentially
            //sign extends, but we don't want to
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            if (!mProc.OpSize16)
            {
                mProc.Instructions["CMPSD"].Impl();
                return;
            }

            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.AddrSize16);
            Value1.OpWord = mProc.mem.GetWord(mProc, lSource);
            Value2.OpWord = mProc.mem.GetWord(mProc, lDest);

            sOpVal lPreVal1 = Value1;

            Value1.OpWord -= Value2.OpWord;

            if (mProc.regs.FLAGSB.DF == false)
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI+=2;
                    mProc.regs.DI+=2;
                }
                else
                {
                    mProc.regs.ESI+=2;
                    mProc.regs.EDI+=2;
                }
            }
            else
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI-=2;
                    mProc.regs.DI-=2;
                }
                else
                {
                    mProc.regs.ESI-=2;
                    mProc.regs.EDI-=2;
                }
            }
            //notice that we don't save the result, we just use it to save the flags
            SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.UInt16, true, true);
            #region Instructions
            /*
            Operation
            temp ←SRC1 − SRC2;
            SetStatusFlags(temp);
            IF (byte comparison)
            THEN IF DF = 0
            THEN
            (E)SI ← (E)SI + 1;
            (E)DI ← (E)DI + 1;
            ELSE
            (E)SI ← (E)SI – 1;
            (E)DI ← (E)DI – 1;
            FI;
            ELSE IF (word comparison)
            THEN IF DF = 0
            (E)SI ← (E)SI + 2;
            (E)DI ← (E)DI + 2;
            ELSE
            (E)SI ← (E)SI – 2;
            (E)DI ← (E)DI – 2;
            FI;
            ELSE (* doubleword comparison*)
            THEN IF DF = 0
            (E)SI ← (E)SI + 4;
            (E)DI ← (E)DI + 4;
            ELSE
            (E)SI ← (E)SI – 4;
            (E)DI ← (E)DI – 4;
            FI;
            FI;
            Flags Affected
            The CF, OF, SF, ZF, AF, and PF flags are set according to the temporary result of the comparison.
            Protected Mode Exceptions
            #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
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
    public class CMPSD : Instruct
    {
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
            mDescription = "Compare String Double GetWord";
            mModFlags = eFLAGS.CF | eFLAGS.OF | eFLAGS.SF | eFLAGS.ZF | eFLAGS.AF | eFLAGS.PF;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            sOpVal Value1 = new sOpVal(), Value2 = new sOpVal();
            //08/28/10 - Changed this back to public comparison rather than using the CMP instruction as CMP potentially
            //sign extends, but we don't want to
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            //CMPSD has no parameters, but rather uses DS:SI, ES:DI, so we need to grab the values from memory
            //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lDest, mProc.AddrSize16);
            Value1.OpDWord = mProc.mem.GetDWord(mProc, lSource);
            Value2.OpDWord = mProc.mem.GetDWord(mProc, lDest);

            sOpVal lPreVal1 = Value1;

            Value1.OpDWord -= Value2.OpDWord;

            if (mProc.regs.FLAGSB.DF == false)
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI += 4;
                    mProc.regs.DI += 4;
                }
                else
                {
                    mProc.regs.ESI += 4;
                    mProc.regs.EDI += 4;
                }
            }
            else
            {
                if (mProc.AddrSize16)
                {
                    mProc.regs.SI -= 4;
                    mProc.regs.DI -= 4;
                }
                else
                {
                    mProc.regs.ESI -= 4;
                    mProc.regs.EDI -= 4;
                }
            }
            //notice that we don't save the result, we just use it to save the flags
            SetFlagsForSubtraction(mProc, lPreVal1, Value2, Value1, TypeCode.UInt32, true, true);
            #region Instructions
            /*
            Operation
            temp ←SRC1 − SRC2;
            SetStatusFlags(temp);
            IF (byte comparison)
            THEN IF DF = 0
            THEN
            (E)SI ← (E)SI + 1;
            (E)DI ← (E)DI + 1;
            ELSE
            (E)SI ← (E)SI – 1;
            (E)DI ← (E)DI – 1;
            FI;
            ELSE IF (word comparison)
            THEN IF DF = 0
            (E)SI ← (E)SI + 2;
            (E)DI ← (E)DI + 2;
            ELSE
            (E)SI ← (E)SI – 2;
            (E)DI ← (E)DI – 2;
            FI;
            ELSE (* doubleword comparison*)
            THEN IF DF = 0
            (E)SI ← (E)SI + 4;
            (E)DI ← (E)DI + 4;
            ELSE
            (E)SI ← (E)SI – 4;
            (E)DI ← (E)DI – 4;
            FI;
            FI;
            Flags Affected
            The CF, OF, SF, ZF, AF, and PF flags are set according to the temporary result of the comparison.
            Protected Mode Exceptions
            #GP(0) If a memory operand effective address is outside the CS, DS, ES, FS, or
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
        public override void Impl()
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
                    mProc.regs.ECX = 0x00002001;
                    //mProc.regs.EDX = 0x178BFBFF;
                    mProc.regs.EDX = 0x013; //Only PSE, VM(E) and FPU enabled
                    break;
                case 0x80000000:
                    mProc.regs.EAX = 0x18;
                    mProc.regs.EBX = 0x68747541;
                    mProc.regs.ECX = 0x444D4163;
                    mProc.regs.EDX = 0x69746E65;
                    break;
                case 0x80000001:
                    mProc.regs.EAX = 0x40F8B2;
                    mProc.regs.EBX = 0x000008CD;
                    mProc.regs.ECX = 0x1f;
                    mProc.regs.EDX = 0xEBD3FBFF;
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
            mDescription = "Convert GetWord to Doubleword";
            mModFlags = 0;
        }
        public override void Impl()
        {
            //08/28/10 - Changed last param from 15 to 7 - its AH after all, duuh!
            //09/04/10 - Per Intel, if Operand Size = 32 then CDQ is executed instead
            if (mProc.OpSize16)
            {
                if (Misc.getBit(mProc.regs.AX, 15) == 1)
                    mProc.regs.DX = 0xFFFF;
                else
                    mProc.regs.DX = 0;
            }
            else
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
                THEN DX ← SignExtend(AX);
                ELSE (* OperandSize = 32, CDQ instruction *)
                EDX ← SignExtend(EAX);
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
        public override void Impl()
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
            AL ← AL + 6;
            CF ← CF OR CarryFromLastAddition; (* CF OR carry from AL ← AL + 6 *)
            AF ← 1;
            ELSE
            AF ← 0;
            FI;
            IF ((AL AND F0H) > 90H) or CF = 1)
            THEN
            AL ← AL + 60H;
            CF ← 1;
            ELSE
            CF ← 0;
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
        public override void Impl()
        {
            bool lCFBefore = false;
            byte lALBefore = mProc.regs.AL;
            if ((mProc.regs.AL & 0xF) > 0x9 || (Misc.getBit(mProc.regs.FLAGS, (int)eFLAGS.AF) == 1))
            {
                mProc.regs.AL -= 0x6;

                lCFBefore = mProc.regs.FLAGSB.CF;
                mProc.regs.setFlagCF(lALBefore, mProc.regs.AL, 0x6);

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
                AL ← AL − 6;
                CF ← CF OR BorrowFromLastSubtraction; (* CF OR borrow from AL ← AL − 6 *)
                AF ← 1;
                ELSE AF ← 0;
                FI;
                IF ((AL > 9FH) or CF = 1)
                THEN
                AL ← AL − 60H;
                CF ← 1;
                ELSE CF ← 0;
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
        public override void Impl()
        {
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;

            //Do the math!
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte--;
                    mProc.mem.SetByte(mProc,Op1Add, lOp1Value.OpByte);
                    Op2Value.OpByte = 0xff;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    mProc.regs.setFlagOF(lOp1Value.OpByte, Op2Value.OpByte, Op1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord--;
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    Op2Value.OpWord = 0xffff;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    mProc.regs.setFlagOF(lOp1Value.OpWord, Op2Value.OpWord, Op1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord--;
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    Op2Value.OpDWord = 0xffffffff;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    mProc.regs.setFlagOF(lOp1Value.OpDWord, Op2Value.OpDWord, Op1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpDWord--;
                    mProc.mem.SetQWord(mProc,Op1Add, lOp1Value.OpQWord);
                    Op2Value.OpQWord = 0xffffffffffffffff;
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    mProc.regs.setFlagOF(lOp1Value.OpQWord, Op2Value.OpQWord, Op1Value.OpQWord);
                    break;
            }
            //Put the value back in Dest's referenced location

//            SetFlagsForSubtraction(mProc, lPreVal1, Op2Value, lOp1Value, Op1TypeCode, false, true);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);
            mProc.regs.setFlagAF(lPreVal1.OpQWord, lOp1Value.OpQWord);
            mProc.regs.setFlagPF(lOp1Value.OpQWord);
            #region Instructions
            /*
                Description
                Subtracts 1 from the destination operand, while preserving the state of the CF flag. The destination
                operand can be a register or a memory location. This instruction allows a loop counter to be
                updated without disturbing the CF flag. (To perform a decrement operation that updates the CF
                flag, use a SUB instruction with an immediate operand of 1.)
                This instruction can be used with a LOCK prefix to allow the instruction to be executed atomically.
                Operation
                DEST ← DEST – 1;
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
        public override void Impl()
        {
            if (Op1Value.OpQWord == 0)
                throw new DivideByZeroException();

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    Word lTempAX = mProc.regs.AX;
                    mProc.regs.AL = (byte)(lTempAX / (UInt16)Op1Value.OpByte);
                    mProc.regs.AH = (byte)(lTempAX % (UInt16)Op1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    UInt32 lDXAX = (UInt32)((mProc.regs.DX << 16) + mProc.regs.AX);
                    mProc.regs.AX = (UInt16)(lDXAX / (UInt32)Op1Value.OpWord);
                    mProc.regs.DX = (UInt16)(lDXAX % (UInt32)Op1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    UInt64 lEDXEAX = (UInt64)((mProc.regs.EDX << 16) + mProc.regs.EAX);
                    mProc.regs.EAX = (UInt32)(lEDXEAX / (UInt64)Op1Value.OpDWord);
                    mProc.regs.EDX = (UInt32)(lEDXEAX % (UInt64)Op1Value.OpDWord);
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

    //    public override void Impl()
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
    //    public override void Impl()
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

    //    public override void Impl()
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
    //    public override void Impl()
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
    //    public override void Impl()
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

    //    public override void Impl()
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
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Make stack frame";
            mModFlags = 0;
        }
        public override void Impl()
        {
            sOpVal lOp2Value = new sOpVal();
            lOp2Value.OpByte= (byte)(Op2Value.OpByte % 0x20);
            Instruct lPush = mProc.Instructions["PUSH"];
            sOpVal FrameTemp;

            //Debug.WriteLine("Before ENTER, SP=" + OpDecoder.ValueToHex(mProc.regs.SP, 4) + ", BP=" + OpDecoder.ValueToHex(mProc.regs.BP, 4));

            FrameTemp = new sOpVal();

            if (mProc.AddrSize16)
            {
                lPush.Op1Add = mProc.RBP;
                lPush.Op1Value.OpWord = mProc.regs.BP;
                lPush.Operand1IsRef = false;
                lPush.Impl();
                FrameTemp.OpWord = mProc.regs.SP;
            }
            else
            {
                lPush.Op1Add = mProc.REBP;
                lPush.Op1Value.OpDWord = mProc.regs.EBP;
                lPush.Operand1IsRef = false;
                lPush.Impl();
                FrameTemp.OpDWord = mProc.regs.ESP;
            }
            if (lOp2Value.OpByte > 0)
            {
                for (int c = 1; c < lOp2Value.OpByte - 1; c++)
                {
                    if (!mProc.OpSize16) //32 bit operand size
                    {
                        if (!mProc.AddrSize16) //32 bit address size
                        {
                            mProc.regs.EBP -= 4;
                            lPush.Op1Add = mProc.REBP;
                            lPush.Op1Value.OpDWord = mProc.regs.EBP;
                            lPush.Operand1IsRef = false;
                            lPush.Impl();
                        }
                        else                    //16 bit address size
                        {
                            mProc.regs.BP -= 2;
                            lPush.Op1Add = mProc.RBP;
                            lPush.Op1Value.OpWord = mProc.regs.BP;
                            lPush.Operand1IsRef = false;
                            lPush.Impl();
                        }
                    }
                    else                 //16 bit operand size
                    {
                        if (!mProc.AddrSize16) //32 bit address size
                        {
                            mProc.regs.EBP -= 4;
                            lPush.Op1Add = mProc.REBP;
                            lPush.Op1Value.OpDWord = mProc.regs.EBP;
                            lPush.Operand1IsRef = false;
                            lPush.Impl();
                        }
                        else                     //16 bit address size
                        {
                            mProc.regs.BP -= 2;
                            lPush.Op1Add = mProc.RBP;
                            lPush.Op1Value.OpWord = mProc.regs.BP;
                            lPush.Operand1IsRef = false;
                            lPush.Impl();
                        }

                    }
                }
                lPush.Op1Value.OpDWord = FrameTemp.OpDWord;
                lPush.Operand1IsRef = false;
                lPush.Impl();
            }
            //CONTINUE
            if (!mProc.AddrSize16)             //32 bit address size
            {
                mProc.regs.EBP = FrameTemp.OpDWord;
                mProc.regs.ESP = mProc.regs.EBP - Op1Value.OpDWord;
            }
            else
            {
                mProc.regs.BP = FrameTemp.OpWord;
                mProc.regs.SP = (Word)(mProc.regs.BP - Op1Value.OpWord);
            }
            #region Instructions
            /*
                 NestingLevel ← NestingLevel MOD 32
                IF StackSize = 32
                THEN
                Push(EBP) ;
                FrameTemp ← ESP;
                ELSE (* StackSize = 16*)
                Push(BP);
                FrameTemp ← SP;
                FI;
                IF NestingLevel = 0
                THEN GOTO CONTINUE;
                FI;
                IF (NestingLevel > 0)
                FOR i ← 1 TO (NestingLevel − 1)
                DO
                IF OperandSize = 32
                THEN
                IF StackSize = 32
                EBP ← EBP − 4;
                Push([EBP]); (* doubleword push *)
                ELSE (* StackSize = 16*)
                BP ← BP − 4;
                Push([BP]); (* doubleword push *)
                FI;
                ELSE (* OperandSize = 16 *)
                IF StackSize = 32
                THEN
                EBP ← EBP − 2;
                Push([EBP]); (* word push *)
                ELSE (* StackSize = 16*)
                BP ← BP − 2;
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
                EBP ← FrameTemp
                ESP ← EBP − Size;
                ELSE (* StackSize = 16*)
                BP ← FrameTemp
                SP ← BP − Size;
                FI;
                END;
                */
            #endregion
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
        public override void Impl()
        {
            mProc.mFPU.mStatusReg.Parse((Word)(mProc.mFPU.mStatusReg.Value & 0x7F00));
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
            FPUInstruction = true;
        }
        public override void Impl()
        {
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
        public override void Impl()
        {
            switch (DecodedInstruction.OpCode)
            {
                case 0xDB:
                    mProc.mFPU.PushInt(System.Convert.ToInt32(Op1Value.OpDWord));
                    break;
                case 0xDF00:
                        mProc.mFPU.PushInt(System.Convert.ToInt16(Op1Value.OpWord));
                        break;
                case 0xDF05:
                        mProc.mFPU.PushInt((Int64)Op1Value.OpQWord);
                    break;
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
        public override void Impl()
        {
            sOpVal OpVal = new sOpVal(0);

            switch (DecodedInstruction.OpCode)
            {
                case 0xDF02:
                    mProc.mem.SetWord(mProc, Op1Add, (Word)System.Convert.ToInt16(mProc.mFPU.DataReg[mProc.mFPU.mStatusReg.Top]));
                    break;
                case 0xDB02:
                    mProc.mem.SetDWord(mProc, Op1Add, (DWord)System.Convert.ToInt32(mProc.mFPU.DataReg[mProc.mFPU.mStatusReg.Top]));
                    break;
                case 0xDF03:
                    mProc.mem.SetWord(mProc, Op1Add, (Word)System.Convert.ToInt16(mProc.mFPU.Pop()));
                    break;
                case 0xDB03:
                    mProc.mem.SetDWord(mProc, Op1Add, (DWord)System.Convert.ToInt32(mProc.mFPU.Pop()));
                    break;
                case 0xDF07:
                    mProc.mem.SetQWord(mProc, Op1Add, (QWord)System.Convert.ToInt64(mProc.mFPU.Pop()));
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
        public override void Impl()
        {
            mProc.mFPU.mControlReg.Parse(Op1Value.OpWord);
            #region Instructions
            /*
    */
            #endregion
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
        public override void Impl()
        {
            mProc.mFPU.PushInt(1);
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
        public override void Impl()
        {
            mProc.mFPU.PushInt(0);
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
        public override void Impl()
        {
            QWord lOp1Value = 0;
            long bits = 0;

            switch (DecodedInstruction.OpCode)
            {
                case 0xDC01:
                    //for (UInt32 c = Op1Add; c < Op1Add + 8; c++)
                    //    lOp1Value = lOp1Value << 8 | (QWord)mProc.mem.GetByte(mProc, c);

                    mProc.mFPU.Multiply((BitConverter.Int64BitsToDouble((long)Op1Value.OpQWord)), 0, 1, false, false);
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
        public override void Impl()
        {
            switch (DecodedInstruction.OpCode)
            {
                case 0xD807:
                    mProc.mFPU.Divide((BitConverter.Int64BitsToDouble((long)Op1Value.OpDWord)), 0, 1, false, false);
                    break;
                case (0xDCF0):
                case (0xDCF1):
                case (0xDCF2):
                case (0xDCF3):
                case (0xDCF4):
                case (0xDCF5):
                case (0xDCF6):
                case (0xDCF7):
                    mProc.mFPU.Divide(DecodedInstruction.OpCode - 0xDCF0, 0, 2, false, false);
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
        public override void Impl()
        {
            switch (DecodedInstruction.OpCode)
            {
                case 0xDD07:
                    mProc.mem.SetWord(mProc, Op1Add, mProc.mFPU.mStatusReg.Value);
                    break;
                case 0xDFE0:
                    mProc.regs.AX = mProc.mFPU.mStatusReg.Value;
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
        public override void Impl()
        {
            mProc.mem.SetWord(mProc, Op1Add, mProc.mFPU.mControlReg.Value);
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
        public override void Impl()
        {
            UInt32 lOp1Add = Op1Add;

            if (DecodedInstruction.ProtectedModeActive)
                //Protected mode
                if (DecodedInstruction.InstructionOpSize16)
                {   //16 bit Protected
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastIP));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandSegSel));
                }
                else
                {   //32 bit Protected
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastIP));
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOpCode & 0x7FF));
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr));
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandSegSel));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mTagReg.Value);
                }
            else
                //Real or VMode
                if (DecodedInstruction.InstructionOpSize16)
                {   //16 bit Real/VMode
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastIP & 0xFFFF));
                    lOp1Add += 2;
                    Word lTemp = (Word)(mProc.mFPU.mLastIP & 0xF0000 >> 4);
                    lTemp |= (Word)(mProc.mFPU.mLastOpCode & 0x7FF);
                    mProc.mem.SetWord(mProc, lOp1Add, lTemp);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xF0000 >> 4));
                }
                else
                {   //32 bit Real/VMode
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mControlReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mStatusReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, mProc.mFPU.mTagReg.Value);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastIP & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    DWord lTemp = (DWord)(mProc.mFPU.mLastIP & 0xFFFF0000 >> 4);
                    lTemp |= (Word)(mProc.mFPU.mLastOpCode & 0x7FF);
                    mProc.mem.SetDWord(mProc, lOp1Add, lTemp);
                    lOp1Add += 4;
                    mProc.mem.SetWord(mProc, lOp1Add, (Word)(mProc.mFPU.mLastOperandPtr & 0xFFFF));
                    lOp1Add += 2;
                    mProc.mem.SetWord(mProc, lOp1Add, 0);
                    lOp1Add += 2;
                    mProc.mem.SetDWord(mProc, lOp1Add, (DWord)(mProc.mFPU.mLastIP & 0xFFFF0000 >> 4));
                }

            #region Instructions
            /*
    */
            #endregion
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
        public override void Impl()
        {
            //mProc.regs.setFlagIF(true);
            System.Diagnostics.Debug.WriteLine("HALT called from: " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.IP.ToString("X8"));
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
        public override void Impl()
        {
            byte lTemp = 0x01;
            mProc.Instructions["INT"].Op1Value.OpByte = lTemp;
            mProc.Instructions["INT"].Impl();
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
        public override void Impl()
        {
            if (Op1Value.OpQWord == 0)
                throw new DivideByZeroException();

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    Word lTempAX = Misc.SignExtend(mProc.regs.AL);
                    mProc.regs.AL = (byte)((Int16)lTempAX / (Int16)Op1Value.OpByte);
                    mProc.regs.AH = (byte)(lTempAX % (UInt16)Op1Value.OpByte);
                    break;
                //SetC the flags
                case TypeCode.UInt16:
                    if (Misc.IsNegative(mProc.regs.AX))
                        mProc.regs.DX = 0xFFFF;
                    UInt32 lDXAX = (UInt32)((mProc.regs.DX << 16) + mProc.regs.AX);
                    mProc.regs.AX = (UInt16)((Int32)lDXAX / (Int32)Op1Value.OpWord);
                    mProc.regs.DX = (UInt16)((Int32)lDXAX % (Int32)Op1Value.OpWord);
                    //SetC the flags
                    break;
                case TypeCode.UInt32:
                    if (Misc.IsNegative(mProc.regs.EAX))
                        mProc.regs.EDX = 0xFFFFFFFF;
                    Int64 lEDXEAX = mProc.regs.EDX;
                    lEDXEAX <<= 32;
                    lEDXEAX |= mProc.regs.EAX;
                    mProc.regs.EAX = (UInt32)((Int64)lEDXEAX / (Int64)Op1Value.OpDWord);
                    mProc.regs.EDX = (UInt32)((Int64)lEDXEAX % (Int64)Op1Value.OpDWord);
                    break;
                #region Instructions
                /*
            Operation
                IF SRC = 0
                THEN #DE; (* divide error *)
                FI;
                IF OpernadSize = 8 (* word/byte operation *)
                THEN
                temp ← AX / SRC; (* signed division *)
                IF (temp > 7FH) OR (temp < 80H)
                (* if a positive result is greater than 7FH or a negative result is less than 80H *)
                THEN #DE; (* divide error *) ;
                ELSE
                AL ← temp;
                AH ← AX SignedModulus SRC;
                FI;
                ELSE
                IF OpernadSize = 16 (* doubleword/word operation *)
                THEN
                temp ← DX:AX / SRC; (* signed division *)
                IF (temp > 7FFFH) OR (temp < 8000H)
                (* if a positive result is greater than 7FFFH *)
                (* or a negative result is less than 8000H *)
                THEN #DE; (* divide error *) ;
                ELSE
                AX ← temp;
                DX ← DX:AX SignedModulus SRC;
                FI;
                ELSE (* quadword/doubleword operation *)
                temp ← EDX:EAX / SRC; (* signed division *)
                IF (temp > 7FFFFFFFH) OR (temp < 80000000H)
                (* if a positive result is greater than 7FFFFFFFH *)
                (* or a negative result is less than 80000000H *)
                THEN #DE; (* divide error *) ;
                ELSE
                EAX ← temp;
                EDX ← EDXE:AX SignedModulus SRC;
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
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Signed Multiply";
            mModFlags = eFLAGS.OF | eFLAGS.CF;
        }
        public override void Impl()
        {
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = Op2Value; // SignExtendOp(Op2Val, Operand2IsRef, Op1Val.GetTypeCode()); ;

            sbyte mOp1_8 = 0, mOp2_8 = 0;
            Int16 mOp1_16 = 0, mOp2_16 = 0, mTemp_16 = 0;
            Int32 mOp1_32 = 0, mOp2_32 = 0, mTemp_32 = 0;
            Int64 mTemp_64 = 0;
            switch (DecodedInstruction.RealOpCode)
            {
                #region 0xF6
                case 0xF6:      //AX← AL ∗ r/m byte
                    mOp1_8 = (sbyte)mProc.regs.AL;
                    mOp2_8 = (sbyte)Op1Value.OpByte;
                    mProc.regs.AX = (Word)(mOp1_8 * mOp2_8);
                    IMUL_SetFlags(mProc.regs.AX, mProc.regs.AL);
                    break;
                #endregion
                #region 0xF7
                case 0xF7:
                    if (mProc.OpSize16)    //DX:AX ← AX ∗ r/m word 
                    {
                        mOp1_16 = (Int16)mProc.regs.AX;
                        mOp2_16 = (Int16)Op1Value.OpWord;
                        mProc.regs.DX = (Word)Misc.GetHi((mOp1_16 * mOp2_16));
                        mProc.regs.AX = (Word)Misc.GetLo((mOp1_16 * mOp2_16));
                        IMUL_SetFlags(Misc.SignExtend(mProc.regs.AX), (Int64)((mProc.regs.DX << 16) + mProc.regs.AX));
                    }
                    else                    //EDX:EAX ← EAX ∗ r/m doubleword
                    {
                        mOp1_32 = (Int16)mProc.regs.EAX;
                        mOp2_32 = (Int16)Op1Value.OpDWord;
                        mProc.regs.EDX = Misc.GetHi((Int64)(mOp1_32 * mOp2_32));
                        mProc.regs.EAX = Misc.GetLo((Int64)(mOp1_32 * mOp2_32));
                        IMUL_SetFlags((Int64)Misc.SignExtend(mProc.regs.EAX), (Int64)((mProc.regs.EDX << 32) + mProc.regs.EAX));


                    }
                    break;
                #endregion
                #region 0x0FAF
                case 0x0FAF:
                    if (mProc.OpSize16)    //IMUL r16,r/m16 word register ← word register ∗ r/m word
                    {
                        mOp1_16 = (Int16)Op1Value.OpWord;
                        mOp2_16 = (Int16)lOp2Value.OpWord;
                        mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                        mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                        //TODO: Make sure this works!
                        mProc.mem.SetWord(mProc,Op1Add, (UInt16)mOp1_16);
                        IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                    }
                    else                    //IMUL r32,r/m32 doubleword register ← doubleword register ∗ r/m doubleword
                    {
                        mOp1_32 = (Int32)Op1Value.OpDWord;
                        mOp2_32 = (Int32)lOp2Value.OpDWord;
                        mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                        mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                        mProc.mem.SetDWord(mProc,Op1Add, (UInt32)mOp1_32);
                        IMUL_SetFlags((Int64)mTemp_64, (Int64)mOp1_32);
                    }
                    break;
                #endregion
                #region 0x6B
                case 0x6B:
                    if (Op3Value.OpQWord != 0)
                    {
                        if (mProc.OpSize16)    //IMUL r16,r/m16,imm8 word register ← r/m16 ∗ sign-extended immediate byte
                        {
                            mOp1_16 = (Int16)lOp2Value.OpWord;
                            mOp2_16 = (Int16)Misc.SignExtend(Op3Value.OpByte);
                            mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc,Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)(mTemp_32), (Int64)(mOp1_16));
                        }
                        else                    //IMUL r32,r/m32,imm8 doubleword register ← r/m32 ∗ sign-extended immediate byte
                        {
                            mOp1_32 = (Int32)lOp2Value.OpDWord;
                            mOp2_32 = (Int32)Misc.SignExtend(Op3Value.OpWord);
                            mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc,Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)(mTemp_64), (Int64)(mOp1_32));
                        }
                    } //end 3 operands
                    else
                    {
                        if (mProc.OpSize16)    //IMUL r16,imm8 word register ← word register ∗ sign-extended immediate byte
                        {
                            mOp1_16 = (Int16)Op1Value.OpWord;
                            mOp2_16 = (Int16)Misc.SignExtend(lOp2Value.OpByte);
                            mTemp_32 = mOp1_16 * mOp2_16;
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc,Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)(mTemp_32), (Int64)(mOp1_16));
                        }
                        else                    //IMUL r32,imm8 doubleword register ← doubleword register ∗ signextended immediate byte
                        {
                            mOp1_32 = (Int32)Op1Value.OpDWord;
                            mOp2_32 = (Int32)Misc.SignExtend(lOp2Value.OpWord);
                            mTemp_64 = mOp1_32 * mOp2_32;
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc,Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)(mTemp_64), (Int64)(mOp1_32));
                        }
                    }
                    break;
                #endregion
                #region 0x69
                case 0x69:
                    if (Op3Val != null)
                    {
                        if (mProc.OpSize16)    //IMUL r16,r/m16,imm16 word register ← r/m16 ∗ immediate word
                        {
                            mOp1_16 = (Int16)lOp2Value.OpWord;
                            mOp2_16 = (Int16)Op3Value.OpWord;
                            mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)((Int16)mOp1_16 * (Int16)mOp2_16);
                            mProc.mem.SetWord(mProc,Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                        }
                        else                    //IMUL r32,r/m32,imm32 doubleword register ← r/m32 ∗ immediate doubleword
                        {
                            mOp1_32 = (Int32)lOp2Value.OpDWord;
                            mOp2_32 = (Int32)Op3Value.OpDWord;
                            mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc,Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)(mTemp_64), (Int64)(mOp1_32));
                        }
                    }
                    else
                    {
                        if (mProc.OpSize16)    //word register ← r/m16 ∗ immediate word
                        {
                            mOp1_16 = (Int16)Op1Value.OpWord;
                            mOp2_16 = (Int16)lOp2Value.OpWord;
                            mTemp_32 = (Int32)(mOp1_16 * mOp2_16);
                            mOp1_16 = (Int16)(mOp1_16 * mOp2_16);
                            mProc.mem.SetWord(mProc,Op1Add, (UInt16)mOp1_16);
                            IMUL_SetFlags((Int64)mTemp_32, (Int64)mOp1_16);
                        }
                        else                    //doubleword register ← r/m32 ∗ immediate doubleword
                        {
                            mOp1_32 = (Int32)Op1Value.OpDWord;
                            mOp2_32 = (Int32)lOp2Value.OpDWord;
                            mTemp_64 = (Int64)(mOp1_32 * mOp2_32);
                            mOp1_32 = (Int32)(mOp1_32 * mOp2_32);
                            mProc.mem.SetDWord(mProc,Op1Add, (UInt32)mOp1_32);
                            IMUL_SetFlags((Int64)mTemp_64, (Int64)mOp1_32);
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
        private void IMUL_SetFlags(cValue Value1, cValue Value2)
        {
            if (Value1.mQWord == Value2.mQWord)
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
        private void IMUL_SetFlags(Int64 Value1, Int64 Value2)
        {
            if (Value1 == Value2 )
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
        public override void Impl()
        {
            cValue lOp2Value = new cValue(Op2Value.OpWord);
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc,Op1Add, mProc.ports.In(lOp2Value, Op1TypeCode).mByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc,Op1Add, mProc.ports.In(lOp2Value, Op1TypeCode).mWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc,Op1Add, mProc.ports.In(lOp2Value, Op1TypeCode).mDWord);
                    break;
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
        public override void Impl()
        {
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;

            //Do the math!
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte++;
                    mProc.mem.SetByte(mProc,Op1Add, lOp1Value.OpByte);
                    Op2Value.OpByte = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    mProc.regs.setFlagOF(lOp1Value.OpByte, Op2Value.OpByte, Op1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord++;
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    Op2Value.OpWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    mProc.regs.setFlagOF(lOp1Value.OpWord, Op2Value.OpWord, Op1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord++;
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    Op2Value.OpDWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    mProc.regs.setFlagOF(lOp1Value.OpDWord, Op2Value.OpDWord, Op1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpDWord++;
                    mProc.mem.SetQWord(mProc,Op1Add, lOp1Value.OpQWord);
                    Op2Value.OpQWord = 0x1;
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    mProc.regs.setFlagOF(lOp1Value.OpQWord, Op2Value.OpQWord, Op1Value.OpQWord);
                    break;
            }
//            SetFlagsForAddition(mProc, lPreVal1, Op2Value, lOp1Value, lOp1Value, Op1TypeCode);
            mProc.regs.setFlagZF(lOp1Value.OpQWord);
            mProc.regs.setFlagAF(lOp1Value.OpQWord, lPreVal1.OpQWord);
            mProc.regs.setFlagPF(lOp1Value.OpQWord);

        }
    }
    public class INS : Instruct
    {
        public INS()
        {
            mName = "INS";
            mProc8086 = true;
            mProc8088 = true;
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
        public override void Impl()
        {
            UInt32 lDest;
            bool lAddrSize16 = mProc.AddrSize16;
            bool lOpSize16 = mProc.OpSize16;
            if (DecodedInstruction.RealOpCode == 0x6c)
            {
                if (lAddrSize16)
                    lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
                else
                    lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
                mProc.mem.SetByte(mProc, lDest, mProc.ports.In(new cValue((Word)mProc.regs.DX), TypeCode.Byte).mByte);
                if (mProc.regs.FLAGSB.DF == false)
                    if (lAddrSize16)
                        mProc.regs.DI++;
                    else
                        mProc.regs.EDI++;
                else
                    if (lAddrSize16)
                        mProc.regs.DI--;
                    else
                        mProc.regs.EDI--;
            }
            else
                if (lOpSize16)
                {
                    if (lAddrSize16)
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
                    else
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
                    mProc.mem.SetWord(mProc, lDest, mProc.ports.In(new cValue((Word)mProc.regs.DX), TypeCode.UInt16).mWord);
                    if (mProc.regs.FLAGSB.DF == false)
                        if (lAddrSize16)
                            mProc.regs.DI+=2;
                        else
                            mProc.regs.EDI+=2;
                    else
                        if (lAddrSize16)
                            mProc.regs.DI-=2;
                        else
                            mProc.regs.EDI-=2;
                }
                else
                {
                    if (lAddrSize16)
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
                    else
                        lDest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
                    mProc.mem.SetDWord(mProc, lDest, mProc.ports.In(new cValue((Word)mProc.regs.DX), TypeCode.UInt32).mDWord);
                    if (mProc.regs.FLAGSB.DF == false)
                        if (lAddrSize16)
                            mProc.regs.DI += 4;
                        else
                            mProc.regs.EDI += 4;
                    else
                        if (lAddrSize16)
                            mProc.regs.DI -= 4;
                        else
                            mProc.regs.EDI -= 4;
                }
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
            mModFlags = 0;
        }
        public override void Impl()
        {
            UInt32 lTempSS, lTempESP;
            UInt32 lTempEFLAGS, lNewSS, lNewESP, lTempCS;
            sIDTEntry lIDT;
            sGDTEntry lGDT;

            //if (mProc.regs.FLAGSB.NT)
            //    System.Diagnostics.Debugger.Break();


            Instruct lPush;
            lPush = mProc.Instructions["PUSH"];
            lPush.Operand1IsRef = false;
            if (mProc.OperatingMode == ProcessorMode.Real)
                lTempSS = mProc.regs.SS.Value;
            else
                lTempSS = mProc.regs.SS.mWholeSelectorValue;
            lTempESP = mProc.regs.ESP;

            switch (mProc.OperatingMode)
            {
                case ProcessorMode.Real:
                    Word lTemp = mProc.regs.FLAGS;

                    lPush.Op1Add = 0x0;
                    lPush.Op1Value.OpWord = lTemp;
                    lPush.Operand1IsRef = false;
                    lPush.Impl();

                    mProc.regs.setFlagIF(false);
                    mProc.regs.setFlagTF(false);
                    mProc.regs.setFlagAC(false);

                    lPush.Op1Add = mProc.RCS;
                    lPush.Op1Value.OpDWord = mProc.regs.CS.Value;
                    lPush.Impl();
                    lPush.Op1Add = mProc.RIP;
                    lPush.Op1Value.OpDWord = mProc.regs.IP;
                    lPush.Impl();
                    lPush.Op1Add = 0x0;
                    mProc.regs.CS.Value = mProc.mem.GetWord(mProc, (Op1Value.OpDWord * 4 + 2));
                    mProc.regs.IP = mProc.mem.GetWord(mProc, Op1Value.OpDWord * 4);
                    break;

                default:    //protected or virtual mode
                    //if (mProc.regs.FLAGSB.VM && mProc.regs.FLAGSB.IOPL < 3)
                    //    throw new Exception("Code GP(0) here");
                    if (Op1Value.OpWord > mProc.mIDTCache.Count - 1
                         || (mProc.mIDTCache[Op1Value.OpWord].GateType != eSystemOrGateDescType.Interrupt_Gate_32
                         && mProc.mIDTCache[Op1Value.OpWord].GateType != eSystemOrGateDescType.Interrupt_Gate_16
                         && mProc.mIDTCache[Op1Value.OpWord].GateType != eSystemOrGateDescType.Trap_Gate_16
                         && mProc.mIDTCache[Op1Value.OpWord].GateType != eSystemOrGateDescType.Trap_Gate_32
                         && mProc.mIDTCache[Op1Value.OpWord].GateType != eSystemOrGateDescType.Task_Gate))
                    
                    {   //#GP((DEST ∗ 8) + 2 + EXT)");
                        //Decimal 13 (GP)
                        mProc.ExceptionNumber = 13;
                        mProc.ExceptionErrorCode = Op1Value.OpDWord * 8 + 2;
                        return;
                    }

                    lIDT = mProc.mIDTCache[Op1Value.OpWord];
                    if ( (lIDT.Descriptor_PL < mProc.regs.CPL) && (mProc.ExceptionNumber==0) && (!mProc.ServicingIRQ))
                        //(* PE=1, DPL<CPL, software interrupt *)
                    {   //#GP((vector_number ∗ 8) + 2 + EXT)");
                        //Decimal 13 (GP)
                        mProc.ExceptionNumber = 13;
                        mProc.ExceptionErrorCode = Op1Value.OpDWord * 8 + 2;
                        return;
                    }
                    //if (!lGDT.access.Present)
                    //    throw new Exception("Code #NP((vector_number ∗ 8) + 2 + EXT)");
                    if (mProc.mIDTCache[Op1Value.OpWord].GateType == eSystemOrGateDescType.Task_Gate)
                    {
                        throw new Exception("Code TASK-GATE");
                    }
                    else
                    {   //TRAP-OR-INTERRUPT-GATE (* PE=1, trap/interrupt gate *)
                        if (lIDT.GDTCachePointer == 0)
                            throw new Exception("#GP(0H + EXT)");
                        if (lIDT.GDTCachePointer > mProc.mGDTCache.Count-1)
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
                            if (!mProc.regs.FLAGSB.VM)
                            //throw new Exception("Code INTER-PRIVILEGE-LEVEL-INTERRUPT"); //(* PE=1, interrupt or trap gate, nonconforming *)
                            {
                                //INTER-PRIVILEGE-LEVEL-INTERRUPT; (* PE=1, interrupt or trap gate, nonconforming *)


                                if (mProc.mCurrTSS.TSS_is_32bit)
                                {
                                    mProc.regs.SS.Value = mProc.mCurrTSS.SS0;
                                    mProc.regs.ESP = mProc.mCurrTSS.ESP0;
                                }
                                

                                mProc.mOverrideStackSize = true;
                                if (lIDT.GateSize32)
                                    mProc.mOverriddenStackSizeIs16 = false;
                                else
                                    mProc.mOverriddenStackSizeIs16 = true;
                                lPush.Op1Value.OpDWord = lTempSS; lPush.Impl();
                                lPush.Op1Value.OpDWord = lTempESP; lPush.Impl();
                                lPush.Op1Value.OpDWord = mProc.regs.EFLAGS; lPush.Impl();
                                if (mProc.ExceptionNumber > 0 || Op1Value.OpWord == 0x14)
                                {
                                    lPush.Op1Add = mProc.RCS; lPush.Impl(); lPush.Op1Add = 0x0;
                                    //No need to subtract bytes used, already did that in the page fault handler
                                    lPush.Op1Value.OpDWord = mProc.regs.EIP/* - mProc.mCurrentInstruction.DecodedInstruction.BytesUsed*/; lPush.Impl();
                                    lPush.Op1Value.OpDWord = mProc.ExceptionErrorCode;
                                    lPush.Impl();
                                    if (mProc.OpSize16Stack)
                                    {
                                        lPush.Op1Value.OpWord = 0;
                                        lPush.Impl();
                                    }
                                }
                                else
                                {
                                    lPush.Op1Add = mProc.RCS; lPush.Impl(); lPush.Op1Add = 0x0;
                                    lPush.Op1Value.OpDWord = mProc.regs.EIP; lPush.Impl();
                                }
                                mProc.regs.CS.Value = lIDT.SegSelector;
                                mProc.regs.EIP = lIDT.PEP_Offset;
                                mProc.mOverrideStackSize = false;
                                mProc.ExceptionNumber = 0;
                                if (mProc.mIDTCache[Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_16 ||
                                  mProc.mIDTCache[Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_32)
                                    mProc.regs.setFlagIF(false);
                                mProc.regs.setFlagTF(false);
                                mProc.regs.setFlagNT(false);
                                mProc.regs.setFlagVM(false);
                                mProc.regs.setFlagRF(false);
                                mProc.regs.CPL = mProc.regs.CS.Selector.access.PrivLvl;
                                //mProc.regs.CS.Selector.access.PrivLvl
                                return;
                            }
                            //(* code segment, DPL<CPL, VM=0 *)
                            else  //VM=1
                                if (lGDT.access.PrivLvl != 0)
                                    throw new Exception("Code #GP(new code segment selector)");
                                else    //INTERRUPT-FROM-VIRTUAL-8086-MODE;
                                {
                                    if (lGDT.access.PrivLvl != ePrivLvl.Kernel_Ring_0)
                                        throw new Exception("INT 0x" + Op1Value.OpWord.ToString("X3") + ": GPE - Interrupt DPL != 0 on VMode Interrupt call");
                                    //INTERRUPT-FROM-VIRTUAL-8086-MODE
                                    if (mProc.mCurrTSS.TSS_is_32bit)
                                    {
                                        lNewSS = mProc.mCurrTSS.SS0;
                                        lNewESP = mProc.mCurrTSS.ESP0;
                                    }
                                    else
                                        throw new Exception("16 bit TSS not coded yet");
                                    lTempEFLAGS = mProc.regs.EFLAGS;
                                    lTempCS = mProc.regs.CS.Value;
                                    mProc.regs.setFlagVM(false);
                                    mProc.regs.setFlagTF(false);
                                    mProc.regs.setFlagRF(false);
                                    //mProc.regs.setFlagIF(false);
                                    //This call triggers the change from Virtual-8086 to Protected mode
                                    mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                                    //mProc.mOpSize16 = false;
                                    //mProc.mAddrSize16 = false;
                                    mProc.regs.SS.Value = lNewSS;
                                    mProc.regs.ESP = lNewESP;

                                    mProc.mOverrideStackSize = true;
                                    if (lIDT.GateSize32)
                                        mProc.mOverriddenStackSizeIs16 = false;
                                    else
                                        mProc.mOverriddenStackSizeIs16 = true;

                                    //Don't pass the register address because we are in Protected mode now
                                    //Passing the address would cause the selectors to be pushed instead of the register values
                                    lPush.Op1Add = 0x00;
                                    lPush.Op1Value.OpDWord = mProc.regs.GS.Value; lPush.Impl();
                                    lPush.Op1Value.OpDWord = mProc.regs.FS.Value; lPush.Impl();
                                    lPush.Op1Value.OpDWord = mProc.regs.DS.Value; lPush.Impl();
                                    lPush.Op1Value.OpDWord = mProc.regs.ES.Value; lPush.Impl();
                                    lPush.Op1Add = mProc.RSS;
                                    lPush.Op1Value.OpDWord = lTempSS; lPush.Impl();
                                    //lPush.Op1Add = mProc.RESP;
                                    lPush.Op1Value.OpDWord = lTempESP; lPush.Impl();
                                    lPush.Op1Add = 0;
                                    lPush.Op1Value.OpDWord = lTempEFLAGS; lPush.Impl();

                                    //Per IASD Vol 3, pg 162 ...
                                    //Error codes are not pushed on the stack for exceptions that are generated externally (with the
                                    //INTR or LINT[1:0] pins) or the INT n instruction, even if an error code is normally produced
                                    //for those exceptions
                                    if (mProc.ExceptionNumber > 0 || Op1Value.OpWord == 0x14)
                                    {
                                        lPush.Op1Value.OpDWord = lTempCS;lPush.Impl();
                                        lPush.Op1Value.OpDWord = mProc.regs.EIP - mProc.mCurrentInstruction.DecodedInstruction.BytesUsed; lPush.Impl();
                                        lPush.Op1Value.OpDWord = (mProc.ExceptionErrorCode * 8) + 2;
                                        lPush.Impl();
                                        if (mProc.OpSize16Stack)
                                        {
                                            lPush.Op1Value.OpWord = 0;
                                            lPush.Impl();
                                        }
                                    }
                                    else
                                    {
                                        lPush.Op1Value.OpDWord = lTempCS; lPush.Impl();
                                        lPush.Op1Value.OpDWord = mProc.regs.EIP; lPush.Impl();
                                    }

                                    mProc.regs.GS.Value = 0;
                                    mProc.regs.FS.Value = 0;
                                    mProc.regs.DS.Value = 0;
                                    mProc.regs.ES.Value = 0;

                                    mProc.regs.CS.Value = lIDT.SegSelector;
                                    mProc.regs.EIP = lIDT.PEP_Offset;
                                    mProc.mOverrideStackSize = false;
                                    mProc.ExceptionNumber = 0;
                                    return;
                                }
                        else    //(* PE=1, interrupt or trap gate, DPL ≥ CPL *)
                            if (mProc.regs.FLAGSB.VM)
                                //throw new Exception("Code #GP(new code segment selector)");
                            {
                                //Decimal 13 (GP)
                                mProc.ExceptionNumber = 13;
                                mProc.ExceptionErrorCode = lIDT.SegSelector;
                                return;
                            }

                            else if ((lGDT.access.SegType == eGDTSegType.Code_Exec_Only_Conforming
                            || lGDT.access.SegType == eGDTSegType.Code_Exec_Only_Conforming_Accessed
                            || lGDT.access.SegType == eGDTSegType.Code_Exec_RO_Conforming
                            || lGDT.access.SegType == eGDTSegType.Code_Exec_RO_Conforming_Accessed)
                            || lGDT.access.PrivLvl == mProc.regs.CPL)
                            {
                                //INTRA-PRIVILEGE-LEVEL-INTERRUPT: (* PE=1, DPL = CPL or conforming segment *)
                                mProc.mOverrideStackSize = true;
                                if (lIDT.GateSize32)
                                    mProc.mOverriddenStackSizeIs16 = false;
                                else
                                    mProc.mOverriddenStackSizeIs16 = true;
                                lPush.Op1Value.OpDWord = mProc.regs.EFLAGS; lPush.Impl();
                                if (mProc.ExceptionNumber > 0 || Op1Value.OpWord == 0x14)
                                {
                                    lPush.Op1Add = mProc.RCS; lPush.Impl(); lPush.Op1Add = 0x0;
                                    lPush.Op1Value.OpDWord = mProc.regs.EIP - mProc.mCurrentInstruction.DecodedInstruction.BytesUsed; lPush.Impl();
                                    lPush.Op1Value.OpDWord = (mProc.ExceptionErrorCode * 8) + 2;
                                    lPush.Impl();
                                    if (mProc.OpSize16Stack)
                                    {
                                        lPush.Op1Value.OpWord = 0;
                                        lPush.Impl();
                                    }
                                }
                                else
                                {
                                    lPush.Op1Add = mProc.RCS; lPush.Impl(); lPush.Op1Add = 0x0;
                                    lPush.Op1Value.OpDWord = mProc.regs.EIP; lPush.Impl();
                                }
                                mProc.regs.CS.Value = lIDT.SegSelector;
                                mProc.regs.EIP = lIDT.PEP_Offset;
                                mProc.mOverrideStackSize = false;
                                mProc.ExceptionNumber = 0;
                                if (mProc.mIDTCache[Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_16 || 
                                    mProc.mIDTCache[Op1Value.OpWord].GateType == eSystemOrGateDescType.Interrupt_Gate_32)
                                    mProc.regs.setFlagIF(false);
                                mProc.regs.setFlagTF(false);
                                mProc.regs.setFlagNT(false);
                                mProc.regs.setFlagVM(false);
                                mProc.regs.setFlagRF(false);
                                return;
                            }
                            else    //(* PE=1, interrupt or trap gate, nonconforming *)
                                    
                            {   //#GP(CodeSegmentSelector + EXT)
                                //(* code segment, DPL>CPL *)
                                mProc.ExceptionNumber = 13;
                                mProc.ExceptionErrorCode = (UInt32)lIDT.SegSelector << 3;
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.OF)
            {
                mProc.Instructions["INT"].Op1Value.OpQWord = 0x04;
                mProc.Instructions["INT"].Impl();
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
        public override void Impl()
        {
            Instruct lPop = mProc.Instructions["POP"];
            lPop.DecodedInstruction = DecodedInstruction;
            lPop.mInternalInstructionCall = false;

            UInt32 lPreIRetFlags, lTempCS, lTempEIP, lTempEFlags, lTempSS, lTempESP, lTempES, lTempDS, lTempFS, lTempGS;
            UInt16 lTempFlags = 0;

            if (mProc.OperatingMode == ProcessorMode.Real)
            {
                #region Real Mode IRET
                lPop.Operand1IsRef = true;
                lPop.Op1Add = mProc.RIP;
                lPop.Impl();

                lPop.Op1Add = mProc.RCS;
                lPop.Impl();
                lPop.Op1Add = 0x0;

                lPreIRetFlags = mProc.regs.EFLAGS;

                Instruct lPopF = mProc.Instructions["POPF"];
                lPopF.Op1Add = 0x0;
                lPopF.Impl();

                //lPop.Op1Add = mProc.RFL;
                //lPop.Impl();

                //Backwards from Intel docs because I pop directly to the flags register, not to tempEFlags
                mProc.regs.EFLAGS = (UInt32)(mProc.regs.EFLAGS & 0x257FD5) | (UInt32)(lPreIRetFlags & 0x1A0000);
                #endregion
            }
            else
            {   //RETURN-FROM-VIRTUAL-8086-MODE
                if (mProc.OperatingMode == ProcessorMode.Virtual8086)
                {
                    if (mProc.OpSize16)
                    {
                        //System.Diagnostics.Debug.WriteLine("IRET: " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.IP.ToString("X8"));
                        lPop.mInternalInstructionCall = true;
                        lPop.Impl();
                        mProc.regs.EIP = lPop.Op1Value.OpDWord;
                        lPop.Impl();
                        //Should be 16-bit pop!
                        mProc.regs.CS.Value = lPop.Op1Value.OpDWord;
                        lPop.Impl();
                        lTempFlags = (Word)(lPop.Op1Value.OpWord); // ^ 0x3000);
                        mProc.regs.FLAGS = lTempFlags;
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

                    lPop.mInternalInstructionCall = true;
                    lPop.Impl();
                    lTempEIP = lPop.Op1Value.OpDWord;

                    int a = 0;
                    if (lTempEIP == 0x0)
                        a += 1;
                    lPop.Impl();
                    lTempCS = lPop.Op1Value.OpDWord;
                    lPop.Impl();
                    lTempEFlags = lPop.Op1Value.OpDWord;

                    //RETURN-TO-VIRTUAL-8086-MODE
                    //Flags(VM) = 1
                    if ((UInt32)(lTempEFlags & 0x20000) == 0x20000 && mProc.regs.CS.Selector.access.PrivLvl == 0)
                    {
                        //lPop.Op1Add = mProc.RESP;
                        lPop.Impl();
                        lTempESP = lPop.Op1Value.OpDWord;
                        lPop.Op1Add = 0;
                        lPop.Impl();
                        lTempSS = lPop.Op1Value.OpWord;
                        lPop.Impl();
                        lTempES = lPop.Op1Value.OpWord;
                        lPop.Impl();
                        lTempDS = lPop.Op1Value.OpWord;
                        lPop.Impl();
                        lTempFS = lPop.Op1Value.OpWord;
                        lPop.Impl();
                        lTempGS = lPop.Op1Value.OpWord;
                        //Setting EFLAGS puts the processor in Virtual-8086 mode
                        mProc.regs.EFLAGS = lTempEFlags;
                        mProc.regs.FLAGSB.SetValues(mProc.regs.EFLAGS);
                        mProc.ZeroDescriptors();
                        mProc.regs.EIP = lTempEIP;
                        mProc.regs.CS.Value = lTempCS;
                        mProc.regs.SS.Value = lTempSS;
                        mProc.regs.ESP = lTempESP;
                        mProc.regs.DS.Value = lTempDS;
                        mProc.regs.ES.Value = lTempES;
                        mProc.regs.FS.Value = lTempFS;
                        mProc.regs.GS.Value = lTempGS;
                        mProc.regs.CPL = ePrivLvl.App_Level_3;
                    }
                    else    //PROTECTED-MODE-RETURN
                    {
                        if (mProc.mGDTCache[(int)lTempCS>>3].access.PrivLvl > mProc.regs.CPL)
                        {
                            mProc.regs.CPL = mProc.mGDTCache[(int)lTempCS>>3].access.PrivLvl;
                            //lPop.Op1Add = mProc.RESP;
                            lPop.Impl();
                            lTempESP = lPop.Op1Value.OpDWord;
                            lPop.Op1Add = mProc.RSS;
                            lPop.Impl();
                            lPop.Op1Add = 0;
                            lTempSS = lPop.Op1Value.OpWord;
                            mProc.regs.SS.Value = lTempSS;
                            mProc.regs.ESP = lTempESP;
                        }
                        //RETURN-TO-SAME-PRIVILEGE-LEVEL
                        mProc.regs.EIP = lTempEIP;
                        //not sure why this references a real mode register, changing it
                        //mProc.regs.CS.mRealModeValue = lTempCS << 4;

                        //Hack with a TRY
                        mProc.regs.CS.Value = lTempCS;
                        //Set the 16 bit flags
                        mProc.regs.FLAGS = (Word)lTempEFlags;
                        mProc.regs.FLAGSB.SetValues((UInt16)lTempEFlags);
                        if (!mProc.OpSize16)
                        {
                            mProc.regs.setFlagRF((lTempEFlags & 0x10000) == 0x10000);
                            mProc.regs.setFlagAC((lTempEFlags & 0x40000) == 0x40000);
                            mProc.regs.setFlagID((lTempEFlags & 0x200000) == 0x200000);
                        }
                        //If CPL <= IOPL
                        mProc.regs.setFlagIF((lTempEFlags & 0x200) == 0x200);
                        //If CPL = 0
                        mProc.regs.setFlagIOPL((UInt16)lTempEFlags);
                        if (!mProc.OpSize16)
                        {
                            mProc.regs.setFlagVM((lTempEFlags & 0x20000) == 0x20000);
                            mProc.regs.setFlagVIF((lTempEFlags & 0x80000) == 0x80000);
                            mProc.regs.setFlagVIP((lTempEFlags & 0x100000) == 0x100000);
                        }
                    }
                }
            }

            #region Instructions
            #endregion
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.CF && !mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.CF || mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.AddrSize16)
            {
                if (mProc.regs.CX == 0)
                    UpdIPForShortJump(Op1Value, Op1TypeCode);
            }
            else
                if (mProc.regs.ECX == 0)
                    UpdIPForShortJump(Op1Value, Op1TypeCode);

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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if ((!mProc.regs.FLAGSB.ZF) & (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            //09/03/2010: Semantic change to match BOCHS
            if (mProc.regs.FLAGSB.ZF || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            UInt16 lCS = 0; UInt32 lIP = 0; UInt16 lJunk = 0;
            bool lIsNegative = false;
            UInt32 lBase = 0;

            //if (ChosenOpCode.Instruction!=null && !ChosenOpCode.Operand1.StartsWith("E") && !ChosenOpCode.Operand1.StartsWith("A"))
            if (ChosenOpCode.Instruction != null && ChosenOpCode.Op1AM != sOpCodeAddressingMethod.EType && ChosenOpCode.Op1AM != sOpCodeAddressingMethod.DirectAddress)
                //Added to check for negative jump value
                lIsNegative = UpdateForNegativeAll(ref mProc, ref Op1Value, Op1TypeCode);

            if (DecodedInstruction.OpCode == 0xea && mProc.OperatingMode == ProcessorMode.Protected)
                lBase = (DWord)(lBase + 5 - 3 - 2);

            if (Operand1IsRef)
            {
                mProc.regs.EIP = Op1Value.OpDWord;
            }
            else
                switch (Op1TypeCode)
                {
                    case TypeCode.Byte:
                        if (!lIsNegative)
                            mProc.regs.EIP += Op1Value.OpByte;
                        else
                            mProc.regs.EIP -= Op1Value.OpByte;
                        break;
                    case TypeCode.UInt16:
                        if (!lIsNegative)
                            mProc.regs.EIP += Op1Value.OpWord;
                        else
                            mProc.regs.EIP -= Op1Value.OpWord;
                        break;
                    case TypeCode.UInt32:
                        if (mProc.OperatingMode == ProcessorMode.Protected)
                        {
                            if (ChosenOpCode.Op1AM == sOpCodeAddressingMethod.JmpRelOffset)
                            {
                                if (!lIsNegative)
                                    mProc.regs.EIP += (UInt32)Op1Value.OpDWord;
                                else
                                    mProc.regs.EIP -= (UInt32)Op1Value.OpDWord;
                            }
                            else
                            {
                                mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord & 0x00FFFFFF);
                                mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                            }
                        }
                        else
                        {
                            if (mProc.OpSize16 && mProc.AddrSize16)
                            {
                                mProc.regs.CS.Value = PhysicalMem.GetSegForLoc(mProc, Op1Value.OpDWord);
                                mProc.regs.EIP = PhysicalMem.GetOfsForLoc(mProc, Op1Value.OpDWord);
                            }
                            else
                            {
                                if (!lIsNegative)
                                    mProc.regs.EIP += (UInt32)Op1Value.OpDWord;
                                else
                                    mProc.regs.EIP -= (UInt32)Op1Value.OpDWord;
                                break;
                            }
                        }
                        break;
                    case TypeCode.UInt64:
                        if (mProc.OperatingMode == ProcessorMode.Protected)
                        {
                            mProc.regs.CS.Value = Misc.GetHi(Op1Value.OpQWord);
                            mProc.regs.EIP = Op1Value.OpDWord;
                            break;
                        }
                        else
                        {
                            mProc.regs.CS.Value = (UInt16)(Op1Value.OpQWord >> 32);
                            mProc.regs.EIP = Op1Value.OpDWord;
                        }
                        break;

                }
            if (!mProc.ProtectedModeActive)
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
        public override void Impl()
        {
            //TODO: Make this work correctly.  Currently the OpCode EA for JMPF (direct bytes in operand version?) points to JMP in the spreadsheet
            //so JMP is (successfully) doing this work

            if ( (DecodedInstruction.OpCode == 0xFF05 || DecodedInstruction.OpCode == 0xEA) && mProc.OperatingMode == ProcessorMode.Protected)
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

                Word lTop = (Word)(Op1Value.OpQWord >> 32);
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
                    throw new Exception("Task switch: Bad TSS Selector");
                if (!theDescriptor.access.Present)
                    throw new Exception("Task switch: TSS descriptor is not present");

                /*  STEP 2: The processor performs limit-checking on the target TSS
                        to verify that the TSS limit is greater than or equal
                        to 67h (2Bh for 16-bit TSS).*/
                if (theDescriptor.access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || theDescriptor.access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
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

                if (theDescriptor.access.SystemDescType == eSystemOrGateDescType.TSS_16_Bu || theDescriptor.access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu)
                    lOldEFlags &= 0xFFFFBFFF;

                if (mProc.regs.ESP == 0x001E5EF4)
                    mProc.regs.ESP = mProc.regs.ESP + 1 - 1;

                if (new_TSS_max == 0x67)
                {
                    mProc.mCurrTSS.EIP = mProc.regs.EIP;
                    mProc.mCurrTSS.EFLAGS = mProc.regs.EFLAGS;
                    mProc.mCurrTSS.EAX = mProc.regs.EAX;
                    mProc.mCurrTSS.ECX = mProc.regs.ECX;
                    mProc.mCurrTSS.EDX = mProc.regs.EDX;
                    mProc.mCurrTSS.EBX = mProc.regs.EBX;
                    mProc.mCurrTSS.ESP = mProc.regs.ESP;
                    mProc.mCurrTSS.EBP = mProc.regs.EBP;
                    mProc.mCurrTSS.ESI = mProc.regs.ESI;
                    mProc.mCurrTSS.EDI = mProc.regs.EDI;
                    mProc.mCurrTSS.ES = (UInt16)mProc.regs.ES.mWholeSelectorValue;
                    mProc.mCurrTSS.CS = (UInt16)mProc.regs.CS.mWholeSelectorValue;
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
                mProc.mCurrTSS.Commit(mProc);

                // Step 7: Load the task register with the segment selector and
                //        descriptor for the new task TSS.
                mProc.regs.TR.mSegSel = lTop;

                DWord lTempCR3 = mProc.mem.GetDWord(mProc, theDescriptor.Base + 28);
                //if (mProc.regs.CR3 != lTempCR3)
                    mProc.regs.CR3 = lTempCR3;
                
                LTR.Load(mProc);

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
                mProc.regs.ESP = mProc.mCurrTSS.ESP;
                mProc.regs.EBP = mProc.mCurrTSS.EBP;
                mProc.regs.ESI = mProc.mCurrTSS.ESI;
                mProc.regs.EDI = mProc.mCurrTSS.EDI;

                mProc.regs.EFLAGS = mProc.mCurrTSS.EFLAGS;
                mProc.regs.CS.Value = mProc.mCurrTSS.CS;
                mProc.regs.SS.Value = mProc.mCurrTSS.SS;
                mProc.regs.DS.Value = mProc.mCurrTSS.DS;
                mProc.regs.ES.Value = mProc.mCurrTSS.ES;
                mProc.regs.FS.Value = mProc.mCurrTSS.FS;
                mProc.regs.GS.Value = mProc.mCurrTSS.GS;
                mProc.regs.LDTR.mSegSel = mProc.mCurrTSS.LDT_SegSel;
                LLDT.Load(mProc);

                /* set CPL to 3 to force a privilege level change and stack switch if SS
                is not properly loaded */
                //mProc.regs.CPL = 3;
                return;
            }


            if (mProc.OperatingMode == ProcessorMode.Protected)
                mProc.mem.SetDWord(mProc, mProc.RCS, PhysicalMem.GetSegForLoc(mProc, (UInt32)Op2Value.OpWord));
            else
                mProc.regs.CS.Value = Op1Value.OpDWord >> 0x10;
            mProc.regs.EIP = Op1Value.OpWord;
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
        public override void Impl()
        {
            if ((mProc.regs.FLAGSB.ZF) || (mProc.regs.FLAGSB.CF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.CF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            //09/03/10: Changed per BOCHS ... semantic only no logical change
            if ((!mProc.regs.FLAGSB.CF && !mProc.regs.FLAGSB.ZF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if ((!mProc.regs.FLAGSB.CF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if ((!mProc.regs.FLAGSB.ZF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if ((mProc.regs.FLAGSB.ZF) || (mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if ((mProc.regs.FLAGSB.SF != mProc.regs.FLAGSB.OF))
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            //9/1/10: changed to match BOCHS instruction
            if ((!mProc.regs.FLAGSB.ZF && (mProc.regs.FLAGSB.SF == mProc.regs.FLAGSB.OF)))
                UpdIPForShortJump(Op1Value, Op1TypeCode);

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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.SF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.OF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (!mProc.regs.FLAGSB.PF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);

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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.SF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
        {
            if (mProc.regs.FLAGSB.ZF)
                UpdIPForShortJump(Op1Value, Op1TypeCode);
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
        public override void Impl()
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
                AH ← EFLAGS(SF:ZF:0:AF:0:PF:1:CF);
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
        public override void Impl()
        {
            if (mProc.OpSize16)
                mProc.mem.SetWord(mProc, Op1Add, (Word)Op2Add);
            else
                if (mProc.AddrSize16)
                    mProc.mem.SetDWord(mProc, Op1Add, (Word)Op2Add);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, Op2Add);
        }
    }
    public class LEAVE : Instruct
    {
        public LEAVE()
        {
            mName = "LEAVE";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "High Level Procedure Exit";
            mModFlags = 0;
        }
        public override void Impl()
        {
            UInt32 lTempEBP = mProc.regs.EBP;
            UInt16 lTempBP = mProc.regs.BP;
            UInt32 lPreESP = mProc.regs.ESP;

            Instruct lPop = mProc.Instructions["POP"];
            lPop.DecodedInstruction = DecodedInstruction;
            lPop.mInternalInstructionCall = false;
            lPop.Op1Add = 0x0;
            if (mProc.OpSize16Stack)
            {
                mProc.regs.SP = mProc.regs.BP;
                lPop.Op1Add = mProc.RBP;
                lPop.Operand1IsRef = true;
                lPop.Impl();
            }
            else
            {
                mProc.regs.ESP = mProc.regs.EBP;
                lPop.Op1Add = mProc.REBP;
                lPop.Operand1IsRef = true;
                lPop.Impl();

                if (mProc.regs.ESP != lTempEBP + 4)
                    throw new Exception("Huh?");
            }
#if DEBUG
            if (mProc.regs.ESP == 0x0)
                System.Diagnostics.Debugger.Break();
#endif
            //Debug.WriteLine("After LEAVE, SP=" + OpDecoder.ValueToHex(mProc.regs.SP, 4) + ", BP=" + OpDecoder.ValueToHex(mProc.regs.BP, 4));
            #region Instructions
            /*
             Operation
                IF StackAddressSize = 32
                THEN
                ESP ← EBP;
                ELSE (* StackAddressSize = 16*)
                SP ← BP;
                FI;
                IF OperandSize = 32
                THEN
                EBP ← DoPop();
                ELSE (* OperandSize = 16*)
                BP ← DoPop();
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
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                mProc.mem.SetWord(mProc, Op1Add, mProc.mem.GetWord(mProc, Op2Add));
                mProc.regs.DS.Value = mProc.mem.GetWord(mProc, Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, Op1Add, mProc.mem.GetDWord(mProc, Op2Add));
                mProc.regs.DS.Value = mProc.mem.GetWord(mProc, Op2Add + 4);
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
        public override void Impl()
        {
            //TODO: Update for protected mode 32 bit
            if (mProc.OpSize16)
            {
                mProc.mem.SetWord(mProc, Op1Add, mProc.mem.GetWord(mProc, Op2Add));
                mProc.regs.ES.Value = mProc.mem.GetWord(mProc, Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, Op1Add, mProc.mem.GetDWord(mProc, Op2Add));
                mProc.regs.ES.Value = mProc.mem.GetWord(mProc, Op2Add + 4);
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
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                mProc.mem.SetWord(mProc, Op1Add, mProc.mem.GetWord(mProc, Op2Add));
                mProc.regs.FS.Value = mProc.mem.GetWord(mProc, Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, Op1Add, mProc.mem.GetDWord(mProc, Op2Add));
                mProc.regs.FS.Value = mProc.mem.GetWord(mProc, Op2Add + 4);
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
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                mProc.mem.SetWord(mProc, Op1Add, mProc.mem.GetWord(mProc, Op2Add));
                mProc.regs.GS.Value = mProc.mem.GetWord(mProc, Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, Op1Add, mProc.mem.GetDWord(mProc, Op2Add));
                mProc.regs.GS.Value = mProc.mem.GetWord(mProc, Op2Add + 4);
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
        public override void Impl()
        {
            if (Op1Value.OpQWord != mProc.regs.GDTR.mOriginalValue)
            {
                mProc.regs.GDTR.Parse(mProc, Op1Value.OpQWord);
                if (mProc.OperatingMode != ProcessorMode.Real)
                    mProc.regs.GDTR.mBase = mProc.mem.PagedMemoryAddress(mProc, mProc.regs.GDTR.Base);

            }
            mProc.RefreshGDTCache();
            mProc.RefreshLDTCache();
            mProc.RefreshIDTCache();
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
        public override void Impl()
        {
            if (Op1Value.OpQWord != mProc.regs.IDTR.mOriginalValue)
            {
                mProc.regs.IDTR.Parse(mProc, Op1Value.OpQWord);
                if (mProc.OperatingMode != ProcessorMode.Real)
                    mProc.regs.IDTR.mBase = mProc.mem.PagedMemoryAddress(mProc, mProc.regs.IDTR.Base);
            }
            mProc.RefreshIDTCache();
            int a = 0;
            if (mProc.regs.IDTR.Base == 0x0012f780)
                a += 1;
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
        public static void Load(Processor_80x86 mProc)
        {
            mProc.regs.LDTR.mBase = mProc.mGDTCache[mProc.regs.LDTR.mSegSel >> 3].Base;
                if (mProc.OperatingMode != ProcessorMode.Real && mProc.regs.LDTR.mSegSel != 0x0)
                    mProc.regs.LDTR.mBase = mProc.mem.PagedMemoryAddress(mProc, mProc.regs.LDTR.Base);
                mProc.regs.LDTR.mLimit = mProc.mGDTCache[mProc.regs.LDTR.mSegSel >> 3].Limit;
            mProc.RefreshLDTCache();
        }
        public override void Impl()
        {
            mProc.regs.LDTR.mSegSel = Op1Value.OpWord;
            Load(mProc);
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
        public override void Impl()
        {
            mProc.regs.CR0 |= (byte)(Op1Value.OpWord & 0x0f);
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
            mDescription = "Load String GetByte";
            mModFlags = 0;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;

            //if (lAddrSize16)
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);
            //else
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.ESI);

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, lAddrSize16);
            do
            {
                mProc.regs.AL = mProc.mem.GetByte(mProc, lSource);
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, lAddrSize16, 1, true, false, true, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;

            //if (mProc.AddrSize16)
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
            mDescription = "Load String GetWord";
            mModFlags = 0;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;

            if (!mProc.OpSize16)
            {
                mProc.Instructions["LODSD"].Impl();
                return;
            }
            
            //if (lAddrSize16)
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);
            //else
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.ESI);

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, lAddrSize16);
            do
            {
                mProc.regs.AX = mProc.mem.GetWord(mProc, lSource);
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, lAddrSize16, 2, true, false, true, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    //if (mProc.AddrSize16)
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
            mDescription = "Load String GetDWord";
            mModFlags = 0;
        }
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;

            //if (lAddrSize16)
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);
            //else
            //    lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.ESI);

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            MOVSW.SetupMOVSSrcDest(mProc, ref lSource, ref lJunk, lAddrSize16);
            do
            {
                mProc.regs.EAX = mProc.mem.GetDWord(mProc, lSource);
                MOVSW.IncDec(mProc, ref lJunk, ref lSource, lAddrSize16, 4, true, false, true, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
            REPAble = true;
            mDescription = "Decrement CX and Loop if CX Not Zero ";
            mModFlags = 0;
        }
        public override void Impl()
        {
            if (mProc.mAddrSize16)
            {
                if (--mProc.regs.CX != 0)
                    UpdIPForShortJump(Op1Value, Op1TypeCode);
            }
            else
                if (--mProc.regs.ECX != 0)
                    UpdIPForShortJump(Op1Value, Op1TypeCode);


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
            REPAble = true;
            mDescription = "Loop While Equal";
            mModFlags = 0;
        }
        public override void Impl()
        {
            if (mProc.mAddrSize16)
            {
                if ((--mProc.regs.CX != 0) & (mProc.regs.FLAGSB.ZF))
                    UpdIPForShortJump(Op1Value, Op1TypeCode);
            }
            else
                if ((--mProc.regs.ECX != 0) & (mProc.regs.FLAGSB.ZF))
                    UpdIPForShortJump(Op1Value, Op1TypeCode);

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
            mDescription = "Loop While Zero";
            mModFlags = 0;
        }
        public override void Impl()
        {
            mProc.Instructions["LOOPE"].ChosenOpCode = ChosenOpCode;
            mProc.Instructions["LOOPE"].Op1Value = Op1Value;
            mProc.Instructions["LOOPE"].Op1TypeCode = Op1TypeCode;
            mProc.Instructions["LOOPE"].Operand1IsRef = Operand1IsRef;
            mProc.Instructions["LOOPE"].Impl();
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
            mDescription = "Loop While Not Equal";
            mModFlags = 0;
        }
        public override void Impl()
        {
            if (mProc.mAddrSize16)
            {
                if ((--mProc.regs.CX != 0) && (!mProc.regs.FLAGSB.ZF))
                {
                    UpdIPForShortJump(Op1Value, Op1TypeCode);
                }
            }
            else
                if ((--mProc.regs.ECX != 0) && (!mProc.regs.FLAGSB.ZF))
                {
                    UpdIPForShortJump(Op1Value, Op1TypeCode);
                }

                //UpdIPForShortJump(Op1Val);
                
            #region Instructions
            #endregion
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
            REPAble = true;
            mDescription = "Loop While Not Equal";
            mModFlags = 0;
        }
        public override void Impl()
        {
            mProc.Instructions["LOOPNE"].ChosenOpCode = ChosenOpCode;
            mProc.Instructions["LOOPNE"].Op1Value = Op1Value;
            mProc.Instructions["LOOPNE"].Op1TypeCode = Op1TypeCode;
            mProc.Instructions["LOOPNE"].Operand1IsRef = Operand1IsRef;
            mProc.Instructions["LOOPNE"].Impl();
            //new LOOPNE().Impl(ref mProc, Op1Val);
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
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                mProc.mem.SetWord(mProc, Op1Add, mProc.mem.GetWord(mProc, Op2Add));
                mProc.regs.SS.Value = mProc.mem.GetWord(mProc, Op2Add + 2);
            }
            else
            {
                mProc.mem.SetDWord(mProc, Op1Add, mProc.mem.GetDWord(mProc, Op2Add));
                mProc.regs.SS.Value = mProc.mem.GetWord(mProc, Op2Add + 4);
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
        public static void Load(Processor_80x86 mProc)
        {
            mProc.regs.TR.mBase = mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].Base;
            mProc.regs.TR.mLimit = mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].Limit;
            mProc.regs.TR.lCache = mProc.mem.Chunk(mProc, 0, mProc.regs.TR.mBase, 104);
            mProc.mCurrTSS = new sTSS(mProc.mem.Chunk(mProc, 0, mProc.regs.TR.mBase, 104),
                mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu);
        }
        public override void Impl()
        {
            mProc.regs.TR.mSegSel = Op1Value.OpWord;
            Load(mProc);
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
        public override void Impl()
        {
            if ((Op2Add >= mProc.REGADDRBASE + mProc.RCSOFS) && mProc.OperatingMode == ProcessorMode.Protected)
            {
                if (mProc.OpSize16)
                    Op2Value.OpWord = (UInt16)Misc.GetSelectorForSegment(mProc, Op2Add, Op2Value.OpWord);
                else
                    Op2Value.OpDWord = Misc.GetSelectorForSegment(mProc, Op2Add, Op2Value.OpDWord);
            }

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, Op1Add, Op2Value.OpByte);
                    return;
                case TypeCode.UInt16:
                    switch (Op2TypeCode)
                    {
                        case TypeCode.Byte: mProc.mem.SetByte(mProc, Op1Add, Op2Value.OpByte); break;
                        default: mProc.mem.SetWord(mProc, Op1Add, Op2Value.OpWord); break;
                    }
                    return;
                case TypeCode.UInt32:
                    switch (Op2TypeCode)
                    {
                        case TypeCode.Byte: mProc.mem.SetByte(mProc, Op1Add, Op2Value.OpByte); break;
                        case TypeCode.UInt16: mProc.mem.SetWord(mProc, Op1Add, Op2Value.OpWord); break;
                        default: mProc.mem.SetDWord(mProc, Op1Add, Op2Value.OpDWord); break;
                    }
                    return;
                case TypeCode.UInt64:
                    switch (Op2TypeCode)
                    {
                        case TypeCode.Byte: mProc.mem.SetByte(mProc, Op1Add, Op2Value.OpByte); break;
                        case TypeCode.UInt16: mProc.mem.SetWord(mProc, Op1Add, Op2Value.OpWord); break;
                        case TypeCode.UInt32: mProc.mem.SetDWord(mProc, Op1Add, Op2Value.OpDWord); break;
                        default: mProc.mem.SetQWord(mProc, Op1Add, Op2Value.OpQWord); break;
                    }
                    return;
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            bool lAddrSize16 = mProc.AddrSize16;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }

            MOVSW.SetupMOVSSrc(mProc, ref lSource, lAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);

            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetByte(mProc,lDest,mProc.mem.GetByte(mProc,lSource));
                MOVSW.IncDec(mProc, ref lDest, ref lSource, lAddrSize16, 1, true, true, true, false);

                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
                //System.Diagnostics.Debug.WriteLine(mProc.regs.CX.ToString("X") + ", SI:" + mProc.regs.SI.ToString("X") + ", " + mProc.regs.DI.ToString("X"));
            }
            while (--mProc.mLoopCount > 0);
            if (mProc.OpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

            #region Instructions
            #endregion
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            bool lAddrSize16 = mProc.AddrSize16;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }

            if (!mProc.OpSize16)
            {
                mProc.Instructions["MOVSD"].Impl();
                return;
            }

            MOVSW.SetupMOVSSrc(mProc, ref lSource, lAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);

            //if (lDest == 0x00118000)
            //    System.Diagnostics.Debugger.Break();
            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetWord(mProc,lDest,mProc.mem.GetWord(mProc,lSource));
                MOVSW.IncDec(mProc, ref lDest, ref lSource, lAddrSize16, 2,true, true, true, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (mProc.OpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

             #region Instructions
            #endregion
        }
        public static void IncDec(Processor_80x86 mProc, ref DWord Dest, ref DWord Src, bool AddrSize16, byte Size, bool IncSource, bool IncDest, bool SourceOverridable, bool DestOverridable)
        {
            DWord Junk = 0;

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

            if (IncSource && ((AddrSize16 && (mProc.regs.SI >= 0xFFFE || mProc.regs.SI <= 0x001)) || (!AddrSize16 && (mProc.regs.ESI >= 0xFFFFFFFE || mProc.regs.ESI <= 0x001))))
                SetupMOVSSrc(mProc, ref Src, AddrSize16, SourceOverridable);

            if (IncDest && ((AddrSize16 && (mProc.regs.DI >= 0xFFFE || mProc.regs.DI <= 0x001)) || (!AddrSize16 && (mProc.regs.EDI >= 0xFFFFFFFE || mProc.regs.EDI <= 0x001))))
                SetupMOVSDest(mProc, ref Dest, AddrSize16, DestOverridable);

        }
        public static void SetupMOVSSrcDest(Processor_80x86 mProc, ref DWord Source, ref DWord Dest, bool AddrSize16)
        {
            if (mProc.mSegmentOverride == mProc.RSS)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RES)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RCS)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.ESI);
            else
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.ESI);
            if (AddrSize16)
                Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
            else
                Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
        }
        public static void SetupMOVSSrc(Processor_80x86 mProc, ref DWord Source, bool AddrSize16, bool Overrideable)
        {
            if (mProc.mSegmentOverride == mProc.RSS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RES && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RFS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.FS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.FS.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RGS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.GS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.GS.Value, mProc.regs.ESI);
            else if (mProc.mSegmentOverride == mProc.RCS && Overrideable)
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.ESI);
            else
                if (AddrSize16)
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);
                else
                    Source = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.ESI);
        }
        public static void SetupMOVSDest(Processor_80x86 mProc, ref DWord Dest, bool AddrSize16, bool Overrideable)
        {
            if (mProc.mSegmentOverride == mProc.RSS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == mProc.RES && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == mProc.RFS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.FS.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.FS.Value, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == mProc.RGS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.GS.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.GS.Value, mProc.regs.EDI);
            else if (mProc.mSegmentOverride == mProc.RCS && Overrideable)
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.EDI);
            else
                if (AddrSize16)
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI);
                else
                    Dest = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI);
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            bool lAddrSize16 = mProc.AddrSize16;
            UInt32 lRealSegOver = mProc.mSegmentOverride;

            MOVSW.SetupMOVSSrc(mProc, ref lSource, lAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);

            do
            {
                //8/28/10 - Per Intel, DS can be overwritten, ES cannot, changing for this reason
                mProc.mem.SetDWord(mProc,lDest, mProc.mem.GetDWord(mProc,lSource));
                MOVSW.IncDec(mProc, ref lDest, ref lSource, lAddrSize16, 4, true,true, true, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT) || (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (mProc.OpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
        public override void Impl()
        {
            if (this.ChosenOpCode.OpCode == 0x0FBE)
                if (mProc.OpSize16)
                    mProc.mem.SetWord(mProc, Op1Add, Misc.SignExtend(Op2Value.OpByte));
                else
                    mProc.mem.SetDWord(mProc, Op1Add, Misc.SignExtend(Misc.SignExtend(Op2Value.OpByte)));
            else
                mProc.mem.SetDWord(mProc, Op1Add, Misc.SignExtend(Op2Value.OpWord));

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
        public override void Impl()
        {
            if (this.ChosenOpCode.OpCode == 0x0FB6)
                if (mProc.OpSize16)
                    mProc.mem.SetWord(mProc, Op1Add, Op2Value.OpByte);
                else
                    mProc.mem.SetDWord(mProc, Op1Add, Op2Value.OpByte);
            else
                mProc.mem.SetDWord(mProc, Op1Add, Op2Value.OpWord);

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
        public override void Impl()
        {
            UInt32 lRes32 = 0;
            UInt64 lRes64 = 0;
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.regs.AX = (UInt16)((UInt16)mProc.regs.AL * (UInt16)Op1Value.OpByte);
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
                    lRes32 = (UInt32)((UInt32)mProc.regs.AX * (UInt32)Op1Value.OpWord);
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
                    lRes64 = (UInt64)((UInt64)mProc.regs.EAX * (UInt64)Op1Value.OpQWord);
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
        public override void Impl()
        {
            sOpVal lOp1Val = Op1Value, lPreVal1 = Op1Value;

            mProc.regs.setFlagCF(Op1Value.OpQWord > 0);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Val.OpByte = (byte)-(lOp1Val.OpByte);
                    mProc.mem.SetByte(mProc,Op1Add, lOp1Val.OpByte);
                    mProc.regs.setFlagOF(lPreVal1.OpByte, (byte)-(lOp1Val.OpByte), lOp1Val.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Val.OpWord = (Word)(-(lOp1Val.OpWord));
                    mProc.mem.SetWord(mProc,Op1Add, lOp1Val.OpWord);
                    mProc.regs.setFlagOF(lPreVal1.OpWord, (Word)(-(lOp1Val.OpWord)), lOp1Val.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Val.OpDWord = (DWord)(-(lOp1Val.OpDWord));
                    mProc.mem.SetDWord(mProc,Op1Add, lOp1Val.OpDWord);
                    mProc.regs.setFlagOF(lPreVal1.OpDWord, (Word)(-(lOp1Val.OpDWord)), lOp1Val.OpDWord);
                    break;
                default:
                    throw new Exception("Cannot negate a quad word");
            }

            SetFlagsForSubtraction(mProc, lPreVal1, Op1Value, lOp1Val, Op1TypeCode, false, false);
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
        public override void Impl()
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lOp1Value = Op1Value;

            //Do the operation
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte = (byte)(~lOp1Value.OpByte);
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    return;
                case TypeCode.UInt16:
                    lOp1Value.OpWord = (Word)(~lOp1Value.OpWord);
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    return;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord = (DWord)(~lOp1Value.OpDWord);
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    return;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord = (QWord)(~lOp1Value.OpQWord);
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = Op2Value;
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);
            //Do the operation
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte |= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord |= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord |= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord |= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
        public override void Impl()
        {

            switch (this.DecodedInstruction.RealOpCode)
            {
                case 0xE6:
                    mProc.ports.Out(Op1Value.OpByte, Op2Value.OpByte);
                    return;
                case 0xE7:
                    if (mProc.OpSize16)
                        mProc.ports.Out(Op1Value.OpByte, Op2Value.OpWord);
                    else
                        mProc.ports.Out(Op1Value.OpByte, Op2Value.OpDWord);
                    return;
                case 0xEE:
                    mProc.ports.Out(Op1Value.OpWord, Op2Value.OpByte);
                    return;
                case 0xEF:
                    if (mProc.OpSize16)
                        mProc.ports.Out(Op1Value.OpWord, Op2Value.OpWord);
                    else
                        mProc.ports.Out(Op1Value.OpWord, Op2Value.OpDWord);
                    return;
            }

            switch (Op2TypeCode)
            {
                case TypeCode.Byte:
                    break;
                case TypeCode.UInt16:
                    break;
                case TypeCode.UInt32:
                    break;
                case TypeCode.UInt64:
                    break;
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
            mProc8086 = true;
            mProc8088 = true;
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
        public override void Impl()
        {
            DWord lSource;
            if (mProc.mSegmentOverride == mProc.RSS)
                lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SI);
            else if (mProc.mSegmentOverride == mProc.RES)
                lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.SI);
            else if (mProc.mSegmentOverride == mProc.RCS)
                lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.CS.Value, mProc.regs.SI);
            else
                lSource = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.SI);

            if (Op1Val == null)
                Op1Val = new cValue((Word)mProc.regs.DX);
            if (DecodedInstruction.RealOpCode == 0x6e)
            {
                mProc.ports.Out(Op1Val, new cValue((byte)mProc.mem.GetByte(mProc,lSource)));
                if (mProc.regs.FLAGSB.DF == false)
                    if (mProc.AddrSize16)
                        mProc.regs.SI += 1;
                    else
                        mProc.regs.ESI += 1;
                else
                    if (mProc.AddrSize16)
                        mProc.regs.SI -= 1;
                    else
                        mProc.regs.ESI -= 1;
            }
            else //if (mProc.mCurrentInstruction.ChosenOpCode.OpCode == 0x6f)
            {
                if (mProc.AddrSize16)
                {
                    mProc.ports.Out(new cValue(mProc.regs.DX), new cValue((Word)mProc.mem.GetWord(mProc,lSource)));
                }
                else
                {
                    mProc.ports.Out(new cValue(mProc.regs.DX), new cValue((DWord)mProc.mem.GetDWord(mProc,lSource)));
                }
                if (mProc.regs.FLAGSB.DF == false)
                    if (mProc.AddrSize16)
                        mProc.regs.SI += 2;
                    else
                        mProc.regs.ESI += 4;
                else
                    if (mProc.AddrSize16)
                        mProc.regs.SI -= 2;
                    else
                        mProc.regs.ESI -= 4;

            }

            ////switch (Op2Val.GetTypeCode())
            ////{
            ////    case TypeCode.Byte:
            ////        if (mProc.regs.FLAGSB.DF == false)
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI += 1;
            ////            else
            ////                mProc.regs.ESI += 1;
            ////        else
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI -= 1;
            ////            else
            ////                mProc.regs.ESI -= 1;
            ////        break;
            ////    case TypeCode.UInt16:
            ////        if (mProc.regs.FLAGSB.DF == false)
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI += 2;
            ////            else
            ////                mProc.regs.ESI += 2;
            ////        else
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI -= 2;
            ////            else
            ////                mProc.regs.ESI -= 2;
            ////        break;
            ////    case TypeCode.UInt32:
            ////        if (mProc.regs.FLAGSB.DF == false)
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI += 4;
            ////            else
            ////                mProc.regs.ESI += 4;
            ////        else
            ////            if (mProc.AddrSize16)
            ////                mProc.regs.SI -= 4;
            ////            else
            ////                mProc.regs.ESI -= 4;
            ////        break;
            ////    default:
            ////        throw new ExceptionNumber("d'oh!");
            //}
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
            mDescription = "POP GetWord Off the Stack";
            mModFlags = 0;
        }
        public override void Impl()
        {
            bool lOpSize16 = mProc.OpSize16Stack;
            bool lAddrSize16 = mProc.AddrSize16Stack;

            if (lAddrSize16)
                if (lOpSize16)
                    Op1Value.OpWord = mProc.mem.GetWord(mProc,PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SP));
                else
                    Op1Value.OpDWord = mProc.mem.GetDWord(mProc,PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SP));
            else
                if (lOpSize16)
                    Op1Value.OpWord = mProc.mem.GetWord(mProc,PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESP));
                else
                    Op1Value.OpDWord = mProc.mem.GetDWord(mProc,PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESP));

            if (Op1Add != mProc.RSP)
                if (lAddrSize16)
                    if (lOpSize16)
                        mProc.regs.SP += 2;
                    else
                        mProc.regs.SP += 4;
                else
                    if (lOpSize16)
                        mProc.regs.ESP += 2;
                    else
                        mProc.regs.ESP += 4;


            if (DecodedInstruction.Op1.EffReg1 == eGeneralRegister.SP || DecodedInstruction.Op1.EffReg1 == eGeneralRegister.ESP
            || DecodedInstruction.Op1.EffReg2 == eGeneralRegister.SP || DecodedInstruction.Op1.EffReg2 == eGeneralRegister.ESP)
            {
                if (lAddrSize16)
                    if (lOpSize16)
                        Op1Add += 2;
                    else
                        Op1Add += 4;
                else
                    if (lOpSize16)
                        Op1Add += 2;
                    else
                        Op1Add += 4;
            }
            
            //Called by IRET for example ... we just want the value back in Op1Val
            if (this.mInternalInstructionCall)
                return;

            if (lOpSize16)
                mProc.mem.SetWord(mProc,Op1Add, Op1Value.OpWord);
            else
                mProc.mem.SetDWord(mProc, Op1Add, Op1Value.OpDWord);

            if (Op1Add == mProc.RSP && lOpSize16)
            {
                mProc.regs.ESP &= 0x0000FFFF;
                //mProc.regs.SP += 2;
            }

            //if (mProc.regs.ESP == 0x0)
            //    System.Diagnostics.Debugger.Break();

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
            mDescription = "DoPop All Registers off Stack (80188)";
            mModFlags = 0;
        }
        public override void Impl()
        {
            if (mProc.OpSize16)
            {
                Instruct lPop = mProc.Instructions["POP"];
                lPop.DecodedInstruction = DecodedInstruction;
                lPop.mInternalInstructionCall = false;
                lPop.Operand1IsRef = true;
                lPop.Op1Add = mProc.RDI;  lPop.Impl();
                lPop.Op1Add = mProc.RSI; lPop.Impl();
                lPop.Op1Add = mProc.RBP; lPop.Impl(); 
                mProc.regs.SP += 2;
                lPop.Op1Add = mProc.RBX; lPop.Impl();
                lPop.Op1Add = mProc.RDX; lPop.Impl();
                lPop.Op1Add = mProc.RCX; lPop.Impl();
                lPop.Op1Add = mProc.RAX; lPop.Impl();
            }
            else
            {
                Instruct lPop = mProc.Instructions["POP"];
                lPop.DecodedInstruction = DecodedInstruction;
                lPop.Operand1IsRef = true;
                lPop.Op1Add = mProc.REDI; lPop.Impl();
                lPop.Op1Add = mProc.RESI; lPop.Impl();
                lPop.Op1Add = mProc.REBP; lPop.Impl();
                mProc.regs.SP += 4;
                lPop.Op1Add = mProc.REBX; lPop.Impl();
                lPop.Op1Add = mProc.REDX; lPop.Impl();
                lPop.Op1Add = mProc.RECX; lPop.Impl();
                lPop.Op1Add = mProc.REAX; lPop.Impl();
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
            mDescription = "DoPop Flags off Stack ";
            mModFlags = 0;
        }
        public override void Impl()
        {
            Instruct lPop = mProc.Instructions["POP"];
            lPop.DecodedInstruction = DecodedInstruction;
            DWord lTempFlags, lPrePopFlags;

            lPop.mInternalInstructionCall = true;
            lPop.Operand1IsRef = false;
            lPop.Impl();
            lTempFlags = lPop.Op1Value.OpDWord;

            //09/04/10: Updated to handle 32 bit POP (removed old code)
            //10/08/10: Updated to handle 16 & 32 bit the same ... POP decides what size data is popped, and the offsets for RFL & REFL are the same
            if (mProc.OperatingMode == ProcessorMode.Virtual8086)
            {
                if (mProc.regs.FLAGSB.IOPL == 3)
                {
                    if (mProc.OpSize16)
                    {
                        //Keep only the following flag ... IOPL
                        lPrePopFlags = (Word)(mProc.regs.FLAGS & 0x3000);
                        mProc.regs.FLAGS = (Word)(lTempFlags & 0x4FFF);
                        mProc.regs.FLAGS |= (Word)lPrePopFlags;
                    }
                    else
                    {
                        //Keep only the following flags ... VM, RF, IOPL, VIP, VIF & Reserved bit 15
                        lPrePopFlags = (DWord)(mProc.regs.EFLAGS & 0x1BB000);
                        mProc.regs.EFLAGS = lTempFlags & 0x244FFF;
                        mProc.regs.EFLAGS |= lPrePopFlags;
                    }
                }
                else
                {
                    throw new Exception("GP(0): Need trap to virtual-8086 monitor");
                }
            }
            else
            {
                if (mProc.regs.CS.Selector.access.PrivLvl == 0)
                {
                    if (mProc.OpSize16)
                    {
                        mProc.regs.FLAGS = (Word)lTempFlags;
                    }
                    else
                    {
                        lPrePopFlags = (DWord)(mProc.regs.EFLAGS & 0x1A0000);
                        mProc.regs.EFLAGS = lTempFlags & 0x25FFFF;
                        mProc.regs.EFLAGS |= lPrePopFlags;
                    }
                }
                else //IOPL > 0
                    if (mProc.OpSize16)
                    {
                        //Keep only the following flag ... IOPL
                        lPrePopFlags = (Word)(mProc.regs.FLAGS & 0x3000);
                        mProc.regs.FLAGS = (Word)(lTempFlags & 0x3D7FFF);
                        mProc.regs.FLAGS |= (Word)lPrePopFlags;
                        mProc.regs.setFlagVIP(false);
                        mProc.regs.setFlagVIF(false);
                    }
                    else
                    {
                        lPrePopFlags = (Word)(mProc.regs.FLAGS & 0x2B000);
                        mProc.regs.EFLAGS = lTempFlags & 0x3D4FFF;
                        mProc.regs.EFLAGS |= (DWord)lPrePopFlags;
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
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Push GetWord Onto the Stack";
            mModFlags = 0;
        }
        public override void Impl()
        {
            bool lOpSize16 = mProc.OpSize16Stack;
            bool lAddrSize16 = mProc.AddrSize16Stack;
            UInt32 lESP = mProc.regs.ESP;
            UInt16 lSP = mProc.regs.SP;

            //If other instructions call this one (like CALL) then we only want to use the 16 bit registers
            if (mInternalInstructionCall)
                lAddrSize16 = true;

            UInt16 lTemp16 = Op1Value.OpWord;
            UInt32 lTemp32 = Op1Value.OpDWord;

            if (Operand1IsRef && Op1Add == mProc.RSP)  //SP & ESP have the same location
                if (lOpSize16)
                    lTemp16 = mProc.regs.SP;
                else
                    lTemp32 = mProc.regs.ESP;
            if ((Op1Add >= mProc.REGADDRBASE + mProc.RCSOFS) && mProc.OperatingMode == ProcessorMode.Protected)
            {
                if (lOpSize16)
                    lTemp16 = (UInt16)Misc.GetSelectorForSegment(mProc, Op1Add, 0);
                else
                    lTemp32 = Misc.GetSelectorForSegment(mProc, Op1Add, 0);
            }

            if (lAddrSize16)
                if (lOpSize16)
                    mProc.regs.SP -= 2;
                else
                    mProc.regs.SP -= 4;
            else
                if (lOpSize16)
                    mProc.regs.ESP -= 2;
                else
                    mProc.regs.ESP -= 4;
                

            if (lAddrSize16)
            {
                if (Op1TypeCode == TypeCode.Byte)
                    Op1TypeCode = TypeCode.UInt16;
                //THis is not true for 8086, only 386+
                //If the register being pushed is SP we need to save it as it was before it was decrimented!
                //if (Op1Addr != null)
                if (lOpSize16)
                    mProc.mem.SetWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SP), lTemp16);
                else
                    mProc.mem.SetDWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.SP), lTemp32);
            }
            else
            {
                if (lOpSize16)
                    mProc.mem.SetWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESP), lTemp16);
                else
                    mProc.mem.SetDWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.SS.Value, mProc.regs.ESP), lTemp32);
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
        public override void Impl()
        {
            UInt16 lOrigSP = mProc.regs.SP;
            UInt32 lOrigESP = mProc.regs.ESP;
            Instruct lPush = mProc.Instructions["PUSH"];

            if (mProc.OpSize16)
            {
                lPush.Op1Value.OpWord = mProc.regs.AX;
                lPush.Operand1IsRef = false;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.CX;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.DX;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.BX;
                lPush.Impl();
                lPush.Op1Value.OpWord = lOrigSP;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.BP;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.SI;
                lPush.Impl();
                lPush.Op1Value.OpWord = mProc.regs.DI;
                lPush.Impl();
            }
            else
            {
                lPush.Op1Value.OpDWord = mProc.regs.EAX;
                lPush.Operand1IsRef = false;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.ECX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EDX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EBX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = lOrigESP;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EBP;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.ESI;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EDI;
                lPush.Impl();
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

        public override void Impl()
        {
            UInt32 lOrigESP = mProc.regs.ESP;
            Instruct lPush = mProc.Instructions["PUSH"];

                lPush.Op1Value.OpDWord = mProc.regs.EAX;
                lPush.Operand1IsRef = false;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.ECX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EDX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EBX;
                lPush.Impl();
                lPush.Op1Value.OpDWord = lOrigESP;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EBP;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.ESI;
                lPush.Impl();
                lPush.Op1Value.OpDWord = mProc.regs.EDI;
                lPush.Impl();

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
        public override void Impl()
        {
            Instruct lPush = mProc.Instructions["PUSH"];

            //Push will decide whether to push the whole 32 bit register or not
            if (mProc.OpSize16)
            {
                lPush.Op1Value.OpQWord = mProc.regs.FLAGS;
                lPush.Operand1IsRef = false;
                lPush.Impl();
            }
            else
            {
                lPush.Op1Value.OpQWord = mProc.regs.EFLAGS & 0xFCFFFF;
                lPush.Operand1IsRef = false;
                lPush.Impl();
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
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
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (byte)(Op1Value.OpByte & 0x80); 
                        lOp1Value.OpByte = (byte)((lOp1Value.OpByte << 1) | (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x80);
                    }
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (Word)(Op1Value.OpWord & 0x8000);
                        lOp1Value.OpWord = (Word)((lOp1Value.OpWord << 1) | (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x8000);
                    }
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (DWord)(Op1Value.OpDWord & 0x80000000);
                        lOp1Value.OpDWord = (DWord)((lOp1Value.OpDWord << 1) + (mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x80000000);
                    }
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (QWord)(Op1Value.OpQWord & 0x8000000000000000);
                        lOp1Value.OpQWord = (QWord)((lOp1Value.OpQWord << 1) + (byte)(mProc.regs.FLAGS & 0x1));
                        mProc.regs.setFlagCF(lTempCF == 0x8000000000000000);
                    }
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }

            if (Op2Value.OpByte == 1)
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
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

            switch (Op1TypeCode)
            {
                case TypeCode.Byte: lTopBitNum = 7; break;
                case TypeCode.UInt16: lTopBitNum = 15; break;
                case TypeCode.UInt32: lTopBitNum = 31; break;
                case TypeCode.UInt64: lTopBitNum = 63; break;
                    
            }
            if (Op2Value.OpByte == 1)
            {
                int lMSBDest = Misc.getBit(lOp1Value.OpQWord, lTopBitNum);
                int lXORd = lMSBDest ^ (mProc.regs.FLAGS & 0x01);
                mProc.regs.setFlagOF(lXORd == 1);
            }

            //Do the operation
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)Op1Value.OpByte & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 7;
                        lOp1Value.OpByte = (byte)(lTempFLAGS0 | (lOp1Value.OpByte >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)Op1Value.OpWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 15;
                        lOp1Value.OpWord = (Word)(lTempFLAGS0 | (lOp1Value.OpWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)Op1Value.OpDWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 31;
                        lOp1Value.OpDWord = (DWord)(lTempFLAGS0 | (lOp1Value.OpDWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    for (byte cnt = 0; cnt < Op2Value.OpQWord; cnt++)
                    {
                        lTempCF = (int)Op1Value.OpQWord & 0x01;
                        //10/01/2010 - Op1Val = Misc.setBit(Op1Val, Op1Val.TopBitNum, new cValue(Misc.getBit(mProc.regs.FLAGS,0)) );
                        lTempFLAGS0 = (mProc.regs.FLAGS & 0x01) << 63;
                        lOp1Value.OpQWord = (QWord)((byte)lTempFLAGS0 | (lOp1Value.OpQWord >> 1));
                        mProc.regs.setFlagCF(lTempCF == 0x01);
                    }
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
        public override void Impl()
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

        public override void Impl()
        {
            mProc.Instructions["REPZ"].Impl();
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
        public override void Impl()
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
        public override void Impl()
        {
            mProc.Instructions["REPNE"].Impl();
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
        public override void Impl()
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
        public override void Impl()
        {
            Instruct lPop = mProc.Instructions["POP"];
            lPop.DecodedInstruction = DecodedInstruction;
            lPop.mInternalInstructionCall = false;

            if (mProc.OpSize16)
            {
                lPop.mProc = mProc;
                lPop.Op1Add = mProc.RIP;
                lPop.Impl();
            }
            else
            {
                lPop.mProc = mProc;
                lPop.Op1Add = mProc.REIP;
                lPop.Impl();
            }
            lPop.Op1Add = 0;

            int a = 0;
            if (mProc.regs.EIP == 0)
                a += 1;

            //TODO: Need to update this for StackSize=32
            mProc.regs.SP += (UInt16)(Op1Value.OpWord & 0xFFFF);

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
        public override void Impl()
        {
            UInt32 lTempEIP = mProc.regs.EIP;
            Instruct lPop = mProc.Instructions["POP"];
                        lPop.mInternalInstructionCall = false;
                        lPop.DecodedInstruction = DecodedInstruction;


            lPop.Op1Add = mProc.REIP;
            lPop.Impl();
            lPop.Op1Add = mProc.RCS;
            lPop.Impl();
            lPop.Op1Add = 0x0;
            //TODO: Need to update for StackSize=32
            mProc.regs.SP += (UInt16)(Op1Value.OpWord & 0xFFFF);
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
        public override void Impl()
        {
            mProc.regs.EAX = (UInt32)mProc.mInsructionsExecuted*25;
            mProc.regs.EDX = (UInt32)(( (mProc.mInsructionsExecuted*25) & 0xFFFFFFFF00000000) >> 32);
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
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
            lOp1Value = Misc.RotateLeft(Op1Value, Op2Value.OpByte, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }
            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 0x01);

            if (Op2Value.OpByte == 1)
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
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
            lOp1Value = Misc.RotateRight(Op1Value, Op2Value.OpByte, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    lTopBitNum = 7;
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    lTopBitNum = 15;
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    lTopBitNum = 31;
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
                    lTopBitNum = 63;
                    break;
            }
            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpQWord, lTopBitNum) == 1);
            if (Op2Value.OpByte == 1)
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
        public override void Impl()
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
            EFLAGS(SF:ZF:0:AF:0:PF:1:CF) ← AH;
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
        public override void Impl()
        {
            Instruct lSHL = mProc.Instructions["SHL"];

            lSHL.Op1Value = Op1Value;
            lSHL.Op2Value = Op2Value;
            lSHL.Op1Add = Op1Add;
            lSHL.Op2Add = Op2Add;
            lSHL.Operand1IsRef = Operand1IsRef;
            lSHL.Operand2IsRef = Operand2IsRef;
            lSHL.Impl();

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
        public override void Impl()
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
        public override void Impl()
        {
            int lTempCount = Op2Value.OpByte, lTopBitNum = 0;
            sOpVal lOp1Value = Op1Value;

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lTopBitNum = 7;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpByte /= 2;
                    }
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
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
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
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
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
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
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValSigned = Op1Value;

            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte = (byte)(lOp1ValSigned.OpByte - (lOp2Value.OpByte + (mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetByte(mProc, Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord = (Word)(lOp1ValSigned.OpWord - (lOp2Value.OpWord + (mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetWord(mProc, Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord = (DWord)(lOp1ValSigned.OpDWord - (lOp2Value.OpDWord + (mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord = (QWord)(lOp1ValSigned.OpQWord - (lOp2Value.OpQWord + (byte)(mProc.regs.FLAGS & 0x01)));
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1ValSigned.OpQWord);
                    break;
            }

            //Set the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lOp2Value, lOp1ValSigned, Op1TypeCode, true, true);

        }
    }
    public class SCASB : Instruct
    {
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            bool lAddrSize16 = mProc.AddrSize16;

            sOpVal lPreVal1 = new sOpVal(mProc.regs.AL);
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            sOpVal lTemp;
            if (mProc.AddrSize16)
                lTemp = new sOpVal(mProc.mem.GetByte(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI)));
            else
                lTemp = new sOpVal(mProc.mem.GetByte(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI)));
            sOpVal lAL = lPreVal1;
            lAL.OpByte -= lTemp.OpByte;


            //notice that we don't save the result, we just use it to save the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lTemp, lAL, TypeCode.Byte, true, true);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI--;
                else
                    mProc.regs.EDI--;
            }
            else
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI++;
                else
                    mProc.regs.EDI++;
            }
            #region Instructions
            #endregion
        }
    }
    public class SCASD : Instruct
    {
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;
            bool lAddrSize16 = mProc.AddrSize16;

            sOpVal lPreVal1 = new sOpVal(mProc.regs.AX);
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                return;
            }
            sOpVal lTemp;
            if (mProc.AddrSize16)
                lTemp = new sOpVal(mProc.mem.GetDWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI)));
            else
                lTemp = new sOpVal(mProc.mem.GetDWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI)));

            sOpVal lAL = lPreVal1;
            lAL.OpDWord -= lTemp.OpDWord;


            //notice that we don't save the result, we just use it to save the flags
            mProc.regs.setFlagCF(lPreVal1.OpDWord, lAL.OpDWord, lTemp.OpDWord);
            mProc.regs.setFlagOF(lPreVal1.OpDWord, lTemp.OpDWord, lAL.OpDWord);
            mProc.regs.setFlagSF(lAL.OpDWord);
            mProc.regs.setFlagZF(lAL.OpDWord);
            mProc.regs.setFlagAF(lPreVal1.OpDWord, lAL.OpDWord);
            mProc.regs.setFlagPF(lAL.OpDWord);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI-=4;
                else
                    mProc.regs.EDI-=4;
            }
            else
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI+=4;
                else
                    mProc.regs.EDI+=4;
            }
           #region Instructions
            #endregion
        }
    }
    public class SCASW : Instruct
    {
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
        public override void Impl()
        {
            DWord lSource = 0;
            DWord lDest = 0;

            sOpVal lPreVal1 = new sOpVal(mProc.regs.AX);
            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (mProc.OpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            
            if (!mProc.OpSize16)
            {
                mProc.Instructions["SCASD"].Impl();
                return;
            }
            bool lAddrSize16 = mProc.AddrSize16;

            sOpVal lTemp;
            if (mProc.AddrSize16)
                lTemp = new sOpVal(mProc.mem.GetWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.DI))); 
            else
                lTemp = new sOpVal(mProc.mem.GetWord(mProc, PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.ES.Value, mProc.regs.EDI))); 

            sOpVal lAL = lPreVal1;
            lAL.OpWord -= lTemp.OpWord;


            //notice that we don't save the result, we just use it to save the flags
//            SetFlagsForSubtraction(mProc, lPreVal1, lTemp, lAL, TypeCode.UInt16, true, true);
            mProc.regs.setFlagCF(lPreVal1.OpWord, lAL.OpWord, lTemp.OpQWord);
            mProc.regs.setFlagOF(lPreVal1.OpWord, lTemp.OpWord, lAL.OpWord);
            mProc.regs.setFlagSF(lAL.OpWord);
            mProc.regs.setFlagZF(lAL.OpWord);
            mProc.regs.setFlagAF(lPreVal1.OpWord, lAL.OpWord);
            mProc.regs.setFlagPF(lAL.OpWord);
            if (mProc.regs.FLAGSB.DF)
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI -= 2;
                else
                    mProc.regs.EDI -= 2;
            }
            else
            {
                if (mProc.AddrSize16)
                    mProc.regs.DI += 2;
                else
                    mProc.regs.EDI += 2;
            }
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
        public override void Impl()
        {
            byte lOp1Value = 0;

            switch (DecodedInstruction.RealOpCode)
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
            Instruct lMov = mProc.Instructions["MOV"];
            lMov.Op1Value.OpQWord = lOp1Value;
            lMov.Op2Value.OpQWord = lOp1Value;
            lMov.Op1Add = Op1Add;
            mProc.Instructions["MOV"].Op1TypeCode = Op1TypeCode;
            mProc.Instructions["MOV"].Op2TypeCode = Op2TypeCode;
            lMov.Operand1IsRef = Operand1IsRef;
            lMov.Operand2IsRef = Operand2IsRef;
            lMov.Impl();
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
        public override void Impl()
        {
            int lTempCount = Op2Value.OpByte, lTopBitNum=0;
            sOpVal lOp1Value = Op1Value;

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lTopBitNum = 7;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF(Misc.getBit(lOp1Value.OpByte, lTopBitNum) == 1);
                        lOp1Value.OpByte *= 2;
                    }
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
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
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
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
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
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
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
                    mProc.regs.setFlagSF(lOp1Value.OpQWord);
                    break;
            }

            if (lTempCount == 1)
            {
                bool lTempOF = ((Misc.getBit(lOp1Value.OpQWord, lTopBitNum))==1) ^ mProc.regs.FLAGSB.CF;
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
        public override void Impl()
        {
            int lTempCount = Op2Value.OpByte, lTopBitNum = 0;
            sOpVal lOp1Value = Op1Value;

            if (mProc.ProcType > eProcTypes.i8086)
                lTempCount = lTempCount & 0x1F;

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lTopBitNum = 7;
                    for (UInt16 cnt = 0; (cnt) < lTempCount; cnt++)
                    {
                        if (cnt == lTempCount - 1)
                            mProc.regs.setFlagCF((lOp1Value.OpByte & 0x01) == 1);
                        lOp1Value.OpByte /= 2;
                    }
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
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
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
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
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
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
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
        public override void Impl()
        {
            int Count = Op3Value.OpByte % 0x20;
            int Size = 0; // Op1Val.TopBitNum + 1;
            sOpVal Dest = Op1Value;
            sOpVal Src = Op2Value;

            if (Count == 0)
                return;

            switch (Op1TypeCode)
            {
                case TypeCode.Byte: Size = 8; break;
                case TypeCode.UInt16: Size = 16; break;
                case TypeCode.UInt32: Size = 32; break;
                case TypeCode.UInt64: Size = 64; break;
            }

            if (Count > Size)
                return;

            mProc.regs.setFlagCF(Misc.getBit(Dest.OpQWord, Count - 1)==1);

            for (int i = 0; i <= Size - 1 - Count; i++)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Dest.OpQWord, i + Count)==1);

            for (int i = Size - Count; i <= Size - 1; i++)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Src.OpQWord, i + Count - Size)==1);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, Op1Add, Dest.OpByte);
                    mProc.regs.setFlagSF(Dest.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, Op1Add, Dest.OpWord);
                    mProc.regs.setFlagSF(Dest.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, Op1Add, Dest.OpDWord);
                    mProc.regs.setFlagSF(Dest.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, Op1Add, Dest.OpQWord);
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
        public override void Impl()
        {
            int Count = Op3Value.OpByte % 0x20;
            int Size = 0; // Op1Val.TopBitNum + 1;
            sOpVal Dest = Op1Value;
            sOpVal Src = Op2Value;

            if (Count == 0)
                return;

            switch (Op1TypeCode)
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
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Dest.OpQWord,i-Count)==1);

            for (int i = Count; i >= 0; i--)
                Dest.OpQWord = Misc.setBit(Dest.OpQWord, i, Misc.getBit(Src.OpQWord, i - Count + Size)==1);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc,Op1Add,Dest.OpByte);
                    mProc.regs.setFlagSF(Dest.OpByte);
                    break;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, Op1Add, Dest.OpWord);
                    mProc.regs.setFlagSF(Dest.OpWord);
                    break;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, Op1Add, Dest.OpDWord);
                    mProc.regs.setFlagSF(Dest.OpDWord);
                    break;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, Op1Add, Dest.OpQWord);
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
        public override void Impl()
        {
            UInt16 lLimit;
            UInt32 lBase;

            lLimit = (UInt16)mProc.regs.IDTR.Limit;
            lBase = mProc.regs.IDTR.Base;

            if (mProc.OpSize16)
                lBase &= 0x00FFFFFF;

            mProc.mem.SetWord(mProc, Op1Add, lLimit);
            mProc.mem.SetDWord(mProc, Op1Add + 4, lBase);

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
        public override void Impl()
        {
            UInt16 lLimit;
            UInt32 lBase;

            lLimit = (UInt16)mProc.regs.GDTR.Limit;
            lBase = mProc.regs.GDTR.Base;

            if (mProc.OpSize16)
                lBase &= 0x00FFFFFF;

            mProc.mem.SetWord(mProc, Op1Add, lLimit);
            mProc.mem.SetDWord(mProc, Op1Add + 4, lBase);

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
        public override void Impl()
        {
            if (DecodedInstruction.Op1.Register != eGeneralRegister.NONE)
                mProc.mem.SetDWord(mProc, Op1Add, (mProc.regs.CR0 & 0xFFFF));
            else
                mProc.mem.SetWord(mProc, Op1Add, (Word)mProc.regs.CR0);
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
        public override void Impl()
        {
            mProc.regs.setFlagCF(true);
            #region Instructions
            /*
              Operation
                CF ← 1;
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

        public override void Impl()
        {
            mProc.regs.setFlagDF(true);

            #region Instructions
            /*
              Operation
                CF ← 1;
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
        public override void Impl()
        {
            //STI won't be processed until the end of the NEXT instruction
            mProc.mSTICalled = true;
            //mProc.regs.setFlagIF(true);

            #region Instructions
            /*
              Operation
                CF ← 1;
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
        public override void Impl()
        {
            if (DecodedInstruction.Op1.Register != eGeneralRegister.NONE)
                mProc.mem.SetDWord(mProc, Op1Add, (DWord)(mProc.regs.TR.SegSel & 0xFFFF));
            else
                mProc.mem.SetWord(mProc, Op1Add, (Word)mProc.regs.TR.SegSel);

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
            mDescription = "Store String - GetByte";
            mModFlags = 0;
        }
        public override void Impl()
        {
            DWord lDest = 0;
            DWord lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;


            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);
            do
            {

               mProc.mem.SetByte(mProc, lDest, mProc.regs.AL);
               MOVSW.IncDec(mProc, ref lDest, ref lJunk, lAddrSize16, 1, false, true, false, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
            mDescription = "Store String - GetWord";
            mModFlags = 0;
        }
        public override void Impl()
        {
            UInt32 lDest = 0;
            UInt32 lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }

            if (!mProc.OpSize16)
            {
                mProc.Instructions["STOSD"].Impl();
                return;
            }

            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);

            do
            {

                mProc.mem.SetWord(mProc, lDest, mProc.regs.AX);
                MOVSW.IncDec(mProc, ref lDest, ref lJunk, lAddrSize16, 2, false, true, false, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
            #region Instructions
            #endregion
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
            mDescription = "Store String - GetDWord";
            mModFlags = 0;
        }
        public override void Impl()
        {
            UInt32 lDest = 0, lJunk = 0;
            bool lOpSize16 = mProc.OpSize16;
            bool lAddrSize16 = mProc.AddrSize16;

            if (mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT && mProc.mLoopCount == 0)
            {
                mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
                if (lOpSize16)
                    mProc.regs.CX = 0;
                else
                    mProc.regs.ECX = 0;
                return;
            }
            //MOVSW.SetupMOVSSrc(mProc, ref lSource, lAddrSize16, true);
            MOVSW.SetupMOVSDest(mProc, ref lDest, lAddrSize16, false);

            //MOVSW.SetupMOVSSrcDest(mProc, ref lJunk, ref lDest, lAddrSize16);
            do
            {

                mProc.mem.SetDWord(mProc, lDest, mProc.regs.EAX);
                MOVSW.IncDec(mProc, ref lDest, ref lJunk, lAddrSize16, 4, false, true, false, false);
                //If we are looping and the processor needs attention, break out of the loop without messing up the 
                //repeat condition so the loop can be resumed after the CPU is serviced
                //OR if we weren't meant to loop in the first place
                if ((mProc.mRepeatCondition == Processor_80x86.NOT_REPEAT ||
                    (mProc.NeedToInterruptLoop() && mProc.mRepeatCondition != Processor_80x86.NOT_REPEAT)))
                {
                    //Don't decrement CX if processor needs attention.  It will be done when we return to the calling code
                    //mProc.regs.CX--;
                    return;
                }
            }
            while (--mProc.mLoopCount > 0);
            if (lOpSize16)
                mProc.regs.CX = 0;
            else
                mProc.regs.ECX = 0;
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;
          #region Instructions
            #endregion
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
        public override void Impl()
        {
            /*Always 2 parameters*/
            sOpVal lPreVal1 = Op1Value;
            //capture value pre-operation
            sOpVal lOp2Value = Op2Value; //This will hold the sign extended version of Op2Val if it is immediate
            sOpVal lOp1ValSigned = Op1Value;

            //TODO: SignExtendWhenImmediate isn'really necessary with the new decoder ... remove all references!!!
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);

            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1ValSigned.OpByte = (byte)((sbyte)lOp1ValSigned.OpByte - (sbyte)lOp2Value.OpByte);
                    mProc.mem.SetByte(mProc, Op1Add, lOp1ValSigned.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1ValSigned.OpWord = (Word)((Int16)lOp1ValSigned.OpWord - (Int16)lOp2Value.OpWord);
                    mProc.mem.SetWord(mProc, Op1Add, lOp1ValSigned.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1ValSigned.OpDWord = (DWord)((Int32)lOp1ValSigned.OpDWord - (Int32)lOp2Value.OpDWord);
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1ValSigned.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1ValSigned.OpQWord = (QWord)((Int64)lOp1ValSigned.OpQWord - (Int64)lOp2Value.OpQWord);
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1ValSigned.OpQWord);
                    break;
            }

            //SetC the flags
            //    //SetC the flags
            SetFlagsForSubtraction(mProc, lPreVal1, lOp2Value, lOp1ValSigned, Op1TypeCode, true, true);
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            //Do the operation
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte &= Op2Value.OpByte;
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord &= Op2Value.OpWord;
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord &= Op2Value.OpDWord;
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord &= Op2Value.OpQWord;
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
        public override void Impl()
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
        public override void Impl()
        {
            //TODO: Fix VERW hack
            mProc.regs.setFlagZF(true);
            #region Instructions
            /*
            */
            #endregion
        }
    }
    public class WAIT : Instruct
    {
        public WAIT()
        {
            mName = "WAIT";
            mProc8086 = true;
            mProc8088 = true;
            mProc80186 = true;
            mProc80286 = true;
            mProc80386 = true;
            mProc80486 = true;
            mProcPentium = true;
            mProcPentiumPro = true;
            mDescription = "Wait";
            mModFlags = 0;
        }
        public override void Impl()
        {
            return; //we'll see what it does :-|
            //Debug.WriteLine("WAIT: Not implemented, ignoring");
            throw new NotImplementedException();
            //TODO: CoProcessor stuff
            #region Instructions
            #endregion
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
        public override void Impl()
        {
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    mProc.mem.SetByte(mProc, Op2Add, Op1Value.OpByte);
                    mProc.mem.SetByte(mProc, Op1Add, Op2Value.OpByte);
                    return;
                case TypeCode.UInt16:
                    mProc.mem.SetWord(mProc, Op2Add, Op1Value.OpWord);
                    mProc.mem.SetWord(mProc, Op1Add, Op2Value.OpWord);
                    return;
                case TypeCode.UInt32:
                    mProc.mem.SetDWord(mProc, Op2Add, Op1Value.OpDWord);
                    mProc.mem.SetDWord(mProc, Op1Add, Op2Value.OpDWord);
                    return;
                case TypeCode.UInt64:
                    mProc.mem.SetQWord(mProc, Op2Add, Op1Value.OpQWord);
                    mProc.mem.SetQWord(mProc, Op1Add, Op2Value.OpQWord);
                    return;
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
        public override void Impl()
        {
            UInt16 lTemp = 0;
            if (mProc.mSegmentOverride == 0)
                mProc.mSegmentOverride = mProc.RDS;
            if (mProc.AddrSize16)
                lTemp = (UInt16)(mProc.regs.BX + mProc.regs.AL);
            else
                lTemp = (UInt16)(mProc.regs.EBX + mProc.regs.AL);

            UInt32 loc = GetSegOverriddenAddress(mProc, lTemp);
            mProc.regs.AL = mProc.mem.GetByte(mProc, loc);
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
        public override void Impl()
        {
            UInt32 TableBase = PhysicalMem.GetLocForSegOfs(mProc, mProc.regs.DS.Value, mProc.regs.BX);
            mProc.regs.AL = mProc.mem.GetByte(mProc,TableBase + mProc.regs.AL);
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
        public override void Impl()
        {
            //capture value pre-operation
            sOpVal lPreVal1 = Op1Value, lOp1Value = Op1Value;
            //08/28/2010 - Changed to use SignExtendOp
            sOpVal lOp2Value = Op2Value;
            if (!Operand2IsRef)
                SignExtendWhenImmediate(ref lOp2Value, Operand2IsRef, ref Op2TypeCode, Op1TypeCode);
            //Do the operation
            switch (Op1TypeCode)
            {
                case TypeCode.Byte:
                    lOp1Value.OpByte ^= lOp2Value.OpByte;
                    mProc.mem.SetByte(mProc, Op1Add, lOp1Value.OpByte);
                    mProc.regs.setFlagSF(lOp1Value.OpByte);
                    break;
                case TypeCode.UInt16:
                    lOp1Value.OpWord ^= lOp2Value.OpWord;
                    mProc.mem.SetWord(mProc, Op1Add, lOp1Value.OpWord);
                    mProc.regs.setFlagSF(lOp1Value.OpWord);
                    break;
                case TypeCode.UInt32:
                    lOp1Value.OpDWord ^= lOp2Value.OpDWord;
                    mProc.mem.SetDWord(mProc, Op1Add, lOp1Value.OpDWord);
                    mProc.regs.setFlagSF(lOp1Value.OpDWord);
                    break;
                case TypeCode.UInt64:
                    lOp1Value.OpQWord ^= lOp2Value.OpQWord;
                    mProc.mem.SetQWord(mProc, Op1Add, lOp1Value.OpQWord);
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
