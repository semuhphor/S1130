namespace S1130.SystemObjects.Instructions
{
    /// <summary>
    /// Implements the IBM 1130 Load instruction (LD).
    /// Loads the value from the effective address into the accumulator.
    /// </summary>
    public class Load : InstructionBase, IInstruction
    {
        /// <summary>
        /// Gets the operation code for the Load instruction.
        /// </summary>
        public OpCodes OpCode { get { return OpCodes.Load; }  }
        
        /// <summary>
        /// Gets the operation name for the Load instruction.
        /// </summary>
        public string OpName { get { return "LD";  } }

        /// <summary>
        /// Executes the Load instruction on the specified CPU.
        /// </summary>
        /// <param name="cpu">The CPU instance to execute the instruction on</param>
        public void Execute(ICpu cpu)
        {
            cpu.Acc = cpu[GetEffectiveAddress(cpu)];
        }
    }
}