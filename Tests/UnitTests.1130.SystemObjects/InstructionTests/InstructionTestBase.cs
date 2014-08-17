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
    }
}
