namespace S1130.SystemObjects.InterruptManagement
{
	public class Interrupt
	{
		private bool _inBag = false;

		internal Interrupt()
		{
		}

		public int InterruptLevel { get; private set; }
		public int InterruptLevelStatusWord { get; private set; }
		public IDevice CausingDevice { get; private set; }

		internal Interrupt Setup(int interruptLevel, IDevice deviceCausingInterrupt, int interruptLevelStatusWord)
		{
			InterruptLevel = interruptLevel;
			CausingDevice = deviceCausingInterrupt;
			InterruptLevelStatusWord = interruptLevelStatusWord;
			_inBag = false;
			return this;
		}

		internal void BagMe()
		{
			if (!_inBag)
			{
				_inBag = true;
				InterruptPool.GetInterruptPool().Add(this);
			}
		}
	}
}