# IBM 1130 Emulator Web Sites - Quick Start Guide

There are two web front ends:

1. **`S1130.Site` (current):** boot DMS, submit a card deck, get a 1403 printout. One private machine per browser. Plain HTML, no Node needed. See [docs/WebSite-Plan.md](docs/WebSite-Plan.md).
2. **`S1130.WebApi` + React (older):** assemble a small program and step it. Described further down.

## S1130.Site: boot DMS and run a deck

### Requirements
- .NET 10 SDK (`dotnet --version`).
- Nothing else: the master DMS disk ships with the site in its disk library, [src/S1130.Site/Disks](src/S1130.Site/Disks/README.md). The default is `dms-r2v12-1403-1442-32k.dsk` (1403 + 1442 + 32K). To boot a different disk, put it in that folder, add it to the README, and set `Site:MasterDisk` to its file name.

### Start it
From the repository root:
```powershell
dotnet run --project src\S1130.Site
```
The site runs at **http://localhost:5299** (the `http` profile in `src\S1130.Site\Properties\launchSettings.json`). To pick another address:
```powershell
dotnet run --project src\S1130.Site --urls http://localhost:5299
```
Stop it with Ctrl+C. If you rebuild while it runs, the build fails because the DLLs are locked, so stop it first.

### Use it
1. Type or paste a deck into the **Deck** box. One card per line, cut to 80 columns and upper-cased. You can also use **Load file**.
2. Click **Load into hopper**. The hopper count goes up.
3. Click **Boot DMS**. DMS cold-starts from this browser's working disk and reads the deck from the 1442.
4. The printout appears in the **1403 Printer** pane. Use **Tear off** to clear it and **Download** to save it as text.

The first page of every boot (`// JOB    000B`) is DMS's own start-up job. Your deck's output starts on the next page.

Example deck (assemble and run). Labels start in column 21, opcodes in 27, the format/tag field in 32-33, operands in 35:
```
// JOB
// ASM
*LIST
                    START LIBF    PRNT3     PRINT ONE LINE, THEN SPACE
                          DC      /2000     CONTROL - PRINT
                          DC      LINE      I/O AREA (WORD COUNT, THEN DATA)
                          DC      ERR       ERROR ROUTINE
                    WAIT  LIBF    PRNT3     TEST - IS PRNT3 STILL BUSY
                          DC      /0000     CONTROL - TEST
                          MDX     WAIT      BUSY, ASK AGAIN
                          EXIT              DONE, RETURN TO DMS
                    ERR   DC      0         ERROR ROUTINE - JUST RETURN
                          BSC  I  ERR
                    LINE  DC      5         WORD COUNT (2 CHARS PER WORD)
                          DC      /6B68     H E   (1403 PRINTER CODE)
                          DC      /1A1A     L L
                          DC      /5D20     O R
                          DC      /1A67     L D
                          DC      /6E7F     . BLANK
                          END     START
// XEQ
```
It prints `HELLORLD.`. The 1403 print chain has no `!`.

### Settings (`src\S1130.Site\appsettings.json`, section `Site`)
| Setting | Default | Meaning |
|---|---|---|
| `DiskLibrary` | `Disks` | Folder of genned system disks (relative to the site folder) |
| `MasterDisk` | `dms-r2v12-1403-1442-32k.dsk` | DMS image copied to each new machine (relative to `DiskLibrary`, or a full path) |
| `DataRoot` | `App_Data` | Per-session folders; each holds `disks\work.dsk` |
| `MaxMachines` | 8 | Machines that may exist at once (more gives HTTP 503) |
| `IdleMinutes` | 30 | Idle machines are stopped and removed; the working disk stays and is reused if the browser comes back |
| `MaxDeckCards` | 2000 | Largest deck accepted (more gives HTTP 413) |

Each browser gets an `s1130_session` cookie and its own machine and working disk. Clearing cookies gives a fresh machine. Delete `src\S1130.Site\App_Data` to throw away all working disks.

### API (used by the page)
```
GET    /api/machine                 lamps/registers snapshot
POST   /api/machine/boot            cold-start DMS
POST   /api/machine/deck            text/plain deck into the hopper
POST   /api/machine/start | stop | reset
GET    /api/machine/printout?since=N
DELETE /api/machine/printout        tear off
```

### Tests
```powershell
dotnet test tests\UnitTests.S1130.Site
```
The DMS tests return early (pass) if `dms.dsk` is missing.

### Troubleshooting
- **Boot DMS returns 409:** the master disk was not found. Check `Site:DiskLibrary` and `Site:MasterDisk`.
- **503:** too many machines; wait for idle ones to be reaped or raise `MaxMachines`.
- **Port in use:** pass a different `--urls`.

---

## S1130.WebApi + React (older front end)

This is a web-based IBM 1130 emulator with:
- ASP.NET Core 8 REST API backend
- React + TypeScript frontend
- Real-time CPU visualization with LED-style displays
- Full assembly and execution control

## Starting the Application

### Terminal 1: Backend API
```powershell
cd <repository root>
dotnet run --project src\S1130.WebApi\S1130.WebApi.csproj
```
**Backend will run at:** http://localhost:5000

### Terminal 2: Frontend
```powershell
cd <repository root>\web-frontend
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
cd <repository root>
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
- Check that port 5000 is available
- Ensure .NET 10 SDK is installed: `dotnet --version`

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
