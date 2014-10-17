using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.InputOuputTests
{
	[TestClass]
	public class ConsoleEntrySwitchesTests
	{
		private Cpu _cpu;

		[TestInitialize]
		public void BeforeEachTest()
		{
			_cpu = new Cpu{Iar = 0x100};
		}

		[TestMethod]
		public void ConsoleEntrySwitches_Read()
		{
			var switches = new ConsoleEntrySwitches(_cpu);
			_cpu.ConsoleSwitches = 0x1234;
			InstructionBuilder.BuildIoccAt(switches, DevFunction.Read, 0, 0x1000, _cpu, 0x500);
			_cpu.IoccDecode(0x500);
			switches.ExecuteIocc();
			Assert.AreEqual(_cpu[0x1000], _cpu.ConsoleSwitches);
		}
	}
}