using System;

namespace S1130.SystemObjects
{
    public class SystemState : ISystemState
    {
        public const int DefaultMemorySize = 32768;

        public SystemState()
        {
            MemorySize = DefaultMemorySize;
            Memory = new ushort[DefaultMemorySize];
            Xr = new IndexRegisters(this);
        }

        public ushort[] Memory { get; set; } 
        public int MemorySize { get; set; }
        public ushort Iar { get; set; }
        public ushort Acc { get; set; }
        public ushort Ext { get; set; }

        public IndexRegisters IndexRegister { get; set; }
        public IndexRegisters Xr { get; private set; }
        public ushort Opcode { get; set; }
        public bool Format { get; set; }
        public ushort Tag { get; set; }
        public ushort Displacement { get; set; }
        public ushort Address { get; set; }
        public bool IndirectAddress { get; set; }
        public ushort Modifiers { get; set; }
        
       public void NextInstruction()
        {
            var firstWord = Memory[Iar];
            Opcode = (ushort) ((firstWord & 0xF800) >> 11);
            Format = (firstWord & 0x0400) != 0;
            Tag = (ushort) ((firstWord & 0x0300) >> 8);
            Iar++;
            if (Format)
            {
                throw new Exception("Long form not yet implemented. See commented code that is not yet tested.");
                // Modifiers, Address, IA, Reset displacement; iar++
            }
            else
            {
                Displacement = (ushort) (firstWord & 0xff);
                Address = 0;
                IndirectAddress = false;
                Modifiers = 0;
            }
        }

        public ushort this[int address]
        {
            get { return Memory[address]; }
            set { Memory[address] = value; }
        }
    }
}