using S1130.SystemObjects;

namespace S1130.AssemblerConverterUtility;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
        {
            Console.WriteLine("IBM 1130 / S1130 Assembler Format Converter");
            Console.WriteLine();
            Console.WriteLine("Usage: asmconv [input-file [output-file]]");
            Console.WriteLine();
            Console.WriteLine("Auto-detects format and converts between:");
            Console.WriteLine("  - IBM 1130 format (fixed-column)");
            Console.WriteLine("  - S1130 format (free-form with |format| specifiers)");
            Console.WriteLine();
            Console.WriteLine("If no files specified, reads from stdin and writes to stdout.");
            Console.WriteLine("If only input-file specified, reads from file and writes to stdout.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  asmconv < input.asm > output.s1130");
            Console.WriteLine("  asmconv input.asm > output.s1130");
            Console.WriteLine("  asmconv input.s1130 output.asm");
            Console.WriteLine("  type input.asm | asmconv > output.s1130");
            return 0;
        }

        try
        {
            // Determine input/output
            TextReader input;
            TextWriter output;

            if (args.Length >= 1)
            {
                // Read from file
                if (!File.Exists(args[0]))
                {
                    Console.Error.WriteLine($"Error: File not found: {args[0]}");
                    return 1;
                }
                input = new StreamReader(args[0]);
            }
            else
            {
                // Read from stdin
                input = Console.In;
            }

            if (args.Length >= 2)
            {
                // Write to file
                output = new StreamWriter(args[1]);
            }
            else
            {
                // Write to stdout
                output = Console.Out;
            }

            // Read all lines
            var lines = new List<string>();
            string? line;
            while ((line = input.ReadLine()) != null)
            {
                lines.Add(line);
            }

            // Close input if it's a file
            if (input != Console.In)
                input.Dispose();

            // Detect format
            var isIBMFormat = DetectIBMFormat(lines.ToArray());

            // Convert all lines at once
            string converted;
            if (isIBMFormat)
            {
                converted = AssemblerConverter.ToS1130Format(string.Join("\n", lines));
            }
            else
            {
                converted = AssemblerConverter.ToIBM1130Format(string.Join("\n", lines));
            }
            
            // Write output
            output.Write(converted);

            // Close output if it's a file
            if (output != Console.Out)
                output.Dispose();

            // Log to stderr (won't interfere with stdout)
            var formatFrom = isIBMFormat ? "IBM 1130" : "S1130";
            var formatTo = isIBMFormat ? "S1130" : "IBM 1130";
            Console.Error.WriteLine($"[{formatFrom} → {formatTo}] {lines.Count} lines");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Detects if the content is in IBM 1130 fixed-column format
    /// vs S1130 free-form format.
    /// </summary>
    static bool DetectIBMFormat(string[] lines)
    {
        var ibmIndicators = 0;
        var s1130Indicators = 0;
        var sampledLines = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            sampledLines++;
            if (sampledLines > 50) // Sample first 50 non-empty lines
                break;

            // S1130 format indicators (check first for reliability):
            // - Format specifiers with pipes: |L|, |I|, |.|, |L1|, etc.
            if (line.Contains("|L|") || line.Contains("|I|") || line.Contains("|.|") ||
                line.Contains("|L1|") || line.Contains("|L2|") || line.Contains("|L3|") ||
                line.Contains("|I1|") || line.Contains("|I2|") || line.Contains("|I3|"))
            {
                s1130Indicators += 10; // Strong indicator
            }

            // IBM format indicators:
            // - Lines start with 20+ spaces (object code area)
            if (line.Length >= 25)
            {
                var leadingSpaces = 0;
                for (int i = 0; i < Math.Min(30, line.Length); i++)
                {
                    if (line[i] == ' ')
                        leadingSpaces++;
                    else
                        break;
                }

                if (leadingSpaces >= 20)
                    ibmIndicators++;
            }

            // Check for S1130 typical pattern: 6 spaces or less indent
            if (line.Length > 0 && !line.StartsWith("                    ")) // Not 20+ spaces
            {
                var trimmed = line.TrimStart();
                if (trimmed.Length > 0)
                {
                    var leadingSpaces = line.Length - trimmed.Length;
                    if (leadingSpaces <= 6)
                        s1130Indicators++;
                }
            }
        }

        // Clear winner?
        if (s1130Indicators > ibmIndicators * 2)
            return false; // S1130 format

        if (ibmIndicators > s1130Indicators * 2)
            return true; // IBM format

        // Default to IBM if unclear (most legacy files are IBM)
        return true;
    }
}
