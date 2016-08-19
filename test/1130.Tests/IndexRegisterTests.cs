using Xunit;
using S1130.SystemObjects;

namespace Tests
{
    public class IndexRegisterTests
    {
        [Fact]
        public void SystemSTateTests()
        {
            CommonTests(new Cpu());
        }

        private void CommonTests(ICpu cpu)
        {
            cpu.Xr[1] = 0xb1;
            cpu.Xr[2] = 0xb2;
            cpu.Xr[3] = 0xb3;

            Assert.Equal(0xb1, cpu[1]);
            Assert.Equal(0xb2, cpu[2]);
            Assert.Equal(0xb3, cpu[3]);

            Assert.Equal(0xb1, cpu.Xr[1]);
            Assert.Equal(0xb2, cpu.Xr[2]);
            Assert.Equal(0xb3, cpu.Xr[3]);

            cpu.Xr[0] = 0xf1;
            Assert.Equal(0xf1, cpu.Iar);
            Assert.Equal(0xf1, cpu.Xr[0]);
        }
    }
}