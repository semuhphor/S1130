using System;
using System.Collections.Concurrent;
using System.Threading;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public class Cpu : ICpu	
    {
        public const int DefaultMemorySize = 32768;								// default size of memory 
		private readonly ConcurrentQueue<Interrupt>[] _interruptQueues;			// queues for active interrupts
		private readonly ConcurrentStack<Interrupt> _currentInterrupts;			// stack of interrupts being serviced
		private int _activeInterruptCount = 0;									// number of interrupts active

        public Cpu()														// System State constructor
        {
            MemorySize = DefaultMemorySize;										// size of memory
            Memory = new ushort[DefaultMemorySize];								// reserve the memory
            Xr = new IndexRegisters(this);										// setup index register shortcut
	        IntPool = InterruptPool.GetPool();							// setup the interrupt pool
			Instructions = InstructionSetBuilder.GetInstructionSet();			// instantiate the instruction set
			_interruptQueues =  new ConcurrentQueue<Interrupt>[6]				// prepare interrupt queue	
					{
						new ConcurrentQueue<Interrupt>(),						// Level 0 
						new ConcurrentQueue<Interrupt>(),						// Level 1  
						new ConcurrentQueue<Interrupt>(), 						// Level 2 
						new ConcurrentQueue<Interrupt>(), 						// Level 3 
						new ConcurrentQueue<Interrupt>(), 						// Level 4 
						new ConcurrentQueue<Interrupt>()						// Level 5 
					};
			_currentInterrupts = new ConcurrentStack<Interrupt>();				// stack for interrupts being served
	        BuildDefaultDevices();												// .. initialize the devices
        }

		public IInstruction[] Instructions { get; private set; }				// property for instruction set access 
		public IInstruction CurrentInstruction { get; private set; }			// property for current instruction
		public InterruptPool IntPool { get; private set; }						// property for the pool of interrupt objects
		public IDevice[] Devices { get; private set; }							// property for devices on machine


		public ushort[] Memory { get; set; }									// property for memory
		public int MemorySize { get; set; }										// property for memory size
		public ushort Iar { get; set; }											// property for Instruction Address Register
		public ushort Acc { get; set; }											// property for Accumulator
        public ushort Ext { get; set; }											// property for Accumulator Extension
		public bool Carry { get; set; }											// property for Carry indicator
		public bool Overflow { get; set; }										// property for Overflow indicator
		public bool Wait { get; set; }											// property for Wait state

		public ushort this[int address]											// c# indexer to access memory
		{
			get { return Memory[address]; }										// .. memory read
			set { Memory[address] = value; }									// .. memory write
		}																		

		public ushort ConsoleSwitches { get; set; }							// property for console entry switches

	    public uint AccExt													// property for accumulator and extension (32bit access)
	    {
		    get { return (uint) ((Acc << 16) | Ext); }							// get ACC and EXT as one 32bit value
		    set																	// set ACC and EXT ...
		    {
			    Acc = (ushort) (value >> 16);									// Set ACC to hi order 16bits of 32bits
			    Ext = (ushort) (value & 0xffff);								// Set EXT to low order 16bits of 32bits
		    }
	    }

        public IndexRegisters Xr { get; private set; }						// property to access index registers

		public ushort AtIar														// property to access memory address IAR
		{
			get { return this[Iar]; }											// read memory
			set { this[Iar] = value; }											// write memory
		}								 												

																			// current instruction
        public ushort Opcode { get; set; }										// opcode
        public bool FormatLong { get; set; }									// long/short format instruction
        public ushort Tag { get; set; }											// tag bits (index register)
        public ushort Displacement { get; set; }								// displacement (may be absolute address)
        public bool IndirectAddress { get; set; }								// indirect address bit
        public ushort Modifiers { get; set; }									// modifiers 

		/*
		 * CurrentInterruptLevel handling code
		 */

		public ConcurrentQueue<Interrupt>[] InterruptQueues { get { return _interruptQueues; } }	// Queue for interrupts
		public ConcurrentStack<Interrupt> CurrentInterrupt { get { return _currentInterrupts; } }	// Stack for interrupts being serviced

		public int ActiveInterruptCount { get { return _activeInterruptCount; } }					// number of interrupts currently active

		public int? CurrentInterruptLevel									// Determine highestActive CurrentInterruptLevel
		{
			get
			{
				for (var i = 0; i < 6; i++)										// iterate through levels 0..5
				{
					if (!_interruptQueues[i].IsEmpty)							// q. interrupt active?
					{															// a. yes ...
						return i;												// .. return the value of the active interrupt
					}
				}																// otherwise ..
				return null;													// .. show none active
			}
		}

		public void AddInterrupt(Interrupt interrupt)						// Add interrupt to interrupt queue
		{
			var interruptLevel = interrupt.InterruptLevel;						// Get tthe interrupt level
			if (interruptLevel >= 0 && interruptLevel <= 5)						// q. CurrentInterruptLevel level in range?
			{																	// a. yes .. 
				_interruptQueues[interruptLevel].Enqueue(interrupt);			// .. queue the interrupt
				Interlocked.Increment(ref _activeInterruptCount);				// .. add number active
			}
		}

		private bool ShouldHandleInterrupt(int interruptLevel)				// true if we should handle the interrupt
		{
			if (!_currentInterrupts.IsEmpty)									// q. interrupt active?
			{																	// a. yes .. 
				Interrupt interrupt;											// .. get current interrupt
				if (_currentInterrupts.TryPeek(out interrupt))					// q. current interrupt on stack?
				{																// a. yes ..
					if (interruptLevel >= interrupt.InterruptLevel)				// q. higher level interrupt?
					{															// a. no ..
						return false;											// .. don't handle the interrupt
					}
				}
			}																	// otherwise...
			return true;														// ... handle the interrupt
		}

		public void HandleInterrupt()										// handle current interrupt
		{
			if (_activeInterruptCount != 0)										// q. any interrupts active?
			{																	// a. yes...
				int? currentInterruptLevel = CurrentInterruptLevel;				// get current interrupt level
				if (currentInterruptLevel.HasValue)								// q. did we get the value?
				{																// a. yes..
					var interruptLevel = currentInterruptLevel.Value;			// .. save the interrupt number
					if (!ShouldHandleInterrupt(interruptLevel))					// q. should the interrupt be handled?
					{															// a. no..
						return;													// .. don't handle it.
					}
					var intVector = Constants.InterruptVectors[interruptLevel];	// get the vector 
					Interrupt interrupt;										// .. and a place for the interrupt
					if (_interruptQueues[interruptLevel].TryPeek(out interrupt))// q. found interrupt causing interrupt?
					{															// a. yes..
						CurrentInterrupt.Push(interrupt);						// .. push it as current interrupt	
						var addressOfInterruptHandler = Memory[intVector];		// .. get the address of the handler
						Memory[addressOfInterruptHandler] = Iar;				// .. save the current IAR return address
						Iar = (ushort) (addressOfInterruptHandler + 1);			// .. and go to the interrupt handler
					}
				}
			}
		}

		public void ClearCurrentInterrupt()									// Clear current interrupt
		{												
			Interrupt intr;														// interrupting interrupt
			if (CurrentInterrupt.TryPop(out intr))								// q. active interrupt available?
			{																	// a. yes ...
				if (intr.CausingDevice.ActiveInterrupt != intr)					// q. is this interrupt still on device?
				{																// a. no ..
					_interruptQueues[intr.InterruptLevel].TryDequeue(out intr);	// .. remove from interrupt queue
					IntPool.PutInterrupt(intr);									// .. return to the pool
					Interlocked.Decrement(ref _activeInterruptCount);			// .. and decrement number of active interrupts
				}
			}
		}

		/*
		 * Instruction decode and execution
		 */

		public void NextInstruction()										// Decode the next instruction
        {
            var firstWord = Memory[Iar++];										// retrieve the first work
            Opcode = (ushort) ((firstWord & 0xF800) >> 11);						// .. get the opcode shifted to low-order bits
			CurrentInstruction = Instructions[Opcode];							// save the current instruction
			if (CurrentInstruction != null)										// q. instruciton found?
			{																	// a. yes .. decode
				var formatBit = (firstWord & 0x0400) != 0;						// .. extract the format bit
				Tag = (ushort) ((firstWord & 0x0300) >> 8);						// .. get the Xr, if any, from tag bits
				Modifiers = (ushort) (firstWord & 0xff);						// .. get out modifiers/displacement
				if (formatBit && CurrentInstruction.HasLongFormat)				// q. long format instruction?
				{																// a. yes ...
					FormatLong = true;											// .. show it is long
					Displacement = Memory[Iar++];								// .. get displacement second word
					IndirectAddress = (firstWord & 0x80) != 0;					// .. and get indirect address bit
				}
				else															// otherwise ..
				{																// .. short format
					FormatLong = false;											// .. show it is short
					Displacement = Modifiers;									// .. get displacement from modifiers (see above)
					IndirectAddress = false;									// .. and it isn't indirect.
				}
			}
        }

		public void ExecuteInstruction()									// Execute current instruction
	    {																		// .. instruction decoded from NextInstruction() above
			if (CurrentInstruction != null)										// q. is the current instruction valid?
			{																	// a. yes ..
				CurrentInstruction.Execute(this);								// .. execute it
			}
			else																// Otherwise..
			{																	// .. not found
				Wait = true;													// .. treat it like a wait state
			}
			HandleInterrupt();													// .. handle any interrupt active
	    }

		/*
		 * Device management
		 */

		public int IoccAddress { get; private set; }						// Address from IOCC
		public int IoccDeviceCode { get; private set; }						// Device code from IOCC
		public DevFuction IoccFunction { get; private set; }				// Function from IOCC
		public int IoccModifiers { get; private set; }						// Modifier from IOCC
		public IDevice IoccDevice { get; private set; }						// Device referenced

		public bool AddDevice(IDevice device)								// Add device to system
		{																		
			if (Devices[device.DeviceCode] != null)								// q. device in use?
			{																	// a. yes ..
				return false;													// .. can't add now
			}
			Devices[device.DeviceCode] = device;								// otherwise ... add the device
			return true;														// .. and tell 'em it worked
		}

		public void IoccDecode(int address)									// Decode an IOCC
		{
			IoccAddress = Memory[address++];									// get the memory address
			ushort secondWord = Memory[address];								// then pull second word
			IoccDeviceCode = (secondWord & 0xf800) >> 11;						// .. extract device code
			IoccFunction = (DevFuction) ((secondWord & 0x0700) >> 8);			// .. extract function
			IoccModifiers = secondWord & 0xff;									// .. and extract modifiers
			IoccDevice = Devices[IoccDeviceCode];								// .. finally, get the device reference
		}

		private void BuildDefaultDevices()									// add default devices to system
		{
			Devices = new IDevice[32];											// init the device array
			AddDevice(new ConsoleEntrySwitches());								// .. add console entry switches
		}
    }
}