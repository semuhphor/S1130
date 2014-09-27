﻿namespace S1130.SystemObjects
{
	public enum DevFuction
	{
		Write = 0x1
		,Read = 0x2
		,SenseInterrupte = 0x3
		,Control = 0x4
		,InitWrite = 0x5
		,InitRead = 0x6
		,SenseDevice = 0x7
	}

	public interface IDevice
	{
		byte DeviceCode { get; }
		void ExecuteIocc(ISystemState state, ushort ioccAddress);
	}
}