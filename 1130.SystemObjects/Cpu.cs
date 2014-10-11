using System.Collections.Concurrent;

namespace S1130.SystemObjects
{
	public class Cpu : ICpu
    {
        public const int DefaultMemorySize = 32768;										// default size of memory 
		private readonly ConcurrentQueue<IInterruptingDevice>[] _interruptQueues;		// queues for devices with interrupts
		private readonly ConcurrentStack<IInterruptingDevice> _currentDevice;			// stack of devices being serviced
		
        public Cpu()														// System State constructor
        {
            MemorySize = DefaultMemorySize;												// size of memory
            Memory = new ushort[DefaultMemorySize];										// reserve the memory
            Xr = new IndexRegisters(this);												// setup index register shortcut
			Instructions = new InstructionSet();										// instantiate the instruction set
			_interruptQueues =  new ConcurrentQueue<IInterruptingDevice>[6]				// prepare interrupting device queue	
					{
						new ConcurrentQueue<IInterruptingDevice>(),						// Level 0 
						new ConcurrentQueue<IInterruptingDevice>(),						// Level 1  
						new ConcurrentQueue<IInterruptingDevice>(), 					// Level 2 
						new ConcurrentQueue<IInterruptingDevice>(), 					// Level 3 
						new ConcurrentQueue<IInterruptingDevice>(), 					// Level 4 
						new ConcurrentQueue<IInterruptingDevice>()						// Level 5 
					};
			_currentDevice = new ConcurrentStack<IInterruptingDevice>();				// stack for devices being served
        }

 	    public IInstructionSet Instructions { get; private set; }						// property for instruction set access 
		public IInstruction CurrentInstruction { get; private set; }					// property for current instruction
		public ushort[] Memory { get; set; }											// property for memory
		public int MemorySize { get; set; }												// property for memory size
		public ushort Iar { get; set; }													// property for Instruction Address Register
		public ushort Acc { get; set; }													// property for Accumulator
        public ushort Ext { get; set; }													// property for Accumulator Extension
		public bool Carry { get; set; }													// property for Carry indicator
		public bool Overflow { get; set; }												// property for Overflow indicator
		public bool Wait { get; set; }													// property for Wait state

		public ushort this[int address]													// c# indexer to access memory
		{
			get { return Memory[address]; }												// .. memory read
			set { Memory[address] = value; }											// .. memory write
		}

		public ushort ConsoleSwitches { get; set; }										// property for console entry switches

	    public uint AccExt																// property for accumulator and extension (32bit access)
	    {
		    get { return (uint) ((Acc << 16) | Ext); }										// get ACC and EXT as one 32bit value
		    set																				// set ACC and EXT ...
		    {
			    Acc = (ushort) (value >> 16);													// Set ACC to hi order 16bits of 32bits
			    Ext = (ushort) (value & 0xffff);												// Set EXT to low order 16bits of 32bits
		    }
	    }

        public IndexRegisters Xr { get; private set; }									// property to access index registers

		public ushort AtIar															// property to access memory address IAR
		{
			get { return this[Iar]; }													// read memory
			set { this[Iar] = value; }													// write memory
		}								 												

																					// current instruction
        public ushort Opcode { get; set; }												// opcode
        public bool FormatLong { get; set; }											// long/short format instruction
        public ushort Tag { get; set; }													// tag bits (index register)
        public ushort Displacement { get; set; }										// displacement (may be absolute address)
        public bool IndirectAddress { get; set; }										// indirect address bit
        public ushort Modifiers { get; set; }											// modifiers 

		/*
		 * Interrupt handling code
		 */

		public ConcurrentQueue<IInterruptingDevice>[] InterruptQueues { get { return _interruptQueues; } }	// Queue for interrupting devices
		public ConcurrentStack<IInterruptingDevice> CurrentDevice { get { return _currentDevice; }  }		// Stack for interrupt being serviced

		public int? Interrupt														// Determine if an interrupt is requested
		{
			get
			{
				for (var i = 0; i < 6; i++)												// iterate through levels 0..5
				{
					if (!_interruptQueues[i].IsEmpty)									// q. interrupt active?
					{																	// a. yes ...
						return i;														// .. return the value of the active interrupt
					}
				}																		// otherwise ..
				return null;															// .. show none active
			}
		}

