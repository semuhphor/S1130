using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;
using System.Linq;

namespace UnitTests.S1130.SystemObjects
{
    /// <summary>
    /// Round-trip tests: assemble → disassemble → reassemble → verify identity.
    /// These tests ensure that disassembly produces valid assembly that assembles to the same binary.
    /// </summary>
    public class RoundTripTests
    {
        [Theory]
        [InlineData("A |L|/0200")]
        [InlineData("LD |L|/0500")]
        [InlineData("STO |L|/0300")]
        [InlineData("S |L|/0400")]
        [InlineData("M |L|/0250")]
        [InlineData("D |L|/0260")]
        public void RoundTrip_Arithmetic_LongFormat(string instruction)
        {
            var cpu = new Cpu();
            
            // Assemble the instruction
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            // Capture the assembled bytes
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            // Disassemble
            var disassembled = cpu.Disassemble(0x100);
            
            // Debug: show what was disassembled
            var debugMessage = $"Disassembled: '{disassembled}'";
            
            // Reassemble the disassembled instruction
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}. {debugMessage}");
            
            // Verify they match
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("A |L1|/0200")]  // Long with index 1
        [InlineData("LD |L2|/0500")]  // Long with index 2
        [InlineData("STO |L3|/0300")]  // Long with index 3
        public void RoundTrip_WithIndexRegister(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("A |I|/0200")]   // Indirect (implies long)
        [InlineData("LD |I|/0500")]   // Indirect (implies long)
        [InlineData("STO |I|/0300")]   // Indirect (implies long)
        public void RoundTrip_IndirectLong(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("A |I1|/0200")]  // Indirect with index (implies long)
        [InlineData("LD |I2|/0500")]  // Indirect with index (implies long)
        [InlineData("STO |I3|/0300")]  // Indirect with index (implies long)
        public void RoundTrip_IndirectWithIndex(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Fact]
        public void RoundTrip_DoubleWordInstructions()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /0100
      LDD  L /0200
      STD  L /0300
      AD   L /0400
      SD   L /0500";
            
            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Capture all words
            var originalWords = new ushort[16];
            for (int i = 0; i < 16; i++)
                originalWords[i] = cpu[0x100 + i];
            
            // Disassemble all instructions
            var disassembled1 = cpu.Disassemble(0x100);
            var disassembled2 = cpu.Disassemble(0x102);
            var disassembled3 = cpu.Disassemble(0x104);
            var disassembled4 = cpu.Disassemble(0x106);
            
            // Reassemble
            var cpu2 = new Cpu();
            var source2 = $@"      ORG  /0100
      {disassembled1}
      {disassembled2}
      {disassembled3}
      {disassembled4}";
            
            var result2 = cpu2.Assemble(source2);
            Assert.True(result2.Success, $"Reassembly failed: {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            // Verify all words match
            for (int i = 0; i < 8; i++)  // 4 instructions × 2 words each
            {
                Assert.Equal(originalWords[i], cpu2[0x100 + i]);
            }
        }
        
        [Theory]
        [InlineData("AND |L|/0200")]
        [InlineData("OR |L|/0300")]
        [InlineData("EOR |L|/0400")]
        public void RoundTrip_Logical_LongFormat(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}'");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("LDX |L1|/0200")]
        [InlineData("LDX |L2|/0300")]
        [InlineData("STX |L3|/0400")]
        public void RoundTrip_IndexRegisters(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}'");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("SLA 16")]
        [InlineData("SRA 8")]
        [InlineData("SLT 4")]
        [InlineData("SRT 12")]
        public void RoundTrip_Shift_Immediate(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
        }
        
        [Theory]
        [InlineData("BSC |L|/0200")]
        public void RoundTrip_BranchSkipConditional(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("BSI |L|/0200")]
        public void RoundTrip_BranchStoreIar(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Theory]
        [InlineData("MDX |L|/0200")]
        public void RoundTrip_ModifyIndex(string instruction)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            var word1 = cpu[0x100];
            var word2 = cpu[0x101];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            Assert.Equal(word1, cpu2[0x100]);
            Assert.Equal(word2, cpu2[0x101]);
        }
        
        [Fact]
        public void RoundTrip_WaitInstruction()
        {
            var cpu = new Cpu();
            
            var source = "      ORG  /0100\n      WAIT";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success);
            
            var word1 = cpu[0x100];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}'");
            
            Assert.Equal(word1, cpu2[0x100]);
        }
        
        [Theory]
        [InlineData("STS |L|/0200", 2)]
        [InlineData("LDS /3", 1)]
        public void RoundTrip_StatusInstructions(string instruction, int wordCount)
        {
            var cpu = new Cpu();
            
            var source = $"      ORG  /0100\n      {instruction}";
            var result = cpu.Assemble(source);
            
            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            
            // Capture the instruction words
            var originalWords = new ushort[wordCount];
            for (int i = 0; i < wordCount; i++)
                originalWords[i] = cpu[0x100 + i];
            
            var disassembled = cpu.Disassemble(0x100);
            
            var cpu2 = new Cpu();
            var source2 = $"      ORG  /0100\n      {disassembled}";
            var result2 = cpu2.Assemble(source2);
            
            Assert.True(result2.Success, $"Reassembly failed for '{disassembled}': {string.Join(", ", result2.Errors.Select(e => e.Message))}");
            
            // Verify the instruction words match
            for (int i = 0; i < wordCount; i++)
            {
                Assert.Equal(originalWords[i], cpu2[0x100 + i]);
            }
        }
        
        [Fact]
        public void RoundTrip_CompleteProgram()
        {
            var cpu = new Cpu();
            
            var source = @"      ORG  /0100
      LD |L|/0500
      A |L|/0501
      STO |L|/0502
      LDX |L1|/0503
      SLA 16
      WAIT
      DC /1234
      DC /5678";
            
            var result = cpu.Assemble(source);
            Assert.True(result.Success);
            
            // Capture all assembled words
            var originalWords = new ushort[20];
            for (int i = 0; i < 20; i++)
                originalWords[i] = cpu[0x100 + i];
            
            // Disassemble all instructions
            var disassembled = new System.Collections.Generic.List<string>();
            ushort addr = 0x100;
            for (int i = 0; i < 6; i++)  // 6 instructions (changed from 7)
            {
                disassembled.Add(cpu.Disassemble(addr));
                // Determine instruction length
                cpu.Iar = addr;
                cpu.NextInstruction();
                addr = cpu.Iar;
            }
            
            // Reassemble
            var cpu2 = new Cpu();
            var source2 = "      ORG  /0100\n";
            foreach (var line in disassembled)
            {
                source2 += $"      {line}\n";
            }
            source2 += "      DC   /1234\n      DC   /5678";
            
            var result2 = cpu2.Assemble(source2);
            Assert.True(result2.Success, $"Reassembly failed: {string.Join(", ", result2.Errors.Select(e => e.Message))}\nSource:\n{source2}");
            
            // Verify first 12 words match (instructions only, not DC data - adjusted from 14)
            for (int i = 0; i < 12; i++)
            {
                Assert.Equal(originalWords[i], cpu2[0x100 + i]);
            }
        }
    }
}
