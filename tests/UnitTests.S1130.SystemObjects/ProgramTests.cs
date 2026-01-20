using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using S1130.SystemObjects;
using S1130.SystemObjects.Instructions;

namespace UnitTests.S1130.SystemObjects
{
	public class ProgramTests
	{
		private Cpu _cpu;
		private readonly ITestOutputHelper _output;

		public ProgramTests(ITestOutputHelper output)
		{
			_output = output;
		}

		protected void BeforeEachTest()
		{
			_cpu = new Cpu {Iar = 0x100};
		}

		[Fact]
		public void CountUpTheAccumulator()
		{
			BeforeEachTest();
			var iar = _cpu.Iar;
			var location = iar;
			_cpu[0x81] = 20;
			_cpu[0x82] = 1;
			InstructionBuilder.BuildShortAtAddress(OpCodes.ShiftLeft, 0, 16, _cpu, location++);				// 0x0100: clear ACC
			InstructionBuilder.BuildLongIndirectAtAddress(OpCodes.LoadIndex, 1, 0x81, _cpu, location);		// 0x0101: load XR1 with starting count of 20
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildLongAtAddress(OpCodes.Add, 0, 0x82, _cpu, location);					// 0x0103: add one to ACC
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildShortAtAddress(OpCodes.ModifyIndex, 1, 0xfe, _cpu, location++);			// 0x0105: Decrement XR1 by 2
			InstructionBuilder.BuildShortAtAddress(OpCodes.ModifyIndex, 0, 0xfc, _cpu, location++);         // 0x0106: Branch to 0x0103.
			InstructionBuilder.BuildShortAtAddress(OpCodes.Wait, 0, 0, _cpu, location);                     // 0x0107: Wait. End of program
			_cpu.Iar = iar;
			Assert.Equal(32, RunUntilWait());
			Assert.Equal(10, _cpu.Acc);
		}

		[Fact]
		public void DoAMillionAdds()
		{
			BeforeEachTest();
			var location = _cpu.Iar;
			_cpu[0x81] = 20;
			_cpu[0x82] = 1;
			InstructionBuilder.BuildLongAtAddress(OpCodes.Add, 0, 0x82, _cpu, location);					// 0x0101: add one to ACC
			location += 2; // ... increment for long instruction
			InstructionBuilder.BuildLongAtAddress(OpCodes.LoadIndex, 0, 0x100, _cpu, location);				// 0x0101: Branch to 0x100
			var watch = Stopwatch.StartNew();
			// while (_cpu.InstructionCount < endingCount)
			while(watch.ElapsedMilliseconds < 1000)
			{
				_cpu.ExecuteInstruction();
			}
			watch.Stop();
			_output.WriteLine("1M Instructions in {0}ms. ({1})", watch.ElapsedMilliseconds, _cpu.InstructionCount);
		}

		[Fact]
		public void AssembleAndRunSomeCode()
		{
			BeforeEachTest();
			const string EXAMPLE_PROGRAM = @"// IBM 1130 Shift Left Test Program
// Demonstrates SLT (Shift Left Together) with carry detection
// 
// This program loads 1 into ACC/EXT registers, then repeatedly
// shifts left until carry is set (bit shifts out), then restarts.
// Watch the bit travel through all 32 bits!
//
       ORG  /100
//
// Main program loop
//
START: LDD  |L|ONE       // Load double-word (0,1) into ACC and EXT
LOOP:  SLT  1            // Shift left together 1 bit
       BSC  |L|START,C   // Carry ON -- Set acc:ext = 1
       BSC  |L|LOOP      // else.. keep shifting.
//
// Data section
//
       BSS  |E|          // Align to even address
ONE:   DC   0            // High word (ACC) = 0
       DC   1            // Low word (EXT) = 1
";
			var result = _cpu.Assemble(EXAMPLE_PROGRAM);
			Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
			_cpu.Iar = 0x100;
			int i = 12; 
			while(i-- > 0)
			{
				_cpu.ExecuteInstruction(); 
			}
			Assert.Equal(0x0010, _cpu.Ext);
			while(0x0100 != _cpu.Iar)
			{
				_cpu.ExecuteInstruction(); 
			}
			Assert.True(_cpu.Carry);
		}


		[Fact]
		public void DoAMillionAddsWithAssembler()
		{
			BeforeEachTest();
			
			var source = @"      ORG /100
LOOP: A |L|ONE
      BSI |L|LOOP
ONE:  DC 1";

			var result = _cpu.Assemble(source);
			
			Assert.True(result.Success, $"Assembly failed: {string.Join(", ", result.Errors.Select(e => $"Line {e.LineNumber}: {e.Message}"))}");
			
			var watch = Stopwatch.StartNew();
			_cpu.Iar = 0x100;
			
			while(watch.ElapsedMilliseconds < 1000)
			{
				_cpu.ExecuteInstruction();
			}
			watch.Stop();
			_output.WriteLine("1M Instructions (Assembler) in {0}ms. ({1})", watch.ElapsedMilliseconds, _cpu.InstructionCount);
		}

		private int RunUntilWait()
		{
			var numberOfInstructions = 0;
			while (!_cpu.Wait)
			{
				_cpu.ExecuteInstruction();
				numberOfInstructions++;
			}
			return numberOfInstructions;
		}
	}
}