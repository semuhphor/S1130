# IBM 1130 Computing System - Technical Research

**Document Version**: 1.0
**Last Updated**: 2025-10-24
**Purpose**: Comprehensive reference documentation for IBM 1130 hardware specifications to guide emulator development

---

## Table of Contents

1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Memory Organization](#memory-organization)
4. [Registers](#registers)
5. [Instruction Formats](#instruction-formats)
6. [Addressing Modes](#addressing-modes)
7. [Instruction Set Reference](#instruction-set-reference)
8. [Interrupt System](#interrupt-system)
9. [I/O System](#io-system)
10. [Disk Storage System](#disk-storage-system)
11. [Peripheral Devices](#peripheral-devices)
12. [Operating System](#operating-system)
13. [References](#references)

---

## Overview

The IBM 1130 Computing System was introduced by IBM in February 1965 as a low-cost, desk-sized computer designed for scientific and engineering applications. It was IBM's first system to use removable disk packs (IBM 2315 cartridges) and employed Solid Logic Technology (SLT), the same electronics packaging used in System/360.

### Key Characteristics

- **Architecture**: 16-bit binary word machine with 15-bit address space
- **Technology**: Solid Logic Technology (SLT) circuits operating at nanosecond speeds
- **Memory**: Magnetic-core memory, 4K-32K words (16-bit words)
- **Performance**: Up to 120,000 additions per second
- **Storage**: Removable disk packs with 512K words (1 MB) capacity
- **Intended Use**: Scientific computing, engineering calculations, educational institutions

### Historical Context

The IBM 1130 was designed as a successor to earlier small-scale computers and competed with systems like the PDP-8 and PDP-11. The architecture was initially 18-bit but changed to 16-bit due to the influence of System/360 development. It became popular in universities and research labs due to its relatively low cost and ease of use.

---

## System Architecture

### CPU Models

| Model | Memory Cycle | Disk | Notes |
|-------|-------------|------|-------|
| 1131-1 | 3.6 μs | No | Basic configuration |
| 1131-2 | 3.6 μs | Yes | With IBM 2310 disk |
| 1131-3 | 2.2 μs | Yes | Faster memory version |
| 1131-4 | 5.9 μs* | No | Console printer model |
| 1131-5 | 2.2 μs | No | Fast memory, no disk |

*Model 4: 3.6 μs under interrupt conditions

### Physical Characteristics

- **CPU Unit**: Compact, desk-like appearance housing console, core storage, and electronics
- **Size**: Approximately desk-sized
- **Console**: Integrated console with switches, lights, and typewriter
- **Power**: Standard electrical supply
- **Cooling**: Air-cooled

### Processing Unit Components

1. **Central Processing Unit (CPU)**: Instruction execution, arithmetic/logic operations
2. **Core Storage**: Magnetic-core memory (4K-32K words)
3. **Console**: Operator interface with entry switches, display lights
4. **I/O Controllers**: Device attachment and control circuitry

---

## Memory Organization

### Address Space

- **Word Size**: 16 bits (binary)
- **Address Space**: 15 bits (bit 15 is always 0)
- **Maximum Memory**: 32,768 words (65,536 bytes)
- **Minimum Configuration**: 4,096 words (8,192 bytes)
- **Expansion**: In 4K or 8K word increments

### Memory Cycle Times

| Configuration | Cycle Time | Access Pattern |
|--------------|------------|----------------|
| Models 1, 2, 5 | 3.6 microseconds | Single access per cycle |
| Model 3 | 2.2 microseconds | Single access per cycle |
| Model 4 (normal) | 5.85 microseconds | Shared with printer |
| Model 4 (interrupt) | 3.6 microseconds | Priority access |

### Memory Technology

**Magnetic-Core Memory**:
- Non-volatile (retains data when powered off)
- Destructive read (must rewrite after reading)
- Direct word addressing (no byte addressing)
- Parity checking available (optional on some models)

### Reserved Memory Locations

| Address | Decimal | Purpose |
|---------|---------|---------|
| /0000 | 0 | Reserved (unused) |
| /0001 | 1 | Index Register XR1 |
| /0002 | 2 | Index Register XR2 |
| /0003 | 3 | Index Register XR3 |
| /0004-0007 | 4-7 | Reserved |
| /0008 | 8 | Interrupt Level 0 handler address |
| /0009 | 9 | Interrupt Level 1 handler address |
| /000A | 10 | Interrupt Level 2 handler address |
| /000B | 11 | Interrupt Level 3 handler address |
| /000C | 12 | Interrupt Level 4 handler address |
| /000D | 13 | Interrupt Level 5 handler address |
| /000E-001F | 14-31 | Reserved for system use |

### Memory Layout Convention

Under Disk Monitor System 2 (DM2):
- **/0000-/001F**: System reserved area
- **/0020-/01FD**: Supervisor and system buffers
- **/01FE**: First available user memory location
- **Upper memory**: User programs and data

---

## Registers

The IBM 1130 implements a minimal register set typical of early minicomputers.

### Hardware Registers

#### Instruction Address Register (IAR)
- **Size**: 16 bits (15-bit effective address space)
- **Function**: Program counter, points to next instruction
- **Auto-increment**: Advanced automatically during instruction fetch
- **Accessible**: Can be used as index register (tag = 0 in some contexts)

#### Accumulator (ACC)
- **Size**: 16 bits
- **Function**: Primary arithmetic/logic register
- **Operations**: All arithmetic, logical, load/store operations
- **Combined**: Can be combined with EXT for 32-bit operations

#### Extension Register (EXT)
- **Size**: 16 bits
- **Function**: Extended accumulator for double-precision operations
- **Operations**: Double-precision arithmetic, multiply/divide
- **Combined ACC+EXT**: Forms 32-bit register for multiply, divide, shifts

### Memory-Mapped Index Registers

#### XR1 (Index Register 1)
- **Location**: Memory address /0001
- **Size**: 16 bits
- **Function**: Indexed addressing, loop counters
- **Tag Bits**: 01 selects XR1

#### XR2 (Index Register 2)
- **Location**: Memory address /0002
- **Size**: 16 bits
- **Function**: Indexed addressing, loop counters
- **Tag Bits**: 10 selects XR2

#### XR3 (Index Register 3)
- **Location**: Memory address /0003
- **Size**: 16 bits
- **Function**: Indexed addressing, loop counters
- **Tag Bits**: 11 selects XR3

**Note**: Index registers stored in main memory can be modified by both standard instructions and special index instructions (LDX, STX, MDX).

### Condition Indicators (Flags)

#### Carry Indicator (C)
- **Size**: 1 bit
- **Set By**: Arithmetic operations (add, subtract, shift)
- **Tested By**: Branch on Carry (BSC C)
- **Cleared**: Explicit clear or by LDS instruction

#### Overflow Indicator (O)
- **Size**: 1 bit
- **Set By**: Signed arithmetic overflow
- **Tested By**: Branch on Overflow (BSC O)
- **Cleared**: Explicit clear or by LDS instruction

### Register Summary Table

| Register | Size | Type | Purpose | Tag/Access |
|----------|------|------|---------|------------|
| IAR | 16-bit | Hardware | Program counter | Tag 00 (some ops) |
| ACC | 16-bit | Hardware | Accumulator | Direct access |
| EXT | 16-bit | Hardware | Extension | Direct access |
| XR1 | 16-bit | Memory /0001 | Index register 1 | Tag 01 |
| XR2 | 16-bit | Memory /0002 | Index register 2 | Tag 10 |
| XR3 | 16-bit | Memory /0003 | Index register 3 | Tag 11 |
| C | 1-bit | Hardware | Carry flag | Load/Store Status |
| O | 1-bit | Hardware | Overflow flag | Load/Store Status |

---

## Instruction Formats

The IBM 1130 uses two instruction formats: short (1 word) and long (2 words).

### Short Format (16 bits, 1 word)

```
┌─────────┬───┬─────┬──────────────┐
│ OP Code │ F │ Tag │ Displacement │
├─────────┼───┼─────┼──────────────┤
│  15-11  │10 │ 9-8 │     7-0      │
└─────────┴───┴─────┴──────────────┘
```

**Fields**:
- **OP Code** (bits 15-11): 5-bit operation code (32 possible opcodes)
- **F** (bit 10): Format bit, 0 = short format
- **Tag** (bits 9-8): Index register selection (00=IAR, 01=XR1, 10=XR2, 11=XR3)
- **Displacement** (bits 7-0): 8-bit signed displacement (-128 to +127)

**Characteristics**:
- Single-word instruction
- Displacement is relative to base address
- No indirect addressing available
- Compact, fast execution

### Long Format (32 bits, 2 words)

**Word 1** (Instruction word):
```
┌─────────┬───┬─────┬───┬──────────┬──────────┐
│ OP Code │ F │ Tag │ I │ Modifier │ Address  │
├─────────┼───┼─────┼───┼──────────┼──────────┤
│  15-11  │10 │ 9-8 │ 8*│   7-6*   │   5-0    │
└─────────┴───┴─────┴───┴──────────┴──────────┘
```

**Word 2** (Address continuation):
```
┌────────────────────────────────┐
│       Address (low bits)       │
├────────────────────────────────┤
│             15-0               │
└────────────────────────────────┘
```

*Bit assignment varies by instruction type

**Fields**:
- **OP Code** (bits 15-11): Operation code
- **F** (bit 10): Format bit, 1 = long format
- **Tag** (bits 9-8): Index register selection
- **I** (bit 8 in some instructions): Indirect addressing flag
- **Modifier** (bits 7-6 or similar): Instruction-specific modifiers
- **Address** (16 bits total from word 1 + word 2): Full address or displacement

**Characteristics**:
- Two-word instruction (occupies 2 memory locations)
- Direct addressing to full address space
- Indirect addressing supported (via I bit)
- Can be indexed and indirect simultaneously

### Format Selection Rules

| Instruction Type | Short Format | Long Format |
|-----------------|--------------|-------------|
| Most operations | Supported | Supported |
| LDS (Load Status) | Only format | Not supported |
| Some shifts | Only format | Not supported |
| Branch/Skip | Different semantics! | Different semantics! |

**Important**: For BSC (Branch or Skip), format determines behavior:
- **Short format**: Skip next instruction if condition true
- **Long format**: Branch to address if condition false (inverted logic!)

---

## Addressing Modes

Effective address calculation depends on format, tag bits, and indirect bit.

### Addressing Mode Summary

| Mode | Format | Tag | I | Formula | Notes |
|------|--------|-----|---|---------|-------|
| Relative | Short | 00 | - | EA = IAR + 1 + sign_extend(Disp) | IAR-relative |
| Indexed Short | Short | 01-11 | - | EA = XR[tag] + sign_extend(Disp) | Index-relative |
| Direct | Long | 00 | 0 | EA = Address | Absolute address |
| Indexed Long | Long | 01-11 | 0 | EA = XR[tag] + Address | Indexed absolute |
| Indirect | Long | 00 | 1 | EA = Memory[Address] | Indirect through memory |
| Indexed Indirect | Long | 01-11 | 1 | EA = Memory[XR[tag] + Address] | Index then indirect |

### Address Calculation Details

#### Short Format with Tag 00 (IAR-relative)
```
Base Address = IAR + 1  (IAR after instruction fetch)
Displacement = sign_extend(bits 7-0)
Effective Address = (Base Address + Displacement) & 0x7FFF
```

**Example**:
```
IAR = /0100
Instruction at /0100: LD . /20  (LD short format, disp=+32)
EA = /0100 + 1 + 32 = /0121
```

#### Short Format with Tag 01, 10, 11 (Indexed)
```
Base Address = XR[tag]
Displacement = sign_extend(bits 7-0)
Effective Address = (Base Address + Displacement) & 0x7FFF
```

**Example**:
```
XR1 = /1000
Instruction: A 1 /10  (Add short, XR1, disp=+16)
EA = /1000 + 16 = /1010
```

#### Long Format, Direct (I=0, Tag=00)
```
Effective Address = Address (16-bit value from words 1+2)
```

**Example**:
```
Instruction: LD L /2000
EA = /2000
```

#### Long Format, Indexed (I=0, Tag≠00)
```
Base Address = XR[tag]
Offset = Address (16-bit value)
Effective Address = (Base Address + Offset) & 0x7FFF
```

**Example**:
```
XR2 = /3000
Instruction: STO L2 /0100
EA = /3000 + /0100 = /3100
```

#### Long Format, Indirect (I=1, Tag=00)
```
Pointer Address = Address
Effective Address = Memory[Pointer Address]
```

**Example**:
```
Memory[/0500] = /2000
Instruction: LD I /0500
EA = Memory[/0500] = /2000
```

#### Long Format, Indexed Indirect (I=1, Tag≠00)
```
Base Address = XR[tag]
Offset = Address
Pointer Address = (Base Address + Offset) & 0x7FFF
Effective Address = Memory[Pointer Address]
```

**Example**:
```
XR1 = /0400
Memory[/0450] = /3000
Instruction: LD I1 /0050
Pointer = /0400 + /0050 = /0450
EA = Memory[/0450] = /3000
```

### Addressing Mode Notes

1. **Sign Extension**: Short format displacement is 8-bit signed, extended to 16 bits with sign preservation
2. **Address Masking**: Effective addresses masked with 0x7FFF to enforce 15-bit address space
3. **Indirect Levels**: Only one level of indirection supported (no auto-increment/decrement)
4. **IAR as Base**: Tag=00 uses IAR+1 for short format, 0 for long format

---

## Instruction Set Reference

### Instruction Categories

1. **Load and Store** (8 instructions): Data transfer operations
2. **Arithmetic** (6 instructions): Add, subtract, multiply, divide
3. **Logical** (3 instructions): Boolean operations
4. **Shift and Rotate** (8 variants): Bit manipulation
5. **Branch and Skip** (4 instructions): Program control
6. **Input/Output** (1 instruction): Device communication
7. **Control** (1 instruction): CPU control

### Load and Store Instructions

#### LD - Load Accumulator
- **Opcode**: 0x18 (binary 11000)
- **Formats**: Short, Long
- **Operation**: ACC ← Memory[EA]
- **Flags**: None affected
- **Example**: `LD . /50` loads from IAR+1+50

#### LDD - Load Double
- **Opcode**: 0x19 (binary 11001)
- **Formats**: Short, Long
- **Operation**: ACC ← Memory[EA], EXT ← Memory[EA+1]
- **Flags**: None affected
- **Note**: Loads two consecutive words

#### STO - Store Accumulator
- **Opcode**: 0x1A (binary 11010)
- **Formats**: Short, Long
- **Operation**: Memory[EA] ← ACC
- **Flags**: None affected
- **Example**: `STO L /2000` stores ACC to /2000

#### STD - Store Double
- **Opcode**: 0x1B (binary 11011)
- **Formats**: Short, Long
- **Operation**: Memory[EA] ← ACC, Memory[EA+1] ← EXT
- **Flags**: None affected
- **Note**: Stores two consecutive words

#### LDX - Load Index Register
- **Opcode**: 0x0C (binary 01100)
- **Formats**: Short, Long
- **Operation**: XR[tag] ← Memory[EA]
- **Flags**: None affected
- **Note**: Does NOT load ACC; loads index register selected by tag bits
- **Example**: `LDX 1 /100` loads XR1 from memory /100

#### STX - Store Index Register
- **Opcode**: 0x0D (binary 01101)
- **Formats**: Short, Long
- **Operation**: Memory[EA] ← XR[tag]
- **Flags**: None affected
- **Note**: Stores index register value to memory

#### LDS - Load Status
- **Opcode**: 0x04 (binary 00100)
- **Format**: Short only (immediate)
- **Operation**: C ← bit 2, O ← bit 3
- **Flags**: Carry and Overflow set from displacement bits
- **Example**: `LDS . /0C` sets C=1, O=1
- **Note**: Immediate operation, no memory access

#### STS - Store Status
- **Opcode**: 0x05 (binary 00101)
- **Formats**: Short, Long
- **Operation**: Memory[EA] ← (Memory[EA] & 0xFF03) | (C << 2) | (O << 3)
- **Flags**: Carry and Overflow cleared after store
- **Note**: Read-modify-write operation, preserves other bits

### Arithmetic Instructions

#### A - Add
- **Opcode**: 0x10 (binary 10000)
- **Formats**: Short, Long
- **Operation**: ACC ← ACC + Memory[EA]
- **Flags**: Carry set on unsigned overflow, Overflow set on signed overflow
- **Precision**: 16-bit addition
- **Example**: `A L /1500` adds memory /1500 to ACC

#### AD - Add Double
- **Opcode**: 0x11 (binary 10001)
- **Formats**: Short, Long
- **Operation**: (ACC,EXT) ← (ACC,EXT) + (Memory[EA],Memory[EA+1])
- **Flags**: Carry and Overflow
- **Precision**: 32-bit addition
- **Note**: Treats ACC+EXT as single 32-bit value

#### S - Subtract
- **Opcode**: 0x12 (binary 10010)
- **Formats**: Short, Long
- **Operation**: ACC ← ACC - Memory[EA]
- **Flags**: Carry (borrow), Overflow
- **Implementation**: Two's complement subtraction

#### SD - Subtract Double
- **Opcode**: 0x13 (binary 10011)
- **Formats**: Short, Long
- **Operation**: (ACC,EXT) ← (ACC,EXT) - (Memory[EA],Memory[EA+1])
- **Flags**: Carry (borrow), Overflow
- **Precision**: 32-bit subtraction

#### M - Multiply
- **Opcode**: 0x14 (binary 10100)
- **Formats**: Short, Long
- **Operation**: (ACC,EXT) ← ACC × Memory[EA] (signed)
- **Flags**: None (overflow not possible)
- **Precision**: 16-bit × 16-bit → 32-bit result
- **Note**: Signed multiplication, full 32-bit result in ACC+EXT
- **Timing**: Variable (Models 1-2: ~34μs, Model 3: ~19μs)

#### D - Divide
- **Opcode**: 0x15 (binary 10101)
- **Formats**: Short, Long
- **Operation**: ACC ← (ACC,EXT) ÷ Memory[EA], EXT ← remainder
- **Flags**: Overflow set if quotient doesn't fit in 16 bits
- **Precision**: 32-bit ÷ 16-bit → 16-bit quotient + 16-bit remainder
- **Special Cases**:
  - Divide by zero: Sets Overflow, ACC and EXT undefined
  - Quotient overflow: Sets Overflow, ACC and EXT unchanged
- **Timing**: Variable (Models 1-2: ~44μs, Model 3: ~24μs)

### Logical Instructions

#### AND - Boolean AND
- **Opcode**: 0x1C (binary 11100)
- **Formats**: Short, Long
- **Operation**: ACC ← ACC AND Memory[EA]
- **Flags**: None
- **Use**: Bit masking, clearing specific bits

#### OR - Boolean OR
- **Opcode**: 0x1D (binary 11101)
- **Formats**: Short, Long
- **Operation**: ACC ← ACC OR Memory[EA]
- **Flags**: None
- **Use**: Bit setting, combining bit fields

#### EOR - Exclusive OR
- **Opcode**: 0x1E (binary 11110)
- **Formats**: Short, Long
- **Operation**: ACC ← ACC XOR Memory[EA]
- **Flags**: None
- **Use**: Bit toggling, comparison

### Shift and Rotate Instructions

Shift instructions use opcode + modifier bits in displacement field to specify operation type.

**Encoding**: Bits 7-0 of instruction word:
- **Bits 7-6**: Shift type (0-3)
- **Bits 5-0**: Shift count (0-63)

#### SL - Shift Left (Opcode 0x02)

| Type | Name | Operation | Flags |
|------|------|-----------|-------|
| 0 | SLA | Shift Left ACC | Carry = last bit shifted out |
| 1 | SLCA | Shift Left and Count ACC | Shifts until ACC=0 or count=0, count in EXT |
| 2 | SLT | Shift Left ACC+EXT (double) | Carry = last bit shifted out |
| 3 | SLC | Shift Left and Count double | Shifts until ACC+EXT=0, count in modifier |

**Examples**:
- `SLA 5` - Shift ACC left 5 positions
- `SLT 16` - Shift 32-bit ACC+EXT left 16 positions

#### SR - Shift Right (Opcode 0x03)

| Type | Name | Operation | Flags |
|------|------|-----------|-------|
| 0 | SRA | Shift Right ACC (arithmetic) | Carry = last bit shifted out |
| 1 | - | Invalid | - |
| 2 | SRT | Shift Right ACC+EXT (arithmetic) | Carry = last bit shifted out |
| 3 | RTE | Rotate Right EXT | Circular rotation |

**Arithmetic Shifts**: Sign bit preserved (bit 15 remains unchanged)

**Examples**:
- `SRA 3` - Arithmetic shift ACC right 3 positions
- `RTE 8` - Rotate EXT right 8 positions

### Branch and Control Instructions

#### BSC - Branch or Skip on Condition
- **Opcode**: 0x09 (binary 01001)
- **Formats**: Short (skip), Long (branch)
- **Short Format**: Skip next instruction if condition is TRUE
- **Long Format**: Branch to address if condition is FALSE (inverted!)
- **Conditions** (specified in displacement/modifier):
  - **Z**: ACC = 0
  - **+**: ACC > 0 (positive, ACC ≠ 0 and bit 15 = 0)
  - **-**: ACC < 0 (negative, bit 15 = 1)
  - **E**: ACC is even (bit 0 = 0)
  - **C**: Carry = 0 (Carry is OFF)
  - **O**: Overflow = 0 (Overflow is OFF)

**Examples**:
- `BSC . O /0000` - Short: Skip next if Overflow OFF
- `BSC L O /2000` - Long: Branch to /2000 if Overflow ON (inverted!)

#### BOSC - Branch Out or Skip on Condition
- **Opcode**: 0x08 (binary 01000)
- **Formats**: Short, Long
- **Operation**: Same as BSC, but also exits interrupt level
- **Use**: Return from interrupt handlers with conditional logic

#### BSI - Branch and Store IAR
- **Opcode**: 0x0A (binary 01010)
- **Formats**: Long only (short format invalid)
- **Operation**:
  1. Memory[EA] ← IAR (save return address)
  2. IAR ← EA + 1 (branch to subroutine body)
- **Use**: Subroutine calls
- **Return**: Execute `BR *SUBR` (indirect branch through stored IAR)

**Example**:
```assembly
      BSI L SUBR    ; Call subroutine, return address stored at SUBR
      ...           ; Return here after subroutine
SUBR  DC  0         ; Return address storage
      ...           ; Subroutine body
      BR  *SUBR     ; Return (indirect branch)
```

#### MDX - Modify Index and Skip
- **Opcode**: 0x0B (binary 01011)
- **Formats**: Short, Long
- **Operation**:
  1. XR[tag] ← XR[tag] + Memory[EA]
  2. Skip next instruction if XR[tag] = 0 OR sign changed
- **Use**: Loop control with index modification
- **Flags**: None

**Example** (count down loop):
```assembly
      LDX 1 /COUNT   ; Load loop counter into XR1
LOOP  ...            ; Loop body
      MDX 1 /MINUS1  ; Decrement XR1, skip if zero
      BR  L LOOP     ; Continue loop
      ...            ; Exit when count reaches 0
MINUS1 DC  -1
COUNT  DC  100
```

#### WAIT - Wait
- **Opcode**: 0x06 (binary 00110)
- **Format**: No operands
- **Operation**: Halt CPU until interrupt received
- **Use**: Idle waiting for I/O completion
- **Resume**: Interrupt handler executes, returns to instruction after WAIT

### Input/Output Instruction

#### XIO - Execute I/O Operation
- **Opcode**: 0x01 (binary 00001)
- **Formats**: Short, Long
- **Operation**: Execute I/O Channel Command (IOCC) at EA
- **IOCC Format** (2-word structure at EA):
  - **Word 1 (WCA)**: Working Control Address (buffer location)
  - **Word 2 (IOCC)**: I/O command
    - Bits 15-11: Device code
    - Bits 10-8: Function (0=Sense, 1=Control, 2=InitRead, 4=InitWrite)
    - Bits 7-0: Device-specific modifiers

**Example**:
```assembly
      XIO L RDIOCC   ; Execute I/O command
RDIOCC DC  BUFFER    ; WCA - buffer address
       DC  /4208     ; IOCC - Device 21 (2501), InitRead, modifiers 08
BUFFER BSS 80        ; 80-word buffer for card data
```

### Timing Information

Instruction execution times vary significantly by model and addressing mode:

| Instruction | Model 1-2 (3.6μs) | Model 3 (2.2μs) | Notes |
|-------------|-------------------|-----------------|-------|
| LD (short) | ~5.8 μs | ~3.6 μs | Base + 1 cycle |
| LD (long) | ~7.2 μs | ~4.4 μs | Base + 2 cycles |
| LD (indirect) | ~10.8 μs | ~6.6 μs | Add 1 cycle |
| A (short) | ~7.2 μs | ~4.4 μs | Read + add |
| M | ~34 μs | ~19 μs | Variable time |
| D | ~44 μs | ~24 μs | Variable time |
| SLA (n bits) | Base + n cycles | Base + n cycles | Variable |

**Add 1 memory cycle for**:
- Indirect addressing
- Each additional word in instruction
- Each memory access in operation

---

## Interrupt System

The IBM 1130 implements a 6-level priority interrupt system with hardware interrupt handlers.

### Interrupt Levels and Priorities

| Level | Priority | Typical Devices | Handler Address |
|-------|----------|----------------|-----------------|
| 0 | Highest | IBM 1442 card reader (column ready) | /0008 |
| 1 | High | IBM 1132 printer, Sync Comm | /0009 |
| 2 | Medium-High | Disk storage, Storage Access Channel | /000A |
| 3 | Medium | IBM 1627 plotter, SAC | /000B |
| 4 | Medium-Low | Paper tape, console, 1442, printer, OMR | /000C |
| 5 | Lowest | Console entry switches, SAC | /000D |

### Interrupt Processing

#### Interrupt Recognition
1. Device signals interrupt request
2. CPU checks interrupt enable status
3. If enabled, CPU completes current instruction
4. CPU compares interrupt level to current level
5. If new interrupt has higher priority, processing begins

#### Interrupt Entry Sequence
1. Save current IAR to memory location /0000 + level
2. Load new IAR from handler address (/0008-/000D)
3. Set current interrupt level to new level
4. Continue execution at handler address

#### Interrupt Return
- **BSC** instruction: Returns and re-enables interrupts
- **BOSC** instruction: Same as BSC but exits interrupt level

### Interrupt Masking

Interrupts can be selectively enabled/disabled:
- Individual device interrupt enable/disable
- Global interrupt enable/disable
- Level-based masking (higher priority interrupts can interrupt lower)

### Nested Interrupts

- Higher-priority interrupts can interrupt lower-priority handlers
- Current interrupt level tracked by hardware
- IAR saved in level-specific location for nesting
- Maximum nesting depth = 6 levels

### Cycle Stealing

Some devices use "cycle stealing" for high-speed data transfer:
- Device takes control of memory bus during unused CPU cycles
- No interrupt required for each word transferred
- Transparent to CPU (slight performance impact)
- Used by: Disk drives, Storage Access Channel

---

## I/O System

### I/O Architecture

The IBM 1130 uses programmed I/O with a single I/O instruction (XIO) and interrupt-driven completion.

### I/O Channel Command (IOCC)

Two-word structure in memory:

**Word 1 - WCA (Working Control Address)**:
- Buffer start address in memory
- Used by device for data transfer

**Word 2 - IOCC (I/O Channel Command)**:
```
┌──────────┬──────────┬───────────────┐
│  Device  │ Function │   Modifiers   │
│  Code    │          │               │
├──────────┼──────────┼───────────────┤
│  15-11   │   10-8   │      7-0      │
└──────────┴──────────┴───────────────┘
```

### Device Codes

| Code | Decimal | Device |
|------|---------|--------|
| /00 | 0 | Console keyboard |
| /01 | 1 | Console printer |
| /02 | 2 | Console entry switches |
| /03 | 3 | (Reserved) |
| /04 | 4 | 2501 Card Reader |
| /05 | 5 | 1442 Card Read Punch |
| /06 | 6 | 1134/1055 Paper Tape |
| /07 | 7 | 1627 Plotter |
| /08 | 8 | 2310/2311 Disk Drive |
| /10 | 16 | 1132 Printer |
| /11 | 17 | 1403 Printer |
| /12 | 18 | Synchronous Comm |
| /14 | 20 | 1231 Optical Mark |

### Function Codes

| Code | Binary | Function | Description |
|------|--------|----------|-------------|
| 0 | 000 | Sense Device | Read device status into ACC |
| 1 | 001 | Control | Device-specific control operation |
| 2 | 010 | Initiate Read | Start data transfer from device to memory |
| 4 | 100 | Initiate Write | Start data transfer from memory to device |

### I/O Operation Sequence

1. **Setup**: Program prepares buffer and IOCC structure
2. **Initiate**: Execute `XIO` instruction pointing to IOCC
3. **Device Activation**: Device begins operation (async)
4. **Program Continues**: CPU executes next instructions
5. **Completion**: Device signals interrupt when done
6. **Handler**: Interrupt handler processes completion

### Sense Device Status

Each device returns device-specific status word in ACC when Function=0 (Sense):

**Common Status Bits**:
- **Bit 15**: Error/Exception
- **Bit 14**: Operation complete
- **Bit 13**: Not ready
- **Bit 12**: Busy
- **Bit 0**: Device-specific

Programs typically sense device before initiating I/O to check ready status.

---

## Disk Storage System

### IBM 2310 Disk Drive

The primary mass storage device for the IBM 1130.

#### Physical Characteristics

- **Cartridge**: IBM 2315 removable disk pack
- **Capacity**: 512,000 16-bit words (1,024,000 bytes, ~1 MB)
- **Surfaces**: 2 (top and bottom of single platter)
- **Cylinders**: Variable (typically 200)
- **Tracks per cylinder**: 2 (one per surface)
- **Sectors per track**: 8
- **Total sectors**: ~3,200 sectors

#### Sector Organization

- **Sector size**: 321 words
  - 1 word: Sector address (cylinder + sector ID)
  - 320 words: Data
- **Sector ID format**:
  - Bits 15-11: Unused
  - Bits 10-3: Cylinder number (0-199)
  - Bits 2-0: Sector number (0-7)

#### Data Transfer

- **Transfer rate**: 35,000 words per second (70 KB/s)
- **Method**: Cycle stealing (transparent to CPU)
- **Latency**: Rotational delay + seek time
- **Seek time**: Variable (cylinder-to-cylinder: ~25ms, average: ~75ms)

#### Disk Functions

**Function 0 - Sense Device**:
Returns status word in ACC:
- **Bit 15**: Data error (parity, check)
- **Bit 14**: Operation complete
- **Bit 13**: Not ready (no cartridge mounted)
- **Bit 12**: Busy (operation in progress)
- **Bit 11**: At cylinder zero
- **Bits 1-0**: Next sector ID

**Function 1 - Control**:
Modifiers specify control operation:
- **Bit 7**: Read check enable
- **Bits 0-7**: Seek direction and magnitude
  - Positive value: Seek toward higher cylinders
  - Negative value: Seek toward lower cylinders

**Function 2 - Initiate Read**:
- Reads current sector into buffer at WCA
- Transfers 320 words (sector address not transferred)
- Sets interrupt when complete
- If Read Check enabled: verifies data matches memory after transfer

**Function 4 - Initiate Write**:
- Writes 320 words from buffer at WCA to current sector
- Updates sector address field
- Sets interrupt when complete

#### Disk File System

DM2 divides disk into areas:

1. **System Area** (Cylinder 0-n):
   - DM2 monitor supervisor
   - Resident system programs
   - Compilers and assemblers
   - Protected from user access

2. **Fixed Area**:
   - User programs in core-image format
   - Directly loadable into memory
   - Cataloged by name

3. **User Area**:
   - General user files
   - Any format
   - Requires explicit I/O

4. **Working Storage**:
   - Temporary files
   - Scratch space
   - Freed on job termination

---

## Peripheral Devices

### Standard Devices

#### IBM 1053 Console Printer/Keyboard
- **Type**: Selectric typewriter mechanism
- **Speed**: 15.5 characters per second
- **Character Set**: 48 characters
- **Functions**: Operator communication, program output
- **Device Codes**: /00 (keyboard), /01 (printer)

#### Console Entry Switches
- **Count**: 16 switches
- **Function**: Manual data entry, program control
- **Access**: Via XIO Sense function
- **Device Code**: /02
- **Use**: Toggle switches for entering addresses, data

### Card Equipment

#### IBM 1442 Card Read Punch
- **Read speed**: 400 cards per minute
- **Punch speed**: 160 cards per minute
- **Capacity**: 1,200-card hopper
- **Format**: 80-column cards
- **Transfer**: Column binary or EBCDIC
- **Device Code**: /05
- **Interrupts**: Level 0 (column ready), Level 4 (card ready)

#### IBM 2501 Card Reader
- **Speed**: 600 or 1,000 cards per minute
- **Capacity**: 1,200-card hopper
- **Format**: 80-column cards
- **Transfer**: Column binary or EBCDIC
- **Device Code**: /04
- **Interrupt**: Level 4

### Printers

#### IBM 1132 Printer
- **Type**: Line printer
- **Speed**: 40, 60, or 80 lines per minute
- **Width**: 120 print positions
- **Character Set**: 48 or 52 characters
- **Device Code**: /10
- **Interrupt**: Level 1

#### IBM 1403 Printer
- **Type**: High-speed line printer
- **Speed**: 340, 600, or 1,100 lines per minute
- **Width**: 132 print positions
- **Character Set**: 48, 52, or 64 characters
- **Device Code**: /11
- **Use**: High-volume output

### Other Peripherals

#### IBM 2311 Disk Drive
- **Capacity**: 2.9 million words per drive
- **Type**: Removable disk pack (larger than 2315)
- **Device Code**: /08 (shared with 2310)

#### IBM 2415 Magnetic Tape
- **Speed**: 18.75 inches per second
- **Density**: 800 BPI
- **Transfer rate**: 15,000 bytes per second
- **Format**: 7-track or 9-track
- **Available**: From 1968

#### IBM 2250 Graphics Display
- **Type**: Vector graphics CRT
- **Resolution**: 1024x1024 addressable points
- **Features**: Light pen, function keyboard
- **Use**: CAD, interactive graphics

#### IBM 1627 Plotter
- **Type**: Drum plotter
- **Size**: 11" wide paper
- **Resolution**: 0.01 inch steps
- **Speed**: 300 steps per second
- **Device Code**: /07

---

## Operating System

### Disk Monitor System Version 2 (DM2)

The standard operating system for disk-equipped IBM 1130 systems.

#### System Characteristics

- **Type**: Single-task, batch-oriented
- **Supervisor Size**: 1,020 bytes (510 words)
- **Memory Resident**: Lower memory (/0000-/01FD)
- **First Available User Memory**: /01FE (word 510)
- **Job Control**: Card-based with `//` and `*` control records

#### Control Record Format

**Job Control Records** (begin with `//`):
```
// JOB
// FOR     (Fortran compile)
// ASM     (Assembly)
// XEQ     (Execute)
// DUP     (Disk Utility Program)
```

**Supervisor Control Records** (begin with `*`):
```
* LIST SOURCE PROGRAM
* IOCS (typewriter)
* STORE WS filename
```

#### System Functions

1. **Job Scheduling**: Sequential batch processing
2. **I/O Management**: Standardized device I/O via IOCS
3. **Memory Management**: Simple allocation (single program)
4. **File Management**: Cataloged files on disk
5. **Program Loading**: Core-image loader

#### IOCS (Input/Output Control System)

Subroutine library providing device-independent I/O:
- **IOCP**: Print a line
- **IOCR**: Read a card
- **IOCW**: Write a card
- **IOCD**: Read disk sector
- **IOCH**: Write disk sector

Programs link with IOCS to simplify I/O programming.

#### Typical Job Stream

```
// JOB
// FOR
*IOCS(TYPEWRITER,CARD)
 (source program cards)
// XEQ
 (data cards)
```

### Programming Languages

**Standard Languages**:
- **FORTRAN IV**: Primary scientific language
- **Assembler**: System programming, performance-critical code
- **RPG**: Report generation (business use)

**Available Compilers**:
- **APL**: Interactive mathematical language (with 2250 display)
- **PL/I**: General-purpose language subset

---

## References

### Primary Documentation

1. **IBM 1130 Functional Characteristics**
   - Document: A26-5881-2 (1966)
   - URL: https://bitsavers.trailing-edge.com/pdf/ibm/1130/functional_characteristics/A26-5881-2_1130_Functional_Characteristics_1966.pdf
   - Alternate: http://media.ibm1130.org/E0006.pdf
   - HTML Version: https://ibm1130.net/functional/Introduction.html

2. **IBM 1130 Assembler Language Manual**
   - Document: C26-5927-2 (1966)
   - URL: https://bitsavers.org/pdf/ibm/1130/lang/C26-5927-2_1130_Assembler_Language_1966.pdf
   - Alternate: http://media.ibm1130.org/E0023.pdf

3. **IBM 1130 System Summary**
   - Document: GA26-5917-9 (December 1971)
   - URL: http://www.bitsavers.org/pdf/ibm/1130/GA26-5917-9_1130_System_Summary_Dec71.pdf

4. **IBM 1130 Disk Monitor System Programming and Operators Guide**
   - Document: C26-3717-1 (1968)
   - URL: http://www.bitsavers.org/pdf/ibm/1130/monitor/C26-3717-1_1130_Disk_Monitor_System_Version_2_Programming_and_Operators_Guide_1968.pdf

### Online Resources

1. **Wikipedia - IBM 1130**
   - URL: https://en.wikipedia.org/wiki/IBM_1130
   - Comprehensive historical and technical overview

2. **IBM1130.org**
   - URL: http://ibm1130.org/
   - Complete manual library and preservation project
   - Manual Library: http://ibm1130.org/lib/manuals/

3. **IBM1130.net**
   - URL: https://ibm1130.net/
   - HTML-formatted technical documentation
   - Functional Characteristics: https://ibm1130.net/functional/Introduction.html
   - CPU Instructions: https://ibm1130.net/functional/CPUinstructions.html

4. **Bitsavers.org - IBM 1130 Collection**
   - URL: http://bitsavers.trailing-edge.com/pdf/ibm/1130/
   - Complete archive of IBM 1130 documentation

### Additional Reading

1. **The National Museum of Computing - IBM 1130**
   - URL: https://www.tnmoc.org/ibm1130
   - Restored working system

2. **Technikum29 - IBM 1130**
   - URL: https://www.technikum29.de/en/computer/ibm1130.php
   - Operational restoration details

---

## Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-24 | Research Team | Initial compilation from IBM documentation and Wikipedia |

---

**End of Document**
