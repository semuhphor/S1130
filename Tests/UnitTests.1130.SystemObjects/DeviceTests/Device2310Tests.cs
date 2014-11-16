using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class Device2310Tests : DeviceTestBase
	{
		private Device2310 _2310;
		private FakeCartridge _cartridge;

		[TestInitialize]
		public override void BeforeEachTest()
		{
			base.BeforeEachTest();
			_2310 = new Device2310(InsCpu);
			_cartridge = new FakeCartridge();
		}

		[TestMethod]
		public void CreateDeviceTests_ShouldHaveCorrectDeviceId()
		{
			Assert.AreEqual(0x04, new Device2310(InsCpu).DeviceCode);
			Assert.AreEqual(0x04, new Device2310(InsCpu, 0).DeviceCode);
			Assert.AreEqual(0x09, new Device2310(InsCpu, 1).DeviceCode);
			Assert.AreEqual(0x0a, new Device2310(InsCpu, 2).DeviceCode);
			Assert.AreEqual(0x0b, new Device2310(InsCpu, 3).DeviceCode);
			Assert.AreEqual(0x0c, new Device2310(InsCpu, 4).DeviceCode);
		}

		[TestMethod]

		public void BadDeviceNumber_InvalidDriveNumberThrowsException()
		{
			TestBadDriveNumber(-1);
			TestBadDriveNumber(5);
		}

		private void TestBadDriveNumber(int driveNumber)
		{
			try
			{
				_2310 = new Device2310(InsCpu, driveNumber);
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("2310 drive number must be between 0 and 4", ex.Message);
				return;
			}
			Assert.Fail("Bad Device number should have thrown exception.");
		}

		public void BadDeviceNumber_AboveFourThrowsException()
		{
			try
			{
				_2310 = new Device2310(InsCpu, 5);
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("2310 drive number must be between 0 and 4", ex.Message);
			}
		}

		[TestMethod]
		public void WithoutDisk_DriveNotReady()
		{
			SenseDevice(_2310);
			Assert.AreEqual(Device2310.NotReady, InsCpu.Acc);
		}

		[TestMethod]
		public void WithDisk_DriveReady_MarksDriveMounted()
		{
			Assert.IsFalse(_cartridge.Mounted);
			_2310.Mount(_cartridge);
			SenseDevice(_2310);
			Assert.AreEqual(0, InsCpu.Acc);
			Assert.IsTrue(_cartridge.MountCalled);
			Assert.IsTrue(_cartridge.Mounted);
		}

		[TestMethod]
		public void UnMount_DriveNotReadey_Flushes()
		{
			_2310.Mount(_cartridge);
			Assert.IsTrue(_cartridge.Mounted);
			_2310.UnMount();
			SenseDevice(_2310);
			Assert.AreEqual(Device2310.NotReady, InsCpu.Acc);
			Assert.IsTrue(_cartridge.FlushCalled);
			Assert.IsFalse(_cartridge.Mounted);
		}

		public class FakeCartridge : ICartridge
		{
			public bool MountCalled { get; set; }
			public bool FlushCalled { get; set; }
			public bool Mounted { get; private set; }

			public void Mount()
			{
				MountCalled = true;
				Mounted = true;
			}

			public void Flush()
			{
				FlushCalled = true;
			}

			public void UnMount()
			{
				Mounted = false;
			}
		}
	}
}