namespace S1130.SystemObjects.Instructions
{
	public class BranchStore : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchStore; }  }
		public string OpName { get { return "BSI";  } }

		public void Execute(ICpu cpu)
		{
			if (cpu.FormatLong && TestCondition(cpu))				// q. Long format & condition met?
			{															// a. yes
				return;													// .. take no action
			}
			var subroutineAddress = (ushort)GetEffectiveAddress(cpu);	// resolve routine address
			cpu[subroutineAddress++] = cpu.Iar;						// save return address and get entry address
			cpu.Iar = subroutineAddress;								// .. and branch to routine
		}
		
		/// <summary>
		/// Disassembles BSI instruction (Branch and Store IAR for subroutine calls).
		/// Format: BSI [L] /address [I] or BSI [I][Xn] /address for short
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var parts = new System.Collections.Generic.List<string>();
			parts.Add("BSI");
			
			// For long format
			if (cpu.FormatLong)
			{
				parts.Add("L");
			}
			
			// Index register
			if (cpu.Tag > 0)
				parts.Add(cpu.Tag.ToString());
			
			// Calculate target address
			ushort targetAddress;
			if (cpu.FormatLong)
			{
				targetAddress = cpu.Displacement;
			}
			else
			{
				int relativeAddress = (address + 1) + (sbyte)cpu.Displacement;
				targetAddress = (ushort)(relativeAddress & 0xFFFF);
			}
			
			parts.Add($"/{targetAddress:X4}");
			
			// Indirect addressing
			if (cpu.IndirectAddress)
				parts.Add("I");
			
			return string.Join(" ", parts);
		}
	}
}