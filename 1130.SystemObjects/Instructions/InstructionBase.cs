namespace S1130.SystemObjects.Instructions
{
    public abstract class InstructionBase
    {
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
    }
}