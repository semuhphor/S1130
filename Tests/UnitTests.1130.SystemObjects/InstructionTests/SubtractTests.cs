using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class SubtractTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_S_Short_NoTag_Positive_NoCarryOrOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0012;
			ExecAndTest(initialAcc: 0x0013, initialCarry: true, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: false, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_S_Short_NoTag_Negative_CarryNoOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xFFFF;
			ExecAndTest(initialAcc: 0x0000, initialCarry: false, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: true, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_S_Short_NoTag_Positive_DoesNotResetOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x000f, initialCarry: false, initialOverflow: true, expectedAcc: 0x000e, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Short_NoTag_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Short_NoTag_CarryAndOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x7fff, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: true, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Short_XR3_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 3, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x10] = 0x0600;
			ExecAndTest(initialAcc: 0x8500, initialCarry: false, initialOverflow: false, expectedAcc: 0x7f00, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Short_XR3_NegativeOffset_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Subtract, 3, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x0100;
			ExecAndTest(initialAcc: 0x8001, initialCarry: false, initialOverflow: false, expectedAcc: 0x7f01, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Long_XR3_OverflowNoCarry()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Subtract, 3,  0x0010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x0010] = 0x0010;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7ff0, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Long_XR3_NegativeOffset_OverflowNoCarry()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Subtract, 3,  0xffff, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x0001;
			ExecAndTest(initialAcc: 0x8000, initialCarry: false, initialOverflow: false, expectedAcc: 0x7fff, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_S_Long_Indirect_XR1_NoOverflowNoCarry()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Subtract, 1,  0x0010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x71f4;
			InsCpu[InsCpu.Xr[1] + 0x0010] = 0x500;
			InsCpu[0x500] = 0x254;
			ExecAndTest(initialAcc: 0x0255, initialCarry: false, initialOverflow: false, expectedAcc: 0x0001, expectedCarry: false, expectedOverflow: false);
		}

		private void ExecAndTest(ushort expectedAcc, bool expectedCarry, bool expectedOverflow, ushort initialAcc, bool initialCarry, bool initialOverflow)
		{
			InsCpu.Acc = initialAcc;
			InsCpu.Carry = initialCarry;
			InsCpu.Overflow = initialOverflow;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(expectedAcc, InsCpu.Acc);
			Assert.AreEqual(expectedCarry, InsCpu.Carry);
			Assert.AreEqual(expectedOverflow, InsCpu.Overflow);
		}
	}
}