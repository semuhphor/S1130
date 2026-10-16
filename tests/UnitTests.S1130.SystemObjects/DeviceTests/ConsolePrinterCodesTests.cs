using System.Linq;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Utility;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	public class ConsolePrinterCodesTests : DeviceTestBase
	{
		private static ushort Word(byte code) => (ushort)(code << 8);

		[Theory]
		[InlineData(0x3C, "a")]
		[InlineData(0x3E, "A")]
		[InlineData(0x7C, "j")]
		[InlineData(0xCE, "J")]
		[InlineData(0xA2, "Z")]
		[InlineData(0xC4, "0")]
		[InlineData(0xE0, "9")]
		[InlineData(0x00, ".")]
		[InlineData(0xBC, "/")]
		[InlineData(0x21, " ")]
		[InlineData(0x81, "\n")]
		[InlineData(0x05, "\r")]
		[InlineData(0x41, "\t")]
		[InlineData(0x11, "\b")]
		public void Decode_KnownCodes(int code, string expected)
		{
			Assert.Equal(expected, ConsolePrinterCodes.Decode(Word((byte)code)));
		}

		[Fact]
		public void Decode_UnassignedCode_PrintsNothing()
		{
			Assert.Equal(string.Empty, ConsolePrinterCodes.Decode(Word(0x09)));
		}

		[Fact]
		public void Decode_EveryLetterAndDigitIsDistinct()
		{
			var all = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var decoded = Enumerable.Range(0, 256).Select(c => ConsolePrinterCodes.Decode(Word((byte)c))).ToList();
			foreach (var ch in all)
			{
				Assert.Single(decoded, ch.ToString());
			}
		}

		[Fact]
		public void Printer_WritesAppearAsText()
		{
			BeforeEachTest();
			var console = new DeviceConsoleKeyboard(InsCpu);
			InsCpu.AddDevice(console);
			foreach (var code in new byte[] { 0x24, 0x34, 0x5C, 0x5C, 0x50 })	// h e l l o
			{
				InsCpu[0x300] = Word(code);
				InsCpu.IoccDeviceCode = console.DeviceCode;
				InsCpu.IoccFunction = DevFunction.Write;
				InsCpu.IoccAddress = 0x300;
				console.ExecuteIocc();
				console.Run();
			}
			Assert.Equal("hello", console.PrintedText);
		}
	}
}
