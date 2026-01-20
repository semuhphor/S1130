using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{

	public class BranchAndSkipTests : InstructionTestBase
	{
		[Fact]
		public void Execute_BSC_Short_ZPM_AlwaysSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus | BranchInstructionBase.Zero | BranchInstructionBase.Minus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_ZPM_NeverBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus | BranchInstructionBase.Zero | BranchInstructionBase.Minus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_OverflowOff_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_OverflowOn_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_CarryOff_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = false;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_OverflowOff_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_OverflowOn_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSC_LongIndirect_XR1_OverflowOn_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectBranchAtIar(OpCodes.BranchSkip, 1, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = true;
			InsCpu.Xr[1] = 0x200;
			InsCpu[0x310] = 0x600;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x600, InsCpu.Iar);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSC_Long_CarryOff_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = false;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_XR2_CarryOn_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 2, BranchInstructionBase.CarryOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = true;
			InsCpu.Xr[2] = 0x500;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x610, InsCpu.Iar);
			Assert.True(InsCpu.Carry);
		}

		[Fact]
		public void Execute_BSC_Short_Even_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Even);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Odd_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Even);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_Even_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Even, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 20;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_Odd_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Even, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Plus_AccPositive_Skip()
		{
			BeforeEachTest();
			InsCpu.DecodeCurrentInstruction();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Plus_AccZero_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Plus_AccNegative_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.DecodeCurrentInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_Plus_AccPositive_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_Plus_AccZero_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Zero_AccPositive_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Zero_AccZero_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Zero_AccNegative_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Zero_AccPositive_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Zero_AccZero_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Minus_AccPositive_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Minus_AccZero_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Minus_AccNegative_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_Minus_AccZero_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_Minus_AccNegative_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_PlusEven_AccNegativeEven_Skip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus | BranchInstructionBase.Even);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Long_PlusEven_AccNegativeEven_NoBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus | BranchInstructionBase.Even, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSC_Short_CarryZero_CarryTrueAccPositive_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff | BranchInstructionBase.Zero);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = true;
			InsCpu.Acc = 0x0024;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
			Assert.True(InsCpu.Carry);
		}

		[Fact]
		public void Execute_BSC_Long_CarryZero_CarryTrueAccPositive_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff | BranchInstructionBase.Zero, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = true;
			InsCpu.Acc = 0x0024;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
			Assert.True(InsCpu.Carry);
		}

		[Fact]
		public void Execute_BSC_Short_CarryOverflowEvenZeroPositive_CarryTrueAccOverflowTrueNegativeOdd_NoSkip()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff | BranchInstructionBase.OverflowOff | BranchInstructionBase.Even | BranchInstructionBase.Zero | BranchInstructionBase.Plus);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.Acc = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
			Assert.True(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSC_Long_CarryOverflowEvenZeroPositive_CarryTrueAccOverflowTrueNegativeOdd_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.CarryOff | BranchInstructionBase.OverflowOff | BranchInstructionBase.Even | BranchInstructionBase.Zero | BranchInstructionBase.Plus, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.Acc = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
			Assert.True(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSC_Short_OverflowResetOnTest()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff);
			InsCpu.DecodeCurrentInstruction();
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSC_Long_OverflowResetOnTest()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
			Assert.Contains(OpName, InsCpu.Disassemble());
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x110, InsCpu.Iar);
			Assert.False(InsCpu.Overflow);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
		}

		protected override string OpName
		{
			get { return "BSC"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.BranchSkip; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}