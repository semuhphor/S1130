using System;
using System.Collections.Concurrent;
using S1130.SystemObjects.Utility;

namespace S1130.SystemObjects.Devices
{
	/* ******************************************************************************************
	 * Device: 2501 card reader
	 * 
	 * This device class emulates a 2501. There are only two commands accepted by the 2501: 
	 * - Initiate read: TransferToMemory 0 to 127 words from "cards" to memory
	 * - Sense device: Return the device's status word
	 * 
	 * The status word is as follows:
	 *		..1. .... .... ....		Error check (not implemented)
	 *		...1 .... .... ....		Last card (interrupt 4)
	 *		.... 1... .... ....		Operation complete (interrupt 4)
	 *		.... .... .... ..1.		Busy (read in progress)
	 *		.... .... .... ...1		Not ready or busy
	 * ******************************************************************************************/
	
	/// <summary>
	/// Emulates an IBM 2501 Card Reader.
	/// Provides card reading operations and status reporting through a concurrent card hopper.
	/// </summary>
	public class Device2501 : DeviceBase
	{
		public const ushort LastCardStatus = 0x1000;
		public const ushort OperationCompleteStatus = 0x0800;
		public const ushort BusyStatus = 0x0002;
		public const ushort NotReadyOrBusyStatus = 0x0001;

		public const ushort Ilsw = 0x1000;

		public readonly ConcurrentQueue<ICard> Hopper = new ConcurrentQueue<ICard>();

		private int _address;
		private bool _readInProgess;
		private bool _complete;
		private bool _lastCard;
		private bool _ipl;  // Track if this is an IPL operation

		public Device2501(ICpu cpu)
		{
			CpuInstance = cpu;
		}

		public override byte DeviceCode
		{
			get { return 0x09; }
		}

		/// <summary>
        /// Initiates an IPL (Initial Program Load) sequence by loading the appropriate boot card
        /// </summary>
        /// <param name="apl">If true, load APL boot card, otherwise load DMS boot card</param>
        /// <param name="privileged">If true and apl is true, load privileged APL boot card</param>
        /// <returns>True if IPL card was loaded successfully</returns>
        public bool InitiateIpl(bool apl = false, bool privileged = false)
        {
            // Clear any existing cards and reset flags
            while (Hopper.TryDequeue(out _)) { }
            _complete = false;
            _lastCard = false;
            _readInProgess = false;
            _ipl = true;

            // Get appropriate IPL card data
            ushort[] bootData = apl 
                ? (privileged ? IplCards.IPLCardAplPriv : IplCards.IPLCardApl)
                : IplCards.IPLCardDms12;

            // Create and load IPL card
            var card = new Card();
            Array.Copy(bootData, card.Columns, Math.Min(bootData.Length, 80));
            Hopper.Enqueue(card);

            // Return success
            return true;
        }

		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.SenseDevice:
					if ((CpuInstance.IoccModifiers & 1) == 1)
					{
						_complete = _lastCard = false;
						DeactivateInterrupt(CpuInstance);
					}
					CpuInstance.Acc = 0;
					if (Hopper.IsEmpty && !_complete) // Only show not ready if we're not completing an operation
					{
						CpuInstance.Acc |= NotReadyOrBusyStatus;
					}
					if (_readInProgess)
					{
						CpuInstance.Acc |= BusyStatus;
					}
					if (_complete)
					{
						CpuInstance.Acc |= OperationCompleteStatus;
					}
					if (_lastCard)
					{
						CpuInstance.Acc |= LastCardStatus;
					}
					break;
				case DevFunction.InitRead:
					if (!_readInProgess)
					{
						_address = CpuInstance.IoccAddress;
						_readInProgess = true;
					}
					break;
			}
		}

		public override void Run()
		{
			if (!Hopper.IsEmpty && _readInProgess)
			{
				ICard card;
				if (Hopper.TryDequeue(out card))
				{
					CpuInstance.TransferToMemory(_address, card.Columns, 80);
					if (_ipl)
					{
						// For IPL operations, we want to indicate both complete and last card
						_complete = true;
						_lastCard = true;
						_ipl = false; // Reset IPL flag since it's done
					}
					else
					{
						// For normal operations, only mark complete if not the last card
						_complete = !Hopper.IsEmpty;
						_lastCard = Hopper.IsEmpty;
					}
				}
			}
			_readInProgess = false;
			ActivateInterrupt(CpuInstance, 4, Ilsw);
			base.Run();
		}

		public static Device2501 operator +(Device2501 cr, Deck deck)
		{
			deck.Cards.ForEach(c => cr.Hopper.Enqueue(c));
			return cr;
		}

		public static Device2501 operator +(Device2501 cr, ICard card)
		{
			cr.Hopper.Enqueue(card);
			return cr;
		}
	}
}
