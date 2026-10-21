using S1130.Site.Machines;

namespace UnitTests.S1130.Site;

/// <summary>Helpers shared by tests that need the reference DMS disk and a scratch folder.</summary>
internal static class TestFiles
{
	public const string MasterDiskName = "dms-r2v12-1403-1442-32k.dsk";

	/// <summary>The site's DMS disk from its disk library (copied next to the test assembly by the build).</summary>
	public static string? FindMasterDisk()
	{
		var copied = Path.Combine(AppContext.BaseDirectory, "Disks", MasterDiskName);
		if (File.Exists(copied)) return copied;
		for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
		{
			var candidate = Path.Combine(dir.FullName, "src", "S1130.Site", "Disks", MasterDiskName);
			if (File.Exists(candidate)) return candidate;
		}
		return null;
	}

	public static string NewScratchDirectory()
	{
		var path = Path.Combine(Path.GetTempPath(), $"s1130-site-{Guid.NewGuid():N}");
		Directory.CreateDirectory(path);
		return path;
	}

	/// <summary>Polls the printout until it contains <paramref name="expected"/> or the time runs out.</summary>
	public static string WaitForPrintout(MachineHost host, string expected, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		var text = "";
		while (DateTime.UtcNow < deadline)
		{
			text = host.ReadPrintout(0).Text;
			if (text.Contains(expected)) break;
			Thread.Sleep(20);
		}
		return text;
	}
}
