using System;

namespace S1130.SystemObjects
{
    public class Cpu : ISystemState
    {
        private readonly ISystemState _state;
        private readonly IInstructionSet _instructionSet;


        public Cpu(ISystemState state)
        {
            _state = state;
            _instructionSet = new InstructionSet();
        }

        #region Properties (reference to ISystemState)

        public ushort AtIar
        {
            get { return _state[Iar]; }
            set { _state[Iar] = value; }
        }

        public ushort Acc
        {
            get { return _state.Acc; }
            set { _state.Acc = value; }
        }

        public ushort Ext
        {
            get { return _state.Ext; }
            set { _state.Ext = value; }
        }

        public ushort Opcode
        {
            get { return _state.Opcode; }
        }

        public bool FormatLong
        {
            get { return _state.FormatLong; }
        }

        public ushort Tag
        {
            get { return _state.Tag; }
        }

        public ushort Displacement
        {
            get { return _state.Displacement; }
        }

        public bool IndirectAddress
        {
            get { return _state.IndirectAddress; }
        }

        public ushort Modifiers
        {
            get { return _state.Modifiers; }
        }

        public ushort[] Memory
        {
            get { return _state.Memory; }
        }

        public int MemorySize
        {
            get { return _state.MemorySize; }
        }

        public ushort Iar
        {
            get { return _state.Iar; }
            set { _state.Iar = value; }
        }

        public ushort this[int address]
        {
            get { return _state.Memory[address]; }
            set { _state.Memory[address] = value; }
        }

	    public bool Carry
	    {
			get { return _state.Carry; }
			set { _state.Carry = value; }
	    }

	    public bool Overflow
	    {
			get { return _state.Overflow; }
			set { _state.Overflow = value; }
		}

        public IndexRegisters Xr
        {
            get { return _state.Xr;  }
        }

        #endregion

        public void NextInstruction()
        {
            _state.NextInstruction();
        }

	    public void ExecuteInstruction()
        {
            _state.ExecuteInstruction();
        }
    }
}