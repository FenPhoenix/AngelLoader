//#define CROSS_PLATFORM

// This is a fast, no-frills, platform-agnostic RTF-to-text converter. It can be used in place of RichTextBox
// when you simply want to convert RTF to plaintext without being tied to Windows.

/*
The goals of this RTF-to-text converter are:
-It should be platform-agnostic and have no dependencies on Windows.
-It should be as accurate as possible with regard to character encodings.
-It should work with unseekable streams, that is, streams that can only be read forward.
-It should be fast and have reasonable memory usage.
 
-We support forward-only streams so we can read straight out of a compressed zip file entry with no copying.
-We go to great lengths to detect character encoding accurately, even converting symbol fonts to their Unicode
 equivalents. We surpass even RichEdit in this respect.
-We're mindful of our allocations and minimize them wherever possible.
-We use the System.Text.Encoding.CodePages package to get all Windows-supported codepages on all platforms
 (only if CROSS_PLATFORM is defined).
*/

/* TODO:
Notes and miscellaneous:
-Test hex that combines into an actual valid character: \'81\'63
-Tiger face: \u-9169?\u-10179?

Perf:
-All StringBuilders that don't need to be ToString()'d should be changed to byte/char lists.
-We could collapse fonts if we find multiple ones with the same name and charset but different numbers.
 I mean it looks like we're plenty fast and memory-reasonable without doing so, but you know, idea.

Memory:
-Check if any resettables' capacity gets too large and if so, Capacity = 0 (deallocate) them.

Other:
-Really implement a proper Peek() function for the stream. I know it's possible, and having to use UnGetChar() is
 getting really nasty and error-prone.
-Consider being extremely forgiving about errors - we want as much plaintext as we can get out of a file, and even
 imperfect text may be useful. We extract a relatively very small portion of text from the file, so statistically
 it's likely we may not even hit broken text even if it exists.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
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

        #endregion

        #region Classes

        // How many times have you thought, "Gee, I wish I could just reach in and grab that backing array from
        // that List, instead of taking the senseless performance hit of having it copied to a newly allocated
        // array all the time in a ToArray() call"? Hooray!
        private sealed class ListWithExposedArray<T>
        {
            internal T[] ItemsArray;
            private int _itemsArrayLength;
            private const int _defaultCapacity = 4;
#pragma warning disable IDE0032
            private int _size;

            [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
            internal int Count => _size;
#pragma warning restore IDE0032

            internal ListWithExposedArray()
            {
                ItemsArray = new T[_defaultCapacity];
                _itemsArrayLength = _defaultCapacity;
            }

            internal ListWithExposedArray(int capacity)
            {
                ItemsArray = new T[capacity];
                _itemsArrayLength = capacity;
            }

            internal void Add(T item)
            {
                if (_size == _itemsArrayLength) EnsureCapacity(_size + 1);
                ItemsArray[_size++] = item;
            }

            public void Clear()
            {
                if (_size > 0)
                {
                    Array.Clear(ItemsArray, 0, _size);
                    _size = 0;
                }
            }

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

            internal void EnsureCapacity(int min)
            {
                if (_itemsArrayLength >= min) return;
                int num = _itemsArrayLength == 0 ? 4 : _itemsArrayLength * 2;
                if ((uint)num > 2146435071U) num = 2146435071;
                if (num < min) num = min;
                Capacity = num;
            }
        }

        private sealed class RTFStream
        {
            #region Private fields

            private Stream _stream = null!;
            // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
            // length as a param and store it.
            private long _length;

            private long _currentPos = -1;

            private const int _bufferLen = 81920;
            private readonly byte[] _buffer = new byte[_bufferLen];
            // Start it ready to roll over to 0 so we don't need extra logic for the first get
            private int _bufferPos = _bufferLen;

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

            #endregion

            internal void Reset(Stream stream, long streamLength)
            {
                _stream = stream;
                _length = streamLength;

                _currentPos = -1;

                Array.Clear(_buffer, 0, _bufferLen);
                _bufferPos = _bufferLen;

                _unGetBuffer.Clear();
            }

            /// <summary>
            /// Puts a char back into the stream and decrements the read position. Actually doesn't really do that
            /// but uses an internal seek-back buffer to allow it work with forward-only streams. But don't worry
            /// about that. Just use it as normal.
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            internal void UnGetChar(char c)
            {
                if (_currentPos < 0) return;
                _unGetBuffer.Push(c);
                _currentPos--;
            }

            private byte StreamReadByte()
            {
                _bufferPos++;
                if (_bufferPos > _bufferLen - 1)
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
            internal bool GetNextChar(out char ch)
            {
                _currentPos++;
                if (_currentPos > _length - 1)
                {
                    // Even if we're on the last char, we still want to read from the buffer first
                    if (_unGetBuffer.Count > 0)
                    {
                        ch = _unGetBuffer.Pop();
                        return true;
                    }
                    else
                    {
                        ch = '\0';
                        return false;
                    }
                }
                else
                {
                    ch = _unGetBuffer.Count > 0 ? _unGetBuffer.Pop() : (char)StreamReadByte();
                    return true;
                }
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

            // Fonts can specify themselves as whatever number they want, so we can't just count by index
            // eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45
            internal int Num;
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

            internal readonly int[] Properties = new int[_propertiesLen];

            internal void Reset()
            {
                RtfDestinationState = 0;
                RtfInternalState = 0;
                InFontTable = false;

                Properties[(int)Property.Hidden] = 0;
                Properties[(int)Property.UnicodeCharSkipCount] = 1;
                Properties[(int)Property.FontNum] = -1;
            }
        }

        // Scopes on the stack need not be mutable, and making them structs is faster/smaller/better cache locality/less GC/whatever
        private readonly struct Scope
        {
            private readonly RtfDestinationState RtfDestinationState;
            private readonly RtfInternalState RtfInternalState;
            private readonly bool InFontTable;

            private readonly int[] Properties;

            public Scope(CurrentScope currentScope)
            {
                RtfDestinationState = currentScope.RtfDestinationState;
                RtfInternalState = currentScope.RtfInternalState;
                InFontTable = currentScope.InFontTable;

                Properties = new int[_propertiesLen];
                Array.Copy(currentScope.Properties, Properties, _propertiesLen);
            }

            internal void DeepCopyTo(CurrentScope dest)
            {
                dest.RtfDestinationState = RtfDestinationState;
                dest.RtfInternalState = RtfInternalState;
                dest.InFontTable = InFontTable;

                Array.Copy(Properties, dest.Properties, _propertiesLen);
            }
        }

        private readonly struct Symbol
        {
            internal readonly int DefaultParam;
            internal readonly bool UseDefaultParam;
            internal readonly KeywordType KeywordType;
            /// <summary>
            /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
            /// </summary>
            internal readonly int Index;

            public Symbol(int defaultParam, bool useDefaultParam, KeywordType keywordType, int index)
            {
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

        private static readonly Dictionary<string, Symbol> _symbolTable = new Dictionary<string, Symbol>
        {
            #region Code pages / charsets / fonts

            // The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
            // "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.
            {"ansi",      new Symbol(_windows1252,   true,      KeywordType.Special,        (int)SpecialType.HeaderCodePage)},

            {"pc",        new Symbol(437,            true,      KeywordType.Special,        (int)SpecialType.HeaderCodePage)},

            // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
            // assume 10000 ("Mac Roman")
            {"mac",       new Symbol(10000,          true,      KeywordType.Special,        (int)SpecialType.HeaderCodePage)},

            {"pca",       new Symbol(850,            true,      KeywordType.Special,        (int)SpecialType.HeaderCodePage)},
            {"ansicpg",   new Symbol(_windows1252,   false,     KeywordType.Special,        (int)SpecialType.HeaderCodePage)},

            {"deff",      new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.DefaultFont)},

            {"fonttbl",   new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.FontTable)},
            {"f",         new Symbol(0,              false,     KeywordType.Property,       (int)Property.FontNum)},
            {"fcharset",  new Symbol(-1,             false,     KeywordType.Special,        (int)SpecialType.Charset)},
            {"cpg",       new Symbol(-1,             false,     KeywordType.Special,        (int)SpecialType.CodePage)},

            #endregion

            #region Encoded characters

            {"uc",        new Symbol(1,              false,     KeywordType.Property,       (int)Property.UnicodeCharSkipCount)},
            {"'",         new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.HexEncodedChar)},
            {"u",         new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.UnicodeChar)},

            #endregion

            // \v to make all plain text hidden (not output to the conversion stream)}, \v0 to make it shown again
            {"v",         new Symbol(1,              false,     KeywordType.Property,       (int)Property.Hidden)},

            #region Newlines

            {"par",       new Symbol(0,              false,     KeywordType.Character,      '\n')},
            {"line",      new Symbol(0,              false,     KeywordType.Character,      '\n')},
            {"softline",  new Symbol(0,              false,     KeywordType.Character,      '\n')},

            #endregion

            #region Control words that map to a single character

            {"tab",       new Symbol(0,              false,     KeywordType.Character,      '\t')},

            // Just convert these to regular spaces because we're just trying to scan for strings in readmes
            // without weird crap tripping us up
            {"bullet",    new Symbol(0,              false,     KeywordType.Character,      '\x2022')},
            {"emspace",   new Symbol(0,              false,     KeywordType.Character,      ' ')},
            {"enspace",   new Symbol(0,              false,     KeywordType.Character,      ' ')},
            {"qmspace",   new Symbol(0,              false,     KeywordType.Character,      ' ')},
            {"~",         new Symbol(0,              false,     KeywordType.Character,      ' ')},
            // TODO: Maybe just convert these all to ASCII equivalents as well?
            {"emdash",    new Symbol(0,              false,     KeywordType.Character,      '\x2014')},
            {"endash",    new Symbol(0,              false,     KeywordType.Character,      '\x2013')},
            {"lquote",    new Symbol(0,              false,     KeywordType.Character,      '\x2018')},
            {"rquote",    new Symbol(0,              false,     KeywordType.Character,      '\x2019')},
            {"ldblquote", new Symbol(0,              false,     KeywordType.Character,      '\x201C')},
            {"rdblquote", new Symbol(0,              false,     KeywordType.Character,      '\x201D')},

            #endregion

            {"bin",       new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.Bin)},
            {"*",         new Symbol(0,              false,     KeywordType.Special,        (int)SpecialType.SkipDest)},

            // We need to do stuff with this (SYMBOL instruction)
            {"fldinst",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.FieldInstruction)}, 

            // Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.
            {"cs",        new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.IgnoreButDontSkipGroup)},
            {"ds",        new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.IgnoreButDontSkipGroup)},
            {"ts",        new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.IgnoreButDontSkipGroup)},

            #region Custom skip-destinations

            // Ignore list item bullets and numeric prefixes etc. We don't need them.
            {"listtext",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"pntext",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},

            #endregion

            #region Required skip-destinations

            {"author",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"buptim",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"colortbl",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"comment",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"creatim",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"doccomm",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"footer",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"footerf",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"footerl",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"footerr",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"footnote",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"ftncn",     new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"ftnsep",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"ftnsepc",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"header",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"headerf",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"headerl",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"headerr",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"info",      new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"keywords",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"operator",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"pict",      new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"printim",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"private1",  new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"revtim",    new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"rxe",       new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"stylesheet",new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"subject",   new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"tc",        new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"title",     new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"txe",       new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},
            {"xe",        new Symbol(0,              false,     KeywordType.Destination,    (int)DestinationType.Skip)},

            #endregion

            #region RTF reserved character escapes

            {"{",         new Symbol(0,              false,     KeywordType.Character,      '{')},
            {"}",         new Symbol(0,              false,     KeywordType.Character,      '}')},
            {"\\",        new Symbol(0,              false,     KeywordType.Character,      '\\')}

            #endregion
        };

        #endregion

        #region Preallocated arrays

        // This "SYMBOL" and the below "Symbol" are unrelated. "SYMBOL" is a fldinst keyword, while "Symbol" is
        // the name of a font.
        private char[]? _SYMBOLChars;
        private char[] SYMBOLChars => _SYMBOLChars ??= new[] { 'S', 'Y', 'M', 'B', 'O', 'L' };

        private char[]? _wingdingsChars;
        private char[] WingdingsChars => _wingdingsChars ??= new[] { 'W', 'i', 'n', 'g', 'd', 'i', 'n', 'g', 's' };

        private char[]? _webdingsChars;
        private char[] WebdingsChars => _webdingsChars ??= new[] { 'W', 'e', 'b', 'd', 'i', 'n', 'g', 's' };

        private char[]? _symbolChars;
        private char[] SymbolChars => _symbolChars ??= new[] { 'S', 'y', 'm', 'b', 'o', 'l' };

        #endregion

        #region Resettables

        private readonly RTFStream _rtfStream = new RTFStream();

        private readonly Header _header = new Header();

        // FMs can have 100+ of these...
        // Highest measured was 131
        private readonly List<FontEntry> _fontEntries = new List<FontEntry>(150);

        // Highest measured was 10
        private readonly Stack<Scope> _scopeStack = new Stack<Scope>(15);

        private readonly CurrentScope _currentScope = new CurrentScope();

        // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
        private int _groupCount;

        private int _binaryCharsLeftToSkip;
        private int _unicodeCharsLeftToSkip;

        private bool _skipDestinationIfUnknown;

        private readonly StringBuilder _returnSB = new StringBuilder();
        private const int _keywordMaxLen = 30;
        private const int _paramMaxLen = 20;
        private readonly StringBuilder _keywordSB = new StringBuilder(_keywordMaxLen, _keywordMaxLen);
        private readonly StringBuilder _parameterSB = new StringBuilder(_paramMaxLen, _paramMaxLen);
        private const int _fldinstSymbolNumberMaxLen = 10;
        private readonly StringBuilder _fldinstSymbolNumberSB = new StringBuilder(_fldinstSymbolNumberMaxLen, _fldinstSymbolNumberMaxLen);
        private const int _fldinstSymbolFontNameMaxLen = 9;
        private readonly StringBuilder _fldinstSymbolFontNameSB = new StringBuilder(_fldinstSymbolFontNameMaxLen, _fldinstSymbolFontNameMaxLen);

        // Highest measured was 17
        private readonly ListWithExposedArray<byte> _hexBuffer = new ListWithExposedArray<byte>(20);

        // Highest measured was 13
        private readonly List<int> _unicodeBuffer = new List<int>(20);

        // Highest measured was 13
        private readonly ListWithExposedArray<char> _unicodeCharsTemp = new ListWithExposedArray<char>(20);

        #endregion

        #region Cached encodings

        // DON'T reset this. We want to build up a dictionary of encodings and amortize it over the entire list
        // of RTF files.
        private readonly Dictionary<int, Encoding> _encodings = new Dictionary<int, Encoding>(31);

        // Common ones explicitly stored to avoid even a dictionary lookup. Don't reset these either.
        private Encoding? _windows1252Encoding;
        private Encoding Windows1252Encoding => _windows1252Encoding ??= Encoding.GetEncoding(_windows1252);

        private Encoding? _Windows1250Encoding;
        private Encoding Windows1250Encoding => _Windows1250Encoding ??= Encoding.GetEncoding(1250);

        private Encoding? _windows1251Encoding;
        private Encoding Windows1251Encoding => _windows1251Encoding ??= Encoding.GetEncoding(1251);

        private Encoding? _shiftJisWinEncoding;
        private Encoding ShiftJisWinEncoding => _shiftJisWinEncoding ??= Encoding.GetEncoding(_shiftJisWin);

        #endregion

        #region Public API

        // If we're on something other than Windows, we don't normally have access to most of the codepages that
        // Windows does. To get access to them, we need to take a dependency on the ~700k System.Text.Encoding.CodePages
        // NuGet package and then call this RegisterProvider method like this. But we don't want to carry around
        // the extra package until we actually need it, so it's disabled for now.
#if CROSS_PLATFORM
        public RtfToTextConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
#endif

        [PublicAPI]
        public (bool Success, string Text) Convert(Stream stream, long streamLength)
        {
            Reset(stream, streamLength);
#if ReleaseTestMode || DebugTestMode

            Error error = ParseRtf();
            return error == Error.OK ? (true, _returnSB.ToString()) : (false, "");
#else
            try
            {
                Error error = ParseRtf();
                return error == Error.OK ? (true, _returnSB.ToString()) : (false, "");
            }
            catch
            {
                return (false, "");
            }
#endif
        }

        #endregion

        private void Reset(Stream stream, long streamLength)
        {
            #region Fixed-size fields

            // Specific capacity and can't grow; no need to deallocate
            _keywordSB.Clear();
            _parameterSB.Clear();
            _fldinstSymbolNumberSB.Clear();
            _fldinstSymbolFontNameSB.Clear();

            // Flat primitives
            _groupCount = 0;
            _binaryCharsLeftToSkip = 0;
            _unicodeCharsLeftToSkip = 0;
            _skipDestinationIfUnknown = false;

            // Types that contain only flat primitives
            _header.Reset();
            _currentScope.Reset();

            #endregion

            // Deallocate these if they get too big
            _hexBuffer.Clear();
            _unicodeBuffer.Clear();
            _unicodeCharsTemp.Clear();
            _fontEntries.Clear();
            _scopeStack.Clear();
            _returnSB.Clear();

            // This one has the seek-back buffer (a Stack<char>) which is technically eligible for deallocation,
            // even though in practice I think it's guaranteed never to have more than like 5 chars in it maybe?
            _rtfStream.Reset(stream, streamLength);

        }

        private Error ParseRtf()
        {
            Error ec;
            int nibbleCount = 0;
            byte b = 0;

            while (_rtfStream.GetNextChar(out char ch))
            {
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
                        ParseUnicodeIfAnyInBuffer();
                        if ((ec = PushScope()) != Error.OK) return ec;
                        break;
                    case '}':
                        // ditto the above
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        ParseUnicodeIfAnyInBuffer();
                        if ((ec = PopScope()) != Error.OK) return ec;
                        break;
                    case '\\':
                        // We have to check what the keyword is before deciding whether to parse the Unicode.
                        if ((ec = ParseKeyword()) != Error.OK) return ec;
                        break;
                    case '\r':
                    case '\n':
                        // These DON'T count as Unicode barriers, so don't parse the Unicode here!
                        break;
                    default:
                        // It's a Unicode barrier, so parse the Unicode here.
                        ParseUnicodeIfAnyInBuffer();
                        switch (_currentScope.RtfInternalState)
                        {
                            case RtfInternalState.Normal:
                                if ((ec = ParseChar(ch)) != Error.OK) return ec;
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

        // It's ugly, but it's gotta be done...
        private Error ParseHex(ref int nibbleCount, ref char ch, ref byte b)
        {
            #region Local functions

            static bool CharSeqEqualUpTo(char[] array1, int len1, char[] array2)
            {
                int array2Len = array2.Length;
                if (len1 < array2Len) return false;

                for (int i0 = 0; i0 < array2Len; i0++)
                {
                    if (array1[i0] != array2[i0]) return false;
                }

                return true;
            }

            Error ResetBufferAndStateAndReturn()
            {
                _hexBuffer.Clear();
                _currentScope.RtfInternalState = RtfInternalState.Normal;
                return Error.OK;
            }

            #endregion

            // If multiple hex chars are directly after another (eg. \'81\'63) then they may be representing one
            // multibyte character (or not, they may also just be two single-byte chars in a row). To deal with
            // this, we have to put all contiguous hex chars into a buffer and when the run ends, we just pass
            // the buffer to the current encoding's byte-to-string decoder and get our correct result.

            _hexBuffer.Clear();

            // Quick-n-dirty goto for now. TODO: Make this better or something
            restartButDontClearBuffer:

            b = (byte)(b << 4);

            if (IsAsciiDigit(ch))
            {
                b += (byte)(ch - '0');
            }
            else
            {
                if (IsAsciiLower(ch))
                {
                    if (ch < 'a' || ch > 'f') return Error.InvalidHex;
                    b += (byte)(ch - 'a' + 10);
                }
                else
                {
                    if (ch < 'A' || ch > 'F') return Error.InvalidHex;
                    b += (byte)(ch - 'A' + 10);
                }
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
                            if (finalChar == "") finalChar = _unicodeUnknown_String;
                        }
                        else
                        {
                            if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, WingdingsChars))
                            {
                                GetCharFromConversionList(codePoint, _wingdingsFontToUnicode, out finalChar);
                            }
                            else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, WebdingsChars))
                            {
                                GetCharFromConversionList(codePoint, _webdingsFontToUnicode, out finalChar);
                            }
                            else if (CharSeqEqualUpTo(fontEntry.Name, fontEntry.NameCharPos, SymbolChars))
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

        private Error PushScope()
        {
            // Don't wait for out-of-memory; just put a sane cap on it.
            if (_scopeStack.Count > 100) return Error.StackOverflow;

            var newScope = new Scope(_currentScope);

            _currentScope.RtfInternalState = RtfInternalState.Normal;

            _scopeStack.Push(newScope);
            _groupCount++;

            return Error.OK;
        }

        private Error PopScope()
        {
            if (_scopeStack.Count == 0) return Error.StackUnderflow;

            _scopeStack.Pop().DeepCopyTo(_currentScope);
            _groupCount--;

            return Error.OK;
        }

        #region Helpers

        private static bool IsAsciiAlpha(char ch) => IsAsciiUpper(ch) || IsAsciiLower(ch);
        private static bool IsAsciiUpper(char ch) => ch >= 'A' && ch <= 'Z';
        private static bool IsAsciiLower(char ch) => ch >= 'a' && ch <= 'z';
        private static bool IsAsciiDigit(char ch) => ch >= '0' && ch <= '9';

        /// <summary>
        /// If <paramref name="codePage"/> is in the cached list, returns the Encoding associated with it;
        /// otherwise, gets the Encoding for <paramref name="codePage"/> and places it in the cached list
        /// for next time.
        /// </summary>
        /// <param name="codePage"></param>
        /// <returns></returns>
        private Encoding GetEncodingFromCachedList(int codePage)
        {
            switch (codePage)
            {
                case _windows1252:
                    return Windows1252Encoding;
                case 1250:
                    return Windows1250Encoding;
                case 1251:
                    return Windows1251Encoding;
                case _shiftJisWin:
                    return ShiftJisWinEncoding;
                default:
                    if (_encodings.TryGetValue(codePage, out Encoding result))
                    {
                        return result;
                    }
                    else
                    {
                        Encoding enc = Encoding.GetEncoding(codePage);
                        _encodings.Add(codePage, enc);
                        return enc;
                    }
            }
        }

        private (bool Success, bool CodePageWas42, Encoding? Encoding, FontEntry? FontEntry)
        GetCurrentEncoding()
        {
            int scopeFontNum = _currentScope.Properties[(int)Property.FontNum];
            FontEntry? fontEntry = null;

            if (scopeFontNum == -1) scopeFontNum = _header.DefaultFontNum;

            for (int i = 0; i < _fontEntries.Count; i++)
            {
                var item = _fontEntries[i];
                if (item.Num == scopeFontNum)
                {
                    fontEntry = _fontEntries[i];
                    break;
                }
            }

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

        private static Error NormalizeUnicodePoint(ref int codePoint)
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
             "Occasionally Word writes SYMBOL_CHARSET (nonUnicode) characters in the range  U+F020..U+F0FF
             instead of U+0020..U+00FF. Internally Word uses the values U+F020..U+F0FF for these characters so
             that plain-text searches don't mistakenly match SYMBOL_CHARSET characters when searching for Unicode
             characters in the range U+0020..U+00FF."
             */
            else if (codePoint >= 0xf020 && codePoint <= 0xf0ff)
            {
                codePoint -= 0xf000; // 61440
            }

            return Error.OK;
        }

        /// <summary>
        /// Copy of framework version but with a fast null return on fail instead of the infernal exception-throwing.
        /// </summary>
        private static string? ConvertFromUtf32(int utf32)
        {
            if (utf32 < 0 || utf32 > 1114111 || utf32 >= 55296 && utf32 <= 57343)
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
                (char)(utf32 / 1024 + 55296),
                (char)(utf32 % 1024 + 56320)
            });
        }

        /// <summary>
        /// Only call this if <paramref name="sb"/>'s length is > 0 and consists solely of the characters '0' through '9'.
        /// It does no checks at all and will throw if either of these things is false.
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        private static int ParseIntFast(StringBuilder sb)
        {
            int result = sb[0] - '0';

            for (int i = 1; i < sb.Length; i++)
            {
                result *= 10;
                result += sb[i] - '0';
            }
            return result;
        }

        #endregion

        private Error ParseKeyword()
        {
            _keywordSB.Clear();
            _parameterSB.Clear();

            bool hasParam = false;
            bool negateParam = false;
            int param = 0;

            if (!_rtfStream.GetNextChar(out char ch)) return Error.EndOfFile;

            if (!IsAsciiAlpha(ch))
            {
                /* From the spec:
                 "A control symbol consists of a backslash followed by a single, non-alphabetical character.
                 For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
                 delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

                 So just go straight to dispatching without looking for a param and without eating the space.
                */
                _keywordSB.Append(ch);
                return DispatchKeyword(0, false);
            }

            int i;
            bool eof = false;
            for (i = 0; i < _keywordMaxLen && IsAsciiAlpha(ch); i++, eof = !_rtfStream.GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                _keywordSB.Append(ch);
            }
            if (i >= _keywordMaxLen) return Error.KeywordTooLong;

            if (ch == '-')
            {
                negateParam = true;
                if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;
            }

            if (IsAsciiDigit(ch))
            {
                hasParam = true;

                for (i = 0; i < _paramMaxLen && IsAsciiDigit(ch); i++, eof = !_rtfStream.GetNextChar(out ch))
                {
                    if (eof) return Error.EndOfFile;
                    _parameterSB.Append(ch);
                }
                if (i >= _paramMaxLen) return Error.ParameterTooLong;

                param = ParseIntFast(_parameterSB);
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

        private Error DispatchKeyword(int param, bool hasParam)
        {
            // ALLOC: _keywordSB.ToString()
            /*
            ToString() is run on every single keyword, which makes me wince.
            I'd like to get rid of it, but I haven't found any faster way.
            I use a dictionary to look up the string currently.
            If I switch to a list and iterate with compare, it's way slower (Equals is the killer).
            I even tried using wrapped char arrays with custom Equals and GetHashCode as the keys in the dict,
            and that was still about 7% slower. So even though this is allocation-heavy (1.2 million calls over
            the 496-file test set), it's still really fast and is just not that big of a deal (low double-digit
            milliseconds over the aforementioned set on my 3950x, which is ~2% of execution time).
            */
            string keywordStr = _keywordSB.ToString();

            if (!_symbolTable.TryGetValue(keywordStr, out Symbol symbol))
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
                _unicodeCharsLeftToSkip--;
                if (_unicodeCharsLeftToSkip < 0) _unicodeCharsLeftToSkip = 0;
                return Error.OK;
            }

            if (symbol.Index != (int)SpecialType.UnicodeChar)
            {
                ParseUnicodeIfAnyInBuffer();
            }

            _skipDestinationIfUnknown = false;
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    return ChangeProperty((Property)symbol.Index, param);
                case KeywordType.Character:
                    return ParseChar((char)symbol.Index);
                case KeywordType.Destination:
                    return ChangeDestination((DestinationType)symbol.Index);
                case KeywordType.Special:
                    return DispatchSpecialKeyword((SpecialType)symbol.Index, param);
                default:
                    return Error.InvalidSymbolTableEntry;
            }
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
            #region Local functions

            // We can't just pass a char array, because then the validation won't work. We have to convert to
            // string first. Fortunately, this loses no time because we have to do the conversion anyway, and we
            // don't even touch the string unless there's a problem, which means no extra allocations if everything
            // is valid.
            static void FixUpBadUnicode(ref string str)
            {
                StringBuilder? sb = null;

                for (int i = 0; i < str.Length; i++)
                {
                    // We need to use (str, i) instead of (str[i]) because the validation check doesn't work if
                    // we do the latter. Unfortunately we need to take some boneheaded pointless null and length
                    // checks, because we can't pull this method out and customize it, because it calls a goddamn
                    // internal method. Access levels... hooray, you're slow and you can't do crap about it.

                    // This won't throw because str is not null and index is within it
                    UnicodeCategory uc = char.GetUnicodeCategory(str, i);

                    if (uc == UnicodeCategory.Surrogate || uc == UnicodeCategory.OtherNotAssigned)
                    {
                        // Don't even instantiate our StringBuilder unless there's a problem
                        sb ??= new StringBuilder(str);
                        sb[i] = _unicodeUnknown_Char;
                    }

                    // We can do (str[i]) here because (str, i) is literally just a wrapper that can throw.
                    if (char.IsHighSurrogate(str[i])) i++;
                }

                // Likewise, don't even change the string at all unless we have to
                if (sb != null) str = sb.ToString();
            }

            #endregion

            if (_unicodeBuffer.Count == 0 || _unicodeCharsLeftToSkip > 0) return;

            // A char is 2 bytes wide, which is the same width as a \uN character is allowed to be, so they match
            // 1-to-1.
            _unicodeCharsTemp.EnsureCapacity(_unicodeBuffer.Count);
            _unicodeCharsTemp.Clear();

            for (int i = 0; i < _unicodeBuffer.Count; i++)
            {
                byte[] temp = BitConverter.GetBytes(_unicodeBuffer[i]);
                // This won't throw because temp is not null and index is within range and is not the last index
                // PERF_TODO: This is the slow call... might not be able to do much about it
                _unicodeCharsTemp.Add(BitConverter.ToChar(temp, 0));
            }

            _unicodeBuffer.Clear();

            string finalString = new string(_unicodeCharsTemp.ItemsArray, 0, _unicodeCharsTemp.Count);

            FixUpBadUnicode(ref finalString);

            PutChar(finalString);
        }

        private Error DispatchSpecialKeyword(SpecialType specialType, int param)
        {
            if (_currentScope.RtfDestinationState == RtfDestinationState.Skip && specialType != SpecialType.Bin)
            {
                return Error.OK;
            }

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

                    Error error;
                    // Make sure the code point is normalized before adding it to the buffer!
                    if ((error = NormalizeUnicodePoint(ref param)) != Error.OK) return error;
                    _unicodeBuffer.Add(param);
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
                    if (_fontEntries.Count > 0)
                    {
                        _fontEntries[_fontEntries.Count - 1].CodePage = param >= 0 && _charSetToCodePage.TryGetValue(param, out int codePage)
                            ? codePage
                            : _header.CodePage;
                    }
                    break;
                case SpecialType.CodePage:
                    if (_fontEntries.Count > 0)
                    {
                        _fontEntries[_fontEntries.Count - 1].CodePage = param >= 0 ? param : _header.CodePage;
                    }
                    break;
                default:
                    return Error.InvalidSymbolTableEntry;
            }

            return Error.OK;
        }

        private Error ParseChar(char ch)
        {
            if (_currentScope.RtfDestinationState == RtfDestinationState.Normal)
            {
                if (_currentScope.InFontTable) _fontEntries[_fontEntries.Count - 1].AppendNameChar(ch);

                // Don't get clever and change the order of things. We need to know if our count is > 0 BEFORE
                // trying to print, because we want to not print if it's > 0. Only then do we decrement it.
                Error error = _unicodeCharsLeftToSkip == 0 ? PutChar(ch) : Error.OK;

                if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
                return error;
            }

            return Error.OK;
        }

        private Error ChangeProperty(Property propertyTableIndex, int val)
        {
            if (_currentScope.RtfDestinationState == RtfDestinationState.Skip)
            {
                return Error.OK;
            }
            else if (propertyTableIndex == Property.FontNum && _currentScope.InFontTable)
            {
                _fontEntries.Add(new FontEntry { Num = val });
            }
            else if ((int)propertyTableIndex > _propertiesLen - 1)
            {
                return Error.InvalidSymbolTableEntry;
            }
            else
            {
                _currentScope.Properties[(int)propertyTableIndex] = val;
            }

            return Error.OK;
        }

        private Error ChangeDestination(DestinationType destinationType)
        {
            if (_currentScope.RtfDestinationState == RtfDestinationState.Skip)
            {
                return Error.OK;
            }

            switch (destinationType)
            {
                case DestinationType.IgnoreButDontSkipGroup:
                    // The group this destination is in may contain text we want to extract, so parse it as normal.
                    // We will still skip over the next nested destination group we find, if any, unless it too is
                    // marked as ignore-but-don't-skip.
                    break;
                case DestinationType.FieldInstruction:
                    return HandleFieldInstruction();
                case DestinationType.Skip:
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                    break;
                default:
                    return Error.InvalidSymbolTableEntry;
            }
            return Error.OK;
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
            _fldinstSymbolNumberSB.Clear();
            _fldinstSymbolFontNameSB.Clear();

            // Declare these before the functions that use them
            int codePoint;

            #region Local functions

            static bool IsSeparatorChar(char ch) => ch == '\\' || ch == '{' || ch == '}';

            string GetCharFromCodePage(int codePage)
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

            char ch;

            Error RewindAndSkipGroup()
            {
                _rtfStream.UnGetChar(ch);
                _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                return Error.OK;
            }

            static bool SeqEqual(StringBuilder sb, char[] chars)
            {
                for (int ci = 0; ci < sb.Length; ci++)
                {
                    if (sb[ci] != chars[ci]) return false;
                }
                return true;
            }

            #endregion

            if (!_rtfStream.GetNextChar(out ch)) return Error.EndOfFile;

            #region Check for SYMBOL instruction

            // Straight-up just check for S, because SYMBOL is the only word we care about.
            if (ch != 'S') return RewindAndSkipGroup();

            int i;
            bool eof = false;
            for (i = 0; i < 6; i++, eof = !_rtfStream.GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                if (ch != SYMBOLChars[i]) return RewindAndSkipGroup();
            }

            #endregion

            // Eat the space
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
                    if (ch != ' ') return RewindAndSkipGroup();
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

                _fldinstSymbolNumberSB.Append(ch);
            }

            if (_fldinstSymbolNumberSB.Length == 0 ||
                i >= _fldinstSymbolNumberMaxLen ||
                (!numIsHex && alphaCharsFound))
            {
                return RewindAndSkipGroup();
            }

            #endregion

            #region Parse parameter

            if (numIsHex)
            {
                // ALLOC: ToString(): int.TryParse(hex)
                // We could implement our own hex parser, but this is called so infrequently (actually not at all
                // in the test set) that it doesn't really matter.
                if (!int.TryParse(_fldinstSymbolNumberSB.ToString(),
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out codePoint))
                {
                    return RewindAndSkipGroup();
                }
            }
            else
            {
                codePoint = ParseIntFast(_fldinstSymbolNumberSB);
            }

            #endregion

            #endregion

            if (negateNum) codePoint = -codePoint;

            Error error;
            if ((error = NormalizeUnicodePoint(ref codePoint)) != Error.OK) return error;

            if (ch != ' ') return RewindAndSkipGroup();

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
                    finalChar = GetCharFromCodePage(_windows1252);
                    break;
                }
                /*
                From the spec:
                "Interprets text in field-argument as the value of a SHIFT-JIS character."

                Note that "SHIFT-JIS" in RTF land means specifically Windows-31J or whatever you want to call it.
                */
                else if (ch == 'j')
                {
                    finalChar = GetCharFromCodePage(_shiftJisWin);
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
                        finalChar = GetCharFromCodePage(useCurrentScopeCodePage);
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
                            finalChar = GetCharFromCodePage(useCurrentScopeCodePage);
                            break;
                        }

                        int fontNameCharCount = 0;

                        while (_rtfStream.GetNextChar(out ch) && ch != '\"')
                        {
                            if (fontNameCharCount >= _fldinstSymbolFontNameMaxLen || IsSeparatorChar(ch))
                            {
                                return RewindAndSkipGroup();
                            }
                            _fldinstSymbolFontNameSB.Append(ch);
                            fontNameCharCount++;
                        }

                        // Just hardcoding the three most common fonts here, because there's only so far you
                        // really want to go down this path.
                        if (SeqEqual(_fldinstSymbolFontNameSB, SymbolChars) &&
                            !GetCharFromConversionList(codePoint, _symbolFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup();
                        }
                        else if (SeqEqual(_fldinstSymbolFontNameSB, WingdingsChars) &&
                                 !GetCharFromConversionList(codePoint, _wingdingsFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup();
                        }
                        else if (SeqEqual(_fldinstSymbolFontNameSB, WebdingsChars) &&
                                 !GetCharFromConversionList(codePoint, _webdingsFontToUnicode, out finalChar))
                        {
                            return RewindAndSkipGroup();
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
                    if (ch != ' ') return RewindAndSkipGroup();

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

            return RewindAndSkipGroup();
        }

        private Error PutChar(char ch)
        {
            //Trace.Write(ch.ToString());
            if (ch != '\0' &&
                _currentScope.Properties[(int)Property.Hidden] == 0 &&
                !_currentScope.InFontTable)
            {
                _returnSB.Append(ch);
            }
            return Error.OK;
        }

        private Error PutChar(string ch)
        {
            //Trace.Write(ch);
            if (ch != "\0" &&
                _currentScope.Properties[(int)Property.Hidden] == 0 &&
                !_currentScope.InFontTable)
            {
                _returnSB.Append(ch);
            }
            return Error.OK;
        }
    }
}
