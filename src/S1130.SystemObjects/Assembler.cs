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
                
                return new AssemblyResult 
                { 
                    Success = success,
                    Errors = errors,
                    Listing = listing
                };
            }
            catch (Exception ex)
            {
                _context.AddError(_currentLine, $"Internal error: {ex.Message}");
                
                var success = false;
                var errors = _context.Errors.ToArray();
                var listing = _context.Listing.ToArray();
                
                return new AssemblyResult 
                { 
                    Success = success,
                    Errors = errors,
                    Listing = listing
                };
            }
        }

        private void Pass1ProcessLine(string line)
        {
            // Reject blank lines
            if (string.IsNullOrWhiteSpace(line))
            {
                _context.AddError(_currentLine, "Blank lines are not allowed");
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
                // Check if it has "L" prefix to determine size
                bool isLongFormat = parts.Operand?.Trim().ToUpper().StartsWith("L ") ?? false;
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else if (operation == "WAIT" || operation == "SLA" || operation == "SLT" || 
                     operation == "SLC" || operation == "SLCA" || operation == "SRA" || operation == "SRT")
            {
                // Short format: 1 word
                _context.LocationCounter++;
            }
            else if (operation == "BSC" || operation == "BSI" || operation == "MDX")
            {
                // Branch instructions can be short or long format
                bool isLongFormat = parts.Operand?.Trim().ToUpper().StartsWith("L ") ?? false;
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else if (operation == "XIO")
            {
                // XIO can be short or long format
                bool isLongFormat = parts.Operand?.Trim().ToUpper().StartsWith("L ") ?? false;
                _context.LocationCounter += isLongFormat ? 2 : 1;
            }
            else
            {
                _context.AddError(_currentLine, $"Unknown operation: {operation}");
            }
        }

        private void Pass2ProcessLine(string line)
        {
            // Reject blank lines
            if (string.IsNullOrWhiteSpace(line))
            {
                _context.AddError(_currentLine, "Blank lines are not allowed");
                return;
            }

            // Handle comments (lines starting with *)
            if (line.TrimStart().StartsWith("*"))
            {
                FormatListingLine(line);
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
            if (operation == "ORG")
            {
                ProcessOrg(parts.Operand);
                FormatListingLine(line);
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
            }
            else if (operation == "EQU")
            {
                // EQU just defines a symbol, no code generation
                FormatListingLine(line);
                // Symbol was already handled in Pass1
            }
            else if (operation == "BSS")
            {
                // BSS reserves storage, no initialization
                FormatListingLine(line);
                ProcessBss(parts.Operand);
            }
            else if (operation == "BES")
            {
                // BES ends a BSS block
                FormatListingLine(line);
                ProcessBes(parts.Operand);
            }
            else if (operation == "LD")
            {
                FormatListingLine(line);
                ProcessLoad(parts.Operand);
            }
            else if (operation == "STO")
            {
                FormatListingLine(line);
                ProcessStore(parts.Operand);
            }
            else if (operation == "LDD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x19, "LDD");
            }
            else if (operation == "STD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1B, "STD");
            }
            else if (operation == "LDX")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x0C, "LDX");
            }
            else if (operation == "STX")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x0D, "STX");
            }
            else if (operation == "A")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x10, "A");
            }
            else if (operation == "S")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x12, "S");
            }
            else if (operation == "M")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x14, "M");
            }
            else if (operation == "D")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x15, "D");
            }
            else if (operation == "AD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x11, "AD");
            }
            else if (operation == "SD")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x13, "SD");
            }
            else if (operation == "AND")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1C, "AND");
            }
            else if (operation == "OR")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1D, "OR");
            }
            else if (operation == "EOR")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x1E, "EOR");
            }
            else if (operation == "LDS")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x04, "LDS");
            }
            else if (operation == "STS")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x05, "STS");
            }
            else if (operation == "SLA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 0, "SLA"); // OpCode 0x02, shift type 0
            }
            else if (operation == "SLT")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 1, "SLT"); // OpCode 0x02, shift type 1
            }
            else if (operation == "SLC")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 2, "SLC"); // OpCode 0x02, shift type 2
            }
            else if (operation == "SLCA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x02, 3, "SLCA"); // OpCode 0x02, shift type 3
            }
            else if (operation == "SRA")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x03, 0, "SRA"); // OpCode 0x03, shift type 0
            }
            else if (operation == "SRT")
            {
                FormatListingLine(line);
                ProcessShift(parts.Operand, 0x03, 1, "SRT"); // OpCode 0x03, shift type 1
            }
            else if (operation == "BSC")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x08, "BSC"); // OpCode 0x08
            }
            else if (operation == "BSI")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x09, "BSI"); // OpCode 0x09
            }
            else if (operation == "MDX")
            {
                FormatListingLine(line);
                ProcessBranch(parts.Operand, 0x0A, "MDX"); // OpCode 0x0A
            }
            else if (operation == "XIO")
            {
                FormatListingLine(line);
                ProcessArithmetic(parts.Operand, 0x01, "XIO"); // OpCode 0x01
            }
            else if (operation == "WAIT")
            {
                FormatListingLine(line);
                ProcessWait();
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

            // Parse branch operand format: "condition_codes,address" or "L condition_codes,address"
            // Condition codes: Z (zero), P (plus), M (minus), O (overflow off), C (carry off), E (even)
            // Example: "BSC Z,LOOP" or "BSC L O,/500" or "BSC ZP,NEXT" (multiple conditions)
            
            bool isLong = false;
            string remainingOperand = operand.Trim();
            
            // Check for "L" prefix (long format)
            if (remainingOperand.ToUpper().StartsWith("L "))
            {
                isLong = true;
                remainingOperand = remainingOperand.Substring(2).TrimStart();
            }
            
            // Parse conditions and address
            var parts = remainingOperand.Split(',');
            if (parts.Length != 2)
            {
                _context.AddError(_currentLine, $"{mnemonic} operand must be in format 'conditions,address' or 'L conditions,address'");
                return;
            }
            
            // Parse condition codes into modifier byte
            byte modifiers = 0;
            var conditions = parts[0].Trim().ToUpper();
            foreach (char c in conditions)
            {
                switch (c)
                {
                    case 'Z': modifiers |= 0x20; break; // Zero
                    case 'P': modifiers |= 0x08; break; // Plus  
                    case 'M': modifiers |= 0x10; break; // Minus
                    case 'O': modifiers |= 0x01; break; // Overflow off
                    case 'C': modifiers |= 0x02; break; // Carry off
                    case 'E': modifiers |= 0x04; break; // Even
                    default:
                        _context.AddError(_currentLine, $"Invalid condition code '{c}' in {mnemonic}");
                        return;
                }
            }
            
            // Parse address with potential indirect and index register modifiers
            var addressPart = parts[1].Trim();
            bool isIndirect = false;
            byte tag = 0;
            
            // Check for "I" suffix (indirect)
            if (addressPart.ToUpper().EndsWith(" I"))
            {
                isIndirect = true;
                addressPart = addressPart.Substring(0, addressPart.Length - 2).TrimEnd();
            }
            
            // Check for index register suffix (,X1, ,X2, ,X3)
            if (addressPart.ToUpper().Contains(",X"))
            {
                var addressParts = addressPart.Split(',');
                if (addressParts.Length == 2)
                {
                    var xreg = addressParts[1].Trim().ToUpper();
                    if (xreg == "X1") tag = 1;
                    else if (xreg == "X2") tag = 2;
                    else if (xreg == "X3") tag = 3;
                    else
                    {
                        _context.AddError(_currentLine, $"Invalid index register: {xreg}");
                        return;
                    }
                    addressPart = addressParts[0].Trim();
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

        /// <summary>
        /// Parse an operand with all modifiers: L (long format), I (indirect), ,X1/X2/X3 (index registers)
        /// Returns: (isLong, isIndirect, tag, address)
        /// </summary>
        private (bool isLong, bool isIndirect, byte tag, ushort address) ParseOperand(string operand)
        {
            if (string.IsNullOrWhiteSpace(operand))
            {
                _context.AddError(_currentLine, "Missing operand");
                return (false, false, 0, 0);
            }

            // Instruction operands consist of:
            // [L] address[,X#][I]
            // Where address is a single token (symbol, hex, or number)
            // Anything after the operand is inline comment
            // Strategy: Take tokens intelligently based on format
            var tokens = operand.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            bool isLong = false;
            bool isIndirect = false;
            byte tag = 0;
            int operandTokenCount = 1;  // Default: just one token (the address)
            
            // Check for "L" prefix (long format)
            if (tokens[0].ToUpper() == "L")
            {
                isLong = true;
                operandTokenCount = 2;  // "L address"
            }
            
            // Check if last operand token is "I" (indirect)
            if (tokens.Length > operandTokenCount && tokens[operandTokenCount].ToUpper() == "I")
            {
                isIndirect = true;
                operandTokenCount++;  // Include the "I"
            }
            
            // Reconstruct operand from just the tokens we need (ignore inline comments)
            operand = string.Join(" ", tokens.Take(operandTokenCount));
            
            // Now strip the L prefix if present
            if (isLong)
            {
                operand = operand.Substring(2).TrimStart();
            }
            
            // Strip the I suffix if present  
            if (isIndirect && operand.ToUpper().EndsWith(" I"))
            {
                operand = operand.Substring(0, operand.Length - 2).TrimEnd();
            }
            
            // Check for index register suffix (,X1, ,X2, ,X3)
            if (operand.ToUpper().Contains(",X"))
            {
                var parts = operand.Split(',');
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
                    operand = parts[0].Trim();
                }
            }
            
            ushort address = ResolveOperand(operand);
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
