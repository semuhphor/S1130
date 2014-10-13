using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	[TestClass]
	public class InterruptHandlingTests : InterruptTestBase
	{
		[TestMethod]
		public void HandlesInterruptCorrectly()
		{
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu.InterruptQueues[4].Enqueue(GetInterrupt(4));		// set up the interrupting device
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
			InsCpu.InterruptQueues[4].Enqueue(GetInterrupt(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[2].Enqueue(GetInterrupt(2));		// set up the interrupting device
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
			InsCpu.InterruptQueues[2].Enqueue(GetInterrupt(2));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[4].Enqueue(GetInterrupt(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(0x601, InsCpu.Iar);							// .. should be in first word of level 2 handler
			Assert.AreEqual(0x000, InsCpu[0x500]);						// .. and nothing should be in level 4 return.
			Assert.AreEqual(0x100, InsCpu[0x600]);						// .. and level 2 return address should be set.
		}

		[TestMethod]
		public void ShouldClearOutCurrentInterrupt()
		{
			var interrupt = GetInterrupt(4);						// set up device to handle
			InsCpu.InterruptQueues[4].Enqueue(interrupt);				// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.IsNotNull(interrupt.CausingDevice.ActiveInterrupt);	// .. device still shows interrupt not complete
			InsCpu.ClearCurrentInterrupt();								// now clear it (ignore BOSC... that's 1130 processing)
			Assert.IsNotNull(interrupt.CausingDevice.ActiveInterrupt);	// .. interrupt still not complete (XIO Required for this.)
			Assert.IsNull(InsCpu.CurrentInterruptLevel);				// .. no interrupt active
			Assert.IsTrue(InsCpu.InterruptQueues[4].IsEmpty);			// .. no device on interrupt 4
			Assert.IsTrue(InsCpu.CurrentInterrupt.IsEmpty);				// .. and none currently active
		}

		[TestMethod]
		public void ShouldClearInterruptsInProperOrder()
		{
			InsCpu.InterruptQueues[4].Enqueue(GetInterrupt(4));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.InterruptQueues[2].Enqueue(GetInterrupt(2));		// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.AreEqual(2, InsCpu.CurrentInterrupt.Count);				// .. and both on interrupt stack
			InsCpu.ClearCurrentInterrupt();								// should clear level 2
			Assert.AreEqual(1, InsCpu.CurrentInterrupt.Count);				// .. and only one currently active
			Assert.AreEqual(4, InsCpu.CurrentInterruptLevel);						// .. level 4 still active
			Assert.IsTrue(InsCpu.InterruptQueues[2].IsEmpty);			// .. no device on interrupt 2
			InsCpu.ClearCurrentInterrupt();								// now clear level 4 (ignore BOSC... that's 1130 processing)
			Assert.IsNull(InsCpu.CurrentInterruptLevel);							// .. no interrupt active
			Assert.IsTrue(InsCpu.InterruptQueues[4].IsEmpty);			// .. no device on interrupt 4
			Assert.IsTrue(InsCpu.CurrentInterrupt.IsEmpty);				// .. and none currently active
		}

		[TestMethod]
		public void TestFullInterruptRoutine()
		{
			var dummyDevice = new DummyDevice(4);						// device causing interrupt
																		// iocc to sense the dummy device
			InstructionBuilder.BuildIoccAt(dummyDevice, DevFuction.SenseDevice, 0, 1, InsCpu, 0x400); 
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InstructionBuilder.BuildShortAtAddress(OpCodes.ShiftLeft, 0, 0, InsCpu, 0x100);	// build a NOOP instruction
			InstructionBuilder.BuildShortAtAddress(OpCodes.Wait, 0, 0, InsCpu, 0x101);		// .. then wait
			InstructionBuilder.BuildLongAtAddress(OpCodes.ExecuteInputOuput, 0, 0x400, InsCpu, 0x501);	// XIO to reset device
			InstructionBuilder.BuildLongIndirectBranchAtAddress(OpCodes.BranchSkip, 0, 0x40, 0x500, InsCpu, 0x503); // .. and the return from interrupt
			InsCpu.InterruptQueues[4].Enqueue(dummyDevice.GetInterrupt());// set up the interrupting device
			Assert.AreEqual(0x100, InsCpu.Iar);							// not in routine
			ExecuteOneInstruction();									// execute first noop...
			Assert.AreEqual(0x501, InsCpu.Iar);							// .. should be in the routine 
			Assert.IsFalse(dummyDevice.InterruptCompleted);				// .. interrupt should not be complete
			Assert.AreEqual(InsCpu[0x500], 0x101);						// .. return should be stored now
			ExecuteOneInstruction();									// execute handler XIO ...
			Assert.AreEqual(0x503, InsCpu.Iar);							// .. should still be in the routine 
			Assert.AreEqual(0x001f, InsCpu.Iar);						// .. value returned by dummy device
			Assert.IsTrue(dummyDevice.InterruptCompleted);				// .. interrupt should now be complete
			ExecuteOneInstruction();									// return from interrupt
			Assert.AreEqual(0x101, InsCpu.Iar);							// .. should have returned from routine
			Assert.IsNull(InsCpu.CurrentInterruptLevel);							// .. no interrupt active
			Assert.IsTrue(InsCpu.InterruptQueues[4].IsEmpty);			// .. no device on interrupt 4
			Assert.IsTrue(InsCpu.CurrentInterrupt.IsEmpty);				// .. and none currently active
			ExecuteOneInstruction();									// return from interrupt
			Assert.AreEqual(0x102, InsCpu.Iar);							// .. should have executed wait
			Assert.IsTrue(InsCpu.Wait);									// .. and in wait state
		}
	}
}