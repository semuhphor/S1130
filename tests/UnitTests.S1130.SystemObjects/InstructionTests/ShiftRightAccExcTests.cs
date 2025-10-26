using Xunit;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	
	public class ShiftRightAccExcTests : InstructionTestBase
	{
		[Fact]
		public void Execute_SRT()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 0, 0x83, InsCpu);
			
			ExecAndTest(initialAccExt: 0x10000200, expectedAccExt: 0x02000040);
		}

		[Fact]
		public void Execute_SRT_XR1()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 1, 0x00, InsCpu);
			
			InsCpu.Xr[1] = 0x85;
			ExecAndTest(initialAccExt: 0x10210000, expectedAccExt: 0x00810800);
		}

		[Fact]
		public void Execute_SRT_XR1_15bits()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x00, InsCpu);
			
			InsCpu.Xr[2] = 0x9f;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000001);
		}

		[Fact]
		public void Execute_SRT_XR1_ClearsAcc()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x00, InsCpu);
			
			InsCpu.Xr[2] = 0xbf;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0x00000000);
		}

		[Fact]
		public void Execute_SRT_XR3_0BitsNOP()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x00, InsCpu);
			
			InsCpu.Xr[2] = 0x80;
			ExecAndTest(initialAccExt: 0x43121234, expectedAccExt: 0x43121234);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x00, InsCpu);
		}

		protected override string OpName
		{
			get { return "SR"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.ShiftRight; }
		}

		[Fact]
		public override void NameAndOpcodeTest()
		{
			BeforeEachTest();
			CheckNameAndOpcode();
		}
	}
}