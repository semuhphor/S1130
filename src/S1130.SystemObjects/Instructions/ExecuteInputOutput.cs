namespace S1130.SystemObjects.Instructions
{
	public class ExecuteInputOutput : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.ExecuteInputOutput; } }
		public string OpName { get { return "XIO"; } }

		public void Execute(ICpu cpu)										// execute XIO
		{
			var address = GetEffectiveAddress(cpu);
			cpu.IoccDecode(address);											// Decode the IOCC
			if (cpu.IoccDevice != null)											// q. device found?
			{																	// a. yes ..
				cpu.IoccDevice.ExecuteIocc();									// .. do what it says
			}
		}
	}
}