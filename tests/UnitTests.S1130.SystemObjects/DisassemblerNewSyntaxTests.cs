using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
    public class DisassemblerNewSyntaxTests
    {
        [Fact]
        public void Disassemble_LoadShort_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x18 << 11) | 0x10);
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("LD /0011", result);
        }

        [Fact]
        public void Disassemble_LoadLong_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x18 << 11) | 0x400);
            cpu[1] = 0x0500;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("LD |L|/0500", result);
        }

        [Fact]
        public void Disassemble_LoadLongIndex1_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x18 << 11) | 0x400 | (1 << 8));
            cpu[1] = 0x0500;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("LD |L1|/0500", result);
        }

        [Fact]
        public void Disassemble_LoadIndirect_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x18 << 11) | 0x400 | 0x80);
            cpu[1] = 0x0500;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("LD |I|/0500", result);
        }

        [Fact]
        public void Disassemble_ShiftLeftArithmetic_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x02 << 11) | 0x08);
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("SLA 8", result);
        }

        [Fact]
        public void Disassemble_ShiftLeftAndCount_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x02 << 11) | 0x48);
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("SLCA 8", result);
        }

        [Fact]
        public void Disassemble_BSC_ShortFormat_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x09 << 11) | 0x20);
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("BSC Z", result);
        }

        [Fact]
        public void Disassemble_BSC_LongFormat_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x09 << 11) | 0x400 | 0x18);
            cpu[1] = 0x0100;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("BSC |L|/0100,+-", result);
        }

        [Fact]
        public void Disassemble_MDX_LongWithTag_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x0A << 11) | 0x400 | (2 << 8));
            cpu[1] = 0x0100;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("MDX |L2|/0100", result);
        }

        [Fact]
        public void Disassemble_MDX_LongWithModifier_UsesNewSyntax()
        {
            var cpu = new Cpu();
            cpu[0] = (ushort)((0x0A << 11) | 0x400 | 5);
            cpu[1] = 0x0100;
            cpu.Iar = 0;
            cpu.NextInstruction();
            
            var result = cpu.Disassemble(0);
            
            Assert.Equal("MDX |L|/0100,5", result);
        }
    }
}
