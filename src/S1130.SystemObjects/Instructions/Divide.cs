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
		/// <remarks>
		/// The 32-bit dividend in Acc:Ext is divided by the 16-bit word at the effective address.
		/// Quotient goes to Acc, remainder to Ext; the remainder takes the sign of the dividend.
		/// Overflow is set (and Acc/Ext left unchanged) on division by zero or when the quotient
		/// is outside -32768..32767. The hardware leaves Acc/Ext undefined in that case.
		/// </remarks>
		public void Execute(ICpu cpu)
		{
			var divider = (int)(short)cpu[GetEffectiveAddress(cpu)];
			if (divider != 0)
			{
				var dividend = (int)cpu.AccExt;
				var quotient = (long)dividend / divider;
				if (quotient > short.MaxValue || quotient < short.MinValue)
				{
					cpu.Overflow = true;
				}
				else
				{
					cpu.Ext = (ushort)(dividend % divider);
					cpu.Acc = (ushort)(quotient & 0xffff);
				}
			}
			else
			{
				cpu.Overflow = true;
			}
			SetIarToNextInstruction(cpu);
		}
	}
}