namespace S1130.SystemObjects.Instructions
{
	public class LoadStatus : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.LoadStatus; } }
		public string OpName { get { return "LDS"; } }

		public override bool HasLongFormat { get { return false; } }

		public void Execute(ICpu cpu)
		{
			cpu.Carry = (cpu.Displacement & 0x02) != 0;
			cpu.Overflow = (cpu.Displacement & 0x01) != 0;
		}
		
		/// <summary>
		/// Disassembles LDS instruction. LDS uses displacement as immediate value (not address).
		/// Format: LDS /immediate_value (0-3 for carry/overflow combinations)
		/// The displacement field contains the literal bit pattern for carry (bit 1) and overflow (bit 0).
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			// LDS uses displacement directly as immediate value, not as address
			// Output in hex format with / prefix as expected by assembler
			byte value = (byte)(cpu.Displacement & 0x03);
			return $"LDS  /{value:X}";
		}
	}
}