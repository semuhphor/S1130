using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class AndTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_And_Short_NoTag_PositiveOffset()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.And, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0x4210, expectedAcc: 0x0210);
		}

		[TestMethod]
		public void Execute_And_Short_Xr2_PositiveOffset()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.And, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[TestMethod]
		public void Execute_And_Long_Xr2_PositiveOffset()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[TestMethod]
		public void Execute_And_Long()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[TestMethod]
		public void Execute_And_Indirect_Xr2_PositiveOffset()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.And, 2, 0x1010, InsCpu);
		}

		protected override string OpName
		{
			get { return "AND"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.And; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}