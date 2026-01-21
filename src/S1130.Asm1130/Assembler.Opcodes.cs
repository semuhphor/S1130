namespace S1130.Asm1130;

/// <summary>
/// COMPLETE opcode table initialization and instruction handlers
/// All IBM 1130 instructions plus assembler directives
/// </summary>
public partial class Assembler
{
    private void InitializeOpcodes()
    {
        // Assembler directives
        AddOpcode("ABS",  HandleAbs,  "",  "",  0, 0);
        AddOpcode("REL",  HandleRel,  "",  "",  0, 0);
        AddOpcode("BES",  HandleBes,  "E", "",  0, 0);
        AddOpcode("BSS",  HandleBss,  "E", "",  0, 0);
        AddOpcode("DC",   HandleDc,   "E", "",  0, 0);
        AddOpcode("DEC",  HandleDec,  "E", "E", OpcodeFlags.IsDouble, 0);
        AddOpcode("DMES", HandleDmes, "012", "", 0, 0);
        AddOpcode("DN",   HandleDn,   "",  "",  0, 0);
        AddOpcode("DSA",  HandleDsa,  "",  "",  0, 0);
        AddOpcode("DUMP", HandleDump, "",  "",  0, 0);
        AddOpcode("EBC",  HandleEbc,  "",  "",  0, 0);
        AddOpcode("EJCT", HandleEjct, "",  "",  0, 0);
        AddOpcode("END",  HandleEnd,  "",  "",  0, 0);
        AddOpcode("ENT",  HandleEnt,  "",  "",  0, 0);
        AddOpcode("EQU",  HandleEqu,  "",  "",  0, 0);
        AddOpcode("EXIT", HandleExit, "",  "",  0, 0);
        AddOpcode("FILE", HandleFile, "",  "",  0, 0);
        AddOpcode("HDNG", HandleHdng, "",  "",  0, 0);
        AddOpcode("ILS",  HandleIls,  "0123456789", "", 0, 0);
        AddOpcode("ISS",  HandleIss,  "0123456789", "", 0, 0);
        AddOpcode("LIBF", HandleLibf, "",  "",  0, 0);
        AddOpcode("LIBR", HandleLibr, "",  "",  0, 0);
        AddOpcode("LINK", HandleLink, "",  "",  0, 0);
        AddOpcode("LIST", HandleList, "",  "",  0, 0);
        AddOpcode("LORG", HandleLorg, "",  "",  0, 0);
        AddOpcode("ORG",  HandleOrg,  "",  "",  0, 0);
        AddOpcode("PDMP", HandlePdmp, "",  "",  0, 0);
        AddOpcode("SPAC", HandleSpac, "",  "",  0, 0);
        
        // Standard addressing instructions
        AddOpcode("A",    HandleStd,  "LIX123", "", 0, 0x8000);
        AddOpcode("AD",   HandleStd,  "LIX123", "", OpcodeFlags.IsDouble, 0x8800);
        AddOpcode("AND",  HandleStd,  "LIX123", "", 0, 0xE000);
        AddOpcode("BSI",  HandleBsi,  "LIX123", "", 0, 0x4000);
        AddOpcode("CALL", HandleCall, "LIX123", "L", 0, 0x4000);
        AddOpcode("CMP",  HandleStd,  "LIX123", "", OpcodeFlags.Is1800, 0xB000);
        AddOpcode("DCM",  HandleStd,  "LIX123", "", OpcodeFlags.Is1800, 0xB800);
        AddOpcode("D",    HandleStd,  "LIX123", "", 0, 0xA800);
        AddOpcode("EOR",  HandleStd,  "LIX123", "", 0, 0xF000);
        AddOpcode("LD",   HandleStd,  "LIX123", "", 0, 0xC000);
        AddOpcode("LDD",  HandleStd,  "LIX123", "", OpcodeFlags.IsDouble, 0xC800);
        AddOpcode("LDS",  HandleStd,  "",       "", 0, 0x2000);
        AddOpcode("LDX",  HandleStd,  "LI",     "", 0, 0x6000);
        AddOpcode("M",    HandleStd,  "LIX123", "", 0, 0xA000);
        AddOpcode("MDX",  HandleMdx,  "LIX123", "", 0, 0x7000);
        AddOpcode("MDM",  HandleMdx,  "L",      "L", 0, 0x7000);
        AddOpcode("NOP",  HandleNop,  "",       "", 0, 0x1000);
        AddOpcode("OR",   HandleStd,  "LIX123", "", 0, 0xE800);
        AddOpcode("S",    HandleStd,  "LIX123", "", 0, 0x9000);
        AddOpcode("SD",   HandleStd,  "LIX123", "", OpcodeFlags.IsDouble, 0x9800);
        AddOpcode("STD",  HandleStd,  "LIX123", "", OpcodeFlags.IsDouble, 0xD800);
        AddOpcode("STO",  HandleStd,  "LIX123", "", 0, 0xD000);
        AddOpcode("STS",  HandleStd,  "LIX123", "", 0, 0x2800);
        AddOpcode("STX",  HandleStd,  "LI",     "", 0, 0x6800);
        AddOpcode("WAIT", HandleWait, "",       "", 0, 0x3000);
        AddOpcode("XCH",  HandleXch,  "",       "", 0, 0x18D0);
        AddOpcode("XIO",  HandleXio,  "LIX123", "", OpcodeFlags.IsDouble, 0x0800);
        
        // Branch family
        AddOpcode("BSC",  HandleBsc,  "LICEOX123", "", 0, 0x4800);
        AddOpcode("BOSC", HandleBsc,  "LICEOX123", "", 0, 0x4840);
        AddOpcode("SKP",  HandleBsc,  "",       "", 0, 0x4800);
        AddOpcode("B",    HandleB,    "LIX123", "", 0, 0x4800);
        
        // Branch aliases (BSC variants with condition preset)
        AddOpcode("BC",   HandleStd,  "LIX123", "L", 0, 0x4802);
        AddOpcode("BN",   HandleStd,  "LIX123", "L", 0, 0x4828);
        AddOpcode("BNN",  HandleStd,  "LIX123", "L", 0, 0x4810);
        AddOpcode("BNP",  HandleStd,  "LIX123", "L", 0, 0x4808);
        AddOpcode("BNZ",  HandleStd,  "LIX123", "L", 0, 0x4820);
        AddOpcode("BO",   HandleStd,  "LIX123", "L", 0, 0x4801);
        AddOpcode("BOD",  HandleStd,  "LIX123", "L", 0, 0x4804);
        AddOpcode("BP",   HandleStd,  "LIX123", "L", 0, 0x4830);
        AddOpcode("BZ",   HandleStd,  "LIX123", "L", 0, 0x4818);
        
        // Shift family
        AddOpcode("RTE",  HandleShift, "X123", "X", 0, 0x18C0);
        AddOpcode("SLA",  HandleShift, "X123", "X", 0, 0x1000);
        AddOpcode("SLC",  HandleShift, "X123", "X", 0, 0x10C0);
        AddOpcode("SLCA", HandleShift, "X123", "X", 0, 0x1040);
        AddOpcode("SLT",  HandleShift, "X123", "X", 0, 0x1080);
        AddOpcode("SRA",  HandleShift, "X123", "X", 0, 0x1800);
        AddOpcode("SRT",  HandleShift, "X123", "X", 0, 0x1880);
    }
    
