using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class ExecuteInputOuputTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_XIO_GetConsoleSwitches()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ExecuteInputOuput, 0, 0x11);
			InsCpu.NextInstruction();
			InstructionBuilder.BuildIoccAt(new ConsoleEntrySwitches(), DevFuction.Read,0,0x500, InsCpu, 0x112);
			InsCpu.ConsoleSwitches = 0x4321;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x4321, InsCpu[0x500]);
		}
	}
}