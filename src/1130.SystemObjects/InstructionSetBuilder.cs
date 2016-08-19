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
            var instructionDictionary = new OpcodeReference().ReferencDictionary;
            foreach (OpCodes opCode in Enum.GetValues(typeof (OpCodes)))
            {
                var instruction = instructionDictionary[opCode];
                Instructions[(int) opCode] = instruction;
            }
            instructionDictionary = null;
        }

	    private InstructionSetBuilder() {}

	    public static IInstruction[] GetInstructionSet()
	    {
		    return Instructions;
	    }
    }
}