using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ShiftRightAccTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_SRA()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 0, 0x03);
			InsCpu.NextInstruction();
			ExecAndTest(initialAcc: 0x1000, expectedAcc: 0x0200);
		}

		[TestMethod]
		public void Execute_SRA_XR1()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x05;
			ExecAndTest(initialAcc: 0x1020, expectedAcc: 0x0081);
		}

		[TestMethod]
		public void Execute_SRA_XR1_15bits()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x0f;
			ExecAndTest(initialAcc: 0x8000, expectedAcc: 0x0001);
		}

		[TestMethod]
		public void Execute_SRA_XR1_ClearsAcc()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x3f;
			ExecAndTest(initialAcc: 0x8000, expectedAcc: 0x0000);
		}

		[TestMethod]
		public void Execute_SRA_XR3_0BitsNOP()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x00;
			ExecAndTest(initialAcc: 0x4312, expectedAcc: 0x4312);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
		}

		protected override string OpName
		{
			get { return "SR"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftRight; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}