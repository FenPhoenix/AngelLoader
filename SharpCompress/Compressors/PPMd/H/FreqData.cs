using System;
using System.Buffers.Binary;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class FreqData : Pointer
{
    internal const int SIZE = 6;

    internal FreqData(byte[] memory)
        : base(memory) { }

    internal int SummFreq
    {
        get => BinaryPrimitives.ReadInt16LittleEndian(Memory.AsSpan(Address)) & 0xffff;
        set => BinaryPrimitives.WriteInt16LittleEndian(Memory.AsSpan(Address), (short)value);
    }

    internal FreqData Initialize(byte[] mem) => Initialize<FreqData>(mem);

    internal void IncrementSummFreq(int dSummFreq) => SummFreq += (short)dSummFreq;

    internal int GetStats() => BinaryPrimitives.ReadInt32LittleEndian(Memory.AsSpan(Address + 2));

    internal void SetStats(State state) => SetStats(state.Address);

    internal void SetStats(int state) =>
        BinaryPrimitives.WriteInt32LittleEndian(Memory.AsSpan(Address + 2), state);
}
