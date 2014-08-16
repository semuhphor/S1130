using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class LoadDoubleTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_LDD_Short_NoTag()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 0, 0x09);
            int address = InsCpu.Iar + 1 + 0x09;
            InsCpu[address++] = 0x1234;
            InsCpu[address]= 0x4567;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x4567, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LDd_Long_NoTag()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.LoadDouble, 0, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu[0x400] = 0x1234;
            InsCpu[0x401] = 0x1235;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x1235, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LDD_Long_Xr3()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.LoadDouble, 3, 0x350, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[3] = 0x500;
            InsCpu[0x850] = 0x1234;
            InsCpu[0x851] = 0x1264;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x1264, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LD_Long_Indirect_XR1()
        {
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.LoadDouble, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu[0x600] = 0x1234;
            InsCpu[0x601] = 0x4321;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x4321, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LD_Short_NoTag_OddAddress()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 0, 0x10);
            int address = InsCpu.Iar + 1 + 0x10;
            InsCpu[address++] = 0x1234;
            InsCpu[address] = 0x4567;
            InsCpu.NextInstruction();
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x1234, InsCpu.Ext);
        }
    }
}