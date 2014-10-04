namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		protected ushort Address;
		protected ushort Device;
		protected DevFuction Func;
		protected byte Modifier;

		protected void DecodeIocc(ISystemState state, ushort ioccAddress)
		{
			Address = state[ioccAddress++];
			Device = (ushort) ((state[ioccAddress] >> 11) & 0x1f);
			Func = (DevFuction) ((state[ioccAddress] >> 8) & 0x7);
			Modifier = (byte) (state[ioccAddress] & 0xff);
		}

		public virtual byte DeviceCode { get; private set; }
		public virtual void ExecuteIocc(ISystemState state, ushort ioccAddress)
		{
			throw new System.NotImplementedException();
		}
	}
}