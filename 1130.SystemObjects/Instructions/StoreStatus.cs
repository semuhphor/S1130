namespace S1130.SystemObjects.Instructions
{
	public class StoreStatus : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get {return OpCodes.StoreStatus; } }
		public string OpName { get { return "STS"; } }

		public void Execute(ISystemState state)
		{
			state[GetEffectiveAddress(state)] &= 0xFF03;
			state[GetEffectiveAddress(state)] |= (ushort) ((state.Carry ? 0x02 : 0x00) | (state.Overflow ? 0x01 : 0x00));
			state.Carry = false;
			state.Overflow = false;
		}
	}
}