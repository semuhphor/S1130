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

            // Handle comment lines (asterisk at start when trimmed)
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("*"))
            {
                IsComment = true;
                Comment = line;
                return;
            }

            // IBM 1130 format:
            // - Label starts in column 1 (no leading whitespace)
            // - Operation follows label or starts after whitespace
            // - Operand follows operation
            // - Comment can start with * or just be text after extra whitespace
            // - Special: * in operand means "current address" (not a comment!)
            
            bool hasLabel = line.Length > 0 && !char.IsWhiteSpace(line[0]);
            
            // Split the line into tokens, but preserve position info
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

            // Get operand - this is tricky because:
            // 1. Operand can be complex: "L VAL", "E 10", "*-*", "*+5", etc.
            // 2. * in operand is NOT a comment marker
            // 3. Only a word starting with * AFTER the operand is a comment
            // 
            // Strategy: Take tokens until we hit a likely comment
            // A likely comment is: a token that's ONLY "*" followed by more text
            // But "*-*" or "*+5" are operands, not comments
            
            if (partIndex < parts.Length)
            {
                var operand = new System.Text.StringBuilder();
                int operandStartIndex = partIndex;
                
                // Take all remaining parts as operand initially
                // We'll separate comment if we find a clear * comment marker
                while (partIndex < parts.Length)
                {
                    var part = parts[partIndex];
                    
                    // If this part is JUST "*" followed by more parts, those parts are comments
                    // But if it's "*-something" or "*+something", it's an operand
                    if (part == "*" && partIndex + 1 < parts.Length)
                    {
                        // Next part will tell us if this is a comment or operand
                        var nextPart = parts[partIndex + 1];
                        if (!nextPart.StartsWith("-") && !nextPart.StartsWith("+"))
                        {
                            // This looks like start of comment: "* comment text"
                            break;
                        }
                    }
                    else if (part.StartsWith("*") && part.Length > 1 && operand.Length > 0)
                    {
                        // Part starts with * and we already have an operand
                        // Check if it looks like an expression continuation or a comment
                        char secondChar = part[1];
                        if (secondChar != '-' && secondChar != '+' && secondChar != '*')
                        {
                            // Doesn't look like an expression, probably a comment like "* this is a comment"
                            break;
                        }
                    }
                    
                    if (operand.Length > 0)
                        operand.Append(' ');
                    operand.Append(part);
                    partIndex++;
                }
                
                Operand = operand.ToString().Trim();

                // Rest is comment if any
                if (partIndex < parts.Length)
                {
                    Comment = string.Join(" ", parts, partIndex, parts.Length - partIndex);
                }
            }
        }
    }
}