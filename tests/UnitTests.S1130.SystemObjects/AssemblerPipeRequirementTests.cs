using S1130.SystemObjects.Assembler;
using Xunit;

namespace UnitTests.S1130.SystemObjects;

/// <summary>
/// Tests to verify whether the S1130 assembler requires pipe delimiters
/// for format specifiers (e.g., |L|) or accepts IBM 1130 format without pipes.
/// </summary>
public class AssemblerPipeRequirementTests
{
    [Fact]
    public void Assembler_RequiresPipes_PipedFormatWorks()
    {
        // Arrange
        var assembler = new Assembler();
        var source = new[]
        {
            "       ORG  /0100",
            "       LD   |L| /0042"
        };
        
        // Act
        var result = assembler.Assemble(source);
        
        // Assert
        Assert.True(result.Success, result.GetErrorSummary());
        Assert.Empty(result.Errors);
        // ORG + LD L generates 2 words (address word + instruction word)
        Assert.Equal(2, result.GeneratedWords.Length);
    }
    
    [Fact]
    public void Assembler_WithoutPipes_DoesNotParseFormatCorrectly()
    {
        // Arrange - IBM 1130 format without pipes
        var assembler = new Assembler();
        var source = new[]
        {
            "       ORG  /0100",
            "       LD   L /0042"
        };
        
        // Act
        var result = assembler.Assemble(source);
        
        // Assert
        // Without pipes, "L /0042" is treated as operand, not format specifier
        // This will cause an assembly error - the assembler expects |L|
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }
    
    [Fact]
    public void Assembler_WithoutPipes_TreatsFormatCodeAsOperand()
    {
        // Arrange - IBM format: LD L VALUE
        var assembler = new Assembler();
        var source = new[]
        {
            "       ORG  /0100",
            "VALUE  EQU  /0042",
            "       LD   L VALUE"
        };
        
        // Act
        var result = assembler.Assemble(source);
        
        // Assert
        // "L VALUE" will be treated as operand, not format+operand
        // This is incorrect - S1130 requires pipes
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }
    
    [Fact]
    public void Assembler_CompareFormats_OnlyPipedWorks()
    {
        // Arrange - Correct S1130 format
        var correctAssembler = new Assembler();
        var correctSource = new[]
        {
            "       ORG  /0100",
            "       LD   |L| /0042"
        };
        
        // Arrange - IBM 1130 format (incorrect for S1130)
        var ibmAssembler = new Assembler();
        var ibmSource = new[]
        {
            "       ORG  /0100",
            "       LD   L /0042"
        };
        
        // Act
        var correctResult = correctAssembler.Assemble(correctSource);
        var ibmResult = ibmAssembler.Assemble(ibmSource);
        
        // Assert
        Assert.True(correctResult.Success); // Piped format works
        Assert.Empty(correctResult.Errors);
        
        Assert.False(ibmResult.Success); // Non-piped format fails
        Assert.NotEmpty(ibmResult.Errors);
    }
}
