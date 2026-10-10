# IBM 1130 Functional Characteristics (GA26-5881) — Structured Summary

**Source:** `ForReferenceOnly/functional/*.html` in the S1130 repo — an HTML conversion of the IBM manual (ibm1130.net).
**Method:** Every number, bit position, device code, and table below was read directly from the source text. Sections marked **[overview only]** are summarized from the table of contents and section headings but were not read in detail; consult the named HTML file for specifics.

> Notation: `/xxxx` = hexadecimal. Bits are numbered 0 (high-order, leftmost) to 15 (low-order).

---

## Part I — Introduction

### 1131 CPU models (Figure 3)

| Model | Core (16-bit words) | Storage cycle (µs) | Internal disk drive |
|---|---:|---:|---|
| 1A | 4,096 | 3.6 | No |
| 2A, 4A | 4,096 | 3.6 | Yes |
| 1B | 8,192 | 3.6 | No |
| 2B, 4B | 8,192 | 3.6 | Yes |
| 3B | 8,192 | 2.2 | Yes |
| 5B | 8,192 | 2.2 | No |
| 1C | 16,384 | 3.6 | No |
| 2C | 16,384 | 3.6 | Yes |
| 3C | 16,384 | 2.2 | Yes |
| 5C | 16,384 | 2.2 | No |
| 1D | 32,768 | 3.6 | No |
| 2D | 32,768 | 3.6 | Yes |
| 3D | 32,768 | 2.2 | Yes |
| 5D | 32,768 | 2.2 | No |

- Machine cycle time = storage cycle time for Models 1, 2, 3, 5. For Model 4 the machine cycle is **5.85 µs**.
- Models 2, 3, 4 include a single internal disk drive using the IBM 2315 cartridge (512,000 words).
- Console printer (part of the CPU) prints up to **15.5 characters/second**.

### Attachable I/O (Introduction, "I/O Devices")

- **Cards:** 1442 Card Read Punch Model 6/7; 1442 Card Punch Model 5; 2501 Card Reader Model A1/A2
- **Disk:** Single Disk Storage (in 1131 Models 2/3/4); 2310 Model B1/B2 (up to 4 drives); 2311 Models 11/12 (up to 2 drives) — a system has 2310s *or* 2311s, not both
- **Printers:** Console printer; 1132 Model 1/2; 1403 Model 6/7
- **Paper tape:** 1134 Reader; 1055 Punch
- **Graphic:** 2250 Display Unit Model 4; 2285 Display Copier (no program control)
- **Other:** 1231 Optical Mark Page Reader Model 1; 1627 Plotter Model 1/2; Synchronous Communications Adapter (BSC/STR to System/360 via 2701/2703, another 1130, 2770, 2780, etc.); IBM System/7 (5010 Processor Module)

### Programming systems
FORTRAN, RPG, Assembler, Disk Monitor System Version 2, Card/Paper Tape Programming System.

---

## Part II — CPU Functional Characteristics (CPU.html)

### Data formats
- Word = 16 bits, positions 0–15; bit 0 is sign for arithmetic.
- **Single precision:** sign + 15 bits; range −32,768 … +32,767. Negatives in two's complement.
- **Double precision:** 32 bits across two consecutive words, sign in bit 0 of the first; range −2,147,483,648 … +2,147,483,647. The leftmost word **must be at an even address** for double-precision instructions. If an odd address is given, the odd word is used *twice*.
- Only one sign bit in 32 — the accumulator holds the high-order half, the extension the low-order half.
- **No fixed internal character code.** All conversion (card code, printer code, EBCDIC, etc.) is done by program. EBCDIC is in Appendix A for reference.

### Core-storage addressing
| Capacity | Max address | Address bits (IAR/SAR effective size) |
|---:|---|---:|
| 4,096 | `/0FFF` | 12 |
| 8,192 | `/1FFF` | 13 |
| 16,384 | `/3FFF` | 14 |
| 32,768 | `/7FFF` | 15 |

- **Wraparound:** high-order address bits beyond the system's capacity are *ignored*. On a 4K system `/1FFF`, `/4FFF`, etc. all address `/0FFF`. Max+1 wraps to `/0000`.

### Reserved core-storage locations
| Address | Purpose |
|---|---|
| `/0001` | Index register 1 (XR1) |
| `/0002` | Index register 2 (XR2) |
| `/0003` | Index register 3 (XR3) |
| `/0008`–`/000D` | Interrupt vectors, levels 0–5 (one word per level) |
| `/0020`–`/0027` | 1132 Printer scan field (decimal 32–39) |

