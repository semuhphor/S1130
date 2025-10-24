namespace S1130.WebApi.Models;

/// <summary>
/// Request to assemble IBM 1130 assembler source code
/// </summary>
public class AssembleRequest
{
    /// <summary>
    /// The IBM 1130 assembler source code to compile
    /// </summary>
    public required string SourceCode { get; set; }
    
    /// <summary>
    /// Optional starting address for assembled code (defaults to 0)
    /// </summary>
    public ushort? StartAddress { get; set; }
}
