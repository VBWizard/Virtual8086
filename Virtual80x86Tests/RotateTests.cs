using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VirtualProcessor;

namespace VirtualProcessor.Tests
{
    [TestClass]
    public class RotateTests
    {
        private Processor_80x86 BuildProc()
        {
            uint memSize = 1024 * 1024;
            eProcTypes procType = eProcTypes.i80386;
            PCSystem system = new PCSystem(memSize, procType, "");
            return new Processor_80x86(system, memSize, procType);
        }

        [TestMethod]
        public void RorSetsOFWhenTopBitsDiffer()
        {
            var proc = BuildProc();
            var ror = new ROR() { mProc = proc };
            sInstruction ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.Byte;
            ins.Op1Add = Processor_80x86.RAL;
            ins.Op1Value.OpByte = 0x80;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            ins.Operand1IsRef = false;
            ins.Operand2IsRef = false;

            proc.regs.AL = 0x80;
            ror.Impl(ref ins);

            Assert.IsTrue(proc.regs.FLAGSB.OF, "OF should be set when high bits differ");
        }

        [TestMethod]
        public void RorClearsOFWhenTopBitsSame()
        {
            var proc = BuildProc();
            var ror = new ROR() { mProc = proc };
            sInstruction ins = new sInstruction();
            ins.Op1TypeCode = TypeCode.Byte;
            ins.Op1Add = Processor_80x86.RAL;
            ins.Op1Value.OpByte = 0x40;
            ins.Op2TypeCode = TypeCode.Byte;
            ins.Op2Value.OpByte = 1;
            ins.Operand1IsRef = false;
            ins.Operand2IsRef = false;

            proc.regs.AL = 0x40;
            ror.Impl(ref ins);

            Assert.IsFalse(proc.regs.FLAGSB.OF, "OF should be clear when high bits are same");
        }
    }
}

