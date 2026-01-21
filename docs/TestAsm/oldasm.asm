                          HDNG    P.T. INPUT SYSTEM LOADER - PHASE 2    CP200010
                    *************************************************** CP200020
                    *                                                 * CP200030
                    *STATUS - VERSION 2, MODIFICATION 9               * CP200040
                    *                                                 * CP200050
                    *FUNCTION/OPERATION-                              * CP200060
                    *   PHASE 2 WILL PERFORM THE FOLLOWING DURING     * CP200070
                    *   AN INITIAL LOAD-                              * CP200080
                    *   * LOAD THE SYSTEM PROGRAMS TO DISK AS         * CP200090
                    *     DIRECTED BY THE LOAD MODE AND PHID RECORDS. * CP200100
                    *   * ESTABLISH SLET.                             * CP200110
                    *   * ESTABLISH THE RELOAD TABLE.                 * CP200120
                    *   * INITIALIZE THE RESIDENT IMAGE AND DCOM.     * CP200130
                    *   * DEFINE PRINCIPAL PRINT AND I/O DEVICES.     * CP200140
                    *   * ESTABLISH ADDRESS OF AND INITIALIZE FIRST   * CP200150
                    *     LET SECTOR.                                 * CP200160
                    *   * PASS CONTROL TO AUXILIARY SUPERVISOR TO CALL* CP200170
                    *     DUP TO LOAD THE SUBROUTINE LIBRARY.         * CP200180
                    *                                                 * CP200190
                    *   A RELOAD-                                     * CP200200
                    *   * RELOAD ANY PHASE(S) CURRENTLY IN SLET.      * CP200210
                    *   * EXPAND RELOADED SYSTEMS PROGRAMS INTO       * CP200220
                    *     CUSHION AREA IF NECESSARY AND SPACE PERMITS.* CP200230
                    *   * COMPRESS OUT OLD OR NEW GAPS BETWEEN     2-4* CP200240
                    *     SYSTEM PROGRAMS (CAUSED BY REDUCED PHASE 2-4* CP200250
                    *     LENGTH DURING A RELOAD)                  2-4* CP200260
                    *   * ADD ONE OR MORE ADDITIONAL PHASES TO A      * CP200270
                    *     SYSTEM PROGRAM CURRENTLY IN SLET.           * CP200280
                    *   * ADD ONE OR MORE NEW PROGRAMS AS SPECIFIED   * CP200290
                    *     IN A SECOND PHID RECORD TO THE SYSTEM       * CP200300
                    *     PROGRAM AREA.                               * CP200310
                    *   * SHIFT THE SCRA, CIB, LET AND USER AREA TO   * CP200320
                    *     MAKE ROOM FOR ADDED PROGRAMS.               * CP200330
                    *   * UPDATE SLET.                                * CP200340
                    *   * UPDATE AND REPROCESS THE RELOAD TABLE.      * CP200350
                    *   * UPDATE THE RESIDENT IMAGE AND DCOM.         * CP200360
                    *   * UPDATE THE CHAIN ADDRESSES IN LET IF        * CP200370
                    *     PROGRAMS WERE ADDED.                        * CP200380
                    *   * REDEFINE PRINCIPAL PRINT AND I/O DEVICES AS * CP200390
                    *     DIRECTED BY THE CONFIGURATION RECORDS.      * CP200400
                    *   * DISPLAY 'END RELOAD' AT COMPLETION.         * CP200410
                    *                                                 * CP200420
                    *ENTRY POINT-                                     * CP200430
                    *   * ENTERED AT 'BA000' FROM PHASE 1.            * CP200440
                    *                                                 * CP200450
                    *INPUT-                                           * CP200460
                    *   * BINARY INPUT RECORDS ONLY.                  * CP200470
                    *                                                 * CP200480
                    *OUTPUT-                                          * CP200490
                    *   * OPERATIONAL SYSTEM CARTRIDGE, LOGICAL DRIVE * CP200500
                    *     ZERO.                                       * CP200510
                    *                                                 * CP200520
                    *EXTERNAL REFERENCES-                             * CP200530
                    *   * NONE.                                       * CP200540
                    *                                                 * CP200550
                    *EXIT-                                            * CP200560
                    *   * BRANCH TO $DUMP AT THE END OF INITIAL LOAD. * CP200570
                    *   * AT END OF A RELOAD BRANCH TO $DUMP IF TYPE  * CP200580
                    *     '81' RECORD FOLLWED BY A '// XEQ MODIF'     * CP200590
                    *     RECORD.  OTHERWISE BRANCH TO $EXIT.         * CP200600
                    *                                                 * CP200610
                    *TABLES/BUFFERS-                                  * CP200620
                    *   * BA902- 60 WORD BUFFER INTO WHICH DATA FROM  * CP200630
                    *            BA906 IS PACKED.                     * CP200640
                    *   * BA908- 1 WD BUFFER FOR DELETE CHARACTERS.   * CP200650
                    *   * BA906- 108 WD BUFFER FOR 108 FRAMES BINARY  * CP200660
                    *            PAPER TAPE DATA, LEFT JUSTIFIED.     * CP200670
                    *   * BUFR1- 320 WORD AREA IN WHICH THE RELOAD    * CP200680
                    *            TABLE IS CONSTRUCTED.                * CP200690
                    *   * BUFR2- 320 WORD DISK I/O BUFFER USED        * CP200700
                    *            PRIMARILY FOR DATA.                  * CP200710
                    *   * BUFR3- 320 WORD DISK I/O BUFFER USED        * CP200720
                    *            PRIMARILY FOR SLET.                  * CP200730
                    *                                                 * CP200740
                    *ATTRIBUTES-                                      * CP200750
                    *   * N/A                                         * CP200760
                    *                                                 * CP200770
                    *NOTES-                                           * CP200780
                    *   * THIS PHASE IS ASSEMBLED IN RELOCATABLE-     * CP200790
                    *     ABSOLUTE FORMAT.                            * CP200800
                    *   * IN ADDITION TO THE FIRST CARD, SECTOR BREAK * CP200810
                    *     CARDS PRECEDE SUBPHASES 1, 2, AND 3.  PHASE * CP200820
                    *     1 USES ABSOLUTE SECTOR ADDRESSES TO LOAD    * CP200830
                    *     PHASE 2.                                    * CP200840
                    *   * PROGRAM REVISED IN MOD 3.                     CP200850
                    *                                                 * CP200860
                    *************************************************** CP200870
                          HDNG    P.T. INPUT SYSTEM LOADER - PHASE 2    CP200880
                          ABS                                           CP200890
                    *                                                   CP200900
                    * COMMA EQUATES                                     CP200910
                    *                                                   CP200920
                    $ACDE EQU     /9F  TABLE OF AREA CODES              CP200930
                    $CH12 EQU     /06  CHANNEL 12 INDICATOR             CP200940
                    $CILA EQU     /5A  ADDRESS OF END OF DISK SUBR      CP200950
                    $COMN EQU     /07  LENGTH OF COMMON (IN WORDS)      CP200960
                    $CORE EQU     /0E  SIZE OF CORE                     CP200970
                    $CPTR EQU     /7E  CNSL PTR CHANNEL 12 INDICATOR    CP200980
                    $CTSW EQU     /0F  CONTROL RECORD TRAP SWITCH       CP200990
                    $CIBA EQU     /05  SCTR ADDR OF CIB                 CP201000
                    $CYLN EQU     /9A  ARM POSITION                     CP201010
                    $DBSY EQU     /EE  NON-ZERO WHEN DISKZ BUSY         CP201020
                    $DADR EQU     /10  BLK ADDR OF PROG TO BE LOADED    CP201030
                    $DCDE EQU     /77  DRIVE CODE OF PROG IN WRK STG    CP201040
                    $DCYL EQU     /A4  TABLE OF DEFECTIVE CYLINDERS     CP201050
                    $DREQ EQU     /12  IND. FOR REQUESTED VERSION DKI/O CP201060
                    $DUMP EQU     /3F  CALL DUMP ENTRY POINT            CP201070
                    $DZIN EQU     /76  DISK SUBROUTINE IN CORE INDR     CP201080
                    $EXIT EQU     /38  CALL EXIT ENTRY POINT            CP201090
                    $FLSH EQU     /71  FLUSH JOB SWITCH                 CP201100
                    $FPAD EQU     /95  TABLE OF FILE PROTECT ADDRESSES  CP201110
                    $HASH EQU     /14  WORK AREA                        CP201120
                    $IBSY EQU     /13  NON-ZERO IF CD/PAP TP DEV. BUSY  CP201130
                    $IBT2 EQU     /B9  LEVEL 2 INTRPT BRANCH TABLE ADDR CP201140
                    $IBT4 EQU     /D4  ADDR OF THE IBT, LEVEL 4         CP201150
                    $IOCT EQU     /32  ZERO IF NO I/O IN PROGRESS       CP201160
                    $IREQ EQU     /2C  ADDR OF INT REQUEST SUBROUTINE   CP201170
                    $I200 EQU     /B3  ILS02 ENTRY POINT                CP201180
                    $I205 EQU     /B8  ILS02 INTERRUPT ENTRY POINT      CP201190
                    $I400 EQU     /C4  ILS04 ENTRY POINT                CP201200
                    $I403 EQU     /D0                                   CP201210
                    $KCSW EQU     /7C  1 IF KB,CP BOTH UTILIZED         CP201220
                    $LAST EQU     /33  LAST CARD INDICATOR              CP201230
                    $LEV0 EQU     /08  LEVEL 0 BRANCH ADDRESS           CP201240
                    $LEV1 EQU     /09  LEVEL 1 BRANCH ADDRESS           CP201250
                    $LEV2 EQU     /0A  LEVEL 2 BRANCH ADDRESS           CP201260
                    $LEV3 EQU     /0B  LEVEL 3 BRANCH ADDRESS           CP201270
                    $LEV4 EQU     /0C  LEVEL 4 BRANCH ADDRESS           CP201280
                    $LEV5 EQU     /0D  LEVEL 5 BRANCH ADDRESS           CP201290
                    $LINK EQU     /39  CALL LINK ENTRY POINT            CP201300
                    $NDUP EQU     /34  DO NOT DUP IF NON-ZERO           CP201310
                    $NXEQ EQU     /35  DO NOT EXECUTE IF NON-ZERO       CP201320
                    $PBSY EQU     /36  NON-ZERO WHEN PRINTER BUSY       CP201330
                    $PGCT EQU     /37  PAGE NO. FOR HEADINGS            CP201340
                    $PHSE EQU     /78  NO. OF PHASE NOW IN CORE         CP201350
                    $PRET EQU     /28  IOCS SOFT ERROR TRAP             CP201360
                    $PST1 EQU     /81  TRAP FOR I/O ERRORS, LEVEL 1     CP201370
                    $PST2 EQU     /85  TRAP FOR I/O ERRORS, LEVEL 2     CP201380
                    $PST3 EQU     /89  TRAP FOR I/O ERRORS, LEVEL 3     CP201390
                    $PST4 EQU     /8D  TRAP FOR I/O ERRORS, LEVEL 4     CP201400
                    $SNLT EQU     /EF  SENSE LIGHT INDICATOR            CP201410
                    $STOP EQU     /91  PROGRAM STOP KEY TRAP            CP201420
                    $SYSC EQU     /E0  MODIFICATION LEVEL               CP201430
                    $UFDR EQU     /7D  DRIVE CODE OF UNFORMATTED I/O    CP201440
                    $UFIO EQU     /79  UNFORMATTED I/O RECORD NO.       CP201450
                    $ULET EQU     /2D  TABLE OF LET ADDRESSES           CP201460
                    $WSDR EQU     /7A  WORKING STORAGE DRIVE CODE       CP201470
                    $WRD1 EQU     /7B  ADDR OF 1ST WD OF CORE LOAD      CP201480
                    $ZEND EQU     /1E0 END OF DISKZ                     CP201490
                    $1132 EQU     /7F  1132 PRINTER CHANNEL 12 INDR     CP201500
                    $1403 EQU     /80  1403 PRINTER CHANNEL 12 INDR     CP201510
                    *                                                   CP201520
                    * ABSOLUTE EQUATES                                  CP201530
                    *                                                   CP201540
                    @IDAD EQU     /0000     CART ID, COLD START SAD     CP201550
                    @DCOM EQU     /0001     DCOM SECTOR ADDRESS         CP201560
                    @RIAD EQU     /0002     RESIDENT IMAGE SECTOR ADDR  CP201570
                    @SLET EQU     /0003     FIRST SLET SECTOR ADDRESS   CP201580
                    @RLTB EQU     /0006     RELOAD TABLE SECTOR ADDRESS CP201590
                    @HDNG EQU     /0007     PAGE HEADING SECTOR ADDRESS CP201600
                    @P2AD EQU     /0630     SYS LDR - PHASE 2 SCTR ADDR CP201610
                    @MSCT EQU     /0658     MAXIMUM SECTOR ADDRESS - 1  CP201620
                    @COLD EQU     /0030     RLTV ADDR 1ST WD COLD START CP201630
                    @CSTR EQU     /00D0                                 CP201640
                    DZ000 EQU     /00F2     DISKZ ENTRY POINT           CP201650
                    @WDCT EQU     0         RLTV ADDR WORD COUNT IN BFR CP201660
                    @SADR EQU     1         RLTV ADDR SCTR ADDR IN BFR  CP201670
                    @NDCY EQU     3         MAX NO. OF DEF CYLINDERS    CP201680
                    @MXDR EQU     5         MAX NO. OF DISK CRIVES      CP201690
                    @CIDN EQU     3         RLTV ADDR OF CARTRIDGE ID   CP201700
                    @STAT EQU     7         RLTV ADDR CART STATUS WORD  CP201710
                    @PRWC EQU     60        PRINTER BUFFER LENGTH       CP201720
                    @CCOL EQU     80        NUMBER OF COLUMNS PER CARD  CP201730
                    @SCNT EQU     320       NUMBER OF WORDS PER SECTOR  CP201740
                    @MNCR EQU     4096      MINIMUM ALLOWABLE CORE SIZE CP201750
                    *                                                   CP201760
                    * SYSTEM DEVICE SUBROUTINE PHASE IDS                CP201770
                    *                                                   CP201780
                    P1403 EQU     140       PHASE ID OF SYS 1403 SUBR   CP201790
                    P1132 EQU     141       PHASE ID OF SYS 1132 SUBR   CP201800
                    PCPAD EQU     142       PHASE ID OF SYS C. P. SUBR  CP201810
                    I2501 EQU     143       PHASE ID OF SYS 2501 SUBR   CP201820
                    I1442 EQU     144       PHASE ID OF SYS 1442 SUBR   CP201830
                    I1134 EQU     145       PHASE ID OF SYS PAPT SUBR   CP201840
                    IKBRD EQU     146       PHASE ID OF SYS KEYBRD SUBR CP201850
                    CDCNV EQU     147       PH ID OF SYS 2501/1442 CONV CP201860
                    C1134 EQU     148       PHASE ID OF SYS 1134 CONV   CP201870
                    CKBRD EQU     149       PHASE ID OF SYS KEYBRD CONV CP201880
                    DISKZ EQU     150       PHASE ID OF DISKZ SUBR      CP201890
                    DISK1 EQU     151       PHASE ID OF DISK1 SUBR      CP201900
                    DISKN EQU     152       PHASE ID OF DISKN SUBR      CP201910
                    PRINT EQU     153       PH ID OF PRINC PRINT SUBR   CP201920
                    PINPT EQU     154       PH ID OF PRINC INPUT SUBR   CP201930
                    PIDEV EQU     155       PRINC INPUT EXCLUDING KEYBD CP201940
                    CNVRT EQU     156       PRINC SYS CONVERSION SUBR   CP201950
                    CVRT  EQU     157       PRINC CONV EXCLUDING KEYBRD CP201960
                    *                                                   CP201970
                    * SYSTEM LOADER COMMUNICATIONS AREA                 CP201980
                    *                                                   CP201990
                    CILWC EQU     /01F0     CORE IMAGE LOADER WORD CNT  CP202000
                    DCYL1 EQU     CILWC+2   SCTR ADDR OF 1ST DEF CYLIN  CP202010
                    DCYL2 EQU     DCYL1+1   SCTR ADDR OF 2ND DEF CYLIN  CP202020
                    DCYL3 EQU     DCYL2+1   SCTR ADDR OF 3RD DEF CYLIN  CP202030
                    LMODE EQU     DCYL3+1   LOAD MODE IMAGE             CP202040
                    CAREA EQU     LMODE+1   NO. SECTORS OF CUSHION AREA CP202050
                    T1442 EQU     CAREA+1   SET 1 IF 1442-6 OR 7 AVAIL  CP202060
                    DINDR EQU     T1442+1   ISS DEVICE INDICATOR        CP202070
                    VERSN EQU     DINDR+1   VERSION AND MODIFICATION NO CP202080
                    CIBFR EQU     VERSN+1   SCTR ADDR OF CORE IMAGE BFR CP202090
                    SCORE EQU     CIBFR+1   CORE SIZE                   CP202100
                    MAXPH EQU     SCORE+1   MAXIMUM PHASE ID            CP202110
                    ASCRA EQU     MAXPH+1   SECTOR ADDRESS OF SCRA      CP202120
                    CARID EQU     ASCRA+1   ID OF CARTRIDGE             CP202130
                    FLETI EQU     CARID+1   FIXED AREA INDICATOR        CP202140
                    FPADR EQU     FLETI+1   FILE PROTECT ADDRESS        CP202150
                    SSBFR EQU     FPADR+1   NO. OF SCTR FOR SLET BFR    CP202160
                    LET00 EQU     SSBFR+1   PRE-LOAD LET SECTOR ADDRESS CP202170
                    SHIFT EQU     LET00+1   NO. OF SCTRS TO SHIFT       CP202180
                    *                                                   CP202190
                    *                       COMMUNICATIONS PATCH AREA   CP202200
                    *                                                   CP202210
                    PTRID EQU     SHIFT+1+2 PH ID OF PRINC PRINT SUBR   CP202220
                    CHN12 EQU     PTRID+1   ADDRESS OF CHANNEL 12 INDR  CP202230
                    RDRID EQU     PTRID+2   PH ID OF PRINC I/O SUBR     CP202240
                    CNVID EQU     RDRID+1   PH ID OF PRINC CONV SUBR    CP202250
                    PRNRD EQU     CNVID+1   PRINCIPAL I/O DEVICE INDR   CP202260
                    PRNPR EQU     PRNRD+1   PRINCIPAL PRINT DEVICE INDR CP202270
                    *                                                   CP202280
                    PAIR1 EQU     PRNPR+1   PH ID PAIRS AT EVEN LOC'S   CP202290
                    PAIR2 EQU     PAIR1+2                               CP202300
                    PAIR3 EQU     PAIR2+2                               CP202310
                    PAIR4 EQU     PAIR3+2                               CP202320
                    PAIR5 EQU     PAIR4+2                               CP202330
                    PAIR6 EQU     PAIR5+2                               CP202340
                    PAIR7 EQU     PAIR6+2                               CP202350
                    PAIR8 EQU     PAIR7+2                               CP202360
                    PAIR9 EQU     PAIR8+2                               CP202370
                    PAIRA EQU     PAIR9+2                               CP202380
                    PAIRB EQU     PAIRA+2                               CP202390
                    PAIRC EQU     PAIRB+2                               CP202400
                    PAIRD EQU     PAIRC+2                               CP202410
                    PAIRE EQU     PAIRD+2                               CP202420
                    PAIRF EQU     PAIRE+2                               CP202430
                    LPHID EQU     PAIRF+1   LAST PH ID FOR INITIAL LOAD CP202440
                    LOLIM EQU     LPHID+1   LOWER PHASE ID BYPASS LIMIT CP202450
                    UPLIM EQU     LOLIM+1   UPPER PHASE ID BYPASS LIMIT CP202460
                    MSG01 EQU     UPLIM+10  E 01 CHECKSUM ERROR         CP202470
                    MSG02 EQU     MSG01+12  E 02 INVALID RCRD OR BLANK  CP202480
                    MSG03 EQU     MSG02+16  E 03 SEQ ERR OR MISSING RCD CP202490
                    MSG04 EQU     MSG03+19  E 04 ORG BACKWARD           CP202500
                    MSG05 EQU     MSG04+11  E 05 INITIALIZE THE CART    CP202510
                    INTPT EQU     MSG05+17  INHIBIT INTRPT REQ SUBR ENT CP202520
                    WRTYZ EQU     INTPT+7   CONSOLE PRINTER SUBR ENTRY  CP202530
                    TZ100 EQU     WRTYZ+2   CONSOLE PTR INTERRUPT ENTRY CP202540
                    PT000 EQU     WRTYZ+59  PAPER TAPE SUBR ENTRY       CP202550
                    PT010 EQU     PT000+3   PAPER TAPE INTERRUPT EXIT   CP202560
                          HDNG    PHASE 2 INITIALIZATION                CP202570
                          ORG     /0376     BEGIN AT END OF DEVICE SUBR CP202580
                    *                                                   CP202590
                    *                                                   CP202600
                          DC      BZ999-BA000+1  WORD COUNT OF PHASE 2  CP202610
                          DC      @P2AD          SCTR ADDR OF PHASE 2   CP202620
                    *                                                   CP202630
                    *                                                   CP202640
                    BA000 NOP               A WAIT MAY BE PATCHED HERE  CP202650
                          LDX  L1 BZ930-1   STORE ADDRESS OF INTERRUPT  CP202660
                          STX  L1 $IBT4     *BRANCH TABLE IN ILS04      CP202670
                          LDX  L3 X         XR3 POINTS TO CONSTANTS     CP202680
                          LD   L  LMODE     FETCH AND                   CP202690
                          STO   3 BY934-X   *STORE LOAD MODE INDICATOR  CP202700
                          BSC  L  BA010,-   BRANCH IF A RELOAD FUNC     CP202710
                          BSI  L  FTCH3     ZERO SLET BUFFER VIA  READ  CP202720
                          LD   L  PAIRF+1   FETCH AND                   CP202730
                          STO   3 BY924-X   *STORE LAST PHASE ID        CP202740
                          MDX     BB000     BRANCH TO READ A RECORD     CP202750
                    BA010 LD   L  CIBFR     FETCH AND                   CP202760
                          STO   3 BY935-X   *STORE CIB SECTOR ADDRESS   CP202770
                    * CLEAR SIGN BITS IN SLET TABLE SCTR ADDRESSES. 2-4 CP202780
                    * LOCATE LAST PH ID & LAST USED SCTR ADDRESS.   2-4 CP202790
                    * COMPRESS OUT EXISTING GAPS BETWEEN            2-4 CP202800
                    * SYSTEM PROGRAMS.                              2-4 CP202810
                          BSI  L  B0000     GO DO ABOVE 3 FUNCTIONS     CP202820
                          HDNG    READ A RECORD                         CP202830
                    *************************************************** CP202840
                    *                                                   CP202850
                    * READ A RECORD                                     CP202860
                    *                                                   CP202870
                    *                                                   CP202880
                    BB000 NOP               A 'WAIT' MAY BE PATCHED IN  CP202890
                          BSI     BB050     TEST FOR BUSY               CP202900
                    *                                                   CP202910
                    BB010 LDD     BB904     TEST 1 CHAR FOR DELETE      CP202920
                          BSI  L  PT000+1   READ 1 FRAME                CP202930
                          BSI     BB050     WAIT FOR OP COMPLETE        CP202940
                          LD   L  BA908+1   TEST FRAME READ IN          CP202950
                          EOR     BB912     *AGAINST DELETE CODE AND    CP202960
                          BSC  L  BB010,+-  *READ AGAIN IF YES          CP202970
                    *                                                   CP202980
                          LD   L  BA908+1   TEST FOR VALID WORD COUNT   CP202990
                          S       BB913                                 CP203000
                          BSI  L  ER018,Z-  ERROR IF WD CNT TOO LARGE   CP203010
                    *                                                   CP203020
                          LD   L  BA908+1                               CP203030
                          SRA     7         FORM FRAME COUNT            CP203040
                          STO  L  BA906     *AHEAD OF BUFFER            CP203050
                          SRA     1         FORM WORD COUNT             CP203060
                          STO  L  CK030+1   *FOR CHECKSUM PROCESSING    CP203070
                    *                                                   CP203080
                    BB020 LDD     BB906     FETCH PARAMETERS            CP203090
                          BSI  L  PT000+1   *TO READ BINARY RECORD      CP203100
                          BSI     BB050     WAIT FOR OP COMPLETE        CP203110
                    *                                                   CP203120
                    *                                                   CP203130
                          BSI  L  PACK0     COMPRESS AA906 INTO AA902   CP203140
                          BSC  L  BC000     DETERMINE RECORD TYPE       CP203150
                    *                                                   CP203160
                    *                                                   CP203170
                    BB901 DC      54*2      FRAME CNT  (FULL LENGTH)    CP203180
                    *************************************************** CP203190
                    *                                                   CP203200
                    BB050 DC      0         ENTRY/RETURN                CP203210
                          LD   L  $IBSY      TEST FOR PAPER TAPE BUSY   CP203220
                          BSC  L  *-4,Z                                 CP203230
                          BSC  I  BB050     RETURN                      CP203240
                    *************************************************** CP203250
                          BSS  E            CONSTANTS AND BUFFERS       CP203260
                    *************************************************** CP203270
                    BB904 DC      /7000     READ WITHOUT CONVERSION     CP203280
                          DC      BA908     BUFFER LOCATION             CP203290
                    BB906 DC      /7000     READ WITHOUT CONVERSION     CP203300
                          DC      BA906     108 FRAME BUFFER            CP203310
                    *                                                   CP203320
                    BB912 DC      /7F00     DELETE CODE CONSTANT        CP203330
                    BB913 DC      /3600     MAXIMUM VALID WORD COUNT    CP203340
                    *                                                   CP203350
                          BSS  E                                        CP203360
                    BA902 DC      *-*       FIRST WORD AFTER PACKING    CP203370
                    PKBFR EQU     BA902     PACKED BINARY INPUT BUFFER  CP203380
                    BA906 DC      *-*       WORD COUNT TO BE SET        CP203390
                          BSS     108       UNPACKED BUFFER             CP203400
                          DC      /FFFF     END OF FRAME BUFFER         CP203410
                    *                                                   CP203420
                    BA908 DC      1         1-WORD DELETE BUFFER        CP203430
                          DC      *-*                                   CP203440
                    *                                                   CP203450
                    *                                                   CP203460
                    *************************************************** CP203470
                    *                                                   CP203480
                    *     THIS SUBROUTINE PACKS LEFT-JUSTIFIED 8-BIT    CP203490
                    *     BINARY DATA FROM PAPER TAPE FRAMES INTO 16    CP203500
                    *     BITS PER WORD.                                CP203510
                    *     XR3 CONTAINS FRAM COUNT                       CP203520
                    *     XR2 POINTS TO COMPRESSED BUFFER               CP203530
                    *     XR1 POINTS TO SOURCE BUFFER                   CP203540
                    *                                                   CP203550
                    *************************************************** CP203560
                    PACK0 DC      0         ENTRY/RETURN ADDRESS        CP203570
                          STX  L3 PK060+1   SAVE XR3                    CP203580
                          LDX  I3 BA906     FRAME COUNT                 CP203590
                          LDX  L1 BA906+1   1ST WD ADDR IN UNPACKED BFR CP203600
                          LDX  L2 BA902                                 CP203610
                    PK020 LD    1 1         2ND HALF OF WORD            CP203620
                          SRA     8                                     CP203630
                          STO     PK900     RIGHT JUSTIFIED             CP203640
                          LD    1 0         1ST HALF OF WORD            CP203650
                          OR      PK900                                 CP203660
                          STO   2 0         STORE A PACKED WORD         CP203670
                          MDX   2 1                                     CP203680
                          MDX   1 2                                     CP203690
                          MDX   3 -2                                    CP203700
                          MDX     PK020     LOOP UNTIL WD CNT FINISHED  CP203710
                    PK060 LDX  L3 *-*       RESTORE XR3                 CP203720
                          BSC  I  PACK0     RETURN                      CP203730
                    *                                                   CP203740
                    PK900 DC      *-*       WORK AREA                   CP203750
                    *                                                   CP203760
                    *************************************************** CP203770
                          HDNG    DETERMINE RECORD TYPE                 CP203780
                    *                                                   CP203790
                    BC000 LDX  L1 PKBFR-1   XR1 PTS TO BINARY RCDR BFR  CP203800
                          LD    1 3         FETCH WORD 3 OF RECORD      CP203810
                          SRA     8         *TO DETERMINE RCRD TYPE     CP203820
                          S     3 BX901-X   TEST FOR AND BRANCH IF      CP203830
                    BC020 BSI  L  BD000,+-  *AN ABS HEADER RECORD - /01 CP203840
                          S     3 BX901-X   TEST FOR AND BRANCH IF      CP203850
                          BSI  L  BD000,+-  *TYPE 2 SECTOR BREAK  - /02 CP203860
                          S     3 BX908-X   TEST FOR AND BRANCH IF      CP203870
                          BSI  L  BE000,+-  *A DATA RECORD        - /0A CP203880
                          S     3 BX905-X   TEST FOR AND BRANCH IF      CP203890
                          BSI  L  BF000,+-  *AN 'F' RECORD        - /0F CP203900
                          S     3 BX912-X   TEST FOR AND BRANCH IF      CP203910
                          BSI  L  BG000,+-  *AN '81' RECORD       - /81 CP203920
                          BSI  L  ER002     SYS TAPE ERROR, PRINT E 02  CP203930
                          HDNG    PROCESS A SECTOR BREAK RECORD         CP203940
                    BD000 DC      0         ENTRY                       CP203950
                    *                                                   CP203960
                    * IF TWO SECTOR BREAK RECORDS ARE READ IN           CP203970
                    * SUCCESSION PRINT ERROR MESSAGE E 03.              CP203980
                    *                                                   CP203990
                          LD    3 BY927-X   TEST TYPE /01 SWITCH        CP204000
                          BSI  L  ER003,Z   BR TO PRINT MESSAGE E 03    CP204010
                          LD    3 BZ911-X   TEST UNCONDITIONAL BYPASS   CP204020
                          BSC  L  BB000,Z   *SW AND GO READ RCD IF ON   CP204030
                          STX  L0 BY927     SET TYPE /01 SWITCH ON      CP204040
                          SRA     16                                    CP204050
                          STO   3 BY933-X   SET TYPE 'A' INDR OFF       CP204060
                          LD    3 BY981-X   ANY PROG DATA LEFT TO WRITE CP204070
                          BSI  L  WRIT2,Z   WRITE PROGRAM DATA TO DISK  CP204080
                          SRA     16        CLEAR                       CP204090
                          STO   3 BY941-X   *BYPASS THIS PHASE INDR AND CP204100
                          STO   3 BY981-X   *PROG DATA TO WRITE INDR    CP204110
                    *                                                   CP204120
                          STX  L0 BY939     SET SECTOR BREAK INDC ON    CP204130
                          LD    3 BY925-X   TEST NOW SKIPPING INDICATOR CP204140
                          BSC  L  BB000,Z   KEEP READING IF ON          CP204150
                          LD    3 BY937-X   PREVIOUS HIGH SCTR ADDR     CP204160
                          STO   3 BY983-X   SAVE FOR ERROR RECOVERY     CP204170
                          A     3 BX901-X   ESTABLISH ADDR FOR FIRST    CP204180
                          STO   3 BY910-X   *SECTOR OF THIS PHASE       CP204190
                    *                                                   CP204200
                    * NOW THAT THE PREVIOUS PHASE IS COMPLETELY         CP204210
                    * PROCESSED, SET THE LAST TWO WORDS IN ITS SLET     CP204220
                    * ENTRY                                             CP204230
                    *                                                   CP204240
                          LD    3 BY982-X   DOES SLET ENTRY NEED FINISH CP204250
                          BSC  L  BD045,+-  *BR IF ALREADY FINISHED     CP204260
                          LDX  I2 C1020+1   RESTORE XR2 POINTER TO SLET CP204270
                          LD    3 BY919-X   STORE WORD COUNT PREV PHASE CP204280
                          A     3 BX901-X   FORM CORRECT WORD COUNT     CP204290
                          STO   2 -2        *PROCESSED IN CURRENT SET   CP204300
                          STO   3 BY977-X   SAVE FOR SHRINK TEST    2-4 CP204310
                          LD    3 BY915-X   STORE SCTR ADDR OF FIRST    CP204320
                          STO   2 -1        *SECTOR IN SLET ENTRY       CP204330
                          SRA     16        RESET WORD COUNT FOR        CP204340
                          STO   3 BY919-X   *EACH PHASE                 CP204350
                    *                                                   CP204360
                    * ON RELOAD ALWAYS WRITE OUT SLET TO THE DISK.      CP204370
                    * ON INITIAL AND RELOAD, WRITE AND THEN READ THE    CP204380
                    * NEXT SLET SECTOR IF THE LAST SLET ENTRY WAS AT    CP204390
                    * THE END OF A SECTOR.                              CP204400
                    *                                                   CP204410
                    BD045 LD    3 BY934-X   TEST FOR INITIAL LOAD       CP204420
                          BSC  L  BD050,Z+  BR IF INITIAL LOAD          CP204430
                          LD    3 BY920-X   IF SLET SCTR FULL           CP204440
                          BSC  L  BD060,Z   *THEN WRITE AND READ SLET   CP204450
                          LD    3 BY982-X   DOES SLET NEED WRITING      CP204460
                          BSI  L  WRIT3,Z   ONLY WRITE SLET             CP204470
                          MDX     BD080     ALREADY TOOK CARE OF SLET   CP204480
                    BD050 BSI  L  C1400     BR TO INSERT PRINCIPAL I/O  CP204490
                          LD    3 BY920-X   IF SLET SCTR FULL           CP204500
                    *                       WRITE SLET SCTR TO DISK AND CP204510
                    BD060 BSI  L  BD200,Z   *READ NEXT SLET SECTOR      CP204520
                    BD080 LD    3 BY975-X   SET JUMP IN RECORD READ     CP204530
                          STO  L  CK060                                 CP204540
                          LD    3 BY973-X   SET INDC FOR REL SECTOR     CP204550
                          STO   3 BY939-X                               CP204560
                          SRA     16        CLEAR RCRD COUNT FOR CK-SUM CP204570
                          STO   3 BY944-X                               CP204580
                          STO   3 BY943-X                               CP204590
                    *                       SET INDC SO NEXT DATA RCRD  CP204600
                          STO  L  BE900     *SETS REL ADDR WORD IN SCTR CP204610
                          STO   3 BY942-X   SET INDC ON FOR SCTR BREAK  CP204620
                          LD    3 BX914-X                               CP204630
                          STO   3 BY938-X                               CP204640
                    *                                               2-4 CP204650
                    * CHECK IF PHASE SIZE HAS SHRUNK DURING RELOAD  2-4 CP204660
                    *                                               2-4 CP204670
                    BD090 LD    3 BY934-X   TEST FOR INITIAL LOAD   2-4 CP204680
                          BSC  L  BD095,Z+  BR IF INITIAL LOAD      2-4 CP204690
                          LD    3 BY982-X   WAS PHASE JUST STORED   2-4 CP204700
                          BSC  L  BD095,+-  BR IF NOT               2-4 CP204710
                          LD    3 BZ908-X   BR IF NEW PROGRAM ADD   2-4 CP204720
                          BSC  L  BD095,Z   *IN PROCESS             2-4 CP204730
                          LD    3 BY977-X   GET NEW WORD COUNT      2-4 CP204740
                          A     3 BX913-X   CALCULATE NUMBER OF     2-4 CP204750
                          SRT     16        *SECTORS IN             2-4 CP204760
                          D     3 BX914-X   *NEW RELOADED PHASE     2-4 CP204770
                          STO   3 BY902-X   SAVE NEW PHASE LENGTH   2-4 CP204780
                          S     3 BY947-X   SUB OLD PHASE LENGTH    2-4 CP204790
                          BSC  L  BD095,-   BR IF NOT NOW SMALLER   2-4 CP204800
                    *                                               2-4 CP204810
                    * SET UP PARAMTERS FOR SLET UPDATE AND          2-4 CP204820
                    * SYSTEM PROGRAM SHIFT                          2-4 CP204830
                    *                                               2-4 CP204840
                          STO   3 BY903-X   SAVE LENGTH DIFFERENCE  2-4 CP204850
                          LD    3 BY915-X   GET SCTR ADDR OF PHASE  2-4 CP204860
                          A     3 BY902-X   CALCULATE ADDR OF START 2-4 CP204870
                          STO   3 BY918-X   *OF NEW GAP, 'TO' ADDR  2-4 CP204880
                          S     3 BY903-X   CALCULATE ADDR OF PHASE 2-4 CP204890
                          STO   3 BY917-X   *AFTER GAP, 'FROM' ADDR 2-4 CP204900
                          BSI     BD099     GO TO UPDATE/SHIFT SUBR 2-4 CP204910
                    *                                               2-4 CP204920
                    BD095 SRA     16        INDICATE SLET DOESN'T   2-4 CP204930
                          STO   3 BY982-X   *NEED SETTING OR WRITING    CP204940
                          BSC  L  BB000     BR TO READ NEXT RECORD      CP204950
                    *                                               2-4 CP204960
                    ************************************************2-4 CP204970
                    *                                               2-4 CP204980
                    * UPDATE SLET TABLE ENTRIES AFFECTED BY LEFT    2-4 CP204990
                    * SHIFT                                         2-4 CP205000
                    *                                               2-4 CP205010
                    BD099 DC      0         ENTRY/RETURN ADDR       2-4 CP205020
                          NOP               WAIT MAY BE PATCHED HERE2-4 CP205030
                    BD100 STX   2 BD109     GET XR2 POINTER TO NEXT 2-4 CP205040
                          LD      BD109     *SLET ENTRY IN ACC, SUB 2-4 CP205050
                          S     3 BY914-X   *ADDR LAST ENTRY        2-4 CP205060
                    *                       IF BUFFER IS FULL,      2-4 CP205070
                    *                       WRITE SCTR BACK TO DISK 2-4 CP205080
                          BSI  L  BD200,-Z  *AND READ NEXT SECTOR   2-4 CP205090
                          LD   L  BUFR3+1   CHECK FOR END OF SLET   2-4 CP205100
                          S     3 BY956-X   *SLET TABLE SECTORS     2-4 CP205110
                          BSC  L  BD120,-Z  BR IF BEYOND END        2-4 CP205120
                          LD    2 3         GET OLD PHASE SCTR ADDR 2-4 CP205130
                          BSC  L  BD110,+-  BR IF NO MORE ENTRIES   2-4 CP205140
                          S     3 BY917-X   UPDATE PHASE SCTR ADDR  2-4 CP205150
                          A     3 BY918-X   *BY AMOUNT OF SHIFT     2-4 CP205160
                          STO   2 3         *DISPLACEMENT           2-4 CP205170
                          MDX   2 4         ADVANCE SLET ENTRY PTR  2-4 CP205180
                          MDX     BD100     BR TO TEST NEXT ENTRY   2-4 CP205190
                    BD109 DC      *-*       TEMP STORAGE FOR XR2    2-4 CP205200
                    BD110 BSI  L  WRIT3     WRITE BACK LAST SECTOR  2-4 CP205210
                    *                                               2-4 CP205220
                    * SHIFT REMAINDER OF SYSTEM PROGRAMS LEFT       2-4 CP205230
                    *                                               2-4 CP205240
                    BD120 LD    3 BY924-X   KNOWN ID OF LAST PHASE  2-4 CP205250
                          BSI  L  SSLET     SEARCH SLET FOR THIS ID 2-4 CP205260
                          BSI  L  ER027,+   BR IF SOMEHOW MISSING   2-4 CP205270
                          LD    2 2         CALCULATE NUMBER OF     2-4 CP205280
                          A     3 BX913-X   *SECTORS IN LAST        2-4 CP205290
                          SRT     16        *SYSTEM PROGRAM         2-4 CP205300
                          D     3 BX914-X   *PHASE                  2-4 CP205310
                          A     2 3         ADD STARTING SCTR ADDR  2-4 CP205320
                          S     3 BX901-X   SET NEW VALUE FOR       2-4 CP205330
                          STO   3 BY937-X   *LAST SECTOR USED       2-4 CP205340
                          A     3 BX901-X   ADD BACK 1              2-4 CP205350
                          S     3 BY918-X   NUMBER OF SCTRS TO SHIFT2-4 CP205360
                          STO   3 BY978-X   *IS DIFFERENCE FROM 'TO'2-4 CP205370
                          LD   L  CAREA     INCREMENT CUSHION SIZE  2-4 CP205380
                          A     3 BY917-X   *BY AMOUNT              2-4 CP205390
                          S     3 BY918-X   *OF SHIFT               2-4 CP205400
                          STO  L  CAREA     *DISPLACEMENT           2-4 CP205410
                          BSI  L  C0300     GO DO THE LEFT SHIFT    2-4 CP205420
                          BSC  I  BD099     RETURN FROM SUBROUTINE  2-4 CP205430
                    *                                                   CP205440
                    *************************************************** CP205450
                    *                                                   CP205460
                    * WRITE A SECTOR OF SLET AND READ NEXT SECTOR       CP205470
                    * OF SLET.                                          CP205480
                    *                                                   CP205490
                    BD200 DC      0         ENTRY/RETURN ADDRESS        CP205500
                          SRA     16        RESET SLET SCTR FULL        CP205510
                          STO   3 BY920-X   *INDICATOR                  CP205520
                          BSI  L  WRIT3     BR TO WRITE SLET SECTOR     CP205530
                          MDX  L  BUFR3+1,1 BUMP TO NEXT SLET SCTR ADDR CP205540
                          BSI  L  FTCH3     BR TO READ SLET SECTOR      CP205550
                          LDX  I2 BY913     SET XR2 ADDR 1ST SLET SET   CP205560
                          STX  L2 C1020+1   *AND SAVE IN 'C1020'+1      CP205570
                          BSC  I  BD200     RETURN                      CP205580
                          HDNG    PROCESS A TYPE 'A' DATA RECORD        CP205590
                    BE000 DC      0         ENTRY                       CP205600
                          SRA     16        SET                         CP205610
                          STO   3 BY928-X   *TYPE 'F' INDICATOR AND     CP205620
                          STO   3 BY927-X   *TYPE '1' INDICATOR OFF     CP205630
                          STX  L0 BY933     SET TYPE 'A' INDR ON        CP205640
                          LD    3 BY941-X   IF BAD PHASE BEING FLUSHED  CP205650
                          BSC  L  BB000,Z   *BR TO READ NEXT RECORD     CP205660
                          LD    3 BY925-X   IS SYS LDR BYPASSING PHASES CP205670
                          BSC  L  BE080,+-  BR IF NOT BYPASSING PHASE   CP205680
                    *                                                   CP205690
                    * SYSTEM LOADER IS BYPASSING PHASES                 CP205700
                    *                                                   CP205710
                          LD    3 BY939-X                               CP205720
                          BSC  L  BE560,-+  BR IF SCTR BREAK INDR OFF   CP205730
                          LD    1 11        PHASE ID FROM 1ST DATA RCD  CP205740
                          BSC  L  BE020,-   IF PHASE ID NEGATIVE        CP205750
                          SRA     16        *MAKE IT                    CP205760
                          S     1 11        *POSITIVE                   CP205770
                    BE020 S     3 BY926-X   PASS TAPE  UNTIL PH ID TO   CP205780
                          BSC  L  BE560,+Z  *GO TO IS REACHED           CP205790
                    *                                                   CP205800
                    * A PROGRAM HAS JUST BEEN BYPASSED. INDICATORS  2-3 CP205810
                    * ARE CLEARED.  THE HANDLING OF BYPASS LIMITS   2-3 CP205820
                    * HAS BEEN MOVED TO BE086.                      2-3 CP205830
                    *                                                   CP205840
                          SRA     16                                    CP205850
                          STO   3 BY925-X   CLEAR INDICATOR             CP205860
                          STO   3 BY941-X   CLEAR PHASE BYPASS INDR     CP205870
                          STO   3 BY944-X   CLEAR CK-SUM RCD SEQ CTR    CP205880
                          LD    3 BY975-X   RE-INITIALIZE CKSUM SUBR    CP205890
                          STO  L  CK060                                 CP205900
                    *                                                   CP205910
                    *                       CODING REMOVED.         2-3 CP205920
                    *                                                   CP205930
                    BE080 LD    1 3         FETCH TYPE & WD COUNT       CP205940
                          AND   3 BY957-X   MASK OUT TYPE               CP205950
                          STO     BE240+1   SAVE WORD COUNT FROM RECORD CP205960
                          BSI  L  CKSUM     BR TO CHECK-SUM SUBROUTINE  CP205970
                          LD    3 BY939-X   TEST FOR NEW SCTR SIGNAL    CP205980
                          BSC  L  BE220,+-  BR IF NOT NEW SECTOR        CP205990
                    *                                                   CP206000
                    * PROCESS FIRST DATA RECORD AFTER SECTOR BREAK      CP206010
                    * RECORD.                                           CP206020
                    *                                                   CP206030
                          LD   L  BE240+1   FETCH WORD COUNT            CP206040
                          S     3 BX901-X   SUBTRACT 1                  CP206050
                          BSI  L  ER004,+   BR IF WORD COUNT 0 OR 1     CP206060
                          LD    3 BY911-X   PREVIOUS CURRENT PHASE NOW  CP206070
                          STO   3 BY912-X   *IS OLD PHASE               CP206080
                          LD    1 11        PHASE ID FROM FIRST RECORD  CP206090
                          BSC  L  BE085,-   IF PHASE ID NEGATIVE        CP206100
                          SRA     16        *MAKE                       CP206110
                          S     1 11        *POSITIVE                   CP206120
                    BE085 STO   3 BY911-X   SET NEW CURRENT PHASE ID    CP206130
                    *                                                   CP206140
                    * BYPASS TEST FOR INITIAL LOAD AND RELOAD.          CP206150
                    * PROGRAMS TO BE SKIPPED BY THIS TEST               CP206160
                    * WERE INDICATED ON LOAD MODE RECORD                CP206170
                    *                                                   CP206180
                    * THE SKIP TABLE IS SEARCHED AND CHECKED        2-3 CP206190
                    * ON EACH OCCASION UNTIL - THE END OF THE TABLE 2-3 CP206200
                    * IS REACHED OR THE CURRENT SLET ID IS FOUND TO 2-3 CP206210
                    * BE LESS THAN A LOWER LIMIT OR THE CURRENT     2-3 CP206220
                    * SLET ID FITS INTO A LIMIT PAIR.               2-3 CP206230
                    *                                                   CP206240
                          LDX  L2 LOLIM     LD ADDR OF SKIP TABLE   2-3 CP206250
                    BE086 LD    2 0         CHK IF NO MORE PAIRS    2-3 CP206260
                          S     3 BX915-X   *IN SKIP TABLE          2-3 CP206270
                          BSC  L  BE090,+-  *BR IF END OF SKIP TBL  2-3 CP206280
                          LD    3 BY911-X   IS CURRENT SLET ID      2-3 CP206290
                          S     2 0         *LESS THAN LOWER  LIMIT 2-3 CP206300
                          BSC  L  BE090,+Z  *BR ON YES              2-3 CP206310
                          LD    3 BY911-X   IS CURRENT SLET ID LESS 2-3 CP206320
                          S     2 1         *THAN OR EQU UPPER LIMIT2-3 CP206330
                          BSC  L  BE087,+   *BR IF YES              2-3 CP206340
                          MDX   2 2         POINT TO NEXT PAIR      2-3 CP206350
                          MDX     BE086     *AND CHECK AGAIN        2-3 CP206360
                    BE087 LD    2 1         GET UPPER LIMIT OF      2-3 CP206370
                          A     3 BX901-X   *BY-PASS PAIR, ADD ONE      CP206380
                          STO   3 BY926-X   PH ID BEING LOOKED FOR      CP206390
                          STO   3 BY925-X   SET BYPASSING INDR ON       CP206400
                          NOP               A WAIT MAY BE PATCHED HERE  CP206410
                          LD    3 BY934-X   IF RELOAD                   CP206420
                          BSI  L  C0720,-   *BR TO REMOVE ANY SLET REF  CP206430
                          BSC  L  BE560     BR TO FLUSH DATA RECORDS    CP206440
                    *                                                   CP206450
                    * THIS PHASE IS TO BE LOADED                        CP206460
                    *                                                   CP206470
                    * SAVE DATA FROM PHASES WITH NEGATIVE IDS FOR       CP206480
                    * SPECIAL RELOAD TABLE.                             CP206490
                    *                                                   CP206500
                    BE090 LD    1 11        LOAD PHASE ID               CP206510
                          BSC  L  BE100,+   BR IF RELOAD TABLE ENTRY    CP206520
                          LD    3 BY934-X   TEST FOR AND BR IF RELOAD   CP206530
                          BSI  L  C0720,-   *TO CHK FOR ID IN TABLE     CP206540
                          MDX     BE180     INITIAL LOAD, NO SLET REF   CP206550
                    *                                                   CP206560
                    * THIS PHASE MAKES SLET TABLE REFERENCE             CP206570
                    *                                                   CP206580
                    BE100 LD   L  BE240+1   VERIFY THAT WD CNT IS AT    CP206590
                          S     3 BX904-X   *LEAST 4, ELSE              CP206600
                          BSI  L  ER021,Z+  *BR TO DISPLAY ERROR E 21   CP206610
                          LD   L  BUFR1     TEST WD CNT NOW IN RELOAD   CP206620
                          S     3 BX914-X   *TABLE                      CP206630
                          A     3 BX902-X   IF 318 OR OVER,             CP206640
                          BSI  L  ER030,-   *BR TO DISPLAY ERROR E 30   CP206650
                          SRA     16        MAKE NEGATIVE               CP206660
                          S     1 11        *PHASE ID                   CP206670
                          STO   1 11        *POSITIVE                   CP206680
                    BE120 STO  L  BUFR1+2   ST IN RELOAD TABLE BUFFER   CP206690
                          LD    1 12                                    CP206700
                    BE140 STO  L  BUFR1+3   ST RLTV LOC IN SPEC PHASE   CP206710
                          LD    1 13                                    CP206720
                    BE160 STO  L  BUFR1+4   ST NO. OF SETS REQUIRED     CP206730
                          MDX  L  BE120+1,3 MODIFY STORAGE ADDRESS      CP206740
                          MDX  L  BE140+1,3 MODIFY STORAGE ADDRESS      CP206750
                          MDX  L  BE160+1,3 MODIFY STORAGE ADDRESS      CP206760
                          MDX  L  BUFR1,3   BUMP WD CNT FOR FILE BY 3   CP206770
                    BE180 BSC  L  OVLAY+3   BRANCH TO OVERLAY 0 OR 1    CP206780
                    *                                                   CP206790
                    * OVERLAY 0 OR 1 NORMALLY RETURNS HERE              CP206800
                    *                                                   CP206810
                    BE200 LD    3 BY910-X   1ST SCTR ADDR OF NEW PHASE  CP206820
                          STO   3 BY945-X                               CP206830
                          STO   3 BY915-X   SAVE FOR SLET               CP206840
                          STX  L0 BY981     INDICATE PROG DATA TO WRITE CP206850
                          LD      BE240+1   LOAD WORD COUNT             CP206860
                          S     3 BX902-X   MODIFY WD CNT FROM RECORD   CP206870
                          STO     BE240+1   SAVE TO SET IN XR2          CP206880
                          LD    3 BY966-X   FETCH NOP CODING            CP206890
                          STO     BE260     CANCEL SKIP PAST INST TO    CP206900
                          MDX     BE240     *ADD TWO.  BR TO LOAD XR2   CP206910
                    BE220 LD    3 BY969-X   NOP THE ADD 2 INSTRUCTION   CP206920
                          STO     BE260     *BY INSERTING A JUMP        CP206930
                    BE240 LDX  L2 *-*       WORD COUNT OF RECORD        CP206940
                          LD    1 1         SAVE CORE LOC OF 1ST WORD   CP206950
                    BE260 MDX     BE280     AT SCTR BREAKS, ADDR OF     CP206960
                          A    L  BX902     *FIRST DATA WD IS 2 LARGER  CP206970
                    BE280 MDX  L0 BE900,0   IF NOT 0 DONT SET RLTV ADDR CP206980
                          MDX     BE320     BR TO TEST LOAD ADDR OF WD  CP206990
                    *                                                   CP207000
                    * DETERMINE RELATIVE ADDRESS FROM                   CP207010
                    * ADDRESS OF FIRST WORD IN SECTOR                   CP207020
                    *                                                   CP207030
                    BE300 S     3 BX901-X   SUBTRACT ONE                CP207040
                          MDX  L  BY943,1   INCR CORR FACTOR CTR        CP207050
                          BSC  L  BE300,Z   LOOP UNTIL ZERO             CP207060
                          LD    3 BY943-X   *AND SAVE RESULTING ADDR    CP207070
                          STO   3 BY940-X   SET UP CORRECTION FACTOR    CP207080
                          STX  L0 BE900     TURN INDC OFF               CP207090
                          LD    1 1         RELOAD ORIGINAL ADDRESS     CP207100
                          A     3 BX902-X   1ST 2 WDS ARE NOT STORED    CP207110
                    *                                                   CP207120
                    BE320 S     3 BY940-X   SUBTRACT RLTV ADDR IN SCTR  CP207130
                          BSI  L  ER004,Z+  BR IF ORG BACKWARDS         CP207140
                    BE340 STO   3 BY936-X   DATA WORD ON RECORD         CP207150
                          S     3 BY919-X   COMPARE RLTV ADDR WITH THE  CP207160
                          BSC  L  BE380,+   *HIGHEST SO FAR.  BR IF LOW CP207170
                          LD    3 BY936-X   SAVE HIGHEST 'BY936' VALUE  CP207180
                          STO   3 BY919-X   *FOR SLET TABLE             CP207190
                    BE380 LD    3 BY936-X   'BY936' CLIMBS WITH EACH WD CP207200
                          SRT     16        SET UP FOR DIVIDE           CP207210
                          D     3 BX914-X   DIVIDE BY WDS PER SCTR      CP207220
                          STD   3 BY900-X   SAVE QUOTIENT & REMAINDER   CP207230
                          LD    3 BZ908-X   IF A PROGRAM IS BEING ADDED CP207240
                          BSC  L  BE400,Z   *BR AROUND LENGTH CHECK     CP207250
                          LDD   3 BY900-X   SECTORS AND WDS ADDITIONAL  CP207260
                          S     3 BY947-X   PHASE LENGTH IN SECTORS     CP207270
                          BSC  L  C0500,-   BRANCH IF A SHIFT REQUIRED  CP207280
                          A     3 BY947-X   ADD PHASE LENGTH IN SECTORS CP207290
                    BE400 S     3 BY938-X   BR IF NO SCTR CHANGE REQ    CP207300
                          BSC  L  BE480,+-  PROGRAM ALREADY IN CORE     CP207310
                          LD    3 BY942-X   TEST FOR AND BR IF          CP207320
                          BSC  L  BE440,Z   *END OF PROG INDR ON        CP207330
                          LD    3 BY900-X   SET INDR WITH               CP207340
                          STO   3 BY938-X   *SCTR COUNT FROM DIVIDE     CP207350
                          MDX  L  BY942,1   SET SCTR BREAK INDR OFF     CP207360
                          MDX     BE460     BR TO SET UP A SCTR ADDR    CP207370
                    BE440 LD    3 BY900-X   SET INDR FOR THIS SCTR      CP207380
                          STO   3 BY938-X   OF PHASE                    CP207390
                          BSI  L  WRIT2     WRITE A SECTOR              CP207400
                          LD    3 BY900-X   FETCH QUOTIENT              CP207410
                    BE460 A     3 BY945-X   ADD SECTOR ADDRESS          CP207420
                          STO   3 BY916-X   MAINTAIN CURR SCTR ADDR     CP207430
                          STO  L  BUFR2+1   SET SECTOR ADDR IN BUFFER   CP207440
                          BSI  L  FTCH2     READ INTO 'BUFR2' FROM DISK CP207450
                          LDD   3 BY900-X   FETCH SPECS FOR THIS PHASE  CP207460
                    BE480 SLT     16        LOAD DISP IN CORE BUFFER    CP207470
                          A     3 BX902-X   INCR BY 2                   CP207480
                          A     3 BY950-X   SET UP RLTV STORAGE LOC     CP207490
                          STO     BE520+1   *IN CORE BUFFER             CP207500
                          LD    3 BY939-X   TEST FOR SCTR BREAK         CP207510
                          BSC  L  BE500,+-  BR IF NO SCTR BREAK         CP207520
                          SRA     16        CLEAR NEW SCTR INDR         CP207530
                          STO   3 BY939-X                               CP207540
                          MDX   1 2         FETCH 3RD DATA WORD INSTEAD CP207550
                    BE500 LD    1 10        NO SCTR ADDR IN THIS RECORD CP207560
                          MDX   2 0         SKIP IF XR2 IS ZERO         CP207570
                          MDX     BE520     *ELSE STORE A WORD          CP207580
                          MDX     BE540     BR TO READ NEXT RECORD      CP207590
                    BE520 STO  L  *-*       DATA WORD TO DISK BUFFER    CP207600
                          LD    3 BY936-X   INCR RLTV ADDR POINTER      CP207610
                          A     3 BX901-X   *BY ONE                     CP207620
                          MDX   1 1         POINT TO NEXT WD OF RECORD  CP207630
                          MDX   2 -1        SKIP AFTER LAST DATA WORD   CP207640
                          MDX     BE340     BR TO PROCESS NEXT DATA WD  CP207650
                    BE540 BSC  L  BB000     BR TO READ NEXT RECORD      CP207660
                    *                                                   CP207670
                    * SYSTEM LOADER SKIPPING THIS PHASE                 CP207680
                    *                                                   CP207690
                    BE560 LD    3 BX903-X   SET                         CP207700
                          STO   3 BY941-X   *BYPASS RECORDS INDR ON     CP207710
                          SRA     16        CLEAR                       CP207720
                          STO   3 BY939-X   *SECTOR BREAK INDICATOR     CP207730
                          BSC  L  BB000     BRANCH TO READ NEXT RECORD  CP207740
                    *                                                   CP207750
                    * WORK AREAS                                        CP207760
                    *                                                   CP207770
                    BE900 DC      *-*       TO SET RLTV SCTR WD IN SCTR CP207780
                    *                                                   CP207790
                    *************************************************** CP207800
                    *                                                   CP207810
                    * PROCESS THE CHECKSUM                              CP207820
                    *                                                   CP207830
                    CKSUM DC      0         ENTRY/RETURN ADDRESS        CP207840
                    CK020 LD   L  PKBFR+1   FETCH WORD 2                CP207850
                          BSC  I  CKSUM,+-  RETURN IF NO CHECKSUM       CP207860
                          MDX  L  BY944,1   INCREMENT THE SEQ NO.       CP207870
                          NOP               AVOID POSSIBLE SKIP         CP207880
                    CK030 LDX  L2 *-*       XR2 = NO. WORDS PER RECORD  CP207890
                          LD    3 BY944-X   FETCH SEQUENCE NUMBER       CP207900
                    CK040 A    L2 PKBFR-1   ADD A WORD                  CP207910
                          BSC     C         SKIP IF NO CARRY            CP207920
                          A     3 BX901-X   ADD ONE                     CP207930
                          MDX   2 -1        DECR WORD CNT, SKIP IF ZERO CP207940
                          MDX     CK040     BRANCH TO ADD NEXT WORD     CP207950
                          S     3 BX901-X   TEST FOR AND                CP207960
                          BSC  I  CKSUM,+-  *RETURN IF BLANK RECORD     CP207970
                    CK060 MDX     CK080     'NOP' IF SEQUENCE ERROR     CP207980
                    *                                                   CP207990
                          LD      CK030+1   GET WORDCOUNT           2-9 CP207999
                          BSI  L  ER001     BR TO PRINT ERROR MSG E 01  CP208000
                          MDX  L  BY944,-1  DECREMENT SEQUENCE NUMBER   CP208010
                          NOP                                           CP208020
                          BSC  L  BB000     BRANCH TO READ NEXT RECORD  CP208030
                    *                                                   CP208040
                    CK080 EOR   3 BY965-X   COMPLIMENT AND              CP208050
                          STO   3 BY944-X   *STORE CHECKSUM             CP208060
                          LD    3 BY966-X   CANCEL                      CP208070
                          STO     CK060     *RESET ABILITY              CP208080
                          MDX     CK020     BRANCH TO PROCESS AGAIN     CP208090
                          HDNG    END OF PROGRAM - 'F' RCRD PROCESSING  CP208100
                    BF000 DC      0         ENTRY                       CP208110
                          STX  L0 BY928     SET TYPE F INDR ON          CP208120
                          LD    3 BY925-X   TEST NOW SKIPPING INDICATOR CP208130
                          OR    3 BY941-X   *OR FLUSH THIS PHASE INDR   CP208140
                          BSC  L  BB000,Z   BR TO READ NEXT RCD IF ON   CP208150
                          LD    3 BY933-X   VERIFY THAT 'F' FOLLOWS 'A' CP208160
                          BSI  L  ER003,+-  BR IF IT DOES NOT           CP208170
                          SRA     16                                    CP208180
                          STO   3 BY933-X   SET TYPE 'A' INDR OFF       CP208190
                          STO   3 BY981-X   IND NO PROG DATA TO WRITE   CP208200
                          BSI  L  WRIT2     WRITE PROGRAM DATA TO DISK  CP208210
                          LDX  I2 C1020+1   RESTORE XR2 POINTER TO SLET CP208220
                          LD    3 BY919-X   FINISH THIS SLET SET        CP208230
                          A     3 BX901-X   *IN CASE THIS SHOULD        CP208240
                          STO   2 -2        *BE THE LAST OR ONLY        CP208250
                          STO   3 BY977-X   SAVE FOR SHRINK TEST    2-4 CP208260
                          LD    3 BY915-X   *PHASE LOADED               CP208270
                          STO   2 -1                                    CP208280
                          BSI  L  WRIT3     WRITE SLET SECTOR TO DISK   CP208290
                          SRA     16                                    CP208300
                          STO   3 BY919-X   RESET WORD COUNT EACH PHASE CP208310
                          BSC  L  BD090     BR TO CHECK FOR GAP     2-4 CP208320
                    *                                                   CP208330
                    *************************************************** CP208340
                          HDNG    END OF SYSTEM DECK - PROCESS '81' RCD CP208350
                    BG000 DC      0         ENTRY/RETURN ADDRESS        CP208360
                          LD    3 BY928-X   IF 'F' RECORD WAS MISSING   CP208370
                          BSI  L  ER003,+-  *DISPLAY OUT OF SEQ ERROR   CP208380
                          LD    3 BY934-X   IF RELOAD, BRANCH TO        CP208390
                          BSC  L  BG060,-   *TEST ADD PROGRAM INDICATOR CP208400
                          LD    3 BY931-X   TEST FOR LAST REQ PH DONE   CP208410
                          BSI  L  ER024,Z   ERROR IF ANY PH MISSING     CP208420
                    *                                                   CP208430
                    * WHEN 'ER024' RETURNS, PRETEND NOTHING IS MISSING  CP208440
                    *                                                   CP208450
                    * SET UP TO FETCH OVERLAY 2                         CP208460
                    *                                                   CP208470
                    BG040 LD    3 BY960-X   FETCH SECTOR ADDRESS OF,    CP208480
                          LDX  L2 C2000     *ENTRY POINT TO             CP208490
                          BSI  L  BH000     *AND BR TO FETCH OVERLAY 2  CP208500
                    *                                                   CP208510
                    BG060 LD    3 BZ908-X   TEST ADD PROGRAM SWITCH     CP208520
                          BSC  L  BG040,+-  BRANCH IF OFF               CP208530
                          LD   L  LPHID     TEST IF ALL PHASES IN   2-9 CP208540
                          S     3 BY911-X                               CP208550
                          BSI  L  ER024,-Z  ERROR IF PHASE MISSING  2-9 CP208560
                    *                                                   CP208570
                    * CALCULATE NEW LET ADDRESS                         CP208580
                    *                                                   CP208590
                          LD    3 BY937-X   FETCH LAST SCTR ADDR USED   CP208600
                          SRA     3         DECR TO LAST CYLINDER ADDR  CP208610
                          SLA     3         ADD 1 CYL MORE, AND 1 CYL   CP208620
                          A     3 BX910-X   *FOR CUSHION, 1 FOR SCRA,   CP208630
                          STO   3 BZ903-X   *AND 2 FOR CIB.  40 SCTRS.  CP208640
                          STO   3 BY918-X   SAVE ALSO AS 'TO' ADDR  2-4 CP208650
                          LD    3 BY958-X   CALCULATE               2-4 CP208660
                          S    L  SHIFT     *'FROM'                 2-4 CP208670
                          STO   3 BY917-X   *ADDR                   2-4 CP208680
                          LD   L  SHIFT     SET SECTORS TO SHIFT    2-4 CP208690
                          STO   3 BY978-X   *COUNT                  2-4 CP208700
                          BSI  L  C0300     GO SHIFT LET/UA LEFT    2-4 CP208710
                          MDX     BG040     SETUP PRINC DEVICE ENTRIES  CP208720
                          HDNG    FETCH AND BRANCH TO OVERLAYS          CP208730
                    BH000 DC      0         ENTRY                       CP208740
                          STX   2 BH020     STORE OVERLAY ENTRY ADDRESS CP208750
                          STO  L  OVLAY+1   STORE OVERLAY SCTR ADDRESS  CP208760
                          LD      BH902     FETCH AND                   CP208770
                          STO  L  OVLAY     *STORE OVERLAY WORD COUNT   CP208780
                          LDD     BH900     FETCH FUNC CODE/I/O AR ADDR CP208790
                          BSI  L  DZ000     BRANCH TO FETCH OVERLAY     CP208800
                    BH010 MDX  L  $DBSY,0   SKIP NEXT IF READ COMPLETE  CP208810
                          MDX     BH010     BR TO TEST READ COMPLETE    CP208820
                          BSC  L  *-*       BRANCH TO THE OVERLAY       CP208830
                    BH020 EQU     *-1       ADDRESS OF OVERLAY ENTRY    CP208840
                    *                                                   CP208850
                    * CONSTANTS AND WORK AREAS                          CP208860
                    *                                                   CP208870
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP208880
                    BH900 DC      /0000     READ FUNCTION CODE          CP208890
                          DC      OVLAY     ADDRESS OF OVERLAY I/O AREA CP208900
                    BH902 DC      2*@SCNT   WORD COUNT OF OVERLAYS      CP208910
                          HDNG    MISCELLANEOUS SUBROUTINES             CP208920
                    *************************************************** CP208930
                    *                                                   CP208940
                    * SET INDICATORS TO BYPASS PHASE                    CP208950
                    *                                                   CP208960
                    BYPAS DC      0         ENTRY                       CP208970
                          LD    3 BX901-X   SET                         CP208980
                          STO   3 BY941-X   *BYPASS PHASES INDICATOR ON CP208990
                          SRA     16        CLEAR                       CP209000
                          STO   3 BY939-X   *FIRST DATA RECORD INDR     CP209010
                          STO   3 BY981-X   IND NO PROG DATA TO STORE   CP209020
                          STO   3 BY982-X   IND NO SLET TO FINISH       CP209030
                          LD    3 BY983-X   REDUCE HIGHEST SCTR ADDR    CP209040
                          STO   3 BY937-X   *WRITTEN TO PREV VALUE      CP209050
                          BSC  L  BB000     BRANCH TO READ NEXT RECORD  CP209060
                    *                                                   CP209070
                    *************************************************** CP209080
                    *                                                   CP209090
                    * READ A SECTOR FROM DISK.                          CP209100
                    *                                                   CP209110
                    FTCH2 DC      0         ENTRY/RETURN ADDRESS        CP209120
                          LDD     F2900     FETCH FUNC CODE, I/O ADDR   CP209130
                          BSI  L  DZ000     BRANCH TO READ A SECTOR     CP209140
                          MDX  L  $DBSY,0   SKIP NEXT IF  READ COMPLETE CP209150
                          MDX     *-3       BR TO TEST READ COMPLETE    CP209160
                          BSC  I  FTCH2     RETURN                      CP209170
                    *                                                   CP209180
                    * CONSTANTS AND WORK AREAS                          CP209190
                    *                                                   CP209200
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP209210
                    F2900 DC      /0000     READ FUNCTION CODE          CP209220
                          DC      BUFR2     ADDRESS OF I/O AREA         CP209230
                    *                                                   CP209240
                    *************************************************** CP209250
                    *                                                   CP209260
                    * READ A SECTOR OF SLET.                            CP209270
                    *                                                   CP209280
                    FTCH3 DC      0         ENTRY/RETURN ADDRESS        CP209290
                          LDD     F3900     FETCH FUNC CODE, I/O ADDR   CP209300
                          BSI  L  DZ000     BRANCH TO READ A SECTOR     CP209310
                          MDX  L  $DBSY,0   SKIP NEXT IF READ COMPLETE  CP209320
                          MDX     *-3       BR TO TEST READ COMPLETE    CP209330
                          BSC  I  FTCH3     RETURN                      CP209340
                    *                                                   CP209350
                    * CONSTANTS AND WORK AREAS                          CP209360
                    *                                                   CP209370
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP209380
                    F3900 DC      /0000     READ FUNCTION CODE          CP209390
                          DC      BUFR3     ADDRESS OF SLET I/O AREA    CP209400
                    *                                                   CP209410
                    *************************************************** CP209420
                    *                                                   CP209430
                    * SLET SEARCH SUBROUTINE.                           CP209440
                    *                                                   CP209450
                    SSLET DC      0         ENTRY/RETURN ADDRESS        CP209460
                          STO     SS900     SAVE PHASE ID TO SEARCH FOR CP209470
                          STX   1 SS080+1   SAVE XR1                    CP209480
                          LD    3 BY955-X   STORE FIRST                 CP209490
                          STO  L  BUFR3+1   *SLET SCTR ADDR TO BUFFER   CP209500
                    SS020 BSI     FTCH3     BR TO FETCH A SLET SECTOR   CP209510
                          LDX  I2 BY913     XR2 POINT TO FIRST SLET SET CP209520
                          LDX   1 80        XR1 INDICATES SETS PER SCTR CP209530
                    SS040 LD      SS900     FETCH THE PHASE ID          CP209540
                          S     2 0         TEST FOR AND BRANCH         CP209550
                          BSC  L  SS060,+-  *IF MATCH FOUND IN SLET     CP209560
                          MDX   2 4         INCREMENT SLET SET POINTER  CP209570
                          MDX   1 -1        DECR SET CNT, SKIP IF ZERO  CP209580
                          MDX     SS040     BRANCH TO TEST NEXT SET     CP209590
                          LD   L  BUFR3+1   FETCH SLET SECTOR ADDRESS   CP209600
                          S     3 BY956-X   TEST FOR AND BRANCH IF LAST CP209610
                          BSC  L  SS080,+-  *SLET SCTR (RET PH ID = 0)  CP209620
                          MDX  L  BUFR3+1,1 INCR SLET SECTOR ADDRESS    CP209630
                          MDX     SS020     BR TO FETCH NEXT SLET SCTR  CP209640
                    SS060 LD      SS900     FETCH FOUND PHASE ID        CP209650
                    SS080 LDX  L1 *-*       RESTORE XR1                 CP209660
                          BSC  I  SSLET     RETURN                      CP209670
                    *                                                   CP209680
                    * CONSTANTS AND WORK AREAS                          CP209690
                    *                                                   CP209700
                    SS900 DC      *-*       PHASE ID BEING LOOKED FOR   CP209710
                    *                                                   CP209720
                    *************************************************** CP209730
                    *                                                   CP209740
                    * UPDATE CORE ADDRESS OF PHASE AND STORE IN SLET.   CP209750
                    *                                                   CP209760
                    UPCAD DC      0         ENTRY/RETURN ADDRESS        CP209770
                          LD    3 BY911-X   FETCH ID OF CURRENT PHASE   CP209780
                          STO   2 0         *AND STORE TO SLET          CP209790
                          LD   L  PKBFR     FETCH ADDR OF CURR PROGRAM  CP209800
                          STO   2 1         *AND STORE TO SLET          CP209810
                          STX   2 UP900     SAVE XR2                    CP209820
                          LD      UP900     FETCH SLET POINTER          CP209830
                          S     3 BY914-X   TEST FOR AND                CP209840
                          BSC  L  UP040,Z   *BRANCH IF NOT LAST SET     CP209850
                          STX  L0 BY920     SET SLET UPDATE INDR ON     CP209860
                    UP040 MDX   2 4         INCR TO NEXT SLET SET       CP209870
                          STX  L2 C1020+1   STORE NEW SLET SET POINTER  CP209880
                          LDX  L1 PKBFR-1   XR1 POINTS TO RECORD BUFFER CP209890
                          STX  L0 BY982     IND SLET ENTRY TO FINISH    CP209900
                          BSC  I  UPCAD     RETURN                      CP209910
                    *                                                   CP209920
                    UP900 DC      *-*       TEMPORARY XR2 STORAGE       CP209930
                    *                                                   CP209940
                    *************************************************** CP209950
                    *                                                   CP209960
                    * WRITE RELOAD TABLE TO DISK.                       CP209970
                    *                                                   CP209980
                    WRIT1 DC      0         ENTRY/RETURN ADDRESS        CP209990
                          LDD     W1900     FETCH FUNC CODE, I/O ADDR   CP210000
                          BSI  L  DZ000     BR TO WRITE RELOAD TABLE    CP210010
                    W1040 MDX  L  $DBSY,0   SKIP IF WRITE COMPLETE      CP210020
                          MDX     W1040     BR TO TEST WRITE COMPLETE   CP210030
                          BSC  I  WRIT1     RETURN                      CP210040
                    *                                                   CP210050
                    * CONSTANTS AND WORK AREAS.                         CP210060
                    *                                                   CP210070
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP210080
                    W1900 DC      /0001     WRITE FUNCTION CODE         CP210090
                          DC      BUFR1     ADDRESS OF RELOAD TABLE BFR CP210100
                    *                                                   CP210110
                    *************************************************** CP210120
                    *                                                   CP210130
                    * WRITE A SECTOR TO THE DISK.                       CP210140
                    *                                                   CP210150
                    WRIT2 DC      0         ENTRY/RETURN ADDRESS        CP210160
                          LD   L  BUFR2+1   FETCH SECTOR ADDRESS        CP210170
                          EOR  L  B2END     TEST FOR AND                CP210180
                          BSC  I  WRIT2,+-  *RETURN IF NOT SET          CP210190
                          LD      W2902     FETCH '81' RECORD INDR      CP210200
                          BSC  L  W2100,+-  BRANCH IF NOT SET           CP210210
                    W2040 LDD     W2900     FETCH FUNC CODE, I/O ADDR   CP210220
                          BSI  L  DZ000     BRANCH TO WRITE A SECTOR    CP210230
                          MDX  L  $DBSY,0   SPIP NEXT IF WRITE COMPLETE CP210240
                          MDX     *-3       BR TO TEST WRITE COMPLETE   CP210250
                          BSC  I  WRIT2     RETURN                      CP210260
                    *                                                   CP210270
                    * SAVE ADDRESS OF HIGHEST SECTOR TO WHICH A         CP210280
                    * PROGRAM WAS LOADED.                               CP210290
                    *                                                   CP210300
                    W2100 LD    3 BY937-X   FETCH PREVIOUS HIGH SECTOR  CP210310
                          S    L  BUFR2+1   TEST FOR AND                CP210320
                          BSC  L  W2140,-   *BRANCH IF GT THIS SECTOR   CP210330
                          LD   L  BUFR2+1   RESET                       CP210340
                          STO   3 BY937-X   *HIGH SECTOR ADDRESS        CP210350
                    W2140 LD    3 BY934-X   FETCH LOAD MODE INDICATOR   CP210360
                          BSC  L  W2040,+Z  BRANCH IF AN INITIAL LOAD   CP210370
                          LD    3 BZ908-X   TEST FOR AND                CP210380
                          BSC  L  W2040,Z   *BRANCH IF PROGRAM(S) ADDED CP210390
                          LD    3 BY935-X   FETCH SECTOR ADDRESS OF CIB CP210400
                          S     3 BX908-X   TEST FOR                    CP210410
                          S    L  BUFR2+1   *AND BRANCH IF              CP210420
                          BSC  L  W2040,Z-  *NO ATTEMPT TO OVERLAY SCRA CP210430
                          BSI  L  ER022     BR TO PRINT ERROR MSG E 22  CP210440
                    *                                                   CP210450
                    * CONSTANTS AND WORK AREAS                          CP210460
                    *                                                   CP210470
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP210480
                    W2900 DC      /0001     WRITE FUNCTION CODE         CP210490
                          DC      BUFR2     ADDRESS OF I/O BUFFER       CP210500
                    W2902 DC      *-*       '81' RECORD READ INDICATOR  CP210510
                    *                                                   CP210520
                    *************************************************** CP210530
                    *                                                   CP210540
                    * WRITE A SLET SECTOR.                              CP210550
                    *                                                   CP210560
                    WRIT3 DC      0         ENTRY/RETURN ADDRESS        CP210570
                          LDD     W3900     FETCH FUNC CODE, I/O ADDR   CP210580
                          BSI  L  DZ000     BRANCH TO WRITE A SLET SCTR CP210590
                          MDX  L  $DBSY,0   SKIP NEXT IF WRITE COMPLETE CP210600
                          MDX     *-3       BR TO TEST WRITE COMPLETE   CP210610
                          BSC  I  WRIT3     RETURN                      CP210620
                    *                                                   CP210630
                    * CONSTANTS AND WORK AREAS                          CP210640
                    *                                                   CP210650
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP210660
                    W3900 DC      /0001     WRITE FUNCTION CODE         CP210670
                          DC      BUFR3     ADDRESS OF SLET I/O AREA    CP210680
                          HDNG    PRINT ERROR MESSAGES                  CP210690
                    ER001 DC      0         ENTRY/RETURN ADDRESS        CP210700
                          SLA     1         GET FRAMECOUNT          2-9 CP210701
                          A     3 BX902-X   ADD 2 FRAMES FOR WC     2-9 CP210702
                          STO   3 BY906-X   STORE FOR DISPLAY       2-9 CP210703
                          LDX  L1 MSG01+1   POINT TO ERROR MESSAGE E 01 CP210710
                          LDX  I2 MSG01     FETCH WORD CNT OF MSG E 01  CP210720
                          BSI  L  CNPTR     PRINT THE MESSAGE           CP210730
                          BSC  I  ER001     RETURN                      CP210740
                    *                                                   CP210750
                    ER002 DC      0         ENTRY/RETURN ADDRESS        CP210760
                          LDX  L1 MSG02+1   POINT TO ERROR MESSAGE E 02 CP210770
                          LDX  I2 MSG02     FETCH WORD CNT OF MSG E 02  CP210780
                          MDX     ER100     BRANCH TO PRINT MESSAGE     CP210790
                    *                                                   CP210800
                    ER003 DC      0         ENTRY/RETURN ADDRESS        CP210810
                          LDX  L1 MSG03+1   POINT TO ERROR MESSAGE E 03 CP210820
                          LDX  I2 MSG03     FETCH WORD CNT OF MSG E 03  CP210830
                          MDX     ER100     BRANCH TO PRINT MESSAGE     CP210840
                    *                                                   CP210850
                    ER004 DC      0         ENTRY/RETURN ADDRESS        CP210860
                          LDX  L1 MSG04+1   POINT TO ERROR MESSAGE E 04 CP210870
                          LDX  I2 MSG04     FETCH WORD CNT OF MSG E 04  CP210880
                          MDX     ER100     BRANCH TO PRINT MESSAGE     CP210890
                    *                                                   CP210900
                    ER005 DC      0         ENTRY/RETURN ADDRESS        CP210910
                          LDX  L1 MSG05+1   POINT TO ERROR MESSAGE E 05 CP210920
                          LDX  I2 MSG05     FETCH WORD CNT OF MSG E 05  CP210930
                          MDX     ER620     PRINT ABORT MESSAGE         CP210940
                    *                                                   CP210950
                    ER018 DC      0         ENTRY/RETURN                CP210960
                          LDX  L1 MSG18+1   POINT TO MESSAGE            CP210970
                          LDX  I2 MSG18     FETCH WORD COUNT            CP210980
                          MDX     ER620     PRINT ABORT MESSAGE         CP210990
                    *                                                   CP211000
                    ER020 DC      0         ENTRY/RETURN ADDRESS        CP211010
                          LDX  L1 MSG20+1   POINT TO ERROR MESSAGE E 20 CP211020
                          LDX  I2 MSG20     FETCH WORD CNT OF MSG E 20  CP211030
                          MDX     ER600     BRANCH TO PRINT THE MESSAGE CP211040
                    *                                                   CP211050
                    ER021 DC      0         ENTRY/RETURN ADDRESS        CP211060
                          LDX  L1 MSG21+1   POINT TO ERROR MESSAGE E 21 CP211070
                          LDX  I2 MSG21     FETCH WORD CNT OF MSG E 21  CP211080
                    ER100 BSI  L  CNPTR     PRINT THE MESSAGE           CP211090
                          BSC  L  BB000     BRANCH TO READ NEXT RECORD  CP211100
                    *                                                   CP211110
                    ER022 DC      0         ENTRY/RETURN ADDRESS        CP211120
                          LDX  L1 MSG22+1   POINT TO ERROR MESSAGE E 22 CP211130
                          LDX  I2 MSG22     FETCH WORD CNT OF MSG E 22  CP211140
                          MDX     ER600     BRANCH TO PRINT MESSAGE     CP211150
                    *                                                   CP211160
                    ER023 DC      0         ENTRY/RETURN ADDRESS        CP211170
                          LDX  L1 MSG23+1   POINT TO ERROR MESSAGE E 23 CP211180
                          LDX  I2 MSG23     FETCH WORD CNT OF MSG E 23  CP211190
                          LD   L  PKBFR-1+11 FETCH PHASE ID             CP211200
                          MDX     ER400     BRANCH TO STORE PHASE ID    CP211210
                    *                                                   CP211220
                    ER024 DC      0         ENTRY/RETURN ADDRESS        CP211230
                          LDX  L1 MSG24+1   POINT TO ERROR MESSAGE E 24 CP211240
                          LDX  I2 MSG24     FETCH WORD CNT OF MSG E 24  CP211250
                          LD   L  PKBFR-1+11 FETCH PHASE ID             CP211260
                          STO   3 BY906-X   STORE PHASE ID FOR DISPLAY  CP211270
                          BSI  L  CNPTR     PRINT THE MESSAGE           CP211280
                          BSC  I  ER024     RETURN                      CP211290
                    *                                                   CP211300
                    ER025 DC      0         ENTRY/RETURN ADDRESS        CP211310
                          LDX  L1 MSG25+1   POINT TO ERROR MESSAGE E 25 CP211320
                          LDX  I2 MSG25     FETCH WORD CNT OF MSG E 25  CP211330
                    ER300 LD    3 BY911-X   FETCH PHASE ID              CP211340
                    ER400 STO   3 BY906-X   STORE PHASE ID FOR DISPLAY  CP211350
                          BSI  L  CNPTR     PRINT THE MESSAGE           CP211360
                          BSI  L  BYPAS     BRANCH TO BYPASS THE PHASE  CP211370
                    *                                                   CP211380
                    ER026 DC      0         ENTRY/RETURN ADDRESS        CP211390
                          STX   1 ER510+1   SAVE XR1                    CP211400
                          STX   2 ER520+1   SAVE XR2                    CP211410
                          STD   3 BY906-X   STORE DISPLAY               CP211420
                          LDX  L1 MSG26+1   POINT TO ERROR MESSAGE E 26 CP211430
                          LDX  I2 MSG26     FETCH WORD CNT OF MSG E 26  CP211440
                          BSI  L  CNPTR     PRINT THE MESSAGE           CP211450
                    ER510 LDX  L1 *-*       RESTORE XR1                 CP211460
                    ER520 LDX  L2 *-*       RESTORE XR2                 CP211470
                          BSC  I  ER026     RETURN                      CP211480
                    *                                                   CP211490
                    ER027 DC      0         ENTRY/RETURN ADDRESS        CP211500
                          LDX  L1 MSG27+1   POINT TO ERROR MESSAGE E 27 CP211510
                          LDX  I2 MSG27     FETCH WORD CNT OF MSG E 27  CP211520
                          MDX     ER620     PRINT ABORT MESSAGE         CP211530
                    *                                                   CP211540
                    ER600 BSI  L  CNPTR     PRINT THE MESSAGE           CP211550
                          NOP               A WAIT MAY BE PATCHED HERE  CP211560
                    ER610 STX  L0 BZ909     SET INDR AND ATTEMPT FINISH CP211570
                          BSC  L  BG040     BRANCH TO FETCH OVERLAY 2   CP211580
                    *                                                   CP211590
                    ER028 DC      0         ENTRY/RETURN ADDRESS        CP211600
                          LDX  L1 MSG28+1   POINT TO ERROR MESSAGE E 28 CP211610
                          LDX  I2 MSG28     FETCH WORD CNT OF MSG E 28  CP211620
                          MDX     ER300     BRANCH TO FETCH DISPLAY     CP211630
                    *                                                   CP211640
                    ER029 DC      0         ENTRY/RETURN ADDRESS        CP211650
                          LDX  L1 MSG29+1   POINT TO ERROR MESSAGE E 29 CP211660
                          LDX  I2 MSG29     FETCH WORD CNT OF MSG E 29  CP211670
                          MDX     ER300     BRANCH TO DISPLAY PHASE ID  CP211680
                    *                                                   CP211690
                    ER030 DC      0         ENTRY                       CP211700
                          LDX  L1 MSG30+1   POINT TO ERROR MESSAGE E 30 CP211710
                          LDX  I2 MSG30     FETCH WORD CNT OF MSG E 30  CP211720
                          MDX     ER300     BRANCH TO PRINT MESSAGE     CP211730
                    *                                                   CP211740
                    ER130 DC      0         ENTRY                       CP211750
                          LDX  L1 MSG30+1   POINT TO ERROR MESSAGE E 30 CP211760
                          LDX  I2 MSG30     FETCH WORD CNT OF MSG E 30  CP211770
                          MDX     ER620     PRINT ABORT MESSAGE         CP211780
                    *                                                   CP211790
                    ER031 DC      0         ENTRY                       CP211800
                          LDX  L1 MSG31+1   POINT TO ERROR MESSAGE E 31 CP211810
                          LDX  I2 MSG31     FETCH WORD CNT OF MSG E 31  CP211820
                          MDX     ER620     PRINT ABORT MESSAGE         CP211830
                    *                                                   CP211840
                    *                                                   CP211850
                    ER032 DC      0         ENTRY                       CP211860
                          LDX  L1 MSG32+1   POINT TO ERROR MSG E 32     CP211870
                          LDX  I2 MSG32     FETCH WD CNT OF MSG E 32    CP211880
                    ER620 STX   1 ER660+1   SAVE XR1                    CP211890
                          STX   2 ER670+1   SAVE XR2                    CP211900
                          LD   L  SS900     LOAD MISSING PHASE ID       CP211910
                          STO   3 BY906-X   SAVE FOR ACCUM DISPLAY      CP211920
                    ER640 BSI  L  CNPTR     PRINT THE MESSAGE           CP211930
                    ER660 LDX  L1 *-*       RESTORE XR1 FOR REPRINT     CP211940
                    ER670 LDX  L2 *-*       RESTORE XR2 FOR REPRINT     CP211950
                          MDX     ER640     NO RECOVERY, PRINT AGAIN    CP211960
                    *                                                   CP211970
                    * ERROR MESSAGES                                    CP211980
                    *                                                   CP211990
                    MSG18 DC      MSG20-*   WD COUNT OF MESSAGE E 18    CP212000
                          DMES    'RE 18 PAPER TAPE ERROR'R'E           CP212010
                    *                                                   CP212020
                    MSG20 DC      MSG21-*   WORD COUNT OF MESSAGE E 20  CP212030
                          DMES    'RE 20 FIXED AREA PRESENT'R'E         CP212040
                    MSG21 DC      MSG22-*   WORD COUNT OF MESSAGE E 21  CP212050
                          DMES    'RE 21 SYSTEM TAPE ERROR'R'E          CP212060
                    MSG22 DC      MSG23-*   WORD COUNT OF MESSAGE E 22  CP212070
                          DMES    'RE 22 SCRA OVERLAY - STOP'R'E        CP212080
                    MSG23 DC      MSG24-*   WORD COUNT OF MESSAGE E 23  CP212090
                          DMES    'RE 23 PHASE ID OUT OF SEQUENCE'R'E   CP212100
                    MSG24 DC      MSG25-*   WORD COUNT OF MESSAGE E 24  CP212110
                          DMES    'RE 24 PHASE MISSING'R'E              CP212120
                    MSG25 DC      MSG26-*   WORD COUNT OF MESSAGE E 25  CP212130
                          DMES    'RE 25 PHASE ID NOT IN PHID RECORD 'R CP212140
                    MSG26 DC      MSG27-*   WORD COUNT OF MESSAGE E 26  CP212150
                          DMES    'RE 26 PHASE ID NOT IN SLET'R'E       CP212160
                    MSG27 DC      MSG28-*   WORD COUNT OF MESSAGE E 27  CP212170
                          DMES    'RE 27 DEFECTIVE SLET'R'E             CP212180
                    MSG28 DC      MSG29-*   WORD COUNT OF MESSAGE E 28  CP212190
                          DMES    'RE 28 SLET FULL'R'E                  CP212200
                    MSG29 DC      MSG30-*   WORD COUNT OF MESSAGE E 29  CP212210
                          DMES    'RE 29 PROGRAM NOT PRESENT'R'E        CP212220
                    MSG30 DC      MSG31-*   WORD COUNT OF MESSAGE E 30  CP212230
                          DMES    'RE 30 RELOAD TABLE FULL'R'E          CP212240
                    MSG31 DC      MSG32-*   WORD COUNT OF MESSAGE E 31  CP212250
                          DMES    'RE 31 MISSING PHASE ID DUE TO'       CP212260
                          DMES     DEFECTIVE SLET OR RELOAD TABLE'R'E   CP212270
                    MSG32 DC      MSG33-*   WORD COUNT OF MESSAGE E 32  CP212280
                          DMES    'RE 32 MISSING SYSTEM I/O PHASE'R'E   CP212290
                    MSG33 BSS     0                                     CP212300
                    *                                                   CP212310
                    * PRINT TO THE CONSOLE PRINTER AND WAIT.            CP212320
                    *                                                   CP212330
                    CNPTR DC      0         ENTRY/RETURN ADDRESS        CP212340
                          BSI  L  WRTYZ     BR TO CONSOLE PRINTER SUBR  CP212350
                          LDD   3 BY906-X   FETCH ACC, EXT TO DISPLAY   CP212360
                          BSI  L  $PRET     BRANCH TO PRE-OP TRAP       CP212370
                          SLT     32        CLEAR                       CP212380
                          STD   3 BY906-X   *DISPLAY WORDS              CP212390
                          BSC  I  CNPTR     RETURN                      CP212400
                          HDNG    PHASE 2 CONSTANTS AND WORK AREAS      CP212410
                    BX901 DC      1         CONSTANT ONE                CP212420
                    BX902 DC      2         CONSTANT TWO                CP212430
                    BX903 DC      3         CONSTANT THREE              CP212440
                    BX904 DC      4         CONSTANT FOUR               CP212450
                    BX905 DC      5         CONSTANT FIVE               CP212460
                    BX906 DC      6         CONSTANT SIX                CP212470
                    BX908 DC      8         CONSTANT EIGHT              CP212480
                    BX909 DC      16        NO. OF SCTRS IN 2 CYLINDERS CP212490
                    BX910 DC      40        NO. OF SCTRS IN 5 CYLINDERS CP212500
                    BX912 DC      /0072     USED TO DETERMINE RCD TYPE  CP212510
                    BX913 DC      @SCNT-1   NO. OF WORDS PER SECTOR - 1 CP212520
                    BX914 DC      @SCNT     NO. OF WORDS PER SECTOR     CP212530
                    BX915 DC      /0999     CON FOR END OF SKIP TBL 2-3 CP212540
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP212550
                    BY900 DC      *-*       USED TO DETERMINE SCTR ADDR CP212560
                          DC      *-*       *AND RLTV LOC OF DATA WORDS CP212570
                    BY902 DC      *-*       USED TO DETERMINE NO. OF    CP212580
                    BY903 DC      *-*       *SECTORS A PHASE OCCUPIES   CP212590
                    BY904 DC      *-*       CURRENT                     CP212600
                    BY905 DC      *-*       *PHASE ID LIMITS            CP212610
                    BY906 DC      *-*       DISPLAY                     CP212620
                          DC      *-*       *WORDS                      CP212630
                    BY909 DC      1         FIRST PHASE ID OF A PAIR    CP212640
                    BY910 DC      8         1ST SCTR ADDR OF NEW PHASE  CP212650
                    BY911 DC      *-*       CURRENT PHASE ID            CP212660
                    BY912 DC      *-*       PREVIOUS PHASE ID           CP212670
                    BY913 DC      BUFR3+2   ADDRESS OF FIRST SLET SET   CP212680
                    BY914 DC      BUFR3+2+316  ADDRESS OF LAST SLET SET CP212690
                    BY915 DC      *-*       1ST SCTR ADDR OF EACH PHASE CP212700
                    BY916 DC      *-*       CURRENT SECTOR ADDRESS      CP212710
                    BY917 DC      *-*       FROM ADDRESS FOR FETCH      CP212720
                    BY918 DC      *-*       TO ADDRESS FOR STORE        CP212730
                    BY919 DC      *-*       MAXIMUM WORD CNT PER PHASE  CP212740
                    BY920 DC      *-*       SLET UPDATE INDICATOR       CP212750
                    BY922 DC      @RLTB-@SLET  NO. OF SECTOR OF SLET    CP212760
                    BY923 DC      *-*       HIGHEST SCTR USED BY SYS    CP212770
                    BY924 DC      *-*       PHASE ID OF LAST SYS PROG   CP212780
                    BY925 DC      *-*       BYPASSING PHASES INDICATOR  CP212790
                    BY926 DC      *-*       PHASE ID TO BE BYPASSED     CP212800
                    BY927 DC      *-*       SECTOR BREAK RECORD INDR    CP212810
                    BY928 DC      1         TYPE 'F' RECORD INDICATOR   CP212820
                    BY931 DC      1         LAST PHASE INDICATOR        CP212830
                    BY933 DC      *-*       TYPE 'A' RECORD INDICATOR   CP212840
                    X     DC      *-*       FOR EMERGENCY USE           CP212850
                    BY934 DC      *-*       LOAD MODE INDICATOR         CP212860
                    BY935 DC      /0333     SECTOR ADDRESS OF CIB       CP212870
                    BY936 DC      *-*       CORE ADDR OF CURR DATA WORD CP212880
                    BY937 DC      7         HIGHEST SCTR ADDR WRITTEN   CP212890
                    BY938 DC      *-*       SCTR ADDR OF SECTOR IN CORE CP212900
                    BY939 DC      *-*       FIRST DATA RECORD INDR      CP212910
                    BY940 DC      *-*       CORE ADDRESS CORRECTION     CP212920
                    BY941 DC      *-*       PHASE BYPASS INDICATOR      CP212930
                    BY942 DC      0         END OF PROGRAM INDICATOR    CP212940
                    BY943 DC      0         USED TO SET CORRECTION      CP212950
                    BY944 DC      0         CHECKSUM SEQUENCE COUNTER   CP212960
                    BY945 DC      1         1ST SCTR ADDR OF NEW PHASE  CP212970
                    BY947 DC      /7777     PHASE LENGTH IN SECTORS     CP212980
                    BY950 DC      BUFR2     ADDRESS OF DATA BUFFER      CP212990
                    BY951 DC      $ZEND-@CSTR-4-2  RLTV LOC OF CIL DATA CP213000
                    BY952 DC      @DCOM     DCOM SECTOR ADDRESS         CP213010
                    BY954 DC      @RIAD     SECTOR ADDRESS OF RES IMAGE CP213020
                    BY955 DC      @SLET     FIRST SLET SECTOR ADDRESS   CP213030
                    BY956 DC      @SLET+2   THIRD SLET SECTOR ADDRESS   CP213040
                    BY957 DC      /00FF     MASK OUT TYPE IN WORD 3     CP213050
                    BY958 DC      @P2AD     SCTR ADDR OF SYS LDR PH 2   CP213060
                    BY960 DC      @P2AD+5+2+3    SCTR ADDR OF OVERLAY 2 CP213070
                    BY961 DC      @P2AD+5+2+3+2  SCTR ADDR OF OVERLAY 3 CP213080
                    BY962 DC      /063F     MAXIMUM SECTOR ADDRESS      CP213090
                    BY963 DC      @MNCR-6   WORD COUNT OF CIB           CP213100
                    BY964 DC      /8000     NEGATIVE SIGN BIT           CP213110
                    BY965 DC      /FFFF     COMPLIMENTING MASK          CP213120
                    BY966 NOP               'NOP' INSTRUCTION           CP213130
                    BY967 MDX     *         SKIP INSTRUCTION            CP213140
                    BY969 MDX     *+2       SKIP INSTRUCTION            CP213150
                    BY970 MDX  X  C1050-C1030-1  SKIP INSTRUCTION       CP213160
                    BY972 DC      /3FFF     USED TO MASK FOR LEFT BITS  CP213170
                    BY973 S    L  /7FFF     SUBTRACT INSTRUCTION        CP213180
                    BY974 EQU     *-1       USED TO MASK OUT SIGN BIT   CP213190
                    BY975 MDX  X  CK080-CK060-1  CHECKSUM SKIP INST     CP213200
                    BY976 DC      /70FF     INST FOR LOOP IN LOC 0  2-8 CP213210
                    BY977 DC      *-*       PHASE FINAL WORD COUNT  2-4 CP213220
                    BY978 DC      *-*       NO. OF SCTRS TO SHIFT   2-4 CP213230
                    BY979 DC      *-*       AVAILABLE               2-4 CP213240
                    BY980 DC      *-*       AVAILABLE               2-4 CP213250
                    BY981 DC      *-*       PROG DATA TO WRITE INDR     CP213260
                    BY982 DC      *-*       SLET ENTRY HALF DONE INDR   CP213270
                    BY983 DC      7         HIGHEST SCTR ADDR RECOVERY  CP213280
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP213290
                    BZ900 DC      *-*       QUOTIENT FROM MOVE SUBR     CP213300
                    BZ901 DC      *-*       REMAINDER FROM MOVE SUBR    CP213310
                    BZ902 DC      *-*       AVAILABLE               2-4 CP213320
                    BZ903 DC      *-*       NEW LET SECTOR ADDRESS      CP213330
                    BZ904 DC      PAIRE     ADDR OF PAIR 14 (LAST PAIR) CP213340
                    BZ905 DC      PAIR8     ADDR OF PAIR 08             CP213350
                    BZ906 DC      *-*       NEW MINUS OLD LET SCTR ADDR CP213360
                    BZ907 DC      *-*       NO. OF SCTRS IN SHIFT BFR   CP213370
                    BZ908 DC      0         NEW PROGRAM RELOADED INDR   CP213380
                    BZ909 DC      *-*       END RELOAD/XEQ MODIF INDR   CP213390
                    BZ910 DC      @IDAD     CARTRIDGE ID SECTOR ADDRESS CP213400
                    BZ911 DC      *-*       LOAD NO MORE PHASES INDR    CP213410
                    BZ912 DC      *-*       PROGRAM MAY BE ADDED INDR   CP213420
                    BZ913 DC      *-*       PHASE ID OF NEXT PROG - 1   CP213430
                    *                                                   CP213440
                    * LEVEL 4 INTERRUPT BRANCH TABLE                    CP213450
                    *                                                   CP213460
                          DC      $PRET     PRE-OPERATIVE ERROR TRAP    CP213470
                    BZ930 BSS     4         RESERVED                    CP213480
                          DC      *-*       1403 PRINTER                CP213490
                          DC      *-*       2501 CARD READER            CP213500
                          DC      *-*       1442 CARD READER            CP213510
                          DC      TZ100     CONSOLE PRINTER             CP213520
                          DC      PT000+4   PAPER TAPE READER/PUNCH     CP213530
                    *                                                   CP213540
                    *                                                   CP213550
                          BSS     BA000+5*@SCNT-*-2  PATCH AREA         CP213560
                    *                                                   CP213570
                    *                                                   CP213580
                    BZ999 EQU     *-1       END OF PHASE 2 MAINLINE     CP213590
                          HDNG    OVERLAY 0 - EXPAND SYS PROG AREA      CP213600
                          ORG     BA000+5*@SCNT-2 ALLOW 5 SECTOR ML     CP213610
                    *                                                   CP213620
                    *                                                   CP213630
                    OVLAY DC      C0999-C0000+2  WORD CNT OF OVERLAY 0  CP213640
                          DC      @P2AD+5        SCTR ADDR OF OVERLAY 0 CP213650
                    *                                                   CP213660
                    *                                                   CP213670
                          DC      0         INDICATES OVERLAY 0         CP213680
                    C0000 MDX     C0020     BR AROUND MODIFIABLE PT     CP213690
                          LDX  L2 BUFR3+2   2ND HALF IS USED ELSEWHERE  CP213700
                    C0020 LD    3 BZ908-X   IF INDR ON, NEW PROG IS     CP213710
                          BSC  L  C0050,+-  *BEING ADDED.  BR IF OFF    CP213720
                          LD   L  C0610+1   FETCH A PAIR ADDRESS        CP213730
                          A     3 BX901-X   POINT TO 2ND OF PAIR        CP213740
                          STO     C0040+1   PLACE ADDR IN INSTRUCTION   CP213750
                          LD    1 11        WHEN LAST PHASE TO LOAD     CP213760
                          S    L  MAXPH     *IS REACHED, AN INDICATOR   CP213770
                          STO   3 BY931-X   *IS CLEARED                 CP213780
                          LD    3 BZ913-X   IF NON-ZERO, USE AS PH ID   CP213790
                          BSC     Z         *IN SEQUENCE TEST           CP213800
                          STO   3 BY912-X   REPLACE PREVIOUS PH ID      CP213810
                          LD    3 BY911-X   CURRENT PHASE               CP213820
                          S     3 BY912-X   PREVIOUS PHASE              CP213830
                          S     3 BX901-X   SEQUENCE TEST               CP213840
                          BSC  L  C0660,Z   BRANCH IF NOT IN SEQUENCE   CP213850
                          STO   3 BZ913-X   CLEAR SPECIAL PH ID         CP213860
                    C0040 LD   L  *-*       THIS PHASE SHOULD NOT       CP213870
                          S     3 BY911-X   *EXCEED PHID PAIR LIMIT     CP213880
                          BSC  L  C0670,+Z  GO TO OUT OF SEQ ERROR      CP213890
                          BSC  L  C0060,Z   BR AROUND SLET SEARCH       CP213900
                    *                                                   CP213910
                    * THIS PHASE IS LAST PHASE OF A PROGRAM PAIR        CP213920
                    *                                                   CP213930
                          LD    3 BX901-X   SET SW TO INDICATE A PROG   CP213940
                          STO   3 BZ912-X   *HAS BEEN ADDED & PREPARE   CP213950
                          BSI  L  C0600     *TO ADD ANOTHER IF REQ      CP213960
                    *                                                   CP213970
                    C0050 LD    1 11        RELOAD THE PH ID            CP213980
                          BSI  L  SSLET     SEARCH SLET FOR PHASE       CP213990
                    *                                                   CP214000
                    * IF PHASE FOUND XR2 POINTS TO THE ADDRESS OF SET   CP214010
                    * IN CORE BUFFER.  THE APPLICABLE SLET SECTOR IS    CP214020
                    * LOADED TO THE IN-CORE BUFFER DURING A SUCCESSFUL  CP214030
                    * SEARCH.                                           CP214040
                    *                                                   CP214050
                          BSI  L  C0600,+   BR TO SEE IF CAN RELOAD     CP214060
                          STX  L2 C0510+1   SAVE XR2 IN 'C0500' SUBR    CP214070
                          LD    2 2         FETCH WD CNT TO RUN CHECK   CP214080
                          A     3 BX913-X   ADD WORDS/SCTR-1        2-4 CP214090
                          SRT     16        PREPARE FOR DIVIDE          CP214100
                          D     3 BX914-X   DETERMINE HOW MANY SCTRS    CP214110
                          STO   3 BY947-X   *SAVE AS LENGTH IN SECTORS  CP214120
                          LD    2 3         LOAD THIS PHASE TO OLD SCTR CP214130
                    *                       STATMENT REMOVED        2-4 CP214140
                          STO   3 BY910-X   FIRST SCTR ADDR OF PHASE    CP214150
                    C0060 BSI  L  UPCAD     BR TO UPDATE OLD SLET ENTRY CP214160
                          SRA     16        CLEAR SW THAT IS SET        CP214170
                          STO   3 BZ912-X   *BETWEEN ADDED PROGRAMS     CP214180
                          BSC  L  BE200     RETURN FROM OVERLAY 0       CP214190
                    *                                                   CP214200
                    *************************************************** CP214210
                    *                                                   CP214220
                    * INSERT A 4 WORD ENTRY TO BE USED TO BUILD A NEW   CP214230
                    * PHASE IN SLET.                                    CP214240
                    *                                                   CP214250
                    C0100 DC      0         ENTRY/EXIT TO SUBROUTINE    CP214260
                          LD    1 11        PHASE ID TO BE INSERTED     CP214270
                          STO     C0912     STORE ID IN INSERTION PATCH CP214280
                          S     3 BX901-X   PHASE ID - 1                CP214290
                          BSI  L  SSLET     CALL SEARCH SLET FOR ID-1   CP214300
                          BSC  L  CO180,+-  BR IF ID-1 WAS NOT FOUND    CP214310
                          SLA     16        ZERO TWO WORDS IN NEW SLET  CP214320
                          STO     C0913     *ENTRY FOR NOW              CP214330
                          STO     C0914                                 CP214340
                          LD    2 2         WORD COUNT OF PH - 1        CP214350
                          A     3 BX913-X   ADD WORDS/SCTR-1        2-4 CP214360
                          SRT     16        SHIFT TO EXTENSION          CP214370
                          D     3 BX914-X   DIVIDE BY WORDS PER SECTOR  CP214380
                          A     2 3         ADD SECTOR ADDR OF ID-1     CP214390
                          STO     C0915     SAVE CALC SCTR ADDR OF PH   CP214400
                    *                                                   CP214410
                    * DETERMINE IF AN EMPTY SLET ENTRY EXISTS TO        CP214420
                    * ABSORD THE NEW SLET ENTRY.                        CP214430
                    *                                                   CP214440
                          MDX   2 4         ADVANCE 1 ENTRY IN SLET TBL CP214450
                          STX   2 C0911     TEST FOR BEYOND END OF SCTR CP214460
                          LD      C0911     CONTENTS OF XR2             CP214470
                          S     3 BY914-X   SUB ADDR LAST SLET ENTRY    CP214480
                          BSC  L  C0120,+   BR IF XR2 STILL IN SECTOR   CP214490
                          LD   L  BUFR3+1   SLET TABLE IS FULL IF THIS  CP214500
                          S     3 BY956-X   *IS LAST SLET SECTOR        CP214510
                          BSI  L  ER028,-   YES, BR IF LAST SLET SECTOR CP214520
                          MDX  L  BUFR3+1,1 INCREMENT SLET SECTOR ADDR  CP214530
                          BSI  L  FTCH3     READ NEXT SLET SECTOR       CP214540
                          LDX  I2 BY913     RESET PT TO FIRST SLET SET  CP214550
                    C0120 LD   I  BY914     TEST FOR EMPTY ENTRY IN     CP214560
                          BSC  L  C0150,+-  *THIS SLET SCTR,BR IF EMPTY CP214570
                          LD   L  BUFR3+1   SAVE SCTR ADDR THAT IS TO   CP214580
                          STO     C0911     *HAVE NEW PHASE ENTRY       CP214590
                          S     3 BY956-X   TEST IF SCTR ADDR ALREADY   CP214600
                          BSC  L  C0130,+-  *IN CORE, BR IF IN CORE     CP214610
                          LD    3 BY956-X   SET LAST SLET SCTR ADDR     CP214620
                          STO  L  BUFR3+1   *IN DISK I/O BUFFER         CP214630
                          BSI  L  FTCH3     READ LAST SCTR INTO CORE    CP214640
                    C0130 LD   I  BY914     TEST FOR EMPTY ENTRY IN     CP214650
                          BSI  L  ER028,Z   *LAST SLET SCTR, BR IF FULL CP214660
                          LD   L  BUFR3+1   IS SCTR TO HAVE PHASE ID    CP214670
                          S       C0911     *INSERTION ALREADY IN CORE  CP214680
                          BSC  L  C0150,+-  BR IF IT IS IN CORE         CP214690
                          LD      C0911     SET ADDR TO SCTR TO HAVE    CP214700
                          STO  L  BUFR3+1   *NEW PHASE ID INSERTED      CP214710
                    C0140 BSI  L  FTCH3     READ THE SECTOR OF SLET     CP214720
                    C0150 LDD   2 0         SAVE 4 WORDS OF SLET TABLE  CP214730
                          STD     C0916     *WHERE NEXT INSERT IS TO GO CP214740
                          LDD   2 2                                     CP214750
                          STD     C0918                                 CP214760
                          LDD     C0912     INSERT 4 WORDS IN SLET      CP214770
                          STD   2 0         *TABLE                      CP214780
                          LDD     C0914                                 CP214790
                          STD   2 2                                     CP214800
                          LDD     C0916     IS NEXT PHASE 0, IE EMPTY   CP214810
                          BSC  L  C0160,Z   NO, BR TO CONTINUE SHIFTING CP214820
                          BSI  L  WRIT3     YES,WRITE SCTR BACK TO DISK CP214830
                          MDX     C0170     BR TO RETURN                CP214840
                    C0160 STD     C0912                                 CP214850
                          LDD     C0918                                 CP214860
                          STD     C0914                                 CP214870
                          MDX   2 4         INCR POINTER ONE 4 WD ENTRY CP214880
                          STX   2 C0911     TEST FOR END OF SECTOR      CP214890
                          LD      C0911     CONTENTS OF XR2             CP214900
                          S     3 BY914-X   SUB ADDR LAST SLET ENTRY    CP214910
                          BSC  L  C0150,+   NO, BR TO LOOP ON ENTRIES   CP214920
                          BSI  L  WRIT3     WRITE SECTOR BACK TO DISK   CP214930
                          MDX  L  BUFR3+1,1 INCREMENT SLET SECTOR ADDR  CP214940
                          LDX  I2 BY913     RESET PT TO FIRST SLET SET  CP214950
                          MDX     C0140     BR TO PROCESS NEXT SECTOR   CP214960
                    C0170 BSC  I  C0100     RETURN                      CP214970
                    *                                                   CP214980
                    CO180 SRT     32        CLEAR ACCUM AND EXTENSION   CP214990
                          LD   L  SS900     FETCH MISSING PHASE ID      CP215000
                          BSI  L  ER026     DISPLAY PH ID NOT IN SLET   CP215010
                          BSC  L  ER610     FETCH OVERLAY 2 TO FINISH   CP215020
                    *                                                   CP215030
                    * CONSTANTS AND WORK AREAS                          CP215040
                    *                                                   CP215050
                    C0911 DC      *-*       TEMPORARY STORAGE           CP215060
                          BSS  E                                        CP215070
                    C0912 DC      *-*       STORAGE FOR 4 WORDS GOING   CP215080
                    C0913 DC      *-*       *TO                         CP215090
                    C0914 DC      *-*       *SLET                       CP215100
                    C0915 DC      *-*       *ENTRY                      CP215110
                    C0916 DC      *-*       STORAGE FOR 4 WORDS COMING  CP215120
                          DC      *-*       *FROM                       CP215130
                    C0918 DC      *-*       *SLET                       CP215140
                          DC      *-*       *ENTRY                      CP215150
                    *                                                   CP215160
                    *************************************************** CP215170
                    *                                                   CP215180
                    * THIS SUBROUTINE MOVES A BLOCK OF 1 TO 90 SECTORS, CP215190
                    * DEPENDING UPON CORE SIZE, FROM 1 PART OF DISK     CP215200
                    * STORAGE TO ANOTHER.                               CP215210
                    *                                                   CP215220
                    C0300 DC      0         ENTRY/RETURN ADDRESS        CP215230
                          NOP               A WAIT MAY BE PATCHEC HERE  CP215240
                          LD   L  SSBFR     RELOCATE BUFFER SIZE TO     CP215250
                          STO   3 BZ907-X   *MAIN CONSTANTS AREA        CP215260
                          LD    3 BY978-X   LOAD SCTRS TO SHIFT CNT 2-4 CP215270
                          SRT     16        SHIFT INTO EXTENSION        CP215280
                          D     3 BZ907-X   DIVIDE BY SCTRS IN BUFFER   CP215290
                          STD   3 BZ900-X   SAVE QUOTIENT AND REMAINDER CP215300
                    *                                               2-4 CP215310
                          LD    3 BY917-X   CALCULATE SHIFT         2-4 CP215320
                          S     3 BY918-X   *DIRECTION (FROM - TO)  2-4 CP215330
                          BSC  L  C0310,-   BR IF LEFT SHIFT        2-4 CP215340
                    *                                               2-4 CP215350
                          LD    3 BY917-X   ADD SECTORS             2-4 CP215360
                          A     3 BY978-X   *TO SHIFT COUNT         2-4 CP215370
                          STO   3 BY917-X   *TO 'FROM'              2-4 CP215380
                          LD    3 BY918-X   *AND TO                 2-4 CP215390
                          A     3 BY978-X   *'TO'                   2-4 CP215400
                          STO   3 BY918-X   *ADDRESSES              2-4 CP215410
                          MDX     C0400     BR TO START RIGHT SHIFT 2-4 CP215420
                    *                                               2-4 CP215430
                    C0310 MDX  L  BZ901,0   TEST REMAINDER FOR 0    2-4 CP215440
                          MDX     C0330     BR IF NOT                   CP215450
                    C0320 MDX  L  BZ900,0   TEST QUOTIENT FOR 0         CP215460
                          MDX     C0380     BR IF NOT                   CP215470
                          MDX     C0355     BR SHIFTING COMPLETED       CP215480
                    *                                                   CP215490
                    * SET UP A WORD COUNT LESS THAN FULL BUFFER SIZE    CP215500
                    *                                                   CP215510
                    C0330 LD    3 BZ901-X   SECTOR COUNT IN REMAINDER   CP215520
                          M     3 BX914-X   WORDS PER SECTOR            CP215530
                          SLT     16                                    CP215540
                          STO  L  BUFR3+0   WORD COUNT                  CP215550
                    *                                                   CP215560
                    C0340 LD    3 BY917-X   SET SCTR ADDR               CP215570
                          STO  L  BUFR3+1   *TO READ FROM               CP215580
                          BSI  L  FTCH3     READ                        CP215590
                          LD    3 BY918-X   SET SCTR ADDR               CP215600
                          STO  L  BUFR3+1   *TO WRITE TO                CP215610
                          BSI  L  WRIT3     WRITE                       CP215620
                          MDX  L  BZ901,0   TEST REMAINDER FOR 0        CP215630
                          MDX     C0410     BR IF NOT                   CP215640
                    *                       STATEMENTS REMOVED      2-4 CP215650
                    C0350 MDX  L  BZ900,0   TEST QUOTIENT FOR 0         CP215660
                          MDX     C0360     BR IF NOT                   CP215670
                    C0355 LD    3 BX914-X   RESET TO 1 SCTR WD COUNT    CP215680
                          STO  L  BUFR3+0                               CP215690
                          BSC  I  C0300     RETURN                      CP215700
                    *                                                   CP215710
                    C0360 LD    3 BY917-X   CALCULATE SHIFT         2-4 CP215720
                          S     3 BY918-X   *DIRECTION              2-4 CP215730
                          BSC  L  C0370,-   BR IF LEFT SHIFT        2-4 CP215740
                          LD    3 BY918-X   DECR SCTR ADDR'S BY SIZE    CP215750
                          S     3 BZ907-X   *OF BUFFER                  CP215760
                          STO   3 BY918-X   WRITE ADDRESS               CP215770
                          LD    3 BY917-X   DECR THE                    CP215780
                          S     3 BZ907-X   *READ ADDR                  CP215790
                          STO   3 BY917-X   *ALSO                       CP215800
                          MDX     C0380     SET NEW WD COUNT FOR I/O    CP215810
                    *                                                   CP215820
                    C0370 LD    3 BY918-X   INCREMENT SCTR              CP215830
                          A     3 BZ907-X   *ADDR TO                    CP215840
                          STO   3 BY918-X   *WRITE TO                   CP215850
                          LD    3 BY917-X   INCREMENT SCTR              CP215860
                          A     3 BZ907-X   *ADDR TO                    CP215870
                          STO   3 BY917-X   *READ FROM                  CP215880
                    *                                                   CP215890
                    C0380 LD    3 BZ907-X   MULTIPLY SCTR COUNT BY      CP215900
                          M     3 BX914-X   *WORDS PER SCTR             CP215910
                          SLT     16        *AND USE                    CP215920
                          STO  L  BUFR3+0   *FOR DISK I/O               CP215930
                          MDX  L  BZ900,-1  DECR QUOTIENT               CP215940
                          NOP               MAY SKIP                    CP215950
                          MDX     C0340     SET UP READ ADDR            CP215960
                    *                                                   CP215970
                    C0400 LD    3 BZ901-X   TEST REMAINDER FOR 0        CP215980
                          BSC  L  C0350,+-  BR IF YES                   CP215990
                          LD    3 BY917-X   DECR THE                    CP216000
                          S     3 BZ901-X   *READ                       CP216010
                          STO   3 BY917-X   *ADDRESS                    CP216020
                          LD    3 BY918-X   DECR THE                    CP216030
                          S     3 BZ901-X   *WRITE                      CP216040
                          STO   3 BY918-X   *ADDRESS                    CP216050
                          MDX     C0330     SET NEW WD COUNT FOR I/O    CP216060
                    *                                                   CP216070
                    C0410 LD    3 BY917-X   CALCULATE SHIFT         2-4 CP216080
                          S     3 BY918-X   *DIRECTION              2-4 CP216090
                          BSC  L  C0420,Z+  BR IF RIGHT SHIFT       2-4 CP216100
                          LD    3 BY918-X   INCR THE                    CP216110
                          A     3 BZ901-X   *WRITE                      CP216120
                          STO   3 BY918-X   *ADDRESS                    CP216130
                          LD    3 BY917-X   INCR THE                    CP216140
                          A     3 BZ901-X   *READ                       CP216150
                          STO   3 BY917-X   *ADDRESS                    CP216160
                          SRA     16        ZERO                        CP216170
                          STO   3 BZ901-X   *THE REMAINDER              CP216180
                          MDX     C0320     BR TO TEST QUOTIENT         CP216190
                    *                                                   CP216200
                    C0420 SRA     16        ZERO                        CP216210
                          STO   3 BZ901-X   *THE REMAINDER              CP216220
                          MDX     C0350     BR TO TEST QUOTIENT         CP216230
                    *                                                   CP216240
                    *************************************************** CP216250
                    *                                                   CP216260
                    * SHIFT SYSTEM PROGRAMS ON SECTOR                   CP216270
                    * OUTWARD TOWARDS THE CORE IMAGE BUFFER.            CP216280
                    *                                                   CP216290
                    C0500 NOP               A WAIT MAY BE PATCHED HERE  CP216300
                          STX  L2 C0580+1   SAVE XR2'S WD COUNT         CP216310
                    C0510 LDX  L2 *-*       RELATIVE LOCATION IN SLET   CP216320
                          LD    2 0         IF THIS IS LAST PHASE       CP216330
                          S     3 BY924-X   *NO SHIFT IS REQUIRED       CP216340
                          BSC  L  C0580,-   BR IF .GE. LAST PHASE   2-4 CP216350
                          LD   L  BUFR3+1   SAVE CURRENT SLET SCTR NO.  CP216360
                          STO     C0950     *FOR RE-READING LATER       CP216370
                    *                                                   CP216380
                    * DETERMINE SECTOR ADDRESS OF LAST SECTOR           CP216390
                    * TO BE MOVED                                       CP216400
                    *                                                   CP216410
                          LD    2 3         CURRENT PHASE SCTR ADDRESS  CP216420
                    *                       STATMENT REMOVED        2-4 CP216430
                          A     3 BY947-X   FORM SCTR ADDR OF SUCCEED-  CP216440
                          STO   3 BY917-X   *ING PHASE, 'FROM' ADDR 2-4 CP216450
                          A     3 BX901-X   'TO' ADDR = 'FROM' ADDR 2-4 CP216460
                          STO   3 BY918-X   * + 1                   2-4 CP216470
                          LD    3 BY935-X   SAVE THE SLET SCTR          CP216480
                          STO  L  BUFR3+1   *NOW IN THE SLET BUFFER     CP216490
                          BSI  L  WRIT3     WRITE TEMPORARILY TO CIB    CP216500
                    *                                                   CP216510
                    * DETERMINE SECTOR ADDRESS OF LAST SECTOR USED FOR  CP216520
                    * SYSTEM PROGRAMS AND VERIFY THAT THERE IS ENOUGH   CP216530
                    * CUSHION TO ABSORB A ONE SECTOR SHIFT.             CP216540
                    *                                                   CP216550
                          NOP                                           CP216560
                          LD    3 BY924-X   KNOWN ID OF LAST PHASE      CP216570
                          BSI  L  SSLET     SEARCH SLET FOR THIS ID     CP216580
                          BSI  L  ER027,+   BR IF SOMEHOW MISSING       CP216590
                          LD    2 2         LOAD WORD COUNT             CP216600
                          SRT     16                                    CP216610
                          D     3 BX914-X   CALC SECTORS INVOLVED       CP216620
                          STO   3 BY923-X   SAVE QUOTIENT               CP216630
                          RTE     16        TEST FOR REMAINDER          CP216640
                          BSC  L  C0520,+   EXACT MULT OF 320 IF ZERO   CP216650
                          MDX  L  BY923,1   SCTRS IN VERY LAST PHASE    CP216660
                    C0520 LD    2 3         SCTR ADDR OF THIS PHASE     CP216670
                    *                       STATMENT REMOVED        2-4 CP216680
                          A     3 BY923-X   FORM ADDR OF LAST SECTOR    CP216690
                          STO   3 BY937-X   *WITH DATA                  CP216700
                          S     3 BY935-X   COMPARE WITH                CP216710
                          A     3 BX901-X   *SCRA                       CP216720
                          S     3 BX908-X   *ADDRESS                    CP216730
                          BSI  L  ER022,-   BRANCH IF NO SPACE LEFT     CP216740
                          LD    3 BY937-X   NUMBER OF SECTORS TO    2-4 CP216750
                          S     3 BY917-X   *SHIFT = ADDR SCTR AFTER2-4 CP216760
                          STO   3 BY978-X   *LAST PHASE - FROM ADDR 2-4 CP216770
                          LD   L  CAREA     DECR BY 1 THS SECTORS       CP216780
                          S     3 BX901-X   *REMAINING IN CUSHION       CP216790
                          BSI  L  ER022,Z+  WHEN USED UP, DISPLAY       CP216800
                          STO  L  CAREA     *SCRA OVERLAY ERROR         CP216810
                          BSI  L  C0300     BR TO SHIFT AREA 1 SECTOR   CP216820
                    *                                                   CP216830
                          LD    3 BY935-X   PREPARE TO FETCH SAVED      CP216840
                          STO  L  BUFR3+1   *SLET SCTR FROM CIB AREA    CP216850
                          BSI  L  FTCH3     READ                        CP216860
                          LD      C0950     RESTORE THE SLET            CP216870
                          STO  L  BUFR3+1   *SCTR ADDR                  CP216880
                          LDX  I2 C0510+1   RESET RELATIVE LOC POINTER  CP216890
                    C0540 LD   L  2         TEST WHETHER THIS SET OF    CP216900
                          S     3 BY914-X   *SLET SCTR IS THE LAST      CP216910
                          BSC  L  C0550,+-  FALL THRU IF NOT LAST       CP216920
                          MDX   2 4         POINT TO NEXT SET           CP216930
                          LD    2 3         LOAD FROM NEXT SET          CP216940
                          BSC     Z         NO MODIFICATION IF ZERO     CP216950
                          A     3 BX901-X   INCR SECTOR ADDRESS         CP216960
                          STO   2 3         STORE BACK                  CP216970
                          LD    2 0         TEST FOR VERY LAST PH ID    CP216980
                          S     3 BY924-X                               CP216990
                          BSC  L  C0560,+-  FALL THRU IF NOT LAST       CP217000
                          MDX     C0540     REPEAT                      CP217010
                    C0550 BSI  L  WRFT3     WRITE UPDATED SLET SECTOR   CP217020
                          LD   L  BUFR3+1   STOP WHEN                   CP217030
                          S     3 BY922-X   *ALL SLET                   CP217040
                          S     3 BY955-X   *UPDATED                    CP217050
                          BSC  L  C0570,-   BR TO END OF THIS SUBR,     CP217060
                          LDX  L2 BUFR3+2-4 *ELSE POINT TO NEXT SCTR    CP217070
                          MDX     C0540     *OF SLET AND REPEAT         CP217080
                    C0560 BSI  L  WRIT3     WRITE UPDATED SLET          CP217090
                    *                                                   CP217100
                    C0570 LD      C0950                                 CP217110
                          STO  L  BUFR3+1   RESTORE IN-CORE SLET SCTR   CP217120
                          BSI  L  FTCH3     *TO CONTINUE LOAD           CP217130
                    C0580 LDX  L2 *-*       RESTORE WD CNT OF DATA RCD  CP217140
                          MDX  L  BY947,1   INCR OLD PHASE LENGTH       CP217150
                          BSC  L  BE380     EXIT BACK TO MAINLINE       CP217160
                    *                                                   CP217170
                    * CONSTANTS AND WORK AREAS                          CP217180
                    *                                                   CP217190
                    C0950 DC      *-*       SAVED SLET SECTOR ADDRESS   CP217200
                    *                                                   CP217210
                    *************************************************** CP217220
                    *                                                   CP217230
                    * WRITE A SLET SECTOR TO DISK AND READ NEXT SECTOR  CP217240
                    *                                                   CP217250
                    WRFT3 DC      0         ENTRY/RETURN ADDRESS        CP217260
                          BSI  L  WRIT3     FILE THIS SLET SECTOR       CP217270
                          MDX  L  BUFR3+1,1 INCR ADDR AND               CP217280
                          BSI  L  FTCH3     *FETCH NEXT SCTR            CP217290
                          BSC  I  WRFT3     RETURN                      CP217300
                    *                                                   CP217310
                    *************************************************** CP217320
                    *                                                   CP217330
                    * THIS SUBROUTINE IS ENTERED DURING RELOAD OF A     CP217340
                    * PHASE WHICH CANNOT BE FOUND IN SLET.              CP217350
                    *                                                   CP217360
                    C0600 DC      0         ENTRY                       CP217370
                    C0610 LDD  L  PAIR1     ESTABLISH CORRECT PH PAIR   CP217380
                          AND   3 BY972-X   REMOVE BITS 0 AND 1         CP217390
                          S     3 BY911-X   COMPARE TO BRACKET PH ID    CP217400
                          BSC  L  C0680,-Z  BRANCH IF PH ID SMALLER     CP217410
                          RTE     16        EXCHANGE ACCUM & EXTENSION  CP217420
                          AND   3 BY972-X   REMOVE BITS 0 AND 1         CP217430
                          S     3 BY911-X   COMPARE PH ID               CP217440
                          BSC  L  C0690,+Z  BRANCH IF PH ID GREATER     CP217450
                    *                                                   CP217460
                    * NEXT SECTION PROCESSES PH ID THAT IS IN BOUNDS    CP217470
                    * OF PHID RECORD PAIR POINTED TO IN 'CO610+1'.      CP217480
                    *                                                   CP217490
                          LDD  I  C0610+1   TEST FOR NEG BYPASS INDI-   CP217500
                          BSC  L  C0615,-Z  *CATION IN PAIR, BR IF OFF  CP217510
                          RTE     16        ELSE USE 2ND PH ID OF PAIR  CP217520
                          AND   3 BY972-X   *TO SET A PH ID TO GO TO.   CP217530
                          A     3 BX901-X   SET AN ID ONE GREATER       CP217540
                          STO   3 BY926-X   *THAN                       CP217550
                          STO   3 BY925-X   *LAST PH ID OF              CP217560
                          SRA     16        *CURRENT PAIR               CP217570
                          BSC  L  BE560     BR TO FLUSH RECORDS         CP217580
                    C0615 LD    3 BZ912-X   IF 'BETWEEN ADDED PROG' SW  CP217590
                          BSC  L  C0655,Z   *IS ON, BR TO SET SOME SWS  CP217600
                          LD   I  C0610+1   RELOAD ADDR FROM POINTER    CP217610
                    *                                                   CP217620
                          S     3 BY911-X   TEST FOR 1ST PH OF PROGRAM  CP217630
                          BSC  L  C0710,Z   BRANCH IF NOT               CP217640
                    C0622 LD      C0610+1   TEST FOR 8TH PAIR AND UP    CP217650
                          S     3 BZ905-X                               CP217660
                          BSC  L  C0700,+Z  BR IF NOT PAIR 8 OR HIGHER  CP217670
                          LD    3 BY911-X   COMPARE NEW PROG WITH LAST  CP217680
                          S     3 BY924-X   *ID IN SLET                 CP217690
                          BSC  L  C0695,+   BR IF NEW PHASE NOT LARGER  CP217700
                    *                                                   CP217710
                          LD    3 BZ912-X   TEST AND BR IF 'BETWEEN     CP217720
                          BSC  L  C0060,Z   *ADDED PROG' SW IS ON       CP217730
                          LD    3 BY966-X   PLACE NOP INST TO ALLOW     CP217740
                          STO  L  C0000     *XR2 TO BE SET              CP217750
                          LD    3 BY911-X                               CP217760
                          STO   3 BZ908-X   SET SW TO ADD PROG AT END   CP217770
                          STO   3 BY912-X                               CP217780
                    *                       STATEMENT REMOVED       2-4 CP217790
                          LD   L  FLETI     TEST FOR AND BR TO ERROR    CP217800
                          BSI  L  ER020,Z   *IF FIXED AREA PRESENT      CP217810
                          LD    3 BY924-X                               CP217820
                          BSI  L  SSLET     LOCATE LAST EXISTING PHASE  CP217830
                          BSI  L  ER027,+-  DEFECTIVE SLET IF MISSING   CP217840
                          MDX   2 4         POINT TO 1ST EMPTY SET      CP217850
                          STX   2 C0650     TEMPORARY STORAGE           CP217860
                          LD      C0650     TEST IF BEYOND LAST ENTRY   CP217870
                          S     3 BY914-X   *IN SLET SECTOR             CP217880
                          BSC  L  C0640,+   BR IF SPACE EXISTS          CP217890
                          LD   L  BUFR3+1   ELSE TEST FOR LAST SLET     CP217900
                          S     3 BY956-X   *SECTOR                     CP217910
                          BSI  L  ER028,-   DISPLAY SLET FULL IF YES    CP217920
                          BSI  L  BD200     FETCH NEXT SLET SECTOR      CP217930
                          STX   2 C0650     SAVE RLTV LOC IN SLET SCTR  CP217940
                    C0640 NOP               A WAIT MAY BE PATCHED HERE  CP217950
                          LD   L  BUFR3+1   SAVE PRESENT SLET SECTOR    CP217960
                          STO     C0950     *ADDRESS                    CP217970
                    *                                                   CP217980
                          LD   L  LET00     'FROM' ADDR IS START    2-4 CP217990
                          STO   3 BY917-X   *OF LET TABLE           2-4 CP218000
                          LD    3 BY958-X   'TO' ADDR IS SUCH THAT  2-4 CP218010
                          S    L  SHIFT     *LET/UA IS SHIFTED UP   2-4 CP218020
                          STO   3 BY918-X   *AGAINST SYSTEM LOADER  2-4 CP218030
                          LD   L  SHIFT     NUMBER OF SECTORS TO    2-4 CP218040
                          STO   3 BY978-X   *SHIFT IS LENGTH LET/UA 2-4 CP218050
                          BSI  L  C0300     SHIFT UA TOWARD END DISK    CP218060
                          LD      C0950     SAVED SLET SCTR ADDR        CP218070
                          STO  L  BUFR3+1   FETCH CURRENT               CP218080
                          BSI  L  FTCH3     *SLET SECTOR                CP218090
                          LDX  I2 C0650     RESTORE RLTV LOC POINTER    CP218100
                          BSC  L  C0060     BR TO PROCESS PHASE         CP218110
                    *                                                   CP218120
                    * CONSTANTS AND WORK AREAS                          CP218130
                    *                                                   CP218140
                    C0650 DC      *-*       TEMPORARY XR2 STORAGE       CP218150
                    *                                                   CP218160
                    C0655 MDX  L  C0610+1,2 ADVANCE POINTER NEXT PAIR   CP218170
                          LD   I  C0610+1   LOAD 1ST PH ID OF NEW PAIR  CP218180
                          S     3 BX901-X   SET UP AN ID ONE LESS TO    CP218190
                          STO   3 BZ913-X   *USE IN SEQUENCE TEST       CP218200
                          MDX     C0622     BR TO MAKE MORE CHECKS      CP218210
                    *                                                   CP218220
                    C0660 BSC  L  C0670,+Z  OUT OF SEQ OR MISSING PHASE CP218230
                          LD    3 BY912-X   RESET FOR RETRY             CP218240
                          STO   3 BY911-X   PREVIOUS PHASE ID           CP218250
                          BSI  L  ER024     PHASE MISSING FROM PROGRAM  CP218260
                          BSI  L  BYPAS     BR TO BYPASS THIS PHASE     CP218270
                    *                                                   CP218280
                    C0670 LD    3 BY912-X   RESET FOR RETRY             CP218290
                          STO   3 BY911-X                               CP218300
                          BSI  L  ER023     PH NO. OUT OF SEQUENCE      CP218310
                    *                                                   CP218320
                    C0680 LDD  I  C0610+1   CHECK PHASE ID PAIR         CP218330
                          BSI  L  ER025,Z   PH ID NOT IN PHID RECORD    CP218340
                    *                                                   CP218350
                    C0690 MDX  L  C0610+1,2 POINT TO NEXT PAIR          CP218360
                          LD      C0610+1                               CP218370
                          S     3 BZ904-X   TEST FOR PAIR NUMBER        CP218380
                          BSI  L  ER025,Z-  ERROR IF BEYOND LAST PAIR   CP218390
                          MDX     C0610     TEST IF PH ID IN THIS PAIR  CP218400
                    *                                                   CP218410
                    C0695 LDD  I  C0610+1   FETCH CURRENT PROG ID PAIR  CP218420
                    C0700 RTE     16        *OTHERWISE SET SW TO PASS   CP218430
                          A     3 BX901-X   *PHASES UNTIL               CP218440
                          STO   3 BY926-X   *NEXT PAIR                  CP218450
                          STO   3 BY925-X   *FROM PHASE ID RCD REACHED  CP218460
                          BSI  L  ER029     DISPLAY 'PROGRAM NOT FOUND' CP218470
                    *                                                   CP218480
                    C0710 LD    3 BZ908-X   THE SW FOR ADDING A PROG    CP218490
                          BSC  L  C0670,Z   *SHOULD BE OFF, ELSE ERROR  CP218500
                          BSI  L  C0100     FORCE A HOLE IN SLET        CP218510
                    *                                                   CP218520
                    * UPON RETURN SYSTEM WILL BE EXPANDED INTO CUSHION  CP218530
                    * TO MAKE ROOM FOR ADDED PHASE                      CP218540
                    *                                                   CP218550
                          LD    1 11        FETCH ID OF PHASE TO ADD    CP218560
                          BSI  L  SSLET     SEARCH FOR DUMMY SLET ENTRY CP218570
                          BSC  I  C0600     RETURN WITH SLET SET IN TOW CP218580
                    *                                                   CP218590
                    *************************************************** CP218600
                    *                                                   CP218610
                    * REMOVE A RELOAD TABLE ENTRY IF PRESENT            CP218620
                    * IN DISK RELOAD TABLE                              CP218630
                    *                                                   CP218640
                    C0720 DC      0         ENTRY/RETURN ADDRESS        CP218650
                          LD   L  BUFR2+1                               CP218660
                          STO     C0970     SAVE SECTOR ADDRESS         CP218670
                          LD    3 BX906-X                               CP218680
                          STO  L  BUFR2+1                               CP218690
                          BSI  L  FTCH2     READ RELOAD TBL INTO BUFR2  CP218700
                          STX   2 C0760+1   SAVE XR2                    CP218710
                          LDX  L2 BUFR2+2   ADDR FIRST DATA WORD        CP218720
                    C0730 LD    2 0         PHASE ID FROM RELOAD TABLE  CP218730
                          S     1 11        NEW PHASE ID                CP218740
                          BSC  L  C0740,+-  FOUND THE TABLE ENTRY       CP218750
                          LD    2 0         STOP SEARCH IF END OF       CP218760
                          EOR   3 BY965-X   *RELOAD TABLE               CP218770
                          BSC  L  C0760,+-  BR TO RETURN                CP218780
                          MDX   2 3         GO TRY NEXT 3 WORD ENTRY    CP218790
                          MDX     C0730     *IN RELOAD TABLE            CP218800
                    C0740 LD    2 3         SHIFT REST OF RELOAD TABLE  CP218810
                          STO   2 0         *3 PLACES LEFT              CP218820
                          EOR   3 BY965-X   IS IT /FFFF                 CP218830
                          BSC  L  C0750,+-  YES, END OF TABLE           CP218840
                          LD    2 4         THIS 3 WORD SHIFT REMOVES   CP218850
                          STO   2 1         *THE FOUND ENTRY FROM THE   CP218860
                          LD    2 5         *TABLE                      CP218870
                          STO   2 2                                     CP218880
                          MDX   2 3                                     CP218890
                          MDX     C0740     LOOP UNTIL END OF TABLE     CP218900
                    C0750 SRA     16        MAKE SURE 3 WORDS AFTER     CP218910
                          STO   2 1         */FFFF ARE ZEROES           CP218920
                          STO   2 2                                     CP218930
                          STO   2 3                                     CP218940
                    *                                                   CP218950
                    * NOTE.. CHECKSUM HAS NOT BEEN ALTERED              CP218960
                    *                                                   CP218970
                          BSI  L  WRIT2     WRITE BACK RELOAD TABLE     CP218980
                    C0760 LDX  L2 *-*       RESTORE XR2                 CP218990
                          LD      C0970     RETRIEVE AND RESTORE        CP219000
                          STO  L  BUFR2+1   *SECTOR ADDRESS             CP219010
                          LD    1 11        RELOAD PHASE ID             CP219020
                          BSC  I  C0720     RETURN                      CP219030
                    *                                                   CP219040
                    * CONSTANTS AND WORK AREAS                          CP219050
                    *                                                   CP219060
                    C0970 DC      *-*       SAVED SECTOR ADDRESS        CP219070
                    *                                                   CP219080
                          BSS     C0000+2*@SCNT-*-1  PATCH AREA         CP219090
                    *                                                   CP219100
                    C0999 EQU     *-1       END OF OVERLAY 0            CP219110
                          HDNG    BUFFER AREAS                          CP219120
                    *                                                   CP219130
                    * RELOAD TABLE BUFFER                               CP219140
                    *                                                   CP219150
                          ORG     @MNCR-3*@SCNT-8                       CP219160
                    BUFR1 DC      1         WORD COUNT                  CP219170
                          DC      @RLTB     SECTOR ADDRESS              CP219180
                    *                                                   CP219190
                    * SECTOR BUFFER                                     CP219200
                    *                                                   CP219210
                          ORG     @MNCR-2*@SCNT-6                       CP219220
                    BUFR2 DC      @SCNT     WORD COUNT                  CP219230
                          DC      /0F0F     SECTOR ADDRESS              CP219240
                    *                                               2-4 CP219250
                    * PERFORM 3 FUNCTIONS AT START OF PHASE 2.      2-4 CP219260
                    * THEN THIS IS OVERLAID BY DISK BUFFER.         2-4 CP219270
                    *                                               2-4 CP219280
                    B0000 DC      0         ENTRY/RETURN ADDRESS        CP219290
                    *                                               2-4 CP219300
                    * CLEAR SIGN BITS FROM SCTR ADDRS IN SLET TABLE 2-4 CP219310
                    *                                               2-4 CP219320
                          LD    3 BY955-X   FETCH AND                   CP219330
                          STO  L  BUFR3+1   *STORE 1ST SLET SCTR ADDR   CP219340
                    C3620 BSI  L  FTCH3     BR TO FETCH A SLET SECTOR   CP219350
                          LDX  I2 BY913     XR2 POINTS TO 1ST SLET WD   CP219360
                    C3640 LD    2 3         FETCH THE SECTOR ADDRESS    CP219370
                          AND   3 BY974-X   MASK OUT SIGN BIT           CP219380
                          STO   2 3         RESTORE SECTOR ADDRESS      CP219390
                          STX   2 C3960     PLACE POINTER               CP219400
                          LD      C3960     *IN ACCUMULATOR             CP219410
                          S     3 BY914-X   TEST FOR AND BR             CP219420
                          BSC  L  C3660,+-  *IF SECTOR COMPLETED        CP219430
                          MDX   2 4         INCREMENT FOR NEXT SET      CP219440
                          MDX     C3640     BRANCH FOR NEXT SET         CP219450
                    C3660 BSI  L  WRIT3     BRANCH TO WRITE SLET SCTR   CP219460
                          LD   L  BUFR3+1   FETCH SLET SCTOR ADDRESS    CP219470
                          S     3 BY956-X   TEST FOR AND BR             CP219480
                          BSC  L  B0010,-   *IF LAST SLET SECTOR    2-4 CP219490
                          MDX  L  BUFR3+1,1 INCREMENT SLET SCTR ADDRESS CP219500
                          MDX     C3620     BR TO FETCH NEXT SLET SCTR  CP219510
                    C3960 DC      *-*       TEMPORARY XR2 STORAGE       CP219520
                    *                                                   CP219530
                    * LOCATE HIGHEST PHASE ID IN SLET AND SAVE FOR      CP219540
                    * RELOAD PROCESSING.  THIS SUBR EXECUTED ONCE ONLY. CP219550
                    * THEN OVERLAID BY DISK BUFFER.                     CP219560
                    *                                                   CP219570
                    B0010 LD    3 BY955-X   FETCH AND STORE SLET        CP219580
                          STO  L  BUFR3+1   *SCTR ADDR IN I/O BUFFER    CP219590
                    B0020 BSI  L  FTCH3     BR TO FETCH A SLET SECTOR   CP219600
                          LDX  L2 -320      WORDS PER SECTOR            CP219610
                    B0040 LD   L2 BUFR3+322 FETCH PHASE ID FROM SLET    CP219620
                          BSC  L  C0800,+-  BR IF END OF SLET       2-4 CP219630
                          LD   L2 BUFR3+322 FETCH AND                   CP219640
                          STO   3 BY924-X   *STORE LARGEST PHASE ID     CP219650
                          LD   L2 BUFR3+324 WD COUNT OF LAST PHASE      CP219660
                          A     3 BX913-X   ADD WORDS/SECTOR - 1    2-4 CP219670
                          SRT     16        SHIFT INTO EXTENSION        CP219680
                          D     3 BX914-X   DIVIDE BY WORDS PER SECTOR  CP219690
                          A    L2 BUFR3+325 ADD BEGINNING SCTR ADDR     CP219700
                          STO   3 BY910-X   FIRST AVIALABLE SECTOR  2-4 CP219710
                          S     3 BX901-X   SUB 1                   2-4 CP219720
                          STO   3 BY937-X   LAST SECTOR USED        2-4 CP219730
                          MDX   2 4                                     CP219740
                          MDX     B0040     LOOP WITHIN SECTOR          CP219750
                          MDX  L  BUFR3+1,1 INCREMENT SLET SCTR ADDRESS CP219760
                          LD   L  BUFR3+1   CHECK IF THREE SLET         CP219770
                          S     3 BY955-X   *SECTORS HAVE BEEN          CP219780
                          S     3 BY922-X   *PROCESSED                  CP219790
                          BSC  L  B0020,+Z  IF NOT FETCH ANOTHER SECTOR CP219800
                    *                                               2-4 CP219810
                    * DURING START OF RELOAD, FIND EXISTING GAPS    2-4 CP219820
                    * BETWEEN SYSTEM PROGRAMS AND SHIFT THEM OUT    2-4 CP219830
                    *                                               2-4 CP219840
                    C0800 LD    3 BY955-X   SET SCTR ADDR TO FIRST  2-4 CP219850
                          STO  L  BUFR3+1   *SLET SECTOR            2-4 CP219860
                          LD    3 BX908-X   INITIALIZE EXPECTED SCTR2-4 CP219870
                          STO   3 BY915-X   *ADDR OF FIRST PHASE= 8 2-4 CP219880
                    C0810 LD   L  BUFR3+1   TEST FOR END OF SLET    2-4 CP219890
                          S     3 BY956-X   *TABLE SECTORS          2-4 CP219900
                          BSC  I  B0000,-Z  RETURN IF END OF SLET   2-4 CP219910
                          BSI  L  FTCH3     FETCH NEST SLET SECTOR  2-4 CP219920
                          LDX  I2 BY913     POINT XR2 TO FIRST ENTRY2-4 CP219930
                    C0820 LD    2 0         RETURN IF END OF SLET   2-4 CP219940
                          BSC  I  B0000,+-  *TABLE ENTRIES          2-4 CP219950
                    * TEST FOR AND SKIP 5 PRINCIPAL I/O SLET ENTRIES2-4 CP219960
                          S       C0841     COMPARE WITH PRINT PH ID2-4 CP219970
                          BSC  L  C0822,Z+  BR IF .LT. PRINT        2-4 CP219980
                          S     3 BX905-X   COMPARE WITH PRINT+5    2-4 CP219990
                          BSC  L  C0825,Z+  BR IF PRINCIPAL I/O     2-4 CP220000
                    C0822 LD    2 3         GET SCTR ADDR OF PHASE  2-4 CP220010
                          STO   3 BY917-X   SAVE AS 'FROM' ADDR     2-4 CP220020
                          S     3 BY915-X   TEST FOR GAP            2-4 CP220030
                          BSC  L  C0830,Z   BR IF GAP FOUND         2-4 CP220040
                          LD    2 2         CALCULATE NUMBER        2-4 CP220050
                          A     3 BX913-X   *OF SECTORS             2-4 CP220060
                          SRT     16        *IN THIS PHASE          2-4 CP220070
                          D     3 BX914-X   *FROM WORD COUNT        2-4 CP220080
                          STO   3 BY947-X   SAVE SCTR CNT THIS PHASE2-4 CP220090
                          LD    2 3         GET SCTR ADDR OF PHASE  2-4 CP220100
                          A     3 BY947-X   ADD LENGTH OF THIS PHASE2-4 CP220110
                          STO   3 BY915-X   *TO GET ADDR NEXT PHASE 2-4 CP220120
                    C0825 MDX   2 4         ADVANCE SLET POINTER    2-4 CP220130
                          STX   2 C0840     IS XR2 POINTER          2-4 CP220140
                          LD      C0840     *BEYOND ADDR LAST SLET  2-4 CP220150
                          S     3 BY914-X   *ENTRY IN SECTOR        2-4 CP220160
                          BSC  L  C0820,+   BR IF MORE IN SECTOR    2-4 CP220170
                          MDX  L0 BUFR3+1,1 INCR SLET SCTR ADDR BY 12-4 CP220180
                          MDX     C0810     BR TO READ NEXT SECTOR  2-4 CP220190
                    *                                               2-4 CP220200
                    C0830 LD    3 BY915-X   SET 'TO' ADDR TO ADDR   2-4 CP220210
                          STO   3 BY918-X   *OF GAP                 2-4 CP220220
                          BSI  L  BD099     GO TO UPDATE/SHIFT SUBR 2-4 CP220230
                          MDX     C0800     LOOP UNTIL NO GAPS LEFT 2-4 CP220240
                    C0840 DC      *-*       TEMP STORAGE FOR XR2    2-4 CP220250
                    C0841 DC      PRINT     PH ID OF PRINCIPAL PRINT2-4 CP220260
                    *                                                   CP220270
                          ORG     B0000+@SCNT  SECTOR BUFFER            CP220280
                    B2END DC      /0F0F     INDICATES END OF BUFFER     CP220290
                          DC      *-*       AVAILABLE                   CP220300
                    *                                                   CP220310
                    * SLET BUFFER                                       CP220320
                    *                                                   CP220330
                          ORG     @MNCR-@SCNT-2                         CP220340
                    BUFR3 DC      @SCNT     WORD COUNT                  CP220350
                          DC      @SLET     SECTOR ADDRESS              CP220360
