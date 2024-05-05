using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace S1130.SystemObjects.Devices;

/* ******************************************************************************************
	 * Device: 1053 Console Printer / Keyboard / Console 
     * 
	 * This device class emulates a 1052 Console Printer and Keyboard (stolen from 029 card punch).
     *
     * This device accepts the following commands:
	 * - Read: Red a key from the keyboard
	 * - Control: 
	 * - Write: Write a data or control character
	 * - Sense device: Return the device's status word
	 * 
	 * The status word is as follows:
	 *		1... .... .... ....		Printer Reponse Interrupt (interrupt 4) 
	 *		.1.. .... .... ....		Keyboard Response Interrupt (interrupt 4)
	 *		..1. .... .... ....		Interrupt Request Key
	 *		...0 .... .... ....		0 - Console/Keyboard in Keyboard Position
     *		...1 .... .... ....		1 - Console/Keyboard in Console Position
	 *		.... 1... .... ....		Printer Busy
	 *		.... .1.. .... ....		Printer Not Ready
	 *		.... ..1. .... ....		Keyboard Ready
	 * ******************************************************************************************/
public class DeviceConsoleKeyboard : DeviceBase
{
	public override byte DeviceCode { get; } = 0x01;
	public const ushort Ilsw = 0x1000;

	public const ushort PrinterReady = 0x0000;
	public const ushort PrinterBusy = 0x0800;

	
	public ConcurrentQueue<ushort> PrinterBuffer = new ConcurrentQueue<ushort>();

	public DeviceConsoleKeyboard(ICpu cpu)
	{
		CpuInstance = cpu;
	}

	public override void ExecuteIocc()
	{
		switch (CpuInstance.IoccFunction)
		{
			case DevFunction.SenseDevice:
				CpuInstance.Acc = PrinterReady;
				break;
			case DevFunction.Read:
				CpuInstance.Acc = 0;
				break;
			case DevFunction.Write:

				break;
			case DevFunction.Control:
				break;
		}
	}
}