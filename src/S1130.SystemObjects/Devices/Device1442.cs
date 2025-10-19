using System;
using System.Collections.Concurrent;

namespace S1130.SystemObjects.Devices
{
    /// <summary>
    /// Emulates an IBM 1442 Card Read Punch.
    /// Implements column-by-column processing with automatic checking for both read and punch operations.
    /// </summary>
    public class Device1442 : DeviceBase
    {
        // Control command modifiers (high byte of modifier word)
        private const byte FeedCycleBit = 0x40;     // Bit 14
        private const byte StartReadBit = 0x20;     // Bit 13
        private const byte StartPunchBit = 0x80;    // Bit 15
        private const byte StackerSelectBit = 0x01;  // Bit 8

        // Status word bits
        public const ushort LastCardStatus = 0x1000;
        public const ushort OperationCompleteStatus = 0x0800;
        public const ushort BusyStatus = 0x0002;
        public const ushort NotReadyOrBusyStatus = 0x0001;
        public const ushort ReadCheckStatus = 0x4000;    // Error during read
        public const ushort PunchCheckStatus = 0x2000;   // Error during punch
        public const ushort FeedCheckStatus = 0x8000;    // Card feed error
        
        // Fields for tracking device state
        private int _currentColumn;
        private int _address;
        private bool _readInProgress;
        private bool _punchInProgress;
        private bool _complete;
        private bool _lastCard;
        private ICard _currentCard;
        private bool _operationStarted;

        // Interrupt level status words
        private const ushort ReadIlsw = 0x0800;  // Level 4 interrupt
        private const ushort PunchIlsw = 0x0100; // Level 0 interrupt

        // Card hoppers
        public readonly ConcurrentQueue<ICard> ReadHopper = new ConcurrentQueue<ICard>();
        public readonly ConcurrentQueue<ICard> PunchHopper = new ConcurrentQueue<ICard>();
        public readonly ConcurrentQueue<ICard> Stacker = new ConcurrentQueue<ICard>();

        // Testing and state tracking 
        public bool HasColumnInterrupt { get; private set; }

        public override byte DeviceCode => 0x02; // 00010 binary

        // Properties for testing and state tracking
        public bool IsReading => _readInProgress;
        public bool IsPunching => _punchInProgress;

        public Device1442(ICpu cpu)
        {
            CpuInstance = cpu;
        }

        public override void ExecuteIocc()
        {
            switch (CpuInstance.IoccFunction)
            {
                case DevFunction.SenseDevice:
                    if ((CpuInstance.IoccModifiers & 1) == 1)
                    {
                        // Reset all status bits
                        _complete = false;
                        _operationStarted = false;
                        _readInProgress = false;
                        _punchInProgress = false;
                        DeactivateInterrupt(CpuInstance);
                        CpuInstance.Acc = 0;
                        return;
                    }
                    
                    // Set status bits in priority order
                    CpuInstance.Acc = 0;
                    if (_readInProgress || _punchInProgress)
                    {
                        CpuInstance.Acc = BusyStatus;
                    }
                    else if (_complete)
                    {
                        CpuInstance.Acc = OperationCompleteStatus;
                        if (_lastCard)
                        {
                            CpuInstance.Acc |= LastCardStatus;
                        }
                    }
                    else if (ReadHopper.IsEmpty && PunchHopper.IsEmpty)
                    {
                        CpuInstance.Acc = NotReadyOrBusyStatus;
                    }
                    break;

                case DevFunction.Control:
                    HandleControlCommand();
                    break;

                case DevFunction.InitRead:
                    if (!_operationStarted && ReadHopper.TryDequeue(out _currentCard))
                    {
                        _readInProgress = true;
                        _currentColumn = 0;
                        _complete = false;
                        _lastCard = false;
                        _operationStarted = true;
                        CpuInstance.Acc = BusyStatus;
                        _address = CpuInstance.IoccAddress;
                        ProcessNextReadColumn();
                    }
                    else if (_readInProgress && _currentCard != null)
                    {
                        _address = CpuInstance.IoccAddress;
                        ProcessNextReadColumn();
                    }
                    break;

                case DevFunction.Write:
                    if (!_operationStarted && PunchHopper.TryDequeue(out _currentCard))
                    {
                        _punchInProgress = true;
                        _currentColumn = 0;
                        _complete = false;
                        _operationStarted = true;
                        CpuInstance.Acc = BusyStatus;
                        _address = CpuInstance.IoccAddress;
                        ProcessNextPunchColumn();
                    }
                    else if (_punchInProgress && _currentCard != null)
                    {
                        _address = CpuInstance.IoccAddress;
                        ProcessNextPunchColumn();
                    }
                    break;
            }
        }

