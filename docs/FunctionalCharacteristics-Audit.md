# S1130.SystemObjects — Conformance Audit vs. IBM 1130 Functional Characteristics

**Date:** 2026-10-10
**Scope:** `src/S1130.SystemObjects` — CPU, instruction set, interrupt mechanism, devices (1442, 2501, 2310, console).
**Reference:** [IBM1130-Functional-Characteristics-Summary.md](./IBM1130-Functional-Characteristics-Summary.md) (verified against `ForReferenceOnly/functional/*.html`).
**Baseline:** all 457 SystemObjects unit tests passed at commit `e35f013`. Several findings below were *encoded in the tests* and required updating test expectations.

> **Status after remediation (same day):** every HIGH and MED item below, plus the LOW items marked ✅, has been fixed. 481 SystemObjects tests + 23 WebApi tests pass. Items marked ⏸ were deliberately left as-is.

### Resolution summary

| # | Item | Status |
|---|---|---|
| 0 | **Short-format EA off by one** (found during remediation, not in the original audit) — emulator used *instruction address* + disp; hardware uses *next-instruction address* + disp (manual example: `/0500` + `/78` → `/0579`). Fixed in `InstructionBase.GetBase`/`GetEffectiveAddressNoXr`; 44 short-format tag-0 tests re-based. MDX/LDX tag-0 paths were already correct. | ✅ HIGH |
| 1.1 | Address wraparound | ✅ `Cpu` indexer now masks `address % MemorySize`; negative still throws |
| 1.2 | XIO clobbers ACC | ⏸ left (benign) |
| 2.1.1 | MDX opcode `0x0A` → `0x0E` | ✅ emulator + assembler + tests |
| 2.2.1 | Divide remainder sign | ✅ |
| 2.2.2 | Divide negative overflow | ✅ |
| 2.3.1 | STS stale bits (`0xFF03` → `0xFF00`) | ✅ |
| 2.4.1 | SRT logical → arithmetic | ✅ test expectations corrected |
| 2.4.2 | Right shift count 0 hangs | ✅ |
| 2.4.3 | SRA count ≥ 48 | ✅ |
| 2.4.4 | SLCA edge (XR bits 8–9 only) | ⏸ left |
| 3.1 | Sense Interrupt (`011`) | ✅ `Cpu.SenseInterruptLevelStatusWord()` = OR of ILSWs on the level being serviced; `XIO` routes function 3 there regardless of device code |
| 3.2 | ILSW bit positions | ✅ 1442 → `0x2000` (bit 2), console → `0x4000` (bit 1) |
| 4.1.* | 1442 (all seven) | ✅ device rewritten: correct modifier bits, sense-reset by level (bit 15 → level 0, bit 14 → level 4) without aborting, Read/Write use `IoccAddress`, Read/Punch Response DSW bits, Op Complete on level 4, Start Punch feeds a card, single hopper with read → punch → stacker flow. Also fixed end-punch bit (`0x1000` → `0x0008`, i.e. bit 12) and the end-punch column is now punched. |
| 4.2.1 | 2501 last card hides Op Complete | ✅ |
| 4.2.2 | 2501 word count bits 9–15 | ✅ |
| 4.2.3 | 2501 spurious interrupt | ✅ |
| 4.3.1 | 2310 device codes | ✅ `0x04, 0x11–0x14` |
| 4.3.2 | 2310 seek direction bit 13 | ✅ `0x04` |
| 4.3.3 | 2310 read-check transfers | ✅ |
| 4.3.4 | 2310 read address not latched | ✅ (write buffer latched too) |
| 4.3.5 | 2310 reset leaves Op Complete | ✅ |
| — | 2310 never cleared Busy after read/write (found during remediation) | ✅ |
| 4.4.1 | Console stub | ✅ implemented: Write/Run/printer response, Control-select → key → Read protocol, INT REQ, function keys, DSW bits per manual |

Semantics changed in tests as a consequence of the manual: 2501 "Not Ready **or** Busy" (bit 15) is now also set while busy and whenever the hopper is empty.

---

Severity key — **HIGH**: real 1130 software would misbehave or crash. **MED**: observable deviation a program could depend on. **LOW**: edge case or cosmetic.

---

