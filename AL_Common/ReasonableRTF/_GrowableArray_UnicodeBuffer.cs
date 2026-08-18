using System;
using System.Runtime.CompilerServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private char[] _unicodeBuffer = new char[_internalBufferDefaultCapacity];
    private int _unicodeBuffer_Count;
    private int _unicodeBuffer_Capacity = _internalBufferDefaultCapacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnicodeBuffer_Add(char item)
    {
        if (_unicodeBuffer_Count == _unicodeBuffer_Capacity)
        {
            UnicodeBuffer_Grow(_unicodeBuffer_Count + 1);
        }
        _unicodeBuffer[_unicodeBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnicodeBuffer_EnsureCapacity(int min)
    {
        if (_unicodeBuffer_Capacity >= min) return;
        UnicodeBuffer_Grow(min);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UnicodeBuffer_Grow(int min)
    {
        int newCapacity = _unicodeBuffer_Capacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < min) newCapacity = min;

        char[] newArray = new char[newCapacity];
        if (_unicodeBuffer_Count > 0) Array.Copy(_unicodeBuffer, 0, newArray, 0, _unicodeBuffer_Count);
        _unicodeBuffer = newArray;
        _unicodeBuffer_Capacity = newCapacity;
    }

    private void UnicodeBuffer_ResetCapacityToDefault()
    {
        if (_unicodeBuffer_Capacity == _internalBufferDefaultCapacity) return;
        _unicodeBuffer = new char[_internalBufferDefaultCapacity];
        _unicodeBuffer_Capacity = _internalBufferDefaultCapacity;
    }
}
