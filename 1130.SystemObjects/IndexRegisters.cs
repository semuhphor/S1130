using System;
using System.Data;

namespace S1130.SystemObjects
{
    public class IndexRegisters
    {
        private readonly ISystemState _state;

        public IndexRegisters(ISystemState state)
        {
            _state = state;
        }

        public ushort this[int indexRegister]
        {
            get 
            {
                CheckIndex(indexRegister);
                return indexRegister > 0 ? _state[indexRegister] : _state.Iar ;
            }

            set
            {
                CheckIndex(indexRegister);
                if (indexRegister > 0)
                {
                    _state[indexRegister] = value;
                }
                else
                {
                    _state.Iar = value;
                }
            }
        }

        private void CheckIndex(int indexRegister)
        {
            if (indexRegister > 3)
            {
                throw new IndexOutOfRangeException(string.Format("Index register out of range {0}", indexRegister));
            };
        }
    }
}