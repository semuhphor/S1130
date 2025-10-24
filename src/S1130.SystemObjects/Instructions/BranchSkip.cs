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
		/// Disassembles BSC/BOSC instruction with condition codes.
		/// New Format: BSC/BOSC formatTag [condition] address
		/// Examples: "BSC . O /0500", "BSC L Z /0100", "BOSC L2 +- /0200", "BSC I1 E /0300"
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var parts = new System.Collections.Generic.List<string>();
			
			// Check for interrupt reset bit (0x40) to determine opcode
			var resetInterrupt = (cpu.Modifiers & 0x40) != 0;
			parts.Add((resetInterrupt ? "BOSC" : "BSC").PadRight(5));
			
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
			
			// Condition codes
			var conditions = new System.Text.StringBuilder();
			var modifiers = cpu.Modifiers & 0x3F; // Mask out reset interrupt bit (0x40)
			
			if ((modifiers & Zero) != 0) conditions.Append("Z");
			if ((modifiers & Minus) != 0) conditions.Append("-");
			if ((modifiers & Plus) != 0) conditions.Append("+");
			if ((modifiers & Even) != 0) conditions.Append("E");
			if ((modifiers & Carry) != 0) conditions.Append("C");
			if ((modifiers & Overflow) != 0) conditions.Append("O");
			
			// Add conditions before address (if any)
			if (conditions.Length > 0)
				parts.Add(conditions.ToString());
			
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
			
			// Add address
			parts.Add($"/{targetAddress:X4}");
			
			return string.Join(" ", parts);
		}
	}
}