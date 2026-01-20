# S1130 Assembler Syntax Summary

## Critical Format Information

The S1130 assembler uses **modern free-form syntax** with explicit `|format|` specifiers.

### ⚠️ Important: Two Different Formats

**The S1130 assembler REQUIRES pipes (`|format|`) for format specifiers.**

1. **S1130 Format** (what the assembler accepts):
   ```
   LABEL  LD  |L| VALUE
   ```
   ✅ This works - pipes are required for long format, indirect, and index registers

2. **IBM 1130 Format** (legacy, for reference only):
   ```
                       LABEL LD   L  VALUE
   ```
   ❌ This does NOT work in S1130 - it's the old fixed-column format

Use the `asmconv` utility to convert between formats.

## S1130 Instruction Syntax

### Basic Pattern

```
[label]  OPERATION  |format|  operand
```

### Format Specifiers

| Specifier | Format | Index | Description |
|-----------|--------|-------|-------------|
| (none) | Short | None | IAR-relative, ±127 words, 1 word instruction (default) |
| `|L|` | Long | None | Absolute address, 2 word instruction |
| `|I|` | Indirect | None | Indirect addressing |
| `|1|` | Short | XR1 | Short + index register 1 |
| `|2|` | Short | XR2 | Short + index register 2 |
| `|3|` | Short | XR3 | Short + index register 3 |
| `|L1|` | Long | XR1 | Long + index XR1 |
| `|L2|` | Long | XR2 | Long + index XR2 |
| `|L3|` | Long | XR3 | Long + index XR3 |
| `|I1|` | Indirect | XR1 | Indirect + XR1 |
| `|I2|` | Indirect | XR2 | Indirect + XR2 |
| `|I3|` | Indirect | XR3 | Indirect + XR3 |

**Note:** Short format is the default when no format specifier is provided. The assembler will automatically use short format for addresses within ±127 words of the instruction.

## Common Instructions

### Load/Store

```
LD   |L| VALUE       Load accumulator from VALUE (long format)
LD   NEAR            Load from NEAR (short format - no specifier)
LD   |1| TABLE       Load from TABLE+XR1
LD   |L2| DATA       Load from DATA+XR2
LD   |I| PTR         Load from address in PTR

STO  |L| RESULT      Store accumulator to RESULT
STO  |1| TABLE       Store to TABLE+XR1

LDD  |L| VALUE       Load double (Acc:Ext from VALUE:VALUE+1)
STD  |L| RESULT      Store double (Acc:Ext to RESULT:RESULT+1)

LDX  |1| COUNT       Load index register XR1
LDX  |L2| /0400      Load XR2 from address 0x0400
STX  |1| SAVE        Store XR1 to SAVE
```

### Arithmetic

```
A    |L| VALUE       Add VALUE to accumulator
S    |L| VALUE       Subtract VALUE from accumulator
M    |L| VALUE       Multiply (result in Acc:Ext)
D    |L| VALUE       Divide (quotient in Acc, remainder in Ext)

AD   |L| VALUE       Add double (32-bit)
SD   |L| VALUE       Subtract double (32-bit)
```

### Logical

```
AND  |L| MASK        Logical AND
OR   |L| BITS        Logical OR
EOR  |L| VALUE       Exclusive OR
```

### Shift

```
SLA  count           Shift left accumulator
SRA  count           Shift right accumulator
SLC  count           Shift left Acc:Ext
```

### Branch - BSC (Branch/Skip on Condition)

**Critical:** Condition comes AFTER target with comma!

```
BSC  |L| target,condition
```

**Format:**
```
BSC  condition               Skip next instruction if condition met
BSC  |L|  target            Unconditional branch
BSC  |I|  target            Indirect branch (return from subroutine)
BSC  |L|  target,C          Branch if carry OFF
BSC  |L|  target,O          Branch if overflow OFF
BSC  |L|  target,Z          Branch if zero
BSC  |L|  target,+-         Branch if not zero
BSC  |L|  target,+          Branch if positive or zero
BSC  |L|  target,-          Branch if negative
BSC  |L|  target,E          Branch if even
```

**Condition Codes:**
- `C` - Carry OFF (clear)
- `O` - Overflow OFF (clear)
- `Z` - Zero
- `+` - Positive or zero
- `-` - Negative
- `E` - Even (bit 15 = 0)
- `+-` - Not zero (positive OR negative)

**Examples:**
```
       BSC  |L| LOOP        Unconditional branch to LOOP
       BSC  |L| DONE,Z      Branch to DONE if ACC = 0
       BSC  |L| ERROR,C     Branch to ERROR if carry is clear
       BSC  |L| LOOP,+-     Branch to LOOP if ACC ≠ 0
       BSC  |I| RTNADDR     Return via indirect branch
```

### Subroutine Call - BSI

```
BSI  |L| SUBR        Call subroutine at SUBR
```

Stores return address at SUBR, branches to SUBR+1.

**Return pattern:**
```
SUBR   DC 0            Return address storage
       ...             Subroutine code
       BSC  |I| SUBR   Return via indirect branch
```

### Index Modify - MDX

```
MDX  |format| target,modifier
```

**Examples:**
```
       LDX  |1| /0010       Load XR1 = 16
LOOP   ...                  Loop body
       MDX  LOOP,-1         Decrement XR1, loop if not zero (short format)
```

### I/O - XIO

```
XIO  |L| IOCC         Execute I/O command
```

**IOCC Structure:**
```
IOCC   DC BUFFER       Word count address
       DC /0900        Device code + function
```

## Directives

```
      ORG /100          Set origin address
LABEL DC 42             Define constant
BUFFER BSS 80           Reserve 80 words
COUNT EQU 5             Define symbol
      END               End of program
```

## Complete Program Example

```
       ORG /100
START  LDX  |1| /0005      Initialize counter
       LD   |L| /0000      Clear accumulator

LOOP   A    |1| TABLE      Add TABLE[XR1]
       MDX  LOOP,-1        Decrement, loop if not zero (short format)
       
       STO  |L| RESULT     Store sum
       WAIT                Halt

TABLE  DC 10,20,30,40,50   Data
RESULT DC 0                 Result storage
```

## Common Mistakes

### ❌ Wrong (IBM 1130 format without pipes):
```
LD   L VALUE
STO  L RESULT
BSC  L LOOP,Z
```

### ✅ Correct (S1130 format with pipes):
```
LD   |L| VALUE
STO  |L| RESULT
BSC  |L| LOOP,Z
```

### ❌ Wrong (condition before target):
```
BSC  |L| Z,DONE
```

### ✅ Correct (condition after target with comma):
```
BSC  |L| DONE,Z
```

## Converting Legacy Code

Use the `asmconv` utility:

```powershell
# Convert IBM 1130 → S1130
asmconv legacy.asm modern.s1130

# Convert S1130 → IBM 1130
asmconv modern.s1130 legacy.asm

# Auto-detect and convert
type input.asm | asmconv > output.s1130
```

See:
- [asmconv README](../src/S1130.AssemblerConverter/README.md)
- [FORMAT-SPECIFIER-SYNTAX.md](FORMAT-SPECIFIER-SYNTAX.md)
- [Assembler.md](Assembler.md)
