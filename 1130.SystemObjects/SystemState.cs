using System.Security.Permissions;

namespace S1130.SystemObjects
{
    public class SystemState : ISystemState
    {
        public const int DefaultMemorySize = 32768;
	    private readonly IInstructionSet _instructionSet;
        public SystemState()
        {
            MemorySize = DefaultMemorySize;
            Memory = new ushort[DefaultMemorySize];
            Xr = new IndexRegisters(this);
			_instructionSet = new InstructionSet();
        }

        public ushort[] Memory { get; set; } 
        public int MemorySize { get; set; }
        public ushort Iar { get; set; }
        public ushort Acc { get; set; }
        public ushort Ext { get; set; }

	    public uint AccExt
	    {
		    get { return (uint) ((Acc << 16) | Ext); }
		    set
		    {
			    Acc = (ushort) (value >> 16);
			    Ext = (ushort) (value & 0xffff);
		    }
	    }

	    public IndexRegisters IndexRegister { get; set; }
        public IndexRegisters Xr { get; private set; }
        public ushort Opcode { get; set; }
        public bool FormatLong { get; set; }
        public ushort Tag { get; set; }
        public ushort Displacement { get; set; }
        public bool IndirectAddress { get; set; }
        public ushort Modifiers { get; set; }
        public bool Carry { get; set; }
        public bool Overflow { get; set; }
		public bool Wait { get; set; }
        
       public void NextInstruction()
        {
            var firstWord = Memory[Iar++];
            Opcode = (ushort) ((firstWord & 0xF800) >> 11);
            FormatLong = (firstWord & 0x0400) != 0 && _instructionSet.MayBeLong(Opcode);
            Tag = (ushort) ((firstWord & 0x0300) >> 8);
			Modifiers = (ushort) (firstWord & 0xff);
            if (FormatLong)
            {
                Displacement = Memory[Iar++];
                IndirectAddress = (firstWord & 0x80) != 0;
            }
            else
            {
				Displacement = Modifiers;
                IndirectAddress = false;
            }
        }

	    public IInstruction GetInstruction()
	    {
		    return _instructionSet.GetInstruction(this);
	    }

	    public void ExecuteInstruction()
	    {
		    _instructionSet.Execute(this);
	    }

	    public ushort this[int address]
        {
            get { return Memory[address]; }
            set { Memory[address] = value; }
        }
    }
}