using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
   public class CpuTests
    {
        private Cpu _cpu;
        private const ushort IarDefault = 0x100;


        [Fact]
        public void AccProperty()
        {
            _cpu = new Cpu { Iar = 0x100 };
            _cpu.Acc = 0x1234;
            Assert.Equal(0x1234, _cpu.Acc);
        }

        [Fact]
        public void ExtProperty()
        {
            _cpu = new Cpu { Iar = 0x100 };
            _cpu.Ext = 0x1234;
			Assert.Equal(0x1234, _cpu.Ext);
        }

        [Fact]
        public void IarProperty()
        {
            _cpu = new Cpu { Iar = 0x100 };
            _cpu.Iar = 0x1234;
			Assert.Equal(0x1234, _cpu.Iar);
        }

        [Fact]
        public void AtIarProperty()
        {
            _cpu = new Cpu { Iar = 0x100 };
            _cpu.AtIar = 0x1234;
            Assert.Equal(0x1234, _cpu[IarDefault]);
            _cpu[IarDefault] = 0x3245;
            Assert.Equal(0x3245, _cpu.AtIar);
        }

		[Fact]
		public void IndexProperty()
		{
            _cpu = new Cpu { Iar = 0x100 };
			_cpu[0x100] = 0x1234;
			Assert.Equal(0x1234, _cpu.Memory[0x100]);
		}

		[Fact]
		public void CarryProperty()
		{
            _cpu = new Cpu { Iar = 0x100 };
			_cpu.Carry = true;
			Assert.True(_cpu.Carry);
			_cpu.Carry = false;
			Assert.False(_cpu.Carry);
		}

		[Fact]
		public void OverflowProperty()
		{
            _cpu = new Cpu { Iar = 0x100 };
			_cpu.Overflow = true;
			Assert.True(_cpu.Overflow);
			_cpu.Overflow = false;
			Assert.False(_cpu.Overflow);
		}

	    [Fact]
	    public void InvalidOpCode()
	    {
            _cpu = new Cpu { Iar = 0x100 };
		    _cpu.AtIar = 0x0000;
			_cpu.ExecuteInstruction();
			Assert.Equal(0x100, _cpu.Iar);
			Assert.True(_cpu.Wait);
	    }

		[Fact]
		public void NextInstruction_ShortLoadInstructionTest()
		{
            _cpu = new Cpu { Iar = 0x100 };
			InstructionBuilder.BuildShortAtIar(OpCodes.Load, 2, 0x72, _cpu);
			Assert.Equal(0x100, _cpu.Iar);
			Assert.Equal((int)OpCodes.Load, _cpu.Opcode);
			Assert.False(_cpu.FormatLong);
			Assert.Equal(2, _cpu.Tag);
			Assert.Equal(0x72, _cpu.Displacement);
			Assert.Equal("LD", _cpu.CurrentInstruction.OpName);
		}

		[Fact]
		public void NextInstruction_LongLoadInstructionIndirectWithXR3Test()
		{
            _cpu = new Cpu { Iar = 0x100 };
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.Load, 3, 0x72, _cpu);
			Assert.Equal(0x100, _cpu.Iar);
			Assert.Equal((int)OpCodes.Load, _cpu.Opcode);
			Assert.True(_cpu.FormatLong);
			Assert.True(_cpu.IndirectAddress);
			Assert.Equal(3, _cpu.Tag);
			Assert.Equal(0x72, _cpu.Displacement);
		}

		[Fact]
		public void IndexerTest()
		{
            _cpu = new Cpu { Iar = 0x100 };
            _cpu = new Cpu { Iar = 0x100 };
			_cpu[0x1000] = 0xbfbf;
			Assert.Equal(0xbfbf, _cpu.Memory[0x1000]);
			_cpu.Memory[0x2000] = 0xfbfb;
			Assert.Equal(0xfbfb, _cpu[0x2000]);
		}

	    [Fact]
	    public void DecodeIoccTest()
	    {
            _cpu = new Cpu { Iar = 0x100 };
            _cpu = new Cpu { Iar = 0x100 };
		    var device2501 = new Device2501(_cpu);
		    InstructionBuilder.BuildIoccAt(device2501, DevFunction.SenseDevice, 2,0x400, _cpu, 0x1000);
			_cpu.IoccDecode(0x1000);
			Assert.Equal(device2501.DeviceCode, _cpu.IoccDeviceCode);
		    Assert.Equal(DevFunction.SenseDevice, _cpu.IoccFunction);
		    Assert.Equal(0x400, _cpu.IoccAddress);
		    Assert.Equal(2, _cpu.IoccModifiers);
	    }

	    [Fact]
	    public void OddIoccDecodeFails()															// Failure occurs when IOCC not on even address.
	    {
            _cpu = new Cpu { Iar = 0x100 };
			var device2501 = new Device2501(_cpu);														// device is 2501
			InstructionBuilder.BuildIoccAt(device2501, DevFunction.SenseDevice, 2, 0x400, _cpu, 0x1001);// .. build sense device at 0x1001 (odd address)
			_cpu.IoccDecode(0x1001);																	// .. try to decode it
			Assert.NotEqual(device2501.DeviceCode, _cpu.IoccDeviceCode);										// Code is not 2501 code
			Assert.NotEqual(DevFunction.SenseDevice, _cpu.IoccFunction);								// .. and not sense device
			Assert.Equal(0, _cpu.IoccModifiers);														// .. and modifier wrong
			Assert.Equal(0x400, _cpu.IoccAddress);													// .. this is correct because of how odd/even works
		}
    }
}
