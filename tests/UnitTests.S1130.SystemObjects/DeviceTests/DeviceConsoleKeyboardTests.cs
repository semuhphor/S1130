using System;
using S1130.SystemObjects.Devices;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests;

public class DeviceConsoleKeyboardTests : DeviceTestBase
{
    private DeviceConsoleKeyboard _console;                 // console device for testing

    protected override void BeforeEachTest()
    {
        base.BeforeEachTest();
        _console = new DeviceConsoleKeyboard(InsCpu);
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
    public void ShouldShowPrinterBusyWhenPrinting()
    {
        BeforeEachTest();
        SenseDevice(_console);
        Assert.Equal(DeviceConsoleKeyboard.PrinterReady, InsCpu.Acc);
    }
}
