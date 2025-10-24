# S1130 Development Process Guide

**Document Version**: 1.0
**Last Updated**: 2025-10-24
**Purpose**: Define development methodology, testing approach, and code quality standards for S1130 emulator improvements

---

## Table of Contents

1. [Overview](#overview)
2. [Development Methodology](#development-methodology)
3. [Test-Driven Development (TDD)](#test-driven-development-tdd)
4. [Code Quality Tools](#code-quality-tools)
5. [Best Practices](#best-practices)
6. [Git Workflow](#git-workflow)
7. [Documentation Standards](#documentation-standards)
8. [Code Review Process](#code-review-process)

---

## Overview

This document establishes the development process for the S1130 IBM 1130 emulator project. All contributors must follow these guidelines to ensure code quality, maintainability, and correctness.

### Core Principles

1. **Accuracy First**: Emulator behavior must match IBM 1130 hardware specifications
2. **Test Coverage**: All code must have comprehensive unit tests
3. **Documentation**: Code and features must be well-documented
4. **Code Quality**: Follow C# best practices and use automated quality tools
5. **Incremental Progress**: Small, focused commits with clear purposes

---

## Development Methodology

### Test-Driven Development (TDD)

All development work **MUST** follow the Red-Green-Refactor cycle:

```
┌─────────────────────────────────────────┐
│                                         │
│   RED → GREEN → REFACTOR → COMMIT      │
│    ↑                            │       │
│    └────────────────────────────┘       │
│                                         │
└─────────────────────────────────────────┘
```

#### Why TDD?

- **Correctness**: Tests define expected behavior before implementation
- **Regression Prevention**: New changes don't break existing functionality
- **Design Quality**: Writing tests first encourages better design
- **Documentation**: Tests serve as executable specifications
- **Confidence**: High test coverage enables safe refactoring

### Development Workflow

For each feature or bug fix:

1. **Understand Requirements** (from IBM documentation or issue report)
2. **Write Failing Test** (RED phase)
3. **Implement Minimum Code** (GREEN phase)
4. **Refactor and Optimize** (REFACTOR phase)
5. **Run All Tests** (ensure no regressions)
6. **Commit Changes** (with descriptive message)
7. **Repeat** for next requirement

---

## Test-Driven Development (TDD)

### The Red-Green-Refactor Cycle

#### 🔴 RED: Write a Failing Test

**Before writing any production code**, write a test that:
- Defines the expected behavior
- Fails because the feature doesn't exist yet
- Is specific and focused on one behavior

**Example**:
```csharp
[Fact]
public void Device2310_ReadCheck_ShouldSetDataErrorOnMismatch()
{
    // Arrange
    var cpu = new Cpu();
    var cartridge = new MemoryCartridge();
    var device = new Device2310(cpu, cartridge);

    // Write known data to disk
    var diskData = new ushort[320];
    diskData[0] = 0x1234;
    cartridge.Write(0, diskData, 320);

    // Write different data to memory buffer
    cpu.Memory[0x100] = 0x5678;

    // Setup IOCC with Read-Check enabled
    cpu.Memory[0x50] = 0x100;  // WCA
    cpu.Memory[0x51] = 0x4082;  // Device 08, InitRead, Read-Check=0x80

    // Act
    cpu.ExecuteIocc(0x50);
    device.Run();  // Complete the read operation

    // Sense device status
    cpu.Memory[0x52] = 0x0;     // WCA (not used for sense)
    cpu.Memory[0x53] = 0x4000;  // Device 08, Sense
    cpu.ExecuteIocc(0x52);

    // Assert
    // Bit 15 (0x8000) should be set indicating Data Error
    Assert.True((cpu.Acc & 0x8000) != 0, "Data Error bit should be set");
}
```

**Run Test**: `dotnet test` → Test should FAIL ❌

#### 🟢 GREEN: Make the Test Pass

Write the **minimum code** necessary to make the test pass:
- Don't over-engineer
- Don't add extra features
- Focus solely on making this test green

**Example**:
```csharp
// In Device2310.cs
private void CompleteReadOperation()
{
    var buffer = _cartridge.Read(_sector);

    if (_readCheck)
    {
        // Compare disk buffer with memory buffer
        bool mismatch = false;
        for (int i = 0; i < 320; i++)
        {
            if (buffer[i] != CpuInstance.Memory[_wca + i])
            {
                mismatch = true;
                break;
            }
        }

        if (mismatch)
        {
            _dataError = true;  // Set error flag
        }
    }

    CpuInstance.TransferToMemory(buffer, 321);
    _complete = true;
    ActivateInterrupt();
}

private ushort BuildStatusWord()
{
    ushort status = 0;

    if (_dataError) status |= 0x8000;  // Add Data Error bit
    if (_complete) status |= 0x4000;
    if (_cartridge == null) status |= 0x2000;
    if (_busy) status |= 0x1000;
    if (_cylinder.Current == 0) status |= 0x0800;

    return status;
}
```

**Run Test**: `dotnet test` → Test should PASS ✅

#### 🔄 REFACTOR: Improve the Code

Now that the test passes, improve the code quality:
- Remove duplication
- Extract methods for clarity
- Add constants for magic numbers
- Improve naming
- Add XML documentation

**Example**:
```csharp
// Extract magic number
private const ushort DataErrorBit = 0x8000;

// Extract comparison logic
private bool VerifyReadCheck(ushort[] diskBuffer)
{
    for (int i = 0; i < SectorDataSize; i++)
    {
        if (diskBuffer[i] != CpuInstance.Memory[_wca + i])
        {
            return false;  // Mismatch found
        }
    }
    return true;  // All data matches
}

// Update CompleteReadOperation
private void CompleteReadOperation()
{
    var buffer = _cartridge.Read(_sector);

    if (_readCheck && !VerifyReadCheck(buffer))
    {
        _dataError = true;
    }

    CpuInstance.TransferToMemory(buffer, SectorDataSize);
    _complete = true;
    ActivateInterrupt();
}
```

**Run All Tests**: `dotnet test` → All tests should still PASS ✅

#### ✅ COMMIT: Save Your Work

Commit with a clear, descriptive message:
```bash
git add .
git commit -m "feat(device2310): implement Read-Check verification with Data Error flag

- Add Read-Check buffer comparison in CompleteReadOperation
- Set Data Error bit (0x8000) when mismatch detected
- Extract VerifyReadCheck method for clarity
- Add test coverage for Read-Check mismatch scenario

Fixes: Device2310 Read-Check feature incomplete (#123)
Ref: IBM 1130 Functional Characteristics A26-5881-2, Section 4.3"
```

### Test Organization

#### Test File Structure

```
tests/
└── UnitTests.S1130.SystemObjects/
    ├── Instructions/
    │   ├── AddTests.cs
    │   ├── DivideTests.cs
    │   └── [one file per instruction]
    ├── Devices/
    │   ├── Device2310Tests.cs
    │   ├── Device2501Tests.cs
    │   └── [one file per device]
    ├── AssemblerTests.cs
    ├── CpuTests.cs
    └── RoundTripTests.cs
```

#### Test Naming Convention

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

**Examples**:
- `Add_ShortFormat_SetsCarryOnUnsignedOverflow()`
- `Divide_ByZero_SetsOverflowFlag()`
- `Device2310_Sense_ReturnsAtCylinderZeroBit()`
- `Assembler_ForwardReference_ResolvesCorrectly()`

#### Arrange-Act-Assert Pattern

All tests must follow AAA structure:

```csharp
[Fact]
public void Example_Test()
{
    // Arrange: Setup test data and dependencies
    var cpu = new Cpu();
    cpu.Memory[0x100] = 0x1234;
    cpu.Acc = 0x5678;

    // Act: Execute the operation being tested
    var instruction = new Add { /* setup */ };
    instruction.Execute(cpu);

    // Assert: Verify expected outcomes
    Assert.Equal(0x68AC, cpu.Acc);
    Assert.True(cpu.Carry);
}
```

#### Test Coverage Requirements

**Minimum Coverage Targets**:
- **Instructions**: 100% (all opcodes, formats, addressing modes)
- **Devices**: 90% (all functions, status bits, error conditions)
- **Assembler**: 95% (all directives, syntax variants, errors)
- **CPU Core**: 100% (register operations, memory access, interrupts)
- **Overall Project**: 85%+

**Coverage Report**:
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

### Testing IBM 1130 Accuracy

When testing for hardware accuracy:

1. **Reference Documentation**: Cite specific manual sections
2. **Known Results**: Use published test programs where available
3. **Edge Cases**: Test boundary conditions (overflow, underflow, zero)
4. **Timing**: Document timing differences (not cycle-accurate)

**Example**:
```csharp
/// <summary>
/// Test divide overflow behavior per IBM 1130 Functional Characteristics
/// Section 3.4.5: "When quotient exceeds 16 bits, Overflow is set and
/// ACC/EXT are left unchanged."
/// </summary>
[Fact]
public void Divide_QuotientOverflow_LeavesRegistersUnchanged()
{
    // Test implementation referencing spec
}
```

---

## Code Quality Tools

### Required Tools

All developers must use these tools in their workflow:

#### 1. Roslyn Analyzers (Built into .NET SDK)

**Installation**: Included with .NET 8 SDK

**Configuration**: Add to `Directory.Build.props`:
```xml
<Project>
  <PropertyGroup>
    <AnalysisMode>All</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

**Usage**:
```bash
dotnet build  # Warnings will cause build failure
```

#### 2. StyleCop Analyzers

**Installation**:
```bash
dotnet add package StyleCop.Analyzers
```

**Purpose**: Enforce C# style guidelines

**Configuration**: Create `stylecop.json` in project root:
```json
{
  "$schema": "https://raw.githubusercontent.com/DotNetAnalyzers/StyleCopAnalyzers/master/StyleCop.Analyzers/StyleCop.Analyzers/Settings/stylecop.schema.json",
  "settings": {
    "documentationRules": {
      "companyName": "S1130 Project",
      "copyrightText": "Licensed under MIT License"
    },
    "orderingRules": {
      "usingDirectivesPlacement": "outsideNamespace"
    }
  }
}
```

#### 3. SonarAnalyzer

**Installation**:
```bash
dotnet add package SonarAnalyzer.CSharp
```

**Purpose**: Detect bugs, code smells, and security vulnerabilities

**Run Analysis**:
```bash
dotnet build  # SonarAnalyzer runs automatically
```

#### 4. Code Coverage (Coverlet)

**Installation**:
```bash
dotnet add package coverlet.collector
```

**Generate Coverage**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**View Report**:
```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# Open report
open coverage-report/index.html
```

#### 5. JetBrains ReSharper / Rider (Optional but Recommended)

**Features**:
- Real-time code analysis
- Quick fixes and refactorings
- Code cleanup on save
- Test runner integration

**Configuration**: Use project's `.editorconfig` and `.DotSettings`

### Code Quality Checks

#### Pre-Commit Checklist

Before committing, ensure:

- [ ] All tests pass: `dotnet test`
- [ ] No build warnings: `dotnet build`
- [ ] Code coverage maintained or improved
- [ ] StyleCop violations resolved
- [ ] SonarAnalyzer issues addressed
- [ ] XML documentation added for public APIs
- [ ] No commented-out code
- [ ] No `TODO` comments (convert to GitHub issues)

#### Continuous Integration

**GitHub Actions Workflow** (`.github/workflows/ci.yml`):
```yaml
name: CI

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
```

---

## Best Practices

### C# Coding Standards

#### Naming Conventions

```csharp
// Classes: PascalCase
public class Device2310 { }

// Methods: PascalCase
public void ExecuteIocc() { }

// Properties: PascalCase
public ushort Accumulator { get; set; }

// Private fields: _camelCase with underscore
private ushort _accumulator;
private bool _readCheck;

// Constants: PascalCase
public const ushort DataErrorBit = 0x8000;

// Local variables: camelCase
int effectiveAddress = 0;

// Parameters: camelCase
public void SetMemory(int address, ushort value) { }
```

#### File Organization

```csharp
// 1. Using directives (outside namespace, alphabetically sorted)
using System;
using System.Collections.Generic;

// 2. Namespace
namespace S1130.SystemObjects.Devices
{
    // 3. XML documentation
    /// <summary>
    /// Emulates the IBM 2310 Disk Drive.
    /// </summary>
    public class Device2310 : DeviceBase
    {
        // 4. Constants
        private const int SectorDataSize = 320;

        // 5. Fields (private)
        private readonly ICartridge _cartridge;
        private bool _readCheck;

        // 6. Constructors
        public Device2310(ICpu cpu, ICartridge cartridge)
            : base(cpu)
        {
            _cartridge = cartridge;
        }

        // 7. Properties (public then private)
        public override byte DeviceCode => 0x08;

        // 8. Public methods
        public override void ExecuteIocc()
        {
            // Implementation
        }

        // 9. Private methods
        private void HandleInitRead()
        {
            // Implementation
        }
    }
}
```

#### Documentation Requirements

**All public APIs require XML documentation**:

```csharp
/// <summary>
/// Executes the I/O Channel Command for the disk device.
/// </summary>
/// <remarks>
/// Processes the IOCC at the address specified by the XIO instruction.
/// Supported functions: Sense, Control (seek), Initiate Read, Initiate Write.
/// See IBM 1130 Functional Characteristics Section 4.3 for details.
/// </remarks>
/// <exception cref="InvalidOperationException">
/// Thrown when device code doesn't match this device.
/// </exception>
public override void ExecuteIocc()
{
    // Implementation
}
```

#### Error Handling

```csharp
// Use specific exceptions
throw new ArgumentOutOfRangeException(nameof(address),
    $"Memory address {address:X4} exceeds maximum {MaxMemory:X4}");

// Validate parameters
public void SetMemory(int address, ushort value)
{
    if (address < 0 || address >= MaxMemory)
    {
        throw new ArgumentOutOfRangeException(nameof(address));
    }

    _memory[address] = value;
}

// Document exceptions
/// <exception cref="ArgumentOutOfRangeException">
/// Address is outside valid range (0-32767).
/// </exception>
```

#### Avoid Code Smells

**Magic Numbers** ❌:
```csharp
// Bad
if ((status & 0x8000) != 0) { }
```

**Named Constants** ✅:
```csharp
// Good
private const ushort DataErrorBit = 0x8000;
if ((status & DataErrorBit) != 0) { }
```

**Long Methods** ❌:
```csharp
// Bad
public void ExecuteIocc()
{
    // 200 lines of code
}
```

**Extracted Methods** ✅:
```csharp
// Good
public void ExecuteIocc()
{
    switch (_function)
    {
        case 0: HandleSenseDevice(); break;
        case 1: HandleControl(); break;
        case 2: HandleInitRead(); break;
        case 4: HandleInitWrite(); break;
    }
}

private void HandleSenseDevice() { /* focused implementation */ }
```

**Duplicated Code** ❌:
```csharp
// Bad
if (tag == 1) { ea = xr1 + disp; }
if (tag == 2) { ea = xr2 + disp; }
if (tag == 3) { ea = xr3 + disp; }
```

**Abstraction** ✅:
```csharp
// Good
ushort GetIndexRegister(int tag)
{
    return tag switch
    {
        1 => _xr1,
        2 => _xr2,
        3 => _xr3,
        _ => 0
    };
}

ea = GetIndexRegister(tag) + disp;
```

### Performance Considerations

1. **Avoid Premature Optimization**: Prioritize correctness and clarity
2. **Profile Before Optimizing**: Use benchmarks to identify bottlenecks
3. **Cache Frequently Accessed Data**: Example: index register values
4. **Use Appropriate Data Structures**: Arrays for memory, dictionaries for symbol tables

**Example Benchmark**:
```csharp
using BenchmarkDotNet.Attributes;

public class InstructionBenchmarks
{
    [Benchmark]
    public void Add_ShortFormat()
    {
        // Measure instruction execution time
    }
}
```

---

## Git Workflow

### Branch Strategy

```
master (main branch)
  ├── feature/emulator-improvements (long-lived feature branch)
  │     ├── fix/device2310-read-check (short-lived task branch)
  │     ├── fix/divide-remainder-semantics
  │     └── feat/disk-next-sector-bits
  └── hotfix/critical-bug (if needed)
```

**Branch Types**:
- `master`: Production-ready code, protected
- `feature/*`: New features or major improvements
- `fix/*`: Bug fixes
- `refactor/*`: Code quality improvements
- `docs/*`: Documentation updates

### Commit Message Format

Follow Conventional Commits:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code refactoring (no behavior change)
- `test`: Adding or updating tests
- `docs`: Documentation changes
- `chore`: Build, tooling, dependencies

**Example**:
```
fix(device2310): implement Read-Check verification

- Add buffer comparison after read operation
- Set Data Error bit (0x8000) on mismatch
- Add comprehensive test coverage for Read-Check scenarios

The Read-Check feature was parsed but never verified. This
implements the comparison logic per IBM 1130 Functional
Characteristics Section 4.3.2.

Fixes #123
Ref: IBM 1130 Functional Characteristics A26-5881-2
```

### Pull Request Process

1. **Create PR**: From feature branch to master
2. **Fill Template**: Description, testing notes, references
3. **Request Review**: At least one reviewer
4. **CI Checks**: All tests pass, coverage maintained
5. **Address Feedback**: Make requested changes
6. **Merge**: Squash and merge when approved

**PR Template**:
```markdown
## Description
Brief description of changes

## Changes Made
- Item 1
- Item 2

## Testing
- [ ] All tests pass
- [ ] New tests added
- [ ] Coverage maintained/improved

## IBM 1130 Reference
- Document: [citation]
- Section: [number]

## Related Issues
Fixes #123
```

---

## Documentation Standards

### Code Documentation

**XML Documentation** for all public APIs:
```csharp
/// <summary>
/// Brief description of class/method.
/// </summary>
/// <remarks>
/// Detailed explanation, usage notes, IBM 1130 references.
/// </remarks>
/// <param name="paramName">Parameter description</param>
/// <returns>Return value description</returns>
/// <exception cref="ExceptionType">When thrown</exception>
```

### Markdown Documentation

**Document Structure**:
1. Title and metadata
2. Table of contents
3. Overview/introduction
4. Detailed sections
5. Examples
6. References

**IBM 1130 References**:
Always cite sources:
```markdown
Per IBM 1130 Functional Characteristics (A26-5881-2), Section 3.4.5,
divide overflow behavior is defined as...
```

### README Updates

Update README.md when:
- Adding new features
- Changing build process
- Adding dependencies
- Updating system requirements

---

## Code Review Process

### Reviewer Checklist

When reviewing code:

- [ ] Code follows TDD (tests present and passing)
- [ ] Changes match IBM 1130 specifications
- [ ] No code smells or anti-patterns
- [ ] Proper error handling
- [ ] XML documentation complete
- [ ] No magic numbers (constants used)
- [ ] No commented-out code
- [ ] Commit messages follow format
- [ ] Test coverage maintained/improved
- [ ] Performance considerations addressed

### Review Comments

**Constructive Feedback**:
```
Consider extracting this logic into a separate method for clarity:

```csharp
private ushort CalculateEffectiveAddress(byte tag, sbyte displacement)
{
    // extracted logic
}
```

This would improve testability and readability.

Reference: Clean Code, Chapter 3 - Functions should do one thing.
```

**Approval**:
```
LGTM! ✅

Changes correctly implement Read-Check per IBM 1130 spec.
Test coverage is comprehensive and all checks pass.
```

---

## Summary

### Key Takeaways

1. **Always use TDD**: Red → Green → Refactor → Commit
2. **Use quality tools**: Analyzers, coverage, linting
3. **Document everything**: Code, tests, decisions
4. **Reference IBM specs**: Cite sources for accuracy
5. **Review carefully**: Maintain high quality standards

### Tool Setup Checklist

- [ ] .NET 8 SDK installed
- [ ] StyleCop.Analyzers package added
- [ ] SonarAnalyzer.CSharp package added
- [ ] Coverlet.collector package added
- [ ] ReportGenerator tool installed
- [ ] .editorconfig configured
- [ ] Directory.Build.props configured
- [ ] CI/CD pipeline configured

### Getting Started

1. Clone repository
2. Install tools (see above)
3. Read IBM 1130 documentation
4. Review existing tests for examples
5. Pick an issue from improvements.md
6. Write failing test (RED)
7. Implement fix (GREEN)
8. Refactor (REFACTOR)
9. Commit and push
10. Create pull request

---

**Questions?** Open an issue on GitHub or contact the maintainers.

**End of Document**
