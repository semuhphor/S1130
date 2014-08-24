using System.Data;

namespace S1130.SystemObjects.Instructions
{
	public class Add : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Add; }  }
		public string OpName { get { return "A";  } }

		public void Execute(ISystemState state)
		{	
			var valueToAdd = state[GetEffectiveAddress(state)];
			var oldAcc = state.Acc;
			var result = (uint) oldAcc + valueToAdd;
			state.Acc = (ushort) (result & 0xffff);
			state.Carry = (result & 0x10000) != 0;
			if (!state.Overflow)
			{
				state.Overflow = Is16BitSignBitOn((~valueToAdd ^ oldAcc) & (valueToAdd ^ state.Acc));
			}
		}
	}
}