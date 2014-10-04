namespace S1130.SystemObjects
{
    public class Constants
    {
        public const ushort FormatLong = 0x400;
        public const ushort Indirect = 0x80;
        public const byte InstructionShift = 11;
        public const byte TagShift = 8;
        public const ushort DisplacementMask = 0xff;

		public static readonly ushort[] InterruptVectors = { 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d };
	}
}