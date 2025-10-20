using System;
using System.Collections.Generic;
using System.Text;

namespace S1130.SystemObjects
{
    /// <summary>
    /// Maintains state during assembly process
    /// </summary>
    public class AssemblyContext
    {
        /// <summary>
        /// List of assembly errors found during processing
        /// </summary>
        public List<AssemblyError> Errors { get; } = new List<AssemblyError>();

        /// <summary>
        /// The location counter for the next instruction
        /// </summary>
        public int LocationCounter { get; set; }

        /// <summary>
        /// Line by line listing of the assembly
        /// </summary>
        public List<string> Listing { get; } = new List<string>();

        /// <summary>
        /// Raw source lines being processed
        /// </summary>
        public string[] SourceLines { get; set; }

        /// <summary>
        /// Whether we have seen an ORG directive
        /// </summary>
        public bool HasOrigin { get; set; }

        /// <summary>
        /// Symbol table mapping labels to addresses
        /// </summary>
        public Dictionary<string, ushort> Symbols { get; } = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Constructor initializes with source code
        /// </summary>
        /// <param name="sourceCode">Assembly source code</param>
        public AssemblyContext(string sourceCode)
        {
            // Split on any common line ending
            SourceLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Add an error to the context
        /// </summary>
        /// <param name="lineNumber">1-based line number where error occurred</param>
        /// <param name="message">Error message</param>
        public void AddError(int lineNumber, string message)
        {
            var error = new AssemblyError 
            { 
                LineNumber = lineNumber, 
                Message = message 
            };
            Errors.Add(error);
        }

        /// <summary>
        /// Add a line to the assembly listing
        /// </summary>
        /// <param name="line">The line to add</param>
        public void AddListingLine(string line)
        {
            Listing.Add(line);
        }
    }
}