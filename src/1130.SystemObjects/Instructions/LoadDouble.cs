namespace S1130.SystemObjects.Instructions
{
    public class LoadDouble : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get { return OpCodes.LoadDouble; } }
        public string OpName { get { return "LDD"; } }

        public void Execute(ICpu cpu)
        {
            var effectiveAddress = GetEffectiveAddress(cpu);
            cpu.Acc = cpu[effectiveAddress];
            cpu.Ext = cpu[effectiveAddress | 1];
        }
    }
}