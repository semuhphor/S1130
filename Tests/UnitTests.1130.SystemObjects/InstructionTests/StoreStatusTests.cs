using Microsoft.VisualStudio.TestTools.UnitTesting;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects.InstructionTests
{
	[TestClass]
	public class StoreStatusTests : InstructionTestBase
	{
		[TestMethod]
		public void Execute_STS_Short_NoTag()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreStatus, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x03, InsCpu[InsCpu.Iar + 0x10] & 0x03);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_STS_Short_NoTag_HighAddress()
		{
			InsCpu.Carry = false;
			InsCpu.Overflow = true;
			InsCpu.Iar = 0x7f00;
			InsCpu.AtIar = InstructionBuilder.BuildShort(OpCodes.StoreStatus, 0, 0x10);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x01, InsCpu[InsCpu.Iar + 0x10] & 0x03);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_STS_Long_NoTag()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreStatus, 0, 0x400, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x02, InsCpu[0x0400] & 0x03);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_STS_Long_Xr3()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = true;
			InstructionBuilder.BuildLongAtIar(OpCodes.StoreStatus, 3, 0x350, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[3] = 0x10;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x03, InsCpu[0x360] & 0x03);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		[TestMethod]
		public void Execute_STS_Long_Indirect_XR1()
		{
			InsCpu.Carry = true;
			InsCpu.Overflow = false;
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreStatus, 1, 0x400, InsCpu);
			InsCpu.NextInstruction();
			InsCpu.Xr[1] = 0x100;
			InsCpu[0x500] = 0x600;
			InsCpu.Acc = 0x1234;
			InsCpu.ExecuteInstruction();
			Assert.AreEqual(0x02, InsCpu[0x0600] & 0x03);
			Assert.IsFalse(InsCpu.Carry);
			Assert.IsFalse(InsCpu.Overflow);
		}

		protected override void BuildAnInstruction()
		{
			InstructionBuilder.BuildLongIndirectAtIar(OpCodes.StoreStatus, 1, 0x400, InsCpu);
		}

		protected override string OpName
		{
			get { return "STS"; }
		}

		protected override OpCodes OpCode
		{
			get { return OpCodes.StoreStatus; }
		}

		[TestMethod]
		public override void NameAndOpcodeTest()
		{
			CheckNameAndOpcode();
		}
	}
}