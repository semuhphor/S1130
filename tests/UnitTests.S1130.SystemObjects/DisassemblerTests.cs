using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
    /// <summary>
    /// Tests for the disassembler functionality.
    /// Validates that instructions can be disassembled back into assembly source.
    /// </summary>
    public class DisassemblerTests
    {
        [Fact]
        public void Disassemble_Add_ShortFormat()
        {
            var cpu = new Cpu();
            
            // Build: A /50  (short format, relative displacement)
            // At address 0x100, target at 0x150 means displacement = 0x150 - 0x101 = 0x4F
            InstructionBuilder.BuildShortAtAddress(OpCodes.Add, 0, 0x4F, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            // Should produce something like "A     /0150"
            Assert.Contains("A", result);
            Assert.Contains("/0150", result);
        }
        
        [Fact]
        public void Disassemble_Add_LongFormat()
        {
            var cpu = new Cpu();
            
            // Build: A L /200
            InstructionBuilder.BuildLongAtAddress(OpCodes.Add, 0, 0x0200, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("A", result);
            Assert.Contains("L", result);
            Assert.Contains("/0200", result);
        }
        
        [Fact]
        public void Disassemble_Add_WithIndexRegister()
        {
            var cpu = new Cpu();
            
            // Build: A 1 /200  (short format with index 1)
            InstructionBuilder.BuildShortAtAddress(OpCodes.Add, 1, 0x10, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("A", result);
            Assert.Contains("1", result);
        }
        
        [Fact]
        public void Disassemble_Add_LongIndirect()
        {
            var cpu = new Cpu();
            
            // Build: A I /200 (indirect, which implies long format)
            InstructionBuilder.BuildLongIndirectAtAddress(OpCodes.Add, 0, 0x0200, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("A", result);
            Assert.Contains("I", result);
            Assert.Contains("/0200", result);
            // Should NOT contain "L" separately since indirect implies long
            Assert.DoesNotContain("IL", result);
        }
        
        [Fact]
        public void Disassemble_Add_LongIndirectWithIndex()
        {
            var cpu = new Cpu();
            
            // Build: A I2 /200 (indirect with index 2, implies long format)
            InstructionBuilder.BuildLongIndirectAtAddress(OpCodes.Add, 2, 0x0200, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("A", result);
            Assert.Contains("I2", result);  // Should be combined as "I2"
            Assert.Contains("/0200", result);
        }
        
        [Fact]
        public void Disassemble_Load_LongFormat()
        {
            var cpu = new Cpu();
            
            // Build: LD L /0500
            InstructionBuilder.BuildLongAtAddress(OpCodes.Load, 0, 0x0500, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("LD", result);
            Assert.Contains("L", result);
            Assert.Contains("/0500", result);
        }
        
        [Fact]
        public void Disassemble_Store_ShortFormat()
        {
            var cpu = new Cpu();
            
            // Build: ST /20 (short format, displacement = 0x20 - 0x101 = -0xE1 = 0x1F in signed byte)
            // Actually, let's use a positive displacement: target 0x120, from 0x100 -> disp = 0x1F
            InstructionBuilder.BuildShortAtAddress(OpCodes.Store, 0, 0x1F, cpu, 0x100);
            
            var result = cpu.Disassemble(0x100);
            
            Assert.Contains("ST", result);
            Assert.Contains("/0120", result);
        }
    }
}
