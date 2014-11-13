using System;
using System.Collections.Concurrent;

namespace S1130.SystemObjects.Devices
{
	/* ******************************************************************************************
	 * Device: 2310 Single Platter Disk Drive
	 * 
	 * This device class emulates a 2319. There are four commands accepted by the 2501: 
	 * - Initiate read: Transfer 1 to 321 words from a specific sector of the current cylinder
	 * - Initiate write: Tranfer 1 to 321 words to a specific sector of the current cylinder
	 * - Control: Move the carriage to another cylinder (+/-)
	 * - Sense device: Return the device's status word
	 * 
	 * The status word is as follows:
	 *		1... .... .... ....		Data error (not implemented)
	 *		.1.. .... .... ....		Operation complete
	 *		..1. .... .... ....		Disk not ready (no file mounted/loaded)
	 *		...1 .... .... ....		Disk busy (Executing a command)
	 *		.... 1... .... ....		Carriage home (at cylinder 0)
	 *		.... .... .... ..11		Next sector
	 * ******************************************************************************************/

	public interface ICartridge
	{
		bool Mounted { get; }
		void Mount();
	}

	public class Device2310 : DeviceBase
	{
		public const ushort OperationComplete = 0x4000;
		public const ushort NotReady = 0x2000;
		public const ushort Busy = 0x1000;
		public const ushort AtCylZero = 0x0400;
		public const ushort NextSectorMask = 0x0003;

		private readonly byte _deviceCode;
		private ICartridge _cartridge;

		public Device2310(ICpu cpu) : this(cpu, 0)
		{
		}

		public Device2310(ICpu cpu, int driveNumber)
		{
			if (driveNumber < 0 || driveNumber >= 5)							// q. actual device?
			{																	// a. no .. throw error
				throw new ArgumentException("2310 drive number must be between 0 and 4");
			}
			byte[] _driveCodes = {0x4, 0x09, 0x0a, 0x0b, 0x0c};					// possible device codes ...
			_deviceCode = _driveCodes[driveNumber];								// ... set up device code
			CpuInstance = cpu;													// ... and save the cpu
		}

		public override byte DeviceCode
		{
			get { return _deviceCode; }
		}

		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.SenseDevice:
					if ((CpuInstance.IoccModifiers & 1) == 1)
					{
						DeactivateInterrupt(CpuInstance);
					}
					CpuInstance.Acc = (ushort) (_cartridge == null ? NotReady : 0);
//					if (_readInProgess)
//					{
////						CpuInstance.Acc |= BusyStatus;
//					}
//					if (_complete)
//					{
//						CpuInstance.Acc |= OperationCompleteStatus;
//					}
//					if (_lastCard)
//					{
//						CpuInstance.Acc |= LastCardStatus;
//					}
					break;
				case DevFunction.InitRead:
					break;
			}
		}

		public void Mount(ICartridge cartridge)
		{
			_cartridge = cartridge;
			_cartridge.Mount();
		}
	}

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

		public const ushort Ilsw = 0x1000;

		public readonly ConcurrentQueue<ICard> Hopper = new ConcurrentQueue<ICard>();

		private int address;
		private bool _readInProgess;
		private bool _complete;
		private bool _lastCard;

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
					if ((CpuInstance.IoccModifiers & 1) == 1)
					{
						_complete = _lastCard = false;
						DeactivateInterrupt(CpuInstance);
					}
					CpuInstance.Acc = (ushort) (Hopper.IsEmpty ? CpuInstance.Acc = NotReadyOrBusyStatus : 0);
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
						address = CpuInstance.IoccAddress;
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
					CpuInstance.Transfer(address, card.Columns, 80);
				}
			}
			_readInProgess = false;
			if (Hopper.IsEmpty)
			{
				_lastCard = true;
			}
			else
			{
				_complete = true;
			}
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
