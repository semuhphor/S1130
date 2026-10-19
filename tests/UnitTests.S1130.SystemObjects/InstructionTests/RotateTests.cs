using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class RotateTests : InstructionTestBase
	{
		[Fact]
		public void Execute_RTE()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 0, 0xC3, InsCpu);
			
			ExecAndTest(initialAccExt: 0x10000201, expectedAccExt: 0x22000040);
		}

		[Fact]
		public void Execute_RTE_XR1_SwapAccExt()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 1, 0xC0, InsCpu);
			
			InsCpu.Xr[1] = 0x10;
			ExecAndTest(initialAccExt: 0x10210570, expectedAccExt: 0x005701021);
		}

		[Fact]
		public void Execute_RTE_XR1_15bits()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0xC0, InsCpu);
			
			InsCpu.Xr[2] = 0x1F;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000001);
		}

		[Fact]
		public void Execute_RTE_XR1_ClearsAcc()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0xC0, InsCpu);
			
			InsCpu.Xr[2] = 0x3F;
			ExecAndTest(initialAccExt: 0x80000002, expectedAccExt: 0x00000005);
		}

		[Fact]
		public void Execute_RTE_XR3_0BitsNOP()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0xC0, InsCpu);
			
			InsCpu.Xr[2] = 0x00;
			ExecAndTest(initialAccExt: 0x43121234, expectedAccExt: 0x43121234);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x00, InsCpu);
		}

		protected override string OpName
		{
			get { return "SR"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftRight; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}