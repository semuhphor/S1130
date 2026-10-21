using System;
using System.Text;

namespace S1130.SystemObjects.Devices;

/* ******************************************************************************************
 * Device: 1132 line printer (device code 00110, interrupt level 1)
 *
 * The 1132 is a drum printer driven by the program one character at a time (IBM 1130
 * Functional Characteristics, "1132 Printer"). A rotating drum carries 48 characters. As each
 * character comes under the hammers an "emitter" signal interrupts the program, which must
 * then load the print buffer (core words 32-39, one bit per column, 120 columns) with the
 * columns that contain that character. The hammers fire, the drum moves one position, and the
 * next emitter interrupt arrives. After one drum revolution the line is complete and the
 * program spaces or skips the carriage.
 *
 *  Read    (010)  stores the character now under the hammers (EBCDIC in the high byte) at the
 *                 IOCC address; while not printing the drum steps on after every read.
 *  Control (100)  modifier bits: 0x80 start printer, 0x40 stop printer, 0x04 start carriage
 *                 (skip), 0x02 stop carriage, 0x01 space one line.
 *  Sense   (111)  DSW -> ACC; modifier bit 15 resets the emitter, skip and space responses.
 *
 * Device status word:
 *   0x8000 read emitter response   0x4000 skip response   0x2000 space response
 *   0x1000 carriage busy           0x0800 print scan check   0x0400 not ready
 *   0x0200 printer busy            0x00FF carriage-tape channels 1,2,3,4,5,6,9,12 (bits 8-15)
 *
 * This is a port of the behaviour of the C reference simulator (ibm1130_prt.c) so programs run
 * identically. Output is collected as text: lines end with LF, a page break is FF, overprinted
 * rows are separated by CR.
 *
 * Timing: the reference waits 1250 time units between emitter events. Here "time" is the
 * number of instructions the CPU has executed (<see cref="EventInterval"/>), or no waiting at all
 * while the CPU sits in a wait state, so a program always has the same chance to refill
 * the buffer as on the reference.
 * ******************************************************************************************/
public class Device1132 : DeviceBase
{
	public override byte DeviceCode { get; } = 0x06;

	public const int InterruptLevel = 1;
	public const ushort Ilsw = 0x8000;                       // level-1 ILSW bit 0 = 1132 printer

	public const ushort EmitterResponse = 0x8000;
	public const ushort SkipResponse = 0x4000;
	public const ushort SpaceResponse = 0x2000;
	public const ushort CarriageBusy = 0x1000;
	public const ushort PrintScanCheck = 0x0800;
	public const ushort NotReady = 0x0400;
	public const ushort PrinterBusy = 0x0200;
	public const ushort ChannelMask = 0x00FF;

	public const ushort StartPrinter = 0x80;
	public const ushort StopPrinter = 0x40;
	public const ushort StartCarriage = 0x04;
	public const ushort StopCarriage = 0x02;
	public const ushort Space = 0x01;

	public const int Columns = 120;
	public const int PageLength = 66;
	public const int PrintBufferAddress = 32;                // 8 words = 128 bits, 120 used
	public const int MaxOverprint = 20;

	private const int Channel1 = 0x0800, Channel6 = 0x0040, Channel9 = 0x0008, Channel12 = 0x0001;
	private const int Channel7 = 0x0020, Channel8 = 0x0010, Channel10 = 0x0004, Channel11 = 0x0002;
	private const int Channels1132 = 0x0FFF & ~(Channel7 | Channel8 | Channel10 | Channel11);

	/// <summary>The drum, in rotation order: character and its EBCDIC code.</summary>
	private static readonly (char Ascii, byte Ebcdic)[] Wheel =
	{
		('A', 0xC1), ('B', 0xC2), ('C', 0xC3), ('D', 0xC4), ('F', 0xC6), ('H', 0xC8), ('I', 0xC9), ('S', 0xE2),
		('T', 0xE3), ('U', 0xE4), ('V', 0xE5), ('1', 0xF1), ('2', 0xF2), ('3', 0xF3), ('4', 0xF4), ('5', 0xF5),
		('6', 0xF6), ('7', 0xF7), ('8', 0xF8), ('9', 0xF9), ('0', 0xF0), ('=', 0x7E), ('$', 0x5B), ('.', 0x4B),
		('\'', 0x7D), (',', 0x6B), (')', 0x5D), ('-', 0x60), ('(', 0x4D), ('+', 0x4E), ('/', 0x61), ('*', 0x5C),
		('&', 0x50), ('J', 0xD1), ('K', 0xD2), ('L', 0xD3), ('M', 0xD4), ('N', 0xD5), ('O', 0xD6), ('P', 0xD7),
		('Q', 0xD8), ('R', 0xD9), ('E', 0xC5), ('G', 0xC7), ('W', 0xE6), ('X', 0xE7), ('Y', 0xE8), ('Z', 0xE9),
	};

