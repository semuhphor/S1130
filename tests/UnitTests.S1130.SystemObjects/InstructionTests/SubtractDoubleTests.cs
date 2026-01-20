using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class SubtractDoubleTests : InstructionTestBase
	{
		[Fact]
		public void Exeucte_SD_Short_NoTag_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 0, 0x0010, InsCpu);
			InsCpu[InsCpu.Iar + 0x0010] = 0x0001;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0002;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: false, expectedAcc: 0x0002, expectedExt: 0x0005, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_SD_Short_NoTag_Positive_OverflowNotReset()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 0, 0x0010, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x0010] = 0x0002;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0003;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: true, expectedAcc: 0x0001, expectedExt: 0x0004, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Exeucte_SD_Short_NoTag_Positive_NoCarryNoOverflow_OddAddress()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 0, 0x0011, InsCpu);
			InsCpu[0x111] = 0x0003;
			ExecAndTest(initialAcc: 0x0004, initialExt: 0x0002, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedExt: 0xffff, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_SD_Short_NoTag_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 0, 0x0010, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x0010] = 0x0000;
			InsCpu[InsCpu.Iar + 0x0011] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialExt: 0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedExt: 0xffff, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_SD_Short_NoTag_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 0, 0x20, InsCpu);
			
			InsCpu[0x120] = 0x0000;
			InsCpu[0x121] = 0x0001;
			ExecAndTest(initialAcc: 0x0000, initialExt:0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0xffff, expectedExt:0xffff, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_SD_Short_Xr2_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 2, 0x0024, InsCpu);
			
			InsCpu.Xr[2] = 0x0250;
			InsCpu[InsCpu.Xr[2] + 0x0024] = 0x3000;
			InsCpu[InsCpu.Xr[2] + 0x0025] = 0x0034;
			ExecAndTest(initialAcc: 0x4000, initialExt:0x0038, initialCarry: false, initialOverflow: false, expectedAcc: 0x1000, expectedExt:0x0004, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_SD_Short_Xr2_Negative_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.SubtractDouble, 2, 0x0024, InsCpu);
			
			InsCpu.Xr[2] = 0x0250;
			InsCpu[InsCpu.Xr[2] + 0x0024] = 0x0000;
			InsCpu[InsCpu.Xr[2] + 0x0025] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialExt:0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedExt:0xFFFF, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Exeucte_SD_Long_NoTag_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.SubtractDouble, 0, 0x400, InsCpu);
			
			InsCpu[0x0400] = 0x0001;
			InsCpu[0x0401] = 0x0002;
			ExecAndTest(initialAcc: 0x0003, initialExt: 0x0007, initialCarry: false, initialOverflow: false, expectedAcc: 0x0002, expectedExt: 0x0005, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_SD_Long_Xr3_Negative_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.SubtractDouble, 3, 0x400, InsCpu);
			
			InsCpu.Xr[3] = 0x0100;
			InsCpu[0x0500] = 0x0000;
			InsCpu[0x0501] = 0x0001;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0xffff, expectedExt: 0xffff, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Exeucte_SD_Indirect_Xr3_Positive_NoCarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.SubtractDouble, 3, 0x400, InsCpu);
			
			InsCpu.Xr[3] = 0x0050;
			InsCpu[0x0450] = 0x500;
			InsCpu[0x0500] = 0x0020;
			InsCpu[0x0501] = 0x0040;
			ExecAndTest(initialAcc: 0x0030, initialExt: 0x0063, initialCarry: false, initialOverflow: false, expectedAcc: 0x0010, expectedExt: 0x0023, expectedCarry: false, expectedOverflow: false);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.SubtractDouble, 3, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "SD"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.SubtractDouble; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}