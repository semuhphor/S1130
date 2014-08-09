using System;

namespace _1130.SystemObjects
{
    public class SystemState
    {
        public const int DefaultMemorySize = 32768;

        public ushort[] Memory = new ushort[DefaultMemorySize];
        public int MemorySize = DefaultMemorySize;
        public ushort Iar;
        public ushort Acc;
        public ushort Ext;

        public ushort Opcode;
        public bool Format;
        public ushort Tag;
        public ushort Displacement;
        public ushort Address;
        public bool IndirectAddress;
        public ushort Modifiers;

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

    public class InstructionBuilder
    {
        public static ushort BuildShort(Instructions instruction, uint tag, uint displacement)
        {
            return (ushort) ((((uint) instruction << 11) | (tag << 8) | (displacement & 0xff)) & 0xffff);
        }
    }
}