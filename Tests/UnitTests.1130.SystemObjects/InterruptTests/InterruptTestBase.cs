using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.InterruptManagement;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	public abstract class InterruptTestBase
	{
		protected Cpu InsCpu;

		protected class DummyDevice : InterruptingDeviceBase
		{
			private readonly int _interruptLevel;
			public bool InterruptCompleted;
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

			public override void InterruptComplete()
			{
				InterruptCompleted = false;
			}

			public override byte DeviceCode
			{
				get { return 0x1f; }
			}
		}

		[TestInitialize]
		public void BeforeEachTest()
		{
			InsCpu = new Cpu { Iar = 0x100 };
		}

		protected void ExecuteOneInstruction()
		{
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
		}
	}
}
