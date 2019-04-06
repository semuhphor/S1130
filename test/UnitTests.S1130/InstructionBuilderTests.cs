using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130
{
    public class InstructionBuilderTests
    {
        [Fact]
        public void BuildShortTest_ShortLoads()
        {
            Assert.Equal(0xc07f, InstructionBuilder.BuildShort(OpCodes.Load, 0, 0x7f));
            Assert.Equal(0xc344, InstructionBuilder.BuildShort(OpCodes.Load, 3, 0x44));
        }
    }
}