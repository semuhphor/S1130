namespace S1130.SystemObjects
{
    public interface IInstructionSet
    {
	    bool MayBeLong(int opcode);
        void Execute(ISystemState state);
	    IInstruction GetInstruction(ISystemState systemState);
    }
}