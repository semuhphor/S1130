#!/usr/bin/env dotnet-script

using System.Text.RegularExpressions;

var file = @"f:\Development\S1130\tests\UnitTests.S1130.SystemObjects\InstructionFormatValidation.cs";
var content = File.ReadAllText(file);

// Fix short format syntax patterns
content = Regex.Replace(content, @"(\w+)    \. /", "$1 /");           // "LD    . /" -> "LD /"
content = Regex.Replace(content, @"(\w+)    1 /", "$1 |1|/");         // "LD    1 /" -> "LD |1|/"
content = Regex.Replace(content, @"(\w+)    2 /", "$1 |2|/");         // "LD    2 /" -> "LD |2|/"
content = Regex.Replace(content, @"(\w+)    3 /", "$1 |3|/");         // "LD    3 /" -> "LD |3|/"

// Change /0100 to /0050 ONLY for short format tests (those without Long or Indirect in description)
var lines = content.Split('\n');
var result = new List<string>();
bool inShortFormat = false;

for (int i = 0; i < lines.Length; i++)
{
    var line = lines[i];
    
    // Track when we're in a short format test
    if (line.Contains("Description = ") && line.Contains("Short format"))
        inShortFormat = true;
    else if (line.Contains("Description = ") && (line.Contains("Long format") || line.Contains("Indirect")))
        inShortFormat = false;
    
    // Only replace /0100 with /0050 in short format tests
    if (inShortFormat && line.Contains("/0100"))
    {
        line = line.Replace("/0100", "/0050");
        line = line.Replace("0x0100", "0x0050");
    }
    
    result.Add(line);
}

File.WriteAllText(file, string.Join('\n', result));
Console.WriteLine("Fixed instruction validation file!");
