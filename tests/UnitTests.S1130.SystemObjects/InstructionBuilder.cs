﻿using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
    public class InstructionBuilder
    {
		private static ushort BuildAShortInstruction(OpCodes opCode, uint tag, uint displacement)
		{
			return (ushort)((((uint)opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
		}

		public static void BuildShortAtIar(OpCodes opCode, uint tag, uint displacement, ICpu cpu)
		{
			cpu[cpu.Iar] = BuildAShortInstruction(opCode, tag, displacement);
			cpu.DecodeCurrentInstruction();
		}
		
        public static void BuildShortAtAddress(OpCodes opCode, uint tag, uint displacement, ICpu cpu, ushort address)
        {
			cpu[address] = (ushort)((((uint)opCode << Constants.InstructionShift) | (tag << Constants.TagShift) | (displacement & Constants.DisplacementMask)) & ushort.MaxValue);
			cpu.DecodeCurrentInstruction();
        }

        public static void BuildLongIndirectAtAddress(OpCodes opCode, uint tag, ushort displacement, ICpu cpu, ushort address)
        {
            cpu[address] = BuildAShortInstruction(opCode, tag, 0);
            cpu[address] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
            cpu[address+1] = displacement;
			cpu.DecodeCurrentInstruction();
        }

        public static void BuildLongAtAddress(OpCodes opCode, uint tag, ushort displacement, ICpu cpu, ushort address)
        {
            cpu[address] = BuildAShortInstruction(opCode, tag, 0);
            cpu[address] |= Constants.FormatLong; // Format Long 
            cpu[address+1] = displacement;
			cpu.DecodeCurrentInstruction();
        }

	    public static ushort BuildShortBranch(OpCodes opCode, uint tag, byte modifiers)
	    {
			return (ushort)((((ushort)opCode << Constants.InstructionShift) | (ushort) (tag << Constants.TagShift) | modifiers & Constants.DisplacementMask) & ushort.MaxValue);
		}

		public static void BuildLongBranchAtAddress(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ICpu cpu, ushort address)
		{
			cpu[address] = BuildAShortInstruction(opCode, tag, 0);
			cpu[address] |= Constants.FormatLong; // Format Long
			cpu[address] |= modifiers; // put in modifiers
			cpu[address + 1] = displacement;
			cpu.DecodeCurrentInstruction();
		}

		public static void BuildLongIndirectBranchAtAddress(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ICpu cpu, ushort address)
		{
			cpu[address] = BuildAShortInstruction(opCode, tag, 0);
			cpu[address] |= Constants.FormatLong | Constants.Indirect; // Format Long & Indirect Addressing
			cpu[address] |= modifiers; // put in modifiers
			cpu[address + 1] = displacement;
			cpu.DecodeCurrentInstruction();
		}

	    public static void BuildIoccAt(IDevice device, DevFunction func, byte modifier, ushort memAddr, ICpu cpu, ushort ioccAddr)
	    {
		    cpu[ioccAddr++] = memAddr;
			cpu[ioccAddr] = (ushort) ((((device.DeviceCode & 0x1f) << 11) | (((int)func & 0x7) << 8) | modifier) & 0xffff);
	    }

		public static void BuildLongIndirectAtIar(OpCodes opCode, uint tag, ushort displacement, ICpu cpu)
		{
			BuildLongIndirectAtAddress(opCode, tag, displacement, cpu, cpu.Iar);
			cpu.DecodeCurrentInstruction();
		}

		public static void BuildLongAtIar(OpCodes opCode, uint tag, ushort displacement, ICpu cpu)
		{
			BuildLongAtAddress(opCode, tag, displacement, cpu, cpu.Iar);
			cpu.DecodeCurrentInstruction();
		}

		public static void BuildLongBranchAtIar(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ICpu cpu)
		{
			BuildLongBranchAtAddress(opCode, tag, modifiers, displacement, cpu, cpu.Iar);
			cpu.DecodeCurrentInstruction();
		}

	    public static void BuildLongIndirectBranchAtIar(OpCodes opCode, uint tag, ushort modifiers, ushort displacement, ICpu cpu)
	    {
		    BuildLongIndirectBranchAtAddress(opCode, tag, modifiers, displacement, cpu, cpu.Iar);
			cpu.DecodeCurrentInstruction();
	    }
	}
}