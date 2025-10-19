using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
    public class TestFixture
    {
        public ICpu Cpu { get; private set; }

        public TestFixture()
        {
            Cpu = new Cpu();
        }
    }
}