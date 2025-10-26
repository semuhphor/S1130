﻿using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
    public class StoreTests : InstructionTestBase
    {
        [Fact]
        public void Execute_ST_Short_NoTag()
        {
			BeforeEachTest();
            InstructionBuilder.BuildShortAtIar(OpCodes.Store, 0, 0x10, InsCpu);
            InsCpu.Acc = 0x2345;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x2345, InsCpu[0x110]);
        }

        [Fact]
        public void Execute_ST_Short_NoTag_HighAddress()
        {
			BeforeEachTest();
	        InsCpu.Iar = 0x7f00;
            InstructionBuilder.BuildShortAtIar(OpCodes.Store, 0, 0x10, InsCpu);
            InsCpu.Acc = 0x2345;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x2345, InsCpu[0x7f10]);
        }

        [Fact]
        public void Execute_ST_Long_NoTag()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.Store, 0, 0x400, InsCpu);
            InsCpu.Acc = 0xbfbf;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0xbfbf, InsCpu[0x400]);
        }

        [Fact]
        public void Execute_ST_Long_Xr3()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongAtIar(OpCodes.Store, 3, 0x350, InsCpu);
            InsCpu.Xr[3] = 0x100;
            InsCpu.Acc = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu[0x450]);
        }

        [Fact]
        public void Execute_ST_Long_Indirect_XR1()
        {
			BeforeEachTest();
            InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Store, 1, 0x400, InsCpu);
            InsCpu.Xr[1] = 0x100;
            InsCpu[0x500] = 0x600;
            InsCpu.Acc = 0x1234;
            InsCpu.ExecuteInstruction();
            Assert.Equal(0x1234, InsCpu[0x600]);
        }

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Store, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STO"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.Store; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}