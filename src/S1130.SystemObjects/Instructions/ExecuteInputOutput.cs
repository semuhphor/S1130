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
			if (cpu.IoccFunction == DevFunction.SenseInterrupte)				// q. sense interrupt?
			{																	// a. yes .. common to all devices, no device code needed
				cpu.Acc = cpu.SenseInterruptLevelStatusWord();					// .. ILSW of the level being serviced
			}
			else
			{
				cpu.IoccDevice?.ExecuteIocc();									// .. otherwise let the device do what it says
			}
			SetIarToNextInstruction(cpu);
		}
	}
}