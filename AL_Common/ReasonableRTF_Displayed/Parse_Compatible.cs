#if !NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReasonableRTF.Enums;

namespace ReasonableRTF_Displayed;

public sealed partial class RRTF_RtfDisplayedReadmeParser
{
    private RtfError ParseRtf()
    {
        // Avoid bounds checks by passing a buffer reference everywhere. We do our own bounds checking.
        ref byte bufferRef = ref GetArrayDataReference(_buffer);
        ref bool isNonPlainTextCharRef = ref GetArrayDataReference(_isNonPlainText);
        ref bool isIgnoreCharRef = ref MemoryMarshal.GetReference(_isIgnoreChar);

        while (!_reachedEndOfStream)
        {
            while (_currentPos < _currentBufferChunkLength)
            {
                if (!_getLangs && _getColorTable && _foundColorTable) return RtfError.OK;

                char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

                // Ordered by most frequently appearing first
                switch (ch)
                {
                    case '\\':
                        RtfError ec = ParseKeyword(ref bufferRef);
                        if (ec != RtfError.OK) return ec;
                        break;
                    case '{':
                        GroupStack_DeepCopyToNext();
                        break;
                    case '}':
                        if (_groupStackTopIndex == 0) return RtfError.StackUnderflow;
                        --_groupStackTopIndex;
                        if (_groupStackTopIndex == 0) return RtfError.OK;
                        break;
                    default:
                    {
                        if (!Unsafe.AddByteOffset(ref isIgnoreCharRef, (nint)ch) &&
                            !GroupStack_CurrentSkipDest &&
                            !GroupStack_CurrentPropertyHidden)
                        {
                            // No measurable perf loss from this, and it lets us avoid duplicating the loop body.
                            char currentChar = (char)(_currentPos < _currentBufferChunkLength
                                ? GetByteAtPos(ref bufferRef, _currentPos)
                                : GetByte(_currentPos));

                            if (Unsafe.AddByteOffset(ref isNonPlainTextCharRef, (nint)currentChar))
                            {
                                SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
                                if (symbolFont > SymbolFont.None)
                                {
                                    AddCharFromConversionList((byte)ch, _symbolFontTables[(int)symbolFont]);
                                }
                                else
                                {
                                    PlainText_Add(ch);
                                }
                            }
                            else
                            {
                                HandlePlainTextRun(ref bufferRef);
                            }
                        }
                        break;
                    }
                }
            }

            if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
        }

        return _groupStackTopIndex > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    private void HandlePlainTextRun(ref byte bufferRef)
    {
        _currentPos--;

        SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
        if (symbolFont > SymbolFont.None)
        {
            uint[] table = _symbolFontTables[(int)symbolFont];
            while (!_reachedEndOfStream)
            {
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        AddCharFromConversionList((byte)ch, table);
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }

                if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
            }
        }
        else
        {
            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                bool finishedOnNonPlainTextChar = SIMD_CopyPlainText(ref bufferRef);

                if (finishedOnNonPlainTextChar)
                {
                    return;
                }
            }

            if (_currentPos < (_currentBufferChunkLength - 1) - _plainTextRunFastPathAmountBackFromBufferEnd &&
                _plainText_Count < (_plainText_Capacity - _plainTextRunFastPathAmountBackFromBufferEnd) - 1)
            {
                char[] plainText = _plainText;
                for (int i = 0; i < _plainTextRunFastPathAmountBackFromBufferEnd; i++)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        plainText[_plainText_Count++] = ch;
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }
            }

            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                // Break out of the scalar loop at the buffer boundary, so that if the plaintext run continues
                // after the next buffer load, we'll be able to jump back into a SIMD parse.
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!_isNonPlainText[(byte)ch])
                    {
                        PlainText_Add(ch);
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }
            }
            else
            {
                while (!_reachedEndOfStream)
                {
                    while (_currentPos < _currentBufferChunkLength)
                    {
                        char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        if (!_isNonPlainText[(byte)ch])
                        {
                            PlainText_Add(ch);
                        }
                        else
                        {
                            _currentPos--;
                            return;
                        }
                    }

                    if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
                }
            }
        }
    }

    private void HandleHexRun(ref byte bufferRef)
    {
        _hexBuffer_Count = 0;

        ushort codePage = GetCurrentCodePage();

        byte byte1;
        byte byte2;

        if (_currentPos < _currentBufferChunkLength - 1)
        {
            byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
            byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
        }
        else
        {
            byte1 = GetByte(IncrementCurrentPos());
            byte2 = GetByte(IncrementCurrentPos());
        }

        if (codePage == 42)
        {
            SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
            if (symbolFont == SymbolFont.None) symbolFont = SymbolFont.Symbol;
            uint[] symbolFontTable = _symbolFontTables[(int)symbolFont];

            AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);
                    }
                    else
                    {
                        _currentPos -= 2;
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    return;
                }
            }

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);
                    }
                    else
                    {
                        _currentPos -= 2;
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    return;
                }
            }
        }
        else if (_sbcsToUtf16Dict.TryGetValue(codePage, out char[]? sbcsMappingTable))
        {
            AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);
                    }
                    else
                    {
                        _currentPos -= 2;
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    return;
                }
            }

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);
                    }
                    else
                    {
                        _currentPos -= 2;
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    return;
                }
            }
        }
        else
        {
            AddByteToHexBuffer(byte1, byte2);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddByteToHexBuffer(byte1, byte2);
                    }
                    else
                    {
                        _currentPos -= 2;
                        AddHexBuffer(codePage);
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    AddHexBuffer(codePage);
                    return;
                }
            }

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddByteToHexBuffer(byte1, byte2);
                    }
                    else
                    {
                        _currentPos -= 2;
                        AddHexBuffer(codePage);
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    AddHexBuffer(codePage);
                    return;
                }
            }
        }
    }
}
#endif
