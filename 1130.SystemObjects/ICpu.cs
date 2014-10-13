using System.Collections.Concurrent;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
    public interface ICpu
    {
        ushort[] Memory { get; }
        int MemorySize { get; }

		ushort ConsoleSwitches { get; set; }
        ushort Iar { get; set; }
        ushort Acc { get; set; }
        ushort Ext { get; set; }
		uint AccExt { get; set; }
        ushort this[int address] { get; set; }

        IndexRegisters Xr { get; }
		IInstruction[] Instructions { get;  }
		IInstruction CurrentInstruction { get; }

        ushort Opcode { get;  }
        bool FormatLong { get;  }
        ushort Tag { get;  }
        ushort Displacement { get;  }
        bool IndirectAddress { get;  }
        ushort Modifiers { get;  }

		bool Carry { get; set;  }
		bool Overflow { get; set; }
		bool Wait { get; set; }
		int? Interrupt { get;  }
	    void AddInterrupt(IInterruptingDevice device);
	    void HandleInterrupt();
	    void ClearCurrentInterrupt();

        void NextInstruction();
	    void ExecuteInstruction();
	    ConcurrentQueue<IInterruptingDevice>[] InterruptQueues { get; }
		ConcurrentStack<IInterruptingDevice> CurrentDevice { get; } 
    }
}