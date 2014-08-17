using System;

namespace S1130.SystemObjects.Instructions
{
	public class LoadIndex : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.LoadIndex; }  }
		public string OpName { get { return "LDX";  } }

		public void Execute(ISystemState state)
		{
			state.Xr[state.Tag] = state.FormatLong ? (state.IndirectAddress ? state[state.Displacement] :  state.Displacement) : Convert.ToUInt16((sbyte) state.Displacement & 0xffff);
		}
	}
}