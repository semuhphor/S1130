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
- The `Cpu` registers the built-in 2310 as device 4 (`Cpu.BuiltInDisk`). Tested with a real `DiskImageCartridge`: seek, read, write, and save on unmount (`BuiltInDiskTests`).
- 2310 now follows the reference for status and errors: next-sector count, busy/empty/hard-error read as not ready, carriage home hidden when busy, data-error bit (bad word count > 321, cleared by sense reset), write to a read-only cartridge is a permanent hard error (no interrupt, cleared by remount), zero word count locks the controller, short counts transfer only that many words, 8-bit seek step, and a `SectorAddressMismatch` flag. `ICartridge` gained `ReadOnly`.
- Console keyboard and printer (`DeviceConsoleKeyboard`, device 1): registered in `BuildDefaultDevices`; printed characters decode to text (`ConsolePrinterCodes`, `PrintedText`). Timing (15.5 chars/s) is not modelled.
- `ProgramLoad` (IPL): expands a card's 80 columns into core words 0-79 (same conversion as the C simulator) and starts at 0 (`FromCard`, `FromReader` for the 1442 then 2501 hopper); `ColdStart` loads the built-in DMS/APL cold-start card and sets the console entry switches to the boot drive. `Cpu.Step()` runs one instruction, or in a wait state only checks for an interrupt; taking an interrupt now ends the wait. A test boots the real `dms.dsk` cold start through the resident monitor load.
- DMS disk findings (`ForReferenceOnly\1130SW\dms.dsk`): built with 1442 reader + 1132 printer (1134 requested), core size 16K baked into the resident monitor (`$CORE` at core word /0E); it must be rebuilt (`loaddms` run on a 32K machine) for DMS to use 32K. The disk boots fine on our 32K CPU.

**Tests and docs**
- 561 SystemObjects + 23 WebApi = 584 tests pass with `dotnet test .\S1130.sln --nologo -v q`.
- Functional-characteristics audit in `docs\FunctionalCharacteristics-Audit.md` (items 3.1/3.2 detail is stale).
- Computer-room plan written and committed.

**Reference review**
- Read the C reference simulator in `ForReferenceOnly` (ILSW model, XIO, console, 1442, disk). Findings are in the session handoff notes; no code was changed.

## To do

Milestones from the plan:
- **M1 Emulator foundations:**
  - ~~Device scheduling~~ done: `DeviceRunner` (see above) calls `Run()` on a background thread.
  - 2310 remaining: seek/rotate timing is still crude (a fixed `LetInstuctionsExecute(10)` before the transfer, which already lets the next instruction run first, as the APL boot card needs); no real seek or rotation delays.
  - ~~Console switches (device 7) and IPL~~ done. Still to do: level 5 / program stop and device 7 sense; register the 1442 in `BuildDefaultDevices`; 1132 printer; raw-column 1442 interface with column timing and punch output queue; observable CPU state for the UI.
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

1. Register the 1442 by default and add the 1132 printer.
2. Run the DMS boot further (monitor prompt / a DUP job) as a console test.
