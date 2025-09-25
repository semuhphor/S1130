using System;

namespace S1130.SystemObjects.Devices
{
	/***************************************************************************
	 * ICartridge 
	 * 
	 * The 2310 disk drive reads and writes mountable cartridges.
	 * 
	 * This searves as an interface to an implementation of a cartridge. The
	 * 1130 emulator internal device will have no implementation of a cartridge.
	 * It is up to the environment to define a cartridge. This way, a the specifics
	 * of the implementation can be direct to a file, to a memory stream, an 
	 * external service, etc. This interface has the following members:
	 * 
	 * Mount(): The drive has been instructed to mount the cartridge. This lets the 
	 *			cartridge implementation do what is needed to prepare (like create a
	 *			memory stream, or open a file or IP connection, prepare a database 
	 *			connection, etc.
	 *			
	 * Mounted: This property indicates if the device is mounted. It is up to the 
	 *			cartridge implementation to set the bit when mounted.
	 *			
	 * Read(): Read a sector. Sector number is 0-7. 
	 * 
	 * Write(): Write a sector. Sector number is 0-7.
	 * 
	 * ReadOnly: Disk cannot be written.
	 * 
	 * Flush(): Provides the cartridge the opportunity to write contents to permanent 
	 *			storage.
	 *			
	 * UnMount(): Disassociate the cartridge from the drive.
	 * 
	 * CurrentCylinder: Current cylinder address. Set after seek request.
	 * 
	 *****************************************************************************/

	public interface ICartridge
	{
		bool Mounted { get; }
		void Mount();
		void UnMount();
		void Flush();
		ushort[] Read(int sectorNumber);
		//void Write(int sector, ushort[] data);

		void Write(int sectorNumber, ArraySegment<ushort> sector, int wc);

		int CurrentCylinder { get; set; }
	}
}