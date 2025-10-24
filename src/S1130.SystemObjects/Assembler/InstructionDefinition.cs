using System.Collections.Generic;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Defines an IBM 1130 instruction or directive, mapping its mnemonic to its opcode
    /// and the list of valid operand patterns it accepts.
    /// </summary>
    public class InstructionDefinition
    {
        /// <summary>
        /// The instruction mnemonic (e.g., "LD", "ADD", "BSC")
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// The 5-bit opcode value (bits 0-4 of instruction word)
        /// For directives (DC, EQU, BSS, BES), this is 0x00 as they don't generate opcodes
        /// </summary>
        public byte Opcode { get; set; }

        /// <summary>
        /// List of valid patterns this instruction can accept
        /// </summary>
        public List<InstructionPattern> ValidPatterns { get; set; } = new List<InstructionPattern>();

        /// <summary>
        /// True if this is a directive (DC, EQU, BSS, BES) rather than a machine instruction
        /// </summary>
        public bool IsDirective { get; set; }

        /// <summary>
        /// True if this is a pseudo-op (NOP, XCH) that maps to another instruction
        /// </summary>
        public bool IsPseudoOp { get; set; }

        /// <summary>
        /// For pseudo-ops, the actual opcode and operand to generate
        /// (e.g., NOP -> SLA 0, XCH -> RTE 16)
        /// </summary>
        public string PseudoOpMapping { get; set; }

        /// <summary>
        /// For shift instructions, the shift type to encode in bits 6-7 of the displacement byte.
        /// Values: 0=SLA/SRA, 1=SLCA, 2=SLT/SRT, 3=SLC/RTE
        /// </summary>
        public byte? ShiftType { get; set; }
    }
}
