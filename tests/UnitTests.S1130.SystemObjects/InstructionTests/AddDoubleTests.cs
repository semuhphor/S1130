using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class AddDoubleTests : InstructionTestBase
	{
		[Fact]
		public void Name()
		{
			
		}

		[Fact]
		public void Exeucte_AD_Short_NoTag_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 0, 0x0010, InsCpu);
			InsCpu[InsCpu.Iar + 0x0010] = 0x0001;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0002;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: false, expectedAcc: 0x0004, expectedExt: 0x0009, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_AD_Short_NoTag_Positive_OverflowNotReset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 0, 0x0010, InsCpu);
			InsCpu[InsCpu.Iar + 0x0010] = 0x0001;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0002;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: true, expectedAcc: 0x0004, expectedExt: 0x0009, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Exeucte_AD_Short_NoTag_Positive_NoCarryNoOverflow_OddAddress()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 0, 0x0011, InsCpu);
			InsCpu[InsCpu.Iar + 0x0011] = 0x0003;
			ExecAndTest(initialAcc: 0x0004, initialExt: 0x0002, initialCarry: false, initialOverflow: false, expectedAcc: 0x0007, expectedExt: 0x0005, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_AD_Short_NoTag_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 0, 0x0010, InsCpu);
			InsCpu[InsCpu.Iar + 0x0010] = 0x4000;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0000;
			ExecAndTest(initialAcc: 0x4000, initialExt: 0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedExt: 0x0000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_AD_Short_NoTag_Negative_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 0, 0x21, InsCpu);
			InsCpu[InsCpu.Iar + 0x21] = 0xffff;
			InsCpu[InsCpu.Iar + 0x22] = 0xffff;
			ExecAndTest(initialAcc: 0x0000, initialExt:0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedExt:0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_AD_Short_Xr2_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 2, 0x0024, InsCpu);
			InsCpu.Xr[2] = 0x0250;
			InsCpu[InsCpu.Xr[2] + 0x0024] = 0x0fff;
			InsCpu[InsCpu.Xr[2] + 0x0025] = 0xffff;
			ExecAndTest(initialAcc: 0x0000, initialExt:0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x1000, expectedExt:0x0000, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_AD_Short_Xr2_Negative_CarryAndOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.AddDouble, 2, 0x0024, InsCpu);
			InsCpu.Xr[2] = 0x0250;
			InsCpu[InsCpu.Xr[2] + 0x0024] = 0x8000;
			InsCpu[InsCpu.Xr[2] + 0x0025] = 0x0000;
			ExecAndTest(initialAcc: 0x8000, initialExt:0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedExt:0x0000, expectedCarry: true, expectedOverflow: true);
		}

		[Fact]
		public void Exeucte_AD_Long_NoTag_Positive_NoCarryNpOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.AddDouble, 0, 0x400, InsCpu);
			InsCpu[0x0400] = 0x0001;
			InsCpu[0x0401] = 0x0002;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: false, expectedAcc: 0x0004, expectedExt: 0x0009, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_AD_Long_Xr3_Negative_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.AddDouble, 3, 0x400, InsCpu);
			InsCpu.Xr[3] = 0x0100;
			InsCpu[0x0500] = 0xffff;
			InsCpu[0x0501] = 0xf100;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0fff, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedExt: 0x00ff, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_AD_Indirect_Xr3_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.AddDouble, 3, 0x400, InsCpu);
			InsCpu.Xr[3] = 0x0050;
			InsCpu[0x0450] = 0x500;
			InsCpu[0x0500] = 0x0020;
			InsCpu[0x0501] = 0x0040;
			ExecAndTest(initialAcc: 0x0001, initialExt: 0x0002, initialCarry: false, initialOverflow: false, expectedAcc: 0x0021, expectedExt: 0x0042, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.AddDouble, 3, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "AD"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.AddDouble; }
		}
	}
}