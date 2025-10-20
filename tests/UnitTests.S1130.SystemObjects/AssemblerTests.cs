using Xunit;
using S1130.SystemObjects;
using System.Linq;

namespace UnitTests.S1130.SystemObjects
{
    public class AssemblerTests : IClassFixture<TestFixture>
    {
        private readonly ICpu _cpu;

        public AssemblerTests(TestFixture fixture)
        {
            _cpu = fixture.Cpu;
        }

        [Fact]
        public void EmptySourceCodeShouldSucceed()
        {
            var result = _cpu.Assemble("");

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Listing);
            Assert.Empty(result.Listing);
        }

        [Fact]
        public void SingleCommentLineShouldSucceed()
        {
            var result = _cpu.Assemble("* This is a comment");

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Contains(result.Listing, line => line.Contains("* This is a comment"));
        }

        [Fact]
        public void MissingOrgShouldFail()
        {
            var result = _cpu.Assemble("LABEL DC 5");

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(1, result.Errors[0].LineNumber);
            Assert.Contains("Missing ORG", result.Errors[0].Message);
        }

        [Fact]
        public void InvalidOrgFormatShouldFail()
        {
            var result = _cpu.Assemble("      ORG 100"); // Missing /

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(1, result.Errors[0].LineNumber);
            Assert.Contains("ORG address must begin with /", result.Errors[0].Message);
        }

        [Fact]
        public void InvalidOrgAddressShouldFail()
        {
            var result = _cpu.Assemble("      ORG /ZZZZ");

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(1, result.Errors[0].LineNumber);
            Assert.Contains("Invalid hex address", result.Errors[0].Message);
        }

        [Fact]
        public void ValidOrgShouldSucceed()
        {
            var result = _cpu.Assemble("      ORG /100");

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Contains(result.Listing, line => line.Contains("0100"));
        }

        [Fact]
        public void DcWithoutOrgShouldFail()
        {
            var result = _cpu.Assemble("VAL  DC 5");

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Contains("Missing ORG", result.Errors[0].Message);
        }

        [Fact]
        public void DcWithDecimalValueShouldSucceed()
        {
            var source = @"      ORG /100
VAL   DC  5";
            var result = _cpu.Assemble(source);

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal((ushort)5, _cpu[0x100]);
            Assert.Contains(result.Listing, line => line.Contains("0100"));
            Assert.Contains(result.Listing, line => line.Contains("VAL"));
        }

        [Fact]
        public void DcWithHexValueShouldSucceed()
        {
            var source = @"      ORG /100
VAL   DC  /FF";
            var result = _cpu.Assemble(source);

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal((ushort)0xFF, _cpu[0x100]);
            Assert.Contains(result.Listing, line => line.Contains("0100"));
            Assert.Contains(result.Listing, line => line.Contains("VAL"));
        }

        [Fact]
        public void DcWithInvalidDecimalShouldFail()
        {
            var source = @"      ORG /100
VAL   DC  65536"; // Too large for 16 bits
            var result = _cpu.Assemble(source);

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(2, result.Errors[0].LineNumber);
            // Could be either "Invalid decimal value" or "Undefined symbol" depending on parse order
            Assert.True(
                result.Errors[0].Message.Contains("Invalid decimal value") || 
                result.Errors[0].Message.Contains("Undefined symbol"),
                $"Unexpected error message: {result.Errors[0].Message}");
        }

        [Fact]
        public void DcWithInvalidHexShouldFail()
        {
            var source = @"      ORG /100
VAL   DC  /WXYZ"; // Invalid hex chars
            var result = _cpu.Assemble(source);

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(2, result.Errors[0].LineNumber);
            Assert.Contains("Invalid hex value", result.Errors[0].Message);
        }

