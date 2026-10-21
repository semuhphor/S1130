using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/// <summary>
	/// Tests for <see cref="Device1132"/>, the 1132 line printer. The tests play the part of the
	/// program: they load the print buffer (core words 32-39) and run the device by hand.
	/// </summary>
	public class Device1132Tests : DeviceTestBase
	{
		private readonly Device1132 _printer;

		public Device1132Tests()
		{
			BeforeEachTest();
			_printer = (Device1132)InsCpu.Devices[6];
			_printer.EventInterval = 0;										// events happen on every Run()
		}

		private void Control(ushort modifier) => IssueControl(_printer, 0, (byte)modifier);

		private ushort Sense(bool reset = false)
		{
			SenseDevice(_printer, (ushort)(reset ? 1 : 0));
			return InsCpu.Acc;
		}

		private void SetBuffer(params int[] columns)						// sets one hammer bit per column plus the "buffer ready" bit
		{
			for (var i = 32; i < 40; i++) InsCpu[i] = 0;
			foreach (var column in columns) InsCpu[32 + column / 16] |= (ushort)(0x8000 >> (column % 16));
			InsCpu[39] |= 1;
		}

		[Fact]
		public void IsRegisteredAsDevice6OnADefaultCpu()
		{
			Assert.NotNull(InsCpu.Devices[6]);
			Assert.IsType<Device1132>(InsCpu.Devices[6]);
			Assert.Equal(1, Device1132.InterruptLevel);
		}

		[Fact]
		public void Read_ReturnsTheDrumCharactersInOrder_AndWrapsAround()
		{
			var address = 0x300;
			InsCpu.IoccFunction = DevFunction.Read;
			InsCpu.IoccAddress = address;

			_printer.ExecuteIocc();
			Assert.Equal(0xC100, InsCpu[address]);							// A
			_printer.ExecuteIocc();
			Assert.Equal(0xC200, InsCpu[address]);							// B
			for (var i = 2; i < Device1132.WheelCharacters; i++) _printer.ExecuteIocc();
			_printer.ExecuteIocc();
			Assert.Equal(0xC100, InsCpu[address]);							// the drum has come round to A again
		}

		[Fact]
		public void Read_WhilePrinting_DoesNotStepTheDrum()
		{
			Control(Device1132.StartPrinter);
			InsCpu.IoccFunction = DevFunction.Read;
			InsCpu.IoccAddress = 0x300;

			_printer.ExecuteIocc();
			_printer.ExecuteIocc();

			Assert.Equal(0xC100, InsCpu[0x300]);
		}

		[Fact]
		public void InitialStatus_IsReadyWithNoTapeChannelAtLineOne()
		{
			Assert.Equal(0, Sense());
		}

		[Fact]
		public void Printing_FiresHammersAndInterruptsForEachCharacter()
		{
			SetBuffer(0);													// 'A' in column 1
			Control(Device1132.StartPrinter);
			Assert.Equal(Device1132.PrinterBusy, Sense() & Device1132.PrinterBusy);

			_printer.Run();													// A printed, drum moves on
			Assert.NotEqual(0, Sense() & Device1132.EmitterResponse);
			Assert.NotNull(_printer.ActiveInterrupt);

			SetBuffer(1, 5);												// 'B' in columns 2 and 6
			_printer.Run();
			Control(Device1132.StopPrinter);
			Control(Device1132.Space);
			_printer.Run();

			Assert.Equal("AB   B", _printer.PrintedText);
			Assert.NotEqual(0, Sense() & Device1132.SpaceResponse);
		}

		[Fact]
		public void SenseWithReset_ClearsResponsesAndTheInterrupt()
		{
			SetBuffer(0);
			Control(Device1132.StartPrinter);
			_printer.Run();
			Assert.NotEqual(0, Sense() & Device1132.EmitterResponse);

			Sense(reset: true);

			Assert.Equal(0, Sense() & Device1132.EmitterResponse);
			Assert.Null(_printer.ActiveInterrupt);
		}

		[Fact]
		public void Printing_WithBufferNotSetInTime_IsAPrintCheck()
		{
			SetBuffer(0);
			InsCpu[39] = 0;													// the "buffer ready" bit is missing
			Control(Device1132.StartPrinter);

			_printer.Run();

			Assert.True(_printer.PrintCheck);
			Assert.Equal(0, Sense() & Device1132.EmitterResponse);			// the operation is abandoned, no more interrupts
		}

		[Fact]
		public void SpacingOnBlankLines_QueuesNewlinesUntilTextArrives()
		{
			Control(Device1132.Space);
			_printer.Run();
			Control(Device1132.Space);
			_printer.Run();
			Assert.Equal("", _printer.PrintedText);

			SetBuffer(0);
			Control(Device1132.StartPrinter);
			_printer.Run();
			Control(Device1132.StopPrinter);
			Control(Device1132.Space);
			_printer.Run();

			Assert.Equal("\n\nA", _printer.PrintedText);
		}

		[Fact]
		public void Overprinting_SeparatesRowsWithCarriageReturn()
		{
			Control(Device1132.StartPrinter);
			SetBuffer(0);
			_printer.Run();													// A in column 1
			SetBuffer(0);
			_printer.Run();													// B on top of it
			Control(Device1132.StopPrinter);
			Control(Device1132.Space);
			_printer.Run();

			Assert.Equal("A\rB", _printer.PrintedText);
		}

		[Fact]
		public void PrintedLine_ShowsWhenThePrinterStops_WithoutWaitingForTheCarriage()
		{
			SetBuffer(0);
			Control(Device1132.StartPrinter);
			_printer.Run();
			Control(Device1132.StopPrinter);

			Assert.Equal("A", _printer.PrintedText);
		}

		[Fact]
		public void Skip_StopsAtTopOfFormAndReportsTheChannel()
		{
			Control(Device1132.StartCarriage);
			_printer.Run();

			var dsw = Sense();
			Assert.NotEqual(0, dsw & Device1132.SkipResponse);
			Assert.Equal(0x0080, dsw & Device1132.ChannelMask);				// channel 1

			Control(Device1132.StopCarriage);
			Assert.Equal(0, Sense() & Device1132.CarriageBusy);
		}

		[Fact]
		public void Skip_ToBottomOfFormThenPastIt_SendsAFormFeed()
		{
			Control(Device1132.StartCarriage);
			_printer.Run();													// to channel 1
			Control(Device1132.StopCarriage);
			Sense(reset: true);

			Control(Device1132.StartCarriage);
			_printer.Run();													// to channel 12
			Assert.Equal(0x0001, Sense() & 0x0001);
			Control(Device1132.StopCarriage);
			Sense(reset: true);

			Control(Device1132.StartCarriage);
			_printer.Run();													// wraps past the page end to channel 1
			Assert.Contains('\f', _printer.PrintedText);
		}

		[Fact]
		public void GoingOffline_CancelsTheOperationWithAFormsCheck()
		{
			SetBuffer(0);
			Control(Device1132.StartPrinter);
			_printer.Online = false;
			Assert.NotEqual(0, Sense() & Device1132.NotReady);

			_printer.Run();

			Assert.True(_printer.FormsCheck);
			Assert.Equal("", _printer.PrintedText);
			Assert.Equal(0, Sense() & Device1132.EmitterResponse);
		}

		[Fact]
		public void Events_WaitForTheConfiguredNumberOfInstructions()
		{
			_printer.EventInterval = 1000;
			SetBuffer(0);
			Control(Device1132.StartPrinter);

			_printer.Run();

			Assert.Equal(0, Sense() & Device1132.EmitterResponse);			// no instructions ran, so nothing is due
			InsCpu.Wait = true;												// a waiting CPU cannot race the printer
			_printer.Run();
			Assert.NotEqual(0, Sense() & Device1132.EmitterResponse);
		}

		[Fact]
		public void TakePrintedText_ReturnsAndClearsTheOutput()
		{
			SetBuffer(0);
			Control(Device1132.StartPrinter);
			_printer.Run();
			Control(Device1132.StopPrinter);
			Control(Device1132.Space);
			_printer.Run();

			Assert.Equal("A", _printer.TakePrintedText());
			Assert.Equal("", _printer.PrintedText);
		}
	}
}
