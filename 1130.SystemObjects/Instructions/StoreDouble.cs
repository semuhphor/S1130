namespace S1130.SystemObjects.Instructions
{
    public class StoreDouble : InstructionBase, IInstruction
    {
        public OpCodes OpCode { get { return OpCodes.StoreDouble;  } }
        public string OpName { get { return "STD"; } }
        public void Execute(ICpu cpu)
        {
            var effectiveAddress = GetEffectiveAddress(cpu);
            cpu[effectiveAddress | 1] = cpu.Ext;
            cpu[effectiveAddress] = cpu.Acc;
        }
    }
}