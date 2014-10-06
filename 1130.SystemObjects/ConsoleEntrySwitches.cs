namespace S1130.SystemObjects
{
	public class ConsoleEntrySwitches : DeviceBase, IDevice
	{
		public override byte DeviceCode { get { return 0x07; } }
		public override void ExecuteIocc(ICpu cpu, ushort ioccAddress)
		{
			DecodeIocc(cpu, ioccAddress);
			switch (Func)
			{
				case DevFuction.Read:
					cpu[Address] = cpu.ConsoleSwitches;
					break;
			}
		}
	}
}