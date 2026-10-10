using System.Collections.Concurrent;

namespace S1130.SystemObjects.Devices;

/* ******************************************************************************************
 * Device: 1131 console printer and keyboard (device code 00001, interrupt level 4)
 *
 * Direct-program-control device (IBM 1130 Functional Characteristics, "Console").
 *
 *  Write   (001)  bits 0-7 of the addressed word go to the printer, one character per IOCC.
 *                 Bit 6 = case (0 lower, 1 upper), bit 7 = 0 data / 1 control character.
 *                 A write issued while the printer is busy is silently ignored.
 *                 When the character has been printed the printer requests a level-4 interrupt
 *                 (DSW bit 0, printer response).
 *  Control (100)  selects the keyboard (KB SELECT light); a key can then be entered.
 *  Read    (010)  stores the latched keyboard character at the addressed word and removes the
 *                 keyboard from select status. If the keyboard is not selected nothing is stored.
 *  Sense   (111)  DSW -> ACC; modifier bit 15 resets all keyboard/printer responses.
 *
 * Device status word:
 *   bit 0  1... .... .... ....  Printer response      (level 4)
 *   bit 1  .1.. .... .... ....  Keyboard response     (level 4)
 *   bit 2  ..1. .... .... ....  Interrupt request key (level 4)
 *   bit 3  ...1 .... .... ....  Console/keyboard switch in CONSOLE position
 *   bit 4  .... 1... .... ....  Printer busy
 *   bit 5  .... .1.. .... ....  Printer not ready
 *   bit 6  .... ..1. .... ....  Keyboard busy (selected)
 *
 * Keyboard characters arrive in IBM card code (same image as a 1442 column, e.g. 'A' = /9000).
 * The EOF, backspace and ERASE FIELD keys deliver words with only bit 12, 13 or 14 set.
 * ******************************************************************************************/
public class DeviceConsoleKeyboard : DeviceBase
{
	public override byte DeviceCode { get; } = 0x01;

	public const int InterruptLevel = 4;
	public const ushort Ilsw = 0x4000;                       // level-4 ILSW bit 1 = console

	public const ushort PrinterResponseStatus = 0x8000;
	public const ushort KeyboardResponseStatus = 0x4000;
	public const ushort InterruptRequestStatus = 0x2000;
	public const ushort ConsoleSwitchStatus = 0x1000;
	public const ushort PrinterBusyStatus = 0x0800;
	public const ushort PrinterNotReadyStatus = 0x0400;
	public const ushort KeyboardBusyStatus = 0x0200;

	public const ushort PrinterReady = 0x0000;
	public const ushort PrinterBusy = PrinterBusyStatus;

	public const ushort EofKey = 0x0008;                     // bit 12
	public const ushort BackspaceKey = 0x0004;               // bit 13
	public const ushort EraseFieldKey = 0x0002;              // bit 14

	public const ushort UpperCaseBit = 0x0200;               // bit 6 of the print word
	public const ushort ControlCharacterBit = 0x0100;        // bit 7 of the print word

	/// <summary>Characters (bits 0-7 of each print word) that have been printed.</summary>
	public ConcurrentQueue<ushort> PrinterBuffer = new ConcurrentQueue<ushort>();

	/// <summary>Position of the two-position CONSOLE/KEYBOARD switch (true = CONSOLE).</summary>
	public bool ConsoleSwitchInConsolePosition { get; set; }

	/// <summary>False when the printer is out of forms.</summary>
	public bool PrinterHasForms { get; set; } = true;

	private bool _printerBusy;
	private ushort _printing;
	private bool _printerResponse;
	private bool _keyboardSelected;
	private bool _keyboardResponse;
	private bool _interruptRequest;
	private ushort _keyboardCharacter;

	public DeviceConsoleKeyboard(ICpu cpu)
	{
		CpuInstance = cpu;
	}

	public bool KeyboardSelected => _keyboardSelected;
	public bool IsPrinterBusy => _printerBusy;

	public override void ExecuteIocc()
	{
		switch (CpuInstance.IoccFunction)
		{
			case DevFunction.SenseDevice:
				SenseDevice();
				break;
			case DevFunction.Read:
				ReadKeyboard();
				break;
			case DevFunction.Write:
				WritePrinter();
				break;
			case DevFunction.Control:
				_keyboardSelected = true;                     // KB SELECT light on
				break;
		}
	}

	private void SenseDevice()
	{
		ushort dsw = 0;
		if (_printerResponse) dsw |= PrinterResponseStatus;
		if (_keyboardResponse) dsw |= KeyboardResponseStatus;
		if (_interruptRequest) dsw |= InterruptRequestStatus;
		if (ConsoleSwitchInConsolePosition) dsw |= ConsoleSwitchStatus;
		if (_printerBusy) dsw |= PrinterBusyStatus;
		if (!PrinterHasForms || _printerBusy) dsw |= PrinterNotReadyStatus;
		if (_keyboardSelected) dsw |= KeyboardBusyStatus;
		CpuInstance.Acc = dsw;

		if ((CpuInstance.IoccModifiers & 1) != 0)
		{
			_printerResponse = _keyboardResponse = _interruptRequest = false;
			DeactivateInterrupt(CpuInstance);
		}
	}

	private void WritePrinter()
	{
		if (_printerBusy || !PrinterHasForms)
		{
			return;                                           // ignored, no indication to the program
		}
		_printing = (ushort)(CpuInstance[CpuInstance.IoccAddress] & 0xFF00);
		_printerBusy = true;
	}

	private void ReadKeyboard()
	{
		if (!_keyboardSelected)
		{
			return;                                           // "no bits are entered into core storage"
		}
		CpuInstance[CpuInstance.IoccAddress] = _keyboardCharacter;
		_keyboardCharacter = 0;
		_keyboardSelected = false;
	}

	/// <summary>Completes any character in progress at the printer and raises the printer response.</summary>
	public override void Run()
	{
		if (_printerBusy)
		{
			PrinterBuffer.Enqueue(_printing);
			_printerBusy = false;
			_printerResponse = true;
			ActivateInterrupt(CpuInstance, InterruptLevel, Ilsw);
		}
		base.Run();
	}

	/// <summary>
	/// Operator presses a character or function key. Only honoured while the keyboard is selected;
	/// the character is latched and a keyboard-response interrupt is requested.
	/// </summary>
	public bool PressKey(ushort cardCode)
	{
		if (!_keyboardSelected)
		{
			return false;
		}
		_keyboardCharacter = cardCode;
		_keyboardResponse = true;
		ActivateInterrupt(CpuInstance, InterruptLevel, Ilsw);
		return true;
	}

	/// <summary>Operator presses INT REQ: restores the keyboard and requests a level-4 interrupt.</summary>
	public void PressInterruptRequest()
	{
		_keyboardSelected = false;
		_keyboardCharacter = 0;
		_interruptRequest = true;
		ActivateInterrupt(CpuInstance, InterruptLevel, Ilsw);
	}
}
