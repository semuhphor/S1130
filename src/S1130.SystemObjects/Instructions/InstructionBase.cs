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
    }
}