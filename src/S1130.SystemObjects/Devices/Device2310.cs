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
		public const ushort DataError = 0x8000;
		public const ushort OperationComplete = 0x4000;
		public const ushort NotReady = 0x2000;
		public const ushort Busy = 0x1000;
		public const ushort AtCylZero = 0x0800;
		public const ushort NextSectorMask = 0x0003;
		public const int InterruptLevel = 2;
		public const byte SeekForward = 0x00;
		public const byte SeekBackward = 0x04;								// control modifier bit 13
		public const byte ReadCheck = 0x80;									// initiate-read modifier bit 8
		public const int SectorMask = 0x07;									// modifier bits 13-15

		private readonly byte _deviceCode;									// device code (calculated)

		// device status 
		private bool _cartMounted;											// true if cart is mouned
		private volatile bool _busy;										// true if operation active (volatile: set by the CPU thread, read by the device runner)
		private volatile bool _seeking;										// true if seeing in progress
		private volatile bool _reading;										// true if read in progress
		private volatile bool _writing;										// true if write in progress
		private volatile bool _complete;									// true if operation finished
		private bool _readCheck;											// true if read check requested
		private ushort _ilsw = 0x8000;										// ILSW (defaulted for drive 0)
		private CylinderTracker _cylinder = new CylinderTracker();			// current cylinder address
		private ICartridge _cartridge;										// cartridge 
		private int _seekOffset;											// number of cyliders to seek (+/-)
		private int _nextSector;											// "next sector spinning into position" (sense bits 14-15), 0-3
		private int _wordCount;												// words to transfer, latched at initiate (1-321)
		private volatile bool _dataError;									// data error status (cleared by sense with reset bit)
		private bool _badCountPending;										// bad word count seen: data error is raised when the op completes
		private volatile bool _hardError;									// permanent error (write to protected cartridge); cleared only by remounting
		private int _sector;												// sector to read
		private int _wcAddress;												// word-count address latched from the IOCC
		private ulong _opStarted;											// instruction count when the current operation started

		/// <summary>Instructions the CPU runs before a started operation completes (immediately while the CPU waits).</summary>
		public const int OperationDelay = 10;

		public Device2310(ICpu cpu, int driveNumber = 0)					// constructor cpu and drive number (0-4)		
		{
			if (driveNumber < 0 || driveNumber >= 5)							// q. actual device?
			{																	// a. no .. throw error
				throw new ArgumentException("2310 drive number must be between 0 and 4");
			}
			// Drive 0 is the CPU single disk (00100); 2310 drives 1-4 are 10001-10100
			_deviceCode = new byte[] {0x04, 0x11, 0x12, 0x13, 0x14}[driveNumber];
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
		/// True if the last write put a first word (the sector address) that does not match the
		/// cylinder/sector written. The real drive just stores it; the C simulator prints a warning.
		/// </summary>
		public bool SectorAddressMismatch { get; private set; }

		/// <summary>
		/// Handles the Sense Device IOCC function, returning device status to the accumulator.
		/// </summary>
		private void HandleSenseDevice()
		{
			ushort newAcc = 0;
			if (_cartridge == null)
			{
				newAcc = NotReady;
			}
			else
			{
				if (_busy)
				{
					newAcc = Busy | NotReady;								// busy also reads as not ready
				}
				else if (_hardError)
				{
					newAcc = NotReady;										// permanent error: drive stays not ready
				}
				else if (_cylinder.Current == 0)
				{
					newAcc |= AtCylZero;									// carriage home is reported only when ready
				}
				if (_complete)
				{
					newAcc |= OperationComplete;
				}
				if (_dataError || _hardError)
				{
					newAcc |= DataError;
				}
			}
			_nextSector = (_nextSector + 1) & NextSectorMask;				// spinning disk: count advances on every sense
			CpuInstance.Acc = (ushort)(newAcc | _nextSector);

			if ((CpuInstance.IoccModifiers & 1) == 1)					// modifier bit 15: reset operation complete and data error
			{
				_complete = false;
				_dataError = false;											// (a hard error survives; only a remount clears it)
				DeactivateInterrupt(CpuInstance);
			}
		}

		/// <summary>
		/// Handles the Control IOCC function for cylinder seeking operations.
		/// </summary>
		private void HandleControl()
		{
			if (!_cartMounted || _busy)
				return;

			_seekOffset = CpuInstance.IoccAddress & 0xff;					// step count is 8 bits (as in the C simulator)
			if (_seekOffset == 0)
				return;

			if ((CpuInstance.IoccModifiers & SeekBackward) != 0)
				_seekOffset *= -1;

			_complete = false;
			_busy = true;
			_opStarted = CpuInstance.InstructionCount;
			_seeking = true;
		}

		/// <summary>
		/// Handles the Initiate Read IOCC function for reading sectors.
		/// </summary>
		private void HandleInitRead()
		{
			if (!_cartMounted || _busy || _hardError)
				return;

			_readCheck = (CpuInstance.IoccModifiers & ReadCheck) != 0;
			_sector = CpuInstance.IoccModifiers & SectorMask;
			_wcAddress = CpuInstance.IoccAddress;
			_complete = false;
			if (!LatchWordCount())											// q. zero count?
				return;														// a. yes .. the controller hangs, busy forever
			_busy = true;
			_seeking = false;
			_opStarted = CpuInstance.InstructionCount;
			_reading = true;
		}

		/// <summary>
		/// Handles the Initiate Write IOCC function for writing sectors.
		/// </summary>
		private void HandleInitWrite()
		{
			if (!_cartMounted || _busy || _hardError)
				return;

			if (_cartridge.ReadOnly)										// q. write protected?
			{																// a. yes .. permanent error, no interrupt
				_hardError = true;
				return;
			}

			_sector = CpuInstance.IoccModifiers & SectorMask;
			_wcAddress = CpuInstance.IoccAddress;
			_complete = false;
			if (!LatchWordCount())											// q. zero count?
				return;														// a. yes .. the controller hangs, busy forever
			_busy = true;
			_seeking = false;
			_opStarted = CpuInstance.InstructionCount;
			_writing = true;
		}

		/// <summary>
		/// Reads the word count from the first word of the IOCC area. A count above 321 is limited to 321 and
		/// raises a data error when the operation completes. A zero count locks up the controller (it stays
		/// busy and never interrupts), as on the real machine.
		/// </summary>
		/// <returns>false if the controller locked up (count of zero)</returns>
		private bool LatchWordCount()
		{
			int wc = CpuInstance[_wcAddress];
			_badCountPending = wc > 321;
			if (wc == 0)
			{
				_busy = true;
				return false;
			}
			_wordCount = _badCountPending ? 321 : wc;
			return true;
		}

		public override void Run()											// do the operation once it is due
		{
			if ((_seeking || _reading || _writing) && !OperationIsDue())		// q. started, but the cpu has not run long enough?
			{																	// a. yes .. come back later (never block: Run may be on the cpu's own thread)
				base.Run();
				return;
			}
			if (_seeking)														// q. seeking?					
			{																	// a. yes ..
				_cylinder += _seekOffset;										// .. move the heads
				_cartridge.CurrentCylinder = _cylinder.Current;					// .. let the cartridge know
				_complete = true;												// .. done with the operation
				_seeking = false;												// .. no longer seeking
				_busy = false;													// .. not busy any more
				ActivateInterrupt(CpuInstance, InterruptLevel, _ilsw);			// .. and set the interrupt
			}
			else if (_reading)													// q. reading?
			{																	// a. yes ..
				var buffer = _cartridge.Read(_sector);							// .. read the sector to buffer in cart
				if (!_readCheck)												// q. a real read (not read-check)?
				{																// a. yes ..
					CpuInstance.TransferToMemory(_wcAddress, buffer, _wordCount);	// .. transfer the requested words at the latched WCA
				}																// (read-check only verifies; no transfer)
				FinishTransfer();												// done tranferring
				_reading = false;												// .. no longer reading
			}
			else if (_writing)													// q. writing?
			{																	// a. yes ..
				var wc = _wordCount;											// .. number of words to write (1-321)
				var data = new ArraySegment<ushort>(CpuInstance.Memory, _wcAddress + 1, wc);
				SectorAddressMismatch = CpuInstance.Memory[_wcAddress + 1] != ((_cylinder.Current << 3) | _sector);
				_cartridge.Write(_sector, data, wc);							// .. write the sector (the cartridge zero-fills the rest)
				FinishTransfer();												// done tranferring
				_writing = false;												// .. no longer writing
			}
			base.Run();
		}

		private bool OperationIsDue() =>
			CpuInstance.IgnoreInstructionCount
			|| CpuInstance.Wait
			|| CpuInstance.InstructionCount - _opStarted >= OperationDelay;

		// Ends a read or write: flags complete (and a data error for a bad word count), then interrupts.
		private void FinishTransfer()
		{
			if (_badCountPending)
			{
				_dataError = true;
				_badCountPending = false;
			}
			_complete = true;
			_busy = false;
			ActivateInterrupt(CpuInstance, InterruptLevel, _ilsw);
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
			_dataError = false;													// remounting is a power cycle: clear errors
			_hardError = false;
			_badCountPending = false;
			SectorAddressMismatch = false;
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