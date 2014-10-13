using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.InterruptManagement;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	public abstract class InterruptTestBase
	{
		protected Cpu InsCpu;

		protected class DummyDevice : IDevice
		{
			private readonly int _interruptLevel;
			public bool InterruptCompleted;
			public DummyDevice(int interruptLevel)
			{
				_interruptLevel = interruptLevel;
			}

			public byte DeviceCode { get { return 0x1f;  } }
			public void ExecuteIocc(ICpu cpu)
			{
				throw new NotImplementedException();
			}

			public Interrupt ActiveInterrupt { get; private set; }

			public Interrupt GetInterrupt()
			{
				return InterruptPool.GetInterruptPool().GetInterrupt(_interruptLevel, this, 0x1f);
			}

			public Interrupt GenerateInterrupt()
			{
				ActiveInterrupt = InterruptPool.GetInterruptPool().GetInterrupt(_interruptLevel, this, 0x8000);
				return ActiveInterrupt;
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

		protected Interrupt GetInterrupt(int level)
		{
			return new DummyDevice(level).GenerateInterrupt();
		}
	}
}
