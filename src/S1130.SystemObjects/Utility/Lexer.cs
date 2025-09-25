using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace S1130.SystemObjects.Utility
{
    public enum TokenType { Number, Identifier, Operator, Punctuation, Unknown }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents a lexer that tokenizes a given code string.
    /// </summary>
    public class Lexer
    {
        private static readonly Regex NumberRegex = new Regex(@"^\d+");
        private static readonly Regex IdentifierRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*");
        private static readonly Regex OperatorRegex = new Regex(@"^(\+|-|\*|/|==|!=|<|>|<=|>=)");
        private static readonly Regex PunctuationRegex = new Regex(@"^(;|,|\(|\)|\{|\})");

        public List<Token> Tokenize(string code)
        {
            var tokens = new List<Token>();
            while (!string.IsNullOrEmpty(code))
            {
                code = code.TrimStart();
                if (TryMatch(NumberRegex, ref code, out var number))
                {
                    tokens.Add(new Token { Type = TokenType.Number, Value = number });
                }
                else if (TryMatch(IdentifierRegex, ref code, out var identifier))
                {
                    tokens.Add(new Token { Type = TokenType.Identifier, Value = identifier });
                }
                else if (TryMatch(OperatorRegex, ref code, out var op))
                {
                    tokens.Add(new Token { Type = TokenType.Operator, Value = op });
                }
                else if (TryMatch(PunctuationRegex, ref code, out var punctuation))
                {
                    tokens.Add(new Token { Type = TokenType.Punctuation, Value = punctuation });
                }
                else
                {
                    tokens.Add(new Token { Type = TokenType.Unknown, Value = code[0].ToString() });
                    code = code.Substring(1);
                }
            }
            return tokens;
        }

        private static bool TryMatch(Regex regex, ref string code, out string value)
        {
            var match = regex.Match(code);
            if (match.Success)
            {
                value = match.Value;
                code = code.Substring(value.Length);
                return true;
            }
            value = null;
            return false;
        }
    }
}