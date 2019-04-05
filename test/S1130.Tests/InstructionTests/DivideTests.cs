using Xunit;
using S1130.SystemObjects.Instructions;

namespace Tests
{
	
	public class DivideTests : InstructionTestBase
	{
		[Fact]
		public void Execute_D_Short_NoTag_PositiveOffset()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Divide, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0100;
			ExecAndTest(initialAccExt: 0x00002001, initialOverflow: false,  expectedAcc: 0x0020, expectedExt: 0x0001,expectedOverflow:false);
		}

		[Fact]
		public void Execute_D_Short_XR2_DivideByZero()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Divide, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Iar + 0x10] = 0;
			ExecAndTestForOverflow(initialAccExt: 0x00002001);
		}

		[Fact]
		public void Execute_D_Long_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Divide, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0xfffe;
			ExecAndTest(initialAccExt: 0x00000015, initialOverflow: false, expectedAcc: 0xfff6, expectedExt: 0x0001, expectedOverflow: false);
		}

		[Fact]
		public void Execute_D_Long()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Divide, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1010] = 0x6014;
			ExecAndTest(initialAccExt: 0x00006014, initialOverflow: false, expectedAcc: 0x0001, expectedExt: 0x0000, expectedOverflow: false);
		}

		[Fact]
		public void Execute_D_Indirect_Xr2_PositiveOffset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Divide, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x0005;
			ExecAndTest(initialAccExt: 0x00001719, initialOverflow: false, expectedAcc: 0x049e, expectedExt: 0x0003, expectedOverflow: false);
		}

		[Fact]
		public void Execute_D_Indirect_Xr2_LargeNumberOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Divide, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x0005;
			ExecAndTestForOverflow(initialAccExt: 0x78921445);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Divide, 2, 0x1010, InsCpu);
		}

		protected override string OpName
		{
			get { return "D"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Divide; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}