// IBM 1130 Shift Left Test Program
// Demonstrates SLT (Shift Left Together) with carry detection
// 
// This program loads 1 into ACC/EXT registers, then repeatedly
// shifts left until carry is set (bit shifts out), then restarts.
// Watch the bit travel through all 32 bits!
//
       ORG  /100
//
// Main program loop
//
START: LDD  |L|ONE       // Load double-word (0,1) into ACC and EXT
LOOP:  SLT  1            // Shift left together 1 bit
       BSC  |L|LOOP,C    // Carry OFF -- keep shifting
       BSC  |L|START     // Carry ON - reload 0,1 into acc/ext
//
// Data section
//
// Define constant 1 as a double-word
       BSS  |E|0         // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
