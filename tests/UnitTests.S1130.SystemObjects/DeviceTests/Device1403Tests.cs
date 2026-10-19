using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/// <summary>
	/// Tests for <see cref="Device1403"/>, the 1403 line printer. The tests play the part of the
	/// program: they put a line in core, issue the IOCCs and run the device by hand.
	/// </summary>
	public class Device1403Tests : DeviceTestBase
	{
		private const int Buffer = 0x300;
		private readonly Device1403 _printer;

		public Device1403Tests()
		{
			BeforeEachTest();
			_printer = new Device1403(InsCpu) { EventInterval = 0 };
			InsCpu.AddDevice(_printer);
		}

		private void Exec(DevFunction function, int address = 0, byte modifier = 0)
		{
			InsCpu.IoccDevice = _printer;
			InsCpu.IoccFunction = function;
			InsCpu.IoccAddress = address;
			InsCpu.IoccModifiers = modifier;
			_printer.ExecuteIocc();
		}

		private ushort Sense(bool reset = false)
		{
			SenseDevice(_printer, (ushort)(reset ? 1 : 0));
			return InsCpu.Acc;
		}

		private void SetLine(string text)                                   // two 1403 print codes per word
		{
			var codes = new System.Collections.Generic.Dictionary<char, int> { ['A'] = 0x64, ['B'] = 0x25, ['C'] = 0x26, ['1'] = 0x40, [' '] = 0x7F };
			for (var i = 0; i < 60; i++)
			{
				var hi = 2 * i < text.Length ? codes[text[2 * i]] : 0x7F;
				var lo = 2 * i + 1 < text.Length ? codes[text[2 * i + 1]] : 0x7F;
				InsCpu[Buffer + i] = (ushort)((hi << 8) | lo);
			}
		}

		private void PrintLine(string text)
		{
			SetLine(text);
			Exec(DevFunction.InitWrite, Buffer);
			_printer.Run();                                                 // transfer complete
			_printer.Run();                                                 // print complete
		}

		[Fact]
		public void HasDeviceCode0x15_AndInterruptsOnLevel4()
		{
			Assert.Equal(0x15, _printer.DeviceCode);
			Assert.Equal(4, Device1403.InterruptLevel);
		}

		[Fact]
		public void Printing_GivesTransferCompleteThenPrintComplete()
		{
			SetLine("ABC");
			Exec(DevFunction.InitWrite, Buffer);
			Assert.Equal(Device1403.PrinterBusy, Sense() & Device1403.PrinterBusy);

			_printer.Run();
			Assert.NotEqual(0, Sense() & Device1403.TransferComplete);
			Assert.Equal(0, Sense() & Device1403.PrintComplete);
			Assert.NotNull(_printer.ActiveInterrupt);

			Sense(reset: true);
			_printer.Run();
			Assert.NotEqual(0, Sense() & Device1403.PrintComplete);
			Assert.Equal(0, Sense() & Device1403.PrinterBusy);
		}

		[Fact]
		public void Space_FlushesTheLineAndInterruptsWithCarriageComplete()
		{
			PrintLine("A C1");
			Sense(reset: true);

			Exec(DevFunction.Control);
			Assert.NotEqual(0, Sense() & Device1403.CarriageBusy);
			_printer.Run();

			Assert.Equal("A C1", _printer.PrintedText);
			Assert.NotEqual(0, Sense() & Device1403.CarriageComplete);
			Assert.Equal(0, Sense() & Device1403.CarriageBusy);
		}

		[Fact]
		public void SenseWithReset_ClearsInterruptBitsAndTheInterrupt()
		{
			PrintLine("A");
			Assert.NotEqual(0, Sense() & Device1403.PrintComplete);

			Sense(reset: true);

			Assert.Equal(0, Sense() & (Device1403.PrintComplete | Device1403.TransferComplete));
			Assert.Null(_printer.ActiveInterrupt);
		}

		[Fact]
		public void BlankLines_AreQueuedUntilTextArrives()
		{
			Exec(DevFunction.Control);
			_printer.Run();
			Exec(DevFunction.Control);
			_printer.Run();
			Assert.Equal("", _printer.PrintedText);

			PrintLine("B");
			Exec(DevFunction.Control);
			_printer.Run();

			Assert.Equal("\n\nB", _printer.PrintedText);
		}

		[Fact]
		public void PrintingTwiceWithoutSpacing_Overprints()
		{
			PrintLine("A");
			PrintLine("B");
			Exec(DevFunction.Control);
			_printer.Run();

			Assert.Equal("A\rB", _printer.PrintedText);
		}

		[Fact]
		public void Skip_WritesThePendingLineAndAFormFeed()
		{
			PrintLine("A");
			InsCpu[0x200] = Device1403.TapeChannel1;
			Exec(DevFunction.Write, 0x200);
			Assert.NotEqual(0, Sense() & Device1403.CarriageBusy);
			_printer.Run();

			Assert.Equal("A\f", _printer.PrintedText);
			Assert.NotEqual(0, Sense() & Device1403.CarriageComplete);
			Assert.Equal(0, Sense() & Device1403.CarriageBusy);
		}

		[Fact]
		public void Skip_OnAFreshPage_DoesNotAddAnotherFormFeed()
		{
			PrintLine("A");
			InsCpu[0x200] = 0x0D00;                                         // whatever channels the program asks for
			Exec(DevFunction.Write, 0x200);
			_printer.Run();
			Exec(DevFunction.Write, 0x200);
			_printer.Run();

			Assert.Equal("A\f", _printer.PrintedText);
		}

		[Fact]
		public void GoingOffline_CancelsTheOperation()
		{
			_printer.Online = false;
			Assert.NotEqual(0, Sense() & Device1403.NotReady);
			SetLine("A");
			Exec(DevFunction.InitWrite, Buffer);

			_printer.Run();

			Assert.True(_printer.FormsCheck);
			Assert.Equal(0, Sense() & Device1403.TransferComplete);
		}

		[Fact]
		public void TakePrintedText_ReturnsAndClearsTheOutput()
		{
			PrintLine("A");
			Exec(DevFunction.Control);
			_printer.Run();

			Assert.Equal("A", _printer.TakePrintedText());
			Assert.Equal("", _printer.PrintedText);
		}
	}
}
