using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
	public class ProgramTests
	{
		private Cpu _cpu;

		protected void BeforeEachTest()
		{
			_cpu = new Cpu {Iar = 0x100};
		}

		[Fact]
		public void CountUpTheAccumulator()
		{
			BeforeEachTest();
			var location = _cpu.Iar;
			_cpu[0x81] = 20;
			_cpu[0x82] = 1;
			InstructionBuilder.BuildShortAtAddress(OpCodes.ShiftLeft, 0, 16, _cpu, location++);				// 0x0100: clear ACC
			InstructionBuilder.BuildLongIndirectAtAddress(OpCodes.LoadIndex, 1, 0x81, _cpu, location);		// 0x0101: load XR1 with starting count of 20
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildLongAtAddress(OpCodes.Add, 0, 0x82, _cpu, location);					// 0x0103: add one to ACC
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildShortAtAddress(OpCodes.ModifyIndex, 1, 0xfe, _cpu, location++);			// 0x0105: Decrement XR1 by 2
			InstructionBuilder.BuildShortAtAddress(OpCodes.ModifyIndex, 0, 0xfc, _cpu, location++);			// 0x0106: Branch to 0x0103.
			InstructionBuilder.BuildShortAtAddress(OpCodes.Wait, 0, 0, _cpu, location);						// 0x0107: Wait. End of program
			Assert.Equal(32, RunUntilWait());
			Assert.Equal(10, _cpu.Acc);
		}

		[Fact]
		public void DoAMillionAdds()
		{
			BeforeEachTest();
			var location = _cpu.Iar;
			var startingCount = _cpu.InstructionCount;
			var endingCount = startingCount + 2000000;
			_cpu[0x81] = 20;
			_cpu[0x82] = 1;
			InstructionBuilder.BuildLongAtAddress(OpCodes.Add, 0, 0x82, _cpu, location);					// 0x0101: add one to ACC
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildLongAtAddress(OpCodes.LoadIndex, 0, 0x100, _cpu, location);				// 0x0101: Branch to 0x100
			var numberOfInstructions = 0;
			var watch = Stopwatch.StartNew();
			// while (_cpu.InstructionCount < endingCount)
			while(watch.ElapsedMilliseconds < 1000)
			{
				_cpu.NextInstruction();
				_cpu.ExecuteInstruction();
				numberOfInstructions++;
			}
			watch.Stop();
			Console.Error.WriteLine("1M Instructions in {0}ms. ({1})", watch.ElapsedMilliseconds, _cpu.InstructionCount);
		}

		[Fact]
		public void DoAMillionAddsWithAssembler()
		{
			BeforeEachTest();
			
			var source = @"      ORG  /100
LOOP  A    L ONE
      LDX  L LOOP
ONE   DC   1";

			var result = _cpu.Assemble(source);
			
			Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
			Assert.Empty(result.Errors);
			
			var numberOfInstructions = 0;
			var watch = Stopwatch.StartNew();
			_cpu.Iar = 0x100;
			
			while(watch.ElapsedMilliseconds < 1000)
			{
				_cpu.NextInstruction();
				_cpu.ExecuteInstruction();
				numberOfInstructions++;
			}
			watch.Stop();
			Console.Error.WriteLine("1M Instructions (Assembler) in {0}ms. ({1})", watch.ElapsedMilliseconds, _cpu.InstructionCount);
		}

		private int RunUntilWait()
		{
			var numberOfInstructions = 0;
			while (!_cpu.Wait)
			{
				_cpu.NextInstruction();
				// Console.Out.WriteLine("{0:x4}: {1}, Acc: {2:x4} XR1: {3:x4}", _cpu.Iar, _cpu.CurrentInstruction.OpCode, _cpu.Acc, _cpu.Xr[1]);
				_cpu.ExecuteInstruction();
				numberOfInstructions++;
			}
			return numberOfInstructions;
		}
	}
}