namespace S1130.SystemObjects.Instructions
{
	public class Or : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Or; }  }
		public string OpName { get { return "OR";  } }

		public void Execute(ICpu cpu)
		{
			cpu.Acc |= cpu[GetEffectiveAddress(cpu)];
			SetIarToNextInstruction(cpu);
		}
	}
}