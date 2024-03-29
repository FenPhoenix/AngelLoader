﻿namespace SharpCompress.Compressors.Rar.UnpackV2017;

internal partial class BitInput
{
    public const int MAX_SIZE = 0x8000; // Size of input buffer.

    public int InAddr; // Curent byte position in the buffer.
    public int InBit; // Current bit position in the current byte.

    public readonly bool ExternalBuffer;

    //BitInput(bool AllocBuffer);
    //~BitInput();

    public readonly byte[] InBuf; // Dynamically allocated input buffer.

    public void InitBitInput() => InAddr = InBit = 0;

    // Move forward by 'Bits' bits.
    public void addbits(uint _Bits)
    {
        var Bits = checked((int)_Bits);
        Bits += InBit;
        InAddr += Bits >> 3;
        InBit = Bits & 7;
    }

    // Return 16 bits from current position in the buffer.
    // Bit at (InAddr,InBit) has the highest position in returning data.
    public uint getbits()
    {
        var BitField = (uint)InBuf[InAddr] << 16;
        BitField |= (uint)InBuf[InAddr + 1] << 8;
        BitField |= InBuf[InAddr + 2];
        BitField >>= (8 - InBit);
        return BitField & 0xffff;
    }

    // Return 32 bits from current position in the buffer.
    // Bit at (InAddr,InBit) has the highest position in returning data.
    public uint getbits32()
    {
        var BitField = (uint)InBuf[InAddr] << 24;
        BitField |= (uint)InBuf[InAddr + 1] << 16;
        BitField |= (uint)InBuf[InAddr + 2] << 8;
        BitField |= InBuf[InAddr + 3];
        BitField <<= InBit;
        BitField |= (uint)InBuf[InAddr + 4] >> (8 - InBit);
        return BitField & 0xffffffff;
    }

    //void SetExternalBuffer(byte *Buf);
}
