using S1130.SystemObjects.Instructions;

namespace S1130.SystemObjects
{
    public interface IInstruction
    {
        OpCodes OpCode { get; }
        string OpName { get; }
		bool HasLongFormat { get; }
        void Execute(ICpu cpu);
        
        /// <summary>
        /// Disassembles the instruction at the current CPU state into assembly source format.
        /// Returns a string that can be reassembled to produce the same instruction.
        /// </summary>
        /// <param name="cpu">CPU with decoded instruction fields (FormatLong, Tag, Displacement, IndirectAddress, Modifiers)</param>
        /// <param name="address">Memory address where the instruction is located (for calculating relative addresses)</param>
        /// <returns>Assembly source string (e.g., "LD L /100", "BSC I1 TARGET,Z")</returns>
        string Disassemble(ICpu cpu, ushort address);
    }
}