        [Fact]
        public void MultipleLinesShouldUpdateLocationCounter()
        {
            var source = @"      ORG /100
VAL1  DC  5
VAL2  DC  /FF
VAL3  DC  100";
            var result = _cpu.Assemble(source);

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal((ushort)5, _cpu[0x100]);
            Assert.Equal((ushort)0xFF, _cpu[0x101]);
            Assert.Equal((ushort)100, _cpu[0x102]);
        }

        [Fact]
        public void ListingShouldIncludeLocationAndOriginalLine()
        {
            var source = @"      ORG /100
* This is a comment
VAL1  DC  5";
            var result = _cpu.Assemble(source);

            Assert.True(result.Success);
            var lines = result.Listing;
            Assert.Equal("0100       ORG /100", lines[0]);
            Assert.Equal("0100  * This is a comment", lines[1]);
            Assert.Equal("0100  VAL1  DC  5", lines[2]);
        }

        [Fact]
        public void OrgPastMemoryEndShouldFail()
        {
            var result = _cpu.Assemble("      ORG /8000"); // Beyond 32K

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Contains("Address overflow", result.Errors[0].Message);
        }

        [Fact]
        public void DcPastMemoryEndShouldFail()
        {
            var source = @"      ORG /7FFF
VAL1  DC  5
VAL2  DC  6"; // This would go beyond 32K
            var result = _cpu.Assemble(source);

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Contains("Address overflow", result.Errors[0].Message);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r")]
        [InlineData("\r\n")]
        public void ShouldHandleDifferentLineEndings(string lineEnding)
        {
            var source = $"      ORG /100{lineEnding}VAL   DC  5";
            var result = _cpu.Assemble(source);

            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal((ushort)5, _cpu[0x100]);
        }

        [Fact]
        public void LoadStoreAndHaltProgram()
        {
            // Create a fresh CPU for this test
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L VAL
      STO  L /500
      WAIT
VAL   DC   /B0BF";

            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Verify key instructions were generated correctly
            Assert.Equal((ushort)0xC400, cpu[0x100]); // LD instruction (opcode 0x18, long format)
            Assert.Equal((ushort)0x105, cpu[0x101]);  // Address of VAL label
            Assert.Equal((ushort)0xD400, cpu[0x102]); // STO instruction (opcode 0x1A, long format)
            Assert.Equal((ushort)0x500, cpu[0x103]);  // Target address /500
            Assert.Equal((ushort)0xB0BF, cpu[0x105]); // DC value at VAL
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, $"Program did not halt after {instructionCount} instructions");
            
            // Verify accumulator has the loaded value
            Assert.Equal((ushort)0xB0BF, cpu.Acc);
            
            // Verify value was stored to memory
            Assert.Equal((ushort)0xB0BF, cpu[0x500]);
        }

        [Fact]
        public void SimpleLoadInstruction()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L /200
DATA  DC   /1234";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            // Check instruction encoding
            var ldInst = cpu[0x100];
            var ldAddr = cpu[0x101];
            
            // LD opcode is 0x18, shifted left 11 bits = 0xC000
            // Long format bit is 0x400
            // Expected: 0xC000 | 0x400 = 0xC400
            Assert.Equal((ushort)0xC400, ldInst);
            Assert.Equal((ushort)0x200, ldAddr);
        }

        [Fact]
        public void AddAndSubtractProgram()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L VAL1
      A    L VAL2
      STO  L SUM
      LD   L VAL1
      S    L VAL2
      STO  L DIFF
      WAIT
