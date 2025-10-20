namespace S1130.SystemObjects.Instructions
{
	public class ModifyIndex : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ModifyIndex; }  }
		public string OpName { get { return "MDX";  } }

		public void Execute(ICpu cpu)
		{
			int oldValue;													// old value
			int newValue;													// .. and new value

			if (cpu.FormatLong)											// q. long format?
			{																// a. yes..
				var effectiveAddress = cpu.Displacement;					// .. Get effective Address
				if (cpu.Tag != 0)											// q. is target XR?
				{															// a. yes ... 
					newValue = oldValue = cpu.Xr[cpu.Tag];					// .. get old value
					newValue += ((cpu.IndirectAddress						// .. plus .. q. indirect?
							? cpu[effectiveAddress]							// .. .. .. ..a. yes .. word @address
							: effectiveAddress));							// .. .. .. .... or just address to the new value
					cpu.Xr[cpu.Tag] = (ushort) (newValue & Mask16);			// reset target
				}
				else														// otherwise..
				{															// .. long and no tag
					newValue = oldValue = cpu[effectiveAddress];			// get the old value from memory..
					newValue += (sbyte) (cpu.Modifiers & 0xff);				// .. increment by displacement
					cpu[effectiveAddress] = (ushort) (newValue & Mask16);	// reset target
				}
			}
			else															// otherwise
			{																// .. short format
				var displacement = (sbyte) (cpu.Displacement & 0xff);		// get amount of change
				newValue = oldValue = cpu.Xr[cpu.Tag];						// .. get old value
				newValue += displacement;									// .. increment by displacement
				cpu.Xr[cpu.Tag] = (ushort) (newValue & Mask16);				// reset target
			}
			var longOrHasTag = cpu.FormatLong || (cpu.Tag != 0);
			var newValueChange = ((newValue == 0) || ((newValue & 0x8000) != (oldValue & 0x8000)));
			if (longOrHasTag && newValueChange)								// q. long or tag & new value changed?
			{																// a. yes..
				cpu.Iar++;													// skip next instruction
			}
		}
		
		/// <summary>
		/// Disassembles MDX instruction.
		/// Format: MDX [L] [Xn] /address [I]
		/// Example: "MDX L 2 /0100 I", "MDX 1 /0050", "MDX L /0200"
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var parts = new System.Collections.Generic.List<string>();
			parts.Add("MDX");
			
			// Format
			if (cpu.FormatLong)
				parts.Add("L");
			
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
			
			// Indirect
			if (cpu.IndirectAddress)
				parts.Add("I");
			
			return string.Join(" ", parts);
		}
	}
}