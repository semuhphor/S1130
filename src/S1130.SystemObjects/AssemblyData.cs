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
    }
}