## 1. CPU core — `Cpu.cs`, `InstructionBase.cs`, `IndexRegisters.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 1.1 | MED | **No address wraparound.** Memory indexer throws `ArgumentOutOfRangeException` for addresses ≥ `MemorySize`. | "High-order address bits beyond the capacity of the system are ignored" — `/8000` on a 32K machine accesses `/0000`. | `Cpu.this[int]` bounds-checks and throws. `GetEffectiveAddress` masks to `0xFFFF`, not to the installed size. |
| 1.2 | LOW | XIO does not clobber ACC. | "The accumulator is used in the analysis of the IOCC; any data in the accumulator … is destroyed." | `ExecuteInputOutput.Execute` leaves ACC alone. Benign (programs must save ACC anyway) but strictly inaccurate. |

**Verified correct:** short/long EA generation (IAR *or* XRn + disp; long ignores IAR; IA=1 one indirection); XR1–3 live at `/0001–3`; interrupt vectors `/0008–/000D`; forced-BSI interrupt entry (store IAR at `Memory[vector]`, continue at +1); equal-or-lower levels blocked while a level is active; unassigned opcodes → `Wait = true`; IOCC decode (`dev(5) | func(3) | mods(8)`, second word at `addr | 1`).

---

## 2. Instruction set

### 2.1 Opcodes — `OpCodes.cs`, `Assembler/InstructionCatalog.cs`

| # | Sev | Finding |
|---|---|---|
| 2.1.1 | **HIGH** | **MDX opcode is `0x0A`; the hardware opcode is `01110` = `0x0E`.** Used consistently in the emulator *and* the assembler, so the test suite is self-consistent, but any real 1130 binary (e.g., `tests/.../disk/dms.dsk`, IPL cards) encodes MDX as `0x70xx`/`0x74xx`, which this CPU treats as an unassigned opcode (→ WAIT). Conversely, real `0x50xx` words (unassigned on hardware) would execute as MDX. |

All other opcodes match Figure 15: LD 18, LDD 19, STO 1A, STD 1B, LDX 0C, STX 0D, LDS 04, STS 05, A 10, AD 11, S 12, SD 13, M 14, D 15, AND 1C, OR 1D, EOR 1E, SL 02, SR 03, BSI 08, BSC 09, WAIT 06, XIO 01.

### 2.2 Arithmetic

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 2.2.1 | MED | **Divide: remainder sign wrong for negative dividends.** | "The sign of the remainder is always the same as the sign of the original dividend." | `cpu.Ext = (ushort)(cpu.AccExt % (short)divider)` — `AccExt` is `uint`, so the modulo is unsigned. −7 ÷ 2 yields remainder +1 instead of −1. |
| 2.2.2 | MED | **Divide: negative quotient overflow not detected.** | Overflow when quotient is outside −2¹⁵ … +2¹⁵−1. | Only `quotient > short.MaxValue` is checked; `quotient < short.MinValue` (e.g., −40000) stores a truncated result with Overflow clear. |

**Verified correct:** A/AD/S/SD carry and borrow; sticky Overflow (`if (!cpu.Overflow)`); double-precision odd-address rule (`effectiveAddress | 1` → odd word used twice); M produces 32-bit signed product; D sets Overflow on ÷0 and leaves ACC/EXT unchanged (hardware: undefined).

### 2.3 Load/Store/Status

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 2.3.1 | MED | **STS leaves stale indicator bits.** | Bits 0–7 unchanged, bits 8–13 zeroed, bits 14–15 ← Carry, Overflow. | `word &= 0xFF03` keeps the *old* bits 14–15, then ORs the new values. If the target word previously had Carry=1 and Carry is now 0, bit 14 stays 1. Mask should be `0xFF00`. |

**Verified correct:** LDS (Carry ← bit 14 = `0x02`, Overflow ← bit 15 = `0x01`); STS resets both indicators; LDX short sign-extends, long/indirect loads; LDX/STX with T=0 target the IAR; STX EA = IAR+disp (short) / addr (long) ignoring the tag.

