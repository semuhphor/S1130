namespace S1130.SystemObjects.Instructions
{
	public class ExecuteInputOuput : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ExecuteInputOuput; } }
		public string OpName { get { return "XIO"; } }

		public void Execute(ICpu cpu)
		{
			var ioccAddr = (ushort) GetEffectiveAddress(cpu);
			new ConsoleEntrySwitches().ExecuteIocc(cpu, ioccAddr);
		}
	}
}