# IBM 1130 Disk Monitor System (DMS) Architecture

## Overview

The Disk Monitor System (DMS) Version 2 Modification 12 (V2M12) is the operating system for the IBM 1130 computer. It provides a complete programming environment with compilers, assemblers, utilities, and I/O device management. DMS is a disk-resident, single-user operating system that manages all aspects of program execution, from loading programs to handling I/O operations.

## System Architecture

### Core Components

DMS is organized into several major subsystems:

1. **Monitor** (Resident Core Image)
2. **DISKZ** (Disk I/O Manager)
3. **DUP** (Disk Utility Program)
4. **I/O Device Drivers** (Phase-based)
5. **Compilers and Assemblers** (FORTRAN, RPG, Assembler)
6. **System Subroutines** (Conversion, I/O Control)

### Memory Organization

DMS uses a sophisticated memory management scheme:

```
Address Range    Purpose
---------------------------------------------------------------------------
0x0000-0x000D    Special locations (XR1-3, interrupt vectors at 0x08-0x0D)
0x000E-0x00EF    Monitor communication area (system variables)
0x00F2-0x01E0    DISKZ (Disk I/O manager)
0x01E0-????      Monitor resident code
0x7DB0-0x7F1E    DUPCO (DUP resident core)
0x7F20-????      CATCO (Concatenated common area)
????-0x7FFF      User program area (varies by core size)
```

### Key Memory Locations

The monitor uses specific memory locations for communication and control:

```assembly
$CIBA   EQU  /5      ; Address of CIB on master cartridge
$CH12   EQU  /6      ; Channel 12 indicator, non-zero = Channel 12
$COMN   EQU  /7      ; Word count of common
$CORE   EQU  /E      ; Core size (/1000=4K, /2000=8K, etc.)
$CTSW   EQU  /F      ; MCR switch, non-zero = // record trapped
$DADR   EQU  /10     ; Sector address of program to be fetched
$IBSY   EQU  /13     ; Principal I/O busy indicator
$PBSY   EQU  /36     ; Principal printer busy indicator
$1132   EQU  /7F     ; Channel 12 indicator for 1132
$1403   EQU  /80     ; Channel 12 indicator for 1403
$DBSY   EQU  /EE     ; Disk busy indicator
```

## DISKZ - Disk I/O Manager

### Purpose
DISKZ is the fundamental disk I/O subsystem that handles all disk read/write operations. It resides at a fixed location in memory (0x00F2-0x01E0) and is always resident.

### Entry Point
- **Address**: 0x00F2 (DZ000)
- **Calling Sequence**: `BSI L DZ000`

### Parameters
DISKZ operations are controlled by an I/O Area (IOAR) with the following format:

```
Word 0:  Word count (number of 16-bit words to transfer)
Word 1:  Sector address on disk
Word 2:  Function code
         0x0000 = Read from disk
         0x7000 = Write to disk
Word 3:  Core memory address for transfer
```

### Operation
1. Caller sets up IOAR with parameters
2. Calls DISKZ via BSI L DZ000
3. DISKZ initiates operation and returns immediately
4. Caller checks $DBSY (location 0xEE) in loop until operation completes
5. $DBSY = 0 when operation is complete

### Example Usage
```assembly
; Read 320 words from sector 0x630 into PHAS2
LDD   LD900           ; Load word count and sector address
STD L PHAS2           ; Store at I/O area
LDD   LD902           ; Load function and address
BSI L DZ000           ; Call DISKZ
MDX L $DBSY,0         ; Test if complete
MDX   *-3             ; Loop until done

LD900  DC   /1000-/200  ; Word count (3840 words)
       DC   /630        ; Sector address
LD902  DC   /0000       ; Read function
       DC   PHAS2       ; Core address
```

## DUP - Disk Utility Program

### Overview
DUP is the user interface and utility manager for DMS. It handles:
- Program loading and execution
- File management (STORE, DELETE, DEFINE)
- System configuration
- Card/Disk operations
- Memory dumps