### 2.4 Shifts — `ShiftLeft.cs`, `ShiftRight.cs`, `ShiftInstructionBase.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 2.4.1 | **HIGH** | **SRT is a logical shift; hardware is arithmetic.** | "Bits shifted in the high-order end (bit 0) of the accumulator are the same value as the original sign bit … This type of shifting is called arithmetic shifting. A right-shift of 32 propagates the sign bit … into all positions." | `ulong work = cpu.AccExt; work >>= count;` — zero-fill. The test `Execute_SRT_XR1_15bits` asserts `0x80000000 >> 31 == 0x00000001`; hardware gives `0xFFFFFFFF`. The test encodes the bug. |
| 2.4.2 | **HIGH** | **Right shifts with count 0 never advance the IAR.** | Count 0 = no-operation; next instruction executes normally. | `ShiftRight.Execute`: `if (shiftInfo.ShiftCount == 0) return;` occurs *before* `SetIarToNextInstruction`. `SRA 0` / `SRT 0` / `RTE 0` loop forever. (`ShiftLeft` handles this correctly.) |
| 2.4.3 | LOW | SRA with count ≥ 48 produces wrong results. | Counts 16–63 clear ACC. | `work >> (16 + count)`; C# masks `ulong` shift amounts to 6 bits, so 16+48=64→0, 16+63=79→15. |
| 2.4.4 | LOW | SLCA/SLC edge case: ACC bit 0 already 1 and count ≠ 0. | "Carry on; bits 8 and 9 of the index register are reset to 00, but the count is not changed. No shifting." | Code sets Carry and writes `XR = count` (whole word), zeroing XR bits 0–7 as well. |

**Verified correct:** SLA/SLT carry = last bit out; count 0 = NOP with Carry unchanged (left shifts); SLCA/SLC stop on 1-in-bit-0 or count exhausted, residual count written back, Carry 1 if stopped by bit; SLCA/SLC with T=0 degrade to SLA/SLT; RTE rotate (including 16 = swap, 32 = identity); shift type from bits 8–9, count from low 6 bits of disp or XR.

### 2.5 Branches — `BranchSkip.cs`, `BranchStore.cs`, `ModifyIndex.cs`, `BranchInstructionBase.cs`

**All verified correct:** modifier bits 10–15 = Z, −, +, E, C-off, O-off; short BSC skips if *any* true, long branches if *none* true; Overflow reset when tested, Carry not; BOSC (bit 9 = `0x40`) resets the level only when the branch/skip is taken; BSI stores IAR at EA and continues at EA+1, long BSI conditional with the same inverted sense; MDX short (IAR/XRn += disp), long T=0 (memory += disp), long T≠0 (XRn += addr or contents), skip on sign-change-or-zero excluding IAR.

---

## 3. Interrupt mechanism — `Cpu.HandleInterrupt`, `ExecuteInputOutput.cs`, `DeviceBase.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 3.1 | **HIGH** | **Sense Interrupt (function `011`) is not implemented.** | "Loads the accumulator with the ILSW for the level being serviced … common to all I/O devices; therefore, no device code is needed." Every IBM-style interrupt handler begins with it. | `ExecuteInputOutput` dispatches *all* functions to `Devices[IoccDeviceCode].ExecuteIocc()`. No device handles `DevFunction.SenseInterrupte`. The `Ilsw` field carried by `Interrupt` objects is never read. With device code 0 (the conventional encoding), `Devices[0]` is null and nothing happens; ACC is left stale. |
| 3.2 | LOW | ILSW bit assignments disagree with the manual's level-4 example. | Level 4 ILSW: bit 1 console, bit 2 1442, bit 3 2501, bit 0 paper tape (Figure 17). | 1442 `ReadIlsw = 0x0800` (bit 4); console `Ilsw = 0x1000` (bit 3). 2501 `0x1000` (bit 3) is correct. Moot until 3.1 is fixed. |

**Verified correct:** vector table; BSI-style entry; priority gating; BOSC → `ClearCurrentInterrupt` pops the level and re-arms if the device still holds the request (models "request remains active until reset by Sense Device").

---

## 4. Devices

