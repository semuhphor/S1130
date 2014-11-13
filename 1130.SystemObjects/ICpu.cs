using System.Collections.Concurrent;
using S1130.SystemObjects.InterruptManagement;

namespace S1130.SystemObjects
{
	public interface ICpu
	{
		ushort[] Memory { get; }
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

		bool AddDevice(IDevice device);
		void Transfer(int wcAddr, ushort[] values, int max);
		ulong InstructionCount { get; }
	}
}