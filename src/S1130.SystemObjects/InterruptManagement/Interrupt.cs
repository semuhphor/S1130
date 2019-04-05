namespace S1130.SystemObjects.InterruptManagement
{
	public class Interrupt
	{
		internal Interrupt()
		{
		}

		public int InterruptLevel { get; private set; }
		public ushort Ilsw { get; private set; }
		public IDevice CausingDevice { get; private set; }
		public bool InBag { get; private set; }

		internal Interrupt Setup(int interruptLevel, IDevice deviceCausingInterrupt, ushort interruptLevelStatusWord)
		{
			InterruptLevel = interruptLevel;
			CausingDevice = deviceCausingInterrupt;
			Ilsw = interruptLevelStatusWord;
			InBag = false;
			return this;
		}

		internal void BagMe()
		{
			if (!InBag)
			{
				InBag = true;
				InterruptPool.GetPool().Add(this);
			}
		}
	}
}