using System.Text;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Devices.Disks;

namespace S1130.Site.Machines;

/// <summary>
/// One visitor's private IBM 1130: the CPU, the thread that runs it, the working disk on the built-in
/// 2310 and the printout collected from the printers.
///
/// The run thread also services the devices (calls their <c>Run</c>) every few instructions, so
/// device timing follows the instruction count and no device waits for an OS timer tick. Every CPU
/// access from outside goes through <see cref="_lock"/>. The run thread holds the lock for a batch
/// of instructions at a time, so callers wait at most one batch.
/// </summary>
public sealed class MachineHost : IDisposable
{
	public const string WorkDiskName = "work.dsk";
	private const int StepsPerBatch = 20_000;
	private const int StepsPerDeviceService = 32;							// well under the 1442/1403 event interval (100)
	private const int MaxPrintoutChars = 2_000_000;

	private readonly Lock _lock = new();
	private readonly AutoResetEvent _wake = new(false);
	private readonly StringBuilder _printout = new();
	private readonly string _diskPath;
	private readonly Thread _thread;
	private Cpu _cpu;
	private DiskImageCartridge? _disk;
	private long _printoutStart;											// characters dropped from the front of _printout
	private volatile bool _running;
	private volatile bool _disposed;
	private string? _error;
	private string? _deviceError;
	private long _lastUsedTicks = DateTime.UtcNow.Ticks;

	/// <param name="sessionDirectory">Folder for this session's files; the working disk lives in its <c>disks</c> folder.</param>
	public MachineHost(string sessionDirectory)
	{
		_diskPath = Path.Combine(sessionDirectory, "disks", WorkDiskName);
		_cpu = new Cpu();
		_thread = new Thread(RunLoop) { IsBackground = true, Name = "S1130 machine" };
		_thread.Start();
	}

	public DateTime LastUsedUtc => new(Interlocked.Read(ref _lastUsedTicks), DateTimeKind.Utc);

	public void Touch() => Interlocked.Exchange(ref _lastUsedTicks, DateTime.UtcNow.Ticks);

