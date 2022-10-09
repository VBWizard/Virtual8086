using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace VirtualProcessor
{
    public enum Registers
    {
        
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct RegStruct
    {
        /// <summary>
        ///The accumulator register (divided into AH / AL): Generates shortest machine code Arithmetic, logic and data transfer. One number must be in AL or AX.  Multiplication & Division.  Input & Output 
        /// </summary>
        [FieldOffset(0)]
        public UInt16 AX;
        [FieldOffset(0)]
        public byte AL;
        [FieldOffset(1)]
        public byte AH;
        /// <summary>
        ///BX - the base address register (divided into BH / BL). 
        /// </summary>
        [FieldOffset(2)]
        public UInt16 BX;
        [FieldOffset(2)]
        public byte BL;
        [FieldOffset(3)]
        public byte BH;
        /// <summary>
        ///CX - the count register (divided into CH / CL): 
        ///Iterative code segments using the LOOP instruction 
        ///Repetitive operations on strings with the REP command 
        ///Count (in CL) of bits to shift and rotate 
        /// </summary>
        [FieldOffset(4)]
        public UInt16 CX;
        [FieldOffset(4)]
        public byte CL;
        [FieldOffset(5)]
        public byte CH;
        /// <summary>
        /// DX - the data register (divided into DH / DL): 
        /// DX:AX concatenated into 32-bit register for some MUL and DIV operations 
        /// Specifying ports in some IN and OUT operations 
        /// </summary>
        [FieldOffset(6)]
        public UInt16 DX;
        [FieldOffset(6)]
        public byte DL;
        [FieldOffset(7)]
        public byte DH;
        /// <summary>
        ///SI - source index register: 
        ///Can be used for pointer addressing of data 
        ///Used as source in some string processing instructions 
        ///Offset address relative to DS 
        /// </summary>
        [FieldOffset(8)]
        public UInt16 SI;
        /// <summary>
        /// DI - destination index register: 
        ///Can be used for pointer addressing of data 
        ///Used as destination in some string processing instructions 
        ///Offset address relative to ES 
        /// </summary>
        [FieldOffset(10)]
        public UInt16 DI;
        /// <summary>
        /// BP - base pointer: 
        ///Primarily used to access parameters passed via the stack 
        ///Offset address relative to SS 
        /// </summary>
        [FieldOffset(12)]
        public UInt16 BP;
        /// <summary>
        /// SP - stack pointer: 
        ///Always points to top item on the stack 
        ///Offset address relative to SS 
        ///Always points to word (byte at even address) 
        ///An empty stack will had SP = FFFEh 
        /// </summary>
        [FieldOffset(14)]
        public UInt16 SP;
        /// <summary>
        ///IP - the instruction pointer: 
        ///Always points to next instruction to be executed 
        ///Offset address relative to CS
        ///IP register always works together with CS segment register and it points to currently executing instruction.
        /// </summary>
        [FieldOffset(16)]
        public UInt16 IP;
        /// <summary>
        ///CS - points at the segment containing the current program. 
        /// </summary>
        [FieldOffset(18)]
        public UInt16 CS;
        /// <summary>
        ///DS - generally points at segment where variables are defined. 
        /// </summary>
        [FieldOffset(20)]
        public UInt16 DS;
        /// <summary>
        ///SS - points at the segment containing the stack. 
        /// </summary>
        [FieldOffset(22)]
        public UInt16 SS;
        /// <summary>
        ///ES - extra segment register, it's up to a coder to define its usage. 
        /// </summary>
        [FieldOffset(24)]
        public UInt16 ES;
        [FieldOffset(26)]
        public sFlags FLAGSB;
        [FieldOffset(50)]
        private UInt16 mFLAGS;
        public UInt16 FLAGS
        {
            set { mFLAGS = value; FLAGSB.SetValues(value); }
            get { return mFLAGS; }
        }

        public void ResetFlags()
        {
            setFlagIF(true);
        }

        #region Set Flag Methods
        /// <summary>
        /// Set Sign Flag (SF) - set to 1 when result is negative. When result is positive it is set to 0. (This flag takes the value of the most significant bit.) 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns></returns>
        public void setFlagSF(UInt16 InVal)
        {
            setFlagSF(System.Convert.ToBoolean(Misc.getBit(InVal, 15)));
        }
        public void setFlagSF(byte InVal)
        {
            setFlagSF(System.Convert.ToBoolean(Misc.getBit(InVal, 7)));
        }
        public void setFlagSF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.SF, Value);
        }
        /// <summary>
        /// Set Zero Flag (ZF) - set to 1 when result is zero. For non-zero result this flag is set to 0.
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns></returns>
        public void setFlagZF(UInt16 InVal)
        {
            if (InVal == 0)
                setFlagZF(true);
            else
                setFlagZF(false);
        }
        public void setFlagZF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.ZF, Value);
        }
        /// <summary>
        /// Set Parity Flag (PF) - this flag is set to 1 when there is even number of one bits in result, and to 0 when there is odd number of one bits. 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of last operation (destination)</param>
        /// <returns>Updated Flags word</returns>
        public void setFlagPF(UInt16 InVal)
        {
            int lTemp = 0;
            for (int cnt = 0; cnt < 15; cnt++)
                if (Misc.getBit(InVal, cnt) == 1)
                    lTemp++;
            if (lTemp % 2 == 0)
                setFlagPF(true);
            else
                setFlagPF(false);
        }
        public void setFlagPF(byte InVal)
        {
            int lTemp = 0;
            for (int cnt = 0; cnt < 7; cnt++)
                if (Misc.getBit(FLAGS, cnt) == 1)
                    lTemp++;
            if (lTemp % 2 == 0)
                setFlagPF(true);
            else
                setFlagPF(false);
        }
        public void setFlagPF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.PF, Value);
        }
        /// <summary>
        /// Set when MSBs of preval & postval don't match
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="inPreVal"></param>
        /// <param name="inPostVal"></param>
        /// <returns></returns>
        public void setFlagOF(UInt16 inPreVal, UInt16 inPostVal)
        {
            UInt16 lPre8 = Misc.getBit(inPreVal, 15);
            UInt16 lPost8 = Misc.getBit(inPostVal, 15);
            if (lPre8 != lPost8)
                setFlagOF(true);
            else
                setFlagOF(false);
        }
        public void setFlagOF(byte inPreVal, byte inPostVal)
        {
            UInt16 lPre8 = Misc.getBit(inPreVal, 7);
            UInt16 lPost8 = Misc.getBit(inPostVal, 7);
            if (lPre8 != lPost8)
                setFlagOF(true);
            else
                setFlagOF(false);
        }
        public void setFlagOF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.OF, Value);
        }
        /// <summary>
        /// Auxiliary Flag (AF) - set to 1 when there is an unsigned overflow for low nibble (4 bits). 
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="inPreVal">Value prior to operation</param>
        /// <param name="inPostVal">Value after operation</param>
        /// <returns>Updated Flags word</returns>
        public void setFlagAF(UInt16 inPreVal, UInt16 inPostVal)
        {
            //Bit 12 here represents the 13th bit of the word (0 based)
            UInt16 lPre8 = Misc.getBit(inPreVal, 12);
            UInt16 lPost8 = Misc.getBit(inPostVal, 12);
            if (lPre8 != lPost8)
                setFlagAF(true);
            else
                setFlagAF(true);
        }
        public void setFlagAF(byte inPreVal, byte inPostVal)
        {
            //Bit 4 here represents the 5th bit of the word (0 based)
            UInt16 lPre8 = Misc.getBit(inPreVal, 4);
            UInt16 lPost8 = Misc.getBit(inPostVal, 4);
            if (lPre8 != lPost8)
                setFlagAF(true);
            else
                setFlagAF(true);
        }
        public void setFlagAF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.AF, Value);
        }
        /// <summary>
        /// Set when an arithmetic carry or borrow has been generated out of the most significant bit position.
        /// </summary>
        /// <param name="Flags">Flags</param>
        /// <param name="InVal">Result of the last instruction</param>
        /// <returns></returns>
        public void setFlagCF(UInt16 InVal)
        {
            //if Input value is > 0x7FFF (binary 111 1111 1111 1111) then 15th (top) bit is set, so set CF
            if (InVal > 0x7FFF)
                setFlagCF(true);
            else
                setFlagCF(false);
        }
        public void setFlagCF(byte InVal)
        {
            //if Input value is > 0x7F (binary 111 1111) then 7th (top) bit is set, so set CF
            if (InVal > 0x7F)
                setFlagCF(true);
            else
                setFlagCF(false);
        }
        public void setFlagCF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.CF, Value);
        }
        public void setFlagIF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.IF, Value);
        }
        public void setFlagTF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS, (int)eFLAGS.TF, Value);
        }
        public void setFlagDF(bool Value)
        {
            FLAGS = Misc.setBit(FLAGS,(int)eFLAGS.DF,Value);
        }
        #endregion

/*        public unsafe static void GetRegisterForString(ref Processor_80x86 proc, String RegName, ref UInt16* Register)
        {
            switch (RegName.ToUpper())
            {
                case "AX":
                    Register = &proc.regs.AX;
                    break;

            }
        }
*/
    }

 }