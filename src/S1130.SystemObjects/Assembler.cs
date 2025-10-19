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
                
                foreach (var line in _context.SourceLines)
                {
                    _currentLine++;
                    ProcessLine(line);
                    if (_context.Errors.Any())
                        break;
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

        private void ProcessLine(string line)
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

            // Handle hex value
            if (operand.StartsWith("/"))
            {
                if (!TryParseHex(operand.Substring(1), out ushort value))
                {
                    _context.AddError(_currentLine, $"Invalid hex value in DC: {operand}");
                    return;
                }
                _cpu[_context.LocationCounter] = value;
            }
            // Handle decimal value
            else if (!ushort.TryParse(operand, out ushort value))
            {
                _context.AddError(_currentLine, $"Invalid decimal value in DC: {operand}");
                return;
            }
            else
            {
                _cpu[_context.LocationCounter] = value;
            }
            
            // Increment location counter AFTER storing the value
            _context.LocationCounter++;
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
