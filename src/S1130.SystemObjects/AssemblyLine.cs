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

            // Handle comment lines
            if (line.TrimStart().StartsWith("*"))
            {
                IsComment = true;
                Comment = line;
                return;
            }

            // IBM 1130 format: if line starts with whitespace, no label
            // Label[5] Operation[5] Operand[~50] Comment[*...]
            bool hasLabel = !char.IsWhiteSpace(line[0]);
            
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            int partIndex = 0;
            
            // Get label if present (line doesn't start with whitespace)
            if (hasLabel && partIndex < parts.Length && !parts[partIndex].StartsWith("*"))
            {
                Label = parts[partIndex++];
            }

            // Get operation
            if (partIndex < parts.Length && !parts[partIndex].StartsWith("*"))
            {
                Operation = parts[partIndex++];
            }

            if (partIndex < parts.Length)
            {
                // Everything up to a comment becomes the operand
                var operand = new System.Text.StringBuilder();
                while (partIndex < parts.Length && !parts[partIndex].StartsWith("*"))
                {
                    if (operand.Length > 0)
                        operand.Append(' ');
                    operand.Append(parts[partIndex++]);
                }
                Operand = operand.ToString();

                // Rest is comment if any
                if (partIndex < parts.Length)
                {
                    Comment = string.Join(" ", parts, partIndex, parts.Length - partIndex);
                }
            }
        }
    }
}