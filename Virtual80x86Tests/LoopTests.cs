using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualProcessor;
using System;

namespace VirtualProcessor.Tests
{
    [TestClass]
    public class LoopTests
    {
        PCSystem mSystem;
        Processor_80x86 mProc;
        LOOP loop;
        LOOPNE loopne;
        sInstruction sIns;

        [TestInitialize]
        public void Setup()
        {
            uint mem = 1024 * 1024;
            mSystem = new PCSystem(mem, eProcTypes.i80386, @"");
            mProc = new Processor_80x86(mSystem, mem, eProcTypes.i80386);
            sIns = new sInstruction();
            loop = new LOOP() { mProc = mProc };
            loopne = new LOOPNE() { mProc = mProc };
        }

        [TestMethod]
        public void LoopUsesCXWhenOpSize16()
        {
            mProc.mCurrInstructOpSize16 = true;
            mProc.regs.ECX = 0x00010000;
            sIns.Op1Value.OpSByte = 0;
            sIns.Op1TypeCode = TypeCode.SByte;

            loop.Impl(ref sIns);

            Assert.AreEqual(0x0001FFFFu, mProc.regs.ECX, "LOOP should operate on CX for 16-bit operand size");
        }

        [TestMethod]
        public void LoopUsesECXWhenOpSize32()
        {
            mProc.mCurrInstructOpSize16 = false;
            mProc.regs.ECX = 0x00010000;
            sIns.Op1Value.OpSByte = 0;
            sIns.Op1TypeCode = TypeCode.SByte;

            loop.Impl(ref sIns);

            Assert.AreEqual(0x0000FFFFu, mProc.regs.ECX, "LOOP should operate on ECX for 32-bit operand size");
        }

        [TestMethod]
        public void LoopNEUsesCXWhenOpSize16()
        {
            mProc.mCurrInstructOpSize16 = true;
            mProc.regs.ECX = 0x00010000;
            mProc.regs.FLAGSB.ZF = false;
            sIns.Op1Value.OpSByte = 0;
            sIns.Op1TypeCode = TypeCode.SByte;

            loopne.Impl(ref sIns);

            Assert.AreEqual(0x0001FFFFu, mProc.regs.ECX, "LOOPNE should operate on CX for 16-bit operand size");
        }

        [TestMethod]
        public void LoopNEUsesECXWhenOpSize32()
        {
            mProc.mCurrInstructOpSize16 = false;
            mProc.regs.ECX = 0x00010000;
            mProc.regs.FLAGSB.ZF = false;
            sIns.Op1Value.OpSByte = 0;
            sIns.Op1TypeCode = TypeCode.SByte;

            loopne.Impl(ref sIns);

            Assert.AreEqual(0x0000FFFFu, mProc.regs.ECX, "LOOPNE should operate on ECX for 32-bit operand size");
        }
    }
}