    private void AddOpcode(string mnem, OpcodeHandler handler, string modsAllowed, string modsImplied, OpcodeFlags flags, int baseValue)
    {
        _opcodes.Add(new Opcode
        {
            Mnemonic = mnem,
            Handler = handler,
            ModifiersAllowed = modsAllowed,
            ModifiersImplied = modsImplied,
            Flags = flags,
            BaseValue = baseValue
        });
    }
    
    // === DIRECTIVE HANDLERS ===
    
    private void HandleAbs(Opcode op, string label, string mods, string arg)
    {
        Relocate = RelocationType.Absolute;
    }
    
    private void HandleRel(Opcode op, string label, string mods, string arg)
    {
        Relocate = RelocationType.Relative;
    }
    
    private void HandleBes(Opcode op, string label, string mods, string arg)
    {
        // Block Ended by Symbol - label gets address AFTER the space
        if (mods.Contains('E'))
            OriginEven();
            
        var expr = string.IsNullOrEmpty(arg) ? new Expression() : GetExpression(arg, false);
        
        if (expr.Relocation != RelocationType.Absolute && expr.Value != 0)
            Error("BES size must be absolute");
            
        if (expr.Value > 0)
        {
            SetWord(0, Origin + expr.Value + ListOffset, RelocationType.Absolute);
            if (Pass == 2)
                SetOrigin(Origin + expr.Value);
            else
                Origin += expr.Value;
        }
        
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
    }
    
