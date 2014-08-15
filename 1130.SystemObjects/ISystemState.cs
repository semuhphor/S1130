using System.Dynamic;

namespace S1130.SystemObjects
{
    public interface ISystemState
    {
        ushort[] Memory { get; }
        int MemorySize { get; }

        ushort Iar { get; set; }
        ushort Acc { get; set; }
        ushort Ext { get; set; }
        ushort this[int address] { get; set; }

        IndexRegisters Xr { get; }

        ushort Opcode { get;  }
        bool FormatLong { get;  }
        ushort Tag { get;  }
        ushort Displacement { get;  }
        bool IndirectAddress { get;  }
        ushort Modifiers { get;  }

        void NextInstruction();
    }
}