import React, { useState } from 'react';
import AssemblerEditor from '../components/AssemblerEditor';
import CPUConsole from '../components/CPUConsole';
import AssemblyListing from '../components/AssemblyListing';
import { AssembleResponse, ListingLine } from '../types/api';
import './EmulatorPage.css';

const EmulatorPage: React.FC = () => {
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [listingLines, setListingLines] = useState<ListingLine[]>([]);
  const [currentIar, setCurrentIar] = useState<number | undefined>(undefined);
  const [executionSpeed, setExecutionSpeed] = useState(5); // Default 5 ips

  const handleAssemblyComplete = (response: AssembleResponse) => {
    if (response.success) {
      // Store the listing lines
      setListingLines(response.listingLines);
      // Trigger a refresh of the CPU console to show the updated state
      setRefreshTrigger(prev => prev + 1);
    } else {
      // Clear listing on assembly failure
      setListingLines([]);
    }
  };

  const handleCpuStateUpdate = (iar: number, speed: number) => {
    setCurrentIar(iar);
    setExecutionSpeed(speed);
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
          <CPUConsole 
            refreshTrigger={refreshTrigger} 
            onCpuStateUpdate={handleCpuStateUpdate}
          />
        </div>
      </div>

      {listingLines.length > 0 && (
        <div className="listing-panel">
          <AssemblyListing 
            listingLines={listingLines}
            currentAddress={currentIar}
            executionSpeed={executionSpeed}
          />
        </div>
      )}

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
