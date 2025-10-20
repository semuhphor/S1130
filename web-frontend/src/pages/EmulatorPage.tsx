import React, { useState } from 'react';
import AssemblerEditor from '../components/AssemblerEditor';
import CPUConsole from '../components/CPUConsole';
import { AssembleResponse } from '../types/api';
import './EmulatorPage.css';

const EmulatorPage: React.FC = () => {
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleAssemblyComplete = (response: AssembleResponse) => {
    if (response.success) {
      // Trigger a refresh of the CPU console to show the updated state
      setRefreshTrigger(prev => prev + 1);
    }
  };

  return (
    <div className="emulator-page">
      <div className="page-header">
        <h1>IBM 1130 Emulator</h1>
        <p>A complete emulation of the 1960s IBM 1130 computer system</p>
      </div>

      <div className="emulator-container">
        <div className="editor-panel">
          <AssemblerEditor onAssemblyComplete={handleAssemblyComplete} />
        </div>

        <div className="console-panel">
          <CPUConsole refreshTrigger={refreshTrigger} />
        </div>
      </div>

      <div className="footer">
        <p>
          IBM 1130 Emulator © 2025 |{' '}
          <a href="https://github.com/semuhphor/S1130" target="_blank" rel="noopener noreferrer">
            View on GitHub
          </a>
        </p>
      </div>
    </div>
  );
};

export default EmulatorPage;
