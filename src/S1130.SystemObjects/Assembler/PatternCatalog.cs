using System.Collections.Generic;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Central catalog of all pattern definitions with their validation rules.
    /// Each pattern knows its structure, valid modifiers, operand count, and value types.
    /// </summary>
    public static class PatternCatalog
    {
        private static Dictionary<InstructionPattern, PatternDefinition> _patterns;

        public static Dictionary<InstructionPattern, PatternDefinition> Patterns
        {
            get
            {
                if (_patterns == null)
                    Initialize();
                return _patterns;
            }
        }

        public static PatternDefinition GetPattern(InstructionPattern pattern)
        {
            return Patterns[pattern];
        }

        private static void Initialize()
        {
            _patterns = new Dictionary<InstructionPattern, PatternDefinition>();

            // === NO OPERAND ===
            
            Add(InstructionPattern.NoOperand, new PatternDefinition
            {
                Pattern = InstructionPattern.NoOperand,
                Description = "No operands",
                RequiresModifier = false,
                ValidModifiers = new string[0],
                OperandCount = 0
            });

            // === IMMEDIATE VALUE ===
            
            Add(InstructionPattern.ImmediateValue, new PatternDefinition
            {
                Pattern = InstructionPattern.ImmediateValue,
                Description = "Immediate value",
                RequiresModifier = false,
                ValidModifiers = new string[0],
                OperandCount = 1,
                Operand1Type = ValueType.ImmediateValue
            });

            // === SHIFT WITH INDEX REGISTER (OPERAND MUST BE 0) ===
            
            Add(InstructionPattern.Index1Zero, new PatternDefinition
            {
                Pattern = InstructionPattern.Index1Zero,
                Description = "Index register 1, operand 0 or blank",
                RequiresModifier = true,
                ValidModifiers = new[] { "1" },
                OperandCount = 0 // Operand must be 0 or blank, treated as no operand
            });

            Add(InstructionPattern.Index2Zero, new PatternDefinition
            {
                Pattern = InstructionPattern.Index2Zero,
                Description = "Index register 2, operand 0 or blank",
                RequiresModifier = true,
                ValidModifiers = new[] { "2" },
                OperandCount = 0
            });

            Add(InstructionPattern.Index3Zero, new PatternDefinition
            {
                Pattern = InstructionPattern.Index3Zero,
                Description = "Index register 3, operand 0 or blank",
                RequiresModifier = true,
                ValidModifiers = new[] { "3" },
                OperandCount = 0
            });

            // === SHORT FORMAT ===
            
            Add(InstructionPattern.ShortNoIndex, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortNoIndex,
                Description = "Short format, no index",
                RequiresModifier = false,
                ValidModifiers = new string[0],
                OperandCount = 1,
                Operand1Type = ValueType.Displacement
            });

            Add(InstructionPattern.ShortIndex1, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex1,
                Description = "Short format with index register 1",
                RequiresModifier = true,
                ValidModifiers = new[] { "1" },
                OperandCount = 1,
                Operand1Type = ValueType.Displacement
            });

            Add(InstructionPattern.ShortIndex2, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex2,
                Description = "Short format with index register 2",
                RequiresModifier = true,
                ValidModifiers = new[] { "2" },
                OperandCount = 1,
                Operand1Type = ValueType.Displacement
            });

            Add(InstructionPattern.ShortIndex3, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex3,
                Description = "Short format with index register 3",
                RequiresModifier = true,
                ValidModifiers = new[] { "3" },
                OperandCount = 1,
                Operand1Type = ValueType.Displacement
            });

            // === LONG FORMAT ===
            
            Add(InstructionPattern.LongNoIndex, new PatternDefinition
            {
                Pattern = InstructionPattern.LongNoIndex,
                Description = "Long format, no index",
                RequiresModifier = true,
                ValidModifiers = new[] { "L" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.LongIndex1, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex1,
                Description = "Long format with index register 1",
                RequiresModifier = true,
                ValidModifiers = new[] { "L1" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.LongIndex2, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex2,
                Description = "Long format with index register 2",
                RequiresModifier = true,
                ValidModifiers = new[] { "L2" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.LongIndex3, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex3,
                Description = "Long format with index register 3",
                RequiresModifier = true,
                ValidModifiers = new[] { "L3" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            // === INDIRECT FORMAT ===
            
            Add(InstructionPattern.IndirectNoIndex, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectNoIndex,
                Description = "Indirect, no index",
                RequiresModifier = true,
                ValidModifiers = new[] { "I" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.IndirectIndex1, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex1,
                Description = "Indirect with index register 1",
                RequiresModifier = true,
                ValidModifiers = new[] { "I1" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.IndirectIndex2, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex2,
                Description = "Indirect with index register 2",
                RequiresModifier = true,
                ValidModifiers = new[] { "I2" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            Add(InstructionPattern.IndirectIndex3, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex3,
                Description = "Indirect with index register 3",
                RequiresModifier = true,
                ValidModifiers = new[] { "I3" },
                OperandCount = 1,
                Operand1Type = ValueType.CoreStorageLocation
            });

            // === SHORT FORMAT WITH CONDITIONS (BSC/BOSC) ===
            
            Add(InstructionPattern.ShortConditionOnly, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortConditionOnly,
                Description = "Short format with condition codes only (no address)",
                RequiresModifier = false,
                ValidModifiers = new string[0],
                OperandCount = 1,
                Operand1Type = ValueType.Conditions
            });

            Add(InstructionPattern.ShortIndex1ConditionOnly, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex1ConditionOnly,
                Description = "Short format with index 1 and condition codes",
                RequiresModifier = true,
                ValidModifiers = new[] { "1" },
                OperandCount = 1,
                Operand1Type = ValueType.Conditions
            });

            Add(InstructionPattern.ShortIndex2ConditionOnly, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex2ConditionOnly,
                Description = "Short format with index 2 and condition codes",
                RequiresModifier = true,
                ValidModifiers = new[] { "2" },
                OperandCount = 1,
                Operand1Type = ValueType.Conditions
            });

            Add(InstructionPattern.ShortIndex3ConditionOnly, new PatternDefinition
            {
                Pattern = InstructionPattern.ShortIndex3ConditionOnly,
                Description = "Short format with index 3 and condition codes",
                RequiresModifier = true,
                ValidModifiers = new[] { "3" },
                OperandCount = 1,
                Operand1Type = ValueType.Conditions
            });

            // === LONG FORMAT WITH CONDITIONS ===
            
            Add(InstructionPattern.LongWithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.LongWithConditions,
                Description = "Long format with address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "L" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.LongIndex1WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex1WithConditions,
                Description = "Long format with index 1, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "L1" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.LongIndex2WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex2WithConditions,
                Description = "Long format with index 2, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "L2" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.LongIndex3WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.LongIndex3WithConditions,
                Description = "Long format with index 3, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "L3" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            // === INDIRECT FORMAT WITH CONDITIONS ===
            
            Add(InstructionPattern.IndirectWithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectWithConditions,
                Description = "Indirect with address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "I" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.IndirectIndex1WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex1WithConditions,
                Description = "Indirect with index 1, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "I1" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.IndirectIndex2WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex2WithConditions,
                Description = "Indirect with index 2, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "I2" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            Add(InstructionPattern.IndirectIndex3WithConditions, new PatternDefinition
            {
                Pattern = InstructionPattern.IndirectIndex3WithConditions,
                Description = "Indirect with index 3, address and conditions",
                RequiresModifier = true,
                ValidModifiers = new[] { "I3" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Conditions
            });

            // === LONG FORMAT WITH TWO OPERANDS (MDX) ===
            
            Add(InstructionPattern.LongTwoOperand, new PatternDefinition
            {
                Pattern = InstructionPattern.LongTwoOperand,
                Description = "Long format with address and modifier (MDX)",
                RequiresModifier = true,
                ValidModifiers = new[] { "L" },
                OperandCount = 2,
                Operand1Type = ValueType.CoreStorageLocation,
                Operand2Type = ValueType.Modifier
            });

            // === EXTENDED VALUE (BSS) ===
            
            Add(InstructionPattern.ExtendedValue, new PatternDefinition
            {
                Pattern = InstructionPattern.ExtendedValue,
                Description = "Extended mode with value (BSS)",
                RequiresModifier = true,
                ValidModifiers = new[] { "E" },
                OperandCount = 1,
                Operand1Type = ValueType.ImmediateValue
            });
        }

        private static void Add(InstructionPattern pattern, PatternDefinition definition)
        {
            _patterns[pattern] = definition;
        }
    }
}
