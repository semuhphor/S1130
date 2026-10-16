using System.Threading;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		public abstract byte DeviceCode { get; }
		public abstract void ExecuteIocc();
		public Interrupt ActiveInterrupt { get; protected set; }

		public virtual bool HoldsInterrupt(Interrupt interrupt) => ActiveInterrupt == interrupt;

		public virtual void Run()
		{
			Thread.Sleep(0);
		}

		protected ICpu CpuInstance;

		protected void ActivateInterrupt(ICpu cpu, int interruptLevel, ushort interruptLevelStatusWord)
		{
			if (ActiveInterrupt == null)
			{
				ActiveInterrupt = cpu.IntPool.GetInterrupt(interruptLevel, this, interruptLevelStatusWord);
				cpu.AddInterrupt(ActiveInterrupt);
			}
		}

		protected void DeactivateInterrupt(ICpu cpu)
		{
			var interrupt = ActiveInterrupt;
			ActiveInterrupt = null;
			if (interrupt != null)
			{
				cpu.RemoveInterrupt(interrupt);									// request gone: its bit leaves the ILSW immediately
			}
		}
	}
}