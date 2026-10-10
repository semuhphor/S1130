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
			if (shiftInfo.ShiftCount != 0)
			{
				int count = shiftInfo.ShiftCount;
				switch (shiftInfo.Type)
				{
					case 0: // SRA - logical shift of Acc only; counts >= 16 clear Acc
						cpu.Acc = count >= 16 ? (ushort)0 : (ushort)(cpu.Acc >> count);
						break;
					case 1: // Invalid instruction
						break;
					case 2: // SRT - arithmetic shift of Acc:Ext; sign bit propagates
						cpu.AccExt = count >= 32
							? (uint)((int)cpu.AccExt >> 31)
							: (uint)((int)cpu.AccExt >> count);
						break;
					case 3: // RTE - rotate Acc:Ext right
						int rotate = count % 32;
						if (rotate != 0)
						{
							cpu.AccExt = (cpu.AccExt >> rotate) | (cpu.AccExt << (32 - rotate));
						}
						break;
				}
			}
			SetIarToNextInstruction(cpu);
		}
	}
}