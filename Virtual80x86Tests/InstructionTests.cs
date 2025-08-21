using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace VirtualProcessor.Tests
{
    [TestClass()]
    public class InstructionTests
    {
        //examples are here: http://www.gabrielececchetti.it/Teaching/CalcolatoriElettronici/Docs/i8086_instruction_set.pdf
        
        static sInstruction sIns;
        static uint iTotalMemory = 1024 * 1024 * 128;
        static eProcTypes mProcessorType = eProcTypes.i80386;
        static PCSystem mSystem;
        static Processor_80x86 mProc;
        static AAA insAAA;
        static AAD insAAD; 
        static AAM insAAM;
        static ADD insADD;
        static AAS insAAS;
        static ADC insADC;
        static STC insSTC;
        static CLC insCLC;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            mSystem = new PCSystem(iTotalMemory, mProcessorType, @"");
            mProc = new Processor_80x86(mSystem, iTotalMemory, mProcessorType);
            sIns = new sInstruction();
            insAAA = new AAA() { mProc = mProc};
            insAAD = new AAD() { mProc = mProc };
            insAAM = new AAM() { mProc = mProc };
            insADD = new ADD() { mProc = mProc };
            insAAS = new AAS() { mProc = mProc };
            insADC = new ADC() { mProc = mProc };
            insSTC = new STC() { mProc = mProc };
            insCLC = new CLC() { mProc = mProc };
            insOUTS = new OUTS() { mProc = mProc };
        }

        [TestMethod()]
        public void AAATests()
        {
            ///TEST: AAA
            sIns.Op1Value.OpByte = 6;
            sIns.Op2Value.OpByte = 5;
            sIns.Operand1IsRef = false;
            sIns.Operand2IsRef = false;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Add = Processor_80x86.RAX;
            insADD.Impl(ref sIns);
            insAAA.Impl(ref sIns);
            Assert.AreEqual(0x0101, mProc.regs.AX, "AAA test failed");

        }

        [TestMethod()]
        public void RDTSC64BitTest()
        {
            sIns = new sInstruction();
            mProc.InstructionsExecuted = 0x100000001;
            mProc.regs.EAX = 0;
            mProc.regs.EDX = 0;
            insRDTSC.Impl(ref sIns);
            Assert.AreEqual(0x00000019u, mProc.regs.EAX, "RDTSC low dword incorrect");
            Assert.AreEqual(0x00000019u, mProc.regs.EDX, "RDTSC high dword incorrect");
        }

        [TestMethod()]
        public void ImplTest()
        {
            

            sIns = new sInstruction();
            mProc.regs.AX = 0x06;


            //TEST: AAD
            sIns = new sInstruction();
            sIns.Op1Value.OpByte = 0x0a;
            mProc.regs.AX = 0x1234;
            insAAD.Impl(ref sIns);
            Assert.AreEqual(0xe8, mProc.regs.AX, "AAD test failed");

            //TEST: AAM
            sIns = new sInstruction();
            sInstruction.bytes = new byte[2] { 0xd4, 0x0a };
            sIns.Op1Value.OpByte = 0x0a;
            mProc.regs.AX = 0x0f;
            insAAM.Impl(ref sIns);
            Assert.AreEqual(1, mProc.regs.AH, "AAM test 1 failed");
            Assert.AreEqual(5, mProc.regs.AL, "AAM test 2 failed");

            //TEST: AAS
            sIns = new sInstruction();
            mProc.regs.AX = 0x02ff;
            insAAS.Impl(ref sIns);
            Assert.AreEqual(1, mProc.regs.AH, "AAS test 1 failed");
            Assert.AreEqual(9, mProc.regs.AL, "AAS test 2 failed");

            //TEST: ADC
            insSTC.Impl(ref sIns);
            mProc.regs.AX = 5;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Value.OpByte = 5;
            sIns.Op1Add = Processor_80x86.RAL;
            sIns.Op2TypeCode = TypeCode.Byte;
            sIns.Op2Value.OpByte = 1;
            sIns.Operand2IsRef = false;
            insADC.Impl(ref sIns);
            Assert.AreEqual(7, mProc.regs.AL, "ADC test failed");

            insCLC.Impl(ref sIns);
            mProc.regs.AX = 5;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Value.OpByte = 5;
            sIns.Op1Add = Processor_80x86.RAL;
            sIns.Op2TypeCode = TypeCode.Byte;
            sIns.Op2Value.OpByte = 1;
            sIns.Operand2IsRef = false;
            insADC.Impl(ref sIns);
            Assert.AreEqual(6, mProc.regs.AL, "ADC test failed");

            insSTC.Impl(ref sIns);
            mProc.regs.AX = 0xFF;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Value.OpByte = 0xFF;
            sIns.Op1Add = Processor_80x86.RAL;
            sIns.Op2TypeCode = TypeCode.Byte;
            sIns.Op2Value.OpByte = 1;
            sIns.Operand2IsRef = false;
            insADC.Impl(ref sIns);
            Assert.AreEqual(1, mProc.regs.AL, "ADC test 2 failed");

            insSTC.Impl(ref sIns);
            mProc.regs.EAX = 0xFFFFFFFF;
            sIns.Op1TypeCode = TypeCode.UInt32;
            sIns.Op1Value.OpDWord = 0xFFFFFFFF;
            sIns.Op1Add = Processor_80x86.RAL;
            sIns.Op2TypeCode = TypeCode.Byte;
            sIns.Op2Value.OpByte = 1;
            sIns.Operand2IsRef = false;
            insADC.Impl(ref sIns);
            Assert.AreEqual(1, mProc.regs.AL, "ADC test 2 failed");

            //TEST: ADD


        }

        [TestMethod()]
        public void IDIVByte_Success()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.AX = 0x0008;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Value.OpByte = 0x02;
            insIDIV.Impl(ref sIns);
            Assert.IsFalse(sIns.ExceptionThrown, "IDIV byte success threw exception");
            Assert.AreEqual((byte)4, mProc.regs.AL, "IDIV byte quotient");
            Assert.AreEqual((byte)0, mProc.regs.AH, "IDIV byte remainder");
        }

        [TestMethod()]
        public void IDIVByte_DivideError()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.AX = 0x7FFF;
            sIns.Op1TypeCode = TypeCode.Byte;
            sIns.Op1Value.OpByte = 0x01;
            insIDIV.Impl(ref sIns);
            Assert.IsTrue(sIns.ExceptionThrown, "IDIV byte divide error not thrown");
            Assert.AreEqual(0x00, sIns.ExceptionNumber, "IDIV byte divide error code");
            Assert.AreEqual(0x7FFF, mProc.regs.AX, "IDIV byte dividend modified");
        }

        [TestMethod()]
        public void IDIVWord_Success()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.DX = 0x0000;
            mProc.regs.AX = 0x8000;
            sIns.Op1TypeCode = TypeCode.UInt16;
            sIns.Op1Value.OpWord = 0x0002;
            insIDIV.Impl(ref sIns);
            Assert.IsFalse(sIns.ExceptionThrown, "IDIV word success threw exception");
            Assert.AreEqual((UInt16)0x4000, mProc.regs.AX, "IDIV word quotient");
            Assert.AreEqual((UInt16)0x0000, mProc.regs.DX, "IDIV word remainder");
        }

        [TestMethod()]
        public void IDIVWord_DivideError()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.DX = 0x0001;
            mProc.regs.AX = 0x0000;
            sIns.Op1TypeCode = TypeCode.UInt16;
            sIns.Op1Value.OpWord = 0x0001;
            insIDIV.Impl(ref sIns);
            Assert.IsTrue(sIns.ExceptionThrown, "IDIV word divide error not thrown");
            Assert.AreEqual(0x00, sIns.ExceptionNumber, "IDIV word divide error code");
            Assert.AreEqual((UInt16)0x0001, mProc.regs.DX, "IDIV word dividend modified (DX)");
            Assert.AreEqual((UInt16)0x0000, mProc.regs.AX, "IDIV word dividend modified (AX)");
        }

        [TestMethod()]
        public void IDIVDWord_Success()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.EDX = 0x00000000;
            mProc.regs.EAX = 0x80000000;
            sIns.Op1TypeCode = TypeCode.UInt32;
            sIns.Op1Value.OpDWord = 0x00000002;
            insIDIV.Impl(ref sIns);
            Assert.IsFalse(sIns.ExceptionThrown, "IDIV dword success threw exception");
            Assert.AreEqual(0x40000000u, mProc.regs.EAX, "IDIV dword quotient");
            Assert.AreEqual(0x00000000u, mProc.regs.EDX, "IDIV dword remainder");
        }

        [TestMethod()]
        public void IDIVDWord_DivideError()
        {
            IDIV insIDIV = new IDIV() { mProc = mProc };
            sIns = new sInstruction();
            mProc.regs.EDX = 0x00000002;
            mProc.regs.EAX = 0x00000000;
            sIns.Op1TypeCode = TypeCode.UInt32;
            sIns.Op1Value.OpDWord = 0x00000001;
            insIDIV.Impl(ref sIns);
            Assert.IsTrue(sIns.ExceptionThrown, "IDIV dword divide error not thrown");
            Assert.AreEqual(0x00, sIns.ExceptionNumber, "IDIV dword divide error code");
            Assert.AreEqual(0x00000002u, mProc.regs.EDX, "IDIV dword dividend modified (EDX)");
            Assert.AreEqual(0x00000000u, mProc.regs.EAX, "IDIV dword dividend modified (EAX)");
        }
      
        public void OUTSWithoutRepLeavesCounter()
        {
            var ins = new sInstruction();
            ins.RealOpCode = 0x6F;
            mProc.mCurrInstructAddrSize16 = true;
            mProc.regs.CX = 5;
            mProc.regs.SI = 0;
            mProc.regs.ES.Value = 0;
            mProc.regs.DX = 0;
            mProc.mem.SetDWord(mProc, ref ins, 0, 0x12345678);
            mProc.mRepeatCondition = Processor_80x86.NOT_REPEAT;

            insOUTS.Impl(ref ins);

            Assert.AreEqual((UInt16)5, mProc.regs.CX, "OUTS should not modify CX without REP");
        }
      
        [TestMethod]
         public void SAHFTests()
        {
            sIns = new sInstruction();

            // Ensure reserved bits (1,3,5) are not modified when clearing flags
            mProc.regs.FLAGS = 0xFF; // set all bits
            mProc.regs.AH = 0x00;    // clear all status flag bits
            insSAHF.Impl(ref sIns);
            Assert.AreEqual(0x2A, mProc.regs.FLAGS & 0xFF, "SAHF did not preserve reserved bits when clearing flags");

            // Ensure status flags update while reserved bits remain unchanged
            mProc.regs.FLAGS = 0x00; // reserved bits start cleared
            mProc.regs.AH = 0xFF;    // set all flag bits in AH
            insSAHF.Impl(ref sIns);
            Assert.AreEqual(0xD5, mProc.regs.FLAGS & 0xFF, "SAHF did not correctly set status flags");

        [TestMethod]
        public void SCASBRepVariants()
        {
            sInstruction ins = new sInstruction();
            uint baseAddr = PhysicalMem.GetLocForSegOfs(mProc, ref mProc.regs.ES, 0);

            // REP / REPE - all bytes equal
            mProc.mRepeatCondition = Processor_80x86.REPEAT_TILL_ZERO;
            mProc.mCurrInstructAddrSize16 = true;
            mProc.regs.CX = 3;
            mProc.regs.DI = 0;
            mProc.regs.AL = 0x5;
            for (int i = 0; i < 3; i++) mProc.mem.SetByte(mProc, ref ins, baseAddr + (uint)i, 0x5);
            insSCASB.Impl(ref ins);
            Assert.AreEqual(0u, mProc.regs.CX, "REP CX not zero");
            Assert.AreEqual(3, mProc.regs.DI, "REP DI incorrect");
            Assert.IsTrue(mProc.regs.FLAGSB.ZF, "REP ZF not set");

            // REPE - stop on mismatch
            mProc.mRepeatCondition = Processor_80x86.REPEAT_TILL_ZERO;
            mProc.regs.CX = 3;
            mProc.regs.DI = 0;
            mProc.regs.AL = 0x5;
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 0, 0x4);
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 1, 0x5);
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 2, 0x5);
            insSCASB.Impl(ref ins);
            Assert.AreEqual(2u, mProc.regs.CX, "REPE CX incorrect");
            Assert.AreEqual(1, mProc.regs.DI, "REPE DI incorrect");
            Assert.IsFalse(mProc.regs.FLAGSB.ZF, "REPE ZF incorrect");

            // REPNE - stop on match
            mProc.mRepeatCondition = Processor_80x86.REPEAT_TILL_NOT_ZERO;
            mProc.regs.CX = 3;
            mProc.regs.DI = 0;
            mProc.regs.AL = 0x5;
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 0, 0x4);
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 1, 0x5);
            mProc.mem.SetByte(mProc, ref ins, baseAddr + 2, 0x4);
            insSCASB.Impl(ref ins);
            Assert.AreEqual(1u, mProc.regs.CX, "REPNE CX incorrect");
            Assert.AreEqual(2, mProc.regs.DI, "REPNE DI incorrect");
            Assert.IsTrue(mProc.regs.FLAGSB.ZF, "REPNE ZF incorrect");

          [TestMethod()]
        public void SHRByteFlags()
        {
            sInstruction ins = new sInstruction();
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            mProc.regs.AL = 0x81;
            ins.Op1TypeCode = TypeCode.Byte;
            ins.Op1Value.OpByte = 0x81;
            ins.Op1Add = Processor_80x86.RAL;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            insSHR.Impl(ref ins);
            Assert.AreEqual(0x40, mProc.regs.AL);
            Assert.IsTrue(mProc.regs.FLAGSB.CF);
            Assert.IsTrue(mProc.regs.FLAGSB.OF);
        }

        [TestMethod()]
        public void SHRWordFlags()
        {
            sInstruction ins = new sInstruction();
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            mProc.regs.AX = 0x8001;
            ins.Op1TypeCode = TypeCode.UInt16;
            ins.Op1Value.OpWord = 0x8001;
            ins.Op1Add = Processor_80x86.RAX;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            insSHR.Impl(ref ins);
            Assert.AreEqual(0x4000, mProc.regs.AX);
            Assert.IsTrue(mProc.regs.FLAGSB.CF);
            Assert.IsTrue(mProc.regs.FLAGSB.OF);
        }

        [TestMethod()]
        public void SHRDWordFlags()
        {
            sInstruction ins = new sInstruction();
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            mProc.regs.EAX = 0x80000001;
            ins.Op1TypeCode = TypeCode.UInt32;
            ins.Op1Value.OpDWord = 0x80000001;
            ins.Op1Add = Processor_80x86.REAX;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            insSHR.Impl(ref ins);
            Assert.AreEqual((UInt32)0x40000000, mProc.regs.EAX);
            Assert.IsTrue(mProc.regs.FLAGSB.CF);
            Assert.IsTrue(mProc.regs.FLAGSB.OF);
        }

        [TestMethod()]
        public void SHRQWordFlags()
        {
            sInstruction ins = new sInstruction();
            mProc.regs.setFlagCF(false);
            mProc.regs.setFlagOF(false);
            UInt32 addr = 0x2000;
            ins.Op1TypeCode = TypeCode.UInt64;
            ins.Op1Value.OpQWord = 0x8000000000000001;
            ins.Op1Add = addr;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            mProc.mem.SetQWord(mProc, ref ins, addr, ins.Op1Value.OpQWord);
            insSHR.Impl(ref ins);
            UInt64 result = mProc.mem.GetQWord(mProc, ref ins, addr);
            Assert.AreEqual(0x4000000000000000, result);
            Assert.IsTrue(mProc.regs.FLAGSB.CF);
            Assert.IsTrue(mProc.regs.FLAGSB.OF);
        }
    }
}