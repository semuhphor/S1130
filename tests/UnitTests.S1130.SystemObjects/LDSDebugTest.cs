using Xunit;
using S1130.SystemObjects;
using System.Linq;

namespace UnitTests.S1130.SystemObjects
{
    public class LDSDebugTest
    {
        [Fact]
        public void DebugMDXAssembly()
        {
            var cpu = new Cpu();
            var source = "      ORG  /0100\n      MDX  L /0200";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            var disasm = cpu.Disassemble(0x100);
            
            // Output for debugging
            System.Console.WriteLine($"Assembled word1: 0x{word1:X4} ({word1})");
            System.Console.WriteLine($"Assembled word2: 0x{word2:X4} ({word2})");
            System.Console.WriteLine($"Disassembled: '{disasm}'");
            
            // Try reassembling
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disasm}";
            var result2 = cpu2.Assemble(source2);
            
            if (!result2.Success)
            {
                System.Console.WriteLine($"Reassembly failed: {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            }
            else
            {
                var word1b = cpu2[0x100];
                var word2b = cpu2[0x101];
                System.Console.WriteLine($"Reassembled word1: 0x{word1b:X4} ({word1b})");
                System.Console.WriteLine($"Reassembled word2: 0x{word2b:X4} ({word2b})");
            }
        }
    }
}
