using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
    public class ShowMemoryTest
    {
        private readonly ITestOutputHelper _output;

        public ShowMemoryTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShowAssembledMemory()
        {
            var cpu = new Cpu();

            var source = @"// IBM 1130 Shift Left Test Program
       ORG  /100
START: LDD  |L|ONE       // Load double-word (0,1) into ACC and EXT
LOOP:  SLT  1            // Shift left together 1 bit
       BSC  |L|LOOP,C    // Carry OFF -- keep shifting
       BSC  |L|START     // Carry ON - reload 0,1 into acc/ext
       BSS  |E|          // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
";

            var result = cpu.Assemble(source);
            
            if (!result.Success)
            {
                _output.WriteLine("=== ASSEMBLY ERRORS ===");
                foreach (var error in result.Errors)
                {
                    _output.WriteLine($"Line {error.LineNumber}: {error.Message}");
                }
            }
            
            Assert.True(result.Success, result.GetErrorSummary());

            _output.WriteLine("=== SYMBOLS ===");
            foreach (var symbol in result.Symbols.OrderBy(kvp => kvp.Value))
            {
                _output.WriteLine($"{symbol.Key,-10} = /{symbol.Value:X3}");
            }
            _output.WriteLine("");

            _output.WriteLine("=== MEMORY CONTENTS /100 through /110 ===");
            _output.WriteLine("Address  Hex    Decimal");
            _output.WriteLine("-------  ----   -------");

            for (int addr = 0x100; addr <= 0x110; addr++)
            {
                ushort value = cpu[(ushort)addr];
                _output.WriteLine($"/{addr:X3}     {value:X4}   {value,5}");
            }
        }
    }
}
