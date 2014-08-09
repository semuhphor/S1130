using Microsoft.VisualStudio.TestTools.UnitTesting;
using _1130.SystemObjects;

namespace UnitTests._1130.SystemObjects
{
    [TestClass]
    public class InstructionBuilderTests
    {
        [TestMethod]
        public void BuildShortTest_ShortLoads()
        {
            Assert.AreEqual(0xc07f, InstructionBuilder.BuildShort(Instructions.Load, 0, 0x7f));
            Assert.AreEqual(0xc344, InstructionBuilder.BuildShort(Instructions.Load, 3, 0x44));
        }
    }
}