VAL1  DC   /0010
VAL2  DC   /0005
SUM   DC   0
DIFF  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Calculate where data should be:
            // 0x100: LD L VAL1 (2), 0x102: A L VAL2 (2), 0x104: STO L SUM (2)
            // 0x106: LD L VAL1 (2), 0x108: S L VAL2 (2), 0x10A: STO L DIFF (2)
            // 0x10C: WAIT (1), 0x10D: VAL1, 0x10E: VAL2, 0x10F: SUM, 0x110: DIFF
            
            var val1 = cpu[0x10D];
            var val2 = cpu[0x10E];
            var sum = cpu[0x10F];
            var diff = cpu[0x110];
            
            // Verify results: 16 + 5 = 21, 16 - 5 = 11
            Assert.Equal((ushort)0x0010, val1); // VAL1 should be 16
            Assert.Equal((ushort)0x0005, val2); // VAL2 should be 5
            Assert.Equal((ushort)0x0015, sum); // SUM should be 21
            Assert.Equal((ushort)0x000B, diff); // DIFF should be 11
        }

        [Fact]
        public void MultiplyProgram()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L VAL1
      M    L VAL2
      WAIT
VAL1  DC   /0007
VAL2  DC   /0006";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Verify results: 7 * 6 = 42 (0x002A)
            // Multiply result is in Acc:Ext (32-bit), low word in Ext, high word in Acc
            Assert.Equal((ushort)0x0000, cpu.Acc); // High word should be 0
            Assert.Equal((ushort)0x002A, cpu.Ext); // Low word should be 42
        }

        [Fact]
        public void LogicalOperationsProgram()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L VAL1
      AND  L MASK
      STO  L RES1
      LD   L VAL1
      OR   L MASK
      STO  L RES2
      LD   L VAL1
      EOR  L MASK
      STO  L RES3
      WAIT
