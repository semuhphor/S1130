# AI Handoff: Reading the IBM 1130 Functional Characteristics Manual

**Audience:** a future AI (or human) working on the S1130 emulator who needs the authoritative hardware behavior.
**Written:** 2026-10-10, after a full review of `ForReferenceOnly/functional/`.
**Companion:** [IBM1130-Functional-Characteristics-Summary.md](./IBM1130-Functional-Characteristics-Summary.md) — the verified structured summary.

---

## 1. What the source is and where it lives

`ForReferenceOnly/functional/*.html` is the IBM manual **GA26-5881, "IBM 1130 Functional Characteristics"**, converted to HTML (the ibm1130.net edition). ~60 files, ~1.1 MB. It is **gitignored** (`ForReferenceOnly/` is in `.gitignore`), so:

- `glob`/`grep` tools that honor `.gitignore` **will not find it**. Use PowerShell `Get-ChildItem -Recurse` or pass the explicit path.
- It is not in the repo history. If it's missing on a fresh clone, the source is https://www.ibm1130.net/functional/.

Start at `TOC.html` or `MainIndex.html`. One HTML file per chapter; one file per instruction (`LDinstr.html`, `BSCBOSCinstr.html`, …).

### Reading the HTML
The pages are table-heavy with nested `<td>`s. A reliable way to get text in PowerShell:

```powershell
$h = Get-Content -Raw 'ForReferenceOnly\functional\CPU.html'
$h -replace '<script[\s\S]*?</script>','' -replace '<style[\s\S]*?</style>','' `
   -replace '<br\s*/?>',"`n" -replace '</(p|div|tr|h\d|li|td|th)>',"`n" `
   -replace '<[^>]+>',' ' -replace '&nbsp;',' ' -replace '&amp;','&' `
   -replace '[ \t]{2,}',' ' -replace '(\r?\n[ \t]*){2,}',"`n"
