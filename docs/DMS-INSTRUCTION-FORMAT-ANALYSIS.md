# DMS Assembly Code Format Analysis

## Overview
This document analyzes the instruction formatting patterns used in the IBM 1130 Disk Monitor System (DMS R2 V12) assembly source code to ensure our assembler correctly supports all authentic IBM 1130 syntax patterns.

## Source Files Analyzed
- `emonitor.asm` - Resident Monitor (1141 lines)
- `dciloadr.asm` - Core Image Loader (257 lines)  
- `asysldr1.asm` - System Loader Phase 1 (1200+ lines)
- Multiple other DMS system files

## Key Findings

### 1. **Column-Based Format (IBM Card Format)**
The DMS code uses traditional IBM card column formatting:
- **Columns 1-5**: Label (optional)
- **Column 6-15**: Operation/OpCode
- **Columns 16+**: Operands
- **Comments**: Typically after operands, sometimes on separate lines starting with `*`

### 2. **Format and Index Register Specification**

#### **Observed Patterns:**

**Load/Store Instructions:**
```assembly
LD      DZ915                # Short format, no index
LD   L  *-*                  # Long format (L prefix)
LD    1 -1                   # Short format with index register 1
LD   L1 *-*                  # Long format with index register 1
LD    2 0                    # Short format with index register 2
LD   L2 DZ000                # Long format with index register 2
LD    3 AZ952-E              # Short format with index register 3
LD   L3 BUFFR+1              # Long format with index register 3
LD   I  *-*                  # Indirect addressing
LD   I1 INPUT+52             # Indirect with index register 1
LD  L1 BUFFR+1               # Long format with index 1 (no space)
```

**Pattern Summary:**
- Space after opcode varies (1-7 spaces typically)
- Format indicator: **L** = Long format
- Index register: **1**, **2**, **3** immediately after format
- Indirect: **I** can combine with index (**I1**, **I2**, **I3**)
- **NO DOT (.) notation** - The dot is NOT used in authentic DMS code
- Format/index can be written together: `L1`, `L2`, `L3`, `I1`, `I2`, `I3`

### 3. **Branch Instructions**

```assembly
BSC  L  START                # Long format branch
BSC     +                    # Conditional branch (skip if +)
BSC     -                    # Conditional branch (skip if -)
BSC     C                    # Branch on carry
BSC     O                    # Branch on overflow
BSC     E                    # Branch on even
BSC     Z                    # Branch on zero
BSC  L  CL080,+-             # Long format with condition
BSC  I  DZ000,+-             # Indirect with condition
BSI   2 $PST2-X2             # BSI with index register 2
BSI  L  *-*                  # BSI long format
BSI     AA050                # BSI short format
BSI  I  CL070                # BSI indirect
```

**Key Observations:**
- Conditions can be single char: `+`, `-`, `C`, `O`, `E`, `Z`
- Conditions can be combined: `+-` (skip if plus OR minus)
- Format comes before condition: `BSC L TARGET,Z`
- BSI (Branch and Store IAR) used for subroutine calls

### 4. **Index Register Instructions**

```assembly
LDX   1 54                   # Load index register 1 (short)
LDX  L1 *-*                  # Load index register 1 (long)
LDX  L2 DZ000                # Load index register 2 (long)
LDX   3 72                   # Load index register 3 (short)
STX   1 DZ100+1              # Store index register 1
STX   2 DZ100+3              # Store index register 2
MDX     CK3                  # Modify index and skip (short)
MDX  L  $DBSY,-1             # Modify index (long) with modifier
MDX   1 -1                   # Modify index 1 (short) with modifier
MDX   2 1                    # Modify index 2, increment
MDX   3 -1                   # Modify index 3, decrement
```

**Pattern:**
- Index register number immediately after opcode (with space)
- Format `L` comes before index number: `LDX L1`, `STX L2`
- MDX has special modifier field (positive/negative value)

### 5. **Double-Word Instructions**

```assembly
LDD     DZ902                # Load double (ACC + EXT)
LDD   1 0                    # Load double with index 1
LDD  L  PAIR1                # Load double long format
STD   1 0                    # Store double with index 1
STD  L  $CIBA-1              # Store double long format
```

### 6. **Shift and Rotate Instructions**

