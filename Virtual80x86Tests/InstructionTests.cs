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
    }
}