### DUP Phases

DUP is organized into phases that are loaded on demand:

```
Phase  ID   Name        Purpose
------------------------------------------------------------------
0      N/A  Monitor     Always resident, controls system
1      N/A  DISKZ       Disk I/O manager (always resident)
2      N/A  DUPCO       DUP resident core (control functions)
3      N/A  DCTL        DUP control - processes control cards
4      N/A  STORE       Stores programs to disk
5      N/A  FILEQ       File queue display
6      N/A  DUMP        Memory dump utility
7      N/A  DUMPLET     Dumps LET (Label Entry Table)
8      N/A  DELETE      Deletes files from disk
9      N/A  DEFINE      Defines disk areas
10     N/A  DEXIT       DUP exit handler
13     N/A  UPCOR       Update core image
14     N/A  PFACE       Principal I/O interface
15     N/A  SIFACE      Principal I/O without keyboard
16     N/A  PTFACE      Paper tape interface
17     N/A  CIFACE      Core image interface
140    P1403  Device driver for 1403 printer
141    P1132  Device driver for 1132 printer
142    PCPAD  Console printer driver
143    I2501  2501 card reader driver
144    I1442  1442 card read/punch driver
145    @1134  Paper tape driver
146    IKBRD  Keyboard driver
147    CDCNV  Card conversion (2501/1442)
148    C1134  Paper tape conversion
149    CKBRD  Keyboard conversion
150    DISKZ  Disk subroutine
153    PRINT  Principal print subroutine
154    PINPT  Principal input subroutine
155    PIDEV  Principal input excluding keyboard
156    CNVRT  Principal conversion subroutine
157    CVRT   Conversion excluding keyboard
206    MODIF  System modification utility
```

### DUPCO - DUP Resident Core

DUPCO resides at address 0x7DB0 and provides core services:

**Entry Points**:
```
BINEB   - Binary to EBCDIC conversion
WRTDC   - Write DCOM (disk common area)
ENTER   - Save index registers and conditions
GET     - Read from disk using DISKZ
LEAVE   - Handle exits from DUP
LINE    - Space printer one line
MASK    - Inhibit keyboard interrupt
MDUMP   - Print selected core locations
PAGE    - Skip to next page
PHID    - Record phase ID
PRINT   - Use system printer to print
PUT     - Write to disk using DISKZ
REST    - Restore CATCO, return to DCTL
RTURN   - Restore index registers and conditions
```

### CATCO - Concatenated Common Area

CATCO (address 0x7F20) is a shared data area used by all DUP phases. It contains:

1. **DCOM Image** - Disk common area copy
2. **I/O Area Headers** - For each DUP phase
3. **Switches** - Communication between phases
4. **Buffer Addresses** - Pointers to I/O buffers

**Key CATCO Switches**:
```
ASMSW   - Non-zero if DEFINE void assembler
BITSW   - Non-zero to allow MDUMP-S
CISW    - Non-zero if STORE CI (core image)
DATSW   - Data count from DUP control record
FORSW   - Non-zero if DEFINE void FORTRAN
IOSW    - Non-zero if input/output required
PRSW    - Non-zero if printing output
PTSW    - Non-zero if paper tape required
STSW    - Non-zero if STORE type function
```

## I/O System Architecture

### Device Drivers

Each I/O device has a device driver loaded as a phase when needed. Drivers are identified by phase IDs:

**Printer Drivers**:
- **P1403 (140)**: IBM 1403 line printer driver
- **P1132 (141)**: IBM 1132 line printer driver  
- **PCPAD (142)**: Console printer driver

**Card Drivers**:
- **I2501 (143)**: IBM 2501 card reader
- **I1442 (144)**: IBM 1442 card read/punch

**Other Drivers**:
- **@1134 (145)**: Paper tape reader/punch
- **IKBRD (146)**: Console keyboard

### Character Encoding and Conversion

