namespace S1130.SystemObjects
{
    public class InstructionBuilder
    {
        public static ushort BuildShort(Instructions instruction, uint tag, uint displacement)
        {
            return (ushort) ((((uint) instruction << 11) | (tag << 8) | (displacement & 0xff)) & 0xffff);
        }
    }
}