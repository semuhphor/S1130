using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ShiftLeftAndCountAccTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SLCA_NoCarry()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x43);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x2000, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_SLCA_XR1_Counts()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x45;
			ExecAndTest(initialAcc: 0x1400, initialCarry: false, expectedAcc: 0xA000, expectedCarry: true);
			Assert.Equal(2, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLCA_XR1_Counts_63Bits()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x7f;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, expectedAcc: 0x8000, expectedCarry: true);
			Assert.Equal(62, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLCA_XR1_Counts_63BitsFindsNothing()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x7f;
			ExecAndTest(initialAcc: 0x0000, initialCarry: false, expectedAcc: 0x0000, expectedCarry: false);
			Assert.Equal(0, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_SLCA_XR3_CountGoesZero_BitShiftsToHigh()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 3, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x45;
			ExecAndTest(initialAcc: 0x0400, initialCarry: true, expectedAcc: 0x8000, expectedCarry: false);
			Assert.Equal(0, InsCpu.Xr[3]);
		}

		[Fact]
		public void Execute_SLCA_XR2_CountNotEnoughToMoveToHighBit()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x45;
			ExecAndTest(initialAcc: 0x0100, initialCarry: true, expectedAcc: 0x2000, expectedCarry: false);
			Assert.Equal(0, InsCpu.Xr[2]);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 2, 0x00);
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