One of the most complex aspects of the 1130 I/O system is that **each device uses a different character encoding**. The DMS provides conversion routines to translate between formats.

#### Encoding Formats

1. **IBM Card Code (Hollerith)**
   - Used by: 1442, 2501, Console Keyboard
   - Example: 'A' = 0x9000 (rows 12,1)
   - Example: Space = 0x0000 (no punches)
   - Format: 16-bit word with bits representing card rows

2. **Console Printer Code**
   - Used by: Console printer only
   - Example: 'A' = 0x3C (uppercase) or 0x3E (lowercase)
   - Example: Space = 0x21
   - Format: 8 bits (bits 0-7), bit 6 = case, bit 7 = control/data
   - Completely unique encoding

3. **EBCDIC Subset**
   - Used by: 1132 printer
   - Example: 'A' = 0xC1
   - Example: Period = 0x4B
   - Limited character set

4. **EBCDIC Full**
   - Used by: 1403 printer
   - Example: 'A' = 0x64
   - Example: Space = 0x7F
   - Example: Period = 0x6E
   - Complete EBCDIC character set

5. **PTTC/8 (Paper Tape Code)**
   - Used by: 1134 paper tape
   - Example: 'A' = 0x61 (upper) or 0x31 (lower)
   - Separate upper/lower case codes

#### Conversion Subroutines

**Phase 147 - CDCNV (Card Conversion)**:
- Converts between IBM Card Code and internal format
- Handles both 2501 and 1442 devices
- Supports read and punch operations

**Phase 148 - C1134 (Paper Tape Conversion)**:
- Converts PTTC/8 code to/from internal format
- Handles upper/lower case translation

**Phase 149 - CKBRD (Keyboard Conversion)**:
- Converts keyboard card-code format to internal format
- Handles special keys (EOF, ERASE FIELD, backspace)

**Phase 156 - CNVRT (Principal Conversion)**:
- Master conversion routine for principal I/O device
- Determines device type and calls appropriate converter
- Handles: cards, printer, keyboard, paper tape

**Phase 157 - CVRT (Conversion excluding keyboard)**:
- Like CNVRT but doesn't handle keyboard input
- Used when keyboard is not the principal input device

#### Conversion Process

When a program performs I/O:

1. **Input Operation**:
   ```
   Device → Device-specific encoding → Conversion routine → 
   Internal format (usually EBCDIC or card code) → Program memory
   ```

2. **Output Operation**:
   ```
   Program memory → Internal format → Conversion routine → 
   Device-specific encoding → Device
   ```

Example for printing "HELLO":
```
1. Program has "HELLO" in EBCDIC: C8 C5 D3 D3 D6
2. Call printer conversion routine
3. If 1132: Convert to EBCDIC subset: C8 C5 D3 D3 D6
4. If 1403: Convert to EBCDIC: 6B 68 5D 5D 5E
5. If Console: Convert to console codes: 24 34 5C 5C 50
6. Send to device driver
7. Driver issues XIO Write for each character
```

### Principal I/O System

The "Principal I/O" system allows programs to work with a single logical I/O device without knowing the physical device details.

**Phases**:
- **PINPT (154)**: Principal input - handles card reader or keyboard
- **PRINT (153)**: Principal print - handles any printer (1132, 1403, console)
- **PIDEV (155)**: Principal input excluding keyboard

**Operation**:
1. System configuration determines principal devices (set during DMS initialization)
2. Indicators in CATCO tell which devices are active:
   - `#PIOD` - Principal I/O device indicator
   - `#PPTR` - Principal printer indicator
   - `$1132` - Set if 1132 is principal printer
   - `$1403` - Set if 1403 is principal printer
3. Program calls principal I/O routine (e.g., PRINT)
4. Principal routine checks indicators and calls appropriate device driver
5. Device driver calls conversion routine if needed
6. Device driver issues XIO commands to hardware

### Interrupt Handling

DMS uses a sophisticated interrupt system with 6 priority levels (0-5):

