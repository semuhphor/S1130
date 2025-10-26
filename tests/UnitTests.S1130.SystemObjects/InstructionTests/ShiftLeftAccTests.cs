using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ShiftLeftAccTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SLA_NoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 0, 0x03, InsCpu);
			
			ExecAndTest(initialAcc: 0x0001, initialCarry: true, initialOverflow: false, expectedAcc: 0x0008, expectedCarry: false, expectedOverflow: false);
		}

		[Fact]
		public void Execute_SLA_Carry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 1, 0x00, InsCpu);
			
			InsCpu.Xr[1] = 4;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, initialOverflow: true, expectedAcc: 0x2340, expectedCarry: true, expectedOverflow: true);
		}

		[Fact]
		public void Execute_SLA_IgnoreDisplacementWithTag()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 1, 0x02, InsCpu);
			
			InsCpu.Xr[1] = 1;
			ExecAndTest(initialAcc: 0x0001, initialCarry: true, initialOverflow: true, expectedAcc: 0x0002, expectedCarry: false, expectedOverflow: true);
		}

		[Fact]
		public void Execute_SLA_ClearAcc_NoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 3, 0x00, InsCpu);
			
			InsCpu.Xr[3] = 16;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, expectedAcc: 0x0000, expectedCarry: false);
		}

		[Fact]
		public void Execute_SLA_Full63BitShift_NoCarry()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 0, 0x3f, InsCpu);
			
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, expectedAcc: 0x0000, expectedCarry: false);
		}

		[Fact]
		public void Execute_NOP()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 1, 0x00, InsCpu);
			
			InsCpu.Xr[1] = 0;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, initialOverflow: true, expectedAcc: 0x1234, expectedCarry: true, expectedOverflow: true);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftLeft, 1, 0x00, InsCpu);
		}

		protected override string OpName
		{
			get { return "SL"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftLeft; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}