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
			if (distance == 0) return;
			ulong work = state.AccExt;
			const uint mask16 = 0xffff;
			const uint mask32 = 0xffffffff;
			switch (type)
			{
				case 0: // SLA 
					work >>= 16;
					work &= mask16;
					work <<= distance;
					state.Acc = (ushort) (work & mask16);
					state.Carry = (work & 0x10000) != 0;
					break;
				case 2: // SLT
					work <<= distance;
					state.AccExt = (uint) (work & mask32);
					state.Carry = (work & 0x100000000) != 0;
					break;
			}
		}
	}
}