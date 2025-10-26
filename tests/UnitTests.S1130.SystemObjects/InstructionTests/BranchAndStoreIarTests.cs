using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class BranchAndStoreIarTests : InstructionTestBase
	{
		[Fact]
		public void Execute_BSI_Short_GoToRoutine()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.BranchStore, 0, 0x10, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x111, InsCpu.Iar);
			Assert.Equal(0x101, InsCpu[0x110]);
		}

		[Fact]
		public void Execute_BSI_Short_XR2_GoToRoutine()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.BranchStore, 2, 0x10, InsCpu);
			InsCpu.Xr[2] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x411, InsCpu.Iar);
			Assert.Equal(0x101, InsCpu[0x410]);
		}

		[Fact]
		public void Execute_BSI_Long_ZPM_NeverBranch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, BranchInstructionBase.Plus | BranchInstructionBase.Zero | BranchInstructionBase.Minus, 0x110, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_BSI_Long_Overflow_BranchBranch_EnsureOverflowReset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, BranchInstructionBase.OverflowOff, 0x110, InsCpu);
			
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x111, InsCpu.Iar);
			Assert.Equal(0x102, InsCpu[0x110]);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_BSI_Long_NoModifiers_Branch()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, 0, 0x110, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x111, InsCpu.Iar);
			Assert.Equal(0x102, InsCpu[0x110]);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.BranchStore, 0, 0, 0x110, InsCpu);
		}

		protected override string OpName
		{
			get { return "BSI"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.BranchStore; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}