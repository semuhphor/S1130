namespace S1130.SystemObjects.Instructions
{
	public class Multiply : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Multiply; }  }
		public string OpName { get { return "M";  } }

		public void Execute(ICpu cpu)
		{
			cpu.AccExt = SignExtend(cpu.Acc) * SignExtend(cpu[GetEffectiveAddress(cpu)]);
			SetIarToNextInstruction(cpu);
		}
	}
}