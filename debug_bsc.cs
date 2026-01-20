using S1130.SystemObjects;

var cpu = new Cpu();

var source = @"      ORG /100
      BSC  O NEXT1
NEXT1 WAIT";

var result = cpu.Assemble(source);

if (!result.Success)
{
    Console.WriteLine("Assembly failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  Line {error.LineNumber}: {error.Message}");
    }
}
else
{
    Console.WriteLine("Assembly succeeded!");
    Console.WriteLine($"Instruction at 0x100: 0x{cpu[0x100]:X4}");
    Console.WriteLine($"Modifier bits: 0x{cpu[0x100] & 0xFF:X2}");
}
