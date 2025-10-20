# AI Agent Instructions for S1130 IBM 1130 Emulator

## Project Overview
This is a complete IBM 1130 computer emulator implemented in C# .NET 8. The emulator simulates the CPU, instruction set, memory management, device I/O, and interrupt handling of the original 1960s mainframe computer.

## Architecture Components

### Core System
- **`Cpu`** class: Central processing unit implementing `ICpu` interface with 16-bit registers (Acc, Ext, Iar), memory management, and instruction execution cycle
- **`IInstruction`** implementations: Each IBM 1130 instruction (Add, Load, Store, etc.) extends `InstructionBase` with opcode-specific logic
- **`DeviceBase`**: Abstract base for all I/O devices (2501 card reader, 2310 disk drive) with interrupt management
- **Interrupt System**: `InterruptPool` manages interrupt objects, devices use `ActivateInterrupt()`/`DeactivateInterrupt()`

### Key Patterns

#### Instruction Architecture
All instructions inherit from `InstructionBase` and implement:
- Address calculation with `GetEffectiveAddressNoXr()` for short/long format and indirect addressing
- Sign extension with `SignExtend()` for 16-bit to 32-bit operations  
- Bit manipulation using `Mask16`/`Mask32` constants
- Format detection via `cpu.FormatLong` and `cpu.IndirectAddress` properties

#### Memory and Addressing
- 16-bit word-addressable memory with default size `32768` words
- Address calculation respects IBM 1130 addressing modes (short/long format, direct/indirect)
- Index registers modify effective addresses via `GetOffset(cpu)` 

#### Device Integration
- Devices implement `IDevice` with unique `DeviceCode` byte
- Use `ExecuteIocc()` for I/O Channel Command execution
- Device state accessed via CPU properties: `IoccDeviceCode`, `IoccFunction`, `IoccAddress`, `IoccModifiers`

**Two Device Architectures:**
1. **Block-Mode Devices** (2501, 2310): DMA-like, single InitRead/InitWrite transfers entire block automatically
2. **Character-Mode Devices** (1442): CPU-intensive, CPU must issue Read/Write command for EACH character
   - IoccAddress used as direct memory address (not IOCC pointer)
   - Device generates interrupt for each character ready
   - Timing critical: Must respond within 700-800µs

## Testing Patterns

### Test Organization
- **Instruction Tests**: Inherit from `InstructionTestBase` with standardized `ExecAndTest()` method
- **Device Tests**: Inherit from `DeviceTestBase` with device-specific I/O helpers (`InitiateRead()`, `SenseDevice()`)
- **Test Naming**: Format: `Execute_[Operation]_[Format]_[Condition]_[ExpectedResult]`

### Test Base Classes
```csharp
// For instruction tests
protected void ExecAndTest(ushort initialAcc, bool initialCarry, bool initialOverflow, 
                          ushort expectedAcc, bool expectedCarry, bool expectedOverflow)

// For device tests  
protected void InitiateRead(IDevice device, int wca, int wc)
protected void SenseDevice(IDevice device, ushort resetBits = 0)
```

### Instruction Building
Use `InstructionBuilder` for test instruction creation:
- `BuildShort(OpCodes opCode, uint tag, uint displacement)` for short format
- `BuildLongAtAddress()` for long format instructions
- `BuildLongIndirectAtAddress()` for indirect addressing

## Development Workflows

### Build and Test
- **Build**: Use `dotnet build` task or VS Code task `process: build`
- **Test**: 335+ unit tests run with `dotnet test` (completes <2 seconds)
- **Watch**: Use `dotnet watch` task for continuous rebuild during development

### Code Organization
- **src/S1130.SystemObjects/**: Main emulator code
- **tests/UnitTests.S1130.SystemObjects/**: Comprehensive test suite
- **Instructions/**: Individual instruction implementations
- **Devices/**: I/O device emulations
- **Utility/**: Conversion codes and helper functions

## IBM 1130 Specifics

### CPU Registers
- **Acc**: 16-bit accumulator for arithmetic operations
- **Ext**: 16-bit extension register for double-precision operations  
- **Iar**: Instruction Address Register (program counter)
- **Index Registers**: 1-3 available for address modification

### Instruction Format
- **Short Format**: Opcode + tag + 8-bit displacement (relative to IAR)
- **Long Format**: Opcode + tag + separate word for 16-bit address
- **Indirect**: Additional level of address indirection via memory

### Status Flags
- **Carry**: Set on arithmetic overflow from bit 0
- **Overflow**: Set on signed arithmetic overflow
- Flags persist across instructions unless explicitly modified

## Common Patterns

### Adding New Instructions
1. Create class inheriting `InstructionBase`
2. Implement `Execute(ICpu cpu)` method
3. Add to `OpCodes` enum and instruction set initialization
4. Create corresponding test class inheriting `InstructionTestBase`

### Adding New Devices
1. Extend `DeviceBase` with unique `DeviceCode`
2. Implement `ExecuteIocc()` for I/O command processing
3. Use interrupt system for device status changes
4. Create tests inheriting `DeviceTestBase`

### Memory Access
Always use CPU indexer for memory access: `cpu[address] = value` respects address bounds and debugging features.

## Project State
- ✅ CPU core and instruction set complete
- ✅ IBM 1130 Assembler fully implemented
- ✅ 2501 Card Reader (block-mode) fully implemented
- ✅ 1442 Card Read Punch (character-mode) fully implemented
- 🚧 2310 Disk Drive partially complete
- 395 comprehensive unit tests passing
- Complete documentation in docs/ folder