### Instruction formats

**Short (F=0), one word:**

| Bits | Field |
|---|---|
| 0–4 | Op code |
| 5 | F = 0 |
| 6–7 | Tag (T) |
| 8–15 | Displacement (8-bit, signed) — except shifts, where bits 8–9 select the shift type |

**Long (F=1), two words:**

| Bits | Field |
|---|---|
| 0–4 | Op code |
| 5 | F = 1 |
| 6–7 | Tag |
| 8 | IA (indirect address) bit |
| 9 | BOSC bit (BSC/BSI) |
| 10–15 | Modifiers (conditions for BSC/BSI) |
| 16–31 | Address field |

### Effective-address generation

**Short format** — displacement (sign-extended from bit 8, range −128…+127) is added to the register selected by T:

| T | Register | EA |
|---|---|---|
| 00 | IAR | IAR + Disp (IAR already points to the word *after* the instruction) |
| 01 | XR1 | XR1 + Disp |
| 10 | XR2 | XR2 + Disp |
| 11 | XR3 | XR3 + Disp |

Short format is **direct addressing only**. Register contents are treated as unsigned.

**Long format, direct (IA=0):**

| T | EA |
|---|---|
| 00 | Address field |
| 01/10/11 | XRn + Address field |

The IAR is *not* used in long-format EA generation.

**Long format, indirect (IA=1):** compute the direct address as above; the word *at that address* is the EA.

**Exceptions (Figure 11):**
- **LDS:** bits 14/15 set Carry/Overflow — no address.
- **LDX:** short → sign-extended disp loaded into IAR/XRn; long direct → address field loaded; long indirect → contents of address loaded.
- **MDX:** short → sign-extended disp added to IAR/XRn; long T=00 → disp added to word at address; long T≠00 → address field (direct) or its contents (indirect) added to XRn.
- **Shifts:** count = low 6 bits of displacement (T=00) or of XRn.
- **STX:** short → EA = IAR + disp; long → address field / contents thereof.
- **WAIT:** displacement unused.

### Program registers and indicators
- **IAR** — 12/13/14/15 bits effective; points to next instruction.
- **XR1–XR3** — core locations `/0001`–`/0003`.
- **ACC** — 16 bits; arithmetic, logical, shift, and I/O status (ILSW/DSW land here).
- **EXT** — 16 bits, logically to the right of ACC; used by AD, D, LDD, M, RTE, SLT, SLC, SRT, STD, SD.
- **Carry / Overflow indicators** — see Figure 13 below.

### Carry and Overflow effects (Figure 13)
| Instr | Carry | Overflow |
|---|---|---|
| A, AD | Reset before; set on carry out of ACC bit 0 | Set if sum exceeds capacity; **once on, not changed** |
| S, SD | Reset before; set on borrow beyond bit 0 | Same as A |
| D | Not affected | Set on divide-by-zero or quotient overflow |
| M | Not affected | Not affected |
| BSC/BOSC | Not affected when tested | **Reset when tested** |
| BSI | Not affected (short); not reset if tested (long) | Not affected (short); reset when tested (long) |
| LDS | ← bit 14 of instruction | ← bit 15 of instruction |
| STS | Set to 0 | Set to 0 |
| SLA, SLT | = last bit shifted out of ACC bit 0 | Not affected |
| SLC, SLCA | 1 if terminated by a 1 reaching ACC bit 0; 0 if terminated by count | Not affected |
| All others | Not affected | Not affected |

### Miscellaneous (non-program) registers
AFR / D-register (16-bit arithmetic factor), CCC (6-bit cycle-control counter — counts cycles, controls shifts), OP (5-bit op code), SAR / M-register (storage address, 12–15 bits), SBR / B-register (16-bit storage buffer), TAG (3-bit: F + T), TAR / U-register (16-bit temporary accumulator used during EA computation).

---

## Part III — Instruction Set (CPUinstructions.html + individual pages)

### Op codes and execution times (Figure 15, 1131 Models 1 and 2, µs)

