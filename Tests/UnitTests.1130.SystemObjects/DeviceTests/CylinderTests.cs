using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	[TestClass]
	public class CylinderTests
	{
		private Cylinder _cyl;

		[TestInitialize]
		public void BeforeEachTest()
		{
			_cyl = new Cylinder();	
		}

		[TestMethod]
		public void Constructor()
		{
			Assert.AreEqual(0, _cyl.Current);
		}

		[TestMethod]
		public void AddOperatorIncrements()
		{
			_cyl += 6;
			Assert.AreEqual(6, _cyl.Current);
		}

		[TestMethod]
		public void SubtractOperatorDecreaments()
		{
			_cyl += 7;
			_cyl -= 3;
			Assert.AreEqual(4, _cyl.Current);
		}

		[TestMethod]
		public void SubtractFloorsAtZero()
		{
			_cyl += 7;
			_cyl -= 10;
			Assert.AreEqual(0, _cyl.Current);
		}

		[TestMethod]
		public void AddCeilingAt202()
		{
			_cyl += 200;
			_cyl += 25;
			Assert.AreEqual(202, _cyl.Current);
		}
	}
}
