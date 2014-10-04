namespace S1130.SystemObjects
{
	public abstract class InterruptingDeviceBase : DeviceBase, IInterruptingDevice
	{
		public abstract int InterruptLevel { get; }
		public abstract int InterruptLevelStatusWordBit { get; }
		public abstract void InterruptComplete();
	}
}