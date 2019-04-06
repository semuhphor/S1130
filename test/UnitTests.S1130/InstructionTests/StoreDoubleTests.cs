using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.InstructionTests
{
    
    public class StoreDoubleTests : InstructionTestBase
    {
        [Fact]
        public void Execute_STD_Short_NoTag()
        {
			BeforeEachTest();
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x09);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.Ext = 0x1234;
            InsCpu.ExecuteInstruction();
            int effectiveAddress = InsCpu.Iar + 0x09;
            Assert.Equal(0x2345, InsCpu[effectiveAddress++]);
            Assert.Equal(0x1234, InsCpu[effectiveAddress]);
        }

        [Fact]
        public void Execute_ST_Long_NoTag()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.StoreDouble, 0, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0xbfbf;
            InsCpu.Ext = 0xfbfb;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0xbfbf, InsCpu[0x400]);
            Assert.Equal(0xfbfb, InsCpu[0x401]);
        }

        [Fact]
        public void Execute_ST_Long_Xr3()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.StoreDouble, 3, 0x350, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[3] = 0x100;
            InsCpu.Acc = 0x1234;
            InsCpu.Ext = 0x4234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu[0x450]);
            Assert.Equal(0x4234, InsCpu[0x451]);
        }

        [Fact]
        public void Execute_ST_Long_Indirect_XR1()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreDouble, 1, 0x400, InsCpu);
            InsCpu.NextInstruction();
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu.Acc = 0x1234;
            InsCpu.Ext = 0x0955;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu[0x600]);
            Assert.Equal(0x0955, InsCpu[0x601]);
        }

        [Fact]
        public void Execute_STD_Short_NoTag_OddAddress()
        {
			BeforeEachTest();
            InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x10);
            InsCpu.NextInstruction();
            InsCpu.Acc = 0x2345;
            InsCpu.Ext = 0x1234;
            InsCpu.ExecuteInstruction();
            var effectiveAddress = InsCpu.Iar + 0x10;
            Assert.Equal(0x2345, InsCpu[effectiveAddress++]);
            Assert.Equal(0, InsCpu[effectiveAddress]);
        }

	    protected override void BuildAnInstruction()
	    {
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreDouble, 0, 0x10);
		}

	    protected override string OpName
	    {
		    get{ return "STD"; }
	    }

	    protected override OpCodes OpCode
	    {
		    get { return OpCodes.StoreDouble; }
	    }

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}