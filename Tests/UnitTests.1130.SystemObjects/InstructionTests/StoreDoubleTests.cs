using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class StoreDoubleTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_STD_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x09);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.Ext = 0x1234;
            InsCpu.ExecuteInstruction();
            int effectiveAddress = InsCpu.Iar + 0x09;
            Assert.AreEqual(0x2345, InsCpu[effectiveAddress++]);
            Assert.AreEqual(0x1234, InsCpu[effectiveAddress]);
        }

        [TestMethod]
        public void Execute_ST_Long_NoTag()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.StoreDouble, 0, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0xbfbf;
            InsCpu.Ext = 0xfbfb;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0xbfbf, InsCpu[0x400]);
            Assert.AreEqual(0xfbfb, InsCpu[0x401]);
        }

        [TestMethod]
        public void Execute_ST_Long_Xr3()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.StoreDouble, 3, 0x350, InsCpu);
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
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreDouble, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu.Acc = 0x1234;
            InsCpu.Ext = 0x0955;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu[0x600]);
            Assert.AreEqual(0x0955, InsCpu[0x601]);
        }

        [TestMethod]
        public void Execute_STD_Short_NoTag_OddAddress()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.Ext = 0x1234;
            InsCpu.ExecuteInstruction();
            var effectiveAddress = InsCpu.Iar + 0x10;
            Assert.AreEqual(0x2345, InsCpu[effectiveAddress++]);
            Assert.AreEqual(0, InsCpu[effectiveAddress]);
        }

	    protected override void BuildAnInstruction()
	    {
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x10);
		}

	    protected override string OpName
	    {
		    get{ return "STD"; }
	    }

	    protected override OpCodes OpCode
	    {
		    get { return OpCodes.StoreDouble; }
	    }

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			CheckNameAndOpcode();
		}
	}
}