| Class | Instr | Mnemonic | Op code | Short T=00 | Short T≠00 | Long T=00 | Long T≠00 |
|---|---|---|---|---:|---:|---:|---:|
| Load/Store | Load ACC | LD | `11000` | 7.6 | 11.2 | 10.8 | 14.8 |
| | Load Double | LDD | `11001` | 11.2 | 14.9 | 14.4 | 18.0 |
| | Store ACC | STO | `11010` | 7.6 | 11.2 | 10.8 | 14.8 |
| | Store Double | STD | `11011` | | | | |
| | Load Index | LDX | `01100` | | | | |
| | Store Index | STX | `01101` | | | | |
| | Load Status | LDS | `00100` | | | | |
| | Store Status | STS | `00101` | | | | |
| Arithmetic | Add | A | `10000` | | | | |
| | Add Double | AD | `10001` | | | | |
| | Subtract | S | `10010` | | | | |
| | Subtract Double | SD | `10011` | | | | |
| | Multiply | M | `10100` | | | | |
| | Divide | D | `10101` | | | | |
| | AND | AND | `11100` | | | | |
| | OR | OR | `11101` | | | | |
| | Exclusive OR | EOR | `11110` | | | | |
| Shift left (short only) | NOP / SLA / SLCA / SLT / SLC | | `00010` | 3.6 (NOP) | | — | — |
| Shift right (short only) | SRA / SRT / RTE | | `00011` | | | — | — |
| Branch | Branch & Store IAR | BSI | `01000` | 7.6 | 11.2 | 10.8 (if taken) | 14.8 |
| | Branch/Skip on Cond. | BSC | `01001` | 3.6 | 3.6 | 7.2 (if taken) | 11.2 |
| | Modify Index & Skip | MDX | `01110` | 4.5 avg / 9.9 max | 11.2 / 16.2 | 18.5 / 23.4 | 18.5 / 23.4 |
| | Wait | WAIT | `00110` | 3.6 | 3.6 | — | — |
| I/O | Execute I/O | XIO | `00001` | 11.2 | 14.8 | 14.4 | 18.4 |

Notes from Figure 15: indirect addressing adds one storage cycle (3.6 µs); shifts cost one or two storage cycles + 0.45·(N−4) µs (N > 4); **all unassigned op codes execute as WAIT**; XIO read/write adds one storage cycle. The manual has separate timing tables for Models 3/5 and Model 4 (same op codes). Blank timing cells above were not transcribed — see the source.

### Key instruction semantics

**Shift class (op `00010` left, `00011` right) — short format only.** Bits 8–9 select the variant: left `00`=SLA, `01`=SLCA, `10`=SLT, `11`=SLC; right `00`/`01`=SRA, `10`=SRT, `11`=RTE. Shift count = low 6 bits of the displacement (T=00) or of the selected XR (T≠00). Left shifts fill with zeros. SRA fills zeros (logical); SRT propagates ACC bit 0 (arithmetic); RTE rotates EXT bit 15 back into ACC bit 0. SLA with T=00 and count 0 = NOP.

**SLCA / SLC (shift-left-and-count).** With T≠00 the count is taken from XRn, shifting stops when a 1 reaches ACC bit 0 *or* the count reaches zero, and the **residual count is written back to XRn**. Count initially 0 → no-op. If ACC bit 0 is already 1 and count ≠ 0 → Carry set, XRn bits 8–9 reset to 00, count unchanged, no shift. With T=00 behaves exactly as SLA/SLT. Designed for scanning ILSW/DSW bits to index a jump table.

**M (multiply).** 16-bit multiplicand (storage) × 16-bit multiplier (ACC) → 32-bit product in ACC:EXT, sign in ACC bit 0. Largest product 2³⁰ (= (−2¹⁵)²).

**D (divide).** 32-bit dividend in ACC:EXT ÷ 16-bit divisor (storage) → quotient in ACC, remainder in EXT. Remainder sign = original dividend sign. Overflow set on divide-by-zero or quotient outside −2¹⁵…+2¹⁵−1 (ACC/EXT then undefined). Carry unaffected. A 16-bit dividend must first be shifted right 16 into EXT (e.g., `SRT 16`).

**BSC / BOSC (op `01001`).** Modifier bits test, when = 1:

| Bit | Tests |
|---|---|
| 10 | ACC zero |
| 11 | ACC negative |
| 12 | ACC plus (> 0) |
| 13 | ACC even |
| 14 | Carry indicator **off** |
| 15 | Overflow indicator **off** |

