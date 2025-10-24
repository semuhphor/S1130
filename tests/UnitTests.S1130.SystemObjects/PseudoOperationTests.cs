using Xunit;
using S1130.SystemObjects;
using System.Linq;

namespace UnitTests.S1130.SystemObjects
{
    public class PseudoOperationTests
    {
        [Fact]
        public void B_Unconditional_Long_Branch()
        {
            // Test B |L|TARGET - unconditional long branch
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      B |L|TARGET
      DC 1
TARGET: DC 2";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            
            // Debug: Check assembled instruction
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            var opcode = (word1 >> 11) & 0x1F;
            var longFormat = (word1 & 0x400) != 0;
            var modifiers = word1 & 0xFF;
            
            Assert.Equal(0x09, opcode); // BSC opcode
            Assert.True(longFormat);
            // Modifiers should be 0x00 for unconditional branch
            // Actually, check what they really are
            var debugInfo = $"Word1: 0x{word1:X4}, Word2: 0x{word2:X4}, Modifiers: 0x{modifiers:X2}";
            
            // Execute the branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction();
            
            // Should branch to TARGET (0x103)
            // Layout: 0x100-0x101: B L TARGET, 0x102: DC 1, 0x103: TARGET: DC 2
            Assert.True(cpu.Iar == 0x103, $"Expected IAR=0x103, got 0x{cpu.Iar:X4}. {debugInfo}");
        }

        [Fact]
        public void BP_BranchIfPositive()
        {
            // Test BP - branch if positive
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      BP   L TARGET
      DC   1
TARGET: DC  2
VAL: DC   5";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load 5 (positive), then BP should branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL - loads 5
            Assert.Equal((ushort)5, cpu.Acc);
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BP L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x105, cpu.Iar);
        }

        [Fact]
        public void BNP_BranchIfNotPositive()
        {
            // Test BNP - branch if not positive
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      BNP  L TARGET
      DC   1
TARGET: DC  2
VAL: DC   0";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load 0 (not positive), then BNP should branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL - loads 0
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BNP L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x105, cpu.Iar);
        }

        [Fact]
        public void BZ_BranchIfZero()
        {
            // Test BZ - branch if zero
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      BZ   L TARGET
      DC   1
TARGET: DC  2
VAL: DC   0";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load 0, then BZ should branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BZ L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x105, cpu.Iar);
        }

        [Fact]
        public void BNZ_BranchIfNotZero()
        {
            // Test BNZ - branch if not zero
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      BNZ  L TARGET
      DC   1
TARGET: DC  2
VAL: DC   5";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load 5 (not zero), then BNZ should branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BNZ L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x105, cpu.Iar);
        }

        [Fact]
        public void BN_BranchIfNegative()
        {
            // Test BN - branch if negative
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      BN   L TARGET
      DC   1
TARGET: DC  2
VAL: DC   /FFFF";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load -1, then BN should branch
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BN L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x105, cpu.Iar);
        }

        [Fact]
        public void SKP_SkipOnCondition()
        {
            // Test SKP Z - skip if zero
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL
      SKP  Z
      DC   1
      DC   2
VAL: DC   0";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load 0, then SKP Z should skip next instruction
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL - loads 0
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // SKP Z
            
            // Should skip to 0x104 (skip the: DC 1 at 0x103)
            Assert.Equal((ushort)0x104, cpu.Iar);
        }

        [Fact]
        public void BO_BranchOnOverflow()
        {
            // Test BO - branch on overflow
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      LD   L VAL1
      A    L VAL2
      BO   L TARGET
      DC   1
TARGET: DC  2
VAL1: DC   /7FFF
VAL2: DC   1";

            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Execute: Load max positive, add 1 to cause overflow
            cpu.Iar = 0x100;
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // LD L VAL1
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // A L VAL2 - should set overflow
            
            Assert.True(cpu.Overflow, "Overflow should be set");
            
            cpu.NextInstruction();
            cpu.ExecuteInstruction(); // BO L TARGET
            
            // Should branch to TARGET
            Assert.Equal((ushort)0x107, cpu.Iar);
        }

        [Fact]
        public void AllPseudoOps_Assemble()
        {
            // Test that all pseudo operations assemble without errors
            var cpu = new Cpu();
            
            var source = @"      ORG /100
      B    L T1
T1    BP   L T2
T2    BNP  L T3
T3    BN   L T4
T4    BNN  L T5
T5    BZ   L T6
T6    BNZ  L T7
T7    BC   L T8
T8    BO   L T9
T9    BOD  L T10
T10   SKP  ZPM
      WAIT";

            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
            Assert.Empty(result.Errors);
        }
    }
}
