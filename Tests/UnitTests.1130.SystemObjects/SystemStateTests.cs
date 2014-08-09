using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
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
        public void NextInstruction_ShortLoadInstructionTest()
        {
            _state.Memory[0x100] = InstructionBuilder.BuildShort(Instructions.Load, 2, 0x72);
            _state.NextInstruction();
            Assert.AreEqual(0x101, _state.Iar);
            Assert.AreEqual((int) Instructions.Load, _state.Opcode);
            Assert.AreEqual(false, _state.Format);
            Assert.AreEqual(2, _state.Tag);
            Assert.AreEqual(0x72, _state.Displacement);
        }

        [TestMethod]
        public void IndexerTest()
        {
            _state[0x1000] = 0xbfbf;
            Assert.AreEqual(0xbfbf, _state.Memory[0x1000]);
            _state.Memory[0x2000] = 0xfbfb;
            Assert.AreEqual(0xfbfb, _state[0x2000]);
        }
    }
}