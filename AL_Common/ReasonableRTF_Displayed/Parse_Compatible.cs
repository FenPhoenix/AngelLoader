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

                        if (!Unsafe.AddByteOffset(ref isNonPlainTextCharRef, (nint)currentChar))
                        {
                            HandlePlainTextRun(ref bufferRef);
                        }
                    }
                    break;
                }
            }
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
            while (_currentPos < _currentBufferChunkLength)
            {
                char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (_isNonPlainText[(byte)ch])
                {
                    _currentPos--;
                    return;
                }
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

            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                // Break out of the scalar loop at the buffer boundary, so that if the plaintext run continues
                // after the next buffer load, we'll be able to jump back into a SIMD parse.
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (_isNonPlainText[(byte)ch])
                    {
                        _currentPos--;
                        return;
                    }
                }
            }
            else
            {
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (_isNonPlainText[(byte)ch])
                    {
                        _currentPos--;
                        return;
                    }
                }
            }
        }
    }
}
