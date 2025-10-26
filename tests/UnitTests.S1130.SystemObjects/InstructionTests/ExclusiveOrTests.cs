using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ExclusiveOrTests : InstructionTestBase
	{
		[Fact]
		public void Execute_Eor_Short_NoTag_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ExclusiveOr, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0x4210, expectedAcc: 0x5024);
		}

		[Fact]
		public void Execute_Eor_Short_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ExclusiveOr, 2, 0x10, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xc024);
		}

		[Fact]
		public void Execute_Eor_Long_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.ExclusiveOr, 2, 0x1010, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xc024);
		}

		[Fact]
		public void Execute_Eor_Long()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.ExclusiveOr, 2, 0x1010, InsCpu);
			
			InsCpu[0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xc024);
		}

		[Fact]
		public void Execute_Eor_Indirect_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ExclusiveOr, 2, 0x1010, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xc024);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ExclusiveOr, 2, 0x1010, InsCpu);
		}

		protected override string OpName
		{
			get { return "EOR"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ExclusiveOr; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}