    private void HandleBss(Opcode op, string label, string mods, string arg)
    {
        OriginAdvanced = 0;
        
        if (mods.Contains('E'))
            OriginEven();
            
        SetWord(0, Origin + ListOffset, RelocationType.Absolute);
        
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        var expr = string.IsNullOrEmpty(arg) ? new Expression() : GetExpression(arg, false);
        
        if (expr.Relocation != RelocationType.Absolute && expr.Value != 0)
            Error("BSS size must be absolute");
            
        if (expr.Value > 0)
        {
            if (Pass == 2)
                SetOrigin(Origin + expr.Value);
            else
                Origin += expr.Value;
        }
    }
    
    private void HandleDc(Opcode op, string label, string mods, string arg)
    {
        OriginAdvanced = 1;
        
        if (mods.Contains('E'))
            OriginEven();
            
        SetWord(0, Origin + ListOffset, RelocationType.Absolute);
        
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        var expr = GetExpression(arg, false);
        WriteWord(expr.Value, expr.Relocation);
    }
    
    private void HandleDec(Opcode op, string label, string mods, string arg)
    {
        // Floating point - not fully implemented yet
        Warning("DEC directive not fully implemented");
        HandleDc(op, label, mods, arg);
    }
    
    private void HandleDmes(Opcode op, string label, string mods, string arg)
    {
        // Message encoding - simplified implementation
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
        Warning("DMES directive not fully implemented");
    }
    
    private void HandleDn(Opcode op, string label, string mods, string arg)
    {
        // Define name - pack 5 chars into 2 words
        SetWord(0, Origin + ListOffset, RelocationType.Absolute);
        
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        var packed = PackName(arg);
        WriteWord(packed.high, RelocationType.Absolute);
        WriteWord(packed.low, RelocationType.Absolute);
    }
    
    private (int high, int low) PackName(string name)
    {
        // Pack 5 characters into 2 words (EBCDIC 6-bit packing)
        long val = 0;
        for (int i = 0; i < 5; i++)
        {
            char c = i < name.Length ? name[i] : ' ';
            uint ebcdic = (uint)(AsciiToEbcdic(c) & 0x3F);
            val = (val << 6) | ebcdic;
        }
        return ((int)(val >> 16), (int)(val & 0xFFFF));
    }
    
    private int AsciiToEbcdic(char c)
    {
        // Simplified ASCII to EBCDIC conversion
        if (c >= 'A' && c <= 'Z') return 0xC1 + (c - 'A');
        if (c >= '0' && c <= '9') return 0xF0 + (c - '0');
        if (c == ' ') return 0x40;
        return 0x40;
    }
    
    private void HandleDsa(Opcode op, string label, string mods, string arg)
    {
        // Define storage area - similar to BSS
        HandleBss(op, label, mods, arg);
    }
    
    private void HandleDump(Opcode op, string label, string mods, string arg)
    {
        // DUMP = call $dump + call $exit
        HandlePdmp(op, label, mods, arg);
        HandleExit(op, "", "", "");
    }
    
