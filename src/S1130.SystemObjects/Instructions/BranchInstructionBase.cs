namespace S1130.SystemObjects.Instructions
{
	public abstract class BranchInstructionBase : InstructionBase
	{
		public const byte OverflowOff = 0x01;		// .... ...1 Check overflow off
		public const byte CarryOff = 0x02;			// .... ..1. Check carry off
		public const byte Even = 0x04;				// .... .1.. Check Acc even
		public const byte Plus = 0x08;				// .... 1... Check Acc plus (gt zero)
		public const byte Minus = 0x10;				// ...1 .... Check Acc negative
		public const byte Zero = 0x20;				// ..1. .... Check Acc zero
	
		protected bool TestCondition(ICpu cpu)
		{
			bool result = false;						// Assume no conditions met

			if ((cpu.Modifiers & OverflowOff) != 0)		// q. checking overflow off cpu?
			{											// a. yes .. 
				if (!cpu.Overflow)						// q. overflow off?
				{										// a. yes...
					result = true;						// .. condition met
				}
				cpu.Overflow = false;					// since overflow is on... reset it after test
			}

			if ((cpu.Modifiers & CarryOff) != 0)		// q. check carry off?
			{											// a. yes ..
				if (!cpu.Carry)							// q. carry off?
				{										// a. yes..
					result = true;						// .. condition met
				}
			}

			if ((cpu.Modifiers & Even) != 0)			// q. Checking for even?
			{											// a. yes ..
				if ((cpu.Acc & 1) == 0)					// q. Acc even?
				{										// a. yes ..
					result = true;						// .. condition met
				}
			}

			var acc = (short)cpu.Acc;					// get sign
			if ((cpu.Modifiers & Plus) != 0)			// q. Checking for Positive?
			{											// a. yes ..
				if (acc > 0)							// q. Acc positive?
				{										// a. yes ..
					result = true;						// .. condition met
				}
			}

			if ((cpu.Modifiers & Minus) != 0)			// q. Checking for Negative?
			{											// a. yes ..
				if (acc < 0)							// q. Acc minus?
				{										// a. yes ..
					result = true;						// .. condition met
				}
			}

			if ((cpu.Modifiers & Zero) != 0)			// q. Checking for zero?
			{											// a. yes ..
				if (acc == 0)							// q. Acc zero?
				{										// a. yes ..
					result = true;						// .. condition met
				}
			}
			return result;								// return results
		}
	}
}