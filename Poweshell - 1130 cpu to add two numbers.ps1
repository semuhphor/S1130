Add-Type -Path 'V:\Development\IBM\IBM1130\1130Emulator\1130.SystemObjects\bin\Debug\S1130.SystemObjects.dll'
$state = New-Object S1130.SystemObjects.SystemState
$cpu = New-Object S1130.SystemObjects.Cpu($state)
$cpu[$cpu.Iar] = [S1130.SystemObjects.InstructionBuilder]::BuildShort([S1130.SystemObjects.Instructions.OpCodes]::Load, 0, 0x10)
$cpu.NextInstruction()
$cpu.Item($cpu.Iar + 0x10) = 1234
$cpu.ExecuteInstruction()
$cpu[$cpu.Iar] = [S1130.SystemObjects.InstructionBuilder]::BuildShort([S1130.SystemObjects.Instructions.OpCodes]::Add, 0, 0x20)
$cpu.NextInstruction()
$cpu.Item($cpu.Iar + 0x20) = 101
$cpu.ExecuteInstruction()
[string]::Format("Is it 1234 + 101 ... or {0}?", 1234 + 101)
$cpu.Acc
"1130 rules!"