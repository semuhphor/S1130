using Xunit;
using S1130.SystemObjects;
using System.Linq;
using Asm = S1130.SystemObjects.Assembler;

namespace UnitTests.S1130.SystemObjects
{
    /// <summary>
    /// Tests for the new pattern-based assembler
    /// </summary>
    public class NewAssemblerTests
    {
        [Fact]
        public void Assemble_SimpleLoad_GeneratesCorrectHex()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "START: LD DATA",
                "DATA: DC 42"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            Assert.NotNull(result.GeneratedWords);
            Assert.Equal(2, result.GeneratedWords.Length);
            
            // Verify symbol table
            Assert.Equal(0, result.Symbols["START"]);
            Assert.Equal(1, result.Symbols["DATA"]);
        }

        [Fact]
        public void Assemble_ShiftInstruction_SLA_GeneratesCorrectHex()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "SLA 8"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            Assert.Single(result.GeneratedWords);
            
            // Expected: opcode 0x02 (ShiftLeft), tag 0, shift type 0 (bits 6-7), count 8 (bits 0-5)
            // = (0x02 << 11) | (0 << 8) | ((0 << 6) | 8)
            // = 0x1000 | 0 | 8 = 0x1008
            ushort expected = 0x1008;
            Assert.Equal(expected, result.GeneratedWords[0]);
        }

        [Fact]
        public void Assemble_ShiftInstruction_SLCA_GeneratesCorrectHex()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "SLCA 8"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            Assert.Single(result.GeneratedWords);
            
            // Expected: opcode 0x02 (ShiftLeft), tag 0, shift type 1 (bits 6-7), count 8 (bits 0-5)
            // = (0x02 << 11) | (0 << 8) | ((1 << 6) | 8)
            // = 0x1000 | 0 | 0x48 = 0x1048
            ushort expected = 0x1048;
            Assert.Equal(expected, result.GeneratedWords[0]);
        }

        [Fact]
        public void Assemble_ShiftInstruction_SLT_GeneratesCorrectHex()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "SLT 8"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            Assert.Single(result.GeneratedWords);
            
            // Expected: opcode 0x02 (ShiftLeft), tag 0, shift type 2 (bits 6-7), count 8 (bits 0-5)
            // = (0x02 << 11) | (0 << 8) | ((2 << 6) | 8)
            // = 0x1000 | 0 | 0x88 = 0x1088
            ushort expected = 0x1088;
            Assert.Equal(expected, result.GeneratedWords[0]);
        }

        [Fact]
        public void Assemble_ShiftInstruction_SLC_GeneratesCorrectHex()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "SLC 8"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            Assert.Single(result.GeneratedWords);
            
            // Expected: opcode 0x02 (ShiftLeft), tag 0, shift type 3 (bits 6-7), count 8 (bits 0-5)
            // = (0x02 << 11) | (0 << 8) | ((3 << 6) | 8)
            // = 0x1000 | 0 | 0xC8 = 0x10C8
            ushort expected = 0x10C8;
            Assert.Equal(expected, result.GeneratedWords[0]);
        }

        [Fact]
        public void Assemble_UndefinedSymbol_ReturnsError()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "LD UNDEFINED"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Message.Contains("UNDEFINED") || e.Message.Contains("Undefined"));
        }

        [Fact]
        public void Assemble_ExpressionWithAddition_Evaluates()
        {
            // Arrange
            var assembler = new Asm.Assembler();
            var source = new[]
            {
                "BASE: DC 0",
                "LD BASE+5"
            };

            // Act
            var result = assembler.Assemble(source);

            // Assert
            Assert.True(result.Success, result.GetErrorSummary());
            Assert.Empty(result.Errors);
            
            // Verify symbol
            Assert.Equal(0, result.Symbols["BASE"]);
        }
    }
}
