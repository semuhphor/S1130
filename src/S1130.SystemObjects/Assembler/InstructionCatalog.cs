using System.Collections.Generic;
using S1130.SystemObjects.Instructions;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Central catalog of all IBM 1130 instructions, directives, and pseudo-ops.
    /// Maps mnemonics to their opcodes and valid operand patterns.
    /// </summary>
    public static class InstructionCatalog
    {
        private static Dictionary<string, InstructionDefinition> _instructions;

        /// <summary>
        /// Gets the complete instruction catalog
        /// </summary>
        public static Dictionary<string, InstructionDefinition> Instructions
        {
            get
            {
                if (_instructions == null)
                    Initialize();
                return _instructions;
            }
        }

        private static void Initialize()
        {
            _instructions = new Dictionary<string, InstructionDefinition>();

            // === PSEUDO-OPS ===
            
            Add("NOP", new InstructionDefinition
            {
                Mnemonic = "NOP",
                Opcode = 0x10, // SLA opcode
                IsPseudoOp = true,
                PseudoOpMapping = "SLA 0", // NOP is SLA with shift count 0
                ValidPatterns = new List<InstructionPattern> { InstructionPattern.NoOperand }
            });

            Add("XCH", new InstructionDefinition
            {
                Mnemonic = "XCH",
                Opcode = 0x03, // RTE opcode
                IsPseudoOp = true,
                PseudoOpMapping = "RTE 16", // XCH is RTE 16 (swap ACC and EXT)
                ValidPatterns = new List<InstructionPattern> { InstructionPattern.NoOperand }
            });

            // === SPECIAL INSTRUCTIONS ===
            
            Add("WAIT", new InstructionDefinition
            {
                Mnemonic = "WAIT",
                Opcode = 0x06,
                ValidPatterns = new List<InstructionPattern> { InstructionPattern.NoOperand }
            });

            // === LOAD/STORE INSTRUCTIONS ===
            
            Add("LD", new InstructionDefinition
            {
                Mnemonic = "LD",
                Opcode = 0x18,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("LDD", new InstructionDefinition
            {
                Mnemonic = "LDD",
                Opcode = 0x0C,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("STO", new InstructionDefinition
            {
                Mnemonic = "STO",
                Opcode = 0x1A,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("STD", new InstructionDefinition
            {
                Mnemonic = "STD",
                Opcode = 0x0D,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === ARITHMETIC INSTRUCTIONS ===
            
            Add("ADD", new InstructionDefinition
            {
                Mnemonic = "ADD",
                Opcode = 0x10, // Note: Different from shift instructions due to format/tag differences
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("SUB", new InstructionDefinition
            {
                Mnemonic = "SUB",
                Opcode = 0x19,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("MPY", new InstructionDefinition
            {
                Mnemonic = "MPY",
                Opcode = 0x1C,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("DIV", new InstructionDefinition
            {
                Mnemonic = "DIV",
                Opcode = 0x1D,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // Double-precision arithmetic
            Add("AD", new InstructionDefinition
            {
                Mnemonic = "AD",
                Opcode = 0x08,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("SD", new InstructionDefinition
            {
                Mnemonic = "SD",
                Opcode = 0x09,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === LOGICAL INSTRUCTIONS ===
            
            Add("AND", new InstructionDefinition
            {
                Mnemonic = "AND",
                Opcode = 0x1E,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("OR", new InstructionDefinition
            {
                Mnemonic = "OR",
                Opcode = 0x1F,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("EOR", new InstructionDefinition
            {
                Mnemonic = "EOR",
                Opcode = 0x1B,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === SHIFT INSTRUCTIONS ===
            // Shift instructions use base opcode 0x02 (left) or 0x03 (right)
            // with shift type encoded in bits 6-7 of displacement byte:
            // Type 0: SLA/SRA, Type 1: SLCA, Type 2: SLT/SRT, Type 3: SLC/RTE
            
            Add("SLA", new InstructionDefinition
            {
                Mnemonic = "SLA",
                Opcode = 0x02, // ShiftLeft
                ShiftType = 0,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("SLCA", new InstructionDefinition
            {
                Mnemonic = "SLCA",
                Opcode = 0x02, // ShiftLeft
                ShiftType = 1,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("SLT", new InstructionDefinition
            {
                Mnemonic = "SLT",
                Opcode = 0x02, // ShiftLeft
                ShiftType = 2,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("SLC", new InstructionDefinition
            {
                Mnemonic = "SLC",
                Opcode = 0x02, // ShiftLeft
                ShiftType = 3,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("SRA", new InstructionDefinition
            {
                Mnemonic = "SRA",
                Opcode = 0x03, // ShiftRight
                ShiftType = 0,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("SRT", new InstructionDefinition
            {
                Mnemonic = "SRT",
                Opcode = 0x03, // ShiftRight
                ShiftType = 2,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            Add("RTE", new InstructionDefinition
            {
                Mnemonic = "RTE",
                Opcode = 0x03, // ShiftRight
                ShiftType = 3,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.Index1Zero,
                    InstructionPattern.Index2Zero,
                    InstructionPattern.Index3Zero
                }
            });

            // === BRANCH INSTRUCTIONS ===
            
            Add("BSC", new InstructionDefinition
            {
                Mnemonic = "BSC",
                Opcode = 0x04,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortConditionOnly,
                    InstructionPattern.ShortIndex1ConditionOnly,
                    InstructionPattern.ShortIndex2ConditionOnly,
                    InstructionPattern.ShortIndex3ConditionOnly,
                    InstructionPattern.LongWithConditions,
                    InstructionPattern.LongIndex1WithConditions,
                    InstructionPattern.LongIndex2WithConditions,
                    InstructionPattern.LongIndex3WithConditions,
                    InstructionPattern.IndirectWithConditions,
                    InstructionPattern.IndirectIndex1WithConditions,
                    InstructionPattern.IndirectIndex2WithConditions,
                    InstructionPattern.IndirectIndex3WithConditions
                }
            });

            Add("BOSC", new InstructionDefinition
            {
                Mnemonic = "BOSC",
                Opcode = 0x04, // Same as BSC, but with interrupt reset bit set
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortConditionOnly,
                    InstructionPattern.ShortIndex1ConditionOnly,
                    InstructionPattern.ShortIndex2ConditionOnly,
                    InstructionPattern.ShortIndex3ConditionOnly,
                    InstructionPattern.LongWithConditions,
                    InstructionPattern.LongIndex1WithConditions,
                    InstructionPattern.LongIndex2WithConditions,
                    InstructionPattern.LongIndex3WithConditions,
                    InstructionPattern.IndirectWithConditions,
                    InstructionPattern.IndirectIndex1WithConditions,
                    InstructionPattern.IndirectIndex2WithConditions,
                    InstructionPattern.IndirectIndex3WithConditions
                }
            });

            Add("BSI", new InstructionDefinition
            {
                Mnemonic = "BSI",
                Opcode = 0x05,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("MDX", new InstructionDefinition
            {
                Mnemonic = "MDX",
                Opcode = 0x07,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongTwoOperand,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === INDEX REGISTER INSTRUCTIONS ===
            
            Add("LDX", new InstructionDefinition
            {
                Mnemonic = "LDX",
                Opcode = 0x06,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3
                }
            });

            Add("STX", new InstructionDefinition
            {
                Mnemonic = "STX",
                Opcode = 0x02,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3
                }
            });

            // === STATUS INSTRUCTIONS ===
            
            Add("LDS", new InstructionDefinition
            {
                Mnemonic = "LDS",
                Opcode = 0x15,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            Add("STS", new InstructionDefinition
            {
                Mnemonic = "STS",
                Opcode = 0x16,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === I/O INSTRUCTION ===
            
            Add("XIO", new InstructionDefinition
            {
                Mnemonic = "XIO",
                Opcode = 0x01,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ShortNoIndex,
                    InstructionPattern.ShortIndex1,
                    InstructionPattern.ShortIndex2,
                    InstructionPattern.ShortIndex3,
                    InstructionPattern.LongNoIndex,
                    InstructionPattern.LongIndex1,
                    InstructionPattern.LongIndex2,
                    InstructionPattern.LongIndex3,
                    InstructionPattern.IndirectNoIndex,
                    InstructionPattern.IndirectIndex1,
                    InstructionPattern.IndirectIndex2,
                    InstructionPattern.IndirectIndex3
                }
            });

            // === DIRECTIVES ===
            
            Add("DC", new InstructionDefinition
            {
                Mnemonic = "DC",
                Opcode = 0x00,
                IsDirective = true,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue
                }
            });

            Add("EQU", new InstructionDefinition
            {
                Mnemonic = "EQU",
                Opcode = 0x00,
                IsDirective = true,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue
                }
            });

            Add("BSS", new InstructionDefinition
            {
                Mnemonic = "BSS",
                Opcode = 0x00,
                IsDirective = true,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.ImmediateValue,
                    InstructionPattern.ExtendedValue
                }
            });

            Add("BES", new InstructionDefinition
            {
                Mnemonic = "BES",
                Opcode = 0x00,
                IsDirective = true,
                ValidPatterns = new List<InstructionPattern>
                {
                    InstructionPattern.NoOperand
                }
            });
        }

        private static void Add(string mnemonic, InstructionDefinition definition)
        {
            _instructions[mnemonic.ToUpperInvariant()] = definition;
        }

        /// <summary>
        /// Looks up an instruction by mnemonic (case-insensitive)
        /// </summary>
        public static bool TryGetInstruction(string mnemonic, out InstructionDefinition instruction)
        {
            return Instructions.TryGetValue(mnemonic.ToUpperInvariant(), out instruction);
        }
    }
}
