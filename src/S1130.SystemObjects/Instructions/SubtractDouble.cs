namespace S1130.SystemObjects.Instructions
{
	public class SubtractDouble : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.SubtractDouble; } }
		public string OpName { get { return "SD"; } }

		public void Execute(ICpu cpu)
		{
			var effectiveAddress = GetEffectiveAddress(cpu);
			long valueToSubtract = (cpu[effectiveAddress] << 16) & 0xffff0000;
			valueToSubtract |= (ushort) (cpu[effectiveAddress | 1] &  0xffff);
			long accExt = cpu.AccExt;
			long result = accExt - valueToSubtract;
			cpu.AccExt = (uint)(result & 0xffffffff);
			cpu.Carry = (result & 0x100000000) != 0;
			if (!cpu.Overflow)
			{
				cpu.Overflow = Is32BitSignBitOn((uint)((accExt ^ valueToSubtract) & (accExt ^ cpu.AccExt)));
			}
		}
	}
}