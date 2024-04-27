using System;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	public class Device2310Tests : DeviceTestBase
	{
		private Device2310 _2310;										// 2310 for testing
		private FakeCartridge _cartridge;								// cartridge to test

		protected override void BeforeEachTest()							// before each test ..
		{
			base.BeforeEachTest();											// .. run the base (creates new cpu)
			_2310 = new Device2310(InsCpu);									// creeate new 2310
			_cartridge = new FakeCartridge();								// .. and new cartridge to use
		}

		[Fact]
		public void CreateDeviceTests_ShouldHaveCorrectDeviceId()		// test that device ids are correct
		{
			BeforeEachTest();
			Assert.Equal(0x04, new Device2310(InsCpu).DeviceCode);		// device zero 
			Assert.Equal(0x09, new Device2310(InsCpu, 1).DeviceCode);	// device one
			Assert.Equal(0x0a, new Device2310(InsCpu, 2).DeviceCode);	// device two
			Assert.Equal(0x0b, new Device2310(InsCpu, 3).DeviceCode);	// device three
			Assert.Equal(0x0c, new Device2310(InsCpu, 4).DeviceCode);	// device four
		}

		[Fact]
		public void BadDeviceNumber_InvalidDriveNumberThrowsException()	// test that it won't create bad devices
		{
			BeforeEachTest();
			TestBadDriveNumber(-1);											// test that device number below zero fails
			TestBadDriveNumber(5);											// .. and device number above four fails
		}

		[Fact]
		public void Seek_TestNewCylIlswAndIntLevel()					// test that seek move head and interrupts
		{
			BeforeEachTest();
			StartDriveAndTestReady();										// prepare drive to accept command
			var acc = Sense(_2310);											// check the status
			Assert.Equal(Device2310.AtCylZero, InsCpu.Acc);					// ensure at cyl 0
			IssueControl(_2310, 1, Device2310.SeekForward);					// move in one cylinder
			acc = Sense(_2310);												// check the status
			Assert.Equal(Device2310.Busy|Device2310.AtCylZero, acc);		// .. we should be busy and still at cyl zero
			_2310.Run();													// .. now run the seek
			Assert.Equal(1, _cartridge.CurrentCylinder);					// assert the move occurred
			var activeInterrupt = _2310.ActiveInterrupt;					// get the active interrupt
			Assert.NotNull(activeInterrupt);								// .. assert an interrupt requested
			Assert.Equal(2, activeInterrupt.InterruptLevel);				// .. assert level 2
			Assert.Equal(0x8000, activeInterrupt.Ilsw);						// .. and is for drive 0
			acc = Sense(_2310);												// check the status
			Assert.Equal(Device2310.OperationComplete, InsCpu.Acc);			// operation is complete
		}

		[Fact]
		public void Seek_TestZeroCylinderSeek()							// test that zero cylinder seek does nothing
		{
			BeforeEachTest();
			StartDriveAndTestReady();										// prepare drive to accept command
			var acc = Sense(_2310);											// check the status
			Assert.Equal(Device2310.AtCylZero, acc);						// ensure at cyl 0
			IssueControl(_2310, 0, Device2310.SeekForward);					// move zero cylinders
			_2310.Run();													// .. and run the seek
			Assert.Equal(0, _cartridge.CurrentCylinder);					// assert no move occurred
			var activeInterrupt = _2310.ActiveInterrupt;					// get the active interrupt
			Assert.Null(activeInterrupt);									// .. assert no interrupt requested
			acc = Sense(_2310);												// check the status
			Assert.Equal(Device2310.AtCylZero, acc);						// ensure at cyl 0
		}

		[Fact]
		public void WithoutDisk_DriveNotReady()							// test that drive not ready before mount
		{
			BeforeEachTest();
			var acc = Sense(_2310);											// sense the drive
			Assert.Equal(Device2310.NotReady, acc);						// .. check that it's not ready
		}

		[Fact]
		public void WithDisk_DriveReady_MarksDriveMounted()				// test that mounting makes drive ready
		{
			BeforeEachTest();
			Assert.False(_cartridge.Mounted);								// check that it's not mounted yet
			_2310.Mount(_cartridge);										// .. mount it.
			var acc = Sense(_2310);											// get DSW from drive
			Assert.Equal(Device2310.AtCylZero, acc);						// .. it is at cyl 0
			Assert.True(_cartridge.MountCalled);							// .. mount was called on cart
			Assert.True(_cartridge.Mounted);								// .. cart thinks it is mounted
		}

		[Fact]
		public void UnMount_DriveNotReadey_Flushes()					// test that unmounting flushes and makes not ready
		{
			BeforeEachTest();
			_2310.Mount(_cartridge);										// mount one up
			Assert.True(_cartridge.Mounted);								// .. check it's mounted
			_2310.UnMount();												// now unmount it
			var acc = Sense(_2310);											// get DSW
			Assert.Equal(Device2310.NotReady, acc);						// .. check not ready
			Assert.True(_cartridge.FlushCalled);							// .. check that device told cart to flush
			Assert.False(_cartridge.Mounted);								// .. check it not mounted any more
		}

		[Fact]
		public void MulipleSeeks()										// test that we can seek around easily
		{
			BeforeEachTest();
			_2310.Mount(_cartridge);										// mount up cartridge
			SeekToCylinder(100, _cartridge);								// go to cylinder 100
			Assert.Equal(100, _cartridge.CurrentCylinder);					// .. assure we are there
			SeekToCylinder(95, _cartridge);									// go to cylinder 95
			Assert.Equal(95, _cartridge.CurrentCylinder);					// .. assure we are there
			SeekToCylinder(0, _cartridge);									// go to cylinder 95
			Assert.Equal(0, _cartridge.CurrentCylinder);					// .. assure we are there
		}

		[Fact]
		public void Read_ReadSector_Zero()								// test read for sector zero
		{
			BeforeEachTest();	
			ReadSectorAndCheck(0, 0x1000);									// read sector zero
		}

		[Fact]
		public void Read_ReadSector_44()								// test read for sector zero
		{
			BeforeEachTest();	
			ReadSectorAndCheck(44, 0x1000);									// read sector zero
		}

		[Fact]
		public void Write_WriteSector_Zero()							// Write the first sector on the disk
		{
			BeforeEachTest();
			WriteSectorAndCheck(0, 0x1000);									// write a sector and check the output
		}

		[Fact]
		public void Write_WriteSector_1205()							// Write sector 1205
		{
			BeforeEachTest();
			WriteSectorAndCheck(1205, 0x1000);									// write a sector and check the output
		}

		[Fact]
		public void Write_WriteSector_47()								// Write sector 1200
		{
			BeforeEachTest();
			WriteSectorAndCheck(47, 0x1000);									// write a sector and check the output
		}

		[Fact]
		public void Read_ReadSector_Last()								// test read for sector zero
		{
			BeforeEachTest();
			int sector = (202 << 3) + 7;									// get last sector	
			ReadSectorAndCheck(sector, 0x1000);								// read sector zero
		}

		#region Helpers

		private void ReadSectorAndCheck(int sector, int wca)			// mount a fake cartride, read it and check results
		{
			var cart = new FakeCartridge();									// make a fake
			_2310.Mount(cart);												// mount it
			SeekToCylinder(sector >> 3, cart);								// .. get to the correct cylinder
			ReadSector(sector, (ushort) wca, cart);							// .. attempt read of sector zero
			CheckSectordReadProperly(wca + 1, InsCpu[wca], cart, sector);	// .. check that the sector read ok.
			Assert.True(cart.ReadCalled);									// ensure it was read
			Assert.Equal(sector & 0x7, cart.SectorNumber);					// ensure correct sector
			_2310.UnMount();												// remove the fake
		}

		private void WriteSectorAndCheck(int sector, int wca)			// mount a fake cartride, write to it and check results
		{
			var cart = new FakeCartridge();									// make a fake
			_2310.Mount(cart);												// mount it
			SeekToCylinder(sector >> 3, cart);								// .. get to the correct cylinder
			WriteSector(sector, (ushort) wca, cart);						// .. attempt write of sector zero
			Assert.True(cart.WriteCalled);									// ensure it was written
			CheckSectorWroteProperly(wca + 1, InsCpu[wca], cart, sector);	// .. check that the sector wrote ok.
			Assert.Equal(sector & 0x7, cart.SectorNumber);					// ensure correct sector
			_2310.UnMount();												// remove the fake
		}

		private void SeekToCylinder(int cylNumber, ICartridge cart)		// seek to specific cylinder
		{
			var offset = cylNumber - cart.CurrentCylinder;					// distance and direction to move
			if (offset == 0)												// q. any movement?
				return;														// a. no .. don't move
			byte dir = (byte) (offset < 0 ? Device2310.SeekBackward : 0);	// set direction
			offset = Math.Abs(offset);										// ensure offset positive
			IssueControl(_2310, offset, dir);								// seek to new cylinder
			var status = GetCurrentStatus(cart) | Device2310.Busy;			// busy .. maybe home
			Assert.Equal(status, Sense(_2310));								// .. ensure it is busy
			_2310.Run();													// .. do the seek
			status = GetCurrentStatus(cart) | Device2310.OperationComplete;	// op copmlete ... maybe home
			Assert.Equal(status, Sense(_2310, 1));							// Reset the interrupt and ensure complete
		}

		private void ReadSector(int sectorNumber, ushort wca, ICartridge cart)	// read a sector into memory
		{																		// note sector number is (cyl * 8) + sector {0-7}
			SeekToCylinder(sectorNumber >> 3, cart);							// go to that cylinder
			InitiateRead(_2310, wca, 32, false, (byte) sectorNumber & 0x07);	// get the requested sector
			_2310.Run();														// DO IT!
		}

		private void WriteSector(int sectorNumber, ushort wca, ICartridge cart) // write sector from memory
		{																		// note sector number is (cyl * 8) + sector {0-7}
			SeekToCylinder(sectorNumber >> 3, cart);							// go to that cylinder
			InitiateDiskWrite(_2310, wca, 321, sectorNumber);					// .. write the requested sector
			_2310.Run();														// DO IT!
		}

		private ushort GetCurrentStatus(ICartridge cart)				// calculate some sense bits
		{																		// check for home
			var status =  (ushort) (cart.CurrentCylinder == 0 ? Device2310.AtCylZero : 0);
			return status;
		}

		private void StartDriveAndTestReady()							// mount a cart and check it's ready
		{
			_2310.Mount(_cartridge);											// mount the cart
			var acc = Sense(_2310);												// ... get the sense valueg
			Assert.Equal(Device2310.AtCylZero, acc);							// ... should be ready at cylinder 0
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
				Assert.Equal("2310 drive number must be between 0 and 4", ex.Message);
				return;															// .. and return after exception
			}
			Assert.Fail("Bad Device number should have thrown exception.");	// uh oh.. wrong or no exception.
		}

		protected void CheckSectordReadProperly(int address, int wc, ICartridge cart, int sectorNumber)
		{
			var cyl = sectorNumber >> 3;
			var sector = sectorNumber & 7;
			cart.CurrentCylinder = cyl;
			var testSector = cart.Read(sector);
			for (var i = 0; i < wc; i++)
			{
				if (InsCpu[address + i] != testSector[i])
				{
					Assert.Fail(string.Format("Sector mismatch at offset {0}: memory: {1:x}, sectorWord: {2:x}", i, InsCpu[address + i], testSector[i]));
				}
			}
		}

		protected void CheckSectorWroteProperly(int address, int wc, ICartridge cart, int sectorNumber)
		{
			var cyl = sectorNumber >> 3;
			var sector = sectorNumber & 7;
			cart.CurrentCylinder = cyl;
			ushort[] sectorWritten = ((FakeCartridge) cart).Sector;
			for (var i = 0; i < wc; i++)
			{
				if (InsCpu[address + i] != sectorWritten[i])
				{
					Assert.Fail(string.Format("Sector mismatch at offset {0}: memory: {1:x}, sectorWord: {2:x}, address: {3:x}, wc: {4}", i, InsCpu[address + i], sectorWritten[i], address, InsCpu[address-1]));
				}
			}
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
			public bool WriteCalled { get; set; }							// indicates write called
			public int SectorNumber { get; set; }							// sector read
			public bool FlushCalled { get; set; }							// indicates flush called
			public bool Mounted { get; private set; }						// indicates if mounted
			public readonly ushort[] Sector = new ushort[321];				// place for a sector

			public ushort[] Read(int sectorNumber)								// read a sector
			{
				Assert.False(sectorNumber < 0 || sectorNumber > 7, "Bad sector number!");	// q. sector ok?
																				// a. no .. fail. With Meaning.
				ReadCalled = true;												// show that a read was called
				SectorNumber = sectorNumber;									// sector number requested
				for (int i = 1; i <= 32; i++)									// load the first 32 words
				{
					Sector[i] = (ushort) (i & 0xffff);							// .. with their offset
				}
				Sector[0] = (ushort) (sectorNumber | CurrentCylinder << 3);		// set the sector number
				return Sector;													// .. and return the sector
			}

			public void Write(int sectorNumber, ArraySegment<ushort> sector, int wc)
			{
				Assert.False(sectorNumber < 0 || sectorNumber > 7, "Bad sector number!");	// q. sector ok?
																				// a. no .. fail. With Meaning.
				InitSector();													// ensure all zeros ..
				WriteCalled = true;												// show that a read was called
				SectorNumber = sectorNumber;									// sector number requested
				Array.Copy(sector.Array, sector.Offset, Sector, 0, sector.Count); 	// copy the requested words
				Sector[0] = (ushort) (sectorNumber | CurrentCylinder << 3);		// set the sector number
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

			private void InitSector()										// reset sector to all zeros
			{
				for (int i = 0; i < Sector.Length; i++)					
				{
					Sector[i] = 0;
				}
			}
		}

		#endregion
	}
}