using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		protected ushort Address;
		protected ushort Device;
		protected DevFuction Func;
		protected byte Modifier;

		protected void DecodeIocc(ICpu cpu, ushort ioccAddress)
		{
			Address = cpu[ioccAddress++];
			Device = (ushort) ((cpu[ioccAddress] >> 11) & 0x1f);
			Func = (DevFuction) ((cpu[ioccAddress] >> 8) & 0x7);
			Modifier = (byte) (cpu[ioccAddress] & 0xff);
		}

		public virtual byte DeviceCode { get; private set; }
		public virtual void ExecuteIocc(ICpu cpu)
		{
			throw new System.NotImplementedException();
		}

		public Interrupt ActiveInterrupt { get; private set; }
	}
}