**Interrupt Vectors** (at addresses 0x08-0x0D):
```
Level 0: Address 0x08 - Highest priority
Level 1: Address 0x09 - 1132 printer interrupts
Level 2: Address 0x0A - Reserved
Level 3: Address 0x0B - Reserved
Level 4: Address 0x0C - Console, 1442, 1403, 2501
Level 5: Address 0x0D - Program stop, keyboard request
```

**Interrupt Level Status Word (ILSW)**:
Each interrupt level has a status word that indicates which device interrupted:
```
Level 4 ILSW bits:
  Bit 0: 1442 punch
  Bit 1: Console (printer/keyboard)
  Bit 2: 1403 printer
  Bit 3: 2501 card reader
```

**Interrupt Handling Flow**:
1. Device completes operation or needs service
2. Device activates interrupt on assigned level
3. CPU saves IAR to interrupt vector location
4. CPU jumps to interrupt handler (vector + 1)
5. Interrupt handler:
   - Executes Sense Interrupt (XIO function 6) to get ILSW
   - Determines which device interrupted
   - Calls device-specific interrupt handler
   - Restores state and returns (BSC I to saved IAR)

### I/O Area (IOAR) Structure

Each I/O operation uses an I/O Area structure:

```
Word 0:  Word count (number of words to transfer)
Word 1:  Sector address (for disk) or buffer address
Word 2:  Function code or device status
Word 3:  Additional parameters (device-specific)
```

**Example - Card Read**:
```
CARDIO  DC   80          ; Read 80 words (1 card)
        DC   BUFFER      ; Store in BUFFER
        DC   0           ; Read function
        DC   0           ; Reserved
```

## System Initialization

### Cold Start Sequence

1. **IPL (Initial Program Load)**:
   - Operator presses START button
   - CPU loads IPL card (first card in deck)
   - IPL card is self-loading boot code
   - Boot code reads more sectors from disk

2. **System Loader Phase 1** (at 0x01E0):
   - Loads at end of DISKZ
   - Reads DCOM (Disk Common Area) from sector 1
   - Reads cartridge ID from sector 0
   - Validates system cartridge
   - Loads Phase 2

3. **System Loader Phase 2** (variable location):
   - Determines core size
   - Loads monitor resident image
   - Initializes interrupt vectors
   - Loads DUPCO
   - Sets up CATCO
   - Transfers control to monitor

4. **Monitor Initialization**:
   - Initializes system variables
   - Determines device configuration
   - Loads DUP Phase 0 (DCTL)
   - Waits for control card

### Warm Start

After initialization, operator enters control cards:

```
// JOB
// FOR
*IOCS(CARD,1132 PRINTER)
*LIST SOURCE PROGRAM
... FORTRAN source code ...
// XEQ
```

Each `//` control card is processed by DCTL, which:
1. Reads control card
2. Parses command
3. Loads appropriate phase
4. Executes command
5. Returns to monitor

## Disk Organization

### Cartridge Structure

An IBM 2310 disk cartridge has:
- 1 disk platter
- 2 surfaces (top and bottom)
- 200 cylinders per surface
- 4 sectors per track
- 320 words per sector

**Total capacity**: 200 × 2 × 4 × 320 = 512,000 words = 1,024,000 bytes

### System Areas

**Fixed System Sectors**:
```
Sector 0:    Cartridge ID (IDAD) - identifies cartridge
Sector 1:    DCOM - Disk Common Area (system configuration)
Sector 2:    RIAD - Resident Image Address (where monitor loads)
Sector 3:    SLET - System Label Entry Table (file directory)
Sectors 4-5: Additional SLET sectors
Sector 6:    RLTB - Reload Table (for cold start)
Sector 7:    HDNG - Page heading for printouts
```

