namespace S1130.SystemObjects.Instructions
{
	public class And : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.And; }  }
		public string OpName { get { return "AND";  } }

		public void Execute(ISystemState state)
		{	
			state.Acc &= state[GetEffectiveAddress(state)];
		}
	}
}