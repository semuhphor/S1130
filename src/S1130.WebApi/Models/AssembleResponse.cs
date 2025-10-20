namespace S1130.WebApi.Models;

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
}
