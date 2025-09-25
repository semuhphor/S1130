# Contributing to S1130

We welcome contributions to the IBM 1130 Emulator! This document provides guidelines to help you contribute effectively.

## Code Style and Conventions

### C# Coding Standards

- **Indentation**: Use tabs, not spaces (consistent with existing codebase)
- **Braces**: Opening braces on same line for methods, properties, and control structures
- **Naming Conventions**:
  - Public properties and methods: `PascalCase`
  - Private fields: `_camelCase` with underscore prefix
  - Local variables: `camelCase`
  - Constants: `PascalCase`

### Documentation Requirements

#### XML Documentation
All public APIs must include XML documentation comments:

```csharp
/// <summary>
/// Executes the instruction on the specified CPU instance.
/// </summary>
/// <param name="cpu">The CPU instance to execute the instruction on</param>
/// <exception cref="DivideByZeroException">Thrown when attempting division by zero</exception>
public void Execute(ICpu cpu)
```

#### Code Comments
- Use inline comments to explain complex logic or hardware emulation nuances
- Include references to IBM 1130 documentation where applicable
- Explain non-obvious implementation decisions

### Error Handling

#### Exception-Prone Areas
Robust error handling is required for:
- **Device operations**: File I/O, hardware simulation
- **Memory access**: Bounds checking, invalid addresses  
- **Instruction execution**: Invalid opcodes, arithmetic overflow
- **Interrupt handling**: State management, timing issues

#### Error Documentation
- Document all possible failure modes in method comments
- Use specific exception types rather than generic exceptions
- Provide meaningful error messages for debugging

## Testing Requirements

### Test Coverage
- **New Features**: Must include comprehensive unit tests
- **Bug Fixes**: Must include regression tests
- **Edge Cases**: Test boundary conditions and error scenarios

### Test Organization
- Place tests in appropriate `tests/UnitTests.S1130.SystemObjects/` subdirectories
- Follow existing naming patterns: `{ClassName}Tests.cs`
- Use descriptive test method names: `Execute_D_Long_Xr2_PositiveOffset()`

### Test Patterns
```csharp
[Fact]
public void Execute_Add_SetsCarryOnOverflow()
{
    // Arrange
    BeforeEachTest();
    InstructionBuilder.BuildLongAtIar(OpCodes.Add, 0, 0x1010, InsCpu);
    
    // Act & Assert
    ExecAndTest(initialAcc: 0xFFFF, expectedAcc: 0xFFFE, expectedCarry: true);
}
```

## Development Process

### Before Starting Work
1. **Check existing issues** for similar work or discussions
2. **Create an issue** describing your proposed changes for significant features
3. **Fork the repository** and create a feature branch

### Making Changes
1. **Write tests first** (Test-Driven Development)
2. **Implement your changes** following coding standards
3. **Ensure all tests pass**: `dotnet test`
4. **Verify build succeeds**: `dotnet build`

### Code Review Process
1. **Submit a pull request** with clear description of changes
2. **Reference related issues** using GitHub keywords (closes #123)
3. **Respond to feedback** constructively and promptly
4. **Update documentation** if APIs or behavior changes

## Architecture Guidelines

### Adding New Instructions
1. Create class inheriting from `InstructionBase`
2. Implement `IInstruction` interface
3. Add comprehensive unit tests
4. Update `OpCodes` enum if needed

### Device Implementation
1. Inherit from `DeviceBase`
2. Implement required abstract methods
3. Handle interrupts properly using `ActivateInterrupt()`
4. Include device status and error handling

### Memory and CPU State
- Prefer immutable operations where possible  
- Document side effects clearly
- Validate parameters at public API boundaries

## Submitting Pull Requests

### Pull Request Template
```
## Description
Brief description of changes made.

## Related Issues  
Closes #123

## Testing
- [ ] All existing tests pass
- [ ] New tests added for new functionality
- [ ] Manual testing performed (if applicable)

## Documentation
- [ ] XML documentation added for public APIs
- [ ] README updated (if needed)
- [ ] Code comments added for complex logic
```

### Review Checklist
- [ ] Code follows project conventions
- [ ] Tests provide adequate coverage
- [ ] Documentation is complete and accurate
- [ ] No breaking changes to public APIs
- [ ] Performance impact considered

## Questions and Support

- **GitHub Issues**: For bug reports and feature requests
- **Discussions**: For questions about implementation or architecture
- **Email**: Contact maintainers for sensitive issues

Thank you for contributing to the S1130 IBM 1130 Emulator!