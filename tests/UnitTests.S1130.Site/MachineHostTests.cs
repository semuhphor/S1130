using S1130.Site.Machines;

namespace UnitTests.S1130.Site;

public class MachineHostTests
{
	[Fact]
	public void Boot_Dms_PrintsTheJobHeaderOnThe1403()
	{
		var master = TestFiles.FindMasterDisk();
		if (master == null) return;												// the reference disk is optional
		var folder = TestFiles.NewScratchDirectory();
		try
		{
			using var host = new MachineHost(folder);
			host.LoadDeck(TextDeck.Parse("// JOB\n", 10));
			host.Boot(master);

			var text = TestFiles.WaitForPrintout(host, "V2 M12", TimeSpan.FromSeconds(10));

			Assert.Contains("// JOB", text);
			Assert.Contains("V2 M12", text);
			Assert.True(File.Exists(Path.Combine(folder, "disks", MachineHost.WorkDiskName)));
		}
		finally
		{
			Directory.Delete(folder, true);
		}
	}

	[Fact]
	public void Boot_Dms_ReadsEveryCardOfAMultiCardDeck()
	{
		var master = TestFiles.FindMasterDisk();
		if (master == null) return;
		var folder = TestFiles.NewScratchDirectory();
		try
		{
			using var host = new MachineHost(folder);
			host.LoadDeck(TextDeck.Parse("// JOB\n// * HELLO FROM THE WEB SITE\n// * SECOND COMMENT\n", 10));
			host.Boot(master);

			var text = TestFiles.WaitForPrintout(host, "SECOND COMMENT", TimeSpan.FromSeconds(20));

			Assert.Contains("HELLO FROM THE WEB SITE", text);
			Assert.Contains("SECOND COMMENT", text);
		}
		finally
		{
			Directory.Delete(folder, true);
		}
	}

	[Fact]
	public void ReadPrintout_Since_ReturnsOnlyNewText_AndClearMovesThePosition()
	{
		var master = TestFiles.FindMasterDisk();
		if (master == null) return;
		var folder = TestFiles.NewScratchDirectory();
		try
		{
			using var host = new MachineHost(folder);
			host.LoadDeck(TextDeck.Parse("// JOB\n", 10));
			host.Boot(master);
			TestFiles.WaitForPrintout(host, "V2 M12", TimeSpan.FromSeconds(10));

			var all = host.ReadPrintout(0);
			Assert.Equal(0, all.Start);
			Assert.Equal(all.Text.Length, all.Next);
			Assert.Equal("", host.ReadPrintout(all.Next).Text);

			host.ClearPrintout();
			var after = host.ReadPrintout(0);									// older positions clamp to the tear-off point
			Assert.True(after.Start >= all.Next);
		}
		finally
		{
			Directory.Delete(folder, true);
		}
	}

	[Fact]
	public void NewMachine_IsStoppedWithAnEmptyHopper()
	{
		var folder = TestFiles.NewScratchDirectory();
		try
		{
			using var host = new MachineHost(folder);
			var s = host.Snapshot();
			Assert.True(s.PoweredOn);
			Assert.False(s.Running);
			Assert.Equal(0, s.HopperCount);
			Assert.False(s.DiskMounted);
		}
		finally
		{
			Directory.Delete(folder, true);
		}
	}
}
