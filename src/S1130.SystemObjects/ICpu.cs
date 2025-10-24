using System;
using System.Collections.Concurrent;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public interface ICpu
	{
		ushort[] Memory { get; }

		bool MasterDebug { get; set; }

		void SetDebug(int location, IDebugSetting debugSetting);
		void ResetDebug(int location);

		int MemorySize { get; }

		ushort ConsoleSwitches { get; set; }
		ushort Iar { get; set; }
		ushort Acc { get; set; }
		ushort Ext { get; set; }
		uint AccExt { get; set; }
		ushort this[int address] { get; set; }

		IndexRegisters Xr { get; }
		IInstruction[] Instructions { get; }
		IInstruction CurrentInstruction { get; }
		InterruptPool IntPool { get; }
		IDevice[] Devices { get; }

		ushort Opcode { get; }
		bool FormatLong { get; }
		ushort Tag { get; }
		ushort Displacement { get; }
		bool IndirectAddress { get; }
		ushort Modifiers { get; }

		bool Carry { get; set; }
		bool Overflow { get; set; }
		bool Wait { get; set; }
		ushort AtIar { get; set; }

		int? CurrentInterruptLevel { get; }
		void AddInterrupt(Interrupt interrupt);
		void HandleInterrupt();
		void ClearCurrentInterrupt();

		void NextInstruction();
		void ExecuteInstruction();
		ConcurrentQueue<Interrupt>[] InterruptQueues { get; }
		ConcurrentStack<Interrupt> CurrentInterrupt { get; }
		int ActiveInterruptCount { get; }

		int IoccAddress { get; set; }
		int IoccDeviceCode { get; set; }
		DevFunction IoccFunction { get; set; }
		int IoccModifiers { get; set; }
		void IoccDecode(int address);
		IDevice IoccDevice { get; set; }
		ArraySegment<ushort> GetBuffer();

		bool AddDevice(IDevice device);
		void TransferToMemory(int wcAddr, ushort[] values, int max);
		void TransferToMemory(ushort[] values, int max);
		ulong InstructionCount { get; }
		bool IgnoreInstructionCount { get; set; }

		void LetInstuctionsExecute(ulong numberOfInstructions);

        /// <summary>
        /// Assembles IBM 1130 assembly code into memory.
        /// </summary>
        /// <param name="sourceCode">Assembly source code with lines separated by CR, LF or CRLF</param>
        /// <returns>Assembly result containing listing, errors and success flag</returns>
        AssemblyResult Assemble(string sourceCode);
        
        /// <summary>
        /// Disassembles the instruction at the specified memory address.
        /// </summary>
        /// <param name="address">Memory address to disassemble</param>
        /// <returns>Assembly source string for the instruction, or error message if invalid</returns>
        string Disassemble(ushort address);
	}
}