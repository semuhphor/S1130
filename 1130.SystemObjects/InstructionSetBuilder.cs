using System;
using System.Reflection;
using S1130.SystemObjects.Instructions;
using OpCodes = S1130.SystemObjects.Instructions.OpCodes;

namespace S1130.SystemObjects
{
    public class InstructionSetBuilder : InstructionBase
    {
        private static readonly IInstruction[] Instructions = new IInstruction[32];
		
        static InstructionSetBuilder()
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

	    private InstructionSetBuilder() {}

	    public static IInstruction[] GetInstructionSet()
	    {
		    return Instructions;
	    }
    }
}