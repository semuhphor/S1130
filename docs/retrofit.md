# Retrofitting the C# Assembler with a Modern Parser

## 1. Problem Statement

The current assembler in the C# `S1130` project relies on `string.Split()` for parsing assembly code. This approach has several significant drawbacks:

*   **Ambiguity:** It cannot reliably distinguish between different parts of an instruction if there is unconventional whitespace.
*   **Fragility:** The parser is not robust and can easily break when presented with slightly different source code formats.
*   **Inflexibility:** It is not well-suited to handle both the original fixed-column punch card format and more modern, free-form text documents.
*   **Poor Maintainability:** The parsing logic is imperative and complex, making it difficult to understand, debug, and extend.

## 2. Analysis of Alternative Approaches

To inform the best path forward, two alternative assembler implementations in Rust have been analyzed.

### `wrightmikea/S1130-rs`

This project is a full-featured IBM 1130 emulator. Its assembler uses a **parser-combinator** library (`nom`).

*   **Approach:** It defines a formal grammar for the assembly language in a declarative way. Small parsers for individual components (labels, opcodes, operands) are combined to create a complete line parser.
*   **Conclusion:** This approach is extremely robust, flexible, and maintainable. It is the gold standard for this type of problem and serves as the inspiration for the proposed solution.

### `softwarewrighter/game-lib`

This project contains a simplified 1130 assembler for an educational game. It uses a basic `string.split_whitespace()` approach.

*   **Approach:** This parser is very simple and handles only a small, fixed subset of the 1130 assembly language. It does not support labels, expressions, or complex formatting. Its instruction encoding is also a custom, simplified mapping, not the true 1130 instruction set.
*   **Conclusion:** While functional for its limited purpose, this approach suffers from the same fragility and lack of flexibility as the current C# implementation. It is not a suitable model for a general-purpose assembler.

## 3. Proposed Solution

The analysis confirms that the parser-combinator strategy is the superior approach. We propose retrofitting the C# assembler with a parser-combinator library to achieve the robustness and flexibility demonstrated in the `wrightmikea/S1130-rs` project.

For the C# project, we recommend using **`Sprache`**, a popular and lightweight parser-combinator library available as a NuGet package.

## 4. Architecture

The core two-pass architecture of the assembler can remain, but the line-parsing mechanism will be completely replaced.

*   **Current Architecture:** `Assembler.cs` -> `AssemblyLine.cs` (manual string splitting)
*   **Proposed Architecture:** `Assembler.cs` -> `AssemblyGrammar.cs` (`Sprache`-based parsing)

A new class, `AssemblyGrammar`, will define the grammar of the 1130 assembly language in a declarative style. The `Assembler` will then use this grammar to parse the source code, replacing the current `AssemblyLine` class.

## 5. Product Requirements

*   The assembler must correctly parse both fixed-format and free-form 1130 assembly code.
*   The parser must be unambiguous and provide clear, actionable error messages with line and column information.
*   The new implementation must be easily extensible to support new instructions, pseudo-operations, or syntax variations.
*   The performance of the refactored assembler should be at least on par with the existing implementation.
*   The refactoring should not introduce any regressions; all existing assembler tests must pass.

## 6. Design

1.  **Add `Sprache` Dependency:** The `Sprache` NuGet package will be added to the `S1130.SystemObjects` project.

2.  **Create `AssemblyGrammar.cs`:** A new static class will be created to house the `Sprache` parser definitions.

3.  **Define the Grammar:** The grammar will be defined using `Sprache`'s fluent API, from the bottom up:
    *   **Basic Elements:** Parsers for whitespace, comments, identifiers, numbers (decimal and hex), and expressions (e.g., `*+5`).
    *   **Line Components:** Parsers for labels, opcodes, and operands.
    *   **Instruction Formats:** Combinators to parse different instruction formats (short, long, indirect, indexed).
    *   **Complete Line:** A top-level parser that combines the above to parse a full line of assembly code into a structured `AssemblyLine` object.

4.  **Refactor `Assembler.cs`:** The `Pass1ProcessLine` and `Pass2ProcessLine` methods will be updated to use `AssemblyGrammar.ParseLine(line)` instead of instantiating `new AssemblyLine(line)`.

## 7. Best Practices

*   **Test-Driven Development (TDD):** The grammar should be developed with a full suite of unit tests. Each parser combinator should be tested independently.
*   **Modularity:** The grammar should be broken down into small, reusable parser methods.
*   **Readability:** The declarative nature of `Sprache` should be leveraged to make the grammar easy to read and understand.
*   **Error Reporting:** `Sprache`'s error reporting capabilities should be used to provide meaningful error messages to the user.

## 8. Recommended Process

1.  **Setup:** Add the `Sprache` NuGet package to the `S1130.SystemObjects.csproj` file.
2.  **Grammar Definition (Bottom-Up):**
    *   Create `AssemblerGrammar.cs`.
    *   Define parsers for basic tokens (identifiers, numbers, comments).
    *   Define parsers for expressions (`*`, `*+1`, etc.).
    *   Define parsers for labels, opcodes, and operands.
3.  **Unit Testing:** Create a new `AssemblerGrammarTests.cs` file and write tests for each parser.
4.  **Integration:**
    *   Modify the `AssemblyLine` class to be a simple data container (POCO).
    *   Update `Assembler.cs` to use the new `AssemblyGrammar` to parse lines.
5.  **Validation:** Run all existing unit tests in `AssemblerTests.cs` to ensure that the new parser is working correctly and that there are no regressions.

## 9. Plan

| Phase | Task | Estimated Effort |
| :--- | :--- | :--- |
| **1. Setup & Basic Grammar** | Add `Sprache` and define parsers for basic tokens. | 1-2 hours |
| **2. Grammar Development** | Define the full grammar for all instruction formats. | 4-6 hours |
| **3. Unit Testing** | Write comprehensive unit tests for the new grammar. | 3-4 hours |
| **4. Integration & Refactoring** | Integrate the new grammar into the `Assembler` class. | 2-3 hours |
| **5. Validation & Bug Fixing** | Run all tests and fix any issues. | 2-4 hours |
| **Total** | | **12-19 hours** |