```assembly
SLA     8                    # Shift left accumulator 8 bits
SLA     16                   # Shift left 16 bits (clear ACC)
SRA     1                    # Shift right accumulator 1 bit
SRA     16                   # Shift right 16 bits
RTE     16                   # Rotate EXT 16 bits (swap halves)
SLT     5                    # Shift left 5 bits (both ACC+EXT)
```

**Pattern:**
- Shift count is the operand
- No format or index specifiers

### 7. **I/O Instructions**

```assembly
XIO     DZ904                # Execute I/O
XIO     SENSE                # XIO with symbolic IOCC
XIO     PT430                # XIO short format
WAIT                         # Wait for interrupt (no operands)
```

### 8. **Logical Instructions**

```assembly
AND     CL120                # AND with memory
AND     PT580                # AND short format
OR    2 DZ980-X2             # OR with index 2
EOR  L1 *-*                  # Exclusive OR, long, index 1
```

### 9. **Special Instructions**

```assembly
BOSC I  CL070                # Branch Out of Subroutine on Condition
BOSC L  CL140                # BOSC long format
BOSC    *-3,E                # BOSC with condition
NOP                          # No operation
```

## Comparison with Our Current Syntax

### What We Implemented (New Card-Format Syntax)
```assembly
LD   . BIT                   # Short format (. = no format/index)
LD   L START                 # Long format
LD   1 TABLE                 # Short with index 1
LD   L1 TABLE                # Long with index 1
LD   I ADDR                  # Indirect
LD   I1 ADDR                 # Indirect with index 1
BSC  L START                 # Long branch
```

### What DMS Actually Uses
```assembly
LD      BIT                  # Short format (NO DOT!)
LD   L  START                # Long format (space after L)
LD    1 TABLE                # Short with index 1
LD   L1 TABLE                # Long with index 1 (no space)
LD   I  ADDR                 # Indirect (space after I)
LD  I1  ADDR                 # Indirect with index 1
BSC  L  START                # Long branch (space after L)
```

## Critical Differences

### 1. **DOT (.) NOT USED**
- **Our syntax**: `LD . BIT` (we invented this)
- **DMS syntax**: `LD      BIT` or `LD   BIT` (just spaces)
- **Conclusion**: The dot notation is NOT authentic IBM 1130 syntax

### 2. **Spacing Variations**
- DMS code has variable spacing between opcode and operand
- Format indicators (`L`, `I`) sometimes have space after them, sometimes not
- Examples:
  - `LD   L  START` (spaces around L)
  - `LD  L1 TABLE` (no space after L)
  - `BSC  L  *-*` (spaces around L)
  - `LDX  L1 *-*` (no space after L)

### 3. **Legacy Comma Syntax**
**We support**: `LD TABLE,X1` (comma-based index specification)
**DMS uses**: `LD  1 TABLE` (space-based, index before address)

**Conclusion**: DMS does NOT use comma syntax for index registers in main code

## Recommendations

### Option 1: Keep Both Syntaxes (Recommended)
- **Keep our new card-format syntax** as a modern, unambiguous alternative
- **Add support for authentic DMS spacing patterns**
- Treat multiple spaces as acceptable
- Make the dot (`.`) optional

### Option 2: Switch to Pure DMS Syntax
- Remove dot (`.`) notation entirely
- Use spacing to indicate short format
- Implement variable-space parsing

### Option 3: Hybrid Approach
- Support both `.` and no-dot for short format
- Support both `L ` (with space) and `L` (no space)
- Support both `I ` and `I` formats
- Most flexible, maintains backward compatibility

## Assembly Examples from DMS

