using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	public abstract class DeviceTestBase
	{
		protected Cpu InsCpu;

		[TestInitialize]
		public virtual void BeforeEachTest()
		{
			InsCpu = new Cpu { Iar = 0x100 };
		}

		protected void SenseDevice(IDevice device, ushort resetBits = 0)
		{
			InsCpu.IoccDeviceCode = device.DeviceCode;
			InsCpu.IoccFunction = DevFunction.SenseDevice;
			device.ExecuteIocc();
		}

		protected void InitiateRead(IDevice device, int wca, int wc)
		{
			InsCpu.IoccDeviceCode = device.DeviceCode;
			InsCpu.IoccFunction = DevFunction.InitRead;
			InsCpu.IoccAddress = wca;
			InsCpu[wca] = (ushort) wc;
			while (wc != 0)
			{
				InsCpu[wca + wc--] = 0xffff;
			}
			device.ExecuteIocc();
		}
	}
}