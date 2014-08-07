using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace _1130.SystemObjects
{
    public enum Instructions
    {
        Load = 0x18
    };

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
        public ushort IA;
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
                throw new Exception("Long form not yet implemented.");
                // Modifiers, Address, IA, Reset displacement; iar++
            }
            else
            {
                Displacement = (ushort) (firstWord & 0xff);
                Address = 0;
                IA = 0;
                Modifiers = 0;
            }
        }

        public ushort BuildShort(Instructions instruction, uint tag, uint displacement)
        {
            return (ushort) ((((uint) instruction << 11) | (tag << 8) | (displacement & 0xff)) & 0xffff);
        }
    }

    public class Cpu
    {
        public SystemState GetInitialSystemState()
        {
            var state = new SystemState();
            return state;
        }

        public void Continue(SystemState state)
        {
            
        }
    }
}