**DCOM Contents**:
```
#NAME   - Name of program (4 words)
#DBCT   - Disk block count
#FCNT   - Files switch (0 = no files)
#SYSC   - Non-zero = system cartridge
#LCNT   - Number of local files
#NCNT   - Number of NOCAL files
#ENTY   - Relative entry address
#TODR   - Drive code of 'TO' drive
#FRDR   - Drive code of 'FROM' drive
#CIDN   - Cartridge IDs for logical drives 0-4
#CIBA   - CIB addresses for logical drives 0-4
#ULET   - LET addresses for logical drives 0-4
```

### File Organization

**LET (Label Entry Table)**:
The LET is a directory structure that tracks all programs on the cartridge.

**LET Entry Format** (4 words):
```
Word 0:  Program name (2 characters packed)
Word 1:  Entry point address
Word 2:  Word count (program size)
Word 3:  Sector address (where program starts on disk)
```

**FLET (Fixed Area Label Entry Table)**:
Like LET but for the fixed area (system programs and commonly-used programs).

**File Areas**:
- **Fixed Area**: System programs, always present
- **Working Storage**: User programs, can be scratched
- **User Area**: Unformatted data area
- **Cushion**: Buffer between areas to allow growth

## Compilers and Assemblers

### FORTRAN Compiler

The FORTRAN compiler is a multi-pass compiler:

**Phase Structure**:
```
Phase    Name        Function
---------------------------------------------------------------
FORT1    Lexer       Tokenizes source code
FORT2    Parser      Builds parse tree, type checking
FORT3    Optimizer   Optimizes code
FORT4    Code Gen    Generates 1130 assembly code
```

**Calling Sequence**:
```
// FOR
*IOCS(CARD, 1132 PRINTER)
*LIST SOURCE PROGRAM
... FORTRAN source ...
// XEQ
```

**IOCS (Input/Output Control System)**:
The FORTRAN compiler uses IOCS specifications to generate I/O code:
```
*IOCS(device_list)

Examples:
*IOCS(CARD)                  - Card reader only
*IOCS(CARD, 1132 PRINTER)    - Card and 1132 printer
*IOCS(CARD, KEYBOARD)        - Card and keyboard
*IOCS(TYPEWRITER)            - Console only
```

### Assembler

The IBM 1130 Assembler is a two-pass assembler with macro support.

**Features**:
- Two-pass assembly (symbol collection, code generation)
- Macro definitions and expansion
- Conditional assembly
- Multiple addressing modes (short, long, indexed, indirect)
- Expression evaluation
- Standard directives: ORG, DC, EQU, BSS, BES

**Calling Sequence**:
```
// ASM
... assembly source ...
// XEQ
```

See `Assembler.md` for complete assembler documentation.

### RPG (Report Program Generator)

RPG is a high-level language for business report generation.

**Phase Structure**:
```
RPGC1    Specification reader
RPGC2    Logic generator
RPGC3    Code generator
```

**Calling Sequence**:
```
// RPG
... RPG specifications ...
// XEQ
```

## Error Handling

### Error Types

**Pre-operative Errors** (detected before execution):
- File not found
- Insufficient memory
- Invalid control card
- Device not available

**Post-operative Errors** (detected during execution):
- I/O errors (read error, punch error, etc.)
- Arithmetic overflow
- Array bounds exceeded (FORTRAN)
- Wait state (illegal instruction)

### Error Traps

DMS sets up error trap vectors:
```
$PRET   EQU  /28     ; Pre-operative error trap address
$PST1   EQU  /81     ; Post-operative error trap, level 1
$PST2   EQU  /85     ; Post-operative error trap, level 2
$PST3   EQU  /89     ; Post-operative error trap, level 3
$PST4   EQU  /8D     ; Post-operative error trap, level 4
```

When an error occurs:
1. Device driver or program detects error
2. Jumps to appropriate error trap
3. Error handler determines error type
4. Prints error message via PRINT routine
5. Either returns to DUP or WAITs for operator intervention

### Error Messages

