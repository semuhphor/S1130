namespace S1130.SystemObjects.Instructions
{
	public class StoreIndex : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.StoreIndex; } }
		public string OpName { get { return "STX"; } }

		public void Execute(ICpu cpu)
		{
			cpu[GetEffectiveAddressNoXr(cpu)] = cpu.Xr[cpu.Tag];
			SetIarToNextInstruction(cpu);
		}
	}
}