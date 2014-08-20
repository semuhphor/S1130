using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class LoadStatusTests : InstructionTestBase
	{

		[TestMethod]
		public void Execute_LDS_Short_NeitherCarryNorOverflow()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x00);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_LDS_Short_CarryNotOverflow()
		{
			InsCpu.Carry = false;
			InsCpu.Overflow = true;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x02);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.IsTrue(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_LDS_Short_OverflowNotCarry()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x01);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsTrue(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_LDS_Short_BothCarryAndOverflow()
		{
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.LoadStatus, 0, 0x03);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.IsTrue(InsCpu.Carry);
			Assert.IsTrue(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_LDS_Long_StillIsShort()
		{
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			var iar = InsCpu.Iar;
			InstructionBuilder.BuildLongAtIar(OpCodes.LoadStatus, 0, 0x03, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(iar+1, InsCpu.Iar);
		}
	}
}