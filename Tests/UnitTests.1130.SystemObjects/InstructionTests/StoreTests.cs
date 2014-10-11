using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
    public class StoreTests : InstructionTestBase
    {
        [TestMethod]
        public void Execute_ST_Short_NoTag()
        {
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Store, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x2345, InsCpu[InsCpu.Iar + 0x10]);
        }

        [TestMethod]
        public void Execute_ST_Short_NoTag_HighAddress()
        {
	        InsCpu.Iar = 0x7f00;
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Store, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x2345, InsCpu[InsCpu.Iar + 0x10]);
        }

        [TestMethod]
        public void Execute_ST_Long_NoTag()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.Store, 0, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0xbfbf;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0xbfbf, InsCpu[0x400]);
        }

        [TestMethod]
        public void Execute_ST_Long_Xr3()
        {
            InstructionBuilder.BuildLongAtIar(OpCodes.Store, 3, 0x350, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[3] = 0x100;
            InsCpu.Acc = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu[0x450]);
        }

        [TestMethod]
        public void Execute_ST_Long_Indirect_XR1()
        {
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Store, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu.Acc = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.AreEqual(0x1234, InsCpu[0x600]);
        }

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Store, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STO"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Store; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}