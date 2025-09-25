namespace S1130.SystemObjects.Instructions
{
    /// <summary>
    /// Implements the IBM 1130 Store instruction (STO).
    /// Stores the accumulator value to the effective address in memory.
    /// </summary>
    public class Store : InstructionBase, IInstruction
    {
        /// <summary>
        /// Gets the operation code for the Store instruction.
        /// </summary>
        public OpCodes OpCode { get {return OpCodes.Store; } }
        
        /// <summary>
        /// Gets the operation name for the Store instruction.
        /// </summary>
        public string OpName { get { return "STO"; } }

        /// <summary>
        /// Executes the Store instruction on the specified CPU.
        /// </summary>
        /// <param name="cpu">The CPU instance to execute the instruction on</param>
        public void Execute(ICpu cpu)
        {
            cpu[GetEffectiveAddress(cpu)] = cpu.Acc;
        }
    }
}