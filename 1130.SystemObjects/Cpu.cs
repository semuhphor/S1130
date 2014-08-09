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
        Load = 0x18,
        LoadDouble = 0x19
    };

    public class Cpu
    {
        public SystemState GetInitialSystemState()
        {
            var state = new SystemState();
            return state;
        }

        //public SystemState Continue(SystemState state)
        //{
        //    ExecuteInstruction(state);
        //    return state;
        //}

        public SystemState ExecuteInstruction(SystemState state)
        {
            var instruction = (Instructions) state.Opcode;

            switch (instruction)
            {
                case Instructions.Load:
                    state.Acc = state[GetEffectiveAddress(state)];
                    break;
                case Instructions.LoadDouble:
                    int effectiveAddress = GetEffectiveAddress(state);
                    state.Acc = state[effectiveAddress++];
                    state.Ext = state[effectiveAddress];
                    break;
                default:
                    throw new Exception("Unknown instruction");
            }
            return state;
        }

        public int GetEffectiveAddress(SystemState state)
        {
            if (!state.Format) // short InstructionBuilder
            {
                return GetBase(state) + (short)state.Displacement;
            }
            return 0;
        }

        private int GetBase(SystemState state)
        {
            return ((int)(state.Tag == 0 ? state.Iar : state[state.Tag]));
        }

    }
}
