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
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 1, 0x80, InsCpu);
			
			InsCpu.Xr[1] = 0x05;
			ExecAndTest(initialAccExt: 0x10210000, expectedAccExt: 0x00810800);
		}

		[Fact]
		public void Execute_SRT_XR1_31bits_PropagatesSign()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x80, InsCpu);
			
			InsCpu.Xr[2] = 0x1F;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0xffffffff);
		}

		[Fact]
		public void Execute_SRT_XR1_32bits_FillsWithSign()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x80, InsCpu);
			
			InsCpu.Xr[2] = 0x3F;
			ExecAndTest(initialAccExt: 0x80000000, expectedAccExt: 0xffffffff);
		}

		[Fact]
		public void Execute_SRT_Negative_ArithmeticShift()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 0, 0x84, InsCpu);
			
			ExecAndTest(initialAccExt: 0xf0000010, expectedAccExt: 0xff000001);
		}

		[Fact]
		public void Execute_SRT_Positive_32bits_ClearsAll()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 0, 0xa0, InsCpu);
			
			ExecAndTest(initialAccExt: 0x7fffffff, expectedAccExt: 0x00000000);
		}

		[Fact]
		public void Execute_SRT_XR3_0BitsNOP()
		{
			BeforeEachTest();
			InstructionBuilder.BuildShortAtIar(OpCodes.ShiftRight, 2, 0x80, InsCpu);
			
			InsCpu.Xr[2] = 0x00;
			ExecAndTest(initialAccExt: 0x43121234, expectedAccExt: 0x43121234);
			Assert.Equal(0x101, InsCpu.Iar);
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