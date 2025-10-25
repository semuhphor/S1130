using System;
using System.IO;
using S1130.SystemObjects.Assembler;

var sourceCode = File.ReadAllText("test_example.asm");
var assembler = new Assembler();
var result = assembler.Assemble(sourceCode.Split('\n'));

Console.WriteLine("=== ASSEMBLY RESULT ===");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Words Generated: {result.GeneratedWords.Length}");
Console.WriteLine();

if (result.Errors.Any())
{
    Console.WriteLine("ERRORS:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  Line {error.LineNumber}: {error.Message}");
    }
}
else
{
    Console.WriteLine("LISTING:");
    foreach (var line in result.ListingLines)
    {
        Console.WriteLine(line);
    }
    
    Console.WriteLine();
    Console.WriteLine("HEX OUTPUT:");
    for (int i = 0; i < result.GeneratedWords.Length; i++)
    {
        Console.WriteLine($"  {i:X4}: {result.GeneratedWords[i]:X4}");
    }
}
