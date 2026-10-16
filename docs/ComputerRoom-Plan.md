# Computer Room: Initial Plan

Goal: a Docker-deployable web site where each invited user has a private, virtual IBM 1130 "computer room". The user inserts a disk (DMS is provided), boots (IPLs) it, feeds card decks to the reader, and collects printouts and punched decks. Source prompt: `Prompts/NextSteps.crit.txt`.

## Decisions

| Area | Decision |
|---|---|
| Build order | Emulator first. Web features come after DMS boots in a console test. |
| Hosting | One ASP.NET container with one in-process 1130 per user. Idle machines are suspended; machine state is lost when the user leaves, but the disk is saved. The user must reboot on return. |
| Accounts | ASP.NET Core Identity: email/password, TOTP 2FA, passkeys. Invite-only; an admin creates accounts. |
| Storage | SQLite for metadata. Disks, decks and printouts are files on a mounted Docker volume. Per-user quota about 50 MB (admin adjustable). |
| Disks | A read-only master DMS disk ships with the site. Each user gets a private copy on first use, can reset it to the master, remove it, and create a new pack (blank or pre-formatted). Disk image format follows the reference simulator (`ForReferenceOnly\ibm1130_disk.c`) so existing DMS images load. |
| Devices | 1130 with console typewriter (1053) and console panel, 1132 printer, 1442 read/punch, 1131 disk drive (device code 4, 2315 cartridge; already implemented as drive 0 of `Device2310`). The 2501 is not needed for the first release. |
| I/O model | Devices only queue raw records: 80-column 12-bit Hollerith images for cards, raw print lines/carriage controls for the 1132, print words for the console. The web layer translates to and from text/PDF. |
| Card input | Text (primary) and binary decks. The site inspects the deck, suggests a format, and lets the user override. Everything is stored as raw 80-column images. |
| Output | "Complete Printout" and "Complete Punch Output" buttons ask for a name and save to the library. On a name clash: overwrite, cancel, or rename. Printouts render to PDF (greenbar style); punched decks export as text or binary. |
| Printer rendering | Design follows racingmars/virtual1403 (job-based interface: add line with/without line feed, new page, end job). **Design reference only**: it is GPLv3, so no code is copied. Layout must use 1132 geometry (120 columns), not 1403. A .NET PDF library is needed (QuestPDF or PDFsharp). |
| Front end | Existing React app, extended with SignalR for live device state. |
| UI | Drawn, clickable, zoomable SVG illustrations (not photos): room view (1130, 1442, 1132); console close-up with lamps, keys and switches the user can operate; disk door that opens to insert/remove a cartridge and power the drive; 1442 hopper/stacker; 1132 paper view. |

## Milestones

### M1. Emulator foundations (console tests only, no web)
1. Finish the 1131/2310/2311 disk. `Device2310` already covers the 1131 as drive 0 (device 4, level 2 ILSW `0x8000`) and 2310 drives 1-4 (devices 0x11-0x14, ILSW `0x4000`-`0x0800`, matching the reference simulator). Remaining: audit it against `ibm1130_disk.c` for seek/read/write/sense completeness, sector timing and interrupts, and image load/save in the reference format.
2. Console switches (device 7): data switches, run/stop/program-start state, level 5 handling (program stop / INT RUN).
3. IPL: cold-start from the 1442 (card boot loader as in `load_cr_boot`/`cr_boot` in `ibm1130_cr.c`), wait state, program load.
4. 1132 printer (`ibm1130_prt.c` as reference): raw line queue, carriage control and forms.
5. 1442 raw-column interface and column-by-column timing; punch output queue.
6. Observable CPU state for the UI: registers, run/wait, current interrupt level.

Acceptance: a console test boots the DMS image and runs a trivial job end to end.

### M2. DMS bring-up
Trace execution against the reference simulator, fix instruction/interrupt differences, and confirm the provided DMS image supports a 1442 as the IPL reader.

### M3. Web back end
Per-user machine host (start, suspend, drop after idle), SignalR hub, library API (disks, decks, printouts), quota enforcement, deck format detection and conversion, PDF generation, Identity with TOTP and passkeys, admin invite flow.

### M4. Computer-room UI
SVG room and zoom views, console panel, typewriter, disk door, 1442 hopper/stacker, 1132 paper, library screens, Complete Printout / Complete Punch Output dialogs.

### M5. Packaging
Dockerfile, volume layout, configuration, master DMS disk provisioning, backup notes.

## Risks / open items
- DMS image must match the device configuration (1442 reader/punch, 1132, 1131 disk); verify against `ForReferenceOnly\dmsr2v12`.
- The disk is mostly implemented; the gaps against DMS needs are not yet measured.
- Rights and licensing for the DMS disk image and any fonts.
- Concurrency limit on simultaneously active machines is not yet decided.
- Whether the console panel shows real register lamps or status lamps only in the first release is not yet decided.
- Console printer timing (15.5 chars/s) is not modelled yet.