	/// <summary>Number of characters on the drum (48).</summary>
	public static int WheelCharacters => Wheel.Length;

	private readonly object _lock = new object();
	private readonly StringBuilder _output = new StringBuilder();
	private readonly int[] _carriageTape = new int[PageLength];
	private readonly char[][] _rows = new char[MaxOverprint][];
	private readonly int[] _rowLength = new int[MaxOverprint];
	private readonly int[] _overprints = new int[Columns];

	private ushort _dsw;
	private int _wheelPosition;
	private int _row;                                        // current line on the page, 0-based
	private int _queuedNewlines;
	private int _maxOverprint;
	private bool _printing, _skipping, _spacing;
	private bool _lineOpen;                                  // text has been written on the current line
	private ulong _lastEvent;
	private bool _online = true;

	public Device1132(ICpu cpu)
	{
		CpuInstance = cpu;
		for (var i = 0; i < MaxOverprint; i++) _rows[i] = new char[Columns];
		_carriageTape[1] = Channel1;                         // channel 1 = top of form (line 2)
		_carriageTape[61] = Channel12;                       // channel 12 = bottom of form (line 62)
		_dsw = CarriageTapeBits(_carriageTape[_row]);
	}

	/// <summary>Instructions the CPU must run between emitter / carriage events (0 = no waiting).</summary>
	public int EventInterval { get; set; } = 1250;

	/// <summary>False when the printer is out of paper or offline; a running operation then stops with a forms check.</summary>
	public bool Online
	{
		get { lock (_lock) { return _online; } }
		set
		{
			lock (_lock)
			{
				_online = value;
				if (value) _dsw &= unchecked((ushort)~NotReady); else _dsw |= NotReady;
			}
		}
	}

	/// <summary>True if the last operation stopped because the buffer was not set in time (print check).</summary>
	public bool PrintCheck { get; private set; }

	/// <summary>True if an operation was cancelled because the printer went offline (forms check).</summary>
	public bool FormsCheck { get; private set; }

	/// <summary>Everything printed so far (see the class description for the control characters).</summary>
	public string PrintedText
	{
		get { lock (_lock) { return _output.ToString(); } }
	}

	/// <summary>Removes and returns what has been printed so far, for the host to show or store.</summary>
	public string TakePrintedText()
	{
		lock (_lock)
		{
			var text = _output.ToString();
			_output.Clear();
			return text;
		}
	}

	/// <summary>The current device status word (test and diagnostic use).</summary>
	public ushort StatusWord
	{
		get { lock (_lock) { return _dsw; } }
	}

