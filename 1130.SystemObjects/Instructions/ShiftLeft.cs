namespace S1130.SystemObjects.Instructions
{
	public class ShiftLeft : ShiftInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ShiftLeft; } }
		public string OpName { get { return "SL"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ISystemState state)
		{
			var shiftInfo = ExtractShiftInfo(state); 
			if (shiftInfo.ShiftCount == 0) return;
			if (state.Tag == 0)
			{
				shiftInfo.Type &= 0x02; // Can't count if no tag specified
			}
			ulong work = state.AccExt;
			switch (shiftInfo.Type)
			{
				case 0: // SLA 
					work >>= 16;
					work &= Mask16;
					work <<= shiftInfo.ShiftCount;
					state.Acc = (ushort) (work & Mask16);
					state.Carry = (work & 0x10000) != 0;
					break;
				case 1: // SLCA
					work >>= 16;
					work &= Mask16;
					state.Carry = false;
					while (shiftInfo.ShiftCount > 0 && (work & 0x8000) == 0)
					{
						work <<= 1;
						shiftInfo.ShiftCount--;
					}
					state.Acc = (ushort) (work & Mask16);
					state.Carry = (shiftInfo.ShiftCount != 0);
					state.Xr[state.Tag] = shiftInfo.ShiftCount;
					break;
				case 2: // SLT
					work <<= shiftInfo.ShiftCount;
					state.AccExt = (uint) (work & Mask32);
					state.Carry = (work & 0x100000000) != 0;
					break;
				case 3:
					state.Carry = false;
					while (shiftInfo.ShiftCount > 0 && (work & 0x80000000) == 0)
					{
						work <<= 1;
						shiftInfo.ShiftCount--;
					}
					state.AccExt = (uint)(work & Mask32);
					state.Carry = (shiftInfo.ShiftCount != 0);
					state.Xr[state.Tag] = shiftInfo.ShiftCount;
					break;
			}
		}
	}
}