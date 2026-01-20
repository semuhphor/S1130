#r "src/S1130.SystemObjects/bin/Debug/net10.0/S1130.SystemObjects.dll"

using S1130.SystemObjects.Assembler;
using System.IO;

var code = File.ReadAllText(@"docs\TestAsm\test1132.s1130");
var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

var assembler = new Assembler();
var result = assembler.Assemble(lines);

if (result.Success)
{
    Console.WriteLine($"✓ Assembly successful! Generated {result.GeneratedWords.Length} words");
    Console.WriteLine("\nFirst 20 lines of listing:");
    foreach (var line in result.Listing.Take(20))
    {
        Console.WriteLine(line);
    }
    
    Console.WriteLine($"\n✓ {result.Symbols.Count} symbols defined");
    Console.WriteLine("\nSymbol table (first 20):");
    foreach (var sym in result.Symbols.Take(20))
    {
        Console.WriteLine($"  {sym.Key,-12} = {sym.Value:X4}");
    }
}
else
{
    Console.WriteLine($"✗ Assembly failed with {result.Errors.Count} errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  Line {error.LineNumber}: {error.Message}");
    }
}
