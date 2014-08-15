namespace S1130.SystemObjects
{
    public class InstructionBuilder
    {
        public static ushort BuildShort(Instructions instruction, uint tag, uint displacement)
        {
            return (ushort) ((((uint) instruction << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
        }

        public static void BuildLongIndirectAtIar(Instructions instruction, uint tag, ushort displacement, ISystemState state)
        {
            state[state.Iar] = BuildShort(instruction, tag, 0);
            state[state.Iar] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
            state[state.Iar+1] = displacement;
        }

        public static void BuildLongAtIar(Instructions instruction, uint tag, ushort displacement, ISystemState state)
        {
            state[state.Iar] = BuildShort(instruction, tag, 0);
            state[state.Iar] |= Constants.FormatLong; // Format Long & Indirect Addressing
            state[state.Iar+1] = displacement;
        }
    }
}