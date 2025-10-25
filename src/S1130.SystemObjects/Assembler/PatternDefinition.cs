using System;
using System.Collections.Generic;
using System.Linq;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Defines the structure, validation, and code generation logic for an instruction pattern.
    /// Each pattern knows how to match operand strings, validate ranges, and generate machine code.
    /// </summary>
    public class PatternDefinition
    {
        public InstructionPattern Pattern { get; set; }
        public string Description { get; set; }
        
        // Pattern structure
        public bool RequiresModifier { get; set; }
        public string[] ValidModifiers { get; set; }
        public int OperandCount { get; set; }
        public ValueType? Operand1Type { get; set; }
        public ValueType? Operand2Type { get; set; }
        
        /// <summary>
        /// Attempts to match the given operand string against this pattern.
        /// Returns true if the operand structure matches (modifiers, operand count).
        /// Does NOT validate value ranges - that's done in ValidateAndGenerate.
        /// </summary>
        public bool Matches(string operand, out string error)
        {
            error = null;
            
            // Pattern: NoOperand
            if (Pattern == InstructionPattern.NoOperand)
            {
                if (!string.IsNullOrWhiteSpace(operand))
                {
                    error = "This instruction takes no operands";
                    return false;
                }
                return true;
            }
            
            // Parse the operand
            var parsed = ParseOperand(operand);
            
            // Check modifier requirement
            if (RequiresModifier)
            {
                if (string.IsNullOrEmpty(parsed.Modifier))
                {
                    error = $"Missing format/index modifier. Expected one of: {string.Join(", ", ValidModifiers.Select(m => $"|{m}|"))}";
                    return false;
                }
                
                // Validate modifier is in allowed list
                if (!ValidModifiers.Contains(parsed.Modifier))
                {
                    error = $"Invalid modifier '|{parsed.Modifier}|'. Expected one of: {string.Join(", ", ValidModifiers.Select(m => $"|{m}|"))}";
                    return false;
                }
            }
            else
            {
                // Pattern doesn't allow modifiers
                if (!string.IsNullOrEmpty(parsed.Modifier))
                {
                    error = "This pattern does not accept format/index modifiers";
                    return false;
                }
            }
            
            // Check operand count
            if (parsed.Operands.Length != OperandCount)
            {
                error = $"Expected {OperandCount} operand(s), got {parsed.Operands.Length}";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Validates operand value ranges and generates instruction words.
        /// Returns null if validation fails.
        /// </summary>
        public ushort[] ValidateAndGenerate(
            byte opcode, 
            string operand, 
            Dictionary<string, int> symbolTable, 
            int currentAddress, 
            List<string> errors,
            byte? shiftType = null)
        {
            var parsed = ParseOperand(operand);
            
            // Evaluate operand expressions to numeric values
            var values = new int[parsed.Operands.Length];
            var evaluator = new ExpressionEvaluator(symbolTable, currentAddress);
            
            for (int i = 0; i < parsed.Operands.Length; i++)
            {
                if (!evaluator.Evaluate(parsed.Operands[i], out values[i], out string evalError))
                {
                    errors.Add(evalError);
                    return null;
                }
            }
            
            // Validate ranges based on value types
            if (Operand1Type.HasValue && parsed.Operands.Length > 0)
            {
                if (!ValidateValueRange(values[0], Operand1Type.Value, out string rangeError))
                {
                    errors.Add(rangeError);
                }
            }
            
            if (Operand2Type.HasValue && parsed.Operands.Length > 1)
            {
                if (!ValidateValueRange(values[1], Operand2Type.Value, out string rangeError))
                {
                    errors.Add(rangeError);
                }
            }
            
            // If there were validation errors, don't generate code
            if (errors.Count > 0)
                return null;
            
            // Generate instruction words based on pattern
            return GenerateWords(opcode, parsed.Modifier, values, shiftType);
        }
        
        private bool ValidateValueRange(int value, ValueType type, out string error)
        {
            error = null;
            
            switch (type)
            {
                case ValueType.Displacement:
                    if (value < -128 || value > 127)
                    {
                        error = $"Displacement {value} out of range (-128 to +127)";
                        return false;
                    }
                    break;
                    
                case ValueType.CoreStorageLocation:
                    if (value < 0 || value > 32767)
                    {
                        error = $"Address {value} out of range (0 to 32767)";
                        return false;
                    }
                    break;
                    
                case ValueType.ShiftCount:
                    if (value < 0 || value > 63)
                    {
                        error = $"Shift count {value} out of range (0 to 63)";
                        return false;
                    }
                    break;
                    
                case ValueType.Modifier:
                    if (value < -128 || value > 127)
                    {
                        error = $"Modifier {value} out of range (-128 to +127)";
                        return false;
                    }
                    break;
                    
                case ValueType.ImmediateValue:
                    if (value < -32768 || value > 65535)
                    {
                        error = $"Value {value} out of range (-32768 to +65535)";
                        return false;
                    }
                    break;
            }
            
            return true;
        }
        
        private ParsedOperand ParseOperand(string operand)
        {
            var result = new ParsedOperand();
            
            if (string.IsNullOrWhiteSpace(operand))
                return result;
            
            // Trim any whitespace
            operand = operand.Trim();
            
            // Check for modifier |xxx|
            if (operand.StartsWith("|"))
            {
                var endPipe = operand.IndexOf('|', 1);
                if (endPipe > 0)
                {
                    result.Modifier = operand.Substring(1, endPipe - 1);
                    operand = operand.Substring(endPipe + 1);
                }
            }
            
            // Split on comma for two-operand instructions or condition codes
            if (!string.IsNullOrEmpty(operand))
            {
                result.Operands = operand.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(s => s.Trim())
                                          .ToArray();
            }
            else
            {
                result.Operands = new string[0];
            }
            
            return result;
        }
        
        private ushort[] GenerateWords(byte opcode, string modifier, int[] values, byte? shiftType = null)
        {
            // Build instruction word(s) based on pattern
            // Bit layout: OpCode(5) | Format(1) | Tag(2) | [Indirect(1) or Modifier bits] | Displacement/Modifiers(7-8)
            
            ushort word1 = 0;
            
            // Set opcode (bits 0-4, shifted left 11)
            word1 |= (ushort)((opcode & 0x1F) << 11);
            
            // Determine format bit (bit 5) and tag bits (bits 6-7)
            bool formatLong = false;
            bool indirect = false;
            byte tag = 0;
            
            if (!string.IsNullOrEmpty(modifier))
            {
                // Parse modifier string (e.g., "L", "L1", "I", "I2", "1", "E")
                if (modifier.StartsWith("I"))
                {
                    indirect = true;
                    formatLong = true;
                    if (modifier.Length > 1)
                        tag = byte.Parse(modifier.Substring(1));
                }
                else if (modifier.StartsWith("L"))
                {
                    formatLong = true;
                    if (modifier.Length > 1)
                        tag = byte.Parse(modifier.Substring(1));
                }
                else if (modifier == "E")
                {
                    // Extended mode for BSS - handled below
                }
                else if (char.IsDigit(modifier[0]))
                {
                    // Short format with index (just "1", "2", or "3")
                    tag = byte.Parse(modifier);
                }
            }
            
            // Set format bit (IBM bit 5 = 0x0400)
            if (formatLong)
                word1 |= 0x0400; // Constants.FormatLong
            
            // Set tag bits (IBM bits 6-7)
            word1 |= (ushort)((tag & 0x03) << 8); // Constants.TagShift
            
            // Handle different pattern types
            if (formatLong)
            {
                // Long format: word 1 has indirect bit (bit 8), word 2 has address
                if (indirect)
                    word1 |= 0x0080; // Constants.Indirect
                
                // Add modifier bits if needed (e.g., for MDX, BSC condition codes)
                if (values.Length > 1)
                {
                    // Two-operand (MDX): second value goes in bits 8-15 of word 1
                    word1 |= (ushort)(values[1] & 0xFF);
                }
                else if (Operand2Type == ValueType.Conditions)
                {
                    // BSC/BOSC condition codes - TODO: need to handle condition string parsing
                    // For now this is a placeholder
                }
                
                // Word 2 contains the address
                ushort word2 = (ushort)(values[0] & 0xFFFF);
                
                return new ushort[] { word1, word2 };
            }
            else
            {
                // Short format: displacement/value in bits 8-15
                if (OperandCount > 0)
                {
                    int displValue = values[0];
                    
                    // For shift instructions, encode shift type in bits 6-7 and count in bits 0-5
                    if (shiftType.HasValue)
                    {
                        // Shift count is in value, shift type goes in bits 6-7
                        displValue = (displValue & 0x3F) | ((shiftType.Value & 0x03) << 6);
                    }
                    
                    word1 |= (ushort)(displValue & 0xFF); // Constants.DisplacementMask
                }
                
                return new ushort[] { word1 };
            }
        }
        
        private class ParsedOperand
        {
            public string Modifier { get; set; }
            public string[] Operands { get; set; } = new string[0];
        }
    }
}