	/// <summary>
	/// Cold-starts DMS from the working disk, making the disk from <paramref name="masterDiskPath"/> first if
	/// this session has none yet. The machine is reset first; cards already in the hopper stay there.
	/// </summary>
	public void Boot(string masterDiskPath)
	{
		lock (_lock)
		{
			ResetLocked();
			if (_disk == null)
			{
				if (!File.Exists(_diskPath))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(_diskPath)!);
					File.Copy(masterDiskPath, _diskPath);
				}
				_disk = DiskImageCartridge.Load(_diskPath);
				_cpu.BuiltInDisk.Mount(_disk);
			}
			ProgramLoad.ColdStart(_cpu, ColdStartKind.Dms, 0);
			_running = true;
		}
		_wake.Set();
	}

	/// <summary>Places cards at the back of the 1442 hopper and readies the reader. Returns the hopper count.</summary>
	public int LoadDeck(IEnumerable<ICard> cards)
	{
		lock (_lock)
		{
			var reader = Reader;
			foreach (var card in cards) reader.Hopper.Enqueue(card);
			reader.Start();
			return UnreadCards;
		}
	}

	/// <summary>Console PROGRAM START: run from the current IAR, leaving any wait state.</summary>
	public void Start()
	{
		lock (_lock)
		{
			_error = null;
			_cpu.Wait = false;
			_running = true;
		}
		_wake.Set();
	}

	/// <summary>Console PROGRAM STOP.</summary>
	public void Stop()
	{
		lock (_lock) { _running = false; }
	}

	/// <summary>Console RESET: clears the processor and devices, keeping the disk and the hopper.</summary>
	public void Reset()
	{
		lock (_lock) { ResetLocked(); }
	}

	public MachineSnapshot Snapshot()
	{
		lock (_lock)
		{
			return new MachineSnapshot(
				PoweredOn: true,
				Running: _running,
				Wait: _cpu.Wait,
				Iar: _cpu.Iar,
				Acc: _cpu.Acc,
				Ext: _cpu.Ext,
				Xr1: _cpu.Xr[1],
				Xr2: _cpu.Xr[2],
				Xr3: _cpu.Xr[3],
				Carry: _cpu.Carry,
				Overflow: _cpu.Overflow,
				ActiveLevels: _cpu.CurrentInterrupt.Select(i => i.InterruptLevel).Distinct().Order().ToArray(),
				InstructionCount: _cpu.InstructionCount,
				HopperCount: UnreadCards,
				DiskMounted: _disk != null,
				Error: _error ?? _deviceError);
		}
	}

	/// <summary>Printer output from character position <paramref name="since"/> on (0 = from the start).</summary>
	public PrintoutChunk ReadPrintout(long since)
	{
		lock (_lock)
		{
			CollectPrinterOutput();
			var start = Math.Clamp(since, _printoutStart, _printoutStart + _printout.Length);
			var text = _printout.ToString((int)(start - _printoutStart), (int)(_printoutStart + _printout.Length - start));
			return new PrintoutChunk(start, text, start + text.Length);
		}
	}

	/// <summary>Tears off the printout: later reads start after everything printed so far.</summary>
	public void ClearPrintout()
	{
		lock (_lock)
		{
			CollectPrinterOutput();
			_printoutStart += _printout.Length;
			_printout.Clear();
		}
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		_running = false;
		_wake.Set();
		_thread.Join(TimeSpan.FromSeconds(2));
		lock (_lock)
		{
			UnmountDisk();
		}
		_wake.Dispose();
	}

	private Device1442 Reader => (Device1442)_cpu.Devices[0x02];

	private int UnreadCards => Reader.Hopper.Count + (Reader.CardAtReadStation != null ? 1 : 0);	// a read card moves on to the punch station

	private void ResetLocked()
	{
		_running = false;
		_error = null;
		_deviceError = null;
		CollectPrinterOutput();
		var hopper = (Reader.CardAtReadStation is { } atStation ? new[] { atStation } : [])
			.Concat(Reader.Hopper).ToArray();									// unread cards go back to the new reader's hopper
		var disk = _disk;
		if (disk != null) _cpu.BuiltInDisk.UnMount();						// saves anything written

		_cpu = new Cpu();
		foreach (var card in hopper) Reader.Hopper.Enqueue(card);
		if (hopper.Length > 0) Reader.Start();
		if (disk != null) _cpu.BuiltInDisk.Mount(disk);
	}

	private void UnmountDisk()
	{
		if (_disk == null) return;
		_cpu.BuiltInDisk.UnMount();
		_disk = null;
	}

	private void RunLoop()
	{
		while (!_disposed)
		{
			if (!_running)
			{
				_wake.WaitOne(100);
				continue;
			}
			bool waiting;
			lock (_lock)
			{
				waiting = RunBatch();
			}
			if (waiting) Thread.Sleep(1);									// wait state: let devices raise interrupts without burning a core
		}
	}

	private bool RunBatch()													// true if the CPU is in a wait state
	{
		try
		{
			for (var i = 0; i < StepsPerBatch && _running; i++)
			{
				if (i % StepsPerDeviceService == 0) RunDevices();
				if (!_cpu.Step())
				{
					RunDevices();												// in a wait every device event is due now
					_cpu.Step();												// take an interrupt a device just raised
					if (_cpu.Wait) return true;
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			_error = $"Processor stopped: {ex.Message}";
			_running = false;
			return true;
		}
		finally
		{
			CollectPrinterOutput();
		}
	}

	private void RunDevices()
	{
		foreach (var device in _cpu.Devices)
		{
			if (device == null) continue;
			try
			{
				device.Run();
			}
			catch (Exception ex)												// a faulty device must not stop the others
			{
				_deviceError = $"Device {device.DeviceCode:X2}: {ex.Message}";
			}
		}
	}

	private void CollectPrinterOutput()
	{
		if (_cpu.Devices[0x15] is Device1403 p1403) _printout.Append(p1403.TakePrintedText());
		if (_cpu.Devices[0x06] is Device1132 p1132) _printout.Append(p1132.TakePrintedText());
		var excess = _printout.Length - MaxPrintoutChars;
		if (excess > 0)
		{
			_printout.Remove(0, excess);
			_printoutStart += excess;
		}
	}
}