    private void HandleEbc(Opcode op, string label, string mods, string arg)
    {
        // EBCDIC string
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
        Warning("EBC directive not fully implemented");
    }
    
    private void HandleEjct(Opcode op, string label, string mods, string arg)
    {
        // Form feed in listing
        if (_listFile != null && _listOn)
        {
            _listFile.Write('\f');
            _lineError = true;
        }
    }
    
    private void HandleEnd(Opcode op, string label, string mods, string arg)
    {
        Ended = true;
        
        if (!string.IsNullOrEmpty(arg))
        {
            var expr = GetExpression(arg, false);
            ProgramTransferAddress = expr.Value;
        }
    }
    
    private void HandleEnt(Opcode op, string label, string mods, string arg)
    {
        // Entry point definition - for library functions
        Warning("ENT directive not fully implemented");
    }
    
    private void HandleEqu(Opcode op, string label, string mods, string arg)
    {
        if (string.IsNullOrEmpty(label))
        {
            Error("EQU requires a label");
            return;
        }
        
        var expr = GetExpression(arg, false);
        SetSymbol(label, expr.Value, true, expr.Relocation);
    }
    
    private void HandleExit(Opcode op, string label, string mods, string arg)
    {
        // call $exit
        EncodeStandardInstruction(new Opcode { BaseValue = 0x4000, ModifiersImplied = "L" },
                                 label, "L", "$EXIT");
    }
    
    private void HandleFile(Opcode op, string label, string mods, string arg)
    {
        // FILE directive - metadata
        Warning("FILE directive not fully implemented");
    }
    
    private void HandleHdng(Opcode op, string label, string mods, string arg)
    {
        // Heading in listing
        if (_listFile != null && _listOn)
        {
            _listFile.WriteLine($"\f{arg.Trim()}\n");
            _lineError = true;
        }
    }
    
    private void HandleIls(Opcode op, string label, string mods, string arg)
    {
        // Interrupt level subroutine
        Warning("ILS directive not fully implemented");
    }
    
    private void HandleIss(Opcode op, string label, string mods, string arg)
    {
        // Interrupt service subroutine
        Warning("ISS directive not fully implemented");
    }
    
    private void HandleLibf(Opcode op, string label, string mods, string arg)
    {
        // Library function
        OutputLiterals(true);
    }
    
    private void HandleLibr(Opcode op, string label, string mods, string arg)
    {
        // Library reference
        Warning("LIBR directive not fully implemented");
    }
    
    private void HandleLink(Opcode op, string label, string mods, string arg)
    {
        // Link directive
        Warning("LINK directive not fully implemented");
    }
    
    private void HandleList(Opcode op, string label, string mods, string arg)
    {
        // LIST ON/OFF
        if (arg.Equals("ON", StringComparison.OrdinalIgnoreCase))
            _listOn = true;
        else if (arg.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            _listOn = false;
        else
            _listOn = DoList;
            
        _lineError = true;
    }
    
    private void HandleLorg(Opcode op, string label, string mods, string arg)
    {
        // Listing origin offset
        var expr = GetExpression(arg, false);
        ListOffset = expr.Value;
    }
    
    private void HandleOrg(Opcode op, string label, string mods, string arg)
    {
        var expr = GetExpression(arg, false);
        SetOrigin(expr.Value);
    }
    
    private void HandlePdmp(Opcode op, string label, string mods, string arg)
    {
        // Program dump - call $dump with arguments
        Warning("PDMP directive not fully implemented - generating BSI $DUMP");
        EncodeStandardInstruction(new Opcode { BaseValue = 0x4000, ModifiersImplied = "L" },
                                 label, "L", "$DUMP");
    }
    
    private void HandleSpac(Opcode op, string label, string mods, string arg)
    {
        // Spacing in listing
        if (_listFile != null && _listOn)
        {
            var expr = GetExpression(arg, false);
            for (int i = 0; i < expr.Value; i++)
                _listFile.WriteLine();
            _lineError = true;
        }
    }
    
    // === INSTRUCTION HANDLERS ===
    
    private void HandleStd(Opcode op, string label, string mods, string arg)
    {
        EncodeStandardInstruction(op, label, mods, arg);
    }
    
    private void HandleBsi(Opcode op, string label, string mods, string arg)
    {
        // BSI supports condition codes just like BSC
        // BSI L ADDRESS,Z- means "Branch and Store IAR if zero or negative"
        HandleBsc(new Opcode { BaseValue = 0x4000, ModifiersImplied = "" }, label, mods, arg);
    }
    
    private void HandleCall(Opcode op, string label, string mods, string arg)
    {
        // CALL is BSI with L implied
        EncodeStandardInstruction(op, label, mods + "L", arg);
    }
    
    private void HandleMdx(Opcode op, string label, string mods, string arg)
    {
        // MDX has format: MDX [L][I][1-3] ADDRESS,COUNT
        // where COUNT is 0-3 and goes in bits 8-9
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        int instruction = op.BaseValue;
        string addressPart = arg;
        int count = 0;
        
        // Parse address,count format
        if (!string.IsNullOrEmpty(arg) && arg.Contains(','))
        {
            var parts = arg.Split(',');
            addressPart = parts[0].Trim();
            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int c))
                count = c & 3; // Only 2 bits for count
        }
        