- **Short:** if *any* specified condition is true → **skip** the next word (which must be a short instruction). No conditions → no skip.
- **Long:** branch to EA only if **none** of the specified conditions is true. No conditions → unconditional branch. (Opposite sense from short!)
- **Bit 9 = 1 → BOSC:** resets the current interrupt priority level, **but only if the branch/skip actually occurs**.
- Indirect (bit 8) with EA = EA of a prior BSI gives subroutine/interrupt return.
- Overflow is reset when tested; Carry is not; ACC unchanged.

**BSI (op `01000`).** Stores the IAR (address of following instruction) at EA, then branches to EA+1. Long form can be conditional on the same modifier bits (bits 10–15) with the same "branch if none true" rule.

**MDX (op `01110`).** Short: sign-extended displacement added to IAR (T=00; no skip possible, any nonzero disp = branch) or XRn. Long T=00: displacement added to the word at the address; long T≠00: address field (or its contents if IA=1) added to XRn. **Skip one word** if the modified location (not IAR) changes sign or becomes zero. Carry/Overflow unaffected.

**LDS / STS.** LDS: Carry ← bit 14, Overflow ← bit 15 of the instruction itself. STS stores status into the addressed word and then resets both indicators.

**WAIT (op `00110`).** Halts the CPU until an interrupt (or console start). All unassigned op codes are WAIT.

**XIO (op `00001`).** EA must be **even**; it points to a two-word IOCC. **ACC is destroyed** by IOCC analysis — save it first. Carry/Overflow unaffected.

### IOCC format (two words at an even address)

| Word | Bits | Field |
|---|---|---|
| 1 | 0–15 | **Address** — storage word to transfer (direct program control) or, for cycle-steal devices, the word-count/table address |
| 2 | 0–4 | **Device code** (5 bits) |
| 2 | 5–7 | **Function code** (3 bits) |
| 2 | 8–15 | **Modifiers** (device-specific) |

**Function codes:**

| Code | Function |
|---|---|
| `000` | Not used |
| `001` | **Write** — one word from storage to device (address = storage word) |
| `010` | **Read** — one word from device to storage |
| `011` | **Sense Interrupt** — ILSW of the level being serviced → ACC (no device code needed) |
| `100` | **Control** — device-specific action using modifiers/address |
| `101` | **Initiate Write** — start cycle-steal output |
| `110` | **Initiate Read** — start cycle-steal input |
| `111` | **Sense Device** — DSW of addressed device → ACC; modifier bit 15=1 resets the device's highest-level interrupt, bit 14 the next, etc. A device not in the system returns all zeros. |

**Device codes:**

| Code | Device |
|---|---|
| `00001` | Console keyboard and console printer |
| `00010` | 1442 Card Read Punch |
| `00011` | 1134 Paper Tape Reader / 1055 Paper Tape Punch |
| `00100` | CPU single disk storage |
| `00101` | 1627 Plotter |
| `00110` | 1132 Printer |
| `00111` | Console entry switches, program stop key, interrupt-run mode |
| `01000` | 1231 Optical Mark Page Reader |
| `01001` | 2501 Card Reader |
| `01010` | Synchronous Communications Adapter |
| `01100` | IBM System/7 |
| `10001`–`10100` | 2310 drives 1–4, or 2311 drive 1 disks 1–4 |
| `10101` | 1403 Printer |
| `10110` | 2311 drive 1, disk 5 |
| `10111` | 2311 drive 2, disks 1–5 (selected by modifier bits 9–11) |
| `11001` | 2250 Display Unit |

**Transfer methods:** *Direct program control* — program must update the IOCC address and any count itself for each word (1442, console, paper tape, plotter…). *Cycle steal* — device accesses core directly after an Initiate Read/Write; address/count updated by hardware (single disk, 1132, 2501, and everything on the Storage Access Channel).

---

## Part IV — I/O Interrupts (IOInterrupts.html)

### Levels, vectors, devices

| Level | Vector | Devices |
|---:|---|---|
| 0 (highest) | `/0008` | 1442 column read / punch response |
| 1 | `/0009` | 1132 Printer; SCA |
| 2 | `/000A` | Disk storage; Storage Access Channel |
| 3 | `/000B` | 1627 Plotter; 2250; SAC; System/7 |
| 4 | `/000C` | 1442 operation complete; keyboard; console printer; 1134; 1055; 2501; 1403; 1231; SAC |
| 5 (lowest) | `/000D` | Console program-stop key and interrupt-run mode; SAC |

