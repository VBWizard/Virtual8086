using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualProcessor;
using System;

namespace VirtualProcessor.Tests
{
    [TestClass]
    public class NEGTests
    {
        private Processor_80x86 CreateProcessor()
        {
            uint memSize = 16 * 1024 * 1024;
            eProcTypes processorType = eProcTypes.i80386;
            PCSystem system = new PCSystem(memSize, processorType, "");
            return new Processor_80x86(system, memSize, processorType);
        }

        [TestMethod]
        public void NegByteOverflowOnlyForMinValue()
        {
            var proc = CreateProcessor();
            var neg = new NEG() { mProc = proc };
            sInstruction ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.Byte;
            ins.Op1Add = Processor_80x86.RAL;

            proc.regs.AL = 0x80;
            ins.Op1Value.OpByte = proc.regs.AL;
            neg.Impl(ref ins);
            Assert.IsTrue(proc.regs.FLAGSB.OF, "OF should be set for 0x80");

            proc.regs.AL = 0x7F;
            ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.Byte;
            ins.Op1Add = Processor_80x86.RAL;
            ins.Op1Value.OpByte = proc.regs.AL;
            neg.Impl(ref ins);
            Assert.IsFalse(proc.regs.FLAGSB.OF, "OF should be clear for 0x7F");
        }

        [TestMethod]
        public void NegWordOverflowOnlyForMinValue()
        {
            var proc = CreateProcessor();
            var neg = new NEG() { mProc = proc };
            sInstruction ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.UInt16;
            ins.Op1Add = Processor_80x86.RAX;

            proc.regs.AX = 0x8000;
            ins.Op1Value.OpWord = proc.regs.AX;
            neg.Impl(ref ins);
            Assert.IsTrue(proc.regs.FLAGSB.OF, "OF should be set for 0x8000");

            proc.regs.AX = 0x7FFF;
            ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.UInt16;
            ins.Op1Add = Processor_80x86.RAX;
            ins.Op1Value.OpWord = proc.regs.AX;
            neg.Impl(ref ins);
            Assert.IsFalse(proc.regs.FLAGSB.OF, "OF should be clear for 0x7FFF");
        }

        [TestMethod]
        public void NegDWordOverflowOnlyForMinValue()
        {
            var proc = CreateProcessor();
            var neg = new NEG() { mProc = proc };
            sInstruction ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.UInt32;
            ins.Op1Add = Processor_80x86.RAX;

            proc.regs.EAX = 0x80000000;
            ins.Op1Value.OpDWord = proc.regs.EAX;
            neg.Impl(ref ins);
            Assert.IsTrue(proc.regs.FLAGSB.OF, "OF should be set for 0x80000000");

            proc.regs.EAX = 0x7FFFFFFF;
            ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.UInt32;
            ins.Op1Add = Processor_80x86.RAX;
            ins.Op1Value.OpDWord = proc.regs.EAX;
            neg.Impl(ref ins);
            Assert.IsFalse(proc.regs.FLAGSB.OF, "OF should be clear for 0x7FFFFFFF");
        }
    }
}
