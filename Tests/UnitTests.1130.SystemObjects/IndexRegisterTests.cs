using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
    [TestClass]
    public class IndexRegisterTests
    {
        [TestMethod]
        public void CpuTests()
        {
            CommonTests(new Cpu(new SystemState()));
        }

        [TestMethod]
        public void SystemSTateTests()
        {
            CommonTests(new SystemState());
        }

        private void CommonTests(ISystemState state)
        {
            state.Xr[1] = 0xb1;
            state.Xr[2] = 0xb2;
            state.Xr[3] = 0xb3;

            Assert.AreEqual(0xb1, state[1]);
            Assert.AreEqual(0xb2, state[2]);
            Assert.AreEqual(0xb3, state[3]);

            Assert.AreEqual(0xb1, state.Xr[1]);
            Assert.AreEqual(0xb2, state.Xr[2]);
            Assert.AreEqual(0xb3, state.Xr[3]);

            state.Xr[0] = 0xf1;
            Assert.AreEqual(0xf1, state.Iar);
            Assert.AreEqual(0xf1, state.Xr[0]);
        }
    }
}