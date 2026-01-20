using System;
using System.Collections.Generic;
using System.Globalization;

namespace S1130.SystemObjects.Assembler
{
    /// <summary>
    /// Evaluates arithmetic expressions in assembler operands.
    /// Supports: decimal numbers, hex numbers (/prefix), symbols, *, +, - operators
    /// </summary>
    public class ExpressionEvaluator
    {
        private readonly Dictionary<string, int> _symbolTable;
        private readonly int _currentAddress;

        public ExpressionEvaluator(Dictionary<string, int> symbolTable, int currentAddress)
        {
            _symbolTable = symbolTable ?? new Dictionary<string, int>();
            _currentAddress = currentAddress;
        }

        /// <summary>
        /// Evaluates an expression string to an integer value.
        /// Supports: LABEL+5, /1000-10, *+20, 100, etc.
        /// </summary>
        public bool Evaluate(string expression, out int result, out string error)
        {
            result = 0;
            error = null;

            if (string.IsNullOrWhiteSpace(expression))
            {
                error = "Empty expression";
                return false;
            }

            try
            {
                var tokens = Tokenize(expression);
                return EvaluateTokens(tokens, out result, out error);
            }
            catch (Exception ex)
            {
                error = $"Expression evaluation failed: {ex.Message}";
                return false;
            }
        }

        private bool EvaluateTokens(List<Token> tokens, out int result, out string error)
        {
            result = 0;
            error = null;
            char currentOp = '+'; // Start with addition for first value

            foreach (var token in tokens)
            {
                if (token.IsOperator)
                {
                    currentOp = token.Operator;
                    continue;
                }

                int value;
                if (!EvaluateToken(token.Value, out value, out error))
                    return false;

                // Apply operator
                if (currentOp == '+')
                    result += value;
                else if (currentOp == '-')
                    result -= value;
            }

            return true;
        }

        private bool EvaluateToken(string token, out int value, out string error)
        {
            value = 0;
            error = null;

            // Handle current location marker
            if (token == "*")
            {
                value = _currentAddress;
                return true;
            }

            // Handle hexadecimal (/prefix)
            if (token.StartsWith("/"))
            {
                var hexPart = token.Substring(1);
                if (int.TryParse(hexPart, NumberStyles.HexNumber, null, out value))
                    return true;

                error = $"Invalid hexadecimal value: {token}";
                return false;
            }

            // Handle decimal number (including negative)
            if (char.IsDigit(token[0]) || (token.Length > 1 && token[0] == '-' && char.IsDigit(token[1])))
            {
                if (int.TryParse(token, out value))
                    return true;

                error = $"Invalid decimal value: {token}";
                return false;
            }

            // Must be a symbol
            if (_symbolTable.TryGetValue(token, out value))
                return true;

            error = $"Undefined symbol: {token}";
            return false;
        }

        private List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            var currentToken = string.Empty;
            char? pendingOperator = null;

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (c == '+' || c == '-')
                {
                    // Check if this is a negative sign at the start of a number
                    if (c == '-' && string.IsNullOrEmpty(currentToken) && 
                        i + 1 < expression.Length && char.IsDigit(expression[i + 1]))
                    {
                        // This is a negative number, not an operator
                        currentToken += c;
                        continue;
                    }

                    // This is an operator
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        if (pendingOperator.HasValue)
                            tokens.Add(new Token { IsOperator = true, Operator = pendingOperator.Value });
                        tokens.Add(new Token { IsOperator = false, Value = currentToken });
                        currentToken = string.Empty;
                    }
                    pendingOperator = c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    // Ignore whitespace
                    continue;
                }
                else
                {
                    currentToken += c;
                }
            }

            // Add final token
            if (!string.IsNullOrEmpty(currentToken))
            {
                if (pendingOperator.HasValue)
                    tokens.Add(new Token { IsOperator = true, Operator = pendingOperator.Value });
                tokens.Add(new Token { IsOperator = false, Value = currentToken });
            }

            return tokens;
        }

        private class Token
        {
            public bool IsOperator { get; set; }
            public char Operator { get; set; }
            public string Value { get; set; }
        }
    }
}
