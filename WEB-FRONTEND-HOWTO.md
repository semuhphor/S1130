# IBM 1130 Emulator Web Frontend - Quick Start Guide

## Overview

This is a complete web-based IBM 1130 emulator with:
- ASP.NET Core 8 REST API backend
- React + TypeScript frontend
- Real-time CPU visualization with LED-style displays
- Full assembly and execution control

## Starting the Application

### Terminal 1: Backend API
```powershell
cd F:\Development\S1130
dotnet run --project src\S1130.WebApi\S1130.WebApi.csproj
```
**Backend will run at:** http://localhost:5000

### Terminal 2: Frontend
```powershell
cd F:\Development\S1130\web-frontend
npm start
```
**Frontend will open at:** http://localhost:3000

## Features

✅ **Assembler Editor**
- Write IBM 1130 assembly code
- Real-time syntax checking and assembly
- Pre-loaded example programs
- Error reporting with line numbers

✅ **LED-Style Register Display**
- All 6 registers visualized (IAR, ACC, EXT, XR1, XR2, XR3)
- Binary LED display (lights up red for 1, dark for 0)
- Hex and decimal values
- Status flags (Carry, Overflow, Wait)

✅ **Execution Control**
- **Step**: Execute one instruction at a time
- **Run**: Continuous execution with adjustable speed
- **Stop**: Halt execution
- Speed control: 1 IPS to 1,000,000 IPS

✅ **Real-Time Updates**
- Automatic polling when CPU is running (100ms refresh)
- Live instruction counter
- Execution status indicator

## Example Usage

1. **Start both terminals** (backend + frontend)
2. **Load the example program** (click "Load Example" button)
3. **Assemble the code** (click "Assemble" button)
4. **Step through execution** (click "Step" to execute one instruction)
5. **Watch the registers change** in the LED display!

## Example Program (Pre-loaded)

```
       ORG  /100
       LD   L DATA1
       A    L DATA2
       STO  L RESULT
       WAIT
DATA1  DC   42
DATA2  DC   58
RESULT DC   0
```

**What it does:**
1. Loads 42 into ACC
2. Adds 58 to ACC (result: 100)
3. Stores 100 to RESULT
4. Halts

## Architecture

### Backend (ASP.NET Core 8)
- `EmulatorService`: Singleton managing the CPU instance
- `AssemblerController`: Assembly and reset endpoints
- `ExecutionController`: Execution control and status

### Frontend (React + TypeScript)
- `AssemblerEditor`: Code editor with assembly controls
- `RegisterDisplay`: LED-style binary visualization
- `ControlPanel`: Step/Run/Stop buttons with speed control
- `CPUConsole`: Combines ControlPanel + RegisterDisplay
- `EmulatorPage`: Main page integrating all components

### API Endpoints
```
POST   /api/assembler/assemble
POST   /api/assembler/reset
GET    /api/execution/status
POST   /api/execution/step
POST   /api/execution/run?instructionsPerSecond=1000
POST   /api/execution/stop
```

## Testing

All 419 tests passing:
```powershell
cd F:\Development\S1130
dotnet test
```

- **Web API Tests**: 24 tests (EmulatorService, Controllers)
- **SystemObjects Tests**: 395 tests (CPU, Instructions, Devices)

## Development Status

✅ **Phase 0**: Foundation (branch, baseline)
✅ **Phase 1**: Backend API (EmulatorService, Controllers, DTOs)
✅ **Phase 2**: React Frontend (all components, integration)
⏭️ **Phase 3**: SignalR real-time updates (optional enhancement)

## Troubleshooting

### Backend won't start
- Use forward slashes on macOS/Linux: `dotnet run --project src/S1130.WebApi/S1130.WebApi.csproj`
- Or explicitly bind to HTTP 5000:
  - macOS/Linux: `ASPNETCORE_URLS=http://localhost:5000 dotnet run --project src/S1130.WebApi/S1130.WebApi.csproj`
  - Windows (PowerShell): `$env:ASPNETCORE_URLS = "http://localhost:5000"; dotnet run --project src\S1130.WebApi\S1130.WebApi.csproj`
- Check that port 5000 is available
- Ensure .NET 8 SDK is installed: `dotnet --version`
- If you need HTTPS locally, trust dev certs: `dotnet dev-certs https --trust` and then remove the ASPNETCORE_URLS override.

### Frontend can't connect to API
- Verify backend is running at http://localhost:5000
- Check CORS settings in `Program.cs`
- Verify `.env` file has correct API URL

### Assembly errors
- IBM 1130 assembly syntax is strict:
  - ORG directive required: `ORG /100` (hex with /)
  - No blank lines allowed
  - Long format for distant addresses: `LD L DATA`
  - Valid condition codes: Z, P, M, O, C, E

## Key Learnings (from TDD implementation)

1. **IBM 1130 CPU cycle**: Must call `NextInstruction()` then `ExecuteInstruction()`
2. **IAR initialization**: Assembler doesn't auto-set IAR - must parse listing
3. **BSC conditions**: Valid codes are Z (zero), P (plus), M (minus), O (overflow off), C (carry off), E (even)
4. **Thread safety**: EmulatorService uses locks for all CPU access
5. **Assembly format**: ORG /hex, no blanks, long format (L) for >255 displacement

## Files Created

### Backend
- `src/S1130.WebApi/` - Web API project
- `src/S1130.WebApi/Models/` - DTOs (4 files)
- `src/S1130.WebApi/Services/EmulatorService.cs` - Core service
- `src/S1130.WebApi/Controllers/` - 2 controllers
- `tests/UnitTests.S1130.WebApi/` - 24 tests

### Frontend
- `web-frontend/src/types/api.ts` - TypeScript types
- `web-frontend/src/services/apiClient.ts` - Axios client
- `web-frontend/src/components/` - 4 components
- `web-frontend/src/pages/EmulatorPage.tsx` - Main page

## Next Steps (Optional - Phase 3)

If you want to add SignalR for real-time updates:
1. Add SignalR hub to backend
2. Replace polling with WebSocket connection
3. Push CPU state changes from server

Current polling approach works well for typical usage!

---

**Enjoy exploring the IBM 1130!** 🖥️✨
