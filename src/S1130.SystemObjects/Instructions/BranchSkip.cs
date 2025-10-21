namespace S1130.SystemObjects.Instructions
{
	public class BranchSkip : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchSkip; }  }
		public string OpName { get { return "BSC";  } }

		public void Execute(ICpu cpu)
		{
			var resetInterrupt = (cpu.Modifiers & 0x40) != 0;			// assume reset interrupts
			var condition = TestCondition(cpu);							// .. check conditions
			if (cpu.FormatLong)											// q. is format long?
			{															// a. yes ..
				condition = condition == false;							// .. invert condtion
				if (condition)											// q. condition met?
				{														// a.yes
					cpu.Iar = (ushort) GetEffectiveAddress(cpu);		// branch to address
				}

			}
			else														// otherwise..
			{															// .. short format
				if (condition)											// q. condition met?
				{														// a. yes ..
					cpu.Iar++;											// .. then skip
				}
			}
			if (resetInterrupt && condition)							// reset requested and condtion met?
			{															// a. yes .. 
				cpu.ClearCurrentInterrupt();							// .. clear the current interrupt
			}
		}
		
		/// <summary>
		/// Disassembles BSC instruction with condition codes.
		/// IBM 1130 ACTUAL Format: BSC [L] address[,condition][,Xn] [I]
		/// Example: "BSC L /0500,Z", "BSC /0100,Z", "BSC L /0200,+-"
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var parts = new System.Collections.Generic.List<string>();
			parts.Add("BSC");
			
			// Long format
			if (cpu.FormatLong)
				parts.Add("L");
			
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
			
			// Build address with optional conditions and index register
			var addressPart = new System.Text.StringBuilder($"/{targetAddress:X4}");
			
			// Condition codes
			var conditions = new System.Text.StringBuilder();
			var modifiers = cpu.Modifiers & 0x3F; // Mask out reset interrupt bit (0x40)
			
			if ((modifiers & Zero) != 0) conditions.Append("Z");
			if ((modifiers & Minus) != 0) conditions.Append("-");
			if ((modifiers & Plus) != 0) conditions.Append("+");
			if ((modifiers & Even) != 0) conditions.Append("E");
			if ((modifiers & Carry) != 0) conditions.Append("C");
			if ((modifiers & Overflow) != 0) conditions.Append("O");
			
			// Add conditions after address
			if (conditions.Length > 0)
			{
				addressPart.Append($",{conditions}");
			}
			
			// Add index register after conditions
			if (cpu.Tag > 0)
			{
				addressPart.Append($",{cpu.Tag}");
			}
			
			parts.Add(addressPart.ToString());
			
			// Indirect
			if (cpu.IndirectAddress)
				parts.Add("I");
			
			return string.Join(" ", parts);
		}
	}
}