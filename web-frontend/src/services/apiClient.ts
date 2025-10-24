import axios from 'axios';
import { AssembleRequest, AssembleResponse, ExecutionStatus, CpuState } from '../types/api';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * API client for IBM 1130 Emulator backend
 */
export const emulatorApi = {
  /**
   * Assembler endpoints
   */
  assembler: {
    /**
     * Assembles IBM 1130 assembler source code and loads it into memory
     */
    assemble: async (request: AssembleRequest): Promise<AssembleResponse> => {
      const response = await apiClient.post<AssembleResponse>('/api/assembler/assemble', request);
      return response.data;
    },

    /**
     * Resets the CPU and clears all memory
     */
    reset: async (): Promise<void> => {
      await apiClient.post('/api/assembler/reset');
    },
  },

  /**
   * Execution endpoints
   */
  execution: {
    /**
     * Gets the current execution status and CPU state
     */
    getStatus: async (): Promise<ExecutionStatus> => {
      const response = await apiClient.get<ExecutionStatus>('/api/execution/status');
      return response.data;
    },

    /**
     * Executes a single instruction step
     */
    step: async (): Promise<CpuState> => {
      const response = await apiClient.post<CpuState>('/api/execution/step');
      return response.data;
    },

    /**
     * Starts continuous execution at the specified speed
     * @param instructionsPerSecond Number of instructions to execute per second (default: 1000)
     */
    run: async (instructionsPerSecond: number = 1000): Promise<void> => {
      await apiClient.post(`/api/execution/run?instructionsPerSecond=${instructionsPerSecond}`);
    },

    /**
     * Stops continuous execution
     */
    stop: async (): Promise<void> => {
      await apiClient.post('/api/execution/stop');
    },
  },
};

export default emulatorApi;
