namespace S1130.SystemObjects.Instructions
{
	public class ExclusiveOr : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ExclusiveOr; }  }
		public string OpName { get { return "EOR";  } }

		public void Execute(ICpu cpu)
		{	
			cpu.Acc ^= cpu[GetEffectiveAddress(cpu)];
		}
	}
}