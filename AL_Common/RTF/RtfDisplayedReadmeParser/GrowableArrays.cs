using System;
using System.Runtime.CompilerServices;

namespace AL_Common.RTF;

public sealed partial class RtfDisplayedReadmeParser
{
    private const int _fontNameBufferDefaultCapacity = 64;

    private byte[] _fontNameBuffer = new byte[_fontNameBufferDefaultCapacity];
    private int _fontNameBuffer_Capacity = _fontNameBufferDefaultCapacity;
    private int _fontNameBuffer_Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FontNameBuffer_Add(byte item)
    {
        if (_fontNameBuffer_Count == _fontNameBuffer_Capacity)
        {
            FontNameBuffer_Grow(_fontNameBuffer_Count + 1);
        }
        _fontNameBuffer[_fontNameBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FontNameBuffer_EnsureCapacity(int min)
    {
        if (_fontNameBuffer_Capacity >= min) return;
        FontNameBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte FontNameBuffer_EnsureCapacityAndGetRef(int min)
    {
        FontNameBuffer_EnsureCapacity(min);
        return ref GetArrayDataReference(_fontNameBuffer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void FontNameBuffer_Grow(int min)
    {
        int newCapacity = _fontNameBuffer_Capacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < min) newCapacity = min;

        byte[] newArray = new byte[newCapacity];
        if (_fontNameBuffer_Count > 0) Array.Copy(_fontNameBuffer, 0, newArray, 0, _fontNameBuffer_Count);
        _fontNameBuffer = newArray;
        _fontNameBuffer_Capacity = newCapacity;
    }

    private void FontNameBuffer_ResetCapacityToDefault()
    {
        if (_fontNameBuffer_Capacity == _fontNameBufferDefaultCapacity) return;
        _fontNameBuffer = new byte[_fontNameBufferDefaultCapacity];
        _fontNameBuffer_Capacity = _fontNameBufferDefaultCapacity;
    }
}