Common error messages:
```
E 02 - NOT READY          Device not ready
E 03 - READ ERROR         Card or disk read error
E 04 - PUNCH ERROR        Card punch error
E 05 - CHECKSUM ERROR     Paper tape checksum error
E 90 - FILE NOT FOUND     Program not in LET
E 91 - INSUFFICIENT CORE  Not enough memory
```

## System Configuration

### Available Devices

DMS supports various device configurations:

**Minimum Configuration**:
- 4K core (expandable to 32K)
- 1 IBM 2310 disk cartridge
- 1442 Card Reader/Punch or 2501 Card Reader
- Console keyboard and printer

**Full Configuration**:
- 32K core
- 5 IBM 2310 disk drives (logical drives 0-4)
- 2501 Card Reader or 1442 Card Reader/Punch
- 1132 or 1403 line printer
- Console keyboard and printer
- 1134 Paper Tape Reader/Punch
- 1627 Plotter
- 2250 Display Unit

### Configuration Setup

Configuration is set during DMS generation:
1. Run DMGEN (DMS Generator) program
2. Specify available devices
3. Specify core size
4. DMGEN creates custom system cartridge
5. Cold start from new cartridge

Configuration is stored in:
- DCOM (disk common area)
- System indicators ($1132, $1403, etc.)
- Device availability flags in CATCO

## Performance Considerations

### Timing

**Instruction Execution**:
- Basic instructions: 3.6 µs (with 1.8 µs memory cycle)
- Multiply: 21.6 µs
- Divide: 25.2 µs

**I/O Timing**:
- 1442 Card Reader: 400 cards/minute (150 ms/card)
- 1442 Card Punch: 160 cards/minute (375 ms/card)
- 2501 Card Reader: 1000 cards/minute (60 ms/card)
- 1132 Printer: 40-110 lpm (11.2 ms/character, 48 cycles/line)
- 1403 Printer: 210-600 lpm (block mode, much faster)
- Disk: 75 ms average seek, 25 ms rotation, ~100 µs/word transfer

**Console Printer**: 15.5 characters/second (64.5 ms/character)

### Optimization

**DMS Optimizations**:
1. **Phase Loading**: Phases are loaded only when needed
2. **Resident Code**: Frequently-used code (DISKZ, DUPCO) always resident
3. **Buffering**: Double buffering for I/O operations
4. **Interrupt-Driven I/O**: CPU continues processing during I/O
5. **Conversion Caching**: Conversion tables precomputed
6. **Direct Disk Access**: DISKZ bypasses monitor for performance

**Program Optimizations**:
1. Use principal I/O routines (PRINT, PINPT) instead of direct XIO
2. Buffer I/O operations
3. Use indexed addressing to avoid repeated calculations
4. Minimize disk seeks by organizing data sequentially
5. Use IOCS subroutines instead of custom I/O code

## Programming Examples

### Example 1: Simple Card Read and Print

```assembly
        ; Read cards and print to 1132 until EOF
        
        ORG   /100
START   BSI I PINPT           ; Call principal input
        DC    80              ; Word count
        DC    BUFFER          ; Buffer address
        
        LD    BUFFER          ; Check first word
        S     EOF             ; Compare to EOF marker
        BSC L DONE,Z          ; Branch if EOF
        
        BSI I PRINT           ; Call principal print
        DC    120             ; Word count (120 chars)
        DC    BUFFER          ; Buffer address
        
        BSC L START           ; Loop
        
DONE    WAIT                  ; End
        
BUFFER  BSS   120             ; 120-word buffer
EOF     DC    /9000           ; EOF marker (12 punch)
```

### Example 2: Using DISKZ

```assembly
        ; Read sector 100 into buffer
        
        ORG   /200
START   LDD   IOAR            ; Get word count and sector
        STD L BUFFER-2        ; Store before buffer
        LDD   IOAR+2          ; Get function and address
        BSI L /00F2           ; Call DISKZ (DZ000)
WAIT    MDX L /EE,0           ; Check $DBSY
        MDX   WAIT            ; Loop until done
        
        ; Process buffer here
        
        WAIT                  ; End
        
IOAR    DC    320             ; Word count (1 sector)
        DC    /64             ; Sector 100 (hex)
        DC    /0000           ; Read function
        DC    BUFFER          ; Buffer address
        
BUFFER  BSS   320             ; Sector buffer
```

