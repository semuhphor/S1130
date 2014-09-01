using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class OrTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_Or_Short_NoTag_PositiveOffset()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Or, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0x4210, expectedAcc: 0x5234);
		}

		[TestMethod]
		public void Execute_Or_Short_Xr2_PositiveOffset()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Or, 2, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[TestMethod]
		public void Execute_Or_Long_Xr2_PositiveOffset()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[TestMethod]
		public void Execute_Or_Long()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu[0x1010] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}

		[TestMethod]
		public void Execute_Or_Indirect_Xr2_PositiveOffset()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Or, 2, 0x1010, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[2] = 0x1000;
			InsCpu[InsCpu.Xr[2] + 0x1010] = 0x0400;
			InsCpu[0x0400] = 0x1234;
			ExecAndTest(initialAcc: 0xd210, expectedAcc: 0xd234);
		}
	}
}