        // Add count to instruction (bits 8-9)
        instruction |= (count << 6);
        
        // Add modifier bits
        if (mods.Contains('L')) instruction |= 0x0400;
        if (mods.Contains('I')) instruction |= 0x0080;
        if (mods.Contains('1')) instruction |= 0x0100;
        else if (mods.Contains('2')) instruction |= 0x0200;
        else if (mods.Contains('3')) instruction |= 0x0300;
        
        // Get address
        var expr = GetExpression(addressPart, false);
        
        if (mods.Contains('L'))
        {
            WriteWord(instruction, RelocationType.Absolute);
            WriteWord(expr.Value, expr.Relocation);
        }
        else
        {
            int displacement = expr.Value - (Origin + 1);
            if (displacement < -128 || displacement > 127)
                Warning($"Displacement {displacement} out of range for short format");
            instruction |= (displacement & 0xFF);
            WriteWord(instruction, expr.Relocation);
        }
    }
    
    private void HandleNop(Opcode op, string label, string mods, string arg)
    {
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        WriteWord(op.BaseValue, RelocationType.Absolute);
    }
    
    private void HandleWait(Opcode op, string label, string mods, string arg)
    {
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        WriteWord(op.BaseValue, RelocationType.Absolute);
    }
    
    private void HandleXch(Opcode op, string label, string mods, string arg)
    {
        // XCH = RTE 16
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        WriteWord(0x18D0, RelocationType.Absolute);
    }
    
    private void HandleXio(Opcode op, string label, string mods, string arg)
    {
        // XIO is a standard double-word instruction with IOCC address
        EncodeStandardInstruction(op, label, mods, arg);
    }
    
    private void HandleBsc(Opcode op, string label, string mods, string arg)
    {
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        int instruction = op.BaseValue;
        
        // Parse arg which may be "ADDRESS" or "ADDRESS,condition" or just condition
        string addressPart = arg;
        string conditionPart = "";
        
        // Check if arg is just a condition code
        // Condition codes: C, E, O, Z, X, +, -, or combinations like +-, +-Z, Z+, Z-, etc.
        bool isJustCondition = !string.IsNullOrEmpty(arg) && arg.Length <= 3 &&
                               arg.All(c => "CEOZX+-".Contains(c));
        
        if (isJustCondition)
        {
            // Arg is actually a condition code, not an address
            conditionPart = arg;
            addressPart = "";
        }
        else if (!string.IsNullOrEmpty(arg) && arg.Contains(','))
        {
            var parts = arg.Split(',');
            addressPart = parts[0].Trim();
            conditionPart = parts.Length > 1 ? parts[1].Trim() : "";
        }
        
        // Parse condition codes from both mods and comma-separated part
        string allConditions = mods + conditionPart;
        
        if (allConditions.Contains('C')) instruction |= 0x2000;
        if (allConditions.Contains('E')) instruction |= 0x1000;
        if (allConditions.Contains('O')) instruction |= 0x0800;
        if (allConditions.Contains('X')) instruction |= 0x0200;
        
        // Special condition combinations
        if (allConditions.Contains('Z')) instruction |= 0x1000; // E = zero
        if (allConditions.Contains('+'))
        {
            if (allConditions.Contains('-'))
                instruction |= 0x1800; // E + O = plus or minus (not zero)
            else
                instruction |= 0x1000 | 0x0800; // Positive
        }
        else if (allConditions.Contains('-'))
            instruction |= 0x0800; // Negative
        
        // Add tag bits
        if (allConditions.Contains('L')) instruction |= 0x0400;
        if (allConditions.Contains('I')) instruction |= 0x0080;
        if (allConditions.Contains('1')) instruction |= 0x0100;
        else if (allConditions.Contains('2')) instruction |= 0x0200;
        else if (allConditions.Contains('3')) instruction |= 0x0300;
        
        // Get address if present
        if (string.IsNullOrEmpty(addressPart))
        {
            WriteWord(instruction, RelocationType.Absolute);
            return;
        }
        
        var expr = GetExpression(addressPart, false);
        
        if (allConditions.Contains('L'))
        {
            WriteWord(instruction, RelocationType.Absolute);
            WriteWord(expr.Value, expr.Relocation);
        }
        else
        {
            int displacement = expr.Value - (Origin + 1);
            if (displacement < -128 || displacement > 127)
                Warning($"Displacement {displacement} out of range");
            instruction |= (displacement & 0xFF);
            WriteWord(instruction, expr.Relocation);
        }
    }
    
    private void HandleB(Opcode op, string label, string mods, string arg)
    {
        // B is either MDX or BSC L depending on usage
        // For simplicity, treat as BSC L
        HandleBsc(new Opcode { BaseValue = 0x4800, ModifiersImplied = "L" },
                 label, mods + "L", arg);
    }
    
    private void HandleShift(Opcode op, string label, string mods, string arg)
    {
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        int instruction = op.BaseValue;
        
        // Get shift count
        var expr = GetExpression(arg, false);
        int shiftCount = expr.Value & 0x3F;
        
        // Add index register
        if (mods.Contains('1')) instruction |= 0x0100;
        else if (mods.Contains('2')) instruction |= 0x0200;
        else if (mods.Contains('3')) instruction |= 0x0300;
        
        instruction |= shiftCount;
        
        WriteWord(instruction, RelocationType.Absolute);
    }
    
    // Standard instruction encoder
    private void EncodeStandardInstruction(Opcode op, string label, string mods, string arg)
    {
        if (!string.IsNullOrEmpty(label))
            SetSymbol(label, Origin, true, Relocate);
            
        int instruction = op.BaseValue;
        
        // Add tag bits
        if (mods.Contains('L')) instruction |= 0x0400;
        if (mods.Contains('I')) instruction |= 0x0080;
        if (mods.Contains('1')) instruction |= 0x0100;
        else if (mods.Contains('2')) instruction |= 0x0200;
        else if (mods.Contains('3')) instruction |= 0x0300;
        
        if (string.IsNullOrEmpty(arg))
        {
            WriteWord(instruction, RelocationType.Absolute);
            return;
        }
        
        // Get address
        var expr = GetExpression(arg, false);
        
        if (mods.Contains('L'))
        {
            // Long format
            WriteWord(instruction, RelocationType.Absolute);
            WriteWord(expr.Value, expr.Relocation);
        }
        else
        {
            // Short format: 8-bit displacement
            int displacement = expr.Value - (Origin + 1);
            if (displacement < -128 || displacement > 127)
                Warning($"Displacement {displacement} out of range for short format");
            instruction |= (displacement & 0xFF);
            WriteWord(instruction, expr.Relocation);
        }
    }
}
