namespace S1130.SystemObjects.Instructions
{
    public class Store : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get {return OpCodes.Store; } }
        public string OpName { get { return "STO"; } }

        public void Execute(ISystemState state)
        {
            state[GetEffectiveAddress(state)] = state.Acc;

        }
    }
}