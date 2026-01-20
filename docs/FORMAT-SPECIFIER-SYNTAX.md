# S1130 Assembler Format Specifier Syntax

## Overview

The S1130 assembler uses a modern free-form syntax with explicit format specifiers enclosed in pipes (`|format|`). This provides clarity and readability while maintaining compatibility with IBM 1130 instruction semantics. The S1130 format can be converted to/from the original IBM 1130 fixed-column card format using the `asmconv` utility.

## Two Format Styles

The S1130 toolchain supports two assembler formats:

### 1. S1130 Free-Form (Recommended)
Modern syntax with `|format|` specifiers - flexible spacing and more readable.

### 2. IBM 1130 Fixed-Column (Legacy)
Original 1960s punched card format - strict column positioning for compatibility.

Use the `asmconv` utility to convert between formats (see [asmconv README](../src/S1130.AssemblerConverter/README.md)).

## S1130 Format Specifiers

Format specifiers are enclosed in pipes and placed between the operation and operand:

```
[label]  OPERATION  |format|  operand
```

### Card-Style Format Specification

The assembler supports format specifiers that correspond to the IBM 1130 card format (columns 32-33):

**Format Specifier Chart:**
| Specifier | Format | Index Reg | Description |
|-----------|--------|-----------|-------------|
| (none) | Short | None | Short format (default), no index register |
| `|L|` | Long | None | Long format, no index register |
| `|1|` | Short | XR1 | Short format with index register 1 |
| `|2|` | Short | XR2 | Short format with index register 2 |
| `|3|` | Short | XR3 | Short format with index register 3 |
| `|L1|` | Long | XR1 | Long format with index register 1 |
| `|L2|` | Long | XR2 | Long format with index register 2 |
| `|L3|` | Long | XR3 | Long format with index register 3 |
| `|I|` | Indirect | None | Indirect addressing, no index register |
| `|I1|` | Indirect | XR1 | Indirect with index register 1 |
| `|I2|` | Indirect | XR2 | Indirect with index register 2 |
| `|I3|` | Indirect | XR3 | Indirect with index register 3 |

### Short Format (No Specifier)

Short format is the **default** when no format specifier is provided. It uses IAR-relative addressing (±127 words).

**Syntax:**
```
OPERATION address
```

**Example:**
```
      ORG /100
START  LD  VALUE        Load from IAR-relative address (short format)
       A  VALUE         Add from IAR-relative address  
       STO RESULT       Store to IAR-relative address
       WAIT
VALUE  DC 42
RESULT  DC 0
```

**Generated Code:**
- Each instruction is 1 word
- Displacement calculated as: `address - (IAR + 1)`
- Range: -128 to +127 from current instruction
- Automatically selected when address is within range

### Long Format (`|L|`)

Explicitly indicates long format (absolute 16-bit addressing).

**Syntax:**
```
OPERATION |L| address
```

**Example:**
```
      ORG /100
START  LD  |L| /0500    Load from absolute address 0x0500
       A  |L| VALUE     Add from VALUE's absolute address  
       STO  |L| RESULT  Store to RESULT's absolute address
       WAIT
VALUE  DC 42
RESULT  DC 0
```

**Generated Code:**
- Each instruction is 2 words (instruction + address)
- Full 16-bit address specified directly
- Range: 0x0000 to 0xFFFF

### Index Register Formats (`|1|`, `|2|`, `|3|`, `|L1|`, `|L2|`, `|L3|`)

Combines format specification with index register selection.

**Syntax:**
```
OPERATION |formatreg| address
```

**Examples:**
```
      ORG /100
       LDX  |1| /0005      Initialize XR1 to 5
      
LOOP   LD  |1| TABLE     Load TABLE[XR1] (short format + XR1)
       A  |L1| DATA      Add DATA[XR1] (long format + XR1)
       STO  |2| RESULT   Store to RESULT[XR2] (short + XR2)
       MDX  LOOP,-1      Decrement and loop
      
TABLE  DC 10,20,30,40,50
DATA   DC 100,200,300,400,500
RESULT  BSS 5
```

**Generated Code:**
- Short format with index (|1|, |2|, |3|): 1 word
- Long format with index (|L1|, |L2|, |L3|): 2 words
- Effective address = base address + index register value

### Indirect Formats (`|I|`, `|I1|`, `|I2|`, `|I3|`)

Adds an additional level of address indirection.

**Syntax:**
```
OPERATION |I| address
OPERATION |Ireg| address
```

**Examples:**
```
      ORG /100
       LD  |I| PTR        Load from address stored in PTR
       LD  |I1| PTRTBL    Load from address in PTRTBL[XR1]
      
PTR    DC /0500           Points to address 0x0500
PTRTBL  DC /0600          Pointer table
        DC /0700
```

### No Specifier (Directives)

Directives like `ORG`, `DC`, `BSS`, `EQU`, `END` don't use format specifiers:

```
      ORG /100
VALUE  DC 42
BUFFER  BSS 10
COUNT  EQU 5
```

## IBM 1130 Fixed-Column Format (Legacy)

For compatibility with original IBM 1130 assembler source code, the toolchain supports the traditional punched card format. Use `asmconv` to convert between formats.

### Column Layout

```
Columns  Content
-------  -------
1-20     Object code area (blank in source)
21-25    Label (5 characters max)
26       Mandatory blank
27-30    Operation code (4 characters)
31       Mandatory blank
32       Format code (blank=' ', L=long, I=indirect)
33       Tag/Index register (blank, 1, 2, or 3)
34       Mandatory blank
35-71    Operand and remarks
72       Blank (optional)
73-80    Sequence number (optional)
```

