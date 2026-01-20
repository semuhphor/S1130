using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class AddTests : InstructionTestBase
	{
		[Fact]
		public void Execute_A_Short_NoTag_Positive_NoCarryOrOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 0, 0x10, InsCpu);
			InsCpu[InsCpu.Iar + 0x10] = 0x0012;
			ExecAndTest(initialAcc: 0x0001, initialCarry: true, initialOverflow: false, expectedAcc: 0x0013, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_A_Short_NoTag_Positive_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 0, 0x10, InsCpu);
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_A_Short_NoTag_Positive_DoesNotResetOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 0, 0x10, InsCpu);
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: true, expectedAcc: 0x0002, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Short_NoTag_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 0, 0x10, InsCpu);
			InsCpu[InsCpu.Iar + 0x10] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Short_NoTag_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 0, 0x10, InsCpu);
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_A_Short_XR3_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 3, 0x10, InsCpu);
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x10] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Short_XR3_NegativeOffset_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Add, 3, 0xff, InsCpu);
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Long_XR3_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Add, 3,  0x0010, InsCpu);
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x0010] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Long_XR3_NegativeOffset_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Add, 3,  0xffff, InsCpu);
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_A_Long_Indirect_XR1_NoOverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Add, 1,  0x0010, InsCpu);
			InsCpu.Xr[1] = 0x71f4;
			InsCpu[InsCpu.Xr[1] + 0x0010] = 0x500;
			InsCpu[0x500] = 0x254;
			ExecAndTest(initialAcc: 0x4100, initialCarry: false, initialOverflow: false, expectedAcc: 0x4354, expectedCarry: false, expectedOverflow: false);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Add, 1, 0x0010, InsCpu);
		}

		protected override string OpName
		{
			get { return "A"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Add; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}