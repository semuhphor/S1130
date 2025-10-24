using S1130.SystemObjects;
using S1130.WebApi.Models;
using System.Threading;

namespace S1130.WebApi.Services;

/// <summary>
/// Service for managing the IBM 1130 emulator instance
/// This is a singleton service that maintains a single CPU instance
/// </summary>
public class EmulatorService
{
    private ICpu _cpu;
    private S1130.SystemObjects.Assembler.Assembler _assembler;
    private readonly object _lock = new();
    private CancellationTokenSource? _executionCts;
    private Task? _executionTask;
    private bool _isRunning;

    public EmulatorService()
    {
        _cpu = new Cpu();
        _assembler = new S1130.SystemObjects.Assembler.Assembler();
    }

    /// <summary>
    /// Gets the current state of the CPU
    /// </summary>
    public CpuStateDto GetState()
    {
        lock (_lock)
        {
            return new CpuStateDto
            {
                Iar = _cpu.Iar,
                Acc = _cpu.Acc,
                Ext = _cpu.Ext,
                Xr1 = _cpu.Xr[1],
                Xr2 = _cpu.Xr[2],
                Xr3 = _cpu.Xr[3],
                CurrentInterruptLevel = _cpu.CurrentInterruptLevel ?? -1,
                Carry = _cpu.Carry,
                Overflow = _cpu.Overflow,
                Wait = _cpu.Wait,
                InstructionCount = (long)_cpu.InstructionCount,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Resets the CPU to initial state
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            StopExecution();
            _cpu = new Cpu();
            _assembler = new S1130.SystemObjects.Assembler.Assembler();
        }
    }

    /// <summary>
    /// Assembles IBM 1130 assembler source code and loads it into memory
    /// </summary>
    /// <param name="sourceCode">The assembler source code</param>
    /// <param name="startAddress">Optional starting address (defaults to first ORG directive)</param>
    /// <returns>Assembly result with success status and any errors</returns>
    public AssembleResponse Assemble(string sourceCode, ushort? startAddress = null)
    {
        lock (_lock)
        {
            StopExecution();
            
            // Split source code into lines
            var sourceLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var result = _assembler.Assemble(sourceLines);
            
            var response = new AssembleResponse
            {
                Success = result.Success,
                Errors = result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}").ToList(),
                // TODO: Generate proper listing lines when assembler supports it
                ListingLines = new List<ListingLineDto>()
            };

            if (result.Success)
            {
                // Load generated code into memory at start address
                var loadAddress = startAddress ?? (ushort)result.StartAddress;
                for (int i = 0; i < result.GeneratedWords.Length; i++)
                {
                    _cpu[loadAddress + i] = result.GeneratedWords[i];
                }
                
                // Set IAR to the starting address
                _cpu.Iar = loadAddress;
                response.LoadedAddress = loadAddress;
                response.WordsLoaded = result.GeneratedWords.Length;
            }

            return response;
        }
    }

    /// <summary>
    /// Executes a single instruction
    /// </summary>
    /// <returns>CPU state after executing the instruction</returns>
    public CpuStateDto ExecuteStep()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                // Clear wait state to allow execution
                _cpu.Wait = false;
                
                _cpu.NextInstruction();  // Fetch and decode the instruction
                _cpu.ExecuteInstruction();  // Execute it
            }
            return GetState();
        }
    }

    /// <summary>
    /// Starts continuous execution at the specified speed
    /// </summary>
    /// <param name="instructionsPerSecond">Target execution speed (instructions per second)</param>
    public void StartExecution(int instructionsPerSecond = 5)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return; // Already running
            }

            // Clear wait state to allow execution to start
            _cpu.Wait = false;

            _isRunning = true;
            _executionCts = new CancellationTokenSource();
            var token = _executionCts.Token;

            _executionTask = Task.Run(() =>
            {
                var delayMs = Math.Max(1, 1000 / instructionsPerSecond);
                
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        bool shouldStop = false;
                        
                        lock (_lock)
                        {
                            if (_cpu.Wait)
                            {
                                shouldStop = true;
                            }
                            else
                            {
                                _cpu.NextInstruction();  // Fetch and decode
                                _cpu.ExecuteInstruction();  // Execute
                                
                                // Check if we hit WAIT after execution
                                if (_cpu.Wait)
                                {
                                    shouldStop = true;
                                }
                            }
                        }
                        
                        if (shouldStop)
                        {
                            break;
                        }
                        
                        // Throttle execution
                        if (delayMs > 0)
                        {
                            Thread.Sleep(delayMs);
                        }
                    }
                }
                finally
                {
                    lock (_lock)
                    {
                        _isRunning = false;
                    }
                }
            }, token);
        }
    }

    /// <summary>
    /// Stops continuous execution
    /// </summary>
    public void StopExecution()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            _executionCts?.Cancel();
            _isRunning = false;
        }
        
        // Wait for the task to complete outside the lock
        try
        {
            _executionTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Task was cancelled, which is expected
        }
        
        _executionCts?.Dispose();
        _executionCts = null;
        _executionTask = null;
    }

    /// <summary>
    /// Indicates whether the CPU is currently running
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_lock)
            {
                return _isRunning;
            }
        }
    }
}
