﻿using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class StoreStatusTests : InstructionTestBase
	{
		[Fact]
		public void Execute_STS_Short_NoTag()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InstructionBuilder.BuildShortAtIar(OpCodes.StoreStatus, 0, 0x10, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x03, InsCpu[0x110] & 0x03);
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_STS_Short_NoTag_HighAddress()
		{
			BeforeEachTest();
			InsCpu.Carry = false;
			InsCpu.Overflow = true;
			InsCpu.Iar = 0x7f00;
			InstructionBuilder.BuildShortAtIar(OpCodes.StoreStatus, 0, 0x10, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x01, InsCpu[0x7f10] & 0x03);
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x7f01, InsCpu.Iar);
		}

		[Fact]
		public void Execute_STS_Long_NoTag()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreStatus, 0, 0x400, InsCpu);
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x02, InsCpu[0x0400] & 0x03);
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_STS_Long_Xr3()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreStatus, 3, 0x350, InsCpu);
			InsCpu.Xr[3] = 0x10;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x03, InsCpu[0x360] & 0x03);
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x102, InsCpu.Iar);
		}

		[Fact]
		public void Execute_STS_Long_Indirect_XR1()
		{
			BeforeEachTest();
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreStatus, 1, 0x400, InsCpu);
			InsCpu.Xr[1] = 0x100;
			InsCpu[0x500] = 0x600;
			InsCpu.Acc = 0x1234;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x02, InsCpu[0x0600] & 0x03);
			Assert.False(InsCpu.Carry);
			Assert.False(InsCpu.Overflow);
            Assert.Equal(0x102, InsCpu.Iar);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreStatus, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STS"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.StoreStatus; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}