using System;

namespace S1130.SystemObjects
{
    public class IndexRegisters
    {
        private readonly ICpu _cpu;

        public IndexRegisters(ICpu cpu)
        {
            _cpu = cpu;
        }

        public ushort this[int indexRegister]
        {
            get 
            {
                CheckIndex(indexRegister);
                return indexRegister > 0 ? _cpu[indexRegister] : _cpu.Iar ;
            }

            set
            {
                CheckIndex(indexRegister);
                if (indexRegister > 0)
                {
                    _cpu[indexRegister] = value;
                }
                else
                {
                    _cpu.Iar = value;
                }
            }
        }

        private void CheckIndex(int indexRegister)
        {
            if (indexRegister > 3)
            {
                throw new IndexOutOfRangeException(string.Format("Index register out of range {0}", indexRegister));
            }
        }
    }
}