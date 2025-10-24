using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;
using S1130.SystemObjects.Utility;
using Xunit;
using Xunit.Abstractions;
using Asm = S1130.SystemObjects.Assembler;

namespace UnitTests.S1130.SystemObjects
{
    /// <summary>
    /// Comprehensive validation of all instruction formats:
    /// - Assembler syntax
    /// - Expected hex values
    /// - Disassembly output
    /// </summary>
    public class InstructionFormatValidation
    {
        private readonly ITestOutputHelper _output;
        private readonly Cpu _cpu;
        private readonly Asm.Assembler _assembler;

        public InstructionFormatValidation(ITestOutputHelper output)
        {
            _output = output;
            _cpu = new Cpu();
            _assembler = new Asm.Assembler();
        }

        public class InstructionTest
        {
            public string Mnemonic { get; set; }
            public string AssemblerFormat { get; set; }
            public ushort[] ExpectedHex { get; set; }
            public string Description { get; set; }
        }

        [Fact]
        public void ValidateAllInstructionFormats()
        {
            var tests = GetAllInstructionTests();
            
            var results = new StringBuilder();
            results.AppendLine("=".PadRight(120, '='));
            results.AppendLine("INSTRUCTION FORMAT VALIDATION REPORT");
            results.AppendLine("=".PadRight(120, '='));
            results.AppendLine();

            int testNumber = 1;
            int passed = 0;
            int failed = 0;

            foreach (var test in tests)
            {
                results.AppendLine($"TEST #{testNumber}: {test.Description}");
                results.AppendLine("-".PadRight(120, '-'));
                results.AppendLine($"Assembler Input:  {test.AssemblerFormat}");
                results.AppendLine($"Expected Hex:     {string.Join(" ", test.ExpectedHex.Select(h => $"{h:X4}").ToArray())}");
                
                try
                {
                    // Add ORG directive and assemble the instruction
                    var sourceLines = new[] { "ORG /0000", test.AssemblerFormat };
                    var assemblyResult = _assembler.Assemble(sourceLines);
                    
                    if (assemblyResult.Errors.Any())
                    {
                        results.AppendLine($"Assembly FAILED:  {string.Join(", ", assemblyResult.Errors.Select(e => e.Message))}");
                        results.AppendLine();
                        failed++;
                        testNumber++;
                        continue;
                    }

                    // Get the assembled machine code
                    var actualHex = assemblyResult.GeneratedWords;
                    
                    results.AppendLine($"Actual Hex:       {string.Join(" ", actualHex.Select(h => $"{h:X4}").ToArray())}");

                    // Load into CPU and disassemble
                    for (int i = 0; i < actualHex.Length; i++)
                    {
                        _cpu[i] = actualHex[i];
                    }
                    _cpu.Iar = 0;

                    var instruction = _cpu.Instructions[actualHex[0] >> 11];
                    var disassembly = instruction.Disassemble(_cpu, 0);
                    results.AppendLine($"Disassembly:      {disassembly}");

                    // Validate
                    bool hexMatch = actualHex.Length == test.ExpectedHex.Length && 
                                   actualHex.SequenceEqual(test.ExpectedHex);
                    
                    if (hexMatch)
                    {
                        results.AppendLine($"Result:           PASS ✓");
                        passed++;
                    }
                    else
                    {
                        results.AppendLine($"Result:           FAIL ✗ (Hex mismatch)");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    results.AppendLine($"Exception:        {ex.Message}");
                    results.AppendLine($"Result:           FAIL ✗");
                    failed++;
                }

                results.AppendLine();
                testNumber++;
            }

            results.AppendLine("=".PadRight(120, '='));
            results.AppendLine($"SUMMARY: {passed} passed, {failed} failed out of {tests.Count} tests");
            results.AppendLine("=".PadRight(120, '='));

            _output.WriteLine(results.ToString());

            // Assert that all tests passed
            Assert.Equal(0, failed);
        }

        private List<InstructionTest> GetAllInstructionTests()
        {
            var tests = new List<InstructionTest>();

            // Helper for calculating short format hex
            ushort Short(OpCodes op, uint tag, uint disp) => 
                (ushort)(((uint)op << 11) | (tag << 8) | (disp & 0xFF));

            // Helper for calculating long format hex (returns 2 words)
            ushort[] Long(OpCodes op, uint tag, ushort addr) => 
                new[] { (ushort)(((uint)op << 11) | (tag << 8) | 1), addr };

            // LOAD (LD) - OpCode 0x18
            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    . /0100",
                ExpectedHex = new[] { Short(OpCodes.Load, 0, 0x0100) },
                Description = "Load - Short format, no index"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    1 /0100",
                ExpectedHex = new[] { Short(OpCodes.Load, 1, 0x0100) },
                Description = "Load - Short format, index 1"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    2 /0100",
                ExpectedHex = new[] { Short(OpCodes.Load, 2, 0x0100) },
                Description = "Load - Short format, index 2"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    3 /0100",
                ExpectedHex = new[] { Short(OpCodes.Load, 3, 0x0100) },
                Description = "Load - Short format, index 3"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    L /0100",
                ExpectedHex = Long(OpCodes.Load, 0, 0x0100),
                Description = "Load - Long format, no index"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    L1 /0100",
                ExpectedHex = Long(OpCodes.Load, 1, 0x0100),
                Description = "Load - Long format, index 1"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    L2 /0100",
                ExpectedHex = Long(OpCodes.Load, 2, 0x0100),
                Description = "Load - Long format, index 2"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    L3 /0100",
                ExpectedHex = Long(OpCodes.Load, 3, 0x0100),
                Description = "Load - Long format, index 3"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    I /0100",
                ExpectedHex = Long(OpCodes.Load, 4, 0x0100),
                Description = "Load - Indirect, no index"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    I1 /0100",
                ExpectedHex = Long(OpCodes.Load, 5, 0x0100),
                Description = "Load - Indirect, index 1"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    I2 /0100",
                ExpectedHex = Long(OpCodes.Load, 6, 0x0100),
                Description = "Load - Indirect, index 2"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    I3 /0100",
                ExpectedHex = Long(OpCodes.Load, 7, 0x0100),
                Description = "Load - Indirect, index 3"
            });

