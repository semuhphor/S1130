using System;

namespace S1130.SystemObjects.Instructions
{
	public class LoadIndex : InstructionBase, IInstruction
	{
		public OpCodes OpCode { get { return OpCodes.LoadIndex; }  }
		public string OpName { get { return "LDX";  } }

		public void Execute(ICpu cpu)
		{
			cpu.Xr[cpu.Tag] = cpu.FormatLong ? (cpu.IndirectAddress ? cpu[cpu.Displacement] :  cpu.Displacement) : Convert.ToUInt16((sbyte) cpu.Displacement & 0xffff);
		}
	}
}