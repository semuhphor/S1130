namespace S1130.SystemObjects
{
	public abstract class DeviceBase
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
	}
	public class ConsoleEntrySwitches : DeviceBase, IDevice
	{
		public byte DeviceCode { get { return 0x07; } }

		public void ExecuteIocc(ISystemState state, ushort ioccAddress)
		{
			DecodeIocc(state, ioccAddress);
			switch (Func)
			{
				case DevFuction.Read:
					state[Address] = state.ConsoleSwitches;
					break;
			}
		}
	}
}