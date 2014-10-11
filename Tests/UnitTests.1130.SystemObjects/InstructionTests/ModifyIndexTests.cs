using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ModifyIndexTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_MDX_Short_BranchAhead()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x111, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_MDX_Long_IncrementMemory_NoSkip()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0x10, 0x1000, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1000] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0x410, InsCpu[0x1000]);
		}

		[TestMethod]
		public void Execute_MDX_Long_XR1_IncrementIndex_NoSkip()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.ModifyIndex, 1, 0x1022, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x1400;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0x2422, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Long_XR2_IncrementIndex_Skip()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.ModifyIndex, 2, 0x1022, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x103, InsCpu.Iar);
			Assert.AreEqual(0x1021, InsCpu.Xr[2]);
		}

		[TestMethod]
		public void Execute_MDX_Long_Indirect_XR1_DecrementIndex_NoSkip()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ModifyIndex, 1, 0x1022, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x1400;
			InsCpu[0x1022] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0x13ff, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Long_Indirect_XR2_IncrementIndex_Skip()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.ModifyIndex, 2, 0x1022, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xffff;
			InsCpu[0x1022] = 0x0020;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x103, InsCpu.Iar);
			Assert.AreEqual(0x1f, InsCpu.Xr[2]);
		}

		[TestMethod]
		public void Execute_MDX_Long_DecrementMemory_NoSkip()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0xF4, 0x1000, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1000] = 0x400;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0x3f4, InsCpu[0x1000]);
		}

		[TestMethod]
		public void Execute_MDX_Long_DecrementMemory_Skip()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0xF4, 0x1000, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1000] = 2;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x103, InsCpu.Iar);
			Assert.AreEqual(0xfff6, InsCpu[0x1000]);
		}

		[TestMethod]
		public void Execute_MDX_Long_IncrementMemory_Skip()
		{
			InstructionBuilder.BuildLongBranchAtIar(OpCodes.ModifyIndex, 0, 0x4, 0x1000, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1000] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x103, InsCpu.Iar);
			Assert.AreEqual(0x0003, InsCpu[0x1000]);
		}

		[TestMethod]
		public void Execute_MDX_Short_BranchBack()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 0, 0xfb);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x0fc, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_MDX_Short_NoBranch_NOP()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 0, 0);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR1_Decrement_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 1, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 2;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
			Assert.AreEqual(1, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR1_Decrement_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 1, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 1;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR1_DecrementFromZero_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 1, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0xffff, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR1_DecrementFromNegative_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 1, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x101, InsCpu.Iar);
			Assert.AreEqual(0xfffe, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR2_IncrementFromNegative_Skip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 2, 0x01);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xffff;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x102, InsCpu.Iar);
			Assert.AreEqual(0x0000, InsCpu.Xr[2]);
		}

		[TestMethod]
		public void Execute_MDX_Short_XR2_IncrementFromZero_NoSignChange_NoSkip()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 2, 0x01);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x0000;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(1, InsCpu.Xr[2]);
			Assert.AreEqual(0x101, InsCpu.Iar);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ModifyIndex, 2, 0x01);
		}

		protected override string OpName
		{
			get { return "MDX"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ModifyIndex; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}