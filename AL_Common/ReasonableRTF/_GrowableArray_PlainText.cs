using System;
using System.Runtime.CompilerServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private const int _plainTextDefaultCapacity = 4096;

    private char[] _plainText = new char[_plainTextDefaultCapacity];
    private int _plainText_Capacity = _plainTextDefaultCapacity;
    private int _plainText_Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PlainText_Add(char item)
    {
        if (_plainText_Count == _plainText_Capacity)
        {
            PlainText_Grow(_plainText_Count + 1);
        }
        _plainText[_plainText_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PlainText_EnsureCapacity(int min)
    {
        if (_plainText_Capacity >= min) return;
        PlainText_Grow(min);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char PlainText_EnsureCapacityAndGetRef(int min)
    {
        PlainText_EnsureCapacity(min);
        return ref GetArrayDataReference(_plainText);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PlainText_Grow(int min)
    {
        int newCapacity = _plainText_Capacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < min) newCapacity = min;

        char[] newArray = new char[newCapacity];
        if (_plainText_Count > 0) Array.Copy(_plainText, 0, newArray, 0, _plainText_Count);
        _plainText = newArray;
        _plainText_Capacity = newCapacity;
    }

    private void PlainText_ResetCapacityToDefault()
    {
        if (_plainText_Capacity == _plainTextDefaultCapacity) return;
        _plainText = new char[_plainTextDefaultCapacity];
        _plainText_Capacity = _plainTextDefaultCapacity;
    }
}
