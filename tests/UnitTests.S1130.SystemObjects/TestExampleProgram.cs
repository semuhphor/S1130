using Xunit;
using S1130.SystemObjects;
using System.Linq;

namespace UnitTests.S1130.SystemObjects
{
    public class TestExampleProgram
    {
        [Fact]
        public void ExampleProgramCompiles()
        {
            var cpu = new Cpu();
            
            var source = @"// IBM 1130 Shift Left Test Program
// Demonstrates SLT (Shift Left Together) with carry detection
// 
// This program loads 1 into ACC/EXT registers, then repeatedly
// shifts left until carry is set (bit shifts out), then restarts.
// Watch the bit travel through all 32 bits!
//
       ORG  /100
//
// Main program loop
//
START: LDD  |L|ONE       // Load double-word (0,1) into ACC and EXT
LOOP:  SLT  1            // Shift left together 1 bit
       BSC  |L|START,C   // Carry ON -- Set acc:ext = 1
       BSC  |L|LOOP      // else.. keep shifting.
//
// Data section
//
       BSS  |E|          // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            Assert.True(result.GeneratedWords.Length > 0);
        }

        [Fact]
        public void BssFormatsWork()
        {
            var cpu = new Cpu();
            
            // Test various BSS formats
            var source = @"
       ORG  /100
       BSS  100          // Reserve 100 words
       BSS  |E|          // Align to even
       BSS  |E|0         // Align to even, reserve 0
       BSS  |E|50        // Align to even, reserve 50
       DC   1
";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
        }
    }
}
