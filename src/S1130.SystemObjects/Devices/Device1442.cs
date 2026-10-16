using System.Collections.Concurrent;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects.Devices
{
    /* ******************************************************************************************
     * Device: IBM 1442 Card Read Punch (Models 6/7; Model 5 is punch-only)
     *
     * Direct-program-control device (see IBM 1130 Functional Characteristics, "Punched Card
     * Input/Output Devices"). The CPU must issue one Read or Write IOCC per card column in
     * response to a level-0 response interrupt. Operation Complete is a level-4 interrupt.
     *
     * Card path (single hopper):  hopper -> read station -> punch station -> stacker.
     * A feed cycle moves every card one station. Start Read reads the card as it moves from the
     * read station to the punch station; Start Punch punches the card at the punch station,
     * feeding one there first if the station is empty.
     *
     * IOCC functions:
     *   Read    (010)  column image from read buffer -> core word at IOCC address
     *   Write   (001)  core word at IOCC address -> punch one column
     *   Control (100)  modifiers: bit 8 stacker select, bit 13 start read, bit 14 feed cycle,
     *                  bit 15 start punch (13/14/15 must not be combined)
     *   Sense   (111)  DSW -> ACC; modifier bit 15 resets level-0 responses, bit 14 resets level 4
     *
     * Device status word:
     *   bit 0  1... .... .... ....  Read Response        (level 0)
     *   bit 1  .1.. .... .... ....  Punch Response       (level 0)
     *   bit 2  ..1. .... .... ....  Error Check
     *   bit 3  ...1 .... .... ....  Last Card
     *   bit 4  .... 1... .... ....  Operation Complete   (level 4)
     *   bit 14 .... .... .... ..1.  Busy
     *   bit 15 .... .... .... ...1  Not Ready or Busy
     *
     * Card image: card rows 12,11,0,1..9 occupy word bits 0-11 (0x8000..0x0010). Punch data is in
     * bits 0-11 and a 1 in bit 12 (0x0008) marks the last column to punch (end-punch).
     * ******************************************************************************************/
    public class Device1442 : DeviceBase
    {
        // Control modifiers (IOCC low byte; IBM bit n = 1 << (15 - n))
        public const byte StackerSelectBit = 0x80;  // bit 8
        public const byte StartReadBit = 0x04;      // bit 13
        public const byte FeedCycleBit = 0x02;      // bit 14
        public const byte StartPunchBit = 0x01;     // bit 15

        // Sense Device reset modifiers
        public const byte ResetLevel0Bit = 0x01;    // bit 15
        public const byte ResetLevel4Bit = 0x02;    // bit 14

        // Device status word
        public const ushort ReadResponseStatus = 0x8000;
        public const ushort PunchResponseStatus = 0x4000;
        public const ushort ErrorCheckStatus = 0x2000;
        public const ushort LastCardStatus = 0x1000;
        public const ushort OperationCompleteStatus = 0x0800;
        public const ushort BusyStatus = 0x0002;
        public const ushort NotReadyOrBusyStatus = 0x0001;

        public const ushort EndPunchBit = 0x0008;   // bit 12 of the punch data word
        public const ushort CardImageMask = 0xFFF0; // bits 0-11

        public const int ResponseInterruptLevel = 0;
        public const int CompleteInterruptLevel = 4;
        public const ushort Level4Ilsw = 0x2000;    // ILSW bit 2 identifies the 1442 on level 4

        public readonly ConcurrentQueue<ICard> Hopper = new ConcurrentQueue<ICard>();
        public readonly ConcurrentQueue<ICard> Stacker = new ConcurrentQueue<ICard>();

        private ICard _readStation;
        private ICard _punchStation;
        private int _column;
        private bool _reading;
        private bool _punching;
        private bool _readResponse;                  // level-0 interrupt request (cleared by sense reset or Read)
        private bool _punchResponse;                 // level-0 interrupt request (cleared by sense reset or Write)
        private bool _columnReady;                   // a column is waiting to be transferred (cleared by Read/Write)
        private bool _complete;
        private bool _lastCard;
        private bool _errorCheck;
        private bool _started;                       // initial feed (operator START) has been done

        public Device1442(ICpu cpu)
        {
            CpuInstance = cpu;
        }

        public override byte DeviceCode => 0x02;     // 00010

        public bool IsReading => _reading;
        public bool IsPunching => _punching;
        public bool ReadResponsePending => _readResponse;
        public bool PunchResponsePending => _punchResponse;
        public bool OperationComplete => _complete;
        public ICard CardAtReadStation => _readStation;
        public ICard CardAtPunchStation => _punchStation;

        /// <summary>
        /// Operator START key: performs the initial feed cycle that registers the first card at the
        /// read station and makes the device ready.
        /// </summary>
        public void Start()
        {
            _started = true;
            if (_readStation == null)
            {
                FeedOneStation();
            }
        }

        private bool Ready => _started && (_readStation != null || _punchStation != null || !Hopper.IsEmpty);

        public override void ExecuteIocc()
        {
            switch (CpuInstance.IoccFunction)
            {
                case DevFunction.SenseDevice:
                    SenseDevice();
                    break;
                case DevFunction.Control:
                    Control();
                    break;
                case DevFunction.Read:
                    ReadColumn();
                    break;
                case DevFunction.Write:
                    PunchColumn();
                    break;
            }
        }

        private void SenseDevice()
        {
            ushort dsw = 0;
            if (_readResponse) dsw |= ReadResponseStatus;
            if (_punchResponse) dsw |= PunchResponseStatus;
            if (_errorCheck) dsw |= ErrorCheckStatus;
            if (_lastCard) dsw |= LastCardStatus;
            if (_complete) dsw |= OperationCompleteStatus;
            if (_reading || _punching) dsw |= BusyStatus;
            if (!Ready || _reading || _punching) dsw |= NotReadyOrBusyStatus;
            CpuInstance.Acc = dsw;

            var mods = CpuInstance.IoccModifiers;
            if ((mods & ResetLevel0Bit) != 0)
            {
                _readResponse = _punchResponse = false;
                ReleaseInterrupt(ResponseInterruptLevel);
            }
            if ((mods & ResetLevel4Bit) != 0)
            {
                _complete = _errorCheck = false;
                ReleaseInterrupt(CompleteInterruptLevel);
            }
        }

        // The 1442 can have a level 0 response and a level 4 completion pending at once.
        private Interrupt _responseInterrupt;
        private Interrupt _completeInterrupt;

        public override bool HoldsInterrupt(Interrupt interrupt) =>
            interrupt == _responseInterrupt || interrupt == _completeInterrupt;

        private void RaiseInterrupt(int level, ushort ilsw)
        {
            var isResponse = level == ResponseInterruptLevel;
            if ((isResponse ? _responseInterrupt : _completeInterrupt) != null)
            {
                return;
            }
            var interrupt = CpuInstance.IntPool.GetInterrupt(level, this, ilsw);
            if (isResponse) _responseInterrupt = interrupt; else _completeInterrupt = interrupt;
            ActiveInterrupt = _responseInterrupt ?? _completeInterrupt;
            CpuInstance.AddInterrupt(interrupt);
        }

        private void ReleaseInterrupt(int level)
        {
            var isResponse = level == ResponseInterruptLevel;
            var interrupt = isResponse ? _responseInterrupt : _completeInterrupt;
            if (interrupt == null)
            {
                return;
            }
            if (isResponse) _responseInterrupt = null; else _completeInterrupt = null;
            ActiveInterrupt = _responseInterrupt ?? _completeInterrupt;
            CpuInstance.RemoveInterrupt(interrupt);
        }

        private void Control()
        {
            if (!Ready || _reading || _punching)
            {
                return;
            }
            var mods = CpuInstance.IoccModifiers;
            var selected = mods & (StartReadBit | FeedCycleBit | StartPunchBit);
            if (selected != 0 && (selected & (selected - 1)) != 0)
            {
                return;                                   // 13/14/15 must not be combined
            }
            switch (selected)
            {
                case FeedCycleBit:
                    FeedOneStation();
                    break;
                case StartReadBit:
                    StartRead();
                    break;
                case StartPunchBit:
                    StartPunch();
                    break;
            }
        }

        /// <summary>Moves every card one station: punch station -> stacker, read -> punch, hopper -> read.</summary>
        private void FeedOneStation()
        {
            if (_punchStation != null)
            {
                Stacker.Enqueue(_punchStation);
            }
            _punchStation = _readStation;
            _readStation = Hopper.TryDequeue(out var next) ? next : null;
        }

        private void StartRead()
        {
            if (_readStation == null)
            {
                return;                                   // nothing registered at the read station
            }
            _reading = true;
            _complete = false;
            _column = 0;
            _lastCard = Hopper.IsEmpty;                   // last card is now passing the read station
            RequestResponse(isRead: true);
        }

        private void StartPunch()
        {
            if (_punchStation == null)
            {
                FeedOneStation();                         // card feeds past the read station unread
                if (_punchStation == null)
                {
                    return;
                }
            }
            _punching = true;
            _complete = false;
            _column = 0;
            RequestResponse(isRead: false);
        }

        private void RequestResponse(bool isRead)
        {
            if (isRead) _readResponse = true; else _punchResponse = true;
            _columnReady = true;
            RaiseInterrupt(ResponseInterruptLevel, 0);
        }

        private void ReadColumn()
        {
            if (!_reading || !_columnReady)
            {
                return;
            }
            CpuInstance[CpuInstance.IoccAddress] = (ushort)(_readStation.Columns[_column] & CardImageMask);
            _column++;
            _columnReady = false;
            _readResponse = false;
            ReleaseInterrupt(ResponseInterruptLevel);
            if (_column < 80)
            {
                RequestResponse(isRead: true);
            }
            else
            {
                _reading = false;
                FeedOneStation();                         // the read card is now at the punch station
                Complete();
            }
        }

        private void PunchColumn()
        {
            if (!_punching || !_columnReady)
            {
                return;
            }
            var data = CpuInstance[CpuInstance.IoccAddress];
            _punchStation.Columns[_column] = (ushort)(data & CardImageMask);
            _column++;
            _columnReady = false;
            _punchResponse = false;
            ReleaseInterrupt(ResponseInterruptLevel);
            if ((data & EndPunchBit) != 0 || _column >= 80)
            {
                _punching = false;
                Complete();                               // card stays at the punch station until the next feed
            }
            else
            {
                RequestResponse(isRead: false);
            }
        }

        private void Complete()
        {
            _complete = true;
            RaiseInterrupt(CompleteInterruptLevel, Level4Ilsw);
        }

        public static Device1442 operator +(Device1442 device, Deck deck)
        {
            deck.Cards.ForEach(c => device.Hopper.Enqueue(c));
            return device;
        }

        public static Device1442 operator +(Device1442 device, ICard card)
        {
            device.Hopper.Enqueue(card);
            return device;
        }
    }
}
