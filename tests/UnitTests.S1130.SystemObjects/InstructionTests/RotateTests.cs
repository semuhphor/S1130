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
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 0, 0xC3);
			InsCpu.NextInstruction();
			ExecAndTest(initialAccExt: 0x10000201, expectedAccExt: 0x22000040);
		}

		[Fact]
		public void Execute_RTE_XR1_SwapAccExt()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xd0;
			ExecAndTest(initialAccExt: 0x10210570, expectedAccExt: 0x005701021);
		}

		[Fact]
		public void Execute_RTE_XR1_15bits()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xdf;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000001);
		}

		[Fact]
		public void Execute_RTE_XR1_ClearsAcc()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xff;
			ExecAndTest(initialAccExt: 0x80000002, expectedAccExt: 0x00000005);
		}

		[Fact]
		public void Execute_RTE_XR3_0BitsNOP()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xc0;
			ExecAndTest(initialAccExt: 0x43121234, expectedAccExt: 0x43121234);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
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