using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
	[TestClass]
	public class CardTests
	{
		[TestMethod]
		public void CreateCard()
		{
			var card = new Card("A1");
			Assert.AreEqual(0x9000, card[0]);
			Assert.AreEqual(0x1000, card[1]);
		}
	}
}