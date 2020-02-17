using System.Collections.Concurrent;

namespace S1130.SystemObjects.InterruptManagement
{
	public class InterruptPool 
	{
		private readonly ConcurrentBag<Interrupt> _interrupts;

		public InterruptPool()
		{
			_interrupts = new ConcurrentBag<Interrupt>();
		}

		public Interrupt GetInterrupt(int interruptLevel, IDevice deviceCausingInterrupt, ushort interruptLevelStatusWord )
		{
			Interrupt item;
			if (_interrupts.TryTake(out item)) 
			{
				return item.Setup(interruptLevel, deviceCausingInterrupt, interruptLevelStatusWord);
			}
			return new Interrupt().Setup(interruptLevel, deviceCausingInterrupt, interruptLevelStatusWord);
		}

		public void PutInterruptInBag(Interrupt item)
		{
			if (!item.InBag)
			{
				item.InBag = true;
				_interrupts.Add(item);
			}
		}

		public int Count { get { return _interrupts.Count; } }
	}
}