            // LOAD DOUBLE (LDD) - OpCode 0x19
            tests.Add(new InstructionTest
            {
                Mnemonic = "LDD",
                AssemblerFormat = "         LDD   . /0100",
                ExpectedHex = new[] { Short(OpCodes.LoadDouble, 0, 0x0100) },
                Description = "Load Double - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDD",
                AssemblerFormat = "         LDD   L /0100",
                ExpectedHex = Long(OpCodes.LoadDouble, 0, 0x0100),
                Description = "Load Double - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDD",
                AssemblerFormat = "         LDD   I /0100",
                ExpectedHex = Long(OpCodes.LoadDouble, 4, 0x0100),
                Description = "Load Double - Indirect"
            });

            // STORE (STO) - OpCode 0x1A
            tests.Add(new InstructionTest
            {
                Mnemonic = "STO",
                AssemblerFormat = "         STO   . /0100",
                ExpectedHex = new[] { Short(OpCodes.Store, 0, 0x0100) },
                Description = "Store - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STO",
                AssemblerFormat = "         STO   L /0100",
                ExpectedHex = Long(OpCodes.Store, 0, 0x0100),
                Description = "Store - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STO",
                AssemblerFormat = "         STO   I /0100",
                ExpectedHex = Long(OpCodes.Store, 4, 0x0100),
                Description = "Store - Indirect"
            });

            // STORE DOUBLE (STD) - OpCode 0x1B
            tests.Add(new InstructionTest
            {
                Mnemonic = "STD",
                AssemblerFormat = "         STD   . /0100",
                ExpectedHex = new[] { Short(OpCodes.StoreDouble, 0, 0x0100) },
                Description = "Store Double - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STD",
                AssemblerFormat = "         STD   L /0100",
                ExpectedHex = Long(OpCodes.StoreDouble, 0, 0x0100),
                Description = "Store Double - Long format"
            });

            // ADD (A) - OpCode 0x10
            tests.Add(new InstructionTest
            {
                Mnemonic = "A",
                AssemblerFormat = "         A     . /0100",
                ExpectedHex = new[] { Short(OpCodes.Add, 0, 0x0100) },
                Description = "Add - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "A",
                AssemblerFormat = "         A     L /0100",
                ExpectedHex = Long(OpCodes.Add, 0, 0x0100),
                Description = "Add - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "A",
                AssemblerFormat = "         A     I /0100",
                ExpectedHex = Long(OpCodes.Add, 4, 0x0100),
                Description = "Add - Indirect"
            });

