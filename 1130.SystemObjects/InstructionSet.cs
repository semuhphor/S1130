using System;
using System.Reflection;
using S1130.SystemObjects.Instructions;
using OpCodes = S1130.SystemObjects.Instructions.OpCodes;

namespace S1130.SystemObjects
{
    public class InstructionSet : InstructionBase, IInstructionSet
    {
        private static readonly IInstruction[] Instructions = new IInstruction[32];

        static InstructionSet()
        {
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
                Instructions[(int) opCode] = instruction;
            }
        }

	    public bool MayBeLong(int opcode)
	    {
		    return Instructions[opcode].HasLongFormat;
	    }

	    public void Execute(ICpu cpu)
	    {
		    var instruction = Instructions[cpu.Opcode];
		    if (instruction != null)
		    {
			    instruction.Execute(cpu);
		    }
		    else
		    {
			    cpu.Wait = true;
		    }
	    }

	    public IInstruction GetInstruction(ICpu cpu)
	    {
		    return Instructions[cpu.Opcode];
	    }
    }
}