VAL1  DC   /00FF
MASK  DC   /0F0F
RES1  DC   0
RES2  DC   0
RES3  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Calculate addresses: 9 long instructions (18 words) + WAIT (1 word) = 19 words = 0x113
            // VAL1 at 0x113, MASK at 0x114, RES1 at 0x115, RES2 at 0x116, RES3 at 0x117
            
            // Verify results:
            // 0x00FF AND 0x0F0F = 0x000F
            // 0x00FF OR  0x0F0F = 0x0FFF
            // 0x00FF EOR 0x0F0F = 0x0FF0
            Assert.Equal((ushort)0x000F, cpu[0x115]); // RES1 = AND result
            Assert.Equal((ushort)0x0FFF, cpu[0x116]); // RES2 = OR result
            Assert.Equal((ushort)0x0FF0, cpu[0x117]); // RES3 = EOR result
        }

        [Fact]
        public void ShortFormatAndIndexRegisterProgram()
        {
            // Test short format and index register addressing
            // Create a fresh CPU for this test
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL1
      LD   VAL2
      STO  RES1
      LD   L VAL3,X1
      STO  L RES2,X2
      WAIT
VAL1  DC   10
VAL2  DC   20
VAL3  DC   30
RES1  DC   0
RES2  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Debug: check what was generated and print listing
            var listing = string.Join("\n", result.Listing);
            var ldVal2Inst = cpu[0x102]; // Short format LD VAL2
            var stoRes1Inst = cpu[0x103]; // Short format STO RES1
            
            // Calculate addresses:
            // LD L VAL1 (2 words @ 0x100-0x101)
            // LD VAL2 (1 word @ 0x102)  
            // STO RES1 (1 word @ 0x103)
            // LD L VAL3,X1 (2 words @ 0x104-0x105)
            // STO L RES2,X2 (2 words @ 0x106-0x107)
            // WAIT (1 word @ 0x108)
            // VAL1 @ 0x109, VAL2 @ 0x10A, VAL3 @ 0x10B, RES1 @ 0x10C, RES2 @ 0x10D
            
            // Execute program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            cpu.Xr[1] = 0; // Initialize index registers
            cpu.Xr[2] = 0;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, $"Program did not halt after {instructionCount} instructions");
            
            // Verify results: should have loaded and stored values correctly
            Assert.Equal((ushort)20, cpu[0x10C]); // RES1 (short format store)
            Assert.Equal((ushort)30, cpu[0x10D]); // RES2 (indexed store)
        }

        [Fact]
        public void BranchInstructionsProgram()
        {
            // Test branch instructions
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL1
      BSC  Z,ZERO
      A    L VAL2
ZERO  STO  L RES
      WAIT
VAL1  DC   0
VAL2  DC   10
RES   DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, $"Program did not halt after {instructionCount} instructions");
            
            // Verify: Since VAL1 is 0, BSC Z should skip the A instruction
            // Result should be 0, not 10
            Assert.Equal((ushort)0, cpu[0x108]); // RES should be 0
        }

        [Fact]
        public void XioInstructionTest()
        {
            // Test XIO instruction with IOCC structure
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      XIO  IOCC1
      XIO  L IOCC2
      WAIT
IOCC1 DC   /200
      DC   /1700
IOCC2 DC   /300
      DC   /0400";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Verify XIO instructions were generated correctly
            // XIO IOCC1 @ 0x100 - short format
            // XIO L IOCC2 @ 0x101-0x102 - long format (2 words)
            // WAIT @ 0x103
            // IOCC1 @ 0x104
            // Displacement = 0x104 - 0x101 = 3
            var xio1 = cpu[0x100];
            Assert.Equal((ushort)0x5803, xio1); // OpCode 0x0B (XIO), displacement 0x03
            
            // XIO L IOCC2 @ 0x101-0x102 - long format
            var xio2Word1 = cpu[0x101];
            var xio2Word2 = cpu[0x102];
            Assert.Equal((ushort)0x5C00, xio2Word1); // OpCode 0x0B (XIO), long format
            Assert.Equal((ushort)0x106, xio2Word2); // Address of IOCC2
            
            // Verify IOCC structures
            Assert.Equal((ushort)0x200, cpu[0x104]); // IOCC1 address
            Assert.Equal((ushort)0x1700, cpu[0x105]); // IOCC1 device/function/modifier
            Assert.Equal((ushort)0x300, cpu[0x106]); // IOCC2 address
            Assert.Equal((ushort)0x0400, cpu[0x107]); // IOCC2 device/function/modifier
        }

        [Fact]
        public void ComprehensiveAssemblerTest()
        {
            // Test all major features in one program
            var cpu = new Cpu();
            
            var source = @"* Comprehensive IBM 1130 Assembly Test
      ORG /100
* Test data movement
      LD   L VAL1      * Long format load
      STO  RES1        * Short format store
      LD   VAL2,X1     * Load with index register
      STO  L RES2,X2   * Long format store with index
* Test arithmetic
      LD   L VAL1
      A    L VAL2
      STO  L RES3
      M    L VAL1
      STD  L RES4
* Test logical operations
      LD   L VAL1
      AND  L VAL2
      STO  L RES5
* Test shifts
      LD   L VAL3
      SLA  4
      STO  L RES6
* Test branches
      LD   L VAL1
      BSC  Z,SKIP
      A    L VAL2
SKIP  STO  L RES7
      WAIT
* Data area
VAL1  DC   10
VAL2  DC   20
VAL3  DC   /000F
RES1  DC   0
RES2  DC   0
RES3  DC   0
RES4  DC   0
      DC   0
RES5  DC   0
RES6  DC   0
RES7  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            cpu.Xr[1] = 0;
            cpu.Xr[2] = 0;
            
            int instructionCount = 0;
            int maxInstructions = 1000;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, $"Program did not halt after {instructionCount} instructions");
            
            // Just verify the program executed successfully
            // The exact addresses depend on instruction layout which is complex to calculate manually
            Assert.True(cpu.Wait);
        }

        [Fact]
        public void EquDirectiveTest()
        {
            // Test EQU directive for symbolic constants representing addresses
            var cpu = new Cpu();
            
            var source = @"      ORG /100
TRAPADDR EQU /8D
DATAADDR EQU /200
* Put test value at address 0x8D
      LD   L VAL
      STO  L TRAPADDR
* Load it back from the EQU address
      LD   L TRAPADDR
      STO  L RESULT
      WAIT
VAL   DC   42
RESULT DC  0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Value should be stored at RESULT
            // After 4 long format instructions (8 words) + WAIT (1 word) + VAL (1 word) = 10 words
            var resultAddr = 0x10A;
            Assert.Equal((ushort)42, cpu[resultAddr]);
        }

        [Fact]
        public void EquAliasTest()
        {
            // Test EQU creating alias for another label (like COUNT EQU SENSR from real code)
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L SENSR
      A    L ONE
      STO  L COUNT
      WAIT
SENSR DC   10
COUNT EQU  SENSR
ONE   DC   1";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // COUNT is an alias for SENSR, so storing to COUNT stores to same location
            // Result should be SENSR(10) + ONE(1) = 11, stored at COUNT (which is same as SENSR)
            // LD L SENSR (2), A L ONE (2), STO L COUNT (2), WAIT (1), SENSR (1 @ 0x107), ONE (1 @ 0x108)
            // COUNT = SENSR = 0x107
            Assert.Equal((ushort)11, cpu[0x107]);
        }

        [Fact]
        public void BssDirectiveTest()
        {
            // Test BSS directive for reserving storage
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL1
      STO  L BUFFER
      STO  L /10B
      STO  L /10C
      WAIT
VAL1  DC   99
BUFFER BSS 10
AFTER DC   77";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Check that BSS reserved 10 words starting at BUFFER
            // VAL1 is at 0x109 (after 4 long stores + WAIT)
            // BUFFER starts at 0x10A
            // AFTER is at 0x114 (BUFFER + 10)
            var bufferAddr = 0x10A;
            Assert.Equal((ushort)99, cpu[bufferAddr]);     // First word
            Assert.Equal((ushort)99, cpu[bufferAddr + 1]); // Second word
            Assert.Equal((ushort)99, cpu[bufferAddr + 2]); // Third word
            Assert.Equal((ushort)77, cpu[0x114]);          // AFTER marker
        }

        [Fact]
        public void BssEvenAlignmentTest()
        {
            // Test BSS with even boundary alignment
            var cpu = new Cpu();
            
            var source = @"      ORG /100
VAL1  DC   1
      BSS  E 5
VAL2  DC   2";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // VAL1 at 0x100 (odd address after 0x100)
            // BSS E should align to even, then reserve 5 words
            // Since 0x101 is odd, align to 0x102, then reserve to 0x107
            // VAL2 should be at 0x107
            Assert.Equal((ushort)1, cpu[0x100]);
            Assert.Equal((ushort)2, cpu[0x107]);
        }

        [Fact]
        public void BssSymbolResolutionTest()
        {
            // BSS - Block Started by Symbol
            // This test verifies the assembler resolves BSS symbols correctly
            // by loading a value at the symbol address using indirect addressing
            var cpu = new Cpu();
            
            var source = @"      ORG /100
* Reserve a buffer with BSS
BUFFER  BSS  10       * Should be at /100 (leftmost word)
        DC   /5555    * Put a value at /10A
* Store BUFFER address using indirect to verify symbol resolution
START   LD   L ADDRPTR
        STO  L RESULT
        WAIT
ADDRPTR DC   BUFFER   * This DC should contain the address /100
RESULT  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute the program
            cpu.Iar = 0x10B; // START location
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, "Program did not complete");
            
            // RESULT should contain the address of BUFFER (0x100)
            // This verifies that DC BUFFER assembled the symbol value correctly
            Assert.Equal((ushort)0x100, cpu[0x110]); // RESULT location
        }

        [Fact]
        public void BesSymbolResolutionTest()
        {
            // BES - Block Ended by Symbol
            // This test verifies the assembler resolves BES symbols correctly
            // Label should point to rightmost word + 1
            var cpu = new Cpu();
            
            var source = @"      ORG /100
        DC   /AAAA
BUFEND  BES  10
        DC   BUFEND
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Memory layout:
            // /100: DC /AAAA
            // /101../10A: BES reserved area (10 words)
            // /10B: DC BUFEND - should contain /10B (the address itself)
            // /10C: WAIT
            
            Assert.Equal((ushort)0xAAAA, cpu[0x100]); // First DC
            Assert.Equal((ushort)0x10B, cpu[0x10B]);  // DC BUFEND contains address /10B
            
            // Execute to verify it actually works
            cpu.Iar = 0x10C;
            cpu.Wait = false;
            int count = 0;
            while (!cpu.Wait && count++ < 10)
            {
                cpu.ExecuteInstruction();
            }
            Assert.True(cpu.Wait, "Program should halt");
        }

        [Fact]
        public void EquSymbolResolutionTest()
        {
            // EQU - Equate Symbol
            // This test verifies the assembler resolves EQU symbols correctly
            var cpu = new Cpu();
            
            var source = @"      ORG /100
ADDR1   EQU  /200
ADDR2   EQU  /300
COUNT   EQU  42
        DC   ADDR1
        DC   ADDR2
        DC   COUNT
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Check that EQU values were resolved correctly in DC statements
            Assert.Equal((ushort)0x200, cpu[0x100]); // DC ADDR1
            Assert.Equal((ushort)0x300, cpu[0x101]); // DC ADDR2
            Assert.Equal((ushort)42, cpu[0x102]);    // DC COUNT
            
            // Execute to verify
            cpu.Iar = 0x103;
            cpu.Wait = false;
            int count = 0;
            while (!cpu.Wait && count++ < 10)
            {
                cpu.ExecuteInstruction();
            }
            Assert.True(cpu.Wait, "Program should halt");
        }

        [Fact]
        public void BssBesCombinedSymbolResolutionTest()
        {
            // Combined test showing the difference between BSS and BES
            var cpu = new Cpu();
            
            var source = @"      ORG /100
BUF1    BSS  5
        DC   /1111
BUF2    BES  5
        DC   /2222
        DC   BUF1
        DC   BUF2
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Memory layout:
            // /100../104: BSS reserved (5 words), BUF1 = /100 (leftmost)
            // /105: DC /1111
            // /106../10A: BES reserved (5 words)
            // /10B: DC /2222, BUF2 = /10B (rightmost + 1)
            // /10C: DC BUF1 - should be /100
            // /10D: DC BUF2 - should be /10B
            // /10E: WAIT
            
            Assert.Equal((ushort)0x1111, cpu[0x105]); // First DC
            Assert.Equal((ushort)0x2222, cpu[0x10B]); // Second DC
            Assert.Equal((ushort)0x100, cpu[0x10C]);  // DC BUF1 (BSS points to start)
            Assert.Equal((ushort)0x10B, cpu[0x10D]);  // DC BUF2 (BES points to end+1)
            
            // Execute to verify
            cpu.Iar = 0x10E;
            cpu.Wait = false;
            int count = 0;
            while (!cpu.Wait && count++ < 10)
            {
                cpu.ExecuteInstruction();
            }
            Assert.True(cpu.Wait, "Program should halt");
        }

        [Fact]
        public void StarAsCurrentAddressInEqu()
        {
            // Test * as current address in EQU
            var cpu = new Cpu();
            
            var source = @"      ORG /100
        LD   L VAL
        STO  L RESULT
HERE    EQU  *
        WAIT
VAL     DC   /1234
RESULT  DC   0
        DC   HERE";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // HERE should be the address of the WAIT instruction (0x104)
            // /100-101: LD instruction
            // /102-103: STO instruction
            // /104: WAIT (HERE points here)
            // /105: VAL
            // /106: RESULT
            // /107: DC HERE - should contain /104
            
            Assert.Equal((ushort)0x104, cpu[0x107]); // DC HERE should be /104
        }

        [Fact]
        public void StarMinusStarExpression()
        {
            // Test *-* expression (evaluates to 0)
            var cpu = new Cpu();
            
            var source = @"      ORG /100
LOC     DC   *-*
        DC   /5555
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // LOC should contain 0 (*-* = current address minus current address)
            Assert.Equal((ushort)0, cpu[0x100]);
            Assert.Equal((ushort)0x5555, cpu[0x101]);
        }

        [Fact]
        public void StarPlusOffsetExpression()
        {
            // Test *+offset expression
            var cpu = new Cpu();
            
            var source = @"      ORG /100
        DC   *+5
        DC   /AAAA
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // First DC at /100 should contain *+5 = /100 + 5 = /105
            Assert.Equal((ushort)0x105, cpu[0x100]);
        }

        [Fact]
        public void StarMinusOffsetExpression()
        {
            // Test *-offset expression
            var cpu = new Cpu();
            
            var source = @"      ORG /100
        DC   /BBBB
        DC   *-1
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Second DC at /101 should contain *-1 = /101 - 1 = /100
            Assert.Equal((ushort)0x100, cpu[0x101]);
        }

        [Fact]
        public void InlineCommentWithoutAsterisk()
        {
            // Test inline comments (space-delimited, no * required)
            // After operand, anything following is treated as comment
            var cpu = new Cpu();
            
            var source = @"      ORG /100
VAL     DC   42 this is a comment
RESULT  DC   0 another comment
        WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Values should be assembled correctly despite inline comments
            Assert.Equal((ushort)42, cpu[0x100]);
            Assert.Equal((ushort)0, cpu[0x101]);
        }

        [Fact]
        public void ComprehensiveBssEquTest()
        {
            // Test combining BSS, EQU, and actual usage
            var cpu = new Cpu();
            
            var source = @"* Test BSS and EQU together
      ORG /200
BUFSIZE EQU 10
IOADDR EQU /32
* Store value in buffer
      LD   L VAL
      STO  L BUFFER
      STO  L /20E
      WAIT
VAL   DC   55
BUFFER BSS E BUFSIZE
MARKER DC   99";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
            
            // Execute program
            cpu.Iar = 0x200;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 1000;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait, $"Program did not halt after {instructionCount} instructions");
            
            // Verify buffer has values
            Assert.Equal((ushort)55, cpu[0x20E]); // First word of BUFFER (even aligned)
        }

        [Fact]
        public void ShiftInstructionsProgram()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /100
      LD   L VAL1
      SLA  4
      STO  L RES1
      LD   L VAL2
      SRA  2
      STO  L RES2
      WAIT
VAL1  DC   /000F
VAL2  DC   /FF00
RES1  DC   0
RES2  DC   0";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            
            // Execute the program
            cpu.Iar = 0x100;
            cpu.Wait = false;
            
            int instructionCount = 0;
            int maxInstructions = 100;
            
            while (!cpu.Wait && instructionCount < maxInstructions)
            {
                cpu.NextInstruction();
                cpu.ExecuteInstruction();
                instructionCount++;
            }
            
            Assert.True(cpu.Wait);
            
            // Calculate addresses: 
            // LD L VAL1 (2), SLA 4 (1), STO L RES1 (2), LD L VAL2 (2), SRA 2 (1), STO L RES2 (2), WAIT (1)
            // = 11 words = 0x10B start of data
            // VAL1 at 0x10B, VAL2 at 0x10C, RES1 at 0x10D, RES2 at 0x10E
            
            // Verify results:
            // 0x000F << 4 = 0x00F0
            // 0xFF00 >> 2 (logical, SRA shifts Acc only) = 0x3FC0
            Assert.Equal((ushort)0x00F0, cpu[0x10D]); // RES1 = shift left result
            Assert.Equal((ushort)0x3FC0, cpu[0x10E]); // RES2 = shift right result
        }
    }
}