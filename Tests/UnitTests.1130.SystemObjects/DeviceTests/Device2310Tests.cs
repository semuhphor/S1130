using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class Device2310Tests : DeviceTestBase
	{
		private Device2310 _2310;										// 2310 for testing
		private FakeCartridge _cartridge;								// cartridge to test

		[TestInitialize]
		public override void BeforeEachTest()							// before each test ..
		{
			base.BeforeEachTest();											// .. run the base (creates new cpu)
			_2310 = new Device2310(InsCpu);									// creeate new 2310
			_cartridge = new FakeCartridge();								// .. and new cartridge to use
		}

		[TestMethod]
		public void CreateDeviceTests_ShouldHaveCorrectDeviceId()		// test that device ids are correct
		{
			Assert.AreEqual(0x04, new Device2310(InsCpu).DeviceCode);		// device zero 
			Assert.AreEqual(0x09, new Device2310(InsCpu, 1).DeviceCode);	// device one
			Assert.AreEqual(0x0a, new Device2310(InsCpu, 2).DeviceCode);	// device two
			Assert.AreEqual(0x0b, new Device2310(InsCpu, 3).DeviceCode);	// device three
			Assert.AreEqual(0x0c, new Device2310(InsCpu, 4).DeviceCode);	// device four
		}

		[TestMethod]
		public void BadDeviceNumber_InvalidDriveNumberThrowsException()	// test that it won't create bad devices
		{
			TestBadDriveNumber(-1);											// test that device number below zero fails
			TestBadDriveNumber(5);											// .. and device number above four fails
		}

		[TestMethod]
		public void Seek_TestNewCylIlswAndIntLevel()					// test that seek move head and interrupts
		{
			StartDriveAndTestReady();										// prepare drive to accept command
			var acc = Sense(_2310);											// check the status
			Assert.AreEqual(InsCpu.Acc, Device2310.AtCylZero);				// ensure at cyl 0
			IssueControl(_2310, 1, Device2310.SeekForward);					// move in one cylinder
			acc = Sense(_2310);												// check the status
			Assert.AreEqual(Device2310.Busy|Device2310.AtCylZero, acc);		// .. we should be busy and still at cyl zero
			_2310.Run();													// .. now run the seek
			Assert.AreEqual(1, _cartridge.CurrentCylinder);					// assert the move occurred
			var activeInterrupt = _2310.ActiveInterrupt;					// get the active interrupt
			Assert.IsNotNull(activeInterrupt);								// .. assert an interrupt requested
			Assert.AreEqual(2, activeInterrupt.InterruptLevel);				// .. assert level 2
			Assert.AreEqual(0x8000, activeInterrupt.Ilsw);					// .. and is for drive 0
			acc = Sense(_2310);												// check the status
			Assert.AreEqual(Device2310.OperationComplete, InsCpu.Acc);		// operation is complete
		}

		[TestMethod]
		public void Seek_TestZeroCylinderSeek()							// test that zero cylinder seek does nothing
		{
			StartDriveAndTestReady();										// prepare drive to accept command
			var acc = Sense(_2310);											// check the status
			Assert.AreEqual(acc, Device2310.AtCylZero);						// ensure at cyl 0
			IssueControl(_2310, 0, Device2310.SeekForward);					// move zero cylinders
			_2310.Run();													// .. and run the seek
			Assert.AreEqual(0, _cartridge.CurrentCylinder);					// assert no move occurred
			var activeInterrupt = _2310.ActiveInterrupt;					// get the active interrupt
			Assert.IsNull(activeInterrupt);									// .. assert no interrupt requested
			acc = Sense(_2310);												// check the status
			Assert.AreEqual(acc, Device2310.AtCylZero);						// ensure at cyl 0
		}

		[TestMethod]
		public void WithoutDisk_DriveNotReady()							// test that drive not ready before mount
		{
			var acc = Sense(_2310);											// sense the drive
			Assert.AreEqual(Device2310.NotReady, acc);						// .. check that it's not ready
		}

		[TestMethod]
		public void WithDisk_DriveReady_MarksDriveMounted()				// test that mounting makes drive ready
		{
			Assert.IsFalse(_cartridge.Mounted);								// check that it's not mounted yet
			_2310.Mount(_cartridge);										// .. mount it.
			var acc = Sense(_2310);											// get DSW from drive
			Assert.AreEqual(Device2310.AtCylZero, acc);						// .. it is at cyl 0
			Assert.IsTrue(_cartridge.MountCalled);							// .. mount was called on cart
			Assert.IsTrue(_cartridge.Mounted);								// .. cart thinks it is mounted
		}

		[TestMethod]
		public void UnMount_DriveNotReadey_Flushes()					// test that unmounting flushes and makes not ready
		{
			_2310.Mount(_cartridge);										// mount one up
			Assert.IsTrue(_cartridge.Mounted);								// .. check it's mounted
			_2310.UnMount();												// now unmount it
			var acc = Sense(_2310);											// get DSW
			Assert.AreEqual(Device2310.NotReady, acc);						// .. check not ready
			Assert.IsTrue(_cartridge.FlushCalled);							// .. check that device told cart to flush
			Assert.IsFalse(_cartridge.Mounted);								// .. check it not mounted any more
		}

		[TestMethod]
		public void MulipleSeeks()										// test that we can seek around easily
		{
			_2310.Mount(_cartridge);										// mount up cartridge
			SeekToCylinder(100);											// go to cylinder 100
			Assert.AreEqual(100, _cartridge.CurrentCylinder);				// .. assure we are there
			SeekToCylinder(95);												// go to cylinder 95
			Assert.AreEqual(95, _cartridge.CurrentCylinder);				// .. assure we are there
			SeekToCylinder(0);												// go to cylinder 95
			Assert.AreEqual(0, _cartridge.CurrentCylinder);					// .. assure we are there
		}

		[TestMethod]
		public void Read_ReadSector_Zero()								// test read for sector zero
		{
			_2310.Mount(_cartridge);										// mount one up
			Assert.IsTrue(_cartridge.Mounted);								// .. check it's mounted
			ReadSector(0);													// .. attempt read or sector zero
			
		}

		#region Helpers

		private void SeekToCylinder(int cylNumber)						// seek to specific cylinder
		{
			var offset = cylNumber - _cartridge.CurrentCylinder;			// distance and direction to move
			if (offset == 0)												// q. any movement?
				return;														// a. no .. don't move
			byte dir = (byte) (offset < 0 ? Device2310.SeekBackward : 0);	// set direction
			offset = Math.Abs(offset);										// ensure offset positive
			IssueControl(_2310, offset, dir);								// seek to new cylinder
			var status = GetCurrentStatus() | Device2310.Busy;				// busy .. maybe home
			Assert.AreEqual(status, Sense(_2310));							// .. ensure it is busy
			_2310.Run();													// .. do the seek
			status = GetCurrentStatus() | Device2310.OperationComplete;		// op copmlete ... maybe home
			Assert.AreEqual(status, Sense(_2310, 1));						// Reset the interrupt and ensure complete
		}

		private void ReadSector(int sectorNumber)						// read a sector into memory
		{																// sector is on current cylinder... Sector 0...7
			Assert.Fail("Test the read function");
		}

		private ushort GetCurrentStatus()								// calculate some sense bits
		{																	// check for home
			return (ushort) (_cartridge.CurrentCylinder == 0 ? Device2310.AtCylZero : 0);
		}

		private void StartDriveAndTestReady()							// mount a cart and check it's ready
		{
			_2310.Mount(_cartridge);											// mount the cart
			var acc = Sense(_2310);												// ... get the sense value
			Assert.AreEqual(Device2310.AtCylZero, acc);							// ... should be ready at cylinder 0
		}

		private ushort Sense(IDevice device, ushort resetBit = 0)		// sense without returning sector counts
		{
			SenseDevice(device, resetBit);										// sense the device
			return (ushort) (InsCpu.Acc & 0xfffc);								// .. return acc without sector count bits
		}

		private void TestBadDriveNumber(int driveNumber)				// check bad drive numbers
		{
			try																	// catch any error
			{																	// .. for bad device
				_2310 = new Device2310(InsCpu, driveNumber);					// try to create bad device
			}
			catch (ArgumentException ex)										// catch an argument exception..
			{																	// .. assure it has the right message
				Assert.AreEqual("2310 drive number must be between 0 and 4", ex.Message);
				return;															// .. and return after exception
			}
			Assert.Fail("Bad Device number should have thrown exception.");		// uh oh.. wrong or no exception.
		}

		#endregion

		#region Fake Cartridge

		/******************************
		 * Fake cartridge for testing 
		 ******************************/

		public class FakeCartridge : ICartridge								// make believe cartridge
		{
			public int CurrentCylinder { get; set; }						// current cylinder (set after seek)
			public bool MountCalled { get; set; }							// indicates mount called
			public bool ReadCalled { get; set; }							// indicates read called
			public int SectorRead { get; set; }								// sector read
			public bool FlushCalled { get; set; }							// indicates flush called
			public bool Mounted { get; private set; }						// indicates if mounted
			private readonly ushort[] _sector = new ushort[321];			// place for a sector

			public ushort[] Read(int sector)								// read a sector
			{
				for (int i = 1; i <= 32; i++)									// load the first 32 words
				{
					_sector[i] = (ushort) (i & 0xffff);							// .. with their offset
				}
				_sector[0] = (ushort) sector;									// set the sector number
				return _sector;													// .. and return the sector
			}

			public void Mount()												// mount	
			{
				MountCalled = true;												// show mount called
				Mounted = true;													// .. and show mounted
			}

			public void Flush()												// flush
			{
				FlushCalled = true;												// show flush called
			}

			public void UnMount()											// Remove cart from drive
			{
				Mounted = false;												// unmounted!
			}
		}

		#endregion
	}
}