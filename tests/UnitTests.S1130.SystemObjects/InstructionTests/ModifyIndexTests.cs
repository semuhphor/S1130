using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ModifyIndexTests : InstructionTestBase
	{
		[Fact]
		public void Execute_MDX_Short_BranchAhead()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 0, 0x10, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x111, InsCpu.Iar);
		}

		[Fact]
		public void Execute_MDX_Long_IncrementMemory_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0x10, 0x1000, InsCpu);
			
			InsCpu[0x1000] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0x410, InsCpu[0x1000]);
		}

		[Fact]
		public void Execute_MDX_Long_XR1_IncrementIndex_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.ModifyIndex, 1, 0x1022, InsCpu);
			
			InsCpu.Xr[1] = 0x1400;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0x2422, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Long_XR2_IncrementIndex_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.ModifyIndex, 2, 0x1022, InsCpu);
			
			InsCpu.Xr[2] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x103, InsCpu.Iar);
			Assert.Equal(0x1021, InsCpu.Xr[2]);
		}

		[Fact]
		public void Execute_MDX_Long_Indirect_XR1_DecrementIndex_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ModifyIndex, 1, 0x1022, InsCpu);
			
			InsCpu.Xr[1] = 0x1400;
			InsCpu[0x1022] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0x13ff, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Long_Indirect_XR2_IncrementIndex_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ModifyIndex, 2, 0x1022, InsCpu);
			
			InsCpu.Xr[2] = 0xffff;
			InsCpu[0x1022] = 0x0020;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x103, InsCpu.Iar);
			Assert.Equal(0x1f, InsCpu.Xr[2]);
		}

		[Fact]
		public void Execute_MDX_Long_DecrementMemory_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0xF4, 0x1000, InsCpu);
			
			InsCpu[0x1000] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0x3f4, InsCpu[0x1000]);
		}

		[Fact]
		public void Execute_MDX_Long_DecrementMemory_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0xF4, 0x1000, InsCpu);
			InsCpu[0x1000] = 2;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x103, InsCpu.Iar);
			Assert.Equal(0xfff6, InsCpu[0x1000]);
		}

		[Fact]
		public void Execute_MDX_Long_IncrementMemory_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0x4, 0x1000, InsCpu);
			InsCpu[0x1000] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x103, InsCpu.Iar);
			Assert.Equal(0x0003, InsCpu[0x1000]);
		}

		[Fact]
		public void Execute_MDX_Short_BranchBack()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 0, 0xfb, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x0fc, InsCpu.Iar);
		}

		[Fact]
		public void Execute_MDX_Short_NoBranch_NOP()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 0, 0, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_MDX_Short_XR1_Decrement_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 1, 0xff, InsCpu);
			InsCpu.Xr[1] = 2;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
			Assert.Equal(1, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Short_XR1_Decrement_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 1, 0xff, InsCpu);
			InsCpu.Xr[1] = 1;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Short_XR1_DecrementFromZero_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 1, 0xff, InsCpu);
			InsCpu.Xr[1] = 0;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0xffff, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Short_XR1_DecrementFromNegative_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 1, 0xff, InsCpu);
			InsCpu.Xr[1] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x101, InsCpu.Iar);
			Assert.Equal(0xfffe, InsCpu.Xr[1]);
		}

		[Fact]
		public void Execute_MDX_Short_XR2_IncrementFromNegative_Skip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 2, 0x01, InsCpu);
			InsCpu.Xr[2] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x102, InsCpu.Iar);
			Assert.Equal(0x0000, InsCpu.Xr[2]);
		}

		[Fact]
		public void Execute_MDX_Short_XR2_IncrementFromZero_NoSignChange_NoSkip()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 2, 0x02, InsCpu);
			InsCpu.Xr[2] = 0x0000;
			InsCpu.ExecuteInstruction();
			Assert.Equal(2, InsCpu.Xr[2]);
			Assert.Equal(0x101, InsCpu.Iar);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.ModifyIndex, 2, 0x01, InsCpu);
		}

		protected override string OpName
		{
			get { return "MDX"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ModifyIndex; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}