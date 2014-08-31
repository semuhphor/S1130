namespace S1130.SystemObjects.Instructions
{
	public class Subtract : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Subtract; }  }
		public string OpName { get { return "S";  } }

		public void Execute(ISystemState state)
		{	
			var valueToSubtract = state[GetEffectiveAddress(state)];
			var oldAcc = state.Acc;
			var result = (uint) oldAcc - valueToSubtract;
			state.Acc = (ushort) (result & 0xffff);
			state.Carry = (result & 0x10000) != 0;
			if (!state.Overflow)
			{
				state.Overflow = Is16BitSignBitOn((oldAcc ^ valueToSubtract) & (oldAcc ^ state.Acc));
			}
		}
	}
}