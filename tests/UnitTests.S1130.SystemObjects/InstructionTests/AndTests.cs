using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class AndTests : InstructionTestBase
	{
		[Fact]
		public void Execute_And_Short_NoTag_PositiveOffset()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.And, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0x4210, expectedAcc: 0x0210);
		}

		[Fact]
		public void Execute_And_Short_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.And, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[Fact]
		public void Execute_And_Long_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[Fact]
		public void Execute_And_Long()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		[Fact]
		public void Execute_And_Indirect_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.And, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0x1210);
		}

		protected override void BuildAnInstruction()
		{
			BeforeEachTest();
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

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}