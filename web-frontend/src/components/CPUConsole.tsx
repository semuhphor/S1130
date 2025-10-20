import React, { useEffect, useState } from 'react';
import { ExecutionStatus } from '../types/api';
import emulatorApi from '../services/apiClient';
import RegisterDisplay from './RegisterDisplay';
import ControlPanel from './ControlPanel';
import './CPUConsole.css';

interface CPUConsoleProps {
  refreshTrigger?: number;
}

const CPUConsole: React.FC<CPUConsoleProps> = ({ refreshTrigger }) => {
  const [status, setStatus] = useState<ExecutionStatus | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchStatus = async () => {
    try {
      const newStatus = await emulatorApi.execution.getStatus();
      setStatus(newStatus);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch status:', err);
      setError('Failed to connect to emulator API. Make sure the backend is running on port 5000.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStatus();
  }, [refreshTrigger]);

  // Poll for status updates when running
  useEffect(() => {
    if (status?.isRunning) {
      const interval = setInterval(fetchStatus, 100); // Poll every 100ms when running
      return () => clearInterval(interval);
    }
  }, [status?.isRunning]);

  if (loading) {
    return (
      <div className="cpu-console">
        <div className="console-loading">Loading CPU state...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="cpu-console">
        <div className="console-error">{error}</div>
      </div>
    );
  }

  if (!status) {
    return (
      <div className="cpu-console">
        <div className="console-error">No CPU status available</div>
      </div>
    );
  }

  return (
    <div className="cpu-console">
      <ControlPanel 
        isRunning={status.isRunning} 
        onStatusChange={fetchStatus}
      />
      <RegisterDisplay cpuState={status.cpuState} />
    </div>
  );
};

export default CPUConsole;
