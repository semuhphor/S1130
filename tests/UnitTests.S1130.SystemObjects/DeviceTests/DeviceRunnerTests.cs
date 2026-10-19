using System;
using System.Diagnostics;
using System.Threading;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices.Disks;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/// <summary>
	/// Tests for <see cref="DeviceRunner"/>, the background thread that lets started device operations finish.
	/// </summary>
	public class DeviceRunnerTests : DeviceTestBase
	{
		private class CountingDevice : DeviceBase							// counts how often it is run
		{
			private readonly byte _code;
			public int Runs;
			public bool ThrowOnRun;

			public CountingDevice(byte code) { _code = code; }
			public override byte DeviceCode => _code;
			public override void ExecuteIocc() { }
			public override void Run()
			{
				Interlocked.Increment(ref Runs);
				if (ThrowOnRun) throw new InvalidOperationException("device failure");
			}
		}

		private static bool WaitFor(Func<bool> condition)					// wait up to 5 seconds for something to happen
		{
			var clock = Stopwatch.StartNew();
			while (!condition())
			{
				if (clock.Elapsed > TimeSpan.FromSeconds(5)) return false;
				Thread.Sleep(1);
			}
			return true;
		}

		[Fact]
		public void Constructor_NullCpu_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new DeviceRunner(null));
		}

		[Fact]
		public void Start_RunsDevicesRepeatedly()
		{
			BeforeEachTest();
			var device = new CountingDevice(0x1f);
			InsCpu.AddDevice(device);
			using var runner = new DeviceRunner(InsCpu);

			runner.Start();

			Assert.True(runner.IsRunning);
			Assert.True(WaitFor(() => device.Runs >= 3));
		}

		[Fact]
		public void Stop_EndsTheThread_AndDevicesNoLongerRun()
		{
			BeforeEachTest();
			var device = new CountingDevice(0x1f);
			InsCpu.AddDevice(device);
			var runner = new DeviceRunner(InsCpu);
			runner.Start();
			Assert.True(WaitFor(() => device.Runs >= 1));

			runner.Stop();
			var runsAtStop = device.Runs;
			Thread.Sleep(50);

			Assert.False(runner.IsRunning);
			Assert.Equal(runsAtStop, device.Runs);
		}

		[Fact]
		public void StartTwice_StopWhenNotStarted_AndRestart_AreAllSafe()
		{
			BeforeEachTest();
			var device = new CountingDevice(0x1f);
			InsCpu.AddDevice(device);
			var runner = new DeviceRunner(InsCpu);

			runner.Stop();														// never started
			runner.Start();
			runner.Start();														// second start is ignored
			runner.Stop();
			var runsAtStop = device.Runs;
			runner.Start();														// can start again

			Assert.True(runner.IsRunning);
			Assert.True(WaitFor(() => device.Runs > runsAtStop));
			runner.Dispose();
			Assert.False(runner.IsRunning);
		}

		[Fact]
		public void FailingDevice_IsRecorded_AndOtherDevicesKeepRunning()
		{
			BeforeEachTest();
			var bad = new CountingDevice(0x1e) { ThrowOnRun = true };
			var good = new CountingDevice(0x1f);
			InsCpu.AddDevice(bad);
			InsCpu.AddDevice(good);
			using var runner = new DeviceRunner(InsCpu);

			runner.Start();

			Assert.True(WaitFor(() => good.Runs >= 3 && bad.Runs >= 3));
			Assert.IsType<InvalidOperationException>(runner.LastError);
		}

		[Fact]
		public void DiskRead_CompletesByItself_WhileRunnerIsActive()
		{
			BeforeEachTest();
			var disk = InsCpu.BuiltInDisk;
			disk.Mount(DiskImageCartridge.CreateBlank(true));
			using var runner = new DeviceRunner(InsCpu);
			runner.Start();

			InitiateRead(disk, 0x1000, 321, false, 3);							// no manual Run() call

			Assert.True(WaitFor(() => disk.ActiveInterrupt != null));			// the disk finished and interrupted
			Assert.Equal(3, InsCpu[0x1001]);									// and the sector address word arrived
		}
	}
}
