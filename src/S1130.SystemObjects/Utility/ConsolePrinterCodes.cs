using System.Collections.Generic;

namespace S1130.SystemObjects.Utility
{
	/// <summary>
	/// Console printer (1053) tilt/rotate codes from Functional Characteristics Appendix A.
	/// The high byte of a print word holds the code: bit 6 selects upper case, bit 7 marks a control function.
	/// </summary>
	public static class ConsolePrinterCodes
	{
		private static readonly Dictionary<byte, char> Graphics = BuildGraphics();

		/// <summary>Returns the text produced by a print word, or an empty string for codes that print nothing.</summary>
		public static string Decode(ushort printWord)
		{
			var code = (byte)(printWord >> 8);
			switch (code)
			{
				case 0x21: return " ";
				case 0x41: return "\t";
				case 0x81:                                      // new line
				case 0x03: return "\n";                         // line feed
				case 0x05: return "\r";                         // restore (carriage return)
				case 0x11: return "\b";
			}
			return Graphics.TryGetValue(code, out var c) ? c.ToString() : string.Empty;
		}

		private static Dictionary<byte, char> BuildGraphics()
		{
			var map = new Dictionary<byte, char>();
			// Letters: lower case code, upper case = lower + 2.
			const string letters = "abcdefghijklmnopqrstuvwxyz";
			byte[] lower =
			{
				0x3C, 0x18, 0x1C, 0x30, 0x34, 0x10, 0x14, 0x24, 0x20,		// a-i
				0x7C, 0x58, 0x5C, 0x70, 0x74, 0x50, 0x54, 0x64, 0x60,		// j-r
				0x98, 0x9C, 0xB0, 0xB4, 0x90, 0x94, 0xA4, 0xA0				// s-z
			};
			for (var i = 0; i < letters.Length; i++)
			{
				map[lower[i]] = letters[i];
				map[(byte)(lower[i] + 2)] = char.ToUpperInvariant(letters[i]);
			}
			map.Remove(0x7E);											// j + 2 is not a code: J's upper case is 0xCE
			map[0xCE] = 'J';
			// Digits 0-9.
			byte[] digits = { 0xC4, 0xFC, 0xD8, 0xDC, 0xF0, 0xF4, 0xD0, 0xD4, 0xE4, 0xE0 };
			for (var i = 0; i < digits.Length; i++)
			{
				map[digits[i]] = (char)('0' + i);
			}
			// Special characters.
			map[0x00] = '.';
			map[0x80] = ',';
			map[0x84] = '-';
			map[0xBC] = '/';
			map[0x44] = '&';
			map[0x40] = '$';
			map[0xC0] = '#';
			map[0x04] = '@';
			map[0x02] = '\u00A2';
			map[0xDE] = '<';
			map[0xFE] = '(';
			map[0xDA] = '+';
			map[0xC6] = '|';
			map[0x42] = '!';
			map[0xD6] = '*';
			map[0xF6] = ')';
			map[0xD2] = ';';
			map[0xF2] = '\u00AC';
			map[0x06] = '%';
			map[0xBE] = '_';
			map[0x46] = '>';
			map[0x86] = '?';
			map[0x82] = ':';
			map[0xE6] = '\'';
			map[0xC2] = '=';
			map[0xE2] = '"';
			return map;
		}
	}
}
