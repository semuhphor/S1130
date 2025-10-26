using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
    public class LoadTests : InstructionTestBase
    {
		[Fact]
		public void Execute_LD_Short_NoTag()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.Load, 0, 0x10, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x10] = 0x1234;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x1234, InsCpu.Acc);
		}

		[Fact]
		public void Execute_LD_Short_NoTag_HighAddress()
		{
			BeforeEachTest();
			InsCpu.Iar = 0x7fe0;
			InstructionBuilder.BuildShortAtIar(OpCodes.Load, 0, 0x04, InsCpu);
			
			InsCpu[InsCpu.Iar + 0x04] = 0x1234;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x1234, InsCpu.Acc);
		}

		[Fact]
        public void Execute_LD_Short_NoTag_NegativeDisplacement()
        {
			BeforeEachTest();
            InstructionBuilder.BuildShortAtIar(OpCodes.Load, 0, 0xe0, InsCpu);
            
	        InsCpu[InsCpu.Iar + GetOffsetFor(0xe0)] = 0x82e0;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x82e0, InsCpu.Acc);
        }

        [Fact]
        public void Execute_LD_Short_Xr2()
        {
			BeforeEachTest();
            InstructionBuilder.BuildShortAtIar(OpCodes.Load, 2, 0x10, InsCpu);
            
	        InsCpu.Xr[2] = 0x400;
			InsCpu[InsCpu.Xr[2] + 0x10] = 0x1222;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1222, InsCpu.Acc);
        }

        [Fact]
        public void Execute_LD_Long_NoTag()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.Load, 0, 0x400, InsCpu);
            
            InsCpu[0x400] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu.Acc);
        }

        [Fact]
        public void Execute_LD_Long_Xr3()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.Load, 3, 0x350, InsCpu);
            
            InsCpu.Xr[3] = 0x100;
            InsCpu[0x450] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu.Acc);
        }

        [Fact]
        public void Execute_LD_Long_Indirect_XR1()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Load, 1, 0x400, InsCpu);
            
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu[0x600] = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu.Acc);
        }

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Load, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "LD"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Load; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}