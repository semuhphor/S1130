using System;
using System.Threading;

namespace S1130.SystemObjects
{
	/// <summary>
	/// Keeps the devices of a CPU working while the program runs.
	///
	/// A device (disk, card reader, printer...) starts an operation when the program issues an XIO,
	/// but it finishes the operation in <see cref="IDevice.Run"/>: it moves the data, sets its status,
	/// and raises its interrupt. Nothing in the CPU calls <c>Run</c>, so this class does it: a background
	/// thread that visits every device over and over, the way the real hardware worked alongside the CPU.
	///
	/// This must be a separate thread. Some devices wait inside <c>Run</c> for the CPU to execute a few
	/// more instructions (<see cref="ICpu.LetInstuctionsExecute"/>), which would never happen if the
	/// CPU's own thread were the one waiting.
	/// </summary>
	public sealed class DeviceRunner : IDisposable
	{
		private readonly ICpu _cpu;											// the cpu whose devices we run
		private readonly object _gate = new object();						// protects start/stop
		private Thread _thread;												// the background thread (null when stopped)
		private volatile bool _stopRequested;								// tells the thread to finish

		/// <param name="cpu">The CPU whose devices should be run.</param>
		public DeviceRunner(ICpu cpu)
		{
			_cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
		}

		/// <summary>True while the background thread is running.</summary>
		public bool IsRunning
		{
			get { lock (_gate) { return _thread != null; } }
		}

		/// <summary>Starts the background thread. Does nothing if it is already running.</summary>
		public void Start()
		{
			lock (_gate)
			{
				if (_thread != null)
					return;
				_stopRequested = false;
				_thread = new Thread(Loop) { IsBackground = true, Name = "S1130 device runner" };
				_thread.Start();
			}
		}

		/// <summary>
		/// Stops the background thread and waits briefly for it to finish. Safe to call when already stopped.
		/// </summary>
		/// <remarks>
		/// If a device is in the middle of waiting for the CPU to run instructions and the CPU is not
		/// running, the thread cannot finish at once. It is a background thread, so it will not keep the
		/// process alive, and it ends as soon as the CPU runs again.
		/// </remarks>
		public void Stop()
		{
			Thread thread;
			lock (_gate)
			{
				thread = _thread;
				_thread = null;
				_stopRequested = true;
			}
			thread?.Join(TimeSpan.FromSeconds(1));
		}

		public void Dispose()
		{
			Stop();
		}

		/// <summary>The most recent exception thrown by a device's <c>Run</c>, or null. The runner keeps going after an error.</summary>
		public Exception LastError => _lastError;

		private volatile Exception _lastError;								// last device failure (see LastError)

		private void Loop()													// visit every device until told to stop
		{
			while (!_stopRequested)
			{
				var devices = _cpu.Devices;
				for (var i = 0; i < devices.Length && !_stopRequested; i++)
				{
					try
					{
						devices[i]?.Run();
					}
					catch (Exception ex)									// a faulty device must not take the whole process down
					{
						_lastError = ex;
					}
				}
				Thread.Sleep(1);											// don't burn a whole core while the devices are idle
			}
		}
	}
}
