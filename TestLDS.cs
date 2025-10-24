using S1130.SystemObjects;

var cpu = new Cpu();
var source = "      ORG  /0100\n      LDS  3";
var result = cpu.Assemble(source);

if (result.Success) 
{
    var word = cpu[0x100];
    Console.WriteLine($"Assembled word: 0x{word:X4} ({word})");
    var disasm = cpu.Disassemble(0x100);
    Console.WriteLine($"Disassembled: '{disasm}'");
    
    // Try reassembling
    var cpu2 = new Cpu();
    var source2 = $"      ORG  /0100\n      {disasm}";
    var result2 = cpu2.Assemble(source2);
    
    if (result2.Success)
    {
        var word2 = cpu2[0x100];
        Console.WriteLine($"Reassembled word: 0x{word2:X4} ({word2})");
        Console.WriteLine($"Match: {word == word2}");
    }
    else
    {
        Console.WriteLine($"Reassembly failed: {string.Join(", ", result2.Errors.Select(e => e.Message))}");
    }
}
else 
{
    Console.WriteLine($"Assembly failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
