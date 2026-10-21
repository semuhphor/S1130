using System;
using System.Text;

namespace S1130.SystemObjects.Devices;

/* ******************************************************************************************
 * Device: 1403 line printer (device code 10101 = 0x15, interrupt level 4, ILSW bit 4)
 *
 * Unlike the 1132 the 1403 is a block device: the program hands over a whole line and the
 * printer does the rest, interrupting when each step is complete. The 1130 version prints
 * 120 columns (IBM 1130 Functional Characteristics, "1403 Printer"). It is attached through
 * the multiplexer on a real machine.
 *
 *  InitWrite (101)  print a line: the IOCC address is the first of 60 words holding two
 *                   characters each (high byte first, 7-bit 1403 print codes, 0x7F = blank).
 *  Control   (100)  space one line (any modifier).
 *  Write     (001)  skip to a carriage-tape channel: the word at the IOCC address holds the
 *                   channel bits (bit 4 = channel 1 ... bit 15 = channel 12) to stop on.
 *  Sense     (111)  DSW -> ACC; modifier bit 15 resets the interrupt-causing bits.
 *
 * Device status word:
 *   0x8000 parity check      0x4000 transfer complete   0x2000 print complete
 *   0x1000 carriage complete 0x0400 ring check          0x0200 sync check
 *   0x0010 channel 9         0x0008 channel 12          0x0004 carriage busy
 *   0x0002 printer busy      0x0001 not ready
 *
 * A print takes two interrupts: transfer complete (the buffer may be reused) and then
 * print complete. Spacing and skipping interrupt with carriage complete.
 *
 * This is a port of the behaviour of the C reference simulator (ibm1130_prt.c) so programs run
 * identically. Output text uses the same conventions as <see cref="Device1132"/>: lines end
 * with LF, a page break is FF, overprinted rows are separated by CR.
 *
 * Timing: the reference waits 100 time units per step. Here "time" is the number of
 * instructions the CPU has executed (<see cref="EventInterval"/>), or no waiting while the CPU
 * sits in a wait state.
 * ******************************************************************************************/
public class Device1403 : DeviceBase
{
	public override byte DeviceCode { get; } = 0x15;

	public const int InterruptLevel = 4;
	public const ushort Ilsw = 0x0800;

	public const ushort ParityCheck = 0x8000;
	public const ushort TransferComplete = 0x4000;
	public const ushort PrintComplete = 0x2000;
	public const ushort CarriageComplete = 0x1000;
	public const ushort RingCheck = 0x0400;
	public const ushort SyncCheck = 0x0200;
	public const ushort Channel9Bit = 0x0010;
	public const ushort Channel12Bit = 0x0008;
	public const ushort CarriageBusy = 0x0004;
	public const ushort PrinterBusy = 0x0002;
	public const ushort NotReady = 0x0001;

	public const int Columns = 120;
	public const int PageLength = 66;
	public const int MaxOverprint = 20;

	public const int TapeChannel1 = 0x0800;
	public const int TapeChannel9 = 0x0008;
	public const int TapeChannel12 = 0x0001;
	private const int TapeMask = 0x0FFF;

	/// <summary>The print codes of the 1403 print chain and the character each one prints.</summary>
	private static readonly (char Ascii, byte Code)[] Chain =
	{
		('A', 0x64), ('B', 0x25), ('C', 0x26), ('D', 0x67), ('E', 0x68), ('F', 0x29), ('G', 0x2A), ('H', 0x6B),
		('I', 0x2C), ('J', 0x58), ('K', 0x19), ('L', 0x1A), ('M', 0x5B), ('N', 0x1C), ('O', 0x5D), ('P', 0x5E),
		('Q', 0x1F), ('R', 0x20), ('S', 0x0D), ('T', 0x0E), ('U', 0x4F), ('V', 0x10), ('W', 0x51), ('X', 0x52),
		('Y', 0x13), ('Z', 0x54), ('0', 0x49), ('1', 0x40), ('2', 0x01), ('3', 0x02), ('4', 0x43), ('5', 0x04),
		('6', 0x45), ('7', 0x46), ('8', 0x07), ('9', 0x08), (' ', 0x7F), ('.', 0x6E), ('(', 0x57), ('+', 0x6D),
		('&', 0x15), ('$', 0x62), ('*', 0x23), (')', 0x2F), ('-', 0x61), ('/', 0x4C), (',', 0x16), ('\'', 0x0B),
		('=', 0x4A),
	};

	private readonly object _lock = new object();
	private readonly StringBuilder _output = new StringBuilder();
	private readonly int[] _carriageTape = new int[PageLength];
	private readonly char[][] _rows = new char[MaxOverprint][];
	private readonly int[] _rowLength = new int[MaxOverprint];
	private readonly int[] _overprints = new int[Columns];

	private ushort _dsw;
	private int _row;                                        // current line on the page, 0-based
	private int _queuedNewlines;
	private int _maxOverprint;
	private int _skipTarget;
	private bool _transferring, _printing, _skipping, _spacing;
	private bool _lineOpen;                                  // text has been written on the current line
	private bool _pageHasText;                               // output written since the last form feed
	private ulong _lastEvent;
	private bool _online = true;

