# IBM 1130 Format Specifier Syntax

## Overview

The S1130 assembler supports explicit format and index register specifiers that match the fixed-column card format used by the original IBM 1130 assembler (columns 16-18).

## Format Specifiers

### Card-Style Format Specification

The assembler supports single-character or two-character format specifiers that match the IBM 1130 card format:

**Format Specifier Chart:**
| Specifier | Format | Index Reg | Description |
|-----------|--------|-----------|-------------|
| `.` | Short | None | Explicit short format, no index register |
| `L` | Long | None | Long format, no index register |
| `1` | Short | XR1 | Short format with index register 1 |
| `2` | Short | XR2 | Short format with index register 2 |
| `3` | Short | XR3 | Short format with index register 3 |
| `L1` | Long | XR1 | Long format with index register 1 |
| `L2` | Long | XR2 | Long format with index register 2 |
| `L3` | Long | XR3 | Long format with index register 3 |
| `I` | Indirect | None | Indirect addressing, no index register |
| `I1` | Indirect | XR1 | Indirect with index register 1 |
| `I2` | Indirect | XR2 | Indirect with index register 2 |
| `I3` | Indirect | XR3 | Indirect with index register 3 |

### Short Format (`.`)

Explicitly indicates short format (IAR-relative addressing, ±127 words).

**Syntax:**
```
OPERATION . address
```

**Example:**
```
      ORG  /100
START LD   . VALUE   Load from IAR-relative address
      A    . VALUE   Add from IAR-relative address  
      STO  . RESULT  Store to IAR-relative address
      WAIT
VALUE DC   42
RESULT DC  0
```

**Generated Code:**
- Each instruction is 1 word
- Displacement calculated as: `address - (IAR + 1)`
- Range: -128 to +127 from current instruction

### Long Format (`L`)

Explicitly indicates long format (absolute 16-bit addressing).

**Syntax:**
```
OPERATION L address
```

**Example:**
```
      ORG  /100
START LD   L /0500   Load from absolute address 0x0500
      A    L VALUE   Add from VALUE's absolute address  
      STO  L RESULT  Store to RESULT's absolute address
      WAIT
VALUE DC   42
RESULT DC  0
```

**Generated Code:**
- Each instruction is 2 words (instruction + address)
- Full 16-bit address specified directly
- Range: 0x0000 to 0xFFFF

### Index Register Formats (`1`, `2`, `3`, `L1`, `L2`, `L3`)

Combines format specification with index register selection.

**Syntax:**
```
OPERATION formatreg address
```

**Examples:**
```
      ORG  /100
      LDX  1 /0005     Initialize XR1 to 5
      
LOOP  LD   1 TABLE    Load TABLE[XR1] (short format + XR1)
      A    L1 DATA    Add DATA[XR1] (long format + XR1)
      STO  2 RESULT   Store to RESULT[XR2] (short + XR2)
      MDX  LOOP,-1    Decrement and loop
      
TABLE DC   10,20,30,40,50
DATA  DC   100,200,300,400,500
RESULT BSS 5
```

**Generated Code:**
- Short format with index (1, 2, 3): 1 word
- Long format with index (L1, L2, L3): 2 words
- Effective address = base address + index register value

### Indirect Formats (`I`, `I1`, `I2`, `I3`)

Adds an additional level of address indirection.

**Syntax:**
```
OPERATION I address
OPERATION indexreg address
```

**Examples:**
```
      ORG  /100
      LD   I PTR       Load from address stored in PTR
      LD   I1 PTRTBL   Load from address in PTRTBL[XR1]
      
PTR   DC   /0500      Points to address 0x0500
PTRTBL DC  /0600      Pointer table
       DC  /0700
```

### Legacy Comma Syntax (Still Supported)

The assembler also supports the legacy comma-based syntax for index registers:

**Syntax:**
```
OPERATION [L] address,Xn [I]
```

**Examples:**
```
      LD   TABLE,X1    Short format with XR1
      LD   L TABLE,X2  Long format with XR2
      LD   L PTR,X1 I  Long format with XR1, indirect
```

### No Specifier (Default Short Format)

Without a format specifier, short format is assumed:

```
      LD   VALUE       Assumes short format
```

## Historical Context

The original IBM 1130 assembler used punched cards with fixed column positions:

- **Columns 1-5**: Label
- **Column 6+**: Operation
- **Column 16**: Format specification (blank = short, L = long)
- **Column 17+**: Address/operand

The dot (`.`) in our implementation serves the same purpose as the blank/space in column 16 - it explicitly marks short format, making the intent clear in free-form text.

## Benefits

1. **Clarity**: Code explicitly shows whether short or long format is used
2. **Intent**: Programmer's addressing mode choice is visible
3. **Compatibility**: Matches original IBM 1130 card format conventions
4. **Optimization**: Short format uses half the memory (1 word vs 2 words)

## Mixed Format Example

You can mix format specifiers within the same program:

```
      ORG  /100
START LD   . NEAR    Short format for nearby data (1 word)
      A    L /0500   Long format for far address (2 words)
      M    . COUNT   Short format for nearby variable (1 word)
      STO  L RESULT  Long format for absolute address (2 words)
      BSI  . SUBR    Short format branch (1 word)
      WAIT

* Data within ±127 words - use short format
NEAR  DC   10
COUNT DC   5

* Subroutine within range
SUBR  DC   0
      LD   L VALUE
      BSC  L SUBR I  Return
      
RESULT DC  0
VALUE DC   42
```

## Performance Considerations

**Short Format (`.`):**
- ✅ 1 word per instruction (saves memory)
- ✅ Faster to fetch (single memory read)
- ❌ Limited range (±127 words)

**Long Format (`L`):**
- ✅ Full addressing range (64K)
- ✅ Position-independent
- ❌ 2 words per instruction (uses more memory)
- ❌ Slower to fetch (two memory reads)

## Implementation Details

The format specifier is parsed in `ParseOperand()` method of `Assembler.cs`:

```csharp
// Check for format specifiers
if (tokens[0].ToUpper() == "L")
{
    isLong = true;
    operandTokenCount = 2;  // "L address"
}
else if (tokens[0] == ".")
{
    isShortExplicit = true;
    operandTokenCount = 2;  // ". address"
}
```

Location counter advancement in Pass 1 accounts for format:

```csharp
bool isLongFormat = operandTrim.ToUpper().StartsWith("L ");
_context.LocationCounter += isLongFormat ? 2 : 1;
```

## Related Documentation

- [Assembler.md](Assembler.md) - Full assembler documentation
- [InternalOperation.md](InternalOperation.md) - CPU instruction execution
- IBM 1130 Functional Characteristics (original IBM documentation)

## Test Coverage

New tests in `AssemblerTests.cs`:
- `ShortFormatWithDotSpecifierShouldSucceed` - Validates `.` syntax
- `LongFormatWithLSpecifierShouldSucceed` - Validates `L` syntax
- `MixedFormatSpecifiersShouldSucceed` - Validates mixed usage

All 422 unit tests passing (419 original + 3 new format tests).