### DISKZ Subroutine (emonitor.asm lines 618-650)
```assembly
DZ000 DC      *-*       ENTRY POINT
      MDX     *-3       
      MDX     DZ020     BR AROUND INT ENTRY POINT

DZ010 DC      *-*       INTERRUPT ADDRESS
      MDX     DZ180     BR TO SERVICE INTERRUPT
DZ020 STX   1 DZ100+1   SAVE XR1
      STX   2 DZ100+3   SAVE XR2
      SLA     8         SHIFT INDICATOR 8 BITS
      STO     DZ945     SAVE FUNCTION INDICATOR
      RTE     16
      STO     DZ235+1   SAVE ADDR OF THE I/O AREA
      MDX     DZ230     BR TO CONTINUE
DZ060 BSC  L  *-*       BR TO SERVICE THE INTERRUPT

DZ070 STX   1 DZ180+1   SAVE ADDR OF THE I/O AREA
      XIO     DZ904     START AN OPERATION

DZ100 LDX  L1 *-*       RESTORE XR1
      LDX  L2 *-*       RESTORE XR2
      LD      DZ010     INTERRUPT ENTRY
      BSC  I  DZ000,+-  NO,MONITOR ENTRY
      STO     DZ110+1   YES,INT ENTRY
      SRA     16        RESET
      STO     DZ010     *INT ENTRY
DZ110 BSC  L  *-*
      NOP               DUMMY OP
```

### Core Image Loader (dciloadr.asm lines 140-180)
```assembly
CL140 LDX   1 72        LOAD WORD COUNT
      LD      CL130     LOAD INDICATOR BIT
      STO  L1 CL270     STORE IN A WORD OF BUFFER
      MDX   1 -1        DECREMENT WORD COUNT
      MDX     *-4       BRANCH IF NOT ZERO

CL150 XIO     CL040-1   TEST 2501 DSW
      BSC  L  CL150,E   BRANCH IF NOT READY
      XIO     CL020-1   START 2501
      STX   0 CL050     SET CL050 ON

      LD      CL020-1
      STO     CL030-1   INITIALIZE IOCC ADDRESS
      LDX  L1 CL270+1   LOAD BUFFER ADDRESS
      LDX  L2 CL270+1   LOAD BUFFER ADDRESS
      LDX   3 72        LOAD COLUMN COUNT

CL160 LD    2 0         TEST IF COLUMN IN CORE
      BSC  L  *-3,E     BRANCH IF NOT
      LD    1 -1        LOAD LAST WORD
CL170 RTE     16        PLACE IN Q REGISTER
      LD    2 0         LOAD NEW WORD
CL180 RTE     16        PLACE ON WORD BOUNDARY
      STO   1 -1        STORE A REGISTER
```

## Instruction Format Summary Table

| Instruction Type | Short Format | Long Format | With Index | Indirect | Indirect+Index |
|-----------------|--------------|-------------|------------|----------|----------------|
| **Load/Store** | `LD  ADDR` | `LD  L  ADDR` | `LD  1 ADDR` | `LD  I  ADDR` | `LD I1 ADDR` |
| **Arithmetic** | `A   ADDR` | `A   L  ADDR` | `A   1 ADDR` | `A  I  ADDR` | `A  I1 ADDR` |
| **Branch** | `BSC ADDR` | `BSC L ADDR` | `BSC 1 ADDR` | `BSC I ADDR` | `BSC I1 ADDR,Z` |
| **BSI** | `BSI SUBR` | `BSI L SUBR` | `BSI 2 SUBR` | `BSI I SUBR` | `BSI I2 SUBR` |
| **LDX/STX** | `LDX 1 VAL` | `LDX L1 ADDR` | N/A | N/A | N/A |
| **MDX** | `MDX ADDR` | `MDX L ADDR,MOD` | `MDX 1 MOD` | N/A | N/A |
| **Double** | `LDD ADDR` | `LDD L ADDR` | `LDD 1 ADDR` | `LDD I ADDR` | N/A |

## Conclusion

**The authentic IBM 1130 DMS assembly syntax:**
1. Does NOT use a dot (`.`) for short format
2. Uses variable spacing between opcode and operands
3. Uses space-delimited format/index specifiers, not commas
4. Places format indicator (`L`, `I`) immediately before or after opcode
5. Places index register number (`1`, `2`, `3`) between opcode and address
6. Combines format and index without spaces: `L1`, `L2`, `L3`, `I1`, `I2`, `I3`

**Our current implementation** introduced the dot (`.`) notation as a modern convenience, but it is NOT historically accurate.

**Recommendation**: Add support for the authentic spacing-based short format while keeping our dot notation as an optional convenience feature for modern users.

---

**Date**: October 20, 2025
**Analyzed by**: GitHub Copilot AI Assistant
**Source**: IBM 1130 DMS R2 V12 Assembly Source Code
