using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
    public class DebugExecutionTest
    {
        private readonly ITestOutputHelper _output;

        public DebugExecutionTest(ITestOutputHelper output)
        {
            _output = output;
        }

        // Helper method to write to both test output and debug output
        private void WriteLine(string message)
        {
            _output.WriteLine(message);
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        [Fact]
        public void ExecuteTestProgram_StepByStep()
        {
            // Assemble the test program
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
            
            // Verify assembly succeeded
            if (!result.Success)
            {
                WriteLine("=== ASSEMBLY ERRORS ===");
                foreach (var error in result.Errors)
                {
                    WriteLine($"Line {error.LineNumber}: {error.Message}");
                }
            }
            
            Assert.True(result.Success, result.GetErrorSummary());

            WriteLine("=== ASSEMBLY SUCCESSFUL ===");
            WriteLine($"Start Address: /{result.StartAddress:X3}");
            WriteLine("");
            
            // Display symbols
            WriteLine("=== SYMBOLS ===");
            foreach (var symbol in result.Symbols.OrderBy(kvp => kvp.Value))
            {
                WriteLine($"{symbol.Key,-10} = /{symbol.Value:X3}");
            }
            WriteLine("");

            // Set IAR to start of program
            cpu.Iar = (ushort)result.StartAddress;
            
            WriteLine("=== STARTING EXECUTION ===");
            WriteLine($"IAR set to: /{cpu.Iar:X3}");
            WriteLine("");

            // Execute instructions in a loop
            int maxInstructions = 100; // Safety limit
            int instructionCount = 0;
            
            while (instructionCount < maxInstructions)
            {
                // Save state before execution
                var iarBefore = cpu.Iar;
                var accBefore = cpu.Acc;
                var extBefore = cpu.Ext;
                var carryBefore = cpu.Carry;
                var overflowBefore = cpu.Overflow;
                
                // Fetch next instruction
                
                // Display current instruction info
                var opcode = (cpu.AtIar >> 11) & 0x1F;
                var format = (cpu.AtIar & 0x0400) != 0 ? "Long" : "Short";
                var tag = (cpu.AtIar >> 8) & 0x03;
                
                WriteLine($"--- Instruction #{instructionCount + 1} ---");
                WriteLine($"Address: /{iarBefore:X3}");
                WriteLine($"Word:    {cpu.AtIar:X4}");
                WriteLine($"Opcode:  {opcode:X2} ({format})");
                WriteLine($"Tag:     {tag}");
                WriteLine($"Before:  ACC={accBefore:X4} EXT={extBefore:X4} Carry={carryBefore} Overflow={overflowBefore}");
                WriteLine($"Disasm:  {cpu.Disassemble(cpu.Iar)}");
                
                // Execute the instruction
                // SET A BREAKPOINT ON THE NEXT LINE TO STEP THROUGH EXECUTION
                cpu.ExecuteInstruction();
                
                // Display state after execution
                WriteLine($"After:   ACC={cpu.Acc:X4} EXT={cpu.Ext:X4} Carry={cpu.Carry} Overflow={cpu.Overflow}");
                WriteLine($"Next IAR: /{cpu.Iar:X3}");
                WriteLine("");
                
                instructionCount++;
                    
                // Check for WAIT instruction (opcode 0x0C)
                if (opcode == 0x0C)
                {
                    WriteLine("=== WAIT INSTRUCTION ENCOUNTERED ===");
                    break;
                }
                
                // Check if we've looped back to START multiple times
                if (cpu.Iar == result.StartAddress && instructionCount > 10)
                {
                    WriteLine("=== RETURNED TO START - STOPPING ===");
                    break;
                }
                
                // Detect if carry occurred (which will cause branch back to START)
                if (cpu.Carry && cpu.Iar == result.StartAddress)
                {
                    WriteLine("=== CARRY DETECTED - PROGRAM LOOPED TO START ===");
                    break;
                }
            }
            
            WriteLine($"=== EXECUTION COMPLETE ===");
            WriteLine($"Total instructions executed: {instructionCount}");
            WriteLine($"Final ACC: {cpu.Acc:X4} ({cpu.Acc})");
            WriteLine($"Final EXT: {cpu.Ext:X4} ({cpu.Ext})");
            WriteLine($"Final Carry: {cpu.Carry}");
            WriteLine($"Final Overflow: {cpu.Overflow}");
            
            // Test passes as long as we executed some instructions
            Assert.True(instructionCount > 0, "Should have executed at least one instruction");
        }
    }
}
