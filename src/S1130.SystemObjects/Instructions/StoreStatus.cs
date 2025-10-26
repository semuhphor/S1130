namespace S1130.SystemObjects.Instructions
{
	public class StoreStatus : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get {return OpCodes.StoreStatus; } }
		public string OpName { get { return "STS"; } }

		public void Execute(ICpu cpu)
		{
			cpu[GetEffectiveAddress(cpu)] &= 0xFF03;
			cpu[GetEffectiveAddress(cpu)] |= (ushort) ((cpu.Carry ? 0x02 : 0x00) | (cpu.Overflow ? 0x01 : 0x00));
			cpu.Carry = false;
			cpu.Overflow = false;
			SetIarToNextInstruction(cpu);
		}
	}
}