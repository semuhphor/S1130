using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class StoreIndexTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_STX_Short_Xr1()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreIndex, 1, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x2001;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x2001, InsCpu[InsCpu.Iar + 0x10]);
		}

		[TestMethod]
		public void Execute_STX_Short_Xr2_HighAddress()
		{
			InsCpu.Iar = 0x7f00;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreIndex, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x2345;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x2345, InsCpu[InsCpu.Iar + 0x10]);
		}

		[TestMethod]
		public void Execute_STX_Long_NoTag()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreIndex, 0, 0x400, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(InsCpu[0x400], InsCpu[0x400]);
		}

		[TestMethod]
		public void Execute_STX_Long_Xr3()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreIndex, 3, 0x350, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x1003;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x1003, InsCpu[0x350]);
		}

		[TestMethod]
		public void Execute_STX_Long_Indirect_XR1()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreIndex, 1, 0x400, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x1001;
			InsCpu[0x400] = 0x600;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x1001, InsCpu[0x600]);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreIndex, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STX"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.StoreIndex; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}