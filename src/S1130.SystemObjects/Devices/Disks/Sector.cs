namespace S1130.SystemObjects.Devices.Disks
{
    /// <summary>
    /// A sector is the smallest accessible unit of a disk
    /// </summary>
    public class Sector
    {
        public bool Written {get; set;}
        public ushort[] Data = new ushort[321];

        public void Load(ushort[] data)
        {
            for(int i = 0; i < Data.Length; i++)
            {
                Data[i] = data[i];
            }
        }
    }
}