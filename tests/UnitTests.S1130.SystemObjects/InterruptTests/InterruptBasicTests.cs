using System.Reflection.Metadata;
using Xunit;
using S1130.SystemObjects.InterruptManagement;

namespace UnitTests.S1130.SystemObjects.InterruptTests
{
	[Collection("Interrupt tests")]
	public class InterruptBasicTests : InterruptTestBase
	{
		[Fact]
		public void NoInterruptActiveTest()
		{
			var InsCpu = GetNewCpu();
			Assert.Null(InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptZero()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 0, 0x1f));
			Assert.Equal(0, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptOne()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 1, 0x1f));
			Assert.Equal(1, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptTwo()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 2, 0x1f));
			Assert.Equal(2, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptThree()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 3, 0x1f));
			Assert.Equal(3, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptFour()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 4, 0x1f));
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void AddsToInterruptFive()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 5, 0x1f));
			Assert.Equal(5, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void InterruptsPriorityFourBeforeFive()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 4, 0x1e));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 5, 0x1f));
			Assert.Equal(4, InsCpu.CurrentInterruptLevel);
		}

		[Fact]
		public void InterruptsPriorityZeroBeforeAnything()
		{
			var InsCpu = GetNewCpu();
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 1, 0x1a));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 2, 0x1b));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 3, 0x1c));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 4, 0x1d));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 5, 0x1e));
			InsCpu.AddInterrupt(GetInterrupt(InsCpu, 0, 0x1f));
			Assert.Equal(0, InsCpu.CurrentInterruptLevel);
		}
	}
}