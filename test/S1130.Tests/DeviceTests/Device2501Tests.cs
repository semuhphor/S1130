using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;

namespace Tests
{
	public class Device2501Tests : DeviceTestBase
	{
		private Device2501 _2501;
		protected Deck ADeck;

		protected override void BeforeEachTest()
		{
			base.BeforeEachTest();
			_2501 = new Device2501(InsCpu);
			ADeck = new Deck() + new [] { new Card(), new Card()};
		}

		[Fact]
		public void ShouldReturnDeviceType0X09()
		{
			BeforeEachTest();
			Assert.Equal(0x09, _2501.DeviceCode);
		}

		[Fact]
		public void ShouldReturnNotReadyWithoutCards()
		{
			BeforeEachTest();
			SenseDevice(_2501);
			Assert.Equal(Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
		}

		[Fact]
		public void ShouldShouldBusyDuringRead()
		{
			BeforeEachTest();
			_2501 += ADeck;
			InitiateRead(_2501, 0x400, 80);	
			SenseDevice(_2501);
			Assert.Equal(Device2501.BusyStatus, InsCpu.Acc);
		}

		[Fact]
		public void ShouldReturnReadyWithCards()
		{
			BeforeEachTest();
			_2501 += ADeck;
			SenseDevice(_2501);
			Assert.Equal(0, InsCpu.Acc);
		}

		[Fact]
		public void CardsShouldGoIntoHopper()
		{
			BeforeEachTest();
			_2501 += ADeck; 
			Assert.Equal(2, _2501.Hopper.Count);
		}

		[Fact]
		public void ShouldReadCards()
		{
			BeforeEachTest();
			_2501 += GetTestCard();
			_2501 += GetTestCard();
			InitiateRead(_2501, 0x1000, 80);
			_2501.Run();
			Assert.NotNull(_2501.ActiveInterrupt);
			Assert.Equal(0x1000, _2501.ActiveInterrupt.Ilsw);
			SenseDevice(_2501);
			Assert.Equal(Device2501.OperationCompleteStatus, InsCpu.Acc);
			CheckCardReadProperly(0x1001, 80);
			SenseDevice(_2501, 1);
			Assert.Equal(0, InsCpu.Acc);
			Assert.True(_2501.ActiveInterrupt == null);
			InitiateRead(_2501, 0x1000, 80);
			_2501.Run();
			Assert.NotNull(_2501.ActiveInterrupt);
			Assert.Equal(0x1000, _2501.ActiveInterrupt.Ilsw);
			SenseDevice(_2501);
			Assert.Equal(Device2501.LastCardStatus | Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
			CheckCardReadProperly(0x1001, 80);
			SenseDevice(_2501, 1);
			Assert.Equal(Device2501.NotReadyOrBusyStatus, InsCpu.Acc);
			Assert.Null(_2501.ActiveInterrupt);
		}

		[Fact]
		public void ShouldOnlyReadNColumns()
		{
			BeforeEachTest();
			_2501 += GetTestCard();
			InsCpu[0x1006] = 0x1006;
			InitiateRead(_2501, 0x1000, 5);
			_2501.Run();
			CheckCardReadProperly(0x1001, 5);
			Assert.Equal((ushort) 0x1006, InsCpu[0x1006]);
		}

		protected void CheckCardReadProperly(int address, int wc)
		{
			var testCard = GetTestCard();
			for (var i = 0; i < wc; i++)
			{
				if (InsCpu[address + i] != testCard[i])
				{
					Assert.True(false, string.Format("Mismatch at {0}: memory: {1:x}, card: {2:x}", i, InsCpu[address + i], testCard[i]));
				}
			}
		}
	}
}
