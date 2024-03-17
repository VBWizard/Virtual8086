using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace VirtualProcessor.Tests
{
    [TestClass]
    public class AddTests
    {
        [TestMethod]
        public void AddTest()
        {
            // Arrange
            Mock<PCSystem> mockSystem = new Mock<PCSystem>();
            uint memSize = 1024 * 1024 * 16; // set to desired memory size
            eProcTypes processorType = eProcTypes.i80386; // set to desired processor type
            Processor_80x86 processor = new Processor_80x86(mockSystem.Object, memSize, processorType);

            // set up instruction with test values
            sInstruction ins = new sInstruction();
            ins.OpCode = 0x03;
            ins.Op1Value.OpDWord = 0;
            ins.Op2Value.OpDWord = 4;

            // set up memory with test values
            processor.mem.SetDWord(processor,ref ins, 0, 0x00000002);
            processor.mem.SetDWord(processor,ref ins, 4, 0x00000003);


            // Act
            ADD add = new ADD();
            add.mProc = processor;
            add.Impl(ref ins);

            // Assert
            Assert.AreEqual(0x00000005, ins.Op1Value.OpDWord);
        }
    }
}