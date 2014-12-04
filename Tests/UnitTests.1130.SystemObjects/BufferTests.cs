using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using Buffer = S1130.SystemObjects.Buffer;

namespace UnitTests.S1130.SystemObjects
{
	[TestClass]
	public class BufferTests
	{
		private Cpu _cpu;
		private Buffer _buffer;
		private const ushort IarDefault = 0x100;

		[TestInitialize]
		public void BeforeEachTest()
		{
			_cpu = new Cpu { Iar = 0x100 };
			_cpu[0x1000] = 80;
			_buffer = new Buffer(0x1000, _cpu);
		}

		[TestMethod]
		public void BufferSetupTest()
		{
			Assert.AreEqual(0x1000, _buffer.WCA);
			Assert.AreEqual(80, _buffer.WordCount);
		}

		[TestMethod]
		public void LowAddressIsAfterWordCount()
		{
			_buffer[0] = 0x1234;
			Assert.AreEqual(0x1234, _cpu[0x1001]);
		}

		[TestMethod]
		public void HighAddressAtEndOfBuffer()
		{
			_buffer[79] = 0x1235;
			Assert.AreEqual(0x1235, _cpu[0x1000 + 80]);
		}

		[TestMethod]
		public void NegativeAddressDiallowed_Write()
		{
			try
			{
				_buffer[-1] = 0x100;
				Assert.Fail("Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.AreEqual("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.Fail("Wrong exception");
			}
		}

		[TestMethod]
		public void NegativeAddressDiallowed_Read()
		{
			try
			{
				var a = _buffer[-1];
				Assert.Fail("Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.AreEqual("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.Fail("Wrong exception");
			}
		}

		[TestMethod]
		public void BufferOverflowDiallowed_Write()
		{
			try
			{
				_buffer[80] = 0x100;
				Assert.Fail("Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.AreEqual("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.Fail("Wrong exception");
			}
		}

		[TestMethod]
		public void BufferOverflowDiallowed_Read()
		{
			try
			{
				var a = _buffer[80];
				Assert.Fail("Expected exception");
			}
			catch (IndexOutOfRangeException ex)
			{
				Assert.AreEqual("Offset outside of buffer", ex.Message);
			}
			catch (Exception e)
			{
				Assert.Fail("Wrong exception");
			}
		}
	}
}