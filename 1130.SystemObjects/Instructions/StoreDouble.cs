namespace S1130.SystemObjects.Instructions
{
    public class StoreDouble : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get { return OpCodes.StoreDouble;  } }
        public string OpName { get { return "STD"; } }
        public void Execute(ISystemState state)
        {
            var effectiveAddress = GetEffectiveAddress(state);
            state[effectiveAddress | 1] = state.Ext;
            state[effectiveAddress] = state.Acc;
        }
    }
}