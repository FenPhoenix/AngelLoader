// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
@NET5: EscapeUriString() is marked obsolete and "can corrupt the Uri string in some cases".
We're supposed to use EscapeDataString(), but that's not a drop-in replacement, it behaves differently.
But this has always worked for us and who cares if the Uri string gets corrupted anyway, all it would do is make
a web search fail, and that's never actually happened anyway. So let's just use our own copy for when they
inevitably remove the built-in one or make it a compiler error or whatever...
*/

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using AL_Common;

namespace AngelLoader;

internal static class EscapeUrl
{
    public static string EscapeUriString(string stringToEscape) =>
        EscapeString(stringToEscape, checkExistingEscaped: false, UnreservedReserved);

    // true for all ASCII letters and digits, as well as the RFC3986 reserved characters, unreserved characters, and hash
    private static readonly SearchValues<char> UnreservedReserved =
        SearchValues.Create("!#$&'()*+,-./0123456789:;=?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz~");

    private const int StackallocThreshold = 512;

    private static string EscapeString(string stringToEscape, bool checkExistingEscaped, SearchValues<char> noEscape)
    {
        ArgumentNullException.ThrowIfNull(stringToEscape);

        return EscapeString(stringToEscape, checkExistingEscaped, noEscape, stringToEscape);
    }

    private static string EscapeString(ReadOnlySpan<char> charsToEscape, bool checkExistingEscaped, SearchValues<char> noEscape, string? backingString)
    {
        Debug.Assert(!noEscape.Contains('%'), "Need to treat % specially; it should be part of any escaped set");
        Debug.Assert(backingString is null || backingString.Length == charsToEscape.Length);

        int indexOfFirstToEscape = charsToEscape.IndexOfAnyExcept(noEscape);
        if (indexOfFirstToEscape < 0)
        {
            // Nothing to escape, just return the original value.
            return backingString ?? charsToEscape.ToString();
        }

        // Otherwise, create a ValueStringBuilder to store the escaped data into,
        // escape the rest, and concat the result with the characters we skipped above.
        var vsb = new AL_Common.ValueStringBuilder(stackalloc char[StackallocThreshold]);

        // We may throw for very large inputs (when growing the ValueStringBuilder).
        vsb.EnsureCapacity(charsToEscape.Length);

        EscapeStringToBuilder(charsToEscape.Slice(indexOfFirstToEscape), ref vsb, noEscape, checkExistingEscaped);

        string result = string.Concat(charsToEscape.Slice(0, indexOfFirstToEscape), vsb.AsSpan());
        vsb.Dispose();
        return result;
    }

    private enum Casing : uint
    {
        // Output [ '0' .. '9' ] and [ 'A' .. 'F' ].
        Upper = 0,

        // Output [ '0' .. '9' ] and [ 'a' .. 'f' ].
        // This works because values in the range [ 0x30 .. 0x39 ] ([ '0' .. '9' ])
        // already have the 0x20 bit set, so ORing them with 0x20 is a no-op,
        // while outputs in the range [ 0x41 .. 0x46 ] ([ 'A' .. 'F' ])
        // don't have the 0x20 bit set, so ORing them maps to
        // [ 0x61 .. 0x66 ] ([ 'a' .. 'f' ]), which is what we want.
        Lower = 0x2020U,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToCharsBuffer(byte value, Span<char> buffer, int startingIndex = 0, Casing casing = Casing.Upper)
    {
        uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
        uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U) | (uint)casing;

        buffer[startingIndex + 1] = (char)(packedResult & 0xFF);
        buffer[startingIndex] = (char)(packedResult >> 8);
    }

    private static void PercentEncodeByte(byte b, ref ValueStringBuilder to)
    {
        to.Append('%');
        ToCharsBuffer(b, to.AppendSpan(2), 0, Casing.Upper);
    }

    private static void EscapeStringToBuilder(
        scoped ReadOnlySpan<char> stringToEscape, ref ValueStringBuilder vsb,
        SearchValues<char> noEscape, bool checkExistingEscaped)
    {
        Debug.Assert(!stringToEscape.IsEmpty && !noEscape.Contains(stringToEscape[0]));

        // Allocate enough stack space to hold any Rune's UTF8 encoding.
        Span<byte> utf8Bytes = stackalloc byte[4];

        while (!stringToEscape.IsEmpty)
        {
            char c = stringToEscape[0];

            if (!char.IsAscii(c))
            {
                if (Rune.DecodeFromUtf16(stringToEscape, out Rune r, out int charsConsumed) != OperationStatus.Done)
                {
                    r = Rune.ReplacementChar;
                }

                Debug.Assert(stringToEscape.EnumerateRunes() is { } e && e.MoveNext() && e.Current == r);
                Debug.Assert(charsConsumed is 1 or 2);

                stringToEscape = stringToEscape.Slice(charsConsumed);

                // The rune is non-ASCII, so encode it as UTF8, and escape each UTF8 byte.
                r.TryEncodeToUtf8(utf8Bytes, out int bytesWritten);
                foreach (byte b in utf8Bytes.Slice(0, bytesWritten))
                {
                    PercentEncodeByte(b, ref vsb);
                }

                continue;
            }

            if (!noEscape.Contains(c))
            {
                // If we're checking for existing escape sequences, then if this is the beginning of
                // one, check the next two characters in the sequence.
                if (c == '%' && checkExistingEscaped)
                {
                    // If the next two characters are valid escaped ASCII, then just output them as-is.
                    if (stringToEscape.Length > 2 && char.IsAsciiHexDigit(stringToEscape[1]) && char.IsAsciiHexDigit(stringToEscape[2]))
                    {
                        vsb.Append('%');
                        vsb.Append(stringToEscape[1]);
                        vsb.Append(stringToEscape[2]);
                        stringToEscape = stringToEscape.Slice(3);
                        continue;
                    }
                }

                PercentEncodeByte((byte)c, ref vsb);
                stringToEscape = stringToEscape.Slice(1);
                continue;
            }

            // We have a character we don't want to escape. It's likely there are more, do a vectorized search.
            int charsToCopy = stringToEscape.IndexOfAnyExcept(noEscape);
            if (charsToCopy < 0)
            {
                charsToCopy = stringToEscape.Length;
            }
            Debug.Assert(charsToCopy > 0);

            vsb.Append(stringToEscape.Slice(0, charsToCopy));
            stringToEscape = stringToEscape.Slice(charsToCopy);
        }
    }
}
