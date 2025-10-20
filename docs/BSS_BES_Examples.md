# BSS and BES Directive Examples

## Understanding BSS and BES

Both `BSS` (Block Started by Symbol) and `BES` (Block Ended by Symbol) reserve blocks of memory, but they differ in how the label is assigned:

- **BSS**: Label refers to the **leftmost (first)** word of the reserved area
- **BES**: Label refers to the **rightmost word + 1** (i.e., the next available location after the reserved area)

## Example 1: Basic BSS

```assembly
        ORG  /100
BUFFER  BSS  10
        DC   /1234
```

Memory layout:
- `BUFFER` = `/100` (leftmost word)
- Reserved area: `/100` through `/109` (10 words)
- `DC /1234` is stored at `/10A`

## Example 2: Basic BES

```assembly
        ORG  /100
        DC   /5678
BUFEND  BES  10
        DC   /ABCD
```

Memory layout:
- `DC /5678` is stored at `/100`
- Reserved area starts at `/101`
- Reserved area: `/101` through `/10A` (10 words)
- `BUFEND` = `/10B` (rightmost `/10A` + 1)
- `DC /ABCD` is stored at `/10B` (same location as `BUFEND`)

## Example 3: Using BSS for Array with Known Start

```assembly
        ORG  /100
ARRAY   BSS  50       * Reserve 50-word array
        * ARRAY points to first element at /100
        * Array occupies /100 through /131
        LD   L ARRAY  * Load from first element
```

Use BSS when you need to reference the **start** of a buffer.

## Example 4: Using BES for Array with Known End

```assembly
        ORG  /100
        BES  50
AEND    DC   0        * AEND marks end of 50-word area
        * Reserved area is /100 through /131
        * AEND = /132 (one past the end)
```

Use BES when you need to reference the **end** of a buffer (useful for boundary checking).

## Example 5: Combined BSS and BES

```assembly
        ORG  /100
START   BSS  20       * START = /100 (first word)
        DC   /9999
END     BES  20       * Reserve another 20 words
                      * END = /129 (one past last word)
        DC   /FFFF    * At /129 (same as END)
```

Memory layout:
- `START` = `/100` (leftmost of first block)
- First reserved block: `/100` through `/113` (20 words)
- `DC /9999` at `/114`
- Second reserved block: `/115` through `/128` (20 words)
- `END` = `/129` (rightmost of second block + 1)
- `DC /FFFF` at `/129`

## Example 6: Even Alignment

Both BSS and BES support the `E` modifier for even boundary alignment:

```assembly
        ORG  /101     * Start at odd address
DBUF    BSS  E 10     * Align to even, then reserve
                      * DBUF = /102 (next even address)
                      * Reserved: /102 through /10B
```

The `E` modifier is useful when working with double-precision instructions that require even-aligned operands.

## Example 7: BSS with Zero Count

```assembly
        ORG  /101     * Odd address
        BSS  E 0      * Just align, don't reserve
        DC   /1234    * Will be at /102 (even)
```

A BSS with count 0 and `E` modifier can be used solely for alignment purposes.

## Important Notes

1. **BSS does NOT clear memory**: The IBM 1130 documentation states "it should not be assumed that an area reserved by a BSS instruction contains zeros." However, this emulator initializes reserved areas to zero for deterministic behavior.

2. **BES is identical to BSS except for label placement**: The only difference is where the label points - to the start (BSS) or the end+1 (BES).

3. **Use cases**:
   - Use **BSS** when you need a pointer to the start of a buffer (most common)
   - Use **BES** when you need a pointer to the end of a buffer (for boundary checks or when building buffers from the end backward)

## Real-World Example from IBM 1130 DMS

From the Disk Monitor System source code:

```assembly
* Input/Output Control System buffers
START   BSS  E 0      * Ensure even alignment
SENSR   BSS  80       * Sense/Read buffer (80 words)
COUNT   EQU  SENSR    * Alias for buffer start
        BES  E 0      * Align after buffer
```

This creates an 80-word buffer with:
- `SENSR` pointing to the first word
- `COUNT` as an alias to `SENSR`
- Even alignment before and after the buffer
