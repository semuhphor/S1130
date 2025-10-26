using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class StoreIndexTests : InstructionTestBase
	{
		[Fact]
		public void Execute_STX_Short_Xr1()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.StoreIndex, 1, 0x10, InsCpu);
			InsCpu.Xr[1] = 0x2001;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x2001, InsCpu[0x110]);
		}

		[Fact]
		public void Execute_STX_Short_Xr2_HighAddress()
		{
			BeforeEachTest();
			InsCpu.Iar = 0x7f00;
			InstructionBuilder.BuildShortAtIar(OpCodes.StoreIndex, 2, 0x10, InsCpu);
			InsCpu.Xr[2] = 0x2345;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x2345, InsCpu[0x7f10]);
		}

		[Fact]
		public void Execute_STX_Long_NoTag()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreIndex, 0, 0x400, InsCpu);
			
			InsCpu.ExecuteInstruction();
			Assert.Equal(InsCpu[0x400], InsCpu[0x400]);
		}

		[Fact]
		public void Execute_STX_Long_Xr3()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreIndex, 3, 0x350, InsCpu);
			
			InsCpu.Xr[3] = 0x1003;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x1003, InsCpu[0x350]);
		}

		[Fact]
		public void Execute_STX_Long_Indirect_XR1()
		{
			BeforeEachTest();
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreIndex, 1, 0x400, InsCpu);
			
			InsCpu.Xr[1] = 0x1001;
			InsCpu[0x400] = 0x600;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x1001, InsCpu[0x600]);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreIndex, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STX"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.StoreIndex; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}