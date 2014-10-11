namespace S1130.SystemObjects.Instructions
{
	public class BranchStore : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchStore; }  }
		public string OpName { get { return "BSI";  } }

		public void Execute(ICpu cpu)
		{
			if (cpu.FormatLong && TestCondition(cpu))				// q. Long format & condition met?
			{															// a. yes
				return;													// .. take no action
			}
			var subroutineAddress = (ushort)GetEffectiveAddress(cpu);	// resolve routine address
			cpu[subroutineAddress++] = cpu.Iar;						// save return address and get entry address
			cpu.Iar = subroutineAddress;								// .. and branch to routine
		}
	}
}