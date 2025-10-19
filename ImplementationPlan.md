# IBM 1130 Web Frontend Implementation Plan

Goal
- Add a small, incremental web UI for the S1130 IBM 1130 emulator.
- Keep tasks small, testable, and reversible.

Notes
- Backend: ASP.NET Core 8 Web API
- Frontend: React 18 + TypeScript
- Follow ICpu/InstructionBase/DeviceBase patterns from the emulator
- Windows PowerShell commands shown

------------------------------------------------------------
Phase 0 — Preparation (30–45 min total)
------------------------------------------------------------

Task 0.1: Create branch
- Command:
  - git checkout -b feature/web-frontend

Task 0.2: Ensure solution builds and tests pass
- Commands:
  - dotnet restore
  - dotnet build
  - dotnet test
- Accept: All tests green; no new warnings.

------------------------------------------------------------
Phase 1 — Minimal Web API to expose CPU state (1–2 hours)
------------------------------------------------------------

Task 1.1: Create Web API project and reference emulator
- Commands (from solution root f:\Development\S1130):
  - cd .\src
  - dotnet new webapi -n S1130.WebApi --framework net8.0
  - cd ..
  - dotnet sln .\S1130.sln add .\src\S1130.WebApi\S1130.WebApi.csproj
  - dotnet add .\src\S1130.WebApi\S1130.WebApi.csproj reference .\src\S1130.SystemObjects\S1130.SystemObjects.csproj
- Accept: Project builds.

