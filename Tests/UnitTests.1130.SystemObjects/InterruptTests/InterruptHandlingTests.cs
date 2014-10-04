using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	[TestClass]
	public class InterruptHandlingTests : InterruptTestBase
	{
		[TestMethod]
		public void HandlesInterruptCorrectly()
		{
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu.InterruptQueues[4].Enqueue(new DummyDevice(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(0x501, InsCpu.Iar);							// .. should be in first word of handler
			Assert.AreEqual(0x100, InsCpu[0x500]);						// .. and return address should be set.
		}

		[TestMethod]
		public void NothingShouldHappenIfNoInterruptActive()
		{
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(0x100, InsCpu.Iar);							// .. next instruction is same place
		}

		[TestMethod]
		public void ShouldHandleHigherLevelInterrupt()
		{
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu[Constants.InterruptVectors[2]] = 0x600;				// set the interrupt vector
			InsCpu.InterruptQueues[4].Enqueue(new DummyDevice(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[2].Enqueue(new DummyDevice(2));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(0x601, InsCpu.Iar);							// .. should be in first word of handler
			Assert.AreEqual(0x100, InsCpu[0x500]);						// .. and level 5 return address should be set.
			Assert.AreEqual(0x501, InsCpu[0x600]);						// .. and level 2 return address should be set.
		}

		[TestMethod]
		public void ShouldNotHandleLowerLevelInterrupt()
		{
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu[Constants.InterruptVectors[2]] = 0x600;				// set the interrupt vector
			InsCpu.InterruptQueues[2].Enqueue(new DummyDevice(2));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[4].Enqueue(new DummyDevice(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(0x601, InsCpu.Iar);							// .. should be in first word of level 2 handler
			Assert.AreEqual(0x000, InsCpu[0x500]);						// .. and nothing should be in level 4 return.
			Assert.AreEqual(0x100, InsCpu[0x600]);						// .. and level 2 return address should be set.
		}

		[TestMethod]
		public void ShouldClearOutCurrentInterrupt()
		{
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.ClearCurrentInterrupt();								// now clear it (ignore BOSC... that's 1130 processing)
			Assert.IsNull(InsCpu.Interrupt);							// .. no interrupt active
			Assert.IsTrue(InsCpu.InterruptQueues[4].IsEmpty);			// .. no device on interrupt 4
			Assert.IsTrue(InsCpu.CurrentDevice.IsEmpty);				// .. and none currently active
		}

		[TestMethod]
		public void ShouldClearInterruptsInProperOrder()
		{
			InsCpu.InterruptQueues[4].Enqueue(new DummyDevice(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[2].Enqueue(new DummyDevice(2));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(2, InsCpu.CurrentDevice.Count);				// .. and both on interrupt stack
			InsCpu.ClearCurrentInterrupt();								// should clear level 2
			Assert.AreEqual(1, InsCpu.CurrentDevice.Count);				// .. and only one currently active
			Assert.AreEqual(4, InsCpu.Interrupt);						// .. level 4 still active
			Assert.IsTrue(InsCpu.InterruptQueues[2].IsEmpty);			// .. no device on interrupt 2
			InsCpu.ClearCurrentInterrupt();								// now clear level 4 (ignore BOSC... that's 1130 processing)
			Assert.IsNull(InsCpu.Interrupt);							// .. no interrupt active
			Assert.IsTrue(InsCpu.InterruptQueues[4].IsEmpty);			// .. no device on interrupt 4
			Assert.IsTrue(InsCpu.CurrentDevice.IsEmpty);				// .. and none currently active
		}
	}
}