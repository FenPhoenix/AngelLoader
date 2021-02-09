using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    // TODO: @DarkMode: We can probably remove this and just use a search
    // If we just search for "{\colortbl" we're extraordinarily unlikely to have it be intended as plain text.
    // Then we can get rid of this slow (in a relative sense) parser.

    public sealed class RtfColorTableParser
    {
        #region Constants

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
                new Symbol("colortbl", 0, false, KeywordType.Special, (int)SpecialType.ColorTable),
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

            private List<byte> _stream = null!;
            // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
            // length as a param and store it.
            /// <summary>
            /// Do not modify!
            /// </summary>
            internal long Length;

            /// <summary>
            /// Do not modify!
            /// </summary>
            internal int CurrentPos;

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
            internal void Reset(List<byte> stream)
            {
                _stream = stream;
                Length = stream.Count;

                CurrentPos = 0;

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
            private byte StreamReadByte() => _stream[CurrentPos];

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

        // Current scope needs to be mutable, so it's a single-instance class
        private sealed class CurrentScope
        {
            internal RtfDestinationState RtfDestinationState;
            internal RtfInternalState RtfInternalState;

            internal readonly int[] Properties = new int[_propertiesLen];

            internal void Reset()
            {
                RtfDestinationState = 0;
                RtfInternalState = 0;

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

            internal readonly int[] Properties = new int[_propertiesLen];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void DeepCopyTo(CurrentScope dest)
            {
                dest.RtfDestinationState = RtfDestinationState;
                dest.RtfInternalState = RtfInternalState;

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

        private const int _propertiesLen = 3;
        private enum Property
        {
            Hidden,
            UnicodeCharSkipCount,
            FontNum
        }

        private enum Error
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
            SkipDest,
            ColorTable
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

        private readonly SymbolDict _symbolTable = new SymbolDict();

        #region Resettables

        private readonly RTFStream _rtfStream = new RTFStream();

        // Highest measured was 10
        private readonly ScopeStack _scopeStack = new ScopeStack();

        private readonly CurrentScope _currentScope = new CurrentScope();

        // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
        private int _groupCount;

        private int _binaryCharsLeftToSkip;
        private int _unicodeCharsLeftToSkip;

        private bool _skipDestinationIfUnknown;

        private const int _keywordMaxLen = 32;
        private readonly ListFast<char> _keyword = new ListFast<char>(_keywordMaxLen);

        // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
        private const int _paramMaxLen = 10;

        private readonly StringBuilder _colorTableSB = new StringBuilder(4096);

        private readonly List<Color> _colorTable = new List<Color>(32);
        private int _colorTableStartIndex;
        private int _colorTableEndIndex;

        #endregion

        #region Public API

        [PublicAPI]
        public (bool Success, List<Color> ColorTable, int ColorTableStartIndex, int ColorTableEndIndex)
        GetColorTable(List<byte> stream)
        {
            Reset(stream);

            try
            {
                Error error = ParseRtf();
                return error == Error.OK
                    ? (true, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex, ColorTableEndIndex: _colorTableEndIndex)
                    : (false, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex, ColorTableEndIndex: _colorTableEndIndex);
            }
            catch
            {
                return (false, _colorTable, _colorTableStartIndex, _colorTableEndIndex);
            }
        }

        #endregion

        private void Reset(List<byte> stream)
        {
            #region Fixed-size fields

            // Specific capacity and won't grow; no need to deallocate
            _keyword.ClearFast();

            // Fixed-size value types
            _groupCount = 0;
            _binaryCharsLeftToSkip = 0;
            _unicodeCharsLeftToSkip = 0;
            _skipDestinationIfUnknown = false;
            _colorTableStartIndex = 0;
            _colorTableEndIndex = 0;

            // Types that contain only fixed-size value types
            _currentScope.Reset();

            #endregion
            _scopeStack.ClearFast();

            _colorTableSB.Clear();
            _colorTable.Clear();

            // This one has the seek-back buffer (a Stack<char>) which is technically eligible for deallocation,
            // even though in practice I think it's guaranteed never to have more than like 5 chars in it maybe?
            // Again, it's a stack so we can't check its capacity. But... meh. See above.
            // Not way into the idea of making another custom type where the only difference is we can access a
            // frigging internal variable, gonna be honest.
            _rtfStream.Reset(stream);
        }

        private bool _exiting;

        private Error ParseRtf()
        {
            while (_rtfStream.CurrentPos < _rtfStream.Length)
            {
                if (_exiting) return Error.OK;

                char ch = _rtfStream.GetNextCharFast();

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

                Error ec;
                switch (ch)
                {
                    case '{':
                        // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        if ((ec = PushScope()) != Error.OK) return ec;
                        break;
                    case '}':
                        // ditto the above
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
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
                        switch (_currentScope.RtfInternalState)
                        {
                            case RtfInternalState.Normal:
                                if (_currentScope.RtfDestinationState == RtfDestinationState.Normal)
                                {
                                    if ((ec = ParseChar(ch)) != Error.OK) return ec;
                                }
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
                    //return Error.InvalidSymbolTableEntry;
                    return Error.OK;
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
                    break;
                case SpecialType.ColorTable:
                    // Spec is to ignore any further color tables after the first one, which is fortunate for us
                    // because it makes us way faster (we quit as soon as we've parsed the color table, which is
                    // usually very close to the top of the file).
                    _exiting = true;
                    return ParseAndBuildColorTable();
                default:
                    //return Error.InvalidSymbolTableEntry;
                    return Error.OK;
            }

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ChangeProperty(Property propertyTableIndex, int val)
        {
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
                case DestinationType.Skip:
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                    return Error.OK;
                default:
                    //return Error.InvalidSymbolTableEntry;
                    return Error.OK;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ParseChar(char ch)
        {
            if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
            return Error.OK;
        }

        #endregion

        private Error ParseAndBuildColorTable()
        {
            Error ClearReturnFields(Error error)
            {
                _colorTableSB.Clear();
                _colorTable.Clear();
                _colorTableStartIndex = 0;
                _colorTableEndIndex = 0;
                return error;
            }

            ClearReturnFields(Error.OK);

            _colorTableStartIndex = _rtfStream.CurrentPos;

            var numSB = new StringBuilder(5);
            var rgbNameSB = new StringBuilder(5);

            while (true)
            {
                if (!_rtfStream.GetNextChar(out char ch)) return ClearReturnFields(Error.EndOfFile);
                if (ch == '}')
                {
                    _rtfStream.UnGetChar('}');
                    _colorTableEndIndex = _rtfStream.CurrentPos;
                    break;
                }
                _colorTableSB.Append(ch);
            }

            string ct = _colorTableSB.ToString();
            List<string> entries = ct.Split(';').ToList();

            // Remove the last blank entry so we don't count it as the auto/default one by hitting a blank entry
            // in the loop below
            if (entries[entries.Count - 1].IsWhiteSpace()) entries.RemoveAt(entries.Count - 1);

            for (int i = 0; i < entries.Count; i++)
            {
                string entry = entries[i].Trim();

                rgbNameSB.Clear();
                numSB.Clear();

                if (entry.IsEmpty())
                {
                    // 0 alpha will be the flag for "this is the omitted default/auto color"
                    _colorTable.Add(Color.FromArgb(0, 0, 0, 0));
                }
                else
                {
                    // Horrible but functional just to get it going
                    var redMatch = Regex.Match(entry, @"\\red(?<Value>[0123456789]{1,3})");
                    var greenMatch = Regex.Match(entry, @"\\green(?<Value>[0123456789]{1,3})");
                    var blueMatch = Regex.Match(entry, @"\\blue(?<Value>[0123456789]{1,3})");

                    if (!redMatch.Success || !blueMatch.Success || !greenMatch.Success ||
                        !byte.TryParse(redMatch.Groups["Value"].Value, out byte red) ||
                        !byte.TryParse(greenMatch.Groups["Value"].Value, out byte green) ||
                        !byte.TryParse(blueMatch.Groups["Value"].Value, out byte blue))
                    {
                        continue;
                    }

                    _colorTable.Add(Color.FromArgb(red, green, blue));
                }
            }

            return Error.OK;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiAlpha(char ch) => IsAsciiUpper(ch) || IsAsciiLower(ch);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiUpper(char ch) => ch >= 'A' && ch <= 'Z';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiLower(char ch) => ch >= 'a' && ch <= 'z';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiDigit(char ch) => ch >= '0' && ch <= '9';

        #endregion
    }
}
