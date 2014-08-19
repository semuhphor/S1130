using S1130.SystemObjects.Instructions;

namespace S1130.SystemObjects
{
    public interface IInstruction
    {
        OpCodes OpCode { get; }
        string OpName { get; }
		bool HasLongFormat { get; }
        void Execute(ISystemState state);
    }
}