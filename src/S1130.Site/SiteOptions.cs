namespace S1130.Site;

/// <summary>Settings from the "Site" configuration section.</summary>
public sealed class SiteOptions
{
	public const string Section = "Site";

	/// <summary>Read-only DMS image copied for each session. Relative paths are resolved against the content root.
	/// When empty, the first <c>ForReferenceOnly/1130SW/dms.dsk</c> found above the content root is used.</summary>
	public string MasterDisk { get; set; } = "";

	/// <summary>Folder that holds per-session data (working disks and, later, the cabinet).</summary>
	public string DataRoot { get; set; } = "App_Data";

	/// <summary>Most machines that may exist at once.</summary>
	public int MaxMachines { get; set; } = 8;

	/// <summary>A machine that has not been used for this long is stopped and its disk saved.</summary>
	public int IdleMinutes { get; set; } = 30;

	/// <summary>Largest deck accepted in one upload.</summary>
	public int MaxDeckCards { get; set; } = 2000;
}
