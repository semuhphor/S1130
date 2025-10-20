using System;
using System.Linq;

namespace S1130.SystemObjects
{
    /// <summary>
    /// IBM 1130 Assembler Implementation
    /// </summary>
    public class Assembler
    {
        private readonly ICpu _cpu;
        private AssemblyContext _context;
        private int _currentLine;

        /// <summary>
        /// Initializes a new instance of the Assembler class.
        /// </summary>
        /// <param name="cpu">The CPU to assemble code for</param>
        public Assembler(ICpu cpu)
        {
            _cpu = cpu;
        }

        /// <summary>
        /// Assembles IBM 1130 assembly code into memory.
        /// </summary>
        /// <param name="sourceCode">Assembly source code with lines separated by CR, LF or CRLF</param>
        /// <returns>Assembly result containing listing, errors and success flag</returns>
        public AssemblyResult Assemble(string sourceCode)
        {
            // Create new context with source code
            _context = new AssemblyContext(sourceCode);
            _currentLine = 0;

            try 
            {
                // Handle empty source as a special case
                if (string.IsNullOrEmpty(sourceCode))
                {
                    return new AssemblyResult 
                    { 
                        Success = true,
                        Errors = Array.Empty<AssemblyError>(),
                        Listing = Array.Empty<string>()
                    };
                }
                
                // Pass 1: Build symbol table
                foreach (var line in _context.SourceLines)
                {
                    _currentLine++;
                    Pass1ProcessLine(line);
                    if (_context.Errors.Any())
                        break;
                }

                if (!_context.Errors.Any())
                {
                    // Reset for pass 2
                    _context.LocationCounter = 0;
                    _context.HasOrigin = false;
                    _currentLine = 0;
                    
                    // Pass 2: Generate code and listing
                    foreach (var line in _context.SourceLines)
                    {
                        _currentLine++;
                        Pass2ProcessLine(line);
                        if (_context.Errors.Any())
                            break;
                    }
                }

                var success = !_context.Errors.Any();
                var errors = _context.Errors.ToArray();
                var listing = _context.Listing.ToArray();
                var listingLines = _context.ListingLines.ToArray();
                
                return new AssemblyResult 
                { 
                    Success = success,
                    Errors = errors,
                    Listing = listing,
                    ListingLines = listingLines
                };
            }
            catch (Exception ex)
            {
                _context.AddError(_currentLine, $"Internal error: {ex.Message}");
                
                var success = false;
                var errors = _context.Errors.ToArray();
                var listing = _context.Listing.ToArray();
                var listingLines = _context.ListingLines.ToArray();
                
                return new AssemblyResult 
                { 
                    Success = success,
                    Errors = errors,
                    Listing = listing,
                    ListingLines = listingLines
                };
            }
        }

