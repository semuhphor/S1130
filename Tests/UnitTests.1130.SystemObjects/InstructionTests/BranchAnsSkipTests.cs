using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class BranchAnsSkipTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_BSC_ZPM_AlwaysJumps()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShortBranch(OpCodes.BranchSkip, 0, BranchInstructionBase.Plus|BranchInstructionBase.Zero|BranchInstructionBase.Minus);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
		}
	}
}