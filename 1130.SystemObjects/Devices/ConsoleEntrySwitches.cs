namespace S1130.SystemObjects.Devices
{
	public class ConsoleEntrySwitches : DeviceBase
	{
		public override byte DeviceCode { get { return 0x07; } }

		public ConsoleEntrySwitches(ICpu cpu)
		{
			CpuInstance = cpu;
		}

		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.Read:
					CpuInstance[CpuInstance.IoccAddress] = CpuInstance.ConsoleSwitches;
					break;
			}
		}
	}
}