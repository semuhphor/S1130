using System;

namespace S1130.SystemObjects.Devices
{
	public class Device2501 : DeviceBase
	{
		public override byte DeviceCode
		{
			get { return 0x09; }
		}

		public override void ExecuteIocc(ICpu cpu)
		{
			throw new NotImplementedException();
		}
	}
}
