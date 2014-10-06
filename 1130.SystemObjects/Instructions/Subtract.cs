namespace S1130.SystemObjects.Instructions
{
	public class Subtract : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Subtract; }  }
		public string OpName { get { return "S";  } }

		public void Execute(ICpu cpu)
		{	
			var valueToSubtract = cpu[GetEffectiveAddress(cpu)];
			var oldAcc = cpu.Acc;
			var result = (uint) oldAcc - valueToSubtract;
			cpu.Acc = (ushort) (result & 0xffff);
			cpu.Carry = (result & 0x10000) != 0;
			if (!cpu.Overflow)
			{
				cpu.Overflow = Is16BitSignBitOn((oldAcc ^ valueToSubtract) & (oldAcc ^ cpu.Acc));
			}
		}
	}
}