	public override void ExecuteIocc()
	{
		lock (_lock)
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.Read:
					CpuInstance[CpuInstance.IoccAddress] = (ushort)(Wheel[_wheelPosition].Ebcdic << 8);
					if (!_printing) _wheelPosition = (_wheelPosition + 1) % Wheel.Length;
					break;
				case DevFunction.SenseDevice:
					CpuInstance.Acc = _dsw;
					if ((CpuInstance.IoccModifiers & 1) != 0)
					{
						_dsw &= unchecked((ushort)~(EmitterResponse | SkipResponse | SpaceResponse));
						DeactivateInterrupt(CpuInstance);
					}
					break;
				case DevFunction.Control:
					Control(CpuInstance.IoccModifiers);
					break;
			}
		}
	}

	private void Control(int modifier)
	{
		if ((modifier & StartPrinter) != 0) _printing = true;
		if ((modifier & StopPrinter) != 0)
		{
			_printing = false;
			EmitRows();                                      // the line is on the paper now
		}
		if ((modifier & StartCarriage) != 0) _skipping = true;
		if ((modifier & StopCarriage) != 0) _skipping = false;
		if ((modifier & Space) != 0) _spacing = true;

		_lastEvent = CpuInstance.InstructionCount;           // restart the event timer
		if (_printing || _skipping || _spacing) _dsw |= PrinterBusy; else _dsw &= unchecked((ushort)~PrinterBusy);
		if (_skipping || _spacing) _dsw |= CarriageBusy; else _dsw &= unchecked((ushort)~CarriageBusy);
	}

	/// <summary>Services the drum and carriage when the next event is due.</summary>
	public override void Run()
	{
		lock (_lock)
		{
			if ((_printing || _skipping || _spacing) && EventIsDue())
			{
				_lastEvent = CpuInstance.InstructionCount;
				Service();
			}
		}
		base.Run();
	}

	private bool EventIsDue()
	{
		return EventInterval <= 0
			|| CpuInstance.Wait                              // nothing can race the hardware while the CPU waits
			|| CpuInstance.InstructionCount - _lastEvent >= (ulong)EventInterval;
	}

	private void Service()
	{
		if ((_dsw & NotReady) != 0)                          // went offline: cancel the operation
		{
			FormsCheck = true;
			_printing = _skipping = _spacing = false;
			EmitRows();
			return;
		}

		if (_spacing)
		{
			FlushLine(spacing: true, skipping: false);
			_dsw &= unchecked((ushort)~(ChannelMask | PrinterBusy | CarriageBusy));
			_dsw |= (ushort)(CarriageTapeBits(_carriageTape[_row]) | SpaceResponse);
			_spacing = false;
			Interrupt();
		}

		if (_skipping)
		{
			do
			{
				FlushLine(spacing: true, skipping: true);
				_dsw &= unchecked((ushort)~ChannelMask);
				_dsw |= CarriageTapeBits(_carriageTape[_row]);
			} while ((_carriageTape[_row] & Channels1132) == 0);   // slew directly to a tape punch
			_dsw |= (ushort)(CarriageTapeBits(_carriageTape[_row]) | SkipResponse);
			Interrupt();
		}

		if (_printing)
		{
			if (!FireHammers(Wheel[_wheelPosition].Ascii))
			{
				PrintCheck = true;                           // buffer was not set in time (the reference, too, only lights the lamp)
				_printing = _skipping = _spacing = false;
				EmitRows();
				return;
			}
			_wheelPosition = (_wheelPosition + 1) % Wheel.Length;
			_dsw |= EmitterResponse;
			Interrupt();
		}

		if (_printing || _skipping || _spacing) _dsw |= PrinterBusy; else _dsw &= unchecked((ushort)~PrinterBusy);
	}

	private void Interrupt()
	{
		ActivateInterrupt(CpuInstance, InterruptLevel, Ilsw);
	}

	/// <summary>Carriage-tape channels as the DSW shows them: channels 1-6 in bits 8-13, 9 in bit 14, 12 in bit 15.</summary>
	private static ushort CarriageTapeBits(int bits)
	{
		return (ushort)(((bits & (Channel1 | 0x0400 | 0x0200 | 0x0100 | 0x0080 | Channel6)) >> 4)
			| ((bits & Channel9) >> 3)
			| (bits & Channel12));
	}

	/// <summary>
	/// Fires the hammers for one drum character at every column whose bit is set in the print buffer.
	/// Returns true if the last buffer word has its low bit set, which the hardware uses to tell that
	/// the program has set the buffer (all bits set means "buffer ready").
	/// </summary>
	private bool FireHammers(char ch)
	{
		var address = PrintBufferAddress;
		int mask = 0, word = 0;
		for (var column = 0; column < Columns; column++)
		{
			if (mask == 0)
			{
				mask = 0x8000;
				word = CpuInstance[address++];
			}
			if ((word & mask) != 0)
			{
				var row = _overprints[column];
				if (row < MaxOverprint)
				{
					if (_rowLength[row] <= column)
					{
						if (_rowLength[row] == 0) Array.Fill(_rows[row], ' ');
						_rowLength[row] = column + 1;
					}
					_rows[row][column] = ch;
					_overprints[column]++;
					_maxOverprint = Math.Max(_maxOverprint, _overprints[column]);
				}
			}
			mask >>= 1;
		}
		return (word & 1) != 0;
	}

	/// <summary>Ends the current line (writing any collected rows) and advances the carriage one line.</summary>
	private void FlushLine(bool spacing, bool skipping)
	{
		EmitRows();
		if (!(spacing || _lineOpen)) return;

		_row = (_row + 1) % PageLength;

		if (!_lineOpen)                                      // spacing only
		{
			if (_row == 0 && _queuedNewlines != 0)
			{
				_output.Append('\n');
				if (skipping) _output.Append('\f');          // crossed a page boundary while skipping
				_queuedNewlines = 0;
			}
			else
			{
				_queuedNewlines++;
			}
			return;
		}

		_lineOpen = false;
		_queuedNewlines++;
	}

	/// <summary>
	/// Writes the rows collected since the last write to the output, so a printed line shows at
	/// once even if the carriage never moves again. Later prints on the same line follow after CR.
	/// </summary>
	private void EmitRows()
	{
		if (_maxOverprint == 0) return;

		if (_lineOpen) _output.Append('\r');
		else for (; _queuedNewlines > 0; _queuedNewlines--) _output.Append('\n');

		for (var r = 0; r < _maxOverprint; r++)
		{
			if (r > 0) _output.Append('\r');                 // overprinted rows are separated by CR
			_output.Append(_rows[r], 0, _rowLength[r]);
		}

		Array.Clear(_overprints);
		Array.Clear(_rowLength);
		_maxOverprint = 0;
		_lineOpen = true;
	}
}
