namespace S1130.WebApi.Models;

/// <summary>
/// Data Transfer Object representing the current state of the IBM 1130 CPU
/// </summary>
public class CpuStateDto
{
    /// <summary>
    /// Instruction Address Register (program counter)
    /// </summary>
    public ushort Iar { get; set; }
    
    /// <summary>
    /// Accumulator register (16-bit)
    /// </summary>
    public ushort Acc { get; set; }
    
    /// <summary>
    /// Extension register (16-bit, used with Acc for 32-bit operations)
    /// </summary>
    public ushort Ext { get; set; }
    
    /// <summary>
    /// Index Register 1
    /// </summary>
    public ushort Xr1 { get; set; }
    
    /// <summary>
    /// Index Register 2
    /// </summary>
    public ushort Xr2 { get; set; }
    
    /// <summary>
    /// Index Register 3
    /// </summary>
    public ushort Xr3 { get; set; }
    
    /// <summary>
    /// Current interrupt level being serviced (0-5), or -1 if none
    /// </summary>
    public int CurrentInterruptLevel { get; set; }
    
    /// <summary>
    /// Carry flag
    /// </summary>
    public bool Carry { get; set; }
    
    /// <summary>
    /// Overflow flag
    /// </summary>
    public bool Overflow { get; set; }
    
    /// <summary>
    /// Wait flag (true if CPU is in WAIT state)
    /// </summary>
    public bool Wait { get; set; }
    
    /// <summary>
    /// Total number of instructions executed
    /// </summary>
    public long InstructionCount { get; set; }
    
    /// <summary>
    /// Timestamp when this state was captured
    /// </summary>
    public DateTime Timestamp { get; set; }
}
