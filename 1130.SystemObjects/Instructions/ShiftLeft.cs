namespace S1130.SystemObjects.Instructions
{
	public class ShiftLeft : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ShiftLeft; } }
		public string OpName { get { return "SL"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ISystemState state)
		{
			var type = (state.Displacement & 0xc0) >> 6;
			var distance = GetShiftDistance(state);
			ulong work = state.AccExt;

			switch (type)
			{
				case 0: // SLA 
					const uint  mask = 0xffff;
					work >>= 16;
					work &= mask;
					work <<= distance;
					state.Acc = (ushort) (work & mask);
					state.Carry = (work & 0x10000) != 0;
					break;
			}
		}
	}
}