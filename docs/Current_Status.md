# Current Status

Keep this file up to date with every change. Last updated: 2026-10-19 (after commit `4d6f9ed`).

See [ComputerRoom-Plan.md](./ComputerRoom-Plan.md) for the target design and milestones.
A separate, simpler web site (boot DMS, submit a deck, get a printout, live lights, cabinet) is planned in [WebSite-Plan.md](./WebSite-Plan.md).

**Web site (`src/S1130.Site`), W1 in progress:** one private machine per browser (HttpOnly session cookie), `MachineHost` (CPU run thread, device runner, working disk copied from the master DMS image, 1403/1132 printout buffer), `MachineManager` (limit, idle stop), `/api/machine` endpoints (boot, deck, start/stop/reset, printout, tear off), and a plain HTML page with register/level/status lamps (polled every 200 ms), a deck box and a greenbar 1403 pane. Real multi-card jobs run: `// JOB` + `// DUP` + `*DUMPLET` prints the LET, and a `// FOR` compile prints its source listing and `END OF COMPILATION`. 14 tests in `tests/UnitTests.S1130.Site`. Run with `dotnet run --project src\S1130.Site`.
- **Fixed (1442 timing):** DMS used to stop after the first card. Its card routine issues Start Read and only then increments `$IOCT`. Our 1442 gave every column and the op-complete at once, so the completion decremented `$IOCT` before the increment. The `MDX` then hit zero and skipped into the middle of the next instruction (a WAIT at `/0396`). `Device1442` now delivers responses and operation complete from `Run()` after `EventInterval` instructions (default 100; at once while the CPU waits), like the printers. `EventInterval = 0` keeps the old during-the-XIO behaviour for unit tests.
- Note: a DMS boot prints two job pages. The first (`// JOB    000B`) is DMS's own cold-start job, which reads no card. The second is the deck's real `// JOB` card.
- **Fixed (speed):** jobs ran about 100x too slow because `DeviceRunner` sleeps between passes, and on Windows `Thread.Sleep(1)` lasts a ~15.6 ms timer tick. That cost each 1442 column (and each disk/printer step) one tick while DMS spun in its wait loop. `MachineHost` no longer uses a `DeviceRunner`: its run thread calls every device's `Run()` every 32 instructions, and at once when the CPU enters a wait. `Device2310.Run()` no longer blocks (it completes once 10 instructions have run, or while the CPU waits), and `DeviceBase.Run()` no longer yields. An assemble-and-run HELLORLD job now takes 0.2 s instead of 32 s.

## Done

**CPU and instruction set**
- Full IBM 1130 instruction set, plus the assembler.
- Interrupt-level status words (ILSW) are faithful to the hardware:
  - Sense Device reset removes the device's request from the CPU immediately (through `DeviceBase.DeactivateInterrupt`).
  - BOSC resets only the level (`IDevice.HoldsInterrupt` keeps a still-held request queued).
  - `ICpu.RemoveInterrupt` and `Interrupt.Pending` support this.

**Devices**
- 2501 card reader (block mode).
- 1442 card read/punch (character mode). Level 0 (response) and level 4 (operation complete) interrupts are held separately, which fixed a single-slot bug. Column responses and operation complete are timed by instruction count (`EventInterval`, delivered from `Run()`).
- 2310 disk drive (`Device2310`): drive 0 is the built-in drive (device 4, ILSW `0x8000`); drives 1-4 use devices 0x11-0x14. Audited against `ibm1130_disk.c` (see "To do" for the gaps).
- `DiskImageCartridge`: in-memory 2315 cartridge whose image file is byte-compatible with the C simulator (203 cyl x 8 sectors x 321 little-endian words = 1,042,608 bytes, no header). Loads/saves files (atomic save, only when changed), creates blank packs (formatted or not), supports read-only. Verified against the real `dms.dsk`.
- `DeviceRunner`: background thread that calls `Run()` on every device so started operations complete while programs run; a failing device is recorded in `LastError` and does not stop the others. `EmulatorService` starts one per CPU (and replaces it on Reset). Device state flags touched by both threads are `volatile`.
- The `Cpu` registers the built-in 2310 as device 4 (`Cpu.BuiltInDisk`). Tested with a real `DiskImageCartridge`: seek, read, write, and save on unmount (`BuiltInDiskTests`).
- 2310 now follows the reference for status and errors: next-sector count, busy/empty/hard-error read as not ready, carriage home hidden when busy, data-error bit (bad word count > 321, cleared by sense reset), write to a read-only cartridge is a permanent hard error (no interrupt, cleared by remount), zero word count locks the controller, short counts transfer only that many words, 8-bit seek step, and a `SectorAddressMismatch` flag. `ICartridge` gained `ReadOnly`.
- Console keyboard and printer (`DeviceConsoleKeyboard`, device 1): registered in `BuildDefaultDevices`; printed characters decode to text (`ConsolePrinterCodes`, `PrintedText`). Timing (15.5 chars/s) is not modelled.
- `ProgramLoad` (IPL): expands a card's 80 columns into core words 0-79 (same conversion as the C simulator) and starts at 0 (`FromCard`, `FromReader` for the 1442 then 2501 hopper); `ColdStart` loads the built-in DMS/APL cold-start card and sets the console entry switches to the boot drive. `Cpu.Step()` runs one instruction, or in a wait state only checks for an interrupt; taking an interrupt now ends the wait. A test boots the real `dms.dsk` cold start through the resident monitor load.
- DMS disk findings (`ForReferenceOnly\1130SW\dms.dsk`): built with 1442 reader + 1132 printer (1134 requested), core size 16K baked into the resident monitor (`$CORE` at core word /0E); it must be rebuilt (`loaddms` run on a 32K machine) for DMS to use 32K. The disk boots fine on our 32K CPU.

