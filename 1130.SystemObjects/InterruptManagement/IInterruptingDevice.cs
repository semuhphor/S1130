namespace S1130.SystemObjects.InterruptManagement
{
	public interface IInterruptingDevice : IDevice
	{
		int InterruptLevel { get; }
		int InterruptLevelStatusWordBit { get; }
		void InterruptComplete();
	}
}