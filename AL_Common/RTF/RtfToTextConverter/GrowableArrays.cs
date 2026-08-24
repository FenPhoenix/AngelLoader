using System;
using System.Runtime.CompilerServices;

namespace AL_Common.RTF;

public sealed partial class RtfToTextConverter
{
    #region Hex buffer

    private byte[] _hexBuffer = new byte[_internalBufferDefaultCapacity];
    private int _hexBuffer_Count;
    private int _hexBuffer_Capacity = _internalBufferDefaultCapacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HexBuffer_Add(byte item)
    {
        if (_hexBuffer_Count == _hexBuffer_Capacity)
        {
            HexBuffer_Grow(_hexBuffer_Count + 1);
        }
        _hexBuffer[_hexBuffer_Count++] = item;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void HexBuffer_Grow(int min)
    {
        int newCapacity = _hexBuffer_Capacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < min) newCapacity = min;

        byte[] newArray = new byte[newCapacity];
        if (_hexBuffer_Count > 0) Array.Copy(_hexBuffer, 0, newArray, 0, _hexBuffer_Count);
        _hexBuffer = newArray;
        _hexBuffer_Capacity = newCapacity;
    }

    private void HexBuffer_ResetCapacityToDefault()
    {
        if (_hexBuffer_Capacity == _internalBufferDefaultCapacity) return;
        _hexBuffer = new byte[_internalBufferDefaultCapacity];
        _hexBuffer_Capacity = _internalBufferDefaultCapacity;
    }

    #endregion

    #region Plain text buffer

    // Highest measured was 56192
    private const int _plainTextDefaultCapacity = ByteSize.KB * 60;

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

    #endregion

    #region Unicode buffer

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

    #endregion

    #region Font name buffer

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

    #endregion
}
