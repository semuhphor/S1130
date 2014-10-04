using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Permissions;

namespace S1130.SystemObjects
{
	public class SystemState : ISystemState
    {
        public const int DefaultMemorySize = 32768;
		private readonly ConcurrentQueue<IInterruptingDevice>[] _interruptQueues;
		private readonly ConcurrentStack<IInterruptingDevice> _currentDevice;
	    private readonly IInstructionSet _instructionSet;
		
        public SystemState()
        {
            MemorySize = DefaultMemorySize;
            Memory = new ushort[DefaultMemorySize];
            Xr = new IndexRegisters(this);
			_instructionSet = new InstructionSet();
			_interruptQueues =  new ConcurrentQueue<IInterruptingDevice>[6]
					{
						new ConcurrentQueue<IInterruptingDevice>(), 
						new ConcurrentQueue<IInterruptingDevice>(), 
						new ConcurrentQueue<IInterruptingDevice>(), 
						new ConcurrentQueue<IInterruptingDevice>(), 
						new ConcurrentQueue<IInterruptingDevice>(), 
						new ConcurrentQueue<IInterruptingDevice>()
					};
			_currentDevice = new ConcurrentStack<IInterruptingDevice>();
        }

        public ushort[] Memory { get; set; } 
        public int MemorySize { get; set; }
		public ushort ConsoleSwitches { get; set; }
		public ushort Iar { get; set; }
        public ushort Acc { get; set; }
        public ushort Ext { get; set; }

	    public uint AccExt
	    {
		    get { return (uint) ((Acc << 16) | Ext); }
		    set
		    {
			    Acc = (ushort) (value >> 16);
			    Ext = (ushort) (value & 0xffff);
		    }
	    }

	    public IndexRegisters IndexRegister { get; set; }
        public IndexRegisters Xr { get; private set; }
        public ushort Opcode { get; set; }
        public bool FormatLong { get; set; }
        public ushort Tag { get; set; }
        public ushort Displacement { get; set; }
        public bool IndirectAddress { get; set; }
        public ushort Modifiers { get; set; }
        public bool Carry { get; set; }
        public bool Overflow { get; set; }
		public bool Wait { get; set; }

		public int? Interrupt
		{
			get
			{
				for (int i = 0; i < 6; i++)
				{
					if (!_interruptQueues[i].IsEmpty)
					{
						return i;
					}
				}
				return null;
			}
		}

		public void AddInterrupt(IInterruptingDevice device)					// Add device to interrupt queue
		{
			var interruptLevel = device.InterruptLevel;								// Get tthe interrupt level
			if (interruptLevel >= 0 && interruptLevel <= 5)							// q. Interrupt level in range?
			{																		// a. yes .. 
				_interruptQueues[interruptLevel].Enqueue(device);					// .. queue the interrupt
			}
		}

		private bool ShouldHandleInterrupt(int interruptLevel)					// true if we should handle the interrupt
		{
			if (!_currentDevice.IsEmpty)											// q. device active?
			{																		// a. yes .. 
				IInterruptingDevice device;											// .. get current device
				if (_currentDevice.TryPeek(out device))								// q. current device on stack?
				{																	// a. yes ..
					if (interruptLevel >= device.InterruptLevel)					// q. higher level interrupt?
					{																// a. no ..
						return false;												// .. don't handle the interrupt
					}
				}
			}																		// otherwise...
			return true;															// ... handle the interrupt
		}

		public void HandleInterrupt()											// handle current interrupt
		{
			if (Interrupt.HasValue)													// q. interrupt active?
			{																		// a. yes..
				var interrupt = Interrupt.Value;									// .. save the interrupt number
				if (!ShouldHandleInterrupt(interrupt))								// q. should the interrupt be handled?
				{																	// a. no..
					return;															// .. don't handle it.
				}
				var intVector = Constants.InterruptVectors[interrupt];				// get the vector 
				IInterruptingDevice device;											// .. and a place for the device
				if (_interruptQueues[interrupt].TryPeek(out device))				// q. found device causing interrupt?
				{																	// a. yes..
					CurrentDevice.Push(device);										// .. push it as current device	
					var addressOfInterruptHandler = Memory[intVector];				// .. get the address of the handler
					Memory[addressOfInterruptHandler] = Iar;						// .. save the current IAR return address
					Iar = (ushort) (addressOfInterruptHandler + 1);					// .. and go to the interrupt handler
				}																
			}
		}

		public void ClearCurrentInterrupt()
		{
			IInterruptingDevice device;		
			if (CurrentDevice.TryPop(out device))
			{
				InterruptQueues[device.InterruptLevel].TryDequeue(out device);
			}
		}

		public void NextInstruction()
        {
            var firstWord = Memory[Iar++];
            Opcode = (ushort) ((firstWord & 0xF800) >> 11);
            FormatLong = (firstWord & 0x0400) != 0 && _instructionSet.MayBeLong(Opcode);
            Tag = (ushort) ((firstWord & 0x0300) >> 8);
			Modifiers = (ushort) (firstWord & 0xff);
            if (FormatLong)
            {
                Displacement = Memory[Iar++];
                IndirectAddress = (firstWord & 0x80) != 0;
            }
            else
            {
				Displacement = Modifiers;
                IndirectAddress = false;
            }
        }

	    public IInstruction GetInstruction()
	    {
		    return _instructionSet.GetInstruction(this);
	    }

		public ConcurrentQueue<IInterruptingDevice>[] InterruptQueues { get { return _interruptQueues; } }
		public ConcurrentStack<IInterruptingDevice> CurrentDevice { get { return _currentDevice; }  }

		public void ExecuteInstruction()
	    {
		    _instructionSet.Execute(this);
	    }

	    public ushort this[int address]
        {
            get { return Memory[address]; }
            set { Memory[address] = value; }
        }
    }
}