using System.Collections.Concurrent;
using S1130.SystemObjects.Instructions;

namespace S1130.SystemObjects.InterruptManagement
{
	public class InterruptPool
	{
		private static InterruptPool _interruptPool;
		private readonly ConcurrentBag<Interrupt> _interrupts;

		private InterruptPool()
		{
			_interrupts = new ConcurrentBag<Interrupt>();
		}

		public static InterruptPool GetInterruptPool()
		{
			if (_interruptPool == null)
			{
				_interruptPool = new InterruptPool();
			}
			return _interruptPool;
		}

		public Interrupt GetInterrupt(int interruptLevel, IDevice deviceCausingInterrupt, ushort interruptLevelStatusWord )
		{
			Interrupt item;
			if (_interrupts.TryTake(out item)) return item.Setup(interruptLevel, deviceCausingInterrupt, interruptLevelStatusWord);
			return new Interrupt().Setup(interruptLevel, deviceCausingInterrupt, interruptLevelStatusWord);
		}

		public void PutInterrupt(Interrupt item)
		{
			item.BagMe();
		}

		internal void Add(Interrupt item)
		{
			_interrupts.Add(item);
		}
	}
}

