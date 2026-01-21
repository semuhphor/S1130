namespace S1130.Asm1130;

/// <summary>
/// File processing and line parsing
/// </summary>
public partial class Assembler
{
    private void ProcessFile(string filename)
    {
        // Add .asm extension if none specified
        var actualFile = filename;
        if (!Path.HasExtension(filename))
            actualFile = Path.ChangeExtension(filename, ".asm");
            
        CurrentFileName = actualFile;
        LineNumber = 0;
        Ended = false;
        
        if (Verbose)
            Console.Error.WriteLine($"--- Starting file {actualFile} pass {Pass}");
            
        try
        {
            _inputFile = new StreamReader(actualFile);
            
            if (_listFile != null)
            {
                var banner = "=== FILE " + new string('=', 72);
                var nameStart = 9;
                var fileDisplay = actualFile.ToCharArray();
                for (int i = 0; i < fileDisplay.Length && nameStart + i < banner.Length; i++)
                    banner = banner[..nameStart] + fileDisplay[i] + banner[(nameStart + i + 1)..];
                _listFile.WriteLine(banner);
                _listOn = true;
            }
            
            string? line;
            while ((line = _inputFile.ReadLine()) != null && !Ended)
            {
                LineNumber++;
                PrepLine(line);
                ParseLine(line);
                ListOut(false);
            }
            
            _inputFile.Close();
            
            // Output any pending literals at end of file
            if (_literals.Count > 0)
            {
                OutputLiterals(true);
                ListOut(false);
            }
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"Error: Cannot open file {actualFile}");
            ErrorCount++;
        }
    }
    
    private void PrepLine(string line)
    {
        // Convert to uppercase
        line = line.ToUpper();
        _wordsOutput = 0;
        _lineError = false;
        
        // Construct listing line
        if (_listFile != null && _listOn)
        {
            if (TabFormat)
            {
                _listLine = $"{LineNumber,5} {DetabLine(line)}";
            }
            else
            {
                // Extract command area (column 21 onward) - be safe with bounds
                var commandArea = line.Length > 20 ? line[20..] : "";
                _listLine = $"{LineNumber,5} {commandArea}";
                
                // Stuff left margin (label area) into left side
                if (line.Length > 0)
                {
                    var leftMargin = line.Length >= 20 ? line[..20] : line;
                    var paddedMargin = leftMargin.Length < 20 ? leftMargin + new string(' ', 20 - leftMargin.Length) : leftMargin;
                    if (_listLine.Length >= 25)
                        _listLine = _listLine[..20] + paddedMargin + _listLine[20..];
                }
            }
        }
    }
    
    private string DetabLine(string line)
    {
        var result = new System.Text.StringBuilder();
        int col = 0;
        foreach (char c in line)
        {
            if (c == '\t')
            {
                do
                {
                    result.Append(' ');
                    col++;
                } while ((col & 7) != 0);
            }
            else
            {
                result.Append(c);
                col++;
            }
        }
        return result.ToString();
    }
    
    private void ParseLine(string line)
    {
        line = line.ToUpper();
        
        // Check for job control card
        if (line.StartsWith("//"))
            return;
            
        // Check for comment or control card
        if (line.StartsWith("*"))
        {
            // Handle SBRK cards if needed
            if (line.StartsWith("*SBRK", StringComparison.OrdinalIgnoreCase))
            {
                // Handle SBRK directive
            }
            return;
        }
        
        string label = "", mnem = "", mods = "", arg = "";
        
        // Determine parsing mode: tab-delimited or column-format
        if (TabFormat || line.Contains('\t'))
        {
            TabFormat = true;
            
            // Skip leading whitespace
            line = line.TrimStart();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('*'))
                return;
                
            var parts = line.Split('\t');
            if (parts.Length > 0) label = parts[0].Trim();
            if (parts.Length > 1) mnem = parts[1].Trim();
            if (parts.Length > 2) mods = parts[2].Trim();
            if (parts.Length > 3)
            {
                // Join remaining parts in case there are tabs in arguments
                arg = string.Join(" ", parts.Skip(3)).Trim();
            }
        }
        else
        {
            // Column format
            if (line.Length > 20 && line[20] == '*')
                return; // Comment line
                
            // Extract fields by column position
            if (line.Length >= 25)
                label = ExtractToken(line, 20, 25).Trim();
            if (line.Length >= 30)
                mnem = ExtractToken(line, 26, 30).Trim();
            if (line.Length >= 33)
                mods = ExtractToken(line, 31, 33).Trim();
            if (line.Length >= 34)
            {
                // Extract argument field - stop at first whitespace (rest is comment)
                var argField = line.Length > 72 ? line[34..72] : line[34..];
                var spaceIndex = argField.IndexOf(' ');
                arg = spaceIndex >= 0 ? argField[..spaceIndex].Trim() : argField.Trim();
            }
        }
        
        // Display origin if label present
        if (!string.IsNullOrEmpty(label))
            SetWord(0, Origin + ListOffset, RelocationType.Absolute);
            
        // If no mnemonic, just define label
        if (string.IsNullOrEmpty(mnem))
        {
            if (!string.IsNullOrEmpty(label))
                SetSymbol(label, Origin, true, Relocate);
            return;
        }
        
        // Look up opcode
        var opcode = LookupOpcode(mnem);
        if (opcode == null)
        {
            if (!string.IsNullOrEmpty(label))
                SetSymbol(label, Origin, true, Relocate);
            Error($"Unknown opcode '{mnem}'");
            return;
        }
        
        // Validate and apply modifiers
        if (opcode.ModifiersAllowed != "\xFF")
        {
            // Special case: LDX/STX allow L1, L2, L3 as a single combined modifier
            var validMods = "";
            if (mods.StartsWith("L") && mods.Length > 1 && char.IsDigit(mods[1]))
            {
                // "L1" = L + 1, "L2" = L + 2, etc.
                validMods = mods; // Keep as-is
            }
            else
            {
                validMods = new string(mods.Where(c => opcode.ModifiersAllowed.Contains(c)).ToArray());
                if (validMods != mods)
                    Warning($"Invalid modifiers removed from {mnem}");
            }
            mods = validMods;
        }
        
        mods += opcode.ModifiersImplied;
        
        // Indirect implies long
        if (mods.Contains('I'))
            mods += "L";
            
        // Calculate origin advancement
        OriginAdvanced = mods.Contains('L') ? 2 : 1;
        
        // Execute opcode handler
        opcode.Handler(opcode, label, mods, arg);
        
        // Warn about 1800-specific instructions
        if ((opcode.Flags & OpcodeFlags.Is1800) != 0 && !Enable1800)
            Warning($"{opcode.Mnemonic} is IBM 1800-specific; use the -8 command line option");
    }
    
    private string ExtractToken(string line, int start, int end)
    {
        if (start >= line.Length)
            return "";
        // Ensure we don't go past the string length
        int actualEnd = Math.Min(end, line.Length);
        int len = actualEnd - start;
        if (len <= 0)
            return "";
        return line.Substring(start, len);
    }
    
    private Opcode? LookupOpcode(string mnemonic)
    {
        // Binary search in sorted opcode table
        int left = 0, right = _opcodes.Count - 1;
        
        while (left <= right)
        {
            int mid = (left + right) / 2;
            int cmp = string.Compare(_opcodes[mid].Mnemonic, mnemonic, StringComparison.Ordinal);
            
            if (cmp == 0)
                return _opcodes[mid];
            else if (cmp < 0)
                left = mid + 1;
            else
                right = mid - 1;
        }
        
        return null;
    }
    
    private void Error(string message)
    {
        if (Pass != 2)
            return;
            
        Console.Error.WriteLine($"E: {CurrentFileName} ({LineNumber}): {message}");
        
        if (_listFile != null && _listOn)
        {
            ListOut(false);
            _lineError = true;
            _listFile.WriteLine($"**** Error: {message}");
        }
        
        ErrorCount++;
    }
    
    private void Warning(string message)
    {
        if (Pass != 2)
            return;
            
        Console.Error.WriteLine($"W: {CurrentFileName} ({LineNumber}): {message}");
        
        if (_listFile != null && _listOn)
        {
            ListOut(false);
            _lineError = true;
            _listFile.WriteLine($"**** Warning: {message}");
        }
        
        WarningCount++;
    }
    
    private void ListOut(bool reset)
    {
        if (_listFile != null && _listOn && !_lineError)
        {
            _listFile.WriteLine(_listLine.TrimEnd());
            if (reset)
                _listLine = $"     {new string(' ', 20)}{Origin:X4}";
        }
    }
    
    private void WriteListHeader()
    {
        if (_listFile == null)
            return;
            
        _listFile.WriteLine($"IBM 1130 Assembler - C# Port - {DateTime.Now}");
        if (ListOffset != 0)
            _listFile.WriteLine($"LIST OFFSET {ListOffset:X4}");
    }
}
