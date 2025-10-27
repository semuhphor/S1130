import React, { useState } from 'react';
import emulatorApi from '../services/apiClient';
import './ControlPanel.css';

interface ControlPanelProps {
  isRunning: boolean;
  onStatusChange?: () => void;
  onSpeedChange?: (speed: number) => void;
}

const ControlPanel: React.FC<ControlPanelProps> = ({ isRunning, onStatusChange, onSpeedChange }) => {
  const [speed, setSpeed] = useState(5); // Instructions per second - default 5 ips
  
  const handleSpeedChange = (newSpeed: number) => {
    setSpeed(newSpeed);
    if (onSpeedChange) {
      onSpeedChange(newSpeed);
    }
  };

  const handleStep = async () => {
    try {
      await emulatorApi.execution.step();
      if (onStatusChange) {
        onStatusChange();
      }
    } catch (error) {
      console.error('Step failed:', error);
    }
  };

  const handleRun = async () => {
    try {
      await emulatorApi.execution.run(speed);
      if (onStatusChange) {
        setTimeout(onStatusChange, 100); // Give it time to start
      }
    } catch (error) {
      console.error('Run failed:', error);
    }
  };

  const handleStop = async () => {
    try {
      await emulatorApi.execution.stop();
      if (onStatusChange) {
        setTimeout(onStatusChange, 100); // Give it time to stop
      }
    } catch (error) {
      console.error('Stop failed:', error);
    }
  };

  const formatSpeed = (ips: number): string => {
    if (ips >= 1000000) {
      return `${(ips / 1000000).toFixed(1)}M IPS`;
    } else if (ips >= 1000) {
      return `${(ips / 1000).toFixed(1)}K IPS`;
    } else {
      return `${ips} IPS`;
    }
  };

  return (
    <div className="control-panel">
      <h3>CPU Control</h3>
      
      <div className="execution-status">
        <div className={`status-indicator ${isRunning ? 'running' : 'stopped'}`} />
        <div className="status-text">
          {isRunning ? 'RUNNING' : 'STOPPED'}
        </div>
      </div>

      <div className="control-buttons">
        <button
          className="btn-step"
          onClick={handleStep}
          disabled={isRunning}
          title="Execute one instruction"
        >
          Step
        </button>
        <button
          className={`btn-run ${isRunning ? 'running' : ''}`}
          onClick={handleRun}
          disabled={isRunning}
          title="Start continuous execution"
        >
          {isRunning ? 'Running...' : 'Run'}
        </button>
        <button
          className="btn-stop"
          onClick={handleStop}
          disabled={!isRunning}
          title="Stop execution"
        >
          Stop
        </button>
      </div>

      <div className="speed-control">
        <label>Execution Speed:</label>
        <div className="speed-slider">
          <input
            type="range"
            min="1"
            max="100"
            step="1"
            value={speed}
            onChange={(e) => handleSpeedChange(Number(e.target.value))}
            disabled={isRunning}
          />
          <div className="speed-value">{formatSpeed(speed)}</div>
        </div>
      </div>
    </div>
  );
};

export default ControlPanel;
