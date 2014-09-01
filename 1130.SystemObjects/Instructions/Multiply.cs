namespace S1130.SystemObjects.Instructions
{
	public class Multiply : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Multiply; }  }
		public string OpName { get { return "M";  } }

		public void Execute(ISystemState state)
		{
			state.AccExt = SignExtend(state.Acc) * SignExtend(state[GetEffectiveAddress(state)]);
		}
	}
}