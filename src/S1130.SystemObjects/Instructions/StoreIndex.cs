namespace S1130.SystemObjects.Instructions
{
	public class StoreIndex : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.StoreIndex; } }
		public string OpName { get { return "STX"; } }

		public void Execute(ICpu cpu)
		{
			// With no tag the "index register" is the IAR as the hardware holds it: already past this instruction.
			var value = cpu.Tag == 0 ? (ushort)(cpu.Iar + cpu.CurrentInstructionLength) : cpu.Xr[cpu.Tag];
			cpu[GetEffectiveAddressNoXr(cpu)] = value;
			SetIarToNextInstruction(cpu);
		}
	}
}