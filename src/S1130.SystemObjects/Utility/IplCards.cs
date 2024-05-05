﻿using System;

namespace S1130.SystemObjects.Utility;

/// <summary>
/// Represents a collection of pre-expanded IPL cards.
/// </summary>
 public class IplCards
{
    public static readonly ushort[] IPLCardDms12 = [				/* DMSV2M12, already expanded to 16 bits */
		0xc80a, 0x18c2, 0xd008, 0xc019, 0x8007, 0xd017, 0xc033, 0x100a,
        0xd031, 0x7015, 0x000c, 0xe800, 0x0020, 0x08f8, 0x4828, 0x7035,
        0x70fa, 0x4814, 0xf026, 0x2000, 0x8800, 0x9000, 0x9800, 0xa000,
        0xb000, 0xb800, 0xb810, 0xb820, 0xb830, 0xb820, 0x3000, 0x08ea,
        0xc0eb, 0x4828, 0x70fb, 0x9027, 0x4830, 0x70f8, 0x8001, 0xd000,
        0xc0f4, 0xd0d9, 0xc01d, 0x1804, 0xe8d6, 0xd0d9, 0xc8e3, 0x18d3,
        0xd017, 0x18c4, 0xd0d8, 0x9016, 0xd815, 0x90db, 0xe8cc, 0xd0ef,
        0xc016, 0x1807, 0x0035, 0x00d0, 0xc008, 0x1803, 0xe8c4, 0xd00f,
        0x080d, 0x08c4, 0x1003, 0x4810, 0x70d9, 0x3000, 0x08df, 0x3000,
        0x7010, 0x00d1, 0x0028, 0x000a, 0x70f3, 0x0000, 0x00d0, 0xa0c0
    ];
    public static readonly ushort[] IPLCardApl = [ 				    /* APLIPL, already expanded */
		0x7021, 0x3000, 0x7038, 0xa0c0, 0x0002, 0x4808, 0x0003, 0x0026,
        0x0001, 0x0001, 0x000c, 0x0000, 0x0000, 0x0800, 0x48f8, 0x0027,
        0x7002, 0x08f2, 0x3800, 0xe0fe, 0x18cc, 0x100e, 0x10c1, 0x4802,
        0x7007, 0x4828, 0x7005, 0x4804, 0x7001, 0x70f3, 0x08e7, 0x70e1,
        0x08ed, 0x70f1, 0xc0e0, 0x1807, 0xd0de, 0xc0df, 0x1801, 0xd0dd,
        0x800d, 0xd00c, 0xc0e3, 0x1005, 0xe80a, 0xd009, 0xc0d8, 0x1008,
        0xd0d6, 0xc0dd, 0x1008, 0x80d4, 0xd0da, 0x1000, 0xb000, 0x00f6,
        0x70e7, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        0x9000, 0x4004, 0x40c0, 0x8001, 0x4004, 0x40c0, 0x0000, 0x0000
    ];
    public static readonly ushort[] IPLCardAplPriv = [ 				/* APLIPL Privileged, already expanded */
		0x7021, 0x3000, 0x7038, 0xa0c0, 0x0002, 0x4808, 0x0003, 0x0026,
        0x0001, 0x0001, 0x000c, 0x0000, 0x0000, 0x0800, 0x48f8, 0x0027,
        0x7002, 0x08f2, 0x3800, 0xe0fe, 0x18cc, 0x100e, 0x10c1, 0x4802,
        0x7007, 0x4828, 0x7005, 0x4804, 0x7001, 0x70f3, 0x08e7, 0x70e1,
        0x08ed, 0x70f1, 0xc0e0, 0x1807, 0xd0de, 0xc0df, 0x1801, 0xd0dd,
        0x800d, 0xd00c, 0xc0e3, 0x1005, 0xe80a, 0xd009, 0xc0d8, 0x1008,
        0xd0d6, 0xc0dd, 0x1008, 0x80d4, 0xd0da, 0x1002, 0xb000, 0x00f6,
        0x70e7, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        0x9000, 0x4004, 0x40c0, 0x8001, 0x4004, 0x40c0, 0x4004, 0x4001
    ];
}
