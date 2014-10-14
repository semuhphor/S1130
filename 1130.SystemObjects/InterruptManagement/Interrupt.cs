namespace S1130.SystemObjects.InterruptManagement
{
	public class Interrupt
	{
		internal Interrupt()
		{
		}

		public int InterruptLevel { get; private set; }
		public ushort InterruptLevelStatusWord { get; private set; }
		public IDevice CausingDevice { get; private set; }
		public bool InBag { get; private set; }

		internal Interrupt Setup(int interruptLevel, IDevice deviceCausingInterrupt, ushort interruptLevelStatusWord)
		{
			InterruptLevel = interruptLevel;
			CausingDevice = deviceCausingInterrupt;
			InterruptLevelStatusWord = interruptLevelStatusWord;
			return this;
		}

		internal void BagMe()
		{
			if (!InBag)
			{
				InBag = true;
				InterruptPool.GetInterruptPool().Add(this);
			}
		}
	}
}