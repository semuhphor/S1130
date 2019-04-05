using System;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace Tests
{
    public abstract class InstructionTestBase
    {
	    protected ICpu InsCpu;
		private readonly Random _rand = new Random();

	    protected int GetOffsetFor(int x)
	    {
		    return (sbyte) x;
	    }

		protected bool RandomBool { get { return ((_rand.Next() & 1) == 0); } }

        protected void BeforeEachTest()
        {
            InsCpu = new Cpu { Iar = 0x100 };
        }

	    protected abstract void BuildAnInstruction();
		protected abstract string OpName { get; }
		protected abstract OpCodes OpCode { get; }
	    public abstract void NameAndOpcodeTest();

		protected void CheckNameAndOpcode()
		{
			BuildAnInstruction();
			InsCpu.NextInstruction();
		    Assert.Equal(OpCode, InsCpu.CurrentInstruction.OpCode);
			Assert.Equal(OpName, InsCpu.CurrentInstruction.OpName);
	    }

	    protected virtual void ExecAndTest(ushort expectedAcc, ushort expectedExt, bool expectedCarry, bool expectedOverflow, ushort initialAcc, ushort initialExt, bool initialCarry, bool initialOverflow)
	    {
		    InsCpu.Acc = initialAcc;
		    InsCpu.Ext = initialExt;
		    InsCpu.Carry = initialCarry;
		    InsCpu.Overflow = initialOverflow;
		    InsCpu.ExecuteInstruction();
		    Assert.Equal(expectedAcc, InsCpu.Acc);
		    Assert.Equal(expectedExt, InsCpu.Ext);
		    Assert.Equal(expectedCarry, InsCpu.Carry);
		    Assert.Equal(expectedOverflow, InsCpu.Overflow);
	    }

	    protected virtual void ExecAndTest(ushort expectedAcc, ushort expectedExt, bool expectedCarry, ushort initialAcc, ushort initialExt, bool initialCarry)
	    {
		    InsCpu.Acc = initialAcc;
		    InsCpu.Ext = initialExt;
		    InsCpu.Carry = initialCarry;
		    InsCpu.Overflow = true;
		    InsCpu.ExecuteInstruction();
		    Assert.Equal(expectedAcc, InsCpu.Acc);
		    Assert.Equal(expectedExt, InsCpu.Ext);
		    Assert.Equal(expectedCarry, InsCpu.Carry);
		    Assert.True(InsCpu.Overflow);
	    }

		protected void ExecAndTest(ushort expectedAcc, bool expectedCarry, bool expectedOverflow, ushort initialAcc, bool initialCarry, bool initialOverflow)
		{
			InsCpu.Acc = initialAcc;
			InsCpu.Carry = initialCarry;
			InsCpu.Overflow = initialOverflow;
			InsCpu.ExecuteInstruction();
			Assert.Equal(expectedAcc, InsCpu.Acc);
			Assert.Equal(expectedCarry, InsCpu.Carry);
			Assert.Equal(expectedOverflow, InsCpu.Overflow);
		}

		protected void ExecAndTest(ushort expectedAcc, bool expectedCarry, ushort initialAcc, bool initialCarry)
		{
			InsCpu.Acc = initialAcc;
			InsCpu.Carry = initialCarry;
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.Equal(expectedAcc, InsCpu.Acc);
			Assert.Equal(expectedCarry, InsCpu.Carry);
			Assert.True(InsCpu.Overflow);
		}

		protected void ExecAndTest(ushort expectedAcc, ushort initialAcc)
	    {
		    var overflowValue = RandomBool;
		    var carryValue = RandomBool;
		    InsCpu.Acc = initialAcc;
		    InsCpu.Carry = carryValue;
			InsCpu.Overflow = overflowValue;
		    InsCpu.ExecuteInstruction();
		    Assert.Equal(expectedAcc, InsCpu.Acc);
		    Assert.Equal(carryValue, InsCpu.Carry);
		    Assert.Equal(overflowValue, InsCpu.Overflow);
	    }

		protected void ExecAndTest(uint expectedAccExt, uint initialAccExt)
	    {
		    var overflowValue = RandomBool;
		    var carryValue = RandomBool;
		    InsCpu.AccExt = initialAccExt;
		    InsCpu.Carry = carryValue;
			InsCpu.Overflow = overflowValue;
		    InsCpu.ExecuteInstruction();
		    Assert.Equal(expectedAccExt, InsCpu.AccExt);
		    Assert.Equal(carryValue, InsCpu.Carry);
		    Assert.Equal(overflowValue, InsCpu.Overflow);
	    }

		protected void ExecAndTest(uint expectedAccExt, ushort initialAcc)
		{
			var overflowValue = RandomBool;
			var carryValue = RandomBool;
			InsCpu.Acc = initialAcc;
			InsCpu.Carry = carryValue;
			InsCpu.Overflow = overflowValue;
			InsCpu.ExecuteInstruction();
			Assert.Equal(expectedAccExt, InsCpu.AccExt);
			Assert.Equal(carryValue, InsCpu.Carry);
			Assert.Equal(overflowValue, InsCpu.Overflow);
		}

		protected void ExecAndTest(ushort expectedAcc, ushort expectedExt, bool expectedOverflow, uint initialAccExt, bool initialOverflow)
		{
			var carryValue = RandomBool;
			InsCpu.AccExt = initialAccExt;
			InsCpu.Carry = carryValue;
			InsCpu.Overflow = initialOverflow;
			InsCpu.ExecuteInstruction();
			Assert.Equal(expectedAcc, InsCpu.Acc);
			Assert.Equal(expectedExt, InsCpu.Ext);
			Assert.Equal(expectedOverflow, InsCpu.Overflow);
			Assert.Equal(carryValue, InsCpu.Carry);
		}

		protected void ExecAndTestForOverflow(uint initialAccExt)
		{
			var carryValue = RandomBool;
			InsCpu.Carry = carryValue;
			InsCpu.AccExt = initialAccExt;
			InsCpu.ExecuteInstruction();
			Assert.True(InsCpu.Overflow);
			Assert.Equal(carryValue, InsCpu.Carry);
		}
	}
}
