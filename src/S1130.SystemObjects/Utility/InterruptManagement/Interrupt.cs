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
		public bool InBag { get; internal set; }
		public bool Pending { get; internal set; }						// true while the request is in its level queue (contributes to the ILSW)

		internal Interrupt Setup(int interruptLevel, IDevice deviceCausingInterrupt, ushort interruptLevelStatusWord)
		{
			InterruptLevel = interruptLevel;
			CausingDevice = deviceCausingInterrupt;
			Ilsw = interruptLevelStatusWord;
			InBag = false;
			Pending = false;
			return this;
		}
	}
}