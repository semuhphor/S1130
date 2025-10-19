using System;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
public class Device1442Tests : DeviceTestBase
    {
        private Device1442 _1442;
        protected Deck ADeck;

        protected override void BeforeEachTest()
        {
            base.BeforeEachTest();
            _1442 = new Device1442(InsCpu);
            ADeck = new Deck() + new [] { new Card(), new Card()};
        }

        [Fact]
        public void ShouldHaveCorrectDeviceCode()
        {
            BeforeEachTest();
            Assert.Equal(0x02, _1442.DeviceCode); // 00010 binary
        }

        [Fact]
        public void ShouldReturnNotReadyWithoutCards()
        {
            BeforeEachTest();
            SenseDevice(_1442);
            Assert.Equal(Device1442.NotReadyOrBusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldShowBusyDuringRead()
        {
            BeforeEachTest();
            _1442.ReadHopper.Enqueue(new Card());
            InitiateRead(_1442, 0x400, 80);
            SenseDevice(_1442);
            Assert.Equal(Device1442.BusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldShowOperationCompleteAfterRead()
        {
            BeforeEachTest();
            _1442.ReadHopper.Enqueue(new Card());
            InitiateRead(_1442, 0x400, 80);
            _1442.CompleteCurrentOperation(); // Simulate completion of read
            SenseDevice(_1442);
            Assert.Equal(Device1442.OperationCompleteStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldMoveCardToStackerAfterRead()
        {
            BeforeEachTest();
            var card = new Card();
            _1442.ReadHopper.Enqueue(card);
            InitiateRead(_1442, 0x400, 80);
            _1442.CompleteCurrentOperation();
            
            Assert.True(_1442.ReadHopper.IsEmpty);
            Assert.True(_1442.Stacker.TryPeek(out var stackedCard));
            Assert.Same(card, stackedCard);
        }

        [Fact]
        public void ShouldInitiatePunchOperation()
        {
            BeforeEachTest();
            var card = new Card();
            _1442.PunchHopper.Enqueue(card);
            InitiatePunch(_1442, 0x400, 80);
            SenseDevice(_1442);
            Assert.Equal(Device1442.BusyStatus, InsCpu.Acc);
        }

        private void InitiatePunch(Device1442 device, int wca, int wc)
        {
            InsCpu.IoccDeviceCode = device.DeviceCode;
            InsCpu.IoccFunction = DevFunction.Write;
            InsCpu.IoccAddress = wca;
            InsCpu[wca] = (ushort)wc;
            while (wc != 0)
            {
                InsCpu[wca + wc--] = 0xffff;
            }
            device.ExecuteIocc();
        }

        [Fact]
        public void ShouldHandleFeedCycleControl()
        {
            BeforeEachTest();
            var card = new Card();
            _1442.ReadHopper.Enqueue(card);
            
            // Issue feed cycle control (bit 14)
            IssueControl(_1442, 0, 0x40);
            
            Assert.True(_1442.ReadHopper.IsEmpty);
            Assert.True(_1442.Stacker.TryPeek(out var stackedCard));
            Assert.Same(card, stackedCard);
        }

        [Fact]
        public void ShouldHandleStartReadControl()
        {
            BeforeEachTest();
            _1442.ReadHopper.Enqueue(new Card());
            
            // Issue start read control (bit 13)
            IssueControl(_1442, 0, 0x20);
            
            Assert.True(_1442.IsReading);
            Assert.Equal(Device1442.BusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldHandleStartPunchControl()
        {
            BeforeEachTest();
            _1442.PunchHopper.Enqueue(new Card());
            
            // Issue start punch control (bit 15)
            IssueControl(_1442, 0, 0x80);
            
            Assert.True(_1442.IsPunching);
            Assert.Equal(Device1442.BusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldNotAllowCombinedControlBits()
        {
            BeforeEachTest();
            
            // Try to combine start read and start punch (bits 13 and 15)
            IssueControl(_1442, 0, 0xA0);
            
            Assert.False(_1442.IsReading);
            Assert.False(_1442.IsPunching);
        }

        [Fact]
        public void ShouldGenerateReadResponseInterrupts()
        {
            BeforeEachTest();
            var card = new Card();
            _1442.ReadHopper.Enqueue(card);
            
            // Start read operation  
            IssueControl(_1442, 0, 0x20);
            
            // Simulate reading first column
            _1442.ProcessNextColumn();
            
            Assert.True(_1442.HasColumnInterrupt);
        }

        [Fact(Skip = "runs forever.")]
        public void ShouldReadRandomCardToMemory()
        {
            BeforeEachTest();
            const ushort memoryStart = 0x400;
            const int programStart = 0x100;
            
            // Create a card with random data
            var cardData = new ushort[80];
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 80; i++)
            {
                cardData[i] = (ushort)(random.Next() & 0xFFF); // 12-bit card column data
            }
            var testCard = new Card(cardData);
            _1442.ReadHopper.Enqueue(testCard);
            
            // Build program to read the card
            var currentAddress = (ushort)programStart;
            
            // Control instruction to start read (0x20 = start read bit)
            InstructionBuilder.BuildIoccAt(_1442, DevFunction.Control, 0x20, 0, InsCpu, currentAddress);
            currentAddress += 2;
            
            // Wait for not busy
            var waitLoop = currentAddress;
            InstructionBuilder.BuildIoccAt(_1442, DevFunction.SenseDevice, 0, 0, InsCpu, currentAddress);
            currentAddress += 2;
            InstructionBuilder.BuildShortAtAddress(OpCodes.BranchSkip, 0, 0x2, InsCpu, currentAddress); // Skip if acc bit 14 on (busy)
            currentAddress++;
            InstructionBuilder.BuildShortAtAddress(OpCodes.BranchSkip, 0, 0x1, InsCpu, currentAddress); // Branch to next if bit 15 on (not ready)
            currentAddress++;
            InstructionBuilder.BuildShortAtAddress(OpCodes.BranchStore, 0, (uint)(waitLoop - currentAddress), InsCpu, currentAddress);
            currentAddress++;

            // Initiate read operation
            InstructionBuilder.BuildIoccAt(_1442, DevFunction.InitRead, 0, memoryStart, InsCpu, currentAddress);
            currentAddress += 2;
            
            // Wait for completion
            waitLoop = currentAddress;
            InstructionBuilder.BuildIoccAt(_1442, DevFunction.SenseDevice, 0, 0, InsCpu, currentAddress);
            currentAddress += 2;
            InstructionBuilder.BuildShortAtAddress(OpCodes.BranchSkip, 0, 0x4, InsCpu, currentAddress); // Skip if acc bit 12 on (complete)
            currentAddress++;
            InstructionBuilder.BuildShortAtAddress(OpCodes.BranchStore, 0, (uint)(waitLoop - currentAddress), InsCpu, currentAddress);
            currentAddress++;
            
            // Execute the program
            InsCpu.Iar = programStart;
            while (InsCpu.Iar < currentAddress)
            {
                InsCpu.ExecuteInstruction();
            }
            
            // Verify the card data was transferred to memory
            for (int i = 0; i < 80; i++)
            {
                Assert.Equal(cardData[i], InsCpu[memoryStart + i]);
            }
            
            // Verify card moved to stacker
            Assert.True(_1442.ReadHopper.IsEmpty);
            Assert.True(_1442.Stacker.TryPeek(out var stackedCard));
            Assert.Same(testCard, stackedCard);
        }
    }
}