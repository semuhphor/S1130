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

        [TestMethod]
        public void Execute_ST_Long_NoTag()
        {
            InstructionBuilder.BuildLongAtIar(Instructions.StoreDouble, 0, 0x400, InsCpu);
            InsCpu.Acc = 0xbfbf;
            InsCpu.Ext = 0xfbfb;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0xbfbf, InsCpu[0x400]);
            Assert.AreEqual(0xfbfb, InsCpu[0x401]);
        }

        [TestMethod]
        public void Execute_ST_Long_Xr3()
        {
            InstructionBuilder.BuildLongAtIar(Instructions.StoreDouble, 3, 0x350, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[3] = 0x100;
            InsCpu.Acc = 0x1234;
            InsCpu.Ext = 0x4234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu[0x450]);
            Assert.AreEqual(0x4234, InsCpu[0x451]);
        }

        [TestMethod]
        public void Execute_ST_Long_Indirect_XR1()
        {
            InstructionBuilder.BuildLongIndirectAtIar(Instructions.StoreDouble, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu.Acc = 0x1234;
            InsCpu.Ext = 0x0955;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu[0x600]);
            Assert.AreEqual(0x0955, InsCpu[0x601]);
        }
    }
}