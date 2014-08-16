using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using FakeItEasy;

namespace UnitTests.S1130.SystemObjects
{
    [TestClass]
    public class CpuTests
    {
        private SystemState _state;
        private Cpu _cpu;
        private const ushort IarDefault = 0x100;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _state = new SystemState { Iar = 0x100 };
            _cpu = new Cpu(_state);
        }

        [TestMethod]
        public void AccProperty()
        {
            _cpu.Acc = 0x1234;
            Assert.AreEqual(0x1234,_state.Acc);
            _state.Acc = 0x2345;
            Assert.AreEqual(0x2345, _cpu.Acc);
        }

        [TestMethod]
        public void ExtProperty()
        {
            _cpu.Ext = 0x1234;
            Assert.AreEqual(0x1234, _state.Ext);
            _state.Ext = 0x2345;
            Assert.AreEqual(0x2345, _cpu.Ext);
        }

        [TestMethod]
        public void IarProperty()
        {
            _cpu.Iar = 0x1234;
            Assert.AreEqual(0x1234, _state.Iar);
            _state.Iar = 0x2345;
            Assert.AreEqual(0x2345, _cpu.Iar);
        }

        [TestMethod]
        public void AtIarProperty()
        {
            _cpu.AtIar = 0x1234;
            Assert.AreEqual(0x1234, _cpu[IarDefault]);
            _cpu[IarDefault] = 0x3245;
            Assert.AreEqual(0x3245, _cpu.AtIar);
        }

        [TestMethod]
        public void IndexProperty()
        {
            _cpu[0x100] = 0x1234;
            Assert.AreEqual(0x1234, _state.Memory[0x100]);
            _state[0x101] = 0x2345;
            Assert.AreEqual(0x2345, _cpu[0x101]);
        }

        [TestMethod]
        public void EnusureCpuCallsStateNextInstruction()
        {
            var fakeState = A.Fake<ISystemState>();
            var cpu = new Cpu(fakeState);
            cpu.NextInstruction();
            A.CallTo(() => fakeState.NextInstruction()).MustHaveHappened();
        }
    }
}
