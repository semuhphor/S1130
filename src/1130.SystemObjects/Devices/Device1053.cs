using System.Collections.Concurrent;

namespace S1130.SystemObjects.Devices
{
	/* ******************************************************************************************
	 * Device: 1053 Console Printer
	 * 
	 * This device class emulates a 1052. There are only two commands accepted by the 2501: 
	 * - Wrote: Transfer a character or control value out of memory (1 per write)
	 * - Sense device: Return the device's status word
	 * 
	 * The status word is as follows:
     *      1... .... .... ....     Printer Response (operation complete)
     *      .... 1... .... ....     Printer Busy
	 *		.... .0.. .... ....		Printer Ready (1 = Not ready = has paper and not busy)
	 * ******************************************************************************************/		
	// public class Device1053 : DeviceBase
    // {
	// 	public const ushort OperationCompleteStatus = 0x8000;
	// 	public const ushort BusyStatus = 0x0800;
	// 	public const ushort NotReadyOrBusyStatus = 0x0400;
    // }
}
