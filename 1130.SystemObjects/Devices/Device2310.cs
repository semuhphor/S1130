using System;

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
				case DevFunction.Control:
				case DevFunction.InitWrite:
				case DevFunction.InitRead:
					CpuInstance.LetInstuctionsExecute(10);
					break;
			}
		}

		public void Mount(ICartridge cartridge)
		{
			_cartridge = cartridge;
			_cartridge.Mount();
		}

		public void UnMount()
		{
			_cartridge.Flush();
			_cartridge.UnMount();
			_cartridge = null;
		}
	}
}