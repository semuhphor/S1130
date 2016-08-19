using Xunit;
using S1130.SystemObjects;

namespace Tests
{
	public class DeckTests
	{
		private Deck _testDeck;

		[Fact]
		public void AddACardToADeck()
		{
			_testDeck = new Deck();
			_testDeck += new Card();
			Assert.Equal(1, _testDeck.Count);
			_testDeck[0][0] = 1;
			_testDeck += new Card();
			Assert.Equal(2, _testDeck.Count);
			Assert.Equal(1, _testDeck[0][0]);
			Assert.Equal(0, _testDeck[1][0]);
		}

		[Fact]
		public void AddDeckToDeck()
		{
			_testDeck = new Deck();
			_testDeck += new Deck() + new[] { new Card(), new Card(), new Card() };
			Assert.Equal(3, _testDeck.Count);
			var	testDeck2 = new Deck() + new[] { new Card(), new Card() };
			testDeck2[1][8] = 10;
			_testDeck += testDeck2;
			Assert.Equal(5, _testDeck.Count);
			Assert.Equal(10, _testDeck[4][8]);
		}

		[Fact]
		public void AddDecksToDeck()
		{
			_testDeck = new Deck();
			var testDecks = new Deck[3];
			testDecks[0] = new Deck() + new[] {new Card(), new Card()};
			testDecks[1] = new Deck() + new[] {new Card(), new Card(), new Card(), new Card()};
			testDecks[2] = new Deck() + new[] {new Card(), new Card(), new Card(), new Card(), new Card()};
			testDecks[2][3][72] = 72;
			_testDeck += testDecks;
			Assert.Equal(11, _testDeck.Count);
			Assert.Equal(72, _testDeck[9][72]);
		}
	}
}
