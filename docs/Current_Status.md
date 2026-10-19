# Current Status

Keep this file up to date with every change. Last updated: 2026-10-19 (after commit `4d6f9ed`).

See [ComputerRoom-Plan.md](./ComputerRoom-Plan.md) for the target design and milestones.

## Done

**CPU and instruction set**
- Full IBM 1130 instruction set, plus the assembler.
- Interrupt-level status words (ILSW) are faithful to the hardware:
  - Sense Device reset removes the device's request from the CPU immediately (through `DeviceBase.DeactivateInterrupt`).
  - BOSC resets only the level (`IDevice.HoldsInterrupt` keeps a still-held request queued).
  - `ICpu.RemoveInterrupt` and `Interrupt.Pending` support this.

**Devices**
- 2501 card reader (block mode).
- 1442 card read/punch (character mode). Level 0 (response) and level 4 (operation complete) interrupts are held separately, which fixed a single-slot bug.
- 2310 disk drive (`Device2310`): drive 0 is the built-in drive (device 4, ILSW `0x8000`); drives 1-4 use devices 0x11-0x14. Audited against `ibm1130_disk.c` (see "To do" for the gaps).
- `DiskImageCartridge`: in-memory 2315 cartridge whose image file is byte-compatible with the C simulator (203 cyl x 8 sectors x 321 little-endian words = 1,042,608 bytes, no header). Loads/saves files (atomic save, only when changed), creates blank packs (formatted or not), supports read-only. Verified against the real `dms.dsk`.
- `DeviceRunner`: background thread that calls `Run()` on every device so started operations complete while programs run; a failing device is recorded in `LastError` and does not stop the others. `EmulatorService` starts one per CPU (and replaces it on Reset). Device state flags touched by both threads are `volatile`.
- The `Cpu` registers the built-in 2310 as device 4 (`Cpu.BuiltInDisk`). Tested with a real `DiskImageCartridge`: seek, read, write, and save on unmount (`BuiltInDiskTests`). with a real `DiskImageCartridge`: seek, read, write, and save on unmount (`BuiltInDiskTests`).
- Console keyboard and printer (`DeviceConsoleKeyboard`, device 1): registered in `BuildDefaultDevices`; printed characters decode to text (`ConsolePrinterCodes`, `PrintedText`). Timing (15.5 chars/s) is not modelled.

**Tests and docs**
- 558 tests pass (535 SystemObjects, 23 WebApi) with `dotnet test .\S1130.sln --nologo -v q`.
- Functional-characteristics audit in `docs\FunctionalCharacteristics-Audit.md` (items 3.1/3.2 detail is stale).
- Computer-room plan written and committed.

**Reference review**
- Read the C reference simulator in `ForReferenceOnly` (ILSW model, XIO, console, 1442, disk). Findings are in the session handoff notes; no code was changed.

## To do

Milestones from the plan:
- **M1 Emulator foundations:**
  - **Device scheduling (found while registering the 2310):** nothing in the product calls `IDevice.Run()`; only unit tests do. `EmulatorService` just loops `ExecuteInstruction()`, so a disk/reader operation started by a running program never completes. Needs a device runner (separate thread, since `LetInstuctionsExecute` waits on the CPU's instruction count) or a rework of device timing. This blocks IPL.
  - 2310 gaps from the audit (in order): data-error sense bit (set by the error states below); error states (write-protect via `DiskImageCartridge.WriteRejected`, bad/zero word count, hard error); seek step width 8 bits not 9; seek/rotate timing (reference defers the read until after the next instruction); sector-address check. Done: next-sector count (bits 14-15, advances each sense) and not-ready while busy/empty, with carriage home hidden when busy.
  - Console switches (device 7) and level 5 / program stop; IPL from the 1442 (cold-start boot); 1132 printer; raw-column 1442 interface with column timing and punch output queue; observable CPU state for the UI.
- **M2 DMS bring-up:** trace against the reference simulator; confirm the DMS image's device configuration (`ForReferenceOnly\dmsr2v12`).
- **M3 Web back end:** per-user machine host, SignalR, library API, quota, deck format detection, PDF generation, Identity (TOTP, passkeys), admin invites.
- **M4 Computer-room UI:** clickable SVG room and zoom views.
- **M5 Packaging:** Dockerfile, volume layout, master DMS disk.

Other open items:
- Update the stale 3.1/3.2 detail in `FunctionalCharacteristics-Audit.md`.
- Decide the concurrency limit for active machines.
- Decide whether the console panel shows register lamps or status lamps only at first.
- Check licensing and rights for the DMS image and any fonts.

## Next steps

1. Add a device runner so started operations complete while the CPU runs, with tests.
2. Add the 2310 error states (and the data-error sense bit), with tests.
3. Then console switches (device 7), IPL and the 1132 printer.
4. Boot DMS in a console test.
