using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class ShiftRightAccTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SRA()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 0, 0x03);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x1000, expectedAcc: 0x0200);
		}

		[Fact]
		public void Execute_SRA_XR1()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x05;
			ExecAndTest(initialAcc: 0x1020, expectedAcc: 0x0081);
		}

		[Fact]
		public void Execute_SRA_XR1_15bits()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x0f;
			ExecAndTest(initialAcc: 0x8000, expectedAcc: 0x0001);
		}

		[Fact]
		public void Execute_SRA_XR1_ClearsAcc()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x3f;
			ExecAndTest(initialAcc: 0x8000, expectedAcc: 0x0000);
		}

		[Fact]
		public void Execute_SRA_XR3_0BitsNOP()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x00;
			ExecAndTest(initialAcc: 0x4312, expectedAcc: 0x4312);
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