using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    public abstract class InstructionTestBase
    {
        protected Cpu InsCpu;

        [TestInitialize]
        public void BeforeEachTest()
        {
            InsCpu = new Cpu(new SystemState { Iar = 0x100 });
        }
    }
}
