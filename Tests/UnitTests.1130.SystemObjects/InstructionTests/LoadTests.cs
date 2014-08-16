using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class LoadTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_LD_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Load, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu[InsCpu.Iar + 0x10] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
        }

        [TestMethod]
        public void Execute_LD_Long_NoTag()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.Load, 0, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu[0x400] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
        }

        [TestMethod]
        public void Execute_LD_Long_Xr3()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.Load, 3, 0x350, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[3] = 0x100;
            InsCpu[0x450] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
        }

        [TestMethod]
        public void Execute_LD_Long_Indirect_XR1()
        {
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Load, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu[0x600] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
        }
    }
}