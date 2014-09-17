using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ShiftLeftAccTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_SLA_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x03);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x0001, initialCarry: true, initialOverflow: false, expectedAcc: 0x0008, expectedCarry: false, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_SLA_Carry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 4;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, initialOverflow: true, expectedAcc: 0x2340, expectedCarry: true, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_SLA_ClearAcc_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 3, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 16;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, expectedAcc: 0x0000, expectedCarry: false);
		}

		[TestMethod]
		public void Execute_SLA_Full63BitShift_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x3f);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, expectedAcc: 0x0000, expectedCarry: false);
		}

		[TestMethod]
		public void Execute_NOP()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0;
			ExecAndTest(initialAcc: 0x1234, initialCarry: true, initialOverflow: true, expectedAcc: 0x1234, expectedCarry: true, expectedOverflow: true);
		}
	}
}