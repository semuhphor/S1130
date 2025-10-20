namespace S1130.WebApi.Models;

/// <summary>
/// Response containing execution status and current CPU state
/// </summary>
public class ExecutionStatusDto
{
    /// <summary>
    /// Indicates whether the CPU is currently running
    /// </summary>
    public bool IsRunning { get; set; }
    
    /// <summary>
    /// Current state of the CPU
    /// </summary>
    public required CpuStateDto CpuState { get; set; }
}
