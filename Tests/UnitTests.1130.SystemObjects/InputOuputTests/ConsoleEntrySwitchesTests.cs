using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InputOuputTests
{
	[TestClass]
	public class ConsoleEntrySwitchesTests
	{
		private Cpu _cpu;

		[TestInitialize]
		public void BeforeEachTest()
		{
			_cpu = new Cpu(new SystemState(){Iar = 0x100});
		}

		[TestMethod]
		public void ConsoleEntrySwitches_Read()
		{
			var switches = new ConsoleEntrySwitches();
			_cpu.ConsoleSwitches = 0x1234;
			InstructionBuilder.BuildIoccAt(switches, DevFuction.Read, 0, 0x1000, _cpu, 0x500);
			switches.ExecuteIocc(_cpu, 0x500);
			Assert.AreEqual(_cpu[0x1000], _cpu.ConsoleSwitches);
		}
	}
}