namespace S1130.SystemObjects.Instructions
{
	public class BranchSkip : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchSkip; }  }
		public string OpName { get { return "BSC";  } }

		public void Execute(ICpu cpu)
		{
			if (cpu.FormatLong)											// q. is format long?
			{																// a. yes ..
				if (base.TestCondition(cpu))								// q. condition met?
				{															// a.yes
					return;													// .. take no action
				}
				cpu.Iar = (ushort) GetEffectiveAddress(cpu);			// branch to address
			}
			else															// otherwise..
			{																// .. short format
				if (base.TestCondition(cpu))								// q. condition met?
				{															// a. yes ..
					cpu.Iar++;											// .. then skip
				}
			}
		}
	}
}