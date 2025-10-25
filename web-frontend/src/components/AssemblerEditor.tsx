import React, { useState } from 'react';
import { AssembleResponse } from '../types/api';
import emulatorApi from '../services/apiClient';
import './AssemblerEditor.css';

interface AssemblerEditorProps {
  onAssemblyComplete?: (response: AssembleResponse) => void;
}

const EXAMPLE_PROGRAM = `// IBM 1130 Shift Left Test Program
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
       BSS  |E|          // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
`;

const AssemblerEditor: React.FC<AssemblerEditorProps> = ({ onAssemblyComplete }) => {
  const [sourceCode, setSourceCode] = useState<string>(EXAMPLE_PROGRAM);
  const [assemblyResult, setAssemblyResult] = useState<AssembleResponse | null>(null);
  const [isAssembling, setIsAssembling] = useState(false);

  const handleAssemble = async () => {
    setIsAssembling(true);
    try {
      // Reset the CPU first to ensure clean state
      await emulatorApi.assembler.reset();
      
      // Then assemble and load the program
      const result = await emulatorApi.assembler.assemble({ sourceCode });
      setAssemblyResult(result);
      if (onAssemblyComplete) {
        onAssemblyComplete(result);
      }
    } catch (error) {
      console.error('Assembly failed:', error);
      setAssemblyResult({
        success: false,
        errors: ['Failed to connect to emulator API'],
        wordsLoaded: 0,
        listingLines: [],
      });
    } finally {
      setIsAssembling(false);
    }
  };

  const handleReset = async () => {
    try {
      await emulatorApi.assembler.reset();
      setAssemblyResult(null);
      setSourceCode('');
    } catch (error) {
      console.error('Reset failed:', error);
    }
  };

  const handleLoadExample = () => {
    setSourceCode(EXAMPLE_PROGRAM);
    setAssemblyResult(null);
  };

  return (
    <div className="assembler-editor">
      <h3>IBM 1130 Assembler</h3>
      
      <textarea
        className="editor-textarea"
        value={sourceCode}
        onChange={(e) => setSourceCode(e.target.value)}
        placeholder="Enter IBM 1130 assembler code here..."
        spellCheck={false}
      />

      <div className="editor-controls">
        <button
          className="btn-assemble"
          onClick={handleAssemble}
          disabled={isAssembling || !sourceCode.trim()}
        >
          {isAssembling ? 'Assembling...' : 'Assemble'}
        </button>
        <button
          className="btn-reset"
          onClick={handleReset}
        >
          Reset
        </button>
        <button
          className="btn-load-example"
          onClick={handleLoadExample}
        >
          Load Example
        </button>
      </div>

      {assemblyResult && (
        <div className={`assembly-result ${assemblyResult.success ? 'success' : 'error'}`}>
          {assemblyResult.success ? (
            <>
              <h4>✓ Assembly Successful</h4>
              <p>Loaded at address: 0x{assemblyResult.loadedAddress?.toString(16).toUpperCase().padStart(4, '0')}</p>
              <p>Words loaded: {assemblyResult.wordsLoaded}</p>
            </>
          ) : (
            <>
              <h4>✗ Assembly Failed</h4>
              <ul className="error-list">
                {assemblyResult.errors.map((error, index) => (
                  <li key={index}>{error}</li>
                ))}
              </ul>
            </>
          )}
        </div>
      )}
    </div>
  );
};

export default AssemblerEditor;
