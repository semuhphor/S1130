using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ShiftLeftAccExtAndCountTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_SLT_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 0, 0x83);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x0001, initialExt: 0x0024, initialCarry: true, expectedAcc: 0x0008, expectedExt: 0x0120,
				expectedCarry: false);
		}

		[TestMethod]
		public void Execute_SLT_Carry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xc4;
			ExecAndTest(initialAcc: 0x1000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0008, expectedCarry: true);
			Assert.AreEqual(1, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_SLT_CountGoesZero_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xdf;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0000, expectedCarry: false);
			Assert.AreEqual(0, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_SLT_CountGoesZero_CountTooSMall_NoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xde;
			ExecAndTest(initialAcc: 0x0000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x4000, expectedExt: 0x0000, expectedCarry: false);
			Assert.AreEqual(0, InsCpu.Xr[1]);
		}

		[TestMethod]
		public void Execute_SLT_NoDecrement_BitAlreadyThere_Carry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0xde;
			ExecAndTest(initialAcc: 0x8000, initialExt: 0x0001, initialCarry: false, expectedAcc: 0x8000, expectedExt: 0x0001, expectedCarry: true);
			Assert.AreEqual(30, InsCpu.Xr[1]);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftLeft, 1, 0x00);
		}

		protected override string OpName
		{
			get { return "SL"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftLeft; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			CheckNameAndOpcode();
		}
	}
}