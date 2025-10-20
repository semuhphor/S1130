# IBM 1130 Web Frontend Implementation Plan

## Primary Goal (Phase 1 Focus)
Create a simple web interface that:
1. Starts an IBM 1130 emulator instance
2. Provides a text box to enter assembler code
3. Has an "Assemble" button to compile the code into the emulator's memory
4. Can Run or Step through the assembled program
5. Shows dynamically-updated registers:
   - IAR (Instruction Address Register)
   - ACC (Accumulator)
   - EXT (Extension Register)
   - XR1, XR2, XR3 (Index Registers)
   - Current Interrupt Level being serviced
6. Register values update in real-time during execution

## Overall Approach
- Add a small, incremental web UI for the S1130 IBM 1130 emulator
- Keep tasks small, testable, and reversible
- Focus on the assembler-driven workflow first, add complexity later

Notes
- Backend: ASP.NET Core 8 Web API (cross-platform: Windows, Linux, macOS)
- Frontend: React 18 + TypeScript (cross-platform: runs in any modern browser)
- The emulator and web interface can run on Windows, Linux, or macOS
- Follow ICpu/InstructionBase/DeviceBase patterns from the emulator
- Windows PowerShell commands shown (use bash equivalents on Linux/macOS)

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
Phase 1 — Core Assembler Web Interface (Priority: Highest)
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
  - Hold a single Cpu instance with assembler
  - Thread-safe lock
  - Methods: 
    - GetState() → CpuStateDto with IAR, ACC, EXT, XR1-3, current interrupt level
    - Reset()
    - Assemble(sourceCode) → returns success/error + line numbers for errors
    - ExecuteStep() → step one instruction
    - StartExecution(ips) → continuous run
    - StopExecution()
    - IsRunning
- Accept: Service compiles; no behavior change yet.

Task 1.4: CPU DTO for assembler workflow
- Add Models/CpuStateDto.cs mapping ICpu:
  - Iar (Instruction Address Register)
  - Acc (Accumulator)
  - Ext (Extension Register)
  - Xr1, Xr2, Xr3 (Index Registers)
  - CurrentInterruptLevel (0-5, or -1 if none active)
  - Carry, Overflow, Wait flags
  - InstructionCount
  - Timestamp
- Add Models/AssembleRequest.cs { SourceCode: string, StartAddress?: ushort }
- Add Models/AssembleResponse.cs { Success: bool, Errors: string[], LoadedAddress: ushort? }
- Accept: DTOs compile.

Task 1.5: AssemblerController
- Add Controllers/AssemblerController.cs:
  - POST /api/assembler/assemble (body: AssembleRequest) → AssembleResponse
    - Compiles assembler source using S1130 assembler
    - Loads into emulator memory
    - Returns errors with line numbers if any
  - POST /api/assembler/reset
    - Resets CPU and clears memory
- Accept: Endpoints return data in Swagger.

Task 1.6: ExecutionController (simplified for assembler workflow)
- Add Controllers/ExecutionController.cs:
  - POST /api/execution/step → returns updated CpuStateDto
  - POST /api/execution/run?instructionsPerSecond=1000 → background execution loop
  - POST /api/execution/stop → halts execution
  - GET /api/execution/status → { isRunning: bool, cpuState: CpuStateDto }
- Stop automatically on WAIT instruction.
- Accept: Can start/stop/step via Swagger; state updates reflect execution.

------------------------------------------------------------
Phase 2 — React Frontend with Assembler Editor (Priority: Highest)
------------------------------------------------------------

Task 2.1: Create React TypeScript app
- Commands (from solution root):
  - npx create-react-app web-frontend --template typescript
  - cd .\web-frontend
  - npm i axios
  - Add to package.json: "proxy": "http://localhost:5000"
- Accept: npm start runs at http://localhost:3000

Task 2.2: API client for assembler workflow
- Create src/services/apiClient.ts:
  - Methods:
    - assemble(sourceCode: string, startAddress?: number)
    - resetCpu()
    - step() → CpuStateDto
    - run(instructionsPerSecond: number)
    - stop()
    - getExecutionStatus() → { isRunning: bool, cpuState: CpuStateDto }
- Accept: TypeScript compiles.

Task 2.3: RegisterDisplay component
- Create src/components/RegisterDisplay.tsx
- Props: name (IAR/ACC/EXT/XR1/XR2/XR3), value (ushort), format (hex/octal/decimal)
- Show 16 LED-like bits for visual feedback
- Accept: Reusable component renders with all formats.

Task 2.4: AssemblerEditor component
- Create src/components/AssemblerEditor.tsx
- Features:
  - Large textarea for assembler source code
  - "Assemble" button (calls API)
  - Display assembler errors with line numbers
  - Syntax highlighting (optional, use react-syntax-highlighter or plain textarea)
- Accept: Can enter code, click Assemble, see errors or success message.

Task 2.5: CPUConsole component
- Create src/components/CPUConsole.tsx
- Display registers dynamically:
  - IAR, ACC, EXT (main registers)
  - XR1, XR2, XR3 (index registers)
  - Current Interrupt Level (0-5, or "None")
  - Carry, Overflow, Wait LEDs/indicators
- Poll /api/execution/status every 200ms for updates during execution
- Accept: Registers update in real-time during Step or Run.

Task 2.6: ControlPanel component
- Create src/components/ControlPanel.tsx
- Buttons:
  - RESET (calls resetCpu)
  - STEP (calls step, updates registers)
  - RUN (starts continuous execution)
  - STOP (halts execution)
