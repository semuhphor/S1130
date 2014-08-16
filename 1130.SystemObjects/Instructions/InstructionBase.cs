namespace S1130.SystemObjects.Instructions
{
    public abstract class InstructionBase
    {
        protected int GetEffectiveAddress(ISystemState state)
        {
            var location = GetBase(state) + state.Displacement;
            if (state.FormatLong && state.IndirectAddress) // long indirect
            {
                location = state[location];
            }
            return location;
        }

        private int GetBase(ISystemState state)
        {
            return (!state.FormatLong || state.Tag != 0) ? state.Xr[state.Tag] : 0;
        }
    }
}