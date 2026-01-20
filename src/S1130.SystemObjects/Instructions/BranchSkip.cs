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
				else
				{
					SetIarToNextInstruction(cpu);						// go to next instruction
				}
			}
			else                                                        // otherwise..
			{                                                           // .. short format
				cpu.Iar++;												// always go to the next word
				if (condition)											// q. condition met?
				{														// a. yes ..
					cpu.Iar++;											// .. then skip another
				}
			}
			if (resetInterrupt && condition)							// reset requested and condtion met?
			{															// a. yes .. 
				cpu.ClearCurrentInterrupt();							// .. clear the current interrupt
			}
		}
		
		/// <summary>
		/// Disassembles BSC/BOSC instruction with condition codes.
		/// New Format: BSC |formatTag|condition,address or BSC |formatTag|address,condition
		/// Examples: "BSC O", "BSC |L|/0100,Z", "BOSC |L2|/0200,+-", "BSC |I1|/0300,E"
		/// Short format conditions only (no address): "BSC Z", "BSC +-O"
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var parts = new System.Collections.Generic.List<string>();
			
			// Check for interrupt reset bit (0x40) to determine opcode
			var resetInterrupt = (cpu.Modifiers & 0x40) != 0;
			parts.Add(resetInterrupt ? "BOSC" : "BSC");
			
			// Build condition codes string in conventional order
			var conditions = new System.Text.StringBuilder();
			var modifiers = cpu.Modifiers & 0x3F; // Mask out reset interrupt bit (0x40)
			
			// Order: + - Z E C O (typical IBM 1130 convention)
			if ((modifiers & Plus) != 0) conditions.Append("+");
			if ((modifiers & Minus) != 0) conditions.Append("-");
			if ((modifiers & Zero) != 0) conditions.Append("Z");
			if ((modifiers & Even) != 0) conditions.Append("E");
			if ((modifiers & CarryOff) != 0) conditions.Append("C");
			if ((modifiers & OverflowOff) != 0) conditions.Append("O");
			
			string condStr = conditions.ToString();
			
			// Determine format
			if (cpu.FormatLong)
			{
				// Long format: has address and conditions
				// Format: MNEMONIC |modifier|/address,conditions
				
				// Build format modifier
				string modifier = "";
				if (cpu.IndirectAddress)
				{
					modifier = cpu.Tag > 0 ? $"|I{cpu.Tag}|" : "|I|";
				}
				else
				{
					modifier = cpu.Tag > 0 ? $"|L{cpu.Tag}|" : "|L|";
				}
				
				ushort targetAddress = cpu.Displacement;
				
				// Format: MNEMONIC |modifier|/address,conditions
				parts.Add($"{modifier}/{targetAddress:X4},{condStr}");
			}
			else
			{
				// Short format: skip only (no address), just conditions
				// or if has index: |X|conditions
				if (cpu.Tag > 0)
				{
					parts.Add($"|{cpu.Tag}|{condStr}");
				}
				else
				{
					parts.Add(condStr);
				}
			}
			
			return string.Join(" ", parts);
		}
	}
}