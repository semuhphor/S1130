using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Utility;
using Xunit;

namespace UnitTests.S1130.SystemObjects.DeviceTests
{
    public class Device2501IplTests : DeviceTestBase
    {
        private Device2501 _2501;

        protected override void BeforeEachTest()
        {
            base.BeforeEachTest();
            _2501 = new Device2501(InsCpu);
        }

        [Fact]
        public void ShouldLoadDmsIplCard()
        {
            BeforeEachTest();
            
            // Initiate IPL
            Assert.True(_2501.InitiateIpl());
            
            // Verify card is loaded
            Assert.False(_2501.Hopper.IsEmpty);

            // Read the card
            InitiateRead(_2501, 0x1000, 80);
            _2501.Run();

            // Verify first few words match DMS boot code
            Assert.Equal((ushort)0xc80a, InsCpu[0x1001]);  // First word
            Assert.Equal((ushort)0x18c2, InsCpu[0x1002]);  // Second word
            Assert.Equal((ushort)0xd008, InsCpu[0x1003]);  // Third word
        }

        [Fact]
        public void ShouldLoadAplIplCard()
        {
            BeforeEachTest();
            
            // Initiate APL IPL
            Assert.True(_2501.InitiateIpl(apl: true));
            
            // Verify card is loaded
            Assert.False(_2501.Hopper.IsEmpty);

            // Read the card
            InitiateRead(_2501, 0x1000, 80);
            _2501.Run();

            // Verify first few words match APL boot code
            Assert.Equal((ushort)0x7021, InsCpu[0x1001]);  // First word
            Assert.Equal((ushort)0x3000, InsCpu[0x1002]);  // Second word
            Assert.Equal((ushort)0x7038, InsCpu[0x1003]);  // Third word
        }

        [Fact]
        public void ShouldLoadAplPrivilegedIplCard()
        {
            BeforeEachTest();
            
            // Initiate privileged APL IPL
            Assert.True(_2501.InitiateIpl(apl: true, privileged: true));
            
            // Verify card is loaded
            Assert.False(_2501.Hopper.IsEmpty);

            // Read the card
            InitiateRead(_2501, 0x1000, 80);
            _2501.Run();

            // Verify characteristic differences from standard APL
            // The privileged boot indicator is at offset 54 (0x36) in the card data
            Assert.Equal((ushort)0x1002, InsCpu[0x1036]);  // Privileged mode indicator
            Assert.Equal((ushort)0x4004, InsCpu[0x104F]);  // Last word different
        }

        [Fact]
        public void ShouldClearExistingCardsBeforeIpl()
        {
            BeforeEachTest();
            
            // Add some dummy cards
            _2501 += new Card();
            _2501 += new Card();
            
            // Initiate IPL
            Assert.True(_2501.InitiateIpl());
            
            // Read card
            InitiateRead(_2501, 0x1000, 80);
            _2501.Run();

            // Verify we got IPL card content
            Assert.Equal((ushort)0xc80a, InsCpu[0x1001]);  // First word of DMS boot
            
            // Verify hopper is empty after read
            Assert.True(_2501.Hopper.IsEmpty);
        }

        [Fact]
        public void ShouldSetCompleteAndRaiseInterrupt()
        {
            BeforeEachTest();
            
            // Initiate IPL and read
            _2501.InitiateIpl();
            InitiateRead(_2501, 0x1000, 80);
            _2501.Run();

            // Verify interrupt is raised
            Assert.NotNull(_2501.ActiveInterrupt);
            Assert.Equal(Device2501.Ilsw, _2501.ActiveInterrupt.Ilsw);

            // Verify status shows complete
            SenseDevice(_2501);
            Assert.Equal(Device2501.OperationCompleteStatus | Device2501.LastCardStatus, InsCpu.Acc);
        }
    }
}