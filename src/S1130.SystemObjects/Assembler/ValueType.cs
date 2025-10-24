namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Defines the type and valid range for operand values in instruction patterns.
    /// Each value type has specific range constraints that are validated during assembly.
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// Displacement: Signed 8-bit offset relative to IAR (-128 to +127)
        /// Used in short format instructions
        /// </summary>
        Displacement,

        /// <summary>
        /// Core Storage Location: Absolute memory address (0 to 32767)
        /// Used in long and indirect format instructions
        /// </summary>
        CoreStorageLocation,

        /// <summary>
        /// Immediate Value: General numeric value (range context-dependent)
        /// Used in DC, EQU directives. Range: -32768 to +65535
        /// </summary>
        ImmediateValue,

        /// <summary>
        /// Shift Count: Number of bits to shift (0 to 63)
        /// Used in shift/rotate instructions (SLA, SRA, SLC, SRT, SLT, RTE, SLCA)
        /// </summary>
        ShiftCount,

        /// <summary>
        /// Modifier: Signed 8-bit increment/decrement value (-128 to +127)
        /// Used in MDX instruction second operand (stored in bits 8-15 of word 1)
        /// </summary>
        Modifier,

        /// <summary>
        /// Condition Codes: Combination of condition flags
        /// Used in BSC/BOSC instructions. Valid characters: +, -, Z, E, C, O
        /// </summary>
        Conditions
    }
}
