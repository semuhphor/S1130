namespace S1130.SystemObjects.Instructions
{
	public abstract class ShiftInstructionBase : InstructionBase
	{
		protected struct ShiftInfo
		{
			public byte Type;
			public byte ShiftCount;
		}

		protected ShiftInfo ExtractShiftInfo(ISystemState state)
		{
			var info =  (byte) (((state.Tag == 0) ? state.Displacement : state[state.Tag]) & 0xff);
			return new ShiftInfo{ShiftCount = (byte) (info & 0x3f), Type = (byte) (info >> 6)};
		}
	}
}