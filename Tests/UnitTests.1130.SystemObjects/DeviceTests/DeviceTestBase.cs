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
			InsCpu = new Cpu { Iar = 0x100, IgnoreInstructionCount = true };
		}

		protected void SenseDevice(IDevice device, ushort resetBits = 0)
		{
			InsCpu.IoccDeviceCode = device.DeviceCode;
			InsCpu.IoccFunction = DevFunction.SenseDevice;
			InsCpu.IoccModifiers = resetBits;
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

		protected ICard GetTestCard()
		{
			var columns = new ushort[80];
			for (int i = 0; i < 80; i++)
			{
				columns[i] = (ushort) (i+1 << 4);
			}
			return new Card(columns);
		}
	}
}