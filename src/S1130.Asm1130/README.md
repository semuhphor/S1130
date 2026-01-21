# IBM 1130 Cross-Assembler (C# Port)

## Overview

This is a C# port of Brian Knittel's `asm1130.c` IBM 1130 cross-assembler. It assembles IBM 1130 assembly language programs in the original format (fixed-column or tab-delimited).

## Project Status

**⚠️ IN DEVELOPMENT** - Core architecture complete, needs additional opcodes

### ✅ Completed
- Two-pass assembler architecture
- Symbol table with forward reference resolution
- Expression evaluator (arithmetic, relocatable expressions)
- Column-format and tab-delimited input parsing
- Load format (`.out`) output
- Listing file generation with cross-references
- Core opcodes: LD, ST, ADD, SUB, MUL, DIV, AND, OR, EOR, BSC, BSI, etc.
- Assembler directives: DC, BSS, EQU, ORG, END, ABS, REL

### 🚧 In Progress  
- Additional opcodes (STO, XIO, and 40+ more)
- Binary card format (`.bin`) output
- Expression parser improvements (comma handling)
- Complete opcode table
- SBRK and DMS-specific features

### ⏳ Not Yet Implemented
- Binary output format
- Floating-point constant handling
- Macro support
- Conditional assembly
- Library functions (LIBF/CALL)

## Usage

```bash
# Assemble a file to load format
dotnet run --project src/S1130.Asm1130 -- input.asm

# Create listing file
dotnet run --project src/S1130.Asm1130 -- -l input.asm

# Binary format output
dotnet run --project src/S1130.Asm1130 -- -b input.asm

# Verbose mode
dotnet run --project src/S1130.Asm1130 -- -v -l input.asm
```

### Command Line Options

```
-b  Binary (relocatable format) output; default is simulator LOAD format
-d  Interpret % and < as ( and ), for assembling DMS sources
-p  Count passes required; no assembly output
-s  Add symbol table to listing
-v  Verbose mode
-w  Write system symbol table as SYMBOLS.SYS
-W  Same as -w but without confirmation prompt
-x  Add cross reference table to listing
-y  Preload system symbol table SYMBOLS.SYS
-8  Enable IBM 1800 instructions
-o  Set output file
-l  Create listing file
```

## Input Format

### Column Format (Strict)
```
         1         2         3         4
12345678901234567890123456789012345678901234567890
                    *COMMENT
                    LABEL OP   T X ARGUMENT
                          DC     /0000
                          LD   L   ADDR
```

- Label: Columns 21-25 (5 characters max)
- Opcode: Columns 27-30 (4 characters)
- Tag: Column 32
- Index: Column 33
- Arguments: Columns 35-72

### Tab-Delimited Format (Loose)
```
LABEL<TAB>OP<TAB>MODS<TAB>ARGS
INIT<TAB>DC<TAB><TAB>0
<TAB>LD<TAB>L<TAB>ADDR
```

## Integration with S1130 Project

This assembler is designed to work with your modern S1130 format via the `asmconv` converter:

```
S1130 Format (modern) 
    ↓ (asmconv)
IBM 1130 Format 
    ↓ (asm1130)
Assembled Output (.out or .bin)
```

This allows testing against the known-good reference implementation while keeping your modern development format.

## Architecture

- **Program.cs**: Main entry point
- **Assembler.cs**: Core assembler class and configuration
- **Assembler.Parser.cs**: Input file parsing (column/tab formats)
- **Assembler.Symbols.cs**: Symbol table and cross-reference management
- **Assembler.Expressions.cs**: Expression evaluator
- **Assembler.Output.cs**: Output file writers
- **Assembler.Opcodes.cs**: Opcode table and instruction encoders
- **Types.cs**: Data structures (Symbol, Expression, Opcode, etc.)

## Next Steps

1. Add remaining ~50 opcodes from original asm1130.c
2. Implement binary card format output
3. Improve expression parser for complex argument forms
4. Add comprehensive unit tests
5. Compare output byte-for-byte with original asm1130.exe

## Original Credit

Based on ASM1130 V1.22 by Brian Knittel
Original C source: 4,702 lines
C# Port: ~1,200 lines (and growing)