            // ADD DOUBLE (AD) - OpCode 0x11
            tests.Add(new InstructionTest
            {
                Mnemonic = "AD",
                AssemblerFormat = "         AD    . /0100",
                ExpectedHex = new[] { Short(OpCodes.AddDouble, 0, 0x0100) },
                Description = "Add Double - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "AD",
                AssemblerFormat = "         AD    L /0100",
                ExpectedHex = Long(OpCodes.AddDouble, 0, 0x0100),
                Description = "Add Double - Long format"
            });

            // SUBTRACT (S) - OpCode 0x12
            tests.Add(new InstructionTest
            {
                Mnemonic = "S",
                AssemblerFormat = "         S     . /0100",
                ExpectedHex = new[] { Short(OpCodes.Subtract, 0, 0x0100) },
                Description = "Subtract - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "S",
                AssemblerFormat = "         S     L /0100",
                ExpectedHex = Long(OpCodes.Subtract, 0, 0x0100),
                Description = "Subtract - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "S",
                AssemblerFormat = "         S     I /0100",
                ExpectedHex = Long(OpCodes.Subtract, 4, 0x0100),
                Description = "Subtract - Indirect"
            });

            // SUBTRACT DOUBLE (SD) - OpCode 0x13
            tests.Add(new InstructionTest
            {
                Mnemonic = "SD",
                AssemblerFormat = "         SD    . /0100",
                ExpectedHex = new[] { Short(OpCodes.SubtractDouble, 0, 0x0100) },
                Description = "Subtract Double - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "SD",
                AssemblerFormat = "         SD    L /0100",
                ExpectedHex = Long(OpCodes.SubtractDouble, 0, 0x0100),
                Description = "Subtract Double - Long format"
            });

            // MULTIPLY (M) - OpCode 0x14
            tests.Add(new InstructionTest
            {
                Mnemonic = "M",
                AssemblerFormat = "         M     . /0100",
                ExpectedHex = new[] { Short(OpCodes.Multiply, 0, 0x0100) },
                Description = "Multiply - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "M",
                AssemblerFormat = "         M     L /0100",
                ExpectedHex = Long(OpCodes.Multiply, 0, 0x0100),
                Description = "Multiply - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "M",
                AssemblerFormat = "         M     I /0100",
                ExpectedHex = Long(OpCodes.Multiply, 4, 0x0100),
                Description = "Multiply - Indirect"
            });

            // DIVIDE (D) - OpCode 0x15
            tests.Add(new InstructionTest
            {
                Mnemonic = "D",
                AssemblerFormat = "         D     . /0100",
                ExpectedHex = new[] { Short(OpCodes.Divide, 0, 0x0100) },
                Description = "Divide - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "D",
                AssemblerFormat = "         D     L /0100",
                ExpectedHex = Long(OpCodes.Divide, 0, 0x0100),
                Description = "Divide - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "D",
                AssemblerFormat = "         D     I /0100",
                ExpectedHex = Long(OpCodes.Divide, 4, 0x0100),
                Description = "Divide - Indirect"
            });

            // AND - OpCode 0x1C
            tests.Add(new InstructionTest
            {
                Mnemonic = "AND",
                AssemblerFormat = "         AND   . /0100",
                ExpectedHex = new[] { Short(OpCodes.And, 0, 0x0100) },
                Description = "AND - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "AND",
                AssemblerFormat = "         AND   L /0100",
                ExpectedHex = Long(OpCodes.And, 0, 0x0100),
                Description = "AND - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "AND",
                AssemblerFormat = "         AND   I /0100",
                ExpectedHex = Long(OpCodes.And, 4, 0x0100),
                Description = "AND - Indirect"
            });

            // OR - OpCode 0x1D
            tests.Add(new InstructionTest
            {
                Mnemonic = "OR",
                AssemblerFormat = "         OR    . /0100",
                ExpectedHex = new[] { Short(OpCodes.Or, 0, 0x0100) },
                Description = "OR - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "OR",
                AssemblerFormat = "         OR    L /0100",
                ExpectedHex = Long(OpCodes.Or, 0, 0x0100),
                Description = "OR - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "OR",
                AssemblerFormat = "         OR    I /0100",
                ExpectedHex = Long(OpCodes.Or, 4, 0x0100),
                Description = "OR - Indirect"
            });

