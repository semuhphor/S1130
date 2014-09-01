namespace S1130.SystemObjects.Instructions
{
	public class Divide : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Divide; }  }
		public string OpName { get { return "D";  } }

		public void Execute(ISystemState state)
		{
			var divider = state[GetEffectiveAddress(state)];
			if (divider != 0)
			{
				var quotient = (int) state.AccExt / (int) SignExtend(divider);
				if (quotient > short.MaxValue)
				{
					state.Overflow = true;
					state.AccExt = (uint) quotient & 0xffffffff; /* Undefined... let's make AccExt the quotient */
				}
				else
				{
					state.Ext = (ushort) (state.AccExt % (short) divider);
					state.Acc = (ushort) (quotient & 0xffff);
				}
			}
			else
			{
				state.Overflow = true;
			}
		}
	}
}