### Example IBM 1130 Format

```
                    START LD   L  VALUE
                          A       TOTAL
                          STO  L  RESULT
                    LOOP  MDX  L1 COUNT-1,1
                    VALUE DC      42
```

### Converting Between Formats

Use the `asmconv` utility for bidirectional conversion:

```powershell
# IBM 1130 → S1130
asmconv legacy.asm modern.s1130

# S1130 → IBM 1130
asmconv modern.s1130 legacy.asm

# Pipe operation
type legacy.asm | asmconv > modern.s1130
```

See the [asmconv documentation](../src/S1130.AssemblerConverter/README.md) for details.

## Historical Context

The original IBM 1130 assembler used punched cards with fixed column positions. The 80-column card layout was dictated by physical card readers and punchers used in 1960s data processing:

- **Columns 1-20**: Reserved for object code generated during assembly
- **Column 26, 31, 34**: Mandatory blanks served as field separators
- **Column 32**: Format specification (blank/space = short, L = long, I = indirect)
- **Column 33**: Index register tag (blank, 1, 2, or 3)

The S1130 free-form syntax with `|format|` specifiers provides several advantages:

## Benefits of S1130 Format

1. **Clarity**: Code explicitly shows whether short or long format is used
2. **Readability**: No column restrictions, flexible spacing, easier to read
3. **Intent**: Programmer's addressing mode choice is clearly visible
4. **Compatibility**: Can be converted to/from IBM 1130 format
5. **Modern**: Familiar syntax for developers used to modern assemblers
6. **Optimization**: Short format uses half the memory (1 word vs 2 words)

## Mixed Format Example

You can mix format specifiers within the same program:

```
      ORG /100
START  LD  NEAR          Short format for nearby data (1 word)
       A  |L| /0500     Long format for far address (2 words)
       M  COUNT          Short format for nearby variable (1 word)
       STO  |L| RESULT  Long format for absolute address (2 words)
       BSI  SUBR         Short format branch (1 word)
       WAIT

* Data within ±127 words - use short format
NEAR   DC 10
COUNT  DC 5

* Subroutine within range
SUBR   DC 0
        LD  |L| VALUE
        BSC  |L| SUBR I  Return
      
RESULT  DC 0
VALUE   DC 42
```

## Performance Considerations

**Short Format (No Specifier):**
- ✅ 1 word per instruction (saves memory)
- ✅ Faster to fetch (single memory read)
- ❌ Limited range (±127 words)

**Long Format (`|L|`):**
- ✅ Full addressing range (64K)
- ✅ Position-independent
- ❌ 2 words per instruction (uses more memory)
- ❌ Slower to fetch (two memory reads)

## Format Conversion Tools

### asmconv Utility

The `asmconv` utility provides bidirectional conversion between IBM 1130 and S1130 formats:

**Features:**
- Auto-detects input format
- Supports files, stdin/stdout, and pipes
- Preserves labels, operations, format codes, and operands
- Fast and lightweight

**Installation:**
```powershell
dotnet build src/S1130.AssemblerConverter/S1130.AssemblerConverter.csproj
```

**Usage:**
```powershell
# Pipe operation
type legacy.asm | asmconv > modern.s1130

# File conversion
asmconv input.asm output.s1130

# Batch convert
Get-ChildItem *.asm | ForEach-Object {
    asmconv $_.Name "$($_.BaseName).s1130"
}
```

See [asmconv README](../src/S1130.AssemblerConverter/README.md) for complete documentation.

## Implementation Details

### Parsing Format Specifiers

The assembler parses `|format|` specifiers in the operand field:

```csharp
// Example: "LD  |L| VALUE" → operation="LD", format="L", operand="VALUE"
var formatMatch = Regex.Match(operand, @"\|([.LI123]+)\|");
if (formatMatch.Success)
{
    formatSpec = formatMatch.Groups[1].Value;
    // Parse format code and tag from formatSpec
    // Remove |format| from operand
}
```

### Location Counter Advancement

During Pass 1, the assembler accounts for instruction format:

```csharp
bool isLongFormat = formatSpec.StartsWith("L") || formatSpec.Length > 1;
_context.LocationCounter += isLongFormat ? 2 : 1;
```

### AssemblerConverter Class

The `AssemblerConverter` class provides conversion methods:

```csharp
// Convert S1130 → IBM 1130
string ibmFormat = AssemblerConverter.ToIBM1130Format(s1130Line);

// Convert IBM 1130 → S1130
string s1130Format = AssemblerConverter.ToS1130Format(ibmLine);
```

**AssemblerLine Class:**
- Encapsulates all line components (Label, Operation, FormatCode, Tag, Operand, Remark)
- Provides `ToIBM1130Format()` and `ToS1130Format()` methods
- Static parsers: `ParseS1130()` and `ParseIBM1130()`

## Related Documentation

- [asmconv README](../src/S1130.AssemblerConverter/README.md) - Format converter utility
- [Assembler.md](Assembler.md) - Full assembler documentation
- [InternalOperation.md](InternalOperation.md) - CPU instruction execution
- IBM 1130 Functional Characteristics (original IBM documentation)

## Test Coverage

Comprehensive testing ensures format handling works correctly:

**AssemblerTests.cs:**
- Format specifier parsing and validation
- Mixed format programs
- Short/long format code generation

**AssemblerConverterTests.cs:**
- 24 comprehensive conversion tests
- Round-trip conversion verification (S1130 → IBM → S1130)
- Format detection accuracy
- Label, operation, and operand preservation

All tests passing ✓
