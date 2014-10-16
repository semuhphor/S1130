using System.Security.AccessControl;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		public abstract byte DeviceCode { get; }
		public abstract void ExecuteIocc(ICpu cpu);
		public Interrupt ActiveInterrupt { get; protected set; }

		protected void ActivateInterrupt(ICpu cpu, int interruptLevel, ushort interruptLevelStatusWord)
		{
			ActiveInterrupt = cpu.IntPool.GetInterrupt(interruptLevel, this, interruptLevelStatusWord);
			cpu.AddInterrupt(ActiveInterrupt);
		}

		protected void DeactivateInterrupt(ICpu cpu)
		{
			ActiveInterrupt = null;
		}
	}
}