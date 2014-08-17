namespace S1130.SystemObjects.Instructions
{
	public class StoreIndex : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.StoreIndex; } }
		public string OpName { get { return "STX"; } }

		public void Execute(ISystemState state)
		{
			state[GetEffectiveAddressNoXr(state)] = state.Xr[state.Tag];
		}
	}
}