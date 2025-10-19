using S1130.SystemObjects;
var cpu = new Cpu(32768);
var result = cpu.Assemble("      ORG /100");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Errors: {result.Errors.Length}");
if (result.Errors.Length > 0) {
    foreach (var err in result.Errors) {
        Console.WriteLine($"  Line {err.LineNumber}: {err.Message}");
    }
}
Console.WriteLine($"Listing lines: {result.Listing.Length}");
foreach (var line in result.Listing) {
    Console.WriteLine($"  '{line}'");
}
