using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace S1130.SystemObjects.Instructions
{
	public class ModifyIndex : BranchInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ModifyIndex; }  }
		public string OpName { get { return "MDX";  } }

		public void Execute(ISystemState state)
		{
			int oldValue= 0;												// old value
			int newValue = 0;												// .. and new value

			if (state.FormatLong)											// q. long format?
			{																// a. yes..
				var effectiveAddress = state.Displacement;					// .. Get effective Address
				if (state.Tag != 0)											// q. is target XR?
				{															// a. yes ... 
					newValue = oldValue = state.Xr[state.Tag];				// .. get old value
					newValue += ((state.IndirectAddress						// .. plus .. q. indirect?
							? state[effectiveAddress]						// .. .. .. ..a. yes .. word @address
							: effectiveAddress));							// .. .. .. .... or just address to the new value
					state.Xr[state.Tag] = (ushort) (newValue & Mask16);		// reset target
				}
				else														// otherwise..
				{															// .. long and no tag
					newValue = oldValue = state[effectiveAddress];			// get the old value from memory..
					newValue += (sbyte) (state.Modifiers & 0xff);			// .. increment by displacement
					state[effectiveAddress] = (ushort) (newValue & Mask16);	// reset target
				}
			}
			else															// otherwise
			{																// .. short format
				var displacement = (sbyte) (state.Displacement & 0xff);		// get amount of change
				newValue = oldValue = state.Xr[state.Tag];					// .. get old value
				newValue += displacement;									// .. increment by displacement
				state.Xr[state.Tag] = (ushort) (newValue & Mask16);			// reset target
			}
			var longOrHasTag = state.FormatLong || (state.Tag != 0);
			var newValueChange = ((newValue == 0) || ((newValue & 0x8000) != (oldValue & 0x8000)));
			if (longOrHasTag && newValueChange)								// q. long or tag & new value changed?
			{																// a. yes..
				state.Iar++;												// skip next instruction
			}
		}
	}
}