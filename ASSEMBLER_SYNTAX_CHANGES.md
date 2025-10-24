# IBM 1130 Assembler Syntax Changes

## Summary
Major assembler syntax overhaul completed to eliminate ambiguity and require explicit format/tag specification on all instructions.

## Changes Made

### 1. Comment Syntax
**Old Syntax:**
- `*` could appear anywhere on line for comments
- Created ambiguity with `*` as current-address operator

**New Syntax:**
- `*` in column 1 only for full-line comments
- `//` for inline comments (can appear after code)

**Examples:**
```assembly
* This is a full-line comment (column 1 only)
      LD   L VAL1      // This is an inline comment
```

### 2. Format/Tag Specification
**Old Syntax:**
- Format/tag was optional: `LD VAL1` defaulted to short format
- Index registers used comma: `LD VAL1,X1`
- Indirect used suffix: `LD L VAL1 I`

**New Syntax:**
- Format/tag is MANDATORY as first token after mnemonic
- `.` represents short format with no index
- `L` represents long format
- `1`, `2`, `3` represent short format with index register
- `L1`, `L2`, `L3` represent long format with index register
- `I`, `I1`, `I2`, `I3` represent indirect addressing

**Examples:**
```assembly
      LD   . VAL1       // Short format, no index
      LD   L VAL1       // Long format, no index
      LD   1 VAL1       // Short format with XR1
      LD   L2 VAL1      // Long format with XR2
      LD   I VAL1       // Short indirect, no index
      LD   I1 VAL1      // Short indirect with XR1
```

### 3. Branch Instructions (BSC/BOSC/BSI/MDX)
**Old Syntax:**
- Conditions appeared first: `BSC O LOOP` or `BSC L O LOOP`
- Index with comma: `BSC O,1 LOOP`
- Indirect as suffix: `BSC L O LOOP I`

**New Syntax:**
- **SHORT FORMAT**: No address - just skips next instruction if condition met
- **LONG/INDIRECT FORMAT**: Requires address for branching
- Format/tag first, then optional conditions, then address (if long/indirect)

**Examples:**
```assembly
      BSC  O            // Short format skip if overflow OFF (no address!)
      BSC  Z            // Short format skip if zero
      BSC               // Short format unconditional skip (NOP - no condition/address)
      BSC  L LOOP       // Long format unconditional branch to LOOP
      BSC  L O LOOP     // Long format branch to LOOP if overflow OFF
      BSC  L2 +- LOOP   // Long format with XR2, branch if positive or minus
      BSC  I O LOOP     // Indirect branch if overflow OFF
      BOSC O            // Short skip with interrupt reset
      BOSC L LOOP       // Long branch with interrupt reset
```

**Important:** Short format BSC/BOSC have NO address operand - they only skip the next instruction. Use long format (L) or indirect (I) to branch to a specific address.

### 4. Special Cases
**Shift Instructions:** No change - shift count is numeric, no format/tag needed
```assembly
      SLA  4            // Shift left 4 bits
      SLT  16           // Shift left together 16 bits
```

**WAIT:** No operands, no change
```assembly
      WAIT
```

**LDS:** Uses immediate value, no format/tag
```assembly
      LDS  /3           // Load status byte
```

## Files Modified

### Core Assembler
- `src/S1130.SystemObjects/AssemblyLine.cs` - Comment parsing
- `src/S1130.SystemObjects/Assembler.cs` - Format/tag parsing, branch processing
- `src/S1130.SystemObjects/Instructions/InstructionBase.cs` - Disassembler output
- `src/S1130.SystemObjects/Instructions/BranchSkip.cs` - BSC/BOSC disassembly
- `src/S1130.SystemObjects/Instructions/BranchStore.cs` - BSI disassembly
- `src/S1130.SystemObjects/Instructions/ModifyIndex.cs` - MDX disassembly

### Tests
- `tests/UnitTests.S1130.SystemObjects/AssemblerTests.cs` - Updated 7 test methods

### Web Frontend
- `web-frontend/src/components/AssemblerEditor.tsx` - Example program

## Test Results
- All 473 unit tests pass
- Assembler correctly parses new syntax
- Disassembler correctly outputs new syntax
- Round-trip (assemble → disassemble → assemble) produces identical output

## Breaking Changes
⚠️ **All existing assembly code must be updated to new syntax:**
1. Add `.` for short format instructions with no index: `LD VAL` → `LD . VAL`
2. Change index register syntax: `LD VAL,X1` → `LD 1 VAL`
3. Change branch syntax: `BSC O LOOP` → `BSC . O LOOP`
4. Change inline comments from `* comment` to `// comment`
5. Ensure full-line comments have `*` in column 1 (first character)

## Benefits
✅ Eliminates ambiguity between `*` as comment vs. current-address operator
✅ Makes format/tag explicit and visible in source code
✅ Simplifies parser - no need to infer format from context
✅ Consistent syntax across all instruction types
✅ Easier to teach and understand for new users
✅ Disassembler output can be directly reassembled without changes

## Migration Guide
To update existing assembly code:
1. Add `.` after instruction mnemonic if no format/tag was specified
2. Replace `,X1` with `1`, `,X2` with `2`, `,X3` with `3`
3. For long format with index: `L VAL,X1` → `L1 VAL`
4. For indirect: move `I` before address and combine with format/tag
5. Change inline `*` comments to `//`
6. Ensure full-line `*` comments start in column 1

## Completion Date
January 2025

## Status
✅ Complete - All tests passing, ready for production use
