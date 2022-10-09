using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    //Note: Block = 512 bytes, Paragraph = 16 bytes
    struct DOSExe
    {
        UInt16 MagicNumber;
        UInt16 LastBlockBytesLSB;
        UInt16 TotalBlocksLSB;
        UInt16 ReallocEntriesLSB;
        UInt16 HeaderParasLSB;
        UInt16 MemParasRqrdLSB;
        UInt16 MaxAddlMemParasLSB;
        UInt16 SSOffsetLSB;
        UInt16 SPInitLSB;
        UInt16 ChecksumLSB;
        UInt16 IPInitLSB;
        UInt16 CSInitLSB;
        UInt16 RelocOffLSB;
        UInt16 OVLNumLSB;
    }

    struct DOSExeReloc
    {
        UInt16 Offset,
               Segment;
    }
}
