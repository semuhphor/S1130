namespace S1130.SystemObjects.Instructions
{
	public class SubtractDouble : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.SubtractDouble; } }
		public string OpName { get { return "SD"; } }

		public void Execute(ISystemState state)
		{
			var effectiveAddress = GetEffectiveAddress(state);
			long valueToSubtract = (state[effectiveAddress] << 16) & 0xffff0000;
			valueToSubtract |= state[effectiveAddress | 1] & 0xffff;
			long accExt = state.AccExt;
			long result = accExt - valueToSubtract;
			state.AccExt = (uint)(result & 0xffffffff);
			state.Carry = (result & 0x100000000) != 0;
			if (!state.Overflow)
			{
				state.Overflow = Is32BitSignBitOn((uint)((accExt ^ valueToSubtract) & (accExt ^ state.AccExt)));
			}
		}
	}
}