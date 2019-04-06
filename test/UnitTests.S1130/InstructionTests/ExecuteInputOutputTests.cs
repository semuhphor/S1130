using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class ExecuteInputOuputTests : InstructionTestBase
	{
		[Fact]
		public void Execute_XIO_GetConsoleSwitches()
		{
			BeforeEachTest();
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ExecuteInputOuput, 0, 0x11);
			InsCpu.NextInstruction();
			InstructionBuilder.BuildIoccAt(new ConsoleEntrySwitches(InsCpu), DevFunction.Read,0,0x500, InsCpu, 0x112);
			InsCpu.ConsoleSwitches = 0x4321;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x4321, InsCpu[0x500]);
		}

		protected override void BuildAnInstruction()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.ExecuteInputOuput, 0, 0x11);
		}

		protected override string OpName
		{
			get { return "XIO"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ExecuteInputOuput; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}