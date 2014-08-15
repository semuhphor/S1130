using System;

namespace S1130.SystemObjects
{
    public class Cpu : ISystemState
    {
        private readonly ISystemState _state;

        public Cpu(ISystemState state)
        {
            _state = state;
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

        public IndexRegisters Xr
        {
            get { return _state.Xr;  }
        }

        #endregion

        public void NextInstruction()
        {
            _state.NextInstruction();
        }

        public ISystemState ExecuteInstruction()
        {
            var instruction = (Instructions) Opcode;
            int effectiveAddress;

            switch (instruction)
            {
                case Instructions.Load:
                    Acc = this[GetEffectiveAddress()];
                    break;
                case Instructions.LoadDouble:
                    effectiveAddress = GetEffectiveAddress();
                    Acc = this[effectiveAddress];
                    Ext = this[effectiveAddress|1];
                    break;
                case Instructions.Store:
                    effectiveAddress = GetEffectiveAddress();
                    this[effectiveAddress] = Acc;
                    break;
                case Instructions.StoreDouble:
                    effectiveAddress = GetEffectiveAddress();
                    this[effectiveAddress|1] = Ext;
                    this[effectiveAddress] = Acc;
                    break;
                default:
                    throw new Exception("Unknown instruction");
            }
            return _state;
        }

        public int GetEffectiveAddress()
        {
            var location = GetBase() + Displacement;
            if (FormatLong && IndirectAddress) // long indirect
            {
                location = this[location];
            }
            return location;
        }

        private int GetBase()
        {
            return (!FormatLong || Tag != 0) ? Xr[Tag] : 0;
        }
    }
}