using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class AddTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_A_Short_NoTag_Positive_NoCarryOrOverflow()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu[InsCpu.Iar + 0x10] = 0x0012;
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x0013, InsCpu.Acc);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_Positive()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Acc = 1;
			InsCpu[InsCpu.Iar + 0x10] = 0xffff;
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x0000, InsCpu.Acc);
			Assert.IsTrue(InsCpu.Carry);
			Assert.IsTrue(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_A_Short_NoTag_ShouldOverflowNoCarry()
		{
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.Add, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.Acc = 0x4000;
			InsCpu[InsCpu.Iar + 0x10] = 0x4000;
			InsCpu.Carry = false;
			InsCpu.Overflow = false;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x8000, InsCpu.Acc);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsTrue(InsCpu.Overflow);
		}
	}
}