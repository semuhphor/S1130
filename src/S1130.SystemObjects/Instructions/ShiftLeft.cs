namespace S1130.SystemObjects.Instructions
{
	public class ShiftLeft : ShiftInstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ShiftLeft; } }
		public string OpName { get { return "SL"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ICpu cpu)
		{
			var shiftInfo = ExtractShiftInfo(cpu);
			if (shiftInfo.ShiftCount != 0)
			{
				if (cpu.Tag == 0)
				{
					shiftInfo.Type &= 0x02; // Can't count if no tag specified
				}
				ulong work = cpu.AccExt;
				switch (shiftInfo.Type)
				{
					case 0: // SLA 
						work >>= 16;
						work &= Mask16;
						work <<= shiftInfo.ShiftCount;
						cpu.Acc = (ushort)(work & Mask16);
						cpu.Carry = (work & 0x10000) != 0;
						break;
					case 1: // SLCA
						work >>= 16;
						work &= Mask16;
						cpu.Carry = false;
						while (shiftInfo.ShiftCount > 0 && (work & 0x8000) == 0)
						{
							work <<= 1;
							shiftInfo.ShiftCount--;
						}
						cpu.Acc = (ushort)(work & Mask16);
						cpu.Carry = (shiftInfo.ShiftCount != 0);
						cpu.Xr[cpu.Tag] = shiftInfo.ShiftCount;
						break;
					case 2: // SLT
						work <<= shiftInfo.ShiftCount;
						cpu.AccExt = (uint)(work & Mask32);
						cpu.Carry = (work & 0x100000000) != 0;
						break;
					case 3: // SLC
						cpu.Carry = false;
						while (shiftInfo.ShiftCount > 0 && (work & 0x80000000) == 0)
						{
							work <<= 1;
							shiftInfo.ShiftCount--;
						}
						cpu.AccExt = (uint)(work & Mask32);
						cpu.Carry = (shiftInfo.ShiftCount != 0);
						cpu.Xr[cpu.Tag] = shiftInfo.ShiftCount;
						break;
				}
			}
			SetIarToNextInstruction(cpu);
		}
	}
}