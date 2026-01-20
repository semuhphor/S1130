using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S1130.SystemObjects
{
    /// <summary>
    /// Represents a single line of assembler code with all its components.
    /// </summary>
    public class AssemblerLine
    {
        public string Label { get; set; } = "";
        public string Operation { get; set; } = "";
        public char FormatCode { get; set; } = ' ';  // blank, L, or I
        public char Tag { get; set; } = ' ';          // blank, 1, 2, or 3
        public string Operand { get; set; } = "";
        public string Remark { get; set; } = "";
        public bool IsComment { get; set; }
        public string CommentText { get; set; } = "";

        /// <summary>
        /// Format this line as IBM 1130 fixed-column format.
        /// </summary>
        public string ToIBM1130Format()
        {
            if (IsComment)
            {
                // Comment lines: 20-column prefix + comment
                return new string(' ', 20) + CommentText;
            }

            // Columns 1-20: Object code area (blank)
            var col1_20 = new string(' ', 20);
            
            // Columns 21-25: Label (5 chars)
            var col21_25 = Label.PadRight(5).Substring(0, 5);
            
            // Column 26: Blank
            var col26 = " ";
            
            // Columns 27-30: Operation (4 chars)
            var col27_30 = Operation.PadRight(4).Substring(0, 4);
            
            // Column 31: Blank
            var col31 = " ";
            
            // Column 32: Format code
            var col32 = FormatCode.ToString();
            
            // Column 33: Tag
            var col33 = Tag.ToString();
            
            // Column 34: Blank
            var col34 = " ";
            
            // Columns 35+: Operand and optional remark
            var col35plus = Operand;
            if (!string.IsNullOrEmpty(Remark))
            {
                col35plus += " " + Remark;
            }

            return $"{col1_20}{col21_25}{col26}{col27_30}{col31}{col32}{col33}{col34}{col35plus}";
        }

        /// <summary>
        /// Format this line as S1130 free-form format.
        /// </summary>
        public string ToS1130Format()
        {
            if (IsComment)
            {
                return CommentText;
            }

            var result = new StringBuilder();

            // Add label if present
            if (!string.IsNullOrEmpty(Label))
            {
                result.Append(Label);
                result.Append("  ");
            }
            else
            {
                result.Append("      ");  // Indent for no label
            }

            // Add operation
            result.Append(Operation);

            // Add format specifier if not a directive without format
            if (!string.IsNullOrEmpty(Operation) && !IsDirectiveWithoutFormat(Operation))
            {
                result.Append("  ");
                result.Append(BuildS1130FormatSpecifier());
            }

            // Add operand if present
            if (!string.IsNullOrEmpty(Operand))
            {
                result.Append(" ");
                result.Append(Operand);
            }

            return result.ToString();
        }

        private string BuildS1130FormatSpecifier()
        {
            var spec = new StringBuilder();

            // Column 32: Format code (blank, L, or I)
            if (FormatCode == 'I' || FormatCode == 'i')
            {
                spec.Append("I");
            }
            else if (FormatCode == 'L' || FormatCode == 'l')
            {
                spec.Append("L");
            }
            else if (Tag == ' ')
            {
                // Explicit short format with no index
                spec.Append(".");
            }

            // Column 33: Tag (1, 2, or 3)
            if (Tag >= '1' && Tag <= '3')
            {
                spec.Append(Tag);
            }

            return spec.ToString();
        }

        private static bool IsDirectiveWithoutFormat(string operation)
        {
            var upper = operation.ToUpper();
            var directivesNoFormat = new[] { "ORG", "DC", "EQU", "BSS", "BES", "END" };
            var instructionsNoFormat = new[] { "WAIT", "SLA", "SLT", "SLC", "SLCA", "SRA", "SRT", "LDS" };
            
            return directivesNoFormat.Contains(upper) || instructionsNoFormat.Contains(upper);
        }

        /// <summary>
        /// Parse a line from S1130 free-form format.
        /// </summary>
        public static AssemblerLine ParseS1130(string line)
        {
            var result = new AssemblerLine();
            
            if (string.IsNullOrWhiteSpace(line))
                return result;

            var trimmedLine = line.TrimEnd();
            
            // Check for comment line
            if (trimmedLine.TrimStart().StartsWith("*"))
            {
                result.IsComment = true;
                result.CommentText = trimmedLine.TrimStart();
                return result;
            }

            // Remove inline comments (//)
            var commentIndex = trimmedLine.IndexOf("//");
            var codeOnly = commentIndex >= 0 ? trimmedLine.Substring(0, commentIndex).TrimEnd() : trimmedLine;
            
            if (string.IsNullOrWhiteSpace(codeOnly))
            {
                result.IsComment = true;
                result.CommentText = trimmedLine.TrimStart();
                return result;
            }

            // Parse tokens
            var tokens = Regex.Split(codeOnly, @"\s+").Where(t => !string.IsNullOrEmpty(t)).ToArray();
            if (tokens.Length == 0)
                return result;

            int tokenIndex = 0;

            // Check if first token is a label
            if (!IsOperationOrDirective(tokens[0]))
            {
                result.Label = tokens[0].ToUpper();
                tokenIndex++;
            }

            // Get operation
            if (tokenIndex < tokens.Length)
            {
                result.Operation = tokens[tokenIndex++].ToUpper();
            }

            // Parse format/tag and operand
            if (tokenIndex < tokens.Length && !IsDirectiveWithoutFormat(result.Operation))
            {
                var formatToken = tokens[tokenIndex];
                
                // Check if this is a format specifier
                if (formatToken == "." || formatToken == "L" || 
                    formatToken == "1" || formatToken == "2" || formatToken == "3" ||
                    formatToken.StartsWith("L") || formatToken.StartsWith("I"))
                {
                    char formatCode, tag;
                    ParseFormatSpecifier(formatToken, out formatCode, out tag);
                    result.FormatCode = formatCode;
                    result.Tag = tag;
                    tokenIndex++;
                }
                
                // Get operand
                if (tokenIndex < tokens.Length)
                {
                    result.Operand = string.Join(" ", tokens.Skip(tokenIndex));
                }
            }
            else if (tokenIndex < tokens.Length)
            {
                // Directive - take rest as operand
                result.Operand = string.Join(" ", tokens.Skip(tokenIndex));
            }

            return result;
        }

        /// <summary>
        /// Parse a line from IBM 1130 fixed-column format.
        /// </summary>
        public static AssemblerLine ParseIBM1130(string line)
        {
            var result = new AssemblerLine();
            
            if (string.IsNullOrWhiteSpace(line))
                return result;

            // Check for comment line (column 21 = *)
            if (line.Length > 20 && line[20] == '*')
            {
                result.IsComment = true;
                result.CommentText = line.Substring(20).TrimEnd();
                return result;
            }

            // Pad line for column extraction
            var paddedLine = line.PadRight(34);

            // Extract columns (0-based indexing)
            result.Label = paddedLine.Substring(20, 5).Trim();        // Columns 21-25
            result.Operation = paddedLine.Substring(26, 4).Trim();    // Columns 27-30
            result.FormatCode = paddedLine[31];                       // Column 32
            result.Tag = paddedLine[32];                              // Column 33
            
            // Columns 35+: Operand and remark
            var operandAndRemark = line.Length > 34 ? line.Substring(34).TrimEnd() : "";
            result.Operand = SplitOperandFromRemark(operandAndRemark);

            return result;
        }

        private static void ParseFormatSpecifier(string spec, out char formatCode, out char tag)
        {
            formatCode = ' ';
            tag = ' ';

            spec = spec.ToUpper();

            // Check for indirect (I) - goes in column 32
            if (spec.StartsWith("I"))
            {
                formatCode = 'I';
                spec = spec.Substring(1);
                
                if (spec.Length > 0 && char.IsDigit(spec[0]))
                {
                    tag = spec[0];
                }
                return;
            }

            // Check for long format (L) - goes in column 32
            if (spec.StartsWith("L"))
            {
                formatCode = 'L';
                spec = spec.Substring(1);
                
                if (spec.Length > 0 && char.IsDigit(spec[0]))
                {
                    tag = spec[0];
                }
                return;
            }
            
            // Check for explicit short format (.)
            if (spec == ".")
            {
                formatCode = ' ';
                return;
            }

            // Just an index register (1, 2, or 3) - short format
            if (spec.Length > 0 && char.IsDigit(spec[0]))
            {
                formatCode = ' ';
                tag = spec[0];
            }
        }

        private static string SplitOperandFromRemark(string operandAndRemark)
        {
            // Split at first unquoted space
            bool inQuotes = false;
            for (int i = 0; i < operandAndRemark.Length; i++)
            {
                if (operandAndRemark[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (operandAndRemark[i] == ' ' && !inQuotes)
                {
                    return operandAndRemark.Substring(0, i);
                }
            }
            return operandAndRemark;
        }

        private static bool IsOperationOrDirective(string token)
        {
            var upper = token.ToUpper();
            
            var instructions = new[] {
                "LD", "LDD", "LDS", "LDX", "STO", "STD", "STS", "STX",
                "A", "AD", "S", "SD", "M", "D",
                "AND", "OR", "EOR",
                "BSC", "BOSC", "BSI", "MDX",
                "SLA", "SLT", "SLC", "SLCA", "SRA", "SRT",
                "WAIT", "XIO", "SVC"
            };
            
            var directives = new[] {
                "ORG", "DC", "EQU", "BSS", "BES", "END"
            };

            return instructions.Contains(upper) || directives.Contains(upper);
        }
    }

    /// <summary>
    /// Converts between S1130 free-form assembler syntax and IBM 1130 fixed-column card format.
    /// Enables writing code in modern syntax and testing on real IBM 1130 systems.
    /// </summary>
    public class AssemblerConverter
    {
        /// <summary>
        /// Convert S1130 free-form syntax to IBM 1130 fixed-column card format.
        /// </summary>
        public static string ToIBM1130Format(string s1130Source)
        {
            var lines = s1130Source.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                var parsedLine = AssemblerLine.ParseS1130(line);
                result.AppendLine(parsedLine.ToIBM1130Format());
            }

            return result.ToString();
        }

        /// <summary>
        /// Convert IBM 1130 fixed-column card format to S1130 free-form syntax.
        /// </summary>
        public static string ToS1130Format(string ibmSource)
        {
            var lines = ibmSource.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                var parsedLine = AssemblerLine.ParseIBM1130(line);
                result.AppendLine(parsedLine.ToS1130Format());
            }

            return result.ToString();
        }
    }
}
