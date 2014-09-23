namespace S1130.SystemObjects.Instructions
{
	public class BranchStore : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchSkip; }  }
		public string OpName { get { return "BSI";  } }

		public void Execute(ISystemState state)
		{
			if (state.FormatLong && TestCondition(state))				// q. Long format & condition met?
			{															// a. yes
				return;													// .. take no action
			}
			var subroutineAddress = (ushort)GetEffectiveAddress(state);	// resolve routine address
			state[subroutineAddress++] = state.Iar;						// save return address and get entry address
			state.Iar = subroutineAddress;								// .. and branch to routine
		}
	}
}