using System;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
    public class Device1442Tests : DeviceTestBase
    {
        private Device1442 _1442;

        protected override void BeforeEachTest()
        {
            base.BeforeEachTest();
            _1442 = new Device1442(InsCpu);
            InsCpu.AddDevice(_1442);
        }

        private void IssueRead(int address)
        {
            InsCpu.IoccDeviceCode = _1442.DeviceCode;
            InsCpu.IoccFunction = DevFunction.Read;
            InsCpu.IoccAddress = address;
            _1442.ExecuteIocc();
        }

        private void IssueWrite(int address)
        {
            InsCpu.IoccDeviceCode = _1442.DeviceCode;
            InsCpu.IoccFunction = DevFunction.Write;
            InsCpu.IoccAddress = address;
            _1442.ExecuteIocc();
        }

        /// <summary>Drives a complete Start Read + 80 column reads the way an interrupt handler would.</summary>
        private void ReadWholeCard(int buffer)
        {
            IssueControl(_1442, 0, Device1442.StartReadBit);
            Assert.True(_1442.IsReading);
            for (int column = 0; column < 80; column++)
            {
                Assert.True(_1442.ReadResponsePending, $"Expected read response for column {column}");
                Assert.Equal(Device1442.ResponseInterruptLevel, _1442.ActiveInterrupt.InterruptLevel);
                SenseDevice(_1442, Device1442.ResetLevel0Bit);
                Assert.Equal(Device1442.ReadResponseStatus | Device1442.BusyStatus | Device1442.NotReadyOrBusyStatus,
                             InsCpu.Acc & ~Device1442.LastCardStatus);
                IssueRead(buffer + column);
            }
            Assert.False(_1442.IsReading);
        }

        private static Card RandomCard(int seed)
        {
            var data = new ushort[80];
            var random = new Random(seed);
            for (int i = 0; i < 80; i++)
            {
                data[i] = (ushort)(random.Next() & 0xFFF0);                     // rows 12..9 in bits 0-11
            }
            return new Card(data);
        }

        [Fact]
        public void ShouldHaveCorrectDeviceCode()
        {
            BeforeEachTest();
            Assert.Equal(0x02, _1442.DeviceCode);                                // 00010 binary
        }

        [Fact]
        public void ControlModifierBits_MatchIoccLowByte()
        {
            Assert.Equal(0x80, Device1442.StackerSelectBit);                     // bit 8
            Assert.Equal(0x04, Device1442.StartReadBit);                         // bit 13
            Assert.Equal(0x02, Device1442.FeedCycleBit);                         // bit 14
            Assert.Equal(0x01, Device1442.StartPunchBit);                        // bit 15
        }

        [Fact]
        public void ShouldReturnNotReadyBeforeOperatorStart()
        {
            BeforeEachTest();
            _1442.Hopper.Enqueue(new Card());
            SenseDevice(_1442);
            Assert.Equal(Device1442.NotReadyOrBusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void ShouldReturnNotReadyWithoutCards()
        {
            BeforeEachTest();
            _1442.Start();
            SenseDevice(_1442);
            Assert.Equal(Device1442.NotReadyOrBusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void StartRegistersFirstCardAtReadStation()
        {
            BeforeEachTest();
            var card = new Card();
            _1442.Hopper.Enqueue(card);
            _1442.Start();
            Assert.Same(card, _1442.CardAtReadStation);
            Assert.True(_1442.Hopper.IsEmpty);
            SenseDevice(_1442);
            Assert.Equal(0, InsCpu.Acc);                                          // ready, not busy
        }

        [Fact]
        public void StartReadRaisesLevel0ReadResponse()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442.Start();
            IssueControl(_1442, 0, Device1442.StartReadBit);
            Assert.True(_1442.IsReading);
            Assert.True(_1442.ReadResponsePending);
            Assert.Equal(0, _1442.ActiveInterrupt.InterruptLevel);
            SenseDevice(_1442);
            Assert.Equal(Device1442.ReadResponseStatus | Device1442.LastCardStatus | Device1442.BusyStatus | Device1442.NotReadyOrBusyStatus, InsCpu.Acc);
        }

        [Fact]
        public void SenseWithLevel0Reset_ClearsResponseButKeepsOperationRunning()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442.Start();
            IssueControl(_1442, 0, Device1442.StartReadBit);
            SenseDevice(_1442, Device1442.ResetLevel0Bit);
            Assert.NotEqual(0, InsCpu.Acc & Device1442.ReadResponseStatus);      // status reported before reset
            Assert.True(_1442.IsReading);                                         // operation NOT aborted
            Assert.False(_1442.ReadResponsePending);
            Assert.Null(_1442.ActiveInterrupt);
        }

        [Fact]
        public void ShouldReadRandomCardToMemory_ColumnByColumn()
        {
            BeforeEachTest();
            var card = RandomCard(42);
            _1442 += card;
            _1442 += new Card();                                                  // second card so first is not "last"
            _1442.Start();

            const int buffer = 0x200;
            ReadWholeCard(buffer);

            for (int i = 0; i < 80; i++)
            {
                Assert.Equal(card.Columns[i], InsCpu[buffer + i]);
            }
            Assert.True(_1442.OperationComplete);
            Assert.Equal(Device1442.CompleteInterruptLevel, _1442.ActiveInterrupt.InterruptLevel);
            Assert.Equal(Device1442.Level4Ilsw, _1442.ActiveInterrupt.Ilsw);
            SenseDevice(_1442);
            Assert.Equal(Device1442.OperationCompleteStatus, InsCpu.Acc);
            Assert.Same(card, _1442.CardAtPunchStation);                          // read card now sits at punch station
            Assert.True(_1442.Stacker.IsEmpty);
        }

        [Fact]
        public void LastCardIsReportedWithOperationComplete()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442.Start();
            ReadWholeCard(0x200);
            SenseDevice(_1442);
            Assert.Equal(Device1442.OperationCompleteStatus | Device1442.LastCardStatus, InsCpu.Acc);
        }

        [Fact]
        public void NewReadWhileLevel4Pending_KeepsBothRequests()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442 += new Card();
            _1442 += new Card();
            _1442.Start();
            ReadWholeCard(0x200);                                                 // level 4 completion left unreset
            IssueControl(_1442, 0, Device1442.StartReadBit);                      // next card requests level 0
            Assert.Equal(2, InsCpu.ActiveInterruptCount);
            Assert.Equal(Device1442.ResponseInterruptLevel, _1442.ActiveInterrupt.InterruptLevel);
            SenseDevice(_1442, Device1442.ResetLevel0Bit);                        // level 0 reset leaves level 4 pending
            Assert.Equal(1, InsCpu.ActiveInterruptCount);
            Assert.Equal(Device1442.CompleteInterruptLevel, _1442.ActiveInterrupt.InterruptLevel);
            SenseDevice(_1442, Device1442.ResetLevel4Bit);
            Assert.Equal(0, InsCpu.ActiveInterruptCount);
            Assert.Null(_1442.ActiveInterrupt);
        }

        [Fact]
        public void SenseWithLevel4Reset_ClearsOperationComplete()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442 += new Card();
            _1442.Start();
            ReadWholeCard(0x200);
            SenseDevice(_1442, Device1442.ResetLevel4Bit);
            Assert.False(_1442.OperationComplete);
            Assert.Null(_1442.ActiveInterrupt);
            SenseDevice(_1442);
            Assert.Equal(0, InsCpu.Acc);
        }

        [Fact]
        public void FeedCycleMovesCardsOneStation()
        {
            BeforeEachTest();
            var a = new Card(); var b = new Card(); var c = new Card();
            _1442 += a; _1442 += b; _1442 += c;
            _1442.Start();                                                        // a at read station
            IssueControl(_1442, 0, Device1442.FeedCycleBit);                      // a -> punch, b -> read
            Assert.Same(a, _1442.CardAtPunchStation);
            Assert.Same(b, _1442.CardAtReadStation);
            Assert.True(_1442.Stacker.IsEmpty);
            IssueControl(_1442, 0, Device1442.FeedCycleBit);                      // a -> stacker, b -> punch, c -> read
            Assert.True(_1442.Stacker.TryPeek(out var stacked));
            Assert.Same(a, stacked);
            Assert.Same(b, _1442.CardAtPunchStation);
            Assert.Same(c, _1442.CardAtReadStation);
        }

        [Fact]
        public void ShouldNotAllowCombinedControlBits()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442.Start();
            IssueControl(_1442, 0, (byte)(Device1442.StartReadBit | Device1442.StartPunchBit));
            Assert.False(_1442.IsReading);
            Assert.False(_1442.IsPunching);
        }

        [Fact]
        public void StartPunchFeedsCardWhenPunchStationEmpty_AndPunchesUntilEndPunchBit()
        {
            BeforeEachTest();
            var blank = new Card();
            _1442 += blank;
            _1442.Start();                                                        // blank at read station

            IssueControl(_1442, 0, Device1442.StartPunchBit);
            Assert.True(_1442.IsPunching);
            Assert.Same(blank, _1442.CardAtPunchStation);                          // fed past the read station unread
            Assert.True(_1442.PunchResponsePending);
            Assert.Equal(0, _1442.ActiveInterrupt.InterruptLevel);

            const int buffer = 0x300;
            InsCpu[buffer + 0] = 0x9000;                                           // 'A' image
            InsCpu[buffer + 1] = 0x8800;
            InsCpu[buffer + 2] = 0x8400 | Device1442.EndPunchBit;                  // last column, still punched

            for (int column = 0; column < 3; column++)
            {
                Assert.True(_1442.PunchResponsePending, $"Expected punch response for column {column}");
                SenseDevice(_1442, Device1442.ResetLevel0Bit);
                Assert.NotEqual(0, InsCpu.Acc & Device1442.PunchResponseStatus);
                IssueWrite(buffer + column);
            }

            Assert.False(_1442.IsPunching);
            Assert.True(_1442.OperationComplete);
            Assert.Equal(Device1442.CompleteInterruptLevel, _1442.ActiveInterrupt.InterruptLevel);
            Assert.Equal(0x9000, blank.Columns[0]);
            Assert.Equal(0x8800, blank.Columns[1]);
            Assert.Equal(0x8400, blank.Columns[2]);                                // end-punch bit itself is not a row
            Assert.Equal(0, blank.Columns[3]);
            Assert.Same(blank, _1442.CardAtPunchStation);                          // stays until next feed
        }

        [Fact]
        public void ReadThenPunchSameCard()
        {
            BeforeEachTest();
            var card = RandomCard(7);
            _1442 += card;
            _1442 += new Card();
            _1442.Start();
            ReadWholeCard(0x200);
            SenseDevice(_1442, Device1442.ResetLevel4Bit);

            IssueControl(_1442, 0, Device1442.StartPunchBit);                     // punches the card just read
            Assert.Same(card, _1442.CardAtPunchStation);
            InsCpu[0x400] = 0x0010 | Device1442.EndPunchBit;
            SenseDevice(_1442, Device1442.ResetLevel0Bit);
            IssueWrite(0x400);
            Assert.Equal(0x0010, card.Columns[0]);
            Assert.True(_1442.OperationComplete);
        }

        [Fact]
        public void WriteWithoutStartPunchIsIgnored()
        {
            BeforeEachTest();
            _1442 += new Card();
            _1442.Start();
            InsCpu[0x400] = 0xFFF0;
            IssueWrite(0x400);
            Assert.False(_1442.IsPunching);
            Assert.Equal(0, _1442.CardAtReadStation.Columns[0]);
        }

        [Fact]
        public void ReadWithoutResponseIsIgnored()
        {
            BeforeEachTest();
            _1442 += RandomCard(3);
            _1442.Start();
            InsCpu[0x200] = 0x1234;
            IssueRead(0x200);
            Assert.Equal(0x1234, InsCpu[0x200]);
        }
    }
}
