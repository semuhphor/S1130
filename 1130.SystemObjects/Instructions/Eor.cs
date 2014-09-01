namespace S1130.SystemObjects.Instructions
{
	public class Eor : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Eor; }  }
		public string OpName { get { return "EOR";  } }

		public void Execute(ISystemState state)
		{	
			state.Acc ^= state[GetEffectiveAddress(state)];
		}
	}
}