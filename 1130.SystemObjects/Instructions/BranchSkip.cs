namespace S1130.SystemObjects.Instructions
{
	public class BranchSkip : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.BranchSkip; }  }
		public string OpName { get { return "BSC";  } }

		public void Execute(ICpu cpu)
		{
			var resetInterrupt = (cpu.Modifiers & 0x40) != 0;			// assume reset interrupts
			var condition = TestCondition(cpu);							// .. check conditions
			if (cpu.FormatLong)											// q. is format long?
			{															// a. yes ..
				condition = condition == false;							// .. invert condtion
				if (condition)											// q. condition met?
				{														// a.yes
					cpu.Iar = (ushort) GetEffectiveAddress(cpu);		// branch to address
				}

			}
			else														// otherwise..
			{															// .. short format
				if (condition)											// q. condition met?
				{														// a. yes ..
					cpu.Iar++;											// .. then skip
				}
			}
			if (resetInterrupt && condition)							// reset requested and condtion met?
			{															// a. yes .. 
				cpu.ClearCurrentInterrupt();							// .. clear the current interrupt
			}
		}
	}
}