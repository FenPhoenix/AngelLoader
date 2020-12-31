//#define CROSS_PLATFORM

/*
Perf log:

             FMInfoGen | RTF_ToPlainTextTest
2020-08-24:  179MB/s     254MB/s
2020-08-20:  157MB/s     211MB/s
2020-08-19:  88.2 MB/s   97MB/s

---

Note to self:
RTFs in 1098 set, base dirs only:
count: 280
total size: 88,714,908
84.6 MB

---

This is a fast, no-frills, platform-agnostic RTF-to-text converter. It can be used in place of RichTextBox when
you simply want to convert RTF to plaintext without being tied to Windows.

The goals of this RTF-to-text converter are:
1. It should be platform-agnostic and have no dependencies on Windows.
2. It should correctly preserve all characters - ASCII or otherwise - as they would appear in rich text.
3. It should work with unseekable streams, that is, streams that can only be read forward.
4. It should be faster and use less memory than the RichTextBox-with-prepass-for-image-strip method.
 
To that end:
1. We use the System.Text.Encoding.CodePages package to get all Windows-supported codepages on all platforms
   (only if CROSS_PLATFORM is defined). We don't use RichTextBox or RichEdit.
2. We go to great lengths to detect character encoding accurately, even converting symbol fonts to their Unicode
   equivalents. We surpass even RichEdit in this respect.
3. We support forward-only streams so we can read straight out of a compressed zip file entry with no copying.
4. We're mindful of our allocations and minimize them wherever possible. We do absolutely nothing other than convert
   to plaintext, simply skipping over anything that can't be converted. We thereby avoid the substantial overhead
   of a full parser.

-Note that we don't check for {\rtf1 at the start of the file to validate, as in FMScanner that will have been
 done already.

TODO (RtfToTextConverter):
Notes and miscellaneous:
-Hex that combines into an actual valid character: \'81\'63
-Tiger face (>2 byte Unicode test): \u-9169?\u-10179?

Perf: (RtfToTextConverter)
-We could collapse fonts if we find multiple ones with the same name and charset but different numbers.
 I mean it looks like we're plenty fast and memory-reasonable without doing so, but you know, idea.

Memory:
-n/a at the moment

Other:
-Really implement a proper Peek() function for the stream. I know it's possible, and having to use UnGetChar() is
 getting really nasty and error-prone.
-Consider being extremely forgiving about errors - we want as much plaintext as we can get out of a file, and
 even imperfect text may be useful. FMScanner extracts a relatively very small portion of text from the file,
 so statistically it's likely it may not even hit broken text even if it exists.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace FMScanner
{
    public sealed class RtfToTextConverter
    {
        #region Constants

        private const int _windows1252 = 1252;
        private const int _shiftJisWin = 932;
        private const char _unicodeUnknown_Char = '\u25A1';
        private const string _unicodeUnknown_String = "\u25A1";
        private const int _maxScopes = 100;

        #endregion

        #region Classes

        // How many times have you thought, "Gee, I wish I could just reach in and grab that backing array from
        // that List, instead of taking the senseless performance hit of having it copied to a newly allocated
        // array all the time in a ToArray() call"? Hooray!
        /// <summary>
        /// Because this list exposes its internal array and also doesn't clear said array on <see cref="ClearFast"/>,
        /// it must be used with care.
        /// <para>
        /// -Only use this with value types. Reference types will be left hanging around in the array.
        /// </para>
        /// <para>
        /// -The internal array is there so you can get at it without incurring an allocation+copy.
        ///  It can very easily become desynced with the <see cref="ListFast{T}"/> if you modify it.
        /// </para>
        /// <para>
        /// -Only use the internal array in conjunction with the <see cref="Count"/> property.
        ///  Using the <see cref="ItemsArray"/>.Length value will lead to catastrophe.
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class ListFast<T>
        {
            internal T[] ItemsArray;
            private int _itemsArrayLength;

#pragma warning disable IDE0032
            private int _size;

            [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
            internal int Count => _size;
#pragma warning restore IDE0032

            internal ListFast(int capacity)
            {
                ItemsArray = new T[capacity];
                _itemsArrayLength = capacity;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Add(T item)
            {
                if (_size == _itemsArrayLength) EnsureCapacity(_size + 1);
                ItemsArray[_size++] = item;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void AddFast(T item) => ItemsArray[_size++] = item;

            /*
            Honestly, for fixed-size value types, doing an Array.Clear() is completely unnecessary. For reference
            types, you definitely want to clear it to get rid of all the references, but for ints or chars etc.,
            all a clear does is set a bunch of fixed-width values to other fixed-width values. You don't save
            space and you don't get rid of loose references, all you do is waste an alarming amount of time. We
            drop fully 200ms from the Unicode parser just by using the fast clear!
            */
            /// <summary>
            /// Just sets <see cref="Count"/> to 0. Doesn't zero out the array or do anything else whatsoever.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ClearFast() => _size = 0;

            [PublicAPI]
            internal int Capacity
            {
                get => _itemsArrayLength;
                set
                {
                    if (value == _itemsArrayLength) return;
                    if (value > 0)
                    {
                        T[] objArray = new T[value];
                        if (_size > 0) Array.Copy(ItemsArray, 0, objArray, 0, _size);
                        ItemsArray = objArray;
                        _itemsArrayLength = value;
                    }
                    else
                    {
                        ItemsArray = Array.Empty<T>();
                        _itemsArrayLength = 0;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void EnsureCapacity(int min)
            {
                if (_itemsArrayLength >= min) return;
                int num = _itemsArrayLength == 0 ? 4 : _itemsArrayLength * 2;
                if ((uint)num > 2146435071U) num = 2146435071;
                if (num < min) num = min;
                Capacity = num;
            }
        }

        private sealed class DictWithTopItem<TKey, TValue> : Dictionary<TKey, TValue>
        {
            internal TValue Top = default!;

            internal DictWithTopItem(int capacity) : base(capacity) { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal new void Add(TKey key, TValue value)
            {
                Top = value;
                base[key] = value;
            }
        }

        private sealed class SymbolDict
        {
            /* ANSI-C code produced by gperf version 3.1 */
            /* Command-line: gperf -t 'C:\\keywords.txt'  */
            /* Computed positions: -k'1-3,$' */

            // Then ported to C# semi-manually. Woe.

            // Two ways we gain perf with this generated perfect hash thing over a standard Dictionary:
            // First, it's just faster to begin with, and second, it lets us finally ditch the StringBuilders and
            // ToString()s and just pass in simple char arrays. We are now unmeasurable. Hallelujah!

            //const int TOTAL_KEYWORDS = 72;
            private const int MIN_WORD_LENGTH = 1;
            private const int MAX_WORD_LENGTH = 10;
            //const int MIN_HASH_VALUE = 1;
            private const int MAX_HASH_VALUE = 241;
            /* maximum key range = 241, duplicates = 0 */

            private readonly byte[] asso_values =
            {
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 50,
                242, 242, 45, 242, 242, 242, 242, 242, 242, 40,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 35, 242, 242, 242, 242, 5, 0, 5,
                95, 20, 5, 45, 55, 0, 242, 0, 15, 35,
                0, 5, 10, 0, 20, 0, 0, 120, 0, 242,
                25, 5, 242, 30, 242, 25, 15, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
                242, 242, 242, 242, 242, 242
            };

            // For emspace, enspace, qmspace, ~
            // Just convert these to regular spaces because we're just trying to scan for strings in readmes
            // without weird crap tripping us up

            // For emdash, endash, lquote, rquote, ldblquote, rdblquote
            // TODO: Maybe just convert these all to ASCII equivalents as well?

            // For cs, ds, ts
            // Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.

            // For listtext, pntext
            // Ignore list item bullets and numeric prefixes etc. We don't need them.

            private readonly Symbol?[] _symbolTable =
            {
                null,
// Entry 16
                // \v to make all plain text hidden (not output to the conversion stream)}, \v0 to make it shown again
                new Symbol("v", 1, false, KeywordType.Property, (int)Property.Hidden),
// Entry 37
                new Symbol("ts", 0, false, KeywordType.Destination, (int)DestinationType.IgnoreButDontSkipGroup),
// Entry 32
                new Symbol("bin", 0, false, KeywordType.Special, (int)SpecialType.Bin),
                null, null, null,
// Entry 35
                new Symbol("cs", 0, false, KeywordType.Destination, (int)DestinationType.IgnoreButDontSkipGroup),
// Entry 20
                new Symbol("tab", 0, false, KeywordType.Character, '\t'),
// Entry 3
                // The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
                // "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.
                new Symbol("ansi", 1252, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 51
                new Symbol("ftncn", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 10
                new Symbol("f", 0, false, KeywordType.Property, (int)Property.FontNum),
// Entry 68
                new Symbol("tc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 58
                new Symbol("info", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 66
                new Symbol("stylesheet", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 39
                new Symbol("pntext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 53
                new Symbol("ftnsepc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 61
                new Symbol("pict", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 52
                new Symbol("ftnsep", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 4
                new Symbol("pc", 437, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 38
                new Symbol("listtext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 69
                new Symbol("title", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 47
                new Symbol("footerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 6
                new Symbol("pca", 850, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
                null, null,
// Entry 25
                new Symbol("~", 0, false, KeywordType.Character, ' '),
// Entry 9
                new Symbol("fonttbl", 0, false, KeywordType.Special, (int)SpecialType.FontTable),
// Entry 59
                new Symbol("keywords", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null,
// Entry 48
                new Symbol("footerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 19
                new Symbol("softline", 0, false, KeywordType.Character, '\n'),
// Entry 18
                new Symbol("line", 0, false, KeywordType.Character, '\n'),
                null,
// Entry 46
                new Symbol("footer", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 49
                new Symbol("footerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 50
                new Symbol("footnote", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null,
// Entry 23
                new Symbol("enspace", 0, false, KeywordType.Character, ' '),
// Entry 42
                new Symbol("colortbl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null,
// Entry 73
                new Symbol("}", 0, false, KeywordType.Character, '}'),
// Entry 43
                new Symbol("comment", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 5
                // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
                // assume 10000 ("Mac Roman")
                new Symbol("mac", 10000, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
                null, null, null,
// Entry 7
                new Symbol("ansicpg", 1252, false, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 17
                new Symbol("par", 0, false, KeywordType.Character, '\n'),
                null, null,
// Entry 72
                new Symbol("{", 0, false, KeywordType.Character, '{'),
// Entry 24
                new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
// Entry 60
                new Symbol("operator", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null,
// Entry 71
                new Symbol("xe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 70
                new Symbol("txe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null,
// Entry 74
                new Symbol("\\", 0, false, KeywordType.Character, '\\'),
// Entry 62
                new Symbol("printim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 11
                new Symbol("fcharset", -1, false, KeywordType.Special, (int)SpecialType.Charset),
                null, null, null, null,
// Entry 63
                new Symbol("private1", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null,
// Entry 64
                new Symbol("revtim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 22
                new Symbol("emspace", 0, false, KeywordType.Character, ' '),
                null, null, null, null,
// Entry 44
                new Symbol("creatim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 65
                new Symbol("rxe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null,
// Entry 33
                new Symbol("*", 0, false, KeywordType.Special, (int)SpecialType.SkipDest),
// Entry 55
                new Symbol("headerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null, null,
// Entry 36
                new Symbol("ds", 0, false, KeywordType.Destination, (int)DestinationType.IgnoreButDontSkipGroup),
                null, null, null,
// Entry 14
                new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar),
// Entry 56
                new Symbol("headerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null,
// Entry 54
                new Symbol("header", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 57
                new Symbol("headerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 12
                new Symbol("cpg", -1, false, KeywordType.Special, (int)SpecialType.CodePage),
                null, null, null, null, null, null, null, null, null,
                null, null, null, null,
// Entry 34
                // We need to do stuff with this (SYMBOL instruction)
                new Symbol("fldinst", 0, false, KeywordType.Destination, (int)DestinationType.FieldInstruction),
                null, null, null, null,
// Entry 67
                new Symbol("subject", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null,
// Entry 8
                new Symbol("deff", 0, false, KeywordType.Special, (int)SpecialType.DefaultFont),
                null, null,
// Entry 13
                new Symbol("uc", 1, false, KeywordType.Property, (int)Property.UnicodeCharSkipCount),
                null, null, null, null, null, null,
// Entry 30
                new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
                null,
// Entry 21
                new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
                null, null,
// Entry 31
                new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),
                null, null,
// Entry 45
                new Symbol("doccomm", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null,
// Entry 40
                new Symbol("author", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null, null, null, null, null, null, null,
// Entry 28
                new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
                null, null, null, null,
// Entry 29
                new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
                null, null, null, null,
// Entry 41
                new Symbol("buptim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
                null, null, null, null,
// Entry 27
                new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null,
// Entry 26
                new Symbol("emdash", 0, false, KeywordType.Character, '\x2014'),
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null,
                null, null,
// Entry 15
                new Symbol("u", 0, false, KeywordType.Special, (int)SpecialType.UnicodeChar)
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint Hash(ListFast<char> str, int len)
            {
                uint hval = (uint)len;

                // Original C code does a stupid thing where it puts default at the top and falls through and junk,
                // but we can't do that in C#, so have something clearer/clunkier
                switch (len)
                {
                    case 1:
                        hval += asso_values[str.ItemsArray[0]];
                        break;
                    case 2:
                        hval += asso_values[str.ItemsArray[1]];
                        hval += asso_values[str.ItemsArray[0]];
                        break;
                    default:
                        hval += asso_values[str.ItemsArray[2]];
                        hval += asso_values[str.ItemsArray[1]];
                        hval += asso_values[str.ItemsArray[0]];
                        break;
                }
                return hval + asso_values[str.ItemsArray[len - 1]];
            }

            // Not a duplicate: this one needs to take a string instead of char[]...
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool SeqEqual(ListFast<char> seq1, string seq2)
            {
                int seq1Count = seq1.Count;
                if (seq1Count != seq2.Length) return false;

                for (int ci = 0; ci < seq1Count; ci++)
                {
                    if (seq1.ItemsArray[ci] != seq2[ci]) return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool TryGetValue(ListFast<char> str, [NotNullWhen(true)] out Symbol? result)
            {
                int len = str.Count;
                if (len <= MAX_WORD_LENGTH && len >= MIN_WORD_LENGTH)
                {
                    uint key = Hash(str, len);

                    if (key <= MAX_HASH_VALUE)
                    {
                        Symbol? symbol = _symbolTable[key];
                        if (symbol == null)
                        {
                            result = null;
                            return false;
                        }

                        if (SeqEqual(str, symbol.Keyword))
                        {
                            result = symbol;
                            return true;
                        }
                    }
                }

                result = null;
                return false;
            }
        }

        private sealed class RTFStream
        {
            #region Private fields

            private Stream _stream = null!;
            // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
            // length as a param and store it.
            /// <summary>
            /// Do not modify!
            /// </summary>
            internal long Length;

            /// <summary>
            /// Do not modify!
            /// </summary>
            internal long CurrentPos;

            private const int _bufferLen = 81920;
            private readonly byte[] _buffer = new byte[_bufferLen];
            // Start it ready to roll over to 0 so we don't need extra logic for the first get
            private int _bufferPos = _bufferLen - 1;

            /*
            We use this as a "seek-back" buffer for when we want to move back in the stream. We put chars back
            "into the stream", but they actually go in here and then when we go to read, we read from this first
            and so on until it's empty, then go back to reading from the main stream again. In this way, we
            support a rudimentary form of peek-and-rewind without ever actually seeking backwards in the stream.
            This is required to support zip entry streams which are unseekable. If we required a seekable stream,
            we would have to copy the entire, potentially very large, zip entry stream to memory first and then
            read it, which is possibly unnecessarily memory-hungry.

            2020-08-15:
            We now have a buffered stream so in theory we could check if we're > 0 in the buffer and just actually
            rewind if we are, but our seek-back buffer is fast enough already so we're just keeping that for now.
            */
            private readonly Stack<char> _unGetBuffer = new Stack<char>(5);
            private bool _unGetBufferEmpty = true;

            #endregion

            // PERF: Everything in here is inlined. This gives a shockingly massive speedup.

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Reset(Stream stream, long streamLength)
            {
                _stream = stream;
                Length = streamLength;

                CurrentPos = 0;

                // Don't clear the buffer; we don't need to and it wastes time
                _bufferPos = _bufferLen - 1;

                _unGetBuffer.Clear();
                _unGetBufferEmpty = true;
            }

            /// <summary>
            /// Puts a char back into the stream and decrements the read position. Actually doesn't really do that
            /// but uses an internal seek-back buffer to allow it work with forward-only streams. But don't worry
            /// about that. Just use it as normal.
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void UnGetChar(char c)
            {
                if (CurrentPos < 0) return;

                _unGetBuffer.Push(c);
                _unGetBufferEmpty = false;
                if (CurrentPos > 0) CurrentPos--;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private byte StreamReadByte()
            {
                _bufferPos++;
                if (_bufferPos == _bufferLen)
                {
                    _bufferPos = 0;
                    _stream.Read(_buffer, 0, _bufferLen);
                }
                return _buffer[_bufferPos];
            }

            /// <summary>
            /// Returns false if the end of the stream has been reached.
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool GetNextChar(out char ch)
            {
                if (CurrentPos == Length)
                {
                    ch = '\0';
                    return false;
                }

                if (!_unGetBufferEmpty)
                {
                    ch = _unGetBuffer.Pop();
                    _unGetBufferEmpty = _unGetBuffer.Count == 0;
                }
                else
                {
                    ch = (char)StreamReadByte();
                }
                CurrentPos++;

                return true;
            }

            /// <summary>
            /// For use in loops that already check the stream position against the end as a loop condition
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal char GetNextCharFast()
            {
                char ch;
                if (!_unGetBufferEmpty)
                {
                    ch = _unGetBuffer.Pop();
                    _unGetBufferEmpty = _unGetBuffer.Count == 0;
                }
                else
                {
                    ch = (char)StreamReadByte();
                }
                CurrentPos++;

                return ch;
            }
        }

        // Class, but only instantiated once and then just reset, so it's fine
        private sealed class Header
        {
            internal int CodePage;
            internal bool DefaultFontSet;
            internal int DefaultFontNum;

            internal Header() => Reset();

            internal void Reset()
            {
                CodePage = _windows1252;
                DefaultFontSet = false;
                DefaultFontNum = 0;
            }
        }

        private sealed class FontEntry
        {
            // Use only as many chars as we need - "Wingdings" is 9 chars and is the longest we need
            private const int NameMaxLength = 9;

            internal int? CodePage;

            // We need to store names in case we get codepage 42 nonsense, we need to know which font to translate
            // to Unicode (Wingdings, Webdings, or Symbol)
            internal readonly char[] Name = new char[NameMaxLength];
            internal int NameCharPos;

            internal void AppendNameChar(char c)
            {
                if (NameCharPos < NameMaxLength) Name[NameCharPos++] = c;
            }
        }

        // Current scope needs to be mutable, so it's a single-instance class
        private sealed class CurrentScope
        {
            internal RtfDestinationState RtfDestinationState;
            internal RtfInternalState RtfInternalState;
            internal bool InFontTable;
            internal SymbolFont SymbolFont;

            internal readonly int[] Properties = new int[_propertiesLen];

            internal void Reset()
            {
                RtfDestinationState = 0;
                RtfInternalState = 0;
                InFontTable = false;
                SymbolFont = SymbolFont.None;

                Properties[(int)Property.Hidden] = 0;
                Properties[(int)Property.UnicodeCharSkipCount] = 1;
                Properties[(int)Property.FontNum] = -1;
            }
        }

        private sealed class ScopeStack
        {
            private readonly Scope[] _scopesArray;
            internal int Count;

            internal ScopeStack()
            {
                _scopesArray = new Scope[_maxScopes];
                for (int i = 0; i < _maxScopes; i++)
                {
                    _scopesArray[i] = new Scope();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Push(CurrentScope currentScope)
            {
                Scope nextScope = _scopesArray[Count++];

                nextScope.RtfDestinationState = currentScope.RtfDestinationState;
                nextScope.RtfInternalState = currentScope.RtfInternalState;
                nextScope.InFontTable = currentScope.InFontTable;
                nextScope.SymbolFont = currentScope.SymbolFont;

                Array.Copy(currentScope.Properties, 0, nextScope.Properties, 0, _propertiesLen);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Scope Pop() => _scopesArray[--Count];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ClearFast() => Count = 0;
        }

        // Scopes on the stack need not be mutable, and making them structs is faster/smaller/better cache locality/less GC/whatever
        private sealed class Scope
        {
            internal RtfDestinationState RtfDestinationState;
            internal RtfInternalState RtfInternalState;
            internal bool InFontTable;
            internal SymbolFont SymbolFont;

            internal readonly int[] Properties = new int[_propertiesLen];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void DeepCopyTo(CurrentScope dest)
            {
                dest.RtfDestinationState = RtfDestinationState;
                dest.RtfInternalState = RtfInternalState;
                dest.InFontTable = InFontTable;
                dest.SymbolFont = SymbolFont;

                Array.Copy(Properties, 0, dest.Properties, 0, _propertiesLen);
            }
        }

        private sealed class Symbol
        {
            internal readonly string Keyword;
            internal readonly int DefaultParam;
            internal readonly bool UseDefaultParam;
            internal readonly KeywordType KeywordType;
            /// <summary>
            /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
            /// </summary>
            internal readonly int Index;

            public Symbol(string keyword, int defaultParam, bool useDefaultParam, KeywordType keywordType, int index)
            {
                Keyword = keyword;
                DefaultParam = defaultParam;
                UseDefaultParam = useDefaultParam;
                KeywordType = keywordType;
                Index = index;
            }
        }

        #endregion

        #region Enums

        private enum SymbolFont
        {
            None,
            Symbol,
            Wingdings,
            Webdings
        }

        private const int _propertiesLen = 3;
        private enum Property
        {
            Hidden,
            UnicodeCharSkipCount,
            FontNum
        }

        [PublicAPI]
        internal enum Error
        {
            /// <summary>
            /// No error.
            /// </summary>
            OK,
            /// <summary>
            /// Unmatched '}'.
            /// </summary>
            StackUnderflow,
            /// <summary>
            /// Too many subgroups (we cap it at 100).
            /// </summary>
            StackOverflow,
            /// <summary>
            /// RTF ended during an open group.
            /// </summary>
            UnmatchedBrace,
            /// <summary>
            /// Invalid hexadecimal character found while parsing \'hh character(s).
            /// </summary>
            InvalidHex,
            /// <summary>
            /// A \uN keyword's parameter was out of range.
            /// </summary>
            InvalidUnicode,
            /// <summary>
            /// A symbol table entry was malformed. Possibly one of its enum values was out of range.
            /// </summary>
            InvalidSymbolTableEntry,
            /// <summary>
            /// Unexpected end of file reached while reading RTF.
            /// </summary>
            EndOfFile,
            /// <summary>
            /// A keyword was found that exceeds the max keyword length.
            /// </summary>
            KeywordTooLong,
            /// <summary>
            /// A parameter was found that exceeds the max parameter length.
            /// </summary>
            ParameterTooLong
        }

        private enum RtfDestinationState
        {
            Normal,
            Skip
        }

        private enum RtfInternalState
        {
            Normal,
            Binary,
            HexEncodedChar
        }

        private enum SpecialType
        {
            HeaderCodePage,
            DefaultFont,
            FontTable,
            Charset,
            CodePage,
            UnicodeChar,
            HexEncodedChar,
            Bin,
            SkipDest
        }

        private enum DestinationType
        {
            FieldInstruction,
            IgnoreButDontSkipGroup,
            Skip
        }

        private enum KeywordType
        {
            Character,
            Property,
            Destination,
            Special
        }

        #endregion

        #region Tables

        #region Font to Unicode conversion tables

        private readonly Dictionary<int, int>
        _charSetToCodePage = new Dictionary<int, int>
        {
            { 0, _windows1252 },    // "ANSI" (1252)

            // TODO: Code page 0 ("Default") is variable... should we force it to 1252?
            // "The system default Windows ANSI code page" says the doc page.
            // Terrible. Fortunately only two known FMs define it in a font entry, and neither one actually uses
            // said font entry. Still, maybe this should be 1252 as well, since we're rolling dice anyway we may
            // as well go with the statistically likeliest?
            { 1, 0 },               // Default

            { 2, 42 },              // Symbol
            { 77, 10000 },          // Mac Roman
            { 78, 10001 },          // Mac Shift Jis
            { 79, 10003 },          // Mac Hangul
            { 80, 10008 },          // Mac GB2312
            { 81, 10002 },          // Mac Big5
            //82                    // Mac Johab (old)
            { 83, 10005 },          // Mac Hebrew
            { 84, 10004 },          // Mac Arabic
            { 85, 10006 },          // Mac Greek
            { 86, 10081 },          // Mac Turkish
            { 87, 10021 },          // Mac Thai
            { 88, 10029 },          // Mac East Europe
            { 89, 10007 },          // Mac Russian
            { 128, _shiftJisWin },  // Shift JIS (Windows-31J) (932)
            { 129, 949 },           // Hangul
            { 130, 1361 },          // Johab
            { 134, 936 },           // GB2312
            { 136, 950 },           // Big5
            { 161, 1253 },          // Greek
            { 162, 1254 },          // Turkish
            { 163, 1258 },          // Vietnamese
            { 177, 1255 },          // Hebrew
            { 178, 1256 },          // Arabic
            //179                   // Arabic Traditional (old)
            //180                   // Arabic user (old)
            //181                   // Hebrew user (old)
            { 186, 1257 },          // Baltic
            { 204, 1251 },          // Russian
            { 222, 874 },           // Thai
            { 238, 1250 },          // Eastern European
            { 254, 437 },           // PC 437
            { 255, 850 }            // OEM
        };

        /*
        Many RTF files put emoji-like glyphs into text not with a Unicode character, but by just putting in a
        regular-ass single-byte char and then setting the font to Wingdings or whatever. So the letter "J"
        would show as "☺" in the Wingdings font. If we want to support this lunacy, we need conversion tables
        from known fonts to their closest Unicode mappings. So here we have them.

        These arrays MUST be of length 224, with entries starting at the codepoint for 0x20 and ending at the
        codepoint for 0xFF. That way, they can be arrays instead of dictionaries, making us smaller and faster.
        */

        // ReSharper disable RedundantExplicitArraySize
        private readonly int[] _symbolFontToUnicode = new int[224]
        {
            ' ',
            0x0021,
            0x2200,
            0x0023,
            0x2203,
            0x0025,
            0x0026,
            0x220D,
            0x0028,
            0x0029,
            0x2217,
            0x002B,
            0x002C,
            0x2212,
            0x002E,
            0x002F,
            0x0030,
            0x0031,
            0x0032,
            0x0033,
            0x0034,
            0x0035,
            0x0036,
            0x0037,
            0x0038,
            0x0039,
            0x003A,
            0x003B,
            0x003C,
            0x003D,
            0x003E,
            0x003F,
            0x2245,
            0x0391,
            0x0392,
            0x03A7,
            0x0394,
            0x0395,
            0x03A6,
            0x0393,
            0x0397,
            0x0399,
            0x03D1,
            0x039A,
            0x039B,
            0x039C,
            0x039D,
            0x039F,
            0x03A0,
            0x0398,
            0x03A1,
            0x03A3,
            0x03A4,
            0x03A5,
            0x03C2,
            0x03A9,
            0x039E,
            0x03A8,
            0x0396,
            0x005B,
            0x2234,
            0x005D,
            0x22A5,
            0x005F,

            // Supposed to be " ‾" but closest Unicode char is "‾" (0x203E)
            0x203E,

            0x03B1,
            0x03B2,
            0x03C7,
            0x03B4,
            0x03B5,

            // Lowercase phi, but capital phi in Windows Symbol
            0x03C6,

            0x03B3,
            0x03B7,
            0x03B9,

            // Capital phi, but lowercase phi in Windows Symbol
            0x03D5,

            0x03BA,
            0x03BB,
            0x03BC,
            0x03BD,
            0x03BF,
            0x03C0,
            0x03B8,
            0x03C1,
            0x03C3,
            0x03C4,
            0x03C5,
            0x03D6,
            0x03C9,
            0x03BE,
            0x03C8,
            0x03B6,
            0x007B,
            0x007C,
            0x007D,
            0x223C,

            // 7F - 9F are undefined
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,

            0x20AC, // Euro sign, but undefined in Win10 Symbol font at least

            0x03D2,
            0x2032,
            0x2264,
            0x2044,
            0x221E,
            0x0192,
            0x2663,
            0x2666,
            0x2665,
            0x2660,
            0x2194,
            0x2190,
            0x2191,
            0x2192,
            0x2193,
            0x00B0,
            0x00B1,
            0x2033,
            0x2265,
            0x00D7,
            0x221D,
            0x2202,
            0x2022,
            0x00F7,
            0x2260,
            0x2261,
            0x2248,
            0x2026,
            0x23D0,
            0x23AF,
            0x21B5,
            0x2135,
            0x2111,
            0x211C,
            0x2118,
            0x2297,
            0x2295,
            0x2205,
            0x2229,
            0x222A,
            0x2283,
            0x2287,
            0x2284,
            0x2282,
            0x2286,
            0x2208,
            0x2209,
            0x2220,
            0x2207,

            // First set of (R), (TM), (C) (nominally serif)
            0x00AE,
            0x00A9,
            0x2122,

            0x220F,
            0x221A,
            0x22C5,
            0x00AC,
            0x2227,
            0x2228,
            0x21D4,
            0x21D0,
            0x21D1,
            0x21D2,
            0x21D3,
            0x25CA,
            0x2329,

            // Second set of (R), (TM), (C) (nominally sans-serif)
            0x00AE,
            0x00A9,
            0x2122,

            0x2211,
            0x239B,
            0x239C,
            0x239D,
            0x23A1,
            0x23A2,
            0x23A3,
            0x23A7,
            0x23A8,
            0x23A9,
            0x23AA,

            // Apple logo or unknown
            _unicodeUnknown_Char,

            0x232A,
            0x222B,
            0x2320,
            0x23AE,
            0x2321,
            0x239E,
            0x239F,
            0x23A0,
            0x23A4,
            0x23A5,
            0x23A6,
            0x23AB,
            0x23AC,
            0x23AD,

            // Undefined
            _unicodeUnknown_Char
        };

        private readonly int[] _wingdingsFontToUnicode = new int[224]
        {
            ' ',
            0x1F589,
            0x2702,
            0x2701,
            0x1F453,
            0x1F56D,
            0x1F56E,
            0x1F56F,
            0x1F57F,
            0x2706,
            0x1F582,
            0x1F583,
            0x1F4EA,
            0x1F4EB,
            0x1F4EC,
            0x1F4ED,
            0x1F5C0,
            0x1F5C1,
            0x1F5CE,
            0x1F5CF,
            0x1F5D0,
            0x1F5C4,
            0x231B,
            0x1F5AE,
            0x1F5B0,
            0x1F5B2,
            0x1F5B3,
            0x1F5B4,
            0x1F5AB,
            0x1F5AC,
            0x2707,
            0x270D,
            0x1F58E,
            0x270C,
            0x1F58F,
            0x1F44D,
            0x1F44E,
            0x261C,
            0x261E,
            0x261D,
            0x261F,
            0x1F590,
            0x263A,
            0x1F610,
            0x2639,
            0x1F4A3,
            0x1F571,
            0x1F3F3,
            0x1F3F1,
            0x2708,
            0x263C,
            0x1F322,
            0x2744,
            0x1F546,
            0x271E,
            0x1F548,
            0x2720,
            0x2721,
            0x262A,
            0x262F,
            0x1F549,
            0x2638,
            0x2648,
            0x2649,
            0x264A,
            0x264B,
            0x264C,
            0x264D,
            0x264E,
            0x264F,
            0x2650,
            0x2651,
            0x2652,
            0x2653,
            0x1F670,
            0x1F675,
            0x26AB,
            0x1F53E,
            0x25FC,
            0x1F78F,
            0x1F790,
            0x2751,
            0x2752,
            0x1F79F,
            0x29EB,
            0x25C6,
            0x2756,
            0x2B29,
            0x2327,
            0x2BB9,
            0x2318,
            0x1F3F5,
            0x1F3F6,
            0x1F676,
            0x1F677,
            0x25AF,
            0x1F10B,
            0x2780,
            0x2781,
            0x2782,
            0x2783,
            0x2784,
            0x2785,
            0x2786,
            0x2787,
            0x2788,
            0x2789,
            0x1F10C,
            0x278A,
            0x278B,
            0x278C,
            0x278D,
            0x278E,
            0x278F,
            0x2790,
            0x2791,
            0x2792,
            0x2793,
            0x1F662,
            0x1F660,
            0x1F661,
            0x1F663,
            0x1F65E,
            0x1F65C,
            0x1F65D,
            0x1F65F,
            0x2219,
            0x2022,
            0x2B1D,
            0x2B58,
            0x1F786,
            0x1F788,
            0x1F78A,
            0x1F78B,
            0x1F53F,
            0x25AA,
            0x1F78E,
            0x1F7C1,
            0x1F7C5,
            0x2605,
            0x1F7CB,
            0x1F7CF,
            0x1F7D3,
            0x1F7D1,
            0x2BD0,
            0x2316,
            0x2BCE,
            0x2BCF,
            0x2BD1,
            0x272A,
            0x2730,
            0x1F550,
            0x1F551,
            0x1F552,
            0x1F553,
            0x1F554,
            0x1F555,
            0x1F556,
            0x1F557,
            0x1F558,
            0x1F559,
            0x1F55A,
            0x1F55B,
            0x2BB0,
            0x2BB1,
            0x2BB2,
            0x2BB3,
            0x2BB4,
            0x2BB5,
            0x2BB6,
            0x2BB7,
            0x1F66A,
            0x1F66B,
            0x1F655,
            0x1F654,
            0x1F657,
            0x1F656,
            0x1F650,
            0x1F651,
            0x1F652,
            0x1F653,
            0x232B,
            0x2326,
            0x2B98,
            0x2B9A,
            0x2B99,
            0x2B9B,
            0x2B88,
            0x2B8A,
            0x2B89,
            0x2B8B,
            0x1F868,
            0x1F86A,
            0x1F869,
            0x1F86B,
            0x1F86C,
            0x1F86D,
            0x1F86F,
            0x1F86E,
            0x1F878,
            0x1F87A,
            0x1F879,
            0x1F87B,
            0x1F87C,
            0x1F87D,
            0x1F87F,
            0x1F87E,
            0x21E6,
            0x21E8,
            0x21E7,
            0x21E9,
            0x2B04,
            0x21F3,
            0x2B01,
            0x2B00,
            0x2B03,
            0x2B02,
            0x1F8AC,
            0x1F8AD,
            0x1F5F6,
            0x2713,
            0x1F5F7,
            0x1F5F9,

            // Windows symbol, which is of course not in Unicode.
            // Closest thing is SQUARED PLUS (⊞) but as you see in some fonts it's not even really that close.
            // Going with unknown for now.
            _unicodeUnknown_Char
        };

        private readonly int[] _webdingsFontToUnicode = new int[224]
        {
            ' ',
            0x1F577,
            0x1F578,
            0x1F572,
            0x1F576,
            0x1F3C6,
            0x1F396,
            0x1F587,
            0x1F5E8,
            0x1F5E9,
            0x1F5F0,
            0x1F5F1,
            0x1F336,
            0x1F397,
            0x1F67E,
            0x1F67C,
            0x1F5D5,
            0x1F5D6,
            0x1F5D7,
            0x23F4,
            0x23F5,
            0x23F6,
            0x23F7,
            0x23EA,
            0x23E9,
            0x23EE,
            0x23ED,
            0x23F8,
            0x23F9,
            0x23FA,
            0x1F5DA,
            0x1F5F3,
            0x1F6E0,
            0x1F3D7,
            0x1F3D8,
            0x1F3D9,
            0x1F3DA,
            0x1F3DC,
            0x1F3ED,
            0x1F3DB,
            0x1F3E0,
            0x1F3D6,
            0x1F3DD,
            0x1F6E3,
            0x1F50D,
            0x1F3D4,
            0x1F441,
            0x1F442,
            0x1F3DE,
            0x1F3D5,
            0x1F6E4,
            0x1F3DF,
            0x1F6F3,
            0x1F56C,
            0x1F56B,
            0x1F568,
            0x1F508,
            0x1F394,
            0x1F395,
            0x1F5EC,
            0x1F67D,
            0x1F5ED,
            0x1F5EA,
            0x1F5EB,
            0x2B94,
            0x2714,
            0x1F6B2,
            0x2B1C,
            0x1F6E1,
            0x1F381,
            0x1F6F1,
            0x2B1B,
            0x1F691,
            0x1F6C8,
            0x1F6E9,
            0x1F6F0,
            0x1F7C8,
            0x1F574,
            0x2B24,
            0x1F6E5,
            0x1F694,
            0x1F5D8,
            0x1F5D9,
            0x2753,
            0x1F6F2,
            0x1F687,
            0x1F68D,
            0x1F6A9,
            0x29B8,
            0x2296,
            0x1F6AD,
            0x1F5EE,
            0x23D0,
            0x1F5EF,
            0x1F5F2,

            // 7F is undefined
            _unicodeUnknown_Char,

            0x1F6B9,
            0x1F6BA,
            0x1F6C9,
            0x1F6CA,
            0x1F6BC,
            0x1F47D,
            0x1F3CB,
            0x26F7,
            0x1F3C2,
            0x1F3CC,
            0x1F3CA,
            0x1F3C4,
            0x1F3CD,
            0x1F3CE,
            0x1F698,
            0x1F4C8,
            0x1F6E2,
            0x1F4B0,
            0x1F3F7,
            0x1F4B3,
            0x1F46A,
            0x1F5E1,
            0x1F5E2,
            0x1F5E3,
            0x272F,
            0x1F584,
            0x1F585,
            0x1F583,
            0x1F586,
            0x1F5B9,
            0x1F5BA,
            0x1F5BB,
            0x1F575,
            0x1F570,
            0x1F5BD,
            0x1F5BE,
            0x1F4CB,
            0x1F5D2,
            0x1F5D3,
            0x1F56E,
            0x1F4DA,
            0x1F5DE,
            0x1F5DF,
            0x1F5C3,
            0x1F4C7,
            0x1F5BC,
            0x1F3AD,
            0x1F39C,
            0x1F398,
            0x1F399,
            0x1F3A7,
            0x1F4BF,
            0x1F39E,
            0x1F4F7,
            0x1F39F,
            0x1F3AC,
            0x1F4FD,
            0x1F4F9,
            0x1F4FE,
            0x1F4FB,
            0x1F39A,
            0x1F39B,
            0x1F4FA,
            0x1F4BB,
            0x1F5A5,
            0x1F5A6,
            0x1F5A7,
            0x1F579,
            0x1F3AE,
            0x1F57B,
            0x1F57C,
            0x1F4DF,
            0x1F581,
            0x1F580,
            0x1F5A8,
            0x1F5A9,
            0x1F5BF,
            0x1F5AA,
            0x1F5DC,
            0x1F512,
            0x1F513,
            0x1F5DD,
            0x1F4E5,
            0x1F4E4,
            0x1F573,
            0x1F323,
            0x1F324,
            0x1F325,
            0x1F326,
            0x2601,
            0x1F328,
            0x1F327,
            0x1F329,
            0x1F32A,
            0x1F32C,
            0x1F32B,
            0x1F31C,
            0x1F321,
            0x1F6CB,
            0x1F6CF,
            0x1F37D,
            0x1F378,
            0x1F6CE,
            0x1F6CD,
            0x24C5,
            0x267F,
            0x1F6C6,
            0x1F588,
            0x1F393,
            0x1F5E4,
            0x1F5E5,
            0x1F5E6,
            0x1F5E7,
            0x1F6EA,
            0x1F43F,
            0x1F426,
            0x1F41F,
            0x1F415,
            0x1F408,
            0x1F66C,
            0x1F66E,
            0x1F66D,
            0x1F66F,
            0x1F5FA,
            0x1F30D,
            0x1F30F,
            0x1F30E,
            0x1F54A
        };
        // ReSharper restore RedundantExplicitArraySize

        #endregion

        private readonly SymbolDict _symbolTable = new SymbolDict();

        #endregion

        #region Preallocated char arrays

        // This "SYMBOL" and the below "Symbol" are unrelated. "SYMBOL" is a fldinst keyword, while "Symbol" is
        // the name of a font.
        private readonly char[] _SYMBOLChars = { 'S', 'Y', 'M', 'B', 'O', 'L' };

        private readonly char[] _wingdingsChars = { 'W', 'i', 'n', 'g', 'd', 'i', 'n', 'g', 's' };

        private readonly char[] _webdingsChars = { 'W', 'e', 'b', 'd', 'i', 'n', 'g', 's' };

        private readonly char[] _symbolChars = { 'S', 'y', 'm', 'b', 'o', 'l' };

        #endregion

        #region Resettables

        private readonly RTFStream _rtfStream = new RTFStream();

        private readonly Header _header = new Header();

        // FMs can have 100+ of these...
        // Highest measured was 131
        // Fonts can specify themselves as whatever number they want, so we can't just count by index
        // eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45
        private readonly DictWithTopItem<int, FontEntry> _fontEntries = new DictWithTopItem<int, FontEntry>(150);
        /*
        Per spec, if we see a \uN keyword whose N falls within the range of 0xF020 to 0xF0FF, we're supposed to
        subtract 0xF000 and then find the last used font whose charset is 2 (codepage 42) and use its symbol font
        to convert the char. However, when the spec says "last used" it REALLY means last used. Period. Regardless
        of scope. Even if the font was used in a scope above us that we should have no knowledge of, it still
        counts as the last used one. Also, we need the last used font WHOSE CODEPAGE IS 42, not the last used font
        period. So we have to track only the charset 2/codepage 42 ones. Globally. Truly bizarre.
        */
        private int _lastUsedFontWithCodePage42 = -1;

        // Highest measured was 10
        private readonly ScopeStack _scopeStack = new ScopeStack();

        private readonly CurrentScope _currentScope = new CurrentScope();

        // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
        private int _groupCount;

        private int _binaryCharsLeftToSkip;
        private int _unicodeCharsLeftToSkip;

        private bool _skipDestinationIfUnknown;

        // Highest measured was 56192
        private readonly ListFast<char> _plainText = new ListFast<char>(ByteSize.KB * 60);

        private const int _keywordMaxLen = 32;
        private readonly ListFast<char> _keyword = new ListFast<char>(_keywordMaxLen);

        // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
        private const int _paramMaxLen = 10;

        private const int _fldinstSymbolNumberMaxLen = 10;
        private readonly ListFast<char> _fldinstSymbolNumber = new ListFast<char>(_fldinstSymbolNumberMaxLen);

        private const int _fldinstSymbolFontNameMaxLen = 9;
        private readonly ListFast<char> _fldinstSymbolFontName = new ListFast<char>(_fldinstSymbolFontNameMaxLen);

        // Highest measured was 17
        private readonly ListFast<byte> _hexBuffer = new ListFast<byte>(20);

        // Highest measured was 13
        private readonly ListFast<char> _unicodeBuffer = new ListFast<char>(20);

        #endregion

        #region Cached encodings

        // DON'T reset this. We want to build up a dictionary of encodings and amortize it over the entire list
        // of RTF files.
        private readonly Dictionary<int, Encoding> _encodings = new Dictionary<int, Encoding>(31);

        // Common ones explicitly stored to avoid even a dictionary lookup. Don't reset these either.
        private readonly Encoding _windows1252Encoding = Encoding.GetEncoding(_windows1252);

        private readonly Encoding _windows1250Encoding = Encoding.GetEncoding(1250);

        private readonly Encoding _windows1251Encoding = Encoding.GetEncoding(1251);

        private readonly Encoding _shiftJisWinEncoding = Encoding.GetEncoding(_shiftJisWin);

        #endregion

        #region Public API

        public RtfToTextConverter()
        {
            // If we're on something other than Windows, we don't normally have access to most of the
            // codepages that Windows does. To get access to them, we need to take a dependency on the
            // ~700k System.Text.Encoding.CodePages NuGet package and then call this RegisterProvider
            // method like this. But we don't want to carry around the extra package until we actually
            // need it, so it's disabled for now.
#if CROSS_PLATFORM
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        private void Reset(Stream stream, long streamLength)
        {
            #region Fixed-size fields

            // Specific capacity and won't grow; no need to deallocate
            _keyword.ClearFast();
            _fldinstSymbolNumber.ClearFast();
            _fldinstSymbolFontName.ClearFast();

            // Fixed-size value types
            _groupCount = 0;
            _binaryCharsLeftToSkip = 0;
            _unicodeCharsLeftToSkip = 0;
            _skipDestinationIfUnknown = false;
            _lastUsedFontWithCodePage42 = -1;

            // Types that contain only fixed-size value types
            _header.Reset();
            _currentScope.Reset();

            #endregion

            _hexBuffer.ClearFast();
            _unicodeBuffer.ClearFast();
            _fontEntries.Clear();
            _scopeStack.ClearFast();
            _plainText.ClearFast();

            // Extremely unlikely we'll hit any of these, but just for safety
            if (_hexBuffer.Capacity > ByteSize.MB) _hexBuffer.Capacity = 0;
            if (_unicodeBuffer.Capacity > ByteSize.MB) _unicodeBuffer.Capacity = 0;
            // For the font entries, we can't check a Dictionary's capacity nor set it, so... oh well.
            // For the scope stack, we can't check its capacity because Stacks don't have a Capacity property(?!?),
            // but we're guaranteed not to exceed 100 (or 128 I guess) because of a check in the only place where
            // we push to it.
            if (_plainText.Capacity > ByteSize.MB) _plainText.Capacity = 0;

            // This one has the seek-back buffer (a Stack<char>) which is technically eligible for deallocation,
            // even though in practice I think it's guaranteed never to have more than like 5 chars in it maybe?
            // Again, it's a stack so we can't check its capacity. But... meh. See above.
            // Not way into the idea of making another custom type where the only difference is we can access a
            // frigging internal variable, gonna be honest.
            _rtfStream.Reset(stream, streamLength);
        }

        [PublicAPI]
        public (bool Success, string Text)
        Convert(Stream stream, long streamLength)
        {
            Reset(stream, streamLength);

#if ReleaseRTFTest || DebugRTFTest

            Error error = ParseRtf();
            return error == Error.OK ? (true, CreateStringFromChars(_plainText)) : throw new Exception("RTF converter error: " + error);
#else
            try
            {
                Error error = ParseRtf();
                return error == Error.OK ? (true, CreateStringFromChars(_plainText)) : (false, "");
            }
            catch
            {
                return (false, "");
            }
#endif
        }

        #endregion

        private Error ParseRtf()
        {
            Error ec;
            int nibbleCount = 0;
            byte b = 0;

            char ch;
            while (_rtfStream.CurrentPos < _rtfStream.Length)
            {
                ch = _rtfStream.GetNextCharFast();

                if (_groupCount < 0) return Error.StackUnderflow;

                if (_currentScope.RtfInternalState == RtfInternalState.Binary)
                {
                    if (--_binaryCharsLeftToSkip <= 0)
                    {
                        _currentScope.RtfInternalState = RtfInternalState.Normal;
                        _binaryCharsLeftToSkip = 0;
                    }
                    continue;
                }

                switch (ch)
                {
                    case '{':
                        // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        if (_unicodeBuffer.Count > 0) ParseUnicodeIfAnyInBuffer();
                        if ((ec = PushScope()) != Error.OK) return ec;
                        break;
                    case '}':
                        // ditto the above
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        if (_unicodeBuffer.Count > 0) ParseUnicodeIfAnyInBuffer();
                        if ((ec = PopScope()) != Error.OK) return ec;
                        break;
                    case '\\':
                        // We have to check what the keyword is before deciding whether to parse the Unicode.
                        // If it's another \uN keyword, then obviously we don't want to parse yet because the
                        // run isn't finished.
                        if ((ec = ParseKeyword()) != Error.OK) return ec;
                        break;
                    case '\r':
                    case '\n':
                        // These DON'T count as Unicode barriers, so don't parse the Unicode here!
                        break;
                    default:
                        // It's a Unicode barrier, so parse the Unicode here.
                        if (_unicodeBuffer.Count > 0 && _unicodeCharsLeftToSkip == 0)
                        {
                            ParseUnicodeIfAnyInBuffer();
                        }

                        switch (_currentScope.RtfInternalState)
                        {
                            case RtfInternalState.Normal:
                                if (_currentScope.RtfDestinationState == RtfDestinationState.Normal)
                                {
                                    if ((ec = ParseChar(ch)) != Error.OK) return ec;
                                }
                                break;
                            case RtfInternalState.HexEncodedChar:
                                if ((ec = ParseHex(ref nibbleCount, ref ch, ref b)) != Error.OK) return ec;
                                break;
                        }
                        break;
                }
            }

            return _groupCount < 0 ? Error.StackUnderflow : _groupCount > 0 ? Error.UnmatchedBrace : Error.OK;
        }

        private Error ParseKeyword()
        {
            bool hasParam = false;
            bool negateParam = false;
            int param = 0;

            if (!_rtfStream.GetNextChar(out char ch)) return Error.EndOfFile;

            _keyword.ClearFast();

            if (!IsAsciiAlpha(ch))
            {
                /* From the spec:
                 "A control symbol consists of a backslash followed by a single, non-alphabetical character.
                 For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
                 delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

                 So just go straight to dispatching without looking for a param and without eating the space.
                */
                _keyword.AddFast(ch);
                return DispatchKeyword(0, false);
            }

            int i;
            bool eof = false;
            for (i = 0; i < _keywordMaxLen && IsAsciiAlpha(ch); i++, eof = !_rtfStream.GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                _keyword.AddFast(ch);
            }
            if (i > _keywordMaxLen) return Error.KeywordTooLong;

            if (ch == '-')
            {
                negateParam = true;
                if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;
            }

            if (IsAsciiDigit(ch))
            {
                hasParam = true;

                // Parse param in real-time to avoid doing a second loop over
                for (i = 0; i < _paramMaxLen && IsAsciiDigit(ch); i++, eof = !_rtfStream.GetNextChar(out ch))
                {
                    if (eof) return Error.EndOfFile;
                    param += ch - '0';
                    param *= 10;
                }
                // Undo the last multiply just one time to avoid checking if we should do it every time through
                // the loop
                param /= 10;
                if (i > _paramMaxLen) return Error.ParameterTooLong;

                if (negateParam) param = -param;
            }

            /* From the spec:
             "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI characters)
             that is not counted in the characters to skip."
             This implements the spec for regular control words and \uN alike. Nothing extra needed for removing
             the space from the skip-chars to count.
            */
            if (ch != ' ') _rtfStream.UnGetChar(ch);

            return DispatchKeyword(param, hasParam);
        }

        #region Act on keywords

        private Error DispatchKeyword(int param, bool hasParam)
        {
            if (!_symbolTable.TryGetValue(_keyword, out Symbol? symbol))
            {
                // If this is a new destination
                if (_skipDestinationIfUnknown)
                {
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                }
                _skipDestinationIfUnknown = false;
                return Error.OK;
            }

            // From the spec:
            // "While this is not likely to occur (or recommended), a \binN keyword, its argument, and the binary
            // data that follows are considered one character for skipping purposes."
            if (symbol.Index == (int)SpecialType.Bin && _unicodeCharsLeftToSkip > 0)
            {
                // Rather than literally counting it as one character for skipping purposes, we just increment
                // the chars left to skip count by the specified length of the binary run, which accomplishes
                // the same thing and is the easiest option.
                // Note: It seems like we should have to add 1 for the space after \binN, but it looks like the
                // numbers somehow work out that we don't have to and it's already implicitly counted. Shrug.
                if (param >= 0) _unicodeCharsLeftToSkip += param;
            }

            // From the spec:
            // "Any RTF control word or symbol is considered a single character for the purposes of counting
            // skippable characters."
            // But don't do it if it's a hex char, because we handle it elsewhere in that case.
            if (symbol.Index != (int)SpecialType.HexEncodedChar &&
                _currentScope.RtfInternalState != RtfInternalState.Binary &&
                _unicodeCharsLeftToSkip > 0)
            {
                if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
                return Error.OK;
            }

            if (symbol.Index != (int)SpecialType.UnicodeChar &&
                _unicodeBuffer.Count > 0 && _unicodeCharsLeftToSkip == 0)
            {
                ParseUnicodeIfAnyInBuffer();
            }

            _skipDestinationIfUnknown = false;
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                        ? ChangeProperty((Property)symbol.Index, param)
                        : Error.OK;
                case KeywordType.Character:
                    return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                        ? ParseChar((char)symbol.Index)
                        : Error.OK;
                case KeywordType.Destination:
                    return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                        ? ChangeDestination((DestinationType)symbol.Index)
                        : Error.OK;
                case KeywordType.Special:
                    var specialType = (SpecialType)symbol.Index;
                    return _currentScope.RtfDestinationState == RtfDestinationState.Normal ||
                           specialType == SpecialType.Bin
                        ? DispatchSpecialKeyword(specialType, param)
                        : Error.OK;
                default:
                    return Error.InvalidSymbolTableEntry;
            }
        }

        private Error DispatchSpecialKeyword(SpecialType specialType, int param)
        {
            switch (specialType)
            {
                case SpecialType.Bin:
                    if (param > 0)
                    {
                        _currentScope.RtfInternalState = RtfInternalState.Binary;
                        _binaryCharsLeftToSkip = param;
                    }
                    break;
                case SpecialType.HexEncodedChar:
                    _currentScope.RtfInternalState = RtfInternalState.HexEncodedChar;
                    break;
                case SpecialType.SkipDest:
                    _skipDestinationIfUnknown = true;
                    break;
                case SpecialType.UnicodeChar:
                    _unicodeCharsLeftToSkip = _currentScope.Properties[(int)Property.UnicodeCharSkipCount];

                    // Make sure the code point is normalized before adding it to the buffer!
                    Error error = NormalizeUnicodePoint(ref param, handleSymbolCharRange: true);
                    if (error != Error.OK) return error;

                    // If our code point has been through a font translation table, it may be longer than 2 bytes.
                    if (param > 0xFFFF)
                    {
                        string? charsAsStr = ConvertFromUtf32(param);
                        if (charsAsStr == null)
                        {
                            _unicodeBuffer.Add(_unicodeUnknown_Char);
                        }
                        else
                        {
                            _unicodeBuffer.Add(charsAsStr[0]);
                            _unicodeBuffer.Add(charsAsStr[1]);
                        }
                    }
                    else
                    {
                        // At this point, param is guaranteed to fit into a char
                        _unicodeBuffer.Add((char)param);
                    }
                    break;
                case SpecialType.HeaderCodePage:
                    _header.CodePage = param >= 0 ? param : _windows1252;
                    break;
                case SpecialType.FontTable:
                    _currentScope.InFontTable = true;
                    break;
                case SpecialType.DefaultFont:
                    // Only set the first one... not likely to be any others anyway, but still
                    if (!_header.DefaultFontSet)
                    {
                        _header.DefaultFontNum = param;
                        _header.DefaultFontSet = true;
                    }
                    break;
                case SpecialType.Charset:
                    // Reject negative codepage values as invalid and just use the header default in that case
                    // (which is guaranteed not to be negative)
                    if (_fontEntries.Count > 0 && _currentScope.InFontTable)
                    {
                        _fontEntries.Top.CodePage = param >= 0 && _charSetToCodePage.TryGetValue(param, out int codePage)
                            ? codePage
                            : _header.CodePage;
                    }
                    break;
                case SpecialType.CodePage:
                    if (_fontEntries.Count > 0 && _currentScope.InFontTable)
                    {
                        _fontEntries.Top.CodePage = param >= 0 ? param : _header.CodePage;
                    }
                    break;
                default:
                    return Error.InvalidSymbolTableEntry;
            }

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ChangeProperty(Property propertyTableIndex, int val)
        {
            if (propertyTableIndex == Property.FontNum)
            {
                if (_currentScope.InFontTable)
                {
                    _fontEntries.Add(val, new FontEntry());
                    return Error.OK;
                }
                else if (_fontEntries.TryGetValue(val, out FontEntry fontEntry))
                {
                    if (fontEntry.CodePage == 42)
                    {
                        // We have to track this globally, per behavior of RichEdit and implied by the spec.
                        _lastUsedFontWithCodePage42 = val;
                    }

                    // Support bare characters that are supposed to be displayed in a symbol font. We use a simple
                    // enum so that we don't have to do a dictionary lookup on every single character, but only
                    // once per font change.
                    _currentScope.SymbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
                }
            }

            _currentScope.Properties[(int)propertyTableIndex] = val;

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ChangeDestination(DestinationType destinationType)
        {
            switch (destinationType)
            {
                case DestinationType.IgnoreButDontSkipGroup:
                    // The group this destination is in may contain text we want to extract, so parse it as normal.
                    // We will still skip over the next nested destination group we find, if any, unless it too is
                    // marked as ignore-but-don't-skip.
                    return Error.OK;
                case DestinationType.FieldInstruction:
                    return HandleFieldInstruction();
                case DestinationType.Skip:
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                    return Error.OK;
                default:
                    return Error.InvalidSymbolTableEntry;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ParseChar(char ch)
        {
            if (_currentScope.InFontTable && _fontEntries.Count > 0)
            {
                _fontEntries.Top.AppendNameChar(ch);
            }

            // Don't get clever and change the order of things. We need to know if our count is > 0 BEFORE
            // trying to print, because we want to not print if it's > 0. Only then do we decrement it.
            Error error = _unicodeCharsLeftToSkip == 0 ? PutChar(ch) : Error.OK;

            if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
            return error;
        }

        #endregion

        #region Handle specially encoded characters

        private Error ParseHex(ref int nibbleCount, ref char ch, ref byte b)
        {
            #region Local functions

            Error ResetBufferAndStateAndReturn()
            {
                _hexBuffer.ClearFast();
                _currentScope.RtfInternalState = RtfInternalState.Normal;
                return Error.OK;
            }

            #endregion

            // If multiple hex chars are directly after another (eg. \'81\'63) then they may be representing one
            // multibyte character (or not, they may also just be two single-byte chars in a row). To deal with
            // this, we have to put all contiguous hex chars into a buffer and when the run ends, we just pass
            // the buffer to the current encoding's byte-to-string decoder and get our correct result.

            _hexBuffer.ClearFast();

            // Quick-n-dirty goto for now. TODO: Make this better or something
            restartButDontClearBuffer:

            b = (byte)(b << 4);

            if (IsAsciiDigit(ch))
            {
                b += (byte)(ch - '0');
            }
            else if (ch >= 'a' && ch <= 'f')
            {
                b += (byte)(ch - 'a' + 10);
            }
            else if (ch >= 'A' && ch <= 'F')
            {
                b += (byte)(ch - 'A' + 10);
            }
            else
            {
                return Error.InvalidHex;
            }

            nibbleCount++;
            if (nibbleCount < 2 && _hexBuffer.Count > 0)
            {
                if (!_rtfStream.GetNextChar(out ch))
                {
                    _rtfStream.UnGetChar(ch);
                    return ResetBufferAndStateAndReturn();
                }
                goto restartButDontClearBuffer;
            }

            if (nibbleCount < 2) return Error.OK;

            _hexBuffer.Add(b);

            nibbleCount = 0;
            b = 0;

            (bool success, bool codePageWas42, Encoding? enc, FontEntry? fontEntry) = GetCurrentEncoding();

            // DON'T try to combine this byte with the next one if:
            // -We're on code page 42 (symbol font translation) - then we're guaranteed to be single-byte, and
            //  combining won't give a correct result
            // -This hex char is part of a Unicode skip
            if (!codePageWas42 && _unicodeCharsLeftToSkip == 0)
            {
                bool lastCharInStream = false;

                // Put the LAST char back (for pch1 it's ch, for pch2 it's pch1) to fix last-char-in-stream
                // corner case
                if (!_rtfStream.GetNextChar(out char pch1))
                {
                    _rtfStream.UnGetChar(ch);
                    lastCharInStream = true;
                }
                if (!_rtfStream.GetNextChar(out char pch2))
                {
                    _rtfStream.UnGetChar(pch1);
                    lastCharInStream = true;
                }

                // Horrific hacks everywhere, argh... I refuse to use another goto
                if (!lastCharInStream)
                {
                    if (pch1 == '\\' && pch2 == '\'')
                    {
                        if (!_rtfStream.GetNextChar(out ch))
                        {
                            _rtfStream.UnGetChar(pch2);
                            _rtfStream.UnGetChar(pch1);

                            return ResetBufferAndStateAndReturn();
                        }

                        goto restartButDontClearBuffer;
                    }
                    else
                    {
                        _rtfStream.UnGetChar(pch2);
                        _rtfStream.UnGetChar(pch1);
                    }
                }
            }

            if (_unicodeCharsLeftToSkip > 0)
            {
                if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
            }
            else
            {
                string finalChar;
                if (!success)
                {
                    finalChar = _unicodeUnknown_String;
                }
                else
                {
                    if (codePageWas42 && _hexBuffer.Count == 1)
                    {
                        int codePoint = _hexBuffer.ItemsArray[0];

                        if (fontEntry == null)
                        {
                            GetCharFromConversionList(codePoint, _symbolFontToUnicode, out finalChar);
                            if (finalChar.IsEmpty()) finalChar = _unicodeUnknown_String;
                        }
                        else
                        {
                            if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _wingdingsChars))
                            {
                                GetCharFromConversionList(codePoint, _wingdingsFontToUnicode, out finalChar);
                            }
                            else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _webdingsChars))
                            {
                                GetCharFromConversionList(codePoint, _webdingsFontToUnicode, out finalChar);
                            }
                            else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _symbolChars))
                            {
                                GetCharFromConversionList(codePoint, _symbolFontToUnicode, out finalChar);
                            }
                            else
                            {
                                try
                                {
                                    finalChar = enc?.GetString(_hexBuffer.ItemsArray, 0, _hexBuffer.Count) ?? _unicodeUnknown_String;
                                }
                                catch
                                {
                                    finalChar = _unicodeUnknown_String;
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            finalChar = enc?.GetString(_hexBuffer.ItemsArray, 0, _hexBuffer.Count) ?? _unicodeUnknown_String;
                        }
                        catch
                        {
                            finalChar = _unicodeUnknown_String;
                        }
                    }
                }

                PutChar(finalChar);
            }

            return ResetBufferAndStateAndReturn();
        }

        /*
        Unlike the hex parser, we can't just cordon this off in its own little world. The reason is that we need
        to skip characters if necessary, and a "character" could mean any of the following:
        -An actual single character
        -A hex-encoded character (\'hh)
        -An entire control word
        -A \binN word, the space after it, and all of its binary data

        If we wanted to handle all that, we would have to pretty much duplicate the entire RTF parser just in
        here. So we mix this in with the regular parser and just accept the less followable logic as a lesser
        cost than the alternative.
        */
        private void ParseUnicodeIfAnyInBuffer()
        {
            string finalString = new string(_unicodeBuffer.ItemsArray, 0, _unicodeBuffer.Count);

            _unicodeBuffer.ClearFast();

            #region Fix up bad Unicode

            // We can't just pass a char array, because then the validation won't work. We have to convert to
            // string first. Fortunately, this loses no time because we have to do the conversion anyway, and we
            // don't even touch the string unless there's a problem, which means no extra allocations if everything
            // is valid.
            StringBuilder? sb = null;

            for (int i = 0; i < finalString.Length; i++)
            {
                // We need to use (str, i) instead of (str[i]) because the validation check doesn't work if
                // we do the latter. Unfortunately we need to take some boneheaded pointless null and length
                // checks, because we can't pull this method out and customize it, because it calls a goddamn
                // internal method. Access levels... hooray, you're slow and you can't do crap about it.

                // This won't throw because str is not null and index is within it
                UnicodeCategory uc = char.GetUnicodeCategory(finalString, i);

                if (uc == UnicodeCategory.Surrogate || uc == UnicodeCategory.OtherNotAssigned)
                {
                    // Don't even instantiate our StringBuilder unless there's a problem
                    sb ??= new StringBuilder(finalString);
                    sb[i] = _unicodeUnknown_Char;
                }

                // We can do (str[i]) here because (str, i) is literally just a wrapper that can throw.
                if (char.IsHighSurrogate(finalString[i])) i++;
            }

            // Likewise, don't even change the string at all unless we have to
            if (sb != null) finalString = sb.ToString();

            #endregion

            PutChar(finalString);
        }

        /*
        Field instructions are completely out to lunch with a totally unique syntax and even escaped control
        words (\\f etc.), so rather than try to shoehorn that into the regular parser and pollute it all up
        with incomprehensible nonsense, I'm just cordoning the whole thing off here. It's still incomprehensible
        nonsense, but hey, at least it stays in the loony bin and doesn't hurt anyone else.

        Also we only need one field instruction - SYMBOL. The syntax should go strictly like this:

        {\*\fldinst SYMBOL [\\f ["FontName"]] [\\a] [\\j] [\\u] [\\h] [\\s n] <arbitrary amounts of other junk>}
        
        With [] denoting optional parameters. I guess one of them must be present (at least we would fail if we
        didn't find any).
        
        Anyway, the spec is clear and simple and so we can just try to parse it exactly and quit on anything
        unexpected. Otherwise, we only want either \\f, \\a, \\j, or \\u. The others we ignore. Once we've found
        what we need, looped through six params and not found what we need, or reached a separator char, we quit
        and skip the rest of the group.

        TODO: I should try to integrate this just to see if it ends up being as bad as I'm assuming.
        Maybe it won't. Who knows.
        */
        private Error HandleFieldInstruction()
        {
            _fldinstSymbolNumber.ClearFast();
            _fldinstSymbolFontName.ClearFast();

            int codePoint;

            // Eat the space
            if (!_rtfStream.GetNextChar(out char ch)) return Error.EndOfFile;

            #region Check for SYMBOL instruction

            // Straight-up just check for S, because SYMBOL is the only word we care about.
            if (ch != 'S') return RewindAndSkipGroup(ch);

            int i;
            bool eof = false;
            for (i = 0; i < 6; i++, eof = !_rtfStream.GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                if (ch != _SYMBOLChars[i]) return RewindAndSkipGroup(ch);
            }

            #endregion

            if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;

            bool numIsHex = false;
            bool negateNum = false;

            #region Parse numeric field parameter

            if (ch == '-')
            {
                _rtfStream.GetNextChar(out ch);
                negateNum = true;
            }

            #region Handle if the param is hex

            if (ch == '0' &&
                _rtfStream.GetNextChar(out char pch) && (pch == 'x' || pch == 'X'))
            {
                _rtfStream.GetNextChar(out ch);
                if (ch == '-')
                {
                    _rtfStream.GetNextChar(out ch);
                    if (ch != ' ') return RewindAndSkipGroup(ch);
                    negateNum = true;
                }
                numIsHex = true;
            }

            #endregion

            #region Read parameter

            bool alphaCharsFound = false;
            bool alphaFound;
            for (i = 0;
                i < _fldinstSymbolNumberMaxLen && ((alphaFound = IsAsciiAlpha(ch)) || IsAsciiDigit(ch));
                i++, eof = !_rtfStream.GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;

                if (alphaFound) alphaCharsFound = true;

                _fldinstSymbolNumber.Add(ch);
            }

            if (_fldinstSymbolNumber.Count == 0 ||
                i >= _fldinstSymbolNumberMaxLen ||
                (!numIsHex && alphaCharsFound))
            {
                return RewindAndSkipGroup(ch);
            }

            #endregion

            #region Parse parameter

            if (numIsHex)
            {
                // ALLOC: ToString(): int.TryParse(hex)
                // We could implement our own hex parser, but this is called so infrequently (actually not at all
                // in the test set) that it doesn't really matter.
                // TODO: Make our own parser anyway, because speed in all things
                if (!int.TryParse(CreateStringFromChars(_fldinstSymbolNumber),
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out codePoint))
                {
                    return RewindAndSkipGroup(ch);
                }
            }
            else
            {
                codePoint = ParseIntFast(_fldinstSymbolNumber);
            }

            #endregion

            #endregion

            if (negateNum) codePoint = -codePoint;

            // TODO: Do we need to handle 0xF020-0xF0FF type stuff and negative values for field instructions?
            Error error = NormalizeUnicodePoint(ref codePoint, handleSymbolCharRange: false);
            if (error != Error.OK) return error;

            if (ch != ' ') return RewindAndSkipGroup(ch);

            const int maxParams = 6;
            const int useCurrentScopeCodePage = -1;

            string finalChar = "";

            #region Parse params

            for (i = 0; i < maxParams; i++)
            {
                if (!_rtfStream.GetNextChar(out pch) || pch != '\\' ||
                    !_rtfStream.GetNextChar(out pch) || pch != '\\')
                {
                    continue;
                }

                if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;

                // From the spec:
                // "Interprets text in field-argument as the value of an ANSI character."
                if (ch == 'a')
                {
                    finalChar = GetCharFromCodePage(_windows1252, codePoint);
                    break;
                }
                /*
                From the spec:
                "Interprets text in field-argument as the value of a SHIFT-JIS character."

                Note that "SHIFT-JIS" in RTF land means specifically Windows-31J or whatever you want to call it.
                */
                else if (ch == 'j')
                {
                    finalChar = GetCharFromCodePage(_shiftJisWin, codePoint);
                    break;
                }
                else if (ch == 'u')
                {
                    finalChar = ConvertFromUtf32(codePoint) ?? _unicodeUnknown_String;
                    break;
                }
                /*
                From the spec:
                "Interprets text in the switch's field-argument as the name of the font from which the character
                whose value is specified by text in the field's field-argument. By default, the font used is that
                for the current text run."

                In other words:
                If it's \\f, we use the current scope's font's codepage, and if it's \\f "FontName" then we use
                code page 1252 and convert from FontName to Unicode using manual conversion tables, that is
                assuming FontName is "Symbol", "Wingdings" or "Webdings". We don't really want to go down the
                rabbit hole of Wingdings 2, Wingdings 3, or whatever else have you...

                Note that RichEdit doesn't go this far. It reads the fldinst parts and all, and displays the
                characters in rich text if you have the appropriate fonts, but it doesn't convert from the fonts
                to equivalent Unicode chars when it converts to plain text. Which is reasonable really, but we
                want to do it because it's some cool ninja shit and also it helps us keep the odd copyright symbol
                intact and stuff.
                */
                else if (ch == 'f')
                {
                    if (!_rtfStream.GetNextChar(out ch))
                    {
                        return Error.EndOfFile;
                    }
                    else if (IsSeparatorChar(ch))
                    {
                        finalChar = GetCharFromCodePage(useCurrentScopeCodePage, codePoint);
                        break;
                    }
                    else if (ch == ' ')
                    {
                        if (!_rtfStream.GetNextChar(out ch))
                        {
                            return Error.EndOfFile;
                        }
                        else if (ch != '\"')
                        {
                            finalChar = GetCharFromCodePage(useCurrentScopeCodePage, codePoint);
                            break;
                        }

                        int fontNameCharCount = 0;

                        while (_rtfStream.GetNextChar(out ch) && ch != '\"')
                        {
                            if (fontNameCharCount >= _fldinstSymbolFontNameMaxLen || IsSeparatorChar(ch))
                            {
                                return RewindAndSkipGroup(ch);
                            }
                            _fldinstSymbolFontName.Add(ch);
                            fontNameCharCount++;
                        }

                        // Just hardcoding the three most common fonts here, because there's only so far you
                        // really want to go down this path.
                        if (SeqEqual(_fldinstSymbolFontName, _symbolChars) &&
                            !GetCharFromConversionList(codePoint, _symbolFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup(ch);
                        }
                        else if (SeqEqual(_fldinstSymbolFontName, _wingdingsChars) &&
                                 !GetCharFromConversionList(codePoint, _wingdingsFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup(ch);
                        }
                        else if (SeqEqual(_fldinstSymbolFontName, _webdingsChars) &&
                                 !GetCharFromConversionList(codePoint, _webdingsFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup(ch);
                        }
                    }
                }
                /*
                From the spec:
                "Inserts the symbol without affecting the line spacing of the paragraph. If large symbols are
                inserted with this switch, text above the symbol may be overwritten."

                This doesn't concern us, so ignore it.
                */
                else if (ch == 'h')
                {
                    if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;
                    if (IsSeparatorChar(ch)) break;
                }
                /*
                From the spec:
                "Interprets text in the switch's field-argument as the integral font size in points."

                This one takes an argument (hence the extra logic to ignore it), but, yeah, we ignore it.
                */
                else if (ch == 's')
                {
                    if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;
                    if (ch != ' ') return RewindAndSkipGroup(ch);

                    int numDigitCount = 0;
                    while (_rtfStream.GetNextChar(out ch) && IsAsciiDigit(ch))
                    {
                        if (numDigitCount > _fldinstSymbolNumberMaxLen) goto breakout;
                        numDigitCount++;
                    }

                    if (IsSeparatorChar(ch)) break;
                }
            }

            breakout:

            #endregion

            if (finalChar != "") PutChar(finalChar);

            return RewindAndSkipGroup(ch);
        }

        #endregion

        #region PutChar

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PutChar(char ch)
        {
            //Trace.Write(ch.ToString());
            if (ch != '\0' &&
                _currentScope.Properties[(int)Property.Hidden] == 0 &&
                !_currentScope.InFontTable)
            {
                // We don't really have a way to set the default font num as the first scope's font num, because
                // the font definitions come AFTER the default font control word, so let's just do this check
                // right here. It's fast if we have a font num for this scope, and if not, it'll only run once
                // anyway, so we shouldn't take much of a speed hit.
                if (_currentScope.SymbolFont == SymbolFont.None &&
                    _currentScope.Properties[(int)Property.FontNum] == -1 &&
                    _header.DefaultFontNum > -1 &&
                    _fontEntries.TryGetValue(_header.DefaultFontNum, out FontEntry fontEntry))
                {
                    _currentScope.SymbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
                }

                // Support bare characters that are supposed to be displayed in a symbol font.
                if (_currentScope.SymbolFont > SymbolFont.None)
                {
#pragma warning disable 8509
                    int[] fontTable = _currentScope.SymbolFont switch
                    {
                        SymbolFont.Symbol => _symbolFontToUnicode,
                        SymbolFont.Wingdings => _wingdingsFontToUnicode,
                        SymbolFont.Webdings => _webdingsFontToUnicode
                    };
#pragma warning restore 8509
                    if (GetCharFromConversionList(ch, fontTable, out string result))
                    {
                        for (int i = 0; i < result.Length; i++)
                        {
                            _plainText.Add(result[i]);
                        }
                    }
                }
                else
                {
                    _plainText.Add(ch);
                }
            }
            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PutChar(string ch)
        {
            // This is only ever called from encoded-char handlers (hex, Unicode, field instructions), so we don't
            // need to duplicate any of the bare-char symbol font stuff here.

            //Trace.Write(ch);
            if (ch != "\0" &&
                _currentScope.Properties[(int)Property.Hidden] == 0 &&
                !_currentScope.InFontTable)
            {
                for (int i = 0; i < ch.Length; i++)
                {
                    _plainText.Add(ch[i]);
                }
            }
            return Error.OK;
        }

        #endregion

        #region Scope push/pop

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PushScope()
        {
            // Don't wait for out-of-memory; just put a sane cap on it.
            if (_scopeStack.Count >= _maxScopes) return Error.StackOverflow;

            _scopeStack.Push(_currentScope);

            _currentScope.RtfInternalState = RtfInternalState.Normal;

            _groupCount++;

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PopScope()
        {
            if (_scopeStack.Count == 0) return Error.StackUnderflow;

            _scopeStack.Pop().DeepCopyTo(_currentScope);
            _groupCount--;

            return Error.OK;
        }

        #endregion

        #region Helpers

        #region Field instruction methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSeparatorChar(char ch) => ch == '\\' || ch == '{' || ch == '}';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetCharFromCodePage(int codePage, int codePoint)
        {
            byte[] bytes = BitConverter.GetBytes(codePoint);
            try
            {
                if (codePage > -1)
                {
                    return GetEncodingFromCachedList(codePage).GetString(bytes);
                }
                else
                {
                    (bool success, _, Encoding? enc, _) = GetCurrentEncoding();
                    return success && enc != null ? enc.GetString(bytes) : _unicodeUnknown_String;
                }
            }
            catch
            {
                return _unicodeUnknown_String;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error RewindAndSkipGroup(char ch)
        {
            _rtfStream.UnGetChar(ch);
            _currentScope.RtfDestinationState = RtfDestinationState.Skip;
            return Error.OK;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CreateStringFromChars(ListFast<char> chars) => new string(chars.ItemsArray, 0, chars.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiAlpha(char ch) => IsAsciiUpper(ch) || IsAsciiLower(ch);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiUpper(char ch) => ch >= 'A' && ch <= 'Z';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiLower(char ch) => ch >= 'a' && ch <= 'z';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiDigit(char ch) => ch >= '0' && ch <= '9';

        /// <summary>
        /// If <paramref name="codePage"/> is in the cached list, returns the Encoding associated with it;
        /// otherwise, gets the Encoding for <paramref name="codePage"/> and places it in the cached list
        /// for next time.
        /// </summary>
        /// <param name="codePage"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Encoding GetEncodingFromCachedList(int codePage)
        {
            switch (codePage)
            {
                case _windows1252:
                    return _windows1252Encoding;
                case 1250:
                    return _windows1250Encoding;
                case 1251:
                    return _windows1251Encoding;
                case _shiftJisWin:
                    return _shiftJisWinEncoding;
                default:
                    if (_encodings.TryGetValue(codePage, out Encoding result))
                    {
                        return result;
                    }
                    else
                    {
                        // NOTE: This can throw, but all calls to this are wrapped in try-catch blocks.
                        // TODO: But weird that we don't put the try-catch here and just return null...?
                        Encoding enc = Encoding.GetEncoding(codePage);
                        _encodings[codePage] = enc;
                        return enc;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool Success, bool CodePageWas42, Encoding? Encoding, FontEntry? FontEntry)
        GetCurrentEncoding()
        {
            int scopeFontNum = _currentScope.Properties[(int)Property.FontNum];

            if (scopeFontNum == -1) scopeFontNum = _header.DefaultFontNum;

            _fontEntries.TryGetValue(scopeFontNum, out FontEntry? fontEntry);

            int codePage = fontEntry?.CodePage ?? _header.CodePage;

            if (codePage == 42) return (true, true, null, fontEntry);

            // Awful, but we're based on nice, relaxing error returns, so we don't want to throw exceptions. Ever.
            Encoding enc;
            try
            {
                enc = GetEncodingFromCachedList(codePage);
            }
            catch
            {
                try
                {
                    enc = GetEncodingFromCachedList(_windows1252);
                }
                catch
                {
                    return (false, false, null, null);
                }
            }

            return (true, false, enc, fontEntry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SymbolFont GetSymbolFontTypeFromFontEntry(FontEntry fontEntry) =>
              CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _symbolChars)
            ? SymbolFont.Symbol
            : CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _wingdingsChars)
            ? SymbolFont.Wingdings
            : CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _webdingsChars)
            ? SymbolFont.Webdings
            : SymbolFont.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetCharFromConversionList(int codePoint, int[] _fontTable, out string finalChar)
        {
            if (codePoint >= 0x20 && codePoint <= 0xFF)
            {
                finalChar = ConvertFromUtf32(_fontTable[codePoint - 0x20]) ?? _unicodeUnknown_String;
            }
            else
            {
                if (codePoint > 255)
                {
                    finalChar = "";
                    return false;
                }
                try
                {
                    finalChar = GetEncodingFromCachedList(_windows1252).GetString(new[] { (byte)codePoint });
                }
                catch
                {
                    finalChar = _unicodeUnknown_String;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error NormalizeUnicodePoint(ref int codePoint, bool handleSymbolCharRange)
        {
            // Per spec, values >32767 are expressed as negative numbers, and we must add 65536 to get the
            // correct value.
            if (codePoint < 0)
            {
                codePoint += 65536;
                if (codePoint < 0 || codePoint > ushort.MaxValue) return Error.InvalidUnicode;
            }

            /*
            From the spec:
            "Occasionally Word writes SYMBOL_CHARSET (nonUnicode) characters in the range U+F020..U+F0FF instead
            of U+0020..U+00FF. Internally Word uses the values U+F020..U+F0FF for these characters so that plain-
            ext searches don't mistakenly match SYMBOL_CHARSET characters when searching for Unicode characters
            in the range U+0020..U+00FF. To find out the correct symbol font to use, e.g., Wingdings, Symbol,
            etc., find the last SYMBOL_CHARSET font control word \fN used, look up font N in the font table and
            find the face name. The charset is specified by the \fcharsetN control word and SYMBOL_CHARSET is for
            N = 2. This corresponds to codepage 42."

            Verified, this does in fact mean "find the last used font that specifically has \fcharset2" (or \cpg42).
            And, yes, that's last used, period, regardless of scope. So we track it globally. That's the official
            behavior, don't ask me.

            NOTE: Verified, these 0xF020-0xF0FF chars can be represented either as negatives or as >32767 positives
            (despite the spec saying that \uN must be signed int16). So we need to fall through to this section
            even if we did the above, because by adding 65536 we might now be in the 0xF020-0xF0FF range.
            */
            if (handleSymbolCharRange && codePoint >= 0xF020 && codePoint <= 0xF0FF)
            {
                codePoint -= 0xF000;

                int fontNum = _lastUsedFontWithCodePage42 > -1
                    ? _lastUsedFontWithCodePage42
                    : _header.DefaultFontNum;

                if (!_fontEntries.TryGetValue(fontNum, out FontEntry? fontEntry) || fontEntry.CodePage != 42)
                {
                    return Error.OK;
                }

                // We already know our code point is within bounds of the array, because the arrays also go from
                // 0x20 - 0xFF, so no need to check.
                if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _wingdingsChars))
                {
                    codePoint = _wingdingsFontToUnicode[codePoint - 0x20];
                }
                else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _webdingsChars))
                {
                    codePoint = _webdingsFontToUnicode[codePoint - 0x20];
                }
                else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, _symbolChars))
                {
                    codePoint = _symbolFontToUnicode[codePoint - 0x20];
                }
            }

            return Error.OK;
        }

        /// <summary>
        /// Copy of framework version but with a fast null return on fail instead of the infernal exception-throwing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? ConvertFromUtf32(int utf32)
        {
            if (utf32 < 0 || utf32 > 1114111 || (utf32 >= 55296 && utf32 <= 57343))
            {
                return null;
            }

            // ALLOC: ConvertFromUtf32: char.ToString()
            // This one needs to happen sooner or later anyway, and it's only max 2 chars
            if (utf32 < 65536) return char.ToString((char)utf32);

            utf32 -= 65536;

            // ALLOC: ConvertFromUtf32: return new string(new char[2])
            // Small enough not to even bother caching it
            return new string(new[]
            {
                (char)((utf32 / 1024) + 55296),
                (char)((utf32 % 1024) + 56320)
            });
        }

        /// <summary>
        /// Only call this if <paramref name="chars"/>'s length is > 0 and consists solely of the characters '0' through '9'.
        /// It does no checks at all and will throw if either of these things is false.
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParseIntFast(ListFast<char> chars)
        {
            int result = chars.ItemsArray[0] - '0';

            for (int i = 1; i < chars.Count; i++)
            {
                result *= 10;
                result += chars.ItemsArray[i] - '0';
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CharSeqEqualUpTo(char[] array1, int len1, char[] array2)
        {
            int array2Len = array2.Length;
            if (len1 < array2Len) return false;

            for (int i0 = 0; i0 < array2Len; i0++)
            {
                if (array1[i0] != array2[i0]) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SeqEqual(ListFast<char> seq1, char[] seq2)
        {
            int seq1Count = seq1.Count;
            if (seq1Count != seq2.Length) return false;

            for (int ci = 0; ci < seq1Count; ci++)
            {
                if (seq1.ItemsArray[ci] != seq2[ci]) return false;
            }
            return true;
        }

        #endregion
    }
}
