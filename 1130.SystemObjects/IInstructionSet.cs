namespace S1130.SystemObjects
{
    public interface IInstructionSet
    {
	    bool MayBeLong(int opcode);
        void Execute(ICpu cpu);
	    IInstruction GetInstruction(ICpu cpu);
    }
}