using System.Reflection.Emit;

namespace S1130.SystemObjects
{
    public interface IInstructionSet
    {
	    bool MayBeLong(int opcode);
		IInstruction this[int opcode] { get; }
    }
}