*SBRK    XX         *SYS LDR - PHASE 2 - OVERLAY 1                      CP220370
                          HDNG    OVERLAY 1 - INITIAL LOAD              CP220380
                          ORG     OVLAY     BEGIN IN OVERLAY AREA       CP220390
                    *                                                   CP220400
                    *                                                   CP220410
                          DC      C1999-C1000+2  WORD CNT OF OVERLAY 1  CP220420
                          DC      @P2AD+5        SCTR ADDR OF OVERLAY 1 CP220430
                    *                                                   CP220440
                    * TAPE TO BE LOADED MUST INCLUDE ALL PHASES         CP220450
                    * INDICATED ON PHID RECORD.                         CP220460
                    *                                                   CP220470
                          DC      1         INDICATES OVERLAY 1         CP220480
                    C1000 NOP               A WAIT MAY BE PATCHED HERE  CP220490
                    *                                                   CP220500
                    * THE ADDR IN NEXT INST HAS BEEN MODIFIED SO THAT   CP220510
                    * XR2 WILL POINT TO LOC OF THIS PH IN SLET SECTOR   CP220520
                    *                                                   CP220530
                    C1020 LDX  L2 BUFR3+2   MODIFIABLE ADDRESS          CP220540
                          LD    1 11        OBTAIN PHASE ID             CP220550
                          S     3 BY924-X   IF LAST PH OF LAST PAIR     CP220560
                          BSC     +-        *IS PROCESSED, SWITCH       CP220570
                          STO   3 BY931-X   *WILL BE SET ON, TO ZERO    CP220580
                          LD    1 11        PHASE ID NUMBER             CP220590
                          S     3 BY909-X   COMPARE WITH 1ST PROG PH    CP220600
                    *                                                   CP220610
                    * FOLLOWING 'NOP' MAY BE MODIFIED TO 'MDX    C1050' CP220620
                    *                                                   CP220630
                    C1030 NOP               BR TO NEXT INST OR 'C1050'  CP220640
                          BSC  L  C1050,+-  BR IF STARTING PH PRESENT   CP220650
                          LD    3 BY909-X   IF 1ST PH ID OF PAIR IS     CP220660
                          BSC  L  C1064,-   *NEG, MATCH NOT REQUIRED    CP220670
                    C1050 LD    3 BY970-X   CANCEL FIRST PHASE TRAP     CP220680
                          STO     C1030     SETUP SKIP TO 'C1050'       CP220690
                          BSI     C1200     TO TEST PH ID LIMITS        CP220700
                    *                                                   CP220710
                          LD   L  BUFR3+1   IF CURRENT SLET SECTOR      CP220720
                          S     3 BY956-X   *ADDR .GT. END SLET ADDR    CP220730
                          BSI  L  ER028,-Z  *BR TO ERROR E 28 SLET FULL CP220740
                          BSI  L  UPCAD     SET UP CORE ADDR IN SLET    CP220750
                          BSC  L  BE200     RETURN FROM OVERLAY 1       CP220760
                    *                                                   CP220770
                    C1064 LD    1 11        IF THE CURRENT PH IS LESS   CP220780
                          S     3 BY909-X   *THAN 1ST PH OF NEXT PROG,  CP220790
                          BSI  L  ER025,+Z  *IT MUST BE EXTRANEOUS      CP220800
                          BSI     C1070     *OTHERWISE A PH IS MISSING  CP220810
                    *                                                   CP220820
                    *************************************************** CP220830
                    *                                                   CP220840
                    * IF LAST PH ID PHID RCRD HAS BEEN STORED DISPLAY   CP220850
                    * PH ID NOT IN PHID RECORD AND SET SW TO PASS ALL   CP220860
                    * ADDITIONAL PHASES.                                CP220870
                    *                                                   CP220880
                    C1070 DC      0         ENTRY                       CP220890
                          LD    3 BY931-X   TEST FOR LAST PH STORED     CP220900
                          BSC  L  C1080,Z   BR IF NOT                   CP220910
                          LD    3 BX901-X                               CP220920
                          SLA     14        SET VERY LARGE NUMBER IN    CP220930
                          STO   3 BY926-X   *SWITCHES                   CP220940
                          STO   3 BY925-X   *TO BYPASS                  CP220950
                          STO   3 BZ911-X   *SUCCEEDING PHASES          CP220960
                          BSI  L  ER025     PH ID NOT IN PHID RCD       CP220970
                    *                                                   CP220980
                    C1080 LD    3 BY912-X   RESET PH ID NUMBERS         CP220990
                          STO   3 BY911-X   *TO PREVIOUS VALUE          CP221000
                          BSI  L  ER024     BR TO PRINT MESSAGE E 14    CP221010
                          BSI  L  BYPAS     BR TO BYPASS THIS PHASE     CP221020
                    *                                                   CP221030
                    *************************************************** CP221040
                    *                                                   CP221050
                    * DETERMINE IF PHASE ID IS WITHIN ONE OF THE        CP221060
                    * RANGES SPECIFIED ON PHID RECORD.                  CP221070
                    *                                                   CP221080
                    C1200 DC      0         WHEN INDR IS NEG, THIS PAIR CP221090
                          LD    3 BY905-X   *OF LIMITS IS COMPLETED     CP221100
                          BSC  L  C1220,-                               CP221110
                    C1210 MDX  L  C1220+1,2 MODIFY ADDR TO NEXT PAIR    CP221120
                          LD   I  C1220+1   TEST FOR BLANK PHID PAIR    CP221130
                          BSC  L  C1210,+-  ADVANCE BY 2 IF BLANK       CP221140
                          LD    3 BY967-X   FETCH A SKIP INSTRUCTION    CP221150
                          STO     C1230-1   RESET MDX TO 7000           CP221160
                    C1220 LDD  L  PAIR1     MODIFIABLE LOAD ADDRESS     CP221170
                          STD   3 BY904-X   STORE THE CURRENT ID PAIR   CP221180
                          MDX     C1230     GO TO 'C1230' OR C1290'     CP221190
                    *                                                   CP221200
                    C1230 MDX  L  C1230-1,C1290-C1230 MODIFY BRANCH     CP221210
                          AND   3 BY972-X   MASK BITS 0 AND 1           CP221220
                          S     3 BY911-X   DOES PH ID MATCH 'BY904'    CP221230
                          BSC  L  C1270,+-  BRANCH IF YES               CP221240
                    *                                                   CP221250
                    * A PHASE OF A SYSTEM PROGRAM (ASM AND/OR FOR)      CP221260
                    * MAY BE BYPASSED.                                  CP221270
                    *                                                   CP221280
                    C1240 LD   L  PAIR1+1   LD 2ND ENTRY OF 1ST PAIR    CP221290
                          AND   3 BY972-X   MASK BITS 0 AND 1           CP221300
                          S     3 BY911-X   ESTABLISH WHICH PAIR THIS   CP221310
                          BSC  L  C1260,-   PH IS IN.  BR IF WITHIN     CP221320
                          LD    3 BY904-X   IF THIS PAIR IS TO BE       CP221330
                          BSI  L  C1070,-   *BYPASSED, PREPARE TO       CP221340
                          MDX  L  C1240+1,2 LOOK FOR ID IN NEXT PAIR    CP221350
                          MDX  L  C1260+1,2 POINT TO NEXT PAIR          CP221360
                          MDX  L  C1310+1,2 POINT TO NEXT PAIR          CP221370
                          STO     C1920     SET SEQ CHECK SW NEG.       CP221380
                          MDX     C1210     BR TO ESTABLISH BOUNDS      CP221390
                    *                                                   CP221400
                    C1260 LD   L  PAIR1     TRY THIS PAIR               CP221410
                          AND   3 BY972-X   MASK OUT BITS 0 AND 1       CP221420
                          S     3 BY911-X   COMPARE CURRENT PHASE ID    CP221430
                          BSI  L  ER025,-Z  BR IF ID NOT IN PHID RECORD CP221440
                          LD   I  C1260+1   TEST IF 'BY904' IS NEG      CP221450
                          BSI  L  C1070,-   MISSING PHASE IF NOT        CP221460
                          BSI  L  BYPAS     BR TO BYPASS THIS PHASE     CP221470
                    *                                                   CP221480
                    C1270 LD    3 BY904-X   COMPARE THE HIGH AND LOW ID CP221490
                          S     3 BY905-X   IF ONLY 1 PH IN PH ID PAIR  CP221500
                          BSC  L  C1360,Z   *DO NOT BRANCH              CP221510
                          LD      *-1       SET SEQ CHECK SW ON         CP221520
                          STO     C1920                                 CP221530
                    C1290 LD    3 BY905-X   COMPARE CURRENT PH WITH     CP221540
                          S     3 BY911-X   *HIGH ID OF PAIR            CP221550
                          BSC  L  C1320,Z   BR IF DIFFERENT             CP221560
                    C1300 LD      *-1       SET 'BY905' NEGATIVE        CP221570
                          STO   3 BY905-X   *TO INDICATE PAIR COMPLETED CP221580
                          STO     C1920     SET SEQ CHECK SW ON         CP221590
                          LD    3 BY966-X   REINSTATE 1ST PH TEST       CP221600
                          STO  L  C1030     *FOR NEXT PAIR              CP221610
                    C1310 LD   L  PAIR2     RESET TO BEGINNING PH ID    CP221620
                          STO   3 BY909-X   *OF NEXT PAIR               CP221630
                          MDX  L  C1310+1,2 ADVANCE THE POINTER         CP221640
                          MDX     C1360     TEST FOR BYPASS             CP221650
                    *                                                   CP221660
                    C1320 BSI  L  C1380,Z+  BR IF EXTRANEOUS PHASE      CP221670
                          LD      C1920     NO SEQUENCE CHECK IF        CP221680
                          BSC  L  C1350,+Z  *INDICATOR IS NEGATIVE      CP221690
                          LD    3 BY911-X   TEST FOR SEQUENCE WITH      CP221700
                          S     3 BY912-X   *PREVIOUS PHASE ID UNLESS   CP221710
                          S     3 BX901-X   *THIS PHASE FOLLOWS A       CP221720
                          BSC  L  C1350,+-  *BYPASSED PROGRAM           CP221730
                          BSI  L  C1380,+Z  DISPLAY PH OUT OF SEQ       CP221740
                          BSI  L  C1070     DISPLAY PHASE MISSING       CP221750
                    C1350 SRA     16        RE-INSTATE SEQ CHECK        CP221760
                          STO     C1920     *FOR CONSECUTIVE PHASES     CP221770
                          LD    3 BY911-X   COMPARE CURRENT PH ID WITH  CP221780
                          S     3 BY905-X   *THE NON-NEG UPPER ID.  IF  CP221790
                          BSC  L  C1300,+-  *NOT =, CONTINUE UNTIL =.   CP221800
                    C1360 LD    3 BY904-X                               CP221810
                          BSI  L  BYPAS,+Z  BR TO BYPASS THIS PHASE     CP221820
                          BSC  I  C1200     RETURN                      CP221830
                    *                                                   CP221840
                    C1380 DC      0         ENTRY                       CP221850
                          LD    3 BY912-X   RESET PHASE ID              CP221860
                          STO   3 BY911-X   *TO PREVIOUS PHASE ID       CP221870
                          BSI  L  ER023     BRANCH TO PRINT MSG E 23    CP221880
                    *                                                   CP221890
                    * CONSTANTS AND WORK AREAS                          CP221900
                    *                                                   CP221910
                    C1920 DC      *-*       SEQ CHECK INDICATOR         CP221920
                    *                                                   CP221930
                    *************************************************** CP221940
                    *                                                   CP221950
                    * ENTERED ON INITIAL LOADS FROM SCTR BK PROCESSING  CP221960
                    *                                                   CP221970
                    * ON INITIAL LOAD SETS THE 5 PHASE IDS FOR THE      CP221980
                    * PRINCIPAL I/O SUBROUTINES AFTER THE DISKN ID.     CP221990
                    *                                                   CP222000
                    C1400 DC      0         ENTRY/RETURN ADDR TO SUBR   CP222010
                          LD   L  PAIR6+1   TEST IF DISKN WAS LAST      CP222020
                          S     3 BY911-X   *PHASE ID TO BE STORED      CP222030
                          BSC  L  C1460,Z   IF NOT DISKN, RETURN        CP222040
                          LD    3 BX905-X   SET COUNTER TO 5 I/O PHA    CP222050
                          STO     C1942     *TO HAVE PRINC IO PH ID SET CP222060
                    C1440 STX   2 C1941     TEST IF SLET SECTOR FULL    CP222070
                          LD      C1941                                 CP222080
                          S     3 BY914-X   LAST VALID SLET SET ADDRESS CP222090
                          BSC  L  C1450,+   BR IF SECTOR NOT FULL       CP222100
                    *                       WRITE SLET SECTOR TO DISK   CP222110
                          BSI  L  BD200     *AND BR TO READ NEXT SECTOR CP222120
                    C1450 LD      C1943     STORE PRINCIPLE I/O PHASE   CP222130
                          STO   2 0         *ID IN SLET SET             CP222140
                          MDX   2 4         INCR POINTER ONE SLET SET   CP222150
                          MDX  L  C1943,1   INCR I/O PHASE ID           CP222160
                          MDX  L  C1942,-1  DECR AND TEST LOOP COUNTER  CP222170
                          MDX     C1440     LOOP FOR 5 I/O PHASE IDS    CP222180
                          STX  L2 C1020+1   SAVE ADDR OF NEXT SLET SET  CP222190
                          LD   L  C1020+1                               CP222200
                          S     3 BY914-X   LAST VALID SLET SET ADDRESS CP222210
                          BSC  L  C1460,+   BR IF SECTOR NOT FULL       CP222220
                          STX  L0 BY920     SET SLET SCTR FULL INDC ON  CP222230
                    C1460 BSC  I  C1400     RETURN                      CP222240
                    *                                                   CP222250
                    * WORK AREAS                                        CP222260
                    *                                                   CP222270
                    C1941 DC      *-*       WORD TO HOLD XR2 FOR CALC   CP222280
                    C1942 DC      *-*       LOOP COUNTER                CP222290
                    C1943 DC      153       PRINCIPLE I/O PH IDS        CP222300
                    *                                                   CP222310
                    *                                                   CP222320
                          BSS     C1000+2*@SCNT-*-1  PATCH AREA         CP222330
                    *                                                   CP222340
                    *                                                   CP222350
                    C1999 EQU     *-1       END OF OVERLAY 1            CP222360
