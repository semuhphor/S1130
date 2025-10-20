namespace S1130.SystemObjects.Instructions
{
   /// <summary>
   /// Base class for all IBM 1130 instruction implementations.
   /// Provides common functionality for address calculation, sign extension, and bit manipulation.
   /// </summary>
   public abstract class InstructionBase
    {
		/// <summary>
		/// Bitmask for 16-bit values.
		/// </summary>
		public const uint Mask16 = 0xffff;
		
		/// <summary>
		/// Bitmask for 32-bit values.
		/// </summary>
		public const uint Mask32 = 0xffffffff;
		
		/// <summary>
		/// Gets a value indicating whether this instruction supports long format.
		/// </summary>
		public virtual bool HasLongFormat { get { return true; }}

	    private int GetEffectiveAddress(ICpu cpu, int baseAddress)
	    {
			var location = baseAddress + GetOffset(cpu);
			if (cpu.FormatLong && cpu.IndirectAddress) // long indirect
			{
				location = cpu[location];
			}
			return location & 0xffff;
		}

	    protected int GetEffectiveAddressNoXr(ICpu cpu)
	    {
			return GetEffectiveAddress(cpu, cpu.FormatLong ? 0 : cpu.Iar);
	    }

		protected bool Is16BitSignBitOn(int value)
		{
			return (value & 0x8000) != 0;
		}

		protected bool Is32BitSignBitOn(uint value)
		{
			return (value & 0x80000000) != 0;
		}

	    protected uint SignExtend(ushort value)
	    {
		    return (uint) (((value & 0x8000) == 0) ? 0 : ~0xffff) | value;
	    }

        protected int GetEffectiveAddress(ICpu cpu)
        {
	        return GetEffectiveAddress(cpu, GetBase(cpu));
        }

		private int GetOffset(ICpu cpu)
		{
			return cpu.FormatLong ? cpu.Displacement : +(sbyte) cpu.Displacement;
		}

        private int GetBase(ICpu cpu)
        {
            return (!cpu.FormatLong || cpu.Tag != 0) ? cpu.Xr[cpu.Tag] : 0;
        }

	    public int GetShiftDistance(ICpu cpu)
	    {
		    return ((cpu.Tag == 0) ? cpu.Displacement : cpu.Xr[cpu.Tag]) & 0x3f;
	    }
	    
	    /// <summary>
	    /// Disassembles a standard format instruction (Load, Store, Arithmetic, etc.).
	    /// Override this method for instructions with special formats (BSC, MDX, Shift, etc.).
	    /// </summary>
	    public virtual string Disassemble(ICpu cpu, ushort address)
	    {
		    // Get the instruction name from the IInstruction implementation
		    var instruction = cpu.CurrentInstruction;
		    if (instruction == null)
			    return "; Unknown instruction";
		    
		    var parts = new System.Collections.Generic.List<string>();
		    parts.Add(instruction.OpName.PadRight(5)); // Mnemonic padded to 5 chars
		    
		    // Build format/modifiers string
		    // Note: Indirect addressing (I) only exists in long format (bit 8 of word 1)
		    // Short format has no indirect bit, so indirect always implies long
		    var formatParts = new System.Collections.Generic.List<string>();
		    
		    if (cpu.IndirectAddress)
		    {
			    // Indirect addressing
			    if (cpu.Tag > 0)
			    {
				    // Indirect with index: "I1", "I2", "I3"
				    formatParts.Add($"I{cpu.Tag}");
			    }
			    else
			    {
				    // Indirect without index: just "I" (long is implied)
				    formatParts.Add("I");
			    }
		    }
		    else if (cpu.FormatLong)
		    {
			    // Long format without indirect
			    if (cpu.Tag > 0)
			    {
				    // Long with index: "L1", "L2", "L3"
				    formatParts.Add($"L{cpu.Tag}");
			    }
			    else
			    {
				    // Just long: "L"
				    formatParts.Add("L");
			    }
		    }
		    else
		    {
			    // Short format
			    if (cpu.Tag > 0)
			    {
				    // Short with index: "1", "2", "3"
				    formatParts.Add(cpu.Tag.ToString());
			    }
			    // If no tag and short format, omit format specifier entirely
		    }
		    
		    // Add format part if we have one
		    if (formatParts.Count > 0)
			    parts.Add(string.Join("", formatParts));
		    
		    // Calculate the target address
		    ushort targetAddress;
		    if (cpu.FormatLong)
		    {
			    targetAddress = cpu.Displacement;
		    }
		    else
		    {
			    // Short format: displacement is relative to IAR after instruction fetch
			    // IAR was advanced past the instruction (1 word for short format)
			    int relativeAddress = (address + 1) + (sbyte)cpu.Displacement;
			    targetAddress = (ushort)(relativeAddress & 0xFFFF);
		    }
		    
		    // Format address in hex with / prefix (matching assembler input format)
		    parts.Add($"/{targetAddress:X4}");
		    
		    return string.Join(" ", parts);
	    }
    }
}