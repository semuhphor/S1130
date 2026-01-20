namespace S1130.SystemObjects.Instructions
{
	public class ShiftRight : ShiftInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ShiftRight; } }
		public string OpName { get { return "SR"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ICpu cpu)
		{
			var shiftInfo = ExtractShiftInfo(cpu);
			if (shiftInfo.ShiftCount == 0) return;
			ulong work = cpu.AccExt;
			switch (shiftInfo.Type)
			{
				case 0: // SRA 
					cpu.Acc = (ushort)((work >> (16 + shiftInfo.ShiftCount)) & Mask16);
					break;
				case 1: // Invalid instruction
					break;
				case 2: // SRT
					work >>= shiftInfo.ShiftCount;
					cpu.AccExt = (uint)(work & Mask32);
					break;
				case 3: // RTE
					work <<= 32 - (shiftInfo.ShiftCount % 32);
					work |= (work >> 32);
					cpu.AccExt = (uint)(work & Mask32);
					break;
			}
			SetIarToNextInstruction(cpu);
		}
	}
}