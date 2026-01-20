# S1130 IBM 1130 Emulator - Internal Operation Guide

## Table of Contents
1. [Overview](#overview)
2. [CPU Architecture](#cpu-architecture)
3. [Instruction Execution Cycle](#instruction-execution-cycle)
4. [Memory System](#memory-system)
5. [Device Architecture](#device-architecture)
6. [Interrupt System](#interrupt-system)
7. [Instruction Set Implementation](#instruction-set-implementation)
8. [Testing Architecture](#testing-architecture)
9. [Common Operations](#common-operations)
10. [Debugging Features](#debugging-features)

---

## Overview

The S1130 is a complete emulator of the IBM 1130 minicomputer system written in C# .NET 8. It accurately simulates:
- 16-bit CPU with accumulator, extension, and index registers
- 32K words of memory (default, configurable)
- Complete instruction set (arithmetic, logical, branch, shift, I/O)
- I/O device system with interrupt handling
- Device-specific operations (card readers, disk drives)

**Important Note:** This is a **functional emulator**, not a cycle-accurate emulator. It reproduces the logical behavior and results of IBM 1130 programs but does not simulate timing at the cycle level. Instructions execute immediately rather than taking the historically accurate number of machine cycles.

**Key Design Principles:**
- Accuracy: Faithfully reproduce IBM 1130 behavior
- Testability: Comprehensive unit test coverage (395+ tests)
- Extensibility: Easy to add new instructions and devices
- Performance: Execute millions of instructions per second

---

## CPU Architecture

### Core Components

The `Cpu` class is the heart of the emulator, implementing the `ICpu` interface:

```csharp
public class Cpu : ICpu
{
    // Registers
    public ushort Acc { get; set; }      // Accumulator (16-bit)
    public ushort Ext { get; set; }      // Extension (16-bit)
    public ushort Iar { get; set; }      // Instruction Address Register (Program Counter)
    public IndexRegisters Xr { get; }    // Index Registers XR1, XR2, XR3
    
    // Status Flags
    public bool Carry { get; set; }      // Carry flag
    public bool Overflow { get; set; }   // Overflow flag
    public bool Wait { get; set; }       // Wait state (halted)
    
    // Memory
    public ushort[] Memory { get; set; } // Main memory array
    public int MemorySize { get; set; }  // Size in words (default 32768)
    
    // Instruction Decode State
    public ushort Opcode { get; set; }
    public bool FormatLong { get; set; }
    public ushort Tag { get; set; }
    public ushort Displacement { get; set; }
    public bool IndirectAddress { get; set; }
    public ushort Modifiers { get; set; }
}
```

### Register Details

#### Accumulator (Acc)
- Primary 16-bit register for arithmetic and logical operations
- Destination for most load operations
- Source for most store operations
- Combined with Ext for 32-bit operations (AccExt property)

#### Extension Register (Ext)
- Used for double-precision arithmetic (32-bit)
- Lower 16 bits of multiplication results
- Upper 16 bits of division operands
- Right operand for double-precision add/subtract

#### Instruction Address Register (Iar)
- Program counter pointing to next instruction
- Automatically incremented during instruction fetch
- Modified by branch instructions
- Saved/restored during interrupt handling

#### Index Registers (XR1, XR2, XR3)
- Three 16-bit index registers for array/table access
- Selected by Tag field (bits 8-9 of instruction)
- Added to displacement for effective address calculation
- Accessed via `cpu.Xr[1]`, `cpu.Xr[2]`, `cpu.Xr[3]`
- Memory-mapped: XR1=0x0001, XR2=0x0002, XR3=0x0003

### Memory Architecture

**Specifications:**
- Word-addressable: Each address references a 16-bit word
- Default size: 32,768 words (64KB)
- Address range: 0x0000 to 0x7FFF
- Big-endian bit numbering (bit 0 = MSB, bit 15 = LSB)

**Access Methods:**
```csharp
// Indexer access with bounds checking
cpu[0x100] = 0x1234;
ushort value = cpu[0x100];

// Direct array access (for performance-critical code)
cpu.Memory[0x100] = 0x1234;

// 32-bit combined access
cpu.AccExt = 0x12345678;  // Sets Acc=0x1234, Ext=0x5678
```

**Special Memory Locations:**
- `0x0000`: Reserved
- `0x0001`: XR1 (index register 1)
- `0x0002`: XR2 (index register 2)
- `0x0003`: XR3 (index register 3)
- `0x0004-0x0007`: Reserved
- `0x0008`: Interrupt level 0 vector
- `0x0009`: Interrupt level 1 vector
- `0x000A`: Interrupt level 2 vector
- `0x000B`: Interrupt level 3 vector
- `0x000C`: Interrupt level 4 vector
- `0x000D`: Interrupt level 5 vector

---

## Instruction Execution Cycle

### Fetch-Decode-Execute Loop

The CPU operates in a continuous cycle:

```csharp
while (!cpu.Wait)
{
    cpu.NextInstruction();      // Fetch and decode
    cpu.ExecuteInstruction();   // Execute and handle interrupts
}
```

### 1. Instruction Fetch (NextInstruction)

**Process:**
```csharp
public void NextInstruction()
{
    // 1. Fetch first word from memory at IAR
    var firstWord = Memory[Iar++];
    
    // 2. Extract opcode (bits 0-4, shifted to position 11-15)
    Opcode = (ushort)((firstWord & 0xF800) >> 11);
    
    // 3. Look up instruction implementation
    CurrentInstruction = Instructions[Opcode];
    
    // 4. Extract format bit (bit 5)
    var formatBit = (firstWord & 0x0400) != 0;
    
    // 5. Extract tag (index register selector, bits 6-7)
    Tag = (ushort)((firstWord & 0x0300) >> 8);
    
    // 6. Extract modifiers/displacement (bits 8-15)
    Modifiers = (ushort)(firstWord & 0xff);
    
    // 7. Determine format and fetch second word if needed
    if (formatBit && CurrentInstruction.HasLongFormat)
    {
        FormatLong = true;
        Displacement = Memory[Iar++];           // Fetch second word
        IndirectAddress = (firstWord & 0x80) != 0;  // Bit 8 of first word
    }
    else
    {
        FormatLong = false;
        Displacement = Modifiers;               // Use low 8 bits
        IndirectAddress = false;
    }
}
```

**Instruction Format:**

**Short Format (1 word):**
```
Bit:  0-4   5   6-7   8-15
     [OP ] [0] [TAG] [DISP]
```
- OP: Opcode (5 bits)
- 0: Format bit (0 = short)
- TAG: Index register (00=none, 01=XR1, 10=XR2, 11=XR3)
- DISP: 8-bit signed displacement (-128 to +127)

**Long Format (2 words):**
```
Word 1:  0-4   5   6-7   8    9-15
        [OP ] [1] [TAG] [I] [unused]

Word 2:  0-15
        [ADDRESS]
```
- OP: Opcode (5 bits)
- 1: Format bit (1 = long)
- TAG: Index register selector
- I: Indirect addressing bit (bit 8)
- ADDRESS: 16-bit address (word 2)

### 2. Instruction Execution

**Process:**
```csharp
public void ExecuteInstruction()
{
    if (CurrentInstruction != null)
    {
        CurrentInstruction.Execute(this);  // Execute instruction
    }
    else
    {
        Wait = true;  // Invalid opcode = WAIT
    }
    
    HandleInterrupt();  // Process any pending interrupts
    _count++;           // Increment instruction counter
}
```

### 3. Address Calculation

All instructions use `InstructionBase.GetEffectiveAddress()` for consistent addressing:

```csharp
protected ushort GetEffectiveAddress(ICpu cpu)
{
    ushort address;
    
    // Calculate base address from displacement
    if (cpu.FormatLong)
    {
        address = cpu.Displacement;  // Direct 16-bit address
    }
    else
    {
        // Sign-extend 8-bit displacement and add to IAR
        int signedDisp = SignExtend(cpu.Displacement);
        address = (ushort)(cpu.Iar + signedDisp);
    }
    
    // Apply index register offset
    address = (ushort)(address + GetOffset(cpu));
    
    // Apply indirect addressing if specified
    if (cpu.IndirectAddress)
    {
        address = cpu[address];  // Follow pointer
    }
    
    return address;
}

protected int GetOffset(ICpu cpu)
{
    // Tag: 0=none, 1=XR1, 2=XR2, 3=XR3
    return cpu.Tag == 0 ? 0 : cpu.Xr[cpu.Tag];
}
```

**Example Address Calculations:**

```
Short format: LD 5
  - Displacement = 5 (unsigned)
  - Sign-extend: 0x0005
  - IAR = 0x0100
  - Effective = 0x0100 + 0x0005 = 0x0105

Short format: LD -3
  - Displacement = 0xFD (253 unsigned)
  - Sign-extend: 0xFFFD (-3 signed)
  - IAR = 0x0100
  - Effective = 0x0100 + 0xFFFD = 0x00FD

Long format: LD L 0x400
  - Displacement = 0x0400
  - Effective = 0x0400 (no IAR addition)

With index: LD 5,X1
  - Base = IAR + 5
  - XR1 = 0x0010
  - Effective = Base + 0x0010

Indirect: LD L 0x400 I
  - Address = 0x0400
  - Value at 0x400 = 0x0500
  - Effective = 0x0500 (follow pointer)
```

---

## Device Architecture

### Two Device Types

The IBM 1130 has two fundamentally different device architectures:

#### 1. Block-Mode Devices (DMA-like)
**Examples:** 2501 Card Reader, 2310 Disk Drive

**Characteristics:**
- CPU issues single command (InitRead/InitWrite)
- Device automatically transfers entire block
- Uses IOCC (I/O Channel Command) structure
- Generates single completion interrupt
- Minimal CPU involvement during transfer

**IOCC Structure (2 words):**
```
Word 1 (even address):  WCA - Word Count Address
                        Points to word count and data buffer

Word 2 (odd address):   Device Code (bits 0-4)
                        Function (bits 5-7)
                        Modifiers (bits 8-15)
```

**Operation Sequence:**
```
1. CPU: Set up IOCC structure in memory
2. CPU: Execute XIO instruction pointing to IOCC
3. Device: Read IOCC, start transfer
4. Device: Transfer data automatically (cycle steal)
5. Device: Generate Level 4 interrupt when complete
6. CPU: Process interrupt, check status
```

#### 2. Character-Mode Devices (CPU-Intensive)
**Examples:** 1442 Card Read Punch

**Characteristics:**
- CPU must issue command for EACH character
- Device generates interrupt for each character ready
- IoccAddress used as direct memory address (not IOCC pointer)
- High CPU overhead, strict timing requirements
- Must respond within 700-800µs

**Operation Sequence (1442 Read):**
```
1. CPU: Execute XIO Control Start Read
2. Device: Dequeue card, start moving through reader
3. Device: Generate Level 0 interrupt (first column ready)
4. CPU: Execute XIO Read with memory address for column 0
5. Device: Transfer column 0, generate next interrupt
6. Repeat steps 4-5 for columns 1-79 (80 total)
7. Device: Generate Level 4 interrupt (operation complete)
```

**Code Example:**
```csharp
// Character-mode Read implementation
case DevFunction.Read:
    if (_readInProgress && _currentColumn < 80)
    {
        // Transfer ONE column to memory
        CpuInstance[CpuInstance.IoccAddress] = _currentCard.Columns[_currentColumn];
        _currentColumn++;
        
        if (_currentColumn < 80)
        {
            // Generate interrupt for next column
            ActivateInterrupt(CpuInstance, 0, ReadIlsw);
        }
        else
        {
            // All columns read - signal completion
            CompleteReadOperation();
        }
    }
    break;
```

### Device Base Class

All devices inherit from `DeviceBase`:

```csharp
public abstract class DeviceBase : IDevice
{
    public abstract byte DeviceCode { get; }  // Unique 5-bit identifier
    public ICpu CpuInstance { get; set; }
    public Interrupt ActiveInterrupt { get; set; }
    
    // Device must implement this
    public abstract void ExecuteIocc();
    
    // Interrupt management helpers
    protected void ActivateInterrupt(ICpu cpu, int level, ushort ilsw)
    {
        if (ActiveInterrupt == null)
        {
            ActiveInterrupt = cpu.IntPool.GetInterrupt(level, ilsw, this);
            cpu.AddInterrupt(ActiveInterrupt);
        }
    }
    
    protected void DeactivateInterrupt(ICpu cpu)
    {
        if (ActiveInterrupt != null)
        {
            // Interrupt will be cleared on BOSC instruction
            ActiveInterrupt = null;
        }
    }
}
```

### XIO Instruction and IOCC Decoding

**XIO (Execute I/O) Instruction:**
```csharp
public class ExecuteInputOutput : InstructionBase, IInstruction
{
    public void Execute(ICpu cpu)
    {
        var address = GetEffectiveAddress(cpu);
        cpu.IoccDecode(address);          // Decode IOCC structure
        if (cpu.IoccDevice != null)
        {
            cpu.IoccDevice.ExecuteIocc(); // Execute device command
        }
    }
}
```

**IOCC Decoding:**
```csharp
public void IoccDecode(int address)
{
    // Read first word (WCA - Word Count Address)
    IoccAddress = Memory[address];
    
    // Read second word (force odd address using bitwise OR)
    ushort secondWord = Memory[address | 1];
    
    // Extract fields from second word
    IoccDeviceCode = (secondWord & 0xf800) >> 11;    // Bits 0-4
    IoccFunction = (DevFunction)((secondWord & 0x0700) >> 8); // Bits 5-7
    IoccModifiers = secondWord & 0xff;                // Bits 8-15
    
    // Look up device
    IoccDevice = Devices[IoccDeviceCode];
}
```

**Note on `address | 1`:**
This forces the second word to be read from an odd address:
- If address is even (0x100): `0x100 | 1 = 0x101` (correct)
- If address is odd (0x101): `0x101 | 1 = 0x101` (same - reads twice, validates error)

This pattern validates that IOCC structures are properly aligned on even addresses.

---

## Interrupt System

### Overview

The IBM 1130 has a 6-level priority interrupt system:
- **Level 0**: Highest priority (device response)
- **Levels 1-4**: Device operations
- **Level 5**: Lowest priority (console)

### Interrupt Queues

```csharp
private readonly ConcurrentQueue<Interrupt>[] _interruptQueues;  // 6 queues (0-5)
private readonly ConcurrentStack<Interrupt> _currentInterrupts;   // Nested interrupts
```

### Interrupt Processing

**Adding an Interrupt:**
```csharp
public void AddInterrupt(Interrupt interrupt)
{
    var level = interrupt.InterruptLevel;
    if (level >= 0 && level <= 5)
    {
        _interruptQueues[level].Enqueue(interrupt);
        Interlocked.Increment(ref _activeInterruptCount);
    }
}
```

**Handling Interrupts:**
```csharp
public void HandleInterrupt()
{
    if (_activeInterruptCount == 0) return;
    
    // Find highest priority active interrupt
    int? currentLevel = CurrentInterruptLevel;
    if (!currentLevel.HasValue) return;
    
    int level = currentLevel.Value;
    
    // Check if current interrupt allows this level
    if (!ShouldHandleInterrupt(level)) return;
    
    // Get interrupt vector address
    var vectorAddr = Constants.InterruptVectors[level];
    
    // Get interrupt object
    Interrupt interrupt;
    if (_interruptQueues[level].TryPeek(out interrupt))
    {
        // Push onto current interrupt stack
        CurrentInterrupt.Push(interrupt);
        
        // Get handler address from vector
        var handlerAddr = Memory[vectorAddr];
        
        // Save return address
        Memory[handlerAddr] = Iar;
        
        // Jump to handler (return address + 1)
        Iar = (ushort)(handlerAddr + 1);
    }
}
```

**Interrupt Vectors:**
```
Level 0: Address 0x0008 (8)
Level 1: Address 0x0009 (9)
Level 2: Address 0x000A (10)
Level 3: Address 0x000B (11)
Level 4: Address 0x000C (12)
Level 5: Address 0x000D (13)
```

**Vector Structure:**
Each vector contains the address of a two-word handler structure:
```
[Vector Address]:     Address of handler structure
[Handler + 0]:        Return address (saved by hardware)
[Handler + 1]:        First instruction of handler
```

**Clearing Interrupts:**

The BOSC (Branch Out and Skip on Condition) instruction clears interrupts:
```csharp
public void ClearCurrentInterrupt()
{
    Interrupt intr;
    if (CurrentInterrupt.TryPop(out intr))
    {
        if (intr.CausingDevice.ActiveInterrupt != intr)
        {
            // Interrupt no longer active on device
            _interruptQueues[intr.InterruptLevel].TryDequeue(out intr);
            IntPool.PutInterruptInBag(intr);  // Return to pool
            Interlocked.Decrement(ref _activeInterruptCount);
        }
    }
}
```

### Interrupt Pool

Interrupts are pooled objects to avoid allocation overhead:

```csharp
public class InterruptPool
{
    private ConcurrentBag<Interrupt> _interruptBag;
    
    public Interrupt GetInterrupt(int level, ushort ilsw, IDevice device)
    {
        Interrupt interrupt;
        if (!_interruptBag.TryTake(out interrupt))
        {
            interrupt = new Interrupt();  // Create new if pool empty
        }
        
        interrupt.InterruptLevel = level;
        interrupt.Ilsw = ilsw;
        interrupt.CausingDevice = device;
        return interrupt;
    }
    
    public void PutInterruptInBag(Interrupt interrupt)
    {
        _interruptBag.Add(interrupt);  // Return to pool
    }
}
```

---

## Instruction Set Implementation

### Instruction Base Class

All instructions extend `InstructionBase`:

```csharp
public abstract class InstructionBase
{
    // Address calculation helpers
    protected ushort GetEffectiveAddress(ICpu cpu) { ... }
    protected ushort GetEffectiveAddressNoXr(ICpu cpu) { ... }
    protected int GetOffset(ICpu cpu) { ... }
    
    // Sign extension for 8-bit displacements
    protected int SignExtend(ushort displacement)
    {
        return (displacement & 0x80) != 0 
            ? (int)(displacement | 0xFF00)  // Negative
            : (int)displacement;             // Positive
    }
    
    // Bit manipulation constants
    protected const uint Mask16 = 0xFFFF;
    protected const uint Mask32 = 0xFFFFFFFF;
}
```

### Instruction Categories

#### 1. Load/Store Instructions
**Examples:** LD, LDD, STO, STD, LDX, STX

```csharp
public class Load : InstructionBase, IInstruction
{
    public OpCodes OpCode => OpCodes.Load;
    public string OpName => "LD";
    public bool HasLongFormat => true;
    
    public void Execute(ICpu cpu)
    {
        var address = GetEffectiveAddress(cpu);
        cpu.Acc = cpu[address];  // Load accumulator from memory
    }
}
```

#### 2. Arithmetic Instructions
**Examples:** A, AD, S, SD, M, D

```csharp
public class Add : InstructionBase, IInstruction
{
    public void Execute(ICpu cpu)
    {
        var address = GetEffectiveAddress(cpu);
        ushort operand = cpu[address];
        
        uint result = (uint)cpu.Acc + (uint)operand;
        
        cpu.Acc = (ushort)(result & Mask16);
        cpu.Carry = (result & 0x10000) != 0;  // Bit 16 set
        
        // Overflow: sign of operands same, result sign different
        bool accNeg = (cpu.Acc & 0x8000) != 0;
        bool opNeg = (operand & 0x8000) != 0;
        bool resNeg = (cpu.Acc & 0x8000) != 0;
        cpu.Overflow = (accNeg == opNeg) && (accNeg != resNeg);
    }
}
```

#### 3. Logical Instructions
**Examples:** AND, OR, EOR

```csharp
public class And : InstructionBase, IInstruction
{
    public void Execute(ICpu cpu)
    {
        var address = GetEffectiveAddress(cpu);
        cpu.Acc &= cpu[address];  // Bitwise AND
        cpu.Carry = false;         // Logical ops clear carry
        cpu.Overflow = false;
    }
}
```

#### 4. Shift Instructions
**Examples:** SLA, SLT, SRA, SRT, SLC, SLCA

```csharp
public class ShiftLeft : ShiftInstructionBase, IInstruction
{
    public void Execute(ICpu cpu)
    {
        int shiftCount = cpu.Modifiers & 0x3f;  // Bits 10-15
        int shiftType = (cpu.Modifiers >> 6) & 0x03;  // Bits 8-9
        
        switch (shiftType)
        {
            case 0:  // SLA - Shift Left Accumulator
                cpu.Acc = (ushort)(cpu.Acc << shiftCount);
                break;
                
            case 1:  // SLT - Shift Left and Count
                // ... complex logic
                break;
        }
    }
}
```

#### 5. Branch Instructions
**Examples:** BSC, BSI, MDX

```csharp
public class BranchSkip : BranchInstructionBase, IInstruction
{
    public void Execute(ICpu cpu)
    {
        byte condition = (byte)(cpu.Modifiers & 0x1f);  // Bits 11-15
        
        bool shouldBranch = false;
        
        // Check condition flags
        if ((condition & 0x10) != 0 && cpu.Carry) shouldBranch = true;
        if ((condition & 0x08) != 0 && cpu.Overflow) shouldBranch = true;
        if ((condition & 0x04) != 0 && cpu.Acc == 0) shouldBranch = true;
        // ... more conditions
        
        if (shouldBranch)
        {
            cpu.Iar = GetEffectiveAddress(cpu);
        }
    }
}
```

---

## Testing Architecture

### Test Organization

**Test Project Structure:**
```
tests/UnitTests.S1130.SystemObjects/
├── InstructionTests/
│   ├── AddTests.cs
│   ├── LoadTests.cs
│   └── ... (one file per instruction)
├── DeviceTests/
│   ├── Device2501Tests.cs
│   ├── Device1442Tests.cs
│   └── DeviceTestBase.cs
├── CpuTests.cs
├── AssemblerTests.cs
└── InstructionBuilder.cs  (test helper)
```

### Test Base Classes

#### InstructionTestBase

```csharp
public abstract class InstructionTestBase
{
    protected Cpu InsCpu;
    
    protected virtual void BeforeEachTest()
    {
        InsCpu = new Cpu { Iar = 0x100 };
    }
    
    protected void ExecAndTest(
        ushort initialAcc, bool initialCarry, bool initialOverflow,
        ushort expectedAcc, bool expectedCarry, bool expectedOverflow)
    {
        InsCpu.Acc = initialAcc;
        InsCpu.Carry = initialCarry;
        InsCpu.Overflow = initialOverflow;
        
        
        InsCpu.ExecuteInstruction();
        
        Assert.Equal(expectedAcc, InsCpu.Acc);
        Assert.Equal(expectedCarry, InsCpu.Carry);
        Assert.Equal(expectedOverflow, InsCpu.Overflow);
    }
}
```

#### DeviceTestBase

```csharp
public abstract class DeviceTestBase
{
    protected Cpu InsCpu;
    
    protected virtual void BeforeEachTest()
    {
        InsCpu = new Cpu { Iar = 0x100 };
    }
    
    protected void InitiateRead(IDevice device, int wca, int wc)
    {
        InsCpu.IoccDeviceCode = device.DeviceCode;
        InsCpu.IoccFunction = DevFunction.InitRead;
        InsCpu.IoccAddress = wca;
        InsCpu[wca] = (ushort)wc;
        device.ExecuteIocc();
    }
    
    protected void SenseDevice(IDevice device, ushort resetBits = 0)
    {
        InsCpu.IoccDeviceCode = device.DeviceCode;
        InsCpu.IoccFunction = DevFunction.SenseDevice;
        InsCpu.IoccModifiers = resetBits;
        device.ExecuteIocc();
    }
    
    protected void IssueControl(IDevice device, ushort address, byte modifier)
    {
        InsCpu.IoccDeviceCode = device.DeviceCode;
        InsCpu.IoccFunction = DevFunction.Control;
        InsCpu.IoccModifiers = modifier;
        InsCpu.IoccAddress = address;
        device.ExecuteIocc();
    }
}
```

### InstructionBuilder Helper

Used to programmatically create instructions for testing:

```csharp
public static class InstructionBuilder
{
    // Build short format instruction
    public static void BuildShortAtAddress(
        OpCodes opCode, uint tag, uint displacement, ICpu cpu, int address)
    {
        ushort instruction = (ushort)(
            (((byte)opCode & 0x1f) << 11) |  // Opcode bits 0-4
            ((tag & 0x03) << 8) |             // Tag bits 6-7
            (displacement & 0xff)             // Displacement bits 8-15
        );
        cpu[address] = instruction;
    }
    
    // Build long format instruction
    public static void BuildLongAtAddress(
        OpCodes opCode, uint tag, ushort displacement, ICpu cpu, int address)
    {
        ushort firstWord = (ushort)(
            (((byte)opCode & 0x1f) << 11) |  // Opcode
            0x0400 |                          // Long format bit
            ((tag & 0x03) << 8)              // Tag
        );
        cpu[address] = firstWord;
        cpu[address + 1] = displacement;
    }
    
    // Build IOCC structure
    public static void BuildIoccAt(
        IDevice device, DevFunction func, byte modifier, 
        ushort memAddr, ICpu cpu, ushort ioccAddr)
    {
        cpu[ioccAddr] = memAddr;  // WCA
        cpu[ioccAddr + 1] = (ushort)(
            ((device.DeviceCode & 0x1f) << 11) |
            (((int)func & 0x7) << 8) |
            modifier
        );
    }
}
```

### Test Naming Convention

Tests follow a consistent naming pattern:

```
Execute_[Operation]_[Format]_[Condition]_[ExpectedResult]
```

**Examples:**
- `Execute_Add_ShortFormat_NoOverflow_SetsAccumulator()`
- `Execute_Load_LongFormat_IndirectAddress_LoadsFromPointer()`
- `Execute_Branch_WithCarrySet_BranchTaken()`

### Example Test

```csharp
[Fact]
public void Execute_Add_ShortFormat_WithCarry_SetsCarryFlag()
{
    BeforeEachTest();
    
    // Set up: Acc = 0xFFFF, Memory[0x105] = 0x0002
    InsCpu.Acc = 0xFFFF;
    InsCpu[0x105] = 0x0002;
    
    // Build instruction: ADD 5 (short format, displacement = 5)
    InstructionBuilder.BuildShortAtAddress(OpCodes.Add, 0, 5, InsCpu, 0x100);
    
    // Execute and verify
    ExecAndTest(
        initialAcc: 0xFFFF, 
        initialCarry: false, 
        initialOverflow: false,
        expectedAcc: 0x0001,   // 0xFFFF + 0x0002 = 0x10001, truncated to 0x0001
        expectedCarry: true,    // Bit 16 was set
        expectedOverflow: false
    );
}
```

---

## Common Operations

### 1. Loading and Executing a Program

```csharp
var cpu = new Cpu();

// Load program into memory
cpu[0x100] = 0xC400;  // LD L 0x105 (load from 0x105)
cpu[0x101] = 0x0105;
cpu[0x102] = 0xD400;  // STO L 0x200 (store to 0x200)
cpu[0x103] = 0x0200;
cpu[0x104] = 0xB000;  // WAIT

cpu[0x105] = 0x1234;  // Data value

// Set program counter and run
cpu.Iar = 0x100;
cpu.Wait = false;

while (!cpu.Wait)
{
    cpu.NextInstruction();
    cpu.ExecuteInstruction();
}

// Result: cpu[0x200] == 0x1234
```

### 2. Using the Assembler

```csharp
var cpu = new Cpu();

var source = @"
      ORG  /100
      LD   L VAL
      A    L VAL
      STO  L RESULT
      WAIT
VAL   DC   /0010
RESULT DC  0
";

var result = cpu.Assemble(source);

if (result.Success)
{
    cpu.Iar = 0x100;
    cpu.Wait = false;
    
    while (!cpu.Wait)
    {
        cpu.NextInstruction();
        cpu.ExecuteInstruction();
    }
    
    // Result: cpu[RESULT] = 0x0020 (0x10 + 0x10)
}
```

### 3. Setting Up Device I/O

```csharp
var cpu = new Cpu();
var reader = new Device2501(cpu);

// Add device to CPU
cpu.AddDevice(reader);

// Load card into reader
var card = new Card(new ushort[80]);  // 80 columns of data
reader.ReadHopper.Enqueue(card);

// Create IOCC structure at 0x300
cpu[0x300] = 80;      // Word count
cpu[0x301] = 0x0900;  // Device 01, function InitRead, modifier 0

// Build XIO instruction to initiate read
InstructionBuilder.BuildLongAtAddress(
    OpCodes.ExecuteInputOutput, 0, 0x300, cpu, 0x100);

cpu.Iar = 0x100;
cpu.NextInstruction();
cpu.ExecuteInstruction();

// Data transferred to cpu[0x301] through cpu[0x350]
```

### 4. Working with Interrupts

```csharp
// Set up interrupt vector
cpu[0x0007] = 0x0400;  // Level 4 vector points to handler

// Set up interrupt handler
cpu[0x0400] = 0x0000;  // Return address (saved by hardware)
cpu[0x0401] = 0xB000;  // WAIT (handler just stops)

// Device generates interrupt
var interrupt = cpu.IntPool.GetInterrupt(4, 0x0800, device);
cpu.AddInterrupt(interrupt);

// Execute will automatically handle interrupt
cpu.NextInstruction();
cpu.ExecuteInstruction();

// IAR now points to 0x0401 (handler)
// Original IAR saved at 0x0400
```

---

## Debugging Features

### 1. Debug Settings

The CPU supports per-address debugging:

```csharp
// Enable debugging for a specific address
cpu.MasterDebug = true;
cpu.SetDebug(0x100, new MyDebugSetting());

// Custom debug action
public class MyDebugSetting : IDebugSetting
{
    public void OnRead(int address, ushort value)
    {
        Console.WriteLine($"Read {value:X4} from {address:X4}");
    }
    
    public void OnWrite(int address, ushort value)
    {
        Console.WriteLine($"Write {value:X4} to {address:X4}");
    }
}
```

### 2. Instruction Counter

Track executed instructions:

```csharp
ulong startCount = cpu.InstructionCount;

// Execute program
while (!cpu.Wait)
{
    cpu.NextInstruction();
    cpu.ExecuteInstruction();
}

ulong executed = cpu.InstructionCount - startCount;
Console.WriteLine($"Executed {executed} instructions");
```

### 3. Memory Inspection

```csharp
// Dump memory range
for (int addr = 0x100; addr < 0x110; addr++)
{
    Console.WriteLine($"{addr:X4}: {cpu[addr]:X4}");
}

// Watch for changes
ushort oldValue = cpu[0x100];
// ... execute code ...
if (cpu[0x100] != oldValue)
{
    Console.WriteLine($"Memory changed: {oldValue:X4} -> {cpu[0x100]:X4}");
}
```

### 4. Register Inspection

```csharp
public void DumpRegisters(ICpu cpu)
{
    Console.WriteLine($"IAR:  {cpu.Iar:X4}");
    Console.WriteLine($"ACC:  {cpu.Acc:X4}");
    Console.WriteLine($"EXT:  {cpu.Ext:X4}");
    Console.WriteLine($"XR1:  {cpu.Xr[1]:X4}");
    Console.WriteLine($"XR2:  {cpu.Xr[2]:X4}");
    Console.WriteLine($"XR3:  {cpu.Xr[3]:X4}");
    Console.WriteLine($"Carry: {cpu.Carry}");
    Console.WriteLine($"Overflow: {cpu.Overflow}");
    Console.WriteLine($"Wait: {cpu.Wait}");
}
```

---

## Performance Characteristics

**Typical Performance (on modern hardware):**
- Simple instructions: 5-10 million per second
- Complex instructions: 2-5 million per second
- With I/O operations: 1-2 million per second

**Optimization Tips:**
1. Use direct `Memory[]` access for performance-critical loops
2. Avoid unnecessary instruction decode (reuse `CurrentInstruction`)
3. Pool interrupt objects (already implemented)
4. Batch memory operations when possible

---

## Summary

The S1130 emulator provides an accurate, testable implementation of the IBM 1130 computer system. Key design elements:

- **Accuracy**: Faithful reproduction of IBM 1130 behavior
- **Modularity**: Instructions and devices are independent, pluggable components
- **Testability**: 395+ comprehensive unit tests
- **Performance**: Millions of instructions per second
- **Extensibility**: Easy to add new instructions and devices

The architecture separates concerns cleanly:
- CPU handles instruction execution and memory
- Instructions implement specific operations
- Devices manage I/O and interrupts
- Tests verify correct behavior

This design makes the emulator maintainable, understandable, and extensible for future development.
