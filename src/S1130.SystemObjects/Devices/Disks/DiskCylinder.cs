namespace S1130.SystemObjects.Devices.Disks
{
    /// <summary>
    /// A cylinder is a collection of 8 sectors numbered 0-7. 
    /// </summary>
    public class DiskCylinder
    {
        public DiskSector[] Sectors = new DiskSector[0];
    }
}