Task 1.2: Enable Swagger + CORS
- Add Swagger and CORS policy (allow http://localhost:3000) in Program.cs.
- Accept: GET /swagger loads locally when running.

Task 1.3: EmulatorService (singleton)
- Add Services/EmulatorService.cs:
  - Hold a single Cpu instance
  - Thread-safe lock
  - Methods: GetState(), Reset(), ExecuteStep(), LoadMemory(addr, data[]), ReadMemory(addr, count), StartExecution(ips), StopExecution(), IsRunning
- Accept: Service compiles; no behavior change yet.

Task 1.4: CPU DTO and Controller
- Add Models/CpuStateDto.cs mapping ICpu: Acc, Ext, Iar, Carry, Overflow, Wait, InstructionCount, Xr1–Xr3, Timestamp.
- Add Controllers/CpuController.cs:
  - GET /api/cpu/state
  - POST /api/cpu/reset
  - POST /api/cpu/step
  - GET /api/cpu/registers (subset for fast polling)
- Accept: Endpoints return data in Swagger.

------------------------------------------------------------
Phase 2 — Memory API (1–2 hours)
------------------------------------------------------------

Task 2.1: Memory DTOs
- Add Models/MemoryBlockDto.cs { Address, Data[] }, and LoadMemoryRequest { Address, Instructions[] }.
- Accept: Compiles.

Task 2.2: MemoryController
- Endpoints:
  - GET /api/memory/{address}?count=16
  - POST /api/memory/load
  - PUT /api/memory/{address} (single word)
- Validate bounds [0..32767], support decimal and 0x-prefixed hex.
- Accept: CRUD verified via Swagger.

------------------------------------------------------------
Phase 3 — Execution control API (1 hour)
------------------------------------------------------------

Task 3.1: ExecutionController
- Endpoints:
  - POST /api/execution/step (return CpuStateDto)
  - POST /api/execution/run?instructionsPerSecond=1000 (background loop)
  - POST /api/execution/stop
  - GET /api/execution/status (isRunning + CpuStateDto)
- Stop automatically on WAIT.
- Accept: Can start/stop and step via Swagger.

------------------------------------------------------------
Phase 4 — Device status API (1–2 hours)
------------------------------------------------------------

Task 4.1: DeviceStatusDto
- Properties: DeviceCode, Name, Busy, InterruptActive, Details (optional).
- Accept: Compiles.

Task 4.2: DeviceController (read-only baseline)
- GET /api/devices
- GET /api/devices/{code}
- Note: Cpu should expose IEnumerable<IDevice> GetAttachedDevices().
- Accept: Returns empty or real device list depending on emulator exposure.

------------------------------------------------------------
Phase 5 — Frontend bootstrapping (1–2 hours)
------------------------------------------------------------

Task 5.1: Create React TypeScript app
- Commands (from solution root):
  - npx create-react-app web-frontend --template typescript
  - cd .\web-frontend
  - npm i axios
  - Add to package.json: "proxy": "http://localhost:5000"
- Accept: npm start runs at http://localhost:3000

Task 5.2: API client
- Create src/services/apiClient.ts:
  - Methods: getCpuState, resetCpu, step, readMemory, writeMemory, loadMemory, run, stop, getExecStatus, getDevices.
- Accept: TypeScript compiles.

Task 5.3: Simple CPU panel page
- Create a basic page that:
  - Shows Acc, Ext, Iar, flags
  - Buttons: Reset, Step
- Accept: Buttons call API and update view.

------------------------------------------------------------
Phase 6 — Console components (2–4 hours, small steps)
------------------------------------------------------------

Task 6.1: RegisterDisplay component
- Props: name, value, format (binary/hex/dec).
- Show 16 LED-like bits for binary mode.
- Accept: Reusable component renders.

Task 6.2: CPUConsole
- Displays: Acc, Ext, Iar, XR1–XR3, Carry/Overflow/Wait LEDs.
- Poll state every 500 ms (temporary; replace later with SignalR).
- Accept: Registers update on Step.

Task 6.3: ControlPanel
- Buttons: START, STOP, SINGLE STEP, RESET, LOAD IAR (input).
- Disable when inappropriate (e.g., START while running).
- Accept: Endpoints wired; UI state reflects isRunning.

------------------------------------------------------------
Phase 7 — Memory tools (2–4 hours, small steps)
------------------------------------------------------------

Task 7.1: MemoryViewer
- Inputs: startAddress, wordCount (default 32)
- Table: Address, Hex, Octal, Decimal
- Highlight IAR row
- Accept: Scrollable, fetches on change.

Task 7.2: MemoryEditor (inline edit)
- Click a cell to edit hex; validate bounds; PUT single word.
- Accept: Value persists after refresh.

Task 7.3: LoadProgram dialog
- Paste multi-word hex, choose starting address, POST load.
- Accept: Block loads; IAR can be set via CPU endpoint.

------------------------------------------------------------
Phase 8 — Devices (2–4 hours, optional milestones)
------------------------------------------------------------

Task 8.1: DevicePanel
- List devices from GET /api/devices
- Show name, code, Busy, Interrupt
- Poll every 1s
- Accept: Renders device cards.

Task 8.2: 2501 Card Reader basics (if available)
- Wire minimal endpoints (e.g., sense/control if exposed).
- Accept: Status reflects device state transitions.

------------------------------------------------------------
Phase 9 — Real-time updates (2–4 hours)
------------------------------------------------------------

Task 9.1: SignalR (backend)
- Add Microsoft.AspNetCore.SignalR
- Create EmulatorHub: broadcast CpuState and memory writes.
- EmulatorService invokes hub on step/run changes.
- Accept: Hub reachable; sample broadcast works.

Task 9.2: SignalR (frontend)
- npm i @microsoft/signalr
- Connect on app load, replace polling where practical.
- Accept: CPUConsole reacts to push updates.

------------------------------------------------------------
Phase 10 — Testing, docs, and polish (ongoing)
------------------------------------------------------------

Task 10.1: Backend tests
- Add minimal Web API unit tests (controller/service).
- Accept: dotnet test passes.

Task 10.2: Frontend tests
- Add React Testing Library tests for RegisterDisplay and CPUConsole.
- Accept: npm test passes locally.

Task 10.3: Developer docs
- Add docs/web-frontend.md:
  - How to run API + UI
  - Endpoint summary
  - Coding conventions (mirror CONTRIBUTING.md)
- Accept: Document committed.

------------------------------------------------------------
Quick Commands (PowerShell)
------------------------------------------------------------

Run API (in API project folder):
- dotnet run

Run API (solution root using launchSettings.json if configured):
- dotnet run --project .\src\S1130.WebApi\S1130.WebApi.csproj

Run frontend:
- cd .\web-frontend
- npm start

Build + test all:
- dotnet build
- dotnet test

Commit steps:
- git add .
- git commit -m "Web: add minimal API and CPU panel"
- git push origin feature/web-frontend

------------------------------------------------------------
Acceptance Checklist
------------------------------------------------------------

Backend
- [ ] S1130.WebApi builds and runs with Swagger and CORS
- [ ] EmulatorService (singleton) exposes Cpu safely
- [ ] CPU endpoints: state/reset/step
- [ ] Memory endpoints: read/write/load
- [ ] Execution endpoints: run/stop/status
- [ ] Optional: devices list

Frontend
- [ ] React app boots
- [ ] API client typed and working
- [ ] CPU panel shows registers and flags
- [ ] Controls: Reset, Step, Start/Stop
- [ ] Memory: view/edit/load
- [ ] Devices panel (basic list)

Quality
- [ ] Back/Front tests added for key paths
- [ ] Docs for dev setup
- [ ] No breaking changes to S1130.SystemObjects