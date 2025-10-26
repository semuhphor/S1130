using System;
using System.Linq;
using S1130.SystemObjects;

var cpu = new Cpu();

var source = @"// IBM 1130 Shift Left Test Program
// Demonstrates SLT (Shift Left Together) with carry detection
// 
// This program loads 1 into ACC/EXT registers, then repeatedly
// shifts left until carry is set (bit shifts out), then restarts.
// Watch the bit travel through all 32 bits!
//
       ORG  /100
//
// Main program loop
//
START: LDD  |L|ONE       // Load double-word (0,1) into ACC and EXT
LOOP:  SLT  1            // Shift left together 1 bit
       BSC  |L|LOOP,C    // Carry OFF -- keep shifting
       BSC  |L|START     // Carry ON - reload 0,1 into acc/ext
//
// Data section
//
       BSS  |E|          // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
";

Console.WriteLine("=== ASSEMBLING PROGRAM ===");
var result = cpu.Assemble(source);

if (!result.Success)
{
    Console.WriteLine("ASSEMBLY FAILED:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  Line {error.LineNumber}: {error.Message}");
    }
    return;
}

Console.WriteLine($"Success! Generated {result.GeneratedWords.Length} words");
Console.WriteLine();

Console.WriteLine("=== MEMORY CONTENTS /100 through /110 ===");
Console.WriteLine("Address  Hex    Decimal  Instruction/Data");
Console.WriteLine("-------  ----   -------  ----------------");

for (int addr = 0x100; addr <= 0x110; addr++)
{
    ushort value = cpu[(ushort)addr];
    string disasm = "";
    
    // Try to disassemble if it looks like an instruction
    if (addr >= 0x100 && addr <= 0x106)
    {
        try
        {
            cpu.Iar = (ushort)addr;
            disasm = cpu.CurrentInstruction?.Disassemble(cpu, (ushort)addr) ?? "";
        }
        catch
        {
            disasm = "(data)";
        }
    }
    else
    {
        disasm = "(data)";
    }
    
    Console.WriteLine($"/{addr:X3}     {value:X4}   {value,5}    {disasm}");
}

Console.WriteLine();
Console.WriteLine("=== SYMBOL TABLE ===");
Console.WriteLine("Symbol    Address");
Console.WriteLine("--------  -------");
var symbols = result.SymbolTable.OrderBy(kvp => kvp.Value);
foreach (var sym in symbols)
{
    if (sym.Value >= 0x100 && sym.Value <= 0x110)
    {
        Console.WriteLine($"{sym.Key,-8}  /{sym.Value:X3}");
    }
}
