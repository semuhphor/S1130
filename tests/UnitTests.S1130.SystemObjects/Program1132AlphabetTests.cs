using System;
using System.IO;
using System.Linq;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using Xunit;

namespace UnitTests.S1130.SystemObjects
{
	/// <summary>
	/// Runs docs\TestAsm\alphabet1132.s1130, a stand-alone (no DMS) program that prints A-Z on the 1132
	/// using level-1 interrupts, to show the CPU, interrupt system and printer working together.
	/// </summary>
	public class Program1132AlphabetTests
	{
		private static string Source() =>
			File.ReadAllText(Path.Combine("..", "..", "..", "..", "..", "docs", "TestAsm", "alphabet1132.s1130"));

		[Fact(Skip = "Work in progress: program misses letters on the reference SIMH and our 1132 prints nothing.")]
		public void Program_PrintsTheAlphabetOnThe1132()
		{
			var cpu = new Cpu();
			var result = cpu.Assemble(Source());
			Assert.True(result.Success, string.Join("\n", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}")));

			using var runner = new DeviceRunner(cpu);
			runner.Start();
			cpu.Iar = 0x100;
			var deadline = DateTime.UtcNow.AddSeconds(10);
			while (DateTime.UtcNow < deadline && !cpu.Wait)							// the program ends in a WAIT
			{
				if (!cpu.Step()) System.Threading.Thread.Sleep(1);
			}

			var printer = (Device1132)cpu.Devices[6];
			Assert.True(cpu.Wait, $"did not finish: IAR={cpu.Iar:X4} text=[{printer.PrintedText}]");
			Assert.False(printer.PrintCheck);
			Assert.Equal("ABCDEFGHIJKLMNOPQRSTUVWXYZ", printer.PrintedText.Trim('\n', '\f', '\r'));
		}
	}
}
