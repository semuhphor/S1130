namespace S1130.Asm1130;

/// <summary>
/// Symbol table and cross-reference management
/// </summary>
public partial class Assembler
{
    private Symbol LookupSymbol(string name, bool define = false)
    {
        if (_symbols.TryGetValue(name, out var symbol))
        {
            return symbol;
        }
        
        if (define)
        {
            symbol = new Symbol { Name = name };
            _symbols[name] = symbol;
            return symbol;
        }
        
        throw new InvalidOperationException($"Symbol {name} not found");
    }
    
    private void AddCrossReference(Symbol symbol, bool isDefinition)
    {
        symbol.CrossReferences.Add(new CrossReference
        {
            FileName = CurrentFileName,
            LineNumber = LineNumber,
            IsDefinition = isDefinition
        });
    }
    
    private int GetSymbol(string name)
    {
        if (_symbols.TryGetValue(name, out var symbol))
        {
            AddCrossReference(symbol, false);
            
            if (symbol.State != SymbolState.Defined && Pass == 1)
                HasForwardReferences = true;
                
            return symbol.Value;
        }
        
        // Undefined symbol
        var newSymbol = LookupSymbol(name, true);
        newSymbol.State = SymbolState.Undefined;
        AddCrossReference(newSymbol, false);
        
        if (Pass == 1)
            HasForwardReferences = true;
            
        return 0;
    }
    
    public void SetSymbol(string name, int value, bool known, RelocationType relocation)
    {
        if (name.Length > 5)
        {
            Error($"Symbol '{name}' exceeds 5 characters");
            name = name[..5];
        }
        
        var symbol = LookupSymbol(name, true);
        
        if (symbol.State == SymbolState.Defined && Pass == 1)
        {
            Error($"Symbol '{name}' multiply defined");
            return;
        }
        
        symbol.Value = value;
        symbol.DefinedPass = Pass;
        symbol.State = known ? SymbolState.Defined : SymbolState.Provisional;
        symbol.Relocation = relocation;
        
        AddCrossReference(symbol, true);
    }
    
    private void LoadSymbolTable()
    {
        const string symbolFile = "SYMBOLS.SYS";
        
        if (!File.Exists(symbolFile))
        {
            Console.Error.WriteLine($"Warning: Symbol table {symbolFile} not found");
            return;
        }
        
        try
        {
            foreach (var line in File.ReadAllLines(symbolFile))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var name = parts[0];
                    if (int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int value))
                    {
                        SetSymbol(name, value, true, RelocationType.Absolute);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading symbol table: {ex.Message}");
        }
    }
    
    private void SaveSymbols()
    {
        const string symbolFile = "SYMBOLS.SYS";
        
        if (Relocate != RelocationType.Absolute)
        {
            Console.Error.WriteLine("Can't save symbol table unless ABS assembly");
            return;
        }
        
        if (File.Exists(symbolFile) && SavePrompt)
        {
            Console.Write($"Overwrite system symbol table {symbolFile}? ");
            var response = Console.ReadLine();
            if (response?.ToUpper() != "Y")
                return;
        }
        
        try
        {
            using var writer = new StreamWriter(symbolFile);
            foreach (var symbol in _symbols.Values.OrderBy(s => s.Name))
            {
                writer.WriteLine($"{symbol.Name,-5} {symbol.Value:X4}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving symbol table: {ex.Message}");
        }
    }
    
    private void WriteSymbolTable()
    {
        if (_listFile == null || _symbols.Count == 0)
            return;
            
        _listFile.WriteLine("\n=== SYMBOL TABLE " + new string('=', 58));
        
        int col = 0;
        foreach (var symbol in _symbols.Values.OrderBy(s => s.Name))
        {
            if (col >= 5)
            {
                _listFile.WriteLine();
                col = 0;
            }
            else if (col > 0)
            {
                _listFile.Write("     ");
            }
            
            _listFile.Write($"{symbol.Name,-6} ");
            if (symbol.State == SymbolState.Defined)
            {
                var relFlag = symbol.Relocation == RelocationType.Relative ? "R" : " ";
                _listFile.Write($"{symbol.Value:X4}{relFlag}");
            }
            else
            {
                _listFile.Write("UUUU ");
            }
            
            col++;
        }
        _listFile.WriteLine();
    }
    
    private void WriteCrossReference()
    {
        if (_listFile == null || _symbols.Count == 0)
            return;
            
        _listFile.WriteLine("\n=== CROSS REFERENCES " + new string('=', 56));
        _listFile.WriteLine("Name  Val   Defd  Referenced");
        
        foreach (var symbol in _symbols.Values.OrderBy(s => s.Name))
        {
            var relFlag = symbol.Relocation == RelocationType.Relative ? "R" : " ";
            _listFile.Write($"{symbol.Name,-5} {symbol.Value:X4}{relFlag} ");
            
            // Find definition
            var def = symbol.CrossReferences.FirstOrDefault(x => x.IsDefinition);
            if (def != null)
                _listFile.Write($"{def.LineNumber,4}");
            else
                _listFile.Write("----");
            
            // List references
            int refCount = 0;
            foreach (var xref in symbol.CrossReferences.Where(x => !x.IsDefinition))
            {
                if (refCount >= 12)
                {
                    _listFile.WriteLine();
                    _listFile.Write("               ");
                    refCount = 0;
                }
                _listFile.Write($" {xref.LineNumber,4}");
                refCount++;
            }
            _listFile.WriteLine();
        }
    }
}
