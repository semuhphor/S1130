using System.Threading;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public abstract class DeviceBase : IDevice
	{
		public abstract byte DeviceCode { get; }
		public abstract void ExecuteIocc();
		public Interrupt ActiveInterrupt { get; protected set; }

		public virtual void Run()
		{
			Thread.Yield();
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
			ActiveInterrupt = null;
		}

		protected void LetInstuctionsExecute(ulong numberOfInstructions)
		{
			var endCount = CpuInstance.InstructionCount + numberOfInstructions;		// calculate the end count of instructions
			while (CpuInstance.InstructionCount < endCount)							// loop until we get to the count
			{
				if (CpuInstance.Wait)												// q. did we hit a wait state?
					break;															// a. yes .. leave now
				Run();																// .. let other threads run
			}
		}
	}
}