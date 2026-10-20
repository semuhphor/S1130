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

		/// <summary>Finishes any work that is due. Must not block: it may be called on the CPU's own thread.</summary>
		public virtual void Run()
		{
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