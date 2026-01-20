using System.Collections.Generic;

namespace S1130.SystemObjects
{
    /// <summary>
    /// Represents an assembly error with line number, source line, and error message.
    /// </summary>
    public class AssemblyError
    {
        /// <summary>
        /// Line number where the error occurred (1-based)
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The source code line that caused the error
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// Description of the error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Type/category of the error
        /// </summary>
        public ErrorType Type { get; set; }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Message}\n  {Line}";
        }
    }

    /// <summary>
    /// Categorizes assembly errors for better error reporting and filtering.
    /// </summary>
    public enum ErrorType
    {
        InvalidOpcode,
        InvalidModifier,
        InvalidOperandCount,
        ValueOutOfRange,
        UndefinedSymbol,
        InvalidExpression,
        InvalidSyntax,
        DuplicateLabel,
        InvalidPattern,
        Other
    }

    /// <summary>
    /// Represents an assembly warning (non-fatal issues).
    /// </summary>
    public class AssemblyWarning
    {
        /// <summary>
        /// Line number where the warning occurred (1-based)
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The source code line that caused the warning
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// Description of the warning
        /// </summary>
        public string Message { get; set; }

        public override string ToString()
        {
            return $"Line {LineNumber}: Warning: {Message}\n  {Line}";
        }
    }

    /// <summary>
    /// Contains the results of an assembly operation, including any errors,
    /// warnings, and generated machine code.
    /// </summary>
    public class AssemblyResult
    {
        /// <summary>
        /// True if assembly completed without errors
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Collection of all errors encountered during assembly
        /// </summary>
        public List<AssemblyError> Errors { get; set; } = new List<AssemblyError>();

        /// <summary>
        /// Collection of all warnings generated during assembly
        /// </summary>
        public List<AssemblyWarning> Warnings { get; set; } = new List<AssemblyWarning>();

        /// <summary>
        /// Generated machine code words (empty if assembly failed)
        /// </summary>
        public ushort[] GeneratedWords { get; set; }

        /// <summary>
        /// Symbol table built during assembly
        /// </summary>
        public Dictionary<string, int> Symbols { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Starting address of the assembled code
        /// </summary>
        public int StartAddress { get; set; }

        /// <summary>
        /// Gets a formatted string of all errors
        /// </summary>
        public string GetErrorSummary()
        {
            if (Success)
                return "Assembly successful";

            var summary = $"Assembly failed with {Errors.Count} error(s):\n\n";
            foreach (var error in Errors)
            {
                summary += error.ToString() + "\n\n";
            }
            return summary;
        }

        /// <summary>
        /// Gets a formatted string of all warnings
        /// </summary>
        public string GetWarningSummary()
        {
            if (Warnings.Count == 0)
                return string.Empty;

            var summary = $"{Warnings.Count} warning(s):\n\n";
            foreach (var warning in Warnings)
            {
                summary += warning.ToString() + "\n\n";
            }
            return summary;
        }
    }
}
