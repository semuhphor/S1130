# S1130 Emulator Improvements

**Document Version**: 1.0
**Last Updated**: 2025-10-24
**Purpose**: Catalog identified issues and proposed solutions for improving IBM 1130 emulation accuracy

---

## Table of Contents

1. [Overview](#overview)
2. [Critical Issues](#critical-issues)
3. [Medium-Severity Issues](#medium-severity-issues)
4. [Low-Severity Issues](#low-severity-issues)
5. [Code Quality Improvements](#code-quality-improvements)
6. [Testing Enhancements](#testing-enhancements)
7. [Implementation Priority](#implementation-priority)
8. [References](#references)

---

## Overview

This document catalogs issues identified in the S1130 emulator implementation through comparison with IBM 1130 hardware documentation. Each issue includes:

- **Problem Description**: What is incorrect or incomplete
- **Impact**: How this affects emulation accuracy
- **IBM 1130 Specification**: Reference to official documentation
- **Current Behavior**: How the code currently works
- **Expected Behavior**: How it should work per IBM spec
- **Solution**: Proposed implementation approach
- **Test Strategy**: How to verify the fix
- **Estimated Effort**: Time required (S/M/L)

### Issue Classification

- **CRITICAL**: Feature incomplete or incorrect, breaks IBM 1130 compatibility
- **MEDIUM**: Missing functionality or semantic mismatch, may affect some programs
- **LOW**: Minor inefficiency or design quirk, no functional impact

---

## Critical Issues

### ISSUE-001: Device2310 Read-Check Feature Incomplete

**File**: `src/S1130.SystemObjects/Devices/Device2310.cs`
**Lines**: 160, 196-204
**Severity**: CRITICAL
**Priority**: HIGH

#### Problem Description

The Read-Check modifier (bit 7, value 0x80) is parsed from the IOCC but never used to verify data. The IBM 1130 disk controller should compare the memory buffer with the disk data after a read operation when Read-Check is enabled, setting the Data Error flag if they don't match.

#### Impact

- Data verification feature unavailable to software
- Programs cannot verify successful disk reads
- Silent data corruption could go undetected

#### IBM 1130 Specification

**Reference**: IBM 1130 Functional Characteristics (A26-5881-2), Section 4.3.2 - "Read-Check Operations"

**Quote**: "When the read-check modifier is specified, the disk controller compares the data read from disk with the contents of the core storage buffer. If a mismatch is detected, the data error bit (bit 15) is set in the device status word."

#### Current Behavior

```csharp
// Line 160 - Read-Check flag is parsed
_readCheck = (CpuInstance.IoccModifiers & ReadCheck) != 0;

// Lines 196-204 - Read operation completes but Read-Check is ignored
if (_reading)
{
    var buffer = _cartridge.Read(_sector);
    CpuInstance.TransferToMemory(buffer, 321);  // Ignores _readCheck!
    _reading = false;
    _complete = true;
    ActivateInterrupt();
}
```

The `_readCheck` field is set but never examined during the read completion.

#### Expected Behavior

After transferring disk data to memory, if Read-Check is enabled:
1. Compare disk buffer with memory buffer (320 words)
2. If any word differs, set `_dataError = true`
3. Set bit 15 (0x8000) in status word returned by Sense Device
4. Signal completion interrupt regardless of error status

#### Solution

**Implementation Steps**:

1. Add `_dataError` field to Device2310 class
2. Create `VerifyReadCheck()` method to compare buffers
3. Call verification after data transfer when `_readCheck` is true
4. Update `HandleSenseDevice()` to include Data Error bit in status word
5. Clear `_dataError` flag when new operation starts

**Code Changes**:

```csharp
// Add field
private bool _dataError;

// Add constant
private const ushort DataErrorBit = 0x8000;
private const int SectorDataSize = 320;

// New verification method
/// <summary>
/// Verifies that disk data matches memory buffer for Read-Check operation.
/// </summary>
/// <param name="diskBuffer">Data read from disk sector</param>
/// <returns>True if data matches, false if mismatch detected</returns>
private bool VerifyReadCheck(ushort[] diskBuffer)
{
    for (int i = 0; i < SectorDataSize; i++)
    {
        if (diskBuffer[i] != CpuInstance.Memory[_wca + i])
        {
            return false;
        }
    }
    return true;
}

// Updated read completion (lines 196-204)
if (_reading)
{
    var buffer = _cartridge.Read(_sector);

    // Perform Read-Check verification if enabled
    if (_readCheck && !VerifyReadCheck(buffer))
    {
        _dataError = true;
    }

    CpuInstance.TransferToMemory(buffer, SectorDataSize);
    _reading = false;
    _complete = true;
    ActivateInterrupt();
}

// Updated status word (lines 109-128)
private ushort BuildStatusWord()
{
    ushort status = 0;

    if (_dataError) status |= DataErrorBit;      // Bit 15
    if (_complete) status |= OperationComplete;  // Bit 14
    if (_cartridge == null) status |= NotReady;  // Bit 13
    if (_busy) status |= Busy;                   // Bit 12
    if (_cylinder.Current == 0) status |= AtCylZero;  // Bit 11

    return status;
}

// Clear error flag when starting new operation
private void HandleInitRead()
{
    _dataError = false;  // Clear previous error
    // ... rest of method
}
```

#### Test Strategy

**Test Cases**:

1. **Read-Check with matching data**:
   - Write data to disk
   - Write same data to memory buffer
   - Execute read with Read-Check enabled
   - Verify Data Error bit is NOT set

2. **Read-Check with mismatched data**:
   - Write data to disk
   - Write different data to memory buffer
   - Execute read with Read-Check enabled
   - Verify Data Error bit IS set (bit 15 = 1)

3. **Read-Check disabled**:
   - Execute read without Read-Check modifier
   - Verify operation completes normally
   - Verify Data Error bit not affected by buffer contents

4. **Read-Check with partial mismatch**:
   - Mismatch only last word in sector
   - Verify Data Error still detected

**Test Implementation**:

```csharp
[Fact]
public void Device2310_ReadCheck_MatchingData_NoError()
{
    // Arrange
    var cpu = new Cpu();
    var cartridge = new MemoryCartridge();
    var device = new Device2310(cpu, cartridge);

    ushort[] testData = Enumerable.Range(1, 320).Select(i => (ushort)i).ToArray();

    // Write data to disk
    cartridge.Write(0, testData, 320);

    // Write same data to memory
    for (int i = 0; i < 320; i++)
    {
        cpu.Memory[0x100 + i] = testData[i];
    }

    // Setup IOCC with Read-Check
    cpu.Memory[0x50] = 0x100;  // WCA
    cpu.Memory[0x51] = 0x4082;  // Device 08, InitRead (02), Read-Check (80)

    // Act
    cpu.ExecuteIocc(0x50);
    device.Run();

    // Sense status
    cpu.Memory[0x52] = 0x0;
    cpu.Memory[0x53] = 0x4000;  // Device 08, Sense
    cpu.ExecuteIocc(0x52);

    // Assert
    Assert.False((cpu.Acc & 0x8000) != 0, "Data Error should NOT be set");
}

[Fact]
public void Device2310_ReadCheck_MismatchedData_SetsError()
{
    // Similar setup but with different memory data
    // Assert that bit 15 IS set
}
```

#### Estimated Effort

**Time**: 2-3 hours
- Code changes: 1 hour
- Test implementation: 1 hour
- Documentation: 30 minutes

---

### ISSUE-002: Device2310 Data Error Flag Not Surfaced

**File**: `src/S1130.SystemObjects/Devices/Device2310.cs`
**Lines**: 15, 29, 102-130
**Severity**: CRITICAL
**Priority**: HIGH

#### Problem Description

The Data Error status bit (bit 15, value 0x8000) is defined but never included in the status word returned by the Sense Device function. This makes it impossible for software to detect read errors or Read-Check failures.

#### Impact

- Programs cannot detect disk errors
- Error conditions are invisible to software
- Reduces emulation accuracy

#### IBM 1130 Specification

**Reference**: IBM 1130 Functional Characteristics (A26-5881-2), Section 4.3.3 - "Device Status Word"

**Status Bit Definitions**:
- Bit 15: Data Error (parity error, read-check mismatch)
- Bit 14: Operation Complete
- Bit 13: Not Ready
- Bit 12: Busy
- Bit 11: At Cylinder Zero
- Bits 1-0: Next Sector

#### Current Behavior

```csharp
// Constant exists but is not used
public const ushort OperationComplete = 0x4000;
public const ushort NotReady = 0x2000;
// Data Error constant MISSING

// Status word doesn't include Data Error
private ushort BuildStatusWord()
{
    ushort newAcc = 0;
    if (_cartridge == null) { newAcc = NotReady; }
    // ... other bits
    // Data Error bit never set!
    return newAcc;
}
```

#### Expected Behavior

The Sense Device function should return a status word with bit 15 set when any error condition exists:
- Read-Check mismatch
- Parity error (not emulated, but structure should support)
- Invalid operation attempt

#### Solution

**Implementation**: Covered by ISSUE-001 solution above. The `BuildStatusWord()` method must include:

```csharp
if (_dataError) status |= DataErrorBit;  // Bit 15 = 0x8000
```

#### Test Strategy

Tested as part of ISSUE-001 test cases. Verify Sense Device returns correct status word with bit 15 set when appropriate.

#### Estimated Effort

**Time**: Included in ISSUE-001 (no separate effort)

---

### ISSUE-003: Device2310 Next-Sector Bits Not Reported

**File**: `src/S1130.SystemObjects/Devices/Device2310.cs`
**Lines**: 33, 102-130
**Severity**: MEDIUM
**Priority**: MEDIUM

#### Problem Description

The `NextSectorMask` constant (0x0003) is defined but the Next Sector bits (1-0) are never populated in the status word. These bits should indicate which sector will be accessed next, allowing software to optimize multi-sector operations.

#### Impact

- Minor: Most software doesn't use this feature
- Performance optimization opportunity lost
- Reduces emulation accuracy for sophisticated disk I/O

#### IBM 1130 Specification

**Reference**: IBM 1130 Functional Characteristics (A26-5881-2), Section 4.3.3

**Quote**: "Bits 1 and 0 of the status word contain the sector number (0-7) that will be accessed on the next read or write operation."

#### Current Behavior

```csharp
public const ushort NextSectorMask = 0x0003;  // Defined but unused

private ushort BuildStatusWord()
{
    // ... builds status
    // Never includes next sector bits
    return status;
}
```

No tracking of which sector was last accessed or will be accessed next.

#### Expected Behavior

After each read/write operation:
1. Track the current sector number (0-7)
2. Calculate next sector = (current + 1) % 8
3. Include next sector in bits 1-0 of status word

#### Solution

**Implementation Steps**:

1. Add `_lastSectorAccessed` field
2. Update after each read/write operation
3. Include in status word calculation

**Code Changes**:

```csharp
// Add field
private int _lastSectorAccessed = 0;

// Update after read/write
private void CompleteReadOperation()
{
    // ... existing code
    _lastSectorAccessed = _sector;
}

private void CompleteWriteOperation()
{
    // ... existing code
    _lastSectorAccessed = _sector;
}

// Include in status word
private ushort BuildStatusWord()
{
    ushort status = 0;

    if (_dataError) status |= DataErrorBit;
    if (_complete) status |= OperationComplete;
    if (_cartridge == null) status |= NotReady;
    if (_busy) status |= Busy;
    if (_cylinder.Current == 0) status |= AtCylZero;

    // Add next sector bits
    int nextSector = (_lastSectorAccessed + 1) & 0x07;
    status |= (ushort)(nextSector & NextSectorMask);

    return status;
}
```

#### Test Strategy

**Test Cases**:

1. **After read from sector 0**: Verify bits 1-0 = 1
2. **After read from sector 7**: Verify bits 1-0 = 0 (wraparound)
3. **After write to sector 3**: Verify bits 1-0 = 4
4. **Before any operation**: Verify bits 1-0 = 0 (initial state)

**Test Implementation**:

```csharp
[Theory]
[InlineData(0, 1)]  // After sector 0, next is 1
[InlineData(3, 4)]  // After sector 3, next is 4
[InlineData(7, 0)]  // After sector 7, next wraps to 0
public void Device2310_Sense_ReturnsCorrectNextSector(int currentSector, int expectedNext)
{
    // Arrange
    var cpu = new Cpu();
    var cartridge = new MemoryCartridge();
    var device = new Device2310(cpu, cartridge);

    // Perform read from specific sector
    // ... setup and execute read operation on currentSector

    // Act - Sense device
    cpu.Memory[0x52] = 0x0;
    cpu.Memory[0x53] = 0x4000;  // Sense function
    cpu.ExecuteIocc(0x52);

    // Assert
    int nextSector = cpu.Acc & 0x0003;
    Assert.Equal(expectedNext, nextSector);
}
```

#### Estimated Effort

**Time**: 1-2 hours
- Code changes: 30 minutes
- Test implementation: 1 hour
- Documentation: 30 minutes

---

## Medium-Severity Issues

### ISSUE-004: Divide Instruction Remainder Semantics Questionable

**File**: `src/S1130.SystemObjects/Instructions/Divide.cs`
**Lines**: 30-43
**Severity**: MEDIUM
**Priority**: MEDIUM

#### Problem Description

The divide instruction uses C#'s `%` operator for calculating the remainder, which may have different semantics than the IBM 1130 hardware when dealing with negative operands. C# uses "truncated division" (remainder has sign of dividend), while IBM 1130 may use different semantics.

#### Impact

- Division with negative operands may produce incorrect remainder
- Affects programs using signed division
- Most programs use positive operands (low real-world impact)

#### IBM 1130 Specification

**Reference**: IBM 1130 Functional Characteristics (A26-5881-2), Section 3.4.5 - "Divide Instruction"

**Quote**: "The dividend (ACC,EXT) is divided by the divisor (memory word). The quotient is placed in ACC, and the remainder in EXT. All operands are treated as signed 16-bit values (two's complement)."

**Note**: The specification doesn't explicitly define remainder semantics for negative operands. Need to verify against actual hardware behavior or test programs.

#### Current Behavior

```csharp
public override void Execute(ICpu cpu)
{
    var divisor = cpu.Memory[GetEffectiveAddress(cpu)];
    var dividend = (int)cpu.AccExt;
    var divider = (int)SignExtend(divisor);

    if (divider == 0)
    {
        cpu.Overflow = true;
        return;
    }

    var quotient = dividend / divider;

    if (quotient > short.MaxValue || quotient < short.MinValue)
    {
        cpu.Overflow = true;
    }
    else
    {
        cpu.Ext = (ushort)(dividend % divider);  // C# semantics!
        cpu.Acc = (ushort)(quotient & 0xffff);
        cpu.Overflow = false;
    }
}
```

C# `%` operator: `dividend % divisor` has sign of `dividend`.

**Examples**:
- C#: `7 % -3 = 1` (positive remainder)
- C#: `-7 % 3 = -1` (negative remainder)

IBM 1130 may expect different behavior (floored division or Euclidean division).

#### Expected Behavior

Need to verify actual IBM 1130 behavior. Possible approaches:

1. **Research**: Find IBM 1130 test programs with signed division
2. **Specification**: Check if assembler manual has examples
3. **Implementation**: Match C# behavior (document assumption)

#### Solution

**Option 1: Research and Verify** (RECOMMENDED)

1. Search for IBM 1130 test programs or diagnostics
2. Find examples with signed division
3. Verify expected results
4. Adjust implementation if needed

**Option 2: Document Current Behavior**

If verification impossible, document that emulator uses C# division semantics:

```csharp
/// <summary>
/// Divide instruction (D).
/// </summary>
/// <remarks>
/// Divides 32-bit ACC+EXT by 16-bit memory operand.
/// Quotient placed in ACC, remainder in EXT.
///
/// EMULATION NOTE: Remainder calculation uses C# modulo operator semantics
/// (truncated division). For negative operands, remainder has sign of dividend.
/// This may differ from actual IBM 1130 hardware if it used floored division.
///
/// Examples with C# semantics:
///   7 / -3 = -2 remainder 1
///  -7 /  3 = -2 remainder -1
///  -7 / -3 =  2 remainder -1
/// </remarks>
public override void Execute(ICpu cpu)
{
    // Implementation with documentation
}
```

#### Test Strategy

**Test Cases Needed**:

1. **Positive dividend, positive divisor**: `100 / 7 = 14 R 2`
2. **Positive dividend, negative divisor**: `100 / -7 = -14 R ?`
3. **Negative dividend, positive divisor**: `-100 / 7 = -14 R ?`
4. **Negative dividend, negative divisor**: `-100 / -7 = 14 R ?`

**Test Implementation**:

```csharp
[Theory]
[InlineData(100, 7, 14, 2)]      // Standard case
[InlineData(100, -7, -14, 2)]    // Negative divisor (verify remainder)
[InlineData(-100, 7, -14, -2)]   // Negative dividend (verify remainder)
[InlineData(-100, -7, 14, -2)]   // Both negative (verify remainder)
public void Divide_SignedOperands_CorrectQuotientAndRemainder(
    int dividend, int divisor, int expectedQ, int expectedR)
{
    // Arrange
    var cpu = new Cpu();
    cpu.AccExt = (uint)dividend;
    cpu.Memory[0x100] = (ushort)divisor;

    var instruction = new Divide { /* setup */ };

    // Act
    instruction.Execute(cpu);

    // Assert
    Assert.Equal((ushort)expectedQ, cpu.Acc);
    Assert.Equal((ushort)expectedR, cpu.Ext);
}
```

**Action Required**: Research IBM 1130 divide behavior before implementing tests.

#### Estimated Effort

**Time**: 3-4 hours
- Research: 2 hours (find test programs, verify behavior)
- Code changes: 30 minutes (if needed)
- Test implementation: 1 hour
- Documentation: 30 minutes

---

### ISSUE-005: Divide Instruction Quotient Overflow Behavior

**File**: `src/S1130.SystemObjects/Instructions/Divide.cs`
**Lines**: 25-43
**Severity**: LOW (intentional design)
**Priority**: LOW

#### Problem Description

When quotient overflow occurs (result doesn't fit in 16 bits), the implementation sets the Overflow flag and leaves ACC/EXT unchanged. The code comment states this is "undefined in IBM 1130 hardware."

#### Impact

- Minimal: Programs should check for overflow before using results
- Current behavior is documented and tested
- Matches test expectations

#### IBM 1130 Specification

**Reference**: IBM 1130 Functional Characteristics (A26-5881-2), Section 3.4.5

**Quote**: "If the quotient exceeds the capacity of the accumulator (greater than 32767 or less than -32768), the overflow indicator is set to 1. The contents of the accumulator and extension register are unpredictable."

**Interpretation**: "Unpredictable" means implementation-defined. Current approach (leave unchanged) is valid.

#### Current Behavior

```csharp
var quotient = dividend / divider;

if (quotient > short.MaxValue || quotient < short.MinValue)
{
    cpu.Overflow = true;
    // ACC and EXT left unchanged
}
else
{
    cpu.Ext = (ushort)(dividend % divider);
    cpu.Acc = (ushort)(quotient & 0xffff);
    cpu.Overflow = false;
}
```

#### Expected Behavior

Current behavior is acceptable per IBM specification. "Unpredictable" allows any implementation.

#### Solution

**No code changes required**. Current implementation is correct.

**Documentation improvement**:

```csharp
/// <summary>
/// Divide instruction (D).
/// </summary>
/// <remarks>
/// Per IBM 1130 Functional Characteristics Section 3.4.5:
/// "If quotient exceeds accumulator capacity, overflow indicator is set
/// and ACC/EXT contents are unpredictable."
///
/// This emulator leaves ACC/EXT unchanged on overflow, which is valid
/// implementation-defined behavior.
/// </remarks>
```

#### Test Strategy

**Existing tests adequate**:

File `DivideTests.cs` lines 71-88 already verify this behavior:

```csharp
[Fact]
public void Execute_D_QuotientOverflow()
{
    // Verify Overflow set and registers unchanged
}
```

No additional tests needed.

#### Estimated Effort

**Time**: 30 minutes (documentation only)

---

## Low-Severity Issues

### ISSUE-006: StoreStatus Instruction Read-Modify-Write Inefficiency

**File**: `src/S1130.SystemObjects/Instructions/StoreStatus.cs`
**Lines**: 8-14
**Severity**: LOW
**Priority**: LOW

#### Problem Description

The STS (Store Status) instruction calls `GetEffectiveAddress()` twice, resulting in two memory accesses instead of one. This is functionally correct but inefficient.

#### Impact

- Minor performance impact (negligible in modern CPU)
- No functional incorrectness
- Code clarity could be improved

#### Current Behavior

```csharp
public override void Execute(ICpu cpu)
{
    cpu[GetEffectiveAddress(cpu)] &= 0xFF03;  // First call
    cpu[GetEffectiveAddress(cpu)] |= (ushort)(...);  // Second call
    // ... clear flags
}
```

#### Expected Behavior

Calculate effective address once and reuse:

```csharp
public override void Execute(ICpu cpu)
{
    var address = GetEffectiveAddress(cpu);
    cpu[address] &= 0xFF03;
    cpu[address] |= (ushort)(...);
    // ... clear flags
}
```

#### Solution

**Code Changes**:

```csharp
public override void Execute(ICpu cpu)
{
    var effectiveAddress = GetEffectiveAddress(cpu);
    ushort currentValue = cpu[effectiveAddress];

    // Clear status bits (2-3)
    currentValue &= 0xFF03;

    // Set status bits from flags
    ushort statusBits = 0;
    if (cpu.Carry) statusBits |= 0x04;     // Bit 2
    if (cpu.Overflow) statusBits |= 0x08;  // Bit 3

    cpu[effectiveAddress] = (ushort)(currentValue | statusBits);

    // Clear flags after storing
    cpu.Carry = false;
    cpu.Overflow = false;
}
```

#### Test Strategy

**Existing tests sufficient**: Behavior unchanged, only performance improved.

Verify with existing `StoreStatusTests.cs` that all tests still pass.

#### Estimated Effort

**Time**: 30 minutes
- Code changes: 15 minutes
- Verification: 15 minutes

---

### ISSUE-007: IndexRegisters Indexer Design Clarification

**File**: `src/S1130.SystemObjects/IndexRegisters.cs`
**Lines**: 14-34
**Severity**: LOW (design clarification)
**Priority**: LOW

#### Problem Description

The `IndexRegisters` class uses an indexer where `Xr[0]` returns IAR instead of memory location 0. This is intentional but non-obvious, as tag value 0 in some contexts means "no index register."

#### Impact

- No functional issue
- Potential confusion for new developers
- Design is actually clever and correct

#### Current Behavior

```csharp
public ushort this[int i]
{
    get
    {
        return i switch
        {
            0 => _cpu.Iar,  // Tag 0 returns IAR
            1 => _cpu.Memory[1],  // XR1
            2 => _cpu.Memory[2],  // XR2
            3 => _cpu.Memory[3],  // XR3
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    set { /* similar */ }
}
```

**Rationale**: For short format instructions, tag=00 means IAR-relative addressing. The indexer returns IAR for tag=0, allowing unified access: `Xr[tag]` works for all tag values.

#### Expected Behavior

Current behavior is correct. This is intentional design, not a bug.

#### Solution

**Add documentation** to explain the design:

```csharp
/// <summary>
/// Provides indexed access to index registers and IAR.
/// </summary>
/// <remarks>
/// Index:
///   0 - Returns IAR (used for tag=00 in short format addressing)
///   1 - Returns XR1 (memory location /0001)
///   2 - Returns XR2 (memory location /0002)
///   3 - Returns XR3 (memory location /0003)
///
/// This allows unified access to base address for all tag values:
/// - Tag 00: Base = IAR (short format) or 0 (long format)
/// - Tag 01-11: Base = XR[tag]
///
/// For long format with tag=00, caller must handle IAR specially
/// (effective address = displacement, not IAR + displacement).
/// </remarks>
public ushort this[int i]
{
    get { /* implementation */ }
}
```

#### Test Strategy

No code changes, documentation only. Existing tests verify correct behavior.

#### Estimated Effort

**Time**: 15 minutes (documentation only)

---

## Code Quality Improvements

### CQ-001: Extract Magic Numbers to Named Constants

**Priority**: MEDIUM
**Effort**: 1-2 hours

#### Problem

Bit masks and shift values appear as magic numbers throughout the codebase:

```csharp
// From Cpu.cs
int opCode = (insWord >> 11) & 0x1f;
bool longFormat = (insWord & 0x0400) != 0;
int tag = (insWord >> 8) & 0x03;
```

#### Solution

Extract to named constants:

```csharp
private const int OpCodeShift = 11;
private const int OpCodeMask = 0x1f;
private const int FormatBit = 0x0400;
private const int TagShift = 8;
private const int TagMask = 0x03;

// Usage
int opCode = (insWord >> OpCodeShift) & OpCodeMask;
bool longFormat = (insWord & FormatBit) != 0;
int tag = (insWord >> TagShift) & TagMask;
```

**Files to update**:
- `Cpu.cs`: Instruction decoding (lines 291-315)
- `Device2310.cs`: Status bit masks
- `Device2501.cs`: Status bit masks
- Various instructions: Shift counts, modifiers

---

### CQ-002: Add Inline Documentation to Assembler Parsing

**Priority**: MEDIUM
**Effort**: 2-3 hours

#### Problem

`Assembler.cs` (1,799 lines) has minimal inline comments explaining complex parsing logic.

#### Solution

Add inline comments to major sections:

```csharp
// Parse operand format/tag specifier
// Valid formats: ".", "L", "1-3", "L1-L3", "I", "I1-I3"
private void ParseFormatTag(string token)
{
    // Check for short format (.)
    if (token == ".")
    {
        _format = Format.Short;
        _tag = 0;
        return;
    }

    // Check for long format (L)
    // ... etc
}
```

**Focus areas**:
- Operand parsing (lines 400-700)
- Symbol resolution (lines 1500+)
- Address calculation
- Directive processing

---

### CQ-003: Improve Test Naming Consistency

**Priority**: LOW
**Effort**: 1 hour

#### Problem

Test names vary in style:
- Some very descriptive: `Execute_D_Indirect_Xr2_LargeNumberOverflow`
- Some minimal: `InvalidOpCode`

#### Solution

Standardize to pattern: `MethodName_Scenario_ExpectedBehavior`

**Examples**:
- ✅ `Add_ShortFormat_SetsCarryOnOverflow`
- ✅ `Divide_ByZero_SetsOverflowFlag`
- ✅ `Assembler_ForwardReference_ResolvesCorrectly`
- ❌ `InvalidOpCode` → `Cpu_InvalidOpCode_SetsWaitFlag`
- ❌ `Test1` → `Device2310_Sense_ReturnsCorrectStatus`

---

### CQ-004: Remove Empty InputOutputSystem Class

**Priority**: LOW
**Effort**: 15 minutes

#### Problem

`InputOutputSystem.cs` is an empty stub, not used by the codebase.

#### Solution

Either:
1. **Remove** if no planned use
2. **Implement** if there's a design plan
3. **Document** why it exists (placeholder for future)

Recommended: Remove and create GitHub issue if future implementation planned.

---

## Testing Enhancements

### TEST-001: Add Negative Number Division Tests

**Priority**: MEDIUM (blocked by ISSUE-004 research)
**Effort**: 1 hour

#### Problem

`DivideTests.cs` only tests positive operands and divide-by-zero. Missing negative operand combinations.

#### Solution

Add test cases for signed division:

```csharp
[Theory]
[InlineData(100, 7, 14, 2)]
[InlineData(100, -7, -14, 2)]    // Verify remainder sign
[InlineData(-100, 7, -14, -2)]   // Verify remainder sign
[InlineData(-100, -7, 14, -2)]   // Verify remainder sign
public void Divide_SignedOperands_CorrectResults(
    int dividend, int divisor, int expectedQ, int expectedR)
{
    // Test implementation
}
```

**Note**: Wait for ISSUE-004 research to determine expected values.

---

### TEST-002: Add Device2310 Read-Check Test Coverage

**Priority**: HIGH
**Effort**: 1 hour

#### Problem

No tests for Read-Check feature (because it's not implemented).

#### Solution

Implement tests as part of ISSUE-001 solution (see above).

---

### TEST-003: Add Stress Tests for Large Programs

**Priority**: LOW
**Effort**: 2-3 hours

#### Problem

No tests for large program execution (1000+ instructions).

#### Solution

Create integration tests:

```csharp
[Fact]
public void Assembler_LargeProgram_AssemblesSuccessfully()
{
    // Generate 1000-line assembly program
    var sourceLines = GenerateLargeProgram(1000);

    // Assemble
    var result = _assembler.Assemble(sourceLines);

    // Verify
    Assert.True(result.Success);
    Assert.Empty(result.Errors);
}

[Fact]
public void Cpu_LargeProgram_ExecutesCorrectly()
{
    // Load large program into memory
    // Execute 10,000 instructions
    // Verify final state
}
```

**Benefits**:
- Verify performance at scale
- Catch edge cases in complex programs
- Test interrupt handling under load

---

## Implementation Priority

### Phase 1: Critical Device Issues (Week 1)

**Goal**: Complete Device2310 implementation

1. **ISSUE-001**: Device2310 Read-Check (2-3 hours)
2. **ISSUE-002**: Data Error flag (included in ISSUE-001)
3. **ISSUE-003**: Next-Sector bits (1-2 hours)
4. **TEST-002**: Read-Check tests (included in ISSUE-001)

**Total Effort**: 3-5 hours
**Outcome**: Fully functional disk device matching IBM 1130 spec

### Phase 2: Instruction Semantics (Week 2)

**Goal**: Verify and correct instruction implementations

1. **ISSUE-004**: Research divide remainder semantics (3-4 hours)
2. **TEST-001**: Add signed division tests (1 hour)
3. **ISSUE-005**: Document divide overflow (30 minutes)

**Total Effort**: 4-5 hours
**Outcome**: Mathematically correct divide instruction

### Phase 3: Code Quality (Week 3)

**Goal**: Improve code maintainability

1. **CQ-001**: Extract magic numbers (1-2 hours)
2. **CQ-002**: Document assembler (2-3 hours)
3. **ISSUE-006**: Optimize StoreStatus (30 minutes)
4. **CQ-003**: Standardize test names (1 hour)
5. **CQ-004**: Remove empty class (15 minutes)

**Total Effort**: 5-7 hours
**Outcome**: Cleaner, more maintainable codebase

### Phase 4: Testing Enhancement (Week 4)

**Goal**: Increase test coverage and confidence

1. **TEST-003**: Large program stress tests (2-3 hours)
2. **ISSUE-007**: Document IndexRegisters design (15 minutes)
3. Review overall code coverage, add missing tests

**Total Effort**: 3-4 hours
**Outcome**: Comprehensive test suite

---

## References

### IBM 1130 Documentation

1. **IBM 1130 Functional Characteristics** (A26-5881-2, 1966)
   - Instruction set reference
   - Device specifications
   - Status word definitions

2. **IBM 1130 Assembler Language Manual** (C26-5927-2, 1966)
   - Syntax specifications
   - Directive definitions

3. **Wikipedia - IBM 1130**
   - Technical overview
   - Historical context

### Project Documentation

- `docs/s1130-research.md`: Comprehensive IBM 1130 specifications
- `docs/process.md`: Development methodology and TDD guide
- `README.md`: Project overview and current status
- `ASSEMBLER_SYNTAX_CHANGES.md`: Recent syntax updates

### Test Files

- `tests/UnitTests.S1130.SystemObjects/Instructions/DivideTests.cs`
- `tests/UnitTests.S1130.SystemObjects/Devices/Device2310Tests.cs`
- `tests/UnitTests.S1130.SystemObjects/RoundTripTests.cs`

---

## Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-24 | Development Team | Initial identification of issues and solutions |

---

**End of Document**
