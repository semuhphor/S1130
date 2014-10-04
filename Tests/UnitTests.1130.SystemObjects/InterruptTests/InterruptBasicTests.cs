using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	[TestClass]
	public class InterruptBasicTests : InterruptTestBase
	{
		[TestMethod]
		public void NoInterruptActiveTest()
		{
			Assert.IsNull(InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptZero()
		{
			InsCpu.AddInterrupt(new DummyDevice(0));
			Assert.AreEqual(0, InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptOne()
		{
			InsCpu.AddInterrupt(new DummyDevice(1));
			Assert.AreEqual(1, InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptTwo()
		{
			InsCpu.AddInterrupt(new DummyDevice(2));
			Assert.AreEqual(2, InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptThree()
		{
			InsCpu.AddInterrupt(new DummyDevice(3));
			Assert.AreEqual(3, InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptFour()
		{
			InsCpu.AddInterrupt(new DummyDevice(4));
			Assert.AreEqual(4, InsCpu.Interrupt);
		}

		[TestMethod]
		public void AddsDeviceToInterruptFive()
		{
			InsCpu.AddInterrupt(new DummyDevice(5));
			Assert.AreEqual(5, InsCpu.Interrupt);
		}

		[TestMethod]
		public void InterruptOutsideRangeIgnored()
		{
			InsCpu.AddInterrupt(new DummyDevice(-1));
			Assert.IsNull(InsCpu.Interrupt);
			InsCpu.AddInterrupt(new DummyDevice(6));
			Assert.IsNull(InsCpu.Interrupt);
		}

		[TestMethod]
		public void InterruptsPriorityFourBeforeFive()
		{
			InsCpu.AddInterrupt(new DummyDevice(4));
			InsCpu.AddInterrupt(new DummyDevice(5));
			Assert.AreEqual(4, InsCpu.Interrupt);
		}

		[TestMethod]
		public void InterruptsPriorityZeroBeforeAnything()
		{
			InsCpu.AddInterrupt(new DummyDevice(1));
			InsCpu.AddInterrupt(new DummyDevice(2));
			InsCpu.AddInterrupt(new DummyDevice(3));
			InsCpu.AddInterrupt(new DummyDevice(4));
			InsCpu.AddInterrupt(new DummyDevice(5));
			InsCpu.AddInterrupt(new DummyDevice(0));
			Assert.AreEqual(0, InsCpu.Interrupt);
		}
	}
}