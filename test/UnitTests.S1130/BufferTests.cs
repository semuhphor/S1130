using System;
using Xunit;
using S1130.SystemObjects;
using Buffer = S1130.SystemObjects.Buffer;

namespace UnitTests.S1130
{
	public class BufferTests
	{
		private Cpu _cpu;
		private Buffer _buffer;
		private const ushort IarDefault = 0x100;

		protected void BeforeEachTest()
		{
			_cpu = new Cpu { Iar = 0x100 };
			_cpu[0x1000] = 80;
			_buffer = new Buffer(0x1000, _cpu);
		}

		[Fact]
		public void BufferSetupTest()
		{
			BeforeEachTest();
			Assert.Equal(0x1000, _buffer.WCA);
			Assert.Equal(80, _buffer.WordCount);
		}

		[Fact]
		public void LowAddressIsAfterWordCount()
		{
			BeforeEachTest();
			_buffer[0] = 0x1234;
			Assert.Equal(0x1234, _cpu[0x1001]);
		}

		[Fact]
		public void HighAddressAtEndOfBuffer()
		{
			BeforeEachTest();
			_buffer[79] = 0x1235;
			Assert.Equal(0x1235, _cpu[0x1000 + 80]);
		}

		[Fact]
		public void NegativeAddressDiallowed_Write()
		{
			BeforeEachTest();
			try
			{
				_buffer[-1] = 0x100;
				Assert.True(false, "Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.Equal("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.True(false, string.Format("Wrong exception: {0}", e.Message));
			}
		}

		[Fact]
		public void NegativeAddressDiallowed_Read()
		{
			BeforeEachTest();
			try
			{
				var a = _buffer[-1];
				Assert.True(false, "Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.Equal("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.True(false, string.Format("Wrong exception: {0}", e.Message));
			}
		}

		[Fact]
		public void BufferOverflowDiallowed_Write()
		{
			BeforeEachTest();
			try
			{
				_buffer[80] = 0x100;
				Assert.True(false, "Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.Equal("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.True(false, string.Format("Wrong exception: {0}", e.Message));
			}
		}

		[Fact]
		public void BufferOverflowDiallowed_Read()
		{
			BeforeEachTest();
			try
			{
				var a = _buffer[80];
				Assert.True(false, "Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.Equal("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.True(false, string.Format("Wrong exception: {0}", e.Message));
			}
		}
	}
}