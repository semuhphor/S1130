using System.Collections.Concurrent;

namespace S1130.SystemObjects.Devices
{
	/* ******************************************************************************************
	 * Device: 2501 card reader
	 * 
	 * This device class emulates a 2501. There are only two commands accepted by the 2501: 
	 * - Initiate read: Transfer 0 to 127 words from "cards" to memory
	 * - Sense device: Return the device's status word
	 * 
	 * The status word is as follows:
	 *		..1. .... .... ....		Error check (not implemented)
	 *		...1 .... .... ....		Last card (interrupt 4)
	 *		.... 1... .... ....		Operation complete (interrupt 4)
	 *		.... .... .... ..1.		Busy (read in progress)
	 *		.... .... .... ...1		Not ready or busy
	 * ******************************************************************************************/		
	public class Device2501 : DeviceBase
	{
		public const ushort LastCardStatus = 0x1000;
		public const ushort OperationCompleteStatus = 0x0800;
		public const ushort BusyStatus = 0x0002;
		public const ushort NotReadyOrBusyStatus = 0x0001;

		public readonly ConcurrentQueue<ICard> Hopper = new ConcurrentQueue<ICard>();

		private bool _readInProgess;

		public Device2501(ICpu cpu)
		{
			CpuInstance = cpu;
		}

		public override byte DeviceCode
		{
			get { return 0x09; }
		}

		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.SenseDevice:
					CpuInstance.Acc = (ushort) (Hopper.IsEmpty ? CpuInstance.Acc = NotReadyOrBusyStatus : 0);
					break;
				case DevFunction.InitRead:
					break;
			}
		}

		public static Device2501 operator +(Device2501 cr, Deck deck)
		{
			deck.Cards.ForEach(c => cr.Hopper.Enqueue(c));
			return cr;
		}
	}
}
