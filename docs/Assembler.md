# S1130 IBM 1130 Assembler Documentation

## Table of Contents
1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Assembly Language Syntax](#assembly-language-syntax)
4. [Directives](#directives)
5. [Instructions](#instructions)
6. [Addressing Modes](#addressing-modes)
7. [Expressions and Symbols](#expressions-and-symbols)
8. [Two-Pass Assembly](#two-pass-assembly)
9. [Error Handling](#error-handling)
10. [Usage Examples](#usage-examples)
11. [Assembler API](#assembler-api)
12. [Implementation Details](#implementation-details)

---

## Overview

The S1130 Assembler is a complete implementation of the IBM 1130 assembly language, capable of translating human-readable assembly code into executable machine code for the S1130 emulator.

**Note:** The S1130 is a **functional emulator**, not a cycle-accurate emulator. While it faithfully reproduces the logical behavior of IBM 1130 programs, it does not simulate instruction timing at the cycle level. Programs will produce correct results but execute much faster than on original hardware.

**Features:**
- Full IBM 1130 instruction set support
- Short and long format instructions
- Direct, indirect, and indexed addressing modes
- Symbolic labels and forward references
- Assembler directives (ORG, DC, EQU, BSS, BES)
- Expression evaluation with current address operator (*)
- Two-pass assembly for symbol resolution
- Comprehensive error reporting with line numbers
- Formatted assembly listing output

**Design Philosophy:**
- Faithful to IBM 1130 assembly syntax
- Clear error messages for debugging
- Efficient two-pass algorithm
- Integrated with CPU emulator

---

## Quick Start

### Basic Usage

```csharp
using S1130.SystemObjects;

var cpu = new Cpu();

var source = @"
      ORG  /100
START LD   L VALUE
      A    L VALUE
      STO  L RESULT
      WAIT
VALUE DC   /0042
RESULT DC  0
";

var result = cpu.Assemble(source);

if (result.Success)
{
    Console.WriteLine("Assembly successful!");
    Console.WriteLine("Listing:");
    foreach (var line in result.Listing)
    {
        Console.WriteLine(line);
    }
    
    // Execute the program
    cpu.Iar = 0x100;
    while (!cpu.Wait)
    {
        cpu.NextInstruction();
        cpu.ExecuteInstruction();
    }
    
    Console.WriteLine($"Result: {cpu[result.Symbols["RESULT"]]:X4}");
}
else
{
    Console.WriteLine("Assembly errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Line {error.LineNumber}: {error.Message}");
    }
}
```

### Assembly Result Structure

```csharp
public class AssemblyResult
{
    public bool Success { get; set; }
    public List<string> Listing { get; set; }
    public List<AssemblyError> Errors { get; set; }
    public Dictionary<string, ushort> Symbols { get; set; }
}

public class AssemblyError
{
    public int LineNumber { get; set; }
    public string Message { get; set; }
}
```

---

## Assembly Language Syntax

### Source Line Format

IBM 1130 assembly uses a columnar format (originally for punched cards):

```
[LABEL] [OPERATION] [OPERAND] [COMMENT]
```

**Field Rules:**
- **Label**: Optional, starts in column 1-5, must start with letter
- **Operation**: Required (unless comment line), starts in column 6+
- **Operand**: Required for most operations, follows operation
- **Comment**: Optional, separated by whitespace

**Important:** Fields are whitespace-separated, not column-based in this implementation.

### Examples

```
* This is a comment line (starts with asterisk)

LOOP  LD   L VALUE    Load VALUE into accumulator
      A    L ONE      Add ONE to accumulator
      STO  L VALUE    Store back to VALUE
      MDX  LOOP,-1    Decrement and branch to LOOP
      WAIT            Halt execution

VALUE DC   5          Define constant 5
ONE   DC   1          Define constant 1
```

### Case Sensitivity

- **Instructions**: Case-insensitive (LD, ld, Ld all equivalent)
- **Labels**: Case-insensitive (LOOP, loop, Loop all same)
- **Hex values**: Case-insensitive (/FF, /ff, /Ff all same)

---

## Directives

### ORG - Set Origin

Sets the starting address for code generation.

**Syntax:** `ORG /address`

**Rules:**
- Must appear before any instructions or data
- Address must be in hexadecimal with leading slash
- Address must be within memory bounds (0-7FFF for 32K)

**Examples:**
```
      ORG  /100        Start at address 0x0100
      ORG  /0000       Start at address 0x0000
      ORG  /7F00       Start at address 0x7F00
```

**Errors:**
```
      ORG  100         ERROR: Missing leading /
      ORG  /ZZZZ       ERROR: Invalid hex digits
      ORG  /8000       ERROR: Address beyond memory
```

### DC - Define Constant

Defines a single 16-bit constant in memory.

**Syntax:** `[label] DC value`

**Value Formats:**
- Decimal: `DC 100`
- Hexadecimal: `DC /FF`
- Symbol: `DC LABEL`

**Examples:**
```
NUM1  DC   42          Decimal constant
NUM2  DC   /002A       Hex constant (same as 42)
PTR   DC   BUFFER      Address of BUFFER label
```

**Errors:**
```
      DC   65536       ERROR: Value too large (>16 bits)
      DC   /GGGG       ERROR: Invalid hex
      DC   UNDEF       ERROR: Undefined symbol
```

### EQU - Equate Symbol

Assigns a value to a symbol without allocating memory.

**Syntax:** `label EQU value`

**Uses:**
- Define constants
- Create aliases for addresses
- Define bit patterns or masks

**Examples:**
```
* Define constants
BUFSIZE EQU  80        Buffer size
MAXVAL  EQU  /FFFF     Maximum value

* Define addresses
IOADDR  EQU  /0300     I/O control address
INTRPT  EQU  /0400     Interrupt handler

* Use in code
      LD   L MAXVAL    Load the constant
```

**Special Expressions:**
```
HERE  EQU  *           Current address
SIZE  EQU  END-START   Difference of labels
```

### BSS - Block Started by Symbol

Reserves a block of uninitialized storage.

**Syntax:** `[label] BSS [E] count`

**Options:**
- `E`: Align to even address before allocating
- `count`: Number of words to reserve (decimal, hex, or symbol)

**Examples:**
```
* Reserve 80 words
BUFFER BSS 80

* Reserve 10 words, aligned to even address
TABLE  BSS E 10

* Reserve space using symbol
       EQU  SIZE EQU 100
DATA   BSS  SIZE

* Using hex count
AREA   BSS  /0050      Reserve 80 (0x50) words
```

**Alignment Example:**
```
      ORG  /101        Start at odd address 0x101
VAL1  DC   5           At address 0x101
BUF   BSS  E 10        Aligned to 0x102, reserves 0x102-0x10B
VAL2  DC   6           At address 0x10C
```

### BES - Block Ended by Symbol

Reserves a block of storage, with label pointing to the LAST word.

**Syntax:** `[label] BES [E] count`

**Difference from BSS:**
- BSS: Label points to FIRST word
- BES: Label points to LAST word

**Examples:**
```
      ORG  /100
START BSS  10          START = 0x100 (first word)
                       Reserved: 0x100-0x109
      
      ORG  /200  
END   BES  10          END = 0x209 (last word)
                       Reserved: 0x200-0x209
```

**Use Case:**
BES is useful for creating stacks or downward-growing data structures:

```
      ORG  /1000
STACK BES  100         Stack grows downward from 0x1063
                       STACK points to bottom (0x1063)
```

---

## Instructions

### Instruction Format

All instructions follow the pattern:

```
[label] OPERATION [format] operand [,Xn] [I]
```

**Format Specifiers:**
- `.`: Short format (explicit, IAR-relative ±127 words)
- `L`: Long format (absolute 16-bit address)
- (none): Auto-detect based on operand

**Other Modifiers:**
- `,X1`, `,X2`, `,X3`: Index register
- `I`: Indirect addressing

### Load/Store Instructions

#### LD - Load Accumulator

```
LD   . operand       Short format (explicit)
LD   L operand       Long format (explicit)
LD   operand,X1      With index register
LD   L operand I     Long format, indirect
```

**Examples:**
```
      LD   . 5         Load from IAR+5 (short format, explicit)
      LD   . -3        Load from IAR-3 (short format, explicit)
      LD   L /0400     Load from address 0x0400 (long format)
      LD   L VALUE     Load from VALUE address (long format)
      LD   . TABLE,X1  Load from TABLE+XR1 (short format)
      LD   L PTR I     Load from address pointed to by PTR (long, indirect)
```

**Note:** The dot (`.`) format specifier makes the short format explicit, matching the fixed-column card format used by the original IBM 1130 assembler where column position indicated format.

#### STO - Store Accumulator

```
STO  operand         Store accumulator to memory
```

**Examples:**
```
      STO  L RESULT    Store to RESULT
      STO  TABLE,X1    Store to TABLE+XR1
```

#### LDD - Load Double

Loads Acc:Ext from two consecutive memory locations.

```
      LDD  L VALUE     Acc = VALUE, Ext = VALUE+1
```

#### STD - Store Double

Stores Acc:Ext to two consecutive memory locations.

```
      STD  L RESULT    RESULT = Acc, RESULT+1 = Ext
```

#### LDX - Load Index

Loads an index register from memory.

```
      LDX  1 VALUE     Load XR1 from VALUE
      LDX  2 /0400     Load XR2 from address 0x0400
```

#### STX - Store Index

Stores an index register to memory.

```
      STX  1 SAVE      Store XR1 to SAVE
```

#### LDS - Load Status

Loads accumulator with CPU status flags.

```
      LDS  0           Load status to accumulator
```

#### STS - Store Status

Stores accumulator to CPU status flags.

```
      STS  0           Store accumulator to status
```

### Arithmetic Instructions

#### A - Add

```
      A    L VALUE     Acc = Acc + VALUE
```

Sets Carry and Overflow flags.

#### S - Subtract

```
      S    L VALUE     Acc = Acc - VALUE
```

Sets Carry and Overflow flags.

#### M - Multiply

```
      M    L VALUE     Acc:Ext = Acc * VALUE (32-bit result)
```

Result: High 16 bits in Acc, low 16 bits in Ext.

#### D - Divide

```
      D    L VALUE     Acc = Acc:Ext / VALUE
                       Ext = Acc:Ext % VALUE (remainder)
```

#### AD - Add Double

```
      AD   L VALUE     Acc:Ext = Acc:Ext + VALUE:VALUE+1
```

#### SD - Subtract Double

```
      SD   L VALUE     Acc:Ext = Acc:Ext - VALUE:VALUE+1
```

### Logical Instructions

#### AND - Logical AND

```
      AND  L VALUE     Acc = Acc AND VALUE
```

Clears Carry and Overflow.

#### OR - Logical OR

```
      OR   L VALUE     Acc = Acc OR VALUE
```

#### EOR - Exclusive OR

```
      EOR  L VALUE     Acc = Acc XOR VALUE
```

### Shift Instructions

#### SLA - Shift Left Accumulator

```
      SLA  count       Shift Acc left by count bits
```

Count: 0-63 (bits 10-15 of instruction)

#### SRA - Shift Right Accumulator

```
      SRA  count       Shift Acc right by count bits
```

#### SLT - Shift Left and Count

Complex shift operation with counting.

```
      SLT  count       Shift left, count leading zeros
```

#### SRT - Shift Right and Count

```
      SRT  count       Shift right, count leading ones
```

#### SLC - Shift Left Acc:Ext

```
      SLC  count       Shift Acc:Ext left as 32-bit value
```

#### SLCA - Shift Left Acc:Ext Arithmetic

```
      SLCA count       Shift Acc:Ext left, preserve sign
```

### Branch Instructions

#### BSC - Branch or Skip on Condition

```
      BSC  [L] [condition][,Xn] target [I]
```

**Format:**
- `L` - Long format (optional, default is short)
- `condition` - Condition codes (optional, unconditional if omitted)
- `,Xn` - Index register 1, 2, or 3 (optional)
- `target` - Branch/skip target address or label
- `I` - Indirect addressing (optional, at end)

**Condition Codes (inverse conditions - branches/skips when condition is FALSE):**
- `O` - Overflow OFF (branches if overflow is NOT set)
- `C` - Carry OFF (branches if carry is NOT set)
- `E` - Even (branches if ACC bit 15 = 0)
- `+` or `&` or `P` - Positive (branches if ACC > 0)
- `-` or `M` - Negative (branches if ACC < 0)
- `Z` - Zero (branches if ACC = 0)

Multiple conditions can be combined: `ZPM` means "branch if zero, positive, or negative" (always branches).

**Behavior:**
- **Short format**: Skip next instruction if condition is TRUE
- **Long format**: Branch to target if condition is TRUE

**Examples:**
```
      BSC  O LOOP      Skip if overflow OFF
      BSC  L O LOOP    Branch if overflow OFF
      BSC  ZPM ALWAYS  Always skip (any value matches)
      BSC  +,2 NEXT    Skip if ACC positive, indexed by XR2
      BSC  L C DONE I  Branch if carry OFF, indirect
      BSC  TARGET      Unconditional skip
      BSC  L TARGET    Unconditional branch
```

#### BSI - Branch and Store IAR

Subroutine call: stores return address and branches.

```
      BSI  L SUBR      Call subroutine at SUBR
```

Stores next IAR at SUBR, branches to SUBR+1.

**Subroutine Pattern:**
```
SUBR  DC   0           Return address saved here
      ...              Subroutine code
      BSC  L SUBR I    Return via indirect branch
```

#### MDX - Modify Index and Skip

Increments/decrements index register and conditionally branches.

```
      MDX  target,modifier
```

**Modifier:**
- Positive: Test index register
- Negative: Decrement and test

**Loop Example:**
```
      LDX  1 /0010     Load counter = 16
LOOP  ...              Loop body
      MDX  LOOP,-1     Decrement XR1, loop if not zero
```

### I/O Instruction

#### XIO - Execute I/O

Executes an I/O operation via IOCC structure.

```
      XIO  L IOCC      Execute I/O command at IOCC
```

**IOCC Structure:**
```
IOCC  DC   BUFFER      Word count address (WCA)
      DC   /0900       Device code + function + modifiers
```

**Example - Read Card:**
```
      XIO  L RIOCC     Initiate card read

RIOCC DC   CARDBUF     Buffer address
      DC   /0950       Device 01, InitRead, 80 words

CARDBUF BSS 80         80-word buffer
```

### Control Instruction

#### WAIT - Wait State

Halts CPU execution.

```
      WAIT             Stop execution
```

Used to end programs or wait for interrupts.

---

## Addressing Modes

### 1. Short Format (Displacement)

**Syntax:** `instruction displacement`

**Calculation:** `EffectiveAddress = IAR + SignExtend(displacement)`

**Range:** -128 to +127 relative to current IAR

**Examples:**
```
      ORG  /100
      LD   5           Load from 0x105 (IAR=0x100, disp=5)
      A    -3          Add from 0x0FD (IAR=0x101, disp=-3)
      STO  10          Store to 0x10B (IAR=0x102, disp=10)
```

**Sign Extension:**
- Displacement 5 (0x05): +5
- Displacement 253 (0xFD): -3 (sign extended to 0xFFFD)
- Displacement 128 (0x80): -128
- Displacement 127 (0x7F): +127

### 2. Long Format (Absolute Address)

**Syntax:** `instruction L address`

**Calculation:** `EffectiveAddress = address`

**Range:** 0x0000 to 0xFFFF

**Examples:**
```
      LD   L /0400     Load from address 0x0400
      LD   L VALUE     Load from VALUE address
      STO  L RESULT    Store to RESULT address
```

### 3. Indexed Addressing

**Syntax:** `instruction operand,Xn` (where n = 1, 2, or 3)

**Calculation:** 
```
BaseAddress = (short format: IAR + disp) or (long format: address)
EffectiveAddress = BaseAddress + IndexRegister[n]
```

**Examples:**
```
* Array access
      LDX  1 /0000     XR1 = 0 (index)
LOOP  LD   TABLE,X1    Load TABLE[XR1]
      ...
      MDX  LOOP,1      Increment XR1, loop

TABLE DC   10          Array elements...
      DC   20
      DC   30
```

**Two-dimensional Arrays:**
```
* Access ARRAY[row][col]
      LD   L ROW       Get row offset
      M    L NCOLS     Multiply by columns per row
      STO  XR1         Store in XR1
      LD   L COL       Get column offset
      A    XR1         Add row offset
      STO  XR1         XR1 = row*NCOLS + col
      LD   ARRAY,X1    Load ARRAY[row][col]
```

### 4. Indirect Addressing

**Syntax:** `instruction L address I`

**Calculation:**
```
PointerAddress = address (+ index if specified)
EffectiveAddress = Memory[PointerAddress]
```

**Examples:**
```
* Simple indirect
      LD   L PTR I     Load from address stored in PTR

* Indirect with indexing
      LD   L TABLE,X1 I    Load from Memory[TABLE+XR1]

* Pointer table
PTRS  DC   BUFFER1
      DC   BUFFER2
      DC   BUFFER3

      LDX  1 /0000     Index = 0
      LD   PTRS,X1 I   Load from BUFFER1
```

**Use Cases:**
1. **Function pointers:** Jump tables for dynamic dispatch
2. **Dynamic data structures:** Linked lists, trees
3. **Indirect I/O:** IOCC structures point to buffers

---

## Expressions and Symbols

### Symbol Definition

**Rules:**
- Must start with letter (A-Z, a-z)
- Can contain letters, digits, underscores
- Maximum length: unlimited (practical limit ~50 chars)
- Case-insensitive
- Cannot be reserved words (ORG, DC, LD, etc.)

**Examples:**
```
VALID    DC  1    ✓ Valid label
VAL123   DC  2    ✓ Valid label
MY_VAR   DC  3    ✓ Valid label
_TEST    DC  4    ✗ Cannot start with underscore
123VAL   DC  5    ✗ Cannot start with digit
LD       DC  6    ✗ Reserved word
```

### Current Address Operator (*)

The `*` operator represents the current location counter.

**Uses:**

**1. Self-reference:**
```
HERE  EQU  *         HERE = current address
```

**2. Size calculation:**
```
START DC   1
      DC   2
      DC   3
END   EQU  *
SIZE  EQU  END-START SIZE = 3 words
```

**3. Relative addressing:**
```
      BSC  *+5       Skip ahead 5 words
      BSC  *-10      Branch back 10 words
```

**4. Zero operator:**
```
ZERO  EQU  *-*       ZERO = 0
```

### Expression Evaluation

**Supported Operations:**
- Addition: `*+10`, `LABEL+5`
- Subtraction: `*-10`, `END-START`
- Current address: `*`

**Examples:**
```
* Table of offsets
      ORG  /100
BASE  EQU  *
OFF1  EQU  *-BASE     OFF1 = 0
      DC   100
OFF2  EQU  *-BASE     OFF2 = 1
      DC   200
OFF3  EQU  *-BASE     OFF3 = 2
      DC   300

* Skip instructions
      BSC  *+3        Skip next 3 words
      DC   1          (skipped)
      DC   2          (skipped)
      DC   3          (skipped)
      WAIT            Continue here
```

---

## Two-Pass Assembly

The assembler uses a classic two-pass algorithm to handle forward references.

### Pass 1: Symbol Collection

**Objectives:**
1. Build symbol table (labels and their addresses)
2. Calculate location counter
3. Validate directives
4. Check for duplicate labels

**Process:**
```
LocationCounter = 0
SymbolTable = {}

For each source line:
    If line has label:
        If label in SymbolTable:
            ERROR: Duplicate label
        Else:
            SymbolTable[label] = LocationCounter
    
    If line has operation:
        If operation == ORG:
            LocationCounter = operand
        Elif operation == DC:
            LocationCounter += 1
        Elif operation == BSS:
            LocationCounter += count
        Elif operation == BES:
            LocationCounter += count
            SymbolTable[label] = LocationCounter - 1  (last word)
        Elif operation == EQU:
            SymbolTable[label] = ResolveExpression(operand)
        Elif operation is instruction:
            LocationCounter += (long format ? 2 : 1)
```

**Pass 1 Output:**
- Symbol table mapping labels to addresses
- Updated location counter
- List of errors (if any)

### Pass 2: Code Generation

**Objectives:**
1. Generate machine code
2. Resolve symbol references
3. Create formatted listing
4. Report unresolved symbols

**Process:**
```
LocationCounter = 0

For each source line:
    If operation == ORG:
        LocationCounter = operand
    
    Elif operation == DC:
        value = ResolveOperand(operand, SymbolTable)
        Memory[LocationCounter] = value
        LocationCounter += 1
    
    Elif operation == BSS/BES:
        LocationCounter += count
    
    Elif operation is instruction:
        Generate machine code using:
            - Opcode
            - Format (short/long)
            - Tag (index register)
            - Address (resolved from SymbolTable)
            - Modifiers (indirect, etc.)
        
        Store in Memory[LocationCounter]
        LocationCounter += (long ? 2 : 1)
    
    Add to formatted listing
```

**Pass 2 Output:**
- Generated machine code in memory
- Formatted assembly listing
- Symbol resolution errors (if any)

### Symbol Resolution

**Forward References:**
```
      LD   L LATER     OK: LATER defined below
      WAIT
LATER DC   42
```

Pass 1 collects "LATER" = address
Pass 2 resolves the reference

**Undefined Symbols:**
```
      LD   L UNDEF     ERROR: UNDEF never defined
```

Pass 1: UNDEF not in symbol table
Pass 2: Cannot resolve, generate error

### Example Assembly Process

**Source Code:**
```
      ORG  /100
START LD   L VALUE
      A    L ONE
      STO  L RESULT
      BSC  START
      WAIT
VALUE DC   10
ONE   DC   1
RESULT DC  0
```

**Pass 1 Symbol Table:**
```
START  = 0x0100
VALUE  = 0x0109
ONE    = 0x010A
RESULT = 0x010B
```

**Pass 2 Generated Code:**
```
0x0100: 0xC400  LD L VALUE (opcode=0x18, long, VALUE=0x109)
0x0101: 0x0109  Address of VALUE
0x0102: 0x8400  A L ONE (opcode=0x10, long, ONE=0x10A)
0x0103: 0x010A  Address of ONE
0x0104: 0xD400  STO L RESULT (opcode=0x1A, long, RESULT=0x10B)
0x0105: 0x010B  Address of RESULT
0x0106: 0x4800  BSC START (opcode=0x09, long, START=0x100)
0x0107: 0x0100  Address of START
0x0108: 0xB000  WAIT
0x0109: 0x000A  10 (VALUE)
0x010A: 0x0001  1 (ONE)
0x010B: 0x0000  0 (RESULT)
```

---

## Error Handling

### Error Types

#### 1. Syntax Errors

**Missing ORG:**
```
      DC   5           ERROR: Missing ORG directive
```

**Invalid ORG format:**
```
      ORG  100         ERROR: ORG address must begin with /
      ORG  /ZZZZ       ERROR: Invalid hex address
```

**Invalid operand:**
```
      LD               ERROR: Missing operand
```

#### 2. Symbol Errors

**Duplicate label:**
```
LABEL DC   1
LABEL DC   2           ERROR: Duplicate label: LABEL
```

**Undefined symbol:**
```
      LD   L UNDEF     ERROR: Undefined symbol: UNDEF
```

#### 3. Value Errors

**Value out of range:**
```
      DC   65536       ERROR: Value too large for 16 bits
      DC   -1          ERROR: Negative value not allowed
```

**Invalid hex:**
```
      DC   /GGGG       ERROR: Invalid hex value
```

#### 4. Address Errors

**Address overflow:**
```
      ORG  /8000       ERROR: Address beyond memory (32K system)
```

**Displacement out of range:**
```
      ORG  /100
      LD   200         ERROR: Displacement must be -128 to +127
```

### Error Reporting

**Error Structure:**
```csharp
public class AssemblyError
{
    public int LineNumber { get; set; }
    public string Message { get; set; }
}
```

**Example Error Output:**
```
Assembly errors:
Line 3: Undefined symbol: UNKNOWN
Line 7: Displacement out of range for short format: 200
Line 12: Invalid hex address: /ZZZZZ
```

**Error Recovery:**
- Assembly continues after error
- All errors reported
- No code generated if any errors
- Symbol table still built (best effort)

---

## Usage Examples

### Example 1: Simple Addition

```csharp
var cpu = new Cpu();

var source = @"
      ORG  /100
      LD   L NUM1
      A    L NUM2
      STO  L RESULT
      WAIT
NUM1  DC   42
NUM2  DC   17
RESULT DC  0
";

var result = cpu.Assemble(source);

if (result.Success)
{
    cpu.Iar = 0x100;
    while (!cpu.Wait)
    {
        cpu.NextInstruction();
        cpu.ExecuteInstruction();
    }
    
    Console.WriteLine($"Result: {cpu[result.Symbols["RESULT"]]}");
    // Output: Result: 59 (42 + 17)
}
```

### Example 2: Array Processing

```csharp
var source = @"
      ORG  /100
* Sum array of 10 numbers
      LD   L /0000     Clear accumulator
      LDX  1 /0000     Index = 0
LOOP  A    ARRAY,X1    Add ARRAY[index]
      MDX  LOOP,1      Increment index, loop if < 10
      STO  L SUM       Store sum
      WAIT

ARRAY DC   1
      DC   2
      DC   3
      DC   4
      DC   5
      DC   6
      DC   7
      DC   8
      DC   9
      DC   10
SUM   DC   0
";

var result = cpu.Assemble(source);
// Result: SUM = 55 (1+2+3+4+5+6+7+8+9+10)
```

### Example 3: Subroutine Call

```csharp
var source = @"
      ORG  /100
* Main program
MAIN  LD   L /0005     Load parameter
      BSI  L SQUARE    Call SQUARE subroutine
      STO  L RESULT    Store result
      WAIT

* Subroutine: Square the accumulator
SQUARE DC  0           Return address storage
       M   L SQ_ARG    Multiply by self
       STO  L SQ_ARG   Save result in Acc
       LD   L SQ_ARG   Restore to Acc
       BSC  L SQUARE I Return via indirect branch
SQ_ARG DC  0           Temporary storage

RESULT DC  0
";

var result = cpu.Assemble(source);
// Result: RESULT = 25 (5 * 5)
```

### Example 4: Using BSS and Symbols

```csharp
var source = @"
      ORG  /100
* Define constants
BUFSIZE EQU 80
MAXVAL  EQU /FFFF

* Allocate buffers
INBUF  BSS  BUFSIZE    Input buffer (80 words)
OUTBUF BSS  BUFSIZE    Output buffer (80 words)
TEMP   DC   0          Temporary variable

* Code that uses buffers
START  LD   L MAXVAL   Load max value
       LDX  1 /0000    Index = 0
LOOP   STO  INBUF,X1   Initialize buffer[i] = MAXVAL
       MDX  LOOP,1     Increment and loop
       WAIT

* Calculate sizes
IBUF_SIZE EQU *-INBUF  Size of input buffer
";

var result = cpu.Assemble(source);
```

### Example 5: Character-Mode I/O (1442)

```csharp
var source = @"
      ORG  /100
* Read card using 1442 character-mode
START  XIO  L IOCC_CTRL     Start read operation
       
* Wait for column interrupt (Level 0)
WAIT_COL LDX 1 /0000        Column index = 0
       
READ_LOOP XIO L IOCC_READ   Read one column
          STO BUFFER,X1     Store in buffer[index]
          MDX READ_LOOP,1   Increment index
          
* All 80 columns read
       WAIT

* I/O control
IOCC_CTRL DC 0
          DC /0220          Device 01 (1442), Control, Start Read

IOCC_READ DC BUFFER         Buffer address
          DC /0200          Device 01, Read function

BUFFER BSS 80               80-column buffer
";
```

### Example 6: Complex Expression

```csharp
var source = @"
      ORG  /100
* Calculate table offsets
TABLE  EQU  *
OFF0   EQU  *-TABLE    Offset 0
ENTRY0 DC   100
OFF1   EQU  *-TABLE    Offset 1
ENTRY1 DC   200
OFF2   EQU  *-TABLE    Offset 2
ENTRY2 DC   300

* Use offsets
       LDX  1 OFF1     Load offset 1
       LD   TABLE,X1   Load ENTRY1 (200)
       WAIT
";

var result = cpu.Assemble(source);
// OFF0 = 0, OFF1 = 1, OFF2 = 2
```

---

## Assembler API

### ICpu Interface

```csharp
public interface ICpu
{
    AssemblyResult Assemble(string sourceCode);
}
```

### Cpu Method

```csharp
public AssemblyResult Assemble(string sourceCode)
{
    var assembler = new Assembler(this);
    return assembler.Assemble(sourceCode);
}
```

### Assembler Class

```csharp
public class Assembler
{
    private readonly ICpu _cpu;
    private AssemblyContext _context;
    
    public Assembler(ICpu cpu)
    {
        _cpu = cpu;
    }
    
    public AssemblyResult Assemble(string sourceCode)
    {
        _context = new AssemblyContext(sourceCode);
        
        try
        {
            // Pass 1: Build symbol table
            Pass1();
            
            if (!_context.Errors.Any())
            {
                // Pass 2: Generate code
                Pass2();
            }
        }
        catch (Exception ex)
        {
            _context.AddError(0, $"Internal error: {ex.Message}");
        }
        
        return new AssemblyResult
        {
            Success = !_context.Errors.Any(),
            Listing = _context.Listing,
            Errors = _context.Errors,
            Symbols = _context.Symbols
        };
    }
}
```

### Assembly Context

```csharp
internal class AssemblyContext
{
    public Dictionary<string, ushort> Symbols { get; }
    public List<string> Listing { get; }
    public List<AssemblyError> Errors { get; }
    public int LocationCounter { get; set; }
    public bool HasOrigin { get; set; }
    
    public void AddError(int lineNumber, string message)
    {
        Errors.Add(new AssemblyError 
        { 
            LineNumber = lineNumber, 
            Message = message 
        });
    }
}
```

---

## Implementation Details

### Line Parsing

**AssemblyLine Class:**
```csharp
internal class AssemblyLine
{
    public string Label { get; }
    public string Operation { get; }
    public string Operand { get; }
    
    public AssemblyLine(string line)
    {
        // Split by whitespace
        var tokens = line.Split(new[] { ' ', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int index = 0;
        
        // First token could be label or operation
        if (tokens.Length > 0 && !IsOperation(tokens[0]))
        {
            Label = tokens[index++];
        }
        
        // Next is operation
        if (index < tokens.Length)
        {
            Operation = tokens[index++];
        }
        
        // Rest is operand
        if (index < tokens.Length)
        {
            Operand = string.Join(" ", tokens.Skip(index));
        }
    }
}
```

### Instruction Encoding

**Opcode Mapping:**
```csharp
private readonly Dictionary<string, byte> _opcodes = new Dictionary<string, byte>
{
    { "LD",   0x18 },  // Load
    { "STO",  0x1A },  // Store
    { "A",    0x10 },  // Add
    { "S",    0x14 },  // Subtract
    { "M",    0x1C },  // Multiply
    { "D",    0x1D },  // Divide
    { "AND",  0x12 },  // Logical AND
    { "OR",   0x13 },  // Logical OR
    { "EOR",  0x11 },  // Exclusive OR
    { "BSC",  0x09 },  // Branch/Skip
    { "BSI",  0x08 },  // Branch and Store
    { "MDX",  0x05 },  // Modify Index
    { "XIO",  0x01 },  // Execute I/O
    { "WAIT", 0x16 },  // Wait
    // ... more opcodes
};
```

### Short Format Generation

```csharp
private void GenerateShortFormat(byte opcode, byte tag, int displacement)
{
    ushort instruction = (ushort)(
        ((opcode & 0x1F) << 11) |  // Opcode in bits 0-4
        ((tag & 0x03) << 8) |       // Tag in bits 6-7
        (displacement & 0xFF)       // Displacement in bits 8-15
    );
    
    _cpu[_context.LocationCounter++] = instruction;
}
```

### Long Format Generation

```csharp
private void GenerateLongFormat(byte opcode, byte tag, ushort address, bool indirect)
{
    ushort firstWord = (ushort)(
        ((opcode & 0x1F) << 11) |  // Opcode in bits 0-4
        0x0400 |                    // Long format bit (bit 5)
        ((tag & 0x03) << 8) |      // Tag in bits 6-7
        (indirect ? 0x80 : 0)      // Indirect bit (bit 8)
    );
    
    _cpu[_context.LocationCounter++] = firstWord;
    _cpu[_context.LocationCounter++] = address;
}
```

### Symbol Resolution

```csharp
private ushort ResolveOperand(string operand)
{
    // Handle current address operator (*)
    if (operand == "*")
    {
        return (ushort)_context.LocationCounter;
    }
    
    // Handle expressions (*+n, *-n)
    if (operand.Contains("+"))
    {
        // Parse and evaluate
    }
    
    // Handle hex values (/XXXX)
    if (operand.StartsWith("/"))
    {
        return ParseHex(operand.Substring(1));
    }
    
    // Handle decimal values
    if (ushort.TryParse(operand, out ushort value))
    {
        return value;
    }
    
    // Handle symbols
    if (_context.Symbols.TryGetValue(operand.ToUpper(), out ushort address))
    {
        return address;
    }
    
    // Undefined symbol
    _context.AddError(_currentLine, $"Undefined symbol: {operand}");
    return 0;
}
```

### Listing Generation

```csharp
private void FormatListingLine(string sourceLine)
{
    string address = $"{_context.LocationCounter:X4}";
    string formatted = $"{address}  {sourceLine}";
    _context.Listing.Add(formatted);
}
```

**Example Listing Output:**
```
0100       ORG /100
0100  * Main program
0100  START LD   L VALUE
0102        A    L ONE
0104        STO  L RESULT
0106        WAIT
0107  VALUE DC   42
0108  ONE   DC   1
0109  RESULT DC  0
```

---

## Performance and Optimization

### Assembly Speed

**Typical Performance:**
- Small programs (<100 lines): <1ms
- Medium programs (100-1000 lines): 1-10ms
- Large programs (1000+ lines): 10-100ms

### Memory Usage

- Symbol table: ~50 bytes per symbol
- Listing: ~100 bytes per line
- Working memory: < 1MB for typical programs

### Optimization Tips

1. **Symbol Table:** Use `Dictionary<string, ushort>` for O(1) lookup
2. **Line Parsing:** Split once, reuse tokens
3. **Error Collection:** Continue after errors (don't abort)
4. **Memory Access:** Write directly to CPU memory array

---

## Future Enhancements

**Potential Additions:**
1. **Macros:** Parameterized code templates
2. **Conditional Assembly:** `IF/ENDIF` directives
3. **Include Files:** `INCLUDE "filename"` directive
4. **Binary Output:** Generate standalone object files
5. **Listing Control:** `LIST`, `NOLIST` directives
6. **Optimization:** Peephole optimization
7. **Debug Symbols:** Source-level debugging info

---

## Summary

The S1130 Assembler provides a complete, accurate implementation of IBM 1130 assembly language. Key features:

- **Complete:** All instructions, addressing modes, and directives
- **Accurate:** Faithful to IBM 1130 syntax and behavior
- **Robust:** Comprehensive error checking and reporting
- **Fast:** Two-pass algorithm with efficient symbol resolution
- **Integrated:** Direct integration with CPU emulator
- **Tested:** 395+ unit tests validate correct operation

The assembler makes the S1130 emulator accessible for writing and testing IBM 1130 programs without manual machine code generation.
