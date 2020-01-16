using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Utility;

namespace UnitTests.S1130.SystemObjects.UtilityTests
{
    public class ConversionCodesTests
    {
        ConversionCodes _conv = new ConversionCodes();

        [Fact]
        public void ToCrColumn_YieldCorrectValue()
        {
            Assert.Equal(0x8000, _conv.ToCrColumn(12));
            Assert.Equal(0x4000, _conv.ToCrColumn(11));
            Assert.Equal(0x2000, _conv.ToCrColumn(0));
            Assert.Equal(0x1000, _conv.ToCrColumn(1));
            Assert.Equal(0x0800, _conv.ToCrColumn(2));
            Assert.Equal(0x0400, _conv.ToCrColumn(3));
            Assert.Equal(0x0200, _conv.ToCrColumn(4));
            Assert.Equal(0x0100, _conv.ToCrColumn(5));
            Assert.Equal(0x0080, _conv.ToCrColumn(6));
            Assert.Equal(0x0040, _conv.ToCrColumn(7));
            Assert.Equal(0x0020, _conv.ToCrColumn(8));
            Assert.Equal(0x0010, _conv.ToCrColumn(9));
            Assert.Equal(0x9000, _conv.ToCrColumn(12,1));
            Assert.Equal(0x4210, _conv.ToCrColumn(11,4,9));
            Assert.Equal(0x0000, _conv.ToCrColumn());
            Assert.Equal(0x0000, _conv.ToCrColumn(10));
        }

        [Fact]
        public void ToCardCode_ConvertsSingleCharacter()
        {
           Assert.Equal(_conv.ToCrColumn(12, 1), _conv.ToCrColumn('A'));
           Assert.Equal(_conv.ToCrColumn(0, 9), _conv.ToCrColumn('Z'));
           Assert.Equal(_conv.ToCrColumn(8, 6), _conv.ToCrColumn('='));
           Assert.Equal(_conv.ToCrColumn(0, 8, 4), _conv.ToCrColumn('%'));
           Assert.Equal(_conv.ToCrColumn(), _conv.ToCrColumn(' '));
        }

        [Fact]
        public void ToCardCode_ConvertsSingleCharacter_NotFound_YieldsBlank()
        {
           Assert.Equal(_conv.ToCrColumn(), _conv.ToCrColumn('~'));
           Assert.Equal(_conv.ToCrColumn(), _conv.ToCrColumn('\\'));
           Assert.Equal(_conv.ToCrColumn(), _conv.ToCrColumn('`'));
        }

		[Fact]
		public void ToCard_CreatesCard()
		{
			Card card = _conv.ToCard("A1");
			Assert.Equal(0x9000, card[0]);
			Assert.Equal(0x1000, card[1]);
            for (int i = 79; i > 1; i--)
            {
                Assert.Equal(0, card[i]);
            }
		}
    }
}
