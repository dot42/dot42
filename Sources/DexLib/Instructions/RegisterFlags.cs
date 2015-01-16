using System;

namespace Dot42.DexLib.Instructions
{
    [Flags]
    public enum RegisterFlags
    {
        // Size
        Bits4 = 0x01,
        Bits8 = 0x02,
        Bits16 = 0x04,
        SizeMask = 0x0F,

        // Direction
        Source = 0x10,
        Destination = 0x20,
        DirectionMask = 0xF0,

        // Usage size
        Wide = 0x1000,

        // Combinations
        Destination4Bits = Destination | Bits4,
        Destination8Bits = Destination | Bits8,
        Destination16Bits = Destination | Bits16,
        Source4Bits = Source | Bits4,
        Source8Bits = Source | Bits8,
        Source16Bits = Source | Bits16,
        DestAndSource4Bits = Destination | Source | Bits4,
        DestAndSource8Bits = Destination | Source | Bits8,
        DestAndSource16Bits = Destination | Source | Bits16,

        Destination4BitsWide = Destination4Bits | Wide,
        Destination8BitsWide = Destination8Bits | Wide,
        Destination16BitsWide = Destination16Bits | Wide,
        Source4BitsWide = Source4Bits | Wide,
        Source8BitsWide = Source8Bits | Wide,
        Source16BitsWide = Source16Bits | Wide,
        DestAndSource4BitsWide = DestAndSource4Bits | Wide,
        DestAndSource8BitsWide = DestAndSource8Bits | Wide,
        DestAndSource16BitsWide = DestAndSource16Bits | Wide 
}
}
