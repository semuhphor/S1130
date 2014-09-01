using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class AddTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_A_Short_NoTag_Positive_NoCarryOrOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0012;
			ExecAndTest(initialAcc: 0x0001, initialCarry: true, initialOverflow: false, expectedAcc: 0x0013, expectedCarry: false, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_Positive_CarryNoOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_Positive_DoesNotResetOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x0001;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: true, expectedAcc: 0x0002, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_CarryNoOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			ExecAndTest(initialAcc: 0x0001, initialCarry: false, initialOverflow: false, expectedAcc: 0x0000, expectedCarry: true, expectedOverflow: false);
		}

		[TestMethod]
		public void Execute_A_Short_XR3_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 3, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x10] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Short_XR3_NegativeOffset_OverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 3, 0xff);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Long_XR3_OverflowNoCarry()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Add, 3,  0x0010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] + 0x0010] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Long_XR3_NegativeOffset_OverflowNoCarry()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Add, 3,  0xffff, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x71f4;
			InsCpu[InsCpu.Xr[3] - 1] = 0x4000;
			ExecAndTest(initialAcc: 0x4000, initialCarry: false, initialOverflow: false, expectedAcc: 0x8000, expectedCarry: false, expectedOverflow: true);
		}

		[TestMethod]
		public void Execute_A_Long_Indirect_XR1_NoOverflowNoCarry()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Add, 1,  0x0010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x71f4;
			InsCpu[InsCpu.Xr[1] + 0x0010] = 0x500;
			InsCpu[0x500] = 0x254;
			ExecAndTest(initialAcc: 0x4100, initialCarry: false, initialOverflow: false, expectedAcc: 0x4354, expectedCarry: false, expectedOverflow: false);
		}
	}
}