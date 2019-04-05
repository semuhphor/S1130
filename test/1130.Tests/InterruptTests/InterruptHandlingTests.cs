using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace Tests
{
	[Collection("Interrupt tests")]
	public class InterruptHandlingTests : InterruptTestBase
	{
		[Fact]
		public void HandlesInterruptCorrectly()
		{
			BeforeEachTest();
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu.AddInterrupt(GetInterrupt(4));						// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.Equal(0x501, InsCpu.Iar);							// .. should be in first word of handler
			Assert.Equal(0x100, InsCpu[0x500]);						// .. and return address should be set.
		}

		[Fact]
		public void NothingShouldHappenIfNoInterruptActive()
		{
			BeforeEachTest();
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.Equal(0x100, InsCpu.Iar);							// .. next instruction is same place
		}

		[Fact]
		public void ShouldHandleHigherLevelInterrupt()
		{
			BeforeEachTest();
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu[Constants.InterruptVectors[2]] = 0x600;				// set the interrupt vector
			GetInterrupt(4);
			Assert.Equal(1, InsCpu.ActiveInterruptCount);			// .. should be one interrupt active
			InsCpu.HandleInterrupt();									// handle the interrupt
			GetInterrupt(2);
			Assert.Equal(2, InsCpu.ActiveInterruptCount);			// .. should be one interrupt active
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.Equal(0x601, InsCpu.Iar);							// .. should be in first word of handler
			Assert.Equal(0x100, InsCpu[0x500]);						// .. and level 5 return address should be set.
			Assert.Equal(0x501, InsCpu[0x600]);						// .. and level 2 return address should be set.
		}

		[Fact]
		public void ShouldNotHandleLowerLevelInterrupt()
		{
			BeforeEachTest();
			InsCpu[Constants.InterruptVectors[4]] = 0x500;				// set the interrupt vector
			InsCpu[Constants.InterruptVectors[2]] = 0x600;				// set the interrupt vector
			InsCpu.AddInterrupt(GetInterrupt(2));						// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			InsCpu.AddInterrupt(GetInterrupt(4));						// set up the interrupting device
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.Equal(0x601, InsCpu.Iar);							// .. should be in first word of level 2 handler
			Assert.Equal(0x000, InsCpu[0x500]);						// .. and nothing should be in level 4 return.
			Assert.Equal(0x100, InsCpu[0x600]);						// .. and level 2 return address should be set.
		}

		[Fact]
		public void ShouldClearOutCurrentInterrupt()
		{
			BeforeEachTest();
			var dev4 = new DummyDevice(InsCpu);							// build first device
			Assert.True(InsCpu.AddDevice(dev4));						// .. add to cpu
			var interrupt = dev4.GetInterrupt(InsCpu, 4);				// Enqueue new interrupt
			InsCpu.HandleInterrupt();									// .. handle the interrupt
			Assert.NotNull(interrupt.CausingDevice.ActiveInterrupt);	// .. device still shows interrupt not complete
			dev4.ClearActiveInterrupt();								// reset interruptint device
			InsCpu.ClearCurrentInterrupt();								// .. now clear it (ignore BOSC... that's 1130 processing)
			Assert.Null(InsCpu.CurrentInterruptLevel);					// .. no interrupt active
			Assert.True(InsCpu.InterruptQueues[4].IsEmpty);				// .. no device on interrupt 4
			Assert.True(InsCpu.CurrentInterrupt.IsEmpty);				// .. and none currently active
		}

		[Fact]
		public void ShouldClearInterruptsInProperOrder()
		{
			BeforeEachTest();
			var dev4 = new DummyDevice(InsCpu);							// build first device
			Assert.True(InsCpu.AddDevice(dev4));						// .. add to cpu
			dev4.GetInterrupt(InsCpu, 4);								// Enqueue new interrupt
			Assert.Equal(1, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			InsCpu.HandleInterrupt();									// handle the interrupt
			var dev2 = new DummyDevice(InsCpu, 0x1e);					// build first device, different address
			Assert.True(InsCpu.AddDevice(dev2));						// .. add to cpu
			dev2.GetInterrupt(InsCpu, 2);								// Enqueue new interrupt
			Assert.Equal(2, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			InsCpu.HandleInterrupt();									// handle the interrupt
			Assert.Equal(2, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			Assert.Equal(2, InsCpu.CurrentInterrupt.Count);				// .. and both on interrupt stack
			dev2.ClearActiveInterrupt();								// reset int 2 device (like XIO SENSE DEVICE)
			Assert.Equal(2, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			InsCpu.ClearCurrentInterrupt();								// ... should clear level 2
			Assert.Equal(1, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			Assert.Single(InsCpu.CurrentInterrupt);						// .. and only one currently active
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);				// .. level 4 still active
			Assert.True(InsCpu.InterruptQueues[2].IsEmpty);				// .. no device on interrupt 2
			dev4.ClearActiveInterrupt();								// reset int 4 device ...
			InsCpu.ClearCurrentInterrupt();								// .. now clear level 4 (ignore BOSC... that's 1130 processing)
			Assert.Equal(0, InsCpu.ActiveInterruptCount);				// .. ensure the count is correct
			Assert.Null(InsCpu.CurrentInterruptLevel);					// .. no interrupt active
			Assert.True(InsCpu.InterruptQueues[4].IsEmpty);				// .. no device on interrupt 4
			Assert.True(InsCpu.CurrentInterrupt.IsEmpty);				// .. and none currently active
			Assert.Equal(2, InsCpu.IntPool.Count);						// .. and two ints should be in the bag
		}

		[Fact]
		public void TestFullInterruptRoutine()
		{
			BeforeEachTest();
			var dummyDevice = new DummyDevice(InsCpu);																// device causing interrupt
			InsCpu.AddDevice(dummyDevice);																			// .. add the device to the system.
			InstructionBuilder.BuildIoccAt(dummyDevice, DevFunction.SenseDevice, 1, 1, InsCpu, 0x400);				// iocc to sense & reset the dummy device
			InsCpu[Constants.InterruptVectors[4]] = 0x500;															// set the interrupt vector
			InstructionBuilder.BuildShortAtAddress(OpCodes.ShiftLeft, 0, 0, InsCpu, 0x100);							// build a NOOP instruction
			InstructionBuilder.BuildShortAtAddress(OpCodes.Wait, 0, 0, InsCpu, 0x101);								// .. then wait
			InstructionBuilder.BuildLongAtAddress(OpCodes.ExecuteInputOuput, 0, 0x400, InsCpu, 0x501);				// XIO to reset device
			InstructionBuilder.BuildLongIndirectBranchAtAddress(OpCodes.BranchSkip, 0, 0x40, 0x500, InsCpu, 0x503); // .. and the return from interrupt
			dummyDevice.GetInterrupt(InsCpu, 4);																	// start the interrupt
			Assert.Equal(0x100, InsCpu.Iar);																		// not in routine
			ExecuteOneInstruction();																				// execute first noop...
			Assert.Equal(0x501, InsCpu.Iar);																		// .. should be in the routine 
			Assert.NotNull(dummyDevice.ActiveInterrupt);															// .. interrupt should not be complete
			var interrupt = dummyDevice.ActiveInterrupt;															// .. (save the interrupt)
			Assert.False(dummyDevice.ActiveInterrupt.InBag);														// .. should not be pooled
			Assert.Equal(0x101, InsCpu[0x500]);																		// .. return should be stored now
			ExecuteOneInstruction();																				// execute handler XIO ...
			Assert.Equal(0x503, InsCpu.Iar);																		// .. should still be in the routine 
			Assert.Equal(0x001f, InsCpu.Acc);																		// .. value returned by dummy device
			Assert.Null(dummyDevice.ActiveInterrupt);																// .. interrupt should now be complete
			Assert.False(interrupt.InBag);																			// .. and interrupt not yet returned to the bag
			ExecuteOneInstruction();																				// return from interrupt
			Assert.True(interrupt.InBag);																			// .. and interrupt now returned to the bag
			Assert.Equal(0x101, InsCpu.Iar);																		// .. should have returned from routine
			Assert.Null(InsCpu.CurrentInterruptLevel);																// .. no interrupt active
			Assert.True(InsCpu.InterruptQueues[4].IsEmpty);															// .. no device on interrupt 4
			Assert.True(InsCpu.CurrentInterrupt.IsEmpty);															// .. and none currently active
			ExecuteOneInstruction();																				// return from interrupt
			Assert.Equal(0x102, InsCpu.Iar);																		// .. should have executed wait
			Assert.True(InsCpu.Wait);																				// .. and in wait state
		}

		[Fact]
		public void TestIfInterruptNotReset()
		{
			BeforeEachTest();
			var dummyDevice = new DummyDevice(InsCpu);																// device causing interrupt
			InsCpu.AddDevice(dummyDevice);																			// .. add the device to the system.
			InstructionBuilder.BuildIoccAt(dummyDevice, DevFunction.SenseDevice, 0, 1, InsCpu, 0x400);				// iocc to sense & NOT reset the dummy device
			InsCpu[Constants.InterruptVectors[4]] = 0x500;															// set the interrupt vector
			InstructionBuilder.BuildShortAtAddress(OpCodes.ShiftLeft, 0, 0, InsCpu, 0x100);							// build a NOOP instruction
			InstructionBuilder.BuildShortAtAddress(OpCodes.Wait, 0, 0, InsCpu, 0x101);								// .. then wait
			InstructionBuilder.BuildLongAtAddress(OpCodes.ExecuteInputOuput, 0, 0x400, InsCpu, 0x501);				// XIO to sense, NOT reset device
			InstructionBuilder.BuildLongIndirectBranchAtAddress(OpCodes.BranchSkip, 0, 0x40, 0x500, InsCpu, 0x503); // .. and the return from interrupt
			dummyDevice.GetInterrupt(InsCpu, 4);																	// start the interrupt
			Assert.Equal(0x100, InsCpu.Iar);																		// not in routine
			ExecuteOneInstruction();																				// execute first noop...
			Assert.Equal(0x501, InsCpu.Iar);																		// .. should be in the routine 
			Assert.NotNull(dummyDevice.ActiveInterrupt);															// .. interrupt should not be complete
			var interrupt = dummyDevice.ActiveInterrupt;															// .. (save the interrupt)
			Assert.False(dummyDevice.ActiveInterrupt.InBag);														// .. should not be pooled
			Assert.Equal(0x101, InsCpu[0x500]);																		// .. return should be stored now
			ExecuteOneInstruction();																				// execute handler XIO ...
			Assert.Equal(0x503, InsCpu.Iar);																		// .. should still be in the routine 
			Assert.Equal(0x001f, InsCpu.Acc);																		// .. value returned by dummy device
			Assert.NotNull(dummyDevice.ActiveInterrupt);															// .. interrupt should not be complete
			Assert.False(interrupt.InBag);																			// .. and interrupt still out in the wild
			ExecuteOneInstruction();																				// return from interrupt
			Assert.Equal(0x501, InsCpu.Iar);																		// .. should be back in the routine
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);															// .. interrupt active
			Assert.False(InsCpu.InterruptQueues[4].IsEmpty);														// .. no device on interrupt 4
			Assert.False(InsCpu.CurrentInterrupt.IsEmpty);															// .. and none currently active
			ExecuteOneInstruction();																				// sense device .. no reset
			ExecuteOneInstruction();																				// return from interrupt
			Assert.Equal(0x501, InsCpu.Iar);																		// .. should be back in the routine
			Assert.Equal(1, InsCpu.ActiveInterruptCount);															// .. interrupt still active
			ExecuteOneInstruction();																				// sense device .. no reset
			ExecuteOneInstruction();																				// return from interrupt
			Assert.Equal(0x501, InsCpu.Iar);																		// .. should be back in the routine
			Assert.Equal(1, InsCpu.ActiveInterruptCount);															// .. interrupt still active
			InsCpu[0x401] |= 1;																						// make the sense device reset the interrupt
			ExecuteOneInstruction();																				// sense device .. no reset
			ExecuteOneInstruction();																				// return from interrupt
			Assert.Equal(0x101, InsCpu.Iar);																		// .. should be back in the routine
			Assert.Equal(0, InsCpu.ActiveInterruptCount);															// .. interrupt still active
		}
	}
}