using System.Reflection.Emit;
using OpCodes = S1130.SystemObjects.Instructions.OpCodes;

namespace S1130.SystemObjects
{
    public class InstructionBuilder
    {
        public static ushort BuildShort(OpCodes opCode, uint tag, uint displacement)
        {
            return (ushort) ((((uint) opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
        }

        public static void BuildLongIndirectAtIar(OpCodes opCode, uint tag, ushort displacement, ISystemState state)
        {
            state[state.Iar] = BuildShort(opCode, tag, 0);
            state[state.Iar] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
            state[state.Iar+1] = displacement;
        }

        public static void BuildLongAtIar(OpCodes opCode, uint tag, ushort displacement, ISystemState state)
        {
            state[state.Iar] = BuildShort(opCode, tag, 0);
            state[state.Iar] |= Constants.FormatLong; // Format Long & Indirect Addressing
            state[state.Iar+1] = displacement;
        }

	    public static ushort BuildShortBranch(OpCodes opCode, uint tag, byte modifiers)
	    {
			return (ushort)((((ushort)opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | ((ushort) modifiers & Constants.DisplacementMask)) & ushort.MaxValue);
		}
    }
}