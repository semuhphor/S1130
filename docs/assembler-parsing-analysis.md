# IBM 1130 Assembler Parsing Analysis and Recommendations

**Document Version**: 1.0
**Date**: 2025-10-24
**Branch**: feature/emulator-improvements

---

## Executive Summary

This document analyzes the current IBM 1130 assembler implementation, identifies parsing issues discovered through testing, and proposes a flexible parsing architecture to support multiple input formats (fixed-column punch cards, whitespace-aware modern syntax, etc.).

### Key Findings

1. **Incomplete Format Tag Support**: The new format/tag notation (e.g., `. 1`) is partially implemented
2. **BSS Directive Bug**: `BSS E` (align to even, no count) fails because parser expects `BSS E <count>`
3. **Shift Instruction Bug**: Shift instructions don't handle format tags (e.g., `SLT . 1` fails)
4. **Mixed Syntax**: Codebase has incomplete transition from old syntax to new format/tag notation
5. **Parser Inflexibility**: Current parser is tightly coupled to one syntax style

---

## Table of Contents

1. [Background and Context](#background-and-context)
2. [Testing Methodology](#testing-methodology)
3. [Identified Issues](#identified-issues)
4. [Current Parser Architecture](#current-parser-architecture)
5. [Multiple Format Requirements](#multiple-format-requirements)
6. [Proposed Solution Architecture](#proposed-solution-architecture)
7. [Implementation Strategy](#implementation-strategy)
8. [Testing Plan](#testing-plan)
9. [References](#references)

---

## Background and Context

### Email Discussion (Bob Flanders)

From `docs/notes.txt`:

> Ever have one of those nights where you can't stop thinking about something. I had that last night about the assembler.
>
> One of the biggest problems of the assembler is that different instructions have different formats. Like BSC. in short form it's a skip with a possible condition. BSC just skips the next instruction (1 word) while BSC + skips the next instruction if ACC is positive. So I had Claude figure out all the possible formats in the assembler and adjust them to use a new format:
>
> **LABEL: OPCODE |FT|OPERAND**
>
> sometimes the operands can be /100, a Label, a label +/- a value, an indicator like C for carry or O for overflow.
> All comments are //

### New Format Tag Notation

The new syntax requires explicit format/tag specification:

| Format Tag | Meaning | Example |
|------------|---------|---------|
| `.` | Short format, no index | `LD . LABEL` |
| `L` | Long format, no index | `LD L /2000` |
| `1`, `2`, `3` | Short with XR1/2/3 | `LD 1 OFFSET` |
| `L1`, `L2`, `L3` | Long with XR1/2/3 | `LD L2 /1000` |
| `I` | Long indirect | `LD I PTR` |
| `I1`, `I2`, `I3` | Long indirect indexed | `LD I1 TABLE` |

### Implementation Status

According to recent commit history:
- ✅ Branch instructions (BSC, BSI, MDX) updated to new syntax
- ✅ Load/Store instructions (LD, LDD, STO, STD) updated
- ❌ Shift instructions (SLT, SLA, SRA, etc.) NOT updated
- ❌ BSS directive alignment modifier parsing incomplete

---

## Testing Methodology

### Test Environment

- **Backend**: ASP.NET Core 8 (dotnet run on port 5000)
- **Frontend**: React 19 + TypeScript (npm start on port 3000)
- **Testing Tool**: Playwright browser automation
- **Test Program**: Example shift-left program from web interface

### Test Process

1. Started backend and frontend servers
2. Navigated to http://localhost:3000
3. Attempted to assemble provided example program
4. Documented errors
5. Made targeted fixes to test hypotheses
6. Analyzed source code to identify root causes

---

## Identified Issues

### Issue 1: BSS E Directive Parsing

**File**: `src/S1130.SystemObjects/Assembler.cs`, lines 822-827
**Severity**: HIGH
**Status**: CONFIRMED

#### Problem Description

The BSS directive's even alignment modifier requires a space after 'E':

```csharp
// Line 823
if (operandUpper.StartsWith("E "))
{
    evenAlign = true;
    countStr = operandUpper.Substring(2).Trim();
}
```

#### Test Case That Failed

```assembly
BSS  E    // Align to even address
```

**Error**: `Line 20: Undefined symbol: E`

#### Workaround

```assembly
BSS  E 0  // Explicitly specify count as 0
```

#### Root Cause

The parser expects format: `BSS E <count>` but IBM 1130 assembler supports `BSS E` (align to even, reserve 0 words).

#### Proposed Fix

```csharp
// Enhanced parser logic
var operandUpper = operand.Trim().ToUpper();
bool evenAlign = false;
string countStr = "0";  // Default count is 0

if (operandUpper == "E")
{
    // Just "E" - align to even, no additional space
    evenAlign = true;
}
else if (operandUpper.StartsWith("E "))
{
    // "E <count>" - align to even and reserve space
    evenAlign = true;
    countStr = operandUpper.Substring(2).Trim();
}
else
{
    // No E modifier, entire operand is count
    countStr = operandUpper;
}
```

#### Test Strategy

```csharp
[Theory]
[InlineData("E", true, 0)]        // Just E
[InlineData("E 0", true, 0)]      // E with explicit 0
[InlineData("E 10", true, 10)]    // E with count
[InlineData("5", false, 5)]       // No E, just count
[InlineData("/0A", false, 10)]    // Hex count
public void ProcessBss_VariousFormats(string operand, bool expectEven, int expectCount)
{
    // Test implementation
}
```

---

### Issue 2: Shift Instructions Don't Support Format Tags

**File**: `src/S1130.SystemObjects/Assembler.cs`, lines 1148-1183
**Severity**: HIGH
**Status**: CONFIRMED

#### Problem Description

The `ProcessShift` method expects a plain number operand:

```csharp
// Line 1163
if (!byte.TryParse(operand.Trim(), out byte shiftCount))
{
    _context.AddError(_currentLine, $"Invalid shift count in {mnemonic}: {operand}");
    return;
}
```

But with new syntax, the operand includes format tag: `. 1`

#### Test Case That Failed

```assembly
LOOP  SLT  . 1       // Shift left together 1 bit
```

**Error**: `Line 13: Invalid shift count in SLT: . 1`

#### Why This Matters

According to the new consistent syntax, ALL instructions should use format/tag notation. Shift instructions are currently an exception.

#### Root Cause

Shift instructions in IBM 1130 are actually short-format only and don't use addressing modes like other instructions. They encode:
- Opcode (5 bits)
- Shift type (2 bits in displacement field)
- Shift count (6 bits in displacement field)

The format tag notation doesn't apply the same way, but for consistency, the syntax was updated to require it.

#### Proposed Fix

```csharp
private void ProcessShift(string operand, byte opcode, byte shiftType, string mnemonic)
{
    // ... address overflow check ...

    // Parse operand with optional format tag
    // Expected formats:
    //   ". 5" - New syntax (format tag + count)
    //   "5"   - Old syntax (just count) - for backward compatibility

    string countStr = operand.Trim();

    // Check for format tag (for new syntax consistency)
    var parts = countStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length == 2)
    {
        // New syntax: ". 5" or other format tag
        string formatTag = parts[0];
        countStr = parts[1];

        // Validate format tag (shifts are always short format)
        if (formatTag != ".")
        {
            _context.AddError(_currentLine,
                $"Shift instructions only support '.' (short format), got '{formatTag}'");
            return;
        }
    }
    else if (parts.Length > 2)
    {
        _context.AddError(_currentLine,
            $"Invalid shift operand format: {operand}. Expected '. <count>' or '<count>'");
        return;
    }
    // else: parts.Length == 1, old syntax (just count)

    // Parse shift count
    if (!byte.TryParse(countStr, out byte shiftCount))
    {
        _context.AddError(_currentLine, $"Invalid shift count in {mnemonic}: {countStr}");
        return;
    }

    // ... rest of method ...
}
```

#### Test Strategy

```csharp
[Theory]
[InlineData(". 1", 1, true)]      // New syntax
[InlineData(". 32", 32, true)]    // New syntax, larger count
[InlineData("5", 5, true)]        // Old syntax (backward compat)
[InlineData("L 1", 0, false)]     // Invalid - long format
[InlineData("1 5", 0, false)]     // Invalid - indexed
[InlineData(". 64", 0, false)]    // Invalid - count > 63
public void ProcessShift_VariousFormats(string operand, byte expectedCount, bool shouldSucceed)
{
    // Test implementation
}
```

---

### Issue 3: Syntax Inconsistency Across Instructions

**Severity**: MEDIUM
**Status**: ARCHITECTURAL

#### Problem Description

The codebase is in a transitional state:
- Some instructions fully support new format/tag syntax (LD, STO, BSC)
- Some instructions don't support it at all (SLT, SLA, etc.)
- Some directives have incomplete support (BSS E)

This creates confusion for users and makes the assembler hard to maintain.

#### Examples of Inconsistency

| Instruction | Old Syntax | New Syntax | Supported? |
|-------------|------------|------------|------------|
| LD | `LD VAR` | `LD . VAR` | ✅ Yes |
| LDD | `LDD TABLE` | `LDD L TABLE` | ✅ Yes |
| BSC | `BSC O LOOP` | `BSC . O LOOP` | ✅ Yes |
| SLT | `SLT 5` | `SLT . 5` | ❌ No |
| BSS | `BSS E` | `BSS E 0` | ⚠️ Partial |

#### Recommendation

**Complete the transition** to new syntax consistently across all instructions, OR **support both syntaxes** with clear documentation about which is preferred.

---

## Current Parser Architecture

### Component Overview

```
┌─────────────────────────────────────────────────────────┐
│                      Assembler.cs                       │
│                    (1,799 lines)                        │
│                                                         │
│  ┌────────────────┐        ┌────────────────┐         │
│  │   Pass1        │───────>│   Pass2        │         │
│  │  (Symbols)     │        │  (Code Gen)    │         │
│  └────────────────┘        └────────────────┘         │
│           │                         │                   │
│           v                         v                   │
│  ┌────────────────────────────────────────┐            │
│  │         AssemblyLine.cs                │            │
│  │   (Line-level parsing)                 │            │
│  │                                        │            │
│  │  Label | Operation | Operand | Comment│            │
│  └────────────────────────────────────────┘            │
│                         │                               │
│                         v                               │
│  ┌──────────────────────────────────────────┐          │
│  │  Instruction-Specific Processors         │          │
│  │                                          │          │
│  │  • ProcessLoad/Store                    │          │
│  │  • ProcessBranch                        │          │
│  │  • ProcessShift                         │          │
│  │  • ProcessBss/Bes                       │          │
│  │  • ProcessOrg/Dc/Equ                    │          │
│  └──────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────┘
```

### Parsing Flow

1. **AssemblyLine Parsing** (AssemblyLine.cs):
   - Splits line into: Label, Operation, Operand, Comment
   - Operand contains EVERYTHING after operation (including format tags)
   - Simple whitespace-based splitting

2. **Instruction Processing** (Assembler.cs):
   - Each instruction type has dedicated method
   - Methods parse operand differently
   - NO SHARED OPERAND PARSER

3. **Problems**:
   - Code duplication (each processor parses format tags differently)
   - Inconsistent error messages
   - Hard to add new syntax variants
   - Difficult to support multiple formats simultaneously

---

## Multiple Format Requirements

### Use Case 1: Fixed-Column Punch Card Format

**Requirements**:
- Column 1-5: Label (optional)
- Column 7: Continuation indicator (optional)
- Column 8-12: Operation
- Column 16-71: Operand
- Column 72-80: Comments

**Example**:
```
      LD    L /2000          Load from absolute address
LOOP  SLT   . 5              Shift left 5 bits
```

**Characteristics**:
- Fixed positions (columns matter)
- Whitespace is significant for positioning
- Compatible with original IBM 1130 punch card assembler

### Use Case 2: Whitespace-Aware Modern Syntax

**Requirements**:
- Flexible whitespace (tabs or spaces)
- Label at start of line (no indent) or absent
- Operation and operand separated by whitespace
- Comments with // anywhere in line

**Example**:
```assembly
      LD    L /2000          // Load from absolute address
LOOP  SLT   . 5              // Shift left 5 bits
```

**Characteristics**:
- Easy to type in modern editors
- Forgiving about whitespace
- Supports inline comments with //

### Use Case 3: Legacy Syntax (Before Format Tag Changes)

**Requirements**:
- Support older assembly programs
- Format/tag notation inferred or omitted
- Backward compatibility

**Example**:
```assembly
      LD    VAR              // Short format inferred
LOOP  SLT   5                // No format tag
```

**Characteristics**:
- Simpler syntax (less typing)
- Requires smart defaults
- May be ambiguous in some cases

### Use Case 4: Hybrid/Transitional Syntax

**Requirements**:
- Mix of old and new syntax in same file
- Gradual migration path

**Example**:
```assembly
      LD    . VAR            // New syntax
LOOP  SLT   5                // Old syntax
      BSC   L O DONE         // New syntax
```

---

## Proposed Solution Architecture

### Design Goals

1. **Modularity**: Separate parsing from code generation
2. **Flexibility**: Support multiple syntax formats
3. **Maintainability**: Reduce code duplication
4. **Testability**: Easy to test each component independently
5. **Backward Compatibility**: Don't break existing programs
6. **Forward Compatibility**: Easy to add new syntax variants

### High-Level Architecture

```
┌──────────────────────────────────────────────────────┐
│                   Assembler Entry                    │
└─────────────────────┬────────────────────────────────┘
                      │
                      v
┌──────────────────────────────────────────────────────┐
│              Syntax Detector / Selector              │
│  • Detects format (fixed-column, whitespace, etc.)  │
│  • Selects appropriate parser                       │
└─────────────────────┬────────────────────────────────┘
                      │
                      v
┌──────────────────────────────────────────────────────┐
│                  Parser Factory                      │
│  • Creates appropriate parser instance               │
└─────────────────────┬────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┬────────────────┐
        │             │             │                │
        v             v             v                v
┌─────────────┐ ┌─────────────┐ ┌──────────┐ ┌─────────────┐
│   Fixed     │ │  Whitespace │ │  Legacy  │ │   Hybrid    │
│   Column    │ │   Aware     │ │  Parser  │ │   Parser    │
│   Parser    │ │   Parser    │ │          │ │             │
└─────────────┘ └─────────────┘ └──────────┘ └─────────────┘
        │             │             │                │
        └─────────────┼─────────────┴────────────────┘
                      │
                      v
┌──────────────────────────────────────────────────────┐
│              Intermediate Representation             │
│  • Normalized instruction structure                  │
│  • Label, Opcode, FormatTag, Address, Modifiers     │
└─────────────────────┬────────────────────────────────┘
                      │
                      v
┌──────────────────────────────────────────────────────┐
│               Code Generator                         │
│  • Instruction encoding                              │
│  • Symbol resolution                                 │
│  • Machine code generation                           │
└──────────────────────────────────────────────────────┘
```

### Key Components

#### 1. Intermediate Representation (IR)

A unified structure representing parsed instructions:

```csharp
public class ParsedInstruction
{
    public string Label { get; set; }
    public string Operation { get; set; }
    public FormatTag FormatTag { get; set; }
    public AddressMode AddressMode { get; set; }
    public string Address { get; set; }
    public Dictionary<string, object> Modifiers { get; set; }
    public string Comment { get; set; }
    public int SourceLine { get; set; }
}

public enum FormatTag
{
    Inferred,      // Not specified, needs inference
    Short,         // .
    Long,          // L
    ShortXR1,      // 1
    ShortXR2,      // 2
    ShortXR3,      // 3
    LongXR1,       // L1
    LongXR2,       // L2
    LongXR3,       // L3
    Indirect,      // I
    IndirectXR1,   // I1
    IndirectXR2,   // I2
    IndirectXR3    // I3
}

public enum AddressMode
{
    Direct,
    Indexed,
    Indirect,
    IndirectIndexed,
    Relative
}
```

#### 2. Parser Interface

```csharp
public interface IAssemblyParser
{
    /// <summary>
    /// Parse a line of assembly code into intermediate representation
    /// </summary>
    ParsedInstruction ParseLine(string line, int lineNumber);

    /// <summary>
    /// Detect if this parser can handle the given input
    /// </summary>
    bool CanParse(string input);

    /// <summary>
    /// Parser name/description
    /// </summary>
    string Name { get; }
}
```

#### 3. Format Detection

```csharp
public class SyntaxDetector
{
    public SyntaxFormat DetectFormat(string[] sourceLines)
    {
        // Analyze source to determine format
        // - Check for fixed columns (label always at column 1-5)
        // - Check for format tags (., L, 1-3, I)
        // - Check comment style (* in col 1 vs. //)
        // - Check operand patterns

        int fixedColumnScore = 0;
        int whitespaceScore = 0;
        int legacyScore = 0;

        foreach (var line in sourceLines.Take(10))  // Sample first 10 lines
        {
            // Scoring logic...
        }

        // Return format with highest score
        if (fixedColumnScore > whitespaceScore && fixedColumnScore > legacyScore)
            return SyntaxFormat.FixedColumn;
        else if (whitespaceScore > legacyScore)
            return SyntaxFormat.Whitespace;
        else
            return SyntaxFormat.Legacy;
    }
}

public enum SyntaxFormat
{
    FixedColumn,     // Original IBM 1130 punch card format
    Whitespace,      // Modern flexible whitespace
    Legacy,          // Pre-format-tag syntax
    Hybrid,          // Mixed formats
    Auto             // Auto-detect per line
}
```

#### 4. Concrete Parser Implementations

**Fixed Column Parser**:
```csharp
public class FixedColumnParser : IAssemblyParser
{
    public ParsedInstruction ParseLine(string line, int lineNumber)
    {
        // Column 1-5: Label (trim whitespace)
        // Column 7: Continuation
        // Column 8-12: Operation
        // Column 16-71: Operand
        // Column 72-80: Comment

        if (line.Length < 8)
            return new ParsedInstruction();  // Empty or comment-only

        var instruction = new ParsedInstruction
        {
            SourceLine = lineNumber,
            Label = line.Substring(0, Math.Min(5, line.Length)).Trim(),
            Operation = line.Length >= 12 ?
                line.Substring(7, Math.Min(5, line.Length - 7)).Trim() : "",
            // ... parse operand starting at column 16
        };

        // Parse operand for format tags, address, modifiers
        if (line.Length >= 16)
        {
            string operand = line.Substring(15, Math.Min(56, line.Length - 15));
            ParseOperand(instruction, operand);
        }

        return instruction;
    }
}
```

**Whitespace-Aware Parser**:
```csharp
public class WhitespaceAwareParser : IAssemblyParser
{
    public ParsedInstruction ParseLine(string line, int lineNumber)
    {
        // Handle comments
        int commentPos = line.IndexOf("//");
        string comment = "";
        if (commentPos >= 0)
        {
            comment = line.Substring(commentPos);
            line = line.Substring(0, commentPos);
        }

        // Split on whitespace
        var parts = line.Split(new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return new ParsedInstruction { Comment = comment };

        int partIndex = 0;
        var instruction = new ParsedInstruction { SourceLine = lineNumber };

        // Label (starts without indent)
        if (line.Length > 0 && !char.IsWhiteSpace(line[0]))
        {
            instruction.Label = parts[partIndex++];
        }

        // Operation
        if (partIndex < parts.Length)
        {
            instruction.Operation = parts[partIndex++].ToUpper();
        }

        // Operand (everything remaining)
        if (partIndex < parts.Length)
        {
            string operand = string.Join(" ", parts.Skip(partIndex));
            ParseOperand(instruction, operand);
        }

        instruction.Comment = comment;
        return instruction;
    }
}
```

**Legacy Parser** (for backward compatibility):
```csharp
public class LegacyParser : IAssemblyParser
{
    public ParsedInstruction ParseLine(string line, int lineNumber)
    {
        // Similar to WhitespaceAwareParser but:
        // - Infer format tags (default to short)
        // - Support old syntax (LD VAR,X1 → LD 1 VAR)
        // - Convert to IR with Inferred format tag

        var instruction = new ParsedInstruction { SourceLine = lineNumber };

        // ... parse similar to WhitespaceAwareParser ...

        // Detect old-style indexing (VAR,X1)
        if (operand.Contains(",X"))
        {
            instruction.FormatTag = ConvertOldIndexSyntax(operand);
        }
        else
        {
            instruction.FormatTag = FormatTag.Inferred;  // Will be inferred later
        }

        return instruction;
    }
}
```

#### 5. Operand Parser (Shared Utility)

```csharp
public class OperandParser
{
    /// <summary>
    /// Parse operand into format tag, address, and modifiers
    /// </summary>
    public static void ParseOperand(ParsedInstruction instruction, string operand)
    {
        if (string.IsNullOrWhiteSpace(operand))
            return;

        var parts = operand.Trim().Split(new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        int partIndex = 0;

        // Try to parse format tag
        if (partIndex < parts.Length)
        {
            string firstPart = parts[partIndex];

            if (IsFormatTag(firstPart))
            {
                instruction.FormatTag = ParseFormatTag(firstPart);
                partIndex++;
            }
            else
            {
                instruction.FormatTag = FormatTag.Inferred;
            }
        }

        // Remaining parts are instruction-specific
        if (partIndex < parts.Length)
        {
            instruction.Address = parts[partIndex];
            partIndex++;
        }

        // Any remaining parts go into Modifiers dictionary
        while (partIndex < parts.Length)
        {
            string modifier = parts[partIndex++];
            // Parse modifier (e.g., "C" for carry, "O" for overflow)
            instruction.Modifiers[modifier] = true;
        }
    }

    private static bool IsFormatTag(string token)
    {
        return token == "." || token == "L" ||
               token == "1" || token == "2" || token == "3" ||
               token == "L1" || token == "L2" || token == "L3" ||
               token == "I" || token == "I1" || token == "I2" || token == "I3";
    }

    private static FormatTag ParseFormatTag(string token)
    {
        return token switch
        {
            "." => FormatTag.Short,
            "L" => FormatTag.Long,
            "1" => FormatTag.ShortXR1,
            "2" => FormatTag.ShortXR2,
            "3" => FormatTag.ShortXR3,
            "L1" => FormatTag.LongXR1,
            "L2" => FormatTag.LongXR2,
            "L3" => FormatTag.LongXR3,
            "I" => FormatTag.Indirect,
            "I1" => FormatTag.IndirectXR1,
            "I2" => FormatTag.IndirectXR2,
            "I3" => FormatTag.IndirectXR3,
            _ => FormatTag.Inferred
        };
    }
}
```

#### 6. Code Generator (Unchanged)

The code generator works with IR, so it doesn't care about input format:

```csharp
public class CodeGenerator
{
    public ushort[] GenerateInstruction(ParsedInstruction instruction)
    {
        // Resolve symbols
        // Encode instruction based on opcode and format
        // Return machine code words

        switch (instruction.Operation)
        {
            case "LD":
                return EncodeLoadStore(instruction, 0x18);
            case "SLT":
                return EncodeShift(instruction, 0x02, 2);
            // ... etc
        }
    }

    private ushort[] EncodeShift(ParsedInstruction instruction, byte opcode, byte shiftType)
    {
        // Parse shift count from address field
        if (!byte.TryParse(instruction.Address, out byte count))
            throw new AssemblyException($"Invalid shift count: {instruction.Address}");

        // Validate format (shifts are always short)
        if (instruction.FormatTag != FormatTag.Short &&
            instruction.FormatTag != FormatTag.Inferred)
        {
            throw new AssemblyException($"Shift instructions must use short format (.)");
        }

        // Encode instruction
        ushort encoded = (ushort)((opcode << 11) | (shiftType << 6) | (count & 0x3F));
        return new[] { encoded };
    }
}
```

### Benefits of This Architecture

1. **Separation of Concerns**:
   - Parsing separated from code generation
   - Each parser handles one format style
   - Shared operand parsing utilities

2. **Easy to Add New Formats**:
   - Implement IAssemblyParser interface
   - Add to factory
   - No changes to code generator

3. **Testable**:
   - Test parsers independently
   - Test code generator with mock IR
   - Test round-trip (parse → generate → disassemble)

4. **Maintainable**:
   - Each parser is small and focused
   - Shared utilities reduce duplication
   - Clear data flow

5. **Format Conversion**:
   - Parse with one parser
   - Output IR
   - Generate code OR re-serialize with different parser
   - **Enables format conversion tool**: Fixed-column ↔ Whitespace ↔ Legacy

---

## Implementation Strategy

### Phase 1: Foundation (Week 1)

**Goal**: Establish IR and shared utilities without breaking existing code

1. **Create IR classes** (2 hours):
   - `ParsedInstruction`
   - `FormatTag` enum
   - `AddressMode` enum

2. **Create `IAssemblyParser` interface** (1 hour)

3. **Extract shared operand parsing** (3 hours):
   - Create `OperandParser` utility class
   - Move format tag parsing logic
   - Add unit tests

4. **Add tests for IR** (2 hours):
   - Test ParsedInstruction creation
   - Test FormatTag conversion
   - Test OperandParser

**Deliverables**:
- New namespace: `S1130.SystemObjects.Parsing`
- Classes: `ParsedInstruction`, `IAssemblyParser`, `OperandParser`
- 20+ unit tests

### Phase 2: Refactor Existing Parser (Week 2)

**Goal**: Refactor current assembler to use IR internally

1. **Create `WhitespaceAwareParser`** (4 hours):
   - Implements `IAssemblyParser`
   - Wraps existing logic from AssemblyLine.cs
   - Returns `ParsedInstruction`

2. **Refactor `Assembler.cs` to consume IR** (6 hours):
   - Update Pass2 to work with `ParsedInstruction`
   - Update instruction processors to use IR
   - Maintain backward compatibility

3. **Fix identified bugs** (4 hours):
   - Fix BSS E parsing (Issue 1)
   - Fix shift instruction format tag support (Issue 2)
   - Add regression tests

4. **Integration testing** (2 hours):
   - Run all existing tests
   - Test with example programs
   - Verify no regressions

**Deliverables**:
- Updated `Assembler.cs`
- `WhitespaceAwareParser` implementation
- Bug fixes for Issues 1 and 2
- All existing tests passing

### Phase 3: Add Alternative Parsers (Week 3)

**Goal**: Support multiple input formats

1. **Implement `FixedColumnParser`** (6 hours):
   - Column-based parsing
   - Punch card format support
   - Unit tests

2. **Implement `LegacyParser`** (4 hours):
   - Old syntax support
   - Format tag inference
   - Unit tests

3. **Implement `SyntaxDetector`** (3 hours):
   - Auto-detect format
   - Format scoring algorithm
   - Unit tests

4. **Implement `ParserFactory`** (2 hours):
   - Select parser based on format
   - Configuration options

**Deliverables**:
- `FixedColumnParser` class
- `LegacyParser` class
- `SyntaxDetector` class
- `ParserFactory` class
- 50+ unit tests

### Phase 4: Format Conversion Tool (Week 4)

**Goal**: Enable conversion between formats

1. **Create IR serializers** (4 hours):
   - `FixedColumnSerializer`
   - `WhitespaceSerializer`
   - `LegacySerializer`

2. **Create CLI conversion tool** (3 hours):
   - Read input format
   - Parse to IR
   - Serialize to output format
   - Command-line interface

3. **Documentation** (3 hours):
   - Usage guide for conversion tool
   - Format specification document
   - Migration guide

4. **End-to-end testing** (2 hours):
   - Convert sample programs
   - Round-trip testing
   - Validation

**Deliverables**:
- Format conversion CLI tool
- Comprehensive documentation
- Sample programs in multiple formats

### Phase 5: Polish and Performance (Week 5)

**Goal**: Optimize and polish

1. **Performance optimization** (4 hours):
   - Profile parser performance
   - Optimize hot paths
   - Benchmark against original

2. **Error message improvements** (3 hours):
   - Better error context
   - Suggested fixes
   - Format-specific messages

3. **Integration with web frontend** (3 hours):
   - Format selector dropdown
   - Live format conversion
   - Syntax highlighting

4. **Final documentation and cleanup** (2 hours):
   - API documentation
   - Architecture guide
   - Code cleanup

**Deliverables**:
- Performance benchmarks
- Improved error messages
- Web UI enhancements
- Final documentation

---

## Testing Plan

### Unit Tests

```csharp
// Test IR
public class ParsedInstructionTests
{
    [Fact]
    public void ParsedInstruction_DefaultValues() { }

    [Fact]
    public void ParsedInstruction_SetProperties() { }
}

// Test OperandParser
public class OperandParserTests
{
    [Theory]
    [InlineData(". VAR", FormatTag.Short, "VAR")]
    [InlineData("L /2000", FormatTag.Long, "/2000")]
    [InlineData("1 OFFSET", FormatTag.ShortXR1, "OFFSET")]
    [InlineData("I PTR", FormatTag.Indirect, "PTR")]
    public void ParseOperand_FormatAndAddress(string operand, FormatTag expectedFormat, string expectedAddress)
    {
        var instruction = new ParsedInstruction();
        OperandParser.ParseOperand(instruction, operand);

        Assert.Equal(expectedFormat, instruction.FormatTag);
        Assert.Equal(expectedAddress, instruction.Address);
    }

    [Fact]
    public void ParseOperand_WithModifiers()
    {
        // Test: ". O LOOP" → format=Short, modifier=O, address=LOOP
    }
}

// Test WhitespaceAwareParser
public class WhitespaceAwareParserTests
{
    [Fact]
    public void ParseLine_WithLabel() { }

    [Fact]
    public void ParseLine_WithoutLabel() { }

    [Fact]
    public void ParseLine_WithComment() { }

    [Theory]
    [InlineData("LOOP  SLT  . 5", "LOOP", "SLT", FormatTag.Short, "5")]
    [InlineData("      LD   L /2000", "", "LD", FormatTag.Long, "/2000")]
    public void ParseLine_CompleteInstructions(string line, string label, string op, FormatTag format, string addr)
    {
        var parser = new WhitespaceAwareParser();
        var instruction = parser.ParseLine(line, 1);

        Assert.Equal(label, instruction.Label);
        Assert.Equal(op, instruction.Operation);
        Assert.Equal(format, instruction.FormatTag);
        Assert.Equal(addr, instruction.Address);
    }
}

// Test FixedColumnParser
public class FixedColumnParserTests
{
    [Fact]
    public void ParseLine_FixedColumns()
    {
        // Label in cols 1-5, operation in 8-12, operand in 16+
        string line = "LOOP SLT   . 5              Comment";
        var parser = new FixedColumnParser();
        var instruction = parser.ParseLine(line, 1);

        Assert.Equal("LOOP", instruction.Label);
        Assert.Equal("SLT", instruction.Operation);
        Assert.Equal(FormatTag.Short, instruction.FormatTag);
        Assert.Equal("5", instruction.Address);
    }
}

// Test SyntaxDetector
public class SyntaxDetectorTests
{
    [Fact]
    public void DetectFormat_FixedColumn()
    {
        string[] lines = {
            "LABEL LD    L /2000",
            "LOOP  SLT   . 5    ",
            "      BSC   L O DONE"
        };

        var detector = new SyntaxDetector();
        var format = detector.DetectFormat(lines);

        Assert.Equal(SyntaxFormat.FixedColumn, format);
    }

    [Fact]
    public void DetectFormat_Whitespace()
    {
        string[] lines = {
            "LABEL LD L /2000 // Comment",
            "LOOP SLT . 5     // Another comment",
            "      BSC L O DONE"
        };

        var detector = new SyntaxDetector();
        var format = detector.DetectFormat(lines);

        Assert.Equal(SyntaxFormat.Whitespace, format);
    }
}
```

### Integration Tests

```csharp
public class AssemblerIntegrationTests
{
    [Fact]
    public void Assemble_WithWhitespaceFormat_Success()
    {
        string[] source = {
            "      ORG  /100",
            "START LDD  L ONE",
            "LOOP  SLT  . 1",
            "      BSC  L LOOP,C",
            "      BSC  L START",
            "ONE   DC   0",
            "      DC   1"
        };

        var assembler = new Assembler(new Cpu());
        var result = assembler.Assemble(source);

        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Equal(7, result.InstructionCount);
    }

    [Fact]
    public void Assemble_FixedColumn_ProducesSameBinary()
    {
        // Same program in fixed-column format should produce identical binary
    }
}
```

### End-to-End Tests

```csharp
public class FormatConversionTests
{
    [Fact]
    public void RoundTrip_WhitespaceToFixedToWhitespace()
    {
        // Parse whitespace → IR → serialize to fixed-column → parse → serialize to whitespace
        // Result should be semantically equivalent
    }

    [Fact]
    public void RoundTrip_Assemble_Disassemble_Reassemble()
    {
        // Assemble → Disassemble → Reassemble
        // Binary should be identical
    }
}
```

---

## Migration Strategy

### For Existing Code

1. **Phase 1-2**: Maintain full backward compatibility
   - No breaking changes to API
   - Existing programs continue to work

2. **Phase 3**: Introduce new parsers as opt-in
   - Add configuration option to select parser
   - Default to current behavior

3. **Phase 4**: Deprecation warnings
   - Warn about old syntax usage
   - Suggest new syntax in error messages

4. **Phase 5**: New parser as default
   - Switch default to new architecture
   - Keep legacy parser available with flag

### For Users

1. **Documentation**: Clear migration guide
2. **Tooling**: Automatic conversion tool
3. **Examples**: Updated example programs
4. **Gradual**: Support both formats during transition

---

## Collaboration with Parallel Work

### Awareness of Forks

From notes.txt:
> While there is a fork of this repo (in order to port it to Rust), we do not want to fork this work into two competing C# repos.

### Coordination Strategy

1. **Communication**:
   - Document design decisions in this file
   - Share with Bob Flanders and other contributors
   - Discuss approach before major refactoring

2. **Incremental Changes**:
   - Small, focused PRs
   - Easy to review and merge
   - Minimize merge conflicts

3. **Feature Flags**:
   - New parsers behind configuration flags
   - Easy to enable/disable
   - Reduces risk of conflicts

4. **API Stability**:
   - Keep public API stable
   - Internal refactoring only
   - Document breaking changes clearly

5. **Testing**:
   - Comprehensive test suite
   - Catch regressions early
   - Validate compatibility

---

## Immediate Action Items

### High Priority (This Week)

1. **Fix BSS E parsing bug** ✅ IDENTIFIED
   - Update Assembler.cs lines 822-827
   - Add test cases
   - Create PR

2. **Fix shift instruction format tag support** ✅ IDENTIFIED
   - Update ProcessShift method
   - Support both `. 5` and `5` syntax
   - Add test cases
   - Create PR

3. **Document current state**
   - Update README with known issues
   - Document new syntax requirements
   - Create migration guide

### Medium Priority (Next 2 Weeks)

4. **Create IR classes**
   - Implement ParsedInstruction
   - Implement OperandParser utility
   - Add unit tests

5. **Refactor current parser to use IR**
   - Update Assembler.cs internally
   - Maintain backward compatibility
   - Verify all tests pass

6. **Add format detection**
   - Implement SyntaxDetector
   - Auto-detect input format
   - Add configuration option

### Lower Priority (Future)

7. **Implement alternative parsers**
   - FixedColumnParser
   - LegacyParser
   - HybridParser

8. **Format conversion tool**
   - CLI tool for format conversion
   - Integration with web UI
   - Documentation

9. **Performance optimization**
   - Profile and optimize
   - Benchmark against original
   - Document performance characteristics

---

## References

### Documentation

- `docs/s1130-research.md` - IBM 1130 specifications
- `docs/process.md` - Development process and TDD guidelines
- `docs/improvements.md` - General emulator improvements
- `docs/notes.txt` - Email discussion about assembler changes

### Source Files

- `src/S1130.SystemObjects/Assembler.cs` - Main assembler implementation
- `src/S1130.SystemObjects/AssemblyLine.cs` - Line parsing
- `src/S1130.SystemObjects/Instructions/` - Instruction implementations

### Test Files

- `tests/UnitTests.S1130.SystemObjects/AssemblerTests.cs` - Assembler tests
- `tests/UnitTests.S1130.SystemObjects/RoundTripTests.cs` - Round-trip tests

### External References

- IBM 1130 Functional Characteristics (A26-5881-2)
- IBM 1130 Assembler Language Manual (C26-5927-2)

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-24 | Development Team | Initial analysis after Playwright testing |

---

**End of Document**