### Mechanism
1. Device raises a request; it is held in the device's DSW until accepted.
2. Current instruction completes. The request is accepted if no equal-or-higher level is in progress.
3. CPU forces a **BSI indirect via the vector word** for that level: IAR (address of the next mainline instruction) is stored at the word the vector points to; execution continues at that word + 1.
4. Subroutine saves registers, issues **XIO Sense Interrupt** (`011`) to get the ILSW for its level (levels 1–5 only — level 0 has only the 1442 so no ILSW is needed), finds the device (e.g., with SLCA), issues **XIO Sense Device** (`111`) with the reset modifier to get the DSW and clear the request, services it.
5. Restores registers and returns with **BOSC indirect** through the subroutine's first word. BOSC resets the level so equal/lower requests can be recognized.

ILSW level 1 example: bit 0 = 1 → 1132; else SCA. Level 4: bit 1 = console, bit 2 = 1442, bit 3 = 2501, bit 0 = paper tape (from the Figure 17 sample routine).

### Level 5 specifics
Program-stop key or INT RUN mode cause level 5; ILSW bit 0 is set. Sense Device must use device code `00111`. After BOSC from a *program-stop* interrupt, another level-5 request is raised immediately and persists until START or RESET is pressed. From an *interrupt-run* interrupt, BOSC simply resets level 5.

### Cycle-steal priority (OIO.html)
CPU level 0 = single disk; level 1 = SAC/2250 (subdivided by 1133 multiplexer: 2310/2311 drives 1–4 at mux 0–3, SAC II/2250 at mux 6, 1403 at mux 7); level 2 = 1132; level 3 = 2501.
Service deadlines: single disk one cycle every 27.8 µs; 2311 every 16 µs; 1403 every 11 µs (M7) / 18 µs (M6) but fully buffered.

---

## Part V — Console (Console.html)

### Console printer (device `00001`)
- Up to 15.5 char/s. One **Write** IOCC per character. Word format: bits 0–7 = character; bit 6 = 0 lower case / 1 upper case; bit 7 = 0 data / 1 control (space, tab, CR, etc.); bits 8–15 unused.
- Each completed print/control raises **level 4**; ILSW bit 1 = console.
- DSW (Figure 22, shared with keyboard): bit 0 printer response; bit 4 printer busy (writes while busy are **silently ignored**); bit 5 printer not ready.

### Keyboard (device `00001`)
- Sends card-code bit patterns (e.g., `A` = `1001 0000 0000 0000`, same as 1442 image).
- Protocol: XIO **Control** selects the keyboard (KB SELECT light on) → operator presses key → **level 4** interrupt, DSW bit 1 (keyboard response) → XIO **Read** stores the character and deselects. Repeat Control for each character. Read when not selected stores nothing.
- Function keys: **INT REQ** → level 4 interrupt with DSW bit 2 (and bit 3 reports Console/Keyboard switch position); **EOF** → word with only bit 12 set; **← backspace** → bit 13; **ERASE FIELD** → bit 14; **NUM** shift; **REST KB** unlocks.
- DSW: bit 1 keyboard response, bit 2 interrupt request, bit 3 keyboard/console switch, bit 6 keyboard busy (Control issued, Read not yet). Sense Device modifier bit 15 resets all responses.

### Console entry switches / level-5 DSW (device `00111`)
- 16 toggle switches. XIO **Read** with device `00111` reads them into the addressed word.
- Sense Device `00111` returns the level-5 DSW: bit 0 = program stop key pressed (also set for interrupt-run).

### Mode switch (7 positions)
SS (single clock step), SMC (single memory cycle), **INT RUN** (level-5 interrupt after every mainline instruction — for trace), **RUN**, **SI** (single instruction), **DISP** (display word at IAR, IAR++), **LOAD** (load CES word at IAR, IAR++; LOAD IAR key loads IAR from CES).

### Function switches
Program Start, Program Stop (→ level 5), IMM STOP (immediate halt, I/O may lose data), Reset (all registers/triggers/indicators), Load IAR, **Program Load** (reads first card or paper-tape record into storage from `/0000` and starts there), Console/Keyboard, Emergency Pull, Power, Alarm Off (SCA). Lights: Disk Unlock, File Ready, Run, Parity Check, KB Select, Forms Check.

---

## Part VI — Disk Storage (DiskStorage.html)

