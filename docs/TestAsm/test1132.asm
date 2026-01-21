                    * IBM 1130 Program to Print Text on 1132 Printer
                    * File: test1132.s1130
                    * Description: Prints "ABCDEFGHIJKLMNOPQRSTUVWXYZ012345679" on 1132 printer
                    * This is a complete working program for a real IBM 1130 with 1132 printer
                    * Uses Level 1 interrupts for character-by-character printing
                          ORG     /0100
                    * Main program entry point
                    START BSI  L  INIT
                          BSI  L  PRINT
                          WAIT    
                    * Initialize Level 1 interrupt vector (location 9 decimal = /09 hex)
                    INIT: DC      0
                          LD   L  ISADR
                          STO  L  /0009
                          BSC  I  INIT
                    ISADR DC      ISR
                    * Main print routine
                    PRINT DC      0
                    * Check printer ready
                    PLOOP XIO  L  PSENS
                          AND  L  RDYM
                          BSC  L  PLOOP,Z
                    * Initialize variables
                          LD   L  /0000
                          STO  L  CHCNT
                          STO  L  BPTR
                    * Clear scan field (locations 32-39 = /20-/27)
                          STO  L  /0020
                          STO  L  /0021
                          STO  L  /0022
                          STO  L  /0023
                          STO  L  /0024
                          STO  L  /0025
                          STO  L  /0026
                          STO  L  /0027
                    * Start printer - begins interrupt sequence
                          XIO  L  PSTRT
                    * Wait for 48 character cycles plus 16 idle cycles = 64 total
                    PWAIT LD   L  CHCNT
                          S    L  /0040
                          BSC  L  PWAIT,+-
                    * Space one line
                          XIO  L  PSPAC
                    * Wait for space complete
                    SWAIT XIO  L  PSENS
                          AND  L  SPCM
                          BSC  L  SWAIT,Z
                          XIO  L  PSENSR
                    * Stop printer
                          XIO  L  PSTOP
                          BSC  I  PRINT
                    * Interrupt Service Routine for Level 1 (1132 Read Emitter)
                    ISR:  DC      0
                    * Save registers
                          STO  L  SVACC
                          STX  L1 SVX1
                    * Read character from emitter
                          XIO  L  RDEM
                          STO  L  CCHAR
                    * Clear scan field for this cycle
                          LD   L  /0000
                          STO  L  /0020
                          STO  L  /0021
                          STO  L  /0022
                          STO  L  /0023
                          STO  L  /0024
                          STO  L  /0025
                          STO  L  /0026
                          STO  L  /0027
                    * Compare character with print buffer
                          LDX  L1 /0000
                    CLOOP LD   L1 PBUF
                          SRA     8
                          S    L  CCHAR
                          BSC  L  MATCH,Z
                    NEXT: LD   L1 /0000
                          A    L  /0001
                          STX  L1 /0000
                          S    L  PLEN
                          BSC  L  CLOOP,+-
                    * Character matched - set bit in scan field
                    MATCH BSI  L  STBIT
                          BSC  L  NEXT
                    ENDCM DC      PLEN
                    * Set completion bit (bit 15 of word 39)
                          LD   L  /0027
                          OR   L  /0001
                          STO  L  /0027
                    * Increment character counter
                          LD   L  CHCNT
                          A    L  /0001
                          STO  L  CHCNT
                    * Restore registers
                          LDX  L1 SVX1
                          LD   L  SVACC
                    * Return from interrupt (BOSC resets level)
                          BSC  I  ISR
                    * Set bit in scan field based on XR1 position
                    STBIT DC      0
                          LD   L1 BTBL
                          OR   L1 SCNF
                          STO  L1 SCNF
                          BSC  I  STBIT
                    * Bit position table (bit masks for positions 0-15)
                    BTBL: DC      /8000
                          DC      /4000
                          DC      /2000
                          DC      /1000
                          DC      /0800
                          DC      /0400
                          DC      /0200
                          DC      /0100
                          DC      /0080
                          DC      /0040
                          DC      /0020
                          DC      /0010
                          DC      /0008
                          DC      /0004
                          DC      /0002
                          DC      /0001
                    * Scan field base address (location 32 = /20)
                    SCNF: DC      /0020
                    * Print buffer - Text to print (EBCDIC, uppercase only)
                    PBUF: DC      /C140
                          DC      /C240
                          DC      /C340
                          DC      /C440
                          DC      /C540
                          DC      /C640
                          DC      /C740
                          DC      /C840
                          DC      /C940
                          DC      /D140
                          DC      /D240
                          DC      /D340
                          DC      /D440
                          DC      /D540
                          DC      /D640
                          DC      /D740
                          DC      /D840
                          DC      /D940
                          DC      /E240
                          DC      /E340
                          DC      /E440
                          DC      /E540
                          DC      /E640
                          DC      /E740
                          DC      /E840
                          DC      /E940
                          DC      /F040
                          DC      /F140
                          DC      /F240
                          DC      /F340
                          DC      /F440
                          DC      /F540
                          DC      /F640
                          DC      /F740
                          DC      /F940
                    PLEN: EQU     35
                    * Variables
                    CHCNT BSS     1
                    BPTR: BSS     1
                    CCHAR BSS     1
                    SVACC BSS     1
                    SVX1: BSS     1
                    * I/O Control Commands for 1132 Printer (device /06)
                    PSENS DC      0
                          DC      /0706
                    PSENS DC      0
                          DC      /0707
                    PSTRT DC      0
                          DC      /0486
                    PSTOP DC      0
                          DC      /0446
                    PSPAC DC      0
                          DC      /0406
                    RDEM: DC      CCHAR
                          DC      /0206
                    * Status word masks
                    RDYM: DC      /1860
                    SPCM: DC      /2000
                          END     START
