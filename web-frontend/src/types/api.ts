/**
 * TypeScript types matching the backend API DTOs
 */

export interface CpuState {
  iar: number;
  acc: number;
  ext: number;
  xr1: number;
  xr2: number;
  xr3: number;
  currentInterruptLevel: number;
  carry: boolean;
  overflow: boolean;
  wait: boolean;
  instructionCount: number;
  timestamp: string;
}

export interface AssembleRequest {
  sourceCode: string;
  startAddress?: number;
}

export interface AssembleResponse {
  success: boolean;
  errors: string[];
  loadedAddress?: number;
  wordsLoaded: number;
}

export interface ExecutionStatus {
  isRunning: boolean;
  cpuState: CpuState;
}
