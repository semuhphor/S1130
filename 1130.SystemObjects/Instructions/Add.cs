using System.Data;

namespace S1130.SystemObjects.Instructions
{
	public class Add : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Add; }  }
		public string OpName { get { return "A";  } }

		public void Execute(ISystemState state)
		{
			var result = (uint) state.Acc + state[GetEffectiveAddress(state)];
			state.Acc = (ushort) (result & 0xffff);
			state.Carry = (result & 0x10000) != 0;
			state.Overflow = (result & 0xffff0000) != 0;
		}
	}
}