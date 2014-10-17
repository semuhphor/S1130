using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class Device2501Tests : DeviceTestBase
	{
		private Device2501 _2501;

		[TestInitialize]
		public override void BeforeEachTest()
		{
			base.BeforeEachTest();
			_2501 = new Device2501(InsCpu);
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
	}
}
