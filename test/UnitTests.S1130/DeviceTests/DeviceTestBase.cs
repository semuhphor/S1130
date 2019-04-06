using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	public abstract class DeviceTestBase
	{
		protected Cpu InsCpu;

		protected virtual void BeforeEachTest()
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

		protected void IssueControl(IDevice device, int wca, byte modifier)
		{
			InsCpu.IoccDevice = device;
			InsCpu.IoccFunction = DevFunction.Control;
			InsCpu.IoccAddress = wca;
			InsCpu.IoccModifiers = modifier;
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

		protected void InitiateRead(IDevice device, int wca, int wc, bool check, int sector)
		{
			InsCpu.IoccDeviceCode = device.DeviceCode;
			InsCpu.IoccFunction = DevFunction.InitRead;
			InsCpu.IoccAddress = wca;
			InsCpu.IoccModifiers = (check ? 0x80 : 0) | sector;  
			InsCpu[wca] = (ushort) wc;
			while (wc != 0)
			{
				InsCpu[wca + wc--] = 0xffff;
			}
			device.ExecuteIocc();
		}

		protected void InitiateDiskWrite(IDevice device, int wca, int wc, int sector)
		{
			InsCpu.IoccDeviceCode = device.DeviceCode;
			InsCpu.IoccFunction = DevFunction.InitWrite;
			InsCpu.IoccAddress = wca;
			InsCpu.IoccModifiers = sector & 0x07;  
			InsCpu[wca] = (ushort) wc;
			while (wc > 1)
			{
				InsCpu[wca + wc--] = (ushort) wc;
			}
			InsCpu[wca+1] = (ushort) sector;
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