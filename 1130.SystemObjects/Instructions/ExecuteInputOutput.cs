namespace S1130.SystemObjects.Instructions
{
	public class ExecuteInputOuput : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ExecuteInputOuput; } }
		public string OpName { get { return "XIO"; } }

		public void Execute(ISystemState state)
		{
			var ioccAddr = (ushort) GetEffectiveAddress(state);
			new ConsoleEntrySwitches().ExecuteIocc(state, ioccAddr);
		}
	}
}