        private void Pass1ProcessLine(string line)
        {
            // Skip blank lines (treat as comments)
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // Handle comments (lines starting with *)
            if (line.TrimStart().StartsWith("*"))
            {
                return;
            }

            var parts = new AssemblyLine(line);

            // Check if we have a label
            if (!string.IsNullOrWhiteSpace(parts.Label))
            {
                var label = parts.Label.ToUpper();
                if (_context.Symbols.ContainsKey(label))
                {
                    _context.AddError(_currentLine, $"Duplicate label: {label}");
                    return;
                }
                _context.Symbols[label] = (ushort)_context.LocationCounter;
            }

            // Check if we have an operation
            if (string.IsNullOrWhiteSpace(parts.Operation))
            {
                _context.AddError(_currentLine, "Line must contain an operation");
                return;
            }

            var operation = parts.Operation.ToUpper();
            if (operation == "ORG")
            {
                ProcessOrg(parts.Operand);
            }
            else if (!_context.HasOrigin)
            {
                _context.AddError(_currentLine, "Missing ORG directive");
                return;
            }
            else if (operation == "DC")
            {
                _context.LocationCounter++;
            }
            else if (operation == "EQU")
            {
                // EQU does not advance location counter, just creates a symbol
                // The label was already processed and added to symbol table
                // Now we need to update its value based on the operand
                if (!string.IsNullOrWhiteSpace(parts.Label))
                {
                    var label = parts.Label.ToUpper();
                    // Parse the operand to get the value
                    ushort value = ResolveOperand(parts.Operand);
                    _context.Symbols[label] = value; // Update/set the symbol value
                }
            }
            else if (operation == "BSS")
            {
                // BSS - Block Started by Symbol
                // Reserves a block of storage
                // Label refers to leftmost (first) word of reserved area
                // Format: BSS [E] count
                // E = even boundary alignment (column 32 in original format)
                if (!string.IsNullOrWhiteSpace(parts.Operand))
                {
                    var operand = parts.Operand.Trim().ToUpper();
                    bool evenAlign = false;
                    string countStr = operand;
                    
                    // Check for E modifier
                    if (operand.StartsWith("E "))
                    {
                        evenAlign = true;
                        countStr = operand.Substring(2).Trim();
                    }
                    
                    // Align to even boundary if requested
                    if (evenAlign && (_context.LocationCounter % 2) != 0)
                    {
                        _context.LocationCounter++;
                    }
                    
                    // If label was specified, it was already added to symbol table
                    // pointing to current location counter (leftmost word)
                    
                    // Parse count (can be decimal, hex, or symbol)
                    int count;
                    if (countStr.StartsWith("/"))
                    {
                        if (TryParseHex(countStr.Substring(1), out ushort hexVal))
                            count = hexVal;
                        else
                            count = 0;
                    }
                    else if (int.TryParse(countStr, out count))
                    {
                        // Parsed as decimal
                    }
                    else
                    {
                        // Try to resolve as symbol
                        ushort symbolValue = ResolveOperand(countStr);
                        if (_context.Errors.Any())
                            return;
                        count = symbolValue;
                    }
                    
                    // Advance location counter by count
                    _context.LocationCounter += count;
                }
            }
            else if (operation == "BES")
            {
                // BES - Block Ended by Symbol
                // Identical to BSS except label refers to rightmost word + 1
                // i.e., the next location available for assignment (after the reserved area)
                // Format: BES [E] count
                if (!string.IsNullOrWhiteSpace(parts.Operand))
                {
                    var operand = parts.Operand.Trim().ToUpper();
                    bool evenAlign = false;
                    string countStr = operand;
                    
                    // Check for E modifier
                    if (operand.StartsWith("E "))
                    {
                        evenAlign = true;
                        countStr = operand.Substring(2).Trim();
                    }
                    
                    // Align to even boundary if requested
                    if (evenAlign && (_context.LocationCounter % 2) != 0)
                    {
                        _context.LocationCounter++;
                    }
                    
                    // Parse count (can be decimal, hex, or symbol)
                    int count;
                    if (countStr.StartsWith("/"))
                    {
                        if (TryParseHex(countStr.Substring(1), out ushort hexVal))
                            count = hexVal;
                        else
                            count = 0;
                    }
                    else if (int.TryParse(countStr, out count))
                    {
                        // Parsed as decimal
                    }
                    else
                    {
                        // Try to resolve as symbol
                        ushort symbolValue = ResolveOperand(countStr);
                        if (_context.Errors.Any())
                            return;
                        count = symbolValue;
                    }
                    
                    // Advance location counter by count
                    _context.LocationCounter += count;
                    
                    // If label was specified, update it to point to location AFTER reserved area
                    if (!string.IsNullOrWhiteSpace(parts.Label))
                    {
                        var label = parts.Label.ToUpper();
                        _context.Symbols[label] = (ushort)_context.LocationCounter;
                    }
                }
            }
            else if (operation == "LD" || operation == "LDD" || operation == "STO" || operation == "STD" ||
                     operation == "LDX" || operation == "STX" || operation == "LDS" || operation == "STS" ||
                     operation == "A" || operation == "AD" || operation == "S" || operation == "SD" ||
                     operation == "M" || operation == "D" ||
                     operation == "AND" || operation == "OR" || operation == "EOR")
            {
                // These can be short or long format
                // Check format specifier: L, L1-L3 = long (2 words)
                // Default (no format specifier or . or 1-3 or I or I1-I3) = short (1 word)
                var operandTrim = parts.Operand?.Trim() ?? "";
                var firstToken = operandTrim.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                var formatSpec = firstToken.ToUpper();
                
                // Only L, L1, L2, L3 indicate long format
                // Everything else (., 1, 2, 3, I, I1, I2, I3, or no prefix) = short format
                bool isLongFormat = formatSpec == "L" || formatSpec == "L1" || formatSpec == "L2" || formatSpec == "L3";
                
                // Long format = 2 words, all others = 1 word
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else if (operation == "WAIT" || operation == "SLA" || operation == "SLT" || 
                     operation == "SLC" || operation == "SLCA" || operation == "SRA" || operation == "SRT" || operation == "RTE")
            {
                // Short format: 1 word
                _context.LocationCounter++;
            }
            else if (operation == "BSC" || operation == "BSI" || operation == "MDX")
            {
                // Branch instructions can be short or long format
                // Only L, L1, L2, L3 indicate long format
                // Everything else defaults to short format
                var operandTrim = parts.Operand?.Trim() ?? "";
                var firstToken = operandTrim.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                var formatSpec = firstToken.ToUpper();
                
                bool isLongFormat = formatSpec == "L" || formatSpec == "L1" || formatSpec == "L2" || formatSpec == "L3";
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else if (operation == "XIO")
            {
                // XIO can be short or long format
                // Only L, L1, L2, L3 indicate long format
                var operandTrim = parts.Operand?.Trim() ?? "";
                var firstToken = operandTrim.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                var formatSpec = firstToken.ToUpper();
                
                bool isLongFormat = formatSpec == "L" || formatSpec == "L1" || formatSpec == "L2" || formatSpec == "L3";
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            // Pseudo operations - generate BSC instructions
            else if (operation == "B" || operation == "BP" || operation == "BNP" || 
                     operation == "BN" || operation == "BNN" || operation == "BZ" || 
                     operation == "BNZ" || operation == "BC" || operation == "BO" || operation == "BOD")
            {
                // These are BSC instructions with preset conditions - can be short or long
                var operandTrim = parts.Operand?.Trim() ?? "";
                var firstToken = operandTrim.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                var formatSpec = firstToken.ToUpper();
                
                bool isLongFormat = formatSpec == "L" || formatSpec == "L1" || formatSpec == "L2" || formatSpec == "L3";
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else if (operation == "SKP")
            {
                // SKP is always short format (skip next instruction)
                _context.LocationCounter++;
            }
            else
            {
                _context.AddError(_currentLine, $"Unknown operation: {operation}");
            }
        }

        private void Pass2ProcessLine(string line)
        {
            // Skip blank lines (treat as comments)
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // Handle comments (lines starting with *)
            if (line.TrimStart().StartsWith("*"))
            {
                FormatListingLine(line);
                AddStructuredListingLine(line, (ushort)_context.LocationCounter, null);
                return;
            }

            var parts = new AssemblyLine(line);

            // Check if we have an operation
            if (string.IsNullOrWhiteSpace(parts.Operation))
            {
                _context.AddError(_currentLine, "Line must contain an operation");
                return;
            }

            var operation = parts.Operation.ToUpper();
            var addressBefore = (ushort)_context.LocationCounter;
            
            if (operation == "ORG")
            {
                ProcessOrg(parts.Operand);
                FormatListingLine(line);
                AddStructuredListingLine(line, addressBefore, null);
            }
            else if (!_context.HasOrigin)
            {
                _context.AddError(_currentLine, "Missing ORG directive");
                return;
            }
            else if (operation == "DC")
            {
                // Format listing BEFORE incrementing location counter
                FormatListingLine(line);
                ProcessDc(parts.Operand);
                // DC stores a value, capture it
                AddStructuredListingLine(line, addressBefore, _context.HasOrigin && addressBefore < _cpu.MemorySize ? _cpu[addressBefore] : (ushort?)null);
            }
            else if (operation == "EQU")
            {
                // EQU just defines a symbol, no code generation
                FormatListingLine(line);
                AddStructuredListingLine(line, addressBefore, null);
                // Symbol was already handled in Pass1
            }
            else if (operation == "BSS")
            {
                // BSS reserves storage, no initialization
                FormatListingLine(line);
                ProcessBss(parts.Operand);
                AddStructuredListingLine(line, addressBefore, null);
            }
            else if (operation == "BES")
            {
                // BES ends a BSS block
                FormatListingLine(line);
                ProcessBes(parts.Operand);
                AddStructuredListingLine(line, addressBefore, null);
            }
            else if (operation == "LD")
            {
                FormatListingLine(line);
                ProcessLoad(parts.Operand);
                AddStructuredListingLine(line, addressBefore, _context.HasOrigin && addressBefore < _cpu.MemorySize ? _cpu[addressBefore] : (ushort?)null);
            }
            else if (operation == "STO")
            {
                FormatListingLine(line);
                ProcessStore(parts.Operand);
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "LDD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x19, "LDD");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "STD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1B, "STD");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "LDX")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x0C, "LDX");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "STX")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x0D, "STX");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "A")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x10, "A");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "S")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x12, "S");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "M")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x14, "M");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "D")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x15, "D");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "AD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x11, "AD");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x13, "SD");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "AND")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1C, "AND");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "OR")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1D, "OR");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "EOR")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1E, "EOR");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "LDS")
            {
                FormatListingLine(line);
                ProcessLoadStatus(parts.Operand);
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "STS")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x05, "STS");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SLA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 0, "SLA");  // Type 0: Shift Left Accumulator
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SLT")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 2, "SLT");  // Type 2: Shift Left AccExt Together (FIXED: was 1, should be 2)
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SLC")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 3, "SLC");  // Type 3: Shift Left and Count AccExt (FIXED: was 2, should be 3)
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SLCA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 1, "SLCA");  // Type 1: Shift Left and Count Accumulator (FIXED: was 3, should be 1)
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SRA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x03, 0, "SRA");  // Type 0: Shift Right Accumulator
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SRT")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x03, 2, "SRT");  // Type 2: Shift Right AccExt Together (FIXED: was 1, should be 2)
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "RTE")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x03, 3, "RTE");  // Type 3: Rotate Ext
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BSC")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x09, "BSC");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BSI")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x08, "BSI");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "MDX")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x0A, "MDX");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "XIO")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x01, "XIO");
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "WAIT")
            {
                FormatListingLine(line);
                ProcessWait();
                AddInstructionListingLine(line, addressBefore);
            }
            // Pseudo operations - convenience mnemonics for branch instructions
            // These generate BSC (Branch or Skip on Condition) instructions with preset condition codes
            // BSC long format INVERTS the condition: branches when TestCondition returns FALSE
            // Strategy: To "Branch if X", test for "NOT X" so TestCondition returns FALSE, then inverts to TRUE
            else if (operation == "B")
            {
                // Unconditional branch: Test for all ACC conditions (Z|P|M)
                // Always at least one will be false, so TestCondition often returns TRUE
                // But we want ALWAYS branch, so use all 3: ensures condition logic always TRUE, inverted to FALSE... 
                // Wait, that's wrong. Let me think: We want to ALWAYS branch.
                // Set all ACC bits: if ACC is in any state, at least one matches, TestCondition=TRUE, inverted=FALSE, no branch?
                // Actually, for unconditional: any modifier will fail sometimes. Better to use modifier 0 and handle specially?
                // Or: ACC is always exactly ONE of Z, P, or M. So Z|P|M always has one TRUE, TestCondition=TRUE, inverted=FALSE.
                // That means we'd NEVER branch! We need TestCondition to return FALSE.
                // Unconditional branch needs to set NO condition bits, or use special BSC logic.
                // Actually reviewing user's opcode: B=0x4C0. That's modifier=0x00!
                // With modifier 0, TestCondition checks nothing, returns FALSE, inverted to TRUE → always branches! ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x00, "B");  // No conditions = always FALSE = always branch
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BP")
            {
                // Branch if Positive (ACC > 0): Test for Z|M (NOT positive)
                // When ACC>0: neither Z nor M match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When ACC≤0: Z or M matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x30, "BP");  // Z|M = 0x20|0x10
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BNP")
            {
                // Branch if Not Positive (ACC ≤ 0): Test for P
                // When ACC>0: P matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                // When ACC≤0: P doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x08, "BNP");  // P = 0x08
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BN")
            {
                // Branch if Negative (ACC < 0): Test for Z|P (NOT negative)
                // When ACC<0: neither Z nor P match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When ACC≥0: Z or P matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x28, "BN");  // Z|P = 0x20|0x08
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BNN")
            {
                // Branch if Not Negative (ACC ≥ 0): Test for M
                // When ACC<0: M matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                // When ACC≥0: M doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x10, "BNN");  // M = 0x10
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BZ")
            {
                // Branch if Zero (ACC == 0): Test for P|M (NOT zero)
                // When ACC=0: neither P nor M match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When ACC≠0: P or M matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x18, "BZ");  // P|M = 0x08|0x10
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BNZ")
            {
                // Branch if Not Zero (ACC ≠ 0): Test for Z
                // When ACC=0: Z matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                // When ACC≠0: Z doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x20, "BNZ");  // Z = 0x20
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BC")
            {
                // Branch on Carry OFF: Carry bit (0x02) in TestCondition checks for Carry OFF
                // When Carry ON: doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When Carry OFF: matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                // WAIT - this is backwards! BC should branch when Carry is ON, not OFF!
                // Need to rethink: TestCondition returns TRUE when Carry is OFF
                // Inverted: TRUE → FALSE, so NO branch when Carry is OFF
                // That means branch when Carry is ON, which is correct! ✓
                // Actually no... if Carry OFF returns TRUE, inverted is FALSE, no branch.
                // If Carry ON, doesn't match, returns FALSE, inverted is TRUE, branch! ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x02, "BC");  // C = 0x02
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BO")
            {
                // Branch on Overflow: Overflow bit (0x01) checks for Overflow OFF (see TestCondition)
                // When Overflow ON: doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When Overflow OFF: matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x01, "BO");  // O = 0x01
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "BOD")
            {
                // Branch if Odd: Even bit (0x04) checks for ACC Even (bit 15 = 0)
                // When ACC Odd: doesn't match, TestCondition=FALSE, inverted=TRUE, branch ✓
                // When ACC Even: matches, TestCondition=TRUE, inverted=FALSE, no branch ✓
                FormatListingLine(line);
                ProcessPseudoBranch(parts.Operand, 0x04, "BOD");  // E = 0x04
                AddInstructionListingLine(line, addressBefore);
            }
            else if (operation == "SKP")
            {
                // Skip on condition - short format only
                FormatListingLine(line);
                ProcessSkip(parts.Operand);
                AddInstructionListingLine(line, addressBefore);
            }
            else
            {
                _context.AddError(_currentLine, $"Unknown operation: {operation}");
            }
        }

        private void ProcessOrg(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing address in ORG directive");
                return;
            }

            if (!operand.StartsWith("/"))
            {
                _context.AddError(_currentLine, "ORG address must begin with /");
                return;
            }

            // Parse hex address
            if (!TryParseHex(operand.Substring(1), out ushort address))
            {
                _context.AddError(_currentLine, $"Invalid hex address in ORG: {operand}");
                return;
            }

            // Check for memory overflow
            if (address >= _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow");
                return;
            }

            _context.LocationCounter = address;
            _context.HasOrigin = true;
        }

        private void ProcessDc(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing value in DC directive");
                return;
            }

            if (_context.LocationCounter >= _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow");
                return;
            }

            // DC operand is just the first token (rest is inline comment)
            var firstToken = operand.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstToken))
            {
                _context.AddError(_currentLine, "Missing value in DC directive");
                return;
            }

            ushort value;
            
            // Handle hex value
            if (firstToken.StartsWith("/"))
            {
                if (!TryParseHex(firstToken.Substring(1), out value))
                {
                    _context.AddError(_currentLine, $"Invalid hex value in DC: {firstToken}");
                    return;
                }
            }
            // Handle decimal value
            else if (ushort.TryParse(firstToken, out value))
            {
                // Successfully parsed as decimal
            }
            // Handle symbol resolution
            else
            {
                value = ResolveOperand(firstToken);
                if (_context.Errors.Any())
                    return;
            }
            
            _cpu[_context.LocationCounter] = value;
            
            // Increment location counter AFTER storing the value
            _context.LocationCounter++;
        }

        private void ProcessBss(string operand)
        {
            // BSS - Block Started by Symbol
            // Reserves a block of storage WITHOUT clearing it
            // Label (if specified) refers to leftmost (first) word
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand in BSS directive");
                return;
            }

            var operandUpper = operand.Trim().ToUpper();
            bool evenAlign = false;
            string countStr = operandUpper;
            
            // Check for E modifier (even boundary alignment)
            if (operandUpper.StartsWith("E "))
            {
                evenAlign = true;
                countStr = operandUpper.Substring(2).Trim();
            }
            
            // Align to even boundary if requested
            if (evenAlign && (_context.LocationCounter % 2) != 0)
            {
                _context.LocationCounter++;
            }
            
            // Parse count (can be decimal, hex, or symbol)
            int count;
            if (countStr.StartsWith("/"))
            {
                if (TryParseHex(countStr.Substring(1), out ushort hexVal))
                    count = hexVal;
                else
                {
                    _context.AddError(_currentLine, $"Invalid hex value in BSS: {countStr}");
                    return;
                }
            }
            else if (int.TryParse(countStr, out count))
            {
                // Parsed as decimal number
            }
            else
            {
                // Try to resolve as symbol
                ushort symbolValue = ResolveOperand(countStr);
                if (_context.Errors.Any())
                    return;
                count = symbolValue;
            }
            
            // Check for overflow
            if (_context.LocationCounter + count > _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow in BSS");
                return;
            }
            
            // BSS does NOT clear memory - just reserves space
            // Per documentation: "it should not be assumed that an area reserved 
            // by a BSS instruction contains zeros"
            // However, for our emulator, we'll initialize to zero for deterministic behavior
            for (int i = 0; i < count; i++)
            {
                _cpu[_context.LocationCounter + i] = 0;
            }
            
            // Advance location counter
            _context.LocationCounter += count;
        }

        private void ProcessBes(string operand)
        {
            // BES - Block Ended by Symbol
            // Identical to BSS except label refers to rightmost word + 1
            // i.e., the next location available for assignment (after the reserved area)
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand in BES directive");
                return;
            }

            var operandUpper = operand.Trim().ToUpper();
            bool evenAlign = false;
            string countStr = operandUpper;
            
            // Check for E modifier (even boundary alignment)
            if (operandUpper.StartsWith("E "))
            {
                evenAlign = true;
                countStr = operandUpper.Substring(2).Trim();
            }
            
            // Align to even boundary if requested
            if (evenAlign && (_context.LocationCounter % 2) != 0)
            {
                _context.LocationCounter++;
            }
            
            // Parse count (can be decimal, hex, or symbol)
            int count;
            if (countStr.StartsWith("/"))
            {
                if (TryParseHex(countStr.Substring(1), out ushort hexVal))
                    count = hexVal;
                else
                {
                    _context.AddError(_currentLine, $"Invalid hex value in BES: {countStr}");
                    return;
                }
            }
            else if (int.TryParse(countStr, out count))
            {
                // Parsed as decimal number
            }
            else
            {
                // Try to resolve as symbol
                ushort symbolValue = ResolveOperand(countStr);
                if (_context.Errors.Any())
                    return;
                count = symbolValue;
            }
            
            // Check for overflow
            if (_context.LocationCounter + count > _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow in BES");
                return;
            }
            
            // Reserve storage (zero-fill the reserved area)
            for (int i = 0; i < count; i++)
            {
                _cpu[_context.LocationCounter + i] = 0;
            }
            
            // Advance location counter - label will point to this final value
            // (the next location after the reserved area)
            _context.LocationCounter += count;
        }

        private void ProcessLoad(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand in LD instruction");
                return;
            }

            var (isLong, isIndirect, tag, address) = ParseOperand(operand);
            if (_context.Errors.Any())
                return;

            // Build instruction: opcode 0x18
            if (isLong)
            {
                // Long format: 2 words
                if (_context.LocationCounter + 1 >= _cpu.MemorySize)
                {
                    _context.AddError(_currentLine, "Address overflow");
                    return;
                }
                
                ushort instruction = (ushort)((0x18 << 11) | (tag << 8) | 0x400);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _cpu[_context.LocationCounter + 1] = address;
                _context.LocationCounter += 2;
            }
            else
            {
                // Short format: 1 word, displacement relative to IAR
                int displacement = address - (_context.LocationCounter + 1);
                if (displacement < -128 || displacement > 127)
                {
                    _context.AddError(_currentLine, $"Displacement {displacement} out of range for short format (use 'L' for long format)");
                    return;
                }
                
                ushort instruction = (ushort)((0x18 << 11) | (tag << 8) | ((byte)displacement & 0xFF));
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _context.LocationCounter++;
            }
        }

        private void ProcessStore(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand in STO instruction");
                return;
            }

            var (isLong, isIndirect, tag, address) = ParseOperand(operand);
            if (_context.Errors.Any())
                return;

            // Build instruction: opcode 0x1A
            if (isLong)
            {
                // Long format: 2 words
                if (_context.LocationCounter + 1 >= _cpu.MemorySize)
                {
                    _context.AddError(_currentLine, "Address overflow");
                    return;
                }
                
                ushort instruction = (ushort)((0x1A << 11) | (tag << 8) | 0x400);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _cpu[_context.LocationCounter + 1] = address;
                _context.LocationCounter += 2;
            }
            else
            {
                // Short format: 1 word, displacement relative to IAR
                int displacement = address - (_context.LocationCounter + 1);
                if (displacement < -128 || displacement > 127)
                {
                    _context.AddError(_currentLine, $"Displacement {displacement} out of range for short format (use 'L' for long format)");
                    return;
                }
                
                ushort instruction = (ushort)((0x1A << 11) | (tag << 8) | ((byte)displacement & 0xFF));
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _context.LocationCounter++;
            }
        }

        private void ProcessArithmetic(string operand, byte opcode, string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, $"Missing operand in {mnemonic} instruction");
                return;
            }

            var (isLong, isIndirect, tag, address) = ParseOperand(operand);
            if (_context.Errors.Any())
                return;

            // Build instruction
            if (isLong)
            {
                // Long format: 2 words
                if (_context.LocationCounter + 1 >= _cpu.MemorySize)
                {
                    _context.AddError(_currentLine, "Address overflow");
                    return;
                }
                
                ushort instruction = (ushort)((opcode << 11) | (tag << 8) | 0x400);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _cpu[_context.LocationCounter + 1] = address;
                _context.LocationCounter += 2;
            }
            else
            {
                // Short format: 1 word, displacement relative to IAR
                int displacement = address - (_context.LocationCounter + 1);
                if (displacement < -128 || displacement > 127)
                {
                    _context.AddError(_currentLine, $"Displacement {displacement} out of range for short format (use 'L' for long format)");
                    return;
                }
                
                ushort instruction = (ushort)((opcode << 11) | (tag << 8) | ((byte)displacement & 0xFF));
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _context.LocationCounter++;
            }
        }

        private void ProcessLoadStatus(string operand)
        {
            // LDS is special - it takes an immediate value (0-3) in the displacement field
            // Bit 1 = Carry, Bit 0 = Overflow
            // The operand is the literal value to put in the displacement, not an address
            
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand in LDS instruction");
                return;
            }

            if (_context.LocationCounter >= _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow");
                return;
            }

            // Parse the immediate value (can be hex with / or decimal)
            ushort value;
            operand = operand.Trim();
            
            if (operand.StartsWith("/"))
            {
                // Hex format
                if (!TryParseHex(operand.Substring(1), out value))
                {
                    _context.AddError(_currentLine, $"Invalid hex value in LDS: {operand}");
                    return;
                }
            }
            else
            {
                // Decimal format
                if (!ushort.TryParse(operand, out value))
                {
                    _context.AddError(_currentLine, $"Invalid decimal value in LDS: {operand}");
                    return;
                }
            }

            // Value must be 0-3 (2 bits: carry and overflow)
            if (value > 3)
            {
                _context.AddError(_currentLine, $"LDS value must be 0-3, got {value}");
                return;
            }

            // Build instruction: OpCode 0x04 (LoadStatus), tag 0, displacement = value
            byte displacement = (byte)value;
            ushort instruction = (ushort)((0x04 << 11) | displacement);
            
            _cpu[_context.LocationCounter] = instruction;
            _context.LocationCounter++;
        }

        private void ProcessShift(string operand, byte opcode, byte shiftType, string mnemonic)
        {
            if (_context.LocationCounter >= _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow");
                return;
            }

            // Parse shift count (should be a decimal number)
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, $"Missing shift count in {mnemonic} instruction");
                return;
            }

            if (!byte.TryParse(operand.Trim(), out byte shiftCount))
            {
                _context.AddError(_currentLine, $"Invalid shift count in {mnemonic}: {operand}");
                return;
            }

            if (shiftCount > 63)
            {
                _context.AddError(_currentLine, $"Shift count must be 0-63, got {shiftCount}");
                return;
            }

            // Build short format shift instruction
            // Format: [5-bit opcode][2-bit tag=00][1-bit F=0][8-bit displacement]
            // Shift type is encoded in bits 6-7 of displacement (tag position for shifts)
            // Shift count is in low 6 bits of displacement
            ushort instruction = (ushort)((opcode << 11) | (shiftType << 6) | (shiftCount & 0x3F));
            _cpu[_context.LocationCounter] = instruction;
            
            _context.LocationCounter++;
        }

        private void ProcessBranch(string operand, byte opcode, string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, $"Missing operand in {mnemonic} instruction");
                return;
            }

            // Parse branch operand format: [L] [condition][,Xn] address [I]
            // Condition codes: O (overflow off), C (carry off), E (even), + or & (positive), - (negative), Z (zero)
            // Examples: 
            //   BSC O LOOP         - Short format, skip if overflow OFF
            //   BSC L O LOOP       - Long format, branch if overflow OFF  
            //   BSC L +,2 LOOP I   - Long format, indexed by XR2, indirect, branch if positive
            //   BSC ZPM LOOP       - Multiple conditions (zero, positive, or minus - always true)
            //   BSC LOOP           - Unconditional (no condition)
            
            bool isLong = false;
            bool isIndirect = false;
            byte tag = 0;
            string remainingOperand = operand.Trim();
            
            // Check for "L" prefix (long format)
            if (remainingOperand.ToUpper().StartsWith("L "))
            {
                isLong = true;
                remainingOperand = remainingOperand.Substring(2).TrimStart();
            }
            
            // Check for "I" suffix (indirect) - must be at the very end
            if (remainingOperand.ToUpper().EndsWith(" I"))
            {
                isIndirect = true;
                remainingOperand = remainingOperand.Substring(0, remainingOperand.Length - 2).TrimEnd();
            }
            
            // Now parse: [condition][,Xn] address
            // Strategy: Look for space-separated parts
            var parts = remainingOperand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            string conditions = "";
            string addressPart = "";
            
            if (parts.Length == 1)
            {
                // Single part: unconditional branch (just address)
                addressPart = parts[0];
                conditions = "";
            }
            else if (parts.Length == 2)
            {
                // Two parts: "condition address" or just address if first part isn't a condition
                // Check if first part contains only valid condition characters
                // Accept both traditional (P, M) and new (+, -, &) syntax
                string firstPart = parts[0].ToUpper();
                string firstNoComma = firstPart.Contains(',') ? firstPart.Split(',')[0] : firstPart;
                
                bool isCondition = firstNoComma.Length > 0;
                foreach (char c in firstNoComma)
                {
                    if (c != 'O' && c != 'C' && c != 'E' && 
                        c != '+' && c != '&' && c != '-' && 
                        c != 'P' && c != 'M' &&  // Traditional syntax
                        c != 'Z')
                    {
                        isCondition = false;
                        break;
                    }
                }
                
                if (isCondition)
                {
                    conditions = firstPart;
                    addressPart = parts[1];
                }
                else
                {
                    // Not a condition - this is an error
                    _context.AddError(_currentLine, $"{mnemonic} invalid condition codes '{parts[0]}'");
                    return;
                }
            }
            else
            {
                _context.AddError(_currentLine, $"{mnemonic} operand format should be: [L] [condition][,Xn] address [I]");
                return;
            }
            
            // Parse condition codes (may include index register)
            // Check if conditions part has ",Xn" suffix
            if (conditions.Contains(","))
            {
                var condParts = conditions.Split(',');
                conditions = condParts[0];  // Actual conditions
                
                // Parse index register
                var xreg = condParts[1].Trim();
                if (xreg == "1" || xreg == "X1") tag = 1;
                else if (xreg == "2" || xreg == "X2") tag = 2;
                else if (xreg == "3" || xreg == "X3") tag = 3;
                else
                {
                    _context.AddError(_currentLine, $"Invalid index register: {xreg}");
                    return;
                }
            }
            
            // Parse condition codes into modifier byte
            byte modifiers = 0;
            foreach (char c in conditions)
            {
                switch (c)
                {
                    case 'Z': modifiers |= 0x20; break; // Zero
                    case '+':                            // Plus (positive)
                    case '&':                            // & is alternate for +
                    case 'P': modifiers |= 0x08; break; // P is traditional syntax for plus
                    case '-':                            // Minus (negative)
                    case 'M': modifiers |= 0x10; break; // M is traditional syntax for minus
                    case 'O': modifiers |= 0x01; break; // Overflow off
                    case 'C': modifiers |= 0x02; break; // Carry off
                    case 'E': modifiers |= 0x04; break; // Even
                    case ' ': break;                     // Ignore spaces
                    default:
                        _context.AddError(_currentLine, $"Invalid condition code '{c}' in {mnemonic}. Valid: O C E + & - Z");
                        return;
                }
            }
            
            ushort address = ResolveOperand(addressPart);
            if (_context.Errors.Any())
                return;

            // Build instruction
            if (isLong)
            {
                // Long format: 2 words
                if (_context.LocationCounter + 1 >= _cpu.MemorySize)
                {
                    _context.AddError(_currentLine, "Address overflow");
                    return;
                }
                
                ushort instruction = (ushort)((opcode << 11) | (tag << 8) | modifiers | 0x400);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _cpu[_context.LocationCounter + 1] = address;
                _context.LocationCounter += 2;
            }
            else
            {
                // Short format: 1 word, displacement relative to IAR
                // Note: For BSC short format, displacement field contains modifiers, not an address offset
                // Short format BSC skips next instruction if condition is met
                int displacement = address - (_context.LocationCounter + 1);
                if (displacement < -128 || displacement > 127)
                {
                    _context.AddError(_currentLine, $"Displacement {displacement} out of range for short format (use 'L' for long format)");
                    return;
                }
                
                ushort instruction = (ushort)((opcode << 11) | (tag << 8) | modifiers);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _context.LocationCounter++;
            }
        }

        private void ProcessWait()
        {
            if (_context.LocationCounter >= _cpu.MemorySize)
            {
                _context.AddError(_currentLine, "Address overflow");
                return;
            }

            // Build short format WAIT instruction: opcode 0x06, tag 0, displacement 0
            ushort instruction = (ushort)(0x06 << 11); // OpCode WAIT
            _cpu[_context.LocationCounter] = instruction;
            
            _context.LocationCounter++;
        }

        private void ProcessPseudoBranch(string operand, byte modifiers, string mnemonic)
        {
            // Pseudo branch operations: B, BP, BNP, BN, BNN, BZ, BNZ, BC, BO, BOD
            // These generate BSC instructions with specific condition codes
            // The modifiers parameter contains the inverted condition bits for the BSC instruction
            
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, $"Missing operand in {mnemonic} instruction");
                return;
            }

            // Parse operand: [L] address [I] or [L] address,Xn [I]
            // Note: No condition codes in operand for pseudo-ops
            bool isLong = false;
            bool isIndirect = false;
            byte tag = 0;
            string remainingOperand = operand.Trim();
            
            // Check for "L" prefix (long format)
            if (remainingOperand.ToUpper().StartsWith("L "))
            {
                isLong = true;
                remainingOperand = remainingOperand.Substring(2).TrimStart();
            }
            
            // Check for "I" suffix (indirect)
            if (remainingOperand.ToUpper().EndsWith(" I"))
            {
                isIndirect = true;
                remainingOperand = remainingOperand.Substring(0, remainingOperand.Length - 2).TrimEnd();
            }
            
            // Check for index register (,X1, ,X2, ,X3 or ,1, ,2, ,3)
            string addressPart = remainingOperand;
            if (addressPart.Contains(","))
            {
                var parts = addressPart.Split(',');
                addressPart = parts[0].Trim();
                var xreg = parts[1].Trim().ToUpper();
                if (xreg == "1" || xreg == "X1") tag = 1;
                else if (xreg == "2" || xreg == "X2") tag = 2;
                else if (xreg == "3" || xreg == "X3") tag = 3;
                else
                {
                    _context.AddError(_currentLine, $"Invalid index register: {xreg}");
                    return;
                }
            }
            
            ushort address = ResolveOperand(addressPart);
            if (_context.Errors.Any())
                return;

            // Build BSC instruction with the specified modifiers
            byte bscOpcode = 0x09; // BSC opcode (BranchSkip)
            
            if (isLong)
            {
                // Long format: 2 words
                if (_context.LocationCounter + 1 >= _cpu.MemorySize)
                {
                    _context.AddError(_currentLine, "Address overflow");
                    return;
                }
                
                ushort instruction = (ushort)((bscOpcode << 11) | (tag << 8) | modifiers | 0x400);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _cpu[_context.LocationCounter + 1] = address;
                _context.LocationCounter += 2;
            }
            else
            {
                // Short format: skip instruction
                int displacement = address - (_context.LocationCounter + 1);
                if (displacement < -128 || displacement > 127)
                {
                    _context.AddError(_currentLine, $"Displacement {displacement} out of range for short format (use 'L' for long format)");
                    return;
                }
                
                ushort instruction = (ushort)((bscOpcode << 11) | (tag << 8) | modifiers);
                if (isIndirect) instruction |= 0x80;
                
                _cpu[_context.LocationCounter] = instruction;
                _context.LocationCounter++;
            }
        }

        private void ProcessSkip(string operand)
        {
            // SKP - Skip on condition (short format only)
            // Operand is just the condition codes: O, C, E, +, -, Z, P, M
            
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing condition codes in SKP instruction");
                return;
            }

            string conditions = operand.Trim().ToUpper();
            
            // Parse condition codes into modifier byte
            byte modifiers = 0;
            foreach (char c in conditions)
            {
                switch (c)
                {
                    case 'Z': modifiers |= 0x20; break; // Zero
                    case '+':
                    case '&':
                    case 'P': modifiers |= 0x08; break; // Plus/Positive
                    case '-':
                    case 'M': modifiers |= 0x10; break; // Minus/Negative
                    case 'O': modifiers |= 0x01; break; // Overflow off
                    case 'C': modifiers |= 0x02; break; // Carry off
                    case 'E': modifiers |= 0x04; break; // Even
                    case ' ': break; // Ignore spaces
                    default:
                        _context.AddError(_currentLine, $"Invalid condition code '{c}' in SKP. Valid: O C E + & - Z P M");
                        return;
                }
            }

            // Generate short format BSC instruction (skip next instruction if condition true)
            byte bscOpcode = 0x09; // BSC opcode (BranchSkip)
            ushort instruction = (ushort)((bscOpcode << 11) | modifiers);
            
            _cpu[_context.LocationCounter] = instruction;
            _context.LocationCounter++;
        }

        /// <summary>
        /// Parse an operand with all modifiers supporting both modern and DMS syntax:
        /// Modern: . BIT, L BIT, 1 BIT, L1 BIT, I BIT, I1 BIT
        /// DMS: BIT, L BIT, 1 BIT, L1 BIT, I BIT, I1 BIT
        /// Returns: (isLong, isIndirect, tag, address)
        /// </summary>
        private (bool isLong, bool isIndirect, byte tag, ushort address) ParseOperand(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand");
                return (false, false, 0, 0);
            }

            var tokens = operand.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            bool isLong = false;
            bool isIndirect = false;
            byte tag = 0;
            string addressToken = "";
            
            if (tokens.Length == 0)
            {
                _context.AddError(_currentLine, "Missing operand");
                return (false, false, 0, 0);
            }
            
            // Parse format specifier (first token)
            var firstToken = tokens[0].ToUpper();
            
            // Check for format specifier patterns (must be EXACT match to avoid false positives)
            if (firstToken == ".")
            {
                // Modern syntax: explicit short format
                isLong = false;
                tag = 0;
                addressToken = tokens.Length > 1 ? tokens[1] : "";
            }
            else if (firstToken == "L")
            {
                // Long format with separate space: "L ADDRESS"
                isLong = true;
                tag = 0;
                addressToken = tokens.Length > 1 ? tokens[1] : "";
                // Check for trailing "I" for indirect
                if (tokens.Length > 2 && tokens[2].ToUpper() == "I")
                {
                    isIndirect = true;
                }
            }
            else if (firstToken == "1" || firstToken == "2" || firstToken == "3")
            {
                // Short format with index register: "1 ADDRESS", "2 ADDRESS", "3 ADDRESS"
                tag = byte.Parse(firstToken);
                addressToken = tokens.Length > 1 ? tokens[1] : "";
            }
            else if (firstToken == "L1" || firstToken == "L2" || firstToken == "L3")
            {
                // Long format with index (combined): "L1 ADDRESS", "L2 ADDRESS", "L3 ADDRESS"
                isLong = true;
                tag = byte.Parse(firstToken.Substring(1));
                addressToken = tokens.Length > 1 ? tokens[1] : "";
            }
            else if (firstToken == "I")
            {
                // Indirect addressing: "I ADDRESS" or "I L ADDRESS"
                // Note: Indirect ALWAYS implies long format (bit 8 only exists in long format)
                isIndirect = true;
                isLong = true;  // Indirect forces long format
                tag = 0;
                // Check if next token is "L" (redundant but allowed)
                if (tokens.Length > 1 && tokens[1].ToUpper() == "L")
                {
                    addressToken = tokens.Length > 2 ? tokens[2] : "";
                }
                else
                {
                    addressToken = tokens.Length > 1 ? tokens[1] : "";
                }
            }
            else if (firstToken == "I1" || firstToken == "I2" || firstToken == "I3")
            {
                // Indirect with index (combined): "I1 ADDRESS", "I2 ADDRESS", "I3 ADDRESS"
                // Note: Indirect ALWAYS implies long format
                isIndirect = true;
                isLong = true;  // Indirect forces long format
                tag = byte.Parse(firstToken.Substring(1));
                addressToken = tokens.Length > 1 ? tokens[1] : "";
            }
            else if (firstToken == "IL" || firstToken == "IL1" || firstToken == "IL2" || firstToken == "IL3")
            {
                // Indirect long (combined): "IL ADDRESS", "IL1 ADDRESS", "IL2 ADDRESS", "IL3 ADDRESS"
                // Note: "L" is redundant since indirect always implies long, but accept it for clarity
                isIndirect = true;
                isLong = true;  // Indirect forces long format
                if (firstToken.Length > 2)
                {
                    tag = byte.Parse(firstToken.Substring(2));
                }
                addressToken = tokens.Length > 1 ? tokens[1] : "";
            }
            else
            {
                // No format specifier - DMS style with just address
                // This is the default case for authentic DMS code
                addressToken = firstToken;
                
                // Check for trailing "I" for indirect (legacy DMS syntax: "ADDRESS I")
                if (tokens.Length > 1 && tokens[1].ToUpper() == "I")
                {
                    isIndirect = true;
                }
            }
            
            // Check for legacy comma-based index register syntax (,X1, ,X2, ,X3)
            if (addressToken.ToUpper().Contains(",X"))
            {
                var parts = addressToken.Split(',');
                if (parts.Length == 2)
                {
                    var xreg = parts[1].Trim().ToUpper();
                    if (xreg == "X1") tag = 1;
                    else if (xreg == "X2") tag = 2;
                    else if (xreg == "X3") tag = 3;
                    else
                    {
                        _context.AddError(_currentLine, $"Invalid index register: {xreg}");
                        return (isLong, isIndirect, 0, 0);
                    }
                    addressToken = parts[0].Trim();
                }
            }
            
            ushort address = ResolveOperand(addressToken);
            return (isLong, isIndirect, tag, address);
        }

        private ushort ResolveOperand(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Empty operand");
                return 0;
            }

            operand = operand.Trim();

            // Handle * (current address) and simple expressions with *
            if (operand.Contains("*"))
            {
                // Handle simple expression: *-* (current address minus current address = 0)
                if (operand == "*-*")
                {
                    return 0;
                }
                
                // Handle single * (current address)
                if (operand == "*")
                {
                    return (ushort)_context.LocationCounter;
                }
                
                // Could add more complex expression support here (e.g., *+5, *-10)
                // For now, handle basic arithmetic with *
                if (operand.Contains("+"))
                {
                    var parts = operand.Split('+');
                    if (parts.Length == 2 && parts[0].Trim() == "*")
                    {
                        if (ushort.TryParse(parts[1].Trim(), out ushort offset))
                        {
                            return (ushort)(_context.LocationCounter + offset);
                        }
                    }
                }
                else if (operand.Contains("-"))
                {
                    var parts = operand.Split('-');
                    if (parts.Length == 2 && parts[0].Trim() == "*")
                    {
                        if (ushort.TryParse(parts[1].Trim(), out ushort offset))
                        {
                            return (ushort)(_context.LocationCounter - offset);
                        }
                    }
                }
                
                _context.AddError(_currentLine, $"Invalid expression with *: {operand}");
                return 0;
            }

            // Handle hex address (/500)
            if (operand.StartsWith("/"))
            {
                if (!TryParseHex(operand.Substring(1), out ushort address))
                {
                    _context.AddError(_currentLine, $"Invalid hex address: {operand}");
                    return 0;
                }
                return address;
            }

            // Try to parse as decimal number
            if (ushort.TryParse(operand, out ushort numValue))
            {
                return numValue;
            }

            // Handle label reference
            var label = operand.ToUpper();
            if (_context.Symbols.TryGetValue(label, out ushort symbolAddress))
            {
                return symbolAddress;
            }

            _context.AddError(_currentLine, $"Undefined symbol: {operand}");
            return 0;
        }

        private void FormatListingLine(string line)
        {
            // IBM 1130 listing format:
            // - Lines without labels (start with whitespace): location + 7 spaces + trimmed line
            // - Lines with labels or comments: location + 2 spaces + original line
            var hasLabel = line.Length > 0 && !char.IsWhiteSpace(line[0]);
            
            if (hasLabel)
            {
                // Has label or is comment - use 2 spaces, keep original line
                _context.AddListingLine($"{_context.LocationCounter:X4}  {line}");
            }
            else
            {
                // No label - use 7 spaces, trim leading whitespace
                _context.AddListingLine($"{_context.LocationCounter:X4}       {line.TrimStart()}");
            }
        }

        private void AddStructuredListingLine(string line, ushort address, ushort? opCode)
        {
            _context.AddListingLine(_currentLine, address, opCode, line);
        }

        private void AddInstructionListingLine(string line, ushort address)
        {
            // Capture the opcode from memory after instruction was written
            ushort? opCode = null;
            if (_context.HasOrigin && address < _cpu.MemorySize)
            {
                opCode = _cpu[address];
            }
            _context.AddListingLine(_currentLine, address, opCode, line);
        }

        private static bool TryParseHex(string hex, out ushort result)
        {
            result = 0;
            try
            {
                result = Convert.ToUInt16(hex, 16);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
