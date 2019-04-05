using System;

namespace S1130.SystemObjects.Devices.Disks
{
    public class FileCartridge : ICartridge
    {
        public int CurrentCylinder { get; set; }						// current cylinder (set after seek)
        public bool MountCalled { get; set; }							// indicates mount called
        public bool ReadCalled { get; set; }							// indicates read called
        public bool WriteCalled { get; set; }							// indicates write called
        public int SectorNumber { get; set; }							// sector read
        public bool FlushCalled { get; set; }							// indicates flush called
        public bool Mounted { get; private set; }						// indicates if mounted
        public readonly ushort[] Sector = new ushort[321];				// place for a sector

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void Mount()
        {
            throw new NotImplementedException();
        }

        public ushort[] Read(int sectorNumber)
        {
            throw new NotImplementedException();
        }

        public void UnMount()
        {
            throw new NotImplementedException();
        }

        public void Write(int sectorNumber, ArraySegment<ushort> sector, int wc)
        {
            throw new NotImplementedException();
        }
    }
}