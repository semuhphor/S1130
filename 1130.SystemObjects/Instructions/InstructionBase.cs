using System.Runtime.Remoting.Messaging;

namespace S1130.SystemObjects.Instructions
{
    public abstract class InstructionBase
    {
		public virtual bool HasLongFormat { get { return true; }}

	    private int GetEffectiveAddress(ISystemState state, int baseAddress)
	    {
			var location = baseAddress + GetOffset(state);
			if (state.FormatLong && state.IndirectAddress) // long indirect
			{
				location = state[location];
			}
			return location & 0xffff;
		}

	    protected int GetEffectiveAddressNoXr(ISystemState state)
	    {
			return GetEffectiveAddress(state, state.FormatLong ? 0 : state.Iar);
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

        protected int GetEffectiveAddress(ISystemState state)
        {
	        return GetEffectiveAddress(state, GetBase(state));
        }

		private int GetOffset(ISystemState state)
		{
			return state.FormatLong ? state.Displacement : +(sbyte) state.Displacement;
		}
        private int GetBase(ISystemState state)
        {
            return (!state.FormatLong || state.Tag != 0) ? state.Xr[state.Tag] : 0;
        }

	    public int GetShiftDistance(ISystemState state)
	    {
		    return ((state.Tag == 0) ? state.Displacement : state.Xr[state.Tag]) & 0x3f;
	    }
    }
}