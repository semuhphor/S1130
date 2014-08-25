using System.Collections.Generic;

namespace S1130.SystemObjects.Instructions
{
	public class AddDouble : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.AddDouble; }  }
		public string OpName { get { return "AD";  } }

		public void Execute(ISystemState state)
		{
			var effectiveAddress = GetEffectiveAddress(state);
			long valueToAdd = (state[effectiveAddress] << 16) & 0xffff0000;
			valueToAdd |= state[effectiveAddress | 1] & 0xffff;
			long accExt = state.AccExt;
			long result = accExt + valueToAdd;
			state.AccExt = (uint) (result & 0xffffffff);
			state.Carry = (result & 0x100000000) != 0;
			if (!state.Overflow)
			{
				state.Overflow = Is32BitSignBitOn((uint)((~valueToAdd ^ accExt) & (valueToAdd ^ state.AccExt)));
			}
		}
	}
}