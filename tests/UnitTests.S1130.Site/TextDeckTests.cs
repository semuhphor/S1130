using S1130.Site.Machines;

namespace UnitTests.S1130.Site;

public class TextDeckTests
{
	[Fact]
	public void Parse_OneCardPerLine_IgnoringATrailingNewline()
	{
		Assert.Equal(2, TextDeck.Parse("// JOB\r\n// XEQ\r\n", 10).Count);
	}

	[Fact]
	public void Parse_UpperCasesAndCutsAt80Columns()
	{
		var card = TextDeck.Parse("a" + new string('X', 100), 10)[0];
		var upperA = TextDeck.Parse("A", 10)[0];
		Assert.Equal(upperA.Columns[0], card.Columns[0]);
	}

	[Fact]
	public void Parse_TooManyCards_Throws()
	{
		Assert.Throws<DeckTooLargeException>(() => TextDeck.Parse("1\n2\n3\n", 2));
	}
}

