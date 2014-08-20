using System;

namespace S1130.SystemObjects.Instructions
{
	public class LoadStatus : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.LoadStatus; } }
		public string OpName { get { return "LDS"; } }

		public override bool HasLongFormat { get { return false; } }

		public void Execute(ISystemState state)
		{
			state.Carry = (state.Displacement & 0x02) != 0;
			state.Overflow = (state.Displacement & 0x01) != 0;
		}
	}
}