using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests;

public class DeviceConsoleKeyboardTests : DeviceTestBase
{
    private DeviceConsoleKeyboard _console;

    protected override void BeforeEachTest()
    {
        base.BeforeEachTest();
        _console = new DeviceConsoleKeyboard(InsCpu);
        InsCpu.AddDevice(_console);
    }

    private void Issue(DevFunction function, int address = 0)
    {
        InsCpu.IoccDeviceCode = _console.DeviceCode;
        InsCpu.IoccFunction = function;
        InsCpu.IoccAddress = address;
        InsCpu.IoccModifiers = 0;
        _console.ExecuteIocc();
    }

    [Fact]
    public void ShouldHaveCorrectDeviceId()
    {
        BeforeEachTest();
        Assert.Equal(0x01, _console.DeviceCode);
    }

    [Fact]
    public void ShouldShowPrinterReadyWhenNothingHappening()
    {
        BeforeEachTest();
        SenseDevice(_console);
        Assert.Equal(DeviceConsoleKeyboard.PrinterReady, InsCpu.Acc);
    }

    [Fact]
    public void WritePrintsOneCharacter_BusyUntilRun_ThenPrinterResponseOnLevel4()
    {
        BeforeEachTest();
        InsCpu[0x300] = 0xC4FF;                                               // bits 0-7 = C4; low byte ignored
        Issue(DevFunction.Write, 0x300);
        Assert.True(_console.IsPrinterBusy);
        SenseDevice(_console);
        Assert.Equal(DeviceConsoleKeyboard.PrinterBusyStatus | DeviceConsoleKeyboard.PrinterNotReadyStatus, InsCpu.Acc);

        _console.Run();
        Assert.False(_console.IsPrinterBusy);
        Assert.True(_console.PrinterBuffer.TryDequeue(out var printed));
        Assert.Equal(0xC400, printed);
        Assert.Equal(DeviceConsoleKeyboard.InterruptLevel, _console.ActiveInterrupt.InterruptLevel);
        Assert.Equal(DeviceConsoleKeyboard.Ilsw, _console.ActiveInterrupt.Ilsw);
        SenseDevice(_console, 1);
        Assert.Equal(DeviceConsoleKeyboard.PrinterResponseStatus, InsCpu.Acc);
        Assert.Null(_console.ActiveInterrupt);
        SenseDevice(_console);
        Assert.Equal(0, InsCpu.Acc);
    }

    [Fact]
    public void WriteWhilePrinterBusyIsIgnored()
    {
        BeforeEachTest();
        InsCpu[0x300] = 0xC100;
        InsCpu[0x301] = 0xC200;
        Issue(DevFunction.Write, 0x300);
        Issue(DevFunction.Write, 0x301);                                      // dropped silently
        _console.Run();
        Assert.Single(_console.PrinterBuffer);
        Assert.True(_console.PrinterBuffer.TryDequeue(out var printed));
        Assert.Equal(0xC100, printed);
    }

    [Fact]
    public void KeyboardProtocol_SelectKeyReadDeselects()
    {
        BeforeEachTest();
        Assert.False(_console.PressKey(0x9000));                              // not selected: key ignored

        Issue(DevFunction.Control);                                           // select keyboard
        Assert.True(_console.KeyboardSelected);
        SenseDevice(_console);
        Assert.Equal(DeviceConsoleKeyboard.KeyboardBusyStatus, InsCpu.Acc);

        Assert.True(_console.PressKey(0x9000));                               // operator types 'A'
        Assert.Equal(4, _console.ActiveInterrupt.InterruptLevel);
        SenseDevice(_console, 1);
        Assert.Equal(DeviceConsoleKeyboard.KeyboardResponseStatus | DeviceConsoleKeyboard.KeyboardBusyStatus, InsCpu.Acc);

        Issue(DevFunction.Read, 0x400);
        Assert.Equal(0x9000, InsCpu[0x400]);
        Assert.False(_console.KeyboardSelected);                              // Read removes select status
    }

    [Fact]
    public void ReadWhenNotSelectedStoresNothing()
    {
        BeforeEachTest();
        InsCpu[0x400] = 0x1234;
        Issue(DevFunction.Read, 0x400);
        Assert.Equal(0x1234, InsCpu[0x400]);
    }

    [Fact]
    public void InterruptRequestKey_SetsDswBit2_AndReportsSwitchPosition()
    {
        BeforeEachTest();
        _console.ConsoleSwitchInConsolePosition = true;
        _console.PressInterruptRequest();
        Assert.Equal(4, _console.ActiveInterrupt.InterruptLevel);
        SenseDevice(_console);
        Assert.Equal(DeviceConsoleKeyboard.InterruptRequestStatus | DeviceConsoleKeyboard.ConsoleSwitchStatus, InsCpu.Acc);
    }

    [Fact]
    public void FunctionKeysDeliverSingleBitWords()
    {
        BeforeEachTest();
        Issue(DevFunction.Control);
        _console.PressKey(DeviceConsoleKeyboard.EofKey);
        Issue(DevFunction.Read, 0x400);
        Assert.Equal(0x0008, InsCpu[0x400]);                                  // bit 12
    }
}
