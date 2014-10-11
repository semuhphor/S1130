using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;
namespace UnitTests.S1130.SystemObjects
{
	[TestClass]
	public class InstrutionSetTests 
	{
		ICpu _cpu = new Cpu();

		[TestInitialize]
		public void BeforeEachTest()
		{
			_cpu = new Cpu();
		}

		[TestMethod]
		public void ShouldHaveInstructionSetAvailable()
		{
			Assert.IsNotNull(_cpu.Instructions);
		}

		[TestMethod]
		public void IndexerReturnsInstruction()
		{
			Assert.AreEqual("A", _cpu.Instructions[(int) OpCodes.Add].OpName);
		}
	}
}
