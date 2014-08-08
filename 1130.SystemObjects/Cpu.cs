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
            var effectiveAddress = state.EffectiveAddress;

            switch (instruction)
            {
                case Instructions.Load:
                    state.Acc = state[effectiveAddress];
                    break;
            }
            return state;
        }
    }
}
