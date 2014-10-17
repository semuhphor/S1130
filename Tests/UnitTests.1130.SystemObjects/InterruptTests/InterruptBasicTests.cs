using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	[TestClass]
	public class InterruptBasicTests : InterruptTestBase
	{
		[TestMethod]
		public void NoInterruptActiveTest()
		{
			Assert.IsNull(InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptZero()
		{
			InsCpu.AddInterrupt(GetInterrupt(0));
			Assert.AreEqual(0, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptOne()
		{
			InsCpu.AddInterrupt(GetInterrupt(1));
			Assert.AreEqual(1, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptTwo()
		{
			InsCpu.AddInterrupt(GetInterrupt(2));
			Assert.AreEqual(2, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptThree()
		{
			InsCpu.AddInterrupt(GetInterrupt(3));
			Assert.AreEqual(3, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptFour()
		{
			InsCpu.AddInterrupt(GetInterrupt(4));
			Assert.AreEqual(4, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void AddsToInterruptFive()
		{
			InsCpu.AddInterrupt(GetInterrupt(5));
			Assert.AreEqual(5, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void InterruptsPriorityFourBeforeFive()
		{
			InsCpu.AddInterrupt(GetInterrupt(4));
			InsCpu.AddInterrupt(GetInterrupt(5));
			Assert.AreEqual(4, InsCpu.CurrentInterruptLevel);
		}

		[TestMethod]
		public void InterruptsPriorityZeroBeforeAnything()
		{
			InsCpu.AddInterrupt(GetInterrupt(1));
			InsCpu.AddInterrupt(GetInterrupt(2));
			InsCpu.AddInterrupt(GetInterrupt(3));
			InsCpu.AddInterrupt(GetInterrupt(4));
			InsCpu.AddInterrupt(GetInterrupt(5));
			InsCpu.AddInterrupt(GetInterrupt(0));
			Assert.AreEqual(0, InsCpu.CurrentInterruptLevel);
		}
	}
}