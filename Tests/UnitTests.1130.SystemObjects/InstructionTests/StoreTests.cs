using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class StoreTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_ST_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(Instructions.Store, 0, 0x10);
            InsCpu.Acc = 0x2345;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x2345, InsCpu[InsCpu.Iar + 0x10]);
        }
    }
}