*SBRK    XX         *SYS LDR - PHASE 2 - OVERLAY 2                      CP222370
                          HDNG    OVERLAY 2 - INITLZ RES MON/IMAGE,DCOM CP222380
                    *                                                   CP222390
                    * DCOM RELATIVE EQUATES                             CP222400
                    *                                                   CP222410
                    #SYSC EQU     8         SYS/NON-SYS CART INDR       CP222420
                    #RP67 EQU     17        1442 MODEL 6 OR 7 INDICATOR CP222430
                    #PIOD EQU     25        PRINCIPLE I/O DEVICE INDR   CP222440
                    #PPTR EQU     26        PRINCIPLE PRINT DEVICE INDR CP222450
                    #CIAD EQU     27        RLTV LOC OF CIL SCTR ADDR   CP222460
                    #ANDU EQU     35        ADJUSTED END OF USER AREA   CP222470
                    #BNDU EQU     40        BASE END OF USER AREA       CP222480
                    #FPAD EQU     45        FILE PROTECT ADDRESS        CP222490
                    *                                                   CP222500
                    #CIDN EQU     55        CARTRIDGE ID                CP222510
                    #CIBA EQU     60        SECTOR ADDRESS OF CIB       CP222520
                    #SCRA EQU     65        SECTOR ADDRESS OF SCRA      CP222530
                    #ULET EQU     80        SECTOR ADDRESS OF LET       CP222540
                    #CSHN EQU     90        CUSHION SECTOR COUNT        CP222550
                    *                                                   CP222560
                    * OTHER EQUATES                                     CP222570
                    *                                                   CP222580
                    Y     EQU     80        MIDDLE OF RESIDENT IM/MON   CP222590
                    @DKIP EQU     DZ000+5   DISKZ INTERRUPT ENTRY POINT CP222600
                    *                                                   CP222610
                    *                                                   CP222620
                          ORG     OVLAY     BEGIN IN OVERLAY AREA       CP222630
                    *                                                   CP222640
                    *                                                   CP222650
                          DC      C2999-C2000+2  WORD CNT OF OVERLAY 2  CP222660
                          DC      @P2AD+5+2+3    SCTR ADDR OF OVERLAY 2 CP222670
                    *                                                   CP222680
                    *                                                   CP222690
                          DC      2         INDICATES OVERLAY 2         CP222700
                    C2000 NOP               A WAIT MAY BE PATCHED HERE  CP222710
                    *                                                   CP222720
                    * FILL IN RESERVED SLET AREA WITH PRINCIPAL PRINT,  CP222730
                    * I/O AND CONVERSION ENTRIES.                       CP222740
                    *                                                   CP222750
                    C2010 LD   I  C2900     FETCH ADDR OF PHASE ID ENT  CP222760
                          STO     C2020+1   *INDIRECTLY, SET BY REQ CRD CP222770
                          BSC  L  C2040,+-  WHEN COMPLETE BR TO CONT    CP222780
                    C2020 LD   L  *-*       FETCH I/O SUBR PHASE ID     CP222790
                          BSI  L  SSLET     BRANCH TO SEARCH SLET       CP222800
                          BSI  L  ER032,+-  BR IF NOT IN SLET TABLE     CP222810
                          LD    2 1         SAVE LAST                   CP222820
                          STO     C2903     *3 WORDS                    CP222830
                          LDD   2 2         *OF SLET                    CP222840
                          STD     C2904     *ENTRY                      CP222850
                          MDX  L  C2900,1   INCR POINTER TO PH ID TABLE CP222860
                    C2030 LD   I  C2900     PH ID OF PRINCIPAL IO ENTRY CP222870
                          MDX  L  C2900,1   INCR POINTER TO PH ID TABLE CP222880
                          BSC  L  C2010,+-  IF 0 NO MORE STORES THIS ID CP222890
                          BSI  L  SSLET     FIND ID TO RECEIVE ENTRY    CP222900
                          BSI  L  ER032,+-  BR IF NO PLACE RESERVED     CP222910
                          LD      C2903     INSERT PREVIOUSLY           CP222920
                          STO   2 1         *SAVED 3 WORDS IN           CP222930
                          LDD     C2904     *PRINCIPLE I/O              CP222940
                          STD   2 2         *SLET ENTRY                 CP222950
                          BSI  L  WRIT3     BRANCH TO WRITE SLET SECTOR CP222960
                          MDX     C2030     BRANCH TO STORE ENTRY TWICE CP222970
                    *                                                   CP222980
                    * CONSTANTS AND WORK AREAS                          CP222990
                    *                                                   CP223000
                    C2900 DC      C2901     POINTER TO PH ID TABLE      CP223010
                    C2901 DC      PTRID     PH ID OF PRINC PRINT SUBR   CP223020
                          DC      PRINT     POINT TO ID OF PRINC PRINT  CP223030
                          DC      0         TO INDICATE END OF STORES   CP223040
                          DC      RDRID     POINT TO ID OF PRINC I/O    CP223050
                          DC      PINPT     PH ID OF PRINC INPUT SUBR   CP223060
                          DC      PIDEV     PRINC INPUT EXCLUDING KEYBD CP223070
                          DC      0         TO INDICATE END OF STORES   CP223080
                          DC      CNVID     POINT TO ID OF PRINC CONV   CP223090
                          DC      CNVRT     PRINC SYS CONVERSION SUBR   CP223100
                          DC      CVRT      PRINC CONV EXCLUDING KEYBRD CP223110
                          DC      0         TO INDICATE END OF STORES   CP223120
                          DC      0         TO INDICATE END OF TABLE    CP223130
                    C2903 DC      *-*       AREA TO SAVE LAST 3 WORDS   CP223140
                          BSS  E            FORCE NEXT LOC TO BE EVEN   CP223150
                    C2904 DC      *-*       *OF A SLET                  CP223160
                          DC      *-*       *ENTRY                      CP223170
                    *                                                   CP223180
                    * CANCEL RECORDS NO LONGER PERMITTED                CP223190
                    *                                                   CP223200
                    C2040 STX  L0 W2902     CANCEL HIGH SCTR ADDR CHECK CP223210
                          LDX  L1 BC020+1   XR1 PTS TO RCRD TYPE BRANCH CP223220
                          LD   L  C2906     FETCH ADDR OF ERR SUBR E 21 CP223230
                          STO   1 0         SECTOR                      CP223240
                          STO   1 2*3       *BREAK,                     CP223250
                          STO   1 3*3       *'F' AND                    CP223260
                          STO   1 4*3       *'81' RECORDS NOT PERMITTED CP223270
                    *                                                   CP223280
                    * FETCH CORE IMAGE LOADER SLET FOR DISKZ.           CP223290
                    *                                                   CP223300
                          LD   L  PAIR6+2   FETCH PHASE ID OF CIL       CP223310
                          BSI  L  SSLET     BRANCH TO SEARCH SLET       CP223320
                          BSI  L  ER027,+-  BRANCH IF CIL MISSING       CP223330
                          LDD   2 2         SAVE WORD COUNT             CP223340
                          STD  L  CILWC     *AND SECTOR ADDRESS OF CIL  CP223350
                    *                                                   CP223360
                    * UPDATE SINGLE ENTRIES IN DCOM                     CP223370
                    * (DCOM IS READ INTO 'BUFR3')                       CP223380
                    *                                                   CP223390
                          BSI  L  C2400     BRANCH TO READ DCOM         CP223400
                          LDX  L2 BUFR3+2   XR2 PTS TO 1ST WORD OF DCOM CP223410
                          LD   L  T1442     STORE                       CP223420
                          STO   2 #RP67     *1442-6 OR 7 INDICATOR      CP223430
                          LDD  L  PRNRD     STORE                       CP223440
                          STO   2 #PIOD     *PRINCIPLE I/O              CP223450
                          RTE     16        *AND PRINCIPAL PRINT        CP223460
                          STO   2 #PPTR     *INDICATORS                 CP223470
                          LD   L  VERSN     SAVE VERSION                CP223480
                          STO   2 #SYSC     *AND MODIFICATION NUMBER    CP223490
                          STO  L  $SYSC      *IN RESIDENT MONITOR   2-4 CP223500
                          LD    3 BY951-X   STORE RLTV LOC OF CIL SCTR  CP223510
                          STO   2 #CIAD     *ADDR IN @IDAD FOR DUP-DEF  CP223520
                    *                                                   CP223530
                    * INITIALIZE RESIDENT IMAGE AND RESIDENT MONITOR.   CP223540
                    * (RESIDENT IMAGE IS READ INTO 'BUFR2')             CP223550
                    *                                                   CP223560
                          LD    3 BY954-X   STORE SCTR ADDR OF RESIDENT CP223570
                          STO  L  BUFR2+1   *IMAGE IN BUFFER I/O AREA   CP223580
                          BSI  L  FTCH2     BRANCH TO READ RES IMAGE    CP223590
                          LDX  L2 BUFR2+2-6 XR2 PTS TO PSEUDO WORD 0    CP223600
                    *                                                   CP223610
                          LD    3 BY976-X   FETCH 70FF                  CP223620
                          STO  L  /0000     *INSTR AND STORE TO LOC 0   CP223630
                          LD   L  CHN12     SAVE CHANNEL 12 INDICATOR   CP223640
                          STO   2 $CH12     *IN RESIDENT IMAGE          CP223650
                          STO  L  $CH12     *AND RESIDENT MONITOR       CP223660
                          LD   L  SCORE     SAVE CORE SIZE              CP223670
                          STO   2 $CORE     *IN RESIDENT IMAGE          CP223680
                          STO  L  $CORE     *AND RESIDENT MONITOR       CP223690
                          LD   L  C2908     SAVE END OF DISKZ ADDRESS   CP223700
                          STO   2 $CILA     *IN RESIDENT IMAGE          CP223710
                          STO  L  $CILA     *AND RESIDENT MONITOR       CP223720
                    *                                                   CP223730
                          STX   2 C2060     SAVE XR2                    CP223740
                          BSI  L  C2100     BRANCH TO INITIALIZE DCOM   CP223750
                          LDX  L2 *-*       RESTORE                     CP223760
                    C2060 EQU     *-1       *XR2                        CP223770
                    *                                                   CP223780
                          LD    3 BY965-X   SAVE DISK SUBR INDICATOR    CP223790
                          STO   2 $DREQ     *IN RESIDENT IMAGE          CP223800
                          STO  L  $DREQ     *AND RESIDENT MONITOR,      CP223810
                          STO   2 $DZIN     *IN RESIDENT IMAGE          CP223820
                          STO  L  $DZIN     *AND RESIDENT MONITOR       CP223830
                          LD    3 BZ903-X   SAVE LET SECTOR ADDRESS     CP223840
                          STO   2 $ULET     *IN RESIDENT IMAGE          CP223850
                          STO  L  $ULET     *AND RESIDENT MONITOR       CP223860
                          MDX   2 Y         XR2 PTS TO 2ND HALF OF RES  CP223870
                          LD   L  DCYL1     SAVE                        CP223880
                          STO   2 $DCYL+0-Y *DEFECTIVE                  CP223890
                          LD   L  DCYL2     *CYLINDERS                  CP223900
                          STO   2 $DCYL+1-Y *ADDRESSES                  CP223910
                          LD   L  DCYL3     *IN                         CP223920
                          STO   2 $DCYL+2-Y *RESIDENT IMAGE             CP223930
                          LD   L  FPADR     SAVE FILE PROTECT ADDRESS   CP223940
                          STO   2 $FPAD-Y   *IN RESIDENT IMAGE          CP223950
                          STO  L  $FPAD     *AND RESIDENT MONITOR       CP223960
                          LD      C2909     SAVE DISKZ INTRPT ENTRY PT  CP223970
                          STO   2 $IBT2-Y   *IN RESIDENT IMAGE          CP223980
                          LD      C2907     SAVE DUMP ENTRY POINT       CP223990
                          STO   2 $IREQ-Y   *IN RESIDENT IMAGE          CP224000
                          LD   L  CIBFR     FETCH SECTOR ADDRESS OF CIB CP224010
                          SRT     16        SHIFT TO EXTENSION          CP224020
                          LD    3 BY963-X   FETCH WORD COUNT OF CIB     CP224030
                          STD  L  $CIBA-1   *AND STORE TO LOWER CORE    CP224040
                    *                                                   CP224050
                          BSI  L  WRIT2     BRANCH TO WRITE RES IMAGE   CP224060
                    *                                                   CP224070
                          LD    3 BY961-X   FETCH SECTOR ADDRESS OF,    CP224080
                          LDX  L2 C3000     *ENTRY POINT TO             CP224090
                          BSI  L  BH000     *AND BR TO FETCH OVERLAY 3  CP224100
                    *                                                   CP224110
                    * CONSTANTS AND WORK AREAS                          CP224120
                    *                                                   CP224130
                    C2906 DC      ER021     ADDRESS OF ERROR SUBR E 21  CP224140
                    C2907 DC      $DUMP     DUMP ENTRY POINT            CP224150
                    C2908 DC      $ZEND-4   END OF DISKZ - 4            CP224160
                    C2909 DC      @DKIP     DISKZ INTERRUPT ENTRY POINT CP224170
                    *                                                   CP224180
                    *************************************************** CP224190
                    *                                                   CP224200
                    * INITIALIZE OR UPDATE THE CARTRIDGE                CP224210
                    * DEPENDENT TABLES IN DCOM.                         CP224220
                    *                                                   CP224230
                    C2100 DC      0         ENTRY/RETURN ADDRESS        CP224240
                          LDX  L2 BUFR3+2   XR2 PTS TO 1ST WORD OF DCOM CP224250
                    *                                                   CP224260
                          LDX   1 -5        SET CARTRIDGE ID COUNT      CP224270
                    C2120 LD   L  CARID     FETCH CARTRIDGE ID          CP224280
                          EOR   2 #CIDN     TEST FOR AND BRANCH IF      CP224290
                          BSC  L  C2140,+-  *MATCH FOUND IN DCOM        CP224300
                          MDX   2 1         POINT TO NEXT DCOM WORD     CP224310
                          MDX   1 1         INCR ID COUNT, SKIP IF ZERO CP224320
                          MDX     C2120     BRANCH TO TEST NEXT CART ID CP224330
                          BSI  L  ER005     NO MATCH, BR TO PRINT MSG   CP224340
                    *             ER005                                 CP224350
                    C2140 LD    3 BY934-X   TEST FOR AND BRANCH         CP224360
                          BSC  L  C2180,Z+  *IF AN INITIAL LOAD         CP224370
                          MDX  L  BZ908,0   TEST FOR AND BRANCH         CP224380
                          MDX     C2160     *IF SYSTEM PROGRAMS ADDED   CP224390
                          LD    2 #ULET     FETCH AND SAVE              CP224400
                          STO   3 BZ903-X   *ADDRESS OF USER AREA       CP224410
                          LD   L  CAREA     STORE NEW CUSHION SIZE      CP224420
                          STO   2 #CSHN     *TO DCOM                    CP224430
                          BSI  L  WRIT3     BRANCH TO WRITE DCOM        CP224440
                          MDX     C2190     BRANCH TO RETURN            CP224450
                    *                                                   CP224460
                    * UPDATE DCOM ON SYS PROGRAMS ADDED RELOAD          CP224470
                    *                                                   CP224480
                    C2160 LD    3 BY937-X   FETCH LAST SAD OF SYS PROGS CP224490
                          A     3 BX908-X   POINT                       CP224500
                          SRA     3         *TO NEXT                    CP224510
                          SLA     3         *CYLINDER                   CP224520
                          STO     C2912     SAVE TEMPORARLY             CP224530
                          S     3 BY937-X   CALCULATE                   CP224540
                          A     3 BX908-X   *NUMBER OF SECTORS          CP224550
                          S     3 BX901-X   *IN CUSHION AREA            CP224560
                          STO   2 #CSHN     *AND STORE TO DCOM          CP224570
                          LD      C2912     CALCULATE DIFFERENCE        CP224580
                          A     3 BX908-X   *BETWEEN OLD SCRA SCTR ADDR CP224590
                          S     2 #SCRA     *AND NEW SCRA SCTR ADDRESS  CP224600
                          STO     C2912     *AND SAVE                   CP224610
                          LD    2 #SCRA     UPDATE                      CP224620
                          A       C2912     *ADDRESS OF                 CP224630
                          STO   2 #SCRA     *SCRA IN DCOM               CP224640
                          STO  L  ASCRA     *AND COMM AREA              CP224650
                          LD    2 #CIBA     UPDATE                      CP224660
                          A       C2912     *ADDRESS OF                 CP224670
                          STO   2 #CIBA     *CIB IN DCOM                CP224680
                          STO  L  CIBFR     *AND COMM AREA              CP224690
                          LD    2 #ULET     UPDATE                      CP224700
                          A       C2912     *ADDRESS OF                 CP224710
                          STO   2 #ULET     *USER AREA IN DCOM          CP224720
                          NOP               A WAIT MAY BE PATCHED HERE  CP224730
                          STO   3 BZ903-X   *AND COMM AREA              CP224740
                          LD    2 #FPAD     UPDATE                      CP224750
                          A       C2912     *FILE PROTECT               CP224760
                          STO   2 #FPAD     *ADDRESS IN DCOM            CP224770
                          STO  L  FPADR     *AND COMM AREA              CP224780
                          LD    2 #ANDU     UPDATE                      CP224790
                          RTE     4         *ADJUSTED                   CP224800
                          A       C2912     *END OF                     CP224810
                          SLT     4         *USER AREA                  CP224820
                          STO   2 #ANDU     *IN DCOM                    CP224830
                          LD    2 #BNDU     UPDATE                      CP224840
                          RTE     4         *BASE                       CP224850
                          A       C2912     *END OF                     CP224860
                          SLT     4         *USER AREA                  CP224870
                          STO   2 #BNDU     *IN DCOM                    CP224880
                          BSI  L  WRIT3     BRANCH TO WRITE DCOM        CP224890
                          BSI  L  C2200     BR TO UPDATE LET CHAIN ADDR CP224900
                          MDX     C2190     BRANCH TO RETURN            CP224910
                    *                                                   CP224920
                    * INITIALIZE DCOM ON AN INITIAL LOAD                CP224930
                    *                                                   CP224940
                    C2180 LD    3 BY937-X   FETCH LAST SAD OF SYS PROGS CP224950
                          A     3 BX908-X   POINT                       CP224960
                          SRA     3         *TO NEXT                    CP224970
                          SLA     3         *CYLINDER                   CP224980
                          STO     C2912     SAVE TEMPORARLY             CP224990
                          S     3 BY937-X   CALCULATE                   CP225000
                          A     3 BX908-X   *NUMBER OF                  CP225010
                          S     3 BX901-X   *SECTORS IN CUSHION AREA    CP225020
                          STO   2 #CSHN     *AND SAVE IN DCOM           CP225030
                          LD      C2912     CALCULATE                   CP225040
                          A     3 BX908-X   *ADDRESS OF SCRA            CP225050
                          STO   2 #SCRA     *AND SAVE IN DCOM           CP225060
                          STO  L  ASCRA     *AND COMM AREA              CP225070
                          A     3 BX908-X   CALCULATE                   CP225080
                          STO   2 #CIBA     *ADDRESS OF CIB AND STORE   CP225090
                          STO  L  CIBFR     *ST IN DCOM AND COMM AREA   CP225100
                          A     3 BX909-X   CALCULATE                   CP225110
                          STO   2 #ULET     *ADDRESS OF USER AREA AND   CP225120
                          STO   3 BZ903-X   *ST IN DCOM AND COMM AREA   CP225130
                          A     3 BX908-X   CALCULATE                   CP225140
                          STO   2 #FPAD     *FILE PROTECT ADDRESS AND   CP225150
                          STO  L  FPADR     *ST IN DCOM AND COMM AREA   CP225160
                          SLA     4         CALCULATE AND STORE IN DCOM CP225170
                          STO   2 #ANDU     *ADJUSTED END UF USER AREA  CP225180
                          STO   2 #BNDU     *AND BASE END OF USER AREA  CP225190
                          BSI  L  WRIT3     BRANCH TO WRITE DCOM        CP225200
                    *                                                   CP225210
                    * INITIALIZE LET SECTOR ON AN INITIAL LOAD.         CP225220
                    * (LET IS BUILT IN 'BUFR3')                         CP225230
                    *                                                   CP225240
                          BSI     C2300     BRANCH TO CLEAR BUFFER      CP225250
                          LDX  L2 BUFR3+2   XR2 PTS TO 1ST WORD OF LET  CP225260
                          LD    3 BZ903-X   FETCH NEW 1ST LET SCTR ADDR CP225270
                          STO   2 -1        *AND STORE TO LET I/O BFR   CP225280
                          LD      C2910     FETCH AND STORE NUMBER OF   CP225290
                          STO   2 3         *WORDS AVAILABLE IN SECTOR  CP225300
                          LD      C2911     FETCH                       CP225310
                          STO   2 5         *AND STORE                  CP225320
                          LD      C2911+1   *DUMMY                      CP225330
                          STO   2 6         *ENTRY NAME                 CP225340
                          LD    3 BY962-X   CALCULATE                   CP225350
                          S    L  FPADR     *AND STORE                  CP225360
                          A     3 BX901-X   *SIZE OF                    CP225370
                          SLA     4         *WORKING STORAGE            CP225380
                          STO   2 7         *IN DISK BYTES              CP225390
                          LD   L  FPADR     FETCH AND STORE             CP225400
                          STO   2 1         *SECTOR ADDRESS OF UA       CP225410
                          BSI  L  WRIT3     BRANCH TO WRITE LET SECTOR  CP225420
                    *                                                   CP225430
                    C2190 BSC  I  C2100     RETURN                      CP225440
                    *                                                   CP225450
                    * CONSTANTS AND WORK AREAS                          CP225460
                    *                                                   CP225470
                    C2910 DC      @SCNT-8   NO. OF WORDS AVAILABLE      CP225480
                    C2911 DC      /7112     '1DUMY' IN                  CP225490
                          DC      /4528     *TRUNCATED EBCDIC NAME CODE CP225500
                    C2912 DC      *-*       TEMPORARY STORAGE           CP225510
                    *                                                   CP225520
                    *************************************************** CP225530
                    *                                                   CP225540
                    * IF LET AND UA HAVE BEEN RELOCATED THE UA ADDRESS  CP225550
                    * (WD 2 IN EACH LET SCTR) AND THE CHAIN ADDR (WD 5  CP225560
                    * IN EACH LET SCTR) MUST BE ADJUSTED.               CP225570
                    *                                                   CP225580
                    C2200 DC      0         ENTRY/RETURN ADDRESS        CP225590
                          LD    3 BZ908-X   TEST FOR AND RETURN         CP225600
                          BSC  I  C2200,+-  *IF NO NEW PROGRAMS ADDED   CP225610
                          LD    3 BZ903-X   FETCH NEW LET SCTR ADDRESS  CP225620
                          S    L  LET00     TEST FOR AND RETURN IF      CP225630
                          BSC  I  C2200,+-  *SAME AS OLD LET SCTR ADDR  CP225640
                          STO   3 BZ906-X   SAVE THE DIFFERENCE         CP225650
                          LDX  L1 BUFR3+2   XR1 POINT TO FIRST LET WORD CP225660
                          LD    3 BZ903-X   FETCH AND STORE             CP225670
                          STO   1 -1        *NEW LET SECTOR ADDRESS     CP225680
                    C2240 BSI  L  FTCH3     BRANCH TO READ A LET SECTOR CP225690
                    *                                                   CP225700
                    * WHEN CHAIN ADDRESS IS 0 THIS IS LAST LET SCTR     CP225710
                    *                                                   CP225720
                          LD    1 1         FETCH,                      CP225730
                          A     3 BZ906-X   *INCREMENT AND STORE        CP225740
                          STO   1 1         *SECTOR ADDRESS OF UA       CP225750
                          LD    1 4         FETCH THE CHAIN ADDRESS     CP225760
                          BSC  L  C2280,+-  BRANCH IF ZERO              CP225770
                          A     3 BZ906-X   INCREMENT AND               CP225780
                          STO   1 4         *STORE THE CHAIN ADDRESS    CP225790
                          BSI  L  WRIT3     BR TO WRITE THE LET SECTOR  CP225800
                          MDX  L  BUFR3+1,1 INCR ADDR FOR NEXT LET SCTR CP225810
                          MDX     C2240     BR TO READ NEXT LET SECTOR  CP225820
                    *                                                   CP225830
                    C2280 LD    3 BX913-X   GET CONSTANT HEX 13F    2-5 CP225831
                          S     1 3         SUB NO. WDS AVAILABLE   2-5 CP225832
                          STO     C2282+1   SET 2ND WD OF LD INSTR  2-5 CP225833
                          STO     C2284+1   SET 2ND WD OF STO INSTR 2-5 CP225834
                    C2282 LD   L1 *-*       LOAD 1DUMY DB COUNT     2-5 CP225835
                          SRT     4         SECTOR COUNT IN ACC     2-5 CP225836
                          S     3 BZ906-X   SUBTRACT DIFFERENCE     2-5 CP225837
                          SLT     4         GET NEW DB COUNT IN ACC 2-5 CP225838
                    C2284 STO  L1 *-*       STORE BACK IN LET SCTR  2-5 CP225839
                          BSI  L  WRIT3     BR TO WR LAST LET SCTR      CP225840
                          BSC  I  C2200     RETURN                      CP225850
                    *                                                   CP225860
                    *                                                   CP225870
                    *************************************************** CP225880
                    *                                                   CP225890
                    * CLEAR 'BUFR3' TO BUILD LET INTO.                  CP225900
                    *                                                   CP225910
                    C2300 DC      0         ENTRY/RETURN ADDRESS        CP225920
                          LDX  L1 @SCNT     XR1 = BUFFER WORD COUNT     CP225930
                          SRA     16        FETCH ZERO                  CP225940
                    C2340 STO  L1 BUFR3+1   STORE TO BUFFER             CP225950
                          MDX   1 -1        DECR WORD CNT, SKIP IF ZERO CP225960
                          MDX     C2340     BRANCH TO CLEAR NEXT WORD   CP225970
                          BSC  I  C2300     RETURN                      CP225980
                    *                                                   CP225990
                    *************************************************** CP226000
                    *                                                   CP226010
                    * READ DCOM INTO 'BUFR3'                            CP226020
                    *                                                   CP226030
                    C2400 DC      0         ENTRY/RETURN ADDRESS        CP226040
                          LD    3 BY952-X   FETCH SECTOR ADDR OF DCOM   CP226050
                          STO  L  BUFR3+1   *AND STORE TO I/O BFR AREA  CP226060
                          BSI  L  FTCH3     BRANCH TO READ DCOM         CP226070
                          BSC  I  C2400     RETURN                      CP226080
                    *                                                   CP226090
                    *                                                   CP226100
                          BSS     C2000+2*@SCNT-*-1  PATCH AREA         CP226110
                    *                                                   CP226120
                    *                                                   CP226130
                    C2999 EQU     *-1       END OF OVERLAY 2            CP226140
