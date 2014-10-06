namespace S1130.SystemObjects
{
	public interface IDevice
	{
		byte DeviceCode { get; }
		void ExecuteIocc(ICpu cpu, ushort ioccAddress);
	}
}