### Geometry (2315 cartridge / single drive / 2310)
| | Bits | Data words | Sectors | Tracks | Cylinders |
|---|---:|---:|---:|---:|---:|
| Word | 16 | | | | |
| Sector | 5,120 | **320** (321 physical; word 0 = sector address in DMS) | | | |
| Track | 20,480 | 1,280 | 4 | | |
| Cylinder | 40,960 | 2,560 | 8 | 2 | |
| Disk | 8,192,000 | **512,000** | 1,600 | 400 | **200** (+3 alternates = 203 positions) |

- Sectors 0–3 upper surface, 4–7 lower. 1,500 rpm (40 ms/rev, 20 ms avg latency). 27.8 µs/word. Sector ≈ 30 ms avg; full cylinder 100 ms. Inter-sector gap > 235 µs (+27.8 µs per word short of 321).
- Seek: 2-cylinder increments at 15 ms each + 22.5 ms settle: **t = 7.5·N + 22.5 ms** (N rounded up to even).
- Each word stored with 4 check/space bits; **modulo-4** bit-count check on read.
- **2311** (1316 pack, 10 surfaces): Model 11 = 5 logical disks (2,560,000 words), Model 12 = 3 (1,536,000). 16 µs/word; 12.5 ms avg latency; seek 25/75/135 ms min/avg/max. Seek happens on the next read/write, not on Control.

### IOCCs (device codes `00100`, `10001`–`10111`)
- **Initiate Read `110`:** address word = **WCA** (word-count address). Word at WCA = count; data goes to WCA+1…. Modifier bits 13–15 = sector (0–7); **bit 8 = 1 → Read-Check** (verify modulo-4 only, no transfer). Max 321 words/command. Operation-complete interrupt when done. Word count 0 is illegal (needs DC reset).
- **Initiate Write `101`:** same WCA convention; short counts are zero-filled to 321. Follow with a read-check.
- **Control `100`:** address word = number of cylinders; **modifier bit 13 = 0 forward (toward center), 1 backward**. Zero cylinders = no-op, no interrupt. (2311: only updates the arm register.)
- **Sense Device `111`:** DSW → ACC; modifier bit 15 resets operation-complete and data-error.

### DSW (Figure 37 — bits per the text)
Operation Complete (interrupt, **level 2**, the only disk interrupt); Data Error (modulo-4 error, over-run at sector pulse, seek incomplete [2311], write-select error, power unsafe); Disk Not Ready (bit 0, also sets bit 2 on write-select error); Disk Busy; Carriage Home (cylinder 0); Sector Count (next sector number, low bits).

---

## Part VII — Punched-Card Devices (Cards.html)

