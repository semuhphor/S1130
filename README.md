S1130
=====

## IBM 1130 Emulator in C#

This is an IBM 1130 emulator modified to build and run with .NET Core, compatible with Linux, Mac, and Windows. 

The emulator provides a complete IBM 1130 system simulation including:
- CPU instruction set implementation
- Device emulation (2501 card reader, 2310 disk drive)
- Memory management
- Interrupt handling

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- A C# compatible IDE or text editor

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/semuhphor/S1130.git
   cd S1130
   ```

2. Restore dependencies and build:
   ```bash
   dotnet build
   ```

#### Building in an IDE (Visual Studio, JetBrains Rider, VS Code)

- Open the solution file: S1130.sln
- Ensure the .NET 8.0 SDK is installed (see Prerequisites above).

Visual Studio (Windows/macOS):
- File > Open > Project/Solution..., select S1130.sln
- Build > Build Solution (Ctrl+Shift+B on Windows, Cmd+Shift+B on macOS)

JetBrains Rider (Windows/macOS/Linux):
- File > Open..., select S1130.sln
- Build > Build Solution (Ctrl+F9 on Windows/Linux, Cmd+F9 on macOS)

Visual Studio Code:
- Install the C# Dev Kit (or C#) extension if prompted
- File > Open Folder..., select the repository root (the folder containing S1130.sln)
- Press Ctrl+Shift+B (Cmd+Shift+B on macOS) to run the default build task, or open the Command Palette and run: .NET: Build
- Alternatively, use Terminal > New Terminal and run: dotnet build

### How to Run (and where is the UI?)

There is currently no end-user UI (GUI or TUI) included in this repository. The emulator is delivered as a class library (src/S1130.SystemObjects) with a comprehensive automated test suite (tests/UnitTests.S1130.SystemObjects). You have a few ways to "run" or interact with it today:

1) Run the test suite (recommended to verify everything works)
- From the repo root:
  ```bash
  dotnet test
  ```
- In JetBrains Rider: open S1130.sln, then right-click the solution or the UnitTests project and choose Run Unit Tests.

2) Execute a tiny sample via PowerShell (no UI, just CPU demonstration)
- There is a legacy script at the repo root: "Poweshell - 1130 cpu to add two numbers.ps1" (Windows PowerShell only). To run it on Windows PowerShell:
  ```powershell
  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
  . "./Poweshell - 1130 cpu to add two numbers.ps1"
  ```
  Note: This is a historical example and not a supported UI.

3) Explore the code or build your own host
- If you want a console app, create a new .NET console project and reference src/S1130.SystemObjects. Then write a small harness that instantiates the CPU and loads memory/programs. The tests in tests/UnitTests.S1130.SystemObjects are good examples of usage.

Planned UI/host options
- The ImplementationPlan mentions a future Web API host. That project is not yet present in this repository. Until it exists, there is no built-in UI.

### Working with Git in Rider (discard changes and switch branches)

If you want to discard local changes and/or switch to a different branch using the IDE, see:
- docs/Rider-Git-HowTo.md — step-by-step with screenshots’ equivalents and Rider menu paths.

Quick start in Rider:
- Discard changes: Commit tool window > Local Changes > select files or root > Rollback. Use Git > Repository > Clean… for untracked files.
- Switch to branch: Click branch name in the status bar > Branches popup > origin/feature/web-frontend > Checkout (fetch first if needed).

### Running Tests

Execute the comprehensive test suite:
```bash
dotnet test
```

The project includes 335+ unit tests covering CPU instructions, device operations, and system integration scenarios. Test execution typically completes in under 2 seconds.

#### Test Output
- **Passed/Failed/Skipped counts**: Summary of test execution results
- **Performance metrics**: The test suite includes performance benchmarks (e.g., "1M Instructions in 1000ms")
- **Coverage**: Tests validate instruction execution, device behavior, and edge cases

## Project Status

- ✅ **CPU Core**: Fully functional instruction set implementation
- ✅ **2501 Card Reader**: Complete with test coverage
- 🚧 **2310 Disk Drive**: Partial implementation in progress (missing Data Error flag and Read-Check behavior; Next-Sector bits not surfaced in Sense)
- 📋 **PowerShell Integration**: Legacy script available but untested

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:
- Code style and conventions  
- Testing requirements
- Pull request process

### Development Guidelines

- **Test-Driven Development**: All new code should include comprehensive unit tests
- **Documentation**: Public APIs should include XML documentation comments
- **Error Handling**: Exception-prone areas require robust error handling and clear failure modes

## Useful Links

- [Getting started with .NET Core](https://www.microsoft.com/net/core)
- [Unit testing with .NET Core](https://docs.microsoft.com/en-us/dotnet/articles/core/testing/unit-testing-with-dotnet-test)
- [IBM 1130 Documentation](https://en.wikipedia.org/wiki/IBM_1130) (Historical reference)

## License

This project maintains the original licensing terms. Please see the repository for specific license information.

---

Thanks,  
Bob Flanders and contributors


## Known incomplete feature and how to complete it

2310 Disk Drive (src/S1130.SystemObjects/Devices/Device2310.cs):
- Missing Data Error bit support and Read-Check behavior
  - Where: HandleInitRead(), Run() when _reading is true
  - Suggested approach:
    - When Read-Check (0x80) modifier is set during InitRead, do not transfer into memory. Instead, compare the sector buffer from cartridge with the CPU memory buffer starting at the Working Control Address (WCA) and length indicated by the word at WCA. If any word differs, set the Data Error bit in the Sense response. Always request the interrupt at completion to preserve sequencing.
    - Add a private const ushort DataError = 0x8000 in Device2310 and OR it into the Sense result only when a mismatch occurs. Ensure current tests are unaffected by making this path conditional on the Read-Check flag.
- Next-Sector bits in Sense not surfaced
  - Where: HandleSenseDevice()
  - Suggested approach:
    - Track the last accessed sector index (0–7) in a private field and, when a cartridge is mounted, report (lastSector + 1) & 0x7 in CpuInstance.Acc low two bits (NextSectorMask). Keep the existing values (Busy, AtCylZero, OperationComplete, NotReady) unchanged. If you add this, update tests that assert exact Sense equality or gate the new bits behind an opt-in flag until tests are updated.
- Guard rails and edge cases
  - Enforce sector bounds (0–7) and cylinder bounds via CylinderTracker. Fail fast or clamp if invalid input appears.
  - For writes, cap wc at 321 (already done) and ensure cartridge.ReadOnly returns an error path if implemented.

Code pointers:
- Device2310 methods to review/edit: HandleSenseDevice, HandleControl, HandleInitRead, HandleInitWrite, Run
- Interfaces: src/S1130.SystemObjects/Devices/ICartridge.cs
- In-memory implementation example: src/S1130.SystemObjects/Devices/Disks/MemoryCartridge.cs
- Tests covering current behavior: tests/UnitTests.S1130.SystemObjects/DeviceTests/Device2310Tests.cs

Testing plan:
- Add unit tests for Read-Check mismatch/match, Data Error bit set/clear, and Next-Sector bit reporting.
- Run: dotnet test

## Run locally

The project includes a backend Web API and an optional React frontend. Here are the steps to run both locally on Windows PowerShell.

Prerequisites:
- .NET 8 SDK (global.json pins 8.0.414)
- Node.js and npm (only required for the frontend)

Run the backend (Web API):

```powershell
dotnet restore
dotnet build
dotnet run --project src\S1130.WebApi\S1130.WebApi.csproj
```

By default the API listens on http://localhost:5000. You can configure the frontend to point to a different URL via the environment variable `REACT_APP_API_URL`.

Run the frontend (optional):

```powershell
cd web-frontend
npm ci      # or npm install
npm start
```

Build the frontend for production:

```powershell
cd web-frontend
npm run build
```

Run tests for the backend and frontend:

```powershell
dotnet test
cd web-frontend; npm test
```

Optional: use Docker to build images defined in `docker-compose.yml` (if present).
