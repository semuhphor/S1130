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
	}
}