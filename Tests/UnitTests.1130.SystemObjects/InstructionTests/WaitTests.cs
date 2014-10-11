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

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Wait, 0, 0);
		}

		protected override string OpName
		{
			get { return "WAIT"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Wait; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			base.CheckNameAndOpcode();
		}
	}
}