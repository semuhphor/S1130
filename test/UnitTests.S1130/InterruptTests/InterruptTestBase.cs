using S1130.SystemObjects;
using S1130.SystemObjects.InterruptManagement;

namespace UnitTests.S1130.InterruptTests
{
	public abstract class InterruptTestBase
	{
		protected class DummyDevice : DeviceBase
		{
			public DummyDevice(ICpu cpu, byte deviceCode)
			{
				_deviceCode = deviceCode;
				CpuInstance = cpu;
			}
			private readonly byte _deviceCode;
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
				ActivateInterrupt(cpu, level, DeviceCode);
				return ActiveInterrupt;
			}

			public void ClearActiveInterrupt()
			{
				ActiveInterrupt = null;
			}
		}

		protected Cpu GetNewCpu()
		{
			return new Cpu { Iar = 0x100 };
		}

		protected void ExecuteOneInstruction(Cpu cpu)
		{
			cpu.NextInstruction();
			cpu.ExecuteInstruction();
		}

		protected Interrupt GetInterrupt(Cpu cpu, int level, byte deviceCode)
		{
			return new DummyDevice(cpu, deviceCode).GetInterrupt(cpu, level);
		}
	}
}
