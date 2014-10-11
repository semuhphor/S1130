using System.Reflection.Emit;

namespace S1130.SystemObjects
{
    public interface IInstructionSet
    {
		IInstruction this[int opcode] { get; }
    }
}