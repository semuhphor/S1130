namespace S1130.Asm1130;

/// <summary>
/// Relocation type for assembled words
/// </summary>
public enum RelocationType
{
    Absolute = 0,   // Absolute address
    Relative = 1,   // Relocatable address
    Libf = 2,       // Library function reference
    Call = 3        // Call reference
}

/// <summary>
/// Symbol definition state
/// </summary>
public enum SymbolState
{
    Undefined = 0,    // Not yet defined
    Provisional = 1,  // Defined with forward references
    Defined = 2       // Fully defined
}

/// <summary>
/// Program type for binary output
/// </summary>
public enum ProgramType
{
    Absolute = 1,
    Relocatable = 2,
    Libf = 3,
    Call = 4,
    IssLibf = 5,
    IssCall = 6,
    Ils = 7
}

/// <summary>
/// Output file mode
/// </summary>
public enum OutputMode
{
    Load,    // Simulator load format (.out)
    Binary   // Binary card format (.bin)
}

/// <summary>
/// Symbol table entry
/// </summary>
public class Symbol
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public int DefinedPass { get; set; }
    public SymbolState State { get; set; }
    public RelocationType Relocation { get; set; }
    public List<CrossReference> CrossReferences { get; } = new();
}

/// <summary>
/// Cross reference entry
/// </summary>
public class CrossReference
{
    public string FileName { get; set; } = "";
    public int LineNumber { get; set; }
    public bool IsDefinition { get; set; }
}

/// <summary>
/// Expression evaluation result
/// </summary>
public class Expression
{
    public int Value { get; set; }
    public RelocationType Relocation { get; set; }
}

/// <summary>
/// Literal constant pending output
/// </summary>
public class Literal
{
    public int Value { get; set; }
    public int TagNumber { get; set; }
    public bool IsHex { get; set; }
    public bool RequiresEvenAddress { get; set; }
}

/// <summary>
/// Opcode definition flags
/// </summary>
[Flags]
public enum OpcodeFlags
{
    None = 0,
    IsDouble = 1,      // Double-word instruction
    Is1800 = 2,        // IBM 1800 specific
    Trap = 4           // Assembler debugging breakpoint
}

/// <summary>
/// Opcode handler delegate
/// </summary>
public delegate void OpcodeHandler(Opcode op, string label, string mods, string arg);

/// <summary>
/// Opcode table entry
/// </summary>
public class Opcode
{
    public string Mnemonic { get; set; } = "";
    public OpcodeHandler Handler { get; set; } = null!;
    public string ModifiersAllowed { get; set; } = "";
    public string ModifiersImplied { get; set; } = "";
    public OpcodeFlags Flags { get; set; }
    public int BaseValue { get; set; }  // Base instruction value
}
