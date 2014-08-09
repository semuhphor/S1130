using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class LoadDoubleTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_LDD_Short_NoTag()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(Instructions.LoadDouble, 0, 0x10);
            int address = InsCpu.Iar + 1 + 0x10;
            InsCpu[address++] = 0x1234;
            InsCpu[address] = 0x4567;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x4567, InsCpu.Ext);
        }
    }
}