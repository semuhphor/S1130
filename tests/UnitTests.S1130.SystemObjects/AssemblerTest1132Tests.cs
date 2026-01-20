using S1130.SystemObjects;
using S1130.SystemObjects.Assembler;
using System.Linq;
using Xunit;

namespace UnitTests.S1130.SystemObjects;

public class AssemblerTest1132Tests
{
    [Fact]
    public void Test1132Program_ShouldAssembleSuccessfully()
    {
        // Read the test1132.s1130 file
        var filePath = System.IO.Path.Combine("..", "..", "..", "..", "..", "docs", "TestAsm", "test1132.s1130");
        var code = System.IO.File.ReadAllText(filePath);
        
        // Assemble using Cpu.Assemble
        var cpu = new Cpu();
        var result = cpu.Assemble(code);

        // Check for success
        Assert.True(result.Success, 
            $"Assembly failed with {result.Errors.Count} errors:\n" +
            string.Join("\n", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}")));

        Assert.NotEmpty(result.GeneratedWords);
        Assert.NotEmpty(result.Symbols);
    }
}
