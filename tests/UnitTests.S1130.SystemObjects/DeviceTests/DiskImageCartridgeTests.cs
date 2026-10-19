using System;
using System.IO;
using S1130.SystemObjects.Devices.Disks;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/// <summary>
	/// Tests for <see cref="DiskImageCartridge"/>: the in-memory 2315 cartridge whose image file is
	/// interchangeable with the C (SIMH) IBM 1130 simulator.
	/// </summary>
	public class DiskImageCartridgeTests : IDisposable
	{
		private readonly string _dir = Path.Combine(Path.GetTempPath(), "s1130cart-" + Guid.NewGuid().ToString("N"));

		public DiskImageCartridgeTests()
		{
			Directory.CreateDirectory(_dir);
		}

		public void Dispose()
		{
			Directory.Delete(_dir, true);
		}

		private static ushort[] SectorOf(ushort address, ushort fill)		// a 321-word sector: address word, then fill value
		{
			var words = new ushort[DiskImageCartridge.WordsPerSector];
			words[0] = address;
			for (var i = 1; i < words.Length; i++) words[i] = fill;
			return words;
		}

		[Fact]
		public void ImageSize_MatchesReferenceSimulator()
		{
			Assert.Equal(1042608, DiskImageCartridge.ImageSize);
			Assert.Equal(1042608, DiskImageCartridge.CreateBlank(false).ToImageBytes().Length);
		}

		[Fact]
		public void Constructor_WrongSize_Throws()
		{
			Assert.Throws<InvalidDataException>(() => new DiskImageCartridge(new byte[1000]));
			Assert.Throws<InvalidDataException>(() => new DiskImageCartridge(new byte[DiskImageCartridge.ImageSize + 1]));
		}

		[Fact]
		public void Constructor_NullImage_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new DiskImageCartridge(null));
		}

		[Fact]
		public void Constructor_ReadsWordsLittleEndian_AtReferenceOffsets()
		{
			var image = new byte[DiskImageCartridge.ImageSize];
			var offset = (2 * 8 + 3) * 642;									// cylinder 2, sector 3
			image[offset] = 0x13; image[offset + 1] = 0x00;					// word 0 = 0x0013 (address 19)
			image[offset + 2] = 0x34; image[offset + 3] = 0x12;				// word 1 = 0x1234
			var cart = new DiskImageCartridge(image) { CurrentCylinder = 2 };

			var sector = cart.Read(3);

			Assert.Equal(0x0013, sector[0]);
			Assert.Equal(0x1234, sector[1]);
		}

		[Fact]
		public void CreateBlank_Unformatted_IsAllZeros()
		{
			var cart = DiskImageCartridge.CreateBlank(false);
			Assert.All(cart.ToImageBytes(), b => Assert.Equal(0, b));
		}

		[Fact]
		public void CreateBlank_Formatted_HasSectorAddressesAndZeroData()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			foreach (var cylinder in new[] { 0, 1, 100, 202 })
			{
				cart.CurrentCylinder = cylinder;
				for (var sec = 0; sec < 8; sec++)
				{
					var sector = cart.Read(sec);
					Assert.Equal(cylinder * 8 + sec, sector[0]);
					for (var i = 1; i < sector.Length; i++) Assert.Equal(0, sector[i]);
				}
			}
		}

		[Fact]
		public void Write_ThenRead_RoundTrips_AndIsolatedByCylinderAndSector()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			cart.CurrentCylinder = 5;
			var data = SectorOf(5 * 8 + 2, 0xABCD);
			cart.Write(2, new ArraySegment<ushort>(data), data.Length);

			Assert.Equal(data, cart.Read(2));
			Assert.Equal(5 * 8 + 1, cart.Read(1)[0]);						// neighbouring sector untouched
			Assert.Equal(0, cart.Read(1)[1]);
			cart.CurrentCylinder = 6;
			Assert.Equal(6 * 8 + 2, cart.Read(2)[0]);						// other cylinder untouched
		}

		[Fact]
		public void Write_ShortCount_ZeroFillsRestOfSector()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			var full = SectorOf(1, 0xFFFF);
			cart.Write(1, new ArraySegment<ushort>(full), full.Length);

			var shortData = new ushort[] { 1, 7, 8, 9 };
			cart.Write(1, new ArraySegment<ushort>(shortData), shortData.Length);

			var sector = cart.Read(1);
			Assert.Equal(new ushort[] { 1, 7, 8, 9 }, sector[..4]);
			for (var i = 4; i < sector.Length; i++) Assert.Equal(0, sector[i]);
		}

		[Fact]
		public void Write_CountOverSectorSize_IsLimitedToSector()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			var big = new ushort[400];
			Array.Fill(big, (ushort)0x55);
			cart.Write(0, new ArraySegment<ushort>(big), 400);

			cart.CurrentCylinder = 0;
			Assert.Equal(0x55, cart.Read(0)[320]);
			Assert.Equal(1, cart.Read(1)[0]);								// did not spill into next sector
		}

		[Fact]
		public void Read_ReturnsCopy_SoCallerCannotChangeDisk()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			cart.Read(0)[1] = 99;
			Assert.Equal(0, cart.Read(0)[1]);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(8)]
		public void ReadAndWrite_BadSector_Throw(int sector)
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			Assert.Throws<ArgumentOutOfRangeException>(() => cart.Read(sector));
			Assert.Throws<ArgumentOutOfRangeException>(
				() => cart.Write(sector, new ArraySegment<ushort>(new ushort[1]), 1));
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(203)]
		public void CurrentCylinder_OutOfRange_Throws(int cylinder)
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			Assert.Throws<ArgumentOutOfRangeException>(() => cart.CurrentCylinder = cylinder);
		}

		[Fact]
		public void ReadOnly_RejectsWrite_AndRemembersUntilRemounted()
		{
			var cart = new DiskImageCartridge(new byte[DiskImageCartridge.ImageSize], null, readOnly: true);
			var data = SectorOf(0, 1);

			cart.Write(0, new ArraySegment<ushort>(data), data.Length);

			Assert.True(cart.ReadOnly);
			Assert.True(cart.WriteRejected);
			Assert.False(cart.IsDirty);
			Assert.Equal(0, cart.Read(0)[1]);
			cart.Mount();
			Assert.False(cart.WriteRejected);
		}

		[Fact]
		public void MountAndUnmount_TrackMountedState()
		{
			var cart = DiskImageCartridge.CreateBlank(false);
			Assert.False(cart.Mounted);
			cart.Mount();
			Assert.True(cart.Mounted);
			cart.UnMount();
			Assert.False(cart.Mounted);
		}

		[Fact]
		public void Flush_MemoryOnlyCartridge_DoesNothing()
		{
			var cart = DiskImageCartridge.CreateBlank(true);
			var data = SectorOf(0, 1);
			cart.Write(0, new ArraySegment<ushort>(data), data.Length);
			cart.Flush();
			Assert.True(cart.IsDirty);										// nowhere to save, so still dirty
		}

		[Fact]
		public void Flush_SavesChangesToFile_AndReloadsIdentically()
		{
			var path = Path.Combine(_dir, "disk.dsk");
			var cart = DiskImageCartridge.CreateBlank(true, path);
			cart.CurrentCylinder = 10;
			var data = SectorOf(10 * 8 + 4, 0x1357);
			cart.Write(4, new ArraySegment<ushort>(data), data.Length);

			cart.Flush();

			Assert.False(cart.IsDirty);
			Assert.False(File.Exists(path + ".tmp"));
			Assert.Equal(DiskImageCartridge.ImageSize, new FileInfo(path).Length);
			var reloaded = DiskImageCartridge.Load(path);
			reloaded.CurrentCylinder = 10;
			Assert.Equal(data, reloaded.Read(4));
		}

		[Fact]
		public void Flush_WithNoChanges_LeavesFileUntouched()
		{
			var path = Path.Combine(_dir, "disk.dsk");
			DiskImageCartridge.CreateBlank(true, path).Flush();				// creates the file
			var before = File.GetLastWriteTimeUtc(path);
			File.SetLastWriteTimeUtc(path, before.AddHours(-1));

			var cart = DiskImageCartridge.Load(path);
			cart.Flush();

			Assert.Equal(before.AddHours(-1), File.GetLastWriteTimeUtc(path));
		}

		[Fact]
		public void LoadThenSave_ProducesIdenticalBytes()
		{
			var path = Path.Combine(_dir, "disk.dsk");
			var original = new byte[DiskImageCartridge.ImageSize];
			new Random(1130).NextBytes(original);
			File.WriteAllBytes(path, original);

			var cart = DiskImageCartridge.Load(path);

			Assert.Equal(original, cart.ToImageBytes());
		}

		[Fact]
		public void Load_RealReferenceImage_IsCompatible()
		{
			var image = FindReferenceImage("dms.dsk");
			if (image == null) return;										// reference files not available here

			var cart = DiskImageCartridge.Load(image, readOnly: true);

			var bad = 0;
			for (var cyl = 0; cyl < DiskImageCartridge.Cylinders; cyl++)
			{
				cart.CurrentCylinder = cyl;
				for (var sec = 0; sec < 8; sec++)
				{
					if (cart.Read(sec)[0] != cyl * 8 + sec) bad++;
				}
			}
			Assert.Equal(0, bad);
			Assert.Equal(File.ReadAllBytes(image), cart.ToImageBytes());
		}

		private static string FindReferenceImage(string name)				// look for ForReferenceOnly\1130SW\<name> above the test folder
		{
			for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
			{
				var candidate = Path.Combine(dir.FullName, "ForReferenceOnly", "1130SW", name);
				if (File.Exists(candidate)) return candidate;
			}
			return null;
		}
	}
}
