using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
	[TestClass]
	public class DeckTests
	{
		private Deck _testDeck;

		[TestInitialize]
		public void BeforeEachTest()
		{
			_testDeck = new Deck();
		}

		[TestMethod]
		public void AddACardToADeck()
		{
			_testDeck += new Card();
			Assert.AreEqual(1, _testDeck.Count);
			_testDeck[0][0] = 1;
			_testDeck += new Card();
			Assert.AreEqual(2, _testDeck.Count);
			Assert.AreEqual(1, _testDeck[0][0]);
			Assert.AreEqual(0, _testDeck[1][0]);
		}

		[TestMethod]
		public void AddDeckToDeck()
		{
			_testDeck += new Deck() + new[] { new Card(), new Card(), new Card() };
			Assert.AreEqual(3, _testDeck.Count);
			var	testDeck2 = new Deck() + new[] { new Card(), new Card() };
			testDeck2[1][8] = 10;
			_testDeck += testDeck2;
			Assert.AreEqual(5, _testDeck.Count);
			Assert.AreEqual(10, _testDeck[4][8]);
		}

		[TestMethod]
		public void AddDecksToDeck()
		{
			var testDecks = new Deck[3];
			testDecks[0] = new Deck() + new[] {new Card(), new Card()};
			testDecks[1] = new Deck() + new[] {new Card(), new Card(), new Card(), new Card()};
			testDecks[2] = new Deck() + new[] {new Card(), new Card(), new Card(), new Card(), new Card()};
			testDecks[2][3][72] = 72;
			_testDeck += testDecks;
			Assert.AreEqual(11, _testDeck.Count);
			Assert.AreEqual(72, _testDeck[9][72]);
		}
	}
}