**Tests and docs**
- `Device1132` printer (device 6, level 1): port of the C `ibm1130_prt.c`. Core-word print buffer (words 32-39), 48-char drum read, emitter/hammer scan with print check, 66-line carriage tape with skip/space, text output via `PrintedText`/`TakePrintedText()`, offline/forms check, timed by instruction count. Registered by default, as is the 1442.
- `Device1403` printer (device 0x15, level 4): port of the C 1403 code. Block device: InitWrite prints 60 words (120 columns), Control spaces, Write skips to a carriage-tape channel; transfer/print/carriage-complete interrupts; same text output and timing model as the 1132. Registered by default. Skip-to-channel writes a form feed (new page). 12 tests.
- New `ForReferenceOnly\1130SW\dms.dsk` is built for 1403 + 1442 + 32K (core word /0E = /8000); the boot test now expects that. **DMS now boots, reads a `// JOB` card from the 1442 and prints the job header, cartridge table and `V2 M12 ACTUAL 32K CONFIG 32K` on the 1403** (test `Dms_With1403_...`).
- CPU bugs found by tracing DMS against the reference: shift with an index register (count = XR & 0x3F, type from the displacement; SLCA/SLC keep the XR high byte), and STX with no tag stores the address of the next instruction.
- Stand-alone A-Z 1132 program (`docs\TestAsm\alphabet1132.s1130`) is work in progress: on the reference SIMH it prints only some letters, on ours nothing; its test is skipped.
- Assembler limitations found: negative LDX immediates, no indirect LDX, BOSC never sets the reset bit.
- 561 SystemObjects + 14 1132 tests + 23 WebApi pass with `dotnet test .\S1130.sln --nologo -v q`.
- Functional-characteristics audit in `docs\FunctionalCharacteristics-Audit.md` (items 3.1/3.2 detail is stale).
- Computer-room plan written and committed.

**Reference review**
- Read the C reference simulator in `ForReferenceOnly` (ILSW model, XIO, console, 1442, disk). Findings are in the session handoff notes; no code was changed.

## To do

Milestones from the plan:
- **M1 Emulator foundations:**
  - ~~Device scheduling~~ done: `DeviceRunner` (see above) calls `Run()` on a background thread.
  - 2310 remaining: seek/rotate timing is still crude (a fixed `LetInstuctionsExecute(10)` before the transfer, which already lets the next instruction run first, as the APL boot card needs); no real seek or rotation delays.
  - ~~Console switches (device 7) and IPL~~ done. Still to do: level 5 / program stop and device 7 sense; register the 1442 in `BuildDefaultDevices` (done); 1132 printer (done); raw-column 1442 interface with column timing and punch output queue; observable CPU state for the UI.
- **M2 DMS bring-up:** trace against the reference simulator; confirm the DMS image's device configuration (`ForReferenceOnly\dmsr2v12`).
- **M3 Web back end:** per-user machine host, SignalR, library API, quota, deck format detection, PDF generation, Identity (TOTP, passkeys), admin invites.
- **M4 Computer-room UI:** clickable SVG room and zoom views. A first room illustration is in `docs/ui/computer-room.svg` (the original sketch is `docs/ui/room-mockup.png`): 1442 on the left, 1131 console with lamp panel and 2310 drive in the centre, 1403 on the right. Add `class="loaded"` to `#disk-drive` to show a cartridge. Lamps are still static and need binding to CPU state.
- **M5 Packaging:** Dockerfile, volume layout, master DMS disk.

Other open items:
- Update the stale 3.1/3.2 detail in `FunctionalCharacteristics-Audit.md`.
- Decide the concurrency limit for active machines.
- Decide whether the console panel shows register lamps or status lamps only at first.
- Check licensing and rights for the DMS image and any fonts.

## Next steps

1. Run the DMS boot further with the 1442 and 1132 registered and check output appears on the 1132.
2. Run the DMS boot further (monitor prompt / a DUP job) as a console test.
