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
            Assert.Contains("Invalid decimal value", result.Errors[0].Message);
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
    }
}