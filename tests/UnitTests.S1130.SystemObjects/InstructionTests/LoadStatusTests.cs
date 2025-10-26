using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class LoadStatusTests : InstructionTestBase
	{

		[Fact]
		public void Execute_LDS_Short_NeitherCarryNorOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InstructionBuilder.BuildShortAtIar(OpCodes.LoadStatus, 0, 0x00, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_LDS_Short_CarryNotOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = true;
			InstructionBuilder.BuildShortAtIar(OpCodes.LoadStatus, 0, 0x02, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_LDS_Short_OverflowNotCarry()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildShortAtIar(OpCodes.LoadStatus, 0, 0x01, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.False(InsCpu.Carry);
			Assert.True(InsCpu.Overflow);
            Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_LDS_Short_BothCarryAndOverflow()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildShortAtIar(OpCodes.LoadStatus, 0, 0x03, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Carry);
			Assert.True(InsCpu.Overflow);
            Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_LDS_Long_StillIsShort()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			var iar = InsCpu.Iar;
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadStatus, 0, 0x03, InsCpu);
			
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