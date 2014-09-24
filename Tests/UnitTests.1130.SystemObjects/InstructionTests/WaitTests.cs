using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class WaitTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_WAIT_SetsWaitState()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Wait, 0, 0);
			InsCpu.NextInstruction();
			Assert.IsFalse(InsCpu.Wait);
			InsCpu.ExecuteInstruction();
			Assert.IsTrue(InsCpu.Wait);
		}
	}
}