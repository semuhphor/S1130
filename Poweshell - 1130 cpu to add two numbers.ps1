Add-Type -Path '.\src\S1130.SystemObjects\bin\Debug\netcoreapp2.2\S1130.SystemObjects.dll'
Add-Type -Path '.\test\UnitTests.S1130\bin\Debug\netcoreapp2.2\UnitTests.S1130.dll'
$cpu = New-Object S1130.SystemObjects.Cpu
$cpu[$cpu.Iar] = [UnitTests.S1130.InstructionBuilder]::BuildShort([S1130.SystemObjects.Instructions.OpCodes]::Load, 0, 0x10)
$cpu.NextInstruction()
$cpu.Item($cpu.Iar + 0x10) = 980
$cpu.ExecuteInstruction()
$cpu[$cpu.Iar] = [UnitTests.S1130.InstructionBuilder]::BuildShort([S1130.SystemObjects.Instructions.OpCodes]::Add, 0, 0x20)
$cpu.NextInstruction()
$cpu.Item($cpu.Iar + 0x20) = 150
$cpu.ExecuteInstruction()
[string]::Format("980 + 150 should add up to {0}... Does it?", 980 + 150)
$cpu.Acc
"1130 rules!"
$a = Read-Host 'Hit enter to end >'