- Disable buttons appropriately (e.g., STEP/RUN disabled while running)
- Accept: All controls functional and synchronized with backend state.

Task 2.7: Main Page integration
- Create src/pages/EmulatorPage.tsx combining:
  - AssemblerEditor (top or left panel)
  - CPUConsole (right panel showing registers)
  - ControlPanel (below or beside console)
- Layout: responsive, clear separation between editor and console
- Accept: Complete workflow works: Write code → Assemble → Step/Run → See registers update.

------------------------------------------------------------
Phase 3 — SignalR for Real-Time Updates (Priority: Medium)
------------------------------------------------------------

Task 3.1: SignalR backend
- Add Microsoft.AspNetCore.SignalR to Web API
- Create Hubs/EmulatorHub.cs:
  - Method: BroadcastCpuState(CpuStateDto)
- EmulatorService triggers hub broadcast on:
  - Each step during Run mode (throttle to ~10-50 updates/sec max)
  - Each manual Step
  - State changes (WAIT, interrupts)
- Accept: Hub reachable; sample broadcast works in Swagger/testing.

Task 3.2: SignalR frontend
- npm i @microsoft/signalr
- Create src/services/signalRClient.ts:
  - Connect to /hubs/emulator on app load
  - Subscribe to CpuState updates
  - Update React state via callback
- Replace polling in CPUConsole with SignalR push updates
- Accept: Registers update smoothly via push instead of polling.

------------------------------------------------------------
Phase 4 — Memory Viewer (Priority: Low, Optional Enhancement)
------------------------------------------------------------

Task 4.1: Memory API
- Add GET /api/memory/{address}?count=16
- Add PUT /api/memory/{address} (write single word)
- Accept: Can read/write memory via Swagger.

Task 4.2: MemoryViewer component
- Create src/components/MemoryViewer.tsx
- Show memory around IAR (±16 words)
- Display: Address | Hex | Octal | Decimal
- Highlight current IAR row
- Accept: Memory view updates as IAR changes.

------------------------------------------------------------
Phase 5 — Device Status (Priority: Low, Future Enhancement)
------------------------------------------------------------

------------------------------------------------------------
Phase 5 — Device Status (Priority: Low, Future Enhancement)
------------------------------------------------------------

Task 5.1: DeviceStatusDto
- Properties: DeviceCode, Name, Busy, InterruptActive, Details (optional).
- Accept: Compiles.

Task 5.2: DeviceController (read-only baseline)
- GET /api/devices
- GET /api/devices/{code}
- Note: Cpu should expose IEnumerable<IDevice> GetAttachedDevices().
- Accept: Returns empty or real device list depending on emulator exposure.

------------------------------------------------------------
Phase 6 — Additional Polish (Future)
------------------------------------------------------------

Task 6.1: Syntax highlighting in assembler editor
- Use react-syntax-highlighter or CodeMirror
- Highlight IBM 1130 assembly instructions, comments, labels
- Accept: Code is more readable.

Task 6.2: Breakpoint support
- Add breakpoints array to EmulatorService
- Stop execution when IAR matches breakpoint
- UI: Click line numbers to toggle breakpoints
- Accept: Can set breakpoints, execution stops correctly.

Task 6.3: Step Over / Step Into for subroutines
- Implement BSC (Branch and Store) awareness
- Step Over: run until return from subroutine
- Step Into: normal single step
- Accept: Buttons work for subroutine debugging.

Task 6.4: Load/Save programs
- Save assembler source to localStorage or file download
- Load previous programs
- Accept: User can persist work between sessions.

------------------------------------------------------------
Removed/Deferred Tasks (Lower Priority)
------------------------------------------------------------

The following from the original plan are now deferred:
- Detailed MemoryEditor with inline editing
- LoadProgram dialog for hex/binary
- Full device panel UI (2501 Card Reader UI, etc.)
- Memory block load APIs beyond assembler workflow
- Advanced memory inspection tools

These can be added later once the core assembler workflow is solid.
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
Acceptance Checklist (Updated for Assembler-First Focus)
------------------------------------------------------------

Phase 1 & 2 — Core Functionality (Highest Priority)
- [ ] S1130.WebApi builds and runs with Swagger and CORS
- [ ] EmulatorService (singleton) exposes Cpu with assembler
- [ ] Assembler endpoint: compile source code, return errors
- [ ] Execution endpoints: step/run/stop/status
- [ ] React app boots and connects to API
- [ ] AssemblerEditor: enter code, click Assemble, see errors
- [ ] CPUConsole: shows IAR, ACC, EXT, XR1-3, interrupt level
- [ ] Registers update dynamically (polling initially)
- [ ] ControlPanel: Reset, Step, Run, Stop buttons work
- [ ] Complete workflow: Write assembler → Assemble → Step/Run → See updates

Phase 3 — Real-Time Enhancement (Medium Priority)
- [ ] SignalR backend broadcasts CpuState on execution
- [ ] SignalR frontend replaces polling with push updates
- [ ] Registers update smoothly during continuous Run

Phase 4 — Optional Enhancements (Low Priority)
- [ ] MemoryViewer shows memory around IAR
- [ ] Memory updates as IAR changes

Phase 5 & 6 — Future Features (Lowest Priority)
- [ ] Device status panel
- [ ] Syntax highlighting in assembler editor
- [ ] Breakpoint support
- [ ] Step Over/Step Into for subroutines
- [ ] Load/Save programs (localStorage or file)

Quality (Ongoing)
- [ ] Backend tests for assembler and execution
- [ ] Frontend tests for key components
- [ ] Docs for dev setup and usage
- [ ] No breaking changes to S1130.SystemObjects