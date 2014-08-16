using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
    [TestClass]
    public class InstructionBuilderTests
    {
        [TestMethod]
        public void BuildShortTest_ShortLoads()
        {
            Assert.AreEqual(0xc07f, InstructionBuilder.BuildShort(OpCodes.Load, 0, 0x7f));
            Assert.AreEqual(0xc344, InstructionBuilder.BuildShort(OpCodes.Load, 3, 0x44));
        }
    }
}