	public Device1403(ICpu cpu)
	{
		CpuInstance = cpu;
		for (var i = 0; i < MaxOverprint; i++) _rows[i] = new char[Columns];
		_carriageTape[1] = TapeChannel1;                     // channel 1 = top of form (line 2)
		_carriageTape[61] = TapeChannel12;                   // channel 12 = bottom of form (line 62)
		UpdateChannelBits();
	}

	/// <summary>Instructions the CPU must run between steps (0 = no waiting).</summary>
	public int EventInterval { get; set; } = 100;

	/// <summary>False when the printer is out of paper or offline; a running operation then stops.</summary>
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

	/// <summary>True if an operation was cancelled because the printer went offline.</summary>
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

	/// <summary>The channel bits requested by the last skip (diagnostic use).</summary>
	public int LastSkipTarget => _skipTarget;

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
				case DevFunction.InitWrite:                  // print a line
					CollectLine(CpuInstance.IoccAddress);
					_transferring = true;
					_dsw |= PrinterBusy;
					_lastEvent = CpuInstance.InstructionCount;
					break;

				case DevFunction.Control:                    // space one line
					if (_skipping) break;                    // the reference reports an error here; ignore
					_spacing = true;
					_dsw |= CarriageBusy;
					_lastEvent = CpuInstance.InstructionCount;
					break;

				case DevFunction.Write:                      // skip to channel
					if (_spacing) break;
					_skipping = true;
					_skipTarget = CpuInstance[CpuInstance.IoccAddress] & TapeMask;
					_dsw |= CarriageBusy;
					_lastEvent = CpuInstance.InstructionCount;
					break;

				case DevFunction.SenseDevice:
					CpuInstance.Acc = _dsw;
					if ((CpuInstance.IoccModifiers & 1) != 0)
					{
						_dsw &= unchecked((ushort)~(ParityCheck | TransferComplete | PrintComplete
							| CarriageComplete | RingCheck | SyncCheck));
						DeactivateInterrupt(CpuInstance);
					}
					break;
			}
		}
	}

	/// <summary>Services the printer when the next step is due.</summary>
	public override void Run()
	{
		lock (_lock)
		{
			if ((_transferring || _printing || _skipping || _spacing) && StepIsDue())
			{
				_lastEvent = CpuInstance.InstructionCount;
				Service();
			}
		}
		base.Run();
	}

	private bool StepIsDue()
	{
		return EventInterval <= 0
			|| CpuInstance.Wait
			|| CpuInstance.InstructionCount - _lastEvent >= (ulong)EventInterval;
	}

	private void Service()
	{
		if ((_dsw & NotReady) != 0)                          // went offline: cancel the operation
		{
			FormsCheck = true;
			_transferring = _printing = _skipping = _spacing = false;
		}
		else if (_transferring)                              // buffer has been taken over
		{
			_transferring = false;
			_printing = true;
			_dsw |= TransferComplete;
			Interrupt();
		}
		else if (_printing)
		{
			EmitRows();                                      // the line is on the paper now
			_printing = false;
			_dsw &= unchecked((ushort)~PrinterBusy);
			_dsw |= PrintComplete;
			Interrupt();
		}
		else if (_skipping)
		{
			NewPage();                                       // any skip to a channel starts a new page
			_skipping = false;
			_dsw &= unchecked((ushort)~CarriageBusy);
			_dsw |= CarriageComplete;
			Interrupt();
		}
		else if (_spacing)
		{
			FlushLine(spacing: true, skipping: false);
			_spacing = false;
			_dsw &= unchecked((ushort)~CarriageBusy);
			_dsw |= CarriageComplete;
			Interrupt();
		}

		UpdateChannelBits();
	}

	private void UpdateChannelBits()
	{
		_dsw &= unchecked((ushort)~(Channel9Bit | Channel12Bit));
		if ((_carriageTape[_row] & TapeChannel9) != 0) _dsw |= Channel9Bit;
		if ((_carriageTape[_row] & TapeChannel12) != 0) _dsw |= Channel12Bit;
	}

	private void Interrupt()
	{
		ActivateInterrupt(CpuInstance, InterruptLevel, Ilsw);
	}

	/// <summary>Translates the 60 core words at <paramref name="address"/> and stores the characters in the print rows.</summary>
	private void CollectLine(int address)
	{
		var word = 0;
		for (var column = 0; column < Columns; column++)
		{
			int code;
			if ((column & 1) == 0)
			{
				word = CpuInstance[address++];
				code = (word >> 8) & 0x7F;
			}
			else
			{
				code = word & 0x7F;
			}

			var ch = ' ';
			foreach (var entry in Chain)
			{
				if (entry.Code == code)
				{
					ch = entry.Ascii;
					break;
				}
			}
			if (ch <= ' ') continue;

			var row = _overprints[column];
			if (row >= MaxOverprint) continue;

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

	/// <summary>Ends the current line, then writes a form feed (unless the page is already fresh), and returns to the top of form.</summary>
	private void NewPage()
	{
		EmitRows();
		_lineOpen = false;
		_queuedNewlines = 0;
		if (_pageHasText) _output.Append('\f');
		_pageHasText = false;
		_row = 1;                                            // channel 1 = line 2
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
				_pageHasText = !skipping;
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
			if (r > 0) _output.Append('\r');
			_output.Append(_rows[r], 0, _rowLength[r]);
		}

		Array.Clear(_overprints);
		Array.Clear(_rowLength);
		_maxOverprint = 0;
		_lineOpen = true;
		_pageHasText = true;
	}
}
