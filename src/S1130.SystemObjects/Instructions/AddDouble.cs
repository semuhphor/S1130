namespace S1130.SystemObjects.Instructions
{
	public class AddDouble : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.AddDouble; }  }
		public string OpName { get { return "AD";  } }

		public void Execute(ICpu cpu)
		{
			var effectiveAddress = GetEffectiveAddress(cpu);
			long valueToAdd = (cpu[effectiveAddress] << 16) & 0xffff0000;
			valueToAdd |= (ushort) (cpu[effectiveAddress | 1] & 0xffff);
			long accExt = cpu.AccExt;
			long result = accExt + valueToAdd;
			cpu.AccExt = (uint) (result & 0xffffffff);
			cpu.Carry = (result & 0x100000000) != 0;
			if (!cpu.Overflow)
			{
				cpu.Overflow = Is32BitSignBitOn((uint)((~valueToAdd ^ accExt) & (valueToAdd ^ cpu.AccExt)));
			}
			SetIarToNextInstruction(cpu);
		}
	}
}