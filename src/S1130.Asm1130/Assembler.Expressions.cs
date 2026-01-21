namespace S1130.Asm1130;

/// <summary>
/// Expression evaluator - handles arithmetic expressions with forward references
/// </summary>
public partial class Assembler
{
    private Expression GetExpression(string text, bool allowUndefined = false)
    {
        text = text.Trim();
        
        if (string.IsNullOrEmpty(text))
        {
            Error("Empty expression");
            return new Expression { Value = 0, Relocation = RelocationType.Absolute };
        }
        
        try
        {
            return EvaluateExpression(text, allowUndefined);
        }
        catch (Exception ex)
        {
            Error($"Expression error: {ex.Message}");
            return new Expression { Value = 0, Relocation = RelocationType.Absolute };
        }
    }
    
    private Expression EvaluateExpression(string expr, bool allowUndefined)
    {
        expr = expr.Trim();
        
        // Handle special character * (current address)
        if (expr == "*")
        {
            return new Expression 
            { 
                Value = Origin + OriginAdvanced, 
                Relocation = Relocate 
            };
        }
        
        // Try parsing as hex literal (starts with /)
        if (expr.StartsWith('/'))
        {
            if (int.TryParse(expr[1..], System.Globalization.NumberStyles.HexNumber, null, out int hexVal))
            {
                return new Expression { Value = hexVal, Relocation = RelocationType.Absolute };
            }
        }
        
        // Try parsing as decimal literal
        if (int.TryParse(expr, out int decVal))
        {
            return new Expression { Value = decVal, Relocation = RelocationType.Absolute };
        }
        
        // Try parsing as symbol
        if (IsValidSymbol(expr))
        {
            if (_symbols.TryGetValue(expr, out var symbol))
            {
                AddCrossReference(symbol, false);
                
                if (symbol.State != SymbolState.Defined)
                {
                    if (Pass == 1)
                        HasForwardReferences = true;
                        
                    if (!allowUndefined && Pass == 2)
                        Error($"Undefined symbol: {expr}");
                }
                
                return new Expression { Value = symbol.Value, Relocation = symbol.Relocation };
            }
            else
            {
                // Create undefined symbol
                var newSym = LookupSymbol(expr, true);
                newSym.State = SymbolState.Undefined;
                AddCrossReference(newSym, false);
                
                if (Pass == 1)
                    HasForwardReferences = true;
                    
                if (!allowUndefined && Pass == 2)
                    Error($"Undefined symbol: {expr}");
                    
                return new Expression { Value = 0, Relocation = RelocationType.Absolute };
            }
        }
        
        // Try parsing as expression with operators
        return EvaluateComplexExpression(expr, allowUndefined);
    }
    
    private Expression EvaluateComplexExpression(string expr, bool allowUndefined)
    {
        // Simple expression parser: handles +, -, *, /
        // Priority: * / before + -
        
        // Look for + or - at lowest precedence
        int parenDepth = 0;
        for (int i = expr.Length - 1; i >= 0; i--)
        {
            char c = expr[i];
            if (c == ')') parenDepth++;
            else if (c == '(') parenDepth--;
            else if (parenDepth == 0 && (c == '+' || c == '-'))
            {
                var left = EvaluateExpression(expr[..i], allowUndefined);
                var right = EvaluateExpression(expr[(i + 1)..], allowUndefined);
                
                int result = c == '+' ? left.Value + right.Value : left.Value - right.Value;
                
                // Determine relocation
                var reloc = RelocationType.Absolute;
                if (c == '+')
                {
                    if (left.Relocation != RelocationType.Absolute && right.Relocation != RelocationType.Absolute)
                    {
                        Error("Cannot add two relocatable values");
                    }
                    else if (left.Relocation != RelocationType.Absolute)
                        reloc = left.Relocation;
                    else if (right.Relocation != RelocationType.Absolute)
                        reloc = right.Relocation;
                }
                else // subtraction
                {
                    if (right.Relocation != RelocationType.Absolute)
                    {
                        if (left.Relocation == right.Relocation)
                            reloc = RelocationType.Absolute; // Rel - Rel = Abs
                        else
                            Error("Invalid relocation in subtraction");
                    }
                    else if (left.Relocation != RelocationType.Absolute)
                        reloc = left.Relocation;
                }
                
                return new Expression { Value = result, Relocation = reloc };
            }
        }
        
        // Look for * or /
        parenDepth = 0;
        for (int i = expr.Length - 1; i >= 0; i--)
        {
            char c = expr[i];
            if (c == ')') parenDepth++;
            else if (c == '(') parenDepth--;
            else if (parenDepth == 0 && (c == '*' || c == '/'))
            {
                var left = EvaluateExpression(expr[..i], allowUndefined);
                var right = EvaluateExpression(expr[(i + 1)..], allowUndefined);
                
                if (left.Relocation != RelocationType.Absolute || right.Relocation != RelocationType.Absolute)
                    Error("Cannot multiply/divide relocatable values");
                
                int result = c == '*' ? left.Value * right.Value : left.Value / right.Value;
                return new Expression { Value = result, Relocation = RelocationType.Absolute };
            }
        }
        
        // Handle parentheses
        if (expr.StartsWith('(') && expr.EndsWith(')'))
        {
            return EvaluateExpression(expr[1..^1], allowUndefined);
        }
        
        Error($"Cannot parse expression: {expr}");
        return new Expression { Value = 0, Relocation = RelocationType.Absolute };
    }
    
    private bool IsValidSymbol(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 6) // Allow 6 chars for @ or $ prefix + 5 chars
            return false;
            
        // Allow @, $, or # as first character (special symbol prefixes)
        if (name[0] == '@' || name[0] == '$' || name[0] == '#')
        {
            if (name.Length == 1)
                return false;
            return IsValidSymbol(name[1..]); // Recursively check the rest
        }
            
        if (!char.IsLetter(name[0]))
            return false;
            
        return name.All(c => char.IsLetterOrDigit(c));
    }
}
