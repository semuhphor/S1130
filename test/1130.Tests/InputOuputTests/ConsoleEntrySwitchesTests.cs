using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace Tests
{
	public class ConsoleEntrySwitchesTests
	{
		private Cpu _cpu;

		public void BeforeEachTest()
		{
			_cpu = new Cpu{Iar = 0x100};
		}

		[Fact]
		public void ConsoleEntrySwitches_Read()
		{
			BeforeEachTest();
			var switches = new ConsoleEntrySwitches(_cpu);
			_cpu.ConsoleSwitches = 0x1234;
			InstructionBuilder.BuildIoccAt(switches, DevFunction.Read, 0, 0x1000, _cpu, 0x500);
			_cpu.IoccDecode(0x500);
			switches.ExecuteIocc();
			Assert.Equal(_cpu[0x1000], _cpu.ConsoleSwitches);
		}
	}
}