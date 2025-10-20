# S1130 Documentation

This directory contains comprehensive documentation for the S1130 IBM 1130 Emulator.

## Documents

### [InternalOperation.md](InternalOperation.md)
Complete guide to the internal workings of the S1130 emulator, including:
- CPU architecture and registers
- Instruction execution cycle
- Memory system
- Device architecture (block-mode vs character-mode)
- Interrupt system
- Instruction set implementation
- Testing architecture
- Common operations and debugging

**Read this if you want to:**
- Understand how the emulator works internally
- Add new instructions or devices
- Debug emulator behavior
- Write comprehensive tests
- Optimize performance

### [Assembler.md](Assembler.md)
Complete guide to the IBM 1130 assembler, including:
- Assembly language syntax
- All directives (ORG, DC, EQU, BSS, BES)
- Complete instruction set documentation
- Addressing modes (short, long, indexed, indirect)
- Expressions and symbols
- Two-pass assembly process
- Error handling
- Usage examples and API

**Read this if you want to:**
- Write IBM 1130 assembly programs
- Understand assembly language syntax
- Learn about addressing modes
- Use the assembler API
- Debug assembly errors

## Quick Start

### For Users
Start with [Assembler.md](Assembler.md) - it contains everything you need to write and assemble IBM 1130 programs.

### For Developers
Start with [InternalOperation.md](InternalOperation.md) - it explains the architecture and how to extend the emulator.

## Additional Resources

### Project Files
- `.github/copilot-instructions.md` - AI agent instructions and patterns
- `README.md` (root) - Project overview and setup
- `ImplementationPlan.md` (root) - Original implementation roadmap

### Code Organization
```
src/S1130.SystemObjects/
├── Cpu.cs                 - Main CPU implementation
├── Assembler.cs           - IBM 1130 assembler
├── Instructions/          - Individual instruction implementations
├── Devices/               - I/O device implementations
└── Utility/               - Helper classes and conversion codes

tests/UnitTests.S1130.SystemObjects/
├── InstructionTests/      - Tests for each instruction
├── DeviceTests/           - Tests for each device
├── AssemblerTests.cs      - Assembler functionality tests
└── ProgramTests.cs        - End-to-end program tests
```

## Contributing

When adding features:
1. Read the relevant documentation
2. Follow existing patterns (see `.github/copilot-instructions.md`)
3. Add comprehensive tests
4. Update documentation as needed

## Getting Help

- Check documentation first
- Look at test files for examples
- Review similar existing implementations
- Examine the copilot instructions for patterns
