using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;

namespace ReasonableRTF_Displayed;

public sealed partial class RRTF_RtfDisplayedReadmeParser
{
    private readonly Vector<byte>[] _symbolFontNameVectors = new Vector<byte>[_symbolArraysLength];

    private void InitSymbolFontNameVectors()
    {
        Span<byte> bytes = stackalloc byte[Vector<byte>.Count];

        for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
        {
            _symbolFontNameVectors[i] = GetZeroPaddedVector(bytes, _symbolFontCharsArrays[i]);
        }

        return;

        static Vector<byte> GetZeroPaddedVector(Span<byte> bytes, byte[] name)
        {
            if (name.Length > Vector<byte>.Count)
            {
                return Vector<byte>.Zero;
            }

            bytes.Clear();
            name.CopyTo(bytes);

            return Unsafe.ReadUnaligned<Vector<byte>>(ref MemoryMarshal.GetReference(bytes));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SymbolFont SIMD_TryGetFontName(ref byte bufferRef, char ch)
    {
        if (_currentPos < _currentBufferChunkLength - (Vector<byte>.Count + 1))
        {
            _currentPos--;

            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, _currentPos);
            Vector<byte> vector = Unsafe.ReadUnaligned<Vector<byte>>(ref searchSpace);
            Vector<byte> equalsTerminatingChar =
                Vector.Equals(_zeroVector, vector) |
                Vector.Equals(_lfVector, vector) |
                Vector.Equals(_crVector, vector) |
                Vector.Equals(_backslashVector, vector) |
                Vector.Equals(_openBraceVector, vector) |
                Vector.Equals(_closingBraceVector, vector) |
                Vector.Equals(_semicolonVector, vector);

            if (equalsTerminatingChar != Vector<byte>.Zero)
            {
                int terminatingCharIndex = LocateFirstFoundByte(equalsTerminatingChar);
                ch = (char)Unsafe.AddByteOffset(ref searchSpace, (nint)terminatingCharIndex);

                if (EarlyOut(terminatingCharIndex))
                {
                    _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                    return SymbolFont.None;
                }

                Vector<byte> maskVec = Vector.GreaterThan(new Vector<byte>((byte)terminatingCharIndex), _indexVec);
                Vector<byte> fontName = Vector.BitwiseAnd(vector, maskVec);

                _currentPos += ch == ';' ? terminatingCharIndex + 1 : terminatingCharIndex;
                return TryFindSymbolFont(fontName, _symbolFontNameVectors);
            }
            else
            {
                ch = (char)GetByteAtPos(ref bufferRef, _currentPos + Vector<byte>.Count);
                if (ch == ';' || _isNonPlainText[(byte)ch])
                {
                    if (EarlyOut(Vector<byte>.Count))
                    {
                        _currentPos += ch == ';' ? Vector<byte>.Count + 1 : Vector<byte>.Count;
                        return SymbolFont.None;
                    }

                    _currentPos += ch == ';' ? Vector<byte>.Count + 1 : Vector<byte>.Count;
                    return TryFindSymbolFont(vector, _symbolFontNameVectors);
                }
                else
                {
                    _currentPos += Vector<byte>.Count + 1;
                    if (Vector<byte>.Count < _maxSupportedSymbolFontNameLength)
                    {
                        vector.CopyTo(_symbolFontNameBuffer);
                        return GetSymbolFont_Scalar(ref bufferRef, ch, Vector<byte>.Count);
                    }
                    else
                    {
                        return SymbolFont.None;
                    }
                }
            }
        }
        else
        {
            return GetSymbolFont_Scalar(ref bufferRef, ch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool EarlyOut(int index)
        {
            return !index.IsBetween(_minSupportedSymbolFontNameLength, _maxSupportedSymbolFontNameLength) ||
                   !_symbolFontNameLengths[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SymbolFont TryFindSymbolFont(Vector<byte> fontName, Vector<byte>[] symbolFontNameVectors)
        {
            for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
            {
                if (fontName == symbolFontNameVectors[i])
                {
                    return (SymbolFont)i;
                }
            }

            return SymbolFont.None;
        }
    }
}
