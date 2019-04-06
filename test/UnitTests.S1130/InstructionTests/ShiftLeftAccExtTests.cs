using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class ShiftLeftAccExtTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SLT_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x83);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x0001, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x0008, expectedExt: 0x0120, expectedCarry: false);
		}

		[Fact]
		public void Execute_SLT_Carry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x84;
			ExecAndTest(initialAcc: 0x1234, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x2340, expectedExt: 0x0240, expectedCarry: true);
		}

		[Fact]
		public void Execute_SLT_ClearAcc_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 3, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x90;
			ExecAndTest(initialAcc: 0x1234, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x0024, expectedExt: 0x0000, expectedCarry: false);
		}

		[Fact]
		public void Execute_SLT_IgnoresDisplacmentWithTag()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 3, 0x02);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x0081;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0001, initialCarry: true, expectedAcc: 0x0000, expectedExt: 0x0002, expectedCarry: false);
		}

		[Fact]
		public void Execute_SLT_Full63BitShift_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0xbf);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x1234, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x0000, expectedExt: 0x0000, expectedCarry: false);
		}

		[Fact]
		public void Execute_SLT_NoShift()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x80);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0;
			ExecAndTest(initialAcc: 0x1234, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x1234, expectedExt: 0x0024, expectedCarry: true);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x80);
		}

		protected override string OpName
		{
			get { return "SL"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftLeft; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}