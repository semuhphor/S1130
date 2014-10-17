using System.Collections.Generic;
using System.Linq;

namespace S1130.SystemObjects
{
	public class Deck
	{
		public Deck()
		{
			Cards = new List<ICard>();
		}

		public List<ICard> Cards;

		public static Deck operator +(Deck deck1, Deck deck2)
		{
			deck1.Cards.AddRange(deck2.Cards);
			return deck1;
		}
		
		public static Deck operator +(Deck deck1, Deck[] decks)
		{
			decks.ToList().ForEach(d => { deck1 += d; });
			return deck1;
		}

		public static Deck operator +(Deck deck, IEnumerable<ICard> cards)
		{
			deck.Cards.AddRange(cards);
			return deck;
		}

		public static Deck operator +(Deck deck, ICard card)
		{
			deck.Cards.Add(card);
			return deck;
		}

		public ICard this[int cardIndex]
		{
			get { return Cards[cardIndex]; }
		}

		public int Count
		{
			get { return Cards.Count; }
		}
	}
}