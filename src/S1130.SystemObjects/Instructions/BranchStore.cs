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
			parts.Add("BSI  "); // Padded to 5 chars
			
			// Build format/tag string
			var formatTag = new System.Text.StringBuilder();
			
			if (cpu.IndirectAddress)
			{
				// Indirect addressing
				formatTag.Append("I");
				if (cpu.Tag > 0)
					formatTag.Append(cpu.Tag);
			}
			else if (cpu.FormatLong)
			{
				// Long format
				formatTag.Append("L");
				if (cpu.Tag > 0)
					formatTag.Append(cpu.Tag);
			}
			else
			{
				// Short format
				if (cpu.Tag > 0)
					formatTag.Append(cpu.Tag);
				else
					formatTag.Append(".");
			}
			
			parts.Add(formatTag.ToString());
			
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
			
			return string.Join(" ", parts);
		}
	}
}