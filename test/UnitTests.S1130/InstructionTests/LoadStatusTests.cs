using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
	
	public class LoadStatusTests : InstructionTestBase
	{

		[Fact]
		public void Execute_LDS_Short_NeitherCarryNorOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x00);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_LDS_Short_CarryNotOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = true;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x02);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_LDS_Short_OverflowNotCarry()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x01);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.False(InsCpu.Carry);
			Assert.True(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_LDS_Short_BothCarryAndOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x03);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Carry);
			Assert.True(InsCpu.Overflow);
		}

		[Fact]
		public void Execute_LDS_Long_StillIsShort()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			var iar = InsCpu.Iar;
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadStatus, 0, 0x03, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.Equal(iar+1, InsCpu.Iar);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadStatus, 0, 0x03, InsCpu);
		}

		protected override string OpName
		{
			get { return "LDS"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.LoadStatus; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}