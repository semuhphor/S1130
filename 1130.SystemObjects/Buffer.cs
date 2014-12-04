using System;

namespace S1130.SystemObjects
{
	/*********************************************************************************
	 * Buffer - provide a buffer in memory that is limited to the addresses needed
	 * for cycle stealing devices
	 * 
	 * This class provides a buffer (window) in memory that only has access to those
	 * words needed by the device for IO.
	 * 
	 * For 1130 IO, the buffer starts with the word count followed by 
	 * that number of words. Buffer provides a word count and an array where
	 * the zeroeth element is the first word after the work count. 
	 * 
	 * Methods will be added, as needed, for transfer into and out of the buffer using
	 * different formats.
	 *********************************************************************************/

	public class Buffer
	{
		private readonly ICpu _cpu;
		private readonly int _bufferLowAddress;
		private readonly int _bufferHighAddress;

		public Buffer(int addressOfWordCount, ICpu cpu)
		{
			_cpu = cpu;
			WCA = addressOfWordCount;
			WordCount = cpu[addressOfWordCount];
			_bufferLowAddress = addressOfWordCount + 1;
			_bufferHighAddress = WCA + WordCount;
		}

		public ushort WordCount { get; private set; }
		public int WCA { get; private set; }

		public ushort this[int offset]
		{
			get { return _cpu[CheckOffset(offset)]; }
			set { _cpu[CheckOffset(offset)] = value; }
		}

		private int CheckOffset(int offset)
		{
			int actualAddress = _bufferLowAddress + offset;
			if (actualAddress < _bufferLowAddress || actualAddress > _bufferHighAddress)
			{
				throw new IndexOutOfRangeException("Offset outside of buffer");
			}
			return actualAddress;
		}
	}
}
