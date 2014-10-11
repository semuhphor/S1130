using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    [TestClass]
    public class LoadIndexTests : InstructionTestBase
    {
		[TestMethod]
		public void Execute_LDX_Short_PositiveDisplacement()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadIndex, 1, 0x10);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x10, InsCpu.Xr[1]);
		}
		[TestMethod]
		public void Execute_LDX_Short_NegativeDisplacement()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadIndex, 3, 0x80);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0xff80, InsCpu.Xr[3]);
		}

		[TestMethod]
		public void Execute_LDX_Long_NoTag()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadIndex, 0, 0x404, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x404, InsCpu.Iar);
			Assert.AreEqual(0x404, InsCpu.Xr[0]);
		}

		[TestMethod]
		public void Execute_LDX_Long_Xr3()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadIndex, 3, 0x350, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x350, InsCpu.Xr[3]);
		}

		[TestMethod]
		public void Execute_LDX_Long_Indirect_XR2()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.LoadIndex, 2, 0x400, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x400] = 0x1234;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x1234, InsCpu.Xr[2]);
		}

	    protected override void BuildAnInstruction()
	    {
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.LoadIndex, 2, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "LDX"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.LoadIndex; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}