using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class Device2501Tests : DeviceTestBase
	{
		private Device2501 _2501;
		private Deck _deck;

		[TestInitialize]
		public override void BeforeEachTest()
		{
			base.BeforeEachTest();
			_2501 = new Device2501(InsCpu);
			_deck = new Deck() + new [] { new Card(), new Card()};
		}

		[TestMethod]
		public void ShouldReturnDeviceType0X09()
		{
			Assert.AreEqual(0x09, _2501.DeviceCode);
		}

		[TestMethod]
		public void ShouldReturnNotReadyWithoutCards()
		{
			InsCpu.IoccDeviceCode = _2501.DeviceCode;
			InsCpu.IoccFunction = DevFunction.SenseDevice;
			_2501.ExecuteIocc();
			Assert.AreEqual(Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
		}

		[TestMethod]
		public void ShouldReturnReadyWithCards()
		{
			_2501 += _deck;
			InsCpu.IoccDeviceCode = _2501.DeviceCode;
			InsCpu.IoccFunction = DevFunction.SenseDevice;
			_2501.ExecuteIocc();
			Assert.AreEqual(0, InsCpu.Acc);
		}

		[TestMethod]
		public void CardsShouldGoIntoHopper()
		{
			_2501 += _deck; 
			Assert.AreEqual(2, _2501.Hopper.Count);
		}
	}
}
