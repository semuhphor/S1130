using System;
using System.Linq;
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
            _1442.ReadHopper.Enqueue(new Card()); // Enqueue second card so first isn't "last card"
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

        [Fact]
        public void ShouldReadRandomCardToMemory()
        {
            BeforeEachTest();
            
            // Register the device with the CPU so XIO can find it
            InsCpu.AddDevice(_1442);
            
            const ushort cardBuffer = 0x200; // Card data buffer address
            
            // Create a card with random data
            var cardData = new ushort[80];
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 80; i++)
            {
                cardData[i] = (ushort)(random.Next() & 0xFFF); // 12-bit card column data
            }
            var testCard = new Card(cardData);
            _1442.ReadHopper.Enqueue(testCard);
            
            // Set up card buffer with word count
            InsCpu[cardBuffer] = 80; // Word count
            
            // Simulate the proper 1442 read sequence as described in the IBM 1130 Functional Characteristics manual:
            // 1. Control Start Read command - starts card moving, triggers read response interrupts for each column
            IssueControl(_1442, 0, 0x20); // Start Read bit (bit 13)
            
            Assert.True(_1442.IsReading, "Card read should have started");
            
            // 2. For each of the 80 columns:
            //    - Device generates a Level 0 (Read Response) interrupt
            //    - Interrupt handler issues a Read command (function 010 = 2) with the address where to store the column
            //    - This must happen within 800µs for model 6, 700µs for model 7
            for (int column = 0; column < 80; column++)
            {
                // Check that column interrupt is active
                Assert.True(_1442.HasColumnInterrupt, $"Expected column interrupt for column {column}");
                
                // Issue Read command to transfer column data from buffer to memory
                // Function code 010 (2) = Read
                // For character-mode devices, IoccAddress is the target memory address (not an IOCC structure)
                InsCpu.IoccDeviceCode = _1442.DeviceCode;
                InsCpu.IoccFunction = (DevFunction)2; // Read function
                InsCpu.IoccAddress = cardBuffer + column + 1; // Direct memory address where to store this column
                _1442.ExecuteIocc();
            }
            
            // 3. After all 80 columns are read, device generates Level 4 (Operation Complete) interrupt
            // The device should no longer be reading
            Assert.False(_1442.IsReading, "Card read should be complete");
            
            // Verify the card data was transferred to memory (starts at cardBuffer+1)
            for (int i = 0; i < 80; i++)
            {
                Assert.Equal(cardData[i], InsCpu[cardBuffer + 1 + i]);
            }
            
            // Verify card moved to stacker
            Assert.True(_1442.ReadHopper.IsEmpty);
            Assert.True(_1442.Stacker.TryPeek(out var stackedCard));
            Assert.Same(testCard, stackedCard);
        }
    }
}