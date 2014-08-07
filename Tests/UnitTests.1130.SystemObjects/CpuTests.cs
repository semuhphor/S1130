using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _1130.SystemObjects;

namespace UnitTests._1130.SystemObjects
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
    }

    [TestClass]
    public class SystemStateTests
    {
        private SystemState _state;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _state = new SystemState {Iar = 0x100};
        }

        [TestMethod]
        public void NextInstruction_ShortLoadInstruction()
        {
            _state.Memory[0x100] = _state.BuildShort(Instructions.Load, 2, 0x72);
            _state.NextInstruction();
            Assert.AreEqual(0x101, _state.Iar);
            Assert.AreEqual((int) Instructions.Load, _state.Opcode);
            Assert.AreEqual(false, _state.Format);
            Assert.AreEqual(2, _state.Tag);
            Assert.AreEqual(0x72, _state.Displacement);
        }

        [TestMethod]
        public void BuildShortTest_ShortLoads()
        {
            Assert.AreEqual(0xc07f, _state.BuildShort(Instructions.Load, 0, 0x7f));
            Assert.AreEqual(0xc344, _state.BuildShort(Instructions.Load, 3, 0x44));
        }
    }
}
