using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class Device2501Tests : DeviceTestBase
	{
		private Device2501 _2501;
		protected Deck ADeck;

		[TestInitialize]
		public override void BeforeEachTest()
		{
			base.BeforeEachTest();
			_2501 = new Device2501(InsCpu);
			ADeck = new Deck() + new [] { new Card(), new Card()};
		}

		[TestMethod]
		public void ShouldReturnDeviceType0X09()
		{
			Assert.AreEqual(0x09, _2501.DeviceCode);
		}

		[TestMethod]
		public void ShouldReturnNotReadyWithoutCards()
		{
			SenseDevice(_2501);
			Assert.AreEqual(Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
		}

		[TestMethod]
		public void ShouldShouldBusyDuringRead()
		{
			_2501 += ADeck;
			InitiateRead(_2501, 0x400, 80);	
			SenseDevice(_2501);
			Assert.AreEqual(Device2501.BusyStatus, InsCpu.Acc);
		}

		[TestMethod]
		public void ShouldReturnReadyWithCards()
		{
			_2501 += ADeck;
			SenseDevice(_2501);
			Assert.AreEqual(0, InsCpu.Acc);
		}

		[TestMethod]
		public void CardsShouldGoIntoHopper()
		{
			_2501 += ADeck; 
			Assert.AreEqual(2, _2501.Hopper.Count);
		}

		[TestMethod]
		public void ShouldReadCards()
		{
			_2501 += GetTestCard();
			_2501 += GetTestCard();
			InitiateRead(_2501, 0x1000, 80);
			_2501.Run();
			Assert.IsNotNull(_2501.ActiveInterrupt);
			Assert.AreEqual(0x1000, _2501.ActiveInterrupt.InterruptLevelStatusWord);
			SenseDevice(_2501);
			Assert.AreEqual(Device2501.OperationCompleteStatus, InsCpu.Acc);
			CheckCardReadProperly(0x1001, 80);
			SenseDevice(_2501, 1);
			Assert.AreEqual(0, InsCpu.Acc);
			Assert.IsNull(_2501.ActiveInterrupt);
			InitiateRead(_2501, 0x1000, 80);
			_2501.Run();
			Assert.IsNotNull(_2501.ActiveInterrupt);
			Assert.AreEqual(0x1000, _2501.ActiveInterrupt.InterruptLevelStatusWord);
			SenseDevice(_2501);
			Assert.AreEqual(Device2501.LastCardStatus | Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
			CheckCardReadProperly(0x1001, 80);
			SenseDevice(_2501, 1);
			Assert.AreEqual(Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
			Assert.IsNull(_2501.ActiveInterrupt);
		}

		protected void CheckCardReadProperly(int address, int wc)
		{
			var testCard = GetTestCard();
			for (var i = 0; i < wc; i++)
			{
				if (InsCpu[address + i] != testCard[i])
				{
					Assert.Fail("Mismatch at {0}: memory: {1:x}, card: {2:x}", i, InsCpu[address + i], testCard[i]);
				}
			}
		}
	}
}
