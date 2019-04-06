using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class ShiftLeftAccExtAndCountTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SLT_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x83);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x0001, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x0008, expectedExt: 0x0120,
				expectedCarry: false);
		}

		[Fact]
		public void Execute_SLT_Carry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xc4;
			ExecAndTest(initialAcc: 0x1000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0008, expectedCarry: true);
			Assert.Equal(1, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLT_CountGoesZero_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xdf;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0000, expectedCarry: false);
			Assert.Equal(0, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLT_CountGoesZero_CountTooSMall_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xde;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x4000, expectedExt: 0x0000, expectedCarry: false);
			Assert.Equal(0, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLT_NoDecrement_BitAlreadyThere_Carry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xde;
			ExecAndTest(initialAcc: 0x8000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0001, expectedCarry: true);
			Assert.Equal(30, InsCpu.Xr[1]);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
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