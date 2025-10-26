using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class SubtractTests : InstructionTestBase
	{
		[Fact]
		public void Execute_S_Short_NoTag_Positive_NoCarryOrOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x0012;
			ExecAndTest(initialAcc: 0x0013, initialCarry: true, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_S_Short_NoTag_Negative_CarryNoOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0xFFFF;
			ExecAndTest(initialAcc: 0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: true, expectedOverflow: false);
		}

		[Fact]
		public void Execute_S_Short_NoTag_Positive_DoesNotResetOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x000f, initialCarry: false, initialOverflow: true, expectedAcc: 0x000e, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Short_NoTag_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Short_NoTag_CarryAndOverflow()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x7fff, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: true, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Short_XR3_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 3, 0x10, InsCpu);
			
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x10] = 0x0600;
			ExecAndTest(initialAcc: 0x8500, initialCarry: false, initialOverflow: false, expectedAcc: 0x7f00, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Short_XR3_NegativeOffset_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 3, 0xff, InsCpu);
			
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x0100;
			ExecAndTest(initialAcc: 0x8001, initialCarry: false, initialOverflow: false, expectedAcc: 0x7f01, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Long_XR3_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Subtract, 3,  0x0010, InsCpu);
			
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x0010] = 0x0010;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7ff0, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Long_XR3_NegativeOffset_OverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.Subtract, 3,  0xffff, InsCpu);
			
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_S_Long_Indirect_XR1_NoOverflowNoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Subtract, 1,  0x0010, InsCpu);
			
			InsCpu.Xr[1] = 0x71f4;
			InsCpu[InsCpu.Xr[1] + 0x0010] = 0x500;
			InsCpu[0x500] = 0x254;
			ExecAndTest(initialAcc: 0x0255, initialCarry: false, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: false, expectedOverflow: false);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.Subtract, 0, 0x10, InsCpu);
		}

		protected override string OpName
		{
			get { return "S"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Subtract; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}