            // EXCLUSIVE OR (EOR) - OpCode 0x1E
            tests.Add(new InstructionTest
            {
                Mnemonic = "EOR",
                AssemblerFormat = "         EOR   . /0100",
                ExpectedHex = new[] { Short(OpCodes.ExclusiveOr, 0, 0x0100) },
                Description = "Exclusive OR - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "EOR",
                AssemblerFormat = "         EOR   L /0100",
                ExpectedHex = Long(OpCodes.ExclusiveOr, 0, 0x0100),
                Description = "Exclusive OR - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "EOR",
                AssemblerFormat = "         EOR   I /0100",
                ExpectedHex = Long(OpCodes.ExclusiveOr, 4, 0x0100),
                Description = "Exclusive OR - Indirect"
            });

            // LOAD INDEX (LDX) - OpCode 0x0C
            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   1 /0100",
                ExpectedHex = new[] { Short(OpCodes.LoadIndex, 1, 0x0100) },
                Description = "Load Index - Index 1, short"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   2 /0100",
                ExpectedHex = new[] { Short(OpCodes.LoadIndex, 2, 0x0100) },
                Description = "Load Index - Index 2, short"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   3 /0100",
                ExpectedHex = new[] { Short(OpCodes.LoadIndex, 3, 0x0100) },
                Description = "Load Index - Index 3, short"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   L1 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 1, 0x0100),
                Description = "Load Index - Index 1, long"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   L2 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 2, 0x0100),
                Description = "Load Index - Index 2, long"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   L3 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 3, 0x0100),
                Description = "Load Index - Index 3, long"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   I1 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 5, 0x0100),
                Description = "Load Index - Index 1, indirect"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   I2 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 6, 0x0100),
                Description = "Load Index - Index 2, indirect"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LDX",
                AssemblerFormat = "         LDX   I3 /0100",
                ExpectedHex = Long(OpCodes.LoadIndex, 7, 0x0100),
                Description = "Load Index - Index 3, indirect"
            });

            // STORE INDEX (STX) - OpCode 0x0D
            tests.Add(new InstructionTest
            {
                Mnemonic = "STX",
                AssemblerFormat = "         STX   1 /0100",
                ExpectedHex = new[] { Short(OpCodes.StoreIndex, 1, 0x0100) },
                Description = "Store Index - Index 1, short"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STX",
                AssemblerFormat = "         STX   L1 /0100",
                ExpectedHex = Long(OpCodes.StoreIndex, 1, 0x0100),
                Description = "Store Index - Index 1, long"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STX",
                AssemblerFormat = "         STX   I1 /0100",
                ExpectedHex = Long(OpCodes.StoreIndex, 5, 0x0100),
                Description = "Store Index - Index 1, indirect"
            });

            // MODIFY INDEX (MDX) - OpCode 0x0A
            tests.Add(new InstructionTest
            {
                Mnemonic = "MDX",
                AssemblerFormat = "         MDX   1 /0100",
                ExpectedHex = new[] { Short(OpCodes.ModifyIndex, 1, 0x0100) },
                Description = "Modify Index - Index 1, short"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "MDX",
                AssemblerFormat = "         MDX   L1 /0100",
                ExpectedHex = Long(OpCodes.ModifyIndex, 1, 0x0100),
                Description = "Modify Index - Index 1, long"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "MDX",
                AssemblerFormat = "         MDX   I1 /0100",
                ExpectedHex = Long(OpCodes.ModifyIndex, 5, 0x0100),
                Description = "Modify Index - Index 1, indirect"
            });

            // LOAD STATUS (LD) - OpCode 0x04
            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    . /0100",
                ExpectedHex = new[] { Short(OpCodes.LoadStatus, 0, 0x0100) },
                Description = "Load Status - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "LD",
                AssemblerFormat = "         LD    L /0100",
                ExpectedHex = Long(OpCodes.LoadStatus, 0, 0x0100),
                Description = "Load Status - Long format"
            });

            // STORE STATUS (STS) - OpCode 0x05
            tests.Add(new InstructionTest
            {
                Mnemonic = "STS",
                AssemblerFormat = "         STS   . /0100",
                ExpectedHex = new[] { Short(OpCodes.StoreStatus, 0, 0x0100) },
                Description = "Store Status - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "STS",
                AssemblerFormat = "         STS   L /0100",
                ExpectedHex = Long(OpCodes.StoreStatus, 0, 0x0100),
                Description = "Store Status - Long format"
            });

            // SHIFT LEFT (SLA, SLC, SLT, SLCA) - OpCode 0x02
            tests.Add(new InstructionTest
            {
                Mnemonic = "SLA",
                AssemblerFormat = "         SLA   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftLeft, 0, 8) },
                Description = "Shift Left Arithmetic - 8 positions"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "SLC",
                AssemblerFormat = "         SLC   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftLeft, 1, 8) },
                Description = "Shift Left and Count - 8 positions"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "SLT",
                AssemblerFormat = "         SLT   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftLeft, 2, 8) },
                Description = "Shift Left Through - 8 positions"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "SLCA",
                AssemblerFormat = "         SLCA  . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftLeft, 3, 8) },
                Description = "Shift Left and Count Arithmetic - 8 positions"
            });

            // SHIFT RIGHT (SRA, SRT, RTE) - OpCode 0x03
            tests.Add(new InstructionTest
            {
                Mnemonic = "SRA",
                AssemblerFormat = "         SRA   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftRight, 0, 8) },
                Description = "Shift Right Arithmetic - 8 positions"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "SRT",
                AssemblerFormat = "         SRT   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftRight, 2, 8) },
                Description = "Shift Right Through - 8 positions"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "RTE",
                AssemblerFormat = "         RTE   . 8",
                ExpectedHex = new[] { Short(OpCodes.ShiftRight, 3, 8) },
                Description = "Rotate Extension - 8 positions"
            });

            // BRANCH AND SKIP (BSC, BOSC) - OpCode 0x09
            // Short format - skip only (no address)
            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . E",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0040) },
                Description = "Branch Skip - Skip if accumulator is even"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . O",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0080) },
                Description = "Branch Skip - Skip if accumulator is odd"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . +",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0100) },
                Description = "Branch Skip - Skip if accumulator is positive"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . -",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0200) },
                Description = "Branch Skip - Skip if accumulator is negative"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . Z",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0400) },
                Description = "Branch Skip - Skip if accumulator is zero"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . C",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0800) },
                Description = "Branch Skip - Skip if carry is OFF"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   . O",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x1000) },
                Description = "Branch Skip - Skip if overflow is OFF"
            });

            // Long format - branch to address
            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   L /0100,E",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0041), (ushort)0x0100 },
                Description = "Branch Skip - Branch to address if even"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSC",
                AssemblerFormat = "         BSC   L /0100,C",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0801), (ushort)0x0100 },
                Description = "Branch Skip - Branch to address if carry OFF"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BOSC",
                AssemblerFormat = "         BOSC  L /0100,E",
                ExpectedHex = new[] { (ushort)(((uint)OpCodes.BranchSkip << 11) | 0x0021), (ushort)0x0100 },
                Description = "Branch or Skip - Branch to address if even"
            });

            // BRANCH AND STORE (BSI) - OpCode 0x08
            tests.Add(new InstructionTest
            {
                Mnemonic = "BSI",
                AssemblerFormat = "         BSI   L /0100",
                ExpectedHex = Long(OpCodes.BranchStore, 0, 0x0100),
                Description = "Branch and Store IAR - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSI",
                AssemblerFormat = "         BSI   I /0100",
                ExpectedHex = Long(OpCodes.BranchStore, 4, 0x0100),
                Description = "Branch and Store IAR - Indirect"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "BSI",
                AssemblerFormat = "         BSI   L1 /0100",
                ExpectedHex = Long(OpCodes.BranchStore, 1, 0x0100),
                Description = "Branch and Store IAR - Long with index 1"
            });

            // WAIT - OpCode 0x06
            tests.Add(new InstructionTest
            {
                Mnemonic = "WAIT",
                AssemblerFormat = "         WAIT  . /0000",
                ExpectedHex = new[] { Short(OpCodes.Wait, 0, 0x0000) },
                Description = "Wait for interrupt"
            });

            // EXECUTE IO (XIO) - OpCode 0x01
            tests.Add(new InstructionTest
            {
                Mnemonic = "XIO",
                AssemblerFormat = "         XIO   . /0001",
                ExpectedHex = new[] { Short(OpCodes.ExecuteInputOutput, 0, 0x0001) },
                Description = "Execute I/O - Short format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "XIO",
                AssemblerFormat = "         XIO   L /0001",
                ExpectedHex = Long(OpCodes.ExecuteInputOutput, 0, 0x0001),
                Description = "Execute I/O - Long format"
            });

            tests.Add(new InstructionTest
            {
                Mnemonic = "XIO",
                AssemblerFormat = "         XIO   I /0001",
                ExpectedHex = Long(OpCodes.ExecuteInputOutput, 4, 0x0001),
                Description = "Execute I/O - Indirect"
            });

            return tests;
        }
    }
}
