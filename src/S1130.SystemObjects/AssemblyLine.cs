using System;

namespace S1130.SystemObjects
{
    /// <summary>
    /// Represents a single line of assembly code
    /// </summary>
    public class AssemblyLine
    {
        /// <summary>
        /// The raw source line
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Label on the line (if any)
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Operation/instruction on the line
        /// </summary>
        public string Operation { get; private set; }

        /// <summary>
        /// Operand for the operation (if any)
        /// </summary>
        public string Operand { get; private set; }

        /// <summary>
        /// Comment on the line (if any)
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// Whether this line is a comment-only line
        /// </summary>
        public bool IsComment { get; private set; }

        /// <summary>
        /// Parse a line of assembly code
        /// </summary>
        /// <param name="source">Source line to parse</param>
        public AssemblyLine(string source)
        {
            Source = source ?? "";
            ParseLine();
        }

        private void ParseLine()
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                // Empty line
                return;
            }

            var line = Source;

            // Handle comment lines (asterisk in column 1 only)
            if (line.Length > 0 && line[0] == '*')
            {
                IsComment = true;
                Comment = line;
                return;
            }
            
            // Handle inline comments (// anywhere in line)
            int commentPos = line.IndexOf("//");
            if (commentPos >= 0)
            {
                Comment = line.Substring(commentPos);
                line = line.Substring(0, commentPos);
            }

            // IBM 1130 format with new syntax:
            // - Label starts in column 1 (no leading whitespace)
            // - Operation follows label or starts after whitespace
            // - Operand follows operation (everything after operation until end of line)
            // - Comments: * in column 1 for full line, // for inline (already stripped above)
            // - * in operand means "current address" 
            
            bool hasLabel = line.Length > 0 && !char.IsWhiteSpace(line[0]);
            
            // Split the line into tokens
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return;
            
            int partIndex = 0;
            
            // Get label if present
            if (hasLabel && partIndex < parts.Length)
            {
                Label = parts[partIndex++];
            }

            // Get operation
            if (partIndex < parts.Length)
            {
                Operation = parts[partIndex++];
            }

            // Get operand - everything remaining is the operand
            if (partIndex < parts.Length)
            {
                Operand = string.Join(" ", parts, partIndex, parts.Length - partIndex).Trim();
            }
        }
    }
}