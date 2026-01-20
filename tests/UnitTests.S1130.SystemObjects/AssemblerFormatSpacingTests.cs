using S1130.SystemObjects;
using System.Linq;
using Xunit;

namespace UnitTests.S1130.SystemObjects
{
    public class AssemblerFormatSpacingTests
    {
        [Fact]
        public void FormatSpecifier_WithSpaceAfterClosingBar_ShouldAssemble()
        {
            // Test: LD |L| VALUE (space after |)
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LD |L| /0042
      WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_WithoutSpaceAfterClosingBar_ShouldAssemble()
        {
            // Test: LD |L|/0042 (no space after |)
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LD |L|/0042
      WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_MultipleSpacesAfterClosingBar_ShouldAssemble()
        {
            // Test: LD |L|   VALUE (multiple spaces after |)
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LD |L|   /0042
      WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_BothSpacingStyles_ShouldProduceSameCode()
        {
            var cpu1 = new Cpu();
            var source1 = @"
      ORG /100
      LD |L| /0500
      STO |L| /0502
      WAIT
";
            var result1 = cpu1.Assemble(source1);
            Assert.True(result1.Success);

            var cpu2 = new Cpu();
            var source2 = @"
      ORG /100
      LD |L|/0500
      STO |L|/0502
      WAIT
";
            var result2 = cpu2.Assemble(source2);
            Assert.True(result2.Success);

            // Verify both produce identical machine code
            for (ushort addr = 0x100; addr < 0x110; addr++)
            {
                Assert.Equal(cpu1[addr], cpu2[addr]);
            }
        }

        [Fact]
        public void FormatSpecifier_IndexRegister_WithSpaceAfterBar_ShouldAssemble()
        {
            // Test: LD |L1| TABLE
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LDX |1| /0005
      LD |L1| /0200
      WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_IndexRegister_WithoutSpaceAfterBar_ShouldAssemble()
        {
            // Test: LD |L1|TABLE
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LDX |1|/0005
      LD |L1|/0200
      WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_ShortFormat_WithAndWithoutSpace_ShouldAssemble()
        {
            var cpu = new Cpu();
            var source = @"
      ORG /100
START LD |.| VALUE1
      A |.|VALUE2
      WAIT
VALUE1 DC 10
VALUE2 DC 20
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_IndirectFormat_WithAndWithoutSpace_ShouldAssemble()
        {
            var cpu = new Cpu();
            var source = @"
      ORG /100
      LD |I| PTR1
      STO |I|PTR2
      WAIT
PTR1  DC /0200
PTR2  DC /0300
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_BSC_WithSpaceAfterBar_ShouldAssemble()
        {
            var cpu = new Cpu();
            var source = @"
      ORG /100
START LD |L| /0001
      BSC |L| DONE,Z
      BSC |L| START
DONE  WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        [Fact]
        public void FormatSpecifier_BSC_WithoutSpaceAfterBar_ShouldAssemble()
        {
            var cpu = new Cpu();
            var source = @"
      ORG /100
START LD |L|/0001
      BSC |L|DONE,Z
      BSC |L|START
DONE  WAIT
";
            var result = cpu.Assemble(source);

            Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }
    }
}
