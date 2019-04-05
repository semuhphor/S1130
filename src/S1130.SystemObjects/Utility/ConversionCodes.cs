using System.Collections.Generic;

namespace S1130.SystemObjects.Utility
{
    internal struct CrCode
    {
        public char Ascii;
        public ushort CardCode;

        public CrCode(char ascii, ushort cardCode) 
        {
            Ascii = ascii;
            CardCode = cardCode;
        }
    }
    
    public class ConversionCodes
    {
        // Ascii character to card code conversion table
        private List<CrCode> CrCodes = new List<CrCode> 
        {
            new CrCode() {Ascii = ' ', CardCode = 0x0000},
            new CrCode() {Ascii = '&', CardCode = 0x8000},	 	
            new CrCode() {Ascii = '-', CardCode = 0x4000},
            new CrCode() {Ascii = '0', CardCode = 0x2000},
            new CrCode() {Ascii = '1', CardCode = 0x1000},
            new CrCode() {Ascii = '2', CardCode = 0x0800},
            new CrCode() {Ascii = '3', CardCode = 0x0400},
            new CrCode() {Ascii = '4', CardCode = 0x0200},
            new CrCode() {Ascii = '5', CardCode = 0x0100},
            new CrCode() {Ascii = '6', CardCode = 0x0080},
            new CrCode() {Ascii = '7', CardCode = 0x0040},
            new CrCode() {Ascii = '8', CardCode = 0x0020},
            new CrCode() {Ascii = '9', CardCode = 0x0010},
            new CrCode() {Ascii = 'A', CardCode = 0x9000},
            new CrCode() {Ascii = 'B', CardCode = 0x8800},
            new CrCode() {Ascii = 'C', CardCode = 0x8400},
            new CrCode() {Ascii = 'D', CardCode = 0x8200},
            new CrCode() {Ascii = 'E', CardCode = 0x8100},
            new CrCode() {Ascii = 'F', CardCode = 0x8080},
            new CrCode() {Ascii = 'G', CardCode = 0x8040},
            new CrCode() {Ascii = 'H', CardCode = 0x8020},
            new CrCode() {Ascii = 'I', CardCode = 0x8010},
            new CrCode() {Ascii = 'J', CardCode = 0x5000},
            new CrCode() {Ascii = 'K', CardCode = 0x4800},
            new CrCode() {Ascii = 'L', CardCode = 0x4400},
            new CrCode() {Ascii = 'M', CardCode = 0x4200},
            new CrCode() {Ascii = 'N', CardCode = 0x4100},
            new CrCode() {Ascii = 'O', CardCode = 0x4080},
            new CrCode() {Ascii = 'P', CardCode = 0x4040},
            new CrCode() {Ascii = 'Q', CardCode = 0x4020},
            new CrCode() {Ascii = 'R', CardCode = 0x4010},
            new CrCode() {Ascii = '/', CardCode = 0x3000},
            new CrCode() {Ascii = 'S', CardCode = 0x2800},
            new CrCode() {Ascii = 'T', CardCode = 0x2400},
            new CrCode() {Ascii = 'U', CardCode = 0x2200},
            new CrCode() {Ascii = 'V', CardCode = 0x2100},
            new CrCode() {Ascii = 'W', CardCode = 0x2080},
            new CrCode() {Ascii = 'X', CardCode = 0x2040},
            new CrCode() {Ascii = 'Y', CardCode = 0x2020},
            new CrCode() {Ascii = 'Z', CardCode = 0x2010},
            new CrCode() {Ascii = ':', CardCode = 0x0820},
            new CrCode() {Ascii = '#', CardCode = 0x0420},		
            new CrCode() {Ascii = '@', CardCode = 0x0220},		
            new CrCode() {Ascii = '\'', CardCode = 0x0120},
            new CrCode() {Ascii = '=', CardCode = 0x00A0},
            new CrCode() {Ascii = '"', CardCode = 0x0060},
            new CrCode() {Ascii = '\xA2', CardCode = 0x8820},		
            new CrCode() {Ascii = '.', CardCode = 0x8420},
            new CrCode() {Ascii = '<', CardCode = 0x8220},		
            new CrCode() {Ascii = '(', CardCode = 0x8120},
            new CrCode() {Ascii = '+', CardCode = 0x80A0},
            new CrCode() {Ascii = '|', CardCode = 0x8060},
            new CrCode() {Ascii = '!', CardCode = 0x4820},
            new CrCode() {Ascii = '$', CardCode = 0x4420},
            new CrCode() {Ascii = '*', CardCode = 0x4220},
            new CrCode() {Ascii = ')', CardCode = 0x4120},
            new CrCode() {Ascii = ';', CardCode = 0x40A0},
            new CrCode() {Ascii = '\xAC', CardCode = 0x4060},
            new CrCode() {Ascii = ',', CardCode = 0x2420},
            new CrCode() {Ascii = '%', CardCode = 0x2220},
            new CrCode() {Ascii = '_', CardCode = 0x2120},
            new CrCode() {Ascii = '>', CardCode = 0x20A0},
            new CrCode() {Ascii = 'a', CardCode = 0xB000},
            new CrCode() {Ascii = 'b', CardCode = 0xA800},
            new CrCode() {Ascii = 'c', CardCode = 0xA400},
            new CrCode() {Ascii = 'd', CardCode = 0xA200},
            new CrCode() {Ascii = 'e', CardCode = 0xA100},
            new CrCode() {Ascii = 'f', CardCode = 0xA080},
            new CrCode() {Ascii = 'g', CardCode = 0xA040},
            new CrCode() {Ascii = 'h', CardCode = 0xA020},
            new CrCode() {Ascii = 'i', CardCode = 0xA010},
            new CrCode() {Ascii = 'j', CardCode = 0xD000},
            new CrCode() {Ascii = 'k', CardCode = 0xC800},
            new CrCode() {Ascii = 'l', CardCode = 0xC400},
            new CrCode() {Ascii = 'm', CardCode = 0xC200},
            new CrCode() {Ascii = 'n', CardCode = 0xC100},
            new CrCode() {Ascii = 'o', CardCode = 0xC080},
            new CrCode() {Ascii = 'p', CardCode = 0xC040},
            new CrCode() {Ascii = 'q', CardCode = 0xC020},
            new CrCode() {Ascii = 'r', CardCode = 0xC010},
            new CrCode() {Ascii = 's', CardCode = 0x6800},
            new CrCode() {Ascii = 't', CardCode = 0x6400},
            new CrCode() {Ascii = 'u', CardCode = 0x6200},
            new CrCode() {Ascii = 'v', CardCode = 0x6100},
            new CrCode() {Ascii = 'w', CardCode = 0x6080},
            new CrCode() {Ascii = 'x', CardCode = 0x6040},
            new CrCode() {Ascii = 'y', CardCode = 0x6020},
            new CrCode() {Ascii = 'z', CardCode = 0x6010}
        };

        // values for column punches
        private ushort[] Punches = new ushort[]
        {
            0x2000, // 0 punch
            0x1000, // 1 punch
            0x0800, // 2 punch
            0x0400, // 3 punch
            0x0200, // 4 punch
            0x0100, // 5 punch
            0x0080, // 6 punch
            0x0040, // 7 punch
            0x0020, // 8 punch
            0x0010, // 9 punch
            0x0000, // 10 is an invalid punch!
            0x4000, // 11 punch
            0x8000  // 12 punch
        } ;

        public ushort ToCrColumn(params int[] punches)
        {
            ushort column = 0x0000;
            var punchList = new List<int>(punches);
            punchList.ForEach(p => 
                {
                    if (p >= 0 && p <=12)
                        column |= Punches[p];
                }
            );
            return column;
        }

        public ushort ToCrColumn(char ascii)
        {
            var index = CrCodes.FindIndex(c => c.Ascii == ascii);
            return (ushort) ((index == -1) ? 0 :  CrCodes[index].CardCode);
        }

        public Card ToCard(string s)
        {
            var charList = new List<char>(s.ToCharArray());
            var card = new Card();
            int i = 0;
            charList.ForEach(c => card[i++] = ToCrColumn(c));
            return card;
        }
    }
}