*SBRK    XX         *SYS LDR - PHASE 2 - OVERLAY 3                      CP226150
                          HDNG    OVERLAY 3 - SET UP RELOAD TABLE       CP226160
                          ORG     OVLAY     BEGIN IN OVERLAY AREA       CP226170
                    *                                                   CP226180
                    *                                                   CP226190
                          DC      C3999-C3000+2  WORD CNT OF OVERLAY 3  CP226200
                          DC      @P2AD+5+2+2+3  SCTR ADDR OF OVERLAY 3 CP226210
                    *                                                   CP226220
                    *                                                   CP226230
                          DC      3         INDICATES OVERLAY 3         CP226240
                    C3000 NOP               A WAIT MAY BE PATCHED HERE  CP226250
                    *                       STATEMENT REMOVED       2-4 CP226260
                          BSI  L  C3800+1   TEST FOR DEVICES NOT HERE   CP226270
                          LD    3 BY965-X   SET FIRST WORD FOLLOWING    CP226280
                          STO  I  BE120+1   *RELOAD TABLE TO /FFFF      CP226290
                          LD    3 BY934-X   IF RELOAD, BR TO UNITE      CP226300
                          BSI  L  C3400,-   *CORE & DISK RELOAD TABLES  CP226310
                          BSI  L  WRIT1     UPDATE RELOAD TABLE ON DISK CP226320
                          BSI  L  CK500     BR TO CALCULATE CHECKSUM    CP226330
                          STO  L  BUFR1+1+@SCNT STORE IN RELOAD TABLE   CP226340
                          BSI  L  WRIT1     UPDATE RELOAD TABLE ON DISK CP226350
                          BSI  L  RLTBL     PROCESS RELOAD TBL IN CORE  CP226360
                    *                                                   CP226370
                    * SET THE WORD COUNT AND SECTOR ADDRESS AT THE      CP226380
                    * END OF DISKZ ON THE COLD START SECTOR.            CP226390
                    *                                                   CP226400
                          LDD  L  CILWC     FETCH AND STORE WORD COUNT  CP226410
                          STD  L  $ZEND-4   *AND SECTOR ADDRESS OF CIL  CP226420
                          LDD     C3902     FETCH AND STORE WD CNT AND  CP226430
                          STD  L  BUFR2     *SCTR ADDR OF COLD START    CP226440
                          BSI  L  FTCH2     BRANCH TO FETCH COLD START  CP226450
                          LDD  L  CILWC     ST WD CNT AND SCTR ADDR OF  CP226460
                          STD  L  BUFR2+$ZEND-@CSTR-4  *CIL TO C.S. END CP226470
                          LDD  L  W2900     FETCH FUNC CODE/I/O AR ADDR CP226480
                          BSI  L  DZ000     BRANCH TO WRITE COLD START  CP226490
                          MDX  L  $DBSY,0   SKIP NEXT IF WRITE COMPLET  CP226500
                          MDX     *-3       BR TO TEST WRITE COMPLETE   CP226510
                          BSI  L  C3200     BR TO CLEAR SYS LDR FR DISK CP226520
                          LDX  L1 $DUMP     SET $IREQ                   CP226530
                          STX  L1 $IREQ     *POINTING TO $DUMP          CP226540
                          LD    3 BY934-X   TEST FOR AND BRANCH TO      CP226550
                          BSC  L  C3900,-   *END RELOAD IF RELOAD MODE  CP226560
                    *                                                   CP226570
                    * CLEAR THE PAGE HEADING BUFFER.                    CP226580
                    *                                                   CP226590
                          LDX  L1 @SCNT     XR1 = SECTOR WORD COUNT     CP226600
                          LD      C3926     FETCH AN EBCDIC BLANK /4040 CP226610
                    C3040 STO  L1 BUFR2+1   STORE TO BUFFER             CP226620
                          MDX   1 -1        DECR WORD CNT, SKIP IF ZERO CP226630
                          MDX     C3040     BRANCH TO STORE NEXT WORD   CP226640
                          LDD     C3924     FETCH AND STORE             CP226650
                          STD  L  BUFR2+2   *'PAGE' TO BUFFER           CP226660
                          LD      C3927     FETCH AND STORE             CP226670
                          STO  L  BUFR2+5   *PAGE COUNT (1) TO BUFFER   CP226680
                          LD    3 BX901-X   INITIALIZE                  CP226690
                          STO  L  $PGCT     *PAGE COUNT IN COMMA        CP226700
                          LD      C3922     FETCH AND STORE             CP226710
                          STO  L  BUFR2+1   *HEADING SECTOR ADDRESS     CP226720
                          BSI  L  WRIT2     BR TO WRITE HEADING SECTOR  CP226730
                    *                                                   CP226740
                    * UPDATE STATUS WORD IN ID SECTOR                   CP226750
                    *                                                   CP226760
                          LD    3 BZ910-X   FETCH AND STORE SECTOR      CP226770
                          STO  L  BUFR2+1   *ADDRESS OF ID SECTOR       CP226780
                          BSI  L  FTCH2     BRANCH TO FETCH ID SECTOR   CP226790
                          LD    3 BX902-X   UPDATE                      CP226800
                          STO  L  BUFR2+2+@STAT  *STATUS (WORD 7 = +2)  CP226810
                          BSI  L  WRIT2     BRANCH TO WRITE ID SECTOR   CP226820
                    *                                                   CP226830
                    * CALL THE AUXILIARY SUPERVISOR TO PLACE A DUMMY    CP226840
                    * 'DUP' RECORD IN THE SUPERVISOR BUFFER.            CP226850
                    *                                                   CP226860
                          NOP               A WAIT MAY BE PATCHED HERE  CP226870
                          SRA     16        CLEAR                       CP226880
                          STO  L  $IOCT     *I/O COUNTER                CP226890
                          BSI  L  $DUMP     BRANCH TO FETCH AUX SUPV    CP226900
                          DC      -5        INDICATES A DUMMY 'DUP'     CP226910
                    *                                                   CP226920
                    * CONSTANTS AND WORK AREAS.                         CP226930
                    *                                                   CP226940
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP226950
                    C3902 DC      320       WORD COUNT                  CP226960
                          DC      @IDAD     SECTOR ADDRESS              CP226970
                    *                                                   CP226980
                    *************************************************** CP226990
                    *                                                   CP227000
                    * CLEAR SYSTEM LOADER FROM DISK.                    CP227010
                    *                                                   CP227020
                    C3200 DC      0         ENTRY/RETURN ADDRESS        CP227030
                          LDX  L1 @SCNT     XR1 = SECTOR WORD COUNT     CP227040
                          SRA     16        CLEAR ACCUMULATOR           CP227050
                    C3220 STO  L1 BUFR2+1   STORE TO BUFFER             CP227060
                          MDX   1 -1        DECR WORD CNT, SKIP IF ZERO CP227070
                          MDX     C3220     BRANCH TO CLEAR NEXT WORD   CP227080
                          LD      C3921     FETCH AND STORE             CP227090
                          STO  L  BUFR2+1   *SECTOR ADDRESS             CP227100
                    C3260 MDX  L  BUFR2+1,1 INCREMENT SECTOR ADDRESS    CP227110
                          BSI  L  WRIT2     BRANCH TO WRITE A SECTOR    CP227120
                          MDX  L  C3920,-1  DECR SCTR CNT, SKIP IF ZERO CP227130
                          MDX     C3260     BRANCH TO WRITE NEXT SECTOR CP227140
                          BSC  I  C3200     RETURN                      CP227150
                    *                                                   CP227160
                    * CONSTANTS AND WORK AREAS.                         CP227170
                    *                                                   CP227180
                    C3920 DC      15        NO. SCTRS OCCUPIED BY PH 2  CP227190
                    C3921 DC      @P2AD-1   SCTR ADDR OF PHASE 2 - 1    CP227200
                    C3922 DC      @HDNG     PAGE HEADING SECTOR         CP227210
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP227220
                    C3924 EBC     .PAGE.                                CP227230
                    C3926 DC      /4040     EBCDIC BLANKS               CP227240
                    C3927 DC      /40F1     EBCDIC FOR BLANK, ONE       CP227250
                    *                                                   CP227260
                    *************************************************** CP227270
                    *                                                   CP227280
                    * COMPARE THE RELOAD TABLE CONSTRUCTED IN CORE TO   CP227290
                    * THE RELOAD TABLE RESIDING ON DISK.                CP227300
                    *                                                   CP227310
                    C3400 DC      0         ENTRY/RETURN ADDRESS        CP227320
                          LD   I  C3940     ADVANCE IN-CORE RELOAD      CP227330
                          EOR   3 BY965-X   *TABLE POINTER TO LOCATION  CP227340
                          BSC  L  C3410,+-  *OF 'FFFF' ENTRY            CP227350
                          MDX  L  C3940,3   PASS GROUP OF 3             CP227360
                          MDX     C3400+1   TEST FOR 'FFFF' AGAIN       CP227370
                    C3410 LD   L  BUFR1+1   PREPARE TO FETCH            CP227380
                          STO  L  BUFR2+1   *RELOAD TABLE               CP227390
                          BSI  L  FTCH2     BR TO FETCH RELOAD TABLE    CP227400
                          LDX  L1 BUFR2+2   XR1 POINTS TO RELOAD TABLE  CP227410
                    C3420 LDX  L2 BUFR1+2   XR2 POINTS TO IN-CORE TABLE CP227420
                          LD    1 0         IF NO MORE ENTRIES ARE IN   CP227430
                          EOR   3 BY965-X   *THE DISK RELOAD TABLE      CP227440
                          BSC  I  C3400,+-  *RETURN                     CP227450
                    C3430 LD    2 0         IF END OF IN-CORE TABLE     CP227460
                          EOR   3 BY965-X   *BEFORE MATE WAS FOUND      CP227470
                          BSC  L  C3450,+-  *BR TO PLACE AT END OF TBL  CP227480
                          LD    1 0         TEST FOR RELOAD PHASE       CP227490
                          S     2 0         *MATE                       CP227500
                          BSC  L  C3440,+-  *BR IF FOUND                CP227510
                          MDX   2 3         ADVANCE 1 SET IN IN-CORE    CP227520
                          MDX     C3430     *TABLE, TRY FOR MATE AGAIN  CP227530
                    *                                                   CP227540
                    * PLACE AN UNMATCHED ENTRY AT END OF IN-CORE TABLE  CP227550
                    *                                                   CP227560
                    C3450 LD   L  BUFR1     FETCH WD CNT IN RELOAD TBL  CP227570
                          S     3 BX914-X   TEST FOR                    CP227580
                          A     3 BX902-X   *AND BRANCH                 CP227590
                          BSI  L  ER130,-   *IF EQ TO/GREATER THAN 318  CP227600
                          LDX  I2 C3940     XR2 = CURRENT POINTER       CP227610
                          LD    1 0         MOVE                        CP227620
                          STO   2 0         *DISK RELOAD TABLE ENTRY    CP227630
                          LD    1 1         *TO END OF                  CP227640
                          STO   2 1         *IN-CORE TABLE              CP227650
                          LD    1 2         *TO BE                      CP227660
                          STO   2 2         *PROCESSED                  CP227670
                          MDX  L  C3940,3   INCR END OF STRING POINTER  CP227680
                          MDX  L  BUFR1,3   INCREMENT STRING WORD COUNT CP227690
                          LD    3 BY965-X   STORE 'FFFF'                CP227700
                          STO  I  C3940     *AT END OF STRING           CP227710
                    *                                                   CP227720
                    C3440 MDX   1 3         STEP THRU RELOAD TABLE FROM CP227730
                          MDX     C3420     *DISK THAT IS NOW IN BUFFER CP227740
                    *                                                   CP227750
                    * CONSTANTS AND WORK AREAS                          CP227760
                    *                                                   CP227770
                    C3940 DC      BUFR1+2   RELOAD TABLE POINTER        CP227780
                    *                                                   CP227790
                    *             STATEMENTS MOVED TO MAINLINE      2-4 CP227800
                    *                                                   CP227810
                    *************************************************** CP227820
                    *                                                   CP227830
                    * DETERMINE WHICH DEVICES WERE NOT INCLUDED IN THE  CP227840
                    * CONFIGURATION DECK.  THE SIGN BIT OF THE SECTOR   CP227850
                    * ADDRESSES OF THE DEVICES NOT PRESENT WILL BE SET  CP227860
                    * ON.  THE SAME WILL BE DONE FOR UNUSED CONVERSION  CP227870
                    * SUBROUTINES.                                      CP227880
                    *                                                   CP227890
                    C3800 BSC  L  *-*       ENTRY/RETURN ADDRESS        CP227900
                    C3810 MDX  L  C3984,-2  DECR LOOP COUNTER           CP227910
                          BSC     +-Z       CONTINUE UNTIL ZERO         CP227920
                          MDX     C3800     RETURN                      CP227930
                    C3820 LDD  L  C3980     FETCH A PAIR OF ENTRIES     CP227940
                          AND  L  DINDR     TEST FOR AND                CP227950
                          BSC  L  C3860,+-  *BRANCH IF BIT IS SET       CP227960
                    C3840 MDX  L  C3820+1,2 INCREMENT THE PAIR POINTER  CP227970
                          MDX     C3810     BRANCH TO TEST NEXT DEVICE  CP227980
                    *                                                   CP227990
                    C3860 RTE     16        ROTATE PHASE ID TO ACC      CP228000
                          BSI  L  SSLET     BRANCH TO SEARCH SLET       CP228010
                          BSI  L  ER032,+-  BR IF PHASE ID MISSING      CP228020
                          LD    3 BY964-X   FETCH SIGN BIT              CP228030
                          OR    2 3         MASK IN PHASE ID            CP228040
                          STO   2 3         *AND RESTORE TO SLET        CP228050
                          BSI  L  WRIT3     BRANCH TO WRITE SLET        CP228060
                          MDX     C3840     BRANCH TO TEST NEXT DEVICE  CP228070
                    *                                                   CP228080
                    * CONSTANTS AND WORK AREAS                          CP228090
                    *                                                   CP228100
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP228110
                    C3980 DC      /0040     MASK FOR BIT 9     ISS 9    CP228120
                          DC      P1403     1403 PRINTER                CP228130
                          DC      /0200     MASK FOR BIT 6     ISS 6    CP228140
                          DC      P1132     1132 PRINTER                CP228150
                          DC      /0800     MASK FOR BIT 4     ISS 4    CP228160
                          DC      I2501     2501 READER                 CP228170
                          DC      /4000     MASK FOR BIT 1     ISS 1    CP228180
                          DC      I1442     1442 READER/PUNCH           CP228190
                          DC      /1000     MASK FOR BIT 3     ISS 3    CP228200
                          DC      I1134     PAPER TAPE READ/PUNCH       CP228210
                          DC      /4800     MASK FOR BITS 1 AND 4       CP228220
                          DC      CDCNV     CARD CONVERSION             CP228230
                    C3982 DC      /1000     MASK FOR BIT 3              CP228240
                          DC      C1134     PAPER TAPE CONVERSION       CP228250
                    *                                                   CP228260
                    C3984 DC      C3982+4-C3980 SIZE OF TABLE + 2       CP228270
                    *                                                   CP228280
                    *************************************************** CP228290
                    *                                                   CP228300
                    * SUBROUTINE TO CALCULATE CHECKSUM OF SLET AND      CP228310
                    * RELOAD TABLE (EXCLUDING LAST WORD OF RELOAD TBL)  CP228320
                    *                                                   CP228330
                    CK500 DC      0         ENTRY/RETURN ADDRESS        CP228340
                          LDD     CK910     SET UP BUFFER TO START      CP228350
                          STD  L  BUFR1     *READING AT 1ST SCTR SLET   CP228360
                          SLA     16        CLEAR CALCULATED            CP228370
                          STO     CK912     *CHECKSUM                   CP228380
                          LDX   1 -4        4 SECTORS TO CHECKSUM       CP228390
                          STX   1 CK914                                 CP228400
                    CK510 MDX  L0 BUFR1+1,1 INCREMENT SECTOR ADDRESS    CP228410
                          BSI  L  FTCH1     READ SECTOR FROM DISK       CP228420
                          LDX  L1 @SCNT-1   WORDS - 1 PER SECTOR        CP228430
                          LD      CK912     RESTORE CHECKSUM TO ACC     CP228440
                    CK520 A    L1 BUFR1+1   CALCULATE CHECKSUM          CP228450
                          BSC     C         IF CARRY                    CP228460
                          A     3 BX901-X   *ADD 1 TO CHECKSUM          CP228470
                          MDX   1 -1        STEP THROUGH SECTOR         CP228480
                          MDX     CK520     BR TO LOOP IN SECTOR        CP228490
                          STO     CK912     SAVE CHECK SUM              CP228500
                          MDX  L0 CK914,1   INCREMENT SECTOR LOOP COUNT CP228510
                          MDX     CK530     BR, IS NOT RELOAD SECTOR    CP228520
                          BSC  I  CK500     RETURN                      CP228530
                    *                                                   CP228540
                    * INCLUDE LAST WORD OF SECTOR IN ALL BUT RELOAD     CP228550
                    * SECTOR                                            CP228560
                    *                                                   CP228570
                    CK530 A    L  BUFR1+1+@SCNT LAST WORD OF SECTOR     CP228580
                          BSC     C         IF CARRY                    CP228590
                          A     3 BX901-X   *ADD 1 TO CHECKSUM          CP228600
                          STO     CK912     SAVE CHECK SUM              CP228610
                          MDX     CK510     BR TO LOOP 4 SECTORS        CP228620
                    *                                                   CP228630
                    * CONSTANTS AND WORK AREAS                          CP228640
                    *                                                   CP228650
                          BSS  E  0                                     CP228660
                    CK910 DC      @SCNT     WORD COUNT 1 SECTOR         CP228670
                          DC      @SLET-1   SECTOR ADDRESS OF SLET - 1  CP228680
                    CK912 DC      *-*       CALCULATED CHECKSUM         CP228690
                    CK914 DC      *-*       SECTOR LOOP COUNTER         CP228700
                    *                                                   CP228710
                    *************************************************** CP228720
                    *                                                   CP228730
                    * READ A SECTOR FROM DISK INTO 'BUFR1'              CP228740
                    *                                                   CP228750
                    FTCH1 DC      0         ENTRY/RETURN ADDRESS        CP228760
                          LDD     F1900     FETCH FUNC CODE, I/O ADDR   CP228770
                          BSI  L  DZ000     BRANCH TO READ A SECTOR     CP228780
                          MDX  L  $DBSY,0   SKIP NEXT IF READ COMPLETE  CP228790
                          MDX     *-3       BR TO TEST READ COMPLETE    CP228800
                          BSC  I  FTCH1     RETURN                      CP228810
                    *                                                   CP228820
                    * CONSTANTS AND WORK AREAS                          CP228830
                    *                                                   CP228840
                          BSS  E  0         FORCE NEXT LOC TO BE EVEN   CP228850
                    F1900 DC      /0000     READ FUNCTION CODE          CP228860
                          DC      BUFR1     ADDRESS OF I/O AREA         CP228870
                    *                                                   CP228880
                    *************************************************** CP228890
                    *                                                   CP228900
                    * PROCESS RELOAD TABLE TO DETERMINE PHASES WHICH    CP228910
                    * REQUIRE SLET TABLE ENTRIES.                       CP228920
                    *                                                   CP228930
                    RLTBL DC      0         ENTRY/RETURN ADDRESS        CP228940
                    RL020 LD   L  BUFR1+2   FETCH A SLET PHASE ID       CP228950
                          EOR   3 BY965-X   TEST FOR AND                CP228960
                          BSC  I  RLTBL,+-  *RETURN IF END OF TABLE     CP228970
                          EOR   3 BY965-X   RESET PHASE ID              CP228980
                          BSI  L  SSLET     BRANCH TO SEARCH SLET       CP228990
                          BSI  L  ER031,+-  BR IF DEF SLET OR RL TBL    CP229000
                          BSC  L  RL150,+-  BR TO SKIP BAD ENTRY        CP229010
                          LDX  I1 RL020+1   XR1 PTS TO POS IN RL TABLE  CP229020
                          LD    1 1         FETCH SECTOR ADDRESS        CP229030
                          SRT     16        DETERMINE WHICH             CP229040
                          D     3 BX914-X   *SECTOR OF THE PHASE        CP229050
                          A     2 3         *TO WHICH THE               CP229060
                          STO  L  BUFR2+1   *CORRECTIONS WILL BE MADE   CP229070
                          RTE     16        FETCH AND STORE RLTV        CP229080
                          STO     RL900     *LOCATION IN SECTOR         CP229090
                          LD    1 2         FETCH AND SAVE              CP229100
                          STO     RL901     *NO. OF SETS TO BE FILLED   CP229110
                          BSI  L  FTCH2     BRANCH TO FETCH THE PHASE   CP229120
                    RL060 LD      RL900     FETCH SET LOCATION          CP229130
                          S     3 BX914-X   TEST FOR AND BRANCH         CP229140
                          BSC  L  RL080,Z+  *IF NOT END OF SECTOR       CP229150
                          BSI  L  WRIT2     BRANCH TO WRITE THE SECTOR  CP229160
                          SRA     16        CLEAR                       CP229170
                          STO     RL900     *SECTOR LOCATION            CP229180
                          MDX  L  BUFR2+1,1 INCREMENT SECTOR ADDRESS    CP229190
                          BSI  L  FTCH2     BR TO FETCH NEXT SECTOR     CP229200
                    RL080 LDX  I1 RL900     RESET XR1 WITH THE REM      CP229210
                          LD   L1 BUFR2+2   FETCH PH ID TO LOOK UP      CP229220
                          BSI  L  SSLET     BRANCH TO SEARCH SLET       CP229230
                          BSC  L  RL120,Z   BRANCH IF PHASE ID FOUND    CP229240
                    *                                                   CP229250
                    * CONTINUE IF THE PHASE REQUESTED IS FIRST PHASE OF CP229260
                    * A PROGRAM THAT HAS BEEN BYPASSED OR VOIDED.  IF   CP229270
                    * YES, INSERT ZEROS IN REMAINDER OF SLET ENTRY IN   CP229280
                    * THE REQUESTING PHASE.                             CP229290
                    *                                                   CP229300
                          LDX   2 5         XR2 = NO. POSS PROGS VOIDED CP229310
                    RL100 LD   I2 RL906-1   FETCH PHASE ID              CP229320
                          AND   3 BY972-X   TEST FOR AND                CP229330
                          EOR  L  SS900     *BRANCH IF A                CP229340
                          BSC  L  RL110,+-  *BYPASSED OR VOIDED PROGRAM CP229350
                          LD   I2 RL908-1   FETCH PHASE ID, TEST    2-5 CP229352
                          AND   3 BY972-X   *FOR AND BRANCH IF LAST 2-5 CP229354
                          EOR  L  SS900     *PHASE OF A BYPASSED OR 2-5 CP229356
                          BSC  L  RL110,+-  *VOIDED PROGRAM         2-5 CP229358
                          MDX   2 -1        DECR PROG CNT, SKIP IF ZERO CP229360
                          MDX     RL100     BRANCH TO TEST NEXT PROGRAM CP229370
                    *                                                   CP229380
                          LD   I  RL020+1   FETCH PH ID OF REQUESTING   CP229390
                          RTE     16        *PHASE AND SHIFT TO EXT     CP229400
                          LD   L  SS900     FETCH MISSING PHASE ID      CP229410
                          BSI  L  ER026     BR TO ERROR E 26, RETURN    CP229420
                    RL110 LDX  L2 RL903-1   XR2 POINTS TO ZEROS         CP229430
                    RL120 MDX   1 1         INCR LOCATION POINTER       CP229440
                          STX   1 RL907     *AND SAVE                   CP229450
                          MDX  L  RL900,1   INCREMENT REMAINDER         CP229460
                          LD      RL907     FETCH LOCATION POINTER      CP229470
                          S     3 BX913-X   TEST FOR AND BRANCH         CP229480
                          BSC  L  RL200+1,-Z  *IF SECTOR COMPLETE       CP229490
                    RL140 LD    2 1         FETCH AN ENTRY              CP229500
                          STO  L1 BUFR2+2   *AND STORE TO BUFFER        CP229510
                          MDX  L  RL900,1   INCREMENT REMAINDER         CP229520
                          NOP               AVOID POSSIBLE SKIP         CP229530
                          MDX  L  RL902,-1  DECR CNTR, SKIP IF ZERO     CP229540
                          MDX     RL180     BRANCH TO STORE NEXT WORD   CP229550
                          LD    3 BX903-X   RESET                       CP229560
                          STO     RL902     *COUNTER                    CP229570
                          MDX  L  RL901,-1  DECR SET CNT, SKIP IF ZERO  CP229580
                          MDX     RL060     BRANCH TO PROCESS NEXT SET  CP229590
                          BSI  L  WRIT2     BRANCH TO WRITE THE SECTOR  CP229600
                    RL150 MDX  L  RL020+1,3 POINT TO NEXT RELOAD ENTRY  CP229610
                          MDX     RL020     BR TO CHECK NEXT ENTRY      CP229620
                    *                                                   CP229630
                    RL160 SRA     16        CLEAR                       CP229640
                          STO     RL900     *REMAINDER                  CP229650
                          MDX  L  BUFR2+1,1 INCREMENT SECTOR ADDRESS    CP229660
                          BSI  L  FTCH2     BRANCH TO FETCH NEXT SECTOR CP229670
                          LDX  I1 RL900     XR1 = NEW REMAINDER         CP229680
                          MDX     RL140     BRANCH TO STORE SLET INFO   CP229690
                    *                                                   CP229700
                    RL180 LD      RL900     FETCH REMAINDER             CP229710
                          S     3 BX914-X   TEST FOR AND BRANCH         CP229720
                          BSC  L  RL200,+-  *IF SECTOR COMPLETED        CP229730
                          MDX   2 1         INCR TO NEXT SLET WORD      CP229740
                          LDX  I1 RL900     POINT TO NEXT BUFFER WORD   CP229750
                          MDX     RL140     BRANCH TO STORE NEXT WORD   CP229760
                    *                                                   CP229770
                    RL200 MDX   2 1         INCR TO NEXT SLET WORD      CP229780
                          BSI  L  WRIT2     BRANCH TO WRITE A SECTOR    CP229790
                          MDX     RL160     BRANCH TO FETCH NEXT SECTOR CP229800
                    *                                                   CP229810
                    * CONSTANTS AND WORK AREAS                          CP229820
                    *                                                   CP229830
                    RL900 DC      *-*       SAVED REMAINDER             CP229840
                    RL901 DC      *-*       SAVED SET CNT TO PATCH      CP229850
                    RL902 DC      3         CTR TO UPDATE 3 WDS/SET     CP229860
                    RL903 DC      0         ZEROES TO PLACE IN A        CP229870
                          DC      0         *REQUESTING PHASE WHEN      CP229880
                          DC      0         *PROG REQUESTED NOT PRESENT CP229890
                    *                                                   CP229900
                    RL906 DC      PAIR2     FIRST ID                    CP229910
                          DC      PAIR3     *OF                         CP229920
                          DC      PAIR8     *PROGRAMS THAT              CP229930
                          DC      PAIR9     *MAY BE                     CP229940
                          DC      PAIRA     *PASSED OR VOIDED           CP229950
                    RL907 DC      *-*       TEMPORARY XR1 STORAGE       CP229960
                    RL908 DC      PAIR2+1   LAST ID                 2-5 CP229962
                          DC      PAIR3+1   *OF                     2-5 CP229963
                          DC      PAIR8+1   *PROGRAMS THAT MAY      2-5 CP229965
                          DC      PAIR9+1   *BE BYPASSED            2-5 CP229967
                          DC      PAIRA+1   *OR VOIDED              2-5 CP229969
                    *                                                   CP229970
                    *************************************************** CP229980
                    *                                                   CP229990
                    * PRINT 'END OF RELOAD' MESSAGE                 2-9 CP230000
                    *                                                   CP230010
                    C3900 LDX  L1 MSGXX+1   POINT TO END MESSAGE        CP230020
                          LDX  I2 MSGXX     FETCH WORD CNT OF END MSG   CP230030
                          BSI  L  CNPTR     BRANCH TO PRINT MESSAGE     CP230040
                          MDX     C3900     REPEAT MESSAGE              CP230050
                    *                                                   CP230060
                    * 'END OF RELOAD' MESSAGE                       2-9 CP230070
                    *                                                   CP230080
                    MSGXX DC      C3998-*   WORD COUNT OF END MESSAGE   CP230090
                          DMES    'REND OF RELOAD 'R'E              2-9 CP230100
                    *                                                   CP230110
                    *                                                   CP230120
                    C3998 BSS     C3000+2*@SCNT-*-1  PATCH AREA         CP230130
                    *                                                   CP230140
                    *                                                   CP230150
                    C3999 EQU     *-1       END OF OVERLAY 3            CP230160
                    *                                                   CP230170
                          END     BA000     END OF PHASE 2              CP230180
