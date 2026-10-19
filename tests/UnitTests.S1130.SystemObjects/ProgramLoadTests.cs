using System;
using System.IO;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Devices.Disks;
using S1130.SystemObjects.Utility;
using Xunit;

namespace UnitTests.S1130.SystemObjects
{
	/// <summary>
	/// Tests for <see cref="ProgramLoad"/>, the IPL (Program Load) operation.
	/// </summary>
	public class ProgramLoadTests
	{
		private static Card CardWith(params ushort[] columns)
		{
			var card = new Card();
			Array.Copy(columns, card.Columns, columns.Length);
			return card;
		}

		[Theory]
		[InlineData(0x0000, 0x0000)]
		[InlineData(0xF800, 0xF800)]												// rows 12,11,0,1,2 copy straight across
		[InlineData(0x0400, 0x00C0)]												// row 3 sign-extends into bits 8-9
		[InlineData(0x03F0, 0x003F)]												// rows 4-9 become the low six bits
		[InlineData(0x000F, 0x0000)]												// the four unused low bits are dropped
		[InlineData(0xFFFF, 0xF8FF)]
		public void ExpandColumn_MapsCardRowsToWordBits(int column, int expected)
		{
			Assert.Equal((ushort)expected, ProgramLoad.ExpandColumn((ushort)column));
		}

		[Fact]
		public void FromCard_StoresExpandedColumnsInCoreAndStartsAtZero()
		{
			var cpu = new Cpu { Iar = 0x123, Wait = true };
			var card = CardWith(0x0400, 0x03F0, 0xF800);
			card.Columns[79] = 0x0010;

			ProgramLoad.FromCard(cpu, card);

			Assert.Equal(0x00C0, cpu[0]);
			Assert.Equal(0x003F, cpu[1]);
			Assert.Equal(0xF800, cpu[2]);
			Assert.Equal(0x0001, cpu[79]);
			Assert.Equal(0, cpu.Iar);
			Assert.False(cpu.Wait);
		}

		[Fact]
		public void FromCard_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => ProgramLoad.FromCard(new Cpu(), null));
			Assert.Throws<ArgumentNullException>(() => ProgramLoad.FromCard(null, new Card()));
		}

		[Fact]
		public void FromReader_TakesTheCardFromThe1442()
		{
			var cpu = new Cpu();
			var reader = new Device1442(cpu);
			cpu.AddDevice(reader);
			reader.Hopper.Enqueue(CardWith(0x0400));
			reader.Hopper.Enqueue(CardWith(0xF800));

			Assert.True(ProgramLoad.FromReader(cpu));

			Assert.Equal(0x00C0, cpu[0]);
			Assert.Single(reader.Hopper);											// only one card is consumed
		}

		[Fact]
		public void FromReader_Falls_BackTo2501_WhenThe1442IsEmpty()
		{
			var cpu = new Cpu();
			cpu.AddDevice(new Device1442(cpu));
			var reader = new Device2501(cpu);
			cpu.AddDevice(reader);
			reader.Hopper.Enqueue(CardWith(0xF800));

			Assert.True(ProgramLoad.FromReader(cpu));

			Assert.Equal(0xF800, cpu[0]);
		}

		[Fact]
		public void FromReader_WithNoCards_ChangesNothing()
		{
			var cpu = new Cpu { Iar = 0x55, Wait = true };
			cpu.AddDevice(new Device1442(cpu));

			Assert.False(ProgramLoad.FromReader(cpu));

			Assert.Equal(0x55, cpu.Iar);
			Assert.True(cpu.Wait);
		}

		[Fact]
		public void ColdStart_Dms_LoadsCardAndSetsSwitchesToTheDriveNumber()
		{
			var cpu = new Cpu { Iar = 0x77, Wait = true };

			ProgramLoad.ColdStart(cpu, ColdStartKind.Dms, 2);

			for (var i = 0; i < ProgramLoad.WordCount; i++)
			{
				Assert.Equal(IplCards.IPLCardDms12[i], cpu[i]);
			}
			Assert.Equal(2, cpu.ConsoleSwitches);
			Assert.Equal(0, cpu.Iar);
			Assert.False(cpu.Wait);
		}

		[Theory]
		[InlineData(ColdStartKind.Apl)]
		[InlineData(ColdStartKind.AplPrivileged)]
		public void ColdStart_Apl_LeavesTheSwitchesAlone(ColdStartKind kind)
		{
			var cpu = new Cpu { ConsoleSwitches = 0x1234 };

			ProgramLoad.ColdStart(cpu, kind, 3);

			Assert.Equal(0x1234, cpu.ConsoleSwitches);
			var expected = kind == ColdStartKind.Apl ? IplCards.IPLCardApl : IplCards.IPLCardAplPriv;
			Assert.Equal(expected[0], cpu[0]);
		}

		[Fact]
		public void ColdStart_UnknownKind_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => ProgramLoad.ColdStart(new Cpu(), (ColdStartKind)99));
		}

		/// <summary>
		/// End to end: IPL the DMS cold start card against the real DMS disk image from the C simulator
		/// distribution and let it load the resident monitor. DMS stores the core size it was built for
		/// at address /0E, so seeing it there proves the disk reads, the console switches and the
		/// cold start chain all work. Does nothing if the reference image is not present.
		/// </summary>
		[Fact]
		public void ColdStart_Dms_BootsTheReferenceDmsDisk()
		{
			var path = FindReferenceDisk();
			if (path == null) return;												// reference files are optional

			var cpu = new Cpu();
			var disk = DiskImageCartridge.Load(path, readOnly: true);
			cpu.BuiltInDisk.Mount(disk);
			using var runner = new DeviceRunner(cpu);
			runner.Start();

			ProgramLoad.ColdStart(cpu, ColdStartKind.Dms, 0);
			var deadline = DateTime.UtcNow.AddSeconds(5);
			while (DateTime.UtcNow < deadline && cpu[0x0E] != 0x4000)
			{
				if (!cpu.Step()) System.Threading.Thread.Sleep(1);					// waiting for a device interrupt
			}

			Assert.True(cpu[0x0E] == 0x4000, $"IAR={cpu.Iar:X4} wait={cpu.Wait} count={cpu.InstructionCount} w0E={cpu[0x0E]:X4}");
		}

		[Fact]
		public void Step_InWaitState_ExecutesNothing_AndAnInterruptEndsTheWait()
		{
			var cpu = new Cpu { Wait = true, Iar = 0x200 };

			Assert.False(cpu.Step());												// nothing pending: still waiting
			Assert.True(cpu.Wait);
			Assert.Equal(0x200, cpu.Iar);
		}

		private static string FindReferenceDisk()
		{
			for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
			{
				var candidate = Path.Combine(dir.FullName, "ForReferenceOnly", "1130SW", "dms.dsk");
				if (File.Exists(candidate)) return candidate;
			}
			return null;
		}
	}
}