        private void HandleControlCommand()
        {
            var modifier = CpuInstance.IoccModifiers;
            var controlBits = (byte)(modifier & (FeedCycleBit | StartReadBit | StartPunchBit));

            // Check for invalid combinations of control bits
            if (BitCount(controlBits) > 1)
            {
                return;
            }

            switch (controlBits)
            {
                case FeedCycleBit:
                    PerformFeedCycle();
                    break;
                case StartReadBit:
                    StartReadOperation();
                    break;
                case StartPunchBit:
                    StartPunchOperation();
                    break;
            }
        }

        private static int BitCount(byte value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        private void PerformFeedCycle()
        {
            if (_currentCard != null)
            {
                Stacker.Enqueue(_currentCard);
                _currentCard = null;
            }

            if (ReadHopper.TryDequeue(out var card))
            {
                Stacker.Enqueue(card);
            }
        }

        private void StartReadOperation()
        {
            if (!_readInProgress && !_punchInProgress && ReadHopper.Count > 0)
            {
                _operationStarted = false;
                _readInProgress = true;
                CpuInstance.Acc = BusyStatus;
                HasColumnInterrupt = true;
                ActivateInterrupt(CpuInstance, 4, ReadIlsw);
            }
        }

        private void StartPunchOperation()
        {
            if (!_readInProgress && !_punchInProgress && PunchHopper.Count > 0)
            {
                _operationStarted = false;
                _punchInProgress = true;
                CpuInstance.Acc = BusyStatus;
                ActivateInterrupt(CpuInstance, 0, PunchIlsw);
            }
        }

        private void ProcessNextReadColumn()
        {
            if (_currentColumn < 80 && _currentCard != null)
            {
                // Read column twice and compare (simulating read check)
                var firstRead = _currentCard.Columns[_currentColumn];
                var secondRead = _currentCard.Columns[_currentColumn];

                if (firstRead == secondRead)
                {
                    CpuInstance[_address] = firstRead;
                    _currentColumn++;
                    HasColumnInterrupt = true;
                    
                    if (_currentColumn >= 80)
                    {
                        CompleteReadOperation();
                    }
                }
                else
                {
                    // Read check error
                    CpuInstance.Acc |= ReadCheckStatus;
                }
            }
        }

        private void ProcessNextPunchColumn()
        {
            if (_currentColumn < 80 && _currentCard != null)
            {
                var punchData = CpuInstance[_address];
                
                // Check for end-punch bit (bit 12)
                if ((punchData & 0x1000) != 0)
                {
                    CompletePunchOperation();
                    return;
                }

                _currentCard.Columns[_currentColumn] = punchData;
                _currentColumn++;
                
                // Simulate punch check
                if (_currentColumn > 0)
                {
                    var previousColumn = _currentCard.Columns[_currentColumn - 1];
                    if (previousColumn != CpuInstance[_address - 1])
                    {
                        CpuInstance.Acc |= PunchCheckStatus;
                    }
                }
                
                HasColumnInterrupt = true;
                ActivateInterrupt(CpuInstance, 0, PunchIlsw);
            }
        }

        private void CompleteReadOperation()
        {
            _readInProgress = false;
            _operationStarted = false;
            _complete = true;
            // Don't change _lastCard here - it was set during init
            HasColumnInterrupt = false;
            
            // Move card to stacker
            if (_currentCard != null)
            {
                Stacker.Enqueue(_currentCard);
                _currentCard = null;
            }
            
            // Signal completion with correct status
            CpuInstance.Acc = OperationCompleteStatus;
            if (_lastCard)
            {
                CpuInstance.Acc |= LastCardStatus;
            }
            ActivateInterrupt(CpuInstance, 4, ReadIlsw);
        }

        private void CompletePunchOperation()
        {
            _punchInProgress = false;
            _operationStarted = false;
            _complete = true;
            HasColumnInterrupt = false;
            
            // Move card to stacker
            if (_currentCard != null)
            {
                Stacker.Enqueue(_currentCard);
                _currentCard = null;
            }
            
            // Signal completion with correct status
            CpuInstance.Acc = OperationCompleteStatus;
            ActivateInterrupt(CpuInstance, 0, PunchIlsw);
        }

        public void CompleteCurrentOperation()
        {
            if (_readInProgress)
            {
                CompleteReadOperation();
            }
            else if (_punchInProgress)
            {
                CompletePunchOperation();
            }
        }

        // Exposed for testing
        public void ProcessNextColumn()
        {
            if (_readInProgress)
            {
                ProcessNextReadColumn();
            }
            else if (_punchInProgress)
            {
                ProcessNextPunchColumn();
            }
        }


    }
}