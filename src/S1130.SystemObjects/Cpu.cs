using System;
using System.Collections.Concurrent;
using System.Threading;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	/// <summary>
	/// Represents the central processing unit of the IBM 1130 emulator.
	/// Manages instruction execution, memory, registers, interrupts, and device interactions.
	/// </summary>
	public class Cpu : ICpu
	{
		public const int DefaultMemorySize = 32768;                             // default size of memory 
		private readonly ConcurrentQueue<Interrupt>[] _interruptQueues;         // queues for active interrupts
		private readonly ConcurrentStack<Interrupt> _currentInterrupts;         // stack of interrupts being serviced
		private int _activeInterruptCount;                                      // number of interrupts active
		private ulong _count;                                                   // number of instructions executed;

		/// <summary>
		/// Gets the complete instruction set available to the CPU.
		/// </summary>
		public IInstruction[] Instructions { get; private set; }
		
		/// <summary>
		/// Gets the currently decoded instruction ready for execution.
		/// </summary>
		public IInstruction CurrentInstruction { get; private set; }
		
		/// <summary>
		/// Gets the interrupt pool for managing interrupt objects.
		/// </summary>
		public InterruptPool IntPool { get; private set; }
		
		/// <summary>
		/// Gets the array of devices attached to the system.
		/// </summary>
		public IDevice[] Devices { get; private set; }

		/// <summary>
		/// Gets or sets the system memory array.
		/// </summary>
		public ushort[] Memory { get; set; }
		
		/// <summary>
		/// Gets or sets the debug settings for memory locations.
		/// </summary>
		public IDebugSetting[] _debugSettings { get; set; }
		
		/// <summary>
		/// Gets or sets the size of system memory in words.
		/// </summary>
		public int MemorySize { get; set; }

		/// <summary>
		/// Gets or sets the Instruction Address Register (program counter).
		/// </summary>
		public ushort Iar { get; set; }
		
		/// <summary>
		/// Gets or sets the Accumulator register.
		/// </summary>
		public ushort Acc { get; set; }
		
		/// <summary>
		/// Gets or sets the Accumulator Extension register.
		/// </summary>
		public ushort Ext { get; set; }
		
		/// <summary>
		/// Gets or sets the Carry flag indicator.
		/// </summary>
		public bool Carry { get; set; }
		
		/// <summary>
		/// Gets or sets the Overflow flag indicator.
		/// </summary>
		public bool Overflow { get; set; }
		
		/// <summary>
		/// Gets or sets the Wait state flag.
		/// </summary>
		public bool Wait { get; set; } 

		/// <summary>
		/// Indexer to access system memory by address.
		/// Provides direct read/write access to memory locations.
		/// </summary>
		/// <param name="address">Memory address to access</param>
		/// <returns>16-bit word at the specified address</returns>
		public ushort this[int address]
		{
			get { return Memory[address]; }
			set { Memory[address] = value; }
		}

		/// <summary>
		/// Gets or sets the console entry switches value.
		/// </summary>
		public ushort ConsoleSwitches { get; set; }

		/// <summary>
		/// Gets or sets the combined Accumulator and Extension registers as a 32-bit value.
		/// High 16 bits represent the Accumulator, low 16 bits represent the Extension.
		/// </summary>
		public uint AccExt
		{
			get { return (uint)((Acc << 16) | Ext); }
			set
			{
				Acc = (ushort)(value >> 16);
				Ext = (ushort)(value & 0xffff);
			}
		}

		/// <summary>
		/// Gets the index registers (XR1, XR2, XR3).
		/// </summary>
		public IndexRegisters Xr { get; private set; }

		/// <summary>
		/// Gets or sets the memory word at the current Instruction Address Register location.
		/// </summary>
		public ushort AtIar
		{
			get { return this[Iar]; }
			set { this[Iar] = value; }
		}

		// Current instruction decode fields
		/// <summary>
		/// Gets or sets the current instruction opcode.
		/// </summary>
		public ushort Opcode { get; set; }
		
		/// <summary>
		/// Gets or sets whether the current instruction uses long format.
		/// </summary>
		public bool FormatLong { get; set; }
		
		/// <summary>
		/// Gets or sets the tag bits (index register selector) of the current instruction.
		/// </summary>
		public ushort Tag { get; set; }
		
		/// <summary>
		/// Gets or sets the displacement field of the current instruction.
		/// </summary>
		public ushort Displacement { get; set; }
		
		/// <summary>
		/// Gets or sets whether the current instruction uses indirect addressing.
		/// </summary>
		public bool IndirectAddress { get; set; }
		
		/// <summary>
		/// Gets or sets the modifier bits of the current instruction.
		/// </summary>
		public ushort Modifiers { get; set; } 

		/*
		 * CurrentInterruptLevel handling code
		 */

		public ConcurrentQueue<Interrupt>[] InterruptQueues { get { return _interruptQueues; } }    // Queue for interrupts
		public ConcurrentStack<Interrupt> CurrentInterrupt { get { return _currentInterrupts; } }   // Stack for interrupts being serviced

		public int ActiveInterruptCount { get { return _activeInterruptCount; } }                   // number of interrupts currently active

		public Cpu()                                                        // System State constructor
		{
			MemorySize = DefaultMemorySize;                                     // size of memory
			Memory = new ushort[DefaultMemorySize];                             // reserve the memory
			Xr = new IndexRegisters(this);                                      // setup index register shortcut
			IntPool = new InterruptPool();                                      // setup the interrupt pool
			Instructions = InstructionSetBuilder.GetInstructionSet();           // instantiate the instruction set
			_interruptQueues = new[]											// prepare interrupt queue	
					{
						new ConcurrentQueue<Interrupt>(),						// Level 0 
						new ConcurrentQueue<Interrupt>(),						// Level 1  
						new ConcurrentQueue<Interrupt>(), 						// Level 2 
						new ConcurrentQueue<Interrupt>(), 						// Level 3 
						new ConcurrentQueue<Interrupt>(), 						// Level 4 
						new ConcurrentQueue<Interrupt>()						// Level 5 
					};
			_currentInterrupts = new ConcurrentStack<Interrupt>();              // stack for interrupts being served
			BuildDefaultDevices();                                              // .. initialize the devices
		}

		public int? CurrentInterruptLevel                                   // Determine highestActive CurrentInterruptLevel
		{
			get
			{
				for (var i = 0; i < 6; i++)                                     // iterate through levels 0..5
				{
					if (!_interruptQueues[i].IsEmpty)                           // q. interrupt active?
					{                                                           // a. yes ...
						return i;                                               // .. return the value of the active interrupt
					}
				}                                                               // otherwise ..
				return null;                                                    // .. show none active
			}
		}

		public void AddInterrupt(Interrupt interrupt)                       // Add interrupt to interrupt queue
		{
			var interruptLevel = interrupt.InterruptLevel;                      // Get the interrupt level
			if (interruptLevel >= 0 && interruptLevel <= 5)                     // q. CurrentInterruptLevel level in range?
			{                                                                   // a. yes .. 
				_interruptQueues[interruptLevel].Enqueue(interrupt);            // .. queue the interrupt
				Interlocked.Increment(ref _activeInterruptCount);               // .. add number active
			}
		}

		private bool ShouldHandleInterrupt(int interruptLevel)              // true if we should handle the interrupt
		{
			if (!_currentInterrupts.IsEmpty)                                    // q. interrupt active?
			{                                                                   // a. yes .. 
				Interrupt interrupt;                                            // .. get current interrupt
				if (_currentInterrupts.TryPeek(out interrupt))                  // q. current interrupt on stack?
				{                                                               // a. yes ..
					if (interruptLevel >= interrupt.InterruptLevel)             // q. higher level interrupt?
					{                                                           // a. no ..
						return false;                                           // .. don't handle the interrupt
					}
				}
			}                                                                   // otherwise...
			return true;                                                        // ... handle the interrupt
		}

		public void HandleInterrupt()                                       // handle current interrupt
		{
			if (_activeInterruptCount != 0)                                     // q. any interrupts active?
			{                                                                   // a. yes...
				int? currentInterruptLevel = CurrentInterruptLevel;             // get current interrupt level
				if (currentInterruptLevel.HasValue)                             // q. did we get the value?
				{                                                               // a. yes..
					var interruptLevel = currentInterruptLevel.Value;           // .. save the interrupt number
					if (!ShouldHandleInterrupt(interruptLevel))                 // q. should the interrupt be handled?
					{                                                           // a. no..
						return;                                                 // .. don't handle it.
					}
					var intVector = Constants.InterruptVectors[interruptLevel]; // get the vector 
					Interrupt interrupt;                                        // .. and a place for the interrupt
					if (_interruptQueues[interruptLevel].TryPeek(out interrupt))// q. found interrupt causing interrupt?
					{                                                           // a. yes..
						CurrentInterrupt.Push(interrupt);                       // .. push it as current interrupt	
						var addressOfInterruptHandler = Memory[intVector];      // .. get the address of the handler
						Memory[addressOfInterruptHandler] = Iar;                // .. save the current IAR return address
						Iar = (ushort)(addressOfInterruptHandler + 1);          // .. and go to the interrupt handler
					}
				}
			}
		}

		public void ClearCurrentInterrupt()                                 // Clear current interrupt
		{
			Interrupt intr;                                                     // interrupting interrupt
			if (CurrentInterrupt.TryPop(out intr))                              // q. active interrupt available?
			{                                                                   // a. yes ...
				if (intr.CausingDevice.ActiveInterrupt != intr)                 // q. is this interrupt still on device?
				{                                                               // a. no ..
					_interruptQueues[intr.InterruptLevel].TryDequeue(out intr); // .. remove from interrupt queue
					IntPool.PutInterruptInBag(intr);                            // .. return to the pool
					Interlocked.Decrement(ref _activeInterruptCount);           // .. and decrement number of active interrupts
				}
			}
		}

		/*
		 * Instruction decode and execution
		 */

		/// <summary>
		/// Decodes the next instruction at the current IAR location.
		/// Advances IAR past the instruction and populates instruction decode fields.
		/// </summary>
		public void NextInstruction()
		{
			var firstWord = Memory[Iar++];                                      // retrieve the first work
			Opcode = (ushort)((firstWord & 0xF800) >> 11);                      // .. get the opcode shifted to low-order bits
			CurrentInstruction = Instructions[Opcode];                          // save the current instruction
			if (CurrentInstruction != null)                                     // q. instruciton found?
			{                                                                   // a. yes .. decode
				var formatBit = (firstWord & 0x0400) != 0;                      // .. extract the format bit
				Tag = (ushort)((firstWord & 0x0300) >> 8);                      // .. get the Xr, if any, from tag bits
				Modifiers = (ushort)(firstWord & 0xff);                     // .. get out modifiers/displacement
				if (formatBit && CurrentInstruction.HasLongFormat)              // q. long format instruction?
				{                                                               // a. yes ...
					FormatLong = true;                                          // .. show it is long
					Displacement = Memory[Iar++];                               // .. get displacement second word
					IndirectAddress = (firstWord & 0x80) != 0;                  // .. and get indirect address bit
				}
				else                                                            // otherwise ..
				{                                                               // .. short format
					FormatLong = false;                                         // .. show it is short
					Displacement = Modifiers;                                   // .. get displacement from modifiers (see above)
					IndirectAddress = false;                                    // .. and it isn't indirect.
				}
			}
		}

		/// <summary>
		/// Executes the currently decoded instruction and handles any pending interrupts.
		/// Sets Wait state if instruction is invalid. Increments instruction counter.
		/// </summary>
		public void ExecuteInstruction()
		{                                                                       // .. instruction decoded from NextInstruction() above
			if (CurrentInstruction != null)                                     // q. is the current instruction valid?
			{                                                                   // a. yes ..
				CurrentInstruction.Execute(this);                               // .. execute it
			}
			else                                                                // Otherwise..
			{                                                                   // .. not found
				Wait = true;                                                    // .. treat it like a wait state
			}
			HandleInterrupt();                                                  // .. handle any interrupt active
			_count++;                                                           // .. increment executed instruction count
		}

		/*
		 * Device management
		 */

		public int IoccAddress { get; set; }                                // Address from IOCC
		public int IoccDeviceCode { get; set; }                             // Device code from IOCC
		public DevFunction IoccFunction { get; set; }                       // Function from IOCC
		public int IoccModifiers { get; set; }                              // Modifier from IOCC
		public IDevice IoccDevice { get; set; }                             // Device referenced

		public bool AddDevice(IDevice device)                               // Add device to system
		{
			if (Devices[device.DeviceCode] != null)                             // q. device in use?
			{                                                                   // a. yes ..
				return false;                                                   // .. can't add now
			}
			Devices[device.DeviceCode] = device;                                // otherwise ... add the device
			return true;                                                        // .. and tell 'em it worked
		}

		public ArraySegment<ushort> GetBuffer()                             // get the i/o buffer only
		{
			return new ArraySegment<ushort>(Memory, IoccAddress + 1, Memory[IoccAddress]);
		}

		public void TransferToMemory(int wcAddr, ushort[] values, int max)  // Cycle steal
		{
			var transferCount = Memory[wcAddr] > max ? max : Memory[wcAddr];    // get amount to transfer
			if (transferCount > 0)                                              // q. anything to transfer
			{                                                                   // a. yes..
				Array.Copy(values, 0, Memory, wcAddr + 1, transferCount);           // .. copy whatever into memory
			}
		}

		public void TransferToMemory(ushort[] values, int max)
		{
			TransferToMemory(IoccAddress, values, max);
		}

		public ulong InstructionCount { get { return _count; } }            // number of instructions executed
		public bool IgnoreInstructionCount { get; set; }                    // ... ignore waiting for instruction
		public bool MasterDebug { get; set; }                               // Set to true to do debug checking.

		public void LetInstuctionsExecute(ulong numberOfInstructions)       // Let some number of instructions execute until wait
		{
			if (IgnoreInstructionCount)                                         // q. wait for instructions to run?
			{                                                                   // a. no .. 
				return;                                                         // .. leave now
			}
			var endCount = InstructionCount + numberOfInstructions;             // calculate the end count of instructions
			while (InstructionCount < endCount)                                 // loop until we get to the count
			{
				if (Wait)                                                       // q. did we hit a wait state?
					break;                                                      // a. yes .. leave now
				Thread.Sleep(0);
			}
		}
		public void IoccDecode(int address)                                 // Decode an IOCC
		{
			IoccAddress = Memory[address];                                      // get the memory address (Note even address?)
			ushort secondWord = Memory[address | 1];                                // then pull second word (Note odd address!)
			IoccDeviceCode = (secondWord & 0xf800) >> 11;                       // .. extract device code
			IoccFunction = (DevFunction)((secondWord & 0x0700) >> 8);           // .. extract function
			IoccModifiers = secondWord & 0xff;                                  // .. and extract modifiers
			IoccDevice = Devices[IoccDeviceCode];                               // .. finally, get the device reference
		}

		private void BuildDefaultDevices()                                  // add default devices to system
		{
			Devices = new IDevice[32];                                          // init the device array
			AddDevice(new ConsoleEntrySwitches(this));                          // .. add console entry switches
		}

		public void SetDebug(int location, IDebugSetting debugSetting)      // set a particulare memory location to debug mode
		{
			_debugSettings[location] = debugSetting;                            // set the debug setting
		}

		public void ResetDebug(int location)                                // turn off debugging a memory location
		{
			_debugSettings[location] = null;                                    // clear the debug setting
		}
	}
}