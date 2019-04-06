using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class WaitTests : InstructionTestBase
	{
		[Fact]
		public void Execute_WAIT_SetsWaitState()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Wait, 0, 0);
			InsCpu.NextInstruction();
			Assert.False(InsCpu.Wait);
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Wait);
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

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}