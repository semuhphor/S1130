using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Fakes;

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
        public void EffectiveAddress()
        {
            _cpu.AtIar = InstructionBuilder.BuildShort(Instructions.Load, 0, 0x20);
            _cpu.NextInstruction();
            Assert.AreEqual(_cpu.Iar + 0x20, _cpu.GetEffectiveAddress());
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
            bool called = false;
            ISystemState fakeState = new StubISystemState()
            {
                NextInstruction = () => { called = true; }
            };
            var cpu = new Cpu(fakeState);
            cpu.NextInstruction();
            Assert.IsTrue(called);
        }
    }
}
