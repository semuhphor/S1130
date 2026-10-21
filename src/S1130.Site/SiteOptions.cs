namespace S1130.Site;

/// <summary>Settings from the "Site" configuration section.</summary>
public sealed class SiteOptions
{
	public const string Section = "Site";

	/// <summary>Folder of genned system disks (see <c>Disks/README.md</c>). Relative paths are resolved against the content root.</summary>
	public string DiskLibrary { get; set; } = "Disks";

	/// <summary>Read-only DMS image copied for each session. Relative paths are resolved against <see cref="DiskLibrary"/>.</summary>
	public string MasterDisk { get; set; } = "dms-r2v12-1403-1442-32k.dsk";

	/// <summary>Folder that holds per-session data (working disks and, later, the cabinet).</summary>
	public string DataRoot { get; set; } = "App_Data";

	/// <summary>Most machines that may exist at once.</summary>
	public int MaxMachines { get; set; } = 8;

	/// <summary>A machine that has not been used for this long is stopped and its disk saved.</summary>
	public int IdleMinutes { get; set; } = 30;

	/// <summary>Largest deck accepted in one upload.</summary>
	public int MaxDeckCards { get; set; } = 2000;
}
