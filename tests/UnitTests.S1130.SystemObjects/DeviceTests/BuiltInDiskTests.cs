using System.IO;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Devices.Disks;
using Xunit;
using System;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/// <summary>
	/// Tests the 1131's built-in 2310 drive as the CPU provides it, working with a real
	/// <see cref="DiskImageCartridge"/> (seek, read, write, and saving on unmount).
	/// </summary>
	public class BuiltInDiskTests : DeviceTestBase, IDisposable
	{
		private readonly string _dir = Path.Combine(Path.GetTempPath(), "s1130disk-" + Guid.NewGuid().ToString("N"));

		public BuiltInDiskTests()
		{
			Directory.CreateDirectory(_dir);
			BeforeEachTest();
		}

		public void Dispose()
		{
			Directory.Delete(_dir, true);
		}

		private void SeekTo(int cylinder)										// move the heads forward from cylinder 0
		{
			IssueControl(InsCpu.BuiltInDisk, cylinder, Device2310.SeekForward);
			InsCpu.BuiltInDisk.Run();
		}

		[Fact]
		public void Cpu_HasBuiltInDisk_AsDevice4()
		{
			Assert.NotNull(InsCpu.BuiltInDisk);
			Assert.Equal(0x04, InsCpu.BuiltInDisk.DeviceCode);
			Assert.Same(InsCpu.BuiltInDisk, InsCpu.Devices[4]);
		}

		[Fact]
		public void Read_FromMountedImage_DeliversSectorIncludingAddressWord()
		{
			var disk = InsCpu.BuiltInDisk;
			disk.Mount(DiskImageCartridge.CreateBlank(true));
			SeekTo(3);

			InitiateRead(disk, 0x1000, 321, false, 5);
			disk.Run();

			Assert.Equal(321, InsCpu[0x1000]);
			Assert.Equal(3 * 8 + 5, InsCpu[0x1001]);							// sector address word
			Assert.Equal(0, InsCpu[0x1002]);
		}

		[Fact]
		public void Write_ThenRead_ReturnsWhatWasWritten()
		{
			var disk = InsCpu.BuiltInDisk;
			disk.Mount(DiskImageCartridge.CreateBlank(true));
			SeekTo(7);

			InitiateDiskWrite(disk, 0x2000, 321, 7 * 8 + 2);					// writes address word then a counting pattern
			disk.Run();
			InitiateRead(disk, 0x3000, 321, false, 2);
			disk.Run();

			for (var i = 0; i < 322; i++)
			{
				Assert.Equal(InsCpu[0x2000 + i], InsCpu[0x3000 + i]);
			}
		}

		[Fact]
		public void Write_ToReadOnlyImage_IsRefusedAndImageUnchanged()
		{
			var disk = InsCpu.BuiltInDisk;
			var image = new DiskImageCartridge(new byte[1042608], null, true);
			disk.Mount(image);

			InitiateDiskWrite(disk, 0x2000, 321, 0);
			disk.Run();

			Assert.Null(disk.ActiveInterrupt);									// hard error: no interrupt
			Assert.False(image.IsDirty);										// nothing stored
			SenseDevice(disk);
			Assert.Equal(Device2310.DataError | Device2310.NotReady, InsCpu.Acc & 0xfffc);
		}

		[Fact]
		public void UnMount_SavesImage_AndItReloadsWithTheWrittenData()
		{
			var path = Path.Combine(_dir, "disk.dsk");
			var disk = InsCpu.BuiltInDisk;
			disk.Mount(DiskImageCartridge.CreateBlank(true, path));
			SeekTo(1);
			InitiateDiskWrite(disk, 0x2000, 321, 1 * 8 + 0);
			disk.Run();

			disk.UnMount();

			var reloaded = DiskImageCartridge.Load(path);
			reloaded.CurrentCylinder = 1;
			var sector = reloaded.Read(0);
			Assert.Equal(8, sector[0]);
			Assert.Equal(InsCpu[0x2002], sector[1]);
			Assert.Equal(InsCpu[0x2000 + 321], sector[320]);
		}

		[Fact]
		public void Sense_WithNoCartridge_ReportsNotReady()
		{
			SenseDevice(InsCpu.BuiltInDisk);
			Assert.Equal(Device2310.NotReady, InsCpu.Acc & Device2310.NotReady);
		}
	}
}
