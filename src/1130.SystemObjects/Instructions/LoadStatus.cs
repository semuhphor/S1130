namespace S1130.SystemObjects.Instructions
{
	public class LoadStatus : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.LoadStatus; } }
		public string OpName { get { return "LDS"; } }

		public override bool HasLongFormat { get { return false; } }

		public void Execute(ICpu cpu)
		{
			cpu.Carry = (cpu.Displacement & 0x02) != 0;
			cpu.Overflow = (cpu.Displacement & 0x01) != 0;
		}
	}
}