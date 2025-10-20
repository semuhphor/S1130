import React from 'react';
import { CpuState } from '../types/api';
import './RegisterDisplay.css';

interface RegisterDisplayProps {
  cpuState: CpuState;
}

/**
 * Converts a number to a binary string with leading zeros
 */
const toBinary = (value: number, bits: number): string => {
  return (value >>> 0).toString(2).padStart(bits, '0');
};

/**
 * Converts a number to hex string with leading zeros
 */
const toHex = (value: number, digits: number): string => {
  return value.toString(16).toUpperCase().padStart(digits, '0');
};

/**
 * Renders a 16-bit register as LED-style bits
 */
const BinaryDisplay: React.FC<{ value: number }> = ({ value }) => {
  const binary = toBinary(value, 16);
  
  return (
    <div className="binary-display">
      {binary.split('').map((bit, index) => (
        <div key={index} className={`bit ${bit === '1' ? 'on' : 'off'}`}>
          {bit}
        </div>
      ))}
    </div>
  );
};

/**
 * Displays CPU registers and flags in a retro LED style
 */
const RegisterDisplay: React.FC<RegisterDisplayProps> = ({ cpuState }) => {
  return (
    <div className="register-display">
      <h3>CPU Registers</h3>
      
      {/* IAR - Instruction Address Register */}
      <div className="register-row">
        <div className="register-label">IAR:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.iar, 4)}</div>
          <div className="dec-value">({cpuState.iar})</div>
          <BinaryDisplay value={cpuState.iar} />
        </div>
      </div>

      {/* ACC - Accumulator */}
      <div className="register-row">
        <div className="register-label">ACC:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.acc, 4)}</div>
          <div className="dec-value">({cpuState.acc})</div>
          <BinaryDisplay value={cpuState.acc} />
        </div>
      </div>

      {/* EXT - Extension Register */}
      <div className="register-row">
        <div className="register-label">EXT:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.ext, 4)}</div>
          <div className="dec-value">({cpuState.ext})</div>
          <BinaryDisplay value={cpuState.ext} />
        </div>
      </div>

      {/* XR1 - Index Register 1 */}
      <div className="register-row">
        <div className="register-label">XR1:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.xr1, 4)}</div>
          <div className="dec-value">({cpuState.xr1})</div>
          <BinaryDisplay value={cpuState.xr1} />
        </div>
      </div>

      {/* XR2 - Index Register 2 */}
      <div className="register-row">
        <div className="register-label">XR2:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.xr2, 4)}</div>
          <div className="dec-value">({cpuState.xr2})</div>
          <BinaryDisplay value={cpuState.xr2} />
        </div>
      </div>

      {/* XR3 - Index Register 3 */}
      <div className="register-row">
        <div className="register-label">XR3:</div>
        <div className="register-value">
          <div className="hex-value">0x{toHex(cpuState.xr3, 4)}</div>
          <div className="dec-value">({cpuState.xr3})</div>
          <BinaryDisplay value={cpuState.xr3} />
        </div>
      </div>

      {/* Flags Section */}
      <div className="flags-section">
        <h3>Status Flags</h3>
        <div className="flags-row">
          <div className="flag">
            <div className="flag-label">CARRY:</div>
            <div className={`flag-indicator ${cpuState.carry ? 'active' : 'inactive'}`}>
              {cpuState.carry ? '1' : '0'}
            </div>
          </div>
          <div className="flag">
            <div className="flag-label">OVERFLOW:</div>
            <div className={`flag-indicator ${cpuState.overflow ? 'active' : 'inactive'}`}>
              {cpuState.overflow ? '1' : '0'}
            </div>
          </div>
          <div className="flag">
            <div className="flag-label">WAIT:</div>
            <div className={`flag-indicator ${cpuState.wait ? 'active' : 'inactive'}`}>
              {cpuState.wait ? '1' : '0'}
            </div>
          </div>
        </div>
      </div>

      {/* Info Section */}
      <div className="info-row">
        <div>Instructions: {cpuState.instructionCount.toLocaleString()}</div>
        <div>Interrupt Level: {cpuState.currentInterruptLevel >= 0 ? cpuState.currentInterruptLevel : 'None'}</div>
      </div>
    </div>
  );
};

export default RegisterDisplay;
