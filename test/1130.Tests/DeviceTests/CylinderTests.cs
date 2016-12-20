using Xunit;
using S1130.SystemObjects.Devices;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
    /* ***********************************************************************
	 * Tests for cylinder calculation
	 * 
	 * Verfies that you can't move the heads outside of cylinders 0-202.
	 * **********************************************************************/

    public class CylinderTests
	{
		private Cylinder _cyl;								// work for a cylider object

		[Fact]
		public void Constructor()							// test that the constructor
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			Assert.Equal(0, _cyl.Current);					// .. starts at cylinder 0
		}

		[Fact]
		public void AddOperatorIncrements()					// test incrementing cylinder
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 6;											// move to cylider 6
			Assert.Equal(6, _cyl.Current);					// .. ensure we get there
		}

		[Fact]
		public void SubtractOperatorDecreaments()			// test decrementing cylinder
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 7;											// start at cylinder 7
			_cyl -= 3;											// .. move back 3
			Assert.Equal(4, _cyl.Current);					// .. ensure we are at cylinder 4
		}

		[Fact]
		public void SubtractFloorsAtZero()					// test we can't go below zero
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 7;											// start at cylinder 7
			_cyl -= 10;											// .. move back 10 (remember the grind?)
			Assert.Equal(0, _cyl.Current);					// .. ensure we stopped at 0.
		}

		[Fact]
		public void AddCeilingAt202()						// test we can't go above 202
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 200;										// start at cylinder 200
			_cyl += 25;											// .. try to move in 25 more (grind...)
			Assert.Equal(202, _cyl.Current);					// .. assert we stopped at 202
		}

		[Fact]
		public void AddMaxInt()								// test that we handle max values 
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 200;										// start at cylinder 200 
			_cyl += int.MaxValue;								// .. try to move to the end of the universe (supergrind)
			Assert.Equal(202, _cyl.Current);					// .. assert we stopped at cyl 202
		}

		[Fact]
		public void AddMinInt()								// test that we handle min (negative) value
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += int.MinValue;								// try to move back from cyl 0
			Assert.Equal(0, _cyl.Current);					// .. assert we go nowhere (stay at zero)
		}

		[Fact]
		public void HomeAtCylZero()							// test the we are home when at cyl 0
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			Assert.True(_cyl.Home);							// starts at home
		}

		[Fact]
		public void NotHomeOffCylZero()						// test the we are not home when not at cyl 0
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 10;											// move in 10 cylinders
			Assert.False(_cyl.Home);							// .. no longer home
		}

		[Fact]
		public void ReturnToCylZeroIsHome()					// test the we get home when we return to cyl 0
		{
			_cyl = new Cylinder();								// .. create a cylider (at cyl zero)
			_cyl += 10;											// move in 10 cylinders
			_cyl -= 10;											// .. and move back 10 cylinder
			Assert.True(_cyl.Home);							// .. we are back home
		}

	}
}
