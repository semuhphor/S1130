using System;

namespace S1130.SystemObjects.Devices
{
	/* ******************************************************************************************
	 * Device: 2310 Single Platter Disk Drive
	 * 
	 * This device class emulates a 2310. There are four commands accepted by the 2310: 
	 * - Initiate read: TransferToMemory 1 to 321 words from a specific sector of the current cylinder
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

	/// <summary>
	/// Emulates an IBM 2310 Single Platter Disk Drive.
	/// Provides read/write operations, cylinder seeking, and status reporting.
	/// </summary>
	public class Device2310 : DeviceBase
	{
		public const ushort OperationComplete = 0x4000;
		public const ushort NotReady = 0x2000;
		public const ushort Busy = 0x1000;
		public const ushort AtCylZero = 0x0800;
		public const ushort NextSectorMask = 0x0003;
		public const int InterruptLevel = 2;
		public const byte SeekForward = 0x00;
		public const byte SeekBackward = 0x40;
		public const byte ReadCheck = 0x80;
		public const int SectorMask = 0x07;

		private readonly byte _deviceCode;									// device code (calculated)

		// device status 
		private bool _cartMounted;											// true if cart is mouned
		private bool _busy;													// true if operation active
		private bool _seeking;												// true if seeing in progress
		private bool _reading;												// true if read in progress
		private bool _writing;												// true if write in progress
		private bool _complete;												// true if operation finished
		private bool _readCheck;											// true if read check requested
		private ushort _ilsw = 0x8000;										// ILSW (defaulted for drive 0)
		private CylinderTracker _cylinder = new CylinderTracker();			// current cylinder address
		private ICartridge _cartridge;										// cartridge 
		private int _seekOffset;											// number of cyliders to seek (+/-)
		private int _sector;												// sector to read
		private int _wcAddress;												// address to write from

		public Device2310(ICpu cpu, int driveNumber = 0)					// constructor cpu and drive number (0-5)		
		{
			if (driveNumber < 0 || driveNumber >= 5)							// q. actual device?
			{																	// a. no .. throw error
				throw new ArgumentException("2310 drive number must be between 0 and 4");
			}
			_deviceCode = new byte[] {0x4, 0x09, 0x0a, 0x0b, 0x0c}[driveNumber];// ... set up device code
			_ilsw >>= driveNumber;												// ... set up ilsw (shift bit on device number)
			CpuInstance = cpu;													// ... and save the cpu
		}

		public override byte DeviceCode										// Get the device code
		{
			get { return _deviceCode; }											// return calculated value
		}

		/// <summary>
		/// Executes the I/O Channel Command (IOCC) operation.
		/// Delegates to specific handlers based on the function type.
		/// </summary>
		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.SenseDevice:
					HandleSenseDevice();
					break;

				case DevFunction.Control:
					HandleControl();
					break;

				case DevFunction.InitRead:
					HandleInitRead();
					break;

				case DevFunction.InitWrite:
					HandleInitWrite();
					break;
			}
		}

		/// <summary>
		/// Handles the Sense Device IOCC function, returning device status to the accumulator.
		/// </summary>
		private void HandleSenseDevice()
		{
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
				if (_complete)
				{
					newAcc |= OperationComplete;
				}
			}
			CpuInstance.Acc = newAcc;
		}

		/// <summary>
		/// Handles the Control IOCC function for cylinder seeking operations.
		/// </summary>
		private void HandleControl()
		{
			if (!_cartMounted || _busy)
				return;

			_seekOffset = CpuInstance.IoccAddress & 0x1ff;
			if (_seekOffset == 0)
				return;

			if ((CpuInstance.IoccModifiers & SeekBackward) != 0)
				_seekOffset *= -1;

			_complete = false;
			_busy = true;
			_seeking = true;
		}

		/// <summary>
		/// Handles the Initiate Read IOCC function for reading sectors.
		/// </summary>
		private void HandleInitRead()
		{
			if (!_cartMounted || _busy)
				return;

			_readCheck = (CpuInstance.IoccModifiers & ReadCheck) != 0;
			_sector = CpuInstance.IoccModifiers & SectorMask;
			_complete = false;
			_busy = true;
			_seeking = false;
			_reading = true;
		}

		/// <summary>
		/// Handles the Initiate Write IOCC function for writing sectors.
		/// </summary>
		private void HandleInitWrite()
		{
			if (!_cartMounted || _busy)
				return;

			_sector = CpuInstance.IoccModifiers & SectorMask;
			_wcAddress = CpuInstance.IoccAddress;
			_complete = false;
			_busy = true;
			_seeking = false;
			_writing = true;
		}

		public override void Run()											// do the operation
		{
			if (_seeking)														// q. seeking?					
			{																	// a. yes ..
				CpuInstance.LetInstuctionsExecute(10);							// .. let the cpu run a little
				_cylinder += _seekOffset;										// .. move the heads
				_cartridge.CurrentCylinder = _cylinder.Current;					// .. let the cartridge know
				_complete = true;												// .. done with the operation
				_seeking = false;												// .. no longer seeking
				_busy = false;													// .. not busy any more
				ActivateInterrupt(CpuInstance, InterruptLevel, _ilsw);			// .. and set the interrupt
			}
			else if (_reading)													// q. reading?
			{																	// a. yes ..
				CpuInstance.LetInstuctionsExecute(10);							// .. let the cpu run a little
				var buffer = _cartridge.Read(_sector);							// .. read the sector to buffer in cart
				CpuInstance.TransferToMemory(buffer, 321);						// .. transfer to memory
				_complete = true;												// done tranferring
				_reading = false;												// .. no longer reading
				ActivateInterrupt(CpuInstance, InterruptLevel, _ilsw);			// .. tell the cpu we are done.
			}
			else if (_writing)													// q. writing?
			{																	// a. yes ..
				CpuInstance.LetInstuctionsExecute(10);							// .. let the cpu run a little
				var wc = CpuInstance[_wcAddress];								// .. number of words to write
				wc = (ushort) ((wc > 321) ? 321 : wc);							// .. ensure max words are 321
				_cartridge.Write(_sector, CpuInstance.GetBuffer(), wc);			// .. write the sector
				_complete = true;												// done tranferring
				_writing = false;												// .. no longer writing
				ActivateInterrupt(CpuInstance, InterruptLevel, _ilsw);			// .. tell the cpu we are done.
			}
			base.Run();
		}

		// Mount cartridge in drive. Assumes we turn on drive

		/// <summary>
		/// Mounts a cartridge in the disk drive and initializes the drive state.
		/// Resets the cylinder position to zero and clears all operation flags.
		/// </summary>
		/// <param name="cartridge">The cartridge to mount in the drive</param>
		/// <exception cref="ArgumentNullException">Thrown when cartridge is null</exception>
		/// <exception cref="InvalidOperationException">Thrown when a cartridge is already mounted</exception>
		public void Mount(ICartridge cartridge)
		{
			if (cartridge == null)
				throw new ArgumentNullException(nameof(cartridge), "Cannot mount a null cartridge");
				
			if (_cartMounted && _cartridge != null)
				throw new InvalidOperationException("A cartridge is already mounted. Unmount the current cartridge first.");
				
			_cartridge = cartridge;
			_cartridge.Mount();
			_cylinder = new CylinderTracker();
			_cartridge.CurrentCylinder = 0;
			_busy = false;
			_complete = false;
			_seeking = false;
			_reading = false;
			_writing = false;
			if (ActiveInterrupt != null)
			{
				DeactivateInterrupt(CpuInstance);
			}
			_cartMounted = true;
		}

		// UnMount cartridge. Save as turning off an removing

		/// <summary>
		/// Unmounts the cartridge from the disk drive.
		/// Flushes any pending writes before removing the cartridge.
		/// </summary>
		public void UnMount()
		{
			_cartridge.Flush();													// let it finalize any writes.
			_cartridge.UnMount();												// .. then unmount from drive
			_cartridge = null;													// .. let the drive know too\
			_cartMounted = false;												// .. show unmounted
		}
	}
}