using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ShiftLeftAndCountAccTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_SLCA_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x43);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x2000, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_SLCA_XR1_Counts()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x45;
			ExecAndTest(initialAcc: 0x1400, initialCarry: false, expectedAcc: 0xA000, expectedCarry: true);
			Assert.AreEqual(2, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_SLCA_XR3_CountGoesZero_BitShiftsToHigh()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 3, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x45;
			ExecAndTest(initialAcc: 0x0400, initialCarry: true, expectedAcc: 0x8000, expectedCarry: false);
			Assert.AreEqual(0, InsCpu.Xr[3]);
		}

		[TestMethod]
		public void Execute_SLCA_XR2_CountNotEnoughToMoveToHighBit()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x45;
			ExecAndTest(initialAcc: 0x0100, initialCarry: true, expectedAcc: 0x2000, expectedCarry: false);
			Assert.AreEqual(0, InsCpu.Xr[2]);
		}
	}
}