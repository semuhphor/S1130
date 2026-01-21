namespace S1130.Asm1130;

/// <summary>
/// IBM 1130 Cross-Assembler - C# Port of asm1130.c
/// Original by Brian Knittel, ported to C# for the S1130 Emulator Project
/// </summary>
class Program
{
    static int Main(string[] args)
    {
        try
        {
            var assembler = new Assembler();
            assembler.Initialize(args);
            
            if (!assembler.HasInputFiles())
            {
                assembler.PrintUsage();
                return 1;
            }

            // Pass 1: Build symbol table
            assembler.StartPass(1);
            assembler.ProcessFiles();

            if (assembler.PassCountOnly)
            {
                assembler.PrintPassReport();
                return 0;
            }

            // Pass 2: Generate output
            assembler.StartPass(2);
            assembler.ProcessFiles();
            assembler.FinalizeAssembly();

            return assembler.ErrorCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }
}
