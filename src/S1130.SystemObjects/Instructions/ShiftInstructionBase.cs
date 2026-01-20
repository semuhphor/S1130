namespace S1130.SystemObjects.Instructions
{
	public abstract class ShiftInstructionBase : InstructionBase
	{
		protected struct ShiftInfo
		{
			public byte Type;
			public byte ShiftCount;
		}

		protected ShiftInfo ExtractShiftInfo(ICpu cpu)
		{
			var info =  (byte) (((cpu.Tag == 0) ? cpu.Displacement : cpu[cpu.Tag]) & 0xff);
			return new ShiftInfo{ShiftCount = (byte) (info & 0x3f), Type = (byte) (info >> 6)};
		}
		
		/// <summary>
		/// Disassembles shift instructions which have a special format.
		/// New syntax format: MNEMONIC shift_count  (e.g., "SLA 16", "SRT 5", "RTE 16")
		/// Or with index register: MNEMONIC |X| (e.g., "SLA |1|")
		/// Shift count can come from displacement or from index register.
		/// 
		/// Shift Left (OpCode 0x02):
		///   Type 0: SLA (Shift Left Accumulator)
		///   Type 1: SLCA (Shift Left and Count Accumulator)
		///   Type 2: SLT (Shift Left AccExt Together)
		///   Type 3: SLC (Shift Left and Count AccExt)
		/// 
		/// Shift Right (OpCode 0x03):
		///   Type 0: SRA (Shift Right Accumulator)
		///   Type 1: Invalid
		///   Type 2: SRT (Shift Right AccExt Together)
		///   Type 3: RTE (Rotate Ext)
		/// </summary>
		public override string Disassemble(ICpu cpu, ushort address)
		{
			var instruction = cpu.CurrentInstruction;
			if (instruction == null)
				return "; Unknown instruction";
			
			// Get shift count and type - either from displacement or from index register
			if (cpu.Tag == 0)
			{
				// Shift count and type in displacement
				byte info = (byte)(cpu.Displacement & 0xFF);
				byte shiftCount = (byte)(info & 0x3F);
				byte shiftType = (byte)(info >> 6);
				
				// Build mnemonic based on opcode and type
				string mnemonic;
				if (instruction.OpName == "SL")
				{
					// Shift Left
					switch (shiftType)
					{
						case 0: mnemonic = "SLA"; break;   // Shift Left Accumulator
						case 1: mnemonic = "SLCA"; break;  // Shift Left and Count Accumulator
						case 2: mnemonic = "SLT"; break;   // Shift Left AccExt Together
						case 3: mnemonic = "SLC"; break;   // Shift Left and Count AccExt
						default: mnemonic = $"; Unknown SL type {shiftType}"; break;
					}
				}
				else // "SR"
				{
					// Shift Right
					switch (shiftType)
					{
						case 0: mnemonic = "SRA"; break;  // Shift Right Accumulator
						case 1: return $"; Invalid SR type 1 at address {address:X4}";  // Invalid
						case 2: mnemonic = "SRT"; break;  // Shift Right AccExt Together
						case 3: mnemonic = "RTE"; break;  // Rotate Ext
						default: mnemonic = $"; Unknown SR type {shiftType}"; break;
					}
				}
				
				return $"{mnemonic} {shiftCount}";
			}
			else
			{
				// Shift count in index register
				// The shift type bits are in the index register value at runtime,
				// but we can't know them at disassembly time.
				// We'll default to the basic type (SLA/SRA) since we can't determine
				// the actual type without runtime register values.
				string mnemonic = instruction.OpName == "SL" ? "SLA" : "SRA";
				return $"{mnemonic} |{cpu.Tag}|";
			}
		}
	}
}