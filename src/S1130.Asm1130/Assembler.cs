namespace S1130.Asm1130;

/// <summary>
/// Main assembler class - coordinates the assembly process
/// </summary>
public partial class Assembler
{
    // Configuration
    public bool Verbose { get; set; }
    public bool TabFormat { get; set; }
    public bool Enable1800 { get; set; }
    public int ListOffset { get; set; }
    public bool PassCountOnly { get; set; }
    public bool PreloadSymbols { get; set; }
    public bool SaveSymbolTable { get; set; }
    public bool SavePrompt { get; set; } = true;
    public bool DoList { get; set; }
    public bool DoXref { get; set; }
    public bool DoSymbols { get; set; }
    public bool DmsMode { get; set; }  // Treat % and < as ( and )
    
    // State
    public int Pass { get; private set; }
    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }
    public int LineNumber { get; private set; }
    public bool Ended { get; private set; }
    public bool HasForwardReferences { get; private set; }
    public bool Assembled { get; private set; }
    public RelocationType Relocate { get; private set; } = RelocationType.Relative;
    public int Origin { get; private set; }
    public int OriginAdvanced { get; private set; }
    public int ProgramTransferAddress { get; private set; } = -1;
    
    // Files
    public string CurrentFileName { get; private set; } = "";
    public string ProgramName { get; private set; } = "";
    public string? OutputFileName { get; private set; }
    public string? ListFileName { get; private set; }
    public OutputMode OutputMode { get; private set; } = OutputMode.Load;
    
    private readonly List<string> _inputFiles = new();
    private StreamWriter? _outputFile;
    private StreamWriter? _listFile;
    private StreamReader? _inputFile;
    
    // Symbol table
    private readonly Dictionary<string, Symbol> _symbols = new();
    
    // Literals
    private readonly List<Literal> _literals = new();
    
    // Listing
    private string _listLine = "";
    private bool _lineError;
    private bool _listOn = true;
    private int _wordsOutput;
    
    // Opcodes
    private readonly List<Opcode> _opcodes = new();
    
    public Assembler()
    {
        InitializeOpcodes();
    }
    
    public void Initialize(string[] args)
    {
        // Check if executable name contains "1800" for auto-enable
        var exeName = Environment.GetCommandLineArgs()[0];
        if (exeName.Contains("1800", StringComparison.OrdinalIgnoreCase))
            Enable1800 = true;
            
        // Process command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith('-'))
            {
                ProcessFlag(args[i][1..]);
            }
            else
            {
                _inputFiles.Add(args[i]);
            }
        }
        
        // Set default output filename if not specified
        if (OutputFileName == null && _inputFiles.Count > 0)
        {
            var ext = OutputMode == OutputMode.Load ? ".out" : ".bin";
            OutputFileName = Path.ChangeExtension(_inputFiles[0], ext);
        }
        
        // Set default list filename if listing requested
        if (DoList && ListFileName == null && _inputFiles.Count > 0)
        {
            ListFileName = Path.ChangeExtension(_inputFiles[0], ".lst");
        }
        
        // Extract program name from first file
        if (_inputFiles.Count > 0)
        {
            ProgramName = Path.GetFileNameWithoutExtension(_inputFiles[0]);
            if (ProgramName.Length > 8)
                ProgramName = ProgramName[..8];
        }
    }
    
    public bool HasInputFiles() => _inputFiles.Count > 0;
    
    public void PrintUsage()
    {
        Console.Error.WriteLine(
            "Usage: asm1130 [-bdpsvwxy8] [-o[file]] [-l[file] [-fXXXX]] [-rN.M] file...\n\n" +
            "-b  binary (relocatable format) output; default is simulator LOAD format\n" +
            "-d  interpret % and < as ( and ), for assembling DMS sources\n" +
            "-p  count passes required; no assembly output is created with this flag\n" +
            "-s  add symbol table to listing\n" +
            "-v  verbose mode\n" +
            "-w  write system symbol table as SYMBOLS.SYS\n" +
            "-W  same as -w but do not confirm overwriting previous file\n" +
            "-x  add cross reference table to listing\n" +
            "-y  preload system symbol table SYMBOLS.SYS\n" +
            "-8  enable IBM 1800 instructions\n" +
            "-o  set output file; default is first input file + .out or .bin\n" +
            "-l  create listing file; default is first input file + .lst\n" +
            "-r  set dms version to VN RM for system SBRK cards\n" +
            "-f  apply offset XXXX (hex) to APPARENT assembly address listing\n");
    }
    
    private void ProcessFlag(string flag)
    {
        for (int i = 0; i < flag.Length; i++)
        {
            switch (flag[i])
            {
                case 'o':
                    OutputFileName = i + 1 < flag.Length ? flag[(i + 1)..] : null;
                    return;
                    
                case 'p':
                    PassCountOnly = true;
                    break;
                    
                case 'v':
                    Verbose = true;
                    break;
                    
                case 'x':
                    DoXref = true;
                    break;
                    
                case 's':
                    DoSymbols = true;
                    break;
                    
                case 'l':
                    ListFileName = i + 1 < flag.Length ? flag[(i + 1)..] : null;
                    DoList = true;
                    return;
                    
                case 'W':
                    SavePrompt = false;
                    goto case 'w';
                    
                case 'w':
                    SaveSymbolTable = true;
                    break;
                    
                case 'y':
                    PreloadSymbols = true;
                    break;
                    
                case 'b':
                    OutputMode = OutputMode.Binary;
                    break;
                    
                case '8':
                    Enable1800 = true;
                    break;
                    
                case 'd':
                    DmsMode = true;
                    break;
                    
                case 'r':
                case 'f':
                    // Skip these for now - not critical
                    return;
                    
                default:
                    throw new ArgumentException($"Unknown flag: -{flag[i]}");
            }
        }
    }
    
    public void StartPass(int passNumber)
    {
        Pass = passNumber;
        ErrorCount = 0;
        Origin = 0;
        LineNumber = 0;
        Relocate = RelocationType.Relative;
        Assembled = false;
        _listOn = DoList;
        _literals.Clear();
        
        if (Pass == 1)
        {
            // First pass: sort opcodes for binary search
            _opcodes.Sort((a, b) => string.Compare(a.Mnemonic, b.Mnemonic, StringComparison.Ordinal));
            
            if (PreloadSymbols)
                LoadSymbolTable();
        }
        else
        {
            // Second pass: open output files
            if (OutputFileName != null)
            {
                _outputFile = new StreamWriter(OutputFileName);
            }
            
            if (DoList && ListFileName != null)
            {
                _listFile = new StreamWriter(ListFileName);
                WriteListHeader();
            }
        }
    }
    
    public void ProcessFiles()
    {
        foreach (var file in _inputFiles)
        {
            ProcessFile(file);
        }
    }
    
    public void FinalizeAssembly()
    {
        // Write transfer address
        if (Pass == 2 && _outputFile != null)
        {
            if (OutputMode == OutputMode.Load)
            {
                if (ProgramTransferAddress >= 0)
                    _outputFile.WriteLine($"={ProgramTransferAddress:X4}");
            }
            // Binary mode handles this in end card
            
            _outputFile.Close();
        }
        
        // Finish listing
        if (_listFile != null)
        {
            if (ErrorCount > 0 || WarningCount > 0)
            {
                _listFile.WriteLine();
                if (ErrorCount > 0)
                    _listFile.WriteLine($"There {(ErrorCount == 1 ? "was" : "were")} {ErrorCount} error{(ErrorCount == 1 ? "" : "s")}");
                if (WarningCount > 0)
                    _listFile.WriteLine($"There {(WarningCount == 1 ? "was" : "were")} {WarningCount} warning{(WarningCount == 1 ? "" : "s")}");
            }
            else
            {
                _listFile.WriteLine("\nThere were no errors in this assembly");
            }
            
            if (ProgramTransferAddress >= 0)
                _listFile.WriteLine($"\nProgram transfer address = {ProgramTransferAddress:X4}");
            
            if (DoXref)
                WriteCrossReference();
            else if (DoSymbols)
                WriteSymbolTable();
            
            _listFile.Close();
        }
        
        if (SaveSymbolTable)
            SaveSymbols();
    }
    
    public void PrintPassReport()
    {
        bool hasUndefined = _symbols.Values.Any(s => 
            s.State == SymbolState.Undefined || s.State == SymbolState.Provisional);
            
        if (hasUndefined)
        {
            Console.WriteLine("There are undefined symbols. Cannot determine pass requirement.");
        }
        else if (HasForwardReferences)
        {
            Console.WriteLine("There are forward references. Two passes are required.");
        }
        else
        {
            Console.WriteLine("There are no forward references. Only one pass is required.");
        }
    }
}
