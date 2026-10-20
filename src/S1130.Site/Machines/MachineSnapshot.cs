namespace S1130.Site.Machines;

/// <summary>What the console lights and device indicators show at one moment.</summary>
public sealed record MachineSnapshot(
	bool PoweredOn,
	bool Running,
	bool Wait,
	ushort Iar,
	ushort Acc,
	ushort Ext,
	ushort Xr1,
	ushort Xr2,
	ushort Xr3,
	bool Carry,
	bool Overflow,
	int[] ActiveLevels,
	ulong InstructionCount,
	int HopperCount,
	bool DiskMounted,
	string? Error)
{
	public static readonly MachineSnapshot Off =
		new(false, false, false, 0, 0, 0, 0, 0, 0, false, false, [], 0, 0, false, null);
}

/// <summary>Printer output from character <see cref="Start"/>; ask again from <see cref="Next"/> for more.</summary>
public sealed record PrintoutChunk(long Start, string Text, long Next);
