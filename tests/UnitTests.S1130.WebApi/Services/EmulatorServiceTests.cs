using S1130.SystemObjects;
using S1130.WebApi.Models;
using S1130.WebApi.Services;
using Xunit;

namespace UnitTests.S1130.WebApi.Services;

public class EmulatorServiceTests
{
    [Fact]
    public void GetState_ReturnsCurrentCpuState()
    {
        // Arrange
        var service = new EmulatorService();
        
        // Act
        var state = service.GetState();
        
        // Assert
        Assert.NotNull(state);
        Assert.Equal(0, state.Iar);
        Assert.Equal(0, state.Acc);
        Assert.Equal(0, state.Ext);
        Assert.Equal(0, state.Xr1);
        Assert.Equal(0, state.Xr2);
        Assert.Equal(0, state.Xr3);
        Assert.Equal(-1, state.CurrentInterruptLevel); // No interrupt
        Assert.False(state.Carry);
        Assert.False(state.Overflow);
        Assert.False(state.Wait);
    }
    
    [Fact]
    public void Reset_ClearsCpuState()
    {
        // Arrange
        var service = new EmulatorService();
        var result = service.Assemble("       ORG  /10\n       DC   5");
        Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors)}");
        
        // Act - Reset should clear everything
        service.Reset();
        var state = service.GetState();
        
        // Assert
        Assert.Equal(0, state.Iar);
        Assert.Equal(0, state.InstructionCount);
    }
    
    [Fact]
    public void Assemble_WithValidCode_ReturnsSuccess()
    {
        // Arrange
        var service = new EmulatorService();
        string validCode = @"       ORG  /100
START  LD   L 1 DATA
       A    L 1 DATA
       STO  L 1 RESULT
       WAIT
DATA   DC   5
RESULT BSS  1";
        
        // Act
        var result = service.Assemble(validCode);
        
        // Assert
        Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors)}");
        Assert.Empty(result.Errors);
        Assert.NotNull(result.LoadedAddress);
    }
    
    [Fact]
    public void Assemble_WithInvalidCode_ReturnsErrors()
    {
        // Arrange
        var service = new EmulatorService();
        string invalidCode = "INVALID_OPCODE 1 2 3";
        
        // Act
        var result = service.Assemble(invalidCode);
        
        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }
    
    [Fact]
    public void ExecuteStep_ExecutesOneInstruction()
    {
        // Arrange
        var service = new EmulatorService();
        string code = @"       ORG  /100
       LD   L DATA
       WAIT
DATA   DC   10";
        var assembleResult = service.Assemble(code);
        Assert.True(assembleResult.Success, $"Assembly failed: {string.Join(", ", assembleResult.Errors)}");
        
        // Verify IAR is at starting address
        var initialState = service.GetState();
        Assert.Equal((ushort)0x100, initialState.Iar); //  Should start at ORG address
        Assert.Equal(0, initialState.InstructionCount); // No instructions executed yet
        
        // Act
        var stateAfterStep = service.ExecuteStep();
        
        // Assert
        Assert.Equal(1L, stateAfterStep.InstructionCount); // One instruction executed
        Assert.NotEqual((ushort)0x100, stateAfterStep.Iar); // IAR should have moved forward
        Assert.Equal(10, stateAfterStep.Acc); // Should have loaded value 10
    }
    
    [Fact]
    public void IsRunning_InitiallyReturnsFalse()
    {
        // Arrange
        var service = new EmulatorService();
        
        // Act
        bool isRunning = service.IsRunning;
        
        // Assert
        Assert.False(isRunning);
    }
    
    [Fact]
    public void StartExecution_SetsIsRunningTrue()
    {
        // Arrange
        var service = new EmulatorService();
        // Simple infinite loop - BSC with ZPM conditions (zero, plus, or minus - always true)
        string code = @"       ORG  /100
LOOP   LD   L DATA
       BSC  L ZPM LOOP
DATA   DC   1";
        var assembleResult = service.Assemble(code);
        Assert.True(assembleResult.Success, $"Assembly failed: {string.Join(", ", assembleResult.Errors)}");
        
        // Act
        service.StartExecution(1000); // 1000 instructions per second
        
        // Assert
        Assert.True(service.IsRunning);
        
        // Cleanup
        service.StopExecution();
    }
    
    [Fact]
    public void StopExecution_SetsIsRunningFalse()
    {
        // Arrange
        var service = new EmulatorService();
        string code = @"       ORG  /100
LOOP   LD   L DATA
       BSC  L ZPM LOOP
DATA   DC   1";
        var assembleResult = service.Assemble(code);
        Assert.True(assembleResult.Success, $"Assembly failed: {string.Join(", ", assembleResult.Errors)}");
        service.StartExecution(1000);
        Assert.True(service.IsRunning);
        
        // Act
        service.StopExecution();
        
        // Assert
        Assert.False(service.IsRunning);
    }
    
    [Fact]
    public void StartExecution_StopsOnWait()
    {
        // Arrange
        var service = new EmulatorService();
        string code = @"       ORG  /100
       LD   L DATA
       WAIT
DATA   DC   5";
        var assembleResult = service.Assemble(code);
        Assert.True(assembleResult.Success, $"Assembly failed: {string.Join(", ", assembleResult.Errors)}");
        
        // Act
        service.StartExecution(10000); // High speed to finish quickly
        
        // Wait for execution to complete (WAIT instruction)
        Thread.Sleep(100);
        
        // Assert
        var state = service.GetState();
        Assert.False(service.IsRunning); // Should have stopped on WAIT
        Assert.True(state.Wait); // Should be in WAIT state
    }
    
    [Fact]
    public void MultipleThreads_CanAccessServiceSafely()
    {
        // Arrange
        var service = new EmulatorService();
        var exceptions = new List<Exception>();
        
        // Act - Multiple threads trying to get state simultaneously
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    var state = service.GetState();
                    Assert.NotNull(state);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();
        
        Task.WaitAll(tasks);
        
        // Assert - No exceptions should have occurred
        Assert.Empty(exceptions);
    }
}