### IBM 1442 Card Read Punch (device `00010`, levels 0 and 4)
- Models: 6 (300 cpm read, 80 col/s punch), 7 (400 cpm, 160 col/s), 5 (punch only, 160 col/s). Serial, column by column; one path — read station then punch station.
- **Data coding:** card rows 12,11,0,1…9 → word bits 0–11; bits 12–15 = 0. 1 = hole.
- **Program Load:** load-card data is *split* (Figure 40) into 80 words at `/0000`–`/004F`, then IAR = 0.
- **IOCCs:** Read `010` (one column → addressed word); Write `001` (addressed word → punch one column); Control `100` with modifiers **bit 13 Start Read, bit 14 Feed Cycle, bit 15 Start Punch, bit 8 Stacker Select** (don't combine 13/14/15); Sense Device `111` — **bit 15 resets level 0, bit 14 resets level 4**.
- **Interrupt protocol (direct program control):**
  - *Level 0* — **Read Response** per column: must be serviced within **800 µs (M6) / 700 µs (M7)**; first column arrives 28.4 / 23.8 ms after Start Read. **Punch Response** per column: data must be supplied within **300 µs**.
  - *Level 4* — **Operation Complete**: 20.6 / 15.4 ms after column 80 (read); 12.5 / 6.25 ms after the end-punch column (punch). Also forced on hopper/feed/transport/registration/punch-check errors.
- **Punch termination:** punching stops at the first data word with **bit 12 = 1** (end-punch). Missing it punches past column 80 with no error indication.
- **DSW non-interrupt bits:** Not Ready, Busy, Last Card (column 80 of last card passed and hopper empty), Error Check.
- To hold rated speed, next Start Read must arrive within 35 ms (M6) / 25 ms (M7) of operation complete.

### IBM 2501 Card Reader (device `01001`, level 4)
- Model A1 600 cpm (100 ms cycle); A2 1000 cpm (60 ms cycle). Columns read twice and compared.
- **Cycle-steal:** IOCC address → data table; **first word = word count in bits 9–15 (max 80 = /50)**; data lands in following words. Word count 0 = feed only.
- **IOCCs:** Initiate Read `110`; Sense Device `111` (bit 15 resets Last Card and Operation Complete).
- **DSW:** Not Ready/Busy, Busy, **Operation Complete** (the only interrupt; after column 80; **ILSW 4 bit 3**), Last Card, Error Check (feed/read check).
- CPU busy only 288 µs per card after initiation. Same load-mode card split as 1442.

---

## Part VIII — Printers (Printers.html)

### IBM 1132 Printer (device `00110`, level 1, cycle-steal level 2)
- Model 1: 80 lpm alphameric / 110 lpm numeric; Model 2: 40 / 55. 120 positions, 48-character wheels, 6 or 8 lpi.
- **Scan-field protocol:** all 120 print wheels rotate in sync. For each of 48 character cycles the program reads the current emitter character, compares it against the 120-character line image, and sets a 1 bit in the **scan field at `/0020`–`/0027`** (7 full words + 8 bits = 120 positions) for every wheel that should fire. The printer cycle-steals the scan field.
- Carriage: tape channels 1–6, 9, 12 programmable; skip speed 10 in/s; skips must be ≥ 4 lines.

### IBM 1403 Printer (device `10101`, level 4, via 1133 SAC multiplexer) **[detail from headings]**
- Fully buffered: fixed 120-character / 60-word print record transferred by **Initiate Write `101`** (cycle steal); separate Control for space/skip; carriage control tape; DSW includes channel 9, channel 12, carriage busy, printer busy, transfer-complete and print-complete interrupts. Model 6 ≈ 340 lpm.

---

## Part IX — Other Devices **[overview only — see source files]**

- **Paper tape** (`PaperTape.html`, device `00011`, level 4): 1134 reader / 1055 punch, PTTC/8 code, program load from 1134, direct-program-control Read/Write per character.
- **1627 Plotter** (`Plotter.html`, device `00101`, level 3): incremental pen-step commands via Write, DSW busy/response.
- **1231 Optical Mark Page Reader** (`OMPR.html`, device `01000`, level 4): data-sheet format, mark recognition, program control sheet, field checking.
- **2250 Display Unit Model 4** (`DisplayUnit.html`, device `11001`, level 3, cycle-steal via SAC): graphic and character modes, display order program in core, 2285 copier. Character mode can consume ~80% of CPU cycles on a 3.6 µs system.
- **Storage Access Channel** (`SAC.html`): SAC / SAC II (1133) cycle-steal channel; priority multiplexing table above.
- **Synchronous Communications Adapter** (`SCA.html`, device `01010`, level 1): BSC and STR modes, half-duplex, timers, transparency, IOCC function/modifier set, DSW, programming timing.
- **IBM System/7** (`System7.html`, device `01100`, level 3): bidirectional interrupts and IOCCs.
- **Appendix A** (`AppendixA.html`): character code tables — card (Hollerith) code, console printer tilt/rotate codes, 1132 and 1403 printer codes, PTTC/8, EBCDIC.
- **Glossary**, **Index**, **ChangeLog** (conversion notes).

---

## Quick cross-reference for emulator work

| Topic | Source file |
|---|---|
| Addressing rules, reserved locations | `CPU.html` |
| Op codes + timings | `CPUinstructions.html` |
| BSC/BOSC condition bits | `BSCBOSCinstr.html` |
| MDX skip rule | `MDXinstr.html` |
| Shift-and-count residual semantics | `SLCAinstr.html`, `SLCinstr.html` |
| IOCC layout, function codes, device codes | `XIOinstr.html` |
| Interrupt vectors, BSI/BOSC sequence, level 5 | `IOInterrupts.html` |
| Console keyboard/printer protocol, mode switch | `Console.html` |
| Disk geometry, WCA convention, seek formula | `DiskStorage.html` |
| 1442 per-column timing, end-punch bit | `Cards.html` |
| 2501 word-count buffer | `Cards.html` |
| 1132 scan field | `Printers.html` |
| Cycle-steal / interrupt priority, service deadlines | `OIO.html` |
