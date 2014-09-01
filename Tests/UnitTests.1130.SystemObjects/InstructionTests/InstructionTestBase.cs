using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    public abstract class InstructionTestBase
    {
	    private const ushort U1 = 1;
        protected Cpu InsCpu;
		private readonly Random _rand = new Random();

	    protected int GetOffsetFor(int x)
	    {
		    return (sbyte) x;
	    }

		protected bool RandomBool { get { return ((_rand.Next() & 1) == 0) ? true : false; } }

	    [TestInitialize]
        public void BeforeEachTest()
        {
            InsCpu = new Cpu(new SystemState { Iar = 0x100 });
        }

	    protected virtual void ExecAndTest(ushort expectedAcc, ushort expectedExt, bool expectedCarry, bool expectedOverflow, ushort initialAcc, ushort initialExt, bool initialCarry, bool initialOverflow)
	    {
		    InsCpu.Acc = initialAcc;
		    InsCpu.Ext = initialExt;
		    InsCpu.Carry = initialCarry;
		    InsCpu.Overflow = initialOverflow;
		    InsCpu.ExecuteInstruction();
		    Assert.AreEqual(expectedAcc, InsCpu.Acc);
		    Assert.AreEqual(expectedExt, InsCpu.Ext);
		    Assert.AreEqual(expectedCarry, InsCpu.Carry);
		    Assert.AreEqual(expectedOverflow, InsCpu.Overflow);
	    }

	    protected void ExecAndTest(ushort expectedAcc, bool expectedCarry, bool expectedOverflow, ushort initialAcc, bool initialCarry, bool initialOverflow)
	    {
		    InsCpu.Acc = initialAcc;
		    InsCpu.Carry = initialCarry;
		    InsCpu.Overflow = initialOverflow;
		    InsCpu.ExecuteInstruction();
		    Assert.AreEqual(expectedAcc, InsCpu.Acc);
		    Assert.AreEqual(expectedCarry, InsCpu.Carry);
		    Assert.AreEqual(expectedOverflow, InsCpu.Overflow);
	    }

	    protected void ExecAndTest(ushort expectedAcc, ushort initialAcc)
	    {
		    var overflowValue = RandomBool;
		    var carryValue = RandomBool;
		    InsCpu.Acc = initialAcc;
		    InsCpu.Carry = carryValue;
			InsCpu.Overflow = overflowValue;
		    InsCpu.ExecuteInstruction();
		    Assert.AreEqual(expectedAcc, InsCpu.Acc);
		    Assert.AreEqual(carryValue, InsCpu.Carry);
		    Assert.AreEqual(overflowValue, InsCpu.Overflow);
	    }
    }
}
