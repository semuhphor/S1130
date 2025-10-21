import React, { useState } from 'react';
import { AssembleResponse } from '../types/api';
import emulatorApi from '../services/apiClient';
import './AssemblerEditor.css';

interface AssemblerEditorProps {
  onAssemblyComplete?: (response: AssembleResponse) => void;
}

const EXAMPLE_PROGRAM = `       ORG  /100
* Animate a bit shifting through ACC and EXT registers
* Shift left by 1 each iteration - watch the bit move!
* When carry occurs, restart from 1
* Use Step to see it move slowly, or Run to animate
*
START  LDD  L ONE
LOOP   SLT  1
       BSC  L LOOP,C
       BSC  L START
*
* Data section
ONE    DC   1
       DC   0
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
