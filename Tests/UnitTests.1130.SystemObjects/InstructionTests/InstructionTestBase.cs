using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
    public abstract class InstructionTestBase
    {
	    private const ushort U1 = 1;
        protected Cpu InsCpu;


	    protected int GetOffsetFor(int x)
	    {
		    return (sbyte) x;
	    }

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
    }
}
