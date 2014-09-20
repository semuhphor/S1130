namespace S1130.SystemObjects.Instructions
{
	public class ShiftRight : ShiftInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ShiftLeft; } }
		public string OpName { get { return "SL"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ISystemState state)
		{
			var shiftInfo = ExtractShiftInfo(state);
			if (shiftInfo.ShiftCount == 0) return;
			ulong work = state.AccExt;
			switch (shiftInfo.Type)
			{
				case 0: // SRA 
					state.Acc = (ushort)((work >> (16 + shiftInfo.ShiftCount)) & Mask16);
					break;
				case 1: // Invalid instruction
					break;
				case 2: // SRT
					work >>= shiftInfo.ShiftCount;
					state.AccExt = (uint)(work & Mask32);
					break;
				case 3: // RTE
					work <<= 32 - (shiftInfo.ShiftCount % 32);
					work |= (work >> 32);
					state.AccExt = (uint)(work & Mask32);
					break;
			}
		}
	}
}