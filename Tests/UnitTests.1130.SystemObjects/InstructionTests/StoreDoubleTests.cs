using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class StoreDoubleTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_STD_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(Instructions.StoreDouble, 0, 0x10);
            InsCpu.Acc = 0x2345;
            InsCpu.Ext = 0x1234;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            int effectiveAddress = InsCpu.Iar + 0x10;
            Assert.AreEqual(0x2345, InsCpu[effectiveAddress++]);
            Assert.AreEqual(0x1234, InsCpu[effectiveAddress]);
        }
    }
}