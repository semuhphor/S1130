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
- 🚧 **2310 Disk Drive**: Partial implementation in progress
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
