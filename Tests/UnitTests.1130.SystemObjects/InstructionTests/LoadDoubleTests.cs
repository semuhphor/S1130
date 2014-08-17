using System.Runtime.InteropServices;
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
            InsCpu.NextInstruction();
            int address = InsCpu.Iar + 0x09;
            InsCpu[address++] = 0x1234;
            InsCpu[address]= 0x4567;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x4567, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LDD_Short_XR3()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 3, 0x10);
            InsCpu.NextInstruction();
	        InsCpu.Xr[3] = 0x200;
	        int address = 0x0210;
            InsCpu[address++] = 0x1234;
            InsCpu[address]= 0x4567;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
			Assert.AreEqual(0x4567, InsCpu.Ext);
        }

        [TestMethod]
        public void Execute_LDD_Short_XR3_NegativeDisplacement()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 3, 0xF0);
            InsCpu.NextInstruction();
	        InsCpu.Xr[3] = 0x200;
	        int address = 0x01f0;
            InsCpu[address++] = 0x1234;
            InsCpu[address]= 0x4567;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x4567, InsCpu.Ext);
        }

		[TestMethod]
		public void Execute_LDD_Short_NoTag_NegativeDisplacement()
		{
			InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 0, 0xF1);
			InsCpu.NextInstruction();
			int address = InsCpu.Iar + GetOffsetFor(0xf1);
			InsCpu[address++] = 0x1234;
			InsCpu[address] = 0x4567;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x1234, InsCpu.Acc);
			Assert.AreEqual(0x4567, InsCpu.Ext);
		}

		[TestMethod]
        public void Execute_LDD_Long_NoTag()
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
        public void Execute_LDD_Long_Indirect_XR1()
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
        public void Execute_LDD_Short_NoTag_OddAddress()
        {
            InsCpu[InsCpu.Iar] = InstructionBuilder.BuildShort(OpCodes.LoadDouble, 0, 0x10);
            InsCpu.NextInstruction();
            int address = InsCpu.Iar + 0x10;
            InsCpu[address++] = 0x1234;
            InsCpu[address] = 0x4567;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu.Acc);
            Assert.AreEqual(0x1234, InsCpu.Ext);
        }

		[TestMethod]
		public void Execute_LDD_Long_Xr3_OddAddress()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadDouble, 3, 0x351, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x500;
			InsCpu[0x851] = 0x1234;
			InsCpu[0x852] = 0x1264;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x1234, InsCpu.Acc);
			Assert.AreEqual(0x1234, InsCpu.Ext);
		}
	}
}