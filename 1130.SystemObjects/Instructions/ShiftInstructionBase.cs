namespace S1130.SystemObjects.Instructions
{
	public abstract class ShiftInstructionBase : InstructionBase
	{
		protected struct ShiftInfo
		{
			public byte Type;
			public byte ShiftCount;
		}

		protected ShiftInfo ExtractShiftInfo(ICpu cpu)
		{
			var info =  (byte) (((cpu.Tag == 0) ? cpu.Displacement : cpu[cpu.Tag]) & 0xff);
			return new ShiftInfo{ShiftCount = (byte) (info & 0x3f), Type = (byte) (info >> 6)};
		}
	}
}