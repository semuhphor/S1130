using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

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
    }
}