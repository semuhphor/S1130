# S1130 Web Site: Plan

Goal: a web site where a visitor gets a private IBM 1130, boots DMS, feeds a card deck and reads the printout. Later it grows live console lights, a cabinet that keeps decks, disks and printouts, disk creation, and perhaps a new interactive OS/supervisor.

This is a separate project from the [Computer Room plan](./ComputerRoom-Plan.md). It reuses the emulator (`S1130.SystemObjects`) but not the existing `S1130.WebApi`.

## Decisions

| Area | Decision |
|---|---|
| Project | New ASP.NET Core project `src/S1130.Site` (.NET 10), with tests in `tests/UnitTests.S1130.Site`. It references `S1130.SystemObjects` only. |
| Front end | Static HTML, CSS and vanilla JavaScript served from `wwwroot`. No build step. The SignalR JS client is the only library. |
| Live updates | One SignalR hub. The server pushes machine snapshots (lights) and new printer lines. Commands also go through the hub. Uploads and downloads use plain HTTP. |
| Sessions | No accounts. An HttpOnly, SameSite=Strict cookie holds a random session id. Each session owns one private machine and one cabinet. |
| Machine life | The machine is created on first use. It is stopped after an idle period with no connected browser (default 30 min). On stop the mounted disk is saved and core state is lost. The user boots again on return. |
| Cabinet | Server side, under `Data/sessions/{id}/` (`decks/`, `disks/`, `printouts/`, `cabinet.json`). It is deleted after a longer inactivity period (default 7 days). Per-session quota (default 50 MB). |
| Master disk | A read-only DMS image from the site's disk library, `src/S1130.Site/Disks` (`Site:DiskLibrary`, `Site:MasterDisk`; names say how each disk was genned, e.g. `dms-r2v12-1403-1442-32k.dsk`). "New DMS disk" copies it into the cabinet. "New blank disk" uses `DiskImageCartridge.CreateBlank`. |
| Devices | The default `Cpu` devices: 1442 reader (decks go to its hopper), 1403 printer (printout), built-in 2310 (device 4) and console. |
| Decks | First release: text decks, one line per card, truncated to 80 columns, converted with `ConversionCodes.ToCard`. Binary decks come later. |
| Printout | Lines are pushed live as the 1403 prints (`Device1403.TakePrintedText`). "Tear off" saves the printout so far to the cabinet as text and clears the pane. Download as `.txt`. PDF/greenbar comes later. |
| UI (first) | One plain HTML page. Console panel with lamp rows (IAR, ACC, EXT, XR1-3, interrupt levels 0-5, Run, Wait, Carry, Overflow) and keys (Boot DMS, Start, Stop, Reset). 1442 hopper with "load deck" and a card count. 2310 with mount/unmount. 1403 printout pane. Cabinet list. SVG artwork comes later. |

## Machine host

`MachineHost` (one per session) owns `Cpu`, `DeviceRunner` and a run thread:

- The run loop calls `cpu.Step()` in batches. In the wait state it sleeps briefly so idle machines cost almost no CPU. A configurable throttle (default: unthrottled while running) protects a shared server.
- All CPU access goes through the host's lock. The hub never touches the `Cpu` directly.
- A sampler takes a snapshot about 20 times a second, sends it only when it changed, and only while a browser is connected. This gives "near-current" lights without slowing the CPU.
- `MachineManager` (singleton) maps session id to host, enforces the concurrency limit (`Site:MaxMachines`) and reaps idle hosts.

## Security basics

- Upload limits: deck size (e.g. 2,000 cards) and disk size (exactly the 1,042,608-byte image format).
- Cabinet item names are sanitized and stored under generated file names, so path traversal is not possible.
- A session can only reach its own cabinet and machine. The session id is never taken from the URL.
- Hub calls are rate limited, and boot/reset are debounced.

## Milestones

### W1. Boot and print (end to end, minimal UI)
New project, session cookie, `MachineHost`/`MachineManager`, Boot DMS (scratch copy of the master), load a text deck into the hopper, return the printout text. The page has basic buttons and a printout pane, polled at first.

Acceptance: a test and the page run a `// JOB` plus a small real job (e.g. `// DUP` `*LIST` or a FORTRAN compile-and-run) and show its printout.

Status: done in code and tests. The site, machine host, endpoints and page are built. Multi-card decks run: `// DUP` `*DUMPLET` prints the LET and a `// FOR` compile prints its listing (test `Boot_Dms_ReadsEveryCardOfAMultiCardDeck`). The fix was 1442 timing: the column interrupts and op-complete no longer arrive during the XIO. Still to do: run a real job through the browser page itself.

### W2. Live lights
SignalR hub, snapshot sampler, lamp panel bound to the snapshots, printout lines streamed live, Start/Stop/Reset keys.

### W3. Cabinet
Per-session storage: save, list, rename, delete and download decks and printouts. Upload decks. Load a cabinet deck into the hopper. Tear-off printouts into the cabinet. Quota and cleanup.

### W4. Disks
New DMS disk (from master), new blank disk (formatted or not), mount/unmount on the built-in drive, download/upload images, save on unmount and on idle stop.

### W5. Packaging
Dockerfile, data volume, configuration, master disk provisioning.

### Later
- SVG console and room art (see `docs/ui/computer-room.svg`), with lamps driven by the same snapshots.
- 1442 punch output to the cabinet, binary decks, PDF greenbar printouts, 1132 as an option.
- Accounts (would replace the cookie-only sessions).
- A new interactive OS/supervisor. The cabinet and blank-disk tools are the groundwork for it.

## Risks / open items

- DUP and FORTRAN compile jobs run. Compile-and-execute with output, the assembler and larger jobs have not been tried and may expose more emulator bugs.
- Licensing of the DMS image if the site is public.
- CPU cost per running machine, which affects the `MaxMachines` default and the throttle.
- How the end of a job is detected (hopper empty and CPU waiting) for "job done" feedback.
