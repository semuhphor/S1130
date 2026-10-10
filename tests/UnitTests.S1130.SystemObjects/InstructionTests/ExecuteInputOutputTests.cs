using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Devices;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ExecuteInputOutputTests : InstructionTestBase
	{
		[Fact]
		public void Execute_XIO_GetConsoleSwitches()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ExecuteInputOutput, 0, 0x11, InsCpu);
			InstructionBuilder.BuildIoccAt(new ConsoleEntrySwitches(InsCpu), DevFunction.Read, 0, 0x500, InsCpu, 0x112);
			InsCpu.ConsoleSwitches = 0x4321;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x4321, InsCpu[0x500]);
			Assert.Equal(0x101, InsCpu.Iar);
		}

		[Fact]
		public void Execute_XIO_SenseInterrupt_ReturnsIlswOfLevelBeingServiced()
		{
			BeforeEachTest();
			var dev = new SenseDummyDevice(InsCpu, 0x1f, 0x2000);
			var dev2 = new SenseDummyDevice(InsCpu, 0x1e, 0x1000);
			InsCpu.AddDevice(dev);
			InsCpu.AddDevice(dev2);
			InsCpu[Constants.InterruptVectors[4]] = 0x500;
			dev.Raise(4);
			dev2.Raise(4);
			InsCpu.HandleInterrupt();                                         // enter the level-4 handler
			Assert.Equal(0x501, InsCpu.Iar);

			// Sense Interrupt is common to all devices: device code 0 in the IOCC.
			// Short EA = (IAR+1) + disp = 0x502 + 0x10 = 0x512 (even, as an IOCC must be).
			InstructionBuilder.BuildShortAtIar(OpCodes.ExecuteInputOutput, 0, 0x10, InsCpu);
			InsCpu[0x512] = 0;
			InsCpu[0x513] = (ushort)((int)DevFunction.SenseInterrupte << 8);
			InsCpu.Acc = 0xdead;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0x3000, InsCpu.Acc);                                 // OR of both devices' ILSW bits
		}

		[Fact]
		public void Execute_XIO_SenseInterrupt_NoInterrupt_ReturnsZero()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ExecuteInputOutput, 0, 0x11, InsCpu);
			InsCpu[0x112] = 0;
			InsCpu[0x113] = (ushort)((int)DevFunction.SenseInterrupte << 8);
			InsCpu.Acc = 0xdead;
			InsCpu.ExecuteInstruction();
			Assert.Equal(0, InsCpu.Acc);
		}

		private class SenseDummyDevice : DeviceBase
		{
			private readonly byte _code;
			private readonly ushort _ilsw;
			public SenseDummyDevice(ICpu cpu, byte code, ushort ilsw) { CpuInstance = cpu; _code = code; _ilsw = ilsw; }
			public override byte DeviceCode => _code;
			public override void ExecuteIocc() { }
			public void Raise(int level) => ActivateInterrupt(CpuInstance, level, _ilsw);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.ExecuteInputOutput, 0, 0x11, InsCpu);
		}

		protected override string OpName
		{
			get { return "XIO"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ExecuteInputOutput; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}