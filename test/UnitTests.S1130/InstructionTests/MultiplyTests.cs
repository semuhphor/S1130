using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class MultiplyTests : InstructionTestBase
	{
		[Fact]
		public void Execute_M_Short_NoTag_PositiveOffset()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Multiply, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0100;
			ExecAndTest(initialAcc: 0x2001, expectedAccExt: 0x00200100);
		}

		[Fact]
		public void Execute_M_Short_NoTag_PositiveOffset_ResultNegative()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Multiply, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x0001, expectedAccExt: 0xffffffff);
		}

		[Fact]
		public void Execute_M_Short_NoTag_PositiveOffset_ResultPositive()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Multiply, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0xffff, expectedAccExt: 0x00000001);
		}

		[Fact]
		public void Execute_M_Short_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Multiply, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0xFFFF;
			ExecAndTest(initialAcc: 0xFFFF, expectedAccExt: 1);
		}

		[Fact]
		public void Execute_M_Long_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Multiply, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x8000;
			ExecAndTest(initialAcc: 0x8000, expectedAccExt: 0x40000000);
		}

		[Fact]
		public void Execute_M_Long()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Multiply, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1010] = 0x6014;
			ExecAndTest(initialAcc: 0x0000, expectedAccExt: 0);
		}

		[Fact]
		public void Execute_M_Indirect_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Multiply, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1715;
			ExecAndTest(initialAcc: 0x2333, expectedAccExt: 0x032c782f);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Multiply, 2, 0x1010, InsCpu);
		}

		protected override string OpName
		{
			get { return "M"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Multiply; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}