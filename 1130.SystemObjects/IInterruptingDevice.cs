namespace S1130.SystemObjects
{
	public interface IInterruptingDevice : IDevice
	{
		int InterruptLevel { get; }
		int InterruptLevelStatusWordBit { get; }
	}
}