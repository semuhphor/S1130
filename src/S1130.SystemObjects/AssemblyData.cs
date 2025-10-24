using System;

namespace S1130.SystemObjects
{
    /// <summary>
    /// Represents an error during assembly
    /// </summary>
    public class AssemblyError
    {
        /// <summary>
        /// The 1-based line number where the error occurred
        /// </summary>
        public int LineNumber { get; init; }

        /// <summary>
        /// Error message describing the issue
        /// </summary>
        public string Message { get; init; }
    }

    /// <summary>
    /// Represents a single line in the assembly listing
    /// </summary>
    public class ListingLine
    {
        /// <summary>
        /// 1-based source line number
        /// </summary>
        public int LineNumber { get; init; }

        /// <summary>
        /// Memory address in hex (4 digits)
        /// </summary>
        public ushort Address { get; init; }

        /// <summary>
        /// Instruction opcode in hex (null for comments, directives without code)
        /// </summary>
        public ushort? OpCode { get; init; }

        /// <summary>
        /// Original source code line
        /// </summary>
        public string SourceCode { get; init; }
    }

    /// <summary>
    /// Results from an assembly operation
    /// </summary>
    public class AssemblyResult
    {
        /// <summary>
        /// Whether assembly was successful (no errors)
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Any errors encountered during assembly
        /// </summary>
        public AssemblyError[] Errors { get; init; }

        /// <summary>
        /// Assembly listing showing the processed code
        /// </summary>
        public string[] Listing { get; init; }

        /// <summary>
        /// Structured listing with line numbers, addresses, opcodes
        /// </summary>
        public ListingLine[] ListingLines { get; init; }
    }
}