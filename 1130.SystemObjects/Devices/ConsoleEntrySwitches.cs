namespace S1130.SystemObjects.Devices
{
	public class ConsoleEntrySwitches : DeviceBase
	{
		public override byte DeviceCode { get { return 0x07; } }
		public override void ExecuteIocc(ICpu cpu)
		{
			switch (cpu.IoccFunction)
			{
				case DevFuction.Read:
					cpu[cpu.IoccAddress] = cpu.ConsoleSwitches;
					break;
			}
		}
	}
}