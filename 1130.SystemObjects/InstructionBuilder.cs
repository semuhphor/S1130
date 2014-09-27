using System.Data;
using System.Net.Sockets;
using OpCodes = S1130.SystemObjects.Instructions.OpCodes;

namespace S1130.SystemObjects
{
    public class InstructionBuilder
    {
        public static ushort BuildShort(OpCodes opCode, uint tag, uint displacement)
        {
            return (ushort) ((((uint) opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
        }

        public static void BuildShortAtAddress(OpCodes opCode, uint tag, uint displacement, ISystemState state, ushort address)
        {
            state[address] = (ushort) ((((uint) opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
        }

        public static void BuildLongIndirectAtAddress(OpCodes opCode, uint tag, ushort displacement, ISystemState state, ushort address)
        {
            state[address] = BuildShort(opCode, tag, 0);
            state[address] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
            state[address+1] = displacement;
        }

        public static void BuildLongAtAddress(OpCodes opCode, uint tag, ushort displacement, ISystemState state, ushort address)
        {
            state[address] = BuildShort(opCode, tag, 0);
            state[address] |= Constants.FormatLong; // Format Long 
            state[address+1] = displacement;
        }

	    public static ushort BuildShortBranch(OpCodes opCode, uint tag, byte modifiers)
	    {
			return (ushort)((((ushort)opCode << Constants.InstructionShift) | (ushort) (tag << Constants.TagShift) | modifiers & Constants.DisplacementMask) & ushort.MaxValue);
		}

		public static void BuildLongBranchAtAddress(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ISystemState state, ushort address)
		{
			state[address] = BuildShort(opCode, tag, 0);
			state[address] |= Constants.FormatLong; // Format Long
			state[address] |= modifiers; // put in modifiers
			state[address + 1] = displacement;
		}

		public static void BuildLongIndirectBranchAtAddress(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ISystemState state, ushort address)
		{
			state[address] = BuildShort(opCode, tag, 0);
			state[address] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
			state[address] |= modifiers; // put in modifiers
			state[address + 1] = displacement;
		}

	    public static void BuildIoccAt(IDevice device, DevFuction func, byte modifier, ushort memAddr, ISystemState state, ushort ioccAddr)
	    {
		    state[ioccAddr++] = memAddr;
			state[ioccAddr] = (ushort) ((((device.DeviceCode & 0x1f) << 11) | (((int)func & 0x7) << 8) | modifier) & 0xffff);
	    }

		public static void BuildLongIndirectAtIar(OpCodes opCode, uint tag, ushort displacement, ISystemState state)
		{
			BuildLongIndirectAtAddress(opCode, tag, displacement, state, state.Iar);
		}

		public static void BuildLongAtIar(OpCodes opCode, uint tag, ushort displacement, ISystemState state)
		{
			BuildLongAtAddress(opCode, tag, displacement, state, state.Iar);
		}

		public static void BuildLongBranchAtIar(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ISystemState state)
		{
			BuildLongBranchAtAddress(opCode, tag, modifiers, displacement, state, state.Iar);
		}

	    public static void BuildLongIndirectBranchAtIar(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ISystemState state)
	    {
		    BuildLongIndirectBranchAtAddress(opCode, tag, modifiers, displacement, state, state.Iar);
	    }
	}
}