namespace S1130.WebApi.Models;

/// <summary>
/// Represents a single line in the assembly listing
/// </summary>
public class ListingLineDto
{
    /// <summary>
    /// 1-based source line number
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Memory address (hex string, 4 digits)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Instruction opcode (hex string, 4 digits, null for comments/directives)
    /// </summary>
    public string? OpCode { get; set; }

    /// <summary>
    /// Original source code line
    /// </summary>
    public string SourceCode { get; set; } = string.Empty;
}

/// <summary>
/// Response from assembling IBM 1130 assembler source code
/// </summary>
public class AssembleResponse
{
    /// <summary>
    /// Indicates whether assembly was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// List of error messages (with line numbers if applicable)
    /// Empty if Success is true
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// The memory address where code was loaded (if successful)
    /// </summary>
    public ushort? LoadedAddress { get; set; }
    
    /// <summary>
    /// Number of words (instructions/data) loaded into memory
    /// </summary>
    public int WordsLoaded { get; set; }

    /// <summary>
    /// Structured assembly listing with line numbers, addresses, and opcodes
    /// </summary>
    public List<ListingLineDto> ListingLines { get; set; } = new();
}