### 4.1 IBM 1442 Card Read Punch — `Device1442.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 4.1.1 | **HIGH** | **Control modifier bits are reversed.** | Bit 13 Start Read, bit 14 Feed Cycle, bit 15 Start Punch, bit 8 Stacker Select. In the IOCC low byte, bit 8 = `0x80` … bit 15 = `0x01`, so Start Read = `0x04`, Feed = `0x02`, Start Punch = `0x01`, Stacker = `0x80`. | `StartReadBit = 0x20`, `FeedCycleBit = 0x40`, `StartPunchBit = 0x80`, `StackerSelectBit = 0x01`. A real driver's Start Read (`0x04`) is ignored; its Start Punch (`0x01`) is read as Stacker Select; a Stacker Select (`0x80`) starts a punch. The same file uses `IoccModifiers & 1` for "bit 15" in Sense Device, confirming the inconsistency. |
| 4.1.2 | **HIGH** | **Sense Device with reset aborts the operation and returns no status.** | Sense Device loads the DSW into ACC. Modifier bit 15 resets the *level-0 responses*; bit 14 resets the *level-4 responses*. The operation continues. | `if ((IoccModifiers & 1) == 1) { _complete = _readInProgress = _punchInProgress = false; DeactivateInterrupt(); Acc = 0; return; }` — a driver that senses-with-reset on the first column interrupt (the normal pattern) kills the read and sees an all-zero DSW. Bit 14 is ignored. |
| 4.1.3 | **HIGH** | **Punch data is read from memory address 0, not the IOCC address.** | Write (`001`): "the data in the core storage location specified by the address of the IOCC is punched." | `DevFunction.Write → ProcessNextPunchColumn()` reads `CpuInstance[_address]`; `_address` is initialised to 0 and never set from `IoccAddress`. The punch-check then reads `CpuInstance[_address - 1]` = `cpu[-1]` and throws. |
| 4.1.4 | MED | **Read Response / Punch Response bits are never set in the DSW.** | DSW bit 0 Read Response, bit 1 Punch Response (level-0 indicators); the handler uses them to tell read from punch. | `ReadCheckStatus = 0x4000` and `FeedCheckStatus = 0x8000` occupy those positions; no code ever sets a "response" bit. |
| 4.1.5 | MED | **Punch Operation Complete raised on level 0.** | Operation Complete is a **level 4** interrupt for both read and punch. | `CompletePunchOperation` → `ActivateInterrupt(cpu, 0, PunchIlsw)`. |
| 4.1.6 | MED | Control Start Punch does not load a card. | "Start Punch … If a card is not at the punch station, a card will feed." | `StartPunchOperation` sets `_punchInProgress` but never dequeues from `PunchHopper`; `_currentCard` stays null, so the subsequent Write does nothing. Only the "legacy" Write-without-Start path dequeues a card. |
| 4.1.7 | LOW | Feed cycle skips the read station. | Hopper → read station → punch station → stacker, one station per feed. | `PerformFeedCycle` moves the hopper card straight to the stacker. |

**Verified correct:** device code `00010`; card rows 12,11,0–9 → bits 0–11; end-punch on bit 12 (`0x1000`); per-column level-0 interrupts with the CPU issuing Read per column; Op Complete for read on level 4; Last Card when hopper empties; Sense Device `& 1` for bit 15.

### 4.2 IBM 2501 Card Reader — `Device2501.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 4.2.1 | MED | Last card suppresses Operation Complete. | Op Complete "occurs after column 80 has passed the read station" for every card. Last Card is set *in addition* when the operator initiates the last-card sequence. | `_complete = !Hopper.IsEmpty; _lastCard = Hopper.IsEmpty;` — after the final card only Last Card is reported; a driver waiting on Op Complete stalls. (IPL path sets both — correct.) |
| 4.2.2 | LOW | Word count uses the whole first word. | "The word count is located in bit positions 9 through 15 and should never exceed 80." | `TransferToMemory` uses `Memory[wcAddr]` unmasked (capped at 80). Should be `& 0x7F`. |
| 4.2.3 | LOW | `Run()` raises a level-4 interrupt unconditionally. | Interrupt only on Op Complete / Last Card. | `ActivateInterrupt(...)` is outside the `if (!Hopper.IsEmpty && _readInProgress)` block; any call to `Run()` with nothing to do still requests an interrupt (guarded only by `ActiveInterrupt == null`). |

**Verified correct:** device code `01001`; cycle-steal via `TransferToMemory(wca, …)` with data at WCA+1; DSW Last Card `0x1000` (bit 3), Op Complete `0x0800` (bit 4), Busy `0x0002`, Not Ready `0x0001`; Sense Device bit 15 resets Last Card and Op Complete; ILSW-4 bit 3.

### 4.3 IBM 2310 / single disk — `Device2310.cs`

