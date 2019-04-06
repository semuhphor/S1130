using System;

namespace S1130.SystemObjects.Devices.Disks
{
    /// <summary>
    /// A memory cartridge is what the S1130 uses as disk. Regardless of where the information resides, it is loaded into a memory
    /// cartridge and resides in memory until flushed. It is the job of the implemntation to load and save the memory cartridge.
    /// 
    /// Note: The memory cartridge notes any sectors that have been written. It is possible to write only updated sectors, if any.
    /// </summary>
    public abstract class MemoryCartridge : ICartridge
    {
        public abstract bool Mounted { get; }
        public abstract int CurrentCylinder { get; set; }
        public abstract void Flush();
        public abstract void Mount();
        public abstract ushort[] Read(int sectorNumber);
        public abstract void UnMount();
        public abstract void Write(int sectorNumber, ArraySegment<ushort> sector, int wc);
    }
}