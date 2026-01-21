namespace S1130.Asm1130;

/// <summary>
/// Output file writing - Load format and Binary format
/// </summary>
public partial class Assembler
{
    private void SetWord(int position, int word, RelocationType relocation)
    {
        if (_listFile == null || !_listOn)
            return;
            
        // Format word into listing line at specified position
        var wordStr = $"{word:X4}";
        var relChar = relocation == RelocationType.Relative ? 'R' :
                      relocation == RelocationType.Libf ? 'L' :
                      relocation == RelocationType.Call ? '$' : ' ';
        
        int listPos = 6 + position * 5; // After line number
        if (listPos + 4 < _listLine.Length)
        {
            _listLine = _listLine[..listPos] + wordStr + _listLine[(listPos + 4)..];
            if (listPos + 4 < _listLine.Length)
                _listLine = _listLine[..(listPos + 4)] + relChar + _listLine[(listPos + 5)..];
        }
        else
        {
            _listLine += new string(' ', listPos - _listLine.Length) + wordStr + relChar;
        }
    }
    
    private void WriteWord(int word, RelocationType relocation)
    {
        // Display in listing
        if (_wordsOutput == 0)
        {
            SetWord(0, Origin + ListOffset, RelocationType.Absolute);
        }
        else if (_wordsOutput >= 4)
        {
            ListOut(true);
            _wordsOutput = 0;
        }
        
        _wordsOutput++;
        SetWord(_wordsOutput, word, relocation);
        
        // Write to output file
        StoreWord(word, relocation);
    }
    
    private void StoreWord(int word, RelocationType relocation)
    {
        if (Pass != 2 || _outputFile == null)
        {
            Origin++;
            return;
        }
        
        switch (OutputMode)
        {
            case OutputMode.Load:
                var relChar = relocation == RelocationType.Absolute ? "" :
                             relocation == RelocationType.Relative ? "R" :
                             relocation == RelocationType.Libf ? "L" :
                             relocation == RelocationType.Call ? "$" : "?";
                _outputFile.WriteLine($" {word:X4}{relChar}");
                break;
                
            case OutputMode.Binary:
                // Binary format not yet implemented
                // Would write to card buffer
                break;
        }
        
        if (relocation != RelocationType.Libf)
            Origin++;
            
        Assembled = true;
    }
    
    private void SetOrigin(int newOrigin)
    {
        if (Pass == 2 && _outputFile != null)
        {
            SetWord(0, newOrigin + ListOffset, RelocationType.Absolute);
            
            if (OutputMode == OutputMode.Load)
            {
                var relChar = Relocate == RelocationType.Relative ? "R" : "";
                _outputFile.WriteLine($"@{newOrigin:X4}{relChar}");
            }
        }
        
        Origin = newOrigin;
    }
    
    private void OriginEven()
    {
        if ((Origin & 1) != 0)
            SetOrigin(Origin + 1);
    }
    
    private void OutputLiterals(bool force)
    {
        if (_literals.Count == 0)
            return;
            
        // Output accumulated literals
        foreach (var lit in _literals)
        {
            if (lit.RequiresEvenAddress)
                OriginEven();
                
            WriteWord(lit.Value, RelocationType.Absolute);
        }
        
        _literals.Clear();
    }
}
