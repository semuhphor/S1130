namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Defines all possible instruction operand patterns for the IBM 1130 assembler.
    /// Each pattern represents a specific combination of format modifiers, index registers,
    /// and operand structures that an instruction can accept.
    /// </summary>
    public enum InstructionPattern
    {
        /// <summary>
        /// No operands required (e.g., WAIT, NOP, XCH, BES)
        /// Format: OPCODE
        /// </summary>
        NoOperand,

        /// <summary>
        /// Single immediate numeric value (e.g., SLA 5, DC 100, EQU /1000, BSS 10)
        /// Format: OPCODE VALUE
        /// </summary>
        ImmediateValue,

        /// <summary>
        /// Shift instruction with index register 1, operand must be 0 or blank
        /// Format: OPCODE |1| or OPCODE |1|0
        /// </summary>
        Index1Zero,

        /// <summary>
        /// Shift instruction with index register 2, operand must be 0 or blank
        /// Format: OPCODE |2| or OPCODE |2|0
        /// </summary>
        Index2Zero,

        /// <summary>
        /// Shift instruction with index register 3, operand must be 0 or blank
        /// Format: OPCODE |3| or OPCODE |3|0
        /// </summary>
        Index3Zero,

        /// <summary>
        /// Short format, no index register, relative displacement from IAR
        /// Format: OPCODE DISP
        /// </summary>
        ShortNoIndex,

        /// <summary>
        /// Short format with index register 1
        /// Format: OPCODE |1|DISP
        /// </summary>
        ShortIndex1,

        /// <summary>
        /// Short format with index register 2
        /// Format: OPCODE |2|DISP
        /// </summary>
        ShortIndex2,

        /// <summary>
        /// Short format with index register 3
        /// Format: OPCODE |3|DISP
        /// </summary>
        ShortIndex3,

        /// <summary>
        /// Long format, no index register, absolute core storage location
        /// Format: OPCODE |L|CSL
        /// </summary>
        LongNoIndex,

        /// <summary>
        /// Long format with index register 1
        /// Format: OPCODE |L1|CSL
        /// </summary>
        LongIndex1,

        /// <summary>
        /// Long format with index register 2
        /// Format: OPCODE |L2|CSL
        /// </summary>
        LongIndex2,

        /// <summary>
        /// Long format with index register 3
        /// Format: OPCODE |L3|CSL
        /// </summary>
        LongIndex3,

        /// <summary>
        /// Indirect addressing, no index register
        /// Format: OPCODE |I|CSL
        /// </summary>
        IndirectNoIndex,

        /// <summary>
        /// Indirect addressing with index register 1
        /// Format: OPCODE |I1|CSL
        /// </summary>
        IndirectIndex1,

        /// <summary>
        /// Indirect addressing with index register 2
        /// Format: OPCODE |I2|CSL
        /// </summary>
        IndirectIndex2,

        /// <summary>
        /// Indirect addressing with index register 3
        /// Format: OPCODE |I3|CSL
        /// </summary>
        IndirectIndex3,

        /// <summary>
        /// Short format with condition codes only (no address - causes skip)
        /// Format: OPCODE COND
        /// </summary>
        ShortConditionOnly,

        /// <summary>
        /// Short format with index register 1 and condition codes
        /// Format: OPCODE |1|COND
        /// </summary>
        ShortIndex1ConditionOnly,

        /// <summary>
        /// Short format with index register 2 and condition codes
        /// Format: OPCODE |2|COND
        /// </summary>
        ShortIndex2ConditionOnly,

        /// <summary>
        /// Short format with index register 3 and condition codes
        /// Format: OPCODE |3|COND
        /// </summary>
        ShortIndex3ConditionOnly,

        /// <summary>
        /// Long format with address and condition codes
        /// Format: OPCODE |L|CSL,COND
        /// </summary>
        LongWithConditions,

        /// <summary>
        /// Long format with index register 1 and condition codes
        /// Format: OPCODE |L1|CSL,COND
        /// </summary>
        LongIndex1WithConditions,

        /// <summary>
        /// Long format with index register 2 and condition codes
        /// Format: OPCODE |L2|CSL,COND
        /// </summary>
        LongIndex2WithConditions,

        /// <summary>
        /// Long format with index register 3 and condition codes
        /// Format: OPCODE |L3|CSL,COND
        /// </summary>
        LongIndex3WithConditions,

        /// <summary>
        /// Indirect addressing with address and condition codes
        /// Format: OPCODE |I|CSL,COND
        /// </summary>
        IndirectWithConditions,

        /// <summary>
        /// Indirect addressing with index register 1 and condition codes
        /// Format: OPCODE |I1|CSL,COND
        /// </summary>
        IndirectIndex1WithConditions,

        /// <summary>
        /// Indirect addressing with index register 2 and condition codes
        /// Format: OPCODE |I2|CSL,COND
        /// </summary>
        IndirectIndex2WithConditions,

        /// <summary>
        /// Indirect addressing with index register 3 and condition codes
        /// Format: OPCODE |I3|CSL,COND
        /// </summary>
        IndirectIndex3WithConditions,

        /// <summary>
        /// Long format with two operands (address, modifier) - used by MDX
        /// Format: OPCODE |L|CSL,VALUE
        /// </summary>
        LongTwoOperand,

        /// <summary>
        /// Extended mode with value - used by BSS
        /// Format: OPCODE |E|VALUE
        /// </summary>
        ExtendedValue
    }
}
