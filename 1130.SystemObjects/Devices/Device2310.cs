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

		#region Cylinder implementation

		#endregion

		private ushort _ilsw = 0x8000;											// default ILSW is for default drive
		private Cylinder _cylinder = new Cylinder();
		private bool _busy;

		public Device2310(ICpu cpu, int driveNumber = 0)
		{
			if (driveNumber < 0 || driveNumber >= 5)							// q. actual device?
			{																	// a. no .. throw error
				throw new ArgumentException("2310 drive number must be between 0 and 4");
			}
			byte[] _driveCodes = {0x4, 0x09, 0x0a, 0x0b, 0x0c};					// possible device codes ...
			_deviceCode = _driveCodes[driveNumber];								// ... set up device code
			_ilsw >>= driveNumber;												// ... set up ilsw (shift bit on device number)
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
					ushort newAcc = 0;
					if (_cartridge == null)
					{
						newAcc = NotReady;
					}
					else
					{
						if (_busy)
						{
							newAcc = Busy;
						}
						if (_cylinder.Current == 0)
						{
							newAcc |= AtCylZero;
						}
					}
					CpuInstance.Acc = newAcc;
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
					if (_cartridge == null)										// q. cart ready?
						break;													// a. no .. can't move
					if (!_busy && _cartridge != null)							// q. not busy & Ready to play?
					{															// a. yes...
						if (CpuInstance.IoccAddress == 0)						// q. move 0 cylinders.
							break;												// a. ok ..  we're done!
						_busy = true;											// otherwise.. make the drive busy
						var dir = CpuInstance.IoccModifiers == 0x04;			// .. dir = tru if moving to home.
						_cylinder += CpuInstance.IoccAddress * ((dir) ? -1 : 1);// calculate new cylinder
						_cartridge.MoveToCylinder(_cylinder.Current);			// .. tell the cartridge
						_busy = false;											// drive is done.
					}
					break;

				case DevFunction.InitWrite:
					break;
				case DevFunction.InitRead:
					CpuInstance.LetInstuctionsExecute(10);
					break;
			}
		}

		// Mount cartridge in drive. Assumes we turn on drive

		public void Mount(ICartridge cartridge)								// mount a cartridge
		{
			_cartridge = cartridge;												// save the cartridge
			_cartridge.Mount();													// tell the cart it's mounted
			_cylinder = new Cylinder();											// set out cylinder to zero
			_cartridge.MoveToCylinder(_cylinder.Current);						// .. let the cartridge know too
		}

		// UnMount cartridge. Save as turning off an removing

		public void UnMount()												// unmount cartridge
		{
			_cartridge.Flush();													// let it finalize any writes.
			_cartridge.UnMount();												// .. then unmount from drive
			_cartridge = null;													// .. let the drive know too
		}
	}
}