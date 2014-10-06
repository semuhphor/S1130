namespace S1130.SystemObjects.Instructions
{
	public class Add : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Add; }  }
		public string OpName { get { return "A";  } }

		public void Execute(ICpu cpu)
		{	
			var valueToAdd = cpu[GetEffectiveAddress(cpu)];
			var oldAcc = cpu.Acc;
			var result = (uint) oldAcc + valueToAdd;
			cpu.Acc = (ushort) (result & 0xffff);
			cpu.Carry = (result & 0x10000) != 0;
			if (!cpu.Overflow)
			{
				cpu.Overflow = Is16BitSignBitOn((~valueToAdd ^ oldAcc) & (valueToAdd ^ cpu.Acc));
			}
		}
	}
}