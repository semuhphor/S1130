namespace S1130.SystemObjects.Instructions
{
    public class Load : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get { return OpCodes.Load; }  }
        public string OpName { get { return "LD";  } }

        public void Execute(ISystemState state)
        {
            state.Acc = state[GetEffectiveAddress(state)];
        }
    }
}