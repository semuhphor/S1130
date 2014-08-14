using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class LoadTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_LD_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(Instructions.Load, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu[InsCpu.Iar + 0x10] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
        }
    }


}