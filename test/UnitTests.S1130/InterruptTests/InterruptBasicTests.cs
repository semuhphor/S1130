using Xunit;

namespace UnitTests.S1130.InterruptTests
{
	[Collection("Interrupt tests")]
	public class InterruptBasicTests : InterruptTestBase
	{
		[Fact]
		public void NoInterruptActiveTest()
		{
			BeforeEachTest();
			Assert.Null(InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptZero()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(0));
			Assert.Equal(0, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptOne()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(1));
			Assert.Equal(1, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptTwo()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(2));
			Assert.Equal(2, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptThree()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(3));
			Assert.Equal(3, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptFour()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(4));
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptFive()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(5));
			Assert.Equal(5, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void InterruptsPriorityFourBeforeFive()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(4));
			InsCpu.AddInterrupt(GetInterrupt(5));
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void InterruptsPriorityZeroBeforeAnything()
		{
			BeforeEachTest();
			InsCpu.AddInterrupt(GetInterrupt(1));
			InsCpu.AddInterrupt(GetInterrupt(2));
			InsCpu.AddInterrupt(GetInterrupt(3));
			InsCpu.AddInterrupt(GetInterrupt(4));
			InsCpu.AddInterrupt(GetInterrupt(5));
			InsCpu.AddInterrupt(GetInterrupt(0));
			Assert.Equal(0, InsCpu.CurrentInterruptLevel);
		}
	}
}