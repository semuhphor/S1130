using S1130.SystemObjects;
using S1130.SystemObjects.Utility;

namespace S1130.Site.Machines;

/// <summary>Turns a text deck (one line per card) into punched cards.</summary>
public static class TextDeck
{
	public const int Columns = 80;
	private static readonly ConversionCodes Codes = new();

	/// <summary>
	/// Each line becomes one card: upper-cased and cut to 80 columns. Characters with no card code punch as blanks.
	/// A trailing newline does not make an extra blank card.
	/// </summary>
	/// <exception cref="DeckTooLargeException">More than <paramref name="maxCards"/> cards.</exception>
	public static List<ICard> Parse(string text, int maxCards)
	{
		var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
		var count = lines.Length;
		if (count > 0 && lines[^1].Length == 0) count--;
		if (count > maxCards) throw new DeckTooLargeException(maxCards);

		var cards = new List<ICard>(count);
		for (var i = 0; i < count; i++)
		{
			var line = lines[i].ToUpperInvariant();
			if (line.Length > Columns) line = line[..Columns];
			cards.Add(Codes.ToCard(line));
		}
		return cards;
	}
}

public sealed class DeckTooLargeException(int maxCards)
	: Exception($"A deck may have at most {maxCards} cards.");
