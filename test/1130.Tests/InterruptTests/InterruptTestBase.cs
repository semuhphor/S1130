using S1130.SystemObjects;
using S1130.SystemObjects.InterruptManagement;

namespace Tests
{
	public abstract class InterruptTestBase
	{
		protected Cpu InsCpu;

		protected class DummyDevice : DeviceBase
		{
			public DummyDevice(ICpu cpu, byte deviceCode = 0x1f)
			{
				_deviceCode = deviceCode;
				CpuInstance = cpu;
			}
			private readonly byte _deviceCode = 0x1f;
			public override byte DeviceCode { get { return _deviceCode;  } }
			public override void ExecuteIocc()
			{
				switch (CpuInstance.IoccFunction)
				{
					case DevFunction.SenseDevice:
						if ((CpuInstance.IoccModifiers & 1) != 0)
						{
							DeactivateInterrupt(CpuInstance);
						}
						CpuInstance.Acc = DeviceCode;
						break;
				}
			}
			public Interrupt GetInterrupt(ICpu cpu, int level)
			{
				ActivateInterrupt(cpu, level, 0x1f);
				return ActiveInterrupt;
			}

			public void ClearActiveInterrupt()
			{
				ActiveInterrupt = null;
			}
		}

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
			return new DummyDevice(InsCpu).GetInterrupt(InsCpu, level);
		}
	}
}
