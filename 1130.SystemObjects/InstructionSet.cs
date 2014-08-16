using System;
using System.Collections.Generic;
using System.Reflection;
using S1130.SystemObjects.Instructions;
using OpCodes = S1130.SystemObjects.Instructions.OpCodes;

namespace S1130.SystemObjects
{
    public class InstructionSet : InstructionBase, IInstructionSet
    {
        private static readonly Dictionary<OpCodes, IInstruction> Instructions;

        static InstructionSet()
        {
            //if (Instructions != null) return;
            Instructions = new Dictionary<OpCodes, IInstruction>();
            var assembly = Assembly.Load("S1130.SystemObjects");
            foreach (OpCodes opCode in Enum.GetValues(typeof (OpCodes)))
            {
                var instructionName = Enum.GetName(typeof (OpCodes), opCode);
                var t = assembly.GetType("S1130.SystemObjects.Instructions." + instructionName);
                if (t == null)
                {
                    throw new Exception("Invalid instruction: " + instructionName);
                }
                var instruction = (IInstruction) Activator.CreateInstance(t);
                Instructions.Add(opCode, instruction);
            }
        }

        public void Execute(ISystemState state)
        {
            Instructions[(OpCodes) state.Opcode].Execute(state);
        }
    }
}