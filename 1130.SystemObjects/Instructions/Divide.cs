namespace S1130.SystemObjects.Instructions
{
	public class Divide : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Divide; }  }
		public string OpName { get { return "D";  } }

		public void Execute(ICpu cpu)
		{
			var divider = cpu[GetEffectiveAddress(cpu)];
			if (divider != 0)
			{
				var quotient = (int) cpu.AccExt / (int) SignExtend(divider);
				if (quotient > short.MaxValue)
				{
					cpu.Overflow = true;
					cpu.AccExt = (uint) quotient & 0xffffffff; /* Undefined... let's make AccExt the quotient */
				}
				else
				{
					cpu.Ext = (ushort) (cpu.AccExt % (short) divider);
					cpu.Acc = (ushort) (quotient & 0xffff);
				}
			}
			else
			{
				cpu.Overflow = true;
			}
		}
	}
}