| # | Sev | Finding | Spec | Code |
|---|---|---|---|---|
| 4.3.1 | **HIGH** | **2310 drive device codes are wrong and collide with the 2501.** | CPU single disk `00100` (0x04); 2310 drives 1–4 = `10001`–`10100` (0x11–0x14). | `new byte[] {0x4, 0x09, 0x0a, 0x0b, 0x0c}` — 0x09 is the 2501 card reader. A system with both a 2501 and 2310 drive 1 cannot register both (`AddDevice` returns false). |
| 4.3.2 | **HIGH** | **Seek direction modifier is the wrong bit.** | Control: "Modifier bit 13 controls the direction of movement: 0 forward (toward the center), 1 backward." Bit 13 = `0x04`. | `SeekBackward = 0x40` (bit 9). Real software's backward seeks go forward. |
| 4.3.3 | MED | Read-Check transfers data into memory. | "Unlike the read command, data is not transferred into core storage." | `HandleInitRead` records `_readCheck` but `Run()` always calls `TransferToMemory`. |
| 4.3.4 | MED | Initiate Read does not latch the IOCC address. | The address word belongs to the command that started the operation. | `HandleInitRead` never saves `IoccAddress`; `Run()` uses `CpuInstance.TransferToMemory(buffer, 321)` → whatever `IoccAddress` the *last* XIO (any device) left behind. `HandleInitWrite` correctly saves `_wcAddress`. |
| 4.3.5 | LOW | Sense Device reset does not clear Operation Complete. | "Operation complete and data error … indicators are reset if modifier bit 15 is a 1." | Only `DeactivateInterrupt` is called; `_complete` stays true, so the DSW keeps reporting Op Complete. |

**Verified correct:** single-disk device code 0x04; Read-Check modifier bit 8 (`0x80`); sector in bits 13–15 (`0x07`); WCA convention with data at WCA+1; 321-word clamp; seek of 0 cylinders is a silent no-op; cylinder range 0–202; Operation Complete on level 2; DSW layout (Data Error bit 0, Op Complete bit 1, Not Ready bit 2, Busy bit 3, Home bit 4, sector count low bits) is consistent with the manual's prose ("Bit 0 and bit 2 will be turned on" for write-select error).

### 4.4 Console — `DeviceConsoleKeyboard.cs`, `ConsoleEntrySwitches.cs`, `Device1053.cs`

| # | Sev | Finding |
|---|---|---|
| 4.4.1 | MED | `DeviceConsoleKeyboard` is a stub: Sense Device always returns 0 (ready), Read sets `Acc = 0` instead of storing a character at `IoccAddress`, Write and Control are empty. The DSW bit comments in the header (Printer Response bit 0, Keyboard Response bit 1, Int Req bit 2, Console/Keyboard switch bit 3, Printer Busy bit 4, Printer Not Ready bit 5, Keyboard Busy bit 6) **do** match the manual. `Device1053` is entirely commented out. Not registered by default (`BuildDefaultDevices` adds only the entry switches). |

**Verified correct:** `ConsoleEntrySwitches` — device code `00111`, Read stores the switch word at `IoccAddress`.

---

## 5. Summary by severity

**HIGH (8):** MDX opcode (2.1.1) · SRT logical not arithmetic (2.4.1) · right-shift count 0 hangs (2.4.2) · Sense Interrupt missing (3.1) · 1442 control bits reversed (4.1.1) · 1442 sense-reset aborts operation (4.1.2) · 1442 punch reads address 0 (4.1.3) · 2310 device codes collide with 2501 (4.3.1) · 2310 seek direction bit (4.3.2)

**MED (10):** no wraparound (1.1) · divide remainder sign (2.2.1) · divide negative overflow (2.2.2) · STS stale bits (2.3.1) · 1442 response bits never set (4.1.4) · 1442 punch complete on level 0 (4.1.5) · 1442 Start Punch loads no card (4.1.6) · 2501 last-card hides op-complete (4.2.1) · 2310 read-check transfers (4.3.3) · 2310 read address not latched (4.3.4) · console stub (4.4.1)

**LOW (8):** XIO/ACC (1.2) · SRA ≥48 (2.4.3) · SLCA edge (2.4.4) · ILSW bits (3.2) · 1442 feed path (4.1.7) · 2501 word-count mask (4.2.2) · 2501 spurious interrupt (4.2.3) · 2310 reset leaves complete (4.3.5)

## 6. Tests that encode incorrect behavior

- `InstructionTests/ShiftRightAccExcTests.cs` — `Execute_SRT_XR1_15bits` (`0x80000000`→`0x00000001`), `Execute_SRT` (sign bit not propagated).
- Any test building MDX via `OpCodes.ModifyIndex` or the assembler's `Opcode = 0x0A` (consistent with each other; both wrong vs hardware).
- `Device1442` tests that drive Control with `0x20`/`0x40`/`0x80` modifiers, or that rely on Sense-with-reset returning 0.
- `Device2310` tests constructing drives 1–4 and asserting device codes `0x09–0x0C`, or using `0x40` as the backward-seek modifier.
