using System;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Utility;

namespace S1130.SystemObjects
{
	/// <summary>
	/// Which built-in cold start card to load when no real card deck is available.
	/// </summary>
	public enum ColdStartKind
	{
		/// <summary>Disk Monitor System V2M12 cold start card; boots from the disk selected by the console entry switches.</summary>
		Dms,
		/// <summary>APL\1130 cold start card.</summary>
		Apl,
		/// <summary>APL\1130 privileged cold start card.</summary>
		AplPrivileged
	}

	/// <summary>
	/// The IPL (Program Load) operation of the IBM 1130.
	/// 
	/// Pressing the PROGRAM LOAD key reads one card from the card reader, stores its 80 columns
	/// in core words 0..79 and starts execution at address 0. That card (the "cold start card")
	/// is a tiny program that reads a bigger loader from disk or from further cards.
	///
	/// A card column holds only 12 bits, so each column is expanded to a 16-bit word:
	///   * bits 0-4 (card rows 12, 11, 0, 1, 2) are copied as is,
	///   * card row 3 (word bit 5) is sign-extended into word bits 8 and 9,
	///   * card rows 4-9 become the low 6 bits of the word.
	/// This matches the C simulator's cr_boot so cartridges and decks stay interchangeable.
	/// </summary>
	public static class ProgramLoad
	{
		/// <summary>Number of columns (and therefore words) loaded by IPL.</summary>
		public const int WordCount = 80;

		/// <summary>
		/// Expands one 12-bit card column (kept in the high bits of a ushort) to the 16-bit word IPL stores.
		/// </summary>
		public static ushort ExpandColumn(ushort column)
		{
			var word = column & 0xF800;											// card rows 12, 11, 0, 1, 2
			if ((column & 0x0400) != 0) word |= 0x00C0;							// row 3 is sign-extended
			word |= (column & 0x03F0) >> 4;										// rows 4-9 are the low six bits
			return (ushort)word;
		}

		/// <summary>
		/// Performs an IPL from the given card: expands its 80 columns into core words 0..79,
		/// sets the instruction address register to zero and leaves the wait state.
		/// </summary>
		/// <param name="cpu">The processor to load.</param>
		/// <param name="card">The cold start card.</param>
		public static void FromCard(ICpu cpu, ICard card)
		{
			if (cpu == null) throw new ArgumentNullException(nameof(cpu));
			if (card == null) throw new ArgumentNullException(nameof(card));

			for (var i = 0; i < WordCount; i++)
			{
				cpu[i] = ExpandColumn(card.Columns[i]);
			}
			Start(cpu);
		}

		/// <summary>
		/// Performs an IPL from the first card waiting in a card reader hopper: the 1442 is tried
		/// first, then the 2501. The card is consumed.
		/// </summary>
		/// <param name="cpu">The processor to load.</param>
		/// <returns>False if neither reader has a card (nothing is changed).</returns>
		public static bool FromReader(ICpu cpu)
		{
			if (cpu == null) throw new ArgumentNullException(nameof(cpu));

			ICard card = null;
			var reader1442 = cpu.Devices[0x02] as Device1442;
			var reader2501 = cpu.Devices[0x09] as Device2501;
			if (reader1442 == null || !reader1442.Hopper.TryDequeue(out card))
			{
				if (reader2501 == null || !reader2501.Hopper.TryDequeue(out card))
				{
					return false;
				}
			}
			FromCard(cpu, card);
			return true;
		}

		/// <summary>
		/// Loads one of the built-in cold start cards directly into core, as the C simulator's BOOT command does.
		/// For a DMS boot the console entry switches are set to the drive number, which is how the DMS
		/// cold start card chooses the disk to boot from (0 = the built-in 2310).
		/// </summary>
		/// <param name="cpu">The processor to load.</param>
		/// <param name="kind">Which cold start card to use.</param>
		/// <param name="driveNumber">Boot drive number placed in the console entry switches (DMS only).</param>
		public static void ColdStart(ICpu cpu, ColdStartKind kind = ColdStartKind.Dms, int driveNumber = 0)
		{
			if (cpu == null) throw new ArgumentNullException(nameof(cpu));

			ushort[] image;
			switch (kind)
			{
				case ColdStartKind.Dms: image = IplCards.IPLCardDms12; break;
				case ColdStartKind.Apl: image = IplCards.IPLCardApl; break;
				case ColdStartKind.AplPrivileged: image = IplCards.IPLCardAplPriv; break;
				default: throw new ArgumentOutOfRangeException(nameof(kind));
			}
			if (kind == ColdStartKind.Dms)
			{
				cpu.ConsoleSwitches = (ushort)driveNumber;
			}
			for (var i = 0; i < WordCount; i++)
			{
				cpu[i] = image[i];
			}
			Start(cpu);
		}

		private static void Start(ICpu cpu)
		{
			cpu.Iar = 0;
			cpu.Wait = false;
		}
	}
}
