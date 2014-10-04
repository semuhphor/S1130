using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	public abstract class InterruptTestBase
	{
		protected Cpu InsCpu;

		protected class DummyDevice : InterruptingDeviceBase
		{
			private readonly int _interruptLevel;
			public DummyDevice(int interruptLevel)
			{
				_interruptLevel = interruptLevel;
			}

			public override int InterruptLevel
			{
				get { return _interruptLevel; }
			}

			public override int InterruptLevelStatusWordBit
			{
				get { throw new NotImplementedException(); }
			}
		}

		[TestInitialize]
		public void BeforeEachTest()
		{
			InsCpu = new Cpu(new SystemState { Iar = 0x100 });
		}
	}
}
