using System.Data.Common;

namespace S1130.SystemObjects.Instructions
{
	public class BranchSkip : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchSkip; }  }
		public string OpName { get { return "BSC";  } }

		public void Execute(ISystemState state)
		{
			if (state.FormatLong)											// q. is format long?
			{																// a. yes ..
				if (base.TestCondition(state))								// q. condition met?
				{															// a.yes
					return;													// .. take no action
				}
				state.Iar = (ushort) GetEffectiveAddress(state);			// branch to address
			}
			else															// otherwise..
			{																// .. short format
				if (base.TestCondition(state))								// q. condition met?
				{															// a. yes ..
					state.Iar++;											// .. then skip
				}
			}
		}
	}
}