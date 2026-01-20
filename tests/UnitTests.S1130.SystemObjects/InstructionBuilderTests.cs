using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;
using UnitTests.S1130.SystemObjects.InstructionTests;

namespace UnitTests.S1130.SystemObjects
{
    public class InstructionBuilderTests
    {
        [Fact]
        public void BuildShortTest_ShortLoads()
        {
            ICpu  InsCpu = new Cpu { Iar = 0x100 };
            InstructionBuilder.BuildShortAtIar(OpCodes.Load, 0, 0x7f, InsCpu);
            Assert.Equal(0xc07f, InsCpu[InsCpu.Iar]);
            InstructionBuilder.BuildShortAtIar(OpCodes.Load, 3, 0x44, InsCpu);
            Assert.Equal(0xc344, InsCpu[InsCpu.Iar]);
        }
    }
}