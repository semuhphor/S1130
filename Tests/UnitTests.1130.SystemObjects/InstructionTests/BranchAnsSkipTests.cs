using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class BranchAnsSkipTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_BSC_Short_ZPM_AlwaysSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus|BranchInstructionBase.Zero|BranchInstructionBase.Minus);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_ZPM_NeverBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus | BranchInstructionBase.Zero | BranchInstructionBase.Minus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_OverflowOff_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow);
			InsCpu.NextInstruction();
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_OverflowOn_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_CarryOff_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry);
			InsCpu.NextInstruction();
			InsCpu.Carry = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_OverflowOff_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_OverflowOn_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSC_LongIndirect_XR1_OverflowOn_Branch()
		{
			InstructionBuilder.BuildLongIndirectBranchAtIar(OpCodes.BranchSkip, 1, BranchInstructionBase.Overflow, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.Xr[1] = 0x200;
			InsCpu[0x310] = 0x600;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x600, InsCpu.Iar);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSC_Long_CarryOff_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Carry = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_XR2_CarryOn_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 2, BranchInstructionBase.Carry, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Carry = true;
			InsCpu.Xr[2] = 0x500;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x610, InsCpu.Iar);
			Assert.IsTrue(InsCpu.Carry);
		}

		[TestMethod]
		public void Execute_BSC_Short_Even_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Even);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Odd_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Even);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_Even_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Even, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_Odd_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Even, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Plus_AccPositive_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Plus_AccZero_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Plus_AccNegative_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_Plus_AccPositive_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_Plus_AccZero_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Zero_AccPositive_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Zero_AccZero_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Zero_AccNegative_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Zero_AccPositive_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Zero_AccZero_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Zero, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Minus_AccPositive_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Minus_AccZero_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Minus_AccNegative_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Minus_AccZero_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_Minus_AccNegative_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Minus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_PlusEven_AccNegativeEven_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus|BranchInstructionBase.Even);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Long_PlusEven_AccNegativeEven_NoBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus|BranchInstructionBase.Even, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x8000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSC_Short_CarryZero_CarryTrueAccPositive_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry|BranchInstructionBase.Zero);
			InsCpu.NextInstruction();
			InsCpu.Carry = true;
			InsCpu.Acc = 0x0024;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
			Assert.IsTrue(InsCpu.Carry);
		}

		[TestMethod]
		public void Execute_BSC_Long_CarryZero_CarryTrueAccPositive_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry|BranchInstructionBase.Zero, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Carry = true;
			InsCpu.Acc = 0x0024;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
			Assert.IsTrue(InsCpu.Carry);
		}

		[TestMethod]
		public void Execute_BSC_Short_CarryOverflowEvenZeroPositive_CarryTrueAccOverflowTrueNegativeOdd_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry|BranchInstructionBase.Overflow|BranchInstructionBase.Even|BranchInstructionBase.Zero|BranchInstructionBase.Plus);
			InsCpu.NextInstruction();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.Acc = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
			Assert.IsTrue(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSC_Long_CarryOverflowEvenZeroPositive_CarryTrueAccOverflowTrueNegativeOdd_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Carry|BranchInstructionBase.Overflow|BranchInstructionBase.Even|BranchInstructionBase.Zero|BranchInstructionBase.Plus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.Acc = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
			Assert.IsTrue(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSC_Short_OverflowResetOnTest()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSC_Long_OverflowResetOnTest()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x110, InsCpu.Iar);
			Assert.IsFalse(InsCpu.Overflow);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchSkip, 0, BranchInstructionBase.Overflow, 0x110, InsCpu);
		}

		protected override string OpName
		{
			get { return "BSC"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.BranchSkip; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			CheckNameAndOpcode();
		}
	}
}