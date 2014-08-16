namespace S1130.SystemObjects.Instructions
{
    public class LoadDouble : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get { return OpCodes.LoadDouble; } }
        public string OpName { get { return "LDD"; } }

        public void Execute(ISystemState state)
        {
            var effectiveAddress = GetEffectiveAddress(state);
            state.Acc = state[effectiveAddress];
            state.Ext = state[effectiveAddress | 1];
        }
    }
}