namespace S1130.SystemObjects.Instructions
{
	/// <summary>
	/// Implements the IBM 1130 Add instruction (A).
	/// Adds the value at the effective address to the accumulator, setting carry and overflow flags.
	/// </summary>
	public class Add : InstructionBase, IInstruction
	{
		/// <summary>
		/// Gets the operation code for the Add instruction.
		/// </summary>
		public OpCodes OpCode { get { return OpCodes.Add; }  }
		
		/// <summary>
		/// Gets the operation name for the Add instruction.
		/// </summary>
		public string OpName { get { return "A";  } }

		/// <summary>
		/// Executes the Add instruction on the specified CPU.
		/// </summary>
		/// <param name="cpu">The CPU instance to execute the instruction on</param>
		public void Execute(ICpu cpu)
		{	
			var valueToAdd = cpu[GetEffectiveAddress(cpu)];
			var oldAcc = cpu.Acc;
			var result = (uint) oldAcc + valueToAdd;
			cpu.Acc = (ushort) (result & 0xffff);
			cpu.Carry = (result & 0x10000) != 0;
			if (!cpu.Overflow)
			{
				cpu.Overflow = Is16BitSignBitOn((~valueToAdd ^ oldAcc) & (valueToAdd ^ cpu.Acc));
			}
		}
	}
}