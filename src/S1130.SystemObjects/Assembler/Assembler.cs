using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Pattern-based two-pass assembler for IBM 1130 assembly language.
    /// Uses instruction patterns to validate syntax and generate machine code.
    /// </summary>
    public class Assembler
    {
        private Dictionary<string, int> _symbolTable;
        private List<ushort> _generatedCode;
        private int _currentAddress;
        private int _startAddress;
        private bool _startAddressSet; // Track if ORG has set the start address
        private List<AssemblyError> _errors;
        private List<AssemblyWarning> _warnings;

        /// <summary>
        /// Assembles source code into machine code
        /// </summary>
        /// <param name="sourceLines">Array of source code lines</param>
        /// <param name="startAddress">Starting address for assembly (default 0)</param>
        /// <returns>Assembly result with generated code or errors</returns>
        public AssemblyResult Assemble(string[] sourceLines, int startAddress = 0)
        {
            // Initialize
            _symbolTable = new Dictionary<string, int>();
            _generatedCode = new List<ushort>();
            _currentAddress = startAddress;
            _startAddress = startAddress;
            _startAddressSet = false; // Reset flag
            _errors = new List<AssemblyError>();
            _warnings = new List<AssemblyWarning>();

            // Pass 1: Build symbol table
            Pass1(sourceLines);

            // Pass 2: Generate code (only if no errors from pass 1)
            if (_errors.Count == 0)
            {
                _currentAddress = startAddress;
                _generatedCode.Clear();
                Pass2(sourceLines);
            }

            // Return results
            return new AssemblyResult
            {
                Success = _errors.Count == 0,
                Errors = _errors,
                Warnings = _warnings,
                GeneratedWords = _generatedCode.ToArray(),
                Symbols = _symbolTable,
                StartAddress = _startAddress
            };
        }

        /// <summary>
        /// Pass 1: Build symbol table and calculate addresses
        /// </summary>
        private void Pass1(string[] sourceLines)
        {
            for (int i = 0; i < sourceLines.Length; i++)
            {
                int lineNumber = i + 1;
                string line = sourceLines[i];

                try
                {
                    var parsed = ParseLine(line);

                    // Skip blank lines and comments
                    if (parsed.IsBlank)
                        continue;

                    // Handle label
                    if (!string.IsNullOrEmpty(parsed.Label))
                    {
                        if (_symbolTable.ContainsKey(parsed.Label))
                        {
                            AddError(lineNumber, line, $"Duplicate label: {parsed.Label}", ErrorType.DuplicateLabel);
                        }
                        else
                        {
                            _symbolTable[parsed.Label] = _currentAddress;
                        }
                    }

                    // Handle directives
                    if (!string.IsNullOrEmpty(parsed.Opcode))
                    {
                        if (parsed.Opcode == "ORG")
                        {
                            // ORG sets the current address
                            var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                            if (evaluator.Evaluate(parsed.Operand, out int address, out string error))
                            {
                                _currentAddress = address;
                                // First ORG sets the start address
                                if (!_startAddressSet)
                                {
                                    _startAddress = address;
                                    _startAddressSet = true;
                                }
                            }
                            else
                            {
                                AddError(lineNumber, line, error, ErrorType.InvalidExpression);
                            }
                            continue;
                        }
                        else if (parsed.Opcode == "EQU")
                        {
                            // EQU assigns value to symbol (doesn't advance address)
                            if (!string.IsNullOrEmpty(parsed.Label))
                            {
                                var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                                if (evaluator.Evaluate(parsed.Operand, out int value, out string error))
                                {
                                    _symbolTable[parsed.Label] = value;
                                }
                                else
                                {
                                    AddError(lineNumber, line, error, ErrorType.InvalidExpression);
                                }
                            }
                            continue; // EQU doesn't generate code or advance address
                        }
                        else if (parsed.Opcode == "BSS")
                        {
                            // BSS reserves space
                            // Format can be: BSS n  or  BSS E n (E for equate, same as EQU + BSS)
                            string sizeExpr = parsed.Operand;
                            
                            if (parsed.Operand != null && parsed.Operand.Trim().StartsWith("E ", StringComparison.OrdinalIgnoreCase))
                            {
                                // BSS E n format - treat as EQU for the label, then reserve n words
                                sizeExpr = parsed.Operand.Substring(2).Trim();
                            }
                            
                            var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                            if (evaluator.Evaluate(sizeExpr, out int size, out string error))
                            {
                                _currentAddress += size;
                            }
                            else
                            {
                                AddError(lineNumber, line, error, ErrorType.InvalidExpression);
                            }
                            continue;
                        }
                        else if (parsed.Opcode == "BES")
                        {
                            // BES marks end of BSS block (no action needed)
                            continue;
                        }
                        else if (parsed.Opcode == "DC")
                        {
                            // DC generates one word
                            _currentAddress++;
                            continue;
                        }

                        // Regular instruction - look it up
                        if (InstructionCatalog.TryGetInstruction(parsed.Opcode, out var instrDef))
                        {
                            // Check if it's long format (generates 2 words)
                            bool isLongFormat = !string.IsNullOrEmpty(parsed.Operand) && 
                                               (parsed.Operand.Contains("|L") || parsed.Operand.Contains("|I"));
                            
                            _currentAddress += isLongFormat ? 2 : 1;
                        }
                        else
                        {
                            AddError(lineNumber, line, $"Unknown instruction: {parsed.Opcode}", ErrorType.InvalidOpcode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddError(lineNumber, line, $"Pass 1 error: {ex.Message}", ErrorType.Other);
                }
            }
        }

        /// <summary>
        /// Pass 2: Generate machine code
        /// </summary>
        private void Pass2(string[] sourceLines)
        {
            for (int i = 0; i < sourceLines.Length; i++)
            {
                int lineNumber = i + 1;
                string line = sourceLines[i];

                try
                {
                    var parsed = ParseLine(line);

                    // Skip blank lines and comments
                    if (parsed.IsBlank)
                        continue;

                    // Handle ORG directive
                    if (parsed.Opcode == "ORG")
                    {
                        var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                        if (evaluator.Evaluate(parsed.Operand, out int address, out string error))
                        {
                            _currentAddress = address;
                        }
                        continue;
                    }

                    // Skip directives that don't generate code
                    if (parsed.Opcode == "EQU" || parsed.Opcode == "BES")
                        continue;

                    // Handle BSS (advance address without generating code)
                    if (parsed.Opcode == "BSS")
                    {
                        string sizeExpr = parsed.Operand;
                        
                        if (parsed.Operand != null && parsed.Operand.Trim().StartsWith("E ", StringComparison.OrdinalIgnoreCase))
                        {
                            // BSS E n format
                            sizeExpr = parsed.Operand.Substring(2).Trim();
                        }
                        
                        var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                        if (evaluator.Evaluate(sizeExpr, out int size, out string error))
                        {
                            // Reserve space (fill with zeros)
                            for (int j = 0; j < size; j++)
                            {
                                _generatedCode.Add(0);
                            }
                            _currentAddress += size;
                        }
                        continue;
                    }                    // Handle DC (define constant)
                    if (parsed.Opcode == "DC")
                    {
                        var evaluator = new ExpressionEvaluator(_symbolTable, _currentAddress);
                        if (evaluator.Evaluate(parsed.Operand, out int value, out string error))
                        {
                            _generatedCode.Add((ushort)(value & 0xFFFF));
                            _currentAddress++;
                        }
                        else
                        {
                            AddError(lineNumber, line, error, ErrorType.InvalidExpression);
                        }
                        continue;
                    }

                    // Look up instruction
                    if (!InstructionCatalog.TryGetInstruction(parsed.Opcode, out var instrDef))
                    {
                        AddError(lineNumber, line, $"Unknown instruction: {parsed.Opcode}", ErrorType.InvalidOpcode);
                        continue;
                    }

                    // Handle pseudo-ops
                    if (instrDef.IsPseudoOp)
                    {
                        if (!string.IsNullOrEmpty(instrDef.PseudoOpMapping))
                        {
                            // Simple pseudo-ops like NOP -> SLA 0, XCH -> RTE 16
                            parsed = ParseLine(instrDef.PseudoOpMapping);
                            if (!InstructionCatalog.TryGetInstruction(parsed.Opcode, out instrDef))
                            {
                                AddError(lineNumber, line, $"Pseudo-op mapping failed: {parsed.Opcode}", ErrorType.Other);
                                continue;
                            }
                        }
                        else if (instrDef.PseudoOpCondition != null)
                        {
                            // Branch pseudo-ops like BP, BN, BZ, etc.
                            if (parsed.Opcode == "SKP")
                            {
                                // SKP is BSC /1 with no conditions
                                parsed.Operand = "/1";
                            }
                            else if (!string.IsNullOrEmpty(instrDef.PseudoOpCondition))
                            {
                                // Add condition to operand (e.g., BP /0100 -> BSC /0100,+)
                                parsed.Operand = parsed.Operand + "," + instrDef.PseudoOpCondition;
                            }
                            // Map to BSC instruction
                            parsed.Opcode = "BSC";
                            if (!InstructionCatalog.TryGetInstruction(parsed.Opcode, out instrDef))
                            {
                                AddError(lineNumber, line, $"Pseudo-op mapping failed: {parsed.Opcode}", ErrorType.Other);
                                continue;
                            }
                        }
                    }

                    // Try to match operand against valid patterns
                    bool matched = false;
                    List<string> matchErrors = new List<string>();

                    foreach (var patternType in instrDef.ValidPatterns)
                    {
                        var pattern = PatternCatalog.GetPattern(patternType);
                        
                        // Check if pattern matches
                        if (pattern.Matches(parsed.Operand ?? string.Empty, out string matchError))
                        {
                            // Pattern matches - now validate and generate
                            var patternErrors = new List<string>();
                            var words = pattern.ValidateAndGenerate(
                                instrDef.Opcode,
                                parsed.Operand ?? string.Empty,
                                _symbolTable,
                                _currentAddress,
                                patternErrors,
                                instrDef.ShiftType);

                            if (patternErrors.Count > 0)
                            {
                                // Validation failed - collect errors
                                matchErrors.AddRange(patternErrors);
                            }
                            else if (words != null)
                            {
                                // Success - add generated words
                                foreach (var word in words)
                                {
                                    _generatedCode.Add(word);
                                }
                                _currentAddress += words.Length;
                                matched = true;
                                break;
                            }
                        }
                        else
                        {
                            matchErrors.Add(matchError);
                        }
                    }

                    if (!matched)
                    {
                        // No pattern matched - report all errors
                        if (matchErrors.Count > 0)
                        {
                            AddError(lineNumber, line, 
                                $"Invalid operand format. Tried {instrDef.ValidPatterns.Count} pattern(s). " +
                                $"Errors: {string.Join("; ", matchErrors.Distinct())}", 
                                ErrorType.InvalidPattern);
                        }
                        else
                        {
                            AddError(lineNumber, line, 
                                $"No valid pattern found for {parsed.Opcode} {parsed.Operand}", 
                                ErrorType.InvalidPattern);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddError(lineNumber, line, $"Pass 2 error: {ex.Message}", ErrorType.Other);
                }
            }
        }

        /// <summary>
        /// Parses a source line into label, opcode, and operand
        /// </summary>
        private ParsedLine ParseLine(string line)
        {
            var result = new ParsedLine { OriginalLine = line };

            if (string.IsNullOrWhiteSpace(line))
            {
                result.IsBlank = true;
                return result;
            }

            // Remove comments (// to end of line)
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                result.Comment = line.Substring(commentIndex + 2).Trim();
                line = line.Substring(0, commentIndex);
            }

            // Trim whitespace
            line = line.Trim();

            if (string.IsNullOrEmpty(line))
            {
                result.IsBlank = true;
                return result;
            }

            // Check for label (ends with :)
            int colonIndex = line.IndexOf(':');
            if (colonIndex >= 0)
            {
                result.Label = line.Substring(0, colonIndex).Trim().ToUpperInvariant();
                line = line.Substring(colonIndex + 1).Trim();
            }

            if (string.IsNullOrEmpty(line))
            {
                return result; // Label only, no instruction
            }

            // Split remaining into opcode and operand (first whitespace)
            var parts = Regex.Split(line, @"\s+", RegexOptions.None, TimeSpan.FromMilliseconds(100));
            
            if (parts.Length > 0)
            {
                result.Opcode = parts[0].Trim().ToUpperInvariant();
            }

            if (parts.Length > 1)
            {
                result.Operand = string.Join("", parts.Skip(1)).Trim();
                
                // Reject old syntax with standalone dots (should use pipe syntax or nothing)
                if (result.Operand.Contains(" . ") || result.Operand.StartsWith(". ") || result.Operand.EndsWith(" ."))
                {
                    result.Operand = null; // Will cause pattern matching to fail with appropriate error
                }
            }

            return result;
        }

        private void AddError(int lineNumber, string line, string message, ErrorType type)
        {
            _errors.Add(new AssemblyError
            {
                LineNumber = lineNumber,
                Line = line,
                Message = message,
                Type = type
            });
        }

        private void AddWarning(int lineNumber, string line, string message)
        {
            _warnings.Add(new AssemblyWarning
            {
                LineNumber = lineNumber,
                Line = line,
                Message = message
            });
        }

        private class ParsedLine
        {
            public string OriginalLine { get; set; }
            public string Label { get; set; }
            public string Opcode { get; set; }
            public string Operand { get; set; }
            public string Comment { get; set; }
            public bool IsBlank { get; set; }
        }
    }
}
