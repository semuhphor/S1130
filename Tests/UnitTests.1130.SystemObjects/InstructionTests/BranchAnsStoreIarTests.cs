using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class BranchAnsStoreIarTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_BSI_Short_GoToRoutine()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.BranchStore, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x112, InsCpu.Iar);
			Assert.AreEqual(0x101, InsCpu[0x111]);
		}

		[TestMethod]
		public void Execute_BSI_Short_XR2_GoToRoutine()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.BranchStore, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x411, InsCpu.Iar);
			Assert.AreEqual(0x101, InsCpu[0x410]);
		}

		[TestMethod]
		public void Execute_BSI_Long_ZPM_NeverBranch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, BranchInstructionBase.Plus | BranchInstructionBase.Zero | BranchInstructionBase.Minus, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_BSI_Long_Overflow_BranchBranch_EnsureOverflowReset()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, BranchInstructionBase.Overflow, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x111, InsCpu.Iar);
			Assert.AreEqual(0x102, InsCpu[0x110]);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_BSI_Long_NoModifiers_Branch()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, 0, 0x110, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x111, InsCpu.Iar);
			Assert.AreEqual(0x102, InsCpu[0x110]);
		}
	}
}