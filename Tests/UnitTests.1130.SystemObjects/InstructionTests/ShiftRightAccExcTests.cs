using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ShiftRightAccExcTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_SRT()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 0, 0x83);
			InsCpu.NextInstruction();
			ExecAndTest(initialAccExt: 0x10000200, expectedAccExt: 0x02000040);
		}

		[TestMethod]
		public void Execute_SRT_XR1()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 1, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x85;
			ExecAndTest(initialAccExt: 0x10210000, expectedAccExt: 0x00810800);
		}

		[TestMethod]
		public void Execute_SRT_XR1_15bits()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x9f;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000001);
		}

		[TestMethod]
		public void Execute_SRT_XR1_ClearsAcc()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0xbf;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000000);
		}

		[TestMethod]
		public void Execute_SRT_XR3_0BitsNOP()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ShiftRight, 2, 0x00);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x80;
			ExecAndTest(initialAccExt: 0x43121234, expectedAccExt: 0x43121234);
		}
	}
}