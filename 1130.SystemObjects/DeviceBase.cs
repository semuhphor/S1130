using System.Security.AccessControl;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		public virtual byte DeviceCode { get; private set; }
		public abstract void ExecuteIocc(ICpu cpu);
		public Interrupt ActiveInterrupt { get; private set; }

		protected void ActivateInterrupt(ICpu cpu, int interruptLevel, ushort interruptLevelStatusWord)
		{
			ActiveInterrupt = cpu.IntPool.GetInterrupt(interruptLevel, this, interruptLevelStatusWord);
			cpu.InterruptQueues[interruptLevel].Enqueue(ActiveInterrupt);
		}

		protected void DeactivateInterrupt(ICpu cpu)
		{
			cpu.IntPool.PutInterrupt(ActiveInterrupt);
			ActiveInterrupt = null;
		}
	}
}