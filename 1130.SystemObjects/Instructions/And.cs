namespace S1130.SystemObjects.Instructions
{
	public class And : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.And; }  }
		public string OpName { get { return "AND";  } }

		public void Execute(ICpu cpu)
		{	
			cpu.Acc &= cpu[GetEffectiveAddress(cpu)];
		}
	}
}