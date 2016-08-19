using Xunit;
using S1130.SystemObjects;

namespace Tests	
{
	public class CardTests
	{
		[Fact]
		public void CreateCard()
		{
			var card = new Card("A1");
			Assert.Equal(0x9000, card[0]);
			Assert.Equal(0x1000, card[1]);
		}
	}
}