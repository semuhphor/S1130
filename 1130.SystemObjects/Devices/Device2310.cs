using System;

namespace S1130.SystemObjects.Devices
{
	public class Device2310 : DeviceBase
	{
		public const ushort OperationComplete = 0x4000;
		public const ushort NotReady = 0x2000;
		public const ushort Busy = 0x1000;
		public const ushort AtCylZero = 0x0400;
		public const ushort NextSectorMask = 0x0003;

		private readonly byte _deviceCode;
		private ICartridge _cartridge;

		public Device2310(ICpu cpu) : this(cpu, 0)
		{
		}

		public Device2310(ICpu cpu, int driveNumber)
		{
			if (driveNumber < 0 || driveNumber >= 5)							// q. actual device?
			{																	// a. no .. throw error
				throw new ArgumentException("2310 drive number must be between 0 and 4");
			}
			byte[] _driveCodes = {0x4, 0x09, 0x0a, 0x0b, 0x0c};					// possible device codes ...
			_deviceCode = _driveCodes[driveNumber];								// ... set up device code
			CpuInstance = cpu;													// ... and save the cpu
		}

		public override byte DeviceCode
		{
			get { return _deviceCode; }
		}

		public override void ExecuteIocc()
		{
			switch (CpuInstance.IoccFunction)
			{
				case DevFunction.SenseDevice:
					if ((CpuInstance.IoccModifiers & 1) == 1)
					{
						DeactivateInterrupt(CpuInstance);
					}
					CpuInstance.Acc = (ushort) (_cartridge == null ? NotReady : 0);
//					if (_readInProgess)
//					{
////						CpuInstance.Acc |= BusyStatus;
//					}
//					if (_complete)
//					{
//						CpuInstance.Acc |= OperationCompleteStatus;
//					}
//					if (_lastCard)
//					{
//						CpuInstance.Acc |= LastCardStatus;
//					}
					break;
				case DevFunction.InitRead:
					CpuInstance.LetInstuctionsExecute(10);
					break;
			}
		}

		public void Mount(ICartridge cartridge)
		{
			_cartridge = cartridge;
			_cartridge.Mount();
		}

		public void UnMount()
		{
			_cartridge.Flush();
			_cartridge = null;
		}
	}
}