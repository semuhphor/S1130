using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
	/* ***********************************************************************
	 * Tests for cylinder calculation
	 * 
	 * Verfies that you can't move the heads outside of cylinders 0-202.
	 * **********************************************************************/

	[TestClass]
	public class CylinderTests
	{
		private Cylinder _cyl;								// work for a cylider object

		[TestInitialize]
		public void BeforeEachTest()						// before each test.. 
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
		}

		[TestMethod]
		public void Constructor()							// test that the constructor
		{
			Assert.AreEqual(0, _cyl.Current);					// .. starts at cylinder 0
		}

		[TestMethod]
		public void AddOperatorIncrements()					// test incrementing cylinder
		{
			_cyl += 6;											// move to cylider 6
			Assert.AreEqual(6, _cyl.Current);					// .. ensure we get there
		}

		[TestMethod]
		public void SubtractOperatorDecreaments()			// test decrementing cylinder
		{
			_cyl += 7;											// start at cylinder 7
			_cyl -= 3;											// .. move back 3
			Assert.AreEqual(4, _cyl.Current);					// .. ensure we are at cylinder 4
		}

		[TestMethod]
		public void SubtractFloorsAtZero()					// test we can't go below zero
		{
			_cyl += 7;											// start at cylinder 7
			_cyl -= 10;											// .. move back 10 (remember the grind?)
			Assert.AreEqual(0, _cyl.Current);					// .. ensure we stopped at 0.
		}

		[TestMethod]
		public void AddCeilingAt202()						// test we can't go above 202
		{
			_cyl += 200;										// start at cylinder 200
			_cyl += 25;											// .. try to move in 25 more (grind...)
			Assert.AreEqual(202, _cyl.Current);					// .. assert we stopped at 202
		}

		[TestMethod]
		public void AddMaxInt()								// test that we handle max values 
		{
			_cyl += 200;										// start at cylinder 200 
			_cyl += int.MaxValue;								// .. try to move to the end of the universe (supergrind)
			Assert.AreEqual(202, _cyl.Current);					// .. assert we stopped at cyl 202
		}

		[TestMethod]
		public void AddMinInt()								// test that we handle min (negative) value
		{
			_cyl += int.MinValue;								// try to move back from cyl 0
			Assert.AreEqual(0, _cyl.Current);					// .. assert we go nowhere (stay at zero)
		}

		[TestMethod]
		public void HomeAtCylZero()							// test the we are home when at cyl 0
		{
			Assert.IsTrue(_cyl.Home);							// starts at home
		}

		[TestMethod]
		public void NotHomeOffCylZero()						// test the we are not home when not at cyl 0
		{
			_cyl += 10;											// move in 10 cylinders
			Assert.IsFalse(_cyl.Home);							// .. no longer home
		}

		[TestMethod]
		public void ReturnToCylZeroIsHome()					// test the we get home when we return to cyl 0
		{
			_cyl += 10;											// move in 10 cylinders
			_cyl -= 10;											// .. and move back 10 cylinder
			Assert.IsTrue(_cyl.Home);							// .. we are back home
		}

	}
}