### Example 3: Character Conversion

```assembly
        ; Convert card code to console printer code
        
        ORG   /300
START   LD    CARD            ; Get card character
        BSI   CONVERT         ; Convert it
        STO   CONS            ; Store console code
        
        ; Print to console
        XIO   CONS,L,001,08   ; Write to console (device 001, function 08)
        
        WAIT
        
CONVERT DC    *-*             ; Entry point
        ; Lookup in conversion table
        LDX 1 CVTTAB          ; Point to table
LOOP    S   1 *+1             ; Compare to table entry
        BSC   FOUND,Z         ; Branch if found
        MDX 1 +2              ; Next table entry (2 words)
        MDX   LOOP            ; Keep searching
        
FOUND   LD  1 +1              ; Get console code
        BSC I CONVERT         ; Return
        
        ; Conversion table: card code, console code
CVTTAB  DC    /9000           ; 'A' in card code
        DC    /3C             ; 'A' in console code (UC)
        DC    /8800           ; 'B' in card code
        DC    /18             ; 'B' in console code (UC)
        ; ... more entries ...
        
CARD    DC    /9000           ; Test data: 'A'
CONS    DC    0               ; Result
```

## Technical Notes

### Version History

- **DMS Version 1**: Original release
- **DMS Version 2**: Major update with improved performance
- **V2M12**: Version 2, Modification 12 (latest in S1130 emulator)

Modifications added:
- M1-M4: Bug fixes
- M5: 1403 printer support
- M6-M7: 1442-6/7 support
- M8: Macro update program
- M9-M12: Additional bug fixes and enhancements

### Limitations

- **Single User**: No multiprogramming or multitasking
- **Single Job**: One job at a time
- **No Virtual Memory**: All code must fit in physical memory
- **Fixed Partitions**: Memory areas predefined at system generation
- **Sequential Devices**: No random access for cards or tape
- **Limited File System**: Simple LET directory, no hierarchical structure

### Compatibility

DMS is compatible with:
- All IBM 1130 models (1131 CPU)
- 2K, 4K, 8K, 16K, or 32K core configurations
- Various device combinations

DMS programs are **not cycle-accurate** - they rely on functional behavior, not timing. This means:
- Programs may run faster on emulator than real hardware
- Timing-sensitive code may not work correctly
- Interrupt priorities are functional, not timing-based

## References

### DMS Source Files

Located in `ForReferenceOnly/dmsr2v12/`:
- `asysldr1.asm` - System loader Phase 1
- `csysldr2.asm` - System loader Phase 2
- `dbootcd.asm` - Boot card
- `emonitor.asm` - Monitor
- `jlptface.asm` - Paper tape interface
- Many other phase sources

### Documentation

- `docs/InternalOperation.md` - S1130 emulator internals
- `docs/Assembler.md` - Assembler reference
- `ForReferenceOnly/functional/` - IBM 1130 Functional Characteristics
- IBM 1130 DMS manuals (various)

## Summary

The IBM 1130 Disk Monitor System is a sophisticated operating system for its era, providing:
- Complete program development environment
- Multiple language support (FORTRAN, RPG, Assembler)
- Device-independent I/O through conversion routines
- Efficient disk-based storage and retrieval
- Interrupt-driven I/O for performance
- Comprehensive error handling

Understanding DMS architecture is essential for:
- Implementing accurate device emulation
- Writing system-level software
- Debugging low-level issues
- Appreciating the engineering of 1960s operating systems

The multi-encoding I/O system, phase-based loading, and interrupt architecture demonstrate advanced operating system concepts that remain relevant today.