		public void AddInterrupt(IInterruptingDevice device)						// Add device to interrupt queue
		{
			var interruptLevel = device.InterruptLevel;									// Get tthe interrupt level
			if (interruptLevel >= 0 && interruptLevel <= 5)								// q. Interrupt level in range?
			{																			// a. yes .. 
				_interruptQueues[interruptLevel].Enqueue(device);						// .. queue the interrupt
			}
		}

		private bool ShouldHandleInterrupt(int interruptLevel)						// true if we should handle the interrupt
		{
			if (!_currentDevice.IsEmpty)												// q. device active?
			{																			// a. yes .. 
				IInterruptingDevice device;												// .. get current device
				if (_currentDevice.TryPeek(out device))									// q. current device on stack?
				{																		// a. yes ..
					if (interruptLevel >= device.InterruptLevel)						// q. higher level interrupt?
					{																	// a. no ..
						return false;													// .. don't handle the interrupt
					}
				}
			}																			// otherwise...
			return true;																// ... handle the interrupt
		}

		public void HandleInterrupt()												// handle current interrupt
		{
			if (Interrupt.HasValue)														// q. interrupt active?
			{																			// a. yes..
				var interrupt = Interrupt.Value;										// .. save the interrupt number
				if (!ShouldHandleInterrupt(interrupt))									// q. should the interrupt be handled?
				{																		// a. no..
					return;																// .. don't handle it.
				}
				var intVector = Constants.InterruptVectors[interrupt];					// get the vector 
				IInterruptingDevice device;												// .. and a place for the device
				if (_interruptQueues[interrupt].TryPeek(out device))					// q. found device causing interrupt?
				{																		// a. yes..
					CurrentDevice.Push(device);											// .. push it as current device	
					var addressOfInterruptHandler = Memory[intVector];					// .. get the address of the handler
					Memory[addressOfInterruptHandler] = Iar;							// .. save the current IAR return address
					Iar = (ushort) (addressOfInterruptHandler + 1);						// .. and go to the interrupt handler
				}																	
			}
		}

		public void ClearCurrentInterrupt()											// Clear current interrupt
		{												
			IInterruptingDevice device;													// interrupting device
			if (CurrentDevice.TryPop(out device))										// q. active device available?
			{																			// a. yes ...
				InterruptQueues[device.InterruptLevel].TryDequeue(out device);			// remove it from the interrupt level queue
			}
		}

		/*
		 * Instruction decode and execution
		 */

		public void NextInstruction()												// Decode the next instruction
        {
            var firstWord = Memory[Iar++];												// retrieve the first work
            Opcode = (ushort) ((firstWord & 0xF800) >> 11);								// .. get the opcode shifted to low-order bits
			CurrentInstruction = Instructions[Opcode];									// save the current instruction
			if (CurrentInstruction != null)												// q. instruciton found?
			{																			// a. yes .. decode
				var formatBit = (firstWord & 0x0400) != 0;								// .. extract the format bit
				Tag = (ushort) ((firstWord & 0x0300) >> 8);								// .. get the Xr, if any, from tag bits
				Modifiers = (ushort) (firstWord & 0xff);								// .. get out modifiers/displacement
				if (formatBit && CurrentInstruction.HasLongFormat)						// q. long format instruction?
				{																		// a. yes ...
					FormatLong = true;													// .. show it is long
					Displacement = Memory[Iar++];										// .. get displacement second word
					IndirectAddress = (firstWord & 0x80) != 0;							// .. and get indirect address bit
				}
				else																	// otherwise ..
				{																		// .. short format
					FormatLong = false;													// .. show it is short
					Displacement = Modifiers;											// .. get displacement from modifiers (see above)
					IndirectAddress = false;											// .. and it isn't indirect.
				}
			}
        }

		public void ExecuteInstruction()											// Execute current instruction
	    {																				// .. instruction decoded from NextInstruction() above
			if (CurrentInstruction != null)												// q. is the current instruction valid?
			{																			// a. yes ..
				CurrentInstruction.Execute(this);										// .. execute it
			}
			else																		// Otherwise..
			{																			// .. not found
				Wait = true;															// .. treat it like a wait state
			}
			HandleInterrupt();															// .. handle any interrupt active
	    }
    }
}