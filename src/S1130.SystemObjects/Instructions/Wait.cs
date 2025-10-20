namespace S1130.SystemObjects.Instructions
{
	public class Wait : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.Wait; } }
		public string OpName { get { return "WAIT"; } }

		public new bool HasLongFormat { get { return false; } }

		public void Execute(ICpu cpu)
		{
			cpu.Wait = true;
		}
		
		/// <summary>
		/// Disassembles WAIT instruction (no operands).
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			return "WAIT";
		}
	}
}