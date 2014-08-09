﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _1130.SystemObjects;

namespace UnitTests._1130.SystemObjects
{
    [TestClass]
    public class CpuTests
    {
        private SystemState _state;
        private Cpu _cpu;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _state = new SystemState { Iar = 0x100 };
            _cpu = new Cpu();
        }

        [TestMethod]
        public void Execute_LD_Short_NoTag()
        {
            _state[_state.Iar] = InstructionBuilder.BuildShort(Instructions.Load, 0, 0x10);
            _state[_state.Iar + 1 + 0x10] = 0x1234;
            _state.NextInstruction();
            _cpu.ExecuteInstruction(_state);
            Assert.AreEqual(0x1234, _state.Acc);
        }

        [TestMethod]
        public void Execute_LDD_Short_NoTag()
        {
            _state[_state.Iar] = InstructionBuilder.BuildShort(Instructions.LoadDouble, 0, 0x10);
            int address = _state.Iar + 1 + 0x10;
            _state[address++] = 0x1234;
            _state[address] = 0x4567;
            _state.NextInstruction();
            _cpu.ExecuteInstruction(_state);
            Assert.AreEqual(0x1234, _state.Acc);
            Assert.AreEqual(0x4567, _state.Ext);
        }

        [TestMethod]
        public void EffectiveAddress()
        {
            _state[_state.Iar] = InstructionBuilder.BuildShort(Instructions.Load, 0, 0x20);
            _state.NextInstruction();
            Assert.AreEqual(_state.Iar + 0x20, _cpu.GetEffectiveAddress(_state));
        }

    }
}
