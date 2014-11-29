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

		[TestMethod]
		public void MoveToNewCylinder()
		{
			StartDriveAndTestReady();										// prepare drive to accept command
			SenseDevice(_2310);												// check the status
			Assert.AreEqual(InsCpu.Acc, Device2310.AtCylZero);				// ensure at cyl 0
			IssueControl(_2310, 1, 0);										// move in one cylinder

			Assert.AreEqual(1, _cartridge.CurrentCylinder);
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
			Assert.AreEqual(Device2310.AtCylZero, InsCpu.Acc);
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

		private void StartDriveAndTestReady()
		{
			_2310.Mount(_cartridge);											// mount the cart
			SenseDevice(_2310);													// ... get the sense value
			Assert.AreEqual(Device2310.AtCylZero, InsCpu.Acc);					// ... should be ready at cylinder 0
		}

		public class FakeCartridge : ICartridge
		{
			public int CurrentCylinder;

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

			public void MoveToCylinder(int offsetOfCylinder)
			{
				CurrentCylinder += offsetOfCylinder;
			}

			public void UnMount()
			{
				Mounted = false;
			}
		}
	}
}