```

Tables come out one cell per line — readable but you must count columns yourself. **Many bit-layout figures (instruction formats, DSW bit maps, Figures 5/6/22/37/41/43) are images and are NOT in the text.** For those, the surrounding prose names the bits ("Bit 4 printer busy"); harvest from the prose.

---

## 2. Hard-won process lesson: do not trust summarizer agents on this material

I first fanned the manual out to five parallel "explore" subagents and asked each for a dense structured summary. **Every one of them fabricated tables** while sounding completely authoritative:

| Agent claimed | Source actually says |
|---|---|
| Reserved locations `/0000`, `/0004–7`, two-word vectors at `/0008–/000F`, `/0010–/001F` | Only `/0001–3` (XRs), `/0008–D` (one word per level), `/0020–27` (1132 scan field) |
| Short EA = IAR + Disp + XRn | EA = (IAR **or** XRn) + Disp — tag 00 means IAR, tag ≠ 0 means XRn *instead of* IAR |
| "Five models, Model 1–5" | Fourteen models 1A…5D with letter suffixes |
| LD opcode `00000` | `11000` |
| XIO functions "0 control, 1 read, 2 write, 3 sense device, 4 sense interrupt…" | `001` Write, `010` Read, `011` Sense Interrupt, `100` Control, `101` Init Write, `110` Init Read, `111` Sense Device |
| 1442 device code `00100` | `00010` (`00100` is the CPU single disk) |

The fabrications were *plausible* — they looked like 1130 facts and used the right vocabulary. Spot-checking a single table against the source was what exposed them. **Rule: for this manual, read the source yourself. If you must delegate, delegate extraction of verbatim text, not summarization, and verify every table you intend to rely on.** The unverified agent drafts were discarded.

---

## 3. What I verified and the mental model I ended up with

Everything below was read directly from the manual text. Details and tables are in the companion summary; this is the shape of the machine as I now hold it.

### The CPU is small and regular, with a few sharp edges
- 16-bit words, two's complement, ACC + EXT form a 32-bit pair. XR1–3 **are core locations `/0001–3`**, not hidden registers — storing to `/0001` changes XR1.
- Two instruction formats. Short: `op(5) F=0 T(2) disp(8)`. Long: `op(5) F=1 T(2) IA(1) bit9 mods(6) | addr(16)`.
- **Short-format EA = base + sign-extended disp**, where base is IAR (already advanced past the instruction) when T=00, else XRn. Short is *direct only*.
- **Long-format EA = addr (+ XRn if T≠00)**; IAR is never involved. IA=1 adds one indirection through that word.
- High address bits beyond the installed core are silently ignored (wraparound). An emulator with 32K should mask to 15 bits.
- Double-precision ops require an even address; with an odd address the hardware uses the odd word twice. That's documented behavior, not UB.
- Overflow is **sticky** for A/AD/S/SD: once set it stays set until tested by BSC/BSI or overwritten by LDS/STS. Carry is reset before every add/subtract.
- **All unassigned opcodes execute as WAIT.**
- Shifts are short-format only; bits 8–9 pick the variant; count is the low 6 bits of disp (T=00) or of XRn. SLCA/SLC with T≠00 write the *residual* count back into XRn — the manual's intended idiom for scanning a status word to index a jump table.
- M: ACC × mem → ACC:EXT. D: ACC:EXT ÷ mem → quotient ACC, remainder EXT (remainder takes the dividend's sign); overflow on ÷0 or quotient out of 16-bit range, ACC/EXT then undefined.
- BSC modifier bits 10–15 = Zero, Negative, Plus, Even, Carry-off, Overflow-off. **Short skips if ANY selected condition is true; long branches if NONE is.** Testing resets Overflow but not Carry. Bit 9 = BOSC, which resets the interrupt level **only if the branch/skip is actually taken**.
- MDX: short adds disp to IAR (T=00, pure branch) or XRn; long T=00 adds disp to the addressed word, long T≠00 adds the address field (or its contents if IA) to XRn. Skips one word if the modified location changes sign or becomes zero — never for IAR.

### I/O is a single instruction plus a two-word command block
- `XIO` EA must be even and points at an IOCC: word 1 = address, word 2 = `dev(5) func(3) mods(8)`. **XIO destroys ACC.**
- Two transfer styles, and the emulator already models both:
  - **Direct program control** (1442, console, paper tape, plotter): one Read/Write IOCC per word, program updates the address itself. Each word arrives via an interrupt. Timing-critical.
  - **Cycle steal** (single disk, 2310/2311, 1132, 2501, anything on the SAC): one Initiate Read/Write, hardware moves the block. Address word points at a **word-count word**; data follows it.
- Device codes and function codes are in the summary. The ones I'd keep in my head: console `00001`, 1442 `00010`, CPU disk `00100`, 1132 `00110`, CES/stop-key `00111`, 2501 `01001`, 2310 drives `10001–10100`.
- Sense Device modifier bit 15 resets the device's highest-level interrupt, bit 14 the next. A device that isn't installed returns an all-zero DSW — indistinguishable from "ready and idle".

### Interrupts are a forced BSI
- Six levels, vectors at `/0008–/000D`, one word each. Level 0 highest.
- On interrupt: finish current instruction → forced **BSI indirect via the vector** (IAR saved in the handler's first word, execution starts at word+1) → handler does Sense Interrupt (ILSW, levels 1–5) → Sense Device with reset → service → **BOSC indirect through its own first word** to return and release the level.
- Level 0 is 1442-only (no ILSW needed). Level 4 is the crowded one (console, 2501, 1442 op-complete, paper tape, 1403, …). Level 5 is the console program-stop key and INT RUN mode, and uses device code `00111` for its DSW.
- Equal-or-lower requests are held off while a level is active; higher ones preempt.

### Devices — the parts the emulator cares about
- **1442**: level 0 per-column Read Response (serve within 800/700 µs) and Punch Response (300 µs); level 4 Operation Complete. Control modifiers: bit 13 Start Read, 14 Feed, 15 Start Punch, 8 Stacker Select. Punching ends at the first word with **bit 12 = 1**. Card rows 12,11,0–9 → bits 0–11.
- **2501**: cycle steal; word count in **bits 9–15** of the first buffer word, max 80; single Operation Complete interrupt on level 4 (ILSW bit 3).
- **Disk** (single/2310): 8 sectors/cylinder, 200 cylinders (+3 spares), 321 words/sector physically, 320 usable (DMS puts the sector address in word 0). Init Read/Write: address = WCA, mods 13–15 = sector, bit 8 = read-check. Control: address = cylinder count, bit 13 = direction (0 = toward center). Seek time 7.5·N + 22.5 ms. Only one interrupt (Operation Complete, level 2). Word count 0 is a hardware hang.
- **Console**: printer takes one Write per character (bit 6 case, bit 7 control-vs-data), ignores writes while busy. Keyboard needs a Control to select before each key; the key then interrupts on level 4 and a Read collects it. INT REQ key → level 4 with DSW bit 2. Console entry switches read via Read with device `00111`.
- **1132**: program drives printing by filling the scan field at `/0020–/0027` once per emitter character cycle (48 cycles/line). Level 1.

---

## 4. Where this matters for the S1130 codebase

A full conformance audit was performed and remediated the same day — see [FunctionalCharacteristics-Audit.md](./FunctionalCharacteristics-Audit.md) for every finding, its severity, and its resolution. Headline fixes: short-format EA was off by one (emulator used the instruction's own address as the IAR base), MDX opcode was `0x0A` instead of `0x0E`, SRT was a logical shift, Sense Interrupt was unimplemented, and the 1442 control-modifier bits were reversed. If you touch these areas again, the audit lists the exact manual text each behavior is based on.

Spec points still worth keeping in mind when extending the code:

- Short-format EA base is `IAR + 1` (handled centrally in `InstructionBase`); long format never uses the IAR.
- The IOCC low byte is IBM bits 8–15, so bit *n* is `1 << (15 − n)`: bit 15 = `0x01`, bit 13 = `0x04`, bit 8 = `0x80`. Several past bugs came from reading "bit 13" as `0x20`.
- Sense Device returns the DSW *and then* applies the reset modifier; it never aborts an operation.
- Level 0 has no ILSW; levels 1–5 do. `Cpu.SenseInterruptLevelStatusWord()` ORs the pending devices on the level being serviced.
- Each `DeviceBase` tracks one `ActiveInterrupt`; devices that can have a level-0 response and a level-4 completion outstanding (1442) release the level-0 one before raising level 4.

---

## 5. Still unread in detail

I only skimmed headings for: `SCA.html` (comms adapter), `DisplayUnit.html` (2250), `OMPR.html` (1231), `Plotter.html`, `PaperTape.html`, the 1403 half of `Printers.html`, `System7.html`, `AppendixA.html` (character code tables), `Glossary.html`. The summary marks these **[overview only]**. If you need them, read the HTML directly — don't trust a summarizer.
