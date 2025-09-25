namespace S1130.SystemObjects.Instructions
{
	/// <summary>
	/// Implements the IBM 1130 Divide instruction (D).
	/// Divides the 32-bit AccExt by the value at the effective address.
	/// Sets overflow flag on division by zero or quotient overflow.
	/// </summary>
	public class Divide : InstructionBase, IInstruction
	{
		/// <summary>
		/// Gets the operation code for the Divide instruction.
		/// </summary>
		public OpCodes OpCode { get { return OpCodes.Divide; }  }
		
		/// <summary>
		/// Gets the operation name for the Divide instruction.
		/// </summary>
		public string OpName { get { return "D";  } }

		/// <summary>
		/// Executes the Divide instruction on the specified CPU.
		/// </summary>
		/// <param name="cpu">The CPU instance to execute the instruction on</param>
		public void Execute(ICpu cpu)
		{
			var divider = cpu[GetEffectiveAddress(cpu)];
			if (divider != 0)
			{
				var quotient = (int) cpu.AccExt / (int) SignExtend(divider);
				if (quotient > short.MaxValue)
				{
					cpu.Overflow = true;
					cpu.AccExt = (uint) quotient & 0xffffffff; /* Undefined... let's make AccExt the quotient */
				}
				else
				{
					cpu.Ext = (ushort) (cpu.AccExt % (short) divider);
					cpu.Acc = (ushort) (quotient & 0xffff);
				}
			}
			else
			{
				cpu.Overflow = true;
			}
		}
	}
}