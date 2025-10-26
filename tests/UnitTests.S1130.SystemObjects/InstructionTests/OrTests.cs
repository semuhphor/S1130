using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class OrTests : InstructionTestBase
	{
		[Fact]
		public void Execute_Or_Short_NoTag_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Or, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0x4210, expectedAcc: 0x5234);
		}

		[Fact]
		public void Execute_Or_Short_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Or, 2, 0x10, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[Fact]
		public void Execute_Or_Long_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[Fact]
		public void Execute_Or_Long()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			
			InsCpu[0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[Fact]
		public void Execute_Or_Indirect_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
		}

		protected